using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopUpgradeItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private UpgradeType upgradeType;
    private Transform shopItemTransform;

    public void Start()
    {
        shopItemTransform = transform;
    }


    public void Click()
    {
        if (PlayerManager.Instance.BuyUpgrade(upgradeType))
        {
            OnPointerEnter(null);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpgradeInfo upgradeInfo = ItemsManager.Instance.upgradesInfo[upgradeType];
        ItemInfoPanel infoPanel = ItemsManager.Instance.itemInfoPanel;
        infoPanel.itemName.text = LocalizeText.CurrentLanguageStrings[upgradeInfo.keyName];
        infoPanel.itemCost.text = upgradeInfo.GetCost(PlayerManager.Instance.player.upgrades[upgradeType]) + " $";
        infoPanel.itemDescription.text = LocalizeText.CurrentLanguageStrings[upgradeInfo.keyDesc];

        Vector3 viewPortPos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        infoPanel.transform.position = Input.mousePosition -
            new Vector3((infoPanel.GetComponent<RectTransform>().rect.x) * (viewPortPos.x < 0.49f ? 1 : -1),
            infoPanel.GetComponent<RectTransform>().rect.y - 7);
        infoPanel.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemsManager.Instance.itemInfoPanel.gameObject.SetActive(false);
    }
}
