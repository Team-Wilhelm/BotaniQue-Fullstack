using System.Text.Json;
using api.Events.Global;
using lib;
using Websocket.Client;

namespace Tests
{
    public class WebSocketTestClient
    {
        public readonly WebsocketClient Client;
        public readonly List<BaseDto> ReceivedMessages = [];
        private static readonly JsonSerializerOptions JsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public WebSocketTestClient(string? url = null)
        {
            Client = url == null ? new WebsocketClient(new Uri("ws://localhost:" + (Environment.GetEnvironmentVariable("FULLSTACK_API_PORT") ?? "8181"))) : new WebsocketClient(new Uri(url));
            Client.MessageReceived.Subscribe(msg =>
            {
                BaseDto baseDto = JsonSerializer.Deserialize<BaseDto>(msg.Text, JsonSerializerOptions);

                if (baseDto.eventType == "ServerSendsErrorMessage" || baseDto.eventType.Contains("ServerResponds") ||
                    baseDto.eventType.Contains("ServerRejects"))
                {
                    var error = JsonSerializer.Deserialize<ServerSendsErrorMessage>(msg.Text, JsonSerializerOptions);
                    Console.WriteLine("Error: " + error!.Error);
                }

                lock (ReceivedMessages)
                    ReceivedMessages.Add(baseDto);
            });
        }

        public async Task<WebSocketTestClient> ConnectAsync()
        {
            await Client.Start();
            if (!Client.IsRunning)
                throw new Exception("Could not start client!");
            return this;
        }

        public void Send<T>(T dto) where T : BaseDto
        {
            Client.Send(JsonSerializer.Serialize(dto));
        }

        public async Task DoAndAssert<T>(T? action = null, Func<List<BaseDto>, bool>? condition = null) where T : BaseDto
        {
            if ((object) (T) action != null)
                Send(action);
            if (condition != null)
            {
                DateTime startTime = DateTime.UtcNow;
                while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(5.0))
                {
                    lock (ReceivedMessages)
                    {
                        if (condition(ReceivedMessages))
                            return;
                    }
                    await Task.Delay(100);
                }
                throw new TimeoutException("Condition not met: ");
            }
        }
    }
}