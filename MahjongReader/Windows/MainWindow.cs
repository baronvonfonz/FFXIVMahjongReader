using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility;
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

    private Dictionary<string, IDalamudTextureWrap> mjaiNotationToTexture;

    public MainWindow(Plugin plugin, IPluginLog pluginLog) : base(
        "Mahjong Reader", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(275, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.Plugin = plugin;
        this.PluginLog = pluginLog;
        internalObservedTiles = new List<ObservedTile>();
        internalRemainingMap = new Dictionary<string, int>();

        mjaiNotationToTexture = new();
        // setup textures
        foreach (var notationToTextureId in TileTextureUtilities.NotationToTextureId) {
            var maybeTex = Plugin.TextureProvider.GetIcon(uint.Parse(notationToTextureId.Value));
            if (maybeTex == null) {
                PluginLog.Error($"Bad texture id for notation ${notationToTextureId.Key}");
                continue;
            }
            mjaiNotationToTexture.Add(notationToTextureId.Key, maybeTex);
        }
    }

    public void Dispose() { }


    private void DrawTileRemaining(string suit, int number, bool isDora) {
        var notation = $"{number}{suit}";
        var count = isDora ? internalRemainingMap[notation] + internalRemainingMap[$"0{suit}"] : internalRemainingMap[notation];
        var isDoraRemaing = isDora ? internalRemainingMap[$"0{suit}"] > 0 : false;
        var texture = mjaiNotationToTexture[notation];
        var scale = new Vector2(texture.Width, texture.Height);
        var textSpacing = new Vector2(0, 0);
        ImGui.TableNextColumn();
        ImGui.Image(texture.ImGuiHandle, scale);
        ImGui.SameLine();
        ImGui.Dummy(textSpacing);
        ImGui.SameLine();
        if (isDoraRemaing) {
            ImGui.TextColored(ImGuiColors.DalamudOrange, "x " + count);
        } else {
            ImGui.Text("x " + count);
        }
    }
    public override void Draw()
    {
        ImGui.BeginTable("#Tiles", 3, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingFixedFit);
        for (var i = 1; i < 10; i++) {
            ImGui.TableNextRow();
            bool isDora = i == 5;

            DrawTileRemaining("m", i, isDora);
            DrawTileRemaining("p", i, isDora);
            DrawTileRemaining("s", i, isDora);

        }
        ImGui.EndTable();

        ImGui.Dummy(new Vector2(0, 40));

        ImGui.BeginTable("#TilesWind", 4, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingFixedFit);
        ImGui.TableNextRow();
        for (var i = 1; i < 5; i++) {
            DrawTileRemaining("z", i, false);
        }
        ImGui.EndTable();

        ImGui.BeginTable("#TilesDragon", 3, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingFixedFit);
        ImGui.TableNextRow();
        ImGui.TableNextRow();
        for (var i = 5; i < 8; i++) {
            DrawTileRemaining("z", i, false);
        }
        ImGui.EndTable();
    }
}
