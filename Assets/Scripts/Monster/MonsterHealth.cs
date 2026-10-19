using UnityEngine;
using UnityEngine.Serialization;

public class MonsterHealth : MonoBehaviour
{
    [Header("Health")]
    [FormerlySerializedAs("maxHealth")]
    [SerializeField] private int maxHp;

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

    private void Start()
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
            Die(attacker);
        }
    }

    private void Die(PlayerRunSession attacker)
    {
        isDead = true;
        currentHp = 0;
        Log("Monster Dead");
        if (attacker != null && attacker.State == PlayerRunState.Running)
        {
            attacker.RecordKill(isElite);
            WorldPickup.SpawnCrystals(isElite ? 3 : 1, transform.position);
        }
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