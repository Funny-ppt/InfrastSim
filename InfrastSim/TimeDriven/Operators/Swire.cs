namespace InfrastSim.TimeDriven.Operators;

internal class Swire : OperatorBase {
    public override string Name => "诗怀雅";

    public override void Resolve(Simulator simu) {
        base.Resolve(simu);

        if (Facility?.Type == FacilityType.ControlCenter && !IsTired) {
            simu.GlobalTradingEfficiency.SetIfGreater(7);
        }

        // TODO: missing skill 2
    }
}
