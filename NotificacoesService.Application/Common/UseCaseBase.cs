using Microsoft.Extensions.Logging;

namespace NotificacoesService.Application.Common;

public abstract class UseCaseBase
{
    protected readonly ILogger Logger;

    protected UseCaseBase(ILogger logger)
    {
        Logger = logger;
    }

    // ── Sobrecarga para Result (void) ─────────────────────────────────────────

    protected async Task<Result> ExecutarComTratamentoAsync(
        Func<Task<Result>> acao,
        string correlationId,
        CancellationToken ct)
    {
        try
        {
            return await acao();
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning(
                "Operação cancelada. CorrelationId: {CorrelationId}",
                correlationId);

            return Result.Failure(
                Error.Unexpected("operacao.cancelada", "Operação cancelada pelo cliente."));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Erro inesperado. CorrelationId: {CorrelationId}",
                correlationId);

            return Result.Failure(
                Error.Unexpected("erro.inesperado", ex.Message));
        }
    }

    // ── Sobrecarga para Result<T> ─────────────────────────────────────────────

    protected async Task<Result<T>> ExecutarComTratamentoAsync<T>(
        Func<Task<Result<T>>> acao,
        string correlationId,
        CancellationToken ct)
    {
        try
        {
            return await acao();
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning(
                "Operação cancelada. CorrelationId: {CorrelationId}",
                correlationId);

            return Result<T>.Failure(
                Error.Unexpected("operacao.cancelada", "Operação cancelada pelo cliente."));
        }
        catch (Exception ex)
        {
            Logger.LogError(ex,
                "Erro inesperado. CorrelationId: {CorrelationId}",
                correlationId);

            return Result<T>.Failure(
                Error.Unexpected("erro.inesperado", ex.Message));
        }
    }
}
