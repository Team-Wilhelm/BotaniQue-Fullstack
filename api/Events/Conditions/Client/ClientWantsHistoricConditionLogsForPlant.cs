using api.Events.Conditions.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.Conditions.Client;

public class ClientWantsHistoricConditionLogsForPlantDto : BaseDtoWithJwt
{
    public required Guid PlantId { get; set; }
    public required DateTime StartDate { get; set; }
    public required DateTime EndDate { get; set; }
}

public class ClientWantsHistoricConditionLogsForPlant(JwtService jwtService, ConditionsLogsService conditionsLogsService) : BaseEventHandler<ClientWantsHistoricConditionLogsForPlantDto>
{
    public override async Task Handle(ClientWantsHistoricConditionLogsForPlantDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        var conditionsLogs = await conditionsLogsService.GetConditionsLogsForPlant(dto.PlantId, dto.StartDate, dto.EndDate, email);
        socket.SendDto(new ServerSendsHistoricConditionLogsForPlantDto
        {
            ConditionsLogs = conditionsLogs
        });
    }
}