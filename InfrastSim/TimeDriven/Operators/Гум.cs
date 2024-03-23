namespace InfrastSim.TimeDriven.Operators;
internal class Гум : OperatorBase {
    public override string Name => "古米";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            EfficiencyModifier.SetValue(Name, 30);
            MoodConsumeRate.SetValue(Name, -25);
        }
        if (Facility is Dormitory dorm && Upgraded >= 1) {
            dorm.SetVipMoodModifier(-35);
            MoodConsumeRate.SetValue(Name, -35);
        }
    }
}

