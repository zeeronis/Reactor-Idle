﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ItemsManager: MonoBehaviour
{
    private static ItemsManager instance;
    public static ItemsManager Instance { get => instance; private set => instance = value; }
    public static bool IsReady { get; private set; }

    #pragma warning disable CS0649
    [SerializeField]
    private Transform worldCanvasTransform;
    [SerializeField]
    public Transform UICanvasTransform;

    [SerializeField]
    private Slider itemHpSliderPrefab;
    [SerializeField]
    private GameObject itemInfoPanelPrefab;
    [SerializeField]
    private GameObject shopReactorItemPrefab;
    [SerializeField]
    private Sprite blockItemSprite;

    [SerializeField]
    private GameObject[] ventPrefabs;
    [SerializeField]
    private GameObject[] pipePrefabs;
    [SerializeField]
    private GameObject[] heatPlatePrefabs;
    [SerializeField]
    private GameObject[] BatteryPrefabs;
    [SerializeField]
    private GameObject[] rodPrefabs;
    [SerializeField]
    private GameObject shopPanel;
    [SerializeField]
    private GameObject upgradesPanel;
    [SerializeField]
    private GameObject shopReactorsContent;
    #pragma warning restore CS0649

    public GameObject explosionItemPrefab;
    [HideInInspector]
    public ItemInfoPanel itemInfoPanel;
    [HideInInspector]
    public Dictionary<ItemType, ItemInfo[]> itemsInfo = new Dictionary<ItemType, ItemInfo[]>();
    [HideInInspector]
    public Dictionary<UpgradeType, UpgradeInfo> upgradesInfo = new Dictionary<UpgradeType, UpgradeInfo>();
    [HideInInspector]
    public ReactorInfo[] reactorsInfo;

    private void Start()
    {
        if (Instance == null)
            Instance = this;

        //Generate.Run(true); 

        float openClosedItemMultipler = 2.5f;

        //LOAD ITEMS
        TextAsset asset = Resources.Load("Items") as TextAsset;
        BinaryFormatter formatter = new BinaryFormatter();

        using (Stream stream = new MemoryStream(asset.bytes))
        {
            itemsInfo = (Dictionary<ItemType, ItemInfo[]>)formatter.Deserialize(stream);
        }

        for (int i = 0; i < itemsInfo[ItemType.Rod].Length; i++)
        {
            itemsInfo[ItemType.Rod][i].prefab = rodPrefabs[i];
        }
        for (int i = 0; i < 5; i++)
        {
            itemsInfo[ItemType.HeatPipe][i].prefab = pipePrefabs[i];
            itemsInfo[ItemType.HeatVent][i].prefab = ventPrefabs[i];
            itemsInfo[ItemType.Battery][i].prefab = BatteryPrefabs[i];
            itemsInfo[ItemType.HeatPlate][i].prefab = heatPlatePrefabs[i];
        }
        ShopItem[] shopitems = shopPanel.GetComponentsInChildren<ShopItem>(true);
        for (int i = 0; i < shopitems.Length; i++)
        {
            itemsInfo[shopitems[i].itemType][shopitems[i].itemGradeType].shopItem = shopitems[i];
            itemsInfo[shopitems[i].itemType][shopitems[i].itemGradeType].openCost = 
                itemsInfo[shopitems[i].itemType][shopitems[i].itemGradeType].cost / openClosedItemMultipler;
        }

        //LOAD UPGRADES
        asset = Resources.Load("Upgrades") as TextAsset;
        using (Stream stream = new MemoryStream(asset.bytes))
        {
            upgradesInfo = (Dictionary<UpgradeType, UpgradeInfo>)formatter.Deserialize(stream);
        }
        ShopUpgradeItem[] upgradeItems = upgradesPanel.GetComponentsInChildren<ShopUpgradeItem>(true);
        for (int i = 0; i < upgradeItems.Length; i++)
        {
            upgradesInfo[upgradeItems[i].UpgradeType].shopUpgrade = upgradeItems[i];
            upgradesInfo[upgradeItems[i].UpgradeType].defaultSprite = upgradeItems[i].GetComponentsInChildren<Image>(true)[1].sprite;
            upgradesInfo[upgradeItems[i].UpgradeType].openCost = 
                upgradesInfo[upgradeItems[i].UpgradeType].costBase / openClosedItemMultipler;
        }

        //LOAD REACTORS
        asset = Resources.Load("Reactors") as TextAsset;
        using (Stream stream = new MemoryStream(asset.bytes))
        {
            reactorsInfo = (ReactorInfo[])formatter.Deserialize(stream);
        }
        for (int i = 0; i < reactorsInfo.Length; i++)
        {
            Instantiate(shopReactorItemPrefab, Vector3.zero,
                        Quaternion.identity, shopReactorsContent.transform)
                .GetComponent<ShopReactorItem>().SetInfo(i);
        }

        itemInfoPanel = Instantiate(itemInfoPanelPrefab, Vector3.zero, Quaternion.identity, 
                                    UICanvasTransform).GetComponent<ItemInfoPanel>();
        itemInfoPanel.gameObject.SetActive(false);

        heatPlatePrefabs = null;
        BatteryPrefabs = null;
        rodPrefabs = null;
        pipePrefabs = null;
        ventPrefabs = null;

        IsReady = true;
    }

    internal void CheckBlockedItems(bool isOpenCheck, bool isCloseCheck)
    {
        var playerMaxMoney = PlayerManager.Instance.player.maxMoney;
        foreach (var item in itemsInfo)
        {
            for (int i = 0; i < item.Value.Length; i++)
            {
                if (item.Value[i].openCost < playerMaxMoney)
                {
                    if (isOpenCheck)
                    {
                        item.Value[i].shopItem.isOpenItem = true;
                        item.Value[i].shopItem.gameObject.GetComponent<Image>().sprite = item.Value[i]
                            .prefab.gameObject.GetComponent<SpriteRenderer>().sprite;
                    }
                }
                else
                {
                    if (isCloseCheck)
                    {
                        item.Value[i].shopItem.gameObject.GetComponent<Image>().sprite = blockItemSprite;
                        item.Value[i].shopItem.isOpenItem = false;
                    }
                }
            }
        }

        foreach (var item in upgradesInfo)
        {
            if(item.Value.openCost < playerMaxMoney)
            {
                if (isOpenCheck && item.Value.shopUpgrade != null)
                {
                    item.Value.shopUpgrade.GetComponentsInChildren<Image>(true)[1].sprite = item.Value.defaultSprite;
                    item.Value.shopUpgrade.isOpenUpgrade = true;
                }
            }
            else
            {
                if (isCloseCheck && item.Value.shopUpgrade != null)
                {
                    item.Value.shopUpgrade.GetComponentsInChildren<Image>(true)[1].sprite = blockItemSprite;
                    item.Value.shopUpgrade.isOpenUpgrade = false;
                }
            }
        }
    }

    internal Slider GetSliderObject(Vector2 position)
    {
        return Instantiate(itemHpSliderPrefab, 
                           position - new Vector2(0, 0.9f),
                           Quaternion.identity,
                           worldCanvasTransform);
    }
}