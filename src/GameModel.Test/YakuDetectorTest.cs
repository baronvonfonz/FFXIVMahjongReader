using GameModel;
using NUnit.Framework;

namespace GameModel.Test {
    public class YakuDetectorTest {
        [Test]
        public void MyTestMethod()
        {
            var yakuDetector = new YakuDetector();
            var test = yakuDetector.GetYakuEligibility(new());
            Assert.AreEqual(4, test.Count);
        }
    }    
}