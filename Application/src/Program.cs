using System.Numerics;
using Generator;
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
        // var uiLayers = new List<UiLayer>();
        // foreach (var layer in uiLayers)
        //     layer.Attach();

        var size = 4;
        var chunks = new Chunk[size * size * size];
        for (int x = 0; x < size; x++)
        for (int y = 0; y < size; y++)
        for (int z = 0; z < size; z++)
        {
            var chunk = new Chunk(new Vector3(x, y, z), new Vector3(16));
            chunk.GenerateBlocks();
            chunk.GenerateMesh();
            chunks[x * size * size + y * size + z] = chunk;
        }

        var camera = new Camera3D
        {
            position = new Vector3(0, 10, 10),
            target = Vector3.Zero,
            up = Vector3.UnitY,
            fovy = 60,
            projection = CameraProjection.CAMERA_PERSPECTIVE
        };
        Raylib.SetCameraMode(camera, CameraMode.CAMERA_FREE);

        while (!Raylib.WindowShouldClose())
        {
            // foreach (var layer in uiLayers)
            //     layer.Update();

            Raylib.BeginDrawing();
            
            Raylib.ClearBackground(Color.BLACK);

            Raylib.UpdateCamera(ref camera);
            Raylib.BeginMode3D(camera);
            foreach (var chunk in chunks)
                chunk.Render();
            Raylib.EndMode3D();

            ImGuiController.Begin();
            ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
            // foreach (var layer in uiLayers)
            //     layer.Render();
            ImGuiController.End();

            Raylib.DrawFPS(0, 0);
            Raylib.EndDrawing();
        }

        ImGuiController.Shutdown();
        Raylib.CloseWindow();
    }
}