using lib;
using Shared.Models;

namespace api.Events.PlantEvents.Server;

public class ServerCreatesNewPlant : BaseDto
{
    public Plant Plant { get; set; }
}