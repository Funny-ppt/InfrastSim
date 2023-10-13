namespace InfrastSim.TimeDriven;
internal class ManufacturingStation : FacilityBase {
    public int Level { get; private set; }
    public override int PowerConsumes => Level switch {
        1 => 10,
        2 => 30,
        3 => 60,
        _ => 0,
    };
    public int BaseCapacity => Level switch {
        1 => 24,
        2 => 36,
        3 => 54,
        _ => 0,
    };

    public override bool IsWorking => Product != null && Operators.Any();
    public Product? Product { get; set; }
    public double ProductProgress { get; private set; }

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

    public override int AcceptOperatorNums => Level;

    OperatorBase?[] _operators = new OperatorBase[3];
#pragma warning disable CS8619 // CS8619:值中的引用类型的为 Null 性与目标类型不匹配。 Where返回的可以保证非 Null 性
    public override IEnumerable<OperatorBase> Operators
        => _operators.Take(AcceptOperatorNums).Where(op => op != null);
#pragma warning restore CS8619
    public int WorkingOperators => Operators.Where(op => !op.IsTired).Count();


}
