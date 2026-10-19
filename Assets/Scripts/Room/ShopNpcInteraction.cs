using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(CircleCollider2D))]
public class ShopNpcInteraction : MonoBehaviour
{
    [Header("Shop UI")]
    [SerializeField] private ShopTestUI shopUI;

    [Header("Debug")]
    [SerializeField] private bool showInteractionHint = true;

    private bool isPlayerNearby;

    private void Reset()
    {
        CircleCollider2D interactionTrigger = GetComponent<CircleCollider2D>();
        interactionTrigger.isTrigger = true;
        interactionTrigger.radius = 1.6f;
    }

    private void Awake()
    {
        GetComponent<CircleCollider2D>().isTrigger = true;
    }

    private void Update()
    {
        if (!isPlayerNearby || shopUI == null)
        {
            return;
        }

        if (WasInteractKeyPressed())
        {
            shopUI.ToggleShop();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject actor = GetActor(other);

        if (actor.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        GameObject actor = GetActor(other);

        if (!actor.CompareTag("Player"))
        {
            return;
        }

        isPlayerNearby = false;

        if (shopUI != null)
        {
            shopUI.CloseShop();
        }
    }

    private void OnGUI()
    {
        if (!showInteractionHint || !isPlayerNearby || shopUI == null || shopUI.IsOpen)
        {
            return;
        }

        float width = 180f;
        GUI.Box(new Rect((Screen.width - width) / 2f, Screen.height - 85f, width, 35f),
            "E : 상점 열기");
    }

    private static bool WasInteractKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    private static GameObject GetActor(Collider2D other)
    {
        return other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;
    }
}
