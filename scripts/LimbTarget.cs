using System;
using Godot;
using Kinematics.scripts.old;

namespace Kinematics.scripts;

public partial class LimbTarget : Node3D
{
	[Export] private float _speed = 15.0f;
	[Export] private Floater.Floater _floater;

	public override void _Process(double delta)
	{
		_floater.Destination = GlobalPosition;
	}
}