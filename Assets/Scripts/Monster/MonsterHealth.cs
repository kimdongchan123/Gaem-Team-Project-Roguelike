using UnityEngine;
using UnityEngine.Serialization;

public class MonsterHealth : MonoBehaviour
{
    public event System.Action<float> Damaged;
    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private bool isElite;
    public bool IsElite => isElite;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog;

    private float currentHealth;
    private bool isDead;
    private bool isDamageable = true;
    private Collider2D hurtboxCollider;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
        EliteMonster elite = GetComponent<EliteMonster>();
        if (elite != null && elite.resistanceType != EliteResistanceType.None) isElite = true;
    }

    public void Init(MonsterData data)
    {
        maxHealth = Mathf.RoundToInt(data.maxHp);
        currentHealth = maxHealth;
    }

    private void Start()
    {
        if (PrototypeArt.Enabled && GetComponent<PrototypeActorVisual>() == null) gameObject.AddComponent<PrototypeActorVisual>();
        if (GetComponent<MonsterHealthBar>() == null) gameObject.AddComponent<MonsterHealthBar>();
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
        hurtboxCollider = collider;
        hurtboxCollider.enabled = isDamageable;
    }

    // 플레이어가 방에 들어오기 전에는 피격 판정을 꺼서 공격이 닿지 않게 함 (MonsterSpawner가 호출)
    public void SetDamageable(bool damageable)
    {
        isDamageable = damageable;
        if (hurtboxCollider != null) hurtboxCollider.enabled = damageable;
    }

    public void TakeDamage(float damageAmount, PlayerRunSession attacker = null)
    {
        if (isDead || !isDamageable || damageAmount <= 0 || float.IsNaN(damageAmount) || float.IsInfinity(damageAmount))
        {
            return;
        }

        currentHealth -= damageAmount;
        Damaged?.Invoke(damageAmount);
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
        if (PrototypeArt.Enabled)
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            int layer = renderer != null ? renderer.sortingLayerID : 0;
            int order = renderer != null ? renderer.sortingOrder + 3 : 3;
            PrototypeEffect.Burst(transform.position, PrototypeArt.Red, 12, layer, order);
            PrototypeEffect.Emit(PrototypeArt.Shape("ring"), transform.position, PrototypeArt.Amber, Vector2.one * 0.7f, 0.25f, Vector2.zero, layer, order, true);
        }
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
