using InfrastSim.Localization;
using InfrastSim.Script;
using InfrastSim.TimeDriven.WebHelper;

namespace InfrastSim.TimeDriven;
internal static partial class ScriptHelper {
    [Alias(Language.CN, "切换设施")]
    public static void With(Simulator simu, string[] args) {
        if (args.Length != 1) {
            throw new ScriptException("切换设施需要一个参数 [设施名]");
        }
        simu.SelectedFacilityString = args[0];
        simu.SelectedFacilityCache = simu.GetFacilityByName(args[0]);
    }

    [Alias(Language.CN, "进驻干员")]
    public static void SetOps(Simulator simu, string[] args) {
        var fac = (simu.SelectedFacilityCache ??= simu.GetFacilityByName(simu.SelectedFacilityString))
            ?? throw new ScriptException($"{simu.SelectedFacilityString} 设施不存在");
        var ops = args.Select(simu.GetOperator);
        fac.AssignMany(ops);
    }

    [Alias(Language.CN, "配置干员")]
    public static void SetOpState(Simulator simu, string[] args) {
        if (args.Length != 3) {
            throw new ScriptException("配置干员需要三个参数 [干员名,进阶,心情]");
        }

        var op = simu.GetOperatorNoThrow(args[0]) ??
            throw new ScriptException($"{args[0]} 干员不存在");
        int upgraded = -1;
        double mood = double.NaN;

        if (args[1] != "_" && !int.TryParse(args[1], out upgraded)) {
            throw new ScriptException($"{args[1]} 不是一个有效的整数");
        }
        if (args[2] != "_" && !double.TryParse(args[2], out mood)) {
            throw new ScriptException($"{args[2]} 不是一个有效的浮点数");
        }
        if (upgraded != -1) op.Upgraded = upgraded;
        if (mood != double.NaN) op.SetMood(mood);
    }

    [Alias(Language.CN, "设置等级")]
    public static void SetLevel(Simulator simu, string[] args) {
        if (args.Length != 1) {
            throw new ScriptException("设置等级需要一个参数 [等级]");
        }
        var fac = (simu.SelectedFacilityCache ??= simu.GetFacilityByName(simu.SelectedFacilityString))
            ?? throw new ScriptException($"{simu.SelectedFacilityString} 设施不存在");
        if (!int.TryParse(args[0], out var level)) {
            throw new ScriptException($"{args[0]} 不是一个有效的整数");
        }
        fac.SetLevel(level);
    }
    [Alias(Language.CN, "设置类型")]
    public static void SetFac(Simulator simu, string[] args) {
        if (args.Length != 1) {
            throw new ScriptException("设置类型需要一个参数 [设施名]");
        }
        if (!WebHelper.Helper.RoomLabelRegex.IsMatch(simu.SelectedFacilityString)) {
            throw new ScriptException("当前设施不是可切换类型的设施");
        }
        var index = WebHelper.Helper.LabelToIndex(simu.SelectedFacilityString);

        simu.Facilities[index]?.RemoveAll();
        simu.Facilities[index] = args[0].ToLower() switch {
            "trading" => new TradingStation(),
            "贸易站" => new TradingStation(),
            "manufacturing" => new ManufacturingStation(),
            "制造站" => new ManufacturingStation(),
            "power" => new PowerStation(),
            "发电站" => new PowerStation(),
            "_" => null,
            _ => throw new ScriptException("无效设施类型名")
        };
    }

    [Alias(Language.CN, "设置产物")]
    public static void SetProduct(Simulator simu, string[] args) {
        if (args.Length != 1) {
            throw new ScriptException("设置产物需要一个参数 [产物名]");
        }
        var fac = (simu.SelectedFacilityCache ??= simu.GetFacilityByName(simu.SelectedFacilityString))
            ?? throw new ScriptException($"{simu.SelectedFacilityString} 设施不存在");
        if (fac is not ManufacturingStation) {
            throw new ScriptException($"{simu.SelectedFacilityString} 设施不是一个制造站");
        }

        try {
            WebHelper.Helper.SetProduct(simu, fac, args[0]);
        } catch (Exception e) {
            throw new ScriptException(e.Message);
        }
    }

    [Alias(Language.CN, "设置策略")]
    public static void SetStrategy(Simulator simu, string[] args) {
        if (args.Length != 1) {
            throw new ScriptException("设置策略需要一个参数 [策略名]");
        }
        var fac = (simu.SelectedFacilityCache ??= simu.GetFacilityByName(simu.SelectedFacilityString))
            ?? throw new ScriptException($"{simu.SelectedFacilityString} 设施不存在");
        if (fac is not TradingStation) {
            throw new ScriptException($"{simu.SelectedFacilityString} 设施不是一个贸易站");
        }

        try {
            WebHelper.Helper.SetStrategy(simu, fac, args[0]);
        } catch (Exception e) {
            throw new ScriptException(e.Message);
        }
    }

    [Alias(Language.CN, "收集产出")]
    public static void Collect(Simulator simu, string[] args) {
        if (args.Length > 1) {
            throw new ScriptException("收集产出需要零个或一个参数 [(订单序号)]");
        }

        var fac = (simu.SelectedFacilityCache ??= simu.GetFacilityByName(simu.SelectedFacilityString))
            ?? throw new ScriptException($"{simu.SelectedFacilityString} 设施不存在");
        if (fac is not TradingStation && args.Length == 1) {
            throw new ScriptException("输入了订单序号，但设施不是贸易站");
        }
        var idx = 0;
        if (args.Length == 1 && !int.TryParse(args[0], out idx)) {
            throw new ScriptException($"{args[0]} 不是一个有效的整数");
        }

        try {
            WebHelper.Helper.Collect(simu, fac, idx);
        } catch (Exception e) {
            throw new ScriptException(e.Message);
        }
    }

    [Alias(Language.CN, "收集全部产出")]
    public static void CollectAll(Simulator simu, string[] args) {
        if (args.Length > 0) {
            throw new ScriptException("收集全部产出不需要参数");
        }

        simu.CollectAll();
    }

    [Alias(Language.CN, "使用无人机")]
    public static void UseDrones(Simulator simu, string[] args) {
        if (args.Length > 1) {
            throw new ScriptException("收集产出需要一个参数 [无人机数量]");
        }

        var fac = (simu.SelectedFacilityCache ??= simu.GetFacilityByName(simu.SelectedFacilityString))
            ?? throw new ScriptException($"{simu.SelectedFacilityString} 设施不存在");
        if (fac is not IApplyDrones canApplyDrones) {
            throw new ScriptException($"{simu.SelectedFacilityString} 设施不是一个制造站或贸易站");
        }
        if (!int.TryParse(args[0], out var amount)) {
            throw new ScriptException($"{args[0]} 不是一个有效的整数");
        }
        canApplyDrones.ApplyDrones(simu, amount);
    }

    [Alias(Language.CN, "模拟")]
    public static void Simulate(Simulator simu, string[] args) {
        if (args.Length != 1) {
            throw new ScriptException("模拟需要一个参数 [时长]");
        }

        TimeSpan span = default;
        if (!int.TryParse(args[0], out var seconds) && !TimeSpan.TryParse(args[0], out span)) {
            throw new ScriptException($"{args[0]} 不是一个有效的时长");
        }
        if (seconds > 0) {
            span = TimeSpan.FromSeconds(seconds);
        }
        if (span <= default(TimeSpan)) {
            throw new ScriptException($"{args[0]} 不是一个有效的时长");
        }
        simu.SimulateUntil(simu.Now + span);
    }
}