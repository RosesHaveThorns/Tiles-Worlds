using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public string gameSceneName;
    public string editSceneName;

    public MainMenuTileListController tileController;

    public Text playerIDText;

    // Button Click Procedures
    public void On2PlayerClick()
    {
        SceneManager.LoadScene(gameSceneName);  // Change to Game Scene
    }

    public void OnNextClick()
    {
        SceneManager.LoadScene(editSceneName);  //Change to Edit Deck Scene
    }

    public void OnSwapDeckClick()
    {
        if (tileController.playerID == 0)
        {
            tileController.playerID = 1;
        }
        else if (tileController.playerID == 1)
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
