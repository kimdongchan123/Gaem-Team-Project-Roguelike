using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private int bagSize = 30;
    [SerializeField] private GameObject[] carriedItemPrefabs;

    [Header("Death Drop")]
    [SerializeField, Range(0f, 1f)] private float normalDropChance = 0.05f;
    [SerializeField, Range(0f, 1f)] private float rareDropChance = 0.25f;
    [SerializeField, Range(0f, 1f)] private float advancedDropChance = 0.4f;
    [SerializeField, Range(0f, 1f)] private float midDropChance = 0.3f;
    [SerializeField] private float dropRadius = 0.8f;

    public int BagSize => bagSize;

    public void DropItemsOnDeath()
    {
        for (int i = 0; i < carriedItemPrefabs.Length && i < bagSize; i++)
        {
            GameObject itemPrefab = carriedItemPrefabs[i];
            if (itemPrefab == null || Random.value > GetDropChance(itemPrefab))
            {
                continue;
            }

            Vector2 randomOffset = Random.insideUnitCircle * dropRadius;
            Instantiate(itemPrefab, (Vector2)transform.position + randomOffset, Quaternion.identity);
        }
    }

    private float GetDropChance(GameObject itemPrefab)
    {
        DroppableItem droppableItem = itemPrefab.GetComponent<DroppableItem>();
        if (droppableItem == null)
        {
            return normalDropChance;
        }

        switch (droppableItem.ItemGrade)
        {
            case ItemGrade.Rare:
                return rareDropChance;
            case ItemGrade.Advanced:
                return advancedDropChance;
            case ItemGrade.Mid:
                return midDropChance;
            default:
                return normalDropChance;
        }
    }
}
