namespace NotificacoesService.Application.Common;

// ── Result<T> — para operações que retornam valor ────────────────────────────

public sealed class Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;

    private Result(T value)
    {
        IsSuccess = true;
        _value = value;
        _error = default;
    }

    private Result(Error error)
    {
        IsSuccess = false;
        _value = default;
        _error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(
            "Result está em estado de falha. Verifique IsSuccess antes de acessar Value.");

    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException(
            "Result está em estado de sucesso. Verifique IsFailure antes de acessar Error.");

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(Error error) => new(error);

    // Conversão implícita de T → Result<T>: permite "return value;" nos use cases
    public static implicit operator Result<T>(T value) => Success(value);

    // Conversão implícita de Error → Result<T>: permite "return Error.X;" nos use cases
    public static implicit operator Result<T>(Error error) => Failure(error);
}

// ── Result — para operações void ─────────────────────────────────────────────

public sealed class Result
{
    private readonly Error? _error;

    private Result(bool isSuccess, Error? error = null)
    {
        IsSuccess = isSuccess;
        _error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public Error Error => IsFailure
        ? _error!
        : throw new InvalidOperationException(
            "Result está em estado de sucesso. Verifique IsFailure antes de acessar Error.");

    public static Result Success() => new(true);
    public static Result Failure(Error error) => new(false, error);

    // Conversão implícita de Error → Result
    public static implicit operator Result(Error error) => Failure(error);
}
