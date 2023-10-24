using InfrastSim.Wasm;

namespace InfrastSimTest {
    [TestClass]
    public class SimulatorTest {
        public void ValidateDataImpl(string data) {

        }

        public void ValidateMowerDataImpl(string data) {

        }

        public void ValidateData(int id) {
            ValidateDataImpl(SimulatorService.GetData(id));
            ValidateMowerDataImpl(SimulatorService.GetDataForMower(id));
        }

        [TestMethod]
        public void TestCreateAndSetup() {
            var id = SimulatorService.Create();
            SimulatorService.SetFacilityState(id, "B101", """
            {   "type": "Trading",
                "level": 3,
                "operators": ["巫恋", "龙舌兰", "乌有"] }
            """);
            SimulatorService.SetFacilityState(id, "B102", """
            {   "type": "Manufacturing",
                "level": 2,
                "operators": [{"name": "多萝西"}, {"name": "赫默"}] }
            """);
            SimulatorService.SetFacilityState(id, "B103", """
            {   "type": "Power",
                "level": 3,
                "operators": [{"name": "承曦格雷伊"}] }
            """);

            ValidateData(id);


            SimulatorService.SetFacilityState(id, "B101", """
                { "strategy": "gold" }
            """);
            SimulatorService.SetFacilityState(id, "B102", """
                { "product": "赤金" }
            """);
            SimulatorService.Simulate(id, 7200);

            ValidateData(id);

            var data1_1 = SimulatorService.GetData(id);
            var id2 = SimulatorService.CreateWithData(data1_1);
            var data2_1 = SimulatorService.GetData(id2);

            Assert.AreEqual(data1_1, data2_1);
        }

        [TestMethod]
        public void TestCoinsistency() {
            var id = SimulatorService.Create();
            SimulatorService.SetFacilityState(id, "B101", """
            {   "type": "Trading",
                "level": 3,
                "operators": ["巫恋", "龙舌兰", "乌有"] }
            """);
            SimulatorService.SetFacilityState(id, "B102", """
            {   "type": "Manufacturing",
                "level": 2,
                "operators": [{"name": "多萝西"}, {"name": "赫默"}] }
            """);
            SimulatorService.SetFacilityState(id, "B103", """
            {   "type": "Power",
                "level": 3,
                "operators": [{"name": "承曦格雷伊"}] }
            """);

            SimulatorService.SetFacilityState(id, "B101", """
                { "strategy": "gold" }
            """);
            SimulatorService.SetFacilityState(id, "B102", """
                { "product": "赤金" }
            """);

            var data1_0 = SimulatorService.GetData(id);
            SimulatorService.Simulate(id, 7200);
            var data1_1 = SimulatorService.GetData(id);
            SimulatorService.Simulate(id, 7200);
            SimulatorService.Simulate(id, 7200);
            SimulatorService.Simulate(id, 7200);
            var data1_3 = SimulatorService.GetData(id);


            var id2 = SimulatorService.CreateWithData(data1_0);
            var data2_0 = SimulatorService.GetData(id2);
            SimulatorService.Simulate(id2, 7200);
            var data2_1 = SimulatorService.GetData(id2);
            SimulatorService.Simulate(id2, 21600);
            var data2_3 = SimulatorService.GetData(id2);

            Assert.AreEqual(data1_0, data2_0);
            Assert.AreEqual(data1_1, data2_1);
            Assert.AreEqual(data1_3, data2_3);
        }
    }
}