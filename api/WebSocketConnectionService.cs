using Fleck;
using Shared.Exceptions;
using Shared.Models.Identity;

namespace api;

public class WsWithMetadata(IWebSocketConnection connection)
{
    public IWebSocketConnection Connection { get; set; } = connection;
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
        _connectedClients[clientId].User = user;
    }
    
    public void RevertAuthentication(IWebSocketConnection connection)
    {
        var clientId = connection.ConnectionInfo.Id;
        _connectedClients[clientId].User = null;
    }

    public void RemoveConnection(IWebSocketConnection connection)
    {
        var clientId = connection.ConnectionInfo.Id;
        _connectedClients.Remove(clientId);
    }

    public User GetUser(IWebSocketConnection connection)
    {
        var clientId = connection.ConnectionInfo.Id;
        var user = _connectedClients[clientId].User;
        
        if (user is null) throw new NotAuthenticatedException();
        return user;
    }
}