using InfrastSim.Script;
using InfrastSim.TimeDriven.WebHelper;

namespace InfrastSimTest;
[TestClass]
public class ScriptTest {
    [TestMethod]
    public void TestScript() {
        var script = """
            //setStartTime 20240104 12:00:00	; 设置启动时间 20240104 12:00:00
            //setCurrentTime 20240104 16:00:00 ; 设置当前时间 20240104 16:00:00
            //setRelevantTime 1:4:0:0 ; 设置相对时间 1:4:0:0

            // 注释可以使用 # 和 // 
            // 语句默认独占一行, ';' 表示换行
            // 命令同时支持中英版本, 不考虑大小写

            with B101 ; 切换设施 B101
            with Office ; 切换设施 办公室 // 切换命令持续到下个切换命令为止, 此前所有命令针对于该设施

              setOps 芬 克洛丝 ; 进驻干员 芬 克洛丝
              useDrones 50 ; 使用无人机 50
              setProduct 源石碎片_固源岩 ; 设置产物 源石碎片_固源岩
              setStrategy 龙门商法 ; 设置策略 龙门商法
              collect ; 收集产出
              collect 3 ; 收集3号订单

            collectAll ; 收集全部产出
            simulate 1:30:00 ; 模拟 1:30:00
        """;
        var simu = new InfrastSim.TimeDriven.Simulator();
        Assert.ThrowsException<ScriptException>(() => Helper.ExecuteScript(simu, script));
    }
}
