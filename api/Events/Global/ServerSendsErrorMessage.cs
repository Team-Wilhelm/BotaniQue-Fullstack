using lib;

namespace api.Events.Global;

public class ServerSendsErrorMessage : BaseDto
{
    public string Error { get; set; } = null!;
}