namespace InfrastSim.TimeDriven.Operators;

internal class Matterhorn : OperatorBase {
    public override string Name => "角峰";
    static string[] _groups = { "喀兰贸易" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            EfficiencyModifier.SetValue(Name, 15);
            trading.Capacity.SetValue(Name, 2);
        }

        // TODO: missing skill 2
    }
}