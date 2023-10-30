using GameModel;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace GameModel.Test {
    public class HandTileSorterTest {
        private HandTileSorter handTileSorter = new HandTileSorter();

        public HandTileSorterTest(ITestOutputHelper output) {
            var converter = new Converter(output);
            Console.SetOut(converter);
        }

        [Fact]
        public void TestHappyPath()
        {
            var handList = new List<string>() { 
                "1z", 
                "1z", 
                "1z", 

                "2z", 
                "2z", 
                "2z", 

                "3z", 
                "3z", 
                "3z", 

                "4z", 
                "4z", 
                "4z", 

                "8p", 
                "8p", 
            };
            var result = handTileSorter.Convert(handList);
            Assert.Equal("88p111222333444z", result);
        }

        [Fact]
        public void TestFourSuits()
        {
            var handList = new List<string>() { 
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
            var result = handTileSorter.Convert(handList);
            Assert.Equal("222m33388p444s111z", result);
        }

        [Fact]
        public void TestOneSuit()
        {
            var handList = new List<string>() { 
                "1z", 
                "1z", 
                "1z", 

                "2z", 
                "2z", 
                "2z", 

                "3z", 
                "3z", 
                "3z", 

                "4z", 
                "4z", 
                "4z", 

                "5z", 
                "5z", 
            };
            var result = handTileSorter.Convert(handList);
            Assert.Equal("11122233344455z", result);
        }
    }
}