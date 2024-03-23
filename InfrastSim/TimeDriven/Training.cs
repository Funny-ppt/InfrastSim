namespace InfrastSim.TimeDriven;
public class Training : FacilityBase {
    public override FacilityType Type => FacilityType.Training;

    public override int AcceptOperatorNums => 2;
    public override int MoodConsumeModifier => 0;
    public override int EffiencyModifier => WorkingOperatorsCount * 5;
    public override bool IsWorking => _operators[0] != null && IsTraining;
    public bool IsTraining { get; internal set; } = false;
}