using GameModel;
using NUnit.Framework;

namespace GameModel.Test {
    public class HandTileSorterTest {
        [Test]
        public void MyTestMethod()
        {
            var handTileSorter = new HandTileSorter();
            var result = handTileSorter.Convert(new());
            Assert.AreEqual(4, result.Length);
        }
    }
}