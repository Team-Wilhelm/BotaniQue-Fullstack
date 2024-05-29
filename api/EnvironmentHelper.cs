namespace api;

public static class EnvironmentHelper
{
    private static readonly List<string?> NonProdEnvironments = ["Development", "Testing"];
    
    public static bool IsTesting()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Testing";
    }
    
    public static bool IsDevelopment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    }
    
    public static bool IsNonProd()
    {
        return NonProdEnvironments.Contains(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
    }
    
    public static bool IsCi()
    {
        return Environment.GetEnvironmentVariable("CI") == "true";
    }
}