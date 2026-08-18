public enum LotoSequenceStep
{
    BeltRunning,
    BeltStopped,
    EnergyIsolated,
    LockApplied,
    TagApplied,
    ZeroEnergyVerified,
    FailedAttempt
}

public enum LotoLockType
{
    Personal,
    Visitor,
    Group
}

public enum LotoTagType
{
    DangerDoNotOperate,
    Inspected,
    EquipmentOperational,
    MaintenanceComplete
}
