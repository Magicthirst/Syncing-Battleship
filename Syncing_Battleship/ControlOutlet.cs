using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Syncing_Battleship_gRPC_Outlet;
using Syncing_Battleship_gRPC_Outlet.Services;

namespace Syncing_Battleship;

public class ControlOutlet
{
    private readonly WebApplication app;

    public ControlOutlet(ushort port, SessionsRouter sessions, Action stop)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.ConfigureKestrel(options =>
        {
            options.Listen(IPAddress.Any, port, listenOptions =>
            {
                listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
            });
        });

        // TODO allow access only for Gateways url

        builder.Services.AddSingleton<SyncControlOutletImpl.LaunchDelegate>(_ => hostId =>
        {
            var (code, sotKey) = sessions.Create(hostId);
            return new NewSessionInfo
            {
                SessionId = code,
                SourceOfTruthKey = sotKey
            };
        });
        builder.Services.AddSingleton<SyncControlOutletImpl.WelcomeDelegate>(_ => sessions.Welcome);
        builder.Services.AddSingleton<SyncControlOutletImpl.AsyncShutdownDelegate>(_ => () =>
        {
            app!.StopAsync();
            stop();
        });
        builder.Services.AddGrpc();

        app = builder.Build();
        app.MapGrpcService<SyncControlOutletImpl>();
    }
    

    public void Start() => _ = app.RunAsync();

    public void WaitForShutdown() => app.WaitForShutdown();
}
