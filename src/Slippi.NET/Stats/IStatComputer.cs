using Slippi.NET.Types;

namespace Slippi.NET.Stats;

public interface IStatComputer<T>
{
    void Setup(GameStartType settings);
    void ProcessFrame(FrameEntryType newFrame, FramesType allFrames);
    T Fetch();
}