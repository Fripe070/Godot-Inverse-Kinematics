using System;
using System.Collections.Generic;
using Godot;

namespace Kinematics.scripts.Floater;

public partial class Floater : RigidBody3D
{
	[Export] private float _innerRayCastLength = 5.0f;

	public Vector3 Destination;
	
	private const float Speed = 10.0f;
	private const float FlyUpSpeed = 5.0f;
	private const float GravityMultiplier = 0.5f;

	private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	private float Gravity => _gravity * GravityMultiplier;

	private List<RayCast3D> _innerRayCasts = new();
	private float _radius = 1.0f;

	private static Vector3[] GetDirections()
	{
		const int middleIndex = 3*3*3 / 2;
		var directions = new Vector3[26];
		for (var i = 0; i < 27; i++)
		{
			if (i == middleIndex)
				continue;
			// ReSharper disable once PossibleLossOfFraction
			directions[i < middleIndex ? i : i - 1] = new Vector3(i % 3 - 1, i / 3 % 3 - 1, i / 9 % 3 - 1);
		}
		return directions;
	}
	
	public override void _Ready()
	{
		var directions = GetDirections();
		foreach (var direction in directions)
		{
			var rayCast = new RayCast3D
			{
				TargetPosition = direction.Normalized() * _innerRayCastLength,
			};
			AddChild(rayCast);
			_innerRayCasts.Add(rayCast);
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		var totalForce = Vector3.Zero;
		int castsHit = 0;
		foreach (var rayCast in _innerRayCasts)
		{
			// DebugDraw3D.DrawArrow(GlobalPosition, GlobalPosition + rayCast.TargetPosition, rayCast.IsColliding() ? new Color(1, 0, 0) : new Color(0, 1, 0), 0.1f);
			if (!rayCast.IsColliding())
				continue;
			
			var dir = rayCast.GetCollisionPoint().DirectionTo(GlobalPosition);
			float dist = rayCast.GetCollisionPoint().DistanceTo(GlobalPosition) / _innerRayCastLength - _radius;
			var addition = dir * (1 - dist);
			DebugDraw2D.SetText("addition", addition.Length());
			totalForce += addition;
			castsHit++;
		}
		if (castsHit != 0)
			totalForce /= castsHit;
		
		DebugDraw2D.SetText("Force", totalForce.Length());
		DebugDraw3D.DrawArrow(GlobalPosition, GlobalPosition + totalForce * FlyUpSpeed, new Color(1, 0, 0), 0.1f);
		
		// ApplyForce(totalForce * FlyUpSpeed, Vector3.Zero);

		
		// var dirToGoal = GlobalPosition.DirectionTo(Destination);
		// ApplyForce(dirToGoal * Speed, Vector3.Zero);
		
		
		DebugDraw3D.DrawArrow(GlobalPosition, GlobalPosition + LinearVelocity, new Color(0, 0, 1), 0.1f);
	}
}