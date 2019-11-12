using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class ItemInfo
{
    [NonSerialized]
    public GameObject prefab;
    [NonSerialized]
    public ShopItem shopItem;

    public float durability;
    public float cost;
    public string keyName;
    public string keyDesc;

    internal virtual string GetLocaleDesc(string nonFormattedText)
    {
        return string.Format(nonFormattedText, 
            durability * (1 + PlayerManager.Instance.player.upgrades[
                prefab.GetComponent<IItem>().ItemType == ItemType.Battery 
                ? UpgradeType.Battery_Durability 
                : UpgradeType.Plate_Durability]));
    }

    internal static float GetItemDurabilityMultipler(ItemType itemType, int itemGrade)
    {
        switch (itemType)
        {
            case ItemType.Rod:
                if (itemGrade < 3)
                    return (PlayerManager.Instance.player.upgrades[UpgradeType.RodGreen_Durability] + 1);
                if (itemGrade < 6)
                    return (PlayerManager.Instance.player.upgrades[UpgradeType.RodBlue_Durability] + 1);
                if (itemGrade < 9)
                    return (PlayerManager.Instance.player.upgrades[UpgradeType.RodBlue_Durability] + 1);
                if (itemGrade < 12)
                    return (PlayerManager.Instance.player.upgrades[UpgradeType.RodBlue_Durability] + 1);
                if (itemGrade < 15)
                    return (PlayerManager.Instance.player.upgrades[UpgradeType.RodBlue_Durability] + 1);
                if (itemGrade < 18)
                    return (PlayerManager.Instance.player.upgrades[UpgradeType.RodBlue_Durability] + 1);
                return 1;
            case ItemType.HeatPipe:
                return (PlayerManager.Instance.player.upgrades[UpgradeType.Pipe_Durability] + 1);
            case ItemType.HeatVent:
                return (PlayerManager.Instance.player.upgrades[UpgradeType.Vent_Durability] + 1);
            case ItemType.HeatPlate:
                return (PlayerManager.Instance.player.upgrades[UpgradeType.Plate_Durability] + 1);
            case ItemType.Battery:
                return (PlayerManager.Instance.player.upgrades[UpgradeType.Battery_Durability] + 1);

            default:
                return 1;
        }
    }
}
