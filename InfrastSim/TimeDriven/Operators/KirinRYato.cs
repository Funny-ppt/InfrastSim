namespace InfrastSim.TimeDriven.Operators;
internal class KirinRYato : OperatorBase {
    public override string Name => "麒麟R夜刀";
    static string[] _groups = { "怪物猎人小队", "异格" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility?.Type == FacilityType.ControlCenter && !IsTired) {
            MoodConsumeRate.SetValue(Name, 50);
            simu.SilverVine.SetValue(Name, 8);

            if (Upgraded >= 2) {
                if (Facility.GroupMemberCount("怪物猎人小队") >= 2) {
                    simu.GlobalManufacturingEfficiency.SetIfGreater(2);
                }
            }
        }
    }
}
