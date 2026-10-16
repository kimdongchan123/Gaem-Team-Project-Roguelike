using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana")]
    [SerializeField] private int maxMana = 100;
    [SerializeField] private float manaRegenPerSecond = 8f;
    [SerializeField, Min(0f)] private float regenDelayAfterUse = 0.8f;

    private float currentMana;
    private float nextRegenTime;
    private PlayerHealth playerHealth;
    private PlayerStats stats;
    private PlayerRunSession session;

    public int CurrentMana => Mathf.FloorToInt(currentMana);
    public int MaxMana => stats != null ? stats.MaxMana : Mathf.Max(1, maxMana);
    public float NormalizedMana => Mathf.Clamp01(currentMana / MaxMana);

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        stats = GetComponent<PlayerStats>();
        session = GetComponent<PlayerRunSession>();
        ResetMana();
    }

    public void ResetMana() { currentMana = MaxMana; nextRegenTime = 0f; }
    public bool RestoreMana(int amount)
    {
        if (amount <= 0 || currentMana >= MaxMana || (playerHealth != null && playerHealth.IsDead)) return false;
        currentMana = Mathf.Min(MaxMana,currentMana+amount); return true;
    }

    private void Update()
    {
        if ((playerHealth != null && playerHealth.IsDead) || (session != null && !session.CanAct) || Time.time < nextRegenTime || currentMana >= MaxMana)
        {
            return;
        }

        currentMana = Mathf.Min(MaxMana, currentMana + (Mathf.Max(0f, manaRegenPerSecond) * Time.deltaTime));
    }

    public bool TryUseMana(int manaAmount)
    {
        if (manaAmount < 0 || (playerHealth != null && playerHealth.IsDead))
        {
            return false;
        }

        if (manaAmount == 0)
        {
            return true;
        }

        if (currentMana < manaAmount)
        {
            return false;
        }

        currentMana -= manaAmount;
        nextRegenTime = Time.time + Mathf.Max(0f, regenDelayAfterUse);
        return true;
    }
}
