using UnityEngine;

// 던전 씬의 Player를 Room_C_Start 중앙으로 옮깁니다.
[DefaultExecutionOrder(-200)]
public class DungeonPlayerSpawn : MonoBehaviour
{
    [Header("Dungeon Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform playerTarget;

    [Header("Test")]
    [Tooltip("던전 Scene만 직접 Play해도 시작 지점으로 이동합니다.")]
    [SerializeField] private bool spawnWhenPlayingDungeonDirectly = true;

    private void Start()
    {
        bool cameFromLobby = LobbySessionData.ConsumeDungeonEntryRequest();

        if (!cameFromLobby && !spawnWhenPlayingDungeonDirectly)
        {
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("[DungeonPlayerSpawn] Spawn Point가 설정되지 않았습니다.", this);
            return;
        }

        if (playerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        if (playerTarget == null)
        {
            Debug.LogError("[DungeonPlayerSpawn] Player를 찾지 못했습니다.", this);
            return;
        }

        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.z = playerTarget.position.z;

        Rigidbody2D playerBody = playerTarget.GetComponent<Rigidbody2D>();
        if (playerBody != null)
        {
            playerBody.linearVelocity = Vector2.zero;
            playerBody.angularVelocity = 0f;
            playerBody.position = new Vector2(spawnPosition.x, spawnPosition.y);
        }

        playerTarget.position = spawnPosition;
        Physics2D.SyncTransforms();

        Debug.Log($"[DungeonPlayerSpawn] 플레이어 시작 지점 이동: {spawnPosition}", this);
        Debug.Log(
            $"[DungeonPlayerSpawn] 로비 스탯 유지 확인 - " +
            $"체력 Lv.{LobbySessionData.HealthLevel}, " +
            $"공격 Lv.{LobbySessionData.AttackLevel}, " +
            $"속도 Lv.{LobbySessionData.MoveSpeedLevel}, " +
            $"남은 포인트 {LobbySessionData.RemainingPoints}",
            this
        );
    }
}
