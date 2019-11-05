using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class RodInfo: ItemInfo
{
    public float outPower;
    public float outHeat;

    internal override string GetLocaleDesc(string nonFormattedText)
    {
        UpgradeType upgradeType;
        int gradeType = prefab.GetComponent<IItem>().itemGradeType;

        if (gradeType < 3) upgradeType = UpgradeType.RodGreen_PowerEff;
        else if (gradeType < 6) upgradeType = UpgradeType.RodYellow_PowerEff;
        else if (gradeType < 9) upgradeType = UpgradeType.RodBlue_PowerEff;
        else if (gradeType < 12) upgradeType = UpgradeType.RodPurple_PowerEff;
        else if (gradeType < 15) upgradeType = UpgradeType.RodRed_PowerEff;
        else upgradeType = UpgradeType.RodOrange_PowerEff;

        return string.Format(nonFormattedText, outPower * (1 + PlayerManager.Instance.player.upgrades[upgradeType]), 
                                               outHeat);
    }
}

