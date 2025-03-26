using Unity.Collections;
using Unity.Entities;

public struct PlayerLeftEvent : IComponentData
{
    public FixedString64Bytes PlayerId;
}