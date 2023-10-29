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
        public void TestMethod()
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
    }
}