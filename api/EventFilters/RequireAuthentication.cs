using Fleck;
using lib;

namespace api.EventFilters;

public class RequireAuthentication : BaseEventFilter
{
    public override Task Handle<T>(IWebSocketConnection socket, T dto)
    {
        throw new NotImplementedException();
    }
}