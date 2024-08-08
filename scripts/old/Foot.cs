using Godot;

namespace Kinematics.scripts.old;

public partial class Foot : Node3D
{
	[Export] private old.Limb _limb;
	[Export] private float _acceptableRadius = 2.0f;
	
	private RayCast3D _rayCast;
	public bool IsGrounded => _rayCast.IsColliding();
	
	
	public override void _Ready()
	{
		_limb.Destination = GlobalPosition;
		
		_rayCast = GetNode<RayCast3D>("RayCast3D");
	}

	public override void _Process(double delta)
	{
		DebugDraw3D.DrawSphere(GlobalPosition, _acceptableRadius, new Color(0.8f, 0.8f, 0.8f, 0.1f));

		if (_rayCast.IsColliding())
		{
			GlobalPosition = new Vector3(GlobalPosition.X, _rayCast.GetCollisionPoint().Y, GlobalPosition.Z);
		}
		
		// If foot is too far away
		if (GlobalPosition.DistanceTo(_limb.Destination) > _acceptableRadius)
		{
			_limb.Destination = GlobalPosition;
		}
	}
}