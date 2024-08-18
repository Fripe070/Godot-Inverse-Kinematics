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
        Render(chain, _rootColor);
    }
    
    private void Render(IKChain chain, Color color)
    {
        DebugDraw3D.DrawSphere(chain.RootPosition, 0.1f, new Color(0.2f, 0.2f, 0.2f));
        
        bool canFitSpheres = chain.TotalLength > chain.Segments.Length * LimbJointRadius * 2;
        for (var i = 0; i < chain.Segments.Length; i++)
        {
            var lastPos = i == 0 ? chain.RootPosition : chain.Segments[i - 1].TipPosition;
            DebugDraw3D.DrawArrow(lastPos, chain.Segments[i].TipPosition, color, 0.1f);
            if (canFitSpheres)
                DebugDraw3D.DrawSphere(chain.Segments[i].TipPosition, LimbJointRadius, color);
            color.H = (color.H + (_hueShift / chain.Segments.Length)) % 1;
        }
    }
}