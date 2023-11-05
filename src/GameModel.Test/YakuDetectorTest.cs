using GameModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace GameModel.Test {
    public class YakuDetectorTest {
        private YakuDetector yakuDetector = new YakuDetector();
        private ITestOutputHelper output;

        public static IEnumerable<object[]> TanyaoData =>
            new List<object[]>
            {
                new object[] {
                    "No Honor Tiles (3)",
                    new List<string>() { 
                        "1z", 
                        "1z", 
                        "1z", 

                        "2m", 
                        "2m", 
                        "2m", 

                        "3p", 
                        "3p", 
                        "3p", 

                        "4s", 
                        "4s", 
                        "4s", 

                        "8p", 
                        "8p", 
                    },
                    3
                },

                new object[] {
                    "No Terminal Tiles (1)",
                    new List<string>() { 
                        "2m", 
                        "2m", 
                        "2m", 
                        "9m",

                        "3p", 
                        "3p", 
                        "3p",
                        "4p" ,

                        "4s", 
                        "4s", 
                        "4s", 
                        "5s",

                        "8p", 
                        "8p", 
                    },
                    1
                },

                new object[] {
                    "No Terminal/Honor Tiles (5)",
                    new List<string>() { 
                        "1z", 
                        "1z", 
                        "1z", 
                        "9m",

                        "3p", 
                        "3p", 
                        "3p",
                        "4p" ,

                        "4s", 
                        "4s", 
                        "4s", 
                        "9s",

                        "8p", 
                        "8p", 
                    },
                    5
                },

                new object[] {
                    "Happy Path",
                    new List<string>() { 
                    "2m", 
                    "2m", 
                    "2m", 
                    "5m",

                    "3p", 
                    "3p", 
                    "3p",
                    "4p" ,

                    "4s", 
                    "4s", 
                    "4s", 
                    "5s",

                    "8p", 
                    "8p", 
                    },
                    0
                },
            };


        [Theory()]
        [MemberData(nameof(TanyaoData))]
        public void TestTanyao(string label, List<string> tiles, int distance) {
            var test1 = yakuDetector.GetYakuEligibility(tiles);
            var eligibility = test1.FirstOrDefault(el => el.Definition.Name == YakuName.Tanyao);
            Assert.NotNull(eligibility);
            Assert.Equal(distance, eligibility.DistanceInTiles);   
        }  
    }
}