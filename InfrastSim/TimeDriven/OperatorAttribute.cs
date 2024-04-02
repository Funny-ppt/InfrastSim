namespace InfrastSim.TimeDriven;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal class OperatorAttribute : Attribute {
    public string Name { get; }
    public OperatorGroup[] Groups { get; }
    public CompositionSkillInfo[] Skills { get; }
}
