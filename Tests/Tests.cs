using api;
using api.Events.Auth.Client;
using api.Events.Auth.Server;
using api.Events.Global;
using lib;
using Shared.Dtos.FromClient;
using Shared.Dtos.FromClient.Identity;

namespace Tests;

[TestFixture]
[NonParallelizable]
public class Tests
{
    [OneTimeSetUp]
    public async Task Setup()
    {
        await Startup.StartApi(["ENVIRONMENT=Development"]);
    }

    [Test]
    public async Task Register()
    {
        var ws = await new WebSocketTestClient().ConnectAsync();
        
        var registerUserDto = GenerateRandomRegisterUserDto();
        await ws.DoAndAssert(new ClientWantsToSignUpDto { RegisterUserDto = registerUserDto },  receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerSignsUserUp)) == 1;
        });
        
        var loginDto = new LoginDto
        {
            Email = registerUserDto.Email,
            Password = registerUserDto.Password
        };

        await ws.DoAndAssert(new ClientWantsToLogInDto { LoginDto = loginDto }, receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerAuthenticatesUser)) == 1;
        });
    }

    [Test]
    public async Task RegisterInvalidEmail()
    {
        var ws = await new WebSocketTestClient().ConnectAsync();
        var registerUserDto = GenerateRandomRegisterUserDto();
        registerUserDto.Email = "invalidEmail";
        await ws.DoAndAssert(new ClientWantsToSignUpDto { RegisterUserDto = registerUserDto },  receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerSignsUserUp)) == 0 
                   && receivedMessages.Count(e => e.eventType == nameof(ServerRespondsValidationError)) == 1;
        });
    }
    
    [Test]
    public async Task RegisterInvalidPassword()
    {
        var ws = await new WebSocketTestClient().ConnectAsync();
        var registerUserDto = GenerateRandomRegisterUserDto();
        registerUserDto.Password = "no";
        await ws.DoAndAssert(new ClientWantsToSignUpDto { RegisterUserDto = registerUserDto },  receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerSignsUserUp)) == 0 
                   && receivedMessages.Count(e => e.eventType == nameof(ServerRespondsValidationError)) == 1;
        });
    }
    
    [Test]
    public async Task RegisterInvalidUsername()
    {
        var ws = await new WebSocketTestClient().ConnectAsync();

        var registerUserDto = GenerateRandomRegisterUserDto();
        registerUserDto.Username =
            "IWantToHaveTheLongestUsernameInTheUnivereseSoICanBreakThisAppIAmSoEvilHaHaHaHaIHopeThisIsMoreThan50Characters";
        await ws.DoAndAssert(new ClientWantsToSignUpDto { RegisterUserDto = registerUserDto },  receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerSignsUserUp)) == 0 
                   && receivedMessages.Count(e => e.eventType == nameof(ServerRespondsValidationError)) == 1;
        });
    }

    private RegisterUserDto GenerateRandomRegisterUserDto()
    {
        var registerUserDto = new RegisterUserDto
        {
            Email = $"user{Guid.NewGuid()}@app.com",
            Password = "verySecurePassword123",
            Username = "Vladimir"
        };
        return registerUserDto;
    }
}