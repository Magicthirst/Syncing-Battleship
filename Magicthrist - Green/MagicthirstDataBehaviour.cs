using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Riptide;
using Syncing_Battleship_Common_Typing;
using static Magicthrist___Green.MagicthirstCommandMark;

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

    public bool TryApplyCommand(Message message, MessageMark mark, int sender, object state, out Message snapshot)
    {
        var src = (MagicthristGameState) state;
        snapshot = message;
        switch ((MagicthirstCommandMark)(mark & MessageMark.FilterExtras))
        {
            case Movement:
                var player = src.Players[sender];
                player.Position = message.GetVector2();
                player.Vector = message.GetVector2();
                return false;

            default:
                throw new ArgumentOutOfRangeException(nameof(mark));
        }
    }
}

[Flags]
[SuppressMessage("ReSharper", "ShiftExpressionZeroLeftOperand")]
internal enum MagicthirstCommandMark : ushort
{
    Movement = MessageMark.Command | 0 << MessageMarkSupply.ExtraShift
}
