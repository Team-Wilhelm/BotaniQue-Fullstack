using lib;
using Shared.Models;

namespace api.Events.PlantEvents.Server;

public class ServerSendsAllPlants: BaseDto
{
    public List<Plant> Plants { get; set; }
}