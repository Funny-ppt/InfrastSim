namespace InfrastSim.TimeDriven.Operators;
internal class Chongyue : OperatorBase {
    public override string Name => "重岳";
    static string[] _groups = { "岁" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility?.Type == FacilityType.ControlCenter && !IsTired) {
            MoodConsumeRate.SetValue("岁", 0.5);
            var count = simu.GroupMemberCount("岁");
            simu.Renjianyanhuo.SetValue(Name, Math.Max(count, 5) * 5);

            if (Upgraded >= 2) {
                simu.Delay(simu => {
                    var ops = simu.OperatorsInFacility.Where(op =>
                        op.Facility is not Dormitory
                     && op.Facility is not ControlCenter);
                    var amount = Math.Floor(simu.Renjianyanhuo / 20) * 0.05 + 0.05;

                    foreach (var op in ops) {
                        op.MoodConsumeRate.SetIfLesser("control-center-extra", amount);
                    }
                });
            }
        }
    }
}
