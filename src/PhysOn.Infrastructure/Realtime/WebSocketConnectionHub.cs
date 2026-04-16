using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using PhysOn.Application.Abstractions;
using PhysOn.Contracts.Realtime;

namespace PhysOn.Infrastructure.Realtime;

public sealed class WebSocketConnectionHub : IRealtimeNotifier
{
    private readonly IClock _clock;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, ClientConnection>> _connections = new();

    public WebSocketConnectionHub(IClock clock)
    {
        _clock = clock;
    }

    public async Task AcceptConnectionAsync(
        Guid accountId,
        Guid sessionId,
        WebSocket socket,
        CancellationToken cancellationToken)
    {
        var connectionId = Guid.NewGuid().ToString("N");
        var client = new ClientConnection(connectionId, socket, _jsonOptions);
        var accountConnections = _connections.GetOrAdd(accountId, _ => new ConcurrentDictionary<string, ClientConnection>());
        accountConnections[connectionId] = client;

        await client.SendAsync(
            new RealtimeEventEnvelope(
                "session.connected",
                Guid.NewGuid().ToString("N"),
                _clock.UtcNow,
                new SessionConnectedDto(sessionId.ToString())),
            cancellationToken);

        var buffer = new byte[4096];

        try
        {
            while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer, cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
            }
        }
        finally
        {
            accountConnections.TryRemove(connectionId, out _);
            if (accountConnections.IsEmpty)
            {
                _connections.TryRemove(accountId, out _);
            }

            if (socket.State == WebSocketState.Open || socket.State == WebSocketState.CloseReceived)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
            }
        }
    }

    public async Task PublishToAccountsAsync(
        IEnumerable<Guid> accountIds,
        string eventName,
        object payload,
        CancellationToken cancellationToken = default)
    {
        var envelope = new RealtimeEventEnvelope(
            eventName,
            Guid.NewGuid().ToString("N"),
            _clock.UtcNow,
            payload);

        var publishTasks = new List<Task>();
        foreach (var accountId in accountIds.Distinct())
        {
            if (!_connections.TryGetValue(accountId, out var accountConnections))
            {
                continue;
            }

            foreach (var connection in accountConnections.Values)
            {
                publishTasks.Add(connection.SendAsync(envelope, cancellationToken));
            }
        }

        await Task.WhenAll(publishTasks);
    }

    private sealed class ClientConnection
    {
        private readonly SemaphoreSlim _sendLock = new(1, 1);
        private readonly JsonSerializerOptions _jsonOptions;

        public ClientConnection(string id, WebSocket socket, JsonSerializerOptions jsonOptions)
        {
            Id = id;
            Socket = socket;
            _jsonOptions = jsonOptions;
        }

        public string Id { get; }
        public WebSocket Socket { get; }

        public async Task SendAsync(RealtimeEventEnvelope envelope, CancellationToken cancellationToken)
        {
            if (Socket.State != WebSocketState.Open)
            {
                return;
            }

            var payload = JsonSerializer.Serialize(envelope, _jsonOptions);
            var buffer = Encoding.UTF8.GetBytes(payload);

            await _sendLock.WaitAsync(cancellationToken);
            try
            {
                if (Socket.State == WebSocketState.Open)
                {
                    await Socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
                }
            }
            finally
            {
                _sendLock.Release();
            }
        }
    }
}
