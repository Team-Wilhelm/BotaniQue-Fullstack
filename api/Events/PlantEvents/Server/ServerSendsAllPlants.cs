using lib;
using Shared.Models;

namespace api.Events.PlantEvents.Server;

public class ServerSendsAllPlantsDto: BaseDto
{
    public List<Plant> Plants { get; set; }
}