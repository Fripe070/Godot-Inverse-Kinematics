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
    private readonly IKChainOptions _options;
    public IKChainSegment[] Segments;
    public Vector3 RootPosition;
    
    public Vector3 EndPosition => Segments[^1].TipPosition;
    public double Error => EndPosition.DistanceTo(_lastTarget);
    public double TotalLength => Segments.Sum(segment => segment.Length);

    private Vector3 _lastTarget;
    
    public IKChain(Vector3 rootPosition, IKChainSegment[] segments, IKChainOptions options = null)
    {
        _options = options ?? new IKChainOptions();
        RootPosition = rootPosition;
        Segments = segments;
    }
    
    public IKChain(Vector3 rootPosition, float segmentLength, int segmentCount, IKChainOptions options = null)
    {
        _options = options ?? new IKChainOptions();
        RootPosition = rootPosition;
        Segments = new IKChainSegment[segmentCount];
        for (var i = 0; i < segmentCount; i++)
            Segments[i] = new IKChainSegment(rootPosition, segmentLength);
    }
    
    // TODO: When StraightIfTooFar is false, and the target becomes too far away, the chain does a weird snapping motion. Gets reduced if steps are increased
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
    
    public void PointTowardsAndUp(Vector3 target, float upAngle)
    {
        var direction = (target - RootPosition).WithY(0).Normalized();
        var axis = Vector3.Up.Cross(direction).Normalized();
        PointIn(direction.Rotated(axis, Mathf.DegToRad(-upAngle)));
    }
}