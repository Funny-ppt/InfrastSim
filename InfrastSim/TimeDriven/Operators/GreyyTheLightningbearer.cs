namespace InfrastSim.TimeDriven.Operators;

internal class GreyyTheLightningbearer : OperatorBase {
    public override string Name => "承曦格雷伊";
    static string[] _groups = { "异格" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is PowerStation power && !IsTired) {
            EfficiencyModifier.SetValue(Name, 0.2); // FIXME: 无人机上限
            if (!simu.PowerStations.Any(
                fac => fac.HasGroupMember("作业平台") && fac.WorkingOperatorsCount > 0)) {
                simu.ExtraPowerStation.SetValue(Name, 1);
            }
        }
    }
}
