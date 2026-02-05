namespace Chip8.Core;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

/// <remarks>
/// Based on Guide to making a CHIP-8 emulator by Tobias V. I. Langhoff (https://tobiasvl.github.io/blog/write-a-chip-8-emulator/)
/// </remarks>
public class Cpu
{
    internal const int MemorySize = 4096;
    internal const int DisplayWidth = 64;
    internal const int DisplayHeight = 32;

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

    public bool[,] Display { get; private set; } = new bool[DisplayWidth, DisplayHeight];


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
        Array.Copy(font, 0, Memory, 0x050, font.Length);
    }

    public void Step()
    {
        // Ensure we have at least two bytes to fetch an opcode
        if (PC >= Memory.Length - 1) throw new InvalidOperationException($"PC (0x{PC:X4}) is out of memory bounds.");

        OpCode = (ushort)(Memory[PC] << 8 | Memory[PC + 1]);
        PC += 2;

        var nnn = (ushort)(OpCode & 0x0FFF);
        var nn = (byte)(OpCode & 0x00FF);
        var n = (byte)(OpCode & 0x000F);
        var x = (byte)((OpCode & 0x0F00) >> 8);
        var y = (byte)((OpCode & 0x00F0) >> 4);

        switch (OpCode & 0xF000)
        {
            case 0x0000:
                if (OpCode == 0x00E0)
                {
                    ClearScreen();
                }
                break;
            case 0x1000:
                JumpTo(nnn);
                break;
            case 0x6000:
                SetRegister(x, nn);
                break;
            case 0x7000:
                AddToRegister(x, nn);
                break;
            case 0xA000:
                SetIndexRegister(nnn);
                break;
            case 0xD000:
                DisplaySprite(x, y, n);
                break;
            default:
                throw new InvalidOperationException($"Unknown opcode: {OpCode}");
        }

        if (DelayTimer > 0)
        {
            DelayTimer--;
        }

        if (SoundTimer > 0)
        {
            SoundTimer--;
        }
    }

    /// <summary>
    /// Clear the screen (opcode=00E0).
    /// </summary>
    private void ClearScreen()
    {
        Array.Clear(Display);
    }

    /// <summary>
    /// Jump (opcode=1NNN).
    /// </summary>
    /// <param name="nnn">The location to jump to</param>
    private void JumpTo(ushort nnn)
    {
        PC = nnn;
    }

    /// <summary>
    /// Set register (opcode=6XNN).
    /// </summary>
    /// <param name="x">The register to set</param>
    /// <param name="nn">The value to set</param>
    private void SetRegister(byte x, byte nn)
    {
        V[x] = nn;
    }

    /// <summary>
    /// Add to register (opcode=7XNN).
    /// Note: The carry flag is not set if the value overflows.
    /// </summary>
    /// <param name="x">The register to add to</param>
    /// <param name="nn">The value to add</param>
    private void AddToRegister(byte x, byte nn)
    {
        V[x] += nn;
    }

    /// <summary>
    /// Set Index register (opcode=ANNN).
    /// </summary>
    /// <param name="nnn">The value to set it to</param>
    private void SetIndexRegister(ushort nnn)
    {
        I = nnn;
    }

    /// <summary>
    /// Display sprite (opcode=DXYN).
    /// 
    /// Display n-byte sprite starting at memory location I at (Vx, Vy), set VF = collision.
    /// 
    /// The interpreter reads n bytes from memory, starting at the address stored in I.These
    /// bytes are then displayed as sprites on screen at coordinates(Vx, Vy). Sprites are
    /// XORed onto the existing screen. If this causes any pixels to be erased, VF is set to
    /// 1, otherwise it is set to 0. If the sprite is positioned so part of it is outside the
    /// coordinates of the display, it wraps around to the opposite side of the screen.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="rows"></param>
    /// <remarks>
    /// https://tobiasvl.github.io/blog/write-a-chip-8-emulator/#dxyn-display
    /// </remarks>
    private void DisplaySprite(byte x, byte y, byte rows)
    {
        var xc = V[x] % DisplayWidth;   // or V[x] & 63, x coord of the display where the sprite will go
        var yc = V[y] % DisplayHeight;  // or V[y] & 31, y coord of the display where the sprite will go

        var flipped = false;    // detect collision

        for (var row = 0; row < rows; row++)
        {
            var currentY = yc + row;
            
            if (currentY >= DisplayHeight)
                break;

            var spriteRow = Memory[I + row];

            for (var bit = 0; bit < 8; bit++)
            {
                var currentX = xc + bit;

                if (currentX >= DisplayWidth)
                    continue;

                var spritePixel = (spriteRow >> (7 - bit)) & 1;
                if (spritePixel == 0) continue;

                var currentPixel = Display[currentX, currentY];
                if (currentPixel) flipped = true;

                // flip bit at position idx
                Display[currentX, currentY] = !Display[currentX, currentY];
            }
        }

        V[0xF] = flipped ? (byte)1 : (byte)0;
    }


    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        var builder = new StringBuilder();

        builder.Append($"PC={PC.ToString("X4")}, ");
        builder.Append($"IR={I.ToString("X4")}");
        builder.AppendLine();

        for (var i = 0; i < V.Length; i++)
        {
            builder.Append($"{i:00}={V[i].ToString("X2")}, ");
        }

        builder.AppendLine();
        builder.AppendLine();

        builder.AppendLine("Memory: ");
        for (var a = 0; a < Memory.Length; a++)
        {
            if (a % 32 == 0)
            {
                builder.Append($"{a:0000} ");
                builder.Append($"{a:X3}: ");
            }
            builder.Append($"{Memory[a]:X2} ");
            if (a % 32 == 31) builder.AppendLine();
        }

        builder.AppendLine();

        builder.AppendLine("Display:");
        for (var w = 0; w < DisplayWidth; w++)
        {
            builder.Append(w % 10);
        }

        builder.AppendLine();

        for (var h = 0; h < DisplayHeight; h++)
        {
            for (var w = 0; w < DisplayWidth; w++)
            {
                var pixel = Display[w, h];
                builder.Append(pixel ? "#" : ".");
            }
            builder.AppendLine();
        }

        return builder.ToString();
    }
}