using lib;
using Shared.Models.Information;

namespace api.Events.Conditions.Server;

public class ServerSendsHistoricConditionLogsForPlantDto : BaseDto
{
    public required List<ConditionsLog> ConditionsLogs { get; set; }
}
