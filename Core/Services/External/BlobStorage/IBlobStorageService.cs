namespace Core.Services.External.BlobStorage;

public interface IBlobStorageService
{
    public Task<string> SaveImageToBlobStorage(string base64Image, string userEmail, string? blobUrl = null);
    public Task<bool> DeleteImageFromBlobStorage(string imageUrl);
    public Task<string> GetImageFromBlobStorage(string imageUrl);
}