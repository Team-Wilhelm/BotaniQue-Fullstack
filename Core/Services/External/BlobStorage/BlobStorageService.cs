using System.Net;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Core.Options;
using Microsoft.Extensions.Options;
using Shared.Exceptions;

namespace Core.Services.External.BlobStorage;

public class BlobStorageService(IOptions<AzureBlobStorageOptions> azureBlobStorageOptions) : IBlobStorageService
{ 
    public async Task<string> SaveImageToBlobStorage(string base64Image, string userEmail, bool isPlantImage, string? blobUrl = null)
    {
        var imageBytes = Convert.FromBase64String(base64Image);
        var blobName = blobUrl is not null 
            ? GetBlobNameFromUrl(blobUrl, isPlantImage) 
            : userEmail + "_" + Guid.NewGuid();
       
        var container = isPlantImage ? azureBlobStorageOptions.Value.PlantImagesContainer : azureBlobStorageOptions.Value.UserProfileImagesContainer;
        var blobClient = new BlobClient(azureBlobStorageOptions.Value.ConnectionString, container, blobName);
        var binaryData = new BinaryData(imageBytes);
        await blobClient.UploadAsync(binaryData, true);
        return WebUtility.UrlDecode(blobClient.Uri.ToString());
    }
    
    public async Task<bool> DeleteImageFromBlobStorage(string blobUrl, bool isPlantImage)
    {
        var blobName = GetBlobNameFromUrl(blobUrl, isPlantImage);
       
        var container = isPlantImage ? azureBlobStorageOptions.Value.PlantImagesContainer : azureBlobStorageOptions.Value.UserProfileImagesContainer;
        var blobClient = new BlobClient(azureBlobStorageOptions.Value.ConnectionString, container, blobName);
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

    public string GenerateSasUri(string blobUrl, bool isPlantImage)
    {
        var blobServiceClient = new BlobServiceClient(azureBlobStorageOptions.Value.ConnectionString);
        var container = isPlantImage ? azureBlobStorageOptions.Value.PlantImagesContainer : azureBlobStorageOptions.Value.UserProfileImagesContainer;
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(container);
        var blobClient = blobContainerClient.GetBlobClient(GetBlobNameFromUrl(blobUrl, isPlantImage));
        
        var blobSasBuilder = new BlobSasBuilder
        {
            BlobContainerName = blobContainerClient.Name,
            BlobName = blobClient.Name,
            Resource = "b",
            StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1),
        };
        blobSasBuilder.SetPermissions(BlobSasPermissions.Read);
        return blobClient.GenerateSasUri(blobSasBuilder).ToString();
    }
    
    public string GetBlobUrlFromSasUri(string sasUri)
    {
        var blobUriBuilder = new BlobUriBuilder(new Uri(sasUri))
        {
            Query = string.Empty
        };
        return blobUriBuilder.ToString();
    }
    
    private string GetBlobNameFromUrl(string blobUrl, bool isPlantImage)
    {
        var container = isPlantImage ? azureBlobStorageOptions.Value.PlantImagesContainer : azureBlobStorageOptions.Value.UserProfileImagesContainer;
        return WebUtility.UrlDecode(new Uri(blobUrl).AbsolutePath.Substring(container.Length + 2));
    }
}