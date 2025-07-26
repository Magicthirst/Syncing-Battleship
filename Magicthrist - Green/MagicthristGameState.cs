using System.Numerics;

namespace Magicthrist___Green;

internal class MagicthristGameState
{
    public Dictionary<int, PlayerState> Players;

    internal class PlayerState(Vector2 position, Vector2 vector)
    {
        public Vector2 Position = position;
        public Vector2 Vector = vector;
    }
}
