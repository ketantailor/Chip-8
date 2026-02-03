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

}
