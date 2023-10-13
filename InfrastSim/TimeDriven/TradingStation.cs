namespace InfrastSim.TimeDriven;
internal class TradingStation : FacilityBase {
    public override bool IsWorking => throw new NotImplementedException();

    public override int PowerConsumes => throw new NotImplementedException();

    public override double MoodConsumeModifier {
        get {
            return Math.Max(0.0, 0.05 * (WorkingOperators - 1));
        }
    }

    public override double EffiencyModifier {
        get {
            return 0.01 * WorkingOperators;
        }
    }

    public override int AcceptOperatorNums => throw new NotImplementedException();

    public override IEnumerable<OperatorBase> Operators => throw new NotImplementedException();
    public int WorkingOperators => Operators.Where(op => !op.IsTired).Count();
}
