using Riptide;
using Syncing_Battleship_Common_Typing;

namespace JPong;

// FIXME(optimization tips):
//   [ ] Riptide framework works sequentially =>
//   instead of creating new objects and structures in this implementation,
//   much of it can be made reusable through shared/global/static variables

public class JPongDataBehaviour : IDataBehaviour
{
    public object DefaultState => JPongGameState.Default;

    public Message FullMessageOf(object state) => Message.Create().AddGameState((JPongGameState) state);

    public bool TryApplyUpdate(Message message, int sender, object state, out Message snapshot)
    {
        var src = (JPongGameState) state;
        var (mask, got) = message.GetGameState();
        src.Apply(got, mask, out mask);

        snapshot = Message.Create();
        if (mask == 0) return false;
        snapshot.AddGameState(src, mask);
        return true;
    }

    public bool TryExtractCommand(Message message, int sender, object state, out Message command)
    {
        command = message;
        return false;
    }
}
