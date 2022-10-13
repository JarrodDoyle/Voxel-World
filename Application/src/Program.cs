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
            position = new Vector3(0, 32, 0),
            target = Vector3.UnitX,
            up = Vector3.UnitY,
            fovy = 60,
            projection = CameraProjection.CAMERA_PERSPECTIVE
        };
        Raylib.SetCameraMode(camera, CameraMode.CAMERA_FREE);
        Raylib.SetCameraMode(camera, CameraMode.CAMERA_FIRST_PERSON);

        var world = new World((int) DateTime.Now.ToBinary(), 2, new Vector3(16));
        var chunkManager = new ChunkManager(world);
        chunkManager.LoadRadius = 8;

        while (!Raylib.WindowShouldClose())
        {
            UpdateCamera(ref camera);
            foreach (var layer in uiLayers)
                layer.Update();

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);

            Raylib.BeginMode3D(camera);
            chunkManager.Update(camera.position);
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

    private static void UpdateCamera(ref Camera3D camera)
    {
        // Read camera stuffs
        var position = camera.position;
        var forward = Vector3.Normalize(camera.target - position);
        var up = camera.up;
        var right = Vector3.Cross(forward, up);
        var rotation = new Vector3(
            MathF.Asin(-forward.Y) * 180f / MathF.PI,
            MathF.Atan2(forward.X, forward.Z) * 180f / MathF.PI,
            0f
        );

        // Handle inputs
        const float mouseSensitivity = 0.25f;
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        {
            var mouseDelta = Raylib.GetMouseDelta();
            rotation.X += mouseDelta.Y * mouseSensitivity;
            rotation.Y += mouseDelta.X * -mouseSensitivity;
            Raylib.DisableCursor();
        }
        else Raylib.EnableCursor();

        var move = Vector3.Zero;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_W)) move.Z += 1;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_S)) move.Z -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_Q)) move.Y += 1;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_E)) move.Y -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_D)) move.X += 1;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_A)) move.X -= 1;

        const float moveSpeed = 10f;
        var speedMultiplier = Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) ? 2.0f : 1.0f;
        if (move.LengthSquared() > 0) move = Vector3.Normalize(move);
        position += right * (move.X * moveSpeed * speedMultiplier * Raylib.GetFrameTime());
        position += forward * (move.Z * moveSpeed * speedMultiplier * Raylib.GetFrameTime());
        position += up * (move.Y * moveSpeed * speedMultiplier * Raylib.GetFrameTime());

        // Write camera stuffs
        var pitch = rotation.X * MathF.PI / 180f;
        var yaw = rotation.Y * MathF.PI / 180f;
        forward = new Vector3(MathF.Sin(yaw) * MathF.Cos(pitch), -MathF.Sin(pitch), MathF.Cos(yaw) * MathF.Cos(pitch));

        camera.position = position;
        camera.target = position + forward;
    }
}