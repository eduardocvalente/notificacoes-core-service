using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NotificacoesService.API.Filters;

public sealed class XIntegrationHeaderFilter : IActionFilter
{
    private const string HeaderName = "X-Integration";

    private readonly IReadOnlyList<string> _authorizedClients;

    public XIntegrationHeaderFilter(IConfiguration configuration)
    {
        _authorizedClients = configuration
            .GetSection("XIntegration:AuthorizedClients")
            .Get<List<string>>() ?? [];
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var clientId)
            || string.IsNullOrWhiteSpace(clientId)
            || !_authorizedClients.Contains(clientId.ToString(), StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new BadRequestObjectResult(new
            {
                error = $"Header '{HeaderName}' ausente ou cliente não autorizado."
            });
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
