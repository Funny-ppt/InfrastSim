namespace InfrastSim.TimeDriven;
internal class Dormitory : FacilityBase {
    public override FacilityType Type => FacilityType.Dormitory;
    public override int PowerConsumes => Level switch {
        1 => 10,
        2 => 20,
        3 => 30,
        4 => 45,
        5 => 65,
        _ => 0,
    };

    public override bool IsWorking => true; // force operators to calculate mood change
    public override int AcceptOperatorNums => 5;

    public override double MoodConsumeModifier {
        get {
            return -(1 + 1.5 + 0.1 * Level);
        }
    }
    public AggregateValue DormMoodModifier { get; } = new(0.0, max: 0.0);
    public double DormMoodModifierForOne = 0.0;
    public override double EffiencyModifier => 0.0;
    public int Atmosphere { get; set; } = 0;

    public override void Reset() {
        base.Reset();

        DormMoodModifier.Clear();
    }
    public override void Resolve(TimeDrivenSimulator simu) {
        base.Resolve(simu);

        DormMoodModifier.SetValue("atomsphere", -0.0004 * Atmosphere);
        foreach (var op in Operators) {
            op.MoodConsumeRate.SetValue("dorm-extra", DormMoodModifier);
            op.MoodConsumeRate.Disable("control-center-mod");
        }
    }
}
