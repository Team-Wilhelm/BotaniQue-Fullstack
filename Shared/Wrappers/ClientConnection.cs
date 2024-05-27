using Fleck;

namespace Shared.Wrappers;

public class ClientConnection
{
    public required IWebSocketConnection Connection { get; set; }
    public string? Email { get; set; }
}