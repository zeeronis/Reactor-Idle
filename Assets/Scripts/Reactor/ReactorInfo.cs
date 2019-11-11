using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class ReactorInfo
{
    public float baseMaxPower;
    public float baseMaxHeat;
    public int[] gridSize;
    public float[] drawStartposition;

    public float cost;
}
