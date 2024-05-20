using lib;
using Shared.Models;

namespace api.Events.PlantEvents.Server;

public class ServerSendsCriticalPlants : BaseDto
{
    public required List<GetCriticalPlantDto> Plants { get; set; }
}