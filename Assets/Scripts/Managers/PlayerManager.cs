using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager instance;
    public static PlayerManager Instance { get => instance; private set => instance = value; }

    [SerializeField]
    private Text moneyText;
    private float money;

    private bool pauseMode;

    public float Money
    {
        get
        {
            return money;
        }
        set
        {
            money = value;
            moneyText.text = value.ToString() + " $";
        }
    }
    public bool PauseMode { get => pauseMode; set => pauseMode = value; }


    private void Start()
    {
        if (Instance == null)
            Instance = this;

        Money = 1000000;
    }
}
