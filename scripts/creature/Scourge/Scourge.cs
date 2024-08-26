using System;
using Godot;
using Kinematics.scripts.player;

namespace Kinematics.scripts.creature.Scourge;

public partial class Scourge : CharacterBody3D
{
	[Export] private Player _player;
	private NavigationAgent3D _navigationAgent;
	
	[Export] private float _movementAccel = 2.0f;
	[Export] private float _maxSpeed = 2.0f;

	private Vector3 MovementTarget
	{
		get => _navigationAgent.TargetPosition;
		set => _navigationAgent.TargetPosition = value;
	}

	public override void _Ready()
	{
		_navigationAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

		_navigationAgent.PathDesiredDistance = 0.5f;
		_navigationAgent.TargetDesiredDistance = 0.5f;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Engine.GetPhysicsFrames() <= 1)
			return;
		
		MovementTarget = _player.GlobalPosition;
		if (_navigationAgent.IsNavigationFinished())
			return;

		var currentAgentPosition = GlobalTransform.Origin;
		var nextPathPosition = _navigationAgent.GetNextPathPosition();

		float scalar = 1f - Random.Shared.Next(-100, 100) / 100.0f;
		Velocity += currentAgentPosition.DirectionTo(nextPathPosition) * _movementAccel * (float)delta * scalar;
		if (Velocity.Length() > _maxSpeed)
			Velocity = Velocity.Normalized() * _maxSpeed;
		
		MoveAndSlide();
	}
}