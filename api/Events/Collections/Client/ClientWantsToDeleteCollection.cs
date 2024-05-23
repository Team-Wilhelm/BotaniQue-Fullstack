using api.Events.Collections.Server;
using api.Events.User.ServerResponses;
using api.Extensions;
using Core.Services;
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
        try
        {
            var email = jwtService.GetEmailFromJwt(dto.Jwt!);
            await collectionsService.DeleteCollection(dto.CollectionId, email);
            socket.SendDto(new ServerDeletesCollection());
        }
        catch (Exception e) when (e is not NotFoundException)
        {
            socket.SendDto(new ServerRejectsUpdate
            {
                Error = "Could not delete collection. Please try again later."
            });
        }
    }
}