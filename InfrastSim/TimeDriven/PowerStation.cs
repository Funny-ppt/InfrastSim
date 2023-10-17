namespace InfrastSim.TimeDriven;
internal class PowerStation : FacilityBase {
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

    public override void Update(Simulator simu, TimeElapsedInfo info) {
        simu.GlobalDronesEffiency.AddValue(TotalEffiencyModifier);

        base.Update(simu, info);
    }
}
