using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    internal ItemType itemType;
    [SerializeField]
    internal int itemGradeType;

    private Transform itemTransform;
    public bool isOpenItem = true;

    public void Start()
    {
        itemTransform = transform;
    }


    public void Click()
    {
        if (!isOpenItem) return;

        ReactorManager.Instance.SelectPreBuildItem(
                        ItemsManager.Instance.itemsInfo[itemType][itemGradeType].prefab,
                        itemTransform.position);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isOpenItem) return;

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
        if (!isOpenItem) return;

        ItemsManager.Instance.itemInfoPanel.gameObject.SetActive(false);
    }
}
