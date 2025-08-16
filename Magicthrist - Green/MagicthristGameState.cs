using System.Numerics;

namespace Magicthrist___Green;

internal class MagicthristGameState
{
    public readonly Dictionary<int, PlayerState> Players = new();

    internal class PlayerState(Vector2 position, Vector2 vector)
    {
        public Vector2 Position = position;
        public Vector2 Vector = vector;

        public static PlayerState Default => new(Vector2.Zero, Vector2.Zero);
    }

    public override string ToString() =>
        $"{nameof(MagicthristGameState)}(" +
            $"Players=[{string.Join(",", Players)}]," +
            $"|Players|={Players.Count}" +
        $")";
}
