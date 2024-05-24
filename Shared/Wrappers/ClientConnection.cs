using Fleck;

namespace Shared;

public class ClientConnection(IWebSocketConnection connection, string email)
{
    public IWebSocketConnection Connection { get; } = connection;
    public string Email { get; } = email;
}