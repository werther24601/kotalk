namespace PhysOn.Contracts.Realtime;

public sealed record RealtimeEventEnvelope(
    string Event,
    string EventId,
    DateTimeOffset OccurredAt,
    object Data);

public sealed record SessionConnectedDto(string SessionId);
