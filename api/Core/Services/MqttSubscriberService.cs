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

    public MqttSubscriberService(IOptions<MqttOptions> options, ConditionsLogsService conditionsLogService)
    {
        _options = options;
        _conditionsLogService = conditionsLogService;
        _jsonSerializerOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        if (string.IsNullOrEmpty(_options.Value.Username) || _options.Value.Username == "FILL_ME_IN")
            throw new Exception("MQTT username not set in appsettings.json");
    }

    public async Task SubscribeAsync()
    {
        var mqttFactory = new MqttFactory();
       
        using var mqttClient = mqttFactory.CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Value.Server, _options.Value.Port)
            .WithCredentials(_options.Value.Username)
            .Build();
        
        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var payload = Encoding.UTF8.GetString((ReadOnlySpan<byte>)e.ApplicationMessage.PayloadSegment);
            var conditions = JsonSerializer.Deserialize<CreateConditionsLogDto>(payload, options:
                _jsonSerializerOptions);
            
            if (conditions is null) return;
            
            await _conditionsLogService.CreateConditionsLogAsync(conditions);
        };

        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f => { f.WithTopic(_options.Value.SubscribeTopic); })
            .Build();

        await mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
        
        Console.ReadLine();
    }
}