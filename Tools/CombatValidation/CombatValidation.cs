using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class CombatValidation
{
    private const string Pending = "CombatValidation.Pending";
    private static int frames;
    private static int passed;
    static CombatValidation() { EditorApplication.update += Tick; }
    public static void Run() { SessionState.SetBool(Pending, true); EditorApplication.EnterPlaymode(); }
    private static void Tick()
    {
        if (!SessionState.GetBool(Pending, false) || !EditorApplication.isPlaying || ++frames < 8) return;
        SessionState.SetBool(Pending, false);
        try { Execute(); Debug.Log("COMBAT_VALIDATION_SUCCESS: " + passed + " checks"); EditorApplication.Exit(0); }
        catch (Exception ex) { Debug.LogException(ex); EditorApplication.Exit(1); }
    }
    private static void Assert(bool value, string name) { if (!value) throw new Exception(name); passed++; Debug.Log("PASS: " + name); }
    private static object Call(object target, string name, params object[] args) => target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
    private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
    private static void Set(object target, string name, object value) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    private static WeaponData Weapon(WeaponAttackType type, int damage = 1, int cost = 0, int pellets = 1, float spread = 0)
    {
        var weapon = ScriptableObject.CreateInstance<WeaponData>();
        weapon.SetTestData("Validation", WeaponGrade.Normal, type, cost > 0, damage, 0.5f, 20f, 1f, cost, pellets, spread, 2f, 2f, 90f, 2f);
        return weapon;
    }
    private static MonsterHealth Monster(Vector2 position, bool multiple = true)
    {
        var obj = new GameObject("Validation Monster"); obj.transform.position = position;
        var hp = obj.AddComponent<MonsterHealth>();
        var child = new GameObject("Child Collider"); child.transform.SetParent(obj.transform, false);
        child.AddComponent<CircleCollider2D>().radius = 0.2f;
        if (multiple) child.AddComponent<BoxCollider2D>().size = Vector2.one * 0.3f;
        return hp;
    }
    private static PlayerProjectile Bullet(Vector2 position, WeaponData weapon = null, Vector2? destination = null, PlayerHealth owner = null)
    {
        var obj = new GameObject("Validation Bullet"); obj.transform.position = position;
        var bullet = obj.AddComponent<PlayerProjectile>(); bullet.SetTargetLayer(1); bullet.SetPlayerLayer(1);
        if (weapon == null) bullet.Initialize(1, 100f, 1f);
        else bullet.Initialize(weapon, weapon.Damage, destination ?? position, owner);
        return bullet;
    }
    private static void Delete(Component obj) { if (obj != null) UnityEngine.Object.DestroyImmediate(obj.gameObject); }
    private static void Execute()
    {
        var player = new GameObject("Validation Player");
        var health = player.AddComponent<PlayerHealth>(); var mana = player.AddComponent<PlayerMana>();
        Assert(mana.CurrentMana == 100, "Mana begins full");
        Assert(mana.TryUseMana(18) && mana.CurrentMana == 82, "Magic attack mana cost");
        Assert(!mana.TryUseMana(100) && mana.CurrentMana == 82, "Insufficient mana is not spent");
        Assert(!mana.TryUseMana(-1) && mana.TryUseMana(0), "Reject negative cost and allow free attack");
        float before = Field<float>(mana, "currentMana"); Call(mana, "Update");
        Assert(Field<float>(mana, "currentMana") == before, "Regeneration waits after casting");
        Set(mana, "nextRegenTime", -1f); Call(mana, "Update");
        Assert(Field<float>(mana, "currentMana") > before, "Regeneration resumes after delay");
        player.AddComponent<PlayerInventory>(); health.TakeDamage(100);
        Assert(health.IsDead && health.CurrentHealth == 0, "Empty inventory death is safe");
        before = Field<float>(mana, "currentMana"); Call(mana, "Update");
        Assert(!mana.TryUseMana(1) && Field<float>(mana, "currentMana") == before, "Dead player cannot cast or regenerate");
        UnityEngine.Object.DestroyImmediate(player);

        var near = Monster(new Vector2(5, 0)); var far = Monster(new Vector2(8, 0));
        var decoration = new GameObject("Decoration"); decoration.transform.position = new Vector3(2,0,0); decoration.AddComponent<CircleCollider2D>();
        var bullet = Bullet(Vector2.zero); Physics2D.SyncTransforms();
        Assert((bool)Call(bullet, "CheckHit", 10f), "Fast bullet sweeps entire path");
        Assert(near.CurrentHealth == 2 && far.CurrentHealth == 3, "Nearest child collider hit once; decoration ignored");
        Assert(!(bool)Call(bullet, "CheckHit", 10f) && near.CurrentHealth == 2, "Resolved projectile cannot hit twice");
        Delete(bullet); UnityEngine.Object.DestroyImmediate(decoration); Delete(far);
        bullet = Bullet(new Vector2(5,0)); Physics2D.SyncTransforms();
        Assert((bool)Call(bullet, "CheckHit", 0f) && near.CurrentHealth == 1, "Muzzle overlap recognized");
        Delete(bullet); Delete(near);
        var area = Weapon(WeaponAttackType.ExplosiveProjectile);
        near = Monster(new Vector2(1,0)); bullet = Bullet(Vector2.zero, area); Physics2D.SyncTransforms();
        Call(bullet, "Explode"); Call(bullet, "Explode");
        Assert(near.CurrentHealth == 2, "Explosion deduplicates colliders and resolves once");
        Delete(bullet); Delete(near);
        var fireball = Weapon(WeaponAttackType.TargetExplosion);
        bullet = Bullet(Vector2.zero, fireball, new Vector2(3,0));
        Set(bullet, "moveSpeed", 100000f); Call(bullet, "Update");
        Assert(Vector2.Distance(bullet.transform.position, new Vector2(3,0)) < 0.001f && Field<bool>(bullet,"hasResolved"), "Fireball stops and explodes at crossed destination");
        Delete(bullet);
        var blackHole = Weapon(WeaponAttackType.BlackHole);
        bullet = Bullet(Vector2.zero, blackHole, new Vector2(100,0));
        Assert(Mathf.Approximately(bullet.transform.position.x, blackHole.TargetRange), "Black hole placement has finite range");
        Delete(bullet);
        near = Monster(Vector2.zero); bullet = Bullet(Vector2.zero, blackHole); Physics2D.SyncTransforms();
        Call(bullet, "UpdateAreaEffect"); Call(bullet, "UpdateAreaEffect");
        Assert(near.CurrentHealth == 2, "Black hole ticks once per interval per enemy");
        Delete(bullet); Delete(near);
        var homing = Weapon(WeaponAttackType.HomingProjectile); near = Monster(new Vector2(0,3));
        bullet = Bullet(Vector2.zero, homing); Physics2D.SyncTransforms(); Call(bullet, "RotateToNearestTarget", 0.01f);
        Assert(Mathf.Abs(Mathf.DeltaAngle(bullet.transform.eulerAngles.z,0)) <= 3.61f, "Homing turn rate is bounded");
        Delete(bullet); Delete(near);

        player = new GameObject("Shooter"); health = player.AddComponent<PlayerHealth>(); mana = player.AddComponent<PlayerMana>();
        var controller = player.AddComponent<PlayerWeaponController>(); var muzzle = new GameObject("Muzzle"); muzzle.transform.SetParent(player.transform,false);
        near = Monster(new Vector2(1,0)); far = Monster(new Vector2(-1,0));
        var melee = Weapon(WeaponAttackType.Melee); controller.SetTestWeaponData(new[]{melee},muzzle.transform,null,1); Physics2D.SyncTransforms();
        Assert(controller.TryAttackAt(Vector2.right), "Melee attack does not require projectile prefab");
        Assert(near.CurrentHealth == 2 && far.CurrentHealth == 3, "Melee hits front once and excludes rear");
        Assert(!controller.TryAttackAt(Vector2.right) && near.CurrentHealth == 2, "Cooldown prevents repeated attacks");
        Delete(near); Delete(far); Set(controller,"nextAttackTime",-1f);
        var template = Bullet(Vector2.zero); template.gameObject.SetActive(false);
        var magic = Weapon(WeaponAttackType.HomingProjectile,1,16,2,18);
        controller.SetTestWeaponData(new[]{magic},muzzle.transform,template,1);
        Assert(controller.TryAttackAt(Vector2.right) && mana.CurrentMana == 84, "Multi projectile magic charges once per attack");
        Assert(!controller.TryAttackAt(Vector2.right) && mana.CurrentMana == 84, "Blocked cooldown does not spend mana");
        foreach(var shot in UnityEngine.Object.FindObjectsByType<PlayerProjectile>(FindObjectsSortMode.None)) Delete(shot);
        Set(controller,"nextAttackTime",-1f); var straight = Weapon(WeaponAttackType.StraightProjectile,1,0,1,40);
        muzzle.transform.rotation = Quaternion.Euler(0,0,70); controller.SetTestWeaponData(new[]{straight},muzzle.transform,template,1);
        controller.TryAttackAt(Vector2.right); var centered = UnityEngine.Object.FindAnyObjectByType<PlayerProjectile>();
        Assert(centered != null && Mathf.Abs(Mathf.DeltaAngle(centered.transform.eulerAngles.z,70)) < 0.01f, "Single projectile remains centered with spread configured");
        Delete(centered);
        var hud = player.AddComponent<PlayerWeaponHud>(); hud.SetTarget(controller,health,mana); Call(hud,"Start");
        health.TakeDamage(2); Call(hud,"Refresh");
        Assert(Mathf.Approximately(Field<RectTransform>(hud,"healthFill").sizeDelta.x,344f*0.75f), "HUD health bar follows damage");
        Assert(Mathf.Approximately(Field<RectTransform>(hud,"manaFill").sizeDelta.x,344f*0.84f), "HUD mana bar follows spending");
        Assert(Field<Text>(hud,"healthText").text == "체력   6 / 8", "HUD displays exact health");
        Assert(Field<Text>(hud,"weaponText").text.Contains("Validation"), "HUD displays equipped weapon");
        Delete(template); UnityEngine.Object.DestroyImmediate(player);
        UnityEngine.Object.DestroyImmediate(area); UnityEngine.Object.DestroyImmediate(fireball); UnityEngine.Object.DestroyImmediate(blackHole); UnityEngine.Object.DestroyImmediate(homing);
        UnityEngine.Object.DestroyImmediate(melee); UnityEngine.Object.DestroyImmediate(magic); UnityEngine.Object.DestroyImmediate(straight);
        passed += PlayerDesignValidation.Run();
    }
}
