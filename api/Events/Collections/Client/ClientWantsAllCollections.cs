using api.Core.Services;
using api.Events.Collections.Server;
using api.Extensions;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.Collections.Client;

public class ClientWantsAllCollectionsDto : BaseDtoWithJwt;

public class ClientWantsAllCollections(CollectionsService collectionsService, JwtService jwtService)
    : BaseEventHandler<ClientWantsAllCollectionsDto>
{
    public override async Task Handle(ClientWantsAllCollectionsDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        var collections = await collectionsService.GetCollectionsForUser(email);
        socket.SendDto(new ServerSendsAllCollections
        {
            Collections = collections.ToList()
        });
    }
}