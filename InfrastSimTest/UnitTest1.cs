using System.Diagnostics;

namespace InfrastSimTest {
    [TestClass]
    public class UnitTest1 {
        [TestMethod]
        public void TestMethod1() {
            var simu = new InfrastSim.TimeDriven.Simulator();
            var json = simu.ToJson();
            Debug.Assert(json != null);
        }
    }
}