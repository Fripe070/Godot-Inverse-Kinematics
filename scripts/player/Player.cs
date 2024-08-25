using System;
using Godot;

namespace Kinematics.scripts.player;

enum JumpMode
{
	Set,
	Add,
	Max,
	MaxAdd
}


public partial class Player : CharacterBody3D
{
	[ExportGroup("Jumping")]
	[Export] private JumpMode _jumpMode = JumpMode.Set;
	[Export] private float _jumpVelocity = 7 ;  // m/s
	[Export] private bool _allowHold = false;
	[Export] private float _jumpBufferTime = 0; // seconds

	[ExportGroup("Crouching")] 
	[Export] private float _crouchSpeedScale = 0.25f;
	
	[ExportGroup("Esoteric settings")] 
	// Most units are based on quake, going off of the assumption that 64 quake units is roughly 1.7 godot metres
	[Export] private float _groundStopSpeed = 2.5f;  // m/s // Minimum velocity used when calculating ground friction.
	[Export] private float _groundFriction = 6;  // m/s
	[Export] private float _airFriction = 0;  // m/s
	[Export] private float _groundAccelerationScalar = 10;  // m/s/s
	[Export] private float _airAccelerationScalar = 1;  // m/s/s
	[Export] private float _moveSpeed = 3.4f;  // m/s
	[Export] private Vector4 _moveDirScalar = new(1, 1, 1, 1);  // Front, back, left, right
	[Export] private bool _useGodotGravity = false;
	[Export] private float _gravityStrength = 21.25f;
	[Export] private float _overBounce = 1.001f;
	[Export] private float _rampSlideThreshold = 4.8f;  // m/s upwards
	[Export] private bool _shouldSurf = false; // Allow for source-like surfing. Maybe not completely accurate since the rest is based on q3, but close enough?
	
	private float _godotGravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	public float Gravity => _useGodotGravity ? _godotGravity : _gravityStrength;
	
	// State
	public bool IsGrounded { get; private set; }
	public bool IsCrouched { get; private set; }
	private double _triedJumpAgo = 0;
	public bool IsDisabled = false;  // TODO: I should probably use a state machine and make this private...
	
	[Signal] public delegate void JumpedEventHandler();
	[Signal] public delegate void LandedEventHandler();

	// TODO: Stairs :skull:
	public override void _PhysicsProcess(double delta)
	{
		// DebugDraw2D.SetText("Velocity (var)", Velocity);
		// DebugDraw2D.SetText("Velocity (Y)", Velocity.Y);
		if (IsDisabled) return;
		
		if (Input.IsActionPressed("jump") || (_allowHold && Input.IsActionPressed("jump")))
			_triedJumpAgo = 0;
		else
			_triedJumpAgo += delta;
		
		GroundedCheck();
		if (IsGrounded)
			GroundMove(delta);
		else
			AirMove(delta);
		
		// DebugDraw3D.DrawArrow(GlobalPosition, GlobalPosition + Velocity, Colors.Aqua, 0.1f);
	}

	private void AirMove(double delta)
	{
		DebugDraw2D.SetText("Mode", "Air");
		ApplyFriction(delta);

		var forwardVec = -Transform.Basis.Z.WithY(0).Normalized();
		var rightVec = Transform.Basis.X.WithY(0).Normalized();
		var input2D = GetInputDir();
		// wishVel.normalised() = wishdir
		// wishVel.Length() = wishspeed
		var wishVel = forwardVec * input2D.Y + rightVec * input2D.X;
		wishVel = wishVel.WithY(0);  // I don't think this ever does anything? But in the spirit of being true to... source, I'm keeping it here
		
		Accelerate(wishVel, _airAccelerationScalar, delta);
		
		// TODO: Write rest
		SlideMove(delta, true);
	}

	private void GroundMove(double delta)
	{
		if (JumpIfCan())
		{
			AirMove(delta);
			return;
		}
		DebugDraw2D.SetText("Mode", "Ground");
		
		ApplyFriction(delta);

		var forwardVec = -Transform.Basis.Z.WithY(0);
		var rightVec = Transform.Basis.X.WithY(0);
		forwardVec = ClipVelocity(forwardVec, GetFloorNormal(), _overBounce).Normalized();
		rightVec = ClipVelocity(rightVec, GetFloorNormal(), _overBounce).Normalized();
		
		var input2D = GetInputDir();
		// wishVel.normalised() = wishdir
		// wishVel.Length() = wishspeed
		var wishVel = forwardVec * input2D.Y + rightVec * input2D.X;
		float speed = wishVel.Length();
		
		if (IsCrouched)
			speed = Mathf.Min(Velocity.Length() * _crouchSpeedScale, speed);

		Accelerate(wishVel.Normalized() * speed, _groundAccelerationScalar, delta);
		// Clip without losing speed
		Velocity = ClipVelocity(Velocity, GetFloorNormal(), _overBounce).Normalized() * Velocity.Length();
		
		// TODO: Write rest
		SlideMove(delta, false);
	}

