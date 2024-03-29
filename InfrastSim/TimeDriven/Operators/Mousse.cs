namespace InfrastSim.TimeDriven.Operators;
internal class Mousse : OperatorBase {
    public override string Name => "慕斯";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is TradingStation trading && !IsTired) {
            EfficiencyModifier.SetValue(Name, 30);
        }
        if (Facility is Dormitory dorm && Upgraded >= 1) {
            dorm.SetVipMoodModifier(-30);
            MoodConsumeRate.SetValue(Name, -30);
        }
    }
}

