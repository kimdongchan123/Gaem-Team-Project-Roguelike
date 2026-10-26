using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class PlayerMenuUI : MonoBehaviour
{
    private PlayerRunSession session;
    private PlayerInventory inventory;
    private PlayerWeaponController controller;
    private Canvas canvas;
    private GameObject panel;
    private GameObject ownedEventSystem;
    private RectTransform reticle;
    private Text runText, pickupText;
    private Font font;
    private PlayerRunState previousState;
    private bool inventoryView, storageView;
    private bool lobbyPanelOpen;
    public bool IsLobbyPanelOpen => lobbyPanelOpen && session != null && session.State == PlayerRunState.Lobby;

    public void OpenLobbyUpgrades()
    {
        if (session == null || session.State != PlayerRunState.Lobby) return;
        lobbyPanelOpen = true; inventoryView = storageView = false;
        session.SetMenuOpen(true); Rebuild();
    }

    public void CloseLobbyUpgrades()
    {
        if (!lobbyPanelOpen) return;
        lobbyPanelOpen = false; inventoryView = false;
        session.SetMenuOpen(false); Rebuild();
    }
    private bool originalCursorVisible;
    private CursorLockMode originalCursorLock;
    private int page;
    private float nextPickupRefresh;
    private readonly string[] statNames = { "체력", "마나", "이동 속도", "가방 크기", "공격력 배수", "대시 재사용 대기" };

    private void Start()
    {
        session = GetComponent<PlayerRunSession>(); inventory = GetComponent<PlayerInventory>();
        controller = GetComponent<PlayerWeaponController>();
        font = PlayerUITheme.GetFont();
        originalCursorVisible = Cursor.visible;
        originalCursorLock = Cursor.lockState;
        Cursor.lockState = CursorLockMode.None;
        var obj = new GameObject("Player Menus",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        canvas = obj.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 200;
        var scaler = obj.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280,720); scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        if (EventSystem.current == null)
            ownedEventSystem = new GameObject("Player UI Events",typeof(EventSystem),typeof(InputSystemUIInputModule));
        reticle = Rect("Crosshair",canvas.transform,Vector2.zero,new Vector2(24,24));
        reticle.anchorMin = reticle.anchorMax = new Vector2(0.5f,0.5f); reticle.pivot = Vector2.one*0.5f;
        Image(Rect("Left",reticle,new Vector2(0,-10),new Vector2(7,2)),Color.white,false);
        Image(Rect("Right",reticle,new Vector2(17,-10),new Vector2(7,2)),Color.white,false);
        Image(Rect("Top",reticle,new Vector2(11,0),new Vector2(2,7)),Color.white,false);
        Image(Rect("Bottom",reticle,new Vector2(11,-17),new Vector2(2,7)),Color.white,false);
        runText = Label(canvas.transform,new Vector2(940,-24),new Vector2(310,120),"",14);
        pickupText = Label(canvas.transform,new Vector2(420,-660),new Vector2(540,36),"",18);
        previousState = session.State;
        Rebuild();
    }
    private void Update()
    {
        if (canvas == null) return;
        if (previousState != session.State)
        {
            previousState = session.State; inventoryView = storageView = lobbyPanelOpen = false; page = 0; Rebuild();
        }
        if (session.State == PlayerRunState.Lobby && lobbyPanelOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseLobbyUpgrades();
        }
        else if ((session.State == PlayerRunState.Running || (session.UsesSceneFlow && session.State == PlayerRunState.Lobby)) &&
            (!session.MenuOpen || inventoryView) && Keyboard.current != null &&
            (Keyboard.current.iKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame))
        {
            inventoryView = !inventoryView; storageView = false; page = 0;
            session.SetMenuOpen(inventoryView); Rebuild();
        }
        bool showAim = session.CanAct && Mouse.current != null && Application.isFocused;
        reticle.gameObject.SetActive(showAim); Cursor.visible = !showAim;
        if (showAim)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform,
                Mouse.current.position.ReadValue(),null,out Vector2 point);
            reticle.anchoredPosition = point;
        }
        runText.text = session.State == PlayerRunState.Running ? $"{session.Floor}층 · {session.Location}\n크리스탈 +{session.RunCrystals} · 일반 {session.NormalKills} / 엘리트 {session.EliteKills}\nI 가방 · E 줍기\nQ / 휠 무기 교체" : session.UsesSceneFlow ? "로비\nE NPC / 던전 입장\nI 가방" : "";
        if (Time.unscaledTime >= nextPickupRefresh)
        {
            nextPickupRefresh = Time.unscaledTime+0.2f; pickupText.text = "";
            if (session.CanAct)
            {
                float nearest = 1.25f;
                foreach (WorldPickup pickup in FindObjectsByType<WorldPickup>(FindObjectsSortMode.None))
                {
                    float distance = Vector2.Distance(transform.position,pickup.transform.position);
                    if (distance <= nearest) { nearest = distance; pickupText.text = "E  줍기 · " + pickup.Label; }
                }
            }
        }
    }
    private void Rebuild()
    {
        if (panel != null) { panel.SetActive(false); Destroy(panel); }
        if (session.State == PlayerRunState.Running && !inventoryView) return;
        if (session.UsesSceneFlow && session.State == PlayerRunState.Lobby && !lobbyPanelOpen && !inventoryView) return;
        RectTransform root = Rect("Menu",canvas.transform,new Vector2(190,-60),new Vector2(900,600));
        panel = root.gameObject; Image(root,new Color(0.035f,0.05f,0.075f,0.98f));
        if (session.State == PlayerRunState.Dead) { BuildDeath(root); return; }
        if (inventoryView) { BuildInventory(root); return; }
        BuildLobby(root);
    }
    private void BuildLobby(RectTransform root)
    {
        Label(root,new Vector2(30,-20),new Vector2(840,42),"로비 / 플레이어 강화",26);
        Label(root,new Vector2(30,-70),new Vector2(840,30),$"블루 크리스탈: {session.Progression.Data.crystals}개    |    강화 1회당 크리스탈 1개",18);
        PlayerStats stats = GetComponent<PlayerStats>();
        string[] values = { $"{stats.MaxHealth} / 15",$"{stats.MaxMana} / 200",$"{stats.MoveSpeed:0.#} / 20",$"{stats.BagSize} / 100",$"{stats.AttackMultiplier:0.0} / 1.5",$"{stats.DashCooldown:0.0}초 / 2초" };
        for (int i=0;i<6;i++)
        {
            int index=i; float y=-120-i*55;
            Label(root,new Vector2(30,y),new Vector2(170,36),statNames[i],18);
            Label(root,new Vector2(210,y),new Vector2(190,36),values[i],18);
            int level=session.Progression.Data.levels[i], max=PlayerProgression.MaxLevels[i];
            Label(root,new Vector2(420,y),new Vector2(180,36),$"강화 {level} / {max}",16);
            Button(root,new Vector2(650,y),new Vector2(215,36),level>=max ? "최대 강화" : "강화 (크리스탈 1개)",
                ()=>{session.Upgrade((PlayerUpgrade)index);Rebuild();},level<max && session.Progression.Data.crystals>0);
        }
        Button(root,new Vector2(30,-475),new Vector2(255,40),"초기화 및 전액 환급",()=>{session.ResetUpgrades();Rebuild();});
        Button(root,new Vector2(310,-475),new Vector2(255,40),"가방 / 보관함",()=>{inventoryView=true;page=0;Rebuild();});
        Button(root,new Vector2(590,-475),new Vector2(275,40),session.UsesSceneFlow ? "닫기" : "탐험 시작",()=>
        {
            if (session.UsesSceneFlow) CloseLobbyUpgrades();
            else {session.BeginRun();Rebuild();}
        });
        Label(root,new Vector2(30,-535),new Vector2(835,50),"가방 초과분은 보관함으로 이동합니다. E로 근처 아이템을 줍습니다.\n강화와 남은 아이템은 이 컴퓨터에 저장됩니다.",15);
    }
    private void BuildInventory(RectTransform root)
    {
        Label(root,new Vector2(30,-20),new Vector2(840,42),$"{(storageView?"보관함":"인벤토리")}   가방 {inventory.ItemCount} / {inventory.BagSize}",26);
        Button(root,new Vector2(30,-80),new Vector2(190,36),"이름순 정렬",()=>{inventory.SortWeapons(false);page=0;Rebuild();});
        Button(root,new Vector2(235,-80),new Vector2(190,36),"등급순 정렬",()=>{inventory.SortWeapons(true);page=0;Rebuild();});
        if (session.State==PlayerRunState.Lobby)
            Button(root,new Vector2(440,-80),new Vector2(210,36),storageView?"가방 보기":"보관함 보기",()=>{storageView=!storageView;page=0;Rebuild();});
        var source = storageView ? inventory.Storage : inventory.Weapons;
        var groups = new List<InventoryRow>();
        var map = new Dictionary<string,InventoryRow>();
        for(int i=0;i<source.Count;i++)
        {
            WeaponData weapon=source[i]; string key=weapon.WeaponId+":"+(int)weapon.WeaponGrade;
            if(!map.TryGetValue(key,out InventoryRow row)) { row=new InventoryRow { weapon=weapon,index=i }; map.Add(key,row);groups.Add(row); }
            row.count++;
        }
        var consumables = storageView ? inventory.StoredConsumables : inventory.Consumables;
        for(int i=0;i<consumables.Count;i++)
        {
            ConsumableData item=consumables[i]; string key="consumable:"+item.ItemId;
            if(!map.TryGetValue(key,out InventoryRow row)) { row=new InventoryRow { consumable=item,index=i };map.Add(key,row);groups.Add(row); }
            row.count++;
        }
        int pages=Mathf.Max(1,Mathf.CeilToInt(groups.Count/7f));page=Mathf.Clamp(page,0,pages-1);
        for(int n=0;n<7 && page*7+n<groups.Count;n++)
        {
            InventoryRow row=groups[page*7+n]; float y=-135-n*47;
            string equipped=row.weapon!=null && !storageView && controller.CurrentWeapon==row.weapon ? " (장착 중)" : "";
            string label=row.weapon!=null ? $"{row.weapon.DisplayName}{equipped}   [{row.weapon.GradeName}]   {row.count}개" : $"{row.consumable.DisplayName}   [소모품]   {row.count}개";
            Label(root,new Vector2(30,y),new Vector2(515,40),label,17);
            if(!storageView && row.weapon!=null) Button(root,new Vector2(555,y),new Vector2(140,34),"장착",()=>{controller.Equip(row.index);Rebuild();});
            if(!storageView && row.consumable!=null && session.State==PlayerRunState.Running)
                Button(root,new Vector2(555,y),new Vector2(140,34),"사용",()=>{inventory.UseConsumable(row.index);Rebuild();});
            if(session.State==PlayerRunState.Lobby)
                Button(root,new Vector2(710,y),new Vector2(155,34),storageView?"꺼내기":"보관",()=>{
                    if(row.weapon!=null)inventory.Transfer(row.index,storageView);else inventory.TransferConsumable(row.index,storageView);Rebuild();},!storageView||inventory.ItemCount<inventory.BagSize);
        }
        Label(root,new Vector2(30,-480),new Vector2(450,28),groups.Count==0?"아이템이 없습니다.":"같은 아이템은 묶어 표시됩니다. (1개당 1칸)",15);
        Button(root,new Vector2(30,-530),new Vector2(160,38),"이전",()=>{page--;Rebuild();},page>0);
        Label(root,new Vector2(210,-530),new Vector2(150,38),$"{page+1} / {pages}",17);
        Button(root,new Vector2(365,-530),new Vector2(160,38),"다음",()=>{page++;Rebuild();},page+1<pages);
        Button(root,new Vector2(650,-530),new Vector2(215,38),"돌아가기",()=>{inventoryView=false;session.SetMenuOpen(lobbyPanelOpen);Rebuild();});
    }
    private class InventoryRow { public WeaponData weapon; public ConsumableData consumable; public int index,count; }
    private void BuildDeath(RectTransform root)
    {
        PlayerRunResult result=session.Result;
        Label(root,new Vector2(30,-25),new Vector2(840,45),"탐험 결과",28);
        if(result==null) return;
        string text=$"생존 시간: {TimeSpan.FromSeconds(result.survivalSeconds):mm\\:ss}\n\n"+
            $"최종 위치: {result.floor}층 · {result.location}\n\n"+
            $"일반 몬스터 처치: {result.normalKills}마리    엘리트: {result.eliteKills}마리\n\n"+
            $"획득 블루 크리스탈: +{result.earnedCrystals}개    최종 보유: {result.finalCrystals}개\n\n"+
            $"남은 무기: {result.retainedWeapons}개    잃은 무기: {result.droppedWeapons.Count}개";
        Label(root,new Vector2(30,-95),new Vector2(840,310),text,20);
        Label(root,new Vector2(30,-420),new Vector2(840,65),result.droppedWeapons.Count==0?"잃은 무기가 없습니다.":"잃은 무기: "+string.Join(", ",result.droppedWeapons),15);
        Button(root,new Vector2(590,-530),new Vector2(275,40),"로비로 돌아가기",()=>{session.ReturnToLobby();Rebuild();});
    }
    private static RectTransform Rect(string name,Transform parent,Vector2 position,Vector2 size)
    {
        var rect=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);
        rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(0,1);rect.anchoredPosition=position;rect.sizeDelta=size;return rect;
    }
    private static Image Image(RectTransform rect,Color color,bool raycast=true)
    {
        var image=rect.gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=raycast;return image;
    }
    private Text Label(Transform parent,Vector2 position,Vector2 size,string value,int fontSize)
    {
        var text=Rect("Label",parent,position,size).gameObject.AddComponent<Text>();text.font=font;text.fontSize=fontSize;
        text.color=Color.white;text.alignment=TextAnchor.MiddleLeft;text.text=value;text.raycastTarget=false;return text;
    }
    private void Button(Transform parent,Vector2 position,Vector2 size,string value,Action action,bool enabled=true)
    {
        var rect=Rect(value,parent,position,size);Image image=Image(rect,new Color(0.1f,0.35f,0.48f));
        var button=rect.gameObject.AddComponent<Button>();button.targetGraphic=image;button.interactable=enabled;button.onClick.AddListener(()=>action());
        Text text=Label(rect,new Vector2(8,0),size-new Vector2(16,0),value,16);text.alignment=TextAnchor.MiddleCenter;
    }
    private void OnDisable() { Cursor.visible=originalCursorVisible; Cursor.lockState=originalCursorLock; if(session!=null)session.SetMenuOpen(false); }
    private void OnDestroy() { if(canvas!=null)Destroy(canvas.gameObject);if(ownedEventSystem!=null)Destroy(ownedEventSystem); }
}
