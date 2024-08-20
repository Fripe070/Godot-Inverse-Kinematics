using System;
using Godot;

namespace Kinematics.scripts.player;

public partial class Swayer : Node3D
{
	[Export] private Player _player;
	[Export] private float _amplitude = 0.05f;
	[Export] private float _frequency = 2.4f;
	
	[Signal] public delegate void DownStepEventHandler();

	private float _offset = 0;
	private bool _oldCheck = false;

	public override void _Process(double delta)
	{
		float xyLength = _player.Velocity.WithY(0).Length();
		if (xyLength < 0.02 || !_player.IsGrounded)
		{
			_offset = 0;
			Position = Vector3.Zero;
			return;
		}
		
		_offset += (float)delta * _frequency * xyLength;
		_offset %= Mathf.Tau;
		Position = Vector3.Up * Mathf.Sin(_offset) * _amplitude * Mathf.Min(1, xyLength);
		
		bool check = (_offset / MathF.Tau + 0.75f) % 1 >= 0.5;
		if (check && !_oldCheck)
			EmitSignal(SignalName.DownStep);
		_oldCheck = check;
	}

	private void ResetOffset()
	{
		// Offset = 0;
	}
}