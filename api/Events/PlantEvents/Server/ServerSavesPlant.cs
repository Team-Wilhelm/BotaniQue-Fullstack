using lib;
using Shared.Models;

namespace api.Events.PlantEvents.Server;

public class ServerSavesPlant : BaseDto
{
    public required Plant Plant { get; set; }
}