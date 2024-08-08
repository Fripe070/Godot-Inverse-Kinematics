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

    [Export] private float _tolerance = 0.01f;
    [Export] private int _maxIterations = 10;
    [Export] private bool _prioritiseEnd = false;
	
    public readonly List<Segment> Segments = new List<Segment>();

    public Vector3 DestinationOffset
    {
        get => Destination - Position;
        set => Destination = Position + value;
    }
    
    public float Error => Destination.DistanceTo(Segments[^1].Position) - Segments[^1].Length;
    
    public override void _Ready()
    {
        for (var i = 0; i <= _segmentCount; i++)
            Segments.Add(new Segment(Position, _segmentLength));
    }
    
    public override void _Process(double delta)
    {
        PointTowardsAndUp(Destination, -70);
        Fabrik(Destination);
        
        DebugDraw2D.SetText("Target pos", Destination);
        DebugDraw2D.SetText("Error", Error);
    }
    
    private void PointTowardsAndUp(Vector3 goal, float upAngle)
    {
        var direction = goal - Position;
        direction.Y = 0;
        direction = direction.Normalized();
        var axis = Vector3.Up.Cross(direction).Normalized();
        direction = direction.Rotated(axis, Mathf.DegToRad((float)upAngle));
        PointIn(direction);
    }
    
    private void PointIn(Vector3 direction)
    {
        float offset = 0;
        foreach (var segment in Segments)
        {
            segment.Position = Position + direction * offset;
            offset += segment.Length;
        }
    }
    
    // TODO: Move IK logic into a separate class, away form the limb construction and maintenance logic
    private void Fabrik(Vector3 target)
    {
        bool tooFar = target.DistanceTo(Position) > Segments.Sum(segment => segment.Length);
        if (tooFar)
        {
            PointIn(Position.DirectionTo(target));
            return;
        }
        
        for (var i = 0; i < _maxIterations; i++)
        {
            if (Error < _tolerance)
                return;
            
            FabrikForward(target);
            FabrikBackward();
        }
        if (_prioritiseEnd && Error > _tolerance && !tooFar)
            FabrikForward(target);
    }
    
    private void FabrikForward(Vector3 destination)
    {
        // We work down the chain, starting from the end
        for (int i = Segments.Count - 1; i >= 0; i--)
        {
            var segment = Segments[i];
            // The segment at the end of the chain (the first one we iterate over) should be moved relative to the destination
            var upSegmentPosition = i == Segments.Count - 1 ? destination : Segments[i + 1].Position;
            
            var direction = upSegmentPosition.DirectionTo(segment.Position);
            segment.Position = upSegmentPosition + direction * segment.Length;
        }
    }
    
    private void FabrikBackward()
    {
        Segments[0].Position = Position;
        // We iterate up the chain, starting from the segment right after the base
        for (var i = 1; i < Segments.Count; i++)
        {
            var segment = Segments[i];
            var downSegmentPosition = Segments[i - 1].Position;
            var direction = downSegmentPosition.DirectionTo(segment.Position);
            
            segment.Position = downSegmentPosition + direction * segment.Length;
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