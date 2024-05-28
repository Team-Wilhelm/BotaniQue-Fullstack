using api.Core.Services;
using api.Events.Collections.Server;
using api.Events.Statistics;
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

public class ClientWantsToCreateCollection(CollectionsService collectionsService, JwtService jwtService, StatsService statsService) : BaseEventHandler<ClientWantsToCreateCollectionDto>
{
    public override async Task Handle(ClientWantsToCreateCollectionDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        await collectionsService.CreateCollection(dto.CreateCollectionDto, email);
        var allCollections = await collectionsService.GetCollectionsForUser(email);
        var stats = await statsService.GetStats(email);
        
        socket.SendDto(new ServerSendsAllCollections
        {
            Collections = allCollections.ToList()
        });
        
        socket.SendDto(new ServerSendsStats{Stats = stats});
    }
}