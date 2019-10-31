﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoolManager : MonoBehaviour
{
    private static PoolManager instance;
    public static PoolManager Instance { get => instance; set => instance = value; }

    [SerializeField]
    private Dictionary<ItemType, List<IItem>> poolItems = new Dictionary<ItemType, List<IItem>>();
    private List<Slider> poolHpSliders = new List<Slider>();
    private List<ExplosionAnimation> poolExplosions = new List<ExplosionAnimation>();


    private void Start()
    {
        if (Instance == null)
            Instance = this;

        foreach (ItemType itemType in Enum.GetValues(typeof(ItemType)))
        {
            poolItems.Add(itemType, new List<IItem>());
        }
    }

    private Slider GetHpSlider(Vector2 position)
    {
        Slider slider;
        if (poolHpSliders.Count > 0)
        {
            slider = poolHpSliders[0];
            slider.transform.position = position - new Vector2(0, 0.9f);
            slider.gameObject.SetActive(true);
            poolHpSliders.Remove(slider);
        }
        else
        {
            slider = ItemsManager.Instance.GetSliderObject(position);
        }
        return slider;
    }

    internal void ReturnToPool(IItem item)
    {
        item.currentlyInUse = false;
        item.hpBar.gameObject.SetActive(false);
        item.gameObject.SetActive(false);
        poolHpSliders.Add(item.hpBar);
        item.hpBar = null;
    }

    internal IItem GetItemObject(ItemType itemType, int itemGrade, Vector3 position, Transform parentTransform)
    {
        foreach (var item in poolItems[itemType])
        {
            if (item.itemGradeType == itemGrade && !item.currentlyInUse)
            {
                item.currentlyInUse = true;
                item.transform.position = position;
                item.hpBar = GetHpSlider(position);
                item.gameObject.SetActive(true);
                if(itemType == ItemType.Rod)
                    item.GetComponent<SpriteRenderer>().color = new Color32(255, 255, 255, 255);
                return item;
            }
        }
        IItem newItem = Instantiate(ItemsManager.Instance.itemsInfo[itemType][itemGrade].prefab,
                                    position, Quaternion.identity, parentTransform).GetComponent<IItem>();
        poolItems[itemType].Add(newItem);
        newItem.hpBar = GetHpSlider(position);
        newItem.currentlyInUse = true;
        return newItem;
    }

    internal ExplosionAnimation GetExplosionObject(Vector3 position, Transform parentTransform)
    {
        foreach (ExplosionAnimation item in poolExplosions)
        {
            if (!item.currentlyInUse)
            {
                item.currentlyInUse = true;
                item.transform.position = position;
                item.gameObject.SetActive(true);
                return item;
            }
        }
        ExplosionAnimation newExplosion = Instantiate(ItemsManager.Instance.explosionItemPrefab,
                                                      position, Quaternion.identity, transform)
                                                      .GetComponent<ExplosionAnimation>();
        newExplosion.currentlyInUse = true;
        poolExplosions.Add(newExplosion);
        return newExplosion;
    }
}