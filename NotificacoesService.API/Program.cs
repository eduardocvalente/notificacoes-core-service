using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NotificacoesService.API.Filters;
using NotificacoesService.API.Middleware;
using NotificacoesService.Application;
using NotificacoesService.Application.Options;
using NotificacoesService.Infrastructure;
using NotificacoesService.Infrastructure.Adapters.Output.Persistence;
using Serilog;
using Serilog.Formatting.Json;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ── Serilog ───────────────────────────────────────────────────────────────
    builder.Host.UseSerilog((ctx, services, lc) => lc
        .ReadFrom.Configuration(ctx.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .WriteTo.Console(new JsonFormatter()));

    // ── Options Pattern ───────────────────────────────────────────────────────
    builder.Services.Configure<SmtpOptions>(
        builder.Configuration.GetSection("Smtp"));
    builder.Services.Configure<DatabaseOptions>(
        builder.Configuration.GetSection("Database"));
    builder.Services.Configure<BrokerOptions>(
        builder.Configuration.GetSection("Broker"));

    // ── Camadas ───────────────────────────────────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ── Filtros ───────────────────────────────────────────────────────────────
    builder.Services.AddScoped<XIntegrationHeaderFilter>();

    // ── Controllers ───────────────────────────────────────────────────────────
    builder.Services.AddControllers();

    // ── Swagger com Swashbuckle 6.x ───────────────────────────────────────────
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "NotificacoesService API",
            Version = "v1",
            Description =
                "API interna para consulta de histórico e reenvio manual de notificações " +
                "do Sistema de Gerenciamento Escolar.",
            Contact = new OpenApiContact
            {
                Name  = "Equipe SGE",
                Email = "sge@escola.com.br"
            }
        });

        // XML docs gerados em build
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

        // Header de autenticação X-Integration
        options.AddSecurityDefinition("X-Integration", new OpenApiSecurityScheme
        {
            Name        = "X-Integration",
            Type        = SecuritySchemeType.ApiKey,
            In          = ParameterLocation.Header,
            Description =
                "Header obrigatório de identificação do serviço cliente. " +
                "Exemplo: <code>matriculas-service</code>"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id   = "X-Integration"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // ── Migrations automáticas ────────────────────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<NotificacaoDbContext>();
        await db.Database.MigrateAsync();
        Log.Information("Migrations aplicadas com sucesso.");
    }

    // ── Pipeline ──────────────────────────────────────────────────────────────
    app.UseMiddleware<ExceptionHandlingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "NotificacoesService v1"));
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("NotificacoesService iniciando...");
    await app.RunAsync();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Log.Fatal(ex, "Falha crítica na inicialização do NotificacoesService.");
    throw;
}
finally
{
    await Log.CloseAndFlushAsync();
}
