using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopUpgradeItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #pragma warning disable CS0649
    [SerializeField]
    private UpgradeType upgradeType;
    #pragma warning restore CS0649
    private Transform shopItemTransform;

    public bool isOpenUpgrade = true;
    public UpgradeType UpgradeType { get => upgradeType; set => upgradeType = value; }

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
        if (!isOpenUpgrade) return;

        UpgradeInfo upgradeInfo = ItemsManager.Instance.upgradesInfo[upgradeType];
        ItemInfoPanel infoPanel = ItemsManager.Instance.itemInfoPanel;
        infoPanel.itemName.text = LocalizeText.CurrentLanguageStrings[upgradeInfo.keyName];
        if(upgradeInfo.maxUpgradeLvl != PlayerManager.Instance.player.upgrades[upgradeType])
        {
            infoPanel.itemCost.text = upgradeInfo.GetCost(PlayerManager.Instance.player.upgrades[upgradeType]) + " $";
        }
        else
        {
            infoPanel.itemCost.text = "MAX LVL";
        }
        infoPanel.itemDescription.text = LocalizeText.CurrentLanguageStrings[upgradeInfo.keyDesc];

        infoPanel.SetPosition(Input.mousePosition);
        infoPanel.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemsManager.Instance.itemInfoPanel.gameObject.SetActive(false);
    }
}
