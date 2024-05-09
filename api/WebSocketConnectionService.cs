using Fleck;

namespace api;

public class WebSocketConnectionService
{
    private readonly Dictionary<Guid, IWebSocketConnection> _connectedClients = new();

    public void AddConnection(IWebSocketConnection connection)
    {
        var clientId = connection.ConnectionInfo.Id;
        _connectedClients.TryAdd(clientId, connection);
    }

    public void RemoveConnection(IWebSocketConnection connection)
    {
        var clientId = connection.ConnectionInfo.Id;
        _connectedClients.Remove(clientId);
    }
}