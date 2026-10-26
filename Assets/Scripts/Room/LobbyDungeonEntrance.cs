using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Collider2D))]
public class LobbyDungeonEntrance : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("확장자 없이 실제 던전 Scene 파일 이름을 입력하세요.")]
    [SerializeField] private string dungeonSceneName = "RoomSystemTest";

    [Header("Interaction")]
    [SerializeField] private bool showInteractionHint = true;

    private bool isPlayerNearby;
    private bool isLoading;

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
        if (!isPlayerNearby || isLoading || !WasInteractKeyPressed())
        {
            return;
        }

        EnterDungeon();
    }

    private bool WasInteractKeyPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(KeyCode.E);
#else
        return false;
#endif
    }

    private void EnterDungeon()
    {
        PlayerRunSession session = FindAnyObjectByType<PlayerRunSession>();
        if (session != null && session.UsesSceneFlow)
        {
            if (!session.MenuOpen) session.EnterDungeon();
            return;
        }
        if (string.IsNullOrWhiteSpace(dungeonSceneName))
        {
            Debug.LogError("[LobbyDungeonEntrance] Dungeon Scene Name이 비어 있습니다.", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(dungeonSceneName))
        {
            Debug.LogError(
                $"[LobbyDungeonEntrance] '{dungeonSceneName}' 씬을 찾지 못했습니다. Build Profiles의 Scene List 및 이름을 확인하세요.",
                this
            );
            return;
        }

        isLoading = true;
        LobbySessionData.RequestDungeonEntry();
        SceneManager.LoadScene(dungeonSceneName, LoadSceneMode.Single);
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
        if (IsPlayer(other))
        {
            isPlayerNearby = false;
        }
    }

    private static bool IsPlayer(Collider2D other)
    {
        GameObject actor = other.attachedRigidbody != null
            ? other.attachedRigidbody.gameObject
            : other.gameObject;

        return actor.CompareTag("Player");
    }

    private void OnGUI()
    {
        if (!showInteractionHint || !isPlayerNearby || isLoading)
        {
            return;
        }

        Rect hintArea = new Rect((Screen.width - 260f) * 0.5f, Screen.height - 90f, 260f, 45f);
        GUI.Box(hintArea, "E : 던전 입장");
    }
}
