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

	private Vector3 _lastPoint;
	
	public override void _Process(double delta)
	{
		foreach (var limb in _limbs)
		{
			limb.Destination = SmoothMove(limb.Destination, _lastPoint, _speed, (float)delta);
			DebugDraw3D.DrawSphere(limb.Destination, 0.02f, new Color(1, 0, 0));
			if (!(_lastPoint.DistanceSquaredTo(limb.Destination) > 0.01f)) continue;
			DebugDraw3D.DrawLine(_lastPoint, limb.Destination, new Color(0.5f, 0.5f, 0.5f));
			DebugDraw3D.DrawSphere(_lastPoint, 0.02f, new Color(0, 1, 0));
		}
		if (!Input.IsActionPressed("grapple")) return;
		if (IsColliding())
			_lastPoint = GetCollisionPoint();
		else
			_lastPoint = GlobalPosition + (
				GlobalTransform.Basis.X * TargetPosition.X
				+ GlobalTransform.Basis.Y * TargetPosition.Y
				+ GlobalTransform.Basis.Z * TargetPosition.Z).Normalized() * 10f;
	}
	
	private static Vector3 SmoothMove(Vector3 from, Vector3 to, float speed, float dt)
	{
		return from.Lerp(to, 1 - Mathf.Exp(-speed * dt));
	}
}