namespace Core.Services.External;

public interface IImageBackgroundRemoverService
{
    public Task<byte[]> RemoveBackground(byte[] imageBytes);
}