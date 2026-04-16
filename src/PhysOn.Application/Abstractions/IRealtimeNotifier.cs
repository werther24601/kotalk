namespace PhysOn.Application.Abstractions;

public interface IRealtimeNotifier
{
    Task PublishToAccountsAsync(
        IEnumerable<Guid> accountIds,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default);
}
