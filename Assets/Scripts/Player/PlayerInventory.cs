using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private int bagSize = 30;
    [SerializeField] private GameObject[] carriedItemPrefabs;
    [SerializeField, Range(0f, 1f)] private float normalDropChance = 0.05f;
    [SerializeField, Range(0f, 1f)] private float rareDropChance = 0.25f;
    [SerializeField, Range(0f, 1f)] private float advancedDropChance = 0.4f;
    [SerializeField, Range(0f, 1f)] private float midDropChance = 0.3f;
    [SerializeField] private float dropRadius = 0.8f;
    private readonly List<WeaponData> weapons = new List<WeaponData>();
    private readonly List<WeaponData> storage = new List<WeaponData>();
    private readonly List<ConsumableData> consumables = new List<ConsumableData>();
    private readonly List<ConsumableData> storedConsumables = new List<ConsumableData>();
    private PlayerStats stats;
    public int BagSize => stats != null ? stats.BagSize : Mathf.Max(1, bagSize);
    public IReadOnlyList<WeaponData> Weapons => weapons;
    public IReadOnlyList<WeaponData> Storage => storage;
    public IReadOnlyList<ConsumableData> Consumables => consumables;
    public IReadOnlyList<ConsumableData> StoredConsumables => storedConsumables;
    public int ItemCount => weapons.Count + consumables.Count;
    public event Action Changed;
    private void Awake() { stats = GetComponent<PlayerStats>(); }

    public void SetWeapons(IEnumerable<WeaponData> data)
    {
        weapons.Clear();
        if (data != null) foreach (WeaponData weapon in data) if (weapon != null) weapons.Add(weapon);
        MoveOverflowToStorage();
        Changed?.Invoke();
    }

    public bool TryAddWeapon(WeaponData weapon)
    {
        var health = GetComponent<PlayerHealth>();
        if (weapon == null || ItemCount >= BagSize || (health != null && health.IsDead)) return false;
        weapons.Add(weapon);
        Changed?.Invoke();
        return true;
    }

    public void SortWeapons(bool byGrade)
    {
        weapons.Sort((a,b) => byGrade ? CompareGrade(a,b) : CompareName(a,b));
        storage.Sort((a,b) => byGrade ? CompareGrade(a,b) : CompareName(a,b));
        consumables.Sort((a,b) => string.Compare(a.DisplayName,b.DisplayName,StringComparison.Ordinal));
        storedConsumables.Sort((a,b) => string.Compare(a.DisplayName,b.DisplayName,StringComparison.Ordinal));
        Changed?.Invoke();
    }
    private static int CompareName(WeaponData a, WeaponData b)
    {
        int result = string.Compare(a.DisplayName,b.DisplayName,StringComparison.Ordinal);
        return result != 0 ? result : a.WeaponGrade.CompareTo(b.WeaponGrade);
    }
    private static int CompareGrade(WeaponData a, WeaponData b)
    {
        int result = a.WeaponGrade.CompareTo(b.WeaponGrade);
        return result != 0 ? result : CompareName(a,b);
    }

    public float GetDropChance(WeaponGrade grade)
    {
        switch (grade)
        {
            case WeaponGrade.Rare: return rareDropChance;
            case WeaponGrade.Legendary: return advancedDropChance;
            case WeaponGrade.Mid: return midDropChance;
            default: return normalDropChance;
        }
    }

    public List<WeaponData> DropWeaponsOnDeath(Func<float> roll = null)
    {
        var dropped = new List<WeaponData>();
        for (int i = weapons.Count - 1; i >= 0; i--)
        {
            WeaponData weapon = weapons[i];
            float chance = Mathf.Clamp01(GetDropChance(weapon.WeaponGrade));
            if (chance <= 0f || (chance < 1f && (roll != null ? roll() : UnityEngine.Random.value) >= chance)) continue;
            dropped.Add(weapon);
            weapons.RemoveAt(i);
            WorldPickup.SpawnWeapon(weapon, (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * dropRadius);
        }
        Changed?.Invoke();
        return dropped;
    }

    public void DropItemsOnDeath()
    {
        DropWeaponsOnDeath();
        if (carriedItemPrefabs == null) return;
        for (int i = 0; i < carriedItemPrefabs.Length && i < BagSize; i++)
        {
            GameObject prefab = carriedItemPrefabs[i];
            if (prefab == null) continue;
            DroppableItem item = prefab.GetComponent<DroppableItem>();
            float chance = item == null ? normalDropChance : item.ItemGrade == ItemGrade.Rare ? rareDropChance :
                item.ItemGrade == ItemGrade.Advanced ? advancedDropChance : item.ItemGrade == ItemGrade.Mid ? midDropChance : normalDropChance;
            if (UnityEngine.Random.value < chance) Instantiate(prefab, (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * dropRadius, Quaternion.identity);
        }
    }

    public void LoadPersistentWeapons(PlayerProgression progression, IEnumerable<WeaponData> catalog, IEnumerable<ConsumableData> consumableCatalog = null)
    {
        var known = new Dictionary<string,WeaponData>();
        foreach (WeaponData weapon in catalog) if (weapon != null) known[weapon.WeaponId] = weapon;
        var knownConsumables = new Dictionary<string,ConsumableData>();
        if (consumableCatalog != null) foreach (ConsumableData item in consumableCatalog) if (item != null) knownConsumables[item.ItemId] = item;
        weapons.Clear(); storage.Clear(); consumables.Clear(); storedConsumables.Clear();
        if (!progression.Data.inventoryInitialized)
        {
            foreach (WeaponData weapon in known.Values) weapons.Add(weapon);
            foreach (ConsumableData item in knownConsumables.Values) consumables.Add(item);
            progression.Data.inventoryInitialized = true;
        }
        else
        {
            foreach (string id in progression.Data.weaponIds) if (known.TryGetValue(id,out WeaponData weapon)) weapons.Add(weapon);
            foreach (string id in progression.Data.storedWeaponIds) if (known.TryGetValue(id,out WeaponData weapon)) storage.Add(weapon);
            foreach (string id in progression.Data.consumableIds) if (knownConsumables.TryGetValue(id,out ConsumableData item)) consumables.Add(item);
            foreach (string id in progression.Data.storedConsumableIds) if (knownConsumables.TryGetValue(id,out ConsumableData item)) storedConsumables.Add(item);
        }
        MoveOverflowToStorage();
        Changed?.Invoke();
        SavePersistentWeapons(progression);
    }

    public void SavePersistentWeapons(PlayerProgression progression)
    {
        progression.Data.weaponIds.Clear(); progression.Data.storedWeaponIds.Clear();
        progression.Data.consumableIds.Clear(); progression.Data.storedConsumableIds.Clear();
        foreach (WeaponData weapon in weapons) progression.Data.weaponIds.Add(weapon.WeaponId);
        foreach (WeaponData weapon in storage) progression.Data.storedWeaponIds.Add(weapon.WeaponId);
        foreach (ConsumableData item in consumables) progression.Data.consumableIds.Add(item.ItemId);
        foreach (ConsumableData item in storedConsumables) progression.Data.storedConsumableIds.Add(item.ItemId);
        progression.Save();
    }

    public void MoveOverflowToStorage()
    {
        while (ItemCount > BagSize && consumables.Count > 0) { storedConsumables.Add(consumables[consumables.Count-1]); consumables.RemoveAt(consumables.Count-1); }
        while (ItemCount > BagSize) { storage.Add(weapons[weapons.Count-1]); weapons.RemoveAt(weapons.Count-1); }
        Changed?.Invoke();
    }

    public bool Transfer(int index, bool fromStorage)
    {
        PlayerRunSession session = GetComponent<PlayerRunSession>();
        if (session == null || session.State != PlayerRunState.Lobby) return false;
        List<WeaponData> source = fromStorage ? storage : weapons;
        List<WeaponData> destination = fromStorage ? weapons : storage;
        if (index < 0 || index >= source.Count || (fromStorage && ItemCount >= BagSize)) return false;
        destination.Add(source[index]); source.RemoveAt(index);
        Changed?.Invoke();
        SavePersistentWeapons(session.Progression);
        return true;
    }

    public bool TryAddConsumable(ConsumableData item)
    {
        var health = GetComponent<PlayerHealth>();
        if (item == null || ItemCount >= BagSize || (health != null && health.IsDead)) return false;
        consumables.Add(item); Changed?.Invoke(); return true;
    }

    public bool UseConsumable(int index)
    {
        PlayerRunSession session = GetComponent<PlayerRunSession>();
        if (session == null || session.State != PlayerRunState.Running || index < 0 || index >= consumables.Count) return false;
        ConsumableData item = consumables[index];
        if (item.Effect == ConsumableEffect.Heal)
        {
            var health = GetComponent<PlayerHealth>();
            if (health == null || health.IsDead || health.CurrentHealth >= health.MaxHealth) return false;
            health.Heal(item.Amount);
        }
        else
        {
            var mana = GetComponent<PlayerMana>();
            if (mana == null || !mana.RestoreMana(item.Amount)) return false;
        }
        consumables.RemoveAt(index); Changed?.Invoke(); return true;
    }

    public bool TransferConsumable(int index, bool fromStorage)
    {
        PlayerRunSession session = GetComponent<PlayerRunSession>();
        if (session == null || session.State != PlayerRunState.Lobby) return false;
        var source = fromStorage ? storedConsumables : consumables;
        var destination = fromStorage ? consumables : storedConsumables;
        if (index < 0 || index >= source.Count || (fromStorage && ItemCount >= BagSize)) return false;
        destination.Add(source[index]); source.RemoveAt(index); Changed?.Invoke();
        SavePersistentWeapons(session.Progression); return true;
    }
}
