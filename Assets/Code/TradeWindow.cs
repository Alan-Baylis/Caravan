using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeWindow : MonoBehaviour 
{
    public GameObject woodText;
    public GameObject buyButton;
    public GameObject sellButton;
    public GameObject player;

    void OnEnable()
    {
        Town currentTown = player.GetComponent<Player>().currentTown;

        woodText.GetComponent<Text>().text = "Wood: " + currentTown.woodAmount.ToString();
        buyButton.transform.GetChild(0).GetComponent<Text>().text = "Buy: " + currentTown.woodBuyPrice.ToString();
        sellButton.transform.GetChild(0).GetComponent<Text>().text = "Sell: " + currentTown.woodSellPrice.ToString();
    }

    public void SellWood()
    {
        if (player.GetComponent<Player>().WoodAmount > 0)
        {
            player.GetComponent<Player>().WoodAmount--;
            player.GetComponent<Player>().currentTown.woodAmount++;
            woodText.GetComponent<Text>().text = "Wood: " + player.GetComponent<Player>().currentTown.woodAmount.ToString();
            player.GetComponent<Player>().GoldAmount += player.GetComponent<Player>().currentTown.woodSellPrice;

            player.GetComponent<Player>().UpdateOnScreenText();
        }
    }

    public void BuyWood()
    {
        Town currentTown = player.GetComponent<Player>().currentTown;
        if (player.GetComponent<Player>().GoldAmount >= currentTown.woodBuyPrice && currentTown.woodAmount > 0)
        {
            player.GetComponent<Player>().WoodAmount++;
            player.GetComponent<Player>().currentTown.woodAmount--;
            woodText.GetComponent<Text>().text = "Wood: " + player.GetComponent<Player>().currentTown.woodAmount.ToString();
            player.GetComponent<Player>().GoldAmount -= player.GetComponent<Player>().currentTown.woodBuyPrice;

            player.GetComponent<Player>().UpdateOnScreenText();
        }
    }
}
