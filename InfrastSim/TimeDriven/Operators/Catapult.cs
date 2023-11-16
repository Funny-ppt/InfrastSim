namespace InfrastSim.TimeDriven.Operators;
internal class Catapult : OperatorBase {
    public override string Name => "空爆";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.3);
            MoodConsumeRate.SetValue(Name, -0.25);
        }

        // TODO: missing skill 2
    }
}

