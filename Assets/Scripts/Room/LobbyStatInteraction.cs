using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

// 스탯 NPC나 단말기 근처에서 E를 누르면 로비 스탯 UI를 켭니다.
[RequireComponent(typeof(Collider2D))]
public class LobbyStatInteraction : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private LobbyStatsUI statsUI;

    [Header("Hint")]
    [SerializeField] private bool showInteractionHint = true;

    private bool isPlayerNearby;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Update()
    {
        if (statsUI == null)
        {
            return;
        }

        if (WasEscapePressed() && statsUI.IsOpen)
        {
            statsUI.Close();
            return;
        }

        if (!isPlayerNearby || !WasInteractPressed())
        {
            return;
        }

        statsUI.Toggle();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        isPlayerNearby = false;

        if (statsUI != null)
        {
            statsUI.Close();
        }
    }

    private void OnDisable()
    {
        isPlayerNearby = false;

        if (statsUI != null)
        {
            statsUI.Close();
        }
    }

    private void OnGUI()
    {
        if (!showInteractionHint || !isPlayerNearby || statsUI == null)
        {
            return;
        }

        string message = statsUI.IsOpen
            ? "E / Esc : 스탯 창 닫기"
            : "E : 스탯 배분";

        Rect hintArea = new Rect(
            (Screen.width - 260f) * 0.5f,
            Screen.height - 90f,
            260f,
            45f
        );

        GUI.Box(hintArea, message);
    }

    private static bool IsPlayer(Collider2D other)
    {
        GameObject actor = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        return actor.CompareTag("Player");
    }

    private static bool WasInteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.E);
#else
        return false;
#endif
    }

    private static bool WasEscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.Escape);
#else
        return false;
#endif
    }
}
