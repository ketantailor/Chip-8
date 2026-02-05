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

        ClassicAssert.AreEqual(Fonts.F1[0x00], cpu.Memory[0x050]);
        ClassicAssert.AreEqual(Fonts.F1[0x4F], cpu.Memory[0x09F]);
    }

    [Test]
    public void Step_WithInvalidCounter_ThrowsException()
    {
        var cpu = new Cpu();

        cpu.PC = 0x1000;

        ClassicAssert.Throws<InvalidOperationException>(() =>
        {
            cpu.Step();
        });
    }

    [Test]
    public void Step_WithInvalidOperation_ThrowsException()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0xFF;

        ClassicAssert.Throws<InvalidOperationException>(() =>
        {
            cpu.Step();
        });
    }

    [Test]
    public void Step_0x00E0_ClearsScreen()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x00;
        cpu.Memory[0x201] = 0xE0;

        cpu.Step();

        for (var x = 0; x < Cpu.DisplayWidth; x++)
        {
            for (var y = 0; y < Cpu.DisplayHeight; y++)
            {
                ClassicAssert.IsFalse(cpu.Display[x, y]);
            }
        }
    }

    [Test]
    public void Step_0x1nnn_SetsPC()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x11;
        cpu.Memory[0x201] = 0x23;

        cpu.Step();

        ClassicAssert.AreEqual(0x0123, cpu.PC);
    }

    [Test]
    public void Step_0x61nn_SetsPC()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x61;
        cpu.Memory[0x201] = 0x23;

        cpu.Step();

        ClassicAssert.AreEqual(0x23, cpu.V[1]);
    }

    [Test]
    public void Step_0x6Fnn_SetsPC()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x6F;
        cpu.Memory[0x201] = 0x34;

        cpu.Step();

        ClassicAssert.AreEqual(0x34, cpu.V[15]);
    }

    [Test]
    public void Step_0x71nn_SetsPC()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x71;
        cpu.Memory[0x201] = 0x23;
        cpu.V[1] = 0x05;

        cpu.Step();

        ClassicAssert.AreEqual(0x28, cpu.V[1]);
    }

    [Test]
    public void Step_0x7Fnn_SetsPC()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x7F;
        cpu.Memory[0x201] = 0x23;
        cpu.V[15] = 0xFF;

        cpu.Step();

        ClassicAssert.AreEqual(0x22, cpu.V[15]);
    }

    [Test]
    public void Step_0xAnnn_SetsI()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0xA1;
        cpu.Memory[0x201] = 0x23;

        cpu.Step();

        ClassicAssert.AreEqual(0x0123, cpu.I);
    }

    [Test]
    public void Step_DecrementsDelayTimer()
    {
        var cpu = new Cpu();

        cpu.DelayTimer = 5;

        cpu.Step();
        ClassicAssert.AreEqual(4, cpu.DelayTimer);

        cpu.Step();
        ClassicAssert.AreEqual(3, cpu.DelayTimer);
    }

    [Test]
    public void Step_DecrementsSoundTimer()
    {
        var cpu = new Cpu();

        cpu.SoundTimer = 15;

        cpu.Step();
        ClassicAssert.AreEqual(14, cpu.SoundTimer);

        cpu.Step();
        ClassicAssert.AreEqual(13, cpu.SoundTimer);
    }

    [Test]
    public void Step_DisplaySprite()
    {
        var cpu = new Cpu();

        cpu.V[0] = 0; // x
        cpu.V[1] = 0; // y
        cpu.I = 0x300; // location of sprite

        cpu.Memory[0x200] = 0xD0;
        cpu.Memory[0x201] = 0x12; // display sprite at x=0, y=0, rows=1

        cpu.Memory[0x300] = 0b10101010; // 0xAA
        cpu.Memory[0x301] = 0b11110000; // 0xF0
        
        
        cpu.Step();


        ClassicAssert.IsTrue(cpu.Display[0, 0]);
        ClassicAssert.IsFalse(cpu.Display[1, 0]);
        ClassicAssert.IsTrue(cpu.Display[2, 0]);
        ClassicAssert.IsFalse(cpu.Display[3, 0]);
        ClassicAssert.IsTrue(cpu.Display[4, 0]);
        ClassicAssert.IsFalse(cpu.Display[5, 0]);
        ClassicAssert.IsTrue(cpu.Display[6, 0]);
        ClassicAssert.IsFalse(cpu.Display[7, 0]);

        ClassicAssert.IsTrue(cpu.Display[0, 1]);
        ClassicAssert.IsTrue(cpu.Display[1, 1]);
        ClassicAssert.IsTrue(cpu.Display[2, 1]);
        ClassicAssert.IsTrue(cpu.Display[3, 1]);
        ClassicAssert.IsFalse(cpu.Display[4, 1]);
        ClassicAssert.IsFalse(cpu.Display[5, 1]);
        ClassicAssert.IsFalse(cpu.Display[6, 1]);
        ClassicAssert.IsFalse(cpu.Display[7, 1]);
    }
}
