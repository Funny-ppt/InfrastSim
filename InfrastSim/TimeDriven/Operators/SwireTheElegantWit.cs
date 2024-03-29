namespace InfrastSim.TimeDriven.Operators;

internal class SwireTheElegantWit : OperatorBase {
    public override string Name => "琳琅诗怀雅";
    static string[] _groups = { "异格" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            EfficiencyModifier.SetValue(Name, 20);

            if (Upgraded >= 2) {
                simu.Delay(simu => {
                    var diff = trading.Capacity - trading.BaseCapacity;
                    EfficiencyModifier.AddValue(Name, diff * 4);
                }, Priority.Swire);
            }
        }
    }
}
