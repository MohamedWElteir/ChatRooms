namespace ChatRooms.Domain.Shared;

public readonly record struct Result<T>
{
    public T? Value { get; }
    public Error? Error { get; }
    public bool IsSuccess => Error is null;
    public bool IsFailure => Error is not null;

    private Result(T value) { Value = value; Error = null; }
    private Result(Error error) { Value = default; Error = error; }

    public static implicit operator Result<T>(T value) => new(value);
    public static implicit operator Result<T>(Error error) => new(error);

    public void Deconstruct(out T? value, out Error? error)
    {
        value = Value;
        error = Error;
    }
}

public readonly record struct Result
{
    public Error? Error { get; }
    public bool IsSuccess => Error is null;
    public bool IsFailure => Error is not null;

    private Result(Error error) => Error = error;

    public static Result Success() => new();
    public static implicit operator Result(Error error) => new(error);

    public void Deconstruct(out bool isSuccess, out Error? error)
    {
        isSuccess = IsSuccess;
        error = Error;
    }
}
