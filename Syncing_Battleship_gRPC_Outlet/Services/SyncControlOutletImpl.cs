using Grpc.Core;
using static Syncing_Battleship_gRPC_Outlet.Services.SyncControlOutletImpl;

namespace Syncing_Battleship_gRPC_Outlet.Services;

public class SyncControlOutletImpl
(
    ILogger<SyncControlOutletImpl> logger,
    LaunchDelegate launchSession,
    WelcomeDelegate tryWelcome,
    AsyncShutdownDelegate shutdownAsync
) : SyncControlOutlet.SyncControlOutletBase
{
    public delegate NewSessionInfo LaunchDelegate(string hostId);
    public delegate bool WelcomeDelegate(int sessionId, string playerId);
    public delegate void AsyncShutdownDelegate();

    public override Task<NewSessionInfo> Launch(SessionLaunchInfo info, ServerCallContext context)
    {
        try
        {
            logger.LogInformation("Creating new session for host={hostId}", info.HostId);
            var sessionInfo = launchSession(info.HostId);
            logger.LogInformation("Created new session for host={hostId}, session id={id}", info.HostId, sessionInfo.SessionId);
            return Task.FromResult(sessionInfo);
        }
        catch (SessionIsAlreadyHostedException)
        {
            throw new RpcException(new Status(StatusCode.AlreadyExists, "A session by this host is already hosted"));
        }
    }

    public override Task<WelcomeResponse> Welcome(WelcomeRequest request, ServerCallContext context)
    {
        logger.LogInformation("Attempting to welcome host={sessionId}, player={playerId}", request.SessionId, request.PlayerId);
        var success = tryWelcome(request.SessionId, request.PlayerId);
        logger.LogInformation("Welcome for host={sessionId}, player={playerId} was {success}", request.SessionId, request.PlayerId, success ? "successful" : "unsuccessful");
        return Task.FromResult(new WelcomeResponse { Success = success });
    }
}
