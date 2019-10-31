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
    private GameObject[] heatPlatePrefabs;
    [SerializeField]
    private GameObject[] BatteryPrefabs;
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
                durability = 15,
                outPower = 1,
                outHeat = 1,
                cost = 10,
                keyName = "Item.0_RodX1.name",
                keyDesc = "Item.Rod.desc"
            },
            new RodInfo() {
                durability = 15,
                outPower = 4,
                outHeat = 8,
                cost = 25,
                keyName = "Item.0_RodX2.name",
                keyDesc = "Item.Rod.desc"
            },
            new RodInfo() {
                durability = 15,
                outPower = 16,
                outHeat = 36,
                cost = 60,
                keyName = "Item.0_RodX4.name",
                keyDesc = "Item.Rod.desc"
            },
        },
        [ItemType.HeatVent] = new HeatVentInfo[]
        {
            new HeatVentInfo() {
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
                durability = 320,
                heatThroughput = 16,
                cost = 160,
                keyName = "Item.Pipe_1.name",
                keyDesc = "Item.Pipe.desc"
            },
        },
        [ItemType.HeatPlate] = new ItemInfo[]
        {
            new ItemInfo() {
                durability = 100,
                cost = 1000,
                keyName = "Item.Plate_1.name",
                keyDesc = "Item.Plate.desc"
            },
        },
        [ItemType.Battery] = new ItemInfo[]
        {
            new ItemInfo() {
                durability = 100,
                cost = 1000,
                keyName = "Item.Battery_1.name",
                keyDesc = "Item.Battery.desc"
            },
        },
    };

    private void Start()
    {
        if (Instance == null)
            Instance = this;

        Generate.Run();
        //return;


        TextAsset asset = Resources.Load("Items") as TextAsset;
        BinaryFormatter formatter = new BinaryFormatter();

        using (Stream stream = new MemoryStream(asset.bytes))
        {
            //BinaryReader binaryReader = new BinaryReader(stream);
       
            itemsInfo = (Dictionary<ItemType, ItemInfo[]>)formatter.Deserialize(stream);
        }
          
        /*
        
        using (FileStream fs = new FileStream("test.dat", FileMode.OpenOrCreate))
        {
            
        }*/

        //test load variant, change to one "for" when stats doe items will be completed
        /*for (int i = 0; i < itemsInfo[ItemType.HeatPipe].Length; i++)
        {
            itemsInfo[ItemType.HeatPipe][i].prefab = pipePrefabs[i];
        }*/
        for (int i = 0; i < itemsInfo[ItemType.HeatVent].Length; i++)
        {
            itemsInfo[ItemType.HeatVent][i].prefab = ventPrefabs[i];
        }
       /* for (int i = 0; i < itemsInfo[ItemType.Battery].Length; i++)
        {
            itemsInfo[ItemType.Battery][i].prefab = BatteryPrefabs[i];
        }
        for (int i = 0; i < itemsInfo[ItemType.HeatPlate].Length; i++)
        {
            itemsInfo[ItemType.HeatPlate][i].prefab = heatPlatePrefabs[i];
        }*/

        for (int i = 0; i < itemsInfo[ItemType.Rod].Length; i++)
        {
            itemsInfo[ItemType.Rod][i].prefab = rodPrefabs[i];
        }
        /*
        for (int i = 0; i < 5; i++)
        {
            itemsInfo[ItemType.HeatPipe][i].prefab = pipePrefabs[i];
            itemsInfo[ItemType.HeatVent][i].prefab = ventPrefabs[i];
            itemsInfo[ItemType.Battery][i].prefab = BatteryPrefabs[i];
            itemsInfo[ItemType.HeatPlate][i].prefab = heatPlatePrefabs[i];
        }*/

        itemInfoPanel = Instantiate(itemInfoPanelPrefab, Vector3.zero, Quaternion.identity, UICanvasTransform).GetComponent<ItemInfoPanel>();
        itemInfoPanel.gameObject.SetActive(false);

        heatPlatePrefabs = null;
        BatteryPrefabs = null;
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