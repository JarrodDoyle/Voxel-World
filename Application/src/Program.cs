using System.Numerics;
using Application.Ui;
using VoxelWorld;
using ImGuiNET;
using Raylib_cs;

namespace Application;

internal static class Program
{
    private static void InitWindow(int width, int height, string title)
    {
        Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT | ConfigFlags.FLAG_VSYNC_HINT |
                              ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.SetTraceLogLevel(TraceLogLevel.LOG_WARNING);
        Raylib.InitWindow(width, height, title);
        Raylib.SetWindowMinSize(640, 480);
    }

    private static void Main(string[] args)
    {
        InitWindow(1280, 720, "Voxel World");
        UiManager.Setup();

        var cameraController = new CameraController(new Vector3(0, 16, 0), 10f, 0.25f);

        // Load a random palette
        var palettePaths = Directory.GetFiles("res/palettes/");
        var paletteIndex = new Random().Next(palettePaths.Length);
        var palettePath = palettePaths[paletteIndex];
        World.LoadPalette(palettePath);
        Console.WriteLine(palettePath);

        var world = new World((int) DateTime.Now.ToBinary(), 2, new Vector3(16));
        var chunkManager = new ChunkManager(world);
        chunkManager.LoadRadius = 8;

        var chunkShader = Raylib.LoadShader("res/shaders/base.vs", "res/shaders/base.fs");

        while (!Raylib.WindowShouldClose())
        {
            HandleInputs(cameraController, chunkManager);
            UiManager.Update();
            ThreadManager.Update();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);

            Raylib.BeginMode3D(cameraController.Camera);
            chunkManager.Update(cameraController.Camera.position);
            chunkManager.Render(chunkShader);
            Raylib.EndMode3D();

            UiManager.Render();
            Raylib.DrawFPS(0, 0);
            var camPos = cameraController.Camera.position;
            Raylib.DrawText($"({(int) camPos.X}, {(int) camPos.Y}, {(int) camPos.Z})", 0, 20,
                Raylib.GetFontDefault().baseSize * 2, Color.WHITE);
            Raylib.EndDrawing();
        }

        UiManager.Shutdown();
        Raylib.UnloadShader(chunkShader);
        Raylib.CloseWindow();
    }

    private static void HandleInputs(CameraController cameraController, ChunkManager chunkManager)
    {
        // Only handle inputs is ImGui doesn't want to
        var io = ImGui.GetIO();
        if (io.WantCaptureMouse || io.WantCaptureKeyboard) return;

        cameraController.Update();

        // Chunk Manager stuffs
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_B)) chunkManager.RebuildMeshes();

        // Toggle vsync
        if (Raylib.IsKeyPressed(KeyboardKey.KEY_V))
        {
            if (Raylib.IsWindowState(ConfigFlags.FLAG_VSYNC_HINT))
                Raylib.ClearWindowState(ConfigFlags.FLAG_VSYNC_HINT);
            else
                Raylib.SetWindowState(ConfigFlags.FLAG_VSYNC_HINT);
        }
    }
}