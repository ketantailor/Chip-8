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
        ClassicAssert.AreEqual(Constants.MemorySize, cpu.Memory.Length);
        ClassicAssert.AreEqual(Constants.DisplayWidth, cpu.Display.GetLength(0));
        ClassicAssert.AreEqual(Constants.DisplayHeight, cpu.Display.GetLength(1));
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

        cpu.Memory[Constants.ProgramStart] = 0xFF;

        ClassicAssert.Throws<InvalidOperationException>(() =>
        {
            cpu.Step();
        });
    }

    [Test]
    public void Step_0x00E0_ClearsDisplay()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x00;
        cpu.Memory[0x201] = 0xE0;

        cpu.Step();

        for (var x = 0; x < Constants.DisplayWidth; x++)
        {
            for (var y = 0; y < Constants.DisplayHeight; y++)
            {
                ClassicAssert.IsFalse(cpu.Display[x, y]);
            }
        }
    }

    [Test]
    public void Step_0X1NNN_SetsPC()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x11;
        cpu.Memory[0x201] = 0x23;

        cpu.Step();

        ClassicAssert.AreEqual(0x0123, cpu.PC);
    }

    [Test]
    public void Set_0X2NNN_CallsSubroutineAndReturns()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x23;
        cpu.Memory[0x201] = 0x98;
        cpu.Memory[0x398] = 0x00;
        cpu.Memory[0x399] = 0xEE;

        cpu.Step();
        ClassicAssert.AreEqual(0x0398, cpu.PC);

        cpu.Step();
        ClassicAssert.AreEqual(0x0202, cpu.PC);
    }

    [Test]
    public void Set_0X3XNN_SkipsWhenEqual()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0x99;

        cpu.Memory[0x200] = 0x31;
        cpu.Memory[0x201] = 0x99;

        cpu.Step();

        ClassicAssert.AreEqual(0x0204, cpu.PC);
    }
    
    [Test]
    public void Set_0X3XNN_ContinuesWhenNotEqual()
    {
        var cpu = new Cpu();

        cpu.V[2] = 0x99;

        cpu.Memory[0x200] = 0x32;
        cpu.Memory[0x201] = 0x98;

        cpu.Step();

        ClassicAssert.AreEqual(0x0202, cpu.PC);
    }

    [Test]
    public void Set_0X4XNN_SkipsWhenNotEqual()
    {
        var cpu = new Cpu();

        cpu.V[3] = 0x99;

        cpu.Memory[0x200] = 0x43;
        cpu.Memory[0x201] = 0x98;

        cpu.Step();

        ClassicAssert.AreEqual(0x0204, cpu.PC);
    }

    [Test]
    public void Set_0X4XNN_ContinuesWhenEqual()
    {
        var cpu = new Cpu();

        cpu.V[4] = 0x99;

        cpu.Memory[0x200] = 0x44;
        cpu.Memory[0x201] = 0x99;

        cpu.Step();

        ClassicAssert.AreEqual(0x0202, cpu.PC);
    }

    [Test]
    public void Set_0X5XY0_SkipsWhenEqual()
    {
        var cpu = new Cpu();

        cpu.V[5] = 0x99;
        cpu.V[6] = 0x99;

        cpu.Memory[0x200] = 0x55;
        cpu.Memory[0x201] = 0x60;

        cpu.Step();

        ClassicAssert.AreEqual(0x0204, cpu.PC);
    }

    [Test]
    public void Set_0X5XY0_ContinuesWhenNotEqual()
    {
        var cpu = new Cpu();

        cpu.V[7] = 0x99;
        cpu.V[8] = 0x98;

        cpu.Memory[0x200] = 0x57;
        cpu.Memory[0x201] = 0x80;

        cpu.Step();

        ClassicAssert.AreEqual(0x0202, cpu.PC);
    }

    [Test]
    public void Step_0X61NN_SetsRegister()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x61;
        cpu.Memory[0x201] = 0x23;

        cpu.Step();

        ClassicAssert.AreEqual(0x23, cpu.V[1]);
    }

    [Test]
    public void Step_0X6FNN_SetsRegister()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x6F;
        cpu.Memory[0x201] = 0x34;

        cpu.Step();

        ClassicAssert.AreEqual(0x34, cpu.V[0xF]);
    }

    [Test]
    public void Step_0X71NN_AddsToRegister()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x71;
        cpu.Memory[0x201] = 0x23;
        cpu.V[0x1] = 0x05;

        cpu.Step();

        ClassicAssert.AreEqual(0x28, cpu.V[0x1]);
    }

    [Test]
    public void Step_0X7FNN_AddsToRegisterWithOverflow()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x7F;
        cpu.Memory[0x201] = 0x23;
        cpu.V[15] = 0xFF;

        cpu.Step();

        ClassicAssert.AreEqual(0x22, cpu.V[15]);
    }

    [Test]
    public void Step_0X8XY0_CopiesToRegister()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0x99;
        cpu.V[2] = 0x98;

        cpu.Memory[0x200] = 0x81;
        cpu.Memory[0x201] = 0x20;

        cpu.Step();

        ClassicAssert.AreEqual(0x98, cpu.V[1]);
    }

    [Test]
    public void Step_0X8XY1_Or()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0b_1010_1100;
        cpu.V[2] = 0b_0011_0010;

        cpu.Memory[0x200] = 0x81;
        cpu.Memory[0x201] = 0x21;

        cpu.Step();

        ClassicAssert.AreEqual(0b_1011_1110, cpu.V[1]);
    }

    [Test]
    public void Step_0X8XY2_And()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0b_1010_1100;
        cpu.V[2] = 0b_0011_0010;

        cpu.Memory[0x200] = 0x81;
        cpu.Memory[0x201] = 0x22;

        cpu.Step();

        ClassicAssert.AreEqual(0b_0010_0000, cpu.V[1]);
    }

    [Test]
    public void Step_0X8XY3_Xor()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0b_1010_1100;
        cpu.V[2] = 0b_0011_0010;

        cpu.Memory[0x200] = 0x81;
        cpu.Memory[0x201] = 0x23;

        cpu.Step();

        ClassicAssert.AreEqual(0b_1001_1110, cpu.V[1]);
    }

    [Test]
    public void Step_0X8XY4_AddWithCarry()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0xF0;
        cpu.V[2] = 0x20;

        cpu.Memory[0x200] = 0x81;
        cpu.Memory[0x201] = 0x24;

        cpu.Step();

        ClassicAssert.AreEqual(0x10, cpu.V[1]);
        ClassicAssert.AreEqual(0x1, cpu.V[0xF]);
    }
    
    [Test]
    public void Step_0X8XY4_AddWithNoCarry()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0xF0;
        cpu.V[2] = 0x02;

        cpu.Memory[0x200] = 0x81;
        cpu.Memory[0x201] = 0x24;

        cpu.Step();

        ClassicAssert.AreEqual(0xF2, cpu.V[1]);
        ClassicAssert.AreEqual(0x0, cpu.V[0xF]);
    }

    [Test]
    public void Step_0X8XY5_SubtractWithNoBorrow()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0x50;
        cpu.V[2] = 0x20;

        cpu.Memory[0x200] = 0x81;
        cpu.Memory[0x201] = 0x25;

        cpu.Step();

        ClassicAssert.AreEqual(0x30, cpu.V[1]);
        ClassicAssert.AreEqual(0x1, cpu.V[0xF]);
    }

    [Test]
    public void Step_0X8XY5_SubtractWithBorrow()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0x20;
        cpu.V[2] = 0x50;

        cpu.Memory[0x200] = 0x81;
        cpu.Memory[0x201] = 0x25;

        cpu.Step();

        ClassicAssert.AreEqual(0xD0, cpu.V[1]);
        ClassicAssert.AreEqual(0x0, cpu.V[0xF]);
    }

    [Test]
    public void Step_0X8XY7_SubtractWithNoBorrow()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0x20;
        cpu.V[2] = 0x50;

        cpu.Memory[0x200] = 0x81;
        cpu.Memory[0x201] = 0x27;

        cpu.Step();

        ClassicAssert.AreEqual(0x30, cpu.V[1]);
        ClassicAssert.AreEqual(0x1, cpu.V[0xF]);
    }

    [Test]
    public void Step_0X8XY7_SubtractWithBorrow()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0x50;
        cpu.V[2] = 0x20;

        cpu.Memory[0x200] = 0x81;
        cpu.Memory[0x201] = 0x27;

        cpu.Step();

        ClassicAssert.AreEqual(0xD0, cpu.V[1]);
        ClassicAssert.AreEqual(0x0, cpu.V[0xF]);
    }

    [Test]
    public void Step_0X8XY6_ShiftRightWithLsbSet()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0b_1010_1101;

        cpu.Memory[0x200] = 0x81;
        cpu.Memory[0x201] = 0x26;

        cpu.Step();

        ClassicAssert.AreEqual(0b_0101_0110, cpu.V[1]);
        ClassicAssert.AreEqual(0x1, cpu.V[0xF]);
    }

    [Test]
    public void Step_0X8XY6_ShiftRightWithLsbClear()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0b_1010_1100;

        cpu.Memory[0x200] = 0x81;
        cpu.Memory[0x201] = 0x26;

        cpu.Step();

        ClassicAssert.AreEqual(0b_0101_0110, cpu.V[1]);
        ClassicAssert.AreEqual(0x0, cpu.V[0xF]);
    }

    [Test]
    public void Step_0X8XYE_ShiftLeftWithMsbSet()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0b_1010_1100;

        cpu.Memory[0x200] = 0x81;
        cpu.Memory[0x201] = 0x2E;

        cpu.Step();

        ClassicAssert.AreEqual(0b_0101_1000, cpu.V[1]);
        ClassicAssert.AreEqual(0x1, cpu.V[0xF]);
    }

    [Test]
    public void Step_0X8XYE_ShiftLeftWithMsbClear()
    {
        var cpu = new Cpu();

        cpu.V[1] = 0b_0010_1100;

        cpu.Memory[0x200] = 0x81;
        cpu.Memory[0x201] = 0x2E;

        cpu.Step();

        ClassicAssert.AreEqual(0b_0101_1000, cpu.V[1]);
        ClassicAssert.AreEqual(0x0, cpu.V[0xF]);
    }

    [Test]
    public void Step_0x8F00_ThrowsException()
    {
        var cpu = new Cpu();

        cpu.Memory[0x200] = 0x80;
        cpu.Memory[0x201] = 0x0F;

        Assert.Throws<InvalidOperationException>(cpu.Step);
    }

    [Test]
    public void Set_0X9XY0_SkipsWhenNotEqual()
    {
        var cpu = new Cpu();

        cpu.V[5] = 0x99;
        cpu.V[6] = 0x98;

        cpu.Memory[0x200] = 0x95;
        cpu.Memory[0x201] = 0x60;

        cpu.Step();

        ClassicAssert.AreEqual(0x0204, cpu.PC);
    }

    [Test]
    public void Set_0X9XY0_ContinuesWhenEqual()
    {
        var cpu = new Cpu();

        cpu.V[7] = 0x99;
        cpu.V[8] = 0x99;

        cpu.Memory[0x200] = 0x97;
        cpu.Memory[0x201] = 0x80;

        cpu.Step();

        ClassicAssert.AreEqual(0x0202, cpu.PC);
    }

    [Test]
    public void Step_0XANNN_SetsI()
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
