using Godot;

namespace Kinematics.scripts;

public static class Extensions
{
    public static Vector3 WithY(this Vector3 vec, float y)
    {
        vec.Y = y;
        return vec;
    }
    
    public static Vector3 RotatedAround(this Vector3 vec, Vector3 pivot, Vector3 axis, float angle)
    {
        return (vec - pivot).Rotated(axis, angle) + pivot;
    }

    public static Vector3 ToFlat3D(this Vector2 vec)
    {
        return new Vector3(vec.X, 0, vec.Y);
    }
}