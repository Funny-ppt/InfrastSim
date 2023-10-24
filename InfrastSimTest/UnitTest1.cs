using InfrastSim.Wasm;

namespace InfrastSimTest {
    [TestClass]
    public class UnitTest1 {
        public void ValidateDataImpl(string data) {

        }

        public void ValidateMowerDataImpl(string data) {

        }

        public void ValidateData(int id) {
            ValidateDataImpl(SimulatorService.GetData(id));
            ValidateMowerDataImpl(SimulatorService.GetDataForMower(id));
        }

        [TestMethod]
        public void TestSimulatorCreateAndSetup() {
            var id = SimulatorService.Create();
            SimulatorService.SetFacilityState(id, "B101", """
            {   "type": "Trading",
                "level": 3,
                "operators": [{"name": "巫恋"}, {"name": "龙舌兰"}, {"name": "乌有"}] }
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

            var data = SimulatorService.GetData(id);
            var id2 = SimulatorService.CreateWithData(data);
            var data2 = SimulatorService.GetData(id2);

            Assert.AreEqual(data, data2);
        }
    }
}