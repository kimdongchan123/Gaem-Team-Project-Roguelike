using UnityEngine;

// 임시 플레이 세션 데이터입니다. 파일 저장은 하지 않습니다.
public enum LobbyStatType
{
    Health,
    Attack,
    MoveSpeed
}

public static class LobbySessionData
{
    private const int StartingPoints = 5;
    private const int StartingStatLevel = 1;
    private const int MaxStatLevel = 10;

    public static int HealthLevel { get; private set; } = StartingStatLevel;
    public static int AttackLevel { get; private set; } = StartingStatLevel;
    public static int MoveSpeedLevel { get; private set; } = StartingStatLevel;
    public static int RemainingPoints { get; private set; } = StartingPoints;

    private static bool pendingDungeonEntry;

    // Domain Reload를 끈 에디터에서도 Play를 새로 시작하면 초기화합니다.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitializeForNewPlaySession()
    {
        ResetStats();
        pendingDungeonEntry = false;
    }

    public static void ResetStats()
    {
        HealthLevel = StartingStatLevel;
        AttackLevel = StartingStatLevel;
        MoveSpeedLevel = StartingStatLevel;
        RemainingPoints = StartingPoints;
    }

    public static bool CanUpgrade(LobbyStatType stat)
    {
        if (RemainingPoints <= 0)
        {
            return false;
        }

        switch (stat)
        {
            case LobbyStatType.Health:
                return HealthLevel < MaxStatLevel;
            case LobbyStatType.Attack:
                return AttackLevel < MaxStatLevel;
            case LobbyStatType.MoveSpeed:
                return MoveSpeedLevel < MaxStatLevel;
            default:
                return false;
        }
    }

    public static bool TryUpgrade(LobbyStatType stat)
    {
        if (!CanUpgrade(stat))
        {
            return false;
        }

        switch (stat)
        {
            case LobbyStatType.Health:
                HealthLevel++;
                break;
            case LobbyStatType.Attack:
                AttackLevel++;
                break;
            case LobbyStatType.MoveSpeed:
                MoveSpeedLevel++;
                break;
            default:
                return false;
        }

        RemainingPoints--;
        return true;
    }

    public static void RequestDungeonEntry()
    {
        pendingDungeonEntry = true;
    }

    public static bool ConsumeDungeonEntryRequest()
    {
        bool wasRequested = pendingDungeonEntry;
        pendingDungeonEntry = false;
        return wasRequested;
    }
}
