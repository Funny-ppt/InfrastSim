namespace InfrastSim.TimeDriven.Operators;

internal class Shamare : OperatorBase {
    public Shamare() { OnResolve += simu => Skills.裁缝Alpha(this, simu); }
    public override string Name => "巫恋";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired && Upgraded >= 2) {
            foreach (var op in trading.Operators) {
                if (op != this) {
                    op.EffiencyModifier.MaxValue = 0;
                }
                op.MoodConsumeRate.SetValue(Name, 0.25);
            }
            EffiencyModifier.SetValue(Name, (trading.Operators.Count() - 1) * 0.45);
        }
    }
}