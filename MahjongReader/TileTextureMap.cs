using System;
using System.Collections.Generic;
using Dalamud.Logging;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;

namespace MahjongReader
{
    public class DiscardTile {
        public TileTexture TileTexture { get; }
        public bool IsMelded { get; }
        public bool IsImmediatelyDiscarded { get; }

        public DiscardTile(TileTexture tileTexture, bool isMelded, bool isImmediatelyDiscarded) {
            TileTexture = tileTexture;
            IsMelded = isMelded;
            IsImmediatelyDiscarded = isImmediatelyDiscarded;
        }

        public override string ToString() {
            return $"{TileTexture} IsMelded: {IsMelded} IsImmediatelyDiscarded: {IsImmediatelyDiscarded}";
        }
    }

    public class TileTexture {
        private string textureId;
        private string mjaiNotation;

        public TileTexture(string iTextureId, string iMjaiNotation) {
            textureId = iTextureId;
            mjaiNotation = iMjaiNotation;
        }

        public string TextureId
        {
            get { return textureId; }
        }

        public string MjaiNotation
        {
            get { return mjaiNotation; }
        }

        public override string ToString() {
            return $"textureId: {textureId} mjaiNotation: {mjaiNotation}";
        }
    }

    public class TileTextureMap
    {
        private static Lazy<Dictionary<string, TileTexture>> tileTextureMapLazy = new Lazy<Dictionary<string, TileTexture>>(() => 
        {
            Dictionary<string, TileTexture> map = new Dictionary<string, TileTexture>();

            // characters/man
            for (int i = 1; i < 10; i++) {
                int nextTextureId = i + 76040; //41 is the actual character/man start
                string fullTextureId = "0" + nextTextureId.ToString();
                map.Add(fullTextureId, new TileTexture(fullTextureId, i.ToString() + "m"));
            }

            // dots/pin
            for (int i = 1; i < 10; i++) {
                int nextTextureId = i + 076049; //50 is the actual dots/pin start
                string fullTextureId = "0" + nextTextureId.ToString();
                map.Add(fullTextureId, new TileTexture(fullTextureId, i.ToString() + "p"));
            }

            // bamboo/sou
            for (int i = 1; i < 10; i++) {
                int nextTextureId = i + 076058; //59 is the actual bamboo/sou start
                string fullTextureId = "0" + nextTextureId.ToString();
                map.Add(fullTextureId, new TileTexture(fullTextureId, i.ToString() + "s"));
            }

            // honor tiles / wind first / dragons
            for (int i = 1; i < 8; i++) {
                int nextTextureId = i + 076067; //68 is the actual honor tile start
                string fullTextureId = "0" + nextTextureId.ToString();
                map.Add(fullTextureId, new TileTexture(fullTextureId, i.ToString() + "z"));
            }

            map.Add("076075", new TileTexture("076075", "0m"));
            map.Add("076076", new TileTexture("076076", "0p"));
            map.Add("076077", new TileTexture("076077", "0s"));

            return map;
        });

        public Dictionary<string, TileTexture> Map => tileTextureMapLazy.Value;

        private TileTextureMap() { }

        public static TileTextureMap Instance { get; } = new TileTextureMap();

        // TODO: support traditonal (IRL Mahjong tile face) textures? Same pattern, different texture id offset
        private const string TEXTURE_PATH_TILE_ICON_PREFIX = "ui/icon/076000/";

        public bool IsValidTileTexturePath(string? texturePath) {
            return texturePath?.StartsWith(TEXTURE_PATH_TILE_ICON_PREFIX) ?? false;
        }

        public TileTexture GetTileTextureFromTexturePath(string texturePath) {
            var textureTex = texturePath.Substring(15, 6);
            if (!Map.ContainsKey(textureTex)) {
                throw new ApplicationException("Unknown key: " + texturePath);
            }

            return Map[textureTex];
        }

        public static void PrintTest(IPluginLog pluginLog) {
            TileTextureMap theMap = TileTextureMap.Instance;
            Dictionary<string, TileTexture> mapInst = theMap.Map;

            foreach (var kvp in mapInst) {
                TileTexture value = kvp.Value;
                pluginLog.Info(value.ToString());
            }
        }
    }
}