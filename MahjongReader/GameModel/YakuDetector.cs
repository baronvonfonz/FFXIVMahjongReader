using System.Collections.Generic;

namespace GameModel
{
    public enum YakuName {
        Tanyao,
    }

    public class YakuDefinition {
        public bool AllowsCalls { get; set; }
        public YakuName Name { get; set; } 
        public int Han { get; set; }

        public YakuDefinition(bool allowsCalls, YakuName name, int han) {
            AllowsCalls = allowsCalls;
            Name = name;
            Han = han;
        }
    }

    public class YakuEligibility {
        public YakuDefinition Definition { get; set; }
        public int DistanceInTiles { get; set; }

        public YakuEligibility(YakuDefinition definition, int distanceInTiles) {
            Definition = definition;
            DistanceInTiles = distanceInTiles;
        }
    }

    public class YakuDetector {
        public YakuDetector() {}

        public List<YakuEligibility> GetYakuEligibility(List<string> mjaiNotations) {
            return new();
        }
    }
}