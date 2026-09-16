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

    [Header("Debug")]
    [SerializeField] private bool showDebugLog;

    private PlayerHealth playerHealth;
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

    public void SetTestWeaponData(WeaponData[] newWeapons, Transform newFirePoint, PlayerProjectile newProjectilePrefab)
    {
        weapons = newWeapons;
        firePoint = newFirePoint;
        projectilePrefab = newProjectilePrefab;
        currentWeaponIndex = 0;
    }

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();

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

        nextAttackTime = Time.time + weapon.AttackDelay;
        FireProjectiles(weapon);
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
            projectile.Initialize(finalDamage, weapon.ProjectileSpeed, weapon.ProjectileLifeTime);
        }
    }

    private void Log(string message)
    {
        if (showDebugLog)
        {
            Debug.Log(message);
        }
    }
}
