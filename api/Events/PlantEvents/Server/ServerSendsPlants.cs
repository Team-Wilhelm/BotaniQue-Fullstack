using lib;
using Shared.Models;

namespace api.Events.PlantEvents.Server;

public class ServerSendsPlants : BaseDto
{
    public Guid? CollectionId { get; set; }
    public required List<Plant> Plants { get; set; }
}