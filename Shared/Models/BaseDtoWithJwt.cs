using lib;

namespace Shared.Models;

public class BaseDtoWithJwt : BaseDto
{
    public required string Jwt { get; init; }
}