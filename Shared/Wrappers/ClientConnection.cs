using Fleck;

namespace Shared.Wrappers;

public class ClientConnection
{
    public IWebSocketConnection Connection { get; set; }
    public string? Email { get; set; }
}