namespace InfrastSim.TimeDriven.Operators;

internal class WaaiFu : OperatorBase {
    public override string Name => "槐琥";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            simu.Delay(simu => {
                var names = manufacturing.Operators.Select(op => op.Name);
                var bonus_eff = 0;
                foreach (var op in manufacturing.Operators) {
                    if (op == this) continue;

                    var eff = 0;
                    foreach (var name in names) {
                        op.MoodConsumeRate.Disable(name);
                        eff += op.EfficiencyModifier.GetValue(name);
                    }
                    bonus_eff += eff / 5 * 5;
                }
                if (Upgraded >= 2) {
                    EfficiencyModifier.SetValue(Name, Math.Min(40, bonus_eff));
                }
            }, Priority.WaaiFu);
        }
    }
}