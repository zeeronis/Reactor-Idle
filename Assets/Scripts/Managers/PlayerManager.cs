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

    private float nextSaveTime = 60f;
    private float autoSaveDelay = 60f;

    private float checkBlockItemsTime = 10f;
    private float checkBlockItemsDelay = 10f;
    private float playerMaxMoney;

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
            if (playerMaxMoney < value) playerMaxMoney = value;
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
            if (value)
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
        if (IsReady)
        {
            if (Time.time > nextSaveTime)
            {
                nextSaveTime = Time.time + autoSaveDelay;
                Save();
            }
            if(Time.time > checkBlockItemsTime)
            {
                nextSaveTime = Time.time + checkBlockItemsDelay;
                ItemsManager.Instance.CheckBlockedItems(true, false);
            }
        }
        else
        {
            if (ItemsManager.IsReady && PoolManager.IsReady && ReactorManager.IsReady)
            {
                nextSaveTime = Time.time + autoSaveDelay;
                if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
                                + "/ReactorIdle/pData.bytes"))
                {
                    Load();
                }
                else
                {
                    NewGame();
                }
                IsReady = true;
            }
        }
    }

    private void NewGame()
    {
        player = new Player
        {
            upgrades = new Dictionary<UpgradeType, int>(),
            reactor = new Reactor() { gradeType = 0 },
            blockedItems = new List<BlockedItem>()
        };
        foreach (UpgradeType upgradeType in Enum.GetValues(typeof(UpgradeType)))
        {
            player.upgrades.Add(upgradeType, 0);
        }
        foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
        {
            for (int itemGrade = 0; itemGrade < ItemsManager.Instance.itemsInfo[itemType].Length; itemGrade++)
            {
                player.blockedItems.Add(new BlockedItem()
                {
                    ItemType = itemType,
                    itemGradeType = itemGrade,
                    openMoneyValue = ItemsManager.Instance.itemsInfo[itemType][itemGrade].cost / 4
                });
            }
        }

        Money = 10;
        ReactorManager.Instance.InitReactor(player.reactor, false);

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
            if (upgradeType == UpgradeType.Plate_Durability) ReactorManager.Instance.CalcMaxHeat();
            return true;
        }
        return false;
    }

    public void AutoSaveValueChanged(int index)
    {
        switch (index)
        {
            case 0:
                autoSaveDelay = 60;
                break;
            case 1:
                autoSaveDelay = 3 * 60;
                break;
            case 2:
                autoSaveDelay = 5 * 60;
                break;
            case 3:
                autoSaveDelay = 10 * 60;
                break;
            case 4:
                autoSaveDelay = 20 * 60;
                break;
            case 5:
                autoSaveDelay = 30 * 60;
                break;

            default:
                autoSaveDelay = 60;
                break;
        }
        nextSaveTime = Time.time + autoSaveDelay;
    }

    public void Save()
    {
        PauseMode = true;
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
        PauseMode = false;
    }

    public void Load()
    {
        PauseMode = true;
        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream fileStream = new FileStream(Environment.GetFolderPath(
                                              Environment.SpecialFolder.ApplicationData)
                                              + "/ReactorIdle/pData.bytes", FileMode.OpenOrCreate))
        {
            player = (Player)formatter.Deserialize(fileStream);
        }
        for (int i = 0; i < player.blockedItems.Count; i++)
        {
            player.blockedItems[i].openMoneyValue = ItemsManager.Instance.itemsInfo[player.blockedItems[i].ItemType][player.blockedItems[i].itemGradeType]
                                                        .cost / 4;
        }

        ReactorManager.Instance.InitReactor(player.reactor, true);
        Money = player.money;
        PauseMode = false;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void ResetGame()
    {
        NewGame();
        Save();
    }

    private void OnApplicationQuit() //PC 
    {
        Save();
    }
}
