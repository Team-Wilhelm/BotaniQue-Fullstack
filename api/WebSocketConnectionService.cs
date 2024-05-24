using Fleck;
using Shared;
using Shared.Wrappers;

namespace api;

public class WebSocketConnectionService
{
    private readonly Dictionary<Guid, ClientConnection> _connectedClients = new();

    public void AddConnection(IWebSocketConnection connection, string email)
    {
        var clientId = connection.ConnectionInfo.Id;
        _connectedClients.TryAdd(clientId, new ClientConnection(connection, email));
    }

    public void RemoveConnection(IWebSocketConnection connection)
    {
        var clientId = connection.ConnectionInfo.Id;
        _connectedClients.Remove(clientId);
    }

    public IWebSocketConnection? GetConnectionByEmail(string email)
    {
        return _connectedClients.Values.FirstOrDefault(clientConnection => clientConnection.Email == email)?.Connection;
    }
}