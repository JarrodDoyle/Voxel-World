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
        InitWindow(1280, 720, "Raylib + Dear ImGui app");

        ImGuiController.Setup();
        var uiLayers = new List<UiLayer> {new GeneratorLayer {Open = true}};
        foreach (var layer in uiLayers)
            layer.Attach();

        var camera = new Camera3D
        {
            position = new Vector3(0, 56, 0),
            target = Vector3.Zero,
            up = Vector3.UnitY,
            fovy = 60,
            projection = CameraProjection.CAMERA_PERSPECTIVE
        };
        Raylib.SetCameraMode(camera, CameraMode.CAMERA_FREE);
        Raylib.SetCameraMode(camera, CameraMode.CAMERA_FIRST_PERSON);

        const int radius = 8;
        var world = new World((int) DateTime.Now.ToBinary(), 0, new Vector3(16));
        var chunkManager = new ChunkManager(world);

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
                chunkManager.LoadChunksAroundPosition(pos, radius);
            }
            else Raylib.EnableCursor();
            
            Raylib.BeginMode3D(camera);
            chunkManager.Update();
            chunkManager.Render();
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