using api;

namespace Tests;

[SetUpFixture]
public class GlobalTestSetup
{
    [OneTimeSetUp]
    public async Task StartApi()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
        await Startup.StartApi(["--db-init"]);
    }
}