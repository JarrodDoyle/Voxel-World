using System.Numerics;
using Raylib_cs;

namespace Application;

public class CameraController
{
    public Camera3D Camera => _camera;
    public float MoveSpeed { get; set; }
    public float MouseSensitivity { get; set; }
    private const float MaxAngleY = 85f;
    private Camera3D _camera;

    public CameraController(Vector3 startPosition, float moveSpeed, float mouseSensitivity)
    {
        _camera = new Camera3D
        {
            position = startPosition,
            target = startPosition + Vector3.UnitX,
            up = Vector3.UnitY,
            fovy = 60,
            projection = CameraProjection.CAMERA_PERSPECTIVE
        };

        MoveSpeed = moveSpeed;
        MouseSensitivity = mouseSensitivity;
    }

    public void Update()
    {
        // Read camera stuffs
        var position = _camera.position;
        var forward = Vector3.Normalize(_camera.target - position);
        var up = _camera.up;
        var right = Vector3.Cross(forward, up);
        var rotation = new Vector3(
            MathF.Asin(-forward.Y) * 180f / MathF.PI,
            MathF.Atan2(forward.X, forward.Z) * 180f / MathF.PI,
            0f
        );

        // Rotate camera (and enable/disable mouse cursor)
        if (Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        {
            var mouseDelta = Raylib.GetMouseDelta();
            rotation.X = Math.Clamp(rotation.X + (mouseDelta.Y * MouseSensitivity), -MaxAngleY, MaxAngleY);
            rotation.Y += mouseDelta.X * -MouseSensitivity;
            Raylib.DisableCursor();
        }
        else Raylib.EnableCursor();

        // Build movement vector
        var move = Vector3.Zero;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_W)) move.Z += 1;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_S)) move.Z -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_Q)) move.Y += 1;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_E)) move.Y -= 1;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_D)) move.X += 1;
        if (Raylib.IsKeyDown(KeyboardKey.KEY_A)) move.X -= 1;

        // Apply movement vector
        var speedMultiplier = Raylib.IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT) ? 2.0f : 1.0f;
        if (move.LengthSquared() > 0) move = Vector3.Normalize(move);
        position += right * (move.X * MoveSpeed * speedMultiplier * Raylib.GetFrameTime());
        position += forward * (move.Z * MoveSpeed * speedMultiplier * Raylib.GetFrameTime());
        position += up * (move.Y * MoveSpeed * speedMultiplier * Raylib.GetFrameTime());

        // Write camera stuffs
        var pitch = rotation.X * MathF.PI / 180f;
        var yaw = rotation.Y * MathF.PI / 180f;
        forward = new Vector3(MathF.Sin(yaw) * MathF.Cos(pitch), -MathF.Sin(pitch), MathF.Cos(yaw) * MathF.Cos(pitch));

        _camera.position = position;
        _camera.target = position + forward;
    }
}