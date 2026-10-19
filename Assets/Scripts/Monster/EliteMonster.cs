using UnityEngine;

public enum WeaponDamageType
{
    NormalFirepower,
    Magic
}

public enum EliteResistanceType
{
    None,
    NormalFirepower,
    Magic
}

public class EliteMonster : MonoBehaviour
{
    public EliteResistanceType resistanceType = EliteResistanceType.NormalFirepower;

    public float resistancePercent = 70f;

    public Color normalFirepowerAuraColor = new Color(1f, 0.6f, 0.1f, 0.6f);

    public Color magicAuraColor = new Color(0.7f, 0.3f, 1f, 0.6f);

    public float auraScale = 1.3f;

    private SpriteRenderer bodyRenderer;

    private SpriteRenderer auraRenderer;

    private void Start()
    {
        bodyRenderer = GetComponent<SpriteRenderer>();
        CreateAura();
    }

    private void LateUpdate()
    {
        if (bodyRenderer != null && auraRenderer != null)
        {
            auraRenderer.sprite = bodyRenderer.sprite;
        }
    }

    public bool IsResistantTo(WeaponDamageType weaponType)
    {
        switch (resistanceType)
        {
            case EliteResistanceType.NormalFirepower:
                return weaponType == WeaponDamageType.NormalFirepower;
            case EliteResistanceType.Magic:
                return weaponType == WeaponDamageType.Magic;
            default:
                return false;
        }
    }

    public int ApplyResistance(int damage, WeaponDamageType weaponType)
    {
        if (!IsResistantTo(weaponType))
        {
            return damage;
        }

        float multiplier = 1f - resistancePercent / 100f;
        return Mathf.Max(1, Mathf.RoundToInt(damage * multiplier));
    }

    private void CreateAura()
    {
        if (bodyRenderer == null || resistanceType == EliteResistanceType.None)
        {
            return;
        }

        GameObject auraObject = new GameObject("EliteAura");
        auraObject.transform.SetParent(transform, false);
        auraObject.transform.localScale = Vector3.one * auraScale;

        auraRenderer = auraObject.AddComponent<SpriteRenderer>();
        auraRenderer.sprite = bodyRenderer.sprite;
        auraRenderer.color = GetAuraColor();
        auraRenderer.sortingLayerID = bodyRenderer.sortingLayerID;
        auraRenderer.sortingOrder = bodyRenderer.sortingOrder - 1;
    }

    private Color GetAuraColor()
    {
        return resistanceType == EliteResistanceType.Magic ? magicAuraColor : normalFirepowerAuraColor;
    }
}