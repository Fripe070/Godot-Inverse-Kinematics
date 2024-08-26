using Godot;
using Kinematics.scripts.IK;
using Kinematics.scripts.Render;

namespace Kinematics.scripts.creature.Scourge;

public partial class Tail : Node3D
{
    [Export] private Mesh _mesh;
    
    [Export] private float _segmentSeparation = 0.5f;
    [Export] private int _segmentCount = 5;

    private IKChain _chain;
    private MeshInstance3D[] _segmentMeshes;
    
    public override void _Ready()
    {
        var chainOptions = new IKChainOptions { };
        _chain = new IKChain(GlobalPosition, _segmentSeparation, _segmentCount, chainOptions);
        
        _segmentMeshes = new MeshInstance3D[_segmentCount];
        for (var i = 0; i < _segmentCount; i++)
        {
            var meshInstance = new MeshInstance3D();
            meshInstance.Mesh = _mesh;
            AddChild(meshInstance);
            meshInstance.SetAsTopLevel(true);
            _segmentMeshes[i] = meshInstance;
        }
    }
    
    public override void _Process(double delta)
    {
        _chain.RootPosition = GlobalPosition;
        _chain.FabrikBackward();
;
        for (var i = 0; i < _segmentMeshes.Length; i++)
        {
            _segmentMeshes[i].Position = _chain.Segments[i].TipPosition;
            var lastPos = i == 0 ? GlobalPosition : _chain.Segments[i - 1].TipPosition;
            _segmentMeshes[i].Transform = _segmentMeshes[i].Transform.LookingAt(lastPos, Vector3.Up);
        }
    }
}