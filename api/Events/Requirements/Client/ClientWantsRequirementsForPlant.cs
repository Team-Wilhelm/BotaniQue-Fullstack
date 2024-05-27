using api.Core.Services;
using api.Events.Requirements.Server;
using api.Extensions;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.Requirements.Client;

public class ClientWantsRequirementsForPlantDto : BaseDtoWithJwt
{
    public Guid PlantId { get; set; }
}

public class ClientWantsRequirementsForPlant(RequirementService requirementService) : BaseEventHandler<ClientWantsRequirementsForPlantDto>
{
    public override async Task Handle(ClientWantsRequirementsForPlantDto dto, IWebSocketConnection socket)
    {
        var requirements = await requirementService.GetRequirements(dto.PlantId);
        socket.SendDto(new ServerSendsRequirementsForPlant
        {
            Requirements = requirements
        });
    }
}

