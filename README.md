# Chip-8

[![License: MIT](https://img.shields.io/badge/License-MIT-blue)](https://github.com/ketantailor/Chip-8/blob/main/LICENSE) [![Build](https://github.com/ketantailor/Chip-8/actions/workflows/build.yaml/badge.svg?branch=main)](https://github.com/ketantailor/Chip-8/actions/workflows/build.yaml)

Chip-8 emulator/interpreter in C# based on [Guide to making a CHIP-8 emulator](https://tobiasvl.github.io/blog/write-a-chip-8-emulator/).


## Usage

```bash
# run the interpreter with the specified rom
dotnet run --project src/Chip-8.RaylibApp --configuration Release -- roms/2-ibm-logo.ch8
```


## Tests Passing

Test | Status
---- | ------
[Chip-8 Splash Screen](https://github.com/Timendus/chip8-test-suite/tree/main?tab=readme-ov-file#chip-8-splash-screen) | 👍
[IBM Logo](https://github.com/Timendus/chip8-test-suite/tree/main?tab=readme-ov-file#ibm-logo) | 👍
[Corax+ Opcode Test](https://github.com/Timendus/chip8-test-suite/tree/main?tab=readme-ov-file#corax-opcode-test) | 
[Flags Test](https://github.com/Timendus/chip8-test-suite/tree/main?tab=readme-ov-file#flags-test) | 
[Quirks Test](https://github.com/Timendus/chip8-test-suite/tree/main?tab=readme-ov-file#quirks-test) | 
[Keypad Test](https://github.com/Timendus/chip8-test-suite/tree/main?tab=readme-ov-file#keypad-test) | 
[Beep Test](https://github.com/Timendus/chip8-test-suite/tree/main?tab=readme-ov-file#beep-test) | 


## Useful Links

- [Cowgod's Chip-8 Technical Reference](http://devernay.free.fr/hacks/chip8/C8TECH10.HTM)
- [Chip-8 Test Suite](https://github.com/Timendus/chip8-test-suite)
