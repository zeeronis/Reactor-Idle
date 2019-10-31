﻿using System.Collections;
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

    private Cell[,] cellsGrid;
    private Dictionary<ItemType, List<Cell>> itemsDictionary = new Dictionary<ItemType, List<Cell>>()
    {
        [ItemType.Rod] = new List<Cell>(),
        [ItemType.HeatPipe] = new List<Cell>(),
        [ItemType.HeatVent] = new List<Cell>(),
        [ItemType.HeatPlate] = new List<Cell>(),
        [ItemType.Battery] = new List<Cell>(),
    };
    private List<Cell> usedRodList = new List<Cell>();
    [SerializeField]
    private GameObject cellPrefab;
    private const float cellOffset = 2.5603f;

    private float updateRate = 0.5f;
    private float nextUpdateTime = 0;

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
        get { return maxPower; }
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

    /* Update tasks
     ================
     = Rods
     = Pipes
     = Vents
     = Check Durability/Heat (for destroy)
     = UI  
     ================
    */
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

        //Rods
        foreach (var cell in itemsDictionary[ItemType.Rod])
        {
            rodMultipler = 1;
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
            addPower += rodInfo.outPower * rodMultipler;

            //destroy rod check
            if(cell.cellItem.durability <= 0)
            {
                _destroyList.Add(cell);
            }
        }


        //Pipes
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
                    if(cell.cellItem.heat <= pipeInfo.heatThroughput)
                    {
                        heatPerCell = cell.cellItem.heat / _selectedItems.Count;
                        cell.cellItem.heat = 0;
                    }
                    else
                    {
                        heatPerCell = pipeInfo.heatThroughput / _selectedItems.Count;
                        cell.cellItem.heat -= pipeInfo.heatThroughput;
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
        foreach (var cell in itemsDictionary[ItemType.HeatVent])
        {
            HeatVentInfo pipeInfo = (HeatVentInfo)ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
            cell.cellItem.heat -= pipeInfo.decreaseHeat;
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
                usedRodList.Add(cell);
                cell.cellItem.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 128);
                cell.cellItem.durability = 0;
            }
            else
            {
                DestroyItem(cell, true);
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
            ItemInfo info = ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
            MaxPower -= info.durability;
        }
        if(cell.cellItem.ItemType == ItemType.HeatPlate)
        {
            ItemInfo info = ItemsManager.Instance.itemsInfo[cell.cellItem.ItemType][cell.cellItem.itemGradeType];
            MaxHeat -= info.durability;
        }
        itemsDictionary[cell.cellItem.ItemType].Remove(cell);
        PoolManager.Instance.ReturnToPool(cell.cellItem);
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
        preBuyItemPrefab.transform.position = position;
        preBuyItemPrefab.GetComponent<SpriteRenderer>().color = isEmptyCell ?
            new Color32(255, 255, 255, 255) : new Color32(255, 131, 131, 131);

    }

    public void SelectPreBuildItem(GameObject prefab)
    {
        preBuyItemPrefab = prefab;
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
                item.durability = itemInfo.durability;
                item.hpBar.maxValue = item.durability;
                item.hpBar.value = item.durability;
                cell.cellItem = item;

                itemsDictionary[item.ItemType].Add(cell);

                if (cell.cellItem.ItemType == ItemType.Battery)
                    MaxPower += itemInfo.durability;
                if (cell.cellItem.ItemType == ItemType.HeatPlate)
                    MaxHeat += itemInfo.durability;
            }
        }
    }

}
