using api.Core.Services;
using api.Events.Collections.Server;
using api.Events.Statistics;
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

public class ClientWantsToDeleteCollection(CollectionsService collectionsService, JwtService jwtService, StatsService statsService) : BaseEventHandler<ClientWantsToDeleteCollectionDto>
{
    public override async Task Handle(ClientWantsToDeleteCollectionDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        await collectionsService.DeleteCollection(dto.CollectionId, email);
        var allCollections = await collectionsService.GetCollectionsForUser(email);
        
        var stats = await statsService.GetStats(email);
        socket.SendDto(new ServerSendsAllCollections()
        {
            Collections = allCollections.ToList()
        });
        
        socket.SendDto(new ServerSendsStats{Stats = stats});
    }
}