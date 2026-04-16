using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using PhysOn.Contracts.Conversations;
using PhysOn.Contracts.Realtime;

namespace PhysOn.Desktop.Services;

public enum RealtimeConnectionState
{
    Idle,
    Connecting,
    Connected,
    Reconnecting,
    Disconnected
}

public sealed class PhysOnRealtimeClient : IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);

    private ClientWebSocket? _socket;
    private CancellationTokenSource? _connectionCts;
    private Task? _connectionTask;
    private bool _disposed;

    public event Action<RealtimeConnectionState>? ConnectionStateChanged;
    public event Action<SessionConnectedDto>? SessionConnected;
    public event Action<MessageItemDto>? MessageCreated;
    public event Action<ReadCursorUpdatedDto>? ReadCursorUpdated;

    public async Task ConnectAsync(string wsUrl, string accessToken, CancellationToken cancellationToken = default)
    {
        await _lifecycleLock.WaitAsync(cancellationToken);
        try
        {
            await DisconnectCoreAsync();

            if (string.IsNullOrWhiteSpace(wsUrl) || string.IsNullOrWhiteSpace(accessToken))
            {
                NotifyStateChanged(RealtimeConnectionState.Idle);
                return;
            }

            _connectionCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            _connectionTask = RunConnectionLoopAsync(new Uri(wsUrl), accessToken, _connectionCts.Token);
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _lifecycleLock.WaitAsync(cancellationToken);
        try
        {
            await DisconnectCoreAsync();
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        await DisconnectAsync();
        _lifecycleLock.Dispose();
    }

    private async Task DisconnectCoreAsync()
    {
        var cts = _connectionCts;
        var socket = _socket;
        var task = _connectionTask;

        _connectionCts = null;
        _connectionTask = null;
        _socket = null;

        if (cts is null && socket is null && task is null)
        {
            NotifyStateChanged(RealtimeConnectionState.Idle);
            return;
        }

        try
        {
            cts?.Cancel();
        }
        catch
        {
            // Ignore cancellation races during shutdown.
        }

        if (socket is not null)
        {
            try
            {
                if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "shutdown", CancellationToken.None);
                }
            }
            catch
            {
                // Ignore close failures during shutdown.
            }
            finally
            {
                socket.Dispose();
            }
        }

        if (task is not null)
        {
            try
            {
                await task;
            }
            catch (OperationCanceledException)
            {
                // Expected when the client is disposed or explicitly disconnected.
            }
        }

        cts?.Dispose();
        NotifyStateChanged(RealtimeConnectionState.Idle);
    }

    private async Task RunConnectionLoopAsync(Uri wsUri, string accessToken, CancellationToken cancellationToken)
    {
        var reconnecting = false;

        while (!cancellationToken.IsCancellationRequested)
        {
            using var socket = new ClientWebSocket();
            socket.Options.SetRequestHeader("Authorization", $"Bearer {accessToken}");
            _socket = socket;

            NotifyStateChanged(reconnecting ? RealtimeConnectionState.Reconnecting : RealtimeConnectionState.Connecting);

            try
            {
                await socket.ConnectAsync(wsUri, cancellationToken);
                NotifyStateChanged(RealtimeConnectionState.Connected);
                await ReceiveLoopAsync(socket, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch
            {
                NotifyStateChanged(RealtimeConnectionState.Disconnected);
            }
            finally
            {
                if (ReferenceEquals(_socket, socket))
                {
                    _socket = null;
                }

                try
                {
                    if (socket.State is WebSocketState.Open or WebSocketState.CloseReceived)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "bye", CancellationToken.None);
                    }
                }
                catch
                {
                    // Ignore connection teardown errors.
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            reconnecting = true;
            NotifyStateChanged(RealtimeConnectionState.Disconnected);

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        NotifyStateChanged(RealtimeConnectionState.Idle);
    }

    private async Task ReceiveLoopAsync(ClientWebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];

        while (!cancellationToken.IsCancellationRequested && socket.State == WebSocketState.Open)
        {
            using var stream = new MemoryStream();
            WebSocketReceiveResult result;

            do
            {
                result = await socket.ReceiveAsync(buffer, cancellationToken);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    return;
                }

                if (result.Count > 0)
                {
                    stream.Write(buffer, 0, result.Count);
                }
            } while (!result.EndOfMessage);

            if (result.MessageType != WebSocketMessageType.Text || stream.Length == 0)
            {
                continue;
            }

            stream.Position = 0;
            DispatchIncomingEvent(stream);
        }
    }

    private void DispatchIncomingEvent(Stream payloadStream)
    {
        using var document = JsonDocument.Parse(payloadStream);
        if (!document.RootElement.TryGetProperty("event", out var eventProperty))
        {
            return;
        }

        if (!document.RootElement.TryGetProperty("data", out var dataProperty))
        {
            return;
        }

        var eventName = eventProperty.GetString();
        switch (eventName)
        {
            case "session.connected":
                var sessionConnected = dataProperty.Deserialize<SessionConnectedDto>(JsonOptions);
                if (sessionConnected is not null)
                {
                    SessionConnected?.Invoke(sessionConnected);
                }
                break;

            case "message.created":
                var messageCreated = dataProperty.Deserialize<MessageItemDto>(JsonOptions);
                if (messageCreated is not null)
                {
                    MessageCreated?.Invoke(messageCreated);
                }
                break;

            case "read_cursor.updated":
                var readCursorUpdated = dataProperty.Deserialize<ReadCursorUpdatedDto>(JsonOptions);
                if (readCursorUpdated is not null)
                {
                    ReadCursorUpdated?.Invoke(readCursorUpdated);
                }
                break;
        }
    }

    private void NotifyStateChanged(RealtimeConnectionState state) => ConnectionStateChanged?.Invoke(state);
}
