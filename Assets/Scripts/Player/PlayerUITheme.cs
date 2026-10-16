using UnityEngine;

// The resource asset references the existing Korean font so builds include its font data.
public class PlayerUITheme : ScriptableObject
{
    [SerializeField] private Font koreanFont;
    public static Font GetFont()
    {
        PlayerUITheme theme = Resources.Load<PlayerUITheme>("PlayerUITheme");
        return theme != null && theme.koreanFont != null ? theme.koreanFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    public static string DisplayName(string name)
    {
        switch (name)
        {
            case "Rocket Launcher": return "로켓 런처";
            case "Shotgun": return "산탄총";
            case "Axe": return "도끼";
            case "Swiss Dagger": return "스위스 단검";
            case "Fire Ball": return "화염구";
            case "Guided Missile": return "유도 미사일";
            case "Black Hole Gun": return "블랙홀 건";
            case "Magic Gatling": return "마법 개틀링";
            case "Health Potion": return "체력 회복약";
            case "Mana Potion": return "마나 회복약";
            default: return name;
        }
    }

    public static string GradeName(WeaponGrade grade)
    {
        switch (grade)
        {
            case WeaponGrade.Normal: return "일반";
            case WeaponGrade.Rare: return "희귀";
            case WeaponGrade.Legendary: return "전설";
            case WeaponGrade.Mid: return "미드롬";
            default: return "등급 없음";
        }
    }
}
