using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager instance;
    public static PlayerManager Instance { get => instance; private set => instance = value; }
    public static bool IsReady { get; private set; }

    [SerializeField]
    private Text moneyText;
    [SerializeField]
    private Button PauseButton;
    [SerializeField]
    private Button ResumeButton;

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

    public bool PauseMode
    {
        get
        {
            return player.pauseMode; 
        }
        set
        {
            player.pauseMode = value;
            if(value)
            {
                PauseButton.gameObject.SetActive(false);
                ResumeButton.gameObject.SetActive(true);
            }
            else
            {
                PauseButton.gameObject.SetActive(true);
                ResumeButton.gameObject.SetActive(false);
            }
        }
    }


    private void Start()
    {
        if (Instance == null)
            Instance = this;

        Screen.SetResolution(840, 480, FullScreenMode.Windowed);
    }

    private void FixedUpdate()
    {
        if (IsReady) return;

        if (ItemsManager.IsReady && PoolManager.IsReady && ReactorManager.IsReady)
        {
            NewGame();
            IsReady = true;
        }
    }

    private void NewGame()
    {
        player = new Player
        {
            upgrades = new Dictionary<UpgradeType, int>(),
            reactor = new Reactor() { gradeType = 0 }
        };
        foreach (UpgradeType type in System.Enum.GetValues(typeof(UpgradeType)))
        {
            player.upgrades.Add(type, 0);
        }
        Money = 10;

        ReactorManager.Instance.InitReactor(player.reactor, false);
        
        //DEBUG Value
        //Money = float.MaxValue;

        IsReady = true;
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

    public void Save()
    {
        ReactorManager.Instance.SaveCells();
        BinaryFormatter formatter = new BinaryFormatter();
        Directory.CreateDirectory(Environment.GetFolderPath(
                                  Environment.SpecialFolder.ApplicationData) 
                                  + "/ReactorIdle");
        using (FileStream fileStream = new FileStream(Environment.GetFolderPath(
                                              Environment.SpecialFolder.ApplicationData) 
                                              + "/ReactorIdle/pData.bytes", FileMode.OpenOrCreate))
        {
            formatter.Serialize(fileStream, player);
        }
    }

    public void Load()
    {
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream fileStream = new FileStream(Environment.GetFolderPath(
                                              Environment.SpecialFolder.ApplicationData)
                                              + "/ReactorIdle/pData.bytes", FileMode.OpenOrCreate))
        {
            player = (Player)formatter.Deserialize(fileStream);
        }
        ReactorManager.Instance.InitReactor(player.reactor, true);
        Money = player.money;
    }
}
