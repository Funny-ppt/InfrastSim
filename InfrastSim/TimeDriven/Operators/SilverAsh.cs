namespace InfrastSim.TimeDriven.Operators;

internal class SilverAsh : OperatorBase {
    public override string Name => "银灰";
    static string[] _groups = { "喀兰贸易" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            EfficiencyModifier.SetValue(Name, Upgraded >= 2 ? 20: 15);
            trading.Capacity.SetValue(Name, Upgraded >= 2 ? 4 : 2);
        }
    }
}