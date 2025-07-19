using System.Security.Cryptography;
using Riptide;
using Syncing_Battleship_Common_Typing;
using Util;
using static Syncing_Battleship_Common_Typing.MessageMark;

namespace Syncing_Battleship;

// FIXME(optimization tips):
//   [ ] constructor args can be optimized for memory economy
//   [ ] all find operations for players field can be replaced with sql-like solution:
//         int lastRowId;
//         OrderedDictionary<int, Player> RowIdsToPlayers;    // original table
//         OrderedDictionary<string, int> idsToRowIds;        // indexing table
//         OrderedDictionary<string, int> riptideIdsToRowIds; // indexing table 
// NOTE: can be made as util generic data structure

public class Session
(
    int id,
    string hostId,
    IDataBehaviour behaviour,
    TimeSpan disconnectionTimeout,
    bool allowNotSotUpdates
)
{
    public event Action? DidFinished;

    public readonly int Id = id;
    private bool running = true;
    private readonly TimeSpan disconnectionTimeout = disconnectionTimeout;

    private ushort sotId = 0;
    internal readonly string SotKey = RandomNumberGenerator.GetHexString(32);
    private readonly object state = behaviour.DefaultState;
    private readonly List<Player> players = [new() { Id=hostId }];

    public Session
    (
        int id,
        string hostId,
        Func<MessageSendMode, IDataBehaviour> configureBehaviour,
        TimeSpan disconnectionTimeout,
        bool allowNotSotUpdates
    ) : this(id, hostId, configureBehaviour(MessageSendMode.Reliable), disconnectionTimeout, allowNotSotUpdates) {}

    public void Join(Connection connection, Message message, MessageMark mark)
    {
        message.GetInt();
        var playerId = message.GetString();
        if (players.TryGet(out var player, p => p.Id == playerId && p.Connection == null))
        {
            player.Connection = connection;
        }
        else
        {
            players.Add(new Player {Id = playerId, Connection = connection});
        }

        if (mark.HasFlag(SourceOfTruth) && message.GetString() == SotKey)
        {
            sotId = connection.Id;
        }

        if (behaviour.TryApplyNewPlayerConnection(connection.Id, state, out var _))
        {
            connection.Send(ReliableMessage(Accepted));
        }
        else
        {
            connection.Send(ReliableMessage(Error));
        }

        Reinit();
    }

    public void Consume(Connection connection, Message message, MessageMark mark)
    {
        if (!running) return;

        if (mark.HasFlag(Update))
        {
            var hasNoRightToSendUpdate = !allowNotSotUpdates && !mark.HasFlag(SourceOfTruth);
            var impersonatingSourceOfTruth = mark.HasFlag(SourceOfTruth) && connection.Id == sotId;
            if (hasNoRightToSendUpdate || impersonatingSourceOfTruth)
            {
                connection.Send(UnreliableMessage(Error403));
                return;
            }

            if (behaviour.TryApplyUpdate(message, connection.Id, state, out var update))
            {
                SendToAll(UnreliableMessage(Update).AddMessage(update));
            }
        }
        else if (mark.HasFlag(Command))
        {
            if (behaviour.TryExtractCommand(message, mark, connection.Id, state, out var command, out var update))
            {
                SendToAll(
                    ReliableMessage(Command).AddMessage(command),
                    exceptConnection: connection
                );
                SendToAll(UnreliableMessage(Update).AddMessage(update));
            }
        }
    }

    public void Disconnect(Connection connection)
    {
        var this0 = this;
        var player = players.Find(it => it.Connection == connection)!;
        var id = player.Id;
        player.Connection = null;
        Task.Run(() =>
        {
            Task.Delay(this0.disconnectionTimeout);
            if (this0.players.RemoveAll(p => p.Id == id && p.Connection == null) == 0) return;

            this0.Reinit();
            if (this0.players.Count == 0)
            {
                this0.running = false;
                this0.DidFinished?.Invoke();
            }
        });
    }

    private void Reinit()
    {
        var message = ReliableMessage(Update).AddMessage(behaviour.FullMessageOf(state));
        SendToAll(message);
    }

    private void SendToAll(Message message, Connection? exceptConnection = null)
    {
        foreach (var player in players.Where(p => p.Connection != exceptConnection))
        {
            player.Connection?.Send(message);
        }
    }

    private static Message ReliableMessage(MessageMark action) => Message.Create(MessageSendMode.Reliable, MessageMark.Server | action);

    private static Message UnreliableMessage(MessageMark action) => Message.Create(MessageSendMode.Unreliable, MessageMark.Server | action);

    private class Player
    {
        public string Id;
        public Connection? Connection;
    }
}