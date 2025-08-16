using System.Diagnostics.CodeAnalysis;
using Riptide;
using Riptide.Utils;
using Syncing_Battleship_Common_Typing;
using static Magicthrist___Green.MagicthirstCommandMark;
using static Magicthrist___Green.MagicthristGameState;

namespace Magicthrist___Green;

public class MagicthirstDataBehaviour : IDataBehaviour
{
    public object DefaultState => new MagicthristGameState();

    public Message FullMessageOf(object state)
    {
        var src = (MagicthristGameState) state;

        var message = Message.Create()
            .AddInt(src.Players.Count);

        foreach (var (id, player) in src.Players)
        {
            message
                .AddInt(id)
                .AddVector2(player.Position)
                .AddVector2(player.Vector);
        }

        RiptideLogger.Log(LogType.Info, $"Full state: {src}");
        return message;
    }

    public bool TryApplyNewPlayerConnection(int newPlayer, object state, out Message snapshot)
    {
        var src = (MagicthristGameState) state;
        RiptideLogger.Log(LogType.Info, $"Adding new player to: {src}");
        src.Players.Add(newPlayer, PlayerState.Default);
        snapshot = Message.Create();
        RiptideLogger.Log(LogType.Info, $"New player were added: {src}");
        return false;
    }

    // public bool TryApplyUpdate(Message message, int sender, object state, out Message snapshot)
    // {
    //     throw new NotImplementedException();
    // }

    public bool TryApplyCommand
    (
        Message message,
        MessageMark fullMark,
        int sender,
        object state,
        out Message copy,
        out Message snapshot
    )
    {
        var mark = fullMark & MessageMark.FilterExtras;
        var src = (MagicthristGameState) state;
        snapshot = message;
        switch ((MagicthirstCommandMark) mark)
        {
            case Movement:
                var player = src.Players[sender];
                player.Position = message.GetVector2();
                player.Vector = message.GetVector2();
                var timestamp = message.GetDouble();

                copy = Message.Create()
                    .AddVector2(player.Position)
                    .AddVector2(player.Vector)
                    .AddDouble(timestamp)
                    .AddInt(sender);

                RiptideLogger.Log(LogType.Debug, $"Received movement position={player.Position}, vector={player.Vector}");
                return false;

            default:
                copy = message;
                RiptideLogger.Log(LogType.Warning, $"not handling command: {fullMark}");
                return false;
        }
    }
}

[Flags]
[SuppressMessage("ReSharper", "ShiftExpressionZeroLeftOperand")]
internal enum MagicthirstCommandMark : ushort
{
    Movement = MessageMark.Command | 1 << MessageMarkSupply.ExtraShift
}
