using Godot;
using Kinematics.scripts.IK;

namespace Kinematics.scripts.Render;

public partial class PathRenderer : Path3D, IIKChainRenderer
{
    private CsgPolygon3D _mesh;
    
    public PathRenderer(CsgPolygon3D mesh)
    {
        _mesh = mesh.Duplicate() as CsgPolygon3D;
    }
    
    public override void _Ready()
    {
        Curve = new Curve3D();
        _mesh.PathNode = GetPath();
        _mesh.Name = "Leg Mesh";
        AddChild(_mesh);
    }
    
    public void Render(IKChain chain)
    {
        Curve.ClearPoints();
        Curve.AddPoint(chain.RootPosition);
        foreach (var segment in chain.Segments)
            Curve.AddPoint(segment.TipPosition);
    }
}