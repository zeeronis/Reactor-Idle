using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Player
{
    public float money;
    public Dictionary<UpgradeType, int> upgrades;
    //public Reactor reactor;
}
