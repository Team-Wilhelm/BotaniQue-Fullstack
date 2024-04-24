using api.EventFilters;
using api.Events.Requirements.Server;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Dtos.FromClient.Requirements;
using Shared.Models;

namespace api.Events.Requirements.Client;

public class ClientWantsToCreateRequirementDto : BaseDtoWithJwt
{
    public CreateRequirementsDto CreateRequirementsDto { get; set; }
}

[ValidateDataAnnotations]
public class ClientWantsToCreateRequirement(RequirementService requirementService) : BaseEventHandler<ClientWantsToCreateRequirementDto>
{
    public override async Task Handle(ClientWantsToCreateRequirementDto dto, IWebSocketConnection socket)
    {
        var createRequirementsDto = dto.CreateRequirementsDto;
        var requirements = await requirementService.CreateRequirements(createRequirementsDto);
        var serverSendsRequirements = new ServerSendsRequirementsForPlant()
        {
            Requirements = requirements
        };
        socket.SendDto(serverSendsRequirements);
    }
}