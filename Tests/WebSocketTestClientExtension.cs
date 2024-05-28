using System.Text.Json;
using api.Events.Global;
using lib;
using Websocket.Client;

namespace Tests;

public static class WebSocketTestClientExtension
{
    public static void SubscribeToErrors(this WebSocketTestClient client)
    {
        client.Client.MessageReceived.Subscribe<ResponseMessage>(msg =>
        {
            var baseDto = JsonSerializer.Deserialize<BaseDto>(msg.Text);
            if (baseDto is ServerSendsErrorMessage errorMessage)
            {
                Console.WriteLine(errorMessage.Error);
            }
        });
    }
}