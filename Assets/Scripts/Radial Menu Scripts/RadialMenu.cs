using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    public RadialButton buttonPrefab;
    public RadialButton selected;

    private List<RadialTile.Action> optionsShown = new List<RadialTile.Action>();

    public Text label;

    public RadialTile ogRadialTile;

    public void SpawnButtons(RadialTile obj)
    {
        if(ogRadialTile.thisTileScript.owner.playerID == ogRadialTile.thisTileScript.gameController.activePlayerID) // if the tile is owned by the active player show buttons
        {
            for (int i = 0; i < obj.options.Length; i++)
            {
                if (ogRadialTile.thisTileScript.unitOnTile != null && ogRadialTile.thisTileScript.unitOnTile.playerOwner == ogRadialTile.thisTileScript.gameController.activePlayerID) // if there is a unit on the tile and the unit is owned by the active player, show all buttons
                {
                    optionsShown.Add(obj.options[i]);
                }
                else if (obj.options[i].useableWhenUnitOnTile == false){    // always show buttons which dont require a tile
                    optionsShown.Add(obj.options[i]);
                }
            }
        }
        else if (ogRadialTile.thisTileScript.unitOnTile != null && ogRadialTile.thisTileScript.unitOnTile.playerOwner == ogRadialTile.thisTileScript.gameController.activePlayerID) // if the unit on the tile is owned by the active player show buttons useableByAll
        {
            for(int i = 0; i < obj.options.Length; i++)
            {
                if (obj.options[i].useableByAll == true)
                {
                    optionsShown.Add(obj.options[i]);
                }
            }
        }

        StartCoroutine(AnimateButtons(obj));
    }

    //Coroutine so that the button spawning is animated
    IEnumerator AnimateButtons(RadialTile obj)
    {
        for (int i = 0; i < optionsShown.Count; i++)
        {
            RadialButton newButton = Instantiate(buttonPrefab) as RadialButton;
            newButton.transform.SetParent(transform, false);

            // Calculate position
            float theta = (2 * Mathf.PI / optionsShown.Count) * i;  // The distance around the circle it should be
            float xPos = Mathf.Sin(theta);
            float yPos = Mathf.Cos(theta);

            // Set Position
            newButton.transform.localPosition = new Vector3(xPos, yPos, 0f) * 50f;

            // Set Graphics
            newButton.circle.color = optionsShown[i].colour;
            newButton.icon.sprite = optionsShown[i].sprite;
            newButton.title = optionsShown[i].title;
            newButton.useableByAll = optionsShown[i].useableByAll;

            // Set References
            newButton.menu = this;

            // Animate Button Scale
            newButton.Animate();

            // Wait so that each button appears slightly later than last
            yield return new WaitForSeconds(0.06f);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if (selected != null)
            {
                if (ogRadialTile.thisTileScript.owner.playerID == ogRadialTile.thisTileScript.gameController.activePlayerID)    // if the tile is owned by teh active player output triggers for all buttons
                {
                    ogRadialTile.thisTileScript.gameController.CheckTriggers(IsRadialButtonSelected: true, RadialButtonSelectedTitle: selected.title, RadialMenuTile: ogRadialTile.thisTileScript);
                }
                if(ogRadialTile.thisTileScript.unitOnTile != null && ogRadialTile.thisTileScript.unitOnTile.playerOwner == ogRadialTile.thisTileScript.gameController.activePlayerID) // if the unit on the tile is owned by the active player output triggers for all buttons useableByAll
                { 
                    if(selected.useableByAll == true)
                    {
                        ogRadialTile.thisTileScript.gameController.CheckTriggers(IsRadialButtonSelected: true, RadialButtonSelectedTitle: selected.title, RadialMenuTile: ogRadialTile.thisTileScript);
                    }
                }
            }

            Destroy(gameObject);
        }
    }
}
