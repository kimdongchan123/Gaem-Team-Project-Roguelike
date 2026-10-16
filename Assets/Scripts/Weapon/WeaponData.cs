using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponData", order = 2)]
public class WeaponData : ScriptableObject
{
    [Header("Basic")]
    [SerializeField] private string weaponName;
    [SerializeField] private string weaponId;
    [SerializeField] private WeaponGrade weaponGrade = WeaponGrade.Normal;
    [SerializeField] private WeaponAttackType attackType = WeaponAttackType.StraightProjectile;
    [SerializeField] private bool isMagicWeapon;

    [Header("Attack")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackDelay = 0.15f;
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private float projectileLifeTime = 1.5f;
    [SerializeField] private int manaCost = 0;

    [Header("Spread")]
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private float spreadAngle = 0f;

    [Header("Special")]
    [SerializeField] private float explosionRadius = 1.2f;
    [SerializeField] private float meleeRange = 1f;
    [SerializeField] private float meleeAngle = 90f;
    [SerializeField] private float effectDuration = 1.5f;
    [SerializeField, Min(0.01f)] private float hitRadius = 0.1f;
    [SerializeField, Min(0.05f)] private float areaDamageInterval = 0.5f;
    [SerializeField, Min(0f)] private float homingTurnSpeed = 360f;
    [SerializeField, Min(0f)] private float targetRange = 10f;

    public string WeaponName => weaponName;
    public string DisplayName => PlayerUITheme.DisplayName(weaponName);
    public string GradeName => PlayerUITheme.GradeName(weaponGrade);
    public string WeaponId => string.IsNullOrEmpty(weaponId) ? weaponName : weaponId;
    public WeaponGrade WeaponGrade => weaponGrade;
    public WeaponAttackType AttackType => attackType;
    public bool IsMagicWeapon => isMagicWeapon;
    public int Damage => Mathf.Max(1, damage);
    public float AttackDelay => Mathf.Max(0.02f, attackDelay);
    public float ProjectileSpeed => Mathf.Max(0f, projectileSpeed);
    public float ProjectileLifeTime => Mathf.Max(0f, projectileLifeTime);
    public int ManaCost => Mathf.Max(0, manaCost);
    public int ProjectileCount => Mathf.Max(1, projectileCount);
    public float SpreadAngle => Mathf.Clamp(spreadAngle, 0f, 180f);
    public float ExplosionRadius => Mathf.Max(0f, explosionRadius);
    public float MeleeRange => Mathf.Max(0f, meleeRange);
    public float MeleeAngle => Mathf.Clamp(meleeAngle, 0f, 360f);
    public float EffectDuration => Mathf.Max(0f, effectDuration);
    public float HitRadius => Mathf.Max(0.01f, hitRadius);
    public float AreaDamageInterval => Mathf.Max(0.05f, areaDamageInterval);
    public float HomingTurnSpeed => Mathf.Max(0f, homingTurnSpeed);
    public float TargetRange => Mathf.Max(0f, targetRange);

    public void SetTestData(
        string newWeaponName,
        WeaponGrade newWeaponGrade,
        WeaponAttackType newAttackType,
        bool newIsMagicWeapon,
        int newDamage,
        float newAttackDelay,
        float newProjectileSpeed,
        float newProjectileLifeTime,
        int newManaCost,
        int newProjectileCount,
        float newSpreadAngle,
        float newExplosionRadius,
        float newMeleeRange,
        float newMeleeAngle,
        float newEffectDuration)
    {
        weaponName = newWeaponName;
        weaponGrade = newWeaponGrade;
        attackType = newAttackType;
        isMagicWeapon = newIsMagicWeapon;
        damage = newDamage;
        attackDelay = newAttackDelay;
        projectileSpeed = newProjectileSpeed;
        projectileLifeTime = newProjectileLifeTime;
        manaCost = newManaCost;
        projectileCount = newProjectileCount;
        spreadAngle = newSpreadAngle;
        explosionRadius = newExplosionRadius;
        meleeRange = newMeleeRange;
        meleeAngle = newMeleeAngle;
        effectDuration = newEffectDuration;
    }
}
