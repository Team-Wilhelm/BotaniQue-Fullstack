using lib;
using Shared.Dtos;

namespace api.Events.Collections.Server;

public class ServerSendsAllCollections : BaseDto
{
    public required List<GetCollectionDto> Collections { get; set; }
}