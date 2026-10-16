using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private int baseHealth = 8;
    [SerializeField] private int baseMana = 100;
    [SerializeField] private float baseMoveSpeed = 10f;
    [SerializeField] private int baseBagSize = 30;
    [SerializeField] private float baseAttackMultiplier = 1f;
    [SerializeField] private float baseDashCooldown = 5f;
    private int[] levels = new int[6];

    public int MaxHealth => Mathf.Clamp(baseHealth + levels[0], 1, 15);
    public int MaxMana => Mathf.Clamp(baseMana + levels[1], 1, 200);
    public float MoveSpeed => Mathf.Clamp(baseMoveSpeed + levels[2], 0f, 20f);
    public int BagSize => Mathf.Clamp(baseBagSize + levels[3], 1, 100);
    public float AttackMultiplier => Mathf.Clamp(baseAttackMultiplier + levels[4] * 0.1f, 0f, 1.5f);
    public float DashCooldown => Mathf.Max(2f, baseDashCooldown - levels[5] * 0.5f);

    public void Apply(PlayerProgressionData data)
    {
        for (int i = 0; i < levels.Length; i++)
            levels[i] = data != null && data.levels != null && i < data.levels.Length ? Mathf.Clamp(data.levels[i], 0, PlayerProgression.MaxLevels[i]) : 0;
    }
}
