using System.ComponentModel.DataAnnotations;
using api.EventFilters;
using api.Events.PlantEvents.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.PlantEvents.Client;

public class ClientWantsAllPlantsDto: BaseDtoWithJwt
{
    [Required, EmailAddress] public string UserEmail { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; }
    [Range(1, int.MaxValue)] public int PageSize { get; set; }
}

[ValidateDataAnnotations]
public class ClientWAntsAllPlants(PlantService plantService): BaseEventHandler<ClientWantsAllPlantsDto>
{
    public override async Task Handle(ClientWantsAllPlantsDto dto, IWebSocketConnection socket)
    {
        var plants = await plantService.GetPlantsForUser(dto.UserEmail, dto.PageNumber, dto.PageSize);
        var serverSendsAllPlantsDto = new ServerSendsAllPlantsDto
        {
            Plants = plants
        };
        socket.SendDto(serverSendsAllPlantsDto);
    }
}