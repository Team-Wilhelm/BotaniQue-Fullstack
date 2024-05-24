using api.Core.Services;
using api.Events.Collections.Server;
using api.Extensions;
using Fleck;
using lib;
using Shared.Exceptions;
using Shared.Models;

namespace api.Events.Collections.Client;

public class ClientWantsToDeleteCollectionDto : BaseDtoWithJwt
{
    public Guid CollectionId { get; set; }
}

public class ClientWantsToDeleteCollection(CollectionsService collectionsService, JwtService jwtService) : BaseEventHandler<ClientWantsToDeleteCollectionDto>
{
    public override async Task Handle(ClientWantsToDeleteCollectionDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        await collectionsService.DeleteCollection(dto.CollectionId, email);
        socket.SendDto(new ServerDeletesCollection());
        
    }
}