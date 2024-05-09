using lib;
using Shared.Models;

namespace api.Events.Collections.Server;

public class ServerSendsPlantsForCollection : BaseDto
{
    public List<Plant> Plants { get; set; }
}