using lib;
using Shared.Models.Information;

namespace api.Events.Conditions;

public class ServerSendsLatestConditionsForPlant : BaseDto
{
    public required ConditionsLog ConditionsLog;
}