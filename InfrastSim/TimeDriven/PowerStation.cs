namespace InfrastSim.TimeDriven;
public class PowerStation : FacilityBase {
    public override FacilityType Type => FacilityType.Power;
    public override int PowerConsumes => Level switch {
        1 => -60,
        2 => -130,
        3 => -270,
        _ => 0,
    };

    public override int AcceptOperatorNums => 1;
    public override double MoodConsumeModifier => 0;
    public override double EffiencyModifier => WorkingOperatorsCount * 0.05;
}
