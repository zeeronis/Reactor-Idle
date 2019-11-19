using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReactorManager : MonoBehaviour
{
    private static ReactorManager instance;
    public static ReactorManager Instance { get => instance; private set => instance = value; }
    public static bool IsReady { get; private set; }

    #pragma warning disable CS0649
    [SerializeField] private Slider powerBar;
    [SerializeField] private Slider heatBar;
    [SerializeField] private Text powerText;
    [SerializeField] private Text heatText;
    [SerializeField] private Text powerMonitorText;
    [SerializeField] private Text heatMonitorText;
    [SerializeField] private Button buttonIncreaceMoney;

    private Reactor reactor;
    private float maxPower;
    private float maxHeat;
    private float lastPowerInc;
    private float lastHeatInc;

    private Cell[,] cellsGrid;
    private Dictionary<ItemType, List<Cell>> reactorItems = new Dictionary<ItemType, List<Cell>>()
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
    #pragma warning restore CS0649

    private bool IsEmpty
    {
        get
        {
            return isEmpty;
        }
        set
        {
            isEmpty = value;
            CheckPlayerBankruptcy();
        }
    }

    private GameObject preBuyItemPrefab;
    public bool buildMod;
    public bool TouchCellsIsBlocked { get; set; }

    public float Power
    {
        get
        {
            return reactor.power;
        }
        private set
        {
            reactor.power = value <= MaxPower ? maxPower: MaxPower;
            powerBar.value = value;
            powerText.text = Formatter.BigNumbersFormat(value) + " / " + Formatter.BigNumbersFormat(maxPower);
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
            reactor.heat = value <= MaxHeat ? maxHeat : MaxHeat;
            heatBar.value = value;
            heatText.text = Formatter.BigNumbersFormat(value) + " / " + Formatter.BigNumbersFormat(maxPower);
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

        #if UNITY_EDITOR
        var sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        #endif

        _destroyList.Clear();
        if (reactor.heat == MaxHeat)
        {
            for (int i = 0; i < reactorItems.Count; i++)
            {
                for (int j = 0; j < reactorItems[(ItemType)i].Count; j++)
                {
                    DestroyItem(reactorItems[(ItemType)i][j], true);
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
        var playerUpgrades = PlayerManager.Instance.player.upgrades;
        IItem cellItem;

        //Rods
        foreach (var cell in reactorItems[ItemType.Rod])
        {
            rodMultipler = 1;
            cellItem = cell.cellItem;
            int cellX = (int)cell.cellIndex.x;
            int cellY = (int)cell.cellIndex.y;
            upgradeEffMultipler = RodInfo.GetRodEffMultipler(cellItem.itemGradeType);
            _selectedItems.Clear();

            if (cellY > 0) //up
            {
                _selectedCell = cellsGrid[cellX, cellY - 1];
                if (_selectedCell.cellItem?.ItemType == ItemType.Rod)
                    rodMultipler += playerUpgrades[UpgradeType.Neighboring_Rods_Eff] * 0.05f; 
                CheckSelectedCell();
            }
            if(cellY < cellsGrid.GetLength(1) - 1) //down
            {
                _selectedCell = cellsGrid[cellX, cellY + 1];
                if (_selectedCell.cellItem?.ItemType == ItemType.Rod)
                    rodMultipler += playerUpgrades[UpgradeType.Neighboring_Rods_Eff] * 0.05f;
                CheckSelectedCell();
            }
            if (cellX > 0) //left
            {
                _selectedCell = cellsGrid[cellX - 1, cellY];
                if (_selectedCell.cellItem?.ItemType == ItemType.Rod)
                    rodMultipler += playerUpgrades[UpgradeType.Neighboring_Rods_Eff] * 0.05f;
                CheckSelectedCell();
            }
            if (cellX < cellsGrid.GetLength(0) - 1) //right
            {
                _selectedCell = cellsGrid[cellX + 1, cellY];
                if (_selectedCell.cellItem?.ItemType == ItemType.Rod)
                    rodMultipler += playerUpgrades[UpgradeType.Neighboring_Rods_Eff] * 0.05f;
                CheckSelectedCell();
            }

            cellItem.durability--;
            cellItem.UpdateDurabilityBar();
            RodInfo rodInfo = (RodInfo)ItemsManager.Instance.itemsInfo[cellItem.ItemType][cellItem.itemGradeType];
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
            if(cellItem.durability <= 0)
            {
                _destroyList.Add(cell);
            }
        }


        //Pipes
        upgradeEffMultipler = 1 + playerUpgrades[UpgradeType.Pipe_Eff];
        foreach (var cell in reactorItems[ItemType.HeatPipe])
        {
            if(cell.cellItem.heat != 0)
            {
                cellItem = cell.cellItem;
                int cellX = (int)cell.cellIndex.x;
                int cellY = (int)cell.cellIndex.y;
                _selectedItems.Clear();
                if (cellY > 0) //up
                {
                    _selectedCell = cellsGrid[cellX, cellY - 1];
                    CheckSelectedCell();
                }
                if (cellY < cellsGrid.GetLength(1) - 1) //down
                {
                    _selectedCell = cellsGrid[cellX, cellY + 1];
                    CheckSelectedCell();
                }
                if (cell.cellIndex.x > 0) //left
                {
                    _selectedCell = cellsGrid[cellX - 1, cellY];
                    CheckSelectedCell();
                }
                if (cell.cellIndex.x < cellsGrid.GetLength(0) - 1) //right
                {
                    _selectedCell = cellsGrid[cellX + 1, cellY];
                    CheckSelectedCell();
                }

                for (int i = 0; i < _selectedItems.Count; i++)
                {
                    if (_selectedItems[i].heat > cellItem.heat)
                    {
                        _selectedItems.Remove(_selectedItems[i]);
                        i--;
                    }
                }

                HeatPipeInfo pipeInfo = (HeatPipeInfo)ItemsManager.Instance.itemsInfo[cellItem.ItemType][cellItem.itemGradeType];
                if (_selectedItems.Count != 0)
                {
                    float heatPerCell = 0;
                    float heatThroughput = pipeInfo.heatThroughput * upgradeEffMultipler;
                    if (cellItem.heat <= heatThroughput)
                    {
                        heatPerCell = cellItem.heat / _selectedItems.Count;
                        cellItem.heat = 0;
                    }
                    else
                    {
                        heatPerCell = heatThroughput / _selectedItems.Count;
                        cellItem.heat -= heatThroughput;
                    }

                    foreach (var item in _selectedItems)
                    {
                        item.heat += heatPerCell;
                    }
                }
                cellItem.UpdateHeatBar();

                //destroy pipe check
                if (cellItem.durability <= cellItem.heat)
                {
                    _destroyList.Add(cell);
                }
            } 
        }


        //Vents
        upgradeEffMultipler = 1 + playerUpgrades[UpgradeType.Vent_Eff];
        foreach (var cell in reactorItems[ItemType.HeatVent])
        {
            cellItem = cell.cellItem;
            HeatVentInfo pipeInfo = (HeatVentInfo)ItemsManager.Instance.itemsInfo[cellItem.ItemType][cellItem.itemGradeType];

            if (cellItem.heat > 0)
            {
                cellItem.heat -= pipeInfo.decreaseHeat * upgradeEffMultipler;
                if (cellItem.heat < 0)
                    cellItem.heat = 0;
                if((cellItem.lastHeat != cellItem.heat))
                {
                    cellItem.UpdateHeatBar();
                    cellItem.lastHeat = cellItem.heat;
                }
               
                //destroy vent check
                if (cellItem.durability <= cellItem.heat)
                {
                    _destroyList.Add(cell);
                }
            }
        }


        //destroy
        foreach (var cell in _destroyList)
        {
            cellItem = cell.cellItem;
            if (cellItem.ItemType == ItemType.Rod)
            {
                reactorItems[ItemType.Rod].Remove(cell);
                usedRodsList.Add(cell);
                cellItem.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 128);
                cellItem.hpBar.gameObject.SetActive(false);
                cellItem.durability = 0;
            }
            else
            {
                DestroyItem(cell, true);
            }
        }

        //AutoReplace rods
        if (PlayerManager.Instance.AutoReplaceMode)
        {
            for (int i = 0; i < usedRodsList.Count; i++)
            {
                if (RodInfo.CanRodAutoReplace(usedRodsList[i].cellItem.itemGradeType))
                {
                    BuyItem(usedRodsList[i].cellIndex, true);
                }
            }
        }

        //UI//Heat//Energy//Money
        if(addHeat > 0) Heat += addHeat;
        if(addPower > 0) Power += addPower;
        if(addHeat != lastHeatInc)
        {
            heatMonitorText.text = "+" + Formatter.BigNumbersFormat(addHeat);
            lastHeatInc = addHeat;
        }
        if(addPower != lastPowerInc)
        {
            powerMonitorText.text = "+" + Formatter.BigNumbersFormat(addPower);
            lastPowerInc = addPower;
        }

        float canDecreaseHeat = maxHeat / 100 * playerUpgrades[UpgradeType.AutoDecreaseHeat];
        if(reactor.heat > 0) Heat -= reactor.heat < canDecreaseHeat ? reactor.heat : canDecreaseHeat;
        float canSellPower = maxPower / 100 * playerUpgrades[UpgradeType.AutoSellPower];
        float soldPower = reactor.power < canSellPower ? reactor.power : canSellPower;
        if(soldPower > 0) Power -= soldPower;
        PlayerManager.Instance.Money += addMoney + soldPower;

        #if UNITY_EDITOR
        //DEBUG
        sw.Stop();
        Debug.Log("ticks: " + sw.ElapsedTicks);
        #endif
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
        if(cell.cellItem.ItemType == ItemType.Rod && cell.cellItem.durability <= 0)
        {
            usedRodsList.Remove(cell);
        }
        else
        {
            reactorItems[cell.cellItem.ItemType].Remove(cell);
        }
      
        if (explosion)
        {
            Vector3 position = cell.transform.position;
            position.z = -1;
            PoolManager.Instance.GetExplosionObject(position, transform).Play();
        }
        if(cell.cellItem.ItemType == ItemType.Battery)
        {
            ItemInfo itemInfo = ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
            float durability = itemInfo.durability * ItemInfo.GetItemDurabilityMultipler(cell.cellItem.ItemType, cell.cellItem.itemGradeType);
            CalcMaxPower();
        }
        if(cell.cellItem.ItemType == ItemType.HeatPlate)
        {
            ItemInfo itemInfo = ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
            float durability = itemInfo.durability * ItemInfo.GetItemDurabilityMultipler(cell.cellItem.ItemType, cell.cellItem.itemGradeType);
            CalcMaxHeat();
        }
        PoolManager.Instance.ReturnItemToPool(cell.cellItem);
        cell.cellItem = null;
    }

    private void CreateGrid(Vector2 size, Vector2 startGridPoint, bool isLoadGame)
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
                if(isLoadGame)
                {
                    if(row < reactor.serializableCells.GetLength(0) && column < reactor.serializableCells.GetLength(1))
                    {
                        if (reactor.serializableCells[row, column] != null)
                        {
                            SetItem(cellsGrid[row, column],
                                    reactor.serializableCells[row, column],
                                    false);
                        }
                    }
                }
            }
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
            float durability = itemInfo.durability * ItemInfo.GetItemDurabilityMultipler(newItem.ItemType, newItem.itemGradeType);
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
            reactorItems[newItem.ItemType].Add(cell);
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
        foreach (var item in reactorItems)
        {
            if (item.Value.Count != 0)
                return;
        }
        IsEmpty = true;
    }

    internal void BuyItem(Vector2 cellIndex, bool isAutoBuy)
    {
        GameObject playerPreBuyItem = null;
        Cell cell = cellsGrid[(int)cellIndex.x, (int)cellIndex.y];
        if(cell.cellItem?.ItemType == ItemType.Rod)
        {
            if (isAutoBuy)
            {
                playerPreBuyItem = preBuyItemPrefab;
                preBuyItemPrefab = ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType]
                                     .prefab;
            }
            SellItem(cell);
        }
        if (cell.cellItem == null)
        {
            IItem item = preBuyItemPrefab.GetComponent<IItem>();
            ItemInfo itemInfo = ItemsManager.Instance.itemsInfo[item.ItemType][item.itemGradeType];
            float itemCost = itemInfo.cost * (isAutoBuy ? 1.5f : 1);
            if (itemCost <= PlayerManager.Instance.Money)
            {
                PlayerManager.Instance.Money -= itemCost;
                SetItem(cell, item, true);
                
            }
        }
        if (isAutoBuy && playerPreBuyItem != null) preBuyItemPrefab = playerPreBuyItem;
    }

    internal void BuyReactor(int reactorType)
    {
        if (reactor.heat == 0 && reactor.power == 0)
        {
            ReactorInfo reactorInfo = ItemsManager.Instance.reactorsInfo[reactorType];
            Player player = PlayerManager.Instance.player;
            if (player.money >= reactorInfo.cost)
            {
                player.money -= reactorInfo.cost;
                player.reactor = new Reactor() { gradeType = reactorType };
                InitReactor(player.reactor, false);
            }
        }
    }

    internal void CalcMaxHeat()
    {
        float maxHeat = ItemsManager.Instance.reactorsInfo[reactor.gradeType].baseMaxHeat;
        foreach (var item in reactorItems[ItemType.HeatPlate])
        {
            maxHeat += ItemsManager.Instance
                .itemsInfo[item.cellItem.ItemType][item.cellItem.itemGradeType].durability 
                * ItemInfo.GetItemDurabilityMultipler(item.cellItem.ItemType,item.cellItem.itemGradeType);
        }
        MaxHeat = maxHeat;
    }

    internal void CalcMaxPower()
    {
        float maxPower = ItemsManager.Instance.reactorsInfo[reactor.gradeType].baseMaxPower;
        foreach (var item in reactorItems[ItemType.Battery])
        {
            maxPower += ItemsManager.Instance
                .itemsInfo[item.cellItem.ItemType][item.cellItem.itemGradeType].durability
                * ItemInfo.GetItemDurabilityMultipler(item.cellItem.ItemType, item.cellItem.itemGradeType);
        }
        MaxPower = maxPower;
    }

    internal void InitReactor(Reactor _reactor, bool isLoadGame)
    {
        ItemsManager.Instance.CheckBlockedItems(!isLoadGame, true);

        //destroy last cells
        if (cellsGrid != null)
        {
            usedRodsList.Clear();
            foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
            {
                reactorItems[itemType].Clear();
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
        reactor = _reactor;
        ReactorInfo reactorInfo = ItemsManager.Instance.reactorsInfo[reactor.gradeType];
        IsEmpty = true;

        CreateGrid(new Vector2(reactorInfo.gridSize[0], reactorInfo.gridSize[1]), 
                   new Vector2(reactorInfo.drawStartposition[0], reactorInfo.drawStartposition[1]), 
                   isLoadGame);
        CalcMaxHeat();
        CalcMaxPower();
        Heat = reactor.heat;
        Power = reactor.power;
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

    internal void CheckPlayerBankruptcy()
    {
        if (isEmpty && PlayerManager.Instance?.Money < 10 && Power == 0)
        {
            buttonIncreaceMoney.gameObject.SetActive(true);
        }
    }
}
