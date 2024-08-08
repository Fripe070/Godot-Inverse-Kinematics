using Godot;

namespace Kinematics.scripts;

public partial class LimbTargeter : RayCast3D
{
	[Export] private Limb[] _limbs;

	public override void _PhysicsProcess(double delta)
	{
		if (!IsColliding()) return;
		var point = GetCollisionPoint();
		DebugDraw3D.DrawSphere(point, 0.02f, new Color(0, 1, 0));
		foreach (var limb in _limbs)
			limb.Destination = point;
	}
}