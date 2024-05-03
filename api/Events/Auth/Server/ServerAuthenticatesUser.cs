using lib;
using Shared.Dtos;

namespace api.Events.Auth.Server;

public class ServerAuthenticatesUser : BaseDto
{
    public string? Jwt { get; set; }
    public GetUserDto GetUserDto { get; set; } = null!;
}