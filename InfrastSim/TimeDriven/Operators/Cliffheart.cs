namespace InfrastSim.TimeDriven.Operators;

internal class Cliffheart : OperatorBase {
    public override string Name => "崖心";
    static string[] _groups = { "喀兰贸易" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is Dormitory dorm) {
            dorm.SetVipMoodModifier(-25);
            MoodConsumeRate.SetValue(Name, -50);
        }
        if (Facility is TradingStation trading && !IsTired && Upgraded >= 2) {
            EfficiencyModifier.SetValue(Name, 15);
            trading.Capacity.SetValue(Name, Upgraded >= 2 ? 4 : 2);
        }
    }
}