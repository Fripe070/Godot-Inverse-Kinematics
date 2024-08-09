using System;
using Godot;
using Kinematics.scripts.Render;

namespace Kinematics.scripts.Walker;

public partial class Walker : Node3D
{
	[Export] private int _legCount = 4;
	[Export] private float _legSegmentLength = 2.0f;
	[Export] private int _legSegmentCount = 4;
	[Export] private float _acceleration = 1.0f;
	[Export] private float _rotationAcceleration = Mathf.DegToRad(30);
	
	private float _drag = 10.0f;
	private float _stationaryVelThreshold = 0.05f;
	private float _stationaryRotThreshold = Mathf.DegToRad(10);
	
	public bool IsMoving { get; private set; }
	public bool IsRotating { get; private set; }

	private Leg[] _legs;
	public Vector3 Velocity;
	public float YawRotationVelocity;
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
		YawRotationVelocity = Mathf.DegToRad(11);
		RotateY(YawRotationVelocity * (float)delta);
		IsRotating = YawRotationVelocity >= _stationaryRotThreshold;
		DebugDraw3D.DrawArrow(GlobalTransform.Origin, GlobalTransform.Origin + GlobalBasis.Z, IsRotating ? new Color(1, 0, 1) : new Color(0.8f, 0, 0.1f) , 0.1f);
		
		var moveDir = MovementTarget - GlobalTransform.Origin;
		moveDir.Y = 0;
		moveDir = moveDir.Normalized() * Mathf.Min(moveDir.Length(), 1);  // Slow down when we start reaching the target (within 1 unit in this case)
		if (!IsRotating)
		{
			Velocity = moveDir * _acceleration; // Should I not be multiplying with delta here????? But that acts __weird__  :sob:
			GlobalTranslate(Velocity * (float)delta);
		}
		IsMoving = Velocity.Length() >= _stationaryVelThreshold || IsRotating;
		
		foreach (var leg in _legs)
		{
			leg.Update(delta);
			leg.Render(_legRenderer);
		}
		DebugDraw3D.DrawArrow(GlobalTransform.Origin, GlobalTransform.Origin + Velocity, new Color(0, 1, 0), 0.1f);
	}
	
	public Tuple<Leg, Leg> GetAdjacentLegs(Leg leg)
	{
		int index = Array.IndexOf(_legs, leg);
		int leftIndex = (index - 1 + _legCount) % _legCount;
		int rightIndex = (index + 1) % _legCount;
		return new Tuple<Leg, Leg>(_legs[leftIndex], _legs[rightIndex]);
	}
}