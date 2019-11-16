using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class UpgradeInfo
{
    public int maxUpgradeLvl;
    public float costBase;
    public float costMultipler;
    public string keyName;
    public string keyDesc;

    [NonSerialized]
    public ShopUpgradeItem shopUpgrade;
    [NonSerialized]
    public Sprite defaultSprite;

    public float GetCost(int lvl)
    {
        return costBase * (float)(Math.Pow(costMultipler, lvl) * ReactorManager.Factorial(lvl));
    }
}
