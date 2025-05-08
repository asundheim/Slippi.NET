namespace Slippi.NET.Types;

public abstract class EventPayload { }

public class GameStartPayload : EventPayload
{
    public GameStartPayload(GameStart gameStart)
    {
        GameStart = gameStart;
    }

    public GameStart GameStart { get; set; }
}

public class FrameStartPayload : EventPayload
{
    public FrameStartPayload(FrameStart frameStart)
    {
        FrameStart = frameStart;
    }

    public FrameStart FrameStart { get; set; }
}

public class PreFrameUpdatePayload : EventPayload
{
    public PreFrameUpdatePayload(PreFrameUpdate preFrameUpdate)
    {
        PreFrameUpdate = preFrameUpdate;
    }

    public PreFrameUpdate PreFrameUpdate { get; set; }
}

public class PostFrameUpdatePayload : EventPayload
{
    public PostFrameUpdatePayload(PostFrameUpdate postFrameUpdate)
    {
        PostFrameUpdate = postFrameUpdate;
    }

    public PostFrameUpdate PostFrameUpdate { get; set; }
}

public class ItemUpdatePayload : EventPayload
{
    public ItemUpdatePayload(ItemUpdate itemUpdate)
    {
        ItemUpdate = itemUpdate;
    }

    public ItemUpdate ItemUpdate { get; set; }
}

public class FrameBookendPayload : EventPayload
{
    public FrameBookendPayload(FrameBookendType frameBookend)
    {
        FrameBookend = frameBookend;
    }

    public FrameBookendType FrameBookend { get; set; }
}

public class GameEndPayload : EventPayload
{
    public GameEndPayload(GameEnd gameEnd)
    {
        GameEnd = gameEnd;
    }

    public GameEnd GameEnd { get; set; }
}

public class GeckoListPayload : EventPayload
{
    public GeckoListPayload(GeckoCodeList geckoList)
    {
        GeckoList = geckoList;
    }

    public GeckoCodeList GeckoList { get; set; }
}
