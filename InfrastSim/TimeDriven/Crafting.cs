namespace InfrastSim.TimeDriven;
internal class Crafting : FacilityBase {
    public override FacilityType Type => FacilityType.Crafting;
    public override int PowerConsumes => Level switch {
        1 or 2 or 3 => 10,
        _ => 0
    };

    public override int AcceptOperatorNums => 1;

    public override double MoodConsumeModifier => throw new NotImplementedException();

    public override double EffiencyModifier => throw new NotImplementedException();
}