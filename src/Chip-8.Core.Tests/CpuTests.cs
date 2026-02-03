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
}
