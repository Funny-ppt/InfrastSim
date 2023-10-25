namespace InfrastSim.TimeDriven.Operators;
internal class Vermeil : OperatorBase {
    public override string Name => "红云";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            MoodConsumeRate.SetValue(Name, -0.25);
            manufacturing.Capacity.SetValue(Name, 8);

            if (Upgraded >= 1) {
                var op = manufacturing.FindOp("泡泡");
                if (op != null && op.Upgraded >= 1) return; // 泡泡优先生效

                simu.Delay(simu => {
                    var amount = 0.02 * (manufacturing.Capacity - manufacturing.BaseCapacity);
                    EfficiencyModifier.SetValue(Name, Math.Max(0, amount));
                });
            }
        }
    }
}
