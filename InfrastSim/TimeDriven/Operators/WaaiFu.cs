namespace InfrastSim.TimeDriven.Operators;

internal class WaaiFu : OperatorBase {
    public override string Name => "槐琥";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            simu.Delay(simu => {
                var names = manufacturing.Operators.Select(op => op.Name);
                foreach (var op in manufacturing.Operators) {
                    if (op == this) continue;

                    foreach (var name in names) {
                        op.MoodConsumeRate.Disable(op.Name);
                    }
                    if (Upgraded >= 2) {
                        EfficiencyModifier.AddValue(Name, Math.Max(0, Util.Align(op.EfficiencyModifier, 0.05)));
                    }
                }
                EfficiencyModifier.SetIfLesser(Name, 0.4);
            });
        }
    }
}