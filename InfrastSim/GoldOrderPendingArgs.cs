namespace InfrastSim;
public class GoldOrderPendingArgs {
    public GoldOrderPendingArgs(AggregateValue priority2Gold, AggregateValue priority3Gold, AggregateValue priority4Gold)
    {
        Priority2Gold = priority2Gold ?? throw new ArgumentNullException(nameof(priority2Gold));
        Priority3Gold = priority3Gold ?? throw new ArgumentNullException(nameof(priority3Gold));
        Priority4Gold = priority4Gold ?? throw new ArgumentNullException(nameof(priority4Gold));
    }

    public AggregateValue Priority2Gold { get; }
    public AggregateValue Priority3Gold { get; }
    public AggregateValue Priority4Gold { get; }
}
