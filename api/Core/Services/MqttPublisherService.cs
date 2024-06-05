using System.Text.Json;
using api.Core.Options;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using Shared.Dtos;

namespace api.Core.Services;

public class MqttPublisherService
{
    private readonly IOptions<MqttOptions> _options;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    private readonly IMqttClient _mqttClient;

    
    public MqttPublisherService(IOptions<MqttOptions> options, MqttClientService mqttClientService)
    {
        _options = options;
        _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        _mqttClient = mqttClientService.GetClient();
        
        if (string.IsNullOrEmpty(_options.Value.Username) || _options.Value.Username == "FILL_ME_IN")
            throw new Exception("MQTT username not set in appsettings.json");
    }
    
    public async Task PublishAsync(MoodDto mood, string deviceId)
    {
        var mqttPublishOptions = new MqttApplicationMessageBuilder()
            .WithTopic($"{_options.Value.PublishTopic}/{deviceId}")
            .WithPayload(JsonSerializer.Serialize(mood, options: _jsonSerializerOptions))
            .WithRetainFlag()
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();
        
        await _mqttClient.PublishAsync(mqttPublishOptions, CancellationToken.None);
    }
}