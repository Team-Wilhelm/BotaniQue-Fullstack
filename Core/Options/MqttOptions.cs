namespace api.Options;

public class MqttOptions
{
    public string Server { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string ClientId { get; set; }
    public string Topic { get; set; }
}