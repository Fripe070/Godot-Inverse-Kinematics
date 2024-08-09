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
    
    public float FootArrivalThreshold = 0.1f;
    
    public float StepHeight = 1.0f;
    public float StopDropDistance = 1.0f;
    
    public float StepSpeed = 10.0f;
    public float StepRaiseSpeed = 5.0f;
}


// TODO: When walking with many (from testing 10) legs there is a weird stretching stretching (likely StraightIfTooFar) when walking, this shouldn't happen even if it is enabled.
// TODO: I should probably clamp the offset length to a maximum of the limb length, or something else to make sure the foot is never too far away
public class Leg
{
    private Walker _walker;
    private LegOptions _options;
    
    private Vector3 _footPosition;
    private bool _isStepping;
    private bool _isGrounded = true;
    
    private Vector3 RootPosition => _walker.GlobalTransform.Origin + _options.RootOffset.Rotated(Vector3.Up, _walker.GlobalBasis.GetEuler().Y);
    private Vector3 DesiredFootPosition => RootPosition + _options.DesiredFootOffset.Rotated(Vector3.Up, _walker.GlobalBasis.GetEuler().Y);
    
    private float AcceptedRadius => _walker.IsMoving ? _options.AcceptedRadius : _options.AcceptedStationaryRadius;
    
    private Vector3 NextStepPosition
    {
        get
        {
            var candidate = DesiredFootPosition;
            if (!_walker.IsMoving) return candidate;
            
            if (_walker.IsRotating)
            {
                var rotAddition = DesiredFootPosition.DirectionTo(RootPosition);
                rotAddition.Y = 0;
                rotAddition = rotAddition.Normalized().Rotated(Vector3.Up, -Mathf.Pi / 2);
                DebugDraw3D.DrawArrow(DesiredFootPosition, DesiredFootPosition + rotAddition, new Color(1, 0.5f, 0), 0.1f);
                candidate += rotAddition * AcceptedRadius;
            }

            var vel = _walker.Velocity;
            vel.Y = 0f;
            float nudgeDist = Mathf.Min(vel.Length() * _options.VelocityStepNudgeMultiplier, AcceptedRadius);
            return candidate + vel.Normalized() * nudgeDist;
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
        DebugDraw3D.DrawArrow(RootPosition, DesiredFootPosition, new Color(0.4f, 0.4f, 0.4f), 0.1f);
        DebugDraw3D.DrawArrow(RootPosition, _footPosition, new Color(1, 1, 1), 0.1f);
        
        var chainOptions = new IKChainOptions { };
        var chain = new IKChain(RootPosition, _options.SegmentLength, _options.SegmentCount, chainOptions);
        chain.PointTowardsAndUp(_footPosition, 60);
        chain.SolveTo(_footPosition);
        renderer.Render(chain, _isStepping ? new Color(0, 1, 0) : new Color(1, 0, 0));
        
        DebugDraw3D.DrawSphere(DesiredFootPosition, 0.1f, new Color(0, 1, 0));
        var col = new Color(1, 0, 0).Lerp(new Color(0, 0, 1), 
            _footPosition.DistanceTo(DesiredFootPosition) / AcceptedRadius);
        DebugDraw3D.DrawSphere(DesiredFootPosition, AcceptedRadius, col);
        DebugDraw3D.DrawArrow(NextStepPosition + Vector3.Up * 2, NextStepPosition, new Color(0, 1, 0), 0.1f);
    }
    
    public void Update(double delta)
    {
        if (_isStepping)
        {
            _footPosition += _walker.Velocity * (float)delta;
            _footPosition = ( _footPosition - _walker.GlobalTransform.Origin
                            ).Rotated(Vector3.Up, _walker.YawRotationVelocity * (float)delta) 
                            + _walker.GlobalTransform.Origin;
        }
        
        bool shouldStartNewStep = _footPosition.DistanceTo(DesiredFootPosition) > AcceptedRadius;
        var (leg1, leg2) = _walker.GetAdjacentLegs(this);
        if (leg1._isStepping || leg2._isStepping)
            shouldStartNewStep = false;
        
        if (_isStepping || shouldStartNewStep)
            Step(delta);
    }

    private void Step(double delta)
    {
        _isStepping = true;
        
        var stepPos = NextStepPosition;
        _footPosition = _footPosition.Lerp(stepPos, 1 - Mathf.Exp(-_options.StepSpeed * (float)delta));
        float hStepDist = Mathf.Sqrt(
            Mathf.Pow(_footPosition.X - stepPos.X, 2) 
            + Mathf.Pow(_footPosition.Z - stepPos.Z, 2));
        if (_options.StopDropDistance > 0 && hStepDist > _options.StopDropDistance)
            _footPosition.Y = Mathf.Lerp(_footPosition.Y, stepPos.Y + _options.StepHeight, 
                                        1 - Mathf.Exp(-_options.StepRaiseSpeed * (float)delta));

        if (_footPosition.DistanceTo(stepPos) >= _options.FootArrivalThreshold) return;
        _isStepping = false;
        _footPosition = stepPos;
    }
}