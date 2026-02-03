namespace Chip8.Core.Tests;

using NUnit.Framework.Legacy;

public class CpuTests
{
    [Test]
    public void Ctor_PropertiesSetCorrectly()
    {
        var cpu = new Cpu();

        ClassicAssert.AreEqual(0x200, cpu.PC);
        ClassicAssert.AreEqual(0x0, cpu.OpCode);
        ClassicAssert.AreEqual(0x0, cpu.I);
        ClassicAssert.AreEqual(0, cpu.Stack.Count);
        ClassicAssert.AreEqual(Cpu.MemorySize, cpu.Memory.Length);
        ClassicAssert.AreEqual(Cpu.DisplayWidth, cpu.Display.GetLength(0));
        ClassicAssert.AreEqual(Cpu.DisplayHeight, cpu.Display.GetLength(1));
    }

    [Test]
    public void LoadFont_WithInvalidFont_ThrowsException()
    {
        var cpu = new Cpu();

        ClassicAssert.Throws<ArgumentException>(() =>
        {
            cpu.LoadFont(Array.Empty<byte>());
        });
    }

    [Test]
    public void LoadFont_WithValidFont_SetsMemoryCorrectly()
    {
        var cpu = new Cpu();

        cpu.LoadFont(Fonts.F1);

        ClassicAssert.AreEqual(Fonts.F1[0x0], cpu.Memory[0x050]);
        ClassicAssert.AreEqual(Fonts.F1[0x4F], cpu.Memory[0x09F]);
    }


    [Test]
    public void Execute_0x00E0_ClearsScreen()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x00;
        cpu.Memory[0x201] = 0xE0;

        cpu.Step();

        for (var x = 0; x < Cpu.DisplayWidth; x++)
        {
            for (var y = 0; y < Cpu.DisplayHeight; y++)
            {
                ClassicAssert.AreEqual(0x0, cpu.Display[x, y]);
            }
        }
    }

    [Test]
    public void Execute_0x1nnn_SetsPC()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x11;
        cpu.Memory[0x201] = 0x23;

        cpu.Step();

        ClassicAssert.AreEqual(0x0123, cpu.PC);
    }

    [Test]
    public void Execute_0x61nn_SetsPC()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x61;
        cpu.Memory[0x201] = 0x23;

        cpu.Step();

        ClassicAssert.AreEqual(0x23, cpu.V[1]);
    }

    [Test]
    public void Execute_0x6Fnn_SetsPC()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x6F;
        cpu.Memory[0x201] = 0x34;

        cpu.Step();

        ClassicAssert.AreEqual(0x34, cpu.V[15]);
    }

    /*
}
