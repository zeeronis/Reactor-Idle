using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class IItem: MonoBehaviour
{
    public Slider hpBar;
    public bool currentlyInUse;

    public ItemType ItemType;
    public int itemGradeType;
    public float durability;
    public float heat;

    
    public void UpdateDurabilityBar()
    {
        hpBar.value = durability;
    }

    public void UpdateHeatBar()
    {
        if (hpBar != null)
        {
            if(heat == 0)
            {
                hpBar.gameObject.SetActive(false);
            }
            else
            {
                hpBar.gameObject.SetActive(true);
                hpBar.value = heat;
            }

        }
            
    }
}
