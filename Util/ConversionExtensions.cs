using System.Collections;

namespace Util;

public static class ConversionExtensions
{
    public static bool[] ToBoolArray(this BitArray bits)
    {
        var bools = new bool[bits.Length];
        bits.CopyTo(bools, 0);
        return bools;
    }

    public static Span<bool> ToBoolSpan(this BitArray bits)
    {
        var bools = new bool[bits.Length];
        bits.CopyTo(bools, 0);
        return bools.AsSpan();
    }

    public static void FillBitsToBoolSpan(this byte bits, Span<bool> bools)
    {
        for (int i = 0; i < 8; i++)
        {
            bools[i] = (bits & 1 << i) != 0;
        }
    }

    public static BitArray ToBitArray(this Span<bool> bools)
    {
        var bits = new BitArray(bools.Length);
        for (int i = 0; i < bools.Length; i++)
        {
            bits[i] = bools[i];
        }
        return bits;
    }

    public static byte ToByte(this Span<bool> bools)
    {
        byte b = 0;
        for (int i = 0; i < bools.Length; i++)
        {
            if (bools[i])
            {
                b |= (byte)(1 << i);
            }
        }
        return b;
    }
}
