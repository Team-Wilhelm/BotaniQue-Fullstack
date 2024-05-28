using api.Core.Services;
using api.Core.Services.External.BlobStorage;
using api.Events.Auth.Server;
using api.Events.Collections.Server;
using api.Events.Global;
using api.Events.PlantEvents.Server;
using api.Events.Statistics;
using api.Extensions;
using Fleck;
using lib;
using Shared.Dtos;
using Shared.Dtos.FromClient.Identity;
using Shared.Exceptions;

namespace api.Events.Auth.Client;

public class ClientWantsToLogInDto : BaseDto
{
    public LoginDto LoginDto { get; set; } = null!;
}

public class ClientWantsToLogIn(WebSocketConnectionService webSocketConnectionService, UserService userService, IBlobStorageService blobStorageService, PlantService plantService, CollectionsService collectionsService, StatsService statsService)
    : BaseEventHandler<ClientWantsToLogInDto>
{
    public override async Task Handle(ClientWantsToLogInDto dto, IWebSocketConnection socket)
    {
        var jwt = await userService.Login(dto.LoginDto);
        if (jwt == null) throw new InvalidCredentialsException();

        var user = await userService.GetUserByEmail(dto.LoginDto.Email);
        
        var criticalPlants = plantService.GetCriticalPlants(user.UserEmail);
        var allCollections = collectionsService.GetCollectionsForUser(user.UserEmail);
        var allPlants = plantService.GetPlantsForUser(user.UserEmail, 1, 100);
        var stats = statsService.GetStats(user.UserEmail);

        await Task.WhenAll(criticalPlants, allCollections, allPlants, stats);
        
        webSocketConnectionService.UpdateConnectionEmail(socket, dto.LoginDto.Email);
        
        var getUserDto = new GetUserDto
        {
            UserEmail = user.UserEmail,
            Username = user.UserName,
        };
        
        if (!string.IsNullOrEmpty(user.BlobUrl))
        {
            getUserDto.BlobUrl = blobStorageService.GenerateSasUri(user.BlobUrl, false);
        }
        
        socket.SendDto(new ServerAuthenticatesUser
        {
            Jwt = jwt,
            
        });
        
       socket.SendDto(new ServerSendsUserInfo
        {
            GetUserDto = getUserDto
        });
       
       socket.SendDto(new ServerSendsCriticalPlants
       {
           Plants = criticalPlants.Result
       });
       
       socket.SendDto(new ServerSendsAllCollections
       {
           Collections = allCollections.Result.ToList()
       });
       
       socket.SendDto(new ServerSendsPlants
       {
           Plants = allPlants.Result
       });
       
       socket.SendDto(new ServerSendsStats
       {
           Stats = stats.Result
       });
    }
}

