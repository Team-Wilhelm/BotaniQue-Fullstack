using api.Events.Auth.Client;
using api.Events.Auth.Server;
using api.Events.Global;
using lib;
using Shared.Dtos.FromClient.Identity;

namespace Tests;

public class AuthTests : TestBase
{
    [Test]
    public async Task RegisterLogInLogOut()
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
        
        await ws.DoAndAssert(new ClientWantsToLogOutDto { UserEmail = registerUserDto.Email }, receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerLogsOutUser)) == 1;
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

    [Test]
    public async Task RegisterUserAlreadyExists()
    {
        var ws = await new WebSocketTestClient().ConnectAsync();
        
        var registerUserDto = GenerateRandomRegisterUserDto();
        await ws.DoAndAssert(new ClientWantsToSignUpDto { RegisterUserDto = registerUserDto }, receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerSignsUserUp)) == 1;
        });
        
        await ws.DoAndAssert(new ClientWantsToSignUpDto { RegisterUserDto = registerUserDto }, receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerSignsUserUp)) == 1 
                   && receivedMessages.Count(e => e.eventType == nameof(ServerRespondsUserAlreadyExists)) == 1;
        });
    }

    [Test]
    public async Task LogInInvalidCredentials()
    {
        var ws = await new WebSocketTestClient().ConnectAsync();
        var loginDto = new LoginDto
        {
            Email = "invalidEmail",
            Password = "invalidPassword"
        };
        
        await ws.DoAndAssert(new ClientWantsToLogInDto { LoginDto = loginDto }, receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerAuthenticatesUser)) == 0 
                   && receivedMessages.Count(e => e.eventType == nameof(ServerRejectsWrongCredentials)) == 1;
        });
    }
}