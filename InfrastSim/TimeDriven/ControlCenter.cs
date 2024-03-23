namespace InfrastSim.TimeDriven;
public class ControlCenter : FacilityBase {
    public override FacilityType Type => FacilityType.ControlCenter;
    public override int PowerConsumes => 0;

    public override int AcceptOperatorNums => Level;
    public override int MoodConsumeModifier => -WorkingOperatorsCount * 5;
    public override int EffiencyModifier => 0;
    public AggregateValue ExtraMoodModifier { get; } = new();

    public override void Reset() {
        base.Reset();

        ExtraMoodModifier.Clear();
    }
    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        foreach (var op in Operators) {
            op.MoodConsumeRate.SetValue("control-center-extra", ExtraMoodModifier);
        }
    }
}
