using api;

namespace Tests;

[SetUpFixture]
public class GlobalTestSetup
{
    [OneTimeSetUp]
    public async Task StartApi()
    {
        await Startup.StartApi(["ENVIRONMENT=Development"]);
    }
}