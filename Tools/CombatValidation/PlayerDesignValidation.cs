using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem.UI;

public static class PlayerDesignValidation
{
    private static int passed;
    private static void Assert(bool condition,string name) { if(!condition)throw new Exception(name);passed++;Debug.Log("PASS: "+name); }
    private static void Set(object obj,string name,object value) => obj.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(obj,value);
    private static T Field<T>(object obj,string name) => (T)obj.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(obj);
    private static object Call(object obj,string name,params object[] args) => obj.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(obj,args);
    private static WeaponData Weapon(string name,WeaponGrade grade)
    {
        var weapon=ScriptableObject.CreateInstance<WeaponData>();
        weapon.SetTestData(name,grade,WeaponAttackType.StraightProjectile,false,1,0.2f,20,1,0,1,0,0,0,0,0);return weapon;
    }
    public static int Run()
    {
        passed=0; string key="LastShelter.Validation."+Guid.NewGuid().ToString("N");
        GameObject player=null,template=null; var resources=new List<UnityEngine.Object>();
        try
        {
            var profile=new PlayerProgression(key);profile.Data.crystals=1000;profile.Save();
            player=new GameObject("Design Validation Player");var body=player.AddComponent<SpriteRenderer>();
            var stats=player.AddComponent<PlayerStats>();var session=player.AddComponent<PlayerRunSession>();
            var health=player.AddComponent<PlayerHealth>();var mana=player.AddComponent<PlayerMana>();var bag=player.AddComponent<PlayerInventory>();
            var move=player.AddComponent<PlayerMovement>();var pivot=new GameObject("Aim Pivot");pivot.transform.SetParent(player.transform,false);move.SetAimPivot(pivot.transform);
            var muzzle=new GameObject("Muzzle");muzzle.transform.SetParent(pivot.transform,false);
            template=new GameObject("Template");template.SetActive(false);var projectile=template.AddComponent<PlayerProjectile>();
            var controller=player.AddComponent<PlayerWeaponController>();
            var catalog=new[]{Weapon("Basic",WeaponGrade.Normal),Weapon("Rare",WeaponGrade.Rare),Weapon("Legend",WeaponGrade.Legendary),Weapon("Mid",WeaponGrade.Mid)};
            resources.AddRange(catalog);
            var potion=ScriptableObject.CreateInstance<ConsumableData>();potion.SetData("hp","Health Potion",ConsumableEffect.Heal,3);resources.Add(potion);
            var manaPotion=ScriptableObject.CreateInstance<ConsumableData>();manaPotion.SetData("mp","Mana Potion",ConsumableEffect.RestoreMana,30);resources.Add(manaPotion);
            controller.SetTestWeaponData(catalog,muzzle.transform,projectile,1);session.Initialize(profile,catalog,new[]{potion,manaPotion});
            Assert(stats.MaxHealth==8&&stats.MaxMana==100&&stats.MoveSpeed==10&&stats.BagSize==30&&stats.AttackMultiplier==1&&stats.DashCooldown==5,"Design base stats");
            Assert(session.State==PlayerRunState.Lobby&&!session.CanAct&&!controller.TryAttackAt(Vector2.right)&&!move.TryDash(),"Lobby blocks combat and dash");
            health.TakeDamage(1);Assert(health.CurrentHealth==8,"Lobby blocks incoming damage");
            for(int i=0;i<6;i++)for(int j=0;j<PlayerProgression.MaxLevels[i];j++)Assert(session.Upgrade((PlayerUpgrade)i),"Upgrade "+i+" step "+j);
            Assert(stats.MaxHealth==15&&stats.MaxMana==200&&stats.MoveSpeed==20&&stats.BagSize==100&&Mathf.Approximately(stats.AttackMultiplier,1.5f)&&stats.DashCooldown==2,"All upgrade caps match design");
            Assert(profile.Data.crystals==802&&!session.Upgrade(PlayerUpgrade.Health),"Upgrade cost and cap enforcement");
            var loaded=new PlayerProgression(key);Assert(loaded.Data.levels[1]==100&&loaded.Data.crystals==802,"Progression persists across reload");
            Assert(session.ResetUpgrades()&&profile.Data.crystals==1000&&stats.MaxHealth==8,"Upgrade reset refunds all costs");
            Assert(!profile.TryUpgrade((PlayerUpgrade)99),"Unknown upgrade rejected");
            profile.Data.crystals=0;Assert(!session.Upgrade(PlayerUpgrade.Health),"Insufficient upgrade currency rejected");profile.Data.crystals=1000;
            while(bag.ItemCount<bag.BagSize)Assert(bag.TryAddWeapon(catalog[0]),"Fill backpack");
            Assert(!bag.TryAddWeapon(catalog[0])&&!bag.TryAddConsumable(potion),"Weapons and consumables share capacity");
            for(int i=0;i<5;i++)session.Upgrade(PlayerUpgrade.BagSize);
            for(int i=0;i<5;i++)bag.TryAddWeapon(catalog[0]);
            int total=bag.ItemCount+bag.Storage.Count+bag.StoredConsumables.Count;
            session.ResetUpgrades();Assert(bag.ItemCount==30&&bag.Storage.Count+bag.StoredConsumables.Count==5,"Reset moves overflow into storage");
            Assert(bag.ItemCount+bag.Storage.Count+bag.StoredConsumables.Count==total,"Reset never deletes items");
            bool fullTransfer=bag.Storage.Count>0?bag.Transfer(0,true):bag.TransferConsumable(0,true);
            Assert(!fullTransfer,"Cannot withdraw into full backpack");
            controller.Equip(1);var equipped=controller.CurrentWeapon;bag.SortWeapons(false);Assert(controller.CurrentWeapon==equipped,"Sorting preserves equipped weapon");
            bag.SortWeapons(true);Assert(controller.CurrentWeapon==equipped,"Grade sorting preserves equipped weapon");
            Assert(bag.Transfer(0,false),"Lobby can store a weapon");
            Assert(bag.Transfer(0,true),"Lobby can withdraw a weapon");
            bag.SetWeapons(catalog); // Keep consumables; all four weapon grades are tested independently below.
            Assert(Mathf.Approximately(bag.GetDropChance(WeaponGrade.Normal),0.05f)&&Mathf.Approximately(bag.GetDropChance(WeaponGrade.Rare),0.25f)&&Mathf.Approximately(bag.GetDropChance(WeaponGrade.Legendary),0.4f)&&Mathf.Approximately(bag.GetDropChance(WeaponGrade.Mid),0.3f),"All four death drop probabilities");
            var dropped=bag.DropWeaponsOnDeath(()=>0.2f);
            Assert(dropped.Count==3&&bag.Weapons.Count==1&&bag.Weapons[0]==catalog[0],"Per weapon death rolls preserve surviving basic weapon");
            bag.SetWeapons(catalog);
            Assert(session.BeginRun()&&session.CanAct,"Lobby starts expedition");
            Assert(!session.Upgrade(PlayerUpgrade.Health)&&!session.ResetUpgrades(),"Upgrades are restricted to lobby");
            Assert(!bag.Transfer(0,false),"Storage is restricted to lobby");
            move.AimAt(new Vector2(-10,3));Assert(body.flipX&&Mathf.Abs(player.transform.eulerAngles.z)<0.001f&&move.AimDirection.x<0,"Mouse left flips upright body and rotates weapon");
            move.AimAt(new Vector2(10,3));Assert(!body.flipX&&move.AimDirection.x>0,"Mouse right mirrors body immediately");
            Assert(Mathf.Approximately(PlayerMovement.NormalizeMovement(1,1).magnitude,1)&&PlayerMovement.NormalizeMovement(0,0)==Vector2.zero,"Diagonal normalized and opposing inputs cancel");
            move.AimAt(Vector2.left);Assert(move.TryDash()&&!move.TryDash()&&Field<Vector2>(move,"dashDirection").x<0,"Stationary dash uses aim and respects cooldown");
            Assert(controller.SwitchWeapon(1)&&controller.SwitchWeapon(-1),"Bidirectional weapon cycling");
            session.SetMenuOpen(true);float balance=mana.CurrentMana;
            Assert(Time.timeScale==0&&!session.CanAct&&!controller.TryAttackAt(Vector2.right)&&!session.CollectCrystals(1),"Inventory pauses combat and loot");
            health.TakeDamage(1);Assert(health.CurrentHealth==8,"Paused inventory prevents damage");
            session.SetMenuOpen(false);Assert(Time.timeScale==1&&session.CanAct,"Closing inventory restores time");
            var pickup=WorldPickup.SpawnCrystals(7,Vector2.zero);Assert(pickup.TryCollect(session)&&!pickup.TryCollect(session)&&session.RunCrystals==7,"Crystal pickup credited exactly once");
            var farPickup=WorldPickup.SpawnCrystals(2,new Vector2(100,0));Assert(!farPickup.TryCollect(session),"Pickup distance enforced");UnityEngine.Object.DestroyImmediate(farPickup.gameObject);
            var weaponPickup=WorldPickup.SpawnWeapon(catalog[0],Vector2.zero);int count=bag.Weapons.Count;
            Assert(weaponPickup.TryCollect(session)&&bag.Weapons.Count==count+1,"Field weapon enters real inventory");
            bag.TryAddConsumable(potion);bag.TryAddConsumable(manaPotion);
            int healthIndex=-1,manaIndex=-1;
            for(int i=0;i<bag.Consumables.Count;i++){if(bag.Consumables[i]==potion)healthIndex=i;if(bag.Consumables[i]==manaPotion)manaIndex=i;}
            Assert(!bag.UseConsumable(healthIndex),"Full health does not waste potion");
            health.TakeDamage(3);Assert(health.CurrentHealth==5,"Combat damage uses health stat");
            session.SetMenuOpen(true);Assert(bag.UseConsumable(healthIndex)&&health.CurrentHealth==8,"Healing potion works from paused inventory");
            session.SetMenuOpen(false);mana.TryUseMana(50);manaIndex=-1;
            for(int i=0;i<bag.Consumables.Count;i++)if(bag.Consumables[i]==manaPotion)manaIndex=i;
            Assert(bag.UseConsumable(manaIndex)&&mana.CurrentMana==80,"Mana potion restores configured amount");
            session.SetStage(3,"Validation Room");
            var monster=new GameObject("Kill Target");var hp=monster.AddComponent<MonsterHealth>();hp.TakeDamage(3,session);
            var elite=new GameObject("Elite Target");hp=elite.AddComponent<MonsterHealth>();Set(hp,"isElite",true);hp.TakeDamage(3,session);
            Assert(session.NormalKills==1&&session.EliteKills==1,"Normal and elite kill credit tracked independently");
            UnityEngine.Object.DestroyImmediate(monster);UnityEngine.Object.DestroyImmediate(elite);
            monster=new GameObject("Fractional Damage Target");hp=monster.AddComponent<MonsterHealth>();hp.TakeDamage(1.1f,session);
            Assert(Mathf.Approximately(hp.CurrentHealth,1.9f),"Attack power increments retain fractional damage");UnityEngine.Object.DestroyImmediate(monster);
            Set(bag,"normalDropChance",1f);Set(bag,"rareDropChance",1f);Set(bag,"advancedDropChance",1f);Set(bag,"midDropChance",1f);
            int beforeBank=profile.Data.crystals;int lost=bag.Weapons.Count;Set(health,"invincibleEndTime",0f);health.TakeDamage(100);
            Assert(health.IsDead&&session.State==PlayerRunState.Dead&&session.Result!=null,"Death creates run result");
            Assert(session.Result.earnedCrystals==7&&session.Result.finalCrystals==beforeBank+7,"Death banks collected crystals");
            Assert(session.Result.droppedWeapons.Count==lost&&session.Result.retainedWeapons==0,"Equipped and backpack weapons share death loss");
            Assert(session.Result.normalKills==1&&session.Result.eliteKills==1&&session.Result.floor==3&&session.Result.location=="Validation Room","Death result includes kills floor and location");
            session.CompleteDeath();Assert(profile.Data.crystals==beforeBank+7,"Death cannot settle twice");
            loaded=new PlayerProgression(key);Assert(loaded.Data.weaponIds.Count==0&&loaded.Data.crystals==beforeBank+7,"Death inventory and currency persist");
            var menu=player.AddComponent<PlayerMenuUI>();Call(menu,"Start");
            Font koreanFont=PlayerUITheme.GetFont();
            Assert(koreanFont!=null&&koreanFont.name!="LegacyRuntime","UI loads bundled Korean font");
            foreach(char character in "체력마나강화보관탐험결과줍기")
                Assert(koreanFont.HasCharacter(character),"Korean glyph available: "+character);
            var localizedWeapon=Weapon("Rocket Launcher",WeaponGrade.Rare);resources.Add(localizedWeapon);
            Assert(localizedWeapon.DisplayName=="로켓 런처"&&localizedWeapon.WeaponId=="Rocket Launcher"&&localizedWeapon.GradeName=="희귀","Korean display names preserve saved item IDs");
            var inputModule=UnityEngine.Object.FindAnyObjectByType<InputSystemUIInputModule>();
            Assert(inputModule!=null&&inputModule.actionsAsset!=null&&inputModule.point.action!=null,"Menus have active Input System UI bindings");
            var panel=Field<GameObject>(menu,"panel");Assert(panel!=null&&panel.GetComponentsInChildren<Button>().Length==1,"Death UI offers lobby return");
            Assert(session.ReturnToLobby()&&!health.IsDead&&session.State==PlayerRunState.Lobby,"Death returns to a live lobby state");
            Call(menu,"Rebuild");panel=Field<GameObject>(menu,"panel");Assert(panel.GetComponentsInChildren<Button>().Length==9,"Lobby UI exposes six upgrades reset inventory and start");
            foreach(Button button in panel.GetComponentsInChildren<Button>())
                if(button.gameObject.name=="탐험 시작") {button.onClick.Invoke();break;}
            Assert(session.State==PlayerRunState.Running&&session.RunCrystals==0&&session.NormalKills==0&&session.Result==null,"Start button begins a fresh run");
            return passed;
        }
        finally
        {
            if(player!=null)UnityEngine.Object.DestroyImmediate(player);if(template!=null)UnityEngine.Object.DestroyImmediate(template);
            foreach(var resource in resources)if(resource!=null)UnityEngine.Object.DestroyImmediate(resource);
            foreach(var pickup in UnityEngine.Object.FindObjectsByType<WorldPickup>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(pickup.gameObject);
            foreach(var projectile in UnityEngine.Object.FindObjectsByType<PlayerProjectile>(FindObjectsSortMode.None))UnityEngine.Object.DestroyImmediate(projectile.gameObject);
            PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();Time.timeScale=1f;
        }
    }
}
