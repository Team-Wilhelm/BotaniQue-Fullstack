using lib;
using Shared.Models;

namespace api.Events.Collections.Server;

public class ServerSendsAllCollections : BaseDto
{
    public List<Collection> Collections { get; set; }
}