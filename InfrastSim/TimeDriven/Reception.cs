namespace InfrastSim.TimeDriven;
public class Reception : FacilityBase {
    public override FacilityType Type => FacilityType.Reception;

    public override int AcceptOperatorNums => 2;
    public override double MoodConsumeModifier => 0; // TODO
    public override double EffiencyModifier => 0; // TODO
}