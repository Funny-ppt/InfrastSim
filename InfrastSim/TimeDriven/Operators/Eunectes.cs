namespace InfrastSim.TimeDriven.Operators;
internal class Eunectes : OperatorBase {
    public override string Name => "森蚺";
    static string[] _groups = { "依赖设施数量" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility?.Type == FacilityType.ControlCenter && !IsTired && Upgraded >= 2) {
            if (simu.IsOpInFacility("Lancet-2", FacilityType.Power)) {
                simu.ExtraPowerStation.SetValue(Name, 2);
            }
        }
        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            simu.Delay(simu => {
                foreach (var op in manufacturing.Operators) {
                    if (!op.Groups.Contains("依赖设施数量")) {
                        op.EfficiencyModifier.MaxValue = 0;
                    }
                }
                EfficiencyModifier.SetValue(Name, (Upgraded >= 2 ? 10 : 5) * simu.PowerStationsCount());
            }, Priority.AccordingToFacilityAmount);
        }
    }
}
