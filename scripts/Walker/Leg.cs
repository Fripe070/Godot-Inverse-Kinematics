using System;
using Godot;
using Kinematics.scripts.IK;
using Kinematics.scripts.Render;

namespace Kinematics.scripts.Walker;


public class LegOptions
{
    public int SegmentCount = 4;
    public float SegmentLength = 1.5f;
    
    public Vector3 RootOffset = new Vector3(0, 0, 0);
    public Vector3 DesiredFootOffset = new Vector3(3, -1, 0);
    public float VelocityStepNudgeMultiplier = 2.0f;
    public float AcceptedRadius = 1.0f;
    public float AcceptedStationaryRadius = 0.4f;
    
    public float StationaryVelThreshold = 1.6f;
    public float FootArrivalThreshold = 0.1f;
    
    public float StepHeight = 1.0f;
    public float StopDropDistance = 1.0f;
    
    public float StepSpeed = 10.0f;
    public float StepRaiseSpeed = 5.0f;
}


public class Leg
{
    private Walker _walker;
    private LegOptions _options;
    
    private Vector3 _footPosition;
    private bool _isStepping;
    
    private Vector3 RootPosition => _walker.GlobalTransform.Origin + _options.RootOffset;
    private Vector3 DesiredFootPosition => RootPosition + _options.DesiredFootOffset;
    
    private float AcceptedRadius => _walker.Velocity.Length() >= _options.StationaryVelThreshold ? _options.AcceptedRadius : _options.AcceptedStationaryRadius;
    
    private Vector3 NextStepPosition
    {
        get
        {
            var vel = _walker.Velocity;
            vel.Y = 0f;
            if (vel.Length() < _options.StationaryVelThreshold) return DesiredFootPosition;
            
            float nudgeDist = Mathf.Min(vel.Length() * _options.VelocityStepNudgeMultiplier, AcceptedRadius);
            return RootPosition + _options.DesiredFootOffset + vel.Normalized() * nudgeDist;
        }
    }
    
    public Leg(Walker walker, LegOptions options)
    {
        _walker = walker;
        _options = options;
        
        _footPosition = DesiredFootPosition;
    }
    
    public void Render(IIKChainRenderer renderer)
    {
        var chainOptions = new IKChainOptions { };
        var chain = new IKChain(RootPosition, _options.SegmentLength, _options.SegmentCount, chainOptions);
        chain.PointTowardsAndUp(_footPosition, 60);
        chain.SolveTo(_footPosition);
        renderer.Render(chain);
        
        DebugDraw3D.DrawSphere(DesiredFootPosition, 0.1f, new Color(0, 1, 0));
        var col = new Color(1, 0, 0).Lerp(new Color(0, 0, 1), 
            _footPosition.DistanceTo(DesiredFootPosition) / AcceptedRadius);
        DebugDraw3D.DrawSphere(DesiredFootPosition, AcceptedRadius, col);
        DebugDraw3D.DrawArrow(NextStepPosition + Vector3.Up * 2, NextStepPosition, new Color(0, 1, 0), 0.1f);
    }
    
    public void Update(double delta)
    {
        if (_isStepping || _footPosition.DistanceTo(DesiredFootPosition) > AcceptedRadius)
            Step(delta);

        if (_isStepping)
            _footPosition += _walker.Velocity * (float)delta;
    }

    private void Step(double delta)
    {
        _isStepping = true;
        
        _footPosition = _footPosition.Lerp(NextStepPosition, 1 - Mathf.Exp(-_options.StepSpeed * (float)delta));
        
        float hStepDist = Mathf.Sqrt(
            Mathf.Pow(_footPosition.X - NextStepPosition.X, 2) 
            + Mathf.Pow(_footPosition.Z - NextStepPosition.Z, 2));
        if (_options.StopDropDistance > 0 && hStepDist > _options.StopDropDistance)
            _footPosition.Y = Mathf.Lerp(_footPosition.Y, NextStepPosition.Y + _options.StepHeight, 
                                        1 - Mathf.Exp(-_options.StepRaiseSpeed * (float)delta));

        if (_footPosition.DistanceTo(NextStepPosition) >= _options.FootArrivalThreshold) return;
        _isStepping = false;
        _footPosition = NextStepPosition;
    }
}