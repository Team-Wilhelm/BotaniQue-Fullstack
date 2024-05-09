using lib;

namespace Shared.Models;

public class BaseDtoWithJwt : BaseDto
{
    public string? Jwt { get; init; }
}