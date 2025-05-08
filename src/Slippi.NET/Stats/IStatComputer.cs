using Slippi.NET.Types;

namespace Slippi.NET.Stats;

public interface IStatComputer<T>
{
    void Setup(GameStart settings);
    void ProcessFrame(FrameEntry newFrame, FramesType allFrames);
    T Fetch();
}