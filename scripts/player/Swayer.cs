using System;
using Godot;

namespace Kinematics.scripts.player;

public partial class Swayer : Node3D
{
	[Export] private Player _player;
	[Export] private bool _enableSway;
	[Export] private bool _enableBob;
	
	[Export] private float _stepAmplitude = 0.05f;
	[Export] private float _stepFrequency = 4f;
	[Export] private float _tiltDegPerMPerS = 5f / 15f;
	[Export] private float _tiltMaxDeg = 15f;
	
	[Signal] public delegate void DownStepEventHandler();

	private float _offset;
	private bool _oldCheck;

	public override void _Process(double delta)
	{
		if (_enableSway)
			Sway(delta);
		if (_enableBob)
			ViewBob(delta);
	}

	private void Sway(double delta)
	{
		var flatVel = _player.Velocity.WithY(0);
		
		var cBasis = -GetParent<Node3D>().GlobalBasis.Y;
		// var cBasis = -Basis.Y;
		var axis = flatVel.Normalized().Cross(cBasis).Normalized();
		
		if (axis == Vector3.Zero) return;
		
		float goalRot = flatVel.Length() * _tiltDegPerMPerS;
		float rotSpeed = 5;
		if (flatVel.Length() < 0.1) 
		{ goalRot = 0; rotSpeed = 15; }
		else if (_player.Velocity.Y < -flatVel.Length()) 
		{ goalRot = 0; rotSpeed = 5; }

		var savedRot = Rotation;
		Rotation = Vector3.Zero;
		GlobalRotate(axis, Mathf.DegToRad(Mathf.Min(_tiltMaxDeg, goalRot)));
		Rotation = savedRot.Lerp(Rotation, 1 - Mathf.Exp((float)-delta * rotSpeed));
	}
	
	private void ViewBob(double delta)
	{
		float xyLength = _player.Velocity.WithY(0).Length();
		
		if (xyLength < 0.02 || !_player.IsGrounded)
		{
			_offset = 0;
			Position = Vector3.Zero;
			return;
		}
		
		_offset += (float)delta * _stepFrequency * xyLength;
		_offset %= Mathf.Tau;
		Position = Vector3.Up * Mathf.Sin(_offset) * _stepAmplitude * Mathf.Min(1, xyLength);
		
		bool check = (_offset / MathF.Tau + 0.75f) % 1 >= 0.5;
		if (check && !_oldCheck)
			EmitSignal(SignalName.DownStep);
		_oldCheck = check;
	}
}