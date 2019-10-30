using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class HeatPipeInfo: ItemInfo
{
    public float heatThroughput;

    internal override string GetLocaleDesc(string nonFormattedText)
    {
        return string.Format(nonFormattedText, heatThroughput);
    }
}
