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
        private HandTileSorter handTileSorter; // someday really gotta pick a single C# field/object pattern

        public YakuDetector() {
            handTileSorter = new HandTileSorter();
        }

        public List<YakuEligibility> GetYakuEligibility(List<string> mjaiNotations) {
            var suitToNumbers = handTileSorter.SuitToNumbers(mjaiNotations);

            // var maybeTanyao = CheckTanyao(suitToNumbers);

            return new();
        }

        // private YakuEligibility CheckTanyao(Dictionary<string, List<int>> suitToNumbers) {
        //     return new(YakuDefinition.T);
        // }
    }
}