using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager instance;
    public static PlayerManager Instance { get => instance; private set => instance = value; }

    [SerializeField]
    private Text moneyText;
    private bool pauseMode;

    public Player player;
    public float Money
    {
        get
        {
            return player.money;
        }
        set
        {
            player.money = value;
            moneyText.text = value.ToString() + " $";
        }
    }

    public bool PauseMode { get => pauseMode; set => pauseMode = value; }


    private void Start()
    {
        if (Instance == null)
            Instance = this;

        NewGame();
    }

    private void NewGame()
    {
        player = new Player
        {
            upgrades = new Dictionary<UpgradeType, int>()
        };
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            player.upgrades.Add(type, 0);
        }
        Money = 10;

        //DEBUG Value
        //Money = 10000;
    }

    internal bool BuyUpgrade(UpgradeType upgradeType)
    {
        float upgradeCost = ItemsManager.Instance.upgradesInfo[upgradeType]
                                .GetCost(player.upgrades[upgradeType]);
        if (Money >= upgradeCost)
        {
            Money -= upgradeCost;
            player.upgrades[upgradeType]++;
            if (upgradeType == UpgradeType.Battery_Durability) ReactorManager.Instance.CalcMaxPower();
            if (upgradeType == UpgradeType.Plate_Durability)   ReactorManager.Instance.CalcMaxHeat();
            return true;
        }
        return false;
    }

    internal void Save()
    {

    }

    internal void Load()
    {

    }
}
