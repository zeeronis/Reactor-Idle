using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UI.Dropdown;

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
    [SerializeField]
    private Dropdown langsDropDown;
    [SerializeField]
    private InputField LoadInputField;
    [SerializeField]
    private InputField SaveInputField;
    public RectTransform UICanvasRect;
    #pragma warning restore CS0649

    private string savePath;
    private const string SAVE_FILE_NAME = "/pData.bytes";
    private const string AUTO_SAVE_FILE_NAME = "/apData.bytes";
    private float nextSaveTime = 60f;

    private float checkBlockItemsTime = 2f;
    private float checkBlockItemsDelay = 2f;

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
            moneyText.text = Formatter.BigNumbersFormat(value);
            if (player.maxMoney < value) player.maxMoney = value;
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

        foreach (var item in LocalizeText.supportedLangs)
        {
            langsDropDown.options.Add(new OptionData(item.ToString()));
        }

        #if UNITY_ANDROID
        savePath = Application.persistentDataPath;
        #endif

        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        savePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/ReactorIdle";
        Directory.CreateDirectory(Environment.GetFolderPath(
                          Environment.SpecialFolder.ApplicationData)
                          + "/ReactorIdle");
        #endif
    }

    private void FixedUpdate()
    {
        if (IsReady)
        {
            if (Time.time > nextSaveTime)
            {
                nextSaveTime = Time.time + player.autoSaveDelay;
                Save(true);
            }
            if(Time.time > checkBlockItemsTime)
            {
                checkBlockItemsTime = Time.time + checkBlockItemsDelay;
                ItemsManager.Instance.CheckBlockedItems(true, false);
            }
        }
        else
        {
            if (ItemsManager.IsReady && PoolManager.IsReady && ReactorManager.IsReady)
            {
                nextSaveTime = Time.time + player.autoSaveDelay;
                if (File.Exists(savePath + SAVE_FILE_NAME) || File.Exists(savePath + AUTO_SAVE_FILE_NAME))
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
            language = SystemLanguage.English,
            upgrades = new Dictionary<UpgradeType, int>(),
            reactor = new Reactor() { gradeType = 0 },
            autoSaveDelay = 60
        };
        foreach (UpgradeType upgradeType in Enum.GetValues(typeof(UpgradeType)))
        {
            player.upgrades.Add(upgradeType, 0);
        }

        Money = 10;
        ReactorManager.Instance.InitReactor(player.reactor, false);
        LocalizeText.SetCurrentLocalization(player.language);
        UpdateValueForLangsDropDown();

        nextSaveTime = Time.time + player.autoSaveDelay;
        AutoReplaceMode = false;
        PauseMode = false;
        IsReady = true;
    }

    #if UNITY_ANDROID
    private void OnApplicationFocus(bool focus)
    {
        if (!focus && !isButtonExit) Save(false);
    }
    #endif

    #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
    private void OnApplicationQuit()
    {
        Save(false);
    }
    #endif

    private void UpdateValueForLangsDropDown()
    {
        for (int i = 0; i < LocalizeText.supportedLangs.Length; i++)
        {
            if (LocalizeText.supportedLangs[i] == player.language)
            {
                langsDropDown.value = i;
                return;
            }
        }
    }

    private void AfterLoadInits()
    {
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
        LocalizeText.SetCurrentLocalization(player.language);
        UpdateValueForLangsDropDown();
    }

    private string GetBase64SaveString()
    {
        PauseMode = true;
        string saveString = "";
        ReactorManager.Instance.SaveCells();

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        using (MemoryStream memoryStream = new MemoryStream())
        {
            binaryFormatter.Serialize(memoryStream, player);
            saveString = Convert.ToBase64String(memoryStream.GetBuffer());
        }

        PauseMode = false;
        return saveString;
    }

    private bool LoadFromBase64String(string base64String)
    {
        PauseMode = true;
        bool successLoad = true;
        MemoryStream memoryStream = null;
        try
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            memoryStream = new MemoryStream(Convert.FromBase64String(base64String));
            player = (Player)binaryFormatter.Deserialize(memoryStream);
            memoryStream.Dispose();

            AfterLoadInits();
        }
        catch (Exception)
        {
            successLoad = false;
        }
        finally
        {
            if(memoryStream != null)
            {
                memoryStream.Dispose();
            }
        }
        
        PauseMode = false;
        return successLoad;
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
            ReactorManager.Instance.CheckPlayerBankruptcy();
            return true;
        }
        return false;
    }

    internal void ChangeAutoReplaceRodsMode()
    {
        AutoReplaceMode = !player.autoReplaceMode;
    }

    internal void ChangePauseMode()
    {
        PauseMode = !player.pauseMode;
    }

    internal void ChangeLanguage(int index)
    {
        player.language = LocalizeText.supportedLangs[index];
        LocalizeText.SetCurrentLocalization(player.language);
    }

    internal void AutoSaveValueChanged(int index)
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

    internal void Save(bool isAutoSave)
    {
        PauseMode = true;
        ReactorManager.Instance.SaveCells();
        BinaryFormatter formatter = new BinaryFormatter();

        using (FileStream fileStream = new FileStream(
                                    savePath + (isAutoSave ? AUTO_SAVE_FILE_NAME : SAVE_FILE_NAME),
                                    FileMode.OpenOrCreate))
        {
            formatter.Serialize(fileStream, player);
        }
        PauseMode = false;
    }

    internal void Load()
    {
        PauseMode = true;
        bool errLoad = false;
        string saveFilePath = "";
        if(File.GetLastWriteTime(savePath + SAVE_FILE_NAME) > File.GetLastWriteTime(savePath + AUTO_SAVE_FILE_NAME))
        {
            saveFilePath = savePath + SAVE_FILE_NAME;
        }
        else
        {
            saveFilePath = savePath + AUTO_SAVE_FILE_NAME;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        using (FileStream fileStream = new FileStream(saveFilePath, FileMode.OpenOrCreate))
        {
            try
            {
                player = (Player)formatter.Deserialize(fileStream);
            }
            catch (SerializationException)
            {
                errLoad = true;
            }
        }
        if (errLoad)
        {
            File.Delete(saveFilePath);
            Load();
            return;
        }

        AfterLoadInits();
        PauseMode = false;
    }

    internal void LoadGameFromString_Click()
    {
        if(LoadInputField.text != String.Empty)
        {
            if (LoadFromBase64String(LoadInputField.text))
            {
                LoadInputField.text = String.Empty;
            }
            else
            {
                LoadInputField.text = "Incorrect load string";
            }
            SaveInputField.text = String.Empty;
        }
    }

    internal void PasteSaveStrFromClipboard()
    {
        LoadInputField.text = GUIUtility.systemCopyBuffer;
    }

    internal void SetSavedDataString_Click()
    {
        SaveInputField.text = GetBase64SaveString();
        GUIUtility.systemCopyBuffer = SaveInputField.text;
    }

    private bool isButtonExit = false;
    internal void ExitGame()
    {
        Save(false);
        Application.Quit();
    }

    internal void ResetGame()
    {
        NewGame();
    }
}
