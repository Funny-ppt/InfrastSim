namespace InfrastSim.TimeDriven;
public class Office : FacilityBase {
    public override FacilityType Type => FacilityType.Office;

    public override int AcceptOperatorNums => 1;
    public override double MoodConsumeModifier => 0.0;
    public override double EffiencyModifier => 0.0;
}