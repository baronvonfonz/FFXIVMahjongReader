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

        public YakuDetectorTest(ITestOutputHelper xOutput) {
            var converter = new Converter(xOutput);
            Console.SetOut(converter);
            output = xOutput;
        }

        [Fact]
        public void TestTanyaoNoHonor()
        {
            var handList1 = new List<string>() { 
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
            };
            var test1 = yakuDetector.GetYakuEligibility(handList1);
            var tanyaoEligibility1 = test1.FirstOrDefault(el => el.Definition.Name == YakuName.Tanyao);
            Assert.NotNull(tanyaoEligibility1);
            Assert.Equal(3, tanyaoEligibility1.DistanceInTiles);
        }

        [Fact]
        public void TestTanyaoNoTerminal()
        {
            var handList2 = new List<string>() { 
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
            };
            var test2 = yakuDetector.GetYakuEligibility(handList2);
            var tanyaoEligibility2 = test2.FirstOrDefault(el => el.Definition.Name == YakuName.Tanyao);
            Assert.NotNull(tanyaoEligibility2);
            Assert.Equal(1, tanyaoEligibility2.DistanceInTiles);
        }

        [Fact]
        public void TestTanyaoMix()
        {
            var handList2 = new List<string>() { 
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
            };
            var test2 = yakuDetector.GetYakuEligibility(handList2);
            var tanyaoEligibility2 = test2.FirstOrDefault(el => el.Definition.Name == YakuName.Tanyao);
            Assert.NotNull(tanyaoEligibility2);
            Assert.Equal(5, tanyaoEligibility2.DistanceInTiles);
        }

        [Fact]
        public void TestTanyaoHappy()
        {
            var handList2 = new List<string>() { 
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
            };
            var test2 = yakuDetector.GetYakuEligibility(handList2);
            var tanyaoEligibility2 = test2.FirstOrDefault(el => el.Definition.Name == YakuName.Tanyao);
            Assert.NotNull(tanyaoEligibility2);
            Assert.Equal(0, tanyaoEligibility2.DistanceInTiles);
        }
    }    
}