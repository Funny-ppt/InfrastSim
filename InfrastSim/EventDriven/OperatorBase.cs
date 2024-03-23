namespace InfrastSim.EventDriven;

internal abstract class OperatorBase : IEventSource {
    public double Mood { get; private set; }
    public AggregateValue MoodConsumeRate { get; private set; } = new(100);

    public DateTime TriggerTime => throw new NotImplementedException();

    public event Action<IEventSource>? TriggerTimeChanged;

    public void NotifyTimeElapsed(TimeElapsedInfo info) {
        throw new NotImplementedException();
    }

    public void NotifyUpdate() {
        throw new NotImplementedException();
    }
}
