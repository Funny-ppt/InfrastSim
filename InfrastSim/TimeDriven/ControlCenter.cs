namespace InfrastSim.TimeDriven;
internal class ControlCenter : FacilityBase {
    public override FacilityType Type => FacilityType.ControlCenter;
    public override int PowerConsumes => 0;

    public override int AcceptOperatorNums => Level;
    public override double MoodConsumeModifier => -WorkingOperatorsCount * 0.05;
    public override double EffiencyModifier => 0.0;
}
