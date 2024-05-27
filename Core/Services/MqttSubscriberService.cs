using System.Text;
using System.Text.Json;
using Core.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using Shared.Dtos;

namespace Core.Services;

public class MqttSubscriberService : IHostedService
{
    private readonly IOptions<MqttOptions> _options;
    private readonly ConditionsLogsService _conditionsLogService;
    private IMqttClient? _mqttClient;

    public MqttSubscriberService(IOptions<MqttOptions> options, ConditionsLogsService conditionsLogService)
    {
        _options = options;
        _conditionsLogService = conditionsLogService;

        if (string.IsNullOrEmpty(_options.Value.Username) || _options.Value.Username == "FILL_ME_IN")
            throw new Exception("MQTT username not set in appsettings.json");
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var mqttFactory = new MqttFactory();
       
        _mqttClient = mqttFactory.CreateMqttClient();
        var mqttClientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(_options.Value.Server, _options.Value.Port)
            .WithCredentials(_options.Value.Username)
            .Build();
        
        _mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
            var conditions = JsonSerializer.Deserialize<CreateConditionsLogDto>(payload, options:
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (conditions is null) return;
            
            await _conditionsLogService.CreateConditionsLogAsync(conditions);
        };

        await _mqttClient.ConnectAsync(mqttClientOptions, CancellationToken.None);

        var mqttSubscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
            .WithTopicFilter(
                f => { f.WithTopic(_options.Value.SubscribeTopic); })
            .Build();

        await _mqttClient.SubscribeAsync(mqttSubscribeOptions, CancellationToken.None);
        
        Console.WriteLine($"Subscribed to {_options.Value.SubscribeTopic}");
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_mqttClient != null)
        {
            await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
            _mqttClient.Dispose();
        }
    }
}