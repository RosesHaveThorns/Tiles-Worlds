using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class EditMenuController : MonoBehaviour
{
    public string mainMenuSceneName;

    public EditMenuTileListController tileController;

    public Text playerIDText;

    public void OnMainMenuReturnClick()
    {
        SceneManager.LoadScene(mainMenuSceneName);  //Change to Main Menu Scene
    }

    public void OnSwapDeckClick()
    {
        if (tileController.playerID == 0)
        {
            tileController.playerID = 1;
        }
        else if(tileController.playerID == 1)
        {
            tileController.playerID = 0;
        }
        else
        {
            Debug.LogError("Unexpected player ID in tile controller");
        }

        tileController.deckList = TileDataMethods.LoadDeck(tileController.allTiles, tileController.playerID);
        tileController.UpdateDeckUI();

        playerIDText.text = "Player: " + tileController.playerID;
    }
}
