using UnityEngine;

public static class PlayerSystemFactory
{
    public static GameObject CreatePlayer(int playerLayer, int monsterLayer, GameObject existingPlayer = null, PlayerProgression progression = null)
    {
        GameObject playerObject = existingPlayer != null ? existingPlayer : new GameObject("Player");
        if (playerObject.GetComponent<PlayerRunSession>() != null) return playerObject;
        if (existingPlayer == null) playerObject.transform.position = Vector3.zero;
        playerObject.layer = playerLayer;
        playerObject.tag = "Player";

        SpriteRenderer spriteRenderer = playerObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = playerObject.AddComponent<SpriteRenderer>();
        if (existingPlayer == null)
        {
            spriteRenderer.sprite = CreateSprite(new Color(0.2f, 0.7f, 1f));
            spriteRenderer.sortingOrder = 10;
            Texture2D bodyTexture = spriteRenderer.sprite.texture;
            bodyTexture.SetPixel(11, 11, Color.white);
            bodyTexture.SetPixel(12, 11, Color.black);
            bodyTexture.Apply();
        }
        if (playerObject.GetComponent<Collider2D>() == null)
            playerObject.AddComponent<CircleCollider2D>().radius = 0.22f;
        CapsuleCollider2D capsule = playerObject.GetComponent<CapsuleCollider2D>();
        if (capsule != null) capsule.size = new Vector2(0.55f, 0.65f);
        playerObject.AddComponent<PlayerStats>();
        PlayerRunSession session = playerObject.AddComponent<PlayerRunSession>();
        playerObject.AddComponent<PlayerHealth>();
        PlayerMana playerMana = playerObject.AddComponent<PlayerMana>();
        playerObject.AddComponent<PlayerInventory>();
        PlayerMovement movement = playerObject.AddComponent<PlayerMovement>();

        GameObject aimPivot = new GameObject("Weapon Aim Pivot");
        aimPivot.transform.SetParent(playerObject.transform, false);
        movement.SetAimPivot(aimPivot.transform);
        GameObject gun = new GameObject("Test Weapon Visual");
        gun.transform.SetParent(aimPivot.transform, false);
        gun.transform.localPosition = new Vector3(0.35f, -0.08f, 0f);
        gun.transform.localScale = new Vector3(0.5f, 0.14f, 1f);
        SpriteRenderer gunRenderer = gun.AddComponent<SpriteRenderer>();
        gunRenderer.sprite = CreateSprite(new Color(0.75f, 0.8f, 0.85f));
        gunRenderer.sortingOrder = 11;

        GameObject firePointObject = new GameObject("FirePoint");
        firePointObject.transform.SetParent(aimPivot.transform, false);
        firePointObject.transform.localPosition = new Vector3(0.55f, 0f, 0f);

        PlayerWeaponController weaponController = playerObject.AddComponent<PlayerWeaponController>();
        WeaponData[] catalog = CreateWeapons();
        weaponController.SetTestWeaponData(
            catalog,
            firePointObject.transform,
            CreateProjectilePrefab(monsterLayer, playerLayer),
            LayerMask.GetMask("Monster"));

        PlayerWeaponHud weaponHud = playerObject.AddComponent<PlayerWeaponHud>();
        weaponHud.SetTarget(weaponController, playerObject.GetComponent<PlayerHealth>(), playerMana);
        session.Initialize(progression ?? new PlayerProgression(), catalog, CreateConsumables());
        playerObject.AddComponent<PlayerMenuUI>();

        return playerObject;
    }

    public static ConsumableData[] CreateConsumables()
    {
        ConsumableData health = ScriptableObject.CreateInstance<ConsumableData>();
        health.SetData("health_potion", "Health Potion", ConsumableEffect.Heal, 3);
        ConsumableData mana = ScriptableObject.CreateInstance<ConsumableData>();
        mana.SetData("mana_potion", "Mana Potion", ConsumableEffect.RestoreMana, 30);
        return new[] { health, mana };
    }

