using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Kinematics.scripts;

public partial class Limb : Node3D
{
    [Export] public Vector3 Destination;
    [Export] private int _segmentCount = 4;
    [Export] private float _segmentLength = 2.0f;

    private float _tolerance = 0.01f;
    private int _maxIterations = 10;
	
    public readonly List<Segment> Segments = new List<Segment>();

    public Vector3 DestinationOffset
    {
        get => Destination - Position;
        set => Destination = Position + value;
    }
    
    public override void _Ready()
    {
        for (var i = 0; i <= _segmentCount; i++)
            Segments.Add(new Segment(Position, _segmentLength));
    }
    
    public override void _Process(double delta)
    {
        PointTowards(Destination, -50);
        Fabrik(Destination);
        
        DebugDraw2D.SetText("Target pos", Destination);
        DebugDraw2D.SetText("Error", (Destination - Segments[^1].Position).Length());
    }
    
    private void PointTowards(Vector3 goal, float upAngle)
    {
        if (upAngle == 0f)
        {
            PointIn(Position.DirectionTo(goal));
            return;
        }

        var direction = goal - Position;
        direction.Y = 0;
        direction = direction.Normalized();
        var axis = Vector3.Up.Cross(direction).Normalized();
        direction = direction.Rotated(axis, Mathf.DegToRad(upAngle));
        PointIn(direction);
    }
    
    private void PointIn(Vector3 direction)
    {
        for (var i = 1; i < Segments.Count; i++)
        {
            var segment = Segments[i];
            var previousSegment = Segments[i - 1];
            segment.Position = previousSegment.Position + direction * segment.Length;
        }
    }
    
    // TODO: Move IK logic into a separate class, away form the limb construction and maintenance logic
    private void Fabrik(Vector3 target)
    {
        if ((target - Position).Length() > Segments.Sum(segment => segment.Length))
        {
            var direction = (target - Position).Normalized();
            var lastPosition = Position;
            for (var i = 1; i < Segments.Count; i++)
                lastPosition = Segments[i].Position = lastPosition + direction * Segments[i].Length;
            return;
        }
        
        for (var i = 1; i < _maxIterations; i++)
        {
            float distanceFromDestination = (Segments[^1].Position - target).Length();
            DebugDraw2D.SetText("Error", distanceFromDestination);
            if (distanceFromDestination < _tolerance)
                return;
            
            FabrikForward(target);
            FabrikBackward();
        }
    }
    
    private void FabrikForward(Vector3 destination)
    {
        Segments[^1].Position = destination;

        for (var i = 1; i < Segments.Count; i++)
        {
            var segment = Segments[^i];
            var previousSegment = Segments[^(i + 1)];
            
            var direction = (previousSegment.Position - segment.Position).Normalized();
            previousSegment.Position = segment.Position + direction * segment.Length;
        }
    }
    
    private void FabrikBackward()
    {
        Segments[0].Position = Position;

        for (var i = 0; i < Segments.Count - 1; i++)
        {
            var segment = Segments[i];
            var nextSegment = Segments[i + 1];
            
            var direction = (nextSegment.Position - segment.Position).Normalized();
            nextSegment.Position = segment.Position + direction * segment.Length;
        }
    }
}

public class Segment
{
    public Vector3 Position;
    public float Length;
    
    public Segment(Vector3 origin, float length)
    {
        Position = origin;
        Length = length;
    }
}