namespace InfrastSim.TimeDriven;
public class Crafting : FacilityBase {
    public override FacilityType Type => FacilityType.Crafting;
    public override int PowerConsumes => Level switch {
        1 or 2 or 3 => 10,
        _ => 0
    };

    public override int AcceptOperatorNums => 1;

    public override bool IsWorking => false;

    public override double MoodConsumeModifier => 0;

    public override double EffiencyModifier => 0;
}