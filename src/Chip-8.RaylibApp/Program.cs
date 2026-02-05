using Chip8.Core;


if (args.Length == 0 || args.Contains("-h") || args.Contains("--help") || args.Contains("-?"))
{
    ShowHelp();
    Environment.Exit(0);
}

var romPath = args.FirstOrDefault(a => !a.StartsWith("-"));
if (!string.IsNullOrEmpty(romPath))
{
    RunEmulator(romPath);
    Environment.Exit(0);
}

ShowHelp();
Environment.Exit(0);


void ShowHelp()
{
    Console.WriteLine("""
        Chip-8 Emulator / Interpreter -- https://github.com/ketantailor/Chip-8

        Usage: chip-8 [ROM_FILE]

        Execute a chip-8 rom file.

        Additional options:
          -h, --help    Show this help.

        """);
}

void RunEmulator(string romPath)
{
    if (!File.Exists(romPath))
        throw new FileNotFoundException($"File {romPath} not found.");

    var romData = File.ReadAllBytes(romPath);

    var cpu = new Cpu();
    cpu.LoadFont(Fonts.F1);
    cpu.Load(romData);

    Console.WriteLine("Initial:");
    Console.WriteLine(cpu);

    for (var i = 0; i < 39; i++) cpu.Step();
    
    Console.WriteLine("After 39 cycles:");
    Console.WriteLine(cpu);
}
