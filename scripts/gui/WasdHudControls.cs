using Godot;
using System;
using System.Collections.Generic;

public partial class WasdHudControls : VBoxContainer
{
	[Export] private Color _pressedColor = Colors.Cyan;
	[Export] private Color _releasedColor = Colors.DimGray;

	[Export] private Color _physicsPressedColor = Colors.Cyan.Lerp(Colors.DimGray, 0.85f);
	[Export] private Color _physicsReleasedColor = Colors.Black;
	
	[ExportGroup("Keys")]
	[Export] private Label _wLabel;
	[Export] private Label _aLabel;
	[Export] private Label _sLabel;
	[Export] private Label _dLabel;
	[Export] private Label _spaceLabel;

	[Export] private Label _leftClickLabel;
	[Export] private Label _rightClickLabel;
	

	private Label[] _labels;
	private Func<bool>[] _labelStates;
	
	public override void _Ready()
	{
		_labels = new Label[7];
		_labelStates = new Func<bool>[_labels.Length];
		
		_labels[0] = _wLabel;
		_labelStates[0] = () => Input.IsActionPressed("move_forward");
		_labels[1] = _aLabel;
		_labelStates[1] = () => Input.IsActionPressed("move_left");
		_labels[2] = _sLabel;
		_labelStates[2] = () => Input.IsActionPressed("move_backward");
		_labels[3] = _dLabel;
		_labelStates[3] = () => Input.IsActionPressed("move_right");
		_labels[4] = _spaceLabel;
		_labelStates[4] = () => Input.IsActionPressed("jump");
		_labels[5] = _leftClickLabel;
		_labelStates[5] = () => Input.IsMouseButtonPressed(MouseButton.Left);
		_labels[6] = _rightClickLabel;
		_labelStates[6] = () => Input.IsMouseButtonPressed(MouseButton.Right);
		
		foreach (var label in _labels)
		{
			label.LabelSettings = (LabelSettings)label.LabelSettings.Duplicate();
		}
	}

	public override void _Process(double delta)
	{
		GeneralProcess();
	}

	public override void _PhysicsProcess(double delta)
	{
		GeneralProcess();
	}

	private void GeneralProcess()
	{
		for (var i = 0; i < _labels.Length; i++)
		{
			bool state = _labelStates[i]();
			var label = _labels[i];
			label.LabelSettings.FontColor = state ? _pressedColor : _releasedColor;
			if (Engine.IsInPhysicsFrame())
				label.GetParent<ColorRect>().Color = state ? _physicsPressedColor : _physicsReleasedColor;
		}
	}
}
