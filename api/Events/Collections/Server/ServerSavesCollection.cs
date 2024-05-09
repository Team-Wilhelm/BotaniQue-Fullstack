using lib;
using Shared.Models;

namespace api.Events.Collections.Server;

public class ServerSavesCollection : BaseDto
{
    public Collection Collection { get; set; }
}