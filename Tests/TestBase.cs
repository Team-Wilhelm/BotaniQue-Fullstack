using System.Text.Json;
using api.Events.Auth.Client;
using api.Events.Auth.Server;
using lib;
using Shared.Dtos.FromClient;
using Shared.Dtos.FromClient.Identity;
using Shared.Models;

namespace Tests;

[TestFixture]
[NonParallelizable]
public abstract class TestBase
{
    /// <summary>
    /// Signs up a randomly generated user and logs them in, returning the JWT token,
    /// so it can be used in subsequent requests.
    /// </summary>
    /// <returns>JWT Token as a string</returns>
    protected async Task<Dictionary<DictionaryKeys, string>> SignUpAndLogIn()
    {
        var webSocketTestClient = await new WebSocketTestClient().ConnectAsync();
        var jwt = "";
        var registerUserDto = GenerateRandomRegisterUserDto();
        await webSocketTestClient.DoAndAssert(new ClientWantsToSignUpDto { RegisterUserDto = registerUserDto },  receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerSignsUserUp)) == 1;
        });
        
        var loginDto = new LoginDto
        {
            Email = registerUserDto.Email,
            Password = registerUserDto.Password
        };
        
        var jwtSubscription = webSocketTestClient.Client.MessageReceived.Subscribe(msg =>
        {
            var serverAuthenticates = JsonSerializer.Deserialize<ServerAuthenticatesUser>(msg.Text, options: new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            jwt = serverAuthenticates.Jwt;
        });
        webSocketTestClient.Send(new ClientWantsToLogInDto { LoginDto = loginDto });
        
        if (jwt == "")
        {
            DateTime startTime = DateTime.UtcNow;
            while (DateTime.UtcNow - startTime < TimeSpan.FromSeconds(5.0))
            {
                await Task.Delay(100);
                if (jwt != "")
                {
                    break;
                }
            }
            // throw new TimeoutException("Cannot get JWT token.");
        }
        jwtSubscription.Dispose();
        
        await webSocketTestClient.DoAndAssert(new ClientWantsToLogInDto { LoginDto = loginDto }, receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerAuthenticatesUser)) == 1;
        });
        return new Dictionary<DictionaryKeys, string>
        {
            { DictionaryKeys.Jwt, jwt },
            { DictionaryKeys.Email, registerUserDto.Email }
        };
    }
    
    protected RegisterUserDto GenerateRandomRegisterUserDto()
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

public enum DictionaryKeys
{
    Jwt,
    Email
}