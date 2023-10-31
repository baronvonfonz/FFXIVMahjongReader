using GameModel;
using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface.Colors;
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

    private Dictionary<string, int> internalSuitCounts;

    public Dictionary<string, int> SuitCounts
    {
        get
        {
            return internalSuitCounts;
        }
        set
        {
            internalSuitCounts = value;
        }
    }

    private Dictionary<string, IDalamudTextureWrap> mjaiNotationToTexture;
    private Dictionary<string, IDalamudTextureWrap> suitToTexture;

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
        internalSuitCounts = new Dictionary<string, int>();

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

        suitToTexture = new();
        // maybe one day we'll support traditional properly
        suitToTexture.Add(Suit.MAN, Plugin.TextureProvider.GetIcon(uint.Parse("076001"))!);
        suitToTexture.Add(Suit.PIN, Plugin.TextureProvider.GetIcon(uint.Parse("076010"))!);
        suitToTexture.Add(Suit.SOU, Plugin.TextureProvider.GetIcon(uint.Parse("076019"))!);
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

    private void DrawSuitRemaining(string suit) {
        var count = internalSuitCounts[suit];
        var texture = suitToTexture[suit];
        var scale = new Vector2(texture.Width, texture.Height);
        var textSpacing = new Vector2(0, 0);
        ImGui.TableNextColumn();
        ImGui.Image(texture.ImGuiHandle, scale);
        ImGui.SameLine();
        ImGui.Dummy(textSpacing);
        ImGui.SameLine();
        ImGui.Text("x " + count);
    }
    public override void Draw()
    {
        ImGui.BeginTable("#Tiles", 4, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingFixedFit);
        for (var i = 1; i < 10; i++) {
            ImGui.TableNextRow();
            bool isDora = i == 5;

            DrawTileRemaining(Suit.MAN, i, isDora);
            DrawTileRemaining(Suit.PIN, i, isDora);
            DrawTileRemaining(Suit.SOU, i, isDora);

            if (i == 3) {
                DrawSuitRemaining(Suit.MAN);
            } else if (i == 5) {
                DrawSuitRemaining(Suit.PIN);
            } else if (i == 7) {
                DrawSuitRemaining(Suit.SOU);
            } else {
                ImGui.TableNextColumn();
            }
        }
        ImGui.EndTable();

        ImGui.Dummy(new Vector2(0, 40));

        ImGui.BeginTable("#TilesWind", 4, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingFixedFit);
        ImGui.TableNextRow();
        for (var i = 1; i < 5; i++) {
            DrawTileRemaining(Suit.HONOR, i, false);
        }
        ImGui.EndTable();

        ImGui.BeginTable("#TilesDragon", 3, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingFixedFit);
        ImGui.TableNextRow();
        ImGui.TableNextRow();
        for (var i = 5; i < 8; i++) {
            DrawTileRemaining(Suit.HONOR, i, false);
        }
        ImGui.EndTable();
    }
}
