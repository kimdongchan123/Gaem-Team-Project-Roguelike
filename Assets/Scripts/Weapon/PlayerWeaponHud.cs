using UnityEngine;

public class PlayerWeaponHud : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private PlayerWeaponController weaponController;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerMana playerMana;

    private GUIStyle labelStyle;

    public void SetTarget(PlayerWeaponController newWeaponController, PlayerHealth newPlayerHealth, PlayerMana newPlayerMana)
    {
        weaponController = newWeaponController;
        playerHealth = newPlayerHealth;
        playerMana = newPlayerMana;
    }

    private void OnGUI()
    {
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 20;
            labelStyle.normal.textColor = Color.white;
        }

        string weaponName = weaponController != null ? weaponController.CurrentWeaponName : "None";
        int weaponIndex = weaponController != null ? weaponController.CurrentWeaponNumber : 0;
        int weaponCount = weaponController != null ? weaponController.WeaponCount : 0;
        string healthText = playerHealth != null ? $"{playerHealth.CurrentHealth}/{playerHealth.MaxHealth}" : "-";
        string manaText = playerMana != null ? $"{playerMana.CurrentMana}/{playerMana.MaxMana}" : "-";

        GUI.Label(new Rect(20f, 20f, 520f, 32f), $"Weapon [{weaponIndex}/{weaponCount}] {weaponName}", labelStyle);
        GUI.Label(new Rect(20f, 52f, 520f, 32f), $"HP {healthText}   Mana {manaText}", labelStyle);
        GUI.Label(new Rect(20f, 84f, 720f, 32f), "WASD Move / Mouse Aim / Left Click Attack / Q Next Weapon / Shift Dash", labelStyle);
    }
}
