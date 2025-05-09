using Slippi.NET.Slp.Reader.Types;
using Slippi.NET.Types;
using static Slippi.NET.Utils.FullWidthConverter;
using static Slippi.NET.Utils.ByteUtils;
using Slippi.NET.Slp.Reader;
using Slippi.NET.Utils;
using UBJson;

namespace Slippi.NET.Slp;

public class SlpFile : IDisposable
{
    public required SlpRef SlpRef { get; init; }

    public required int RawDataPosition { get; set; }

    public required int RawDataLength { get; set; }

    public required int MetadataPosition { get; set; }

    public required int MetadataLength { get; set; }

    public Dictionary<int, int> MessageSizes { get; set; } = [];

    public int IterateEvents(EventCallbackFunc callback, int? startPos = null)
    {
        int readPosition = startPos is not null && startPos > 0 ? startPos.Value : RawDataPosition;
        int stopReadingAt = RawDataPosition + RawDataLength;

        // Generate read buffers for each message
        Dictionary<int, byte[]> commandPayloadBuffers = MessageSizes.ToDictionary(key => key.Key, value => new byte[value.Value + 1]);

        Span<byte> splitMessageBuffer = stackalloc byte[1];

        Span<byte> commandByteBuffer = stackalloc byte[1];
        while (readPosition < stopReadingAt)
        {
            SlpRef.ReadRef(commandByteBuffer, readPosition);

            byte commandByte = commandByteBuffer[0];
            if (!commandPayloadBuffers.TryGetValue(commandByte, out byte[]? payloadBuffer))
            {
                // If we don't have an entry for this command, return false to indicate failed read
                return readPosition;
            }

            Span<byte> buffer = payloadBuffer.AsSpan();
            if (buffer.Length > stopReadingAt - readPosition)
            {
                // Can't read that many bytes
                return readPosition;
            }

            int advanceAmount = buffer.Length;

            SlpRef.ReadRef(buffer, readPosition);
            if (commandByte == (byte)Command.SPLIT_MESSAGE)
            {
                // Here we have a split message, we will collect data from them until the last
                // message of the list is received
                SlpDataReader reader = new SlpDataReader(buffer);
                ushort size = reader.ReadUInt16(0x201) ?? 512;
                bool isLastMessage = reader.ReadBool(0x204) ?? false;
                byte internalCommand = reader.ReadUInt8(0x203) ?? 0;

                // If this is the first message, initialize the splitMessageBuffer
                // with the internal command byte because our parseMessage function
                // seems to expect a command byte at the start
                if (splitMessageBuffer.Length == 0)
                {
                    splitMessageBuffer = new byte[1] { internalCommand };
                }

                // Collect new data into splitMessageBuffer
                Span<byte> appendBuf = buffer.Slice(1, size);
                Span<byte> mergedBuf = [..splitMessageBuffer, ..appendBuf];
                splitMessageBuffer = mergedBuf;

                if (isLastMessage)
                {
                    commandByte = splitMessageBuffer[0]; // ?? 0 - huh?
                    buffer = new byte[splitMessageBuffer.Length];
                    splitMessageBuffer.CopyTo(buffer);
                    splitMessageBuffer = [];
                }
            }

            EventPayload? parsedPayload = ParseMessage((Command)commandByte, buffer);
            bool shouldStop = callback((Command)commandByte, parsedPayload, buffer.ToArray());
            if (shouldStop)
            {
                break;
            }

            readPosition += advanceAmount;
        }

        return readPosition;
    }

    public static EventPayload? ParseMessage(Command command, Span<byte> payload)
    {
        SlpDataReader x = new SlpDataReader(payload);

        return command switch
        {
            Command.GAME_START => ParseGameStart(x, payload),
            Command.FRAME_START => ParseFrameStart(x),
            Command.PRE_FRAME_UPDATE => ParsePreFrameUpdate(x),
            Command.POST_FRAME_UPDATE => ParsePostFrameUpdate(x),
            Command.ITEM_UPDATE => ParseItemUpdate(x),
            Command.FRAME_BOOKEND => ParseFrameBookend(x),
            Command.GAME_END => ParseGameEnd(x),
            Command.GECKO_LIST => ParseGeckoList(x, payload),
            _ => null
        };
    }

