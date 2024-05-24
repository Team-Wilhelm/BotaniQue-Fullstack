using Fleck;
using Shared;

namespace api;

public class WebSocketConnectionService
{
    private readonly Dictionary<Guid, ClientConnection> _connectedClients = new();

    public void AddConnection(IWebSocketConnection connection, string email)
    {
        var clientId = connection.ConnectionInfo.Id;
        var existingConnection = _connectedClients.Values.FirstOrDefault(clientConnection => clientConnection.Email == email);

        if (existingConnection != null)
        {
            _connectedClients.Remove(existingConnection.Connection.ConnectionInfo.Id);
        }

        _connectedClients.TryAdd(clientId, new ClientConnection(connection, email));
    }

    public void RemoveConnection(IWebSocketConnection connection)
    {
        var clientId = connection.ConnectionInfo.Id;
        _connectedClients.Remove(clientId);
    }

    public ClientConnection? GetConnectionByEmail(string email)
    {
        return _connectedClients.Values.FirstOrDefault(clientConnection => clientConnection.Email == email);
    }
}