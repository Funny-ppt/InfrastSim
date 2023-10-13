namespace InfrastSim.EventDriven;
internal abstract class EventTrigger {
    IEventSource _source;
    public event Action<IEventSource>? Action;

    public EventTrigger(IEventSource source) {
        _source = source;
    }
}
