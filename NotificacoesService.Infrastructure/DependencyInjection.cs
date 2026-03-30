using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificacoesService.Application.Options;
using NotificacoesService.Application.Ports.Output;
using NotificacoesService.Domain.Ports.Output;
using NotificacoesService.Infrastructure.Adapters.Input.Messaging;
using NotificacoesService.Infrastructure.Adapters.Output.Email;
using NotificacoesService.Infrastructure.Adapters.Output.Persistence;
using NotificacoesService.Infrastructure.Adapters.Output.Persistence.Repositories;
using NotificacoesService.Infrastructure.Adapters.Output.Templates;

namespace NotificacoesService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var databaseOptions = configuration
            .GetSection("Database")
            .Get<DatabaseOptions>()
            ?? throw new InvalidOperationException("Seção 'Database' não encontrada na configuração.");

        services.AddDbContext<NotificacaoDbContext>(options =>
            options.UseNpgsql(
                databaseOptions.ConnectionString,
                npgsql => npgsql
                    .EnableRetryOnFailure(3)
                    .CommandTimeout(databaseOptions.CommandTimeout)));

        services.AddScoped<INotificacaoRepository, NotificacaoRepository>();
        services.AddScoped<IEmailGateway, SmtpEmailGateway>();
        services.AddScoped<ITemplateRenderer, RazorTemplateRenderer>();

        services.AddHostedService<MatriculaRealizadaConsumer>();
        services.AddHostedService<NotaLancadaConsumer>();
        services.AddHostedService<AlunoAtualizadoConsumer>();

        return services;
    }
}
