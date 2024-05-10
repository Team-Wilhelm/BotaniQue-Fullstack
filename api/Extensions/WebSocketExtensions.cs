using System.Text.Json;
using Fleck;
using lib;
using Serilog;

namespace api.Extensions;

public static class WebSocketExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static void SendDto<T>(this IWebSocketConnection ws, T dto) where T : BaseDto
    {
        Log.Information("Sending message: {message}", JsonSerializer.Serialize(dto, Options));
        ws.Send(JsonSerializer.Serialize(dto, Options) ?? throw new ArgumentException("Failed to serialize dto"));
    }
}