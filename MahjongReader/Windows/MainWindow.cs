using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace MahjongReader.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin Plugin;
    private IPluginLog PluginLog;

    private List<ObservedTile> internalObservedTiles;

    public List<ObservedTile> ObservedTiles
    {
        get
        {
            return internalObservedTiles;
        }
        set
        {
            internalObservedTiles = value;
        }
    }

    private Dictionary<string, int> internalRemainingMap;

    public Dictionary<string, int> RemainingMap
    {
        get
        {
            return internalRemainingMap;
        }
        set
        {
            internalRemainingMap = value;
        }
    }

    public MainWindow(Plugin plugin, IPluginLog pluginLog) : base(
        "My Amazing Window", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.Plugin = plugin;
        this.PluginLog = pluginLog;
        internalObservedTiles = new List<ObservedTile>();
        internalRemainingMap = new Dictionary<string, int>();
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.Spacing();
        // var observedTiles = Plugin.GetObservedTiles();
        // var remainingMap = TileTextureUtilities.TileCountTracker.RemainingFromObserved(observedTiles);
        // foreach (var kvp in remainingMap) {
        //     PluginLog.Info($"{kvp.Key} - {kvp.Value}");
        // }
        ImGui.Indent(55);
        foreach (var kvp in internalRemainingMap) {
            ImGui.Text($"{kvp.Key} - {kvp.Value}");
        }
    }
}
