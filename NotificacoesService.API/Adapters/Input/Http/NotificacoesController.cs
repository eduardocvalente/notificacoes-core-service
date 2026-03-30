using Microsoft.AspNetCore.Mvc;
using NotificacoesService.API.Common;
using NotificacoesService.API.Filters;
using NotificacoesService.Application.DTOs;
using NotificacoesService.Application.Ports.Input;

namespace NotificacoesService.API.Adapters.Input.Http;

/// <summary>
/// Gerenciamento de histórico e reenvio de notificações do Sistema de Gerenciamento Escolar.
/// </summary>
[Route("api/notificacoes")]
[Produces("application/json")]
[ServiceFilter(typeof(XIntegrationHeaderFilter))]
public sealed class NotificacoesController : ApiControllerBase
{
    private readonly IConsultarNotificacoesUseCase _consultarUseCase;
    private readonly IReenviarNotificacaoUseCase _reenviarUseCase;

    public NotificacoesController(
        IConsultarNotificacoesUseCase consultarUseCase,
        IReenviarNotificacaoUseCase reenviarUseCase)
    {
        _consultarUseCase = consultarUseCase;
        _reenviarUseCase = reenviarUseCase;
    }

    /// <summary>
    /// Lista todas as notificações de um destinatário, ordenadas por data de criação (desc).
    /// </summary>
    /// <param name="destinatarioId">ID do aluno ou professor destinatário.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>Lista de notificações do destinatário.</returns>
    /// <remarks>
    /// Exemplo de requisição:
    /// <code>GET /api/notificacoes/destinatario/3fa85f64-5717-4562-b3fc-2c963f66afa6</code>
    /// </remarks>
    [HttpGet("destinatario/{destinatarioId}")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificacaoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ListarPorDestinatario(
        [FromRoute] Guid destinatarioId,
        CancellationToken ct)
    {
        var correlationId = HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        var resultado = await _consultarUseCase.ExecutarAsync(
            new ConsultarNotificacoesInput(destinatarioId, correlationId), ct);

        return FromResult(resultado);
    }

    /// <summary>
    /// Reenvia uma notificação que falhou (status <b>Falha</b>, menos de 3 tentativas).
    /// </summary>
    /// <param name="id">ID da notificação a ser reenviada.</param>
    /// <param name="ct">Token de cancelamento.</param>
    /// <returns>204 No Content em caso de sucesso.</returns>
    /// <remarks>
    /// Exemplo de requisição:
    /// <code>POST /api/notificacoes/3fa85f64-5717-4562-b3fc-2c963f66afa6/reenviar</code>
    ///
    /// Regras:
    /// - A notificação precisa existir (404 caso contrário).
    /// - A notificação não pode ter status <b>Enviada</b> (409).
    /// - A notificação não pode ter 3 ou mais tentativas (409).
    /// </remarks>
    [HttpPost("{id}/reenviar")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Reenviar(
        [FromRoute] Guid id,
        CancellationToken ct)
    {
        var correlationId = HttpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        var resultado = await _reenviarUseCase.ExecutarAsync(
            new ReenviarNotificacaoInput(id, correlationId), ct);

        return FromResult(resultado);
    }
}
