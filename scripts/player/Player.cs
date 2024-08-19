using System;
using Godot;

namespace Kinematics.scripts.player;

public partial class Player : CharacterBody3D
{
	[ExportGroup("Jumping")]
	[Export] private float _jumpVelocity;  // m/s
	[Export] private bool _allowHold = false;
	[Export] private float _jumpBufferTime = 0; // seconds

	[ExportGroup("Crouching")] 
	[Export] private bool _quakeCrouch = true;
	[Export] private float _quakeCrouchScale = 0.25f;
	
	[ExportGroup("Esoteric constants")] 
	// Most units are based on quake, going off of the assumption that 64 quake units is roughly 1.7 godot metres
	[Export] private float _groundStopSpeed = 2.5f;  // m/s // Minimum velocity used when calculating ground friction.
	[Export] private float _groundFriction = 6;  // m/s
	[Export] private float _airFriction = 0;  // m/s
	[Export] private float _groundAcceleration = 380;  // m/s/s
	[Export] private float _airAcceleration = 40;  // m/s/s
	[Export] private Vector4 _moveDirScalar = new(1, 1, 1, 1);  // Front, back, left, right
	
	private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	
	// State
	public bool IsGrounded { get; private set; }
	public bool IsCrouched { get; private set; }
	private double _triedJumpAgo = 0;
	
	[Signal] public delegate void JumpedEventHandler();
	
	public override void _PhysicsProcess(double delta)
	{
		if (Input.IsActionPressed("jump") || (_allowHold && Input.IsActionPressed("jump")))
			_triedJumpAgo = 0;
		else
			_triedJumpAgo += delta;
		
		GroundedCheck();
		// if (IsGrounded)
		// 	GroundMove(delta);
		// else
		// 	AirMove(delta);
		GroundMove(delta);
		DebugDraw2D.SetText("IsGrounded", IsGrounded);
		DebugDraw3D.DrawArrow(GlobalPosition, GlobalPosition + Velocity, Colors.Aqua, 0.1f);
	}

	private void AirMove(double delta)
	{
		ApplyFriction(delta);

		var inputDir = TransformInput(GetInputDir());
		Accelerate(inputDir, _groundAcceleration, delta);
		// TODO: Write rest
		StepSlideMove(delta);
	}

	private void GroundMove(double delta)
	{
		if (JumpIfCan())
		{
			AirMove(delta);
			return;
		}
		
		ApplyFriction(delta);

		// normalised = wishdir, length = wishspeed
		var inputDir = TransformInput(GetInputDir());
		float speed = inputDir.Length();
		
		if (_quakeCrouch && IsCrouched)
			speed = Mathf.Min(Velocity.Length() * _quakeCrouchScale, speed);

		Accelerate(inputDir.Normalized() * speed, _groundAcceleration, delta);
		// TODO: Write rest
		StepSlideMove(delta);
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

	private void StepSlideMove(double delta)
	{
		var newVel = Velocity;
		newVel.Y -= _gravity * (float)delta;
		Velocity = newVel;
		MoveAndSlide();
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

		Velocity = new Vector3(Velocity.X, _jumpVelocity, Velocity.Z);
		IsGrounded = false;
		EmitSignal(SignalName.Jumped);
		return true;
	}

	private void GroundedCheck()
	{
		// This method will also be used for detecting the material we're walking on and whatever later
		IsGrounded = IsOnFloor();
	}

	private Vector2 GetInputDir()
	{
		var dir = Input.GetVector("move_left", "move_right", "move_backward", "move_forward");
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

	private Vector3 TransformInput(Vector2 inputDir)
	{
		var forwardVec = -Transform.Basis.Z;
		var rightVec = Transform.Basis.X;
		forwardVec.Y = rightVec.Y = 0;
		if (IsGrounded)
		{
			forwardVec = ClipVelocity(forwardVec, GetFloorNormal(), 1.001f);
			rightVec = ClipVelocity(rightVec, GetFloorNormal(), 1.001f);
		}
		forwardVec = forwardVec.Normalized();
		rightVec = rightVec.Normalized();
		
		var vec = forwardVec * inputDir.Y + rightVec * inputDir.X;
		return vec * 10;  // TODO: WHy is the player so slow if I dont add this? - I want movement values to be accurately scaled to their quake counterparts, and 10 is arbitrary here
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