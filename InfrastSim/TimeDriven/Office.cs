namespace InfrastSim.TimeDriven;
public class Office : FacilityBase {
    public override FacilityType Type => FacilityType.Office;

    public override int AcceptOperatorNums => 1;
    public override int MoodConsumeModifier => 0;
    public override int EffiencyModifier => WorkingOperatorsCount * 5;

    public static readonly TimeSpan ProduceTime = TimeSpan.FromHours(12);
}