namespace Core.Services.External.BlobStorage;

public class MockBlobStorageService : IBlobStorageService
{
    public Task<string> SaveImageToBlobStorage(string base64Image, string userEmail, string? blobUrl = null)
    {
        return Task.FromResult("https://www.example.com");
    }

    public Task<bool> DeleteImageFromBlobStorage(string imageUrl)
    {
        return Task.FromResult(true);
    }

    public Task<string> GetImageFromBlobStorage(string imageUrl)
    {
        return Task.FromResult("base64Image");
    }
}