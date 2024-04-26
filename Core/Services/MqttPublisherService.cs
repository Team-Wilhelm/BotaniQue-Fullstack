using System.Text.Json;
using Core.Options;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using Shared.Dtos;

namespace Core.Services;

public class MqttPublisherService
{
    private readonly IOptions<MqttOptions> _options;
    
    public MqttPublisherService(IOptions<MqttOptions> options)
    {
        _options = options;
        
        if (string.IsNullOrEmpty(_options.Value.Username) || _options.Value.Username == "FILL_ME_IN")
            throw new Exception("MQTT username not set in appsettings.json");
    }
    public async Task PublishAsync(MoodDto mood, long deviceId)
    {
        var mqttFactory = new MqttFactory();
       
        using var mqttClient = mqttFactory.CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Value.Server, _options.Value.Port)
            .WithCredentials(_options.Value.Username)
            .Build();
        
        await mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
        
        var mqttPublishOptions = new MqttApplicationMessageBuilder()
            .WithTopic($"{_options.Value.PublishTopic}/{deviceId}")
            .WithPayload(JsonSerializer.Serialize(mood, options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase}))
            .WithRetainFlag()
            .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
            .Build();
        
        await mqttClient.PublishAsync(mqttPublishOptions, CancellationToken.None);
    }
}