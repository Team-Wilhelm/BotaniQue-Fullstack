namespace Shared.Models.Information;

public enum RequirementLevel
{
    Low,
    Medium,
    High
}

public static class RequirementLevelExtensions
{
    public static (int Min, int Max) GetRange(this RequirementLevel level)
    {
        return level switch
        {
            RequirementLevel.Low => (0, 33),
            RequirementLevel.Medium => (34, 66),
            RequirementLevel.High => (67, 100),
            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
        };
    }
    
    public static bool IsInRange(this RequirementLevel level, double value)
    {
        var range = level.GetRange();
        return range.Min <= value && value <= range.Max;
    }
}

public enum RequirementType
{
    SoilMoisture,
    Light,
    Temperature,
    Humidity
}