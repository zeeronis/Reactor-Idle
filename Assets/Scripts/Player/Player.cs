using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Player
{
    public bool pauseMode;
    public float money;
    public Dictionary<UpgradeType, int> upgrades;
    public List<BlockedItem> blockedItems;
    public Reactor reactor;
}
