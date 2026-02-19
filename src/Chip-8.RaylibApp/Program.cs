using Chip8.Core;
using Raylib_cs;

const int TargetFPS = 60;
const int StepsPerFrame = 12; // ~700 instructions/second at 60 FPS
const int Scale = 10;

Color CharcoalGray = new Color(0x36, 0x45, 0x4F);
Color Gold = new Color(0xFF, 0xD7, 0x00);

Color Background = CharcoalGray;
Color Foreground = Gold;


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

    Raylib.InitWindow(Constants.DisplayWidth * Scale, Constants.DisplayHeight * Scale, "Chip-8");
    Raylib.SetTargetFPS(TargetFPS);

    while (!Raylib.WindowShouldClose())
    {
        for (var i = 0; i < StepsPerFrame; i++)
            cpu.Step();

        Raylib.BeginDrawing();
        Raylib.ClearBackground(Background);

        for (var x = 0; x < Constants.DisplayWidth; x++)
        {
            for (var y = 0; y < Constants.DisplayHeight; y++)
            {
                if (cpu.Display[x, y])
                {
                    Raylib.DrawRectangle(x * Scale, y * Scale, Scale, Scale, Foreground);
                }
            }
        }

        Raylib.EndDrawing();
    }
}
