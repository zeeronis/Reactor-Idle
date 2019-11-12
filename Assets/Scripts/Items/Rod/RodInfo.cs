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

    internal static float GetRodEffMultipler(int gradeType)
    {
        if (gradeType < 3)
            return 1 + PlayerManager.Instance.player.upgrades[UpgradeType.RodGreen_PowerEff];
        if (gradeType < 6)
            return 1 + PlayerManager.Instance.player.upgrades[UpgradeType.RodYellow_PowerEff];
        if (gradeType < 9)
            return 1 + PlayerManager.Instance.player.upgrades[UpgradeType.RodBlue_PowerEff];
        if (gradeType < 12)
            return 1 + PlayerManager.Instance.player.upgrades[UpgradeType.RodPurple_PowerEff];
        if (gradeType < 15)
            return 1 + PlayerManager.Instance.player.upgrades[UpgradeType.RodRed_PowerEff];
        if (gradeType < 18)
            return 1 + PlayerManager.Instance.player.upgrades[UpgradeType.RodOrange_PowerEff];
        return 1;
    }
    internal static bool CanRodAutoReplace(int gradeType)
    {
        if (gradeType < 3)
            return PlayerManager.Instance.player.upgrades[UpgradeType.RodGreen_AutoReplace] == 1 ? true : false;
        if (gradeType < 6)
            return PlayerManager.Instance.player.upgrades[UpgradeType.RodYellow_PowerEff] == 1 ? true : false;
        if (gradeType < 9)
            return PlayerManager.Instance.player.upgrades[UpgradeType.RodBlue_PowerEff] == 1 ? true : false;
        if (gradeType < 12)
            return PlayerManager.Instance.player.upgrades[UpgradeType.RodPurple_PowerEff] == 1 ? true : false;
        if (gradeType < 15)
            return PlayerManager.Instance.player.upgrades[UpgradeType.RodRed_PowerEff] == 1 ? true : false;
        if (gradeType < 18)
            return PlayerManager.Instance.player.upgrades[UpgradeType.RodOrange_PowerEff] == 1 ? true : false;
        return false;
    }
}

