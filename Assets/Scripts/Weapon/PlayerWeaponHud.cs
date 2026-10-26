using UnityEngine;
using UnityEngine.UI;

public class PlayerWeaponHud : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMana playerMana;

    private GameObject canvasObject;
    private RectTransform healthFill;
    private RectTransform manaFill;
    private Text healthText;
    private Text manaText;
    private Text weaponText;
    private Text detailText;
    private Text statusText;
    private Font font;
    private PlayerRunSession session;

    public void SetTarget(PlayerWeaponController controller, PlayerHealth health, PlayerMana mana)
    {
        weaponController = controller;
        playerHealth = health;
        playerMana = mana;
    }

    private void Start()
    {
        if (weaponController == null) weaponController = GetComponent<PlayerWeaponController>();
        if (playerHealth == null) playerHealth = GetComponent<PlayerHealth>();
        if (playerMana == null) playerMana = GetComponent<PlayerMana>();
        session = GetComponent<PlayerRunSession>();
        font = PlayerUITheme.GetFont();
        canvasObject = new GameObject("Player HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

        RectTransform panel = CreateRect("Status Panel", canvasObject.transform, new Vector2(24f, -24f), new Vector2(380f, 224f));
        AddImage(panel, new Color(0.035f, 0.05f, 0.075f, 0.94f));
        CreateText("플레이어 상태", panel, new Vector2(18f, -12f), new Vector2(344f, 22f), 15);
        healthFill = CreateBar("Health", panel, -44f, new Color(0.85f, 0.2f, 0.27f), out healthText);
        manaFill = CreateBar("Mana", panel, -80f, new Color(0.15f, 0.45f, 0.85f), out manaText);
        weaponText = CreateText("", panel, new Vector2(18f, -118f), new Vector2(344f, 26f), 18);
        detailText = CreateText("", panel, new Vector2(18f, -148f), new Vector2(344f, 22f), 13);
        statusText = CreateText("", panel, new Vector2(18f, -177f), new Vector2(344f, 22f), 14);
        CreateText("WASD 이동 · Shift 대시 · 좌클릭 공격 · Q 교체", panel,
            new Vector2(18f, -204f), new Vector2(344f, 16f), 11);
        Refresh();
    }

    private void Update()
    {
        if (canvasObject != null)
        {
            canvasObject.SetActive(session == null || session.CanMove);
            Refresh();
        }
    }

    private void Refresh()
    {
        float hp = playerHealth != null ? (float)playerHealth.CurrentHealth / Mathf.Max(1, playerHealth.MaxHealth) : 0f;
        float mp = playerMana != null ? playerMana.NormalizedMana : 0f;
        healthFill.sizeDelta = new Vector2(344f * Mathf.Clamp01(hp), 28f);
        manaFill.sizeDelta = new Vector2(344f * mp, 28f);
        healthText.text = playerHealth != null ? $"체력   {playerHealth.CurrentHealth} / {playerHealth.MaxHealth}" : "체력   --";
        manaText.text = playerMana != null ? $"마나   {playerMana.CurrentMana} / {playerMana.MaxMana}" : "마나   --";
        WeaponData weapon = weaponController != null ? weaponController.CurrentWeapon : null;
        weaponText.text = weapon != null ? $"[{weaponController.CurrentWeaponNumber}/{weaponController.WeaponCount}]  {weapon.DisplayName}" : "장착한 무기 없음";
        int cost = weapon != null && weapon.IsMagicWeapon ? weapon.ManaCost : 0;
        detailText.text = weapon != null ? $"공격 {weapon.Damage} × {weapon.ProjectileCount} · 간격 {weapon.AttackDelay:0.##}초 · 마나 {cost}" : "";
        bool dead = playerHealth != null && playerHealth.IsDead;
        bool lowMana = weaponController != null && !weaponController.HasEnoughMana;
        float cooldown = weaponController != null ? weaponController.AttackCooldownRemaining : 0f;
        statusText.text = dead ? "사망" : weapon == null ? "무기 없음" : lowMana ? "마나 부족 · 회복 중" : cooldown > 0f ? $"재사용 대기  {cooldown:0.0}초" : "공격 준비 완료";
        if (session != null && session.State == PlayerRunState.Lobby) statusText.text = "로비 · E 상호작용 / I 가방";
        statusText.color = dead || lowMana ? new Color(1f, 0.65f, 0.35f) : new Color(0.55f, 0.9f, 0.75f);
    }

    private RectTransform CreateBar(string name, Transform parent, float y, Color color, out Text label)
    {
        RectTransform background = CreateRect(name, parent, new Vector2(18f, y), new Vector2(344f, 28f));
        AddImage(background, new Color(0.13f, 0.17f, 0.22f));
        RectTransform fill = CreateRect("Fill", background, Vector2.zero, new Vector2(344f, 28f));
        AddImage(fill, color);
        label = CreateText("", background, new Vector2(10f, 0f), new Vector2(324f, 28f), 15);
        return fill;
    }

    private static void AddImage(RectTransform rect, Color color)
    {
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
    }

    private Text CreateText(string value, Transform parent, Vector2 position, Vector2 size, int fontSize)
    {
        RectTransform rect = CreateRect("Label", parent, position, size);
        Text text = rect.gameObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleLeft;
        text.text = value;
        text.raycastTarget = false;
        return text;
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private void OnEnable()
    {
        if (canvasObject != null) canvasObject.SetActive(true);
    }

    private void OnDisable()
    {
        if (canvasObject != null) canvasObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (canvasObject != null) Destroy(canvasObject);
    }
}
