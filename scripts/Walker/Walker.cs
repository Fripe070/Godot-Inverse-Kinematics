using System;
using Godot;
using Kinematics.scripts.Render;

namespace Kinematics.scripts.Walker;

public partial class Walker : Node3D
{
	[Export] private int _legCount = 4;
	[Export] private float _legSegmentLength = 2.0f;
	[Export] private int _legSegmentCount = 4;
	
	[Export] private float _drag = 1.0f;
	
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
				RootOffset = new Vector3(1, 0, 0).Rotated(Vector3.Up, legYRotation),
				DesiredFootOffset = (new Vector3(3, -1, 0)).Rotated(Vector3.Up, legYRotation),
				SegmentCount = _legSegmentCount,
				SegmentLength = _legSegmentLength
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
		Velocity *= 1 - Mathf.Min((float)delta * _drag, 1);
		
		foreach (var leg in _legs)
		{
			leg.Update(delta);
			leg.Render(_legRenderer);
		}
	}
	
	public Tuple<Leg, Leg> GetAdjacentLegs(Leg leg)
	{
		int index = Array.IndexOf(_legs, leg);
		int leftIndex = (index - 1 + _legCount) % _legCount;
		int rightIndex = (index + 1) % _legCount;
		return new Tuple<Leg, Leg>(_legs[leftIndex], _legs[rightIndex]);
	}
}