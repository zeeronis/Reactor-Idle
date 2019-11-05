using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2 cellIndex;
    public IItem cellItem;

    private static Cell mouseDownCell;
    private bool isBuildAction;


    private void OnMouseDown()
    {
        mouseDownCell = this;
        if (cellItem == null)
        {
            isBuildAction = true;
        }
        else
        {
            isBuildAction = false;
        }
    }

    private void OnMouseUp()
    {
        if (mouseDownCell == this)
        {
            if (isBuildAction && ReactorManager.Instance.buildMod)
            {
                ReactorManager.Instance.BuyItem(cellIndex);
            }
            else if (cellItem != null)
            {
                ReactorManager.Instance.SellItem(this);
            }
        }
    }
}
