namespace api.Core.Options;

public class AzureVisionOptions
{
    public string BaseUrl { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string RemoveBackgroundEndpoint { get; set; } = null!;
}