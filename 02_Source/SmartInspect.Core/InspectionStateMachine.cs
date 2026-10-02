namespace SmartInspect.Core;

public enum InspectionState { Idle, WaitingTrigger, AcquiringImage, Inspecting, Saving, Reporting, Completed, Error }

public sealed class InspectionStateMachine
{
    public InspectionState State { get; private set; } = InspectionState.Idle;
    public bool IsRunning => State is InspectionState.AcquiringImage or InspectionState.Inspecting or InspectionState.Saving or InspectionState.Reporting;
    public event Action<InspectionState>? Changed;

    public bool TryStart()
    {
        if (State is not (InspectionState.Idle or InspectionState.WaitingTrigger)) return false;
        MoveTo(InspectionState.AcquiringImage);
        return true;
    }

    public void MoveTo(InspectionState next)
    {
        bool allowed = next == InspectionState.Error || (State, next) switch
        {
            (InspectionState.Idle, InspectionState.WaitingTrigger or InspectionState.AcquiringImage) => true,
            (InspectionState.WaitingTrigger, InspectionState.Idle or InspectionState.AcquiringImage) => true,
            (InspectionState.AcquiringImage, InspectionState.Inspecting) => true,
            (InspectionState.Inspecting, InspectionState.Saving) => true,
            (InspectionState.Saving, InspectionState.Reporting) => true,
            (InspectionState.Reporting, InspectionState.Completed) => true,
            (InspectionState.Completed or InspectionState.Error, InspectionState.Idle or InspectionState.WaitingTrigger) => true,
            _ => State == next
        };
        if (!allowed) throw new InvalidOperationException($"无效检测状态转换：{State} → {next}");
        State = next;
        Changed?.Invoke(next);
    }
}
