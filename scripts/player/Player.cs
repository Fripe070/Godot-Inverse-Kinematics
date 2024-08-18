using Godot;

namespace Kinematics.scripts.player;

public partial class Player : CharacterBody3D
{
	private float _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

	public override void _PhysicsProcess(double delta)
	{
	}
}