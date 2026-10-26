using UnityEngine;

// Preserve the existing NPC's serialized UI reference, but open the real saved upgrades.
public class LobbyStatsUI : MonoBehaviour
{
    private PlayerMenuUI menu;
    private PlayerMenuUI Menu
    {
        get { if (menu == null) menu = FindAnyObjectByType<PlayerMenuUI>(); return menu; }
    }
    public bool IsOpen => Menu != null && Menu.IsLobbyPanelOpen;
    public void Open() { if (Menu != null) Menu.OpenLobbyUpgrades(); }
    public void Close() { if (Menu != null) Menu.CloseLobbyUpgrades(); }
    public void Toggle() { if (IsOpen) Close(); else Open(); }
    private void OnDisable() { Close(); }
}
