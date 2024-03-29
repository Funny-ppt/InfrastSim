namespace InfrastSim.TimeDriven.Operators;
internal class Weedy : OperatorBase {
    public override string Name => "温蒂";
    static string[] _groups = { "依赖设施数量" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            simu.Delay(simu => {
                foreach (var op in manufacturing.Operators) {
                    if (!op.Groups.Contains("依赖设施数量")) {
                        op.EfficiencyModifier.MaxValue = 0;
                    }
                }
                EfficiencyModifier.SetValue(Name, (Upgraded >= 2 ? 15 : 10) * simu.PowerStationsCount());
            }, Priority.AccordingToFacilityAmount);
        }
    }
}
