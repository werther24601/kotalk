namespace PhysOn.Contracts.Common;

public sealed record ApiEnvelope<T>(T Data);

public sealed record ListEnvelope<T>(IReadOnlyList<T> Items, string? NextCursor);

public sealed record ApiErrorEnvelope(ApiError Error);

public sealed record ApiError(
    string Code,
    string Message,
    bool Retryable = false,
    IReadOnlyDictionary<string, string>? FieldErrors = null);
