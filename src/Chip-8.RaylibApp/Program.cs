using Chip8.Core;
using Raylib_cs;

const int Scale = 10;

Color CharcoalGray = Color.FromHSV(204, 0.316f, 0.310f);    // #36454F
Color Gold = Color.FromHSV(51, 1.0f, 1.0f);                 // #FFD700

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
    Raylib.SetTargetFPS(30);

    Raylib.BeginDrawing();
    Raylib.ClearBackground(Background);
    Raylib.EndDrawing();

    while (!Raylib.WindowShouldClose())
    {
        cpu.Step();

        if (cpu.DisplayUpdated)
        {
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
}
