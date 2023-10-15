namespace InfrastSim.TimeDriven.Operators;
internal class KirinRYato : OperatorBase {
    public override string Name => "麒麟R夜刀";
    static string[] _groups = { "怪物猎人小队" };
    public override string[] Groups => _groups;

    public override void Resolve(TimeDrivenSimulator simu) {
        base.Resolve(simu);

        if (Facility?.Type == FacilityType.ControlCenter && !IsTired) {
            MoodConsumeRate.SetValue(Name, 0.5);
            simu.SilverVine.SetValue(Name, 8);

            if (Upgraded >= 2) {
                if (Facility.HasGroupMember("怪物猎人小队")) {
                    simu.GlobalManufacturingEffiency.SetIfGreater(0.02);
                }
            }
        }
    }
}
