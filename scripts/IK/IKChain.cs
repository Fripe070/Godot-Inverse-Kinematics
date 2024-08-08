using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;

namespace Kinematics.scripts.IK;


public class IKChainOptions
{
    public float Tolerance = 0.01f;
    public int MaxIterations = 10;
    public bool PrioritiseEnd = true;
    public bool StraightIfTooFar = true;
}


public class IKChain
{
    public readonly IKChainSegment[] Segments;
    public Vector3 RootPosition;
    private readonly IKChainOptions _options;
    
    public double Error => Segments[^1].Position.DistanceTo(RootPosition) - Segments[^1].Length;
    public double TotalLength => Segments.Sum(segment => segment.Length);
    public Vector3 LastTarget { get; private set; }
    
    public IKChain(Vector3 rootPosition, IKChainSegment[] segments, IKChainOptions options = null)
    {
        RootPosition = rootPosition;
        Segments = segments;
        _options = options ?? new IKChainOptions();
    }
    
    public IKChain(Vector3 rootPosition, float segmentLength, int segmentCount, IKChainOptions options = null)
    {
        RootPosition = rootPosition;
        Segments = new IKChainSegment[segmentCount];
        for (var i = 0; i < segmentCount; i++)
            Segments[i] = new IKChainSegment(rootPosition, segmentLength);

        _options = options ?? new IKChainOptions();
    }
    
    public double SolveTo(Vector3 target)
    {
        LastTarget = target;
        
        bool tooFar = target.DistanceTo(RootPosition) > Segments.Sum(segment => segment.Length);
        if (_options.StraightIfTooFar && tooFar)
        {
            PointIn(RootPosition.DirectionTo(target));
            return Error;
        }
        
        for (var i = 0; i < _options.MaxIterations; i++)
        {
            if (Error < _options.Tolerance)
                return Error;
            
            FabrikForward(target);
            FabrikBackward();
        }
        if (_options.PrioritiseEnd && Error > _options.Tolerance && !tooFar)
            FabrikForward(target);
        return Error;
    }
    
    private void FabrikForward(Vector3 target)
    {
        // We work down the chain, starting from the end
        for (int i = Segments.Length - 1; i >= 0; i--)
        {
            var segment = Segments[i];
            // The segment at the end of the chain (the first one we iterate over) should be moved relative to the destination
            var upSegmentPosition = i == Segments.Length - 1 ? target : Segments[i + 1].Position;
            var direction = upSegmentPosition.DirectionTo(segment.Position);
            segment.Position = upSegmentPosition + direction * segment.Length;
        }
    }
    
    private void FabrikBackward()
    {
        Segments[0].Position = RootPosition;
        // We iterate up the chain, starting from the segment right after the base
        for (var i = 1; i < Segments.Length; i++)
        {
            var segment = Segments[i];
            var downSegmentPosition = Segments[i - 1].Position;
            var direction = downSegmentPosition.DirectionTo(segment.Position);
            segment.Position = downSegmentPosition + direction * segment.Length;
        }
    }
    
    public void PointIn(Vector3 direction)
    {
        float offset = 0;
        foreach (var segment in Segments)
        {
            segment.Position = RootPosition + direction * offset;
            offset += segment.Length;
        }
    }
}