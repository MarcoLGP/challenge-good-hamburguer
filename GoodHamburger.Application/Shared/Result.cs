namespace GoodHamburger.Application.Shared;

public sealed record Result(IReadOnlyList<ApplicationError> Errors)
{
    public bool IsSuccess => Errors.Count == 0;

    public static Result Success() => new(Array.Empty<ApplicationError>());

    public static Result Failure(params ApplicationError[] errors) => new(errors);
}

public sealed record Result<T>(IReadOnlyList<ApplicationError> Errors, T? Value)
{
    public bool IsSuccess => Errors.Count == 0;

    public static Result<T> Success(T value) => new(Array.Empty<ApplicationError>(), value);

    public static Result<T> Failure(params ApplicationError[] errors) => new(errors, default);
}
