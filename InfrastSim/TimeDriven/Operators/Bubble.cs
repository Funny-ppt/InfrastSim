namespace InfrastSim.TimeDriven.Operators;
internal class Bubble : OperatorBase {
    public override string Name => "泡泡";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            MoodConsumeRate.SetValue(Name, -25);
            manufacturing.Capacity.SetValue(Name, 10);

            if (Upgraded >= 1) {
                simu.Delay(simu => {
                    foreach (var kvp in manufacturing.Capacity.EnumerateValues()) {
                        var op = simu.GetOperatorNoThrow(kvp.Key);
                        if (op != null) {
                            var factor = kvp.Value switch {
                                >= 17 => 3,
                                > 0 => 1,
                                _ => 0
                            };
                            op.EfficiencyModifier.SetValue(Name, kvp.Value * factor);
                        }
                    }
                });
            }
        }
    }
}
