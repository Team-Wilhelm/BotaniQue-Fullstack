using lib;

namespace api.Events.Requirements.Server;

public class ServerSendsRequirementsForPlant : BaseDto
{
    public Shared.Models.Information.Requirements Requirements { get; set; }
}