using Godot;

namespace Kinematics.scripts.player;

public partial class GrappleHook : Node3D
{
    private Player _player;
    private Node3D _cameraTransform;
    public float PlayerDist;
    
    public GrappleHook WIthData(Player player, float playerDist, Node3D cameraTransform)
    {
        _player = player;
        PlayerDist = playerDist;
        _cameraTransform = cameraTransform;
        return this;
    }

    public override void _PhysicsProcess(double delta)
    {
        var inputDir = _player.GetInputDir();
        var inputDir3D = new Vector3(inputDir.X, 0, -inputDir.Y);
        inputDir3D.Y += Input.GetAxis("crouch", "jump");
		
        var movementVec = _cameraTransform.GlobalTransform.Basis * inputDir3D;
        _player.Velocity += movementVec * (float)delta;
        _player.Velocity += Vector3.Down * _player.Gravity * (float)delta;
        
        _player.MoveAndSlide();
        
        if (_player.GlobalPosition.DistanceTo(GlobalPosition) <= PlayerDist) return;
        _player.GlobalPosition = GlobalPosition + GlobalPosition.DirectionTo(_player.GlobalPosition) * PlayerDist;
        _player.Velocity = ClipVelocity( _player.Velocity, GlobalPosition.DirectionTo(_player.GlobalPosition), 1.0f);
    }
    
    
    private Vector3 ClipVelocity(Vector3 vec, Vector3 normal, float overBounce)
    {
        // Supposedly meant ot help wit sliding off of the surface you're standing on?
        float backoff = vec.Dot(normal);
        if (backoff < 0)
            backoff *= overBounce;
        else
            backoff /= overBounce;
        return vec - normal * backoff;
    }
}