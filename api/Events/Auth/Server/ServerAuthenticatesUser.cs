using lib;

namespace api.Events.Auth.Server;

public class ServerAuthenticatesUser : BaseDto
{
    public string? Jwt { get; set; }
}