using Riptide;
using Util;

namespace JPong;

public class JPongGameState
{
    public static JPongGameState Default = new();

    public JPongGameState
    (
        long packedBallXY = 0L,
        long packedBallVelocity = 0L,
        float leftPaddleY = 0F,
        float rightPaddleY = 0F,
        string leftPaddleDirection = "Idle",
        string rightPaddleDirection = "Idle",
        int firstScore = 0,
        int secondScore = 0,
        string? guestUuid = null
    )
    {
        PackedBallXY = packedBallXY;
        PackedBallVelocity = packedBallVelocity;
        LeftPaddleY = leftPaddleY;
        RightPaddleY = rightPaddleY;
        LeftPaddleDirection = leftPaddleDirection;
        RightPaddleDirection = rightPaddleDirection;
        Score = (firstScore, secondScore);
        GuestUuid = guestUuid;
    }

    public long PackedBallXY;
    public long PackedBallVelocity;
    public float LeftPaddleY;
    public float RightPaddleY;
    public string LeftPaddleDirection;
    public string RightPaddleDirection;
    public (int first, int second) Score;
    public string? GuestUuid;

    public void Apply(JPongGameState snapshot, byte inMask, out byte outMask)
    {
        Span<bool> bools = stackalloc bool[8];
        inMask.FillBitsToBoolSpan(bools);
        OptionallyApplyParameter(ref bools[0], ref PackedBallXY, snapshot.PackedBallXY);
        OptionallyApplyParameter(ref bools[1], ref PackedBallVelocity, snapshot.PackedBallVelocity);
        OptionallyApplyParameter(ref bools[2], ref LeftPaddleY, snapshot.LeftPaddleY);
        OptionallyApplyParameter(ref bools[3], ref RightPaddleY, snapshot.RightPaddleY);
        OptionallyApplyParameter(ref bools[4], ref LeftPaddleDirection, snapshot.LeftPaddleDirection);
        OptionallyApplyParameter(ref bools[5], ref RightPaddleDirection, snapshot.RightPaddleDirection);
        OptionallyApplyParameter(ref bools[6], ref Score, snapshot.Score);
        OptionallyApplyParameter(ref bools[7], ref GuestUuid, snapshot.GuestUuid);
        outMask = bools.ToByte();
    }

    private static void OptionallyApplyParameter<T>(ref bool apply, ref T field, T value)
    {
        if (!apply)
        {
        }
        else if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            apply = false;
        }
        else
        {
            field = value;
        }
    }
}

public static class GameStateMessageExtension
{
    public static Message AddGameState(this Message message, JPongGameState state, byte mask = 0b11111111)
    {
        Console.WriteLine("Adding JPong GameState to message");
        message.AddByte(mask);
        Console.WriteLine($"mask={Convert.ToString(mask, 2)}");
        if ((mask & (1 << 0)) != 0)
        {
            Console.WriteLine($"PackedBallXY={state.PackedBallXY}");
            message.AddLong(state.PackedBallXY);
        }
        if ((mask & (1 << 1)) != 0)
        {
            Console.WriteLine($"PackedBallVelocity={state.PackedBallVelocity}");
            message.AddLong(state.PackedBallVelocity);
        }
        if ((mask & (1 << 2)) != 0)
        {
            Console.WriteLine($"LeftPaddleY={state.LeftPaddleY}");
            message.AddFloat(state.LeftPaddleY);
        }
        if ((mask & (1 << 3)) != 0)
        {
            Console.WriteLine($"RightPaddleY={state.RightPaddleY}");
            message.AddFloat(state.RightPaddleY);
        }
        if ((mask & (1 << 4)) != 0)
        {
            Console.WriteLine($"LeftPaddleDirection={state.LeftPaddleDirection}");
            message.AddString(state.LeftPaddleDirection);
        }
        if ((mask & (1 << 5)) != 0)
        {
            Console.WriteLine($"RightPaddleDirection={state.RightPaddleDirection}");
            message.AddString(state.RightPaddleDirection);
        }
        if ((mask & (1 << 6)) != 0)
        {
            Console.WriteLine($"Score.first={state.Score.first}");
            message.AddInt(state.Score.first);
            Console.WriteLine($"Score.first={state.Score.second}");
            message.AddInt(state.Score.second);
        }
        if ((mask & (1 << 7)) != 0)
        {
            Console.WriteLine($"GuestId={state.GuestUuid}");
            message.AddString(state.GuestUuid ?? "");
        }
        return message;
    }

    public static (byte mask, JPongGameState) GetGameState(this Message message)
    {
        var state = new JPongGameState();

        var mask = message.GetByte();
        if ((mask & (1 << 0)) != 0) state.PackedBallXY = message.GetLong();
        if ((mask & (1 << 1)) != 0) state.PackedBallVelocity = message.GetLong();
        if ((mask & (1 << 2)) != 0) state.LeftPaddleY = message.GetFloat();
        if ((mask & (1 << 3)) != 0) state.RightPaddleY = message.GetFloat();
        if ((mask & (1 << 4)) != 0) state.LeftPaddleDirection = message.GetString();
        if ((mask & (1 << 5)) != 0) state.RightPaddleDirection = message.GetString();
        if ((mask & (1 << 6)) != 0)
        {
            state.Score.first = message.GetInt();
            state.Score.second = message.GetInt();
        }
        if ((mask & (1 << 7)) != 0)
        {
            state.GuestUuid = message.GetString();
            if (state.GuestUuid == "")
            {
                state.GuestUuid = null;
            }
        }

        return (mask, state);
    }
}
