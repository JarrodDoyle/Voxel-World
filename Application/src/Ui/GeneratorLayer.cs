using System.Numerics;
using VoxelWorld;
using ImGuiNET;

namespace Application.Ui;

public class GeneratorLayer : UiLayer
{
    private Vector3 _centerPos = Vector3.Zero;
    
    public override void Attach()
    {
        Console.WriteLine("Attached layer");
    }

    public override void Detach()
    {
        Console.WriteLine("Detached layer");
    }

    public override void Render()
    {
        var isOpen = Open;
        if (!isOpen) return;

        // ImGui.Begin("Generation Controller", ref isOpen);
        //
        // ImGui.Value("Loaded Chunks", ChunkManager.LoadedChunksCount());
        //
        // // See and edit the seed
        // var seed = ChunkManager.Seed;
        // if (ImGui.DragInt("Seed", ref seed)) ChunkManager.Seed = seed;
        //
        // // Move world
        // if (ImGui.InputFloat3("World center", ref _centerPos))
        //     ChunkManager.LoadChunksAroundPos(_centerPos, 5);
        //
        // // Generation
        // if (ImGui.Button("Regenerate Meshes")) ChunkManager.RegenerateMeshes();
        // ImGui.SameLine();
        // if (ImGui.Button("Generate Chunks"))
        // {
        //     ChunkManager.Seed = (int) DateTime.Now.ToBinary();
        //     ChunkManager.RegenerateChunks();
        // }
        //
        // ImGui.End();

        Open = isOpen;
    }

    public override void Update()
    {
    }
}