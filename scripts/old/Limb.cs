using Godot;
using Kinematics.scripts.IK;
using Kinematics.scripts.Render;

namespace Kinematics.scripts.old;

public partial class Limb : Node3D
{
    [Export]
    public Vector3 RelativeDestination
    {
        get => Destination - GlobalPosition;
        set => Destination = GlobalPosition + value;
    }

    [Export] private int _segmentCount = 4;
    [Export] private float _segmentLength = 2.0f;

    public Vector3 Destination;

    private IKChain _chain;
    private IIKChainRenderer _renderer;

    public override void _Ready()
    {
        var chainOptions = new IKChainOptions
        {
            // PrioritiseEnd = true,
            // StraightIfTooFar = false
        };
        _chain = new IKChain(GlobalPosition, _segmentLength, _segmentCount, chainOptions);
        _renderer = new DebugRenderer();
    }

    public override void _Process(double delta)
    {
        PointTowardsAndUp(Destination, -70);
        _chain.SolveTo(Destination);

        _renderer.Render(_chain);

        DebugDraw2D.SetText("Target pos", Destination);
        DebugDraw2D.SetText("Error", _chain.Error);

        var averageLength = 0f;
        averageLength += _chain.Segments[0].TipPosition.DistanceTo(_chain.RootPosition);
        for (var i = 1; i < _chain.Segments.Length; i++)
            averageLength += _chain.Segments[i].TipPosition.DistanceTo(_chain.Segments[i - 1].TipPosition);
        DebugDraw2D.SetText("Average length", averageLength / _chain.Segments.Length);
    }

    private void PointTowardsAndUp(Vector3 goal, float upAngle)
    {
        var direction = goal - _chain.RootPosition;
        direction.Y = 0;
        direction = direction.Normalized();
        var axis = Vector3.Up.Cross(direction).Normalized();
        _chain.PointIn(direction.Rotated(axis, Mathf.DegToRad(upAngle)));
    }
}