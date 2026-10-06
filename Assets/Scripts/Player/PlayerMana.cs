using UnityEngine;

public class PlayerMana : MonoBehaviour
{
    [Header("Mana")]
    [SerializeField] private int maxMana = 100;
    [SerializeField] private float manaRegenPerSecond = 8f;

    private float currentMana;

    public int CurrentMana => Mathf.FloorToInt(currentMana);
    public int MaxMana => maxMana;

    private void Awake()
    {
        currentMana = maxMana;
    }

    private void Update()
    {
        if (currentMana >= maxMana)
        {
            return;
        }

        currentMana = Mathf.Min(maxMana, currentMana + (manaRegenPerSecond * Time.deltaTime));
    }

    public bool TryUseMana(int manaAmount)
    {
        if (manaAmount <= 0)
        {
            return true;
        }

        if (currentMana < manaAmount)
        {
            return false;
        }

        currentMana -= manaAmount;
        return true;
    }
}
