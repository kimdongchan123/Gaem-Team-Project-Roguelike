using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerWeaponController : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private WeaponData[] weapons;
    [SerializeField] private int startWeaponIndex;
    [SerializeField] private Transform firePoint;
    [SerializeField] private PlayerProjectile projectilePrefab;
    [SerializeField] private float attackPowerMultiplier = 1f;
    [SerializeField] private LayerMask targetLayer;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog;

    private PlayerHealth playerHealth;
    private PlayerMana playerMana;
    private int currentWeaponIndex;
    private float nextAttackTime;
    private PlayerInventory inventory;
    private PlayerStats stats;
    private PlayerRunSession session;
    private PlayerMovement movement;
    private WeaponData equippedWeapon;
    private IReadOnlyList<WeaponData> WeaponList => inventory != null ? inventory.Weapons : weapons;
    private float AttackMultiplier => stats != null ? stats.AttackMultiplier : attackPowerMultiplier;
    private readonly HashSet<MonsterHealth> meleeTargets = new HashSet<MonsterHealth>();

    public WeaponData CurrentWeapon
    {
        get
        {
            if (WeaponList == null || WeaponList.Count == 0)
            {
                return null;
            }

            return WeaponList[Mathf.Clamp(currentWeaponIndex, 0, WeaponList.Count - 1)];
        }
    }

    public string CurrentWeaponName => CurrentWeapon != null ? CurrentWeapon.WeaponName : "None";
    public int CurrentWeaponNumber => WeaponCount > 0 ? currentWeaponIndex + 1 : 0;
    public int WeaponCount => WeaponList != null ? WeaponList.Count : 0;
    public bool HasEnoughMana => CurrentWeapon == null || !CurrentWeapon.IsMagicWeapon || CurrentWeapon.ManaCost == 0 ||
        (playerMana != null && playerMana.CurrentMana >= CurrentWeapon.ManaCost);
    public float AttackCooldownRemaining => Mathf.Max(0f, nextAttackTime - Time.time);

    public void SetTestWeaponData(
        WeaponData[] newWeapons,
        Transform newFirePoint,
        PlayerProjectile newProjectilePrefab,
        LayerMask newTargetLayer)
    {
        weapons = newWeapons;
        firePoint = newFirePoint;
        projectilePrefab = newProjectilePrefab;
        targetLayer = newTargetLayer;
        currentWeaponIndex = 0;
        if (inventory != null) inventory.SetWeapons(newWeapons);
        equippedWeapon = CurrentWeapon;
    }

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMana = GetComponent<PlayerMana>();
        inventory = GetComponent<PlayerInventory>();
        stats = GetComponent<PlayerStats>();
        session = GetComponent<PlayerRunSession>();
        movement = GetComponent<PlayerMovement>();
        if (inventory != null)
        {
            inventory.Changed += SyncInventory;
            if (weapons != null) inventory.SetWeapons(weapons);
        }

        if (weapons == null || weapons.Length == 0)
        {
            currentWeaponIndex = 0;
            return;
        }

        currentWeaponIndex = Mathf.Clamp(startWeaponIndex, 0, weapons.Length - 1);
    }

    private void Update()
    {
        if ((playerHealth != null && playerHealth.IsDead) || (session != null && !session.CanAct))
        {
            return;
        }

        TrySwitchWeapon();
        TryAttack();
    }

    private void TrySwitchWeapon()
    {
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame) SwitchWeapon(1);
        else if (Mouse.current != null && Mathf.Abs(Mouse.current.scroll.y.ReadValue()) > 0f)
            SwitchWeapon(Mouse.current.scroll.y.ReadValue() > 0f ? 1 : -1);
    }

    public bool SwitchWeapon(int direction)
    {
        if (WeaponCount <= 1 || (playerHealth != null && playerHealth.IsDead) || (session != null && !session.CanAct)) return false;
        currentWeaponIndex = (currentWeaponIndex + (direction >= 0 ? 1 : -1) + WeaponCount) % WeaponCount;
        equippedWeapon = CurrentWeapon;
        return true;
    }

    public bool Equip(int index)
    {
        if (index < 0 || index >= WeaponCount || (session != null && session.State == PlayerRunState.Dead)) return false;
        currentWeaponIndex = index; equippedWeapon = CurrentWeapon; return true;
    }

    public void ResetCooldown() { nextAttackTime = 0f; }

    private void SyncInventory()
    {
        currentWeaponIndex = Mathf.Clamp(currentWeaponIndex, 0, Mathf.Max(0, WeaponCount - 1));
        for (int i = 0; i < WeaponCount; i++) if (WeaponList[i] == equippedWeapon) { currentWeaponIndex = i; break; }
        equippedWeapon = CurrentWeapon;
    }

    private void OnDestroy() { if (inventory != null) inventory.Changed -= SyncInventory; }

    private void TryAttack()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.isPressed)
        {
            return;
        }

        TryAttackAt(GetMouseWorldPosition());
    }

    public bool TryAttackAt(Vector2 targetPosition)
    {
        WeaponData weapon = CurrentWeapon;
        if ((playerHealth != null && playerHealth.IsDead) || (session != null && !session.CanAct) || weapon == null || firePoint == null ||
            Time.time < nextAttackTime ||
            (weapon.AttackType != WeaponAttackType.Melee && projectilePrefab == null))
        {
            return false;
        }

        if (weapon.IsMagicWeapon && weapon.ManaCost > 0 &&
            (playerMana == null || !playerMana.TryUseMana(weapon.ManaCost)))
        {
            Log("Not enough mana.");
            return false;
        }

        if (weapon.AttackType == WeaponAttackType.Melee)
        {
            AttackMelee(weapon);
        }
        else
        {
            FireProjectiles(weapon, targetPosition);
        }

        nextAttackTime = Time.time + weapon.AttackDelay;
        return true;
    }

    private void FireProjectiles(WeaponData weapon, Vector2 targetPosition)
    {
        int projectileCount = Mathf.Max(weapon.ProjectileCount, 1);
        float startAngle = projectileCount > 1 ? -(weapon.SpreadAngle * 0.5f) : 0f;
        float angleStep = projectileCount > 1 ? weapon.SpreadAngle / (projectileCount - 1) : 0f;

        for (int i = 0; i < projectileCount; i++)
        {
            float projectileAngle = startAngle + (angleStep * i);
            Quaternion rotation = firePoint.rotation * Quaternion.Euler(0f, 0f, projectileAngle);
            PlayerProjectile projectile = Instantiate(projectilePrefab, firePoint.position, rotation);
            float finalDamage = Mathf.Max(0f, weapon.Damage * AttackMultiplier);

            projectile.SetTargetLayer(targetLayer);
            projectile.Initialize(weapon, finalDamage, targetPosition, playerHealth);
            projectile.gameObject.SetActive(true);
        }
    }

    private void AttackMelee(WeaponData weapon)
    {
        meleeTargets.Clear();
        // Range and facing both use the player as the origin.
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, weapon.MeleeRange, targetLayer);
        foreach (Collider2D hitCollider in hitColliders)
        {
            Vector2 targetDirection = hitCollider.ClosestPoint(transform.position) - (Vector2)transform.position;
            Vector2 facing = movement != null ? movement.AimDirection : (Vector2)firePoint.right;
            float targetAngle = Vector2.Angle(facing, targetDirection);
            if (targetAngle > weapon.MeleeAngle * 0.5f)
            {
                continue;
            }

            MonsterHealth monsterHealth = hitCollider.GetComponentInParent<MonsterHealth>();
            if (monsterHealth != null && !monsterHealth.IsDead && meleeTargets.Add(monsterHealth))
            {
                float finalDamage = Mathf.Max(0f, weapon.Damage * AttackMultiplier);
                monsterHealth.TakeDamage(finalDamage, session);
            }
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        if (Mouse.current == null || Camera.main == null)
        {
            return firePoint != null ? (Vector2)firePoint.position : (Vector2)transform.position;
        }

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    private void Log(string message)
    {
        if (showDebugLog)
        {
            Debug.Log(message);
        }
    }
}
