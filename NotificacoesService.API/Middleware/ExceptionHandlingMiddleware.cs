namespace NotificacoesService.API.Middleware;

/// <summary>
/// Captura exceções técnicas inesperadas que escapam dos use cases.
/// Erros de domínio são convertidos em <c>Result.Failure</c> internamente
/// e não chegam aqui.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Erro técnico inesperado não capturado pelos use cases. Path: {Path}",
                context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                status = 500,
                title = "erro.interno",
                detail = "Ocorreu um erro interno. Tente novamente ou contate o suporte."
            });
        }
    }
}
