namespace Slippi.NET.Types;

public abstract class EventPayloadTypes { }

public class GameStartPayload : EventPayloadTypes
{
    public GameStartPayload(GameStartType gameStart)
    {
        GameStart = gameStart;
    }

    public GameStartType GameStart { get; set; }
}

public class FrameStartPayload : EventPayloadTypes
{
    public FrameStartPayload(FrameStartType frameStart)
    {
        FrameStart = frameStart;
    }

    public FrameStartType FrameStart { get; set; }
}

public class PreFrameUpdatePayload : EventPayloadTypes
{
    public PreFrameUpdatePayload(PreFrameUpdateType preFrameUpdate)
    {
        PreFrameUpdate = preFrameUpdate;
    }

    public PreFrameUpdateType PreFrameUpdate { get; set; }
}

public class PostFrameUpdatePayload : EventPayloadTypes
{
    public PostFrameUpdatePayload(PostFrameUpdateType postFrameUpdate)
    {
        PostFrameUpdate = postFrameUpdate;
    }

    public PostFrameUpdateType PostFrameUpdate { get; set; }
}

public class ItemUpdatePayload : EventPayloadTypes
{
    public ItemUpdatePayload(ItemUpdateType itemUpdate)
    {
        ItemUpdate = itemUpdate;
    }

    public ItemUpdateType ItemUpdate { get; set; }
}

public class FrameBookendPayload : EventPayloadTypes
{
    public FrameBookendPayload(FrameBookendType frameBookend)
    {
        FrameBookend = frameBookend;
    }

    public FrameBookendType FrameBookend { get; set; }
}

public class GameEndPayload : EventPayloadTypes
{
    public GameEndPayload(GameEndType gameEnd)
    {
        GameEnd = gameEnd;
    }

    public GameEndType GameEnd { get; set; }
}

public class GeckoListPayload : EventPayloadTypes
{
    public GeckoListPayload(GeckoListType geckoList)
    {
        GeckoList = geckoList;
    }

    public GeckoListType GeckoList { get; set; }
}
