using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class HeatPipeInfo: ItemInfo
{
    public float heatThroughput;

    internal override string GetLocaleDesc(string nonFormattedText)
    {
        return string.Format(nonFormattedText, heatThroughput * (1 + PlayerManager.Instance.player.upgrades[UpgradeType.Pipe_Eff]));
    }
}
