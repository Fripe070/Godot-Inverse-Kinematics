using Godot;

namespace Kinematics.scripts.player;

public partial class GrappleGun : RayCast3D
{
	[Export] private Player _player;
	[Export] private Node3D _cameraTransform;
	private bool _prevPlayerDisabledState;
	
	private GrappleHook _hook;
	
	public override void _Process(double delta)
	{
		if (_hook == null)
		{
			if (!Input.IsActionJustPressed("shoot")) return;
			_prevPlayerDisabledState = _player.IsDisabled;
			_player.IsDisabled = true;
			FIreHook();
		}
		else if (DetachCheck())
		{
			_player.IsDisabled = _prevPlayerDisabledState;
			_hook.QueueFree();
			_hook = null;
		}
	}

	private void FIreHook()
	{
		var hitLocGlobal = IsColliding() ? GetCollisionPoint() : ToGlobal(Position + TargetPosition);
		var hookScene = GD.Load<PackedScene>("res://scenes/GrappleHook.tscn");
		
		_hook = hookScene.Instantiate<GrappleHook>().WIthData(_player, _player.GlobalPosition.DistanceTo(hitLocGlobal), _cameraTransform);
		GetTree().Root.AddChild(_hook);
		_hook.GlobalPosition = hitLocGlobal;
	}

	private bool DetachCheck()
	{
		if (Input.IsActionJustPressed("shoot")) return true;
		if (Input.IsActionJustPressed("jump")) return true;
		
		var pVel = _player.Velocity;
		float dot = Vector3.Up.Dot(pVel.Normalized());
		float relativeY = (_player.GlobalPosition.Y - _hook.GlobalPosition.Y) / _hook.PlayerDist;
		
		return dot > 0.1f && relativeY > -0.3;
	}
}