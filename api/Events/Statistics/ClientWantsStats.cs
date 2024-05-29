using api.Core.Services;
using api.Extensions;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.Statistics;

public class ClientWantsStatsDto : BaseDtoWithJwt
{
    
}

public class ClientWantsStats(StatsService statsService, JwtService jwtService) : BaseEventHandler<ClientWantsStatsDto>
{
    public override async Task Handle(ClientWantsStatsDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);

        var stats = await statsService.GetStats(email);
        socket.SendDto(new ServerSendsStats{Stats = stats});
    }
}

public class ServerSendsStats : BaseDto
{
    public Stats Stats { get; set; } = null!;
}
