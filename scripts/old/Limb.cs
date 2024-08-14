using Godot;
using Kinematics.scripts.IK;
using Kinematics.scripts.Render;

namespace Kinematics.scripts.old;

[Tool]
public partial class Limb : Node3D
{
    [Export]
    public Vector3 RelativeDestination
    {
        get => Destination - Position;
        set => Destination = Position + value;
    }

    [Export] private int _segmentCount = 4;
    [Export] private float _segmentLength = 2.0f;
    [Export] private bool _constrainSinglePlane = true;
    
    private float TotalLength => _segmentCount * _segmentLength;

    public Vector3 Destination;

    private IKChain _chain;
    private IIKChainRenderer _renderer;

    public override void _Ready()
    {
        var chainOptions = new IKChainOptions
        {
        };
        _chain = new IKChain(Position, _segmentLength, _segmentCount, chainOptions);
        _renderer = new DebugRenderer();
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint())
        {
            DebugDraw3D.DrawSphere(GlobalPosition, 0.1f, new Color(1, 1, 1));
            DebugDraw3D.DrawSphere(GlobalPosition, TotalLength, new Color(1, 1, 1));
            return;
        }
        
        if (_constrainSinglePlane)
            _chain.PointTowardsAndUp(Destination, 70);
        _chain.SolveTo(Destination);

        _renderer.Render(_chain);
    }
}