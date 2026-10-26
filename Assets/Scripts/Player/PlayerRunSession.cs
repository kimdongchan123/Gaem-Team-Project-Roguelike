using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum PlayerRunState { Lobby, Running, Dead }

public class PlayerRunResult
{
    public float survivalSeconds;
    public int earnedCrystals, finalCrystals, retainedWeapons, normalKills, eliteKills, floor;
    public string location;
    public readonly List<string> droppedWeapons = new List<string>();
}

public class PlayerRunSession : MonoBehaviour
{
    public PlayerProgression Progression { get; private set; }
    public PlayerRunState State { get; private set; } = PlayerRunState.Lobby;
    public PlayerRunResult Result { get; private set; }
    public int RunCrystals { get; private set; }
    public int NormalKills { get; private set; }
    public int EliteKills { get; private set; }
    public int Floor { get; private set; } = 1;
    public string Location { get; private set; } = "샘플 전투장";
    public bool MenuOpen { get; private set; }
    public bool CanAct => State == PlayerRunState.Running && !MenuOpen;
    public bool UsesSceneFlow { get; private set; }
    public bool CanMove => !MenuOpen && (State == PlayerRunState.Running || (UsesSceneFlow && State == PlayerRunState.Lobby));
    private string lobbySceneName = "LobbyScene";
    private string dungeonSceneName = "RoomSystemTest";
    public float SurvivalSeconds => State == PlayerRunState.Running ? Mathf.Max(0f, Time.time - startedAt) : Result != null ? Result.survivalSeconds : 0f;
    public event Action RunStarted;
    public event Action LobbyEntered;
    private float startedAt;
    private float priorTimeScale = 1f;
    private PlayerInventory inventory;
    private PlayerStats stats;

    public void Initialize(PlayerProgression progression, IEnumerable<WeaponData> catalog, IEnumerable<ConsumableData> consumableCatalog = null)
    {
        Progression = progression;
        inventory = GetComponent<PlayerInventory>();
        stats = GetComponent<PlayerStats>();
        if (stats != null) stats.Apply(progression.Data);
        inventory.LoadPersistentWeapons(progression, catalog, consumableCatalog);
        ApplyUpgrades();
    }

    public void ConfigureSceneFlow(string lobby, string dungeon)
    {
        UsesSceneFlow = true;
        lobbySceneName = lobby;
        dungeonSceneName = dungeon;
    }

    public bool EnterDungeon()
    {
        if (!UsesSceneFlow || State != PlayerRunState.Lobby || MenuOpen || !Application.CanStreamedLevelBeLoaded(dungeonSceneName)) return false;
        inventory.SavePersistentWeapons(Progression);
        LobbySessionData.RequestDungeonEntry();
        SceneManager.LoadScene(dungeonSceneName);
        return true;
    }

    public bool BeginRun(bool resetPosition = true)
    {
        if (Progression == null || State != PlayerRunState.Lobby) return false;
        SetMenuOpen(false);
        ApplyUpgrades();
        GetComponent<PlayerHealth>().ResetHealth();
        GetComponent<PlayerMana>().ResetMana();
        var movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.ResetMovement();
        var controller = GetComponent<PlayerWeaponController>();
        if (controller != null) controller.ResetCooldown();
        if (resetPosition) transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        RunCrystals = NormalKills = EliteKills = 0;
        Floor = 1;
        Location = "샘플 전투장";
        Result = null;
        State = PlayerRunState.Running;
        startedAt = Time.time;
        RunStarted?.Invoke();
        return true;
    }

    public void RecordKill(bool elite)
    {
        if (State != PlayerRunState.Running) return;
        if (elite) EliteKills++; else NormalKills++;
    }

    public bool CollectCrystals(int amount)
    {
        if (!CanAct || amount <= 0) return false;
        RunCrystals += amount;
        return true;
    }

    public void SetStage(int floor, string location)
    {
        if (State != PlayerRunState.Running) return;
        Floor = Mathf.Max(1, floor);
        Location = location ?? "";
    }

    public void CompleteDeath()
    {
        if (State != PlayerRunState.Running || Progression == null) return;
        float elapsed = SurvivalSeconds;
        State = PlayerRunState.Dead;
        SetMenuOpen(false);
        List<WeaponData> dropped = inventory.DropWeaponsOnDeath();
        Progression.Data.crystals += RunCrystals;
        inventory.SavePersistentWeapons(Progression);
        Result = new PlayerRunResult { survivalSeconds = elapsed, earnedCrystals = RunCrystals,
            finalCrystals = Progression.Data.crystals, retainedWeapons = inventory.Weapons.Count,
            normalKills = NormalKills, eliteKills = EliteKills, floor = Floor, location = Location };
        foreach (WeaponData weapon in dropped) Result.droppedWeapons.Add(weapon.DisplayName);
    }

    public bool ReturnToLobby()
    {
        if (State != PlayerRunState.Dead) return false;
        if (UsesSceneFlow && !Application.CanStreamedLevelBeLoaded(lobbySceneName))
        {
            Debug.LogError($"로비 씬을 불러올 수 없습니다: {lobbySceneName}", this);
            return false;
        }
        SetMenuOpen(false);
        State = PlayerRunState.Lobby;
        ApplyUpgrades();
        LobbyEntered?.Invoke();
        if (UsesSceneFlow) SceneManager.LoadScene(lobbySceneName);
        return true;
    }

    public bool Upgrade(PlayerUpgrade upgrade)
    {
        if (State != PlayerRunState.Lobby || Progression == null || !Progression.TryUpgrade(upgrade)) return false;
        ApplyUpgrades();
        return true;
    }

    public bool ResetUpgrades()
    {
        if (State != PlayerRunState.Lobby || Progression == null) return false;
        Progression.ResetUpgrades();
        ApplyUpgrades();
        inventory.MoveOverflowToStorage();
        inventory.SavePersistentWeapons(Progression);
        return true;
    }

    public void SetMenuOpen(bool open)
    {
        if (MenuOpen == open) return;
        MenuOpen = open;
        if (open) { priorTimeScale = Time.timeScale; Time.timeScale = 0f; }
        else Time.timeScale = priorTimeScale;
    }

    private void ApplyUpgrades()
    {
        if (stats != null) stats.Apply(Progression.Data);
        var hp = GetComponent<PlayerHealth>();
        if (hp != null) hp.ResetHealth();
        var mp = GetComponent<PlayerMana>();
        if (mp != null) mp.ResetMana();
    }

    private void OnDestroy() { if (MenuOpen) Time.timeScale = priorTimeScale; }
}
