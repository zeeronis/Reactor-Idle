using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class HeatVentInfo: ItemInfo
{
    public float decreaseHeat;

    internal override string GetLocaleDesc(string nonFormattedText)
    {
        return string.Format(nonFormattedText, decreaseHeat * (1 + PlayerManager.Instance.player.upgrades[UpgradeType.Vent_Eff]));
    }
}

