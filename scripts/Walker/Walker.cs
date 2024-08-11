using System;
using Godot;
using Kinematics.scripts.Render;

namespace Kinematics.scripts.Walker;

public partial class Walker : Node3D
{
	[Export] private CsgPolygon3D _legMesh;
	
	[Export] private int _legCount = 4;
	[Export] private float _legSegmentLength = 1.0f;
	[Export] private int _legSegmentCount = 4;
	[Export] private float _acceleration = 5.0f;
	[Export] private float _rotationAccelerationDeg = 90;
	private float RotationAcceleration => Mathf.DegToRad(_rotationAccelerationDeg);
	
	private float _drag = 10.0f;
	private float _stationaryVelThreshold = 0.05f;
	private float _majorRotationThreshold = Mathf.DegToRad(40);
	private float _majorRotationStopThreshold = Mathf.DegToRad(20);
	
	public bool IsMoving { get; private set; }
	public bool IsSignificantlyRotating { get; private set; }

	private Leg[] _legs;
	private PathRenderer[] _legRenderers;
	
	public Vector3 Velocity;
	public float YawRotationVelocity;
	public Vector3 MovementTarget { get; set; }
	
	private Vector3 ForwardVec => -GlobalTransform.Basis.Z;
	
	public override void _Ready()
	{
		MovementTarget = GlobalPosition;
		_legs = new Leg[_legCount];
		_legRenderers = new PathRenderer[_legCount];

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
			
			_legRenderers[i] = new PathRenderer(_legMesh);
			_legRenderers[i].Name = $"Leg Renderer #{i}";
			AddChild(_legRenderers[i]);
		}
	}

	public override void _Process(double delta)
	{
		float yawToTarget = -GlobalTransform.Origin.DirectionTo(MovementTarget).WithY(0).SignedAngleTo(ForwardVec.WithY(0), Vector3.Up);
		DebugDraw2D.SetText("YawToTarget", Mathf.RadToDeg(yawToTarget));
		DebugDraw3D.DrawArrow(GlobalTransform.Origin, GlobalTransform.Origin + ForwardVec.Rotated(Vector3.Up, yawToTarget), new Color(0, 0, 1), 0.1f);
		
		YawRotationVelocity = Mathf.Min(yawToTarget, RotationAcceleration);
		
		RotateY(YawRotationVelocity * (float)delta);
		
		if (IsSignificantlyRotating)
			IsSignificantlyRotating = Mathf.Abs(YawRotationVelocity) >= _majorRotationStopThreshold;
		else
			IsSignificantlyRotating = Mathf.Abs(YawRotationVelocity) >= _majorRotationThreshold;
		
		
		DebugDraw3D.DrawArrow(GlobalTransform.Origin, GlobalTransform.Origin + ForwardVec, IsSignificantlyRotating ? new Color(1, 0, 1) : new Color(0.8f, 0, 0.1f) , 0.1f);

		var moveDir = (MovementTarget - GlobalTransform.Origin).WithY(0);
		moveDir = moveDir.Normalized() * Mathf.Min(moveDir.Length(), 1);  // Slow down when we start reaching the target (within 1 unit in this case)
	
		Velocity = moveDir * _acceleration;
		if (IsSignificantlyRotating)
			Velocity = Vector3.Zero;
		GlobalTranslate(Velocity * (float)delta);
		
		IsMoving = Velocity.Length() >= _stationaryVelThreshold || IsSignificantlyRotating;
		
		for (var i = 0; i < _legCount; i++)
		{
			_legs[i].Update(delta);
			_legs[i].Render(_legRenderers[i]);
			_legs[i].Render(new DebugRenderer());
		}
		// DebugDraw3D.DrawArrow(GlobalTransform.Origin, GlobalTransform.Origin + Velocity, new Color(0, 1, 0), 0.1f);
	}
	
	public Tuple<Leg, Leg> GetAdjacentLegs(Leg leg)
	{
		int index = Array.IndexOf(_legs, leg);
		int leftIndex = (index - 1 + _legCount) % _legCount;
		int rightIndex = (index + 1) % _legCount;
		return new Tuple<Leg, Leg>(_legs[leftIndex], _legs[rightIndex]);
	}
}