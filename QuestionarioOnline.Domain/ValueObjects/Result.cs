namespace QuestionarioOnline.Domain.ValueObjects;

/// <summary>
/// Result Pattern - Retorno explícito de sucesso ou falha
/// Evita uso de exceptions para fluxo de negócio
/// 
/// É um Value Object porque:
/// - Imutável (readonly properties)
/// - Comparado por valor (IsSuccess, Error)
/// - Sem identidade própria
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public bool IsNotFound { get; }
    public string Error { get; }
    public List<string> Errors { get; }

    protected Result(bool isSuccess, string error, List<string>? errors = null, bool isNotFound = false)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Sucesso não pode ter erro");

        if (!isSuccess && string.IsNullOrEmpty(error) && (errors == null || !errors.Any()))
            throw new InvalidOperationException("Falha deve ter pelo menos um erro");

        IsSuccess = isSuccess;
        IsNotFound = isNotFound;
        Error = error;
        Errors = errors ?? new List<string>();
    }

    public static Result Success() => new(true, string.Empty);

    public static Result Failure(string error) => new(false, error);

    public static Result Failure(List<string> errors) => new(false, errors.First(), errors);

    public static Result NotFound(string error) => new(false, error, isNotFound: true);

    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);

    public static Result<T> Failure<T>(string error) => new(default!, false, error);

    public static Result<T> Failure<T>(List<string> errors) => new(default!, false, errors.First(), errors);

    public static Result<T> NotFound<T>(string error) => new(default!, false, error, isNotFound: true);
}

/// <summary>
/// Result Pattern com valor de retorno
/// </summary>
public class Result<T> : Result
{
    public T Value { get; }

    protected internal Result(T value, bool isSuccess, string error, List<string>? errors = null, bool isNotFound = false)
        : base(isSuccess, error, errors, isNotFound)
    {
        Value = value;
    }
}
