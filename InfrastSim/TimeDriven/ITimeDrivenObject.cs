namespace InfrastSim.TimeDriven;
internal interface ITimeDrivenObject {
    void Reset();
    void Resolve(Simulator simu);
    void Update(Simulator simu, TimeElapsedInfo info);
}
