using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReactorManager : MonoBehaviour
{
    private static ReactorManager instance;
    public static ReactorManager Instance { get => instance; private set => instance = value; }
    public static bool IsReady { get; private set; }

    [SerializeField] private Slider powerBar;
    [SerializeField] private Slider heatBar;
    [SerializeField] private Text powerText;
    [SerializeField] private Text heatText;
    [SerializeField] private Button buttonIncreaceMoney;

    private Reactor reactor;
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
    private bool isEmpty;

    private bool IsEmpty
    {
        get
        {
            return isEmpty;
        }
        set
        {
            isEmpty = value;
            if (isEmpty && PlayerManager.Instance?.Money < 10 && Power == 0)
            {
                buttonIncreaceMoney.gameObject.SetActive(true);
            }
        }
    }
    private GameObject preBuyItemPrefab;
    public bool buildMod = false;

    public float Power
    {
        get
        {
            return reactor.power;
        }
        private set
        {
            reactor.power = value <= MaxPower ? value: MaxPower;
            powerBar.value = value;
            powerText.text = reactor.power + " / " + MaxPower;
        }
    }
    public float Heat
    {
        get
        {
            return reactor.heat;
        }
        private set
        {
            reactor.heat = value <= MaxHeat ? value : MaxHeat;
            heatBar.value = value;
            heatText.text = reactor.heat + " / " + MaxHeat;
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

        IsReady = true;
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
        if (nextUpdateTime > Time.time || PlayerManager.Instance.PauseMode || !PlayerManager.IsReady)
            return;

        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        _destroyList.Clear();
        if (reactor.heat == MaxHeat)
        {
            for (int i = 0; i < itemsDictionary.Count; i++)
            {
                for (int j = 0; j < itemsDictionary[(ItemType)i].Count; j++)
                {
                    DestroyItem(itemsDictionary[(ItemType)i][j], true);
                }
            }
            IsEmpty = true;
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
        itemsDictionary[cell.cellItem.ItemType].Remove(cell);
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
            CalcMaxPower();
        }
        if(cell.cellItem.ItemType == ItemType.HeatPlate)
        {
            ItemInfo itemInfo = ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
            float durability = itemInfo.durability * GetItemDurabilityMultipler(cell.cellItem.ItemType, cell.cellItem.itemGradeType);
            CalcMaxHeat();
        }
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
                if(reactor.isLoadGame)
                {
                    if(reactor.serializableCells[row,column] != null)
                    {
                        SetItem(cellsGrid[row, column],
                                reactor.serializableCells[row, column],
                                false);
                    }
                }
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

    private void SetItem(Cell cell, object item, bool isNew)
    {
        IsEmpty = false;
        IItem newItem;
        Vector3 position = cell.transform.position;
        position.z = 0;

        if (isNew)
        {
            newItem = PoolManager.Instance.GetItemObject((item as IItem).ItemType,
                                                         (item as IItem).itemGradeType,
                                                          position, transform);
            ItemInfo itemInfo = ItemsManager.Instance.itemsInfo[(item as IItem).ItemType][(item as IItem).itemGradeType];
            float durability = itemInfo.durability * GetItemDurabilityMultipler(newItem.ItemType, newItem.itemGradeType);
            newItem.durability = durability;
            newItem.heat = 0;

            if (newItem.ItemType == ItemType.Battery)
                MaxPower += durability;
            if (newItem.ItemType == ItemType.HeatPlate)
                MaxHeat += durability;
        }
        else
        {
            newItem = PoolManager.Instance.GetItemObject((item as SerializableCell).ItemType,
                                                         (item as SerializableCell).itemGradeType,
                                                          position, transform);
            newItem.durability = (item as SerializableCell).durability;
            newItem.heat = (item as SerializableCell).heat;
            newItem.itemGradeType = (item as SerializableCell).itemGradeType;
            newItem.ItemType = (item as SerializableCell).ItemType;
        }


        if (newItem.hpBar != null)
        {
            if (isNew)
            {
                newItem.hpBar.maxValue = newItem.durability;
            }
            else
            {
                newItem.hpBar.maxValue =  ItemsManager.Instance.itemsInfo[newItem.ItemType][newItem.itemGradeType]
                                            .durability;
            }

            newItem.hpBar.value = newItem.durability;
            if(ItemType.Rod == newItem.ItemType)
            {
                newItem.UpdateDurabilityBar();
            }
            else
            {
                newItem.UpdateHeatBar();
            }
        }

        cell.cellItem = newItem;
        if(newItem.ItemType == ItemType.Rod && newItem.durability <= 0)
        {
            usedRodsList.Add(cell);
            newItem.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 128);
            newItem.hpBar.gameObject.SetActive(false);
        }
        else
        {
            itemsDictionary[newItem.ItemType].Add(cell);
        }
    }

    internal void IncreaceMoneyClick()
    {
        PlayerManager.Instance.Money++;
        if (PlayerManager.Instance.Money >= 10)
            buttonIncreaceMoney.gameObject.SetActive(false);
    }

    internal void DecreaseHeatClick()
    {
        if(reactor.heat > 0) Heat--;
    }

    internal void SellEnergyClick()
    {
        PlayerManager.Instance.Money += reactor.power;
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
        foreach (var item in itemsDictionary)
        {
            if (item.Value.Count != 0)
                return;
        }
        IsEmpty = true;
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
                SetItem(cell, item, true);
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

    internal void InitReactor(Reactor _reactor)
    {
        bool lastPauseMode = PlayerManager.Instance.PauseMode;
        PlayerManager.Instance.PauseMode = true;

        //destroy last cells
        if (cellsGrid != null)
        {
            usedRodsList.Clear();
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                itemsDictionary[itemType].Clear();
            }
            for (int row = 0; row < cellsGrid.GetLength(0); row++)
            {
                for (int column = 0; column < cellsGrid.GetLength(0); column++)
                {
                    if(cellsGrid[row, column].cellItem != null)
                    {
                        PoolManager.Instance.ReturnItemToPool(cellsGrid[row, column].cellItem);
                    }
                    Destroy(cellsGrid[row, column].gameObject);
                }
            }
        }
        IsEmpty = true;

        //get reactor info from ItemsManager
        reactor = _reactor;
        MaxHeat = 100;
        MaxPower = 100;
        Heat = reactor.heat;
        Power = reactor.power;

        CreateGrid(new Vector2(4, 4), new Vector2(-10, -5));//destroy last items/cells if generate new reactor
        CalcMaxHeat();
        CalcMaxPower();

        PlayerManager.Instance.PauseMode = lastPauseMode;
    }

    internal void SaveCells()
    {
        reactor.serializableCells = new SerializableCell[cellsGrid.GetLength(0), cellsGrid.GetLength(1)];
        for (int row = 0; row < cellsGrid.GetLength(0); row++)
        {
            for (int column = 0; column < cellsGrid.GetLength(1); column++)
            {
                if(cellsGrid[row, column].cellItem != null)
                {
                    reactor.serializableCells[row, column] = new SerializableCell()
                    {
                        ItemType = cellsGrid[row, column].cellItem.ItemType,
                        itemGradeType = cellsGrid[row, column].cellItem.itemGradeType,
                        heat = cellsGrid[row, column].cellItem.heat,
                        durability = cellsGrid[row, column].cellItem.durability,
                    };
                }
            }
        }
    }
}
