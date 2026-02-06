# AGENTS.md

This file provides guidelines for agentic coding agents operating in this CHIP-8 emulator repository.

## Build/Lint/Test Commands

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

## Code Style Guidelines

### Imports
- Use C# namespace imports at the top of files
- Group using statements alphabetically
- Place system namespaces before third-party ones

### Formatting
- Follow C# standard formatting conventions
- Use 4 spaces for indentation (no tabs)
- Place opening braces on the same line as control statements
- Use blank lines to separate logical sections

### Naming Conventions
- PascalCase for classes, methods, properties, and namespaces
- camelCase for local variables and parameters
- Prefix private fields with underscore (e.g., _fieldName)
- Use descriptive names that clearly indicate purpose
- Follow .NET naming conventions

### Types
- Use explicit types rather than `var` when type isn't obvious
- Prefer `readonly` and `const` for immutable values
- Use `ushort` for CHIP-8 specific values like PC, I registers
- Use `byte[]` for memory arrays
- Use `bool[,]` for 2D display arrays

### Error Handling
- Throw appropriate exceptions with descriptive messages
- Use `ArgumentException` for invalid method arguments
- Use `InvalidOperationException` for invalid state transitions
- Handle out-of-bounds access gracefully
- Prefer checked operations in debug builds

### Documentation
- Include XML documentation comments for all public members
- Document parameters, return values, and exceptions
- Use /// comments for class, method, and property descriptions
- Include examples where helpful

### Testing
- Follow the existing test structure using NUnit
- Test each opcode implementation
- Test edge cases and error conditions
- Use descriptive test names that indicate what's being tested
- Include tests for memory boundaries and overflow scenarios

## Project Structure

- **Chip-8.Core** - Core emulator logic (CPU, memory, display state)
- **Chip-8.Core.Tests** - NUnit tests for the core library
- **Chip-8.RaylibApp** - Raylib-based graphical frontend

## Key Files

- `Cpu.cs` - Main CPU implementation with opcode execution
- `Constants.cs` - CHIP-8 constants and font data
- `CpuTests.cs` - Unit tests for CPU functionality
- `Fonts.cs` - Font data storage

## Opcodes Implemented
- `00E0` - Clear display
- `1NNN` - Jump
- `6XNN` - Set VX = NN
- `7XNN` - Add NN to VX
- `ANNN` - Set I = NNN
- `DXYN` - Draw sprite

## Memory Layout
- `0x000-0x04F` - Reserved
- `0x050-0x09F` - Font data (80 bytes)
- `0x200-0xFFF` - Program/data space (ROMs load here)