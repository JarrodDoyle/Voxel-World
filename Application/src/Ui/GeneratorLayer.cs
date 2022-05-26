using Generator;
using ImGuiNET;

namespace Application.Ui;

public class GeneratorLayer : UiLayer
{
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

        ImGui.Begin("Generation Controller", ref isOpen);

        ImGui.Value("Loaded Chunks", ChunkManager.LoadedChunksCount());

        // See and edit the seed
        var seed = ChunkManager.Seed;
        if (ImGui.DragInt("Seed", ref seed)) ChunkManager.Seed = seed;

        // Generation
        if (ImGui.Button("Regenerate")) ChunkManager.RegenerateChunks();
        ImGui.SameLine();
        if (ImGui.Button("Regenerate Random"))
        {
            ChunkManager.Seed = (int) DateTime.Now.ToBinary();
            ChunkManager.RegenerateChunks();
        }

        ImGui.End();

        Open = isOpen;
    }

    public override void Update()
    {
    }
}