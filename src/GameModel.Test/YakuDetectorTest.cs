using GameModel;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GameModel.Test {
    public class YakuDetectorTest {
        private YakuDetector yakuDetector = new YakuDetector();

        [Fact]
        public void TestTanyao()
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
            var test = yakuDetector.GetYakuEligibility(handList1);
            var tanyaoEligibility1 = test.FirstOrDefault(el => el.Definition.Name == YakuName.Tanyao);
            Assert.NotNull(tanyaoEligibility1);
            Assert.Equal(3, tanyaoEligibility1.DistanceInTiles);
        }
    }    
}