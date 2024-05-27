using api.Core.Services;
using api.Events.Conditions.Server;
using api.Extensions;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.Conditions.Client;

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