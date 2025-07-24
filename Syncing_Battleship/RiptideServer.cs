using Riptide;
using Riptide.Utils;
using Syncing_Battleship_Common_Typing;

namespace Syncing_Battleship;

public class RiptideServer
(
    ushort port,
    ushort maxClients,
    SessionsRouter sessions,
    string logsPath
)
{
    private readonly HashSet<Connection> connected = [];
    private readonly Server server = new();
    private StreamWriter logs;

    public void Start()
    {
        logs = new StreamWriter(logsPath, append: true)
        {
            AutoFlush = true
        };
        RiptideLogger.Initialize(logs.WriteLine, true);

        server.ClientConnected += ClientConnected;
        server.MessageReceived += MessageReceived;
        server.ClientDisconnected += ClientDisconnected;
        server.ConnectionFailed += ConnectionFailed;

        server.Start(port, maxClients, useMessageHandlers: false);
        RiptideLogger.Log(LogType.Info, $"Сервер запущен на порту {port} с максимальным количеством клиентов {maxClients}");
    }

    public void Update()
    {
        server.Update();
    }

    public void Stop()
    {
        server.Stop();
        RiptideLogger.Log(LogType.Info, "Сервер остановлен.");
        logs.Close();
    }

    private async void ClientConnected(object? sender, ServerConnectedEventArgs e)
    {
        RiptideLogger.Log(LogType.Debug, $"ClientConnected, sender={sender}");
        RiptideLogger.Log(LogType.Debug, $"ClientConnected, e={e}");
        connected.Add(e.Client);
    }

    private async void ConnectionFailed(object? sender, ServerConnectionFailedEventArgs e)
    {
        RiptideLogger.Log(LogType.Debug, $"ConnectionFailed, sender={sender}");
        RiptideLogger.Log(LogType.Debug, $"ConnectionFailed, e={e}");
    }

    private async void MessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        RiptideLogger.Log(LogType.Debug, $"MessageReceived, sender={sender}");
        RiptideLogger.Log(LogType.Debug, $"MessageReceived, e={e}");
        if (connected.Contains(e.FromConnection))
        {
            sessions.Consume(e.FromConnection, e.Message, (MessageMark) e.MessageId);
        }
    }

    private async void ClientDisconnected(object? sender, ServerDisconnectedEventArgs e)
    {
        RiptideLogger.Log(LogType.Debug, $"ClientDisconnected, sender={sender}");
        RiptideLogger.Log(LogType.Debug, $"ClientDisconnected, e={e}");
        connected.Remove(e.Client);
        sessions.Disconnect(e.Client);
    }
}
