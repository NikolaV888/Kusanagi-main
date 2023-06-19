using System.Text;
using UnityEngine;

[CreateAssetMenu(menuName="Item/Weapon", order=999)]
public class WeaponItem : EquipmentItem
{
    public enum weaponType { None, Sword, DualSword, Fan, Count }

    [Header("Weapon")]
    public weaponType type = weaponType.None;
    public AmmoItem requiredAmmo; 

    // tooltip
    public override string ToolTip()
    {
        StringBuilder tip = new StringBuilder(base.ToolTip());
        if (requiredAmmo != null)
            tip.Replace("{REQUIREDAMMO}", requiredAmmo.name);
        return tip.ToString();
    }
}