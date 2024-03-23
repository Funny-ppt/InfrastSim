namespace InfrastSim.TimeDriven.Operators;
internal class Vulcan : OperatorBase {
    public override string Name => "火神";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility is ManufacturingStation manufacturing && !IsTired) {
            EfficiencyModifier.SetValue(Name, -5);
            MoodConsumeRate.SetValue(Name, Upgraded >= 2 ? -25 : -15);
            manufacturing.Capacity.SetValue(Name, Upgraded >= 2 ? 19 : 16);
        }
    }
}
