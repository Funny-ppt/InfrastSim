namespace InfrastSim.TimeDriven.Operators;

internal class Myrtle : OperatorBase {
    public override string Name => "桃金娘";
    static string[] _groups = { "杜林" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);


        if (Facility is TradingStation trading && !IsTired) {
            MoodConsumeRate.SetValue(Name, -25);
            trading.Capacity.SetValue(Name, 5);
        }
        if (Facility is Dormitory dorm && Upgraded >= 1) {
            dorm.SetDormMoodModifier(-15);
        }
    }
}
