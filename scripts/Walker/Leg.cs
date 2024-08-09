using Godot;
using Kinematics.scripts.IK;
using Kinematics.scripts.Render;

namespace Kinematics.scripts.Walker;


public class LegOptions
{
    public int SegmentCount = 4;
    public float SegmentLength = 2.0f;
    
    public Vector3 RootOffset = new Vector3(0, 0, 0);
    public Vector3 DesiredFootOffset = new Vector3(3, -1, 0);
    
    public float AcceptedRadius = 2.0f;
}


public class Leg
{
    private Walker _walker;
    private LegOptions _options;
    
    private Vector3 _footPosition;
    
    private Vector3 RootPosition => _walker.GlobalTransform.Origin + _options.RootOffset;
    private Vector3 DesiredFootPosition => RootPosition + _options.DesiredFootOffset;
    
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
            _footPosition.DistanceTo(DesiredFootPosition) / _options.AcceptedRadius);
        DebugDraw3D.DrawSphere(DesiredFootPosition, _options.AcceptedRadius, col);
    }
    
    public void Update(double delta)
    {
        float dist = _footPosition.DistanceTo(DesiredFootPosition);
        if (dist > _options.AcceptedRadius)
        {
            _footPosition = DesiredFootPosition;
        }
    }
}