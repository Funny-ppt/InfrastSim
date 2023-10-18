namespace InfrastSim.TimeDriven.Operators;
internal class RathalosSNoirCorne : OperatorBase {
    public override string Name => "火龙S黑角";
    static string[] _groups = { "怪物猎人小队", "异格" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility?.Type == FacilityType.ControlCenter && !IsTired) {
            var count = Facility.GroupMemberCount("怪物猎人小队");
            simu.SilverVine.SetValue(Name, 2 * count);

            if (Upgraded >= 2 && Facility.HasGroupMember("怪物猎人小队")) {
                simu.GlobalTradingEffiency.SetIfGreater(0.07);
            }
        }
    }
}
