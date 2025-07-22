using Barebone.Graphics;
using Barebone.Inputs;

namespace Barebone
{
    /// <summary>
    /// The abstract platform that is running your game. It provides your game with the services that are impacted by that platform.
    /// </summary>
    public interface IPlatform
    {
        IImmediateRenderer ImmediateRenderer { get; }
        IInput Input { get; }
    }
}
