using System.Net;
using Humanizer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Syncing_Battleship_gRPC_Outlet;
using Syncing_Battleship_gRPC_Outlet.Services;
using Util;

namespace Syncing_Battleship;

public class ControlOutlet
{
    private readonly WebApplication app;

    public ControlOutlet(ushort port, SessionsRouter sessions, Action stop)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls($"http://localhost:{port}");

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
        app.UseMiddleware<AdminSafeListMiddleware>("127.0.0.1;::1");
        app.MapGrpcService<SyncControlOutletImpl>();
    }
    

    public void Start() => _ = app.RunAsync();

    public void WaitForShutdown() => app.WaitForShutdown();

    private class AdminSafeListMiddleware
    {
        private readonly RequestDelegate next;
        private readonly ILogger<AdminSafeListMiddleware> logger;
        private readonly byte[][] safelist;

        public AdminSafeListMiddleware
        (
            RequestDelegate next,
            ILogger<AdminSafeListMiddleware> logger,
            string safelist
        )
        {
            var ips = safelist.Split(';');
            this.safelist = new byte[ips.Length][];
            for (var i = 0; i < ips.Length; i++)
            {
                this.safelist[i] = IPAddress.Parse(ips[i]).GetAddressBytes();
            }

            this.next = next;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var remoteIp = context.Connection.RemoteIpAddress;
            logger.LogDebug("Request from Remote IP address: {RemoteIp}", remoteIp);
            if (remoteIp == null)
            {
                context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                return;
            }

            var bytes = remoteIp.GetAddressBytes();
            if (!safelist.Any(address => address.SequenceEqual(bytes)))
            {
                logger.LogWarning(
                    "Forbidden Request from Remote IP address: {RemoteIp}", remoteIp);
                context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                return;
            }

            await next.Invoke(context);
        }
    }
}
