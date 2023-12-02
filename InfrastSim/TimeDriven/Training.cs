namespace InfrastSim.TimeDriven;
public class Training : FacilityBase {
    public override FacilityType Type => FacilityType.Training;

    public override int AcceptOperatorNums => 2;
    public override double MoodConsumeModifier => 0.0;
    public override double EffiencyModifier => 0.05 * WorkingOperatorsCount;
    public override bool IsWorking => _operators[0] != null && IsTraining;
    public bool IsTraining { get; internal set; } = false;
}