using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class Formatter
{
    public static string BigNumbersFormat(float value)
    {
        if (value < 1000)
            return value.ToString();
        if (value >= 1000000000000000000000000000000000000000d)
            return value / 1000000000000000000000000000000000000000d + "ai";
        else if (value >= 1000000000000000000000000000000000000f)
            return value / 1000000000000000000000000000000000000f + "ah";
        else if (value >= 1000000000000000000000000000000000f)
            return value / 1000000000000000000000000000000000f + "ag";
        else if (value >= 1000000000000000000000000000000f)
            return value / 1000000000000000000000000000000f + "af";
        else if (value >= 1000000000000000000000000000f)
            return value / 1000000000000000000000000000f + "ae";
        else if (value >= 1000000000000000000000000f)
            return value / 1000000000000000000000000f + "ad";
        else if (value >= 1000000000000000000000f)
            return value / 1000000000000000000000f + "ac";
        else if (value >= 1000000000000000000)
            return value / 1000000000000000000 + "ab";
        else if (value >= 1000000000000000)
            return value / 1000000000000000 + "aa";
        else if (value >= 1000000000000)
            return value / 1000000000000 + "t";
        else if (value >= 1000000000)
            return value / 1000000000 + "b";
        else if (value >= 1000000)
            return value / 1000000 + "m";
        else if (value >= 1000)
            return value / 1000 + "k";
        return "Err";
    }
}

