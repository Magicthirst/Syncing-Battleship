using Riptide;

namespace Syncing_Battleship_Common_Typing;

public interface IDataBehaviour
{
    object DefaultState { get; }

    Message FullMessageOf(object state);

    bool TryApplyUpdate(Message message, int sender, object state, out Message snapshot);

    bool TryExtractCommand(Message message, int sender, object state, out Message command);
}
