using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;

namespace Kinematics.scripts.IK;


public class IKChainOptions
{
    public float Tolerance = 0.001f;
    public int MaxIterations = 10;
    public bool PrioritiseEnd = true;
    public bool StraightIfTooFar = true;
}


public class IKChain
{
    public readonly IKChainSegment[] Segments;
    public Vector3 RootPosition;
    private readonly IKChainOptions _options;
    
    public double Error => Segments[^1].TipPosition.DistanceTo(_lastTarget);
    public double TotalLength => Segments.Sum(segment => segment.Length);

    private Vector3 _lastTarget;
    
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
    
    // TODO: When StraightIfTooFar is false, and the target becomes too far away, the chain does a weird snapping motion
    public double SolveTo(Vector3 target)
    {
        _lastTarget = target;
        
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
        Segments[^1].TipPosition = target;
        // We work down the chain, starting from the end
        for (int i = Segments.Length - 2; i >= 0; i--)
        {
            var currentSegment = Segments[i];
            var previousSegment = Segments[i + 1];
            var direction = previousSegment.TipPosition.DirectionTo(currentSegment.TipPosition);
            currentSegment.TipPosition = previousSegment.TipPosition + direction * previousSegment.Length;
        }
    }
    
    private void FabrikBackward()
    {
        // We iterate up the chain, starting from the segment right after the base
        for (var i = 0; i < Segments.Length; i++)
        {
            var currentSegment = Segments[i];
            // Length is determined by the current segment when going backwards, which is very convenient here
            var previousSegmentTip = i == 0 ? RootPosition : Segments[i - 1].TipPosition;  
            var direction = previousSegmentTip.DirectionTo(currentSegment.TipPosition);
            currentSegment.TipPosition = previousSegmentTip + direction * currentSegment.Length;
        }
    }
    
    public void PointIn(Vector3 direction)
    {
        float offset = 0;
        foreach (var segment in Segments)
        {
            offset += segment.Length;
            segment.TipPosition = RootPosition + direction * offset;
        }
    }
}