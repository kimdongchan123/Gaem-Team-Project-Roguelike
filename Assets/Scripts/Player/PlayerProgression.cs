using System;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerUpgrade { Health, Mana, MoveSpeed, BagSize, AttackPower, DashCooldown }

[Serializable]
public class PlayerProgressionData
{
    public int crystals;
    public int[] levels = new int[6];
    public bool inventoryInitialized;
    public List<string> weaponIds = new List<string>();
    public List<string> storedWeaponIds = new List<string>();
    public List<string> consumableIds = new List<string>();
    public List<string> storedConsumableIds = new List<string>();
}

// Storage is injected so automated tests never change the player's real save.
public class PlayerProgression
{
    private readonly string saveKey;
    public PlayerProgressionData Data { get; private set; }
    public static readonly int[] MaxLevels = { 7, 100, 10, 70, 5, 6 };

    public PlayerProgression(string key = "LastShelter.Player.v1")
    {
        saveKey = key;
        try { Data = JsonUtility.FromJson<PlayerProgressionData>(PlayerPrefs.GetString(key, "")); }
        catch (ArgumentException) { Data = null; }
        if (Data == null) Data = new PlayerProgressionData();
        if (Data.levels == null || Data.levels.Length != 6) Data.levels = new int[6];
        for (int i = 0; i < 6; i++) Data.levels[i] = Mathf.Clamp(Data.levels[i], 0, MaxLevels[i]);
        Data.crystals = Mathf.Max(0, Data.crystals);
        if (Data.weaponIds == null) Data.weaponIds = new List<string>();
        if (Data.storedWeaponIds == null) Data.storedWeaponIds = new List<string>();
        if (Data.consumableIds == null) Data.consumableIds = new List<string>();
        if (Data.storedConsumableIds == null) Data.storedConsumableIds = new List<string>();
    }

    public bool TryUpgrade(PlayerUpgrade upgrade)
    {
        int index = (int)upgrade;
        if (index < 0 || index >= 6 || Data.crystals < 1 || Data.levels[index] >= MaxLevels[index]) return false;
        Data.levels[index]++;
        Data.crystals--;
        Save();
        return true;
    }

    public int ResetUpgrades()
    {
        int refund = 0;
        for (int i = 0; i < 6; i++) { refund += Data.levels[i]; Data.levels[i] = 0; }
        Data.crystals += refund;
        Save();
        return refund;
    }

    public void Save() { PlayerPrefs.SetString(saveKey, JsonUtility.ToJson(Data)); PlayerPrefs.Save(); }
}
