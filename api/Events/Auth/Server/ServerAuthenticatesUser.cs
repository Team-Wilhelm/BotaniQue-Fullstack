using lib;
using Shared.Dtos;

namespace api.Events.Auth.Server;

public class ServerAuthenticatesUser : BaseDto
{
    public required string Jwt { get; init; }
}