using api.Extensions;
using Core.Services;
using Fleck;
using lib;
using Shared.Models;
using Shared.Models.Exceptions;

namespace api.Events.ImageUpload;

public class ClientWantsToRemoveBackgroundFromImageDto : BaseDtoWithJwt
{
    // public required IFormFile Image { get; set; }
}

public class ClientWantsToRemoveBackgroundFromImage(ImageBackgroundRemoverService backgroundRemoverService) : BaseEventHandler<ClientWantsToRemoveBackgroundFromImageDto>
{
    private string[] allowedExtensions = [".jpg", ".jpeg", ".png"];
    
    public override async Task Handle(ClientWantsToRemoveBackgroundFromImageDto dto, IWebSocketConnection socket)
    {
        /*var image = dto.Image;
        
        if (image.Length == 0) throw new InvalidFileFormatException("File is empty.");
        
        var extension = Path.GetExtension(image.FileName);
        if (!allowedExtensions.Contains(extension)) throw new InvalidFileFormatException("Invalid file format. Please upload a valid file.");*/
        
        var removedBackgroundImage = await backgroundRemoverService.RemoveBackground("hello");
        socket.SendDto(new ServerSendsImageWithoutBackground
        {
            Image = removedBackgroundImage
        });
    }
}

