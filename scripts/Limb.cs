using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace Kinematics.scripts;

public partial class Limb : Node3D
{
    [Export]
    private Vector3 _destinationOffset = new Vector3(15, 0, 0);
    [Export]
    private int _segmentCount = 4;
    [Export]
    private float _segmentLength = 2.0f;

    [Export] private Node3D _targetObject;
	
    private float _tolerance = 0.01f;
    private int _maxIterations = 10;
	
    public readonly List<Segment> Segments = new List<Segment>();
    
    public Vector3 Destination
    {
        get => Position + _destinationOffset;
        set => _destinationOffset = value - Position;
    }

    public override void _Ready()
    {
        var offset = new Vector3(0, _segmentLength, 0);
        
        var lastPosition = Position;
        for (var i = 0; i <= _segmentCount; i++)
        {
            Segments.Add(new Segment(lastPosition, _segmentLength));
            lastPosition += offset;
        }
    }

    private List<Vector3> _destinationOffsets = new();

    public override void _Process(double delta)
    {
        float time = Time.GetTicksMsec() / 1000.0f / 5f;
        _destinationOffset = new Vector3(Mathf.Cos(time) * 15, 0, Mathf.Sin(time) * 8) * 0.5f;
        if (Engine.GetFramesDrawn() % 60 * 2 == 0)
            _destinationOffsets.Add(_destinationOffset);
        for (var i = 0; i < _destinationOffsets.Count -1; i++)
            DebugDraw3D.DrawLine(Position + _destinationOffsets[i], Position + _destinationOffsets[i + 1], new Color(0, 0, 1));

        // Destination = _targetObject.Position;

        // Temporary and scuffed way to make the limb look like a leg        
        Segments[0].Position = Position;
        for (var i = 1; i < Segments.Count; i++)
            Segments[i].Position = Segments[i-1].Position + new Vector3(0, Segments[i].Length, 0);
        
        Fabrik();
        DebugDraw2D.SetText("Target pos", Destination);
        DebugDraw2D.SetText("Error", (Destination - Segments[^1].Position).Length());
    }
    
    // TODO: Move IK logic into a separate class, away form the limb construction and maintenance logic
    private void Fabrik()
    {
        var destination = Destination;
        
        if ((destination - Position).Length() > Segments.Sum(segment => segment.Length))
        {
            var direction = (destination - Position).Normalized();
            var lastPosition = Position;
            for (var i = 1; i < Segments.Count; i++)
                lastPosition = Segments[i].Position = lastPosition + direction * Segments[i].Length;
            return;
        }
        
        for (var i = 1; i < _maxIterations; i++)
        {
            float distanceFromDestination = (Segments[^1].Position - destination).Length();
            DebugDraw2D.SetText("Error", distanceFromDestination);
            if (distanceFromDestination < _tolerance)
                return;
            
            FabrikForward(destination);
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