using Core.Services.External.BackgroundRemoval;

namespace Core.Services.External;

public class MockImageBackgroundRemoverService : IImageBackgroundRemoverService
{ 
    public Task<byte[]> RemoveBackground(byte[] imageBytes)
    {
        return Task.FromResult(imageBytes);
    }
}