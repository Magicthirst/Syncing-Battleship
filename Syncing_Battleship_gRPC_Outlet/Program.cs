using Humanizer;
using Syncing_Battleship_gRPC_Outlet.Services;
using Syncing_Battleship_gRPC_Outlet;

// ReSharper disable once InconsistentNaming
const int TEST_SESSION_ID = 123;

P p = new();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<SyncControlOutletImpl.LaunchDelegate>(_ =>
{
    return _ => new NewSessionInfo
    {
        SessionId = TEST_SESSION_ID,
        SourceOfTruthKey = "TEST KEY"
    };
});
builder.Services.AddSingleton<SyncControlOutletImpl.WelcomeDelegate>(_ => (sessionId, playerId) =>
{
    Console.WriteLine($"Welcoming player={playerId} in session={sessionId}");
    return true;
});
builder.Services.AddSingleton<SyncControlOutletImpl.AsyncShutdownDelegate>(_ => () =>
{
    Task.Run(() =>
    {
        Task.Delay(5.Seconds());
        p.app.StopAsync();
    });
});
builder.Services.AddGrpc();


p.app = builder.Build();
p.app.MapGrpcService<SyncControlOutletImpl>();
p.app.Run();

internal class P
{
    public WebApplication app;
}
