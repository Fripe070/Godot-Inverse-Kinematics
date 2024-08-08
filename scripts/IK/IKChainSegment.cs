using Godot;

namespace Kinematics.scripts.IK;

public class IKChainSegment
{
    public Vector3 TipPosition;
    public readonly float Length;
    
    public IKChainSegment(Vector3 tipPosition, float length)
    {
        TipPosition = tipPosition;
        Length = length;
    }
}