using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ItemsManager: MonoBehaviour
{
    private static ItemsManager instance;
    public static ItemsManager Instance { get => instance; private set => instance = value; }

    [SerializeField]
    private Transform worldCanvasTransform;
    [SerializeField]
    public Transform UICanvasTransform;

    [SerializeField]
    private Slider itemHpSliderPrefab;
    [SerializeField]
    private GameObject itemInfoPanelPrefab;

    [SerializeField]
    private GameObject[] ventPrefabs;
    [SerializeField]
    private GameObject[] pipePrefabs;
    [SerializeField]
    private GameObject[] rodPrefabs;


    public GameObject explosionItemPrefab;
    [HideInInspector]
    public ItemInfoPanel itemInfoPanel;
    [HideInInspector]
    public Dictionary<ItemType, ItemInfo[]> itemsInfo = new Dictionary<ItemType, ItemInfo[]>()
    {
        [ItemType.Rod] = new RodInfo[] 
        {
            new RodInfo() {
                rodType = RodType.GreenX1,
                durability = 15,
                outEnergy = 1,
                outHeat = 1,
                cost = 10,
                keyName = "Item.RodX1.name",
                keyDesc = "Item.Rod.desc"
            },
            new RodInfo() {
                rodType = RodType.GreenX2,
                durability = 15,
                outEnergy = 4,
                outHeat = 8,
                cost = 25,
                keyName = "Item.RodX2.name",
                keyDesc = "Item.Rod.desc"
            },
            new RodInfo() {
                rodType = RodType.GreenX4,
                durability = 15,
                outEnergy = 16,
                outHeat = 36,
                cost = 60,
                keyName = "Item.RodX4.name",
                keyDesc = "Item.Rod.desc"
            },
        },
        [ItemType.HeatVent] = new HeatVentInfo[]
        {
            new HeatVentInfo() {
                heatVentType = ItemGradeType.MK1,
                durability = 40,
                decreaseHeat = 4,
                cost = 50,
                keyName = "Item.Vent_1.name",
                keyDesc = "Item.Vent.desc"
            }
        },
        [ItemType.HeatPipe] = new HeatPipeInfo[]
        {
            new HeatPipeInfo() {
                heatPipeType = ItemGradeType.MK1,
                durability = 75,
                heatThroughput = 12,
                cost = 160,
                keyName = "Item.Pipe_1.name",
                keyDesc = "Item.Pipe.desc"
            },
        },
    };

    private void Start()
    {
        if (Instance == null)
            Instance = this;

        for (int i = 0; i < itemsInfo[ItemType.Rod].Length; i++)
        {
            itemsInfo[ItemType.Rod][i].prefab = rodPrefabs[i];
        }
        for (int i = 0; i < itemsInfo[ItemType.HeatPipe].Length; i++)
        {
            itemsInfo[ItemType.HeatPipe][i].prefab = pipePrefabs[i];
        }
        for (int i = 0; i < itemsInfo[ItemType.HeatVent].Length; i++)
        {
            itemsInfo[ItemType.HeatVent][i].prefab = ventPrefabs[i];
        }

        itemInfoPanel = Instantiate(itemInfoPanelPrefab, Vector3.zero, Quaternion.identity, UICanvasTransform).GetComponent<ItemInfoPanel>();
        itemInfoPanel.gameObject.SetActive(false);

        rodPrefabs = null;
        pipePrefabs = null;
        ventPrefabs = null;
    }

    public Slider GetSliderObject(Vector2 position)
    {
        return Instantiate(itemHpSliderPrefab, 
                           position - new Vector2(0, 0.9f),
                           Quaternion.identity,
                           worldCanvasTransform);
    }
}