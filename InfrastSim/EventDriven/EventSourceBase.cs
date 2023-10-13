namespace InfrastSim.EventDriven;
internal class EventSourceBase : IEventSource {

    public DateTime TriggerTime => throw new NotImplementedException();

    public event Action<IEventSource>? TriggerTimeChanged;

    public void NotifyTimeElapsed(TimeElapsedInfo info) {
        throw new NotImplementedException();
    }

    public void NotifyUpdate() {
        throw new NotImplementedException();
    }

    public void AddTrigger(EventTrigger trigger) {
        
    }
}
