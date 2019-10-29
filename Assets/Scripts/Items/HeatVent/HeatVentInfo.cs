using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class HeatVentInfo: ItemInfo
{
    public ItemGradeType heatVentType;
    public float decreaseHeat;

    internal override string GetLocaleDesc(string nonFormattedText)
    {
        return string.Format(nonFormattedText, decreaseHeat);
    }
}

