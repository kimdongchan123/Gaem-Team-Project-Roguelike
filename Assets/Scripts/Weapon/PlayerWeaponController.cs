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

    public WeaponData CurrentWeapon
    {
        get
        {
            if (weapons == null || weapons.Length == 0)
            {
                return null;
            }

            return weapons[currentWeaponIndex];
        }
    }

    public string CurrentWeaponName => CurrentWeapon != null ? CurrentWeapon.WeaponName : "None";
    public int CurrentWeaponNumber => weapons != null && weapons.Length > 0 ? currentWeaponIndex + 1 : 0;
    public int WeaponCount => weapons != null ? weapons.Length : 0;

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
    }

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerMana = GetComponent<PlayerMana>();

        if (weapons == null || weapons.Length == 0)
        {
            currentWeaponIndex = 0;
            return;
        }

        currentWeaponIndex = Mathf.Clamp(startWeaponIndex, 0, weapons.Length - 1);
    }

    private void Update()
    {
        if (playerHealth != null && playerHealth.IsDead)
        {
            return;
        }

        TrySwitchWeapon();
        TryAttack();
    }

    private void TrySwitchWeapon()
    {
        if (Keyboard.current == null || !Keyboard.current.qKey.wasPressedThisFrame)
        {
            return;
        }

        if (weapons == null || weapons.Length <= 1)
        {
            return;
        }

        currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;
        Log($"Weapon Changed: {CurrentWeapon.WeaponName}");
    }

    private void TryAttack()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.isPressed)
        {
            return;
        }

        WeaponData weapon = CurrentWeapon;
        if (weapon == null || projectilePrefab == null || firePoint == null)
        {
            return;
        }

        if (Time.time < nextAttackTime)
        {
            return;
        }

        if (Attack(weapon))
        {
            nextAttackTime = Time.time + weapon.AttackDelay;
        }
    }

    private bool Attack(WeaponData weapon)
    {
        if (weapon.IsMagicWeapon && playerMana != null && !playerMana.TryUseMana(weapon.ManaCost))
        {
            Log("Not enough mana.");
            return false;
        }

        if (weapon.AttackType == WeaponAttackType.Melee)
        {
            AttackMelee(weapon);
            return true;
        }

        FireProjectiles(weapon);
        return true;
    }

    private void FireProjectiles(WeaponData weapon)
    {
        int projectileCount = Mathf.Max(weapon.ProjectileCount, 1);
        float startAngle = -(weapon.SpreadAngle * 0.5f);
        float angleStep = projectileCount > 1 ? weapon.SpreadAngle / (projectileCount - 1) : 0f;

        for (int i = 0; i < projectileCount; i++)
        {
            float projectileAngle = startAngle + (angleStep * i);
            Quaternion rotation = firePoint.rotation * Quaternion.Euler(0f, 0f, projectileAngle);
            PlayerProjectile projectile = Instantiate(projectilePrefab, firePoint.position, rotation);
            int finalDamage = Mathf.Max(1, Mathf.RoundToInt(weapon.Damage * attackPowerMultiplier));

            projectile.gameObject.SetActive(true);
            projectile.Initialize(weapon, finalDamage, GetMouseWorldPosition(), playerHealth);
        }
    }

    private void AttackMelee(WeaponData weapon)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(firePoint.position, weapon.MeleeRange, targetLayer);
        foreach (Collider2D hitCollider in hitColliders)
        {
            Vector2 targetDirection = hitCollider.transform.position - transform.position;
            float targetAngle = Vector2.Angle(transform.right, targetDirection);
            if (targetAngle > weapon.MeleeAngle * 0.5f)
            {
                continue;
            }

            MonsterHealth monsterHealth = hitCollider.GetComponent<MonsterHealth>();
            if (monsterHealth != null)
            {
                int finalDamage = Mathf.Max(1, Mathf.RoundToInt(weapon.Damage * attackPowerMultiplier));
                monsterHealth.TakeDamage(finalDamage);
            }
        }
    }

    private Vector2 GetMouseWorldPosition()
    {
        if (Mouse.current == null || Camera.main == null)
        {
            return firePoint.position;
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