    public unsafe Metadata? GetMetadata()
    {
        if (MetadataLength <= 0)
        {
            // This will happen on a severed incomplete file
            return null;
        }

        Span<byte> buffer = stackalloc byte[MetadataLength];
        SlpRef.ReadRef(buffer, MetadataPosition);

        Metadata? metadata = null;
        try
        {
            fixed (byte* pBuffer = &buffer[0])
            {
                using UnmanagedMemoryStream stream = new UnmanagedMemoryStream(pBuffer, buffer.Length);
                metadata = UBJsonReader.Parse<Metadata>(stream);
            }
        }
        catch (Exception e)
        {
            System.Console.WriteLine("Failed to deserialize metadata");
            System.Console.WriteLine(e);
        }

        return metadata;
    }

    public GameEnd? GetGameEnd()
    {
        if (!MessageSizes.TryGetValue((byte)Command.GAME_END, out int gameEndPayloadSize) || gameEndPayloadSize <= 0)
        {
            return null;
        }

        // Add one to account for command byte
        int gameEndSize = gameEndPayloadSize + 1;
        int gameEndPosition = RawDataPosition + RawDataLength - gameEndSize;

        Span<byte> buffer = stackalloc byte[gameEndSize];
        SlpRef.ReadRef(buffer, gameEndPosition);
        if (buffer[0] != (byte)Command.GAME_END)
        {
            // This isn't even a game end payload
            return null;
        }

        EventPayload? gameEndMessage = ParseMessage(Command.GAME_END, buffer);
        return (gameEndMessage as GameEndPayload)?.GameEnd;
    }

    public List<PostFrameUpdate> ExtractFinalPostFrameUpdates()
    {
        // The following should exist on all replay versions
        int postFrameSize;
        if (MessageSizes.TryGetValue((byte)Command.POST_FRAME_UPDATE, out int postFramePayloadSize))
        {
            postFrameSize = postFramePayloadSize + 1;
        }
        else
        {
            // Technically this should not be possible
            return [];
        }

        int gameEndSize;
        if (MessageSizes.TryGetValue((byte)Command.GAME_END, out int gameEndPayloadSize))
        {
            gameEndSize = gameEndPayloadSize + 1;
        }
        else
        {
            gameEndSize = 0;
        }

        int frameBookendSize;
        if (MessageSizes.TryGetValue((byte)Command.FRAME_BOOKEND, out int frameBookendPayloadSize))
        {
            frameBookendSize = frameBookendPayloadSize + 1;
        }
        else
        {
            frameBookendSize = 0;
        }

        int? frameNum = null;
        int postFramePosition = RawDataPosition + RawDataLength - gameEndSize - frameBookendSize - postFrameSize;
        List<PostFrameUpdate> postFrameUpdates = new List<PostFrameUpdate>();
        do
        {
            byte[] buffer = new byte[postFrameSize];
            SlpRef.ReadRef(buffer.AsSpan(), postFramePosition);
            if (buffer[0] != (byte)Command.POST_FRAME_UPDATE)
            {
                break;
            }

            PostFrameUpdate? postFrameMessage = (ParseMessage(Command.POST_FRAME_UPDATE, buffer) as PostFrameUpdatePayload)?.PostFrameUpdate;
            if (postFrameMessage is null)
            {
                break;
            }

            if (frameNum is null)
            {
                frameNum = postFrameMessage.Frame;
            }
            else if (frameNum != postFrameMessage.Frame)
            {
                // If post frame message is found but the frame doesn't match, it's not part of the final frame
                break;
            }

            postFrameUpdates.Add(postFrameMessage);
            postFramePosition -= postFrameSize;
        } while (postFramePosition >= RawDataPosition);

        postFrameUpdates.Reverse();
        return postFrameUpdates;
    }

    private static EventPayload? ParseGameStart(SlpDataReader x, Span<byte> payload)
    {
        const int matchIdLength = 51;
        const int matchIdStart = 0x2be;
        string? matchId = null;
        if (payload.Length >= matchIdStart + matchIdLength)
        {
            Span<byte> matchIdBuf = payload.Slice(matchIdStart, matchIdLength);
            matchId = StringUtils.Instance.ReadUtf8(matchIdBuf) ?? string.Empty;
        }

        List<Player> players = new List<Player>(4);
        for (int i = 0; i < 4; i++)
        {
            players.Add(ParsePlayerObject(x, payload, i));
        }

        return new GameStartPayload(
            new GameStart(
                slpVersion: $"{x.ReadUInt8(0x1)}.{x.ReadUInt8(0x2)}.{x.ReadUInt8(0x3)}",
                timerType: x.ReadUInt8(0x5, 0x03).EnumCast<TimerType>(),
                inGameMode: x.ReadUInt8(0x5, 0xe0),
                friendlyFireEnabled: x.ReadUInt8(0x6, 0x01) > 0,
                isTeams: x.ReadBool(0xd),
                stageId: x.ReadUInt16(0x13),
                startingTimerSeconds: x.ReadUInt32(0x15),
                itemSpawnBehavior: x.ReadUInt8(0x10).EnumCast<ItemSpawnType>(),
                enabledItems: GetEnabledItems(x),
                players,
                scene: x.ReadUInt8(0x1a3),
                gameMode: x.ReadUInt8(0x1a4).EnumCast<GameMode>(),
                language: x.ReadUInt8(0x2bd).EnumCast<Language>(),
                gameInfoBlock: ParseGameInfoObject(x),
                randomSeed: x.ReadUInt32(0x13d),
                isPAL: x.ReadBool(0x1a1),
                isFrozenPS: x.ReadBool(0x1a2),
                matchInfo: new MatchInfo(
                    matchId,
                    gameNumber: x.ReadUInt32(0x2f1),
                    tiebreakerNumber: x.ReadUInt32(0x2f5)
                )
            )
        );
    }

