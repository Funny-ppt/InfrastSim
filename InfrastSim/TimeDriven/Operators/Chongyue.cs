namespace InfrastSim.TimeDriven.Operators;
internal class Chongyue : OperatorBase {
    public override string Name => "重岳";
    static string[] _groups = { "岁" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility?.Type == FacilityType.ControlCenter && !IsTired) {
            MoodConsumeRate.SetValue(Name, 50);
            var count = simu.GroupMemberCount("岁");
            simu.Renjianyanhuo.SetValue(Name, Math.Min(count, 5) * 5);

            if (Upgraded >= 2) {
                simu.Delay(simu => {
                    var ops = simu.OperatorsInFacility.Where(op =>
                        op.Facility is not Dormitory
                     && op.Facility is not ControlCenter);
                    var amount = simu.Renjianyanhuo / 20 * 5 + 5;

                    foreach (var op in ops) {
                        op.MoodConsumeRate.SetIfLesser("control-center-extra", amount);
                    }
                });
            }
        }
    }
}
