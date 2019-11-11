using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class BlockedItem
{
    public ItemType ItemType;
    public int itemGradeType;

    [NonSerialized]
    public float openMoneyValue;
}