    private static Player ParsePlayerObject(SlpDataReader x, Span<byte> payload, int playerIndex)
    {
        // Controller Fix stuff
        int cfOffset = playerIndex * 0x8;
        uint? dashback = x.ReadUInt32(0x141 + cfOffset);
        uint? shieldDrop = x.ReadUInt32(0x145 + cfOffset);
        string controllerFix = "None";
        if (dashback != shieldDrop)
        {
            controllerFix = "Mixed";
        }
        else if (dashback == 1)
        {
            controllerFix = "UCF";
        }
        else if (dashback == 2)
        {
            controllerFix = "Dween";
        }

        // Nametag stuff
        const int nametagLength = 0x10;
        int nametagOffset = playerIndex * nametagLength;
        int nametagStart = 0x161 + nametagOffset;
        string nametag = string.Empty;
        if (payload.Length >= nametagStart + nametagLength)
        {
            Span<byte> nametagBuf = payload.Slice(nametagStart, nametagLength);
            string? nametagString = StringUtils.Instance.ReadShiftJIS(nametagBuf);
            nametag = string.IsNullOrEmpty(nametagString) ? string.Empty : ToHalfwidth(nametagString);
        }

        // Display name
        const int displayNameLength = 0x1f;
        int displayNameOffset = playerIndex * displayNameLength;
        int displayNameStart = 0x1a5 + displayNameOffset;
        string displayName = string.Empty;
        if (payload.Length >= displayNameStart + displayNameLength)
        {
            Span<byte> displayNameBuf = payload.Slice(displayNameStart, displayNameLength);
            string? displayNameString = StringUtils.Instance.ReadShiftJIS(displayNameBuf);
            displayName = string.IsNullOrEmpty(displayNameString) ? string.Empty : ToHalfwidth(displayNameString);
        }

        // Connect code
        const int connectCodeLength = 0xa;
        int connectCodeOffset = playerIndex * connectCodeLength;
        int connectCodeStart = 0x221 + connectCodeOffset;
        string connectCode = string.Empty;
        if (payload.Length >= connectCodeStart + connectCodeLength)
        {
            Span<byte> connectCodeBuf = payload.Slice(connectCodeStart, connectCodeLength);
            string? connectCodeString = StringUtils.Instance.ReadShiftJIS(connectCodeBuf);
            connectCode = string.IsNullOrEmpty(connectCodeString) ? string.Empty : ToHalfwidth(connectCodeString);
        }

        // User Id
        const int userIdLength = 0x1d;
        int userIdOffset = playerIndex * userIdLength;
        int userIdStart = 0x249 + userIdOffset;
        string userId = string.Empty;
        if (payload.Length >= userIdStart + userIdLength)
        {
            Span<byte> userIdBuf = payload.Slice(userIdStart, userIdLength);
            connectCode = StringUtils.Instance.ReadUtf8(userIdBuf) ?? string.Empty;
        }

        int offset = playerIndex * 0x24;
        return new Player(
            playerIndex,
            port: playerIndex + 1,
            characterId: x.ReadUInt8(0x65 + offset),
            type: x.ReadUInt8(0x66 + offset),
            startStocks: x.ReadUInt8(0x67 + offset),
            characterColor: x.ReadUInt8(0x68 + offset),
            teamShade: x.ReadUInt8(0x6c + offset),
            handicap: x.ReadUInt8(0x6d + offset),
            teamId: x.ReadUInt8(0x6e + offset),
            staminaMode: x.ReadUInt8(0x6c + playerIndex * 0x24, 0x01) > 0,
            silentCharacter: x.ReadUInt8(0x6c + playerIndex * 0x24, 0x02) > 0,
            invisible: x.ReadUInt8(0x6c + playerIndex * 0x24, 0x08) > 0,
            lowGravity: x.ReadUInt8(0x6c + playerIndex * 0x24, 0x04) > 0,
            blackStockIcon: x.ReadUInt8(0x6c + playerIndex * 0x24, 0x10) > 0,
            metal: x.ReadUInt8(0x6c + playerIndex * 0x24, 0x20) > 0,
            startOnAngelPlatform: x.ReadUInt8(0x6c + playerIndex * 0x24, 0x40) > 0,
            rumbleEnabled: x.ReadUInt8(0x6c + playerIndex * 0x24, 0x80) > 0,
            cpuLevel: x.ReadUInt8(0x74 + offset),
            offenseRatio: x.ReadFloat(0x7d + offset),
            defenseRatio: x.ReadFloat(0x81 + offset),
            modelScale: x.ReadFloat(0x85 + offset),
            controllerFix,
            nametag,
            displayName,
            connectCode,
            userId
        );
    }

