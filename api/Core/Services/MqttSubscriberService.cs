using System.Text;
using System.Text.Json;
using api.Core.Options;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using Shared.Dtos;

namespace api.Core.Services;

public class MqttSubscriberService
{
    private readonly IOptions<MqttOptions> _options;
    private readonly ConditionsLogsService _conditionsLogService;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IMqttClient _mqttClient;

    public MqttSubscriberService(IOptions<MqttOptions> options, ConditionsLogsService conditionsLogService, MqttClientService mqttClientService)
    {
        _options = options;
        _conditionsLogService = conditionsLogService;
        _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        _mqttClient = mqttClientService.GetClient();

        if (string.IsNullOrEmpty(_options.Value.Username) || _options.Value.Username == "FILL_ME_IN")
            throw new Exception("MQTT username not set in appsettings.json");
    }

    public async Task SubscribeAsync()
    {
        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            Console.WriteLine("Received message");
            var payload = Encoding.UTF8.GetString((ReadOnlySpan<byte>)e.ApplicationMessage.PayloadSegment);
            var conditions = JsonSerializer.Deserialize<CreateConditionsLogDto>(payload, options:
                _jsonSerializerOptions);

            if (conditions is not null)
            {
                await _conditionsLogService.CreateConditionsLogAsync(conditions);
            }
        };

        var mqttFactory = new MqttFactory();
        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder().WithTopicFilter(
                f => { f.WithTopic(_options.Value.SubscribeTopic); }).Build();

        await _mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
    }
}