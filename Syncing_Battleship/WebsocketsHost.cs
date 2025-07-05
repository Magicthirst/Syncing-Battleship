using Riptide;
using Riptide.Utils;
using Syncing_Battleship_Common_Typing;

namespace Syncing_Battleship;

public class WebsocketsHost
(
    ushort port,
    ushort maxClients,
    SessionsRouter sessions
)
{
    private readonly HashSet<Connection> connected = [];
    private readonly Server server = new();

    public void Start()
    {
        RiptideLogger.Initialize(Console.WriteLine, true);

        server.ClientConnected += ClientConnected;
        server.MessageReceived += MessageReceived;
        server.ClientDisconnected += ClientDisconnected;

        server.Start(port, maxClients);
        Console.WriteLine($"Сервер запущен на порту {port} с максимальным количеством клиентов {maxClients}");
    }

    public void Update()
    {
        server.Update();
    }

    public void Stop()
    {
        server.Stop();
        Console.WriteLine("Сервер остановлен.");
    }

    private async void ClientConnected(object? sender, ServerConnectedEventArgs e)
    {
        Console.Write("sender="); Console.WriteLine(sender);
        Console.Write("e="); Console.WriteLine(e);
        connected.Add(e.Client);
    }

    private async void MessageReceived(object? sender, MessageReceivedEventArgs e)
    {
        Console.Write("sender="); Console.WriteLine(sender);
        Console.Write("e="); Console.WriteLine(e);
        if (connected.Contains(e.FromConnection))
        {
            sessions.Consume(e.FromConnection, e.Message, (MessageMark) e.MessageId);
        }
    }

    private async void ClientDisconnected(object? sender, ServerDisconnectedEventArgs e)
    {
        Console.Write("sender="); Console.WriteLine(sender);
        Console.Write("e="); Console.WriteLine(e);
        connected.Remove(e.Client);
        sessions.Disconnect(e.Client);
    }
}