    private static GameInfo ParseGameInfoObject(SlpDataReader x)
    {
        const int offset = 0x5;

        return new GameInfo(
            gameBitfield1: x.ReadUInt8(0x0 + offset),
            gameBitfield2: x.ReadUInt8(0x1 + offset),
            gameBitfield3: x.ReadUInt8(0x2 + offset),
            gameBitfield4: x.ReadUInt8(0x3 + offset),
            bombRainEnabled: (x.ReadUInt8(0x6 + offset) & 0xff) > 0, // not sure why the unnecessary bitmask
            selfDestructScoreValue: x.ReadInt8(0xc + offset),
            itemSpawnBitfield1: x.ReadUInt8(0x23 + offset),
            itemSpawnBitfield2: x.ReadUInt8(0x24 + offset),
            itemSpawnBitfield3: x.ReadUInt8(0x25 + offset),
            itemSpawnBitfield4: x.ReadUInt8(0x26 + offset),
            itemSpawnBitfield5: x.ReadUInt8(0x27 + offset),
            damageRatio: x.ReadFloat(0x30 + offset)
        );
    }

    private static ulong GetEnabledItems(SlpDataReader x)
    {
        ulong[] offsets = [0x1, 0x100, 0x10000, 0x1000000, 0x100000000];
        ulong enabledItems = 0;
        for (int i = 0; i < offsets.Length; i++)
        {
            byte b = x.ReadUInt8(0x28 + i) ?? 0;
            enabledItems += b * offsets[i];
        }

        return enabledItems;
    }

    private static EventPayload? ParseFrameStart(SlpDataReader x)
    {
        return new FrameStartPayload(
            new FrameStart(
                frame: x.ReadInt32(0x1),
                seed: x.ReadUInt32(0x5),
                sceneFrameCounter: x.ReadUInt32(0x9)
            )
        );
    }

    private static EventPayload? ParsePreFrameUpdate(SlpDataReader x)
    {
        return new PreFrameUpdatePayload(
            new PreFrameUpdate(
                frame: x.ReadInt32(0x1),
                playerIndex: x.ReadUInt8(0x5),
                isFollower: x.ReadBool(0x6),
                seed: x.ReadUInt32(0x7),
                actionStateId: x.ReadUInt16(0xb),
                positionX: x.ReadFloat(0xd),
                positionY: x.ReadFloat(0x11),
                facingDirection: x.ReadFloat(0x15),
                joystickX: x.ReadFloat(0x19),
                joystickY: x.ReadFloat(0x1d),
                cStickX: x.ReadFloat(0x21),
                cStickY: x.ReadFloat(0x25),
                trigger: x.ReadFloat(0x29),
                buttons: x.ReadUInt32(0x2d),
                physicalButtons: x.ReadUInt16(0x31),
                physicalLTrigger: x.ReadFloat(0x33),
                physicalRTrigger: x.ReadFloat(0x37),
                rawJoystickX: x.ReadInt8(0x3b),
                percent: x.ReadFloat(0x3c)
            )
        );
    }

