using Godot;
using Kinematics.scripts.Render;

namespace Kinematics.scripts.Walker;

public partial class Walker : Node3D
{
	[Export] private int _legCount = 4;
	[Export] private float _drag = 0.5f;
	
	private Leg[] _legs;
	public Vector3 Velocity;
	public Vector3 MovementTarget { get; set; }
	
	private IIKChainRenderer _legRenderer;
	
	public override void _Ready()
	{
		MovementTarget = GlobalPosition;
		_legs = new Leg[_legCount];

		float legYRotation = Mathf.Pi / _legCount;
		for (var i = 0; i < _legCount; i++)
		{
			_legs[i] = new Leg(this, new LegOptions
			{
				RootOffset = new Vector3(0, 0, 0),
				DesiredFootOffset = (new Vector3(3, -1, 0)).Rotated(Vector3.Up, legYRotation),
			});
			legYRotation += Mathf.Pi * 2 / _legCount;
		}
		
		_legRenderer = new DebugRenderer();
	}

	public override void _Process(double delta)
	{
		var moveDir = GlobalTransform.Origin.DirectionTo(MovementTarget);
		moveDir.Y = 0;
		Velocity += moveDir * (float)delta * 3;
		
		GlobalTranslate(Velocity * (float)delta);
		Velocity *= 1 - (float)delta * _drag;
		
		foreach (var leg in _legs)
		{
			leg.Update(delta);
			leg.Render(_legRenderer);
		}
	}
}