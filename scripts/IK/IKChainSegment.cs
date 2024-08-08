using Godot;

namespace Kinematics.scripts.IK;

public class IKChainSegment
{
    public Vector3 Position;
    public readonly float Length;
    
    public IKChainSegment(Vector3 position, float length)
    {
        Position = position;
        Length = length;
    }
}