using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ShopReactorItem: MonoBehaviour
{
    #pragma warning disable CS0649
    [SerializeField]
    private Text nameText;
    [SerializeField]
    private Text sizeText;
    [SerializeField]
    private Text costText;
    #pragma warning restore CS0649

    private int reactorType;

    public void SetInfo(int _reactorType)
    {
        reactorType = _reactorType;
        ReactorInfo info = ItemsManager.Instance.reactorsInfo[reactorType];
        nameText.text = "Mk." + reactorType;
        sizeText.text = info.gridSize[0] + "x" + info.gridSize[0];
        costText.text = Formatter.BigNumbersFormat(info.cost);
    }

    public void BuyButtonClick()
    {
        ReactorManager.Instance.BuyReactor(reactorType);
    }
}
