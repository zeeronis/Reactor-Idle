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

    #pragma warning disable CS0649
    [SerializeField]
    private Text moneyText;
    [SerializeField]
    private Button pauseResumeButton;
    [SerializeField]
    private Sprite[] pauseResumeSprites;
    [SerializeField]
    private Button autoReplaceButton;
    [SerializeField]
    private Sprite[] autoReplaceSprites;
    #pragma warning restore CS0649

    private float nextSaveTime = 60f;

    private float checkBlockItemsTime = 2f;
    private float checkBlockItemsDelay = 2f;
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
                pauseResumeButton.GetComponent<Image>().sprite = pauseResumeSprites[0];
            }
            else
            {
                pauseResumeButton.GetComponent<Image>().sprite = pauseResumeSprites[1];
            }
        }
    }
    public bool AutoReplaceMode
    {
        get
        {
            return player.autoReplaceMode;
        }
        set
        {
            player.autoReplaceMode = value;
            if (value)
            {
                autoReplaceButton.GetComponent<Image>().sprite = autoReplaceSprites[0];
            }
            else
            {
                autoReplaceButton.GetComponent<Image>().sprite = autoReplaceSprites[1];
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
                Debug.Log("Save");
                nextSaveTime = Time.time + player.autoSaveDelay;
                Save();
            }
            if(Time.time > checkBlockItemsTime)
            {
                Debug.Log("Check");
                checkBlockItemsTime = Time.time + checkBlockItemsDelay;
                ItemsManager.Instance.CheckBlockedItems(true, false);
            }
        }
        else
        {
            if (ItemsManager.IsReady && PoolManager.IsReady && ReactorManager.IsReady)
            {
                nextSaveTime = Time.time + player.autoSaveDelay;
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
            autoSaveDelay = 60
        };
        foreach (UpgradeType upgradeType in Enum.GetValues(typeof(UpgradeType)))
        {
            player.upgrades.Add(upgradeType, 0);
        }

        AutoReplaceMode = false;
        PauseMode = false;
        Money = 10;
        ReactorManager.Instance.InitReactor(player.reactor, false);

        IsReady = true;
    }

    internal bool BuyUpgrade(UpgradeType upgradeType)
    {
        if (player.upgrades[upgradeType] == ItemsManager.Instance.upgradesInfo[upgradeType].maxUpgradeLvl)
            return false;

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

    public void ChangeAutoReplaceRodsMode()
    {
        AutoReplaceMode = !player.autoReplaceMode;
    }

    public void ChangePauseMode()
    {
        PauseMode = !player.pauseMode;
    }

    public void AutoSaveValueChanged(int index)
    {
        switch (index)
        {
            case 0:
                player.autoSaveDelay = 60;
                break;
            case 1:
                player.autoSaveDelay = 3 * 60;
                break;
            case 2:
                player.autoSaveDelay = 5 * 60;
                break;
            case 3:
                player.autoSaveDelay = 10 * 60;
                break;
            case 4:
                player.autoSaveDelay = 20 * 60;
                break;
            case 5:
                player.autoSaveDelay = 30 * 60;
                break;

            default:
                player.autoSaveDelay = 60;
                break;
        }
        nextSaveTime = Time.time + player.autoSaveDelay;
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
        foreach (UpgradeType upgradeType in Enum.GetValues(typeof(UpgradeType)))
        {
            if (!player.upgrades.ContainsKey(upgradeType))
                player.upgrades.Add(upgradeType, 0);
        }

        ReactorManager.Instance.InitReactor(player.reactor, true);
        Money = player.money;
        PauseMode = player.pauseMode;
        AutoReplaceMode = player.autoReplaceMode;
        nextSaveTime = Time.time + player.autoSaveDelay;
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
