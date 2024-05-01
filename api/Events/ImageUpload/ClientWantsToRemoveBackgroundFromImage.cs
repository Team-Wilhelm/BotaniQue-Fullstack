using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Models;
using Shared.Models.Exceptions;

namespace api.Events.ImageUpload;

public class ClientWantsToRemoveBackgroundFromImageDto : BaseDtoWithJwt
{
    public required string Base64Image { get; set; }
}

public class ClientWantsToRemoveBackgroundFromImage(ImageBackgroundRemoverService backgroundRemoverService) : BaseEventHandler<ClientWantsToRemoveBackgroundFromImageDto>
{
    public override async Task Handle(ClientWantsToRemoveBackgroundFromImageDto dto, IWebSocketConnection socket)
    {
        byte[] imageBytes = Convert.FromBase64String(dto.Base64Image);
        
        if (imageBytes.Length == 0) throw new InvalidFileFormatException("File is empty.");
        
        var removedBackgroundImage = await backgroundRemoverService.RemoveBackground(imageBytes);
        var removedBackgroundImageBase64 = Convert.ToBase64String(removedBackgroundImage);
        socket.SendDto(new ServerSendsImageWithoutBackground
        {
            Base64Image = removedBackgroundImageBase64
        });
    }
}

