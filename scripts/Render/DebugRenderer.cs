using Godot;
using Kinematics.scripts.IK;

namespace Kinematics.scripts.Render;

public class DebugRenderer : IIKChainRenderer
{
    private const float LimbJointRadius = 0.1f;
    private readonly Color _rootColor = new(1, 0, 0);
    private readonly float _hueShift = 0.15f;

    public void Render(IKChain chain)
    {
        DebugDraw3D.DrawSphere(chain.RootPosition, 0.1f, new Color(1, 1, 1));
        
        bool canFitSpheres = chain.TotalLength > chain.Segments.Length * LimbJointRadius * 2;
        var col = _rootColor;
        for (var i = 0; i < chain.Segments.Length; i++)
        {
            var segment = chain.Segments[i];
            if (canFitSpheres)
                DebugDraw3D.DrawSphere(segment.Position, LimbJointRadius, col);
            if (i < chain.Segments.Length - 1) // Last segment won't have a next one
                DebugDraw3D.DrawArrow(segment.Position, chain.Segments[i + 1].Position, col, 0.1f);
            col.H = (col.H + (_hueShift / chain.Segments.Length)) % 1;
        }
        var lastSegment = chain.Segments[^1];
        var dirToDest = lastSegment.Position.DirectionTo(chain.LastTarget);
        DebugDraw3D.DrawArrow(lastSegment.Position, lastSegment.Position + dirToDest * lastSegment.Length, col, 0.1f);
    }
}