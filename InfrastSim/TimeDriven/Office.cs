namespace InfrastSim.TimeDriven;
public class Office : FacilityBase {
    public override FacilityType Type => FacilityType.Office;

    public override int AcceptOperatorNums => 1;
    public override double MoodConsumeModifier => 0.0;
    public override double EffiencyModifier => WorkingOperatorsCount * 0.05;

    public static readonly TimeSpan ProduceTime = TimeSpan.FromHours(12);
}