using UnityEngine;
using UnityEngine.Serialization;

public class MonsterHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private bool isElite;
    public bool IsElite => isElite;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog;

    private float currentHealth;
    private bool isDead;

    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void Init(MonsterData data)
    {
        maxHealth = Mathf.RoundToInt(data.maxHp);
        currentHealth = maxHealth;
    }

    private void Start()
    {
        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite == null || sprite.sprite == null) return;
        GameObject hurtbox = new GameObject("Damage Hurtbox");
        hurtbox.layer = gameObject.layer;
        hurtbox.transform.SetParent(transform, false);
        hurtbox.transform.localPosition = transform.InverseTransformPoint(sprite.bounds.center);
        Vector3 scale = transform.lossyScale;
        BoxCollider2D collider = hurtbox.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(sprite.bounds.size.x / Mathf.Max(0.001f, Mathf.Abs(scale.x)) + 0.15f,
            sprite.bounds.size.y / Mathf.Max(0.001f, Mathf.Abs(scale.y)) + 0.15f);
    }

    public void TakeDamage(float damageAmount, PlayerRunSession attacker = null)
    {
        if (isDead || damageAmount <= 0 || float.IsNaN(damageAmount) || float.IsInfinity(damageAmount))
        {
            return;
        }

        currentHealth -= damageAmount;
        Log($"Monster Hit: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die(attacker);
        }
    }

    private void Die(PlayerRunSession attacker)
    {
        isDead = true;
        currentHealth = 0;
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