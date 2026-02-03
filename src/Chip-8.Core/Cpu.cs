namespace Chip8.Core;

using System;
using System.Collections.Generic;
using System.Text;

/// <remarks>
/// Based on Guide to making a CHIP-8 emulator by Tobias V. I. Langhoff (https://tobiasvl.github.io/blog/write-a-chip-8-emulator/)
/// </remarks>
public class Cpu
{
    private const int MemorySize = 4096;
    private const int DisplayWidth = 64;
    private const int DisplayHeight = 32;

    /// <summary>Program counter, points to the current instruction in memory.</summary>
    public ushort PC { get; set; }

    /// <summary>Index register.</summary>
    public ushort I { get; set; }

    /// <summary>Stack used to call subroutines/functions and return from them</summary>
    public Stack<ushort> Stack { get; } = new();

    public byte DelayTimer { get; set; }
    public byte SoundTimer { get; set; }

    /// <summary>Registers</summary>
    public byte[] V { get; } = new byte[16];

    public byte[] Memory { get; private set; } = new byte[MemorySize];

    /// <summary>Current OpCode.</summary>
    public ushort OpCode { get; private set; }


    public Cpu()
    {
        PC = 0x200;
        OpCode = 0;
        I = 0;
    }

    /// <summary>
    /// Load font data into memory.
    /// </summary>
    /// <param name="font">The font data</param>
    /// <exception cref="ArgumentException">Unexpected data length</exception>
    public void LoadFont(byte[] font)
    {
        if (font?.Length != 80) throw new ArgumentException($"Font data should be 80 bytes, received {font?.Length}.", nameof(font));
        font.CopyTo(Memory, 0x0);
    }

    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.Append($"PC={PC.ToString("X2")}, ");
        builder.Append($"IR={PC.ToString("X2")}");
        builder.AppendLine();

        for (var i = 0; i < V.Length; i++)
        {
            builder.Append($"{i:00}={V[i].ToString("X2")}, ");
        }
        builder.AppendLine();

        builder.Append("Memory: ");
        builder.Append(Convert.ToHexString(Memory));
        builder.AppendLine();

        return builder.ToString();
    }

}
