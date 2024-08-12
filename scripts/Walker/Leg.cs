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
    
    private bool _isStepping;
    private bool _isGrounded = true;
    private Vector3 _footPositionGlobal;
    
    private Vector3 RootPosition => _options.RootOffset;
    private Vector3 IdealFootPosition => RootPosition + _options.DesiredFootOffset;
    
    private float AcceptedRadius => _walker.IsMoving ? _options.AcceptedRadius : _options.AcceptedStationaryRadius;
    
    public Leg(Walker walker, LegOptions options)
    {
        _walker = walker;
        _options = options;
        
        _footPositionGlobal = ToGlobal(IdealFootPosition);
    }
    
    public void Render(IIKChainRenderer renderer)
    {
        // DebugDraw3D.DrawSphere(ToGlobal(RootPosition), 0.1f, new Color(0.2f, 0.2f, 0.2f));
        // DebugDraw3D.DrawSphere(ToGlobal(IdealFootPosition), 0.1f, new Color(0.2f, 0.2f, 0.2f));
        // DebugDraw3D.DrawSphere(_footPositionGlobal, 0.1f, new Color(0.2f, 0.2f, 0.5f));
        // DebugDraw3D.DrawSphere(ToGlobal(GetNextStepPosition()), 0.1f, new Color(0.8f, 0.2f, 0.2f));
        // DebugDraw3D.DrawSphere(ToGlobal(IdealFootPosition), AcceptedRadius * _walker.Scale.X, new Color(0.8f, 0.8f, 0.2f));
        // if(_isStepping)
        //     DebugDraw3D.DrawSphere(_footPositionGlobal, 0.5f, new Color(0.2f, 0.8f, 0.2f));
        
        var chainOptions = new IKChainOptions { };
        var chain = new IKChain(RootPosition, _options.SegmentLength, _options.SegmentCount, chainOptions);
        chain.PointTowardsAndUp(ToLocal(_footPositionGlobal), 60);
        chain.SolveTo(ToLocal(_footPositionGlobal));
        renderer.Render(chain);
    }
    
    public void Update(double delta)
    {
        if (_isStepping)
        {
            var localFootPos = ToLocal(_footPositionGlobal);
            localFootPos += _walker.Velocity * (float)delta;
            localFootPos = localFootPos.Rotated(Vector3.Up, _walker.YawRotationVelocity * (float)delta);
            _footPositionGlobal = ToGlobal(localFootPos);
        }
        
        bool shouldStartNewStep = ToLocal(_footPositionGlobal).DistanceTo(IdealFootPosition) > AcceptedRadius;
        var (leg1, leg2) = _walker.GetAdjacentLegs(this);
        if (leg1._isStepping || leg2._isStepping)
            shouldStartNewStep = false;
        
        if (_isStepping || shouldStartNewStep)
            Step(delta);
    }

    private void Step(double delta)
    {
        _isStepping = true;
        
        var stepDestination = GetNextStepPosition();
        
        var localFootPos = ToLocal(_footPositionGlobal);
        localFootPos = localFootPos.Lerp(stepDestination, 1 - Mathf.Exp(-_options.StepSpeed * (float)delta));
        
        float hStepDist = localFootPos.WithY(0).DistanceTo(stepDestination.WithY(0));
        if (_options.StopDropDistance > 0 && hStepDist > _options.StopDropDistance)
            localFootPos.Y = Mathf.Lerp(
                localFootPos.Y, 
                stepDestination.Y + _options.StepHeight, 
                1 - Mathf.Exp(-_options.StepRaiseSpeed * (float)delta));
        
        _footPositionGlobal = ToGlobal(localFootPos);
        
        if (localFootPos.DistanceTo(stepDestination) >= _options.FootArrivalThreshold) return;
        _isStepping = false;
        _footPositionGlobal = ToGlobal(stepDestination);
    }
    
    private Vector3 GetNextStepPosition()
    {
        var candidate = IdealFootPosition;
        if (!_walker.IsMoving) return candidate;

        var rotAddition = IdealFootPosition
            .DirectionTo(RootPosition).WithY(0).Normalized()
            .Rotated(Vector3.Up, -Mathf.Pi / 2) * _walker.YawRotationVelocity;
        DebugDraw3D.DrawArrow(candidate, candidate + rotAddition, new Color(1, 0, 0), 0.1f);
        candidate += rotAddition;

        float nudgeDist = _walker.Velocity.WithY(0).Length() * _options.VelocityStepNudgeMultiplier;
        candidate += _walker.Velocity.WithY(0).Normalized() * nudgeDist;
        candidate.Y += _walker.Velocity.Y;

        return IdealFootPosition 
               + IdealFootPosition.DirectionTo(candidate) 
               * Mathf.Min(AcceptedRadius, IdealFootPosition.DistanceTo(candidate));
    }
    
    public Vector3 ToGlobal(Vector3 localPos)
    {
        return _walker.ToGlobal(localPos);
    }
    public Vector3 ToLocal(Vector3 globalPos)
    {
        return _walker.ToLocal(globalPos);
    }
}