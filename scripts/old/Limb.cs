using Godot;
using Kinematics.scripts.IK;
using Kinematics.scripts.Render;

namespace Kinematics.scripts.old;

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
        if (_constrainSinglePlane)
            _chain.PointTowardsAndUp(Destination, 70);
        _chain.SolveTo(Destination);

        _renderer.Render(_chain);
    }
}