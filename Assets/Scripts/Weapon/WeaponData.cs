using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "ScriptableObjects/WeaponData", order = 2)]
public class WeaponData : ScriptableObject
{
    [Header("Basic")]
    [SerializeField] private string weaponName;
    [SerializeField] private WeaponGrade weaponGrade = WeaponGrade.Normal;

    [Header("Attack")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackDelay = 0.15f;
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private float projectileLifeTime = 1.5f;

    [Header("Spread")]
    [SerializeField] private int projectileCount = 1;
    [SerializeField] private float spreadAngle = 0f;

    public string WeaponName => weaponName;
    public WeaponGrade WeaponGrade => weaponGrade;
    public int Damage => damage;
    public float AttackDelay => attackDelay;
    public float ProjectileSpeed => projectileSpeed;
    public float ProjectileLifeTime => projectileLifeTime;
    public int ProjectileCount => projectileCount;
    public float SpreadAngle => spreadAngle;

    public void SetTestData(
        string newWeaponName,
        WeaponGrade newWeaponGrade,
        int newDamage,
        float newAttackDelay,
        float newProjectileSpeed,
        float newProjectileLifeTime,
        int newProjectileCount,
        float newSpreadAngle)
    {
        weaponName = newWeaponName;
        weaponGrade = newWeaponGrade;
        damage = newDamage;
        attackDelay = newAttackDelay;
        projectileSpeed = newProjectileSpeed;
        projectileLifeTime = newProjectileLifeTime;
        projectileCount = newProjectileCount;
        spreadAngle = newSpreadAngle;
    }
}
