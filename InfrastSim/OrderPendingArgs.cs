namespace InfrastSim; 
public class OrderPendingArgs {
    public OrderPendingArgs(Order order) {
        CurrentOrder = OriginOrder = order ?? throw new ArgumentNullException(nameof(order));
    }

    public Order OriginOrder { get; }
    public Order CurrentOrder { get; set; }
}
