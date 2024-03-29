namespace InfrastSim.TimeDriven.Operators;

internal class Degenbrecher : OperatorBase {
    public override string Name => "锏";
    static string[] _groups = { "喀兰贸易" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            EfficiencyModifier.SetValue(Name, Upgraded >= 2 ? 25 : 20);
            trading.Capacity.SetValue(Name, Upgraded >= 2 ? -6 : -2);

            if (Upgraded >= 2) {
                simu.Delay(simu => {
                    var cap = trading.Capacity
                        .EnumerateValues()
                        .Where(kvp => kvp.Key != "base")
                        .Sum(kvp => kvp.Value);
                    EfficiencyModifier.AddValue(Name, Math.Clamp(cap / 5 * 25, 0, 100));
                });
            }
        }
    }
}