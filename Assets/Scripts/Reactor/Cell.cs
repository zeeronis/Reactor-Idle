using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2 cellIndex;
    public IItem cellItem;

    private Vector3 mousePosition;
    private bool isBuildAction;


    private void OnMouseDown()
    {
        mousePosition = Input.mousePosition;
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
        if (mousePosition == Input.mousePosition)
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

    private void OnMouseEnter()
    {
        if (ReactorManager.Instance.buildMod)
        {
            ReactorManager.Instance.MovePreBuyItem(transform.position, 
                                                   cellItem == null ? true : false);
        }
    }
}
