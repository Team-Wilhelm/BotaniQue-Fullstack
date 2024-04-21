using lib;
using Shared.Models;

namespace api.Events.PlantEvents.Server;

public class ServerSendsPlant: BaseDto
{
    public Plant Plant { get; set; }
}