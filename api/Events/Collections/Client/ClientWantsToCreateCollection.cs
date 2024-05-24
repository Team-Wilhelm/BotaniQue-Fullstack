using api.Core.Services;
using api.Events.Collections.Server;
using api.Extensions;
using Fleck;
using lib;
using Shared.Dtos.FromClient.Collections;
using Shared.Models;

namespace api.Events.Collections.Client;

public class ClientWantsToCreateCollectionDto : BaseDtoWithJwt
{
    public required CreateCollectionDto CreateCollectionDto { get; set; }
}

public class ClientWantsToCreateCollection(CollectionsService collectionsService, JwtService jwtService) : BaseEventHandler<ClientWantsToCreateCollectionDto>
{
    public override async Task Handle(ClientWantsToCreateCollectionDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        await collectionsService.CreateCollection(dto.CreateCollectionDto, email);
        var allCollections = await collectionsService.GetCollectionsForUser(email);
        socket.SendDto(new ServerSendsAllCollections()
        {
            Collections = allCollections.ToList()
        });
    }
}