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
}
