using System.Net;
using System.Net.Http.Headers;
using Core.Options;
using Microsoft.Extensions.Options;
using Shared.Exceptions;

namespace Core.Services.External.BackgroundRemoval;

public class ImageBackgroundRemoverService(IOptions<AzureVisionOptions> options) : IImageBackgroundRemoverService
{
    public async Task<byte[]> RemoveBackground(byte[] imageBytes)
    {
        var request = options.Value.BaseUrl + options.Value.RemoveBackgroundEndpoint;

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", $"{options.Value.Key}");

        HttpResponseMessage response;
        using (var content = new ByteArrayContent(imageBytes))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            response = await client.PostAsync(request, content);
        }

        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new AppException("Failed to remove background from image.");
        }
        
        // The response is image/png
        var removedBgImageBytes = await response.Content.ReadAsByteArrayAsync();
        return removedBgImageBytes;
    }
}