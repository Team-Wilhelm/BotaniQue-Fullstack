using api.Core.Services;
using api.Events.Collections.Server;
using api.Events.PlantEvents.Server;
using api.Extensions;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.Collections.Client;

public class ClientWantsPlantsForCollectionDto : BaseDtoWithJwt
{
    public Guid CollectionId { get; set; }
}

public class ClientWantsPlantsForCollection(CollectionsService collectionsService, JwtService jwtService) : BaseEventHandler<ClientWantsPlantsForCollectionDto>
{
    public override async Task Handle(ClientWantsPlantsForCollectionDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        var plants = await collectionsService.GetPlantsInCollection(dto.CollectionId, email);
        socket.SendDto(new ServerSendsPlants
        {
            CollectionId = dto.CollectionId,
            Plants = plants.ToList()
        });
    }
}