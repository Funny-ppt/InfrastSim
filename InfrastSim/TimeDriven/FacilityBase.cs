namespace InfrastSim.TimeDriven;
internal abstract class FacilityBase {
    public abstract bool IsWorking { get; }
    public abstract int PowerConsumes { get; }
    public abstract double MoodConsumeModifier { get; }
    public abstract double EffiencyModifier { get; }
    public abstract int AcceptOperatorNums { get; }
    public abstract IEnumerable<OperatorBase> Operators { get; }
}
