using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Kinematics.scripts.old;

public partial class Spider : Node3D
{
	[Export] private int _legCount = 4;
	[Export] private int _segmentCount = 4;
	
	public List<Foot> Feet = new List<Foot>();
	
	public override void _Ready()
	{
		float rotation = Mathf.Pi / 4;
		for (var i = 0; i < _legCount; i++)
		{
			var scene = GD.Load<PackedScene>("res://scenes/Leg.tscn");
			var instance = (Node3D)scene.Instantiate();
			AddChild(instance);
			Feet.Add(instance.GetNode<Foot>("Foot"));
			instance.RotateY(rotation);
			rotation += Mathf.Pi * 2 / _legCount;
		}
	}

	public override void _Process(double delta)
	{
		DebugDraw3D.DrawSphereXf(Transform, new Color(1, 1, 1));
		
		// Set own position to average of all feet
		float average = Feet.Sum(foot => foot.GlobalPosition.Y) / Feet.Count;
		average += 1;
		GlobalPosition = new Vector3(GlobalPosition.X, average, GlobalPosition.Z);
		
		Position += new Vector3(
			0, 
			0,
			(float)delta * 3.5f
			);
	}
}