using UnityEngine;

// 로비 NPC/단말기에서 상호작용할 때만 열리는 임시 스탯 UI입니다.
// 실제 플레이어 능력치에는 아직 적용하지 않습니다.
public class LobbyStatsUI : MonoBehaviour
{
    private bool isOpen;

    public bool IsOpen => isOpen;

    public void Open()
    {
        isOpen = true;
    }

    public void Close()
    {
        isOpen = false;
    }

    public void Toggle()
    {
        isOpen = !isOpen;
    }

    private void OnDisable()
    {
        Close();
    }

    private void OnGUI()
    {
        if (!isOpen)
        {
            return;
        }

        float width = Mathf.Min(340f, Mathf.Max(180f, Screen.width - 24f));
        float height = Mathf.Min(300f, Mathf.Max(100f, Screen.height - 24f));
        Rect area = new Rect(12f, 12f, width, height);

        GUILayout.BeginArea(area, GUI.skin.box);
        GUILayout.Label("LOBBY - STAT TEST");
        GUILayout.Label($"남은 스탯 포인트: {LobbySessionData.RemainingPoints}");
        GUILayout.Space(10f);

        DrawStatRow("체력", LobbyStatType.Health, LobbySessionData.HealthLevel);
        DrawStatRow("공격력", LobbyStatType.Attack, LobbySessionData.AttackLevel);
        DrawStatRow("이동속도", LobbyStatType.MoveSpeed, LobbySessionData.MoveSpeedLevel);

        GUILayout.Space(8f);
        if (GUILayout.Button("스탯 초기화"))
        {
            LobbySessionData.ResetStats();
        }

        if (GUILayout.Button("닫기"))
        {
            Close();
        }

        GUILayout.Label("임시 스탯 데이터만 변경됩니다.");
        GUILayout.EndArea();
    }

    private void DrawStatRow(string label, LobbyStatType stat, int level)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label} : Lv.{level}", GUILayout.ExpandWidth(true));

        bool wasEnabled = GUI.enabled;
        GUI.enabled = LobbySessionData.CanUpgrade(stat);

        if (GUILayout.Button("+", GUILayout.Width(42f)))
        {
            LobbySessionData.TryUpgrade(stat);
        }

        GUI.enabled = wasEnabled;
        GUILayout.EndHorizontal();
    }
}
