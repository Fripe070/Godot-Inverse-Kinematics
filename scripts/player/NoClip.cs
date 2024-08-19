using Godot;
using Godot.Collections;

namespace Kinematics.scripts.player;

public partial class NoClip : Node
{
	[ExportGroup("References")]
	[Export] private Node3D _cameraTransform;
	[Export] private Player _playerController;
	[Export] private Node[] _colliders;

	[ExportGroup("")]  // Break out of group
	[Export] private float _noClipSpeed = 10; // m/s
	[Export] private float _noClipSprintScalar = 2;
	[Export] private bool _influenceVelocity = true;
	
	private ProcessModeEnum[] _colliderOldState;
	public bool IsNoClipping { get; private set; }
	
	public override void _Input(InputEvent @event)
	{
		if (!IsNoClipping) return;
		if (@event is not InputEventMouseButton { Pressed: true } mouseEvent) return;
		_noClipSpeed *= mouseEvent.ButtonIndex switch
		{
			MouseButton.WheelDown => 0.9f,
			MouseButton.WheelUp => 1.1f,
			_ => 1.0f
		};
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("noclip"))
			UpdateState(!IsNoClipping);
		if (!IsNoClipping) return;

		var inputDir = _playerController.GetInputDir();
		var inputDir3D = new Vector3(inputDir.X, 0, -inputDir.Y);
		inputDir3D.Y += Input.GetAxis("crouch", "jump");
		
		var movementVec = _cameraTransform.GlobalTransform.Basis * inputDir3D;
		movementVec *= _noClipSpeed;
		if (Input.IsActionPressed("sprint"))
			movementVec *= _noClipSprintScalar;

		_playerController.Position += movementVec * (float)delta;
		if (_influenceVelocity)
			_playerController.Velocity = movementVec;
	}

	private void UpdateState(bool shouldNoClip)
	{
		IsNoClipping = shouldNoClip;
		_playerController.IsDisabled = IsNoClipping;
		
		if (shouldNoClip)
		{
			_colliderOldState = new ProcessModeEnum[_colliders.Length];
			for (var i = 0; i < _colliders.Length; i++)
			{
				_colliderOldState[i] = _colliders[i].ProcessMode;
				_colliders[i].ProcessMode = ProcessModeEnum.Disabled;
			}
		}
		else
		{
			for (var i = 0; i < _colliders.Length; i++)
				_colliders[i].ProcessMode = _colliderOldState[i];
			_colliderOldState = null;
		}
	}
}