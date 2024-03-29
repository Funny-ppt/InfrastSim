namespace InfrastSim.TimeDriven;
public class Reception : FacilityBase {
    public override FacilityType Type => FacilityType.Reception;

    public override int AcceptOperatorNums => 2;
    public override int MoodConsumeModifier => 0; // TODO
    public override int EffiencyModifier => 0; // TODO
}