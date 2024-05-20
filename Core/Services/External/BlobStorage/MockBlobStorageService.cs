namespace Core.Services.External.BlobStorage;

public class MockBlobStorageService : IBlobStorageService
{
    public Task<string> SaveImageToBlobStorage(string base64Image, string userEmail, bool isPlantImage, string? blobUrl = null)
    {
        return Task.FromResult("https://www.example.com");
    }

    public Task<bool> DeleteImageFromBlobStorage(string imageUrl, bool isPlantImage)
    {
        return Task.FromResult(true);
    }

    public Task<string> GetImageFromBlobStorage(string imageUrl)
    {
        return Task.FromResult("base64Image");
    }
    
    public string GenerateSasUri(string blobUrl, bool isPlantImage)
    {
        return "https://www.example.com";
    }
    
    public string GetBlobUrlFromSasUri(string sasUri)
    {
        return "https://www.example.com";
    }
}