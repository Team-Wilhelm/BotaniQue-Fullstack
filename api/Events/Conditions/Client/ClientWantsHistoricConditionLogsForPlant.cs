using api.Core.Services;
using api.Events.Conditions.Server;
using api.Extensions;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.Conditions.Client;

public class ClientWantsHistoricConditionLogsForPlantDto : BaseDtoWithJwt
{
    public required Guid PlantId { get; set; }
    public required int TimeSpanInDays { get; set; }
}

public class ClientWantsHistoricConditionLogsForPlant(JwtService jwtService, ConditionsLogsService conditionsLogsService) : BaseEventHandler<ClientWantsHistoricConditionLogsForPlantDto>
{
    public override async Task Handle(ClientWantsHistoricConditionLogsForPlantDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        var conditionsLogs = await conditionsLogsService.GetConditionsLogsForPlant(dto.PlantId, dto.TimeSpanInDays, email);
        socket.SendDto(new ServerSendsHistoricConditionLogsForPlantDto
        {
            ConditionsLogs = conditionsLogs
        });
    }
}