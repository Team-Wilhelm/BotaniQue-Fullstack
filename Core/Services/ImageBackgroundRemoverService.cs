using System.Net.Http.Headers;
using System.Text;
using Azure;
using Azure.AI.Vision.ImageAnalysis;
using Core.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Core.Services;

public class ImageBackgroundRemoverService(IOptions<AzureVisionOptions> options)
{
    public async Task<byte[]> RemoveBackground(IFormFile image)
    {
        byte[] bytes = File.ReadAllBytes("api/logo.png");
        string bytesStr = string.Join(",", bytes);
        Console.WriteLine(bytesStr);

        throw new NotImplementedException();
    }

    public async Task<byte[]> RemoveBackground(string imageUrl)
    {
        var request = options.Value.BaseUrl + options.Value.RemoveBackgroundEndpoint;
        
        var url1 =
            "https://media.istockphoto.com/id/1372896722/photo/potted-banana-plant-isolated-on-white-background.jpg?s=612x612&w=0&k=20&c=bioeNAo7zEqALK6jvyGlxeP_Y7h6j0QjuWbwY4E_eP8=";

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", $"{options.Value.Key}");

        var requestBody = new
        {
            url = url1
        };
        
        // Request body
        byte[] byteData = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestBody));

        HttpResponseMessage response;
        using (var content = new ByteArrayContent(byteData))
        {
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            response = await client.PostAsync(request, content);
        }
        
        var responseContent = await response.Content.ReadAsStringAsync();
        Console.WriteLine(responseContent);
        
        return Encoding.UTF8.GetBytes(responseContent);
    }
}