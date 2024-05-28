using System.ComponentModel.DataAnnotations;
using api.Core.Services;
using api.EventFilters;
using api.Events.Global;
using api.Events.PlantEvents.Server;
using api.Events.Statistics;
using api.Extensions;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.PlantEvents.Client;

public class ClientWantsToDeletePlantDto: BaseDtoWithJwt
{
    [Required] public Guid PlantId { get; set; }
}

[ValidateDataAnnotations]
public class ClientWantsToDeletePlant(PlantService plantService, JwtService jwtService, StatsService statsService): BaseEventHandler<ClientWantsToDeletePlantDto>
{
    public override async Task Handle(ClientWantsToDeletePlantDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        var stats = await statsService.GetStats(email);
        
        await plantService.DeletePlant(dto.PlantId, email);
        socket.SendDto( new ServerConfirmsDelete());
        
        socket.SendDto(new ServerSendsStats{Stats = stats});
    }
}