	private void Accelerate(Vector3 wishVel, float accel, double delta)
	{
		var wishDir = wishVel.Normalized();
		float wishSpeed = wishVel.Length();
		
		float projectedSpeed = Velocity.Dot(wishDir);  // this vector is normalised, what happens if it isn't?
		float addSpeed = wishSpeed - projectedSpeed;
		if (addSpeed <= 0) return;
		
		float accelSpeed = accel * wishSpeed * (float)delta;
		accelSpeed = Mathf.Min(accelSpeed, addSpeed);
		
		Velocity += wishDir * accelSpeed;
	}

	private void ApplyFriction(double delta)
	{
		var flatVel = Velocity;
		if (IsGrounded)
			flatVel.Y = 0;
		
		float speed = flatVel.Length();
		if (speed < 0.01f)
		{
			Velocity = new Vector3(0, Velocity.Y, 0);
			return;
		}

		float velDrop = 0;
		if (IsGrounded)
			velDrop += Mathf.Max(_groundStopSpeed, speed) * _groundFriction * (float)delta;
		else 
			velDrop += speed * _airFriction * (float)delta;

		float speedScalar = Mathf.Max(0, speed - velDrop) / speed;  // 0/0 can't happen due to our clamp from earlier
		Velocity *= speedScalar;
	}

	private bool JumpIfCan()
	{
		if (!IsGrounded || _triedJumpAgo > _jumpBufferTime) return false;

		Velocity = _jumpMode switch
		{
			JumpMode.Set => new Vector3(Velocity.X, _jumpVelocity, Velocity.Z),
			JumpMode.Add => new Vector3(Velocity.X, Velocity.Y + _jumpVelocity, Velocity.Z),
			JumpMode.Max => new Vector3(Velocity.X, Mathf.Max(Velocity.Y, _jumpVelocity), Velocity.Z),
			JumpMode.MaxAdd => new Vector3(Velocity.X, Mathf.Max(0, Velocity.Y) + _jumpVelocity, Velocity.Z),
			_ => throw new ArgumentOutOfRangeException()
		};

		IsGrounded = false;
		EmitSignal(SignalName.Jumped);
		return true;
	}

	private void GroundedCheck()
	{
		// This method will also be used for detecting the material we're walking on and whatever later
		bool newGrounded = IsOnFloor();
		if (Velocity.Y > _rampSlideThreshold)
		{
			newGrounded = false;
		}
		
		if (!IsGrounded && newGrounded)
			EmitSignal(SignalName.Landed);
		IsGrounded = newGrounded;
	}

	private void SlideMove(double delta, bool applyGravity)
	{
		if (applyGravity)
		{
			Velocity += Vector3.Down * Gravity * (float)delta;
			
			if (IsGrounded) 
				Velocity = ClipVelocity(Velocity, GetFloorNormal(), _overBounce);
			else if (IsOnWall() && _shouldSurf)
				Velocity = ClipVelocity(Velocity, GetWallNormal(), _overBounce);
		}
		
		MoveAndSlide();
	}

	public Vector2 GetInputDir()
	{
		var dir = Input.GetVector("move_left", "move_right", "move_backward", "move_forward");
		dir *= _moveSpeed;
		if (_moveDirScalar.Equals(new Vector4(1, 1, 1, 1))) 
			return dir;
		
		if (dir.Y < 0)
			dir.Y *= _moveDirScalar.Y;  // Backwards
		else
			dir.Y *= _moveDirScalar.X;  // Forwards
		if (dir.X < 0)
			dir.X *= _moveDirScalar.Z;  // Left
		else
			dir.X *= _moveDirScalar.W;  // Right
		return dir;
	}

	private Vector3 ClipVelocity(Vector3 vec, Vector3 normal, float overBounce)
	{
		// Supposedly meant ot help wit sliding off of the surface you're standing on?
		float backoff = vec.Dot(normal);
		if (backoff < 0)
			backoff *= overBounce;
		else
			backoff /= overBounce;
		return vec - normal * backoff;
	}
}