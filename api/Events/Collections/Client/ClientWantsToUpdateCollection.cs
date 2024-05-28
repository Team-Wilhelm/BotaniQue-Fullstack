using api.Core.Services;
using api.Events.Collections.Server;
using api.Extensions;
using Fleck;
using lib;
using Shared.Dtos.FromClient.Collections;
using Shared.Models;

namespace api.Events.Collections.Client;

public class ClientWantsToUpdateCollectionDto : BaseDtoWithJwt
{
    public required UpdateCollectionDto UpdateCollectionDto { get; set; }
}

public class ClientWantsToUpdateCollection(CollectionsService collectionsService, JwtService jwtService) : BaseEventHandler<ClientWantsToUpdateCollectionDto>
{
    public override async Task Handle(ClientWantsToUpdateCollectionDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        await collectionsService.UpdateCollection(dto.UpdateCollectionDto, email);
        var allCollections = await collectionsService.GetCollectionsForUser(email);
        socket.SendDto(new ServerSendsAllCollections()
        {
            Collections = allCollections.ToList()
        });
    }
}