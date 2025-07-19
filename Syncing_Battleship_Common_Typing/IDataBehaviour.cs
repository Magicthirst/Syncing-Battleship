using Riptide;

namespace Syncing_Battleship_Common_Typing;

public interface IDataBehaviour
{
    object DefaultState { get; }

    Message FullMessageOf(object state);

    bool TryApplyUpdate(Message message, int sender, object state, out Message snapshot)
    {
        snapshot = message;
        return false;
    }

    bool TryApplyNewPlayerConnection(int newPlayer, object state, out Message snapshot)
    {
        snapshot = Message.Create();
        return false;
    }

    bool TryExtractCommand
    (
        Message message,
        MessageMark mark,
        int sender,
        object state,
        out Message command,
        out Message snapshot
    )
    {
        command = message;
        snapshot = message;
        return false;
    }
}
