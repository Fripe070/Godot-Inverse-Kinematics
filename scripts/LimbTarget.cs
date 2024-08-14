using Godot;
using System;
using Kinematics.scripts.old;

public partial class LimbTarget : Node3D
{
	[Export] private float _speed = 15.0f;
	[Export] private Limb[] _limbs = Array.Empty<Limb>();

	public override void _Process(double delta)
	{
		foreach (var limb in _limbs)
			limb.Destination += limb.Destination.DirectionTo(GlobalPosition) * Mathf.Min(_speed * (float)delta, limb.Destination.DistanceTo(GlobalPosition));
	}
}
