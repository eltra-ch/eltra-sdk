namespace EltraCloudContracts.ObjectDictionary.Epos4.Definitions
{
    public enum Epos4OperationModes
    {
        None = 0x00,
        ProfilePosition = 0x001,
        ProfileVelocity = 0x003,
        Homing = 0x006,
        InterpolatedPosition = 0x007,
        CyclicSynchronousVelocity = 0x009,
        CyclicSynchronousTorqueMode = 0x00A,
        CyclicSynchronousTorqueModeWithCommutationAngle = 0x000B
    }
}
