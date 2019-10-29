using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReactorManager : MonoBehaviour
{
    private static ReactorManager instance;
    public static ReactorManager Instance { get => instance; private set => instance = value; }

    [SerializeField] private Slider powerBar;
    [SerializeField] private Slider heatBar;
    [SerializeField] private Text powerText;
    [SerializeField] private Text heatText;

    private float power;
    private float heat;
    private float maxPower = 100;
    private float maxHeat = 100;

    private Cell[,] cellsGrid;
    private Dictionary<ItemType, List<Cell>> itemsDictionary = new Dictionary<ItemType, List<Cell>>()
    {
        [ItemType.Rod] = new List<Cell>(),
        [ItemType.HeatPipe] = new List<Cell>(),
        [ItemType.HeatVent] = new List<Cell>(),
    };
    [SerializeField]
    private GameObject cellPrefab;
    private const float cellOffset = 2.5603f;

    private float updateRate = 0.5f;
    private float nextUpdateTime = 0;

    private GameObject preBuyItem;
    public bool buildMod = false;

    public float Power
    {
        get
        {
            return power;
        }
        private set
        {
            power = value <= maxPower ? value: maxPower;
            powerBar.value = value;
            powerText.text = (int)power + " / " + (int)maxPower;
        }
    }
    public float Heat
    {
        get
        {
            return heat;
        }
        private set
        {
            heat = value <= maxHeat ? value : maxHeat;
            heatBar.value = value;
            heatText.text = (int)heat + " / " + (int)maxHeat;
        }
    }


    private void Start()
    {
        if (Instance == null)
            Instance = this;

        heatBar.maxValue = maxHeat;
        powerBar.maxValue = maxPower;

        CreateGrid(new Vector2(4, 4), new Vector2(-10, -5));
    }

    /* Update tasks
     ================
     = Rods
     = Pipes
     = Vents
     = Check Durability/Heat (for destroy)
     = UI  
     ================
    */
    private void FixedUpdate()
    {
        if (nextUpdateTime > Time.time || PlayerManager.Instance.PauseMode)
            return;

        if(heat == maxHeat)
        {
            for (int i = 0; i < itemsDictionary.Count; i++)
            {
                for (int j = 0; j < itemsDictionary[(ItemType)i].Count; j++)
                {
                    DestroyItem(itemsDictionary[(ItemType)i][j], true);
                }
            }
            return;
        }

        nextUpdateTime = Time.time + updateRate;

        float addMoney = 0;
        float addHeat = 0;
        float addPower = 0;
        Cell selectedCell;
        List<IItem> selectedItems = new List<IItem>();
        List<Cell> destroyList = new List<Cell>();

        //Rods
        foreach (var cell in itemsDictionary[ItemType.Rod])
        {
            selectedItems.Clear();
            if (cell.cellIndex.y > 0) //up
            {
                selectedCell = cellsGrid[(int)cell.cellIndex.x, (int)cell.cellIndex.y - 1];
                if(selectedCell.cellItem != null && selectedCell.cellItem.ItemType != ItemType.Rod)
                {
                    selectedItems.Add(selectedCell.cellItem);
                }
            }
            if(cell.cellIndex.y < cellsGrid.GetLength(1) - 1) //down
            {
                selectedCell = cellsGrid[(int)cell.cellIndex.x, (int)cell.cellIndex.y + 1];
                if (selectedCell.cellItem != null && selectedCell.cellItem.ItemType != ItemType.Rod)
                {
                    selectedItems.Add(selectedCell.cellItem);
                }
            }
            if (cell.cellIndex.x > 0) //left
            {
                selectedCell = cellsGrid[(int)cell.cellIndex.x - 1, (int)cell.cellIndex.y];
                if (selectedCell.cellItem != null && selectedCell.cellItem.ItemType != ItemType.Rod)
                {
                    selectedItems.Add(selectedCell.cellItem);
                }
            }
            if (cell.cellIndex.x < cellsGrid.GetLength(0) - 1) //right
            {
                selectedCell = cellsGrid[(int)cell.cellIndex.x + 1, (int)cell.cellIndex.y];
                if (selectedCell.cellItem != null && selectedCell.cellItem.ItemType != ItemType.Rod)
                {
                    selectedItems.Add(selectedCell.cellItem);
                }
            }

            cell.cellItem.durability--;
            cell.cellItem.UpdateDurabilityBar();
            RodInfo rodInfo = (RodInfo)ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
            if(selectedItems.Count != 0)
            {
                float heatPerCell = rodInfo.outHeat / selectedItems.Count;
                foreach (var item in selectedItems)
                {
                    item.heat += heatPerCell;
                }
            }
            else
            {
                addHeat += rodInfo.outHeat;
            }
            addPower += rodInfo.outEnergy;

            //destroy rod check
            if(cell.cellItem.durability <= 0)
            {
                destroyList.Add(cell);
            }
        }


        //Pipes
        foreach (var cell in itemsDictionary[ItemType.HeatPipe])
        {
            if (cell.cellIndex.y > 0) //up
            {
                selectedCell = cellsGrid[(int)cell.cellIndex.x, (int)cell.cellIndex.y - 1];
                if (selectedCell.cellItem != null && selectedCell.cellItem.ItemType != ItemType.Rod)
                {
                    selectedItems.Add(selectedCell.cellItem);
                }
            }
            if (cell.cellIndex.y < cellsGrid.GetLength(1) - 1) //down
            {
                selectedCell = cellsGrid[(int)cell.cellIndex.x, (int)cell.cellIndex.y + 1];
                if (selectedCell.cellItem != null && selectedCell.cellItem.ItemType != ItemType.Rod)
                {
                    selectedItems.Add(selectedCell.cellItem);
                }
            }
            if (cell.cellIndex.x > 0) //left
            {
                selectedCell = cellsGrid[(int)cell.cellIndex.x - 1, (int)cell.cellIndex.y];
                if (selectedCell.cellItem != null && selectedCell.cellItem.ItemType != ItemType.Rod)
                {
                    selectedItems.Add(selectedCell.cellItem);
                }
            }
            if (cell.cellIndex.x < cellsGrid.GetLength(0) - 1) //right
            {
                selectedCell = cellsGrid[(int)cell.cellIndex.x + 1, (int)cell.cellIndex.y];
                if (selectedCell.cellItem != null && selectedCell.cellItem.ItemType != ItemType.Rod)
                {
                    selectedItems.Add(selectedCell.cellItem);
                }
            }

            HeatPipeInfo pipeInfo = (HeatPipeInfo)ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
            if (selectedItems.Count != 0)
            {
                float heatPerCell = 0;
                if (cell.cellItem.heat > selectedItems.Count * pipeInfo.heatThroughput)
                {
                    heatPerCell = pipeInfo.heatThroughput;
                    cell.cellItem.heat -= selectedItems.Count * pipeInfo.heatThroughput;
                }
                else
                {
                    heatPerCell = cell.cellItem.heat / selectedItems.Count;
                    cell.cellItem.heat = 0;
                }

                foreach (var item in selectedItems)
                {
                    item.heat += heatPerCell;
                }
            }
            cell.cellItem.UpdateHeatBar();

            //destroy pipe check
            if (cell.cellItem.durability <= cell.cellItem.heat)
            {
                destroyList.Add(cell);
            }
        }


        //Vents
        foreach (var cell in itemsDictionary[ItemType.HeatVent])
        {
            HeatVentInfo pipeInfo = (HeatVentInfo)ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
            cell.cellItem.heat -= pipeInfo.decreaseHeat;
            if (cell.cellItem.heat < 0)
                cell.cellItem.heat = 0;
            cell.cellItem.UpdateHeatBar();

            //destroy pipe check
            if (cell.cellItem.durability <= cell.cellItem.heat)
            {
                destroyList.Add(cell);
            }
        }


        //Destroy
        foreach (var cell in destroyList)
        {
            DestroyItem(cell, cell.cellItem.ItemType != ItemType.Rod? true : false);
        }
        

        //UI
        Heat += addHeat;
        Power += addPower;
        PlayerManager.Instance.Money += addMoney;
    }

    private void DestroyItem(Cell cell, bool explosion)
    {
        if (explosion)
        {
            Vector3 position = cell.transform.position;
            position.z = -1;
            ExplosionAnimation explosionAnimation = Instantiate(ItemsManager.Instance.explosionItemPrefab,
                                                                             position,
                                                                             Quaternion.identity, 
                                                                             transform).GetComponent<ExplosionAnimation>();
            explosionAnimation.Play();
        }
        itemsDictionary[cell.cellItem.ItemType].Remove(cell);
        Destroy(cell.cellItem.hpBar.gameObject);
        Destroy(cell.cellItem.gameObject);
        cell.cellItem = null;
    }

    private void CreateGrid(Vector2 size, Vector2 startGridPoint)
    {
        cellsGrid = new Cell[(int)size.x, (int)size.y];
        for (int row = 0; row < size.x; row++)
        {
            for (int column = 0; column < size.y; column++)
            {
                cellsGrid[row, column] = Instantiate(cellPrefab,
                                                 new Vector3(row * cellOffset + startGridPoint.x, column * -cellOffset - startGridPoint.y, 1),
                                                 Quaternion.identity,
                                                 gameObject.transform)
                                                 .GetComponent<Cell>();
                cellsGrid[row, column].cellIndex = new Vector2(row, column);
            }
        }
    }


    public void DecreaseHeatClick()
    {
        if(heat > 0) Heat--;
    }

    public void SellEnergyClick()
    {
        PlayerManager.Instance.Money += power;
        Power = 0;
    }

    public void MovePreBuyItem(Vector2 position, bool isEmptyCell)
    {
        preBuyItem.transform.position = position;
        preBuyItem.GetComponent<SpriteRenderer>().color = isEmptyCell ?
            new Color32(255, 255, 255, 255) : new Color32(255, 131, 131, 131);

    }

    public void SelectPreBuildItem(GameObject prefab)
    {
        if(preBuyItem != null)
        {
            Destroy(preBuyItem);
        }

        preBuyItem = Instantiate(prefab,
                                 Camera.main.ScreenToWorldPoint(Input.mousePosition),
                                 Quaternion.identity,
                                 transform);
        buildMod = true;
    }

    public void SellItem(Cell cell)
    {
        ItemInfo itemInfo = ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
        if (cell.cellItem.ItemType == ItemType.Rod)
        {
            PlayerManager.Instance.Money += itemInfo.cost * (cell.cellItem.durability / itemInfo.durability);
        }
        else
        {
            if (cell.cellItem.heat != 0)
            {
                PlayerManager.Instance.Money += itemInfo.cost - itemInfo.cost * (cell.cellItem.heat / itemInfo.durability);
            }
            else
            {
                PlayerManager.Instance.Money += itemInfo.cost;
            }
        }
        
        DestroyItem(cell, false);
    }

    public void BuyItem(Vector2 cellIndex)
    {
        Cell cell = cellsGrid[(int)cellIndex.x, (int)cellIndex.y];
        if (cell.cellItem == null)
        {
            IItem item = preBuyItem.GetComponent<IItem>();
            ItemInfo itemInfo = ItemsManager.Instance.itemsInfo[item.ItemType][item.itemGradeType];
            if(itemInfo.cost <= PlayerManager.Instance.Money)
            {
                PlayerManager.Instance.Money -= itemInfo.cost;

                item.durability = itemInfo.durability;
                item.hpBar = ItemsManager.Instance.GetSliderObject(cell.transform.position);
                item.hpBar.maxValue = item.durability;
                item.hpBar.value = item.durability;
                cell.cellItem = item;

                itemsDictionary[item.ItemType].Add(cell);

                preBuyItem = null;
                buildMod = false;
            }
        }
    }

}
