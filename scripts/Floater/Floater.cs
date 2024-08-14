using Godot;
using System;

public partial class Floater : CharacterBody3D
{
	[Export] private float _minHeight = 5.0f;
	[Export] private float _maxHeight = 7.0f;
	
	
	private const float Speed = 10.0f;
	private const float FlyUpSpeed = 1.0f;
	private const float GravityMultiplier = 0.5f;

	private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
	private float Gravity => _gravity * GravityMultiplier;

	private RayCast3D _minHeightRayCast;
	private RayCast3D _maxHeightRayCast;
	
	public override void _Ready()
	{
		_minHeightRayCast = new RayCast3D();
		_minHeightRayCast.TargetPosition = new Vector3(0, -_minHeight, 0);
		AddChild(_minHeightRayCast);
		_maxHeightRayCast = new RayCast3D();
		_maxHeightRayCast.TargetPosition = new Vector3(0, -_maxHeight, 0);
		AddChild(_maxHeightRayCast);
	}

	public override void _PhysicsProcess(double delta)
	{
		bool tooLow = _minHeightRayCast.IsColliding();
		bool tooHigh = !_maxHeightRayCast.IsColliding();
		
		var velocity = Velocity;
		
		if (tooLow)
			velocity.Y += FlyUpSpeed * (float)delta;
		else if (tooHigh)
			velocity.Y -= FlyUpSpeed * (float)delta;
		else
		{
			var randomVertical = (float)GD.RandRange(-1.0f, 1.0f);
			velocity.Y += randomVertical * FlyUpSpeed * (float)delta;
		}

		var randomVector = new Vector3(GD.Randf(), 0.5f, GD.Randf()) * 2 - Vector3.One;
		randomVector = randomVector.Normalized() * Speed;
		
		velocity += randomVector.Normalized() * Speed * (float)delta;
		
		Velocity = velocity;
		MoveAndSlide();
		
		DrawDebug();
	}

	private void DrawDebug()
	{
		DebugDraw3D.DrawArrow(GlobalPosition, GlobalPosition + _maxHeightRayCast.TargetPosition, _maxHeightRayCast.IsColliding() ? Colors.Green : Colors.Red, 0.1f);
		DebugDraw3D.DrawArrow(GlobalPosition, GlobalPosition + _minHeightRayCast.TargetPosition, _minHeightRayCast.IsColliding() ? Colors.Green : Colors.Red, 0.1f);
	}
}
