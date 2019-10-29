using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class RodInfo: ItemInfo
{
    public RodType rodType;
    public float outEnergy;
    public float outHeat;

    internal override string GetLocaleDesc(string nonFormattedText)
    {
        return string.Format(nonFormattedText, outEnergy, outHeat);
    }
}

