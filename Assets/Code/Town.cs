using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Town : MonoBehaviour
{
    public int woodAmount;
    public int woodBuyPrice;
    public int woodSellPrice;
    void Start()
    {
        woodAmount = Random.Range(5, 10);
        woodBuyPrice = Random.Range(20, 40);
        woodSellPrice = woodBuyPrice;
    }
}
