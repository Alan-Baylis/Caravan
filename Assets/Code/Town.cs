using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Town : MonoBehaviour
{
    public int WoodAmount { get; set; }
    public int WoodBuyPrice { get; set; }
    public int WoodSellPrice { get; set; }
    void Start()
    {
        WoodAmount = Random.Range(5, 10);
        WoodBuyPrice = Random.Range(20, 40);
        WoodSellPrice = WoodBuyPrice;
    }
}
