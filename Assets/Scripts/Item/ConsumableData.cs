using UnityEngine;

public enum ConsumableEffect { Heal, RestoreMana }

[CreateAssetMenu(fileName = "ConsumableData", menuName = "ScriptableObjects/ConsumableData")]
public class ConsumableData : ScriptableObject
{
    [SerializeField] private string itemId;
    [SerializeField] private string itemName;
    [SerializeField] private ConsumableEffect effect;
    [SerializeField, Min(1)] private int amount = 3;
    public string ItemId => string.IsNullOrEmpty(itemId) ? itemName : itemId;
    public string ItemName => itemName;
    public string DisplayName => PlayerUITheme.DisplayName(itemName);
    public ConsumableEffect Effect => effect;
    public int Amount => Mathf.Max(1, amount);
    public void SetData(string id, string displayName, ConsumableEffect type, int value)
    { itemId = id; itemName = displayName; effect = type; amount = Mathf.Max(1,value); }
}
