using Riptide;
using Syncing_Battleship_Common_Typing;
using Syncing_Battleship_gRPC_Outlet;
using Util;
using static Syncing_Battleship_Common_Typing.MessageMark;

namespace Syncing_Battleship;

public class SessionsRouter
(
    Func<MessageSendMode, IDataBehaviour> configureBehaviour,
    TimeSpan disconnectionTimeout,
    bool allowNotSotUpdates
)
{
    private readonly Dictionary<int, Session> sessions = [];
    private readonly Dictionary<Connection, Session> connectionsSessions = [];
    private readonly Dictionary<int, HashSet<string>> allowedPlayers = [];

    public (int code, string sotKey) Create(string hostUuid)
    {
        var code = hostUuid.GetHashCode();
        if (sessions.ContainsKey(code))
        {
            throw new SessionIsAlreadyHostedException();
        }

        var session = new Session(code, hostUuid, configureBehaviour, disconnectionTimeout, allowNotSotUpdates);
        session.DidFinished += () => OnSessionFinished(session);
        sessions[code] = session;
        allowedPlayers[code] = [hostUuid];

        return (code, session.SotKey);
    }

    public bool Welcome(int sessionId, string playerId)
    {
        if (!allowedPlayers.TryGetValue(sessionId, out var players)) return false;

        players.Add(playerId);
        return true;
    }

    public void Consume(Connection connection, Message message, MessageMark mark)
    {
        if (connectionsSessions.TryGetValue(connection, out var session))
        {
            session.Consume(connection, message, mark);
            return;
        }

        var messageCopy = Message.Create().AddMessage(message);
        if (!mark.HasFlag(Connected))
        {
            connection.Send(Message.Create(MessageSendMode.Reliable, Error400));
            return;
        }

        var sessionId = message.GetInt();
        var playerId = message.GetString();
        if (!sessions.TryGetValue(sessionId, out session) || !allowedPlayers[sessionId].Contains(playerId))
        {
            connection.Send(Message.Create(MessageSendMode.Reliable, Error404));
            return;
        }

        connectionsSessions[connection] = session;
        session.Join(connection, messageCopy, mark);
    }

    public void Disconnect(Connection connection)
    {
        if (connectionsSessions.TryGetValue(connection, out var session))
        {
            session.Disconnect(connection);
            connectionsSessions.Remove(connection);
        }
    }

    private void OnSessionFinished(Session session)
    {
        sessions.Remove(session.Id);
        connectionsSessions.RemoveWhere((_, s) => s == session);
    }
}
