using System.Numerics;
using Riptide;

namespace Magicthrist___Green;

internal static class MessageExtensions
{
    public static Vector2 GetVector2(this Message message) => new
    (
        x: message.GetFloat(),
        y: message.GetFloat()
    );

    public static Message AddVector2(this Message message, Vector2 vector2) =>
        message.AddFloat(vector2.X).AddFloat(vector2.Y);
}
