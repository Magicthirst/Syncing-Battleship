using Riptide;
using Syncing_Battleship_Common_Typing;

namespace Magicthrist___Green;

public class MagicthirstDataBehaviour : IDataBehaviour
{
    public object DefaultState => new MagicthristGameState();

    public Message FullMessageOf(object state) => Message.Create();

    // public bool TryApplyNewPlayerConnection(int newPlayer, object state, out Message snapshot)
    // {
    //     throw new NotImplementedException();
    // }

    // public bool TryApplyUpdate(Message message, int sender, object state, out Message snapshot)
    // {
    //     throw new NotImplementedException();
    // }

    // public bool TryExtractCommand(Message message, int sender, object state, out Message command)
    // {
    //     throw new NotImplementedException();
    // }
}
