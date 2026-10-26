using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 8;
    [SerializeField] private float invincibleTimeAfterHit = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugLog;

    private int currentHealth;
    private float invincibleEndTime;
    private bool isDead;
    private PlayerStats stats;
    private PlayerRunSession session;
    private Color originalColor = Color.white;
    public bool IsInvincible => Time.time < invincibleEndTime;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => stats != null ? stats.MaxHealth : Mathf.Max(1, maxHealth);
    public bool IsDead => isDead;

    private void Awake()
    {
        stats = GetComponent<PlayerStats>();
        session = GetComponent<PlayerRunSession>();
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) originalColor = sprite.color;
        ResetHealth();
    }

    public void ResetHealth()
    {
        currentHealth = MaxHealth; isDead = false; invincibleEndTime = 0f;
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) sprite.color = originalColor;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead || damageAmount <= 0 || Time.time < invincibleEndTime || (session != null && !session.CanAct))
        {
            return;
        }

        currentHealth -= damageAmount;
        invincibleEndTime = Time.time + invincibleTimeAfterHit;

        Log($"Player Hit: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int healAmount)
    {
        if (isDead || healAmount <= 0)
        {
            return;
        }

        currentHealth = Mathf.Min(currentHealth + healAmount, MaxHealth);
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;
        currentHealth = 0;

        PlayerInventory playerInventory = GetComponent<PlayerInventory>();
        if (session != null)
        {
            session.CompleteDeath();
        }
        else if (playerInventory != null)
        {
            playerInventory.DropItemsOnDeath();
        }

        Log("Player Dead");

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.gray;
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
