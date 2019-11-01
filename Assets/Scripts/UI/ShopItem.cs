using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private ItemType itemType;
    [SerializeField]
    private int itemGradeType;

    private Transform itemTransform;

    public void Start()
    {
        itemTransform = transform;
    }


    public void Click()
    {
        ReactorManager.Instance.SelectPreBuildItem(
                    ItemsManager.Instance.itemsInfo[itemType][itemGradeType].prefab,
                    itemTransform.position);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        ItemInfo itemInfo = ItemsManager.Instance.itemsInfo[itemType][itemGradeType];
        ItemInfoPanel infoPanel = ItemsManager.Instance.itemInfoPanel;
        infoPanel.itemName.text = LocalizeText.CurrentLanguageStrings[itemInfo.keyName];
        infoPanel.itemCost.text = itemInfo.cost + " $";

        infoPanel.itemDescription.text = itemInfo.GetLocaleDesc(LocalizeText.CurrentLanguageStrings[itemInfo.keyDesc]);

        infoPanel.transform.position = Input.mousePosition - new Vector3(-infoPanel.GetComponent<RectTransform>().rect.x + 15, infoPanel.GetComponent<RectTransform>().rect.y - 7);
        infoPanel.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemsManager.Instance.itemInfoPanel.gameObject.SetActive(false);
    }
}
