using Azure.Storage.Blobs;
using Core.Options;
using Microsoft.Extensions.Options;
using Shared.Exceptions;

namespace Core.Services.External.BlobStorage;

public class BlobStorageServiceService(IOptions<AzureBlobStorageOptions> azureBlobStorageOptions) : IBlobStorageService
{ 
    public async Task<string> SaveImageToBlobStorage(string base64Image, string userEmail, string? blobUrl = null)
    {
        var imageBytes = Convert.FromBase64String(base64Image);
        blobUrl ??= userEmail + "_" + Guid.NewGuid();
        var blobClient = new BlobClient(azureBlobStorageOptions.Value.ConnectionString, azureBlobStorageOptions.Value.PlantImagesContainer, blobUrl);
        var binaryData = new BinaryData(imageBytes);
        await blobClient.UploadAsync(binaryData, true);
        return blobClient.Uri.ToString();
    }
    
    public async Task<bool> DeleteImageFromBlobStorage(string imageUrl)
    {
        var blobClient = new BlobClient(new Uri(imageUrl));
        return await blobClient.DeleteIfExistsAsync();
    }
    
    public async Task<string> GetImageFromBlobStorage(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return "";
        
        var blobClient = new BlobClient(new Uri(imageUrl));
        if (await blobClient.ExistsAsync() == false) throw new NotFoundException("Image not found");
        
        using var memoryStream = new MemoryStream();
        await blobClient.DownloadToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();
        return Convert.ToBase64String(imageBytes);
    }
}