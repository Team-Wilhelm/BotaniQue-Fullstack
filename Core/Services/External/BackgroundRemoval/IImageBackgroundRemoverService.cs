namespace Core.Services.External.BackgroundRemoval;

public interface IImageBackgroundRemoverService
{
    public Task<byte[]> RemoveBackground(byte[] imageBytes);
}