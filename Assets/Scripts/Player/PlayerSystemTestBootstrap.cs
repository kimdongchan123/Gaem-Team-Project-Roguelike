using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSystemTestBootstrap : MonoBehaviour
{
    private const string PlayerObjectName = "Test_Player";
    private const string MonsterObjectName = "Test_Monster";

    private Transform followTarget;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateTestScene()
    {
        if (SceneManager.GetActiveScene().path != "Assets/Scenes/SampleScene.unity") return;
        if (FindAnyObjectByType<PlayerHealth>() != null)
        {
            return;
        }

        Camera mainCamera = SetupCamera();

        int playerLayer = LayerMask.NameToLayer("Player");
        int monsterLayer = LayerMask.NameToLayer("Monster");

        GameObject playerObject = PlayerSystemFactory.CreatePlayer(playerLayer, monsterLayer);
        PlayerRunSession session = playerObject.GetComponent<PlayerRunSession>();
        session.RunStarted += () =>
        {
            ClearTestWorld();
            CreateMonster(monsterLayer, playerLayer, new Vector2(4f, 0f));
            CreateMonster(monsterLayer, playerLayer, new Vector2(4f, 2f));
            CreateMonster(monsterLayer, playerLayer, new Vector2(4f, -2f));
            WorldPickup.SpawnCrystals(3, new Vector2(-2f, 0f));
            ConsumableData[] potions = PlayerSystemFactory.CreateConsumables();
            WorldPickup.SpawnConsumable(potions[0], new Vector2(-3f, -1f));
            WorldPickup.SpawnConsumable(potions[1], new Vector2(-3f, 1f));
            WorldPickup.SpawnWeapon(playerObject.GetComponent<PlayerInventory>().Weapons.Count > 0 ?
                playerObject.GetComponent<PlayerInventory>().Weapons[0] : PlayerSystemFactory.CreateWeapons()[0], new Vector2(-2f, 2f));
        };
        session.LobbyEntered += ClearTestWorld;

        PlayerSystemTestBootstrap cameraFollow = mainCamera.gameObject.AddComponent<PlayerSystemTestBootstrap>();
        cameraFollow.followTarget = playerObject.transform;
    }

    private static void ClearTestWorld()
    {
        foreach (MonsterHealth monster in FindObjectsByType<MonsterHealth>(FindObjectsSortMode.None)) Destroy(monster.gameObject);
        foreach (WorldPickup pickup in FindObjectsByType<WorldPickup>(FindObjectsSortMode.None)) Destroy(pickup.gameObject);
        foreach (PlayerProjectile projectile in FindObjectsByType<PlayerProjectile>(FindObjectsSortMode.None)) Destroy(projectile.gameObject);
    }

    private void LateUpdate()
    {
        if (followTarget == null)
        {
            return;
        }

        transform.position = new Vector3(followTarget.position.x, followTarget.position.y, -10f);
    }

    private static Camera SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5f;
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
        mainCamera.backgroundColor = new Color(0.08f, 0.09f, 0.11f);

        return mainCamera;
    }

    private static GameObject CreateMonster(int monsterLayer, int playerLayer, Vector2 position)
    {
        GameObject monsterObject = new GameObject(MonsterObjectName);
        monsterObject.transform.position = position;
        monsterObject.layer = monsterLayer;
        monsterObject.tag = "Monster";

        SpriteRenderer spriteRenderer = monsterObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = PlayerSystemFactory.CreateSprite(new Color(1f, 0.35f, 0.25f));
        spriteRenderer.sortingOrder = 5;

        CircleCollider2D collider = monsterObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;

        monsterObject.AddComponent<MonsterHealth>();

        MonsterAttack monsterAttack = monsterObject.AddComponent<MonsterAttack>();
        monsterAttack.SetTestAttackData(1, 0.9f, 1f, LayerMask.GetMask("Player"));

        return monsterObject;
    }

}
