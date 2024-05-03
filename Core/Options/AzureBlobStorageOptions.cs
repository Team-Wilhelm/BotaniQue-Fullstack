namespace Core.Options;

public class AzureBlobStorageOptions
{
    public string ConnectionString { get; set; } = null!;
    public string PlantImagesContainer { get; set; } = null!;
    public string UserProfileImagesContainer { get; set; } = null!;
}