using System.Linq;
using Godot;

namespace Kinematics.scripts;

public partial class LimbRenderer : Node
{
	[Export]
	private Limb _limb;
	
	private float _limbJointRadius = 0.1f;

	public override void _Process(double delta)
	{
		DebugDraw2D.SetText("Segments", _limb.Segments.Count);
		DebugDraw3D.DrawSphere(_limb.Position, 0.1f, new Color(1, 1, 1));
		DebugDraw3D.DrawSphere(_limb.Destination, 0.1f, new Color(1, 0, 0));
		
		float totalLength = _limb.Segments.Sum(segment => segment.Length);
		bool canFitSpheres = totalLength > _limb.Segments.Count * _limbJointRadius * 2;
		
		var col = new Color(0.7f, 0.5f, 0.5f);
		for (var i = 0; i < _limb.Segments.Count; i++)
		{
			var segment = _limb.Segments[i];
			if (canFitSpheres)
				DebugDraw3D.DrawSphere(segment.Position, _limbJointRadius, col);
			if (i < _limb.Segments.Count - 1) // Last segment won't have an ext one
				DebugDraw3D.DrawArrow(segment.Position, _limb.Segments[i+1].Position, col, 0.1f);
				
			col.H = (col.H + (0.5f / _limb.Segments.Count)) % 1;
		}
		var lastSegment = _limb.Segments[^1];
		var dirToDest = lastSegment.Position.DirectionTo(_limb.Destination);
		DebugDraw3D.DrawArrow(lastSegment.Position, lastSegment.Position + dirToDest * lastSegment.Length, col, 0.1f);
	}
}