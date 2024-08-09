using Godot;
using Kinematics.scripts.IK;

namespace Kinematics.scripts.Render;

public interface IIKChainRenderer
{
    void Render(IKChain chain);
    void Render(IKChain chain, Color color);
}