namespace api.Core.Services.External.BackgroundRemoval;

public class MockImageBackgroundRemoverService : IImageBackgroundRemoverService
{ 
    public Task<byte[]> RemoveBackground(byte[] imageBytes)
    {
        return Task.FromResult(imageBytes);
    }
}