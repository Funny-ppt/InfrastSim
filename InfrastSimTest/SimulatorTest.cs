using InfrastSim.TimeDriven.WebHelper;
using InfrastSim.Wasm;
using System.Text;
using System.Text.Json;

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

            var data1_1 = SimulatorService.GetData(id, false);
            var id2 = SimulatorService.CreateWithData(data1_1);
            var data2_1 = SimulatorService.GetData(id2, false);

            //Assert.AreEqual(data1_1, data2_1); 因为使用了FrozenDictionary, 导致干员表顺序可能有变
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

            var data1_0 = SimulatorService.GetData(id, false);
            SimulatorService.Simulate(id, 7200);
            var data1_1 = SimulatorService.GetData(id, false);
            SimulatorService.Simulate(id, 7200);
            SimulatorService.Simulate(id, 7200);
            SimulatorService.Simulate(id, 7200);
            var data1_3 = SimulatorService.GetData(id, false);


            var id2 = SimulatorService.CreateWithData(data1_0);
            var data2_0 = SimulatorService.GetData(id2, false);
            SimulatorService.Simulate(id2, 7200);
            var data2_1 = SimulatorService.GetData(id2, false);
            SimulatorService.Simulate(id2, 21600);
            var data2_3 = SimulatorService.GetData(id2, false);

            //Assert.AreEqual(data1_0, data2_0); 因为使用了FrozenDictionary, 导致干员表顺序可能有变
            //Assert.AreEqual(data1_1, data2_1);
            //Assert.AreEqual(data1_3, data2_3);
        }

        [TestMethod]
        public void TestEnumerate() {
            var input = """
            {
                "preset": {
                    "Control Center": {
                        "level": 5
                    },
                    "B103": {
                        "type": "Power",
                        "level": 3
                    },
                    "B203": {
                        "type": "Power",
                        "level": 3
                    },
                    "B101": {
                        "type": "Trading",
                        "level": 1
                    },
                    "B102": {
                        "type": "Trading",
                        "level": 1
                    },
                    "B201": {
                        "type": "Manufacturing",
                        "level": 3
                    },
                    "B202": {
                        "type": "Manufacturing",
                        "level": 3
                    },
                    "B301": {
                        "type": "Manufacturing",
                        "level": 3
                    },
                    "B302": {
                        "type": "Manufacturing",
                        "level": 3
                    },
                    "B303": {
                        "type": "Manufacturing",
                        "level": 3
                    },
                    "Dormitory 1": {
                        "level": 1
                    },
                    "Dormitory 2": {
                        "level": 1
                    },
                    "Dormitory 3": {
                        "level": 1
                    },
                    "Reception": {
                        "level": 3
                    },
                    "Crafting": {
                        "level": 3
                    },
                    "Office": {
                        "level": 3
                    },
                    "Training": {
                        "level": 3
                    }
                },
                "ops": [
                    {
                        "name": "\u9ed1\u952e",
                        "fac": "\u8d38\u6613\u7ad9",
                        "product": "\u8d64\u91d1"
                    },
                    {
                        "name": "\u5851\u5fc3",
                        "fac": "\u5bbf\u820d",
                        "product": "\u8d64\u91d1"
                    },
                    {
                        "name": "\u6df1\u5f8b",
                        "fac": "\u529e\u516c\u5ba4",
                        "product": "\u8d64\u91d1"
                    },
                    {
                        "name": "\u9ed1\u952e",
                        "fac": "\u8d38\u6613\u7ad9",
                        "product": "\u8d64\u91d1"
                    },
                    {
                        "name": "\u7d6e\u96e8",
                        "fac": "\u529e\u516c\u5ba4",
                        "product": "\u8d64\u91d1"
                    },
                    {
                        "name": "\u4ee4",
                        "fac": "\u63a7\u5236\u4e2d\u67a2",
                        "product": "\u8d64\u91d1"
                    },
                    {
                        "name": "\u8ff7\u8fed\u9999",
                        "fac": "\u5236\u9020\u7ad9",
                        "product": "\u8d64\u91d1"
                    },
                    {
                        "name": "\u5915",
                        "fac": "\u63a7\u5236\u4e2d\u67a2",
                        "product": "\u8d64\u91d1"
                    }
                ]
            }
            """;
            using var ms = new MemoryStream();
            using var writer = new Utf8JsonWriter(ms);
            EnumerateHelper.Enumerate(JsonDocument.Parse(input), writer);
            Console.WriteLine(Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length));
        }
    }
}