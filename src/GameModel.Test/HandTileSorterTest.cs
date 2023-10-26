using GameModel;
using System.Collections.Generic;
using Xunit;

namespace GameModel.Test {
    public class HandTileSorterTest {
        [Fact]
        public void TestMethod()
        {
            var handTileSorter = new HandTileSorter();
            var result = handTileSorter.Convert(new ());
            Assert.Equal(4, result.Length);
        }
    }
}