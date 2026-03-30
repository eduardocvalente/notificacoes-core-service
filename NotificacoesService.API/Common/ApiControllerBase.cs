using Microsoft.AspNetCore.Mvc;
using NotificacoesService.Application.Common;

namespace NotificacoesService.API.Common;

/// <summary>
/// Classe base para todos os controllers. Centraliza o mapeamento
/// de <see cref="Result"/> e <see cref="Result{T}"/> para HTTP status codes.
/// </summary>
[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    // ── Sobrecarga para Result (void) ─────────────────────────────────────────

    /// <summary>Converte um <see cref="Result"/> em <see cref="IActionResult"/>.</summary>
    protected IActionResult FromResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return MapError(result.Error);
    }

    // ── Sobrecarga para Result<T> ─────────────────────────────────────────────

    /// <summary>Converte um <see cref="Result{T}"/> em <see cref="IActionResult"/>.</summary>
    protected IActionResult FromResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return MapError(result.Error);
    }

    // ── Mapeamento centralizado ErrorType → HTTP ──────────────────────────────

    private IActionResult MapError(Error error) => error.Type switch
    {
        ErrorType.Validation => BadRequest(BuildProblemDetails(error, StatusCodes.Status400BadRequest)),
        ErrorType.NotFound   => NotFound(BuildProblemDetails(error, StatusCodes.Status404NotFound)),
        ErrorType.Conflict   => Conflict(BuildProblemDetails(error, StatusCodes.Status409Conflict)),
        ErrorType.Unexpected => StatusCode(
            StatusCodes.Status500InternalServerError,
            BuildProblemDetails(error, StatusCodes.Status500InternalServerError)),
        _ => StatusCode(
            StatusCodes.Status500InternalServerError,
            BuildProblemDetails(error, StatusCodes.Status500InternalServerError))
    };

    private static ProblemDetails BuildProblemDetails(Error error, int statusCode) => new()
    {
        Status = statusCode,
        Title  = error.Code,
        Detail = error.Description
    };
}
