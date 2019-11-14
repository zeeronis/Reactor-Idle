using System;
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
    private Sprite blockItemSprite;
    [SerializeField]
    private GameObject[] shopTabs;
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
        for (int i = 0; i < shopTabs.Length; i++)
        {
            ShopItem[] shopitems = shopTabs[i].GetComponentsInChildren<ShopItem>();
            for (int j = 0; j < shopitems.Length; j++)
            {
                itemsInfo[shopitems[j].itemType][shopitems[j].itemGradeType].shopItem = shopitems[j];
            }
        }

        asset = Resources.Load("Upgrades") as TextAsset;
        using (Stream stream = new MemoryStream(asset.bytes))
        {
            upgradesInfo = (Dictionary<UpgradeType, UpgradeInfo>)formatter.Deserialize(stream);
        }

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

        itemInfoPanel = Instantiate(itemInfoPanelPrefab, Vector3.zero, Quaternion.identity, UICanvasTransform).GetComponent<ItemInfoPanel>();
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
        var playerMoney = PlayerManager.Instance.player.money;
        var blockedItems = PlayerManager.Instance.player.blockedItems;
        for (int i = 0; i < blockedItems.Count; i++)
        {
            ItemInfo info = itemsInfo[blockedItems[i].ItemType][blockedItems[i].itemGradeType];
            if (blockedItems[i].openMoneyValue < playerMoney)
            {
                if (isOpenCheck)
                {
                    info.shopItem.isOpenItem = true;
                    info.shopItem.gameObject.GetComponent<Image>().sprite = info.prefab.gameObject.GetComponent<SpriteRenderer>().sprite;
                    blockedItems.Remove(blockedItems[i]);
                    i--;
                }
            }
            else
            {
                if (isCloseCheck)
                {
                    info.shopItem.gameObject.GetComponent<Image>().sprite = blockItemSprite;
                    info.shopItem.isOpenItem = false;
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