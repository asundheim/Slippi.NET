using Slippi.NET.Types;
using Slippi.NET.Utils;

namespace Slippi.NET.Tests;

public class GameTimerTests
{
    [Fact]
    public void ShouldReturnUnknownIfNoStartingTimerIsProvided()
    {
        // Arrange
        int frame = 1234;
        TimerType timerType = TimerType.DECREASING;
        int? startingTimerSeconds = null;

        // Act
        string gameTimer = GameTimer.FrameToGameTimer(frame, timerType, startingTimerSeconds);

        // Assert
        Assert.Equal("Unknown", gameTimer);
    }

    [Fact]
    public void ShouldSupportIncreasingTimers()
    {
        // Arrange
        int frame = 2014;
        TimerType timerType = TimerType.INCREASING;
        int startingTimerSeconds = 0;

        // Act
        string gameTimer = GameTimer.FrameToGameTimer(frame, timerType, startingTimerSeconds);

        // Assert
        Assert.Equal("00:33.57", gameTimer);
    }

    [Fact]
    public void ShouldSupportDecreasingTimers()
    {
        // Arrange
        int frame = 4095;
        TimerType timerType = TimerType.DECREASING;
        int startingTimerSeconds = 180;

        // Act
        string gameTimer = GameTimer.FrameToGameTimer(frame, timerType, startingTimerSeconds);

        // Assert
        Assert.Equal("01:51.76", gameTimer);
    }

    [Fact]
    public void ShouldSupportWhenTheExactLimitIsHit()
    {
        // Arrange
        int frame = 10800;
        TimerType timerType = TimerType.DECREASING;
        int startingTimerSeconds = 180;

        // Act
        string gameTimer = GameTimer.FrameToGameTimer(frame, timerType, startingTimerSeconds);

        // Assert
        Assert.Equal("00:00.00", gameTimer);
    }
}