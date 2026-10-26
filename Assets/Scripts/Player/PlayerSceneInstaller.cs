using UnityEngine;
using UnityEngine.SceneManagement;

// Installs the shared sample systems onto the scene's existing player, preserving scene references.
[DefaultExecutionOrder(-300)]
public class PlayerSceneInstaller : MonoBehaviour
{
    [SerializeField] private string lobbySceneName = "LobbyScene";
    [SerializeField] private string dungeonSceneName = "RoomSystemTest";
    [SerializeField, Min(1)] private int dungeonFloor = 1;
    private PlayerRunSession session;

    private void Awake()
    {
        TempPlayerController2D temporary = GetComponent<TempPlayerController2D>();
        if (temporary != null) temporary.enabled = false;
        PlayerSystemFactory.CreatePlayer(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Monster"), gameObject);
        session = GetComponent<PlayerRunSession>();
        session.ConfigureSceneFlow(lobbySceneName, dungeonSceneName);
        bool isDungeon = gameObject.scene.name == dungeonSceneName;
        if (isDungeon) session.BeginRun(false);
        foreach (PlayerCameraController camera in FindObjectsByType<PlayerCameraController>(FindObjectsSortMode.None))
            camera.SetTarget(transform);
        RoomSystem.CurrentRoomChanged += OnRoomChanged;
        if (isDungeon)
        {
            foreach (RoomSystem room in FindObjectsByType<RoomSystem>(FindObjectsSortMode.None))
            {
                Transform root = room.transform.parent != null ? room.transform.parent : room.transform;
                foreach (MonsterSpawner spawner in root.GetComponentsInChildren<MonsterSpawner>(true))
                    spawner.BindRoom(room);
            }
            session.SetStage(dungeonFloor, "던전 시작 지점");
        }
    }

    private void OnRoomChanged(RoomSystem room)
    {
        if (session != null && room != null) session.SetStage(dungeonFloor, room.RoomId);
    }

    private void OnDestroy() { RoomSystem.CurrentRoomChanged -= OnRoomChanged; }
}
