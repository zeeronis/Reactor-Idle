using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class ItemInfo
{
    [NonSerialized]
    public GameObject prefab;
    public float durability;
    public float cost;
    public string keyName;
    public string keyDesc;

    internal virtual string GetLocaleDesc(string nonFormattedText)
    {
        return string.Format(nonFormattedText, durability);
    }
}
