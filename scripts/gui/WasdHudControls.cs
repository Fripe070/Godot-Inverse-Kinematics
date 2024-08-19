using Godot;
using System;

public partial class WasdHudControls : VBoxContainer
{
	[Export] private Label _wLabel;
	[Export] private Label _aLabel;
	[Export] private Label _sLabel;
	[Export] private Label _dLabel;
	[Export] private Label _spaceLabel;

	[Export] private Label _leftClickLabel;
	[Export] private Label _rightClickLabel;
	
	public override void _Ready()
	{
		_wLabel.LabelSettings = (LabelSettings)_wLabel.LabelSettings.Duplicate();
		_aLabel.LabelSettings = (LabelSettings)_aLabel.LabelSettings.Duplicate();
		_sLabel.LabelSettings = (LabelSettings)_sLabel.LabelSettings.Duplicate();
		_dLabel.LabelSettings = (LabelSettings)_dLabel.LabelSettings.Duplicate();
		_spaceLabel.LabelSettings = (LabelSettings)_spaceLabel.LabelSettings.Duplicate();
		_leftClickLabel.LabelSettings = (LabelSettings)_leftClickLabel.LabelSettings.Duplicate();
		_rightClickLabel.LabelSettings = (LabelSettings)_rightClickLabel.LabelSettings.Duplicate();
	}

	public override void _Process(double delta)
	{
		Handle(_wLabel, Input.IsActionPressed("move_forward"));
		Handle(_aLabel, Input.IsActionPressed("move_left"));
		Handle(_sLabel, Input.IsActionPressed("move_backward"));
		Handle(_dLabel, Input.IsActionPressed("move_right"));
		Handle(_spaceLabel, Input.IsActionPressed("jump"));
		
		Handle(_leftClickLabel, Input.IsMouseButtonPressed(MouseButton.Left));
		Handle(_rightClickLabel, Input.IsMouseButtonPressed(MouseButton.Right));
	}

	private void Handle(Label label, bool state)
	{
		label.LabelSettings.FontColor = state ? Colors.Aqua : Colors.DarkGray;
	}
}
