namespace InfrastSim.EventDriven;

internal interface IEventSource
{
    DateTime TriggerTime { get; }
    event Action<IEventSource>? TriggerTimeChanged;

    void NotifyTimeElapsed(TimeElapsedInfo info);
    void NotifyUpdate();
}
