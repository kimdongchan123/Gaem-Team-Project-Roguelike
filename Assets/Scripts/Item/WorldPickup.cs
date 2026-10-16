using UnityEngine;
using UnityEngine.InputSystem;

public class WorldPickup : MonoBehaviour
{
    public WeaponData Weapon { get; private set; }
    public int Crystals { get; private set; }
    public ConsumableData Consumable { get; private set; }
    private bool collected;
    private static Sprite pickupSprite;
    public string Label => Weapon != null ? Weapon.DisplayName : Consumable != null ? Consumable.DisplayName : $"블루 크리스탈 {Crystals}개";

    private static WorldPickup Create(Vector2 position, Color color)
    {
        GameObject obj = new GameObject("World Pickup"); obj.transform.position = position;
        obj.transform.localScale = Vector3.one * 0.3f;
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>();
        if (pickupSprite == null)
        {
            Texture2D texture = Texture2D.whiteTexture;
            pickupSprite = Sprite.Create(texture,new Rect(0f,0f,texture.width,texture.height),Vector2.one*0.5f,texture.width);
        }
        renderer.sprite = pickupSprite; renderer.color = color; renderer.sortingOrder = 8;
        return obj.AddComponent<WorldPickup>();
    }
    public static WorldPickup SpawnWeapon(WeaponData weapon, Vector2 position)
    {
        WorldPickup pickup = Create(position,new Color(1f,0.8f,0.2f)); pickup.Weapon = weapon; return pickup;
    }
    public static WorldPickup SpawnCrystals(int amount, Vector2 position)
    {
        WorldPickup pickup = Create(position,new Color(0.1f,0.8f,1f)); pickup.Crystals = Mathf.Max(1,amount);
        pickup.transform.rotation = Quaternion.Euler(0f,0f,45f); return pickup;
    }
    public static WorldPickup SpawnConsumable(ConsumableData item, Vector2 position)
    {
        WorldPickup pickup = Create(position,new Color(0.4f,1f,0.5f)); pickup.Consumable = item; return pickup;
    }
    public bool TryCollect(PlayerRunSession session)
    {
        if (collected || session == null || !session.CanAct || Vector2.Distance(session.transform.position,transform.position)>1.25f) return false;
        bool accepted = Weapon != null ? session.GetComponent<PlayerInventory>().TryAddWeapon(Weapon) : Consumable != null ?
            session.GetComponent<PlayerInventory>().TryAddConsumable(Consumable) : session.CollectCrystals(Crystals);
        if (!accepted) return false;
        collected = true; Destroy(gameObject); return true;
    }
    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            TryCollect(FindAnyObjectByType<PlayerRunSession>());
    }
}
