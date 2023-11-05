using System;
using System.Collections.Generic;
using System.Linq;

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

            var maybeTanyao = CheckTanyao(suitToNumbers);

            return new() {
                maybeTanyao,
            };
        }

        // TODO: kuitan is assumed enabled
        public static YakuDefinition TANYAO_DEFINITION = new(true, YakuName.Tanyao, 1);
        private YakuEligibility CheckTanyao(Dictionary<string, List<int>> suitToNumbers) {
            var distance = 0;

            if (suitToNumbers.ContainsKey(Suit.HONOR)) {
                distance += suitToNumbers[Suit.HONOR].Count;
            }

            foreach (var suit in Suit.NOT_HONORS) {
                if (!suitToNumbers.ContainsKey(suit)) {
                    continue;
                }
                distance += suitToNumbers[suit].Count(number => number == 1  || number == 9);
            }

            return new(TANYAO_DEFINITION, distance);
        }
    }
}