using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Core.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.Exceptions;

namespace Core.Services;

public class ImageBackgroundRemoverService(IOptions<AzureVisionOptions> options)
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
        var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{Guid.NewGuid()}.png");
        await File.WriteAllBytesAsync(filePath, removedBgImageBytes);
        
        return removedBgImageBytes;
    }
}