using lib;
using Shared.Models.Information;

namespace api.Events.Conditions.Server;

public class ServerSendsLatestConditionsForPlant : BaseDto
{
    public required ConditionsLog ConditionsLog { get; set; }
}