using api.Core.Services;
using api.Core.Services.External.BlobStorage;
using api.Events.Auth.Server;
using api.Events.Collections.Server;
using api.Events.Global;
using api.Events.PlantEvents.Server;
using api.Events.Statistics;
using api.Extensions;
using Fleck;
using Shared.Dtos;

namespace api.Events.Auth.Client;

public class InitialDataHelper(PlantService plantService, IBlobStorageService blobStorageService, CollectionsService collectionsService, StatsService statsService, UserService userService, WebSocketConnectionService webSocketConnectionService, JwtService jwtService)
{
    
    public async Task SendInitialData(IWebSocketConnection socket, string jwt)
    {
        var email = jwtService.GetEmailFromJwt(jwt);
        webSocketConnectionService.UpdateConnectionEmail(socket, email);
        
        var user = await userService.GetUserByEmail(email);
        
        socket.SendDto(new ServerAuthenticatesUser
        {
            Jwt = jwt,
        });
        
        var criticalPlants = await plantService.GetCriticalPlants(user.UserEmail);
        socket.SendDto(new ServerSendsCriticalPlants
        {
            Plants = criticalPlants
        });
        
        var getUserDto = new GetUserDto
        {
            UserEmail = user.UserEmail,
            Username = user.UserName,
        };
        
        if (!string.IsNullOrEmpty(user.BlobUrl))
        {
            getUserDto.BlobUrl = blobStorageService.GenerateSasUri(user.BlobUrl, false);
        }
        
        socket.SendDto(new ServerSendsUserInfo
        {
            GetUserDto = getUserDto
        });
        
        var allCollections = await collectionsService.GetCollectionsForUser(user.UserEmail);
        socket.SendDto(new ServerSendsAllCollections
        {
            Collections = allCollections.ToList()
        });
        
        var stats = await statsService.GetStats(user.UserEmail);
        socket.SendDto(new ServerSendsStats
        {
            Stats = stats
        });
    }
}