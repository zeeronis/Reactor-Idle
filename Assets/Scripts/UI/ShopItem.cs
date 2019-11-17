using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #pragma warning disable CS0649
    [SerializeField]
    internal ItemType itemType;
    [SerializeField]
    internal int itemGradeType;
    #pragma warning restore CS0649
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

        infoPanel.SetPosition(Input.mousePosition);
        infoPanel.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isOpenItem) return;

        ItemsManager.Instance.itemInfoPanel.gameObject.SetActive(false);
    }
}
