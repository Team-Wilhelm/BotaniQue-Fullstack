using System.ComponentModel.DataAnnotations;
using api.EventFilters;
using api.Events.Collections.Server;
using api.Events.PlantEvents.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.PlantEvents.Client;

public class ClientWantsAllPlantsDto: BaseDtoWithJwt
{
    [Range(1, int.MaxValue)] public int PageNumber { get; set; }
    [Range(1, int.MaxValue)] public int PageSize { get; set; }
}

[ValidateDataAnnotations]
public class ClientWantsAllPlants(PlantService plantService, JwtService jwtService): BaseEventHandler<ClientWantsAllPlantsDto>
{
    public override async Task Handle(ClientWantsAllPlantsDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        var plants = await plantService.GetPlantsForUser(email, dto.PageNumber, dto.PageSize);
        var serverSendsAllPlantsDto = new ServerSendsPlantsForCollection
        {
            Plants = plants
        };
        socket.SendDto(serverSendsAllPlantsDto);
    }
}