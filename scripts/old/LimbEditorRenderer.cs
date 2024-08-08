using Godot;

namespace Kinematics.scripts.old;

[Tool]
public partial class LimbEditorRenderer : Node3D
{
	public override void _Process(double delta)
	{
		if (!Engine.IsEditorHint()) return;
		
		var forward = GlobalTransform.Basis.Z;
		var pos = GlobalTransform.Origin;
		
		DebugDraw3D.DrawSphere(pos, 0.1f, new Color(1, 1, 1));
		DebugDraw3D.DrawArrow(pos, pos + forward, new Color(1, 1, 1), 0.1f);
	}
}