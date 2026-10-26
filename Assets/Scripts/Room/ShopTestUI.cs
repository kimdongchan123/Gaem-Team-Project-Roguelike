using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ShopTestUI : MonoBehaviour
{
    [Serializable]
    private class ShopItem
    {
        public string itemName = "아이템";
        [Min(0)] public int price = 10;
        [Min(0)] public int stock = 1;
    }

    [Header("Test Currency")]
    [SerializeField] private int startingGold = 100;

    [Header("Test Items")]
    [SerializeField] private ShopItem[] items =
    {
        new ShopItem { itemName = "회복약", price = 30, stock = 3 },
        new ShopItem { itemName = "방어구", price = 70, stock = 1 }
    };

    private int currentGold;
    private int[] remainingStock;
    private int[] purchasedCount;
    private bool isOpen;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        currentGold = Mathf.Max(0, startingGold);
        int itemCount = items == null ? 0 : items.Length;
        remainingStock = new int[itemCount];
        purchasedCount = new int[itemCount];

        for (int i = 0; i < itemCount; i++)
        {
            remainingStock[i] = items[i] == null ? 0 : Mathf.Max(0, items[i].stock);
        }
    }

    private void Update()
    {
        if (isOpen && WasCloseKeyPressed())
        {
            CloseShop();
        }
    }

    public void ToggleShop()
    {
        isOpen = !isOpen;
    }

    public void CloseShop()
    {
        isOpen = false;
    }

    private void PurchaseItem(int itemIndex)
    {
        if (items == null || itemIndex < 0 || itemIndex >= items.Length || items[itemIndex] == null)
        {
            return;
        }

        ShopItem item = items[itemIndex];
        int price = Mathf.Max(0, item.price);

        if (remainingStock[itemIndex] <= 0 || currentGold < price)
        {
            return;
        }

        currentGold -= price;
        remainingStock[itemIndex]--;
        purchasedCount[itemIndex]++;

        Debug.Log($"[Shop Test] {item.itemName} 구매 성공 / 수량: {purchasedCount[itemIndex]} / 남은 골드: {currentGold}");
        // 실제 인벤토리 지급은 아직 구현하지 않습니다.
    }

    private void OnGUI()
    {
        if (!isOpen)
        {
            return;
        }

        float width = Mathf.Min(480f, Screen.width - 20f);
        float height = Mathf.Min(340f, Screen.height - 20f);
        float left = (Screen.width - width) / 2f;
        float top = (Screen.height - height) / 2f;

        GUI.Box(new Rect(left, top, width, height), "상점 (임시 UI)");
        GUI.Label(new Rect(left + 18f, top + 30f, width - 36f, 25f), $"보유 골드 : {currentGold} G");

        int itemCount = items == null ? 0 : items.Length;
        for (int i = 0; i < itemCount; i++)
        {
            float rowY = top + 68f + i * 57f;
            if (rowY + 40f > top + height - 48f)
            {
                break;
            }

            ShopItem item = items[i];
            if (item == null)
            {
                continue;
            }

            int price = Mathf.Max(0, item.price);
            float buttonWidth = 68f;
            float buttonX = left + width - buttonWidth - 18f;

            GUI.Label(new Rect(left + 18f, rowY, width - buttonWidth - 45f, 42f),
                $"{item.itemName}  |  {price} G  |  재고 {remainingStock[i]}  |  구매 {purchasedCount[i]}");

            bool canPurchase = remainingStock[i] > 0 && currentGold >= price;
            bool wasEnabled = GUI.enabled;
            GUI.enabled = canPurchase;

            if (GUI.Button(new Rect(buttonX, rowY, buttonWidth, 35f), "구매"))
            {
                PurchaseItem(i);
            }

            GUI.enabled = wasEnabled;
        }

        if (GUI.Button(new Rect(left + width - 105f, top + height - 42f, 88f, 28f), "닫기"))
        {
            CloseShop();
        }
    }

    private static bool WasCloseKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}
