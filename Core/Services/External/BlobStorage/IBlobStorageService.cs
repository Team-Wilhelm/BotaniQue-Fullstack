using Azure.Storage.Blobs;

namespace Core.Services.External.BlobStorage;

public interface IBlobStorageService
{
    public Task<string> SaveImageToBlobStorage(string base64Image, string userEmail, bool isPlantImage, string? blobUrl = null);
    public Task<bool> DeleteImageFromBlobStorage(string imageUrl);
    public Task<string> GetImageFromBlobStorage(string imageUrl);
    public string GenerateSasUri(string blobUrl, bool isPlantImage);
    public string GetBlobUrlFromSasUri(string sasUri);
}