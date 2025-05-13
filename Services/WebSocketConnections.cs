using System.Net.WebSockets;
using System.Collections.Concurrent;
using System.Text;

namespace backend.Services;

public class WebSocketConnections
{
    private readonly ConcurrentDictionary<string, WebSocket> _connections = new();

    public string AddConnection(WebSocket socket)
    {
        var connectionId = Guid.NewGuid().ToString();
        _connections.TryAdd(connectionId, socket);
        return connectionId;
    }

    public void RemoveConnection(string connectionId)
    {
        _connections.TryRemove(connectionId, out _);
    }

    public async Task BroadcastAsync(string message)
    {
        foreach (var (_, socket) in _connections)
        {
            if (socket.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }
    }
}