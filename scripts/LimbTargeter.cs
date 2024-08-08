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
			limb.Destination = SmoothMove(limb.Destination, point, 5, (float)delta);
	}
	
	private static Vector3 SmoothMove(Vector3 from, Vector3 to, float speed, float dt)
	{
		return from.Lerp(to, 1 - Mathf.Exp(-speed * dt));
	}
}