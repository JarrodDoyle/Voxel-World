using System.Numerics;
using Application.Ui;
using Generator;
using ImGuiNET;
using Raylib_cs;

namespace Application;

internal static class Program
{
    private static void InitWindow(int width, int height, string title)
    {
        Raylib.SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT |
                              ConfigFlags.FLAG_WINDOW_RESIZABLE);
        Raylib.SetTraceLogLevel(TraceLogLevel.LOG_WARNING);
        Raylib.InitWindow(width, height, title);
        Raylib.SetWindowMinSize(640, 480);
    }

    private static void Main(string[] args)
    {
        InitWindow(1280, 720, "Raylib + Dear ImGui app");

        ImGuiController.Setup();
        var uiLayers = new List<UiLayer> {new GeneratorLayer {Open = true}};
        foreach (var layer in uiLayers)
            layer.Attach();

        var startTime = DateTime.Now;
        ChunkManager.Seed = (int) DateTime.Now.ToBinary();
        ChunkManager.ChunkDimensions = new Vector3(16);
        ChunkManager.LoadChunksAroundPos(Vector3.Zero, 5);
        Console.WriteLine($"Chunk gen time: {DateTime.Now - startTime}");

        var camera = new Camera3D
        {
            position = new Vector3(0, 10, 10),
            target = Vector3.Zero,
            up = Vector3.UnitY,
            fovy = 60,
            projection = CameraProjection.CAMERA_PERSPECTIVE
        };
        Raylib.SetCameraMode(camera, CameraMode.CAMERA_FIRST_PERSON);

        while (!Raylib.WindowShouldClose())
        {
            foreach (var layer in uiLayers)
                layer.Update();

            Raylib.BeginDrawing();

            Raylib.ClearBackground(Color.BLACK);

            if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT))
            {
                Raylib.DisableCursor();
                Raylib.UpdateCamera(ref camera);

                var pos = new Vector3(
                    MathF.Floor(camera.position.X / 16),
                    MathF.Floor(camera.position.Y / 16),
                    MathF.Floor(camera.position.Z / 16));
                ChunkManager.LoadChunksAroundPos(pos, 3);
            }
            else
            {
                Raylib.EnableCursor();
            }

            Raylib.BeginMode3D(camera);
            ChunkManager.Render();
            Raylib.EndMode3D();

            ImGuiController.Begin();
            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
            foreach (var layer in uiLayers)
                layer.Render();
            ImGuiController.End();

            Raylib.DrawFPS(0, 0);
            Raylib.EndDrawing();
        }

        ImGuiController.Shutdown();
        Raylib.CloseWindow();
    }
}