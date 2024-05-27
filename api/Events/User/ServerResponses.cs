using api.Events.Global;
using lib;

namespace api.Events.User;

public class ServerConfirmsUpdateUsername : BaseDto
{
    public string Username { get; set; } = null!;
}

public class ServerConfirmsUpdatePassword : BaseDto
{
}
public class ServerConfirmsProfileImageUpdate : BaseDto
{
    public string BlobUrl { get; set; } = null!;
}

public class ServerConfirmsDeleteProfileImage : BaseDto
{
}
public class ServerRejectsUpdate : ServerSendsErrorMessage
{
}