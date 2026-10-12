using UnityEngine;
using UnityEngine.Serialization;

public class MonsterHealth : MonoBehaviour
{
    [Header("Health")]
    [FormerlySerializedAs("maxHealth")]
    [SerializeField] private int maxHp = 3;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog;

    private int currentHp;
    private bool isDead;

    public int CurrentHealth => currentHp;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHp = maxHp;
    }

    public void Init(MonsterData data)
    {
        maxHp = Mathf.RoundToInt(data.maxHp);
        currentHp = maxHp;
    }

    public void TakeDamage(int damageAmount)
    {
        ApplyDamage(damageAmount);
    }

    public void TakeDamage(int damageAmount, WeaponDamageType weaponType)
    {
        EliteMonster elite = GetComponent<EliteMonster>();
        if (elite != null)
        {
            damageAmount = elite.ApplyResistance(damageAmount, weaponType);
        }

        ApplyDamage(damageAmount);
    }

    private void ApplyDamage(int damageAmount)
    {
        if (isDead || damageAmount <= 0)
        {
            return;
        }

        currentHp -= damageAmount;
        Log($"Monster Hit: {currentHp}/{maxHp}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        currentHp = 0;
        Log("Monster Dead");
        Destroy(gameObject);
    }

    private void Log(string message)
    {
        if (showDebugLog)
        {
            Debug.Log(message);
        }
    }
}