using api.EventFilters;
using api.Events.Global;
using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.Requirements.Client;

public class ClientWantsToDeleteRequirementDto : BaseDtoWithJwt
{
    public Guid RequirementId { get; set; }
}

public class ClientWantsToDeleteRequirement(RequirementService requirementService) : BaseEventHandler<ClientWantsToDeleteRequirementDto>
{
    public override async Task Handle(ClientWantsToDeleteRequirementDto dto, IWebSocketConnection socket)
    {
        await requirementService.DeleteRequirements(dto.RequirementId);
        socket.SendDto(new ServerConfirmsDelete());
    }
}