using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TileInfoUIUpdater : MonoBehaviour
{
    public Image panelBG;

    public Text nameText;
    public Text descText;
    public Text costText;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    public void Show(TileMain tileScript)
    {
        // Set cost amounts based on set
        if (tileScript.isBaseTile)
        {
            costText.text = "No Cost";
        }
        else if (tileScript.setName == "Forest")
        {
            costText.text = tileScript.resourceCosts[0].ToString() + " wd  " + tileScript.resourceCosts[1].ToString() + " fd";
        }
        else if (tileScript.setName == "Medieval")
        {
            costText.text = tileScript.resourceCosts[2].ToString() + " ir  " + tileScript.resourceCosts[3].ToString() + " wp";
        }
        else if (tileScript.setName == "Modern")
        {
            costText.text = tileScript.resourceCosts[4].ToString() + " en  " + tileScript.resourceCosts[5].ToString() + " gl";
        }

        nameText.text = tileScript.tileName;
        descText.text = tileScript.description;

        if (tileScript.owner.playerID == 0)
        {
            panelBG.color = tileScript.gameController.player0.PlayerUIColour;
        }
        else if (tileScript.owner.playerID == 1)
        {
            panelBG.color = tileScript.gameController.player1.PlayerUIColour;
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }
}
