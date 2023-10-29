using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace GameModel {
    
    public class HandTileSorter {
        private readonly List<string> suitOrder = new List<string> { "m", "p", "s", "z" };

        public HandTileSorter() {}

        public string Convert(List<string> mjaiNotations) {
            if (mjaiNotations.Count < 1) {
                throw new ApplicationException("At least one tile is required to converted to a string.");
            }
            var suitToNumbers = new Dictionary<string, List<int>>();

            mjaiNotations.ForEach(notation => {
                var number = notation.Substring(0, 1);
                var suit = notation.Substring(1, 1);

                if (suitToNumbers.ContainsKey(suit)) {
                    var list = suitToNumbers[suit];
                    list.Add(Int32.Parse(number));
                } else {
                    suitToNumbers[suit] = new List<int>
                    {
                        Int32.Parse(number)
                    };
                 }
            });

            var returnString = "";

            foreach (var suit in suitOrder) {
                if (!suitToNumbers.ContainsKey(suit)) {
                    continue;
                }
                var theNumbers = suitToNumbers[suit];
                returnString += $"{string.Join("", theNumbers)}{suit}";
            }

            return returnString;
        }
    }
}