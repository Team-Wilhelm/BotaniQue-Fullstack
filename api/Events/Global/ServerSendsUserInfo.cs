using lib;
using Shared.Dtos;

namespace api.Events.Global;

public class ServerSendsUserInfo : BaseDto
{
    public GetUserDto GetUserDto { get; set; } = null!;
}