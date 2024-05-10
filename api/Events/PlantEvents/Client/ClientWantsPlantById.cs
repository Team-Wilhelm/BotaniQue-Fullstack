using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using api.EventFilters;
using api.Events.PlantEvents.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.PlantEvents.Client;

public class ClientWantsPlantByIdDto: BaseDtoWithJwt
{
    [Required] public Guid PlantId { get; set; }
}

[ValidateDataAnnotations]
public class ClientWantsPlantById(PlantService plantService, JwtService jwtService): BaseEventHandler<ClientWantsPlantByIdDto>
{
    public override async Task Handle(ClientWantsPlantByIdDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        var plant = await plantService.GetPlantById(dto.PlantId, email);
        socket.SendDto(new ServerSendsPlant
        {
            Plant = plant
        });
    }
}