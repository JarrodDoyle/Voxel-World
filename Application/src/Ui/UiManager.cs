using System.Numerics;
using ImGuiNET;

namespace Application.Ui;

public static class UiManager
{
    public static List<Panel> Panels { get; set; } = new();

    public static void Setup()
    {
        ImGuiBackend.Setup();
        SetStyle();

        foreach (var panel in Panels)
            panel.Detach();
        Panels.Clear();
        Panels.Add(new GeneratorLayer());
        foreach (var panel in Panels)
            panel.Attach();
    }

    public static void Shutdown()
    {
        foreach (var panel in Panels)
            panel.Detach();
        ImGuiBackend.Shutdown();
    }

    public static void Update()
    {
        foreach (var panel in Panels)
            panel.Update();
    }

    public static void Render()
    {
        ImGuiBackend.Begin();
        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), ImGuiDockNodeFlags.PassthruCentralNode);
        // ImGui.ShowDemoWindow();
        foreach (var panel in Panels)
            panel.Render();
        ImGuiBackend.End();
    }

    private static void SetStyle()
    {
        // Style modified from https://github.com/ocornut/imgui/issues/707#issuecomment-917151020
        var style = ImGui.GetStyle();
        style.CellPadding = new Vector2(4.00f, 3.00f);
        style.WindowRounding = 3;
        style.ChildRounding = 3;
        style.FrameRounding = 3;
        style.PopupRounding = 3;
        style.GrabRounding = 3;

        var colors = style.Colors;
        colors[(int) ImGuiCol.Text] = new Vector4(1.00f, 1.00f, 1.00f, 1.00f);
        colors[(int) ImGuiCol.TextDisabled] = new Vector4(0.50f, 0.50f, 0.50f, 1.00f);
        colors[(int) ImGuiCol.WindowBg] = new Vector4(0.11f, 0.11f, 0.11f, 1.00f);
        colors[(int) ImGuiCol.ChildBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.00f);
        colors[(int) ImGuiCol.PopupBg] = new Vector4(0.19f, 0.19f, 0.19f, 0.92f);
        colors[(int) ImGuiCol.Border] = new Vector4(0.19f, 0.19f, 0.19f, 0.92f);
        colors[(int) ImGuiCol.BorderShadow] = new Vector4(0.00f, 0.00f, 0.00f, 0.24f);
        colors[(int) ImGuiCol.FrameBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.71f);
        colors[(int) ImGuiCol.FrameBgHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int) ImGuiCol.FrameBgActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.TitleBg] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.TitleBgActive] = new Vector4(0.10f, 0.10f, 0.10f, 1.00f);
        colors[(int) ImGuiCol.TitleBgCollapsed] = new Vector4(0.00f, 0.00f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.MenuBarBg] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int) ImGuiCol.ScrollbarBg] = new Vector4(0.05f, 0.05f, 0.05f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrab] = new Vector4(0.34f, 0.34f, 0.34f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.40f, 0.40f, 0.40f, 0.54f);
        colors[(int) ImGuiCol.ScrollbarGrabActive] = new Vector4(0.56f, 0.56f, 0.56f, 0.54f);
        colors[(int) ImGuiCol.CheckMark] = new Vector4(0.86f, 0.61f, 0.33f, 1.00f);
        colors[(int) ImGuiCol.SliderGrab] = new Vector4(0.86f, 0.61f, 0.33f, 0.54f);
        colors[(int) ImGuiCol.SliderGrabActive] = new Vector4(0.86f, 0.61f, 0.33f, 0.92f);
        colors[(int) ImGuiCol.Button] = new Vector4(0.05f, 0.05f, 0.05f, 0.71f);
        colors[(int) ImGuiCol.ButtonHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int) ImGuiCol.ButtonActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.Header] = new Vector4(0.00f, 0.00f, 0.00f, 0.71f);
        colors[(int) ImGuiCol.HeaderHovered] = new Vector4(0.19f, 0.19f, 0.19f, 0.54f);
        colors[(int) ImGuiCol.HeaderActive] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.Separator] = new Vector4(0.19f, 0.19f, 0.19f, 0.92f);
        colors[(int) ImGuiCol.SeparatorHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.92f);
        colors[(int) ImGuiCol.SeparatorActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int) ImGuiCol.ResizeGrip] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int) ImGuiCol.ResizeGripHovered] = new Vector4(0.44f, 0.44f, 0.44f, 0.29f);
        colors[(int) ImGuiCol.ResizeGripActive] = new Vector4(0.40f, 0.44f, 0.47f, 1.00f);
        colors[(int) ImGuiCol.Tab] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TabHovered] = new Vector4(0.86f, 0.61f, 0.33f, 0.54f);
        colors[(int) ImGuiCol.TabActive] = new Vector4(0.86f, 0.61f, 0.33f, 0.36f);
        colors[(int) ImGuiCol.TabUnfocused] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TabUnfocusedActive] = new Vector4(0.14f, 0.14f, 0.14f, 1.00f);
        colors[(int) ImGuiCol.DockingPreview] = new Vector4(0.86f, 0.61f, 0.33f, 0.54f);
        colors[(int) ImGuiCol.DockingEmptyBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.PlotLines] = new Vector4(0.61f, 0.61f, 0.61f, 1.00f);
        colors[(int) ImGuiCol.PlotLinesHovered] = new Vector4(1.00f, 0.43f, 0.35f, 1.00f);
        colors[(int) ImGuiCol.PlotHistogram] = new Vector4(0.90f, 0.70f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.PlotHistogramHovered] = new Vector4(1.00f, 0.60f, 0.00f, 1.00f);
        colors[(int) ImGuiCol.TableHeaderBg] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TableBorderStrong] = new Vector4(0.00f, 0.00f, 0.00f, 0.52f);
        colors[(int) ImGuiCol.TableBorderLight] = new Vector4(0.28f, 0.28f, 0.28f, 0.29f);
        colors[(int) ImGuiCol.TableRowBg] = new Vector4(1.00f, 1.00f, 1.00f, 0.06f);
        colors[(int) ImGuiCol.TableRowBgAlt] = new Vector4(1.00f, 1.00f, 1.00f, 0.12f);
        colors[(int) ImGuiCol.TextSelectedBg] = new Vector4(0.20f, 0.22f, 0.23f, 1.00f);
        colors[(int) ImGuiCol.DragDropTarget] = new Vector4(0.86f, 0.61f, 0.33f, 1.00f);
        colors[(int) ImGuiCol.NavHighlight] = new Vector4(0.86f, 0.61f, 0.33f, 1.00f);
        colors[(int) ImGuiCol.NavWindowingHighlight] = new Vector4(0.86f, 0.61f, 0.33f, 1.00f);
        colors[(int) ImGuiCol.NavWindowingDimBg] = new Vector4(0.51f, 0.55f, 0.60f, 0.20f);
        colors[(int) ImGuiCol.ModalWindowDimBg] = new Vector4(0.51f, 0.55f, 0.60f, 0.35f);

        var io = ImGui.GetIO();
        io.Fonts.Clear();
        io.Fonts.AddFontFromFileTTF("Resources/JetBrainsMono-Medium.ttf", 16);
        ImGuiBackend.ReloadFonts();
    }
}