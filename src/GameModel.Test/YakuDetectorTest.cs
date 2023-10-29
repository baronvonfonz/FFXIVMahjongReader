using GameModel;
using System.Collections.Generic;
using Xunit;

namespace GameModel.Test {
    public class YakuDetectorTest {
        [Fact]
        public void TestMethod()
        {
            var yakuDetector = new YakuDetector();
            var test = yakuDetector.GetYakuEligibility(new());
        }
    }    
}