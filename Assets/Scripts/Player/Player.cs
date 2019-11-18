using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Player
{
    public SystemLanguage language;
    public int autoSaveDelay;
    public bool pauseMode;
    public bool autoReplaceMode;
    public float money;
    public float maxMoney;
    public Dictionary<UpgradeType, int> upgrades;
    public Reactor reactor;
}
