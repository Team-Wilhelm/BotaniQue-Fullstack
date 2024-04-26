using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Models;

namespace api.Events.ImageUpload;

public class ClientWantsToRemoveBackgroundFromImageDto : BaseDtoWithJwt
{
    public required string ImageUrl { get; set; }
}

public class ClientWantsToRemoveBackgroundFromImage(ImageBackgroundRemoverService backgroundRemoverService) : BaseEventHandler<ClientWantsToRemoveBackgroundFromImageDto>
{
    private string[] allowedExtensions = [".jpg", ".jpeg", ".png"];
    
    public override async Task Handle(ClientWantsToRemoveBackgroundFromImageDto dto, IWebSocketConnection socket)
    {
        /*if (image.Length == 0) throw new InvalidFileFormatException("File is empty.");
        
        var extension = Path.GetExtension(image.FileName);
        if (!allowedExtensions.Contains(extension)) throw new InvalidFileFormatException("Invalid file format. Please upload a valid file.");*/
        
        var removedBackgroundImage = await backgroundRemoverService.RemoveBackground(dto.ImageUrl);
        socket.SendDto(new ServerSendsImageWithoutBackground
        {
            Image = removedBackgroundImage
        });
    }
}

