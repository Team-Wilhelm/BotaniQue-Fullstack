using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.Conditions;

public class ClientWantsLatestConditionsForPlantDto : BaseDtoWithJwt
{
    public Guid PlantId { get; set; }
}

public class ClientWantsLatestConditionsForPlant(JwtService jwtService, ConditionsLogsService conditionsLogsService) : BaseEventHandler<ClientWantsLatestConditionsForPlantDto>
{
    public override async Task Handle(ClientWantsLatestConditionsForPlantDto dto, IWebSocketConnection socket)
    {
        var email = jwtService.GetEmailFromJwt(dto.Jwt!);
        var conditionsLog = await conditionsLogsService.GetLatestConditionsLogForPlant(dto.PlantId, email);
        
        var serverResponse = new ServerSendsLatestConditionsForPlant
        {
            ConditionsLog = conditionsLog
        };
        
        socket.SendDto(serverResponse);
    }
}