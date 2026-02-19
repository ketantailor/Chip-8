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

    public byte[] Memory { get; private set; } = new byte[Constants.MemorySize];

    /// <summary>Current OpCode.</summary>
    public ushort OpCode { get; private set; }

    public bool[,] Display { get; private set; } = new bool[Constants.DisplayWidth, Constants.DisplayHeight];
    public bool DisplayUpdated = false;


    public Cpu()
    {
        PC = Constants.ProgramStart;
        OpCode = 0;
        I = 0;
    }

    public void Load(byte[] rom)
    {
        Array.Copy(rom, 0, Memory, Constants.ProgramStart, rom.Length);
        PC = Constants.ProgramStart;
    }

    /// <summary>
    /// Load font data into memory.
    /// </summary>
    /// <param name="font">The font data</param>
    /// <exception cref="ArgumentException">Unexpected data length</exception>
    public void LoadFont(byte[] font)
    {
        if (font?.Length != Constants.FontSize)
            throw new ArgumentException($"Font data should be {Constants.FontSize} bytes, received {font?.Length}.", nameof(font));
        Array.Copy(font, 0, Memory, Constants.FontStart, font.Length);
    }

    public void Step()
    {
        // ensure we have at least two bytes to fetch an opcode
        if (PC >= Memory.Length - 1) throw new InvalidOperationException($"PC (0x{PC:X4}) is out of memory bounds.");

        DisplayUpdated = false;

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
                    ClearDisplay();
                }
                if (OpCode == 0x00EE)
                {
                    PopFromStack();
                }
                break;
            case 0x1000:
                JumpTo(nnn);
                break;
            case 0x2000:
                CallSubroutine(nnn);
                break;
            case 0x3000:
                SkipIfEqual(x, nn);
                break;
            case 0x4000:
                SkipIfNotEqual(x, nn);
                break;
            case 0x5000:
                SkipIfRegistersEqual(x, y);
                break;
            case 0x6000:
                SetRegister(x, nn);
                break;
            case 0x7000:
                AddToRegister(x, nn);
                break;
            case 0x8000:
                switch(n)
                {
                    case 0x0:
                        CopyToRegister(x, y);
                        break;
                    case 0x1:
                        RegisterOr(x, y);
                        break;
                    case 0x2:
                        RegisterAnd(x, y);
                        break;
                    default:
                        throw new InvalidOperationException($"Unknown opcode: {OpCode:x}");
                }
                break;
            case 0x9000:
                SkipIfRegistersNotEqual(x, y);
                break;
            case 0xA000:
                SetIndexRegister(nnn);
                break;
            case 0xD000:
                DisplaySprite(x, y, n);
                break;
            default:
                throw new InvalidOperationException($"Unknown opcode: {OpCode:x}");
        }

        if (DelayTimer > 0) DelayTimer--;
        if (SoundTimer > 0) SoundTimer--;
    }


    /// <summary>
    /// Clear the display (opcode=00E0).
    /// </summary>
    private void ClearDisplay()
    {
        Array.Clear(Display);
        DisplayUpdated = true;
    }

    /// <summary>
    /// Pop from stack, exit a subroutine (opcode=00EE).
    /// </summary>
    private void PopFromStack()
    {
        var pc = Stack.Pop();
        JumpTo(pc);
    }

    /// <summary>
    /// Jump to location NNN (opcode=1NNN).
    /// </summary>
    /// <param name="nnn">The location to jump to</param>
    private void JumpTo(ushort nnn)
    {
        PC = nnn;
    }

    /// <summary>
    /// Call subroutine at location NNN (opcode=2NNN).
    /// </summary>
    /// <param name="nnn">The location to jump to</param>
    private void CallSubroutine(ushort nnn)
    {
        Stack.Push(PC);
        PC = nnn;
    }

    /// <summary>
    /// Skip if VX is equal to NN (opcode=3XNN).
    /// </summary>
    /// <param name="x">The register to compare with</param>
    /// <param name="nn">The value to compare</param>
    private void SkipIfEqual(byte x, byte nn)
    {
        if (V[x] == nn)
            PC += 2;
    }

    /// <summary>
    /// Skip if VX is not equal to NN (opcode=4XNN).
    /// </summary>
    /// <param name="x">The register to compare with</param>
    /// <param name="nn">The value to compare</param>
    private void SkipIfNotEqual(byte x, byte nn)
    {
        if (V[x] != nn)
            PC += 2;
    }

    /// <summary>
    /// Skip if VX is equal to VY (opcode=5XY0).
    /// </summary>
    /// <param name="x">The first register to compare</param>
    /// <param name="y">The second register to compare</param>
    private void SkipIfRegistersEqual(byte x, byte y)
    {
        if (V[x] == V[y])
            PC += 2;
    }

    /// <summary>
    /// Skip if VX is not equal to VY (opcode=9XY0).
    /// </summary>
    /// <param name="x">The first register to compare</param>
    /// <param name="y">The second register to compare</param>
    private void SkipIfRegistersNotEqual(byte x, byte y)
    {
        if (V[x] != V[y])
            PC += 2;
    }

    /// <summary>
    /// Set register X to NN (opcode=6XNN).
    /// </summary>
    /// <param name="x">The register to set</param>
    /// <param name="nn">The value to set</param>
    private void SetRegister(byte x, byte nn)
    {
        V[x] = nn;
    }

    /// <summary>
    /// Adds NN to register X (opcode=7XNN).
    /// Note: The carry flag is not set if the value overflows.
    /// </summary>
    /// <param name="x">The register to add to</param>
    /// <param name="nn">The value to add</param>
    private void AddToRegister(byte x, byte nn)
    {
        V[x] += nn;
    }

    /// <summary>
    /// Copy VY to VX (opcode=8XY0).
    /// </summary>
    /// <param name="x">The register to copy to</param>
    /// <param name="y">The register to copy from</param>
    private void CopyToRegister(byte x, byte y)
    {
        V[x] = V[y];
    }

    /// <summary>
    /// VX = VX OR VY (opcode=8XY1).
    /// </summary>
    private void RegisterOr(byte x, byte y)
    {
        V[x] |= V[y];
    }

    /// <summary>
    /// VX = VX AND VY (opcode=8XY1).
    /// </summary>
    private void RegisterAnd(byte x, byte y)
    {
        V[x] &= V[y];
    }

    /// <summary>
    /// Set Index register to NNN (opcode=ANNN).
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
        var xc = V[x] % Constants.DisplayWidth;   // or V[x] & 63, x coord of the display where the sprite will go
        var yc = V[y] % Constants.DisplayHeight;  // or V[y] & 31, y coord of the display where the sprite will go

        var flipped = false;    // detect collision

        for (var row = 0; row < rows; row++)
        {
            var currentY = yc + row;
            
            if (currentY >= Constants.DisplayHeight)
                break;

            var spriteRow = Memory[I + row];

            for (var bit = 0; bit < 8; bit++)
            {
                var currentX = xc + bit;

                if (currentX >= Constants.DisplayWidth)
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

        DisplayUpdated = true;
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
        for (var w = 0; w < Constants.DisplayWidth; w++)
        {
            builder.Append(w % 10);
        }

        builder.AppendLine();

        for (var h = 0; h < Constants.DisplayHeight; h++)
        {
            for (var w = 0; w < Constants.DisplayWidth; w++)
            {
                var pixel = Display[w, h];
                builder.Append(pixel ? "#" : ".");
            }
            builder.AppendLine();
        }

        return builder.ToString();
    }
}