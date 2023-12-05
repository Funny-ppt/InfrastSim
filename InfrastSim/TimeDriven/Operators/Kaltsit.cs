namespace InfrastSim.TimeDriven.Operators;
internal class Kaltsit : OperatorBase {
    public override string Name => "凯尔希";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        // TODO: missing skill 1

        if (Facility?.Type == FacilityType.ControlCenter && !IsTired && Upgraded >= 2) {
            simu.GlobalManufacturingEfficiency.SetIfGreater(0.02);
        }
    }
}
