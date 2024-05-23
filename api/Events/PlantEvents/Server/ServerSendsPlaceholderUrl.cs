using lib;

namespace api.Events.PlantEvents.Server;

public class ServerSendsPlaceholderUrl : BaseDto
{
    public string PlaceholderUrl { get; set; } = null!;
}