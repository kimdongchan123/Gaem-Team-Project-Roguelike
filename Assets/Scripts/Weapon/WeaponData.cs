using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponData", order = 2)]
public class WeaponData : ScriptableObject
{
    [Header("Basic")]
    [SerializeField] private string weaponName;
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

    public string WeaponName => weaponName;
    public WeaponGrade WeaponGrade => weaponGrade;
    public WeaponAttackType AttackType => attackType;
    public bool IsMagicWeapon => isMagicWeapon;
    public int Damage => damage;
    public float AttackDelay => attackDelay;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileLifeTime => projectileLifeTime;
    public int ManaCost => manaCost;
    public int ProjectileCount => projectileCount;
    public float SpreadAngle => spreadAngle;
    public float ExplosionRadius => explosionRadius;
    public float MeleeRange => meleeRange;
    public float MeleeAngle => meleeAngle;
    public float EffectDuration => effectDuration;

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
