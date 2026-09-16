using UnityEngine;

public enum ItemGrade
{
    Normal,
    Rare,
    Advanced,
    Mid
}

public class DroppableItem : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private ItemGrade itemGrade = ItemGrade.Normal;

    public ItemGrade ItemGrade => itemGrade;
}
