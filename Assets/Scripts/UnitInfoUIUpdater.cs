using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitInfoUIUpdater : MonoBehaviour
{
    public Text nameText;
    public Text HPText;
    public Text AtkPowerText;
    public Text SpeedPtsText;
    public Image panelBG;
    
    // Start is called before the first frame update
    void Start()
    {
        gameObject.SetActive(false);
    }

    // Update is called once per frame
    public void Show(TileMain tileScript)
    {
        UnitMain unitScript = tileScript.unitOnTile;
        nameText.text = unitScript.unitName;
        HPText.text = unitScript.healthPoints + "/" + unitScript.healthMax;
        AtkPowerText.text = unitScript.atkStrength + "";
        SpeedPtsText.text = unitScript.speedPoints + "/" + unitScript.speedMax;

        if(unitScript.playerOwner == 0)
        {
            panelBG.color = tileScript.gameController.player0.PlayerUIColour;
        }
        else if (unitScript.playerOwner == 1)
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
