using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemInfoPanel : MonoBehaviour
{
    #pragma warning disable CS0649
    [SerializeField]
    private Transform _transform;
    [SerializeField]
    private RectTransform _rectTransform;
    #pragma warning restore CS0649

    public Text itemName;
    public Text itemDescription;
    public Text itemCost;

    public void SetPosition(Vector3 position)
    {
        Vector3 viewPortPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

        _transform.position = position - new Vector3(_rectTransform.rect.x * PlayerManager.Instance.UICanvasRect.localScale.x * (viewPortPos.x < 0.49f ? 1.2f : -1.2f),
                                                     _rectTransform.rect.y * PlayerManager.Instance.UICanvasRect.localScale.y);
    }
}
