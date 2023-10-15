namespace InfrastSim.TimeDriven;
internal class ControlCenter : FacilityBase {
    public override FacilityType Type => FacilityType.ControlCenter;
    public override int PowerConsumes => 0;

    public override int AcceptOperatorNums => Level;
    public override double MoodConsumeModifier => -WorkingOperatorsCount * 0.05;
    public override double EffiencyModifier => 0.0;
    public AggregateValue ExtraMoodModifier { get; } = new();

    public override void Reset() {
        base.Reset();

        ExtraMoodModifier.Clear();
    }
    public override void Resolve(TimeDrivenSimulator simu) {
        base.Resolve(simu);

        foreach (var op in Operators) {
            op.MoodConsumeRate.SetValue("control-center-extra", ExtraMoodModifier);
        }
    }
}
