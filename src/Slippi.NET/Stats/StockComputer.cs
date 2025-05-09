using Slippi.NET.Types;
using Slippi.NET.Stats.Types;
using Slippi.NET.Stats.Utils;

namespace Slippi.NET.Stats;

public class StockComputer : IStatComputer<IList<Stock>>
{
    private readonly Dictionary<PlayerIndexedType, StockState> _state = new();
    private List<PlayerIndexedType> _playerPermutations = new();
    private readonly List<Stock> _stocks = new();

    public void Setup(GameStart settings)
    {
        // Reset state
        _state.Clear();
        _playerPermutations = StatsUtils.GetSinglesPlayerPermutationsFromSettings(settings).ToList();
        _stocks.Clear();

        foreach (var indices in _playerPermutations)
        {
            _state[indices] = new StockState { Stock = null };
        }
    }

    public void ProcessFrame(FrameEntry frame, FramesType allFrames)
    {
        foreach (var indices in _playerPermutations)
        {
            if (_state.TryGetValue(indices, out var state))
            {
                HandleStockCompute(allFrames, state, indices, frame, _stocks);
            }
        }
    }

    public IList<Stock> Fetch()
    {
        return _stocks;
    }

    private static void HandleStockCompute(
        FramesType frames,
        StockState state,
        PlayerIndexedType indices,
        FrameEntry frame,
        List<Stock> stocks)
    {
        var playerFrame = frame.Players?[indices.PlayerIndex]?.Post;
        if (playerFrame is null)
        {
            return;
        }

        var currentFrameNumber = playerFrame.Frame ?? 0;
        var prevFrameNumber = currentFrameNumber - 1;
        var prevPlayerFrame = frames.TryGetValue(prevFrameNumber, out var prevFrame)
            ? prevFrame.Players![indices.PlayerIndex]?.Post
            : null;

        // If there is currently no active stock, wait until the player is no longer spawning.
        // Once the player is no longer spawning, start the stock
        if (state.Stock == null)
        {
            if (StatsUtils.IsDead((State)(playerFrame.ActionStateId ?? 0)))
            {
                return;
            }

            state.Stock = new Stock
            {
                PlayerIndex = indices.PlayerIndex,
                StartFrame = currentFrameNumber,
                EndFrame = null,
                StartPercent = 0,
                EndPercent = null,
                CurrentPercent = 0,
                Count = playerFrame.StocksRemaining ?? 0,
                DeathAnimation = null
            };

            stocks.Add(state.Stock);
        }
        else if (prevPlayerFrame != null && StatsUtils.DidLoseStock(playerFrame, prevPlayerFrame))
        {
            state.Stock.EndFrame = playerFrame.Frame;
            state.Stock.EndPercent = prevPlayerFrame.Percent ?? 0;
            state.Stock.DeathAnimation = playerFrame.ActionStateId;
            state.Stock = null;
        }
        else
        {
            state.Stock.CurrentPercent = playerFrame.Percent ?? 0;
        }
    }
}