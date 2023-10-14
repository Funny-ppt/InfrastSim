namespace InfrastSim.TimeDriven;
internal class Reception : FacilityBase {
    public override FacilityType Type => FacilityType.Reception;

    public override int AcceptOperatorNums => 2;
    public override double MoodConsumeModifier => throw new NotImplementedException();
    public override double EffiencyModifier => throw new NotImplementedException();
}