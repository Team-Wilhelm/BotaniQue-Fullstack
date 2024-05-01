using api.Events.PlantEvents.Client;
using api.Events.PlantEvents.Server;
using lib;
using Shared.Dtos.FromClient.Plant;
using Shared.Dtos.FromClient.Requirements;
using Shared.Models.Information;

namespace Tests;

public class PlantTests : TestBase
{
    [Test]
    public async Task CreatePlant()
    {
        var jwtAndEmail = await SignUpAndLogIn();
        var jwt = jwtAndEmail[DictionaryKeys.Jwt];
        var email = jwtAndEmail[DictionaryKeys.Email];
        var createPlantDto = GenerateRandomCreatePlantDto(email);
        
        var webSocketTestClient = await new WebSocketTestClient().ConnectAsync();
        
        await webSocketTestClient.DoAndAssert(new ClientWantsToCreatePlantDto { CreatePlantDto = createPlantDto, Jwt = jwt }, receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerSendsPlant)) == 1;
        });
        
        await webSocketTestClient.DoAndAssert(new ClientWantsAllPlantsDto
        {
            UserEmail = email, 
            Jwt = jwt,
            PageNumber = 1,
            PageSize = 10
        }, receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerSendsAllPlants)) == 1;
        });
    }
    
    private CreatePlantDto GenerateRandomCreatePlantDto(string email)
    {
        var createPlantDto = new CreatePlantDto
        {
            UserEmail = email,
            CollectionId = null,
            Nickname = "Nickname",
            Base64Image = "https://realurl.com",
            CreateRequirementsDto = new CreateRequirementsDto
            {
                SoilMoistureLevel = RequirementLevel.Low,
                LightLevel = RequirementLevel.Medium,
                TemperatureLevel = RequirementLevel.High,
                HumidityLevel = RequirementLevel.Low
            }
        };
        return createPlantDto;
    }
}