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
    private float maxPower;
    private float maxHeat;
    private bool autoReplaceMode;

    private Cell[,] cellsGrid;
    private Dictionary<ItemType, List<Cell>> itemsDictionary = new Dictionary<ItemType, List<Cell>>()
    {
        [ItemType.Rod] = new List<Cell>(),
        [ItemType.HeatPipe] = new List<Cell>(),
        [ItemType.HeatVent] = new List<Cell>(),
        [ItemType.HeatPlate] = new List<Cell>(),
        [ItemType.Battery] = new List<Cell>(),
    };
    private List<Cell> usedRodsList = new List<Cell>();
    [SerializeField]
    private GameObject cellPrefab;
    private const float cellOffset = 2.5603f;

    private float updateRate = 0.5f;
    private float nextUpdateTime = 0;

    [SerializeField]
    private Transform preBuyItemSelected;
    private int selectedItemTab = -1;
    private int currentTab = 0;
    private GameObject preBuyItemPrefab;
    public bool buildMod = false;

    public float Power
    {
        get
        {
            return power;
        }
        private set
        {
            power = value <= MaxPower ? value: MaxPower;
            powerBar.value = value;
            powerText.text = (int)power + " / " + (int)MaxPower;
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
            heat = value <= MaxHeat ? value : MaxHeat;
            heatBar.value = value;
            heatText.text = (int)heat + " / " + (int)MaxHeat;
        }
    }
    public float MaxPower
    {
        get { return maxPower; }
        set
        {
            maxPower = value;
            powerBar.maxValue = maxPower;
        }
    }
    public float MaxHeat
    {
        get { return maxHeat; }
        set
        {
            maxHeat = value;
            heatBar.maxValue = maxHeat;
        }
    }

    private void Start()
    {
        if (Instance == null)
            Instance = this;

        MaxHeat = 100;
        MaxPower = 100;

        CreateGrid(new Vector2(4, 4), new Vector2(-10, -5));
    }

    private static float Recfact(float start, float n)
    {
        float i;
        if (n <= 16)
        {
            float r = start;
            for (i = start + 1; i < start + n; i++) r *= i;
            return r;
        }
        i = n / 2;
        return Recfact(start, i) * Recfact(start + i, n - i);
    }
    public static float Factorial(float n) { return Recfact(1, n); }

    private List<IItem> _selectedItems = new List<IItem>();
    private List<Cell> _destroyList = new List<Cell>();
    private Cell _selectedCell;

    private void FixedUpdate()
    {
        if (nextUpdateTime > Time.time || PlayerManager.Instance.PauseMode)
            return;

        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        _destroyList.Clear();
        if (heat == MaxHeat)
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
        float rodMultipler;
        float upgradeEffMultipler = 1;

        //Rods
        foreach (var cell in itemsDictionary[ItemType.Rod])
        {
            rodMultipler = 1;
            upgradeEffMultipler = GetRodEffMultipler(cell.cellItem.itemGradeType);
            _selectedItems.Clear();
            if (cell.cellIndex.y > 0) //up
            {
                _selectedCell = cellsGrid[(int)cell.cellIndex.x, (int)cell.cellIndex.y - 1];
                if (_selectedCell.cellItem?.ItemType == ItemType.Rod) rodMultipler += 1f;
                CheckSelectedCell();
            }
            if(cell.cellIndex.y < cellsGrid.GetLength(1) - 1) //down
            {
                _selectedCell = cellsGrid[(int)cell.cellIndex.x, (int)cell.cellIndex.y + 1];
                if (_selectedCell.cellItem?.ItemType == ItemType.Rod) rodMultipler += 1f;
                CheckSelectedCell();
            }
            if (cell.cellIndex.x > 0) //left
            {
                _selectedCell = cellsGrid[(int)cell.cellIndex.x - 1, (int)cell.cellIndex.y];
                if (_selectedCell.cellItem?.ItemType == ItemType.Rod) rodMultipler += 1f;
                CheckSelectedCell();
            }
            if (cell.cellIndex.x < cellsGrid.GetLength(0) - 1) //right
            {
                _selectedCell = cellsGrid[(int)cell.cellIndex.x + 1, (int)cell.cellIndex.y];
                if (_selectedCell.cellItem?.ItemType == ItemType.Rod) rodMultipler += 1f;
                CheckSelectedCell();
            }

            cell.cellItem.durability--;
            cell.cellItem.UpdateDurabilityBar();
            RodInfo rodInfo = (RodInfo)ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
            if(_selectedItems.Count != 0)
            {
                float heatPerCell = (rodInfo.outHeat * rodMultipler) / _selectedItems.Count;
                foreach (var item in _selectedItems)
                {
                    item.heat += heatPerCell;
                }
            }
            else
            {
                addHeat += rodInfo.outHeat * rodMultipler;
            }
            addPower += (rodInfo.outPower * upgradeEffMultipler) * rodMultipler;

            //destroy rod check
            if(cell.cellItem.durability <= 0)
            {
                _destroyList.Add(cell);
            }
        }


        //Pipes
        upgradeEffMultipler = 1 + PlayerManager.Instance.player.upgrades[UpgradeType.Pipe_Eff];
        foreach (var cell in itemsDictionary[ItemType.HeatPipe])
        {
            if(cell.cellItem.heat != 0)
            {
                _selectedItems.Clear();
                if (cell.cellIndex.y > 0) //up
                {
                    _selectedCell = cellsGrid[(int)cell.cellIndex.x, (int)cell.cellIndex.y - 1];
                    CheckSelectedCell();
                }
                if (cell.cellIndex.y < cellsGrid.GetLength(1) - 1) //down
                {
                    _selectedCell = cellsGrid[(int)cell.cellIndex.x, (int)cell.cellIndex.y + 1];
                    CheckSelectedCell();
                }
                if (cell.cellIndex.x > 0) //left
                {
                    _selectedCell = cellsGrid[(int)cell.cellIndex.x - 1, (int)cell.cellIndex.y];
                    CheckSelectedCell();
                }
                if (cell.cellIndex.x < cellsGrid.GetLength(0) - 1) //right
                {
                    _selectedCell = cellsGrid[(int)cell.cellIndex.x + 1, (int)cell.cellIndex.y];
                    CheckSelectedCell();
                }

                for (int i = 0; i < _selectedItems.Count; i++)
                {
                    if (_selectedItems[i].heat > cell.cellItem.heat)
                    {
                        _selectedItems.Remove(_selectedItems[i]);
                        i--;
                    }
                }
                HeatPipeInfo pipeInfo = (HeatPipeInfo)ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
                if (_selectedItems.Count != 0)
                {
                    float heatPerCell = 0;
                    float heatThroughput = pipeInfo.heatThroughput * upgradeEffMultipler;
                    if (cell.cellItem.heat <= heatThroughput)
                    {
                        heatPerCell = cell.cellItem.heat / _selectedItems.Count;
                        cell.cellItem.heat = 0;
                    }
                    else
                    {
                        heatPerCell = heatThroughput / _selectedItems.Count;
                        cell.cellItem.heat -= heatThroughput;
                    }

                    foreach (var item in _selectedItems)
                    {
                        item.heat += heatPerCell;
                    }
                }
                cell.cellItem.UpdateHeatBar();

                //destroy pipe check
                if (cell.cellItem.durability <= cell.cellItem.heat)
                {
                    _destroyList.Add(cell);
                }
            } 
        }


        //Vents
        upgradeEffMultipler = 1 + PlayerManager.Instance.player.upgrades[UpgradeType.Vent_Eff];
        foreach (var cell in itemsDictionary[ItemType.HeatVent])
        {
            HeatVentInfo pipeInfo = (HeatVentInfo)ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
            cell.cellItem.heat -= pipeInfo.decreaseHeat * upgradeEffMultipler;
            if (cell.cellItem.heat < 0)
                cell.cellItem.heat = 0;
            cell.cellItem.UpdateHeatBar();

            //destroy vent check
            if (cell.cellItem.durability <= cell.cellItem.heat)
            {
                _destroyList.Add(cell);
            }
        }


        //destroy
        foreach (var cell in _destroyList)
        {
            if (cell.cellItem.ItemType == ItemType.Rod)
            {
                itemsDictionary[ItemType.Rod].Remove(cell);
                usedRodsList.Add(cell);
                cell.cellItem.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 128);
                cell.cellItem.hpBar.gameObject.SetActive(false);
                cell.cellItem.durability = 0;
            }
            else
            {
                DestroyItem(cell, true);
            }
        }

        //AutoReplace rods
        if (autoReplaceMode)
        {
            for (int i = 0; i < usedRodsList.Count; i++)
            {
                //
            }
        }

        //UI//Heat//Energy
        Heat += addHeat;
        Power += addPower;
        PlayerManager.Instance.Money += addMoney;

        //DEBUG
        sw.Stop();
        Debug.Log("ticks: " + sw.ElapsedTicks);
    }

    private float GetRodEffMultipler(int gradeType)
    {
        if (gradeType < 3)
            return 1 + PlayerManager.Instance.player.upgrades[UpgradeType.RodGreen_PowerEff];
        if (gradeType < 6)
            return 1 + PlayerManager.Instance.player.upgrades[UpgradeType.RodYellow_PowerEff];
        if (gradeType < 9)
            return 1 + PlayerManager.Instance.player.upgrades[UpgradeType.RodBlue_PowerEff];
        if (gradeType < 12)
            return 1 + PlayerManager.Instance.player.upgrades[UpgradeType.RodPurple_PowerEff];
        if (gradeType < 15)
            return 1 + PlayerManager.Instance.player.upgrades[UpgradeType.RodRed_PowerEff];
        if (gradeType < 18)
            return 1 + PlayerManager.Instance.player.upgrades[UpgradeType.RodOrange_PowerEff];
        return 1;
    }

    private void CheckSelectedCell()
    {
        
        if (_selectedCell.cellItem != null
                    && _selectedCell.cellItem.ItemType != ItemType.Rod
                    && _selectedCell.cellItem.ItemType != ItemType.HeatPlate)
        {
            if (_selectedCell.cellItem.ItemType != ItemType.Battery)
            {
                _selectedItems.Add(_selectedCell.cellItem);
            }
            else
            {
                _destroyList.Add(_selectedCell);
            }
        }
    }

    private void DestroyItem(Cell cell, bool explosion)
    {
        if (explosion)
        {
            Vector3 position = cell.transform.position;
            position.z = -1;
            PoolManager.Instance.GetExplosionObject(position, transform).Play();
        }
        if(cell.cellItem.ItemType == ItemType.Battery)
        {
            ItemInfo itemInfo = ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
            float durability = itemInfo.durability * GetItemDurabilityMultipler(cell.cellItem.ItemType, cell.cellItem.itemGradeType);
            MaxPower -= durability;
        }
        if(cell.cellItem.ItemType == ItemType.HeatPlate)
        {
            ItemInfo itemInfo = ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
            float durability = itemInfo.durability * GetItemDurabilityMultipler(cell.cellItem.ItemType, cell.cellItem.itemGradeType);
            MaxHeat -= durability;
        }
        itemsDictionary[cell.cellItem.ItemType].Remove(cell);
        PoolManager.Instance.ReturnItemToPool(cell.cellItem);
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

    private float GetItemDurabilityMultipler(ItemType itemType, int itemGrade)
    {
        switch (itemType)
        {
            case ItemType.Rod:
                if(itemGrade < 3)
                    return (PlayerManager.Instance.player.upgrades[UpgradeType.RodGreen_Durability] + 1);
                if (itemGrade < 6)
                    return (PlayerManager.Instance.player.upgrades[UpgradeType.RodBlue_Durability] + 1);
                if (itemGrade < 9)
                    return (PlayerManager.Instance.player.upgrades[UpgradeType.RodBlue_Durability] + 1);
                if (itemGrade < 12)
                    return (PlayerManager.Instance.player.upgrades[UpgradeType.RodBlue_Durability] + 1);
                if (itemGrade < 15)
                    return (PlayerManager.Instance.player.upgrades[UpgradeType.RodBlue_Durability] + 1);
                if (itemGrade < 18)
                    return (PlayerManager.Instance.player.upgrades[UpgradeType.RodBlue_Durability] + 1);
                return 1;
            case ItemType.HeatPipe:
                return (PlayerManager.Instance.player.upgrades[UpgradeType.Pipe_Durability] + 1);
            case ItemType.HeatVent:
                return (PlayerManager.Instance.player.upgrades[UpgradeType.Vent_Durability] + 1);
            case ItemType.HeatPlate:
                return (PlayerManager.Instance.player.upgrades[UpgradeType.Plate_Durability] + 1);
            case ItemType.Battery:
                return (PlayerManager.Instance.player.upgrades[UpgradeType.Battery_Durability] + 1);

            default:
                return 1;
        }
    }


    internal void DecreaseHeatClick()
    {
        if(heat > 0) Heat--;
    }

    internal void SellEnergyClick()
    {
        PlayerManager.Instance.Money += power;
        Power = 0;
    }

    internal void ShopTabChanged(int tabIndex)
    {
        currentTab = tabIndex;
        if(currentTab == selectedItemTab)
        {
            preBuyItemSelected.gameObject.SetActive(true);
        }
        else
        {
            preBuyItemSelected.gameObject.SetActive(false);
        }
    }

    internal void SelectPreBuildItem(GameObject prefab, Vector2 shopItemPos)
    {
        selectedItemTab = currentTab;
        preBuyItemSelected.gameObject.SetActive(true);
        preBuyItemSelected.position = shopItemPos;
        preBuyItemPrefab = prefab;
        buildMod = true;
    }

    internal void SellItem(Cell cell)
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

    internal void BuyItem(Vector2 cellIndex)
    {
        Cell cell = cellsGrid[(int)cellIndex.x, (int)cellIndex.y];
        if (cell.cellItem == null || cell.cellItem.durability == 0)
        {
            IItem item = preBuyItemPrefab.GetComponent<IItem>();
            ItemInfo itemInfo = ItemsManager.Instance.itemsInfo[item.ItemType][item.itemGradeType];
            if(itemInfo.cost <= PlayerManager.Instance.Money)
            {
                PlayerManager.Instance.Money -= itemInfo.cost;

                Vector3 position = cell.transform.position;
                position.z = 0;
                item = PoolManager.Instance.GetItemObject(item.ItemType, item.itemGradeType,
                                                          position, transform);

                float durability = itemInfo.durability * GetItemDurabilityMultipler(item.ItemType, item.itemGradeType);
                item.durability = durability;
                item.heat = 0;
                if(item.hpBar != null)
                {
                    item.hpBar.maxValue = item.durability;
                    item.hpBar.value = item.durability;
                }

                cell.cellItem = item;
                itemsDictionary[item.ItemType].Add(cell);

                if (cell.cellItem.ItemType == ItemType.Battery)
                    MaxPower += durability;
                if (cell.cellItem.ItemType == ItemType.HeatPlate)
                    MaxHeat += durability;
            }
        }
    }

    internal void CalcMaxHeat()
    {
        float maxHeat = 100;
        foreach (var item in itemsDictionary[ItemType.HeatPlate])
        {
            maxHeat += ItemsManager.Instance
                .itemsInfo[item.cellItem.ItemType][item.cellItem.itemGradeType].durability 
                * GetItemDurabilityMultipler(item.cellItem.ItemType,item.cellItem.itemGradeType);
        }
        MaxHeat = maxHeat;
    }

    internal void CalcMaxPower()
    {
        float maxPower = 100;
        foreach (var item in itemsDictionary[ItemType.Battery])
        {
            maxPower += ItemsManager.Instance
                .itemsInfo[item.cellItem.ItemType][item.cellItem.itemGradeType].durability
                * GetItemDurabilityMultipler(item.cellItem.ItemType, item.cellItem.itemGradeType);
        }
        MaxPower = maxPower;
    }
}
