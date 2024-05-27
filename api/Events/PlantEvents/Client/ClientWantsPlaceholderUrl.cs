using api.Core.Options;
using api.Core.Services.External.BlobStorage;
using api.Events.PlantEvents.Server;
using api.Extensions;
using Fleck;
using lib;
using Microsoft.Extensions.Options;
using Shared.Models;

namespace api.Events.PlantEvents.Client;

public class ClientWantsPlaceholderUrlDto : BaseDtoWithJwt;

public class ClientWantsPlaceholderUrl(IOptions<AzureBlobStorageOptions> blobOptions, IBlobStorageService blobStorageService) : BaseEventHandler<ClientWantsPlaceholderUrlDto>
{
    public override Task Handle(ClientWantsPlaceholderUrlDto dto, IWebSocketConnection socket)
    {
        var placeholderUrl = blobOptions.Value.DefaultPlantImageUrl;
        var sasUri = blobStorageService.GenerateSasUri(placeholderUrl, true);
        socket.SendDto(new ServerSendsPlaceholderUrl
        {
            PlaceholderUrl = sasUri
        });
        return Task.CompletedTask;
    }
}