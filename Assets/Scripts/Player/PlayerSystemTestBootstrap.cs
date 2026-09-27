using UnityEngine;

public class PlayerSystemTestBootstrap : MonoBehaviour
{
    private const string PlayerObjectName = "Test_Player";
    private const string MonsterObjectName = "Test_Monster";

    private Transform followTarget;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateTestScene()
    {
        if (FindAnyObjectByType<PlayerHealth>() != null)
        {
            return;
        }

        Camera mainCamera = SetupCamera();

        int playerLayer = LayerMask.NameToLayer("Player");
        int monsterLayer = LayerMask.NameToLayer("Monster");

        GameObject playerObject = CreatePlayer(playerLayer, monsterLayer);
        CreateMonster(monsterLayer, playerLayer, new Vector2(4f, 0f));
        CreateMonster(monsterLayer, playerLayer, new Vector2(4f, 2f));
        CreateMonster(monsterLayer, playerLayer, new Vector2(4f, -2f));

        PlayerSystemTestBootstrap cameraFollow = mainCamera.gameObject.AddComponent<PlayerSystemTestBootstrap>();
        cameraFollow.followTarget = playerObject.transform;
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

    private static GameObject CreatePlayer(int playerLayer, int monsterLayer)
    {
        GameObject playerObject = new GameObject(PlayerObjectName);
        playerObject.transform.position = Vector3.zero;
        playerObject.layer = playerLayer;
        playerObject.tag = "Player";

        SpriteRenderer spriteRenderer = playerObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateSprite(new Color(0.2f, 0.7f, 1f));
        spriteRenderer.sortingOrder = 10;

        CircleCollider2D collider = playerObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.35f;

        playerObject.AddComponent<PlayerHealth>();
        playerObject.AddComponent<PlayerInventory>();
        playerObject.AddComponent<PlayerMovement>();

        GameObject firePointObject = new GameObject("FirePoint");
        firePointObject.transform.SetParent(playerObject.transform);
        firePointObject.transform.localPosition = new Vector3(0.55f, 0f, 0f);

        PlayerWeaponController weaponController = playerObject.AddComponent<PlayerWeaponController>();
        weaponController.SetTestWeaponData(
            CreateTestWeapons(),
            firePointObject.transform,
            CreateProjectilePrefab(monsterLayer));

        return playerObject;
    }

    private static GameObject CreateMonster(int monsterLayer, int playerLayer, Vector2 position)
    {
        GameObject monsterObject = new GameObject(MonsterObjectName);
        monsterObject.transform.position = position;
        monsterObject.layer = monsterLayer;
        monsterObject.tag = "Monster";

        SpriteRenderer spriteRenderer = monsterObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateSprite(new Color(1f, 0.35f, 0.25f));
        spriteRenderer.sortingOrder = 5;

        CircleCollider2D collider = monsterObject.AddComponent<CircleCollider2D>();
        collider.radius = 0.4f;

        monsterObject.AddComponent<MonsterHealth>();

        MonsterAttack monsterAttack = monsterObject.AddComponent<MonsterAttack>();
        monsterAttack.SetTestAttackData(1, 0.9f, 1f, LayerMask.GetMask("Player"));

        return monsterObject;
    }

    private static WeaponData[] CreateTestWeapons()
    {
        WeaponData ak47Data = ScriptableObject.CreateInstance<WeaponData>();
        ak47Data.SetTestData("AK47", WeaponGrade.Normal, 1, 0.12f, 16f, 1.2f, 1, 0f);

        WeaponData shotgunData = ScriptableObject.CreateInstance<WeaponData>();
        shotgunData.SetTestData("Shotgun", WeaponGrade.Normal, 1, 0.65f, 13f, 0.8f, 5, 35f);

        return new[] { ak47Data, shotgunData };
    }

    private static PlayerProjectile CreateProjectilePrefab(int monsterLayer)
    {
        GameObject projectileObject = new GameObject("Test_Bullet");
        projectileObject.SetActive(false);
        projectileObject.layer = monsterLayer;

        SpriteRenderer spriteRenderer = projectileObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateSprite(new Color(1f, 0.95f, 0.25f));
        spriteRenderer.sortingOrder = 20;
        projectileObject.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

        PlayerProjectile projectile = projectileObject.AddComponent<PlayerProjectile>();
        projectile.SetTargetLayer(LayerMask.GetMask("Monster"));

        return projectile;
    }

    private static Sprite CreateSprite(Color color)
    {
        Texture2D texture = new Texture2D(16, 16);
        texture.filterMode = FilterMode.Point;

        Color[] pixels = new Color[16 * 16];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
    }
}
