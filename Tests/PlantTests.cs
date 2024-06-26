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
        var createPlantDto = GenerateRandomCreatePlantDto();
        
        var webSocketTestClient = await new WebSocketTestClient().ConnectAsync();
        
        await webSocketTestClient.DoAndAssert(new ClientWantsToCreatePlantDto { CreatePlantDto = createPlantDto, Jwt = jwt }, receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerSavesPlant)) == 1;
        });
    }

    [Test]
    public async Task GetAllPlants()
    {
        var jwtAndEmail = await SignUpAndLogIn();
        var jwt = jwtAndEmail[DictionaryKeys.Jwt];

        var webSocketTestClient = await new WebSocketTestClient().ConnectAsync();

        await webSocketTestClient.DoAndAssert(new ClientWantsAllPlantsDto
        {
            Jwt = jwt,
            PageNumber = 1,
            PageSize = 10
        }, receivedMessages =>
        {
            return receivedMessages.Count(e => e.eventType == nameof(ServerSendsPlants)) == 1;
        });
    }
    
    private CreatePlantDto GenerateRandomCreatePlantDto()
    {
        var createPlantDto = new CreatePlantDto
        {
            CollectionId = null,
            Nickname = "Nickname",
            Base64Image = "iVBORw0KGgoAAAANSUhEUgAAAFgAAABHCAYAAACDFYB6AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAEnQAABJ0Ad5mH3gAAACmSURBVHhe7dAxAQAADMOg+TfdqeALEriFKhgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYKxgrGCsYK5jaHjFvRBBJ1UDnAAAAAElFTkSuQmCC",
            CreateRequirementsDto = new CreateRequirementsDto
            {
                SoilMoistureLevel = RequirementLevel.Low,
                LightLevel = RequirementLevel.Medium,
                TemperatureLevel = 25,
                HumidityLevel = RequirementLevel.Low
            }
        };
        return createPlantDto;
    }
}