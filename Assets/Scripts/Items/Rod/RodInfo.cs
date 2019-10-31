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
        return string.Format(nonFormattedText, outPower, outHeat);
    }
}

