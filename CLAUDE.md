# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

CHIP-8 emulator/interpreter written in C# targeting .NET 10. Uses Raylib for graphics rendering.

Based on [Guide to making a CHIP-8 emulator](https://tobiasvl.github.io/blog/write-a-chip-8-emulator/).

## Build Commands

```bash
# Build all projects
dotnet build src/Chip-8.slnx

# Run tests
dotnet test src/Chip-8.slnx

# Run a single test
dotnet test src/Chip-8.slnx --filter "FullyQualifiedName~TestMethodName"

# Run the emulator with a ROM
dotnet run --project src/Chip-8.RaylibApp -- path/to/rom.ch8
```

## Architecture

### Project Structure

- **Chip-8.Core** - Core emulator logic (CPU, memory, display state)
- **Chip-8.Core.Tests** - NUnit tests for the core library
- **Chip-8.RaylibApp** - Raylib-based graphical frontend

### CPU Implementation (`Cpu.cs`)

The `Cpu` class contains all emulator state and opcode execution:
- `PC` - Program counter (starts at 0x200)
- `I` - Index register
- `V[16]` - General-purpose registers (V0-VF)
- `Memory[4096]` - 4KB memory
- `Display[64,32]` - Boolean array for 64x32 monochrome display
- `Stack` - Subroutine return addresses
- `DelayTimer`, `SoundTimer` - Timers decremented each step

`Step()` fetches, decodes, and executes one instruction. Opcodes are decoded by masking with `0xF000` and extracting `nnn`, `nn`, `n`, `x`, `y` fields.

### Memory Layout

- `0x000-0x04F` - Reserved
- `0x050-0x09F` - Font data (80 bytes)
- `0x200-0xFFF` - Program/data space (ROMs load here)

### Implemented Opcodes

- `00E0` - Clear display
- `1NNN` - Jump
- `6XNN` - Set VX = NN
- `7XNN` - Add NN to VX
- `ANNN` - Set I = NNN
- `DXYN` - Draw sprite

## Reference Documentation

- [Cowgod's Chip-8 Technical Reference](http://devernay.free.fr/hacks/chip8/C8TECH10.HTM)
- [Chip-8 Test Suite](https://github.com/Timendus/chip8-test-suite) - Use these ROMs for testing
