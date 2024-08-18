using Godot;

namespace Kinematics.scripts.player;

public partial class LookController : Node
{
	[ExportGroup("Dependencies")]
	[Export] private Node3D _pitchPivot;
	[Export] private Node3D _yawPivot;
	
	[ExportGroup("Settings")] 
	[Export] private float _sensitivity = 1;
	[Export] private float _minPitch = -90;
	[Export] private float _maxPitch = 90;
	
	public override void _Ready()
	{
		Input.UseAccumulatedInput = false;
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		if (Input.MouseMode != Input.MouseModeEnum.Captured)
		{
			if (@event is InputEventMouseButton)
				Input.MouseMode = Input.MouseModeEnum.Captured;
			return;
		}
		if (@event.IsActionPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
			return;
		}
		if (@event is not InputEventMouseMotion)
			return;

		var viewportTransform = GetTree().Root.GetFinalTransform();
		// Cursed casting because C#. Should be type-safe!
		var motion = ((InputEventMouseMotion)@event.XformedBy(viewportTransform)).Relative;

		motion *= _sensitivity;
		motion /= 100;  // Just to get things into a manageable range
		
		_yawPivot.RotateObjectLocal(Vector3.Down, Mathf.DegToRad(motion.X));
		_pitchPivot.RotateObjectLocal(Vector3.Left, Mathf.DegToRad(motion.Y));

		var newPitchRot = _pitchPivot.Rotation;
		newPitchRot.X = Mathf.Clamp(newPitchRot.X, Mathf.DegToRad(_minPitch), Mathf.DegToRad(_maxPitch));
		_pitchPivot.Rotation = newPitchRot;
		
		_yawPivot.Orthonormalize();
		_pitchPivot.Orthonormalize();
	}
}