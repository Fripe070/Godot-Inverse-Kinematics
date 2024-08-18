using System;
using System.Linq;
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
	public float YawRotationVelocity { get; private set; }

	public Vector3 GlobalMovementTarget;
	public Vector3 LocalMovementTarget => ToLocal(GlobalMovementTarget);
	
	public override void _Ready()
	{
		GlobalMovementTarget = GlobalPosition;
		
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
		float yawToTarget = -GlobalPosition
			.DirectionTo(GlobalMovementTarget).WithY(0)
			.SignedAngleTo(GlobalBasis.Z.WithY(0), Vector3.Up);
		// I know this is a broken formulae, but I have not figured out gow to make it not overshoot otherwise
		YawRotationVelocity = Mathf.Sign(yawToTarget) * Mathf.Min(RotationAcceleration, Mathf.Abs(yawToTarget));
		RotateY(YawRotationVelocity * (float)delta);
		
		if (IsSignificantlyRotating)
			IsSignificantlyRotating = Mathf.Abs(YawRotationVelocity) >= _majorRotationStopThreshold;
		else
			IsSignificantlyRotating = Mathf.Abs(YawRotationVelocity) >= _majorRotationThreshold;
		
		var moveDir = LocalMovementTarget.WithY(0);
		Velocity = moveDir.Normalized() * Mathf.Min(_acceleration, moveDir.Length());
		if (IsSignificantlyRotating)
			Velocity = Vector3.Zero;
		
		Translate(Velocity * (float)delta);
		IsMoving = Velocity.Length() >= _stationaryVelThreshold || IsSignificantlyRotating;
		
		// DebugDraw3D.DrawArrow(GlobalPosition, ToGlobal(Velocity), new Color(1, 1, 0), 0.1f);
		// DebugDraw3D.DrawArrow(Position, Position + ForwardVec.Rotated(Vector3.Up, yawToTarget), new Color(0, 0, 1), 0.1f);
		// DebugDraw3D.DrawArrow(Position, Position + ForwardVec, IsSignificantlyRotating ? new Color(1, 0, 1) : new Color(0.8f, 0, 0.7f) , 0.1f);
		// DebugDraw3D.DrawArrow(Position + ForwardVec, Position + ForwardVec + ForwardVec.Rotated(Vector3.Up, Mathf.DegToRad(90))*YawRotationVelocity, new Color(0.4f, 0.4f, 0.4f), 0.1f);
		
		for (var i = 0; i < _legCount; i++)
		{
			_legs[i].Update(delta);
			_legs[i].Render(_legRenderers[i]);
			// _legs[i].Render(new DebugRenderer());
		}
		
		// float averageLegY = _legs.Average(leg => leg.FootPositionGlobal.Y);
		// GlobalPosition = new Vector3(GlobalPosition.X, averageLegY + 1f, GlobalPosition.Z);
	}
	
	public Tuple<Leg, Leg> GetAdjacentLegs(Leg leg)
	{
		int index = Array.IndexOf(_legs, leg);
		int leftIndex = (index - 1 + _legCount) % _legCount;
		int rightIndex = (index + 1) % _legCount;
		return new Tuple<Leg, Leg>(_legs[leftIndex], _legs[rightIndex]);
	}
}