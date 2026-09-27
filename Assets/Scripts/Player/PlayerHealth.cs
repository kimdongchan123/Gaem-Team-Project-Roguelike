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

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead || damageAmount <= 0 || Time.time < invincibleEndTime)
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

        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
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
        if (playerInventory != null)
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
