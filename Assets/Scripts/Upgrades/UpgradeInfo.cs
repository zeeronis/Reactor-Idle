using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class UpgradeInfo
{
    public float costBase;
    public float costMultipler;
    public string keyName;
    public string keyDesc;

    public float GetCost(int lvl)
    {
        return costBase * (float)(Math.Pow(costMultipler, lvl) * ReactorManager.Factorial(lvl));
    }
}
