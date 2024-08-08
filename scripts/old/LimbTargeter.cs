using Godot;

namespace Kinematics.scripts.old;

public partial class LimbTargeter : RayCast3D
{
	[Export] private old.Limb[] _limbs;
	[Export] private float _speed = 5;
	
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is not InputEventMouseButton) return;
		var emb = (InputEventMouseButton)@event;
		if (!emb.IsPressed()) return;
		
		switch (emb.ButtonIndex)
		{
			case MouseButton.WheelUp:
				_speed += 1;
				break;
			case MouseButton.WheelDown:
				_speed -= 1;
				break;
		}
	}
	
	public override void _Process(double delta)
	{
		if (!Input.IsActionPressed("grapple")) return;

		Vector3 point;
		if (IsColliding()) 
			point = GetCollisionPoint();
		else
			point = GlobalPosition + (
				GlobalTransform.Basis.X * TargetPosition.X
				+ GlobalTransform.Basis.Y * TargetPosition.Y
				+ GlobalTransform.Basis.Z * TargetPosition.Z).Normalized() * 10f;
		
		DebugDraw3D.DrawSphere(point, 0.02f, new Color(0, 1, 0));
		
		foreach (var limb in _limbs)
			limb.Destination = SmoothMove(limb.Destination, point, _speed, (float)delta);
	}
	
	private static Vector3 SmoothMove(Vector3 from, Vector3 to, float speed, float dt)
	{
		return from.Lerp(to, 1 - Mathf.Exp(-speed * dt));
	}
}