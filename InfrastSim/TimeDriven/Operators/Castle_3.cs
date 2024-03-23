namespace InfrastSim.TimeDriven.Operators;
internal class Castle_3 : OperatorBase {
    public override string Name => "Castle-3";
    static string[] _groups = { "作业平台" };
    public override string[] Groups => _groups;

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            if (manufacturing.IsProduceCombatRecord()) {
                EfficiencyModifier.SetValue(Name, 30);
            }
        }
        if (Facility is PowerStation && !IsTired) {
            EfficiencyModifier.SetValue(Name, 10);
        }
    }
}

