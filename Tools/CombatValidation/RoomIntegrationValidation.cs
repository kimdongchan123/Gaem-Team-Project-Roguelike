using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RoomIntegrationValidation
{
    private static int phase, passed, frames, expectedCount, expectedBank;
    private static float deadline, waitUntil;
    private static RoomSystem combatRoom;
    private static void Assert(bool condition,string name)
    { if(!condition)throw new Exception(name);passed++;Debug.Log("PASS: "+name); }
    private static object Call(object target,string name,params object[] args) => target.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance).Invoke(target,args);
    private static void Set(object target,string name,object value) => target.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).SetValue(target,value);
    private static void Next(int value) {phase=value;frames=0;deadline=Time.realtimeSinceStartup+45f;}
    public static void Begin(int previousChecks)
    {
        passed=previousChecks;
        PlayerPrefs.DeleteKey("LastShelter.Player.v1");PlayerPrefs.Save();
        EditorApplication.update+=Tick;
        Next(1);SceneManager.LoadScene("LobbyScene");
    }
    private static void Tick()
    {
        try
        {
            if(Time.realtimeSinceStartup>deadline)throw new Exception("Scene integration timed out in phase "+phase);
            if(++frames<12)return;
            var session=UnityEngine.Object.FindAnyObjectByType<PlayerRunSession>();
            if(session==null)return;
            if(phase==1&&SceneManager.GetActiveScene().name=="LobbyScene")
            {
                Assert(UnityEngine.Object.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None).Length==1,"Real lobby has one installed player");
                Assert(session.State==PlayerRunState.Lobby&&session.CanMove&&!session.CanAct,"Real lobby allows movement and blocks attacks");
                Assert(session.GetComponent<TempPlayerController2D>()==null||!session.GetComponent<TempPlayerController2D>().enabled,"Temporary controller is replaced");
                var menu=session.GetComponent<PlayerMenuUI>();
                Assert(!menu.IsLobbyPanelOpen&&!session.MenuOpen,"Lobby opens without covering the physical room");
                var npc=UnityEngine.Object.FindAnyObjectByType<LobbyStatsUI>();npc.Open();
                Assert(menu.IsLobbyPanelOpen&&session.MenuOpen&&Time.timeScale==0,"Actual stat NPC opens shared upgrades");
                session.Progression.Data.crystals=20;session.Progression.Save();
                Assert(session.Upgrade(PlayerUpgrade.Health)&&session.Upgrade(PlayerUpgrade.AttackPower),"NPC upgrade model applies persistent upgrades");
                npc.Close();Assert(!session.MenuOpen&&Time.timeScale==1,"NPC closes shared modal cleanly");
                ValidateWallMovement(session);
                var inventory=session.GetComponent<PlayerInventory>();
                inventory.TryAddWeapon(inventory.Weapons[0]);expectedCount=inventory.Weapons.Count;
                Assert(!session.GetComponent<PlayerWeaponController>().TryAttackAt(Vector2.right),"Lobby weapon cannot fire");
                Next(2);Call(UnityEngine.Object.FindAnyObjectByType<LobbyDungeonEntrance>(),"EnterDungeon");
            }
            else if(phase==2&&SceneManager.GetActiveScene().name=="RoomSystemTest")
            {
                ValidateDungeon(session);
                Assert(session.GetComponent<PlayerInventory>().Weapons.Count==expectedCount,"Scene transition preserves inventory without duplicate starters");
                Assert(session.GetComponent<PlayerStats>().MaxHealth==9&&Mathf.Approximately(session.GetComponent<PlayerStats>().AttackMultiplier,1.1f),"Dungeon receives real lobby upgrades");
                var shop=UnityEngine.Object.FindAnyObjectByType<ShopTestUI>();shop.ToggleShop();
                Assert(session.MenuOpen&&Time.timeScale==0,"Shop modal blocks player combat");shop.CloseShop();
                foreach(var room in UnityEngine.Object.FindObjectsByType<RoomSystem>(FindObjectsSortMode.None))
                    if(room.Type==RoomSystem.RoomType.Combat&&room.transform.parent.GetComponentInChildren<MonsterSpawner>()!=null){combatRoom=room;break;}
                Assert(combatRoom!=null,"Actual combat room has a bound spawner");
                session.transform.position=combatRoom.GetComponent<BoxCollider2D>().bounds.center;
                session.GetComponent<Rigidbody2D>().position=session.transform.position;Physics2D.SyncTransforms();
                waitUntil=Time.realtimeSinceStartup+1.6f;Next(3);
            }
            else if(phase==3&&Time.realtimeSinceStartup>=waitUntil)
            {
                Assert(RoomSystem.CurrentRoom==combatRoom&&session.Location==combatRoom.RoomId,"Actual room triggers update final location");
                Assert(combatRoom.State==RoomSystem.RoomState.Active,"Entering combat locks the room");
                var monsters=UnityEngine.Object.FindObjectsByType<MonsterHealth>(FindObjectsSortMode.None);
                Assert(monsters.Length>0,"Actual dungeon spawns configured monsters");
                foreach(var monster in monsters)monster.TakeDamage(100000,session);
                Next(4);
            }
            else if(phase==4)
            {
                Assert(combatRoom.IsCleared,"Defeating spawned enemies opens the combat room");
                var pickups=UnityEngine.Object.FindObjectsByType<WorldPickup>(FindObjectsSortMode.None);
                Assert(pickups.Length>0&&session.NormalKills+session.EliteKills>0,"Actual dungeon kills produce credit and crystals");
                foreach(var pickup in pickups)
                {
                    session.transform.position=pickup.transform.position;
                    session.GetComponent<Rigidbody2D>().position=pickup.transform.position;
                    pickup.TryCollect(session);
                }
                expectedBank=session.Progression.Data.crystals+session.RunCrystals;
                Set(session.GetComponent<PlayerHealth>(),"invincibleEndTime",0f);
                session.GetComponent<PlayerHealth>().TakeDamage(100000);
                Assert(session.State==PlayerRunState.Dead&&session.Result.location==combatRoom.RoomId,"Dungeon death records actual room");
                expectedCount=session.GetComponent<PlayerInventory>().Weapons.Count;
                Assert(session.Result.finalCrystals==expectedBank,"Dungeon death settles collected rewards");
                Next(5);
            }
            else if(phase==5)
            {
                Assert(!session.CanMove&&session.GetComponent<PlayerHealth>().IsDead,"Dead dungeon player cannot move");
                Assert(session.ReturnToLobby(),"Death result loads physical lobby");Next(6);
            }
            else if(phase==6&&SceneManager.GetActiveScene().name=="LobbyScene")
            {
                Assert(session.State==PlayerRunState.Lobby&&session.CanMove&&!session.MenuOpen,"Returning lobby is playable without stale modal state");
                Assert(session.Progression.Data.crystals==expectedBank&&session.GetComponent<PlayerInventory>().Weapons.Count==expectedCount,"Lobby return restores settled inventory and currency");
                Assert(session.GetComponent<PlayerStats>().MaxHealth==9,"Upgrade survives dungeon death and return");
                Next(7);SceneManager.LoadScene("RoomSystemTest");
            }
            else if(phase==7&&SceneManager.GetActiveScene().name=="RoomSystemTest")
            {
                ValidateDungeon(session);
                Assert(session.RunCrystals==0&&session.NormalKills==0&&session.Result==null,"Direct dungeon play starts a fresh run");
                PlayerPrefs.DeleteKey("LastShelter.Player.v1");PlayerPrefs.Save();
                EditorApplication.update-=Tick;
                Debug.Log("COMBAT_VALIDATION_SUCCESS: "+passed+" checks");EditorApplication.Exit(0);
            }
        }
        catch(Exception exception)
        {EditorApplication.update-=Tick;Debug.LogException(exception);EditorApplication.Exit(1);}
    }
    private static void ValidateDungeon(PlayerRunSession session)
    {
        Assert(session.State==PlayerRunState.Running&&session.CanAct,"Real dungeon starts shared player combat");
        var spawn=GameObject.Find("DungeonEntrySpawn");
        Assert(spawn!=null&&Vector2.Distance(session.transform.position,spawn.transform.position)<0.25f,"Dungeon player appears at configured spawn");
        Assert(UnityEngine.Object.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None).Length==1,"Dungeon has no duplicate sample player");
        Assert(UnityEngine.Object.FindAnyObjectByType<PlayerCameraController>()!=null,"Existing dungeon camera is retained");
    }
    private static void ValidateWallMovement(PlayerRunSession session)
    {
        var movement=session.GetComponent<PlayerMovement>();var body=session.GetComponent<Rigidbody2D>();
        Assert(body!=null&&body.bodyType==RigidbodyType2D.Dynamic,"Room player uses dynamic rigidbody movement");
        Vector2 saved=body.position;var mode=Physics2D.simulationMode;Physics2D.simulationMode=SimulationMode2D.Script;
        try
        {
            body.position=Vector2.zero;session.transform.position=Vector3.zero;Physics2D.SyncTransforms();Set(movement,"moveInput",Vector2.right);
            for(int i=0;i<5;i++){Call(movement,"FixedUpdate");Physics2D.Simulate(0.02f);}
            Assert(body.position.x>0.5f,"Player can walk across actual lobby floor");
            var wall=GameObject.Find("Wall_Left").GetComponent<BoxCollider2D>();
            body.position=new Vector2(wall.bounds.max.x+0.5f,wall.bounds.center.y);session.transform.position=body.position;Physics2D.SyncTransforms();
            Set(movement,"moveInput",Vector2.left);
            for(int i=0;i<20;i++){Call(movement,"FixedUpdate");Physics2D.Simulate(0.02f);}
            Assert(body.position.x>wall.bounds.max.x,"Player cannot pass through the actual lobby wall");
            movement.AimAt((Vector2)session.transform.position+Vector2.left);movement.ResetMovement();Set(movement,"moveInput",Vector2.left);movement.TryDash();
            for(int i=0;i<8;i++){Call(movement,"FixedUpdate");Physics2D.Simulate(0.02f);}
            Assert(body.position.x>wall.bounds.max.x,"Dash cannot bypass actual lobby wall");
        }
        finally
        {movement.ResetMovement();body.position=saved;session.transform.position=saved;body.linearVelocity=Vector2.zero;Physics2D.SyncTransforms();Physics2D.simulationMode=mode;}
    }
}