    public static WeaponData[] CreateWeapons()
    {
        WeaponData ak47Data = ScriptableObject.CreateInstance<WeaponData>();
        ak47Data.SetTestData("AK47", WeaponGrade.Normal, WeaponAttackType.StraightProjectile, false, 1, 0.18f, 20f, 0.9f, 0, 1, 0f, 0f, 0f, 0f, 0f);

        WeaponData rocketLauncherData = ScriptableObject.CreateInstance<WeaponData>();
        rocketLauncherData.SetTestData("Rocket Launcher", WeaponGrade.Rare, WeaponAttackType.ExplosiveProjectile, false, 3, 1.2f, 10f, 1.3f, 0, 1, 0f, 1.8f, 0f, 0f, 0f);

        WeaponData shotgunData = ScriptableObject.CreateInstance<WeaponData>();
        shotgunData.SetTestData("Shotgun", WeaponGrade.Normal, WeaponAttackType.SpreadProjectile, false, 1, 0.85f, 16f, 0.35f, 0, 6, 40f, 0f, 0f, 0f, 0f);

        WeaponData m249Data = ScriptableObject.CreateInstance<WeaponData>();
        m249Data.SetTestData("M249", WeaponGrade.Rare, WeaponAttackType.StraightProjectile, false, 1, 0.1f, 18f, 0.6f, 0, 1, 0f, 0f, 0f, 0f, 0f);

        WeaponData axeData = ScriptableObject.CreateInstance<WeaponData>();
        axeData.SetTestData("Axe", WeaponGrade.Normal, WeaponAttackType.Melee, false, 3, 0.75f, 0f, 0f, 0, 1, 0f, 0f, 1.5f, 120f, 0f);

        WeaponData swissData = ScriptableObject.CreateInstance<WeaponData>();
        swissData.SetTestData("Swiss Dagger", WeaponGrade.Normal, WeaponAttackType.Melee, false, 1, 0.2f, 0f, 0f, 0, 1, 0f, 0f, 0.9f, 65f, 0f);

        WeaponData fireBallData = ScriptableObject.CreateInstance<WeaponData>();
        fireBallData.SetTestData("Fire Ball", WeaponGrade.Rare, WeaponAttackType.TargetExplosion, true, 3, 1f, 10f, 1.2f, 18, 1, 0f, 1.5f, 0f, 0f, 0f);

        WeaponData guidedMissileData = ScriptableObject.CreateInstance<WeaponData>();
        guidedMissileData.SetTestData("Guided Missile", WeaponGrade.Rare, WeaponAttackType.HomingProjectile, true, 2, 0.9f, 9f, 1.6f, 16, 2, 18f, 0f, 0f, 0f, 0f);

        WeaponData blackHoleGunData = ScriptableObject.CreateInstance<WeaponData>();
        blackHoleGunData.SetTestData("Black Hole Gun", WeaponGrade.Rare, WeaponAttackType.BlackHole, true, 1, 2.5f, 0f, 0f, 30, 1, 0f, 1.8f, 0f, 0f, 2f);

        WeaponData magicGatlingData = ScriptableObject.CreateInstance<WeaponData>();
        magicGatlingData.SetTestData("Magic Gatling", WeaponGrade.Rare, WeaponAttackType.MagicRapidProjectile, true, 1, 0.08f, 22f, 0.65f, 3, 1, 0f, 0f, 0f, 0f, 0f);

        return new[]
        {
            ak47Data,
            rocketLauncherData,
            shotgunData,
            m249Data,
            axeData,
            swissData,
            fireBallData,
            guidedMissileData,
            blackHoleGunData,
            magicGatlingData
        };
    }

    public static PlayerProjectile CreateProjectilePrefab(int monsterLayer, int playerLayer)
    {
        GameObject projectileObject = new GameObject("Test_Bullet");
        projectileObject.SetActive(false);
        projectileObject.layer = LayerMask.NameToLayer("Default");

        SpriteRenderer spriteRenderer = projectileObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = CreateSprite(new Color(1f, 0.95f, 0.25f));
        spriteRenderer.sortingOrder = 20;
        projectileObject.transform.localScale = new Vector3(0.25f, 0.25f, 1f);

        PlayerProjectile projectile = projectileObject.AddComponent<PlayerProjectile>();
        projectile.SetTargetLayer(LayerMask.GetMask("Monster"));
        projectile.SetPlayerLayer(LayerMask.GetMask("Player"));

        return projectile;
    }

    public static Sprite CreateSprite(Color color)
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

