using UnityEngine;

// 몬스터 머리 위에 작은 HP바를 표시 (MonsterHealth가 자동으로 붙여줌)
public class MonsterHealthBar : MonoBehaviour
{
    [Header("Bar")]
    [SerializeField] private Vector2 barSize = new Vector2(0.8f, 0.1f);
    [SerializeField] private float heightOffset = 0.15f;
    [SerializeField] private Color backgroundColor = new Color(0f, 0f, 0f, 0.7f);
    [SerializeField] private Color fullColor = new Color(0.3f, 0.9f, 0.3f);
    [SerializeField] private Color emptyColor = new Color(0.9f, 0.2f, 0.2f);
    [Tooltip("몬스터 스프라이트보다 몇 단계 위에 그릴지. 방 암전(Fog)보다 낮아야 암전 상태에서 가려짐")]
    [SerializeField] private int sortingOrderOffset = 1;
    [SerializeField] private bool hideWhenFull;

    private static Sprite pixelSprite;

    private MonsterHealth health;
    private SpriteRenderer monsterSprite;
    private Transform barRoot;
    private Transform fill;
    private SpriteRenderer fillRenderer;

    private void Start()
    {
        health = GetComponent<MonsterHealth>();
        monsterSprite = GetComponentInChildren<SpriteRenderer>();

        // 몬스터 크기 변화(자폭 몬스터 등)에 영향받지 않도록 부모 없이 생성
        // 몬스터와 같은 정렬 기준을 써서 방 암전에 몬스터와 함께 가려지고, 부채꼴 시야에서만 같이 보이게 함
        int baseOrder = monsterSprite != null ? monsterSprite.sortingOrder + sortingOrderOffset : sortingOrderOffset;
        barRoot = new GameObject(name + " HP Bar").transform;
        CreateBarPart("Background", backgroundColor, baseOrder);
        fill = CreateBarPart("Fill", fullColor, baseOrder + 1).transform;
        fillRenderer = fill.GetComponent<SpriteRenderer>();
        LateUpdate();
    }

    private void LateUpdate()
    {
        if (barRoot == null || health == null)
        {
            return;
        }

        float ratio = health.MaxHealth > 0f ? Mathf.Clamp01(health.CurrentHealth / health.MaxHealth) : 0f;
        barRoot.gameObject.SetActive(!(hideWhenFull && ratio >= 1f));

        float top = monsterSprite != null ? monsterSprite.bounds.max.y : transform.position.y + 0.5f;
        barRoot.position = new Vector3(transform.position.x, top + heightOffset, transform.position.z);

        // 왼쪽 기준으로 줄어들게 함
        fill.localScale = new Vector3(barSize.x * ratio, barSize.y, 1f);
        fill.localPosition = new Vector3(-barSize.x * (1f - ratio) * 0.5f, 0f, 0f);
        fillRenderer.color = Color.Lerp(emptyColor, fullColor, ratio);
    }

    private void OnDestroy()
    {
        if (barRoot != null)
        {
            Destroy(barRoot.gameObject);
        }
    }

    private GameObject CreateBarPart(string partName, Color color, int order)
    {
        GameObject part = new GameObject(partName);
        part.transform.SetParent(barRoot, false);
        part.transform.localScale = new Vector3(barSize.x, barSize.y, 1f);
        SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = GetPixelSprite();
        renderer.color = color;
        renderer.sortingOrder = order;
        if (monsterSprite != null) renderer.sortingLayerID = monsterSprite.sortingLayerID;
        return part;
    }

    private static Sprite GetPixelSprite()
    {
        if (pixelSprite == null)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            pixelSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }
        return pixelSprite;
    }
}
