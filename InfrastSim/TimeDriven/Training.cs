namespace InfrastSim.TimeDriven;
public class Training : FacilityBase {
    public override FacilityType Type => FacilityType.Training;

    public override int AcceptOperatorNums => 1;
    public override double MoodConsumeModifier => 0.0;
    public override double EffiencyModifier => 0.05 * WorkingOperatorsCount;
}