using Fleck;
using Shared.Models.Identity;

namespace api;

public class WsWithMetadata(IWebSocketConnection connection)
{
    public IWebSocketConnection Connection { get; set; } = connection;
    public bool IsAuthenticated { get; set; } = false;
    public User? User { get; set; }
}

public class WebSocketConnectionService
{
    private readonly Dictionary<Guid, WsWithMetadata> _connectedClients = new();

    public void AddConnection(IWebSocketConnection connection)
    {
        var clientId = connection.ConnectionInfo.Id;
        _connectedClients.TryAdd(clientId, new WsWithMetadata(connection));
    }
    
    public void AuthenticateConnection(Guid clientId, User user)
    {
        _connectedClients[clientId].IsAuthenticated = true;
        _connectedClients[clientId].User = user;
    }

    public void RemoveConnection(IWebSocketConnection connection)
    {
        var clientId = connection.ConnectionInfo.Id;
        _connectedClients.Remove(clientId);
    }

    public WsWithMetadata GetClient(Guid clientId)
    {
        return _connectedClients[clientId];
    }
}