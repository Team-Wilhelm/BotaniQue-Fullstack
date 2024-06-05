using api.Core.Options;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;

namespace api.Core.Services;

public class MqttClientService
{
    private readonly IMqttClient _mqttClient;
    private readonly IOptions<MqttOptions> _options;
    
    public MqttClientService(IOptions<MqttOptions> options)
    {
        _options = options;
        var factory = new MqttFactory();
        _mqttClient = factory.CreateMqttClient();
    }

    public async Task ConnectAsync()
    {
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Value.Server, _options.Value.Port)
            .WithCredentials(_options.Value.Username)
            .Build();

        await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);
    }
    
    public IMqttClient GetClient()
    {
        return _mqttClient;
    }
}