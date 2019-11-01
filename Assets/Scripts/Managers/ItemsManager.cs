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
    public Dictionary<ItemType, ItemInfo[]> itemsInfo = new Dictionary<ItemType, ItemInfo[]>();

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