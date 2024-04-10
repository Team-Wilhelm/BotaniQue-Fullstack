using System.Reflection;
using Fleck;
using Infrastructure;
using lib;
using Microsoft.EntityFrameworkCore;

public static class Startup
{
    public static void Main(string[] args)
    {
        var app = StartApi(args);
        app.Run();
    }
    
    public static WebApplication StartApi(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // TODO: builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

        // Add services to the container.
        var connectionString = builder.Configuration.GetConnectionString("BotaniqueDb");
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
        {
            connectionString ??= Environment.GetEnvironmentVariable("DefaultConnection");
            options.UseNpgsql(connectionString ?? throw new Exception("Connection string cannot be null"),
                b => b.MigrationsAssembly("Vital"));
        });
        
        var services = builder.FindAndInjectClientEventHandlers(Assembly.GetExecutingAssembly());
        var app = builder.Build();
        builder.WebHost.UseUrls("http://*:9999");
        
        var port = Environment.GetEnvironmentVariable("PORT") ?? "8181";
        var wsServer = new WebSocketServer($"ws://0.0.0.0:{port}");
        
        wsServer.Start(socket =>
        {
            socket.OnOpen = () => Console.WriteLine("Open!");
            socket.OnClose = () => Console.WriteLine("Close!");
            socket.OnMessage =  async message =>
            {
                try
                {
                    await app.InvokeClientEventHandler(services, socket, message);
                }
                catch (Exception e)
                {
                    // e.Handle(socket);
                }
            };
        });

        return app;
    }
}