    private static EventPayload? ParsePostFrameUpdate(SlpDataReader x)
    {
        SelfInducedSpeeds selfInducedSpeeds = new SelfInducedSpeeds(
            airX: x.ReadFloat(0x35),
            y: x.ReadFloat(0x39),
            attackX: x.ReadFloat(0x3d),
            attackY: x.ReadFloat(0x41),
            groundX: x.ReadFloat(0x45)
        );

        return new PostFrameUpdatePayload(
            new PostFrameUpdate(
                frame: x.ReadInt32(0x1),
                playerIndex: x.ReadUInt8(0x5),
                isFollower: x.ReadBool(0x6),
                internalCharacterId: x.ReadUInt8(0x7),
                actionStateId: x.ReadUInt16(0x8),
                positionX: x.ReadFloat(0xa),
                positionY: x.ReadFloat(0xe),
                facingDirection: x.ReadFloat(0x12),
                percent: x.ReadFloat(0x16),
                shieldSize: x.ReadFloat(0x1a),
                lastAttackLanded: x.ReadUInt8(0x1e),
                currentComboCount: x.ReadUInt8(0x1f),
                lastHitBy: x.ReadUInt8(0x20),
                stocksRemaining: x.ReadUInt8(0x21),
                actionStateCounter: x.ReadFloat(0x22),
                miscActionState: x.ReadFloat(0x2b),
                isAirborne: x.ReadBool(0x2f),
                lastGroundId: x.ReadUInt16(0x30),
                jumpsRemaining: x.ReadUInt8(0x32),
                lCancelStatus: x.ReadUInt8(0x33),
                hurtboxCollisionState: x.ReadUInt8(0x34),
                selfInducedSpeeds: selfInducedSpeeds,
                hitlagRemaining: x.ReadFloat(0x49),
                animationIndex: x.ReadUInt32(0x4d),
                instanceHitBy: x.ReadUInt16(0x51),
                instanceId: x.ReadUInt16(0x53)
            )
        ); 
    }

    private static EventPayload? ParseItemUpdate(SlpDataReader x)
    {
        return new ItemUpdatePayload(
            new ItemUpdate(
                frame: x.ReadInt32(0x1),
                typeId: x.ReadUInt16(0x5),
                state: x.ReadUInt8(0x7),
                facingDirection: x.ReadFloat(0x8),
                velocityX: x.ReadFloat(0xc),
                velocityY: x.ReadFloat(0x10),
                positionX: x.ReadFloat(0x14),
                positionY: x.ReadFloat(0x18),
                damageTaken: x.ReadUInt16(0x1c),
                expirationTimer: x.ReadFloat(0x1e),
                spawnId: x.ReadUInt32(0x22),
                missileType: x.ReadUInt8(0x26),
                turnipFace: x.ReadUInt8(0x27),
                chargeShotLaunched: x.ReadUInt8(0x28),
                chargePower: x.ReadUInt8(0x29),
                owner: x.ReadInt8(0x2a),
                instanceId: x.ReadUInt16(0x2b)
            )
        );
    }

    private static EventPayload? ParseFrameBookend(SlpDataReader x)
    {
        return new FrameBookendPayload(new FrameBookendType(frame: x.ReadInt32(0x1), latestFinalizedFrame: x.ReadInt32(0x5)));
    }

    private static EventPayload? ParseGameEnd(SlpDataReader x)
    {
        List<Placement> placements = new List<Placement>(4);
        for (int i = 0; i < 4; i++)
        {
            placements.Add(new Placement()
            {
                PlayerIndex = i,
                Position = x.ReadInt8(0x3 + i)
            });
        }

        return new GameEndPayload(
            new GameEnd(
                gameEndMethod: x.ReadUInt8(0x1).EnumCast<GameEndMethod>(),
                lrasInitiatorIndex: x.ReadInt8(0x2),
                placements
            )
        );
    }

    private static EventPayload ParseGeckoList(SlpDataReader x, Span<byte> payload)
    {
        List<GeckoCode> codes = [];
        int pos = 1;
        while (pos < payload.Length)
        {
            uint word1 = x.ReadUInt32(pos) ?? 0;
            uint codetype = word1 >> 24 & 0xfe;
            uint address = (word1 & 0x01ffffff) + 0x80000000;

            uint offset = 8; // Default code length, most codes are this length
            if (codetype == 0xc0 || codetype == 0xc2)
            {
                uint lineCount = x.ReadUInt32(pos + 4) ?? 0;
                offset = 8 + lineCount * 8;
            }
            else if (codetype == 0x06)
            {
                uint byteLen = x.ReadUInt32(pos + 4) ?? 0;
                offset = 8 + (byteLen + 7 & 0xfffffff8);
            }
            else if (codetype == 0x08)
            {
                offset = 16;
            }

            codes.Add(new GeckoCode(type: codetype, address: address, contents: payload.Slice(pos, (int)offset).ToArray()));

            pos += (int)offset;
        }

        return new GeckoListPayload(
            new GeckoCodeList(
                codes: codes,
                contents: payload.Slice(1).ToArray()
            )
        );
    }

    public void Dispose()
    {
        if (SlpRef is IDisposable disposableRef)
        {
            disposableRef.Dispose();
        }
    }
}
