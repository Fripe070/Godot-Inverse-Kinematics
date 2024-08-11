using Godot;
using Kinematics.scripts.IK;

namespace Kinematics.scripts.Render;

public interface IIKChainRenderer
{
    public void Render(IKChain chain);
}