using Godot;
using Kinematics.scripts.Render;

namespace Kinematics.scripts.Walker;

public partial class Walker : Node3D
{
	[Export] private int _legCount = 4;
	[Export] private float _drag = 0.5f;
	
	private Leg[] _legs;
	private Vector3 _velocity;
	
	private IIKChainRenderer _legRenderer;
	
	public override void _Ready()
	{
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
		GlobalTranslate(_velocity * (float)delta);
		_velocity *= 1 - (float)delta * _drag;
		
		_velocity += new Vector3(0, 0, 0.3f) * (float)delta;

		foreach (var leg in _legs)
		{
			leg.Update(delta);
			leg.Render(_legRenderer);
		}
	}
}