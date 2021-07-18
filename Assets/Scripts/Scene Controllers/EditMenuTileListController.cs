using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditMenuTileListController : MonoBehaviour
{
    // Prev & Next Buttons for Stopping Interactions when required
    public GameObject prevButton;
    public GameObject nextButton;

    private Button prevButtonComponent;
    private Button nextButtonComponent;

    // Main Tile Arrays/Lists
    public GameObject[] allTiles;
    public GameObject[] deckList = new GameObject[20];
    public List<GameObject> collection;

    // Collection UI Lists
    public GameObject collectionLeftUI;
    public GameObject collectionRightUI;

    private GameObject collectionLeftUIGrid;
    private GameObject collectionRightUIGrid;

    // Collection Page Variables
    private int collectionPage = 2;  // Page 1 is Main menu
    private int pageMinTile = 13;    // The last page shows 12 tiles, 13th tile is first shown
    private int pageMaxTile = 40;    // This page shows 28 tiles, + the 12 from last page

    private GameObject[] shownLeftCollectionTiles = new GameObject[16]; //cant have more than 28 total tiles shown
    private GameObject[] shownRightCollectionTiles = new GameObject[12];

    public GameObject tileInfoPrefab;

    // !!! Deck UI
    public GameObject deckUI;

    private GameObject deckUIGrid;

    private GameObject[] shownDeckTiles = new GameObject[20]; // used for keeping track of gameobjects for deleting

    public GameObject tileInfoDeckPrefab;

    public int playerID = 0;

    // Start is called before the first frame update
    void Start()
    {
        // Get Button components
        prevButtonComponent = prevButton.GetComponent<Button>();
        nextButtonComponent = nextButton.GetComponent<Button>();

        // Get UI grids
        collectionLeftUIGrid = collectionLeftUI.transform.Find("P_Grid").gameObject; // Gets the child grid of the collectionList using the trnansform as they are less resource intensive
        collectionRightUIGrid = collectionRightUI.transform.Find("P_Grid").gameObject;

        deckUIGrid = deckUI.transform.Find("P_Grid").gameObject;

        // Load all Tile prefabs from Resources and sort them by code
        Object[] loaded = Resources.LoadAll("", typeof(GameObject));
        allTiles = new GameObject[loaded.Length];

        int x = 0;
        foreach (GameObject tile in loaded)
        {
            allTiles[x] = tile;
            x++;
        }

        // load deck from file, try twice, if faled first time, attempt to create an empty deck file
        deckList = TileDataMethods.LoadDeck(allTiles, playerID);

        if(deckList == null)
        {
            deckList = TileDataMethods.LoadDeck(allTiles, playerID); // Attempt a second try, as LoadDeck should have created a new save file
            if(deckList == null)
            {
                Debug.LogError("Second Attempt To Load Deck Failed");
            }
        }

        // load collection from file, try twice, if faled first time, attempt to create an empty collection file

        collection = TileDataMethods.LoadCollection(allTiles);

        if (collection == null)
        {
            collection = TileDataMethods.LoadCollection(allTiles); // Attempt a second try, as LoadCollection should have created a new save file
            if (collection == null)
            {
                Debug.LogError("Second Attempt To Load Collection Failed");
            }
        }

        // Set prev and next page buttons' interactability (always starts on first page)
        prevButtonComponent.interactable = false;

        // Allow next button interactability if more pages are needed
        if (collection.Count > pageMaxTile)
        {
            nextButtonComponent.interactable = true;
        }
        else // otherwise stop next button interactability as on last required page
        {
            nextButtonComponent.interactable = false;
        }

        // Updater UI Lists after loading
        UpdateCollectionUI();
        UpdateDeckUI();
    }

    public void NextPage()
    {
        if (collection.Count > pageMaxTile) // Only show pages needed, dont show empty pages
        {
            collectionPage++;
            pageMaxTile = pageMaxTile + 28;
            pageMinTile = pageMinTile + 28;

            UpdateCollectionUI();
        }

        // Allow button interactability if more pages are needed
        if (collection.Count > pageMaxTile)
        {
            nextButtonComponent.interactable = true;
        }
        else // otherwise stop button interactability as on last required page
        {
            nextButtonComponent.interactable = false;
        }
    }

    public void PrevPage()
    {
        if (collectionPage > 2) // Dont show the first page, athst the main menu
        {
            collectionPage--;
            pageMaxTile = pageMaxTile - 28;
            pageMinTile = pageMinTile - 28;

            UpdateCollectionUI();
        }

        // Allow button interactability if on pages other than the first (editDeck scene) page
        if (collectionPage > 2)
        {
            nextButtonComponent.interactable = true;
        }

        // Stop button interactability if on first (editDeck scene) page
        if (collectionPage == 2)
        {
            prevButtonComponent.interactable = false;
        }
    }


    public void UpdateDeckUI()
    {
        // remove old tileinfo prefabs
        foreach (GameObject infoPanel in shownDeckTiles)
        {
            Destroy(infoPanel);
        }

        // Add new tileInfo prefabs
        for (int i = 0; i < deckList.Length; i++)   // deckList.Length should be 20, bu this saves me time if I change it in the future
        {
            if (deckList[i] != null) // check if there is a tile in the element
            {
                shownDeckTiles[i] = Instantiate(tileInfoDeckPrefab);

                DeckTileInfoUpdate updater = shownDeckTiles[i].GetComponent<DeckTileInfoUpdate>();

                if (updater == null)
                {
                    Debug.LogError("UI Updater could not be found on tile when adding to deck UI: " + shownDeckTiles[i].name);
                }

                updater.tilePrefab = deckList[i];   // Sets the tilePrefab to the correct tile in the collection
                updater.sceneController = this.gameObject;
                updater.SetupVars();

                updater.UpdateUI();

                shownDeckTiles[i].transform.SetParent(deckUIGrid.transform);
                shownDeckTiles[i].transform.localScale = new Vector3(1f, 1f, 1f); // The prefabs scale up to 1.2851, so this scales them back down

            }
        }
    }

    private void UpdateCollectionUI()
    {

        // Remove old tileinfos
        foreach(GameObject infoPanel in shownLeftCollectionTiles)
        {
            Destroy(infoPanel);
        }

        foreach (GameObject infoPanel in shownRightCollectionTiles)
        {
            Destroy(infoPanel);
        }

        // Add new tileInfos
        int tilesOnPage = collection.Count - (12 + 28 * (collectionPage - 2));  // calculates how many tiles there are left to show

        if (tilesOnPage > 28)   // If there are more than 28 tiles left to show
        {
            tilesOnPage = 28;
        }

        int tilesOnLeft;
        int tilesOnRight;

        if (tilesOnPage > 16) // ie left list filled
        {
            tilesOnLeft = 16;
            tilesOnRight = tilesOnPage - 16;
        } else // ie right side is empty
        {
            tilesOnLeft = tilesOnPage;
            tilesOnRight = 0;
        }


        for (int i = 0; i < tilesOnLeft; i++)
        {
            shownLeftCollectionTiles[i] = Instantiate(tileInfoPrefab);

            TileInfoUpdater updater = shownLeftCollectionTiles[i].GetComponent<TileInfoUpdater>();

            if (updater == null)
            {
                Debug.LogError("UI Updater could not be found on tile when adding to colleciton UI: " + shownLeftCollectionTiles[i].name);
            }

            updater.tilePrefab = collection[pageMinTile + i - 1];   // Sets the tilePrefab to the correct tile in the collection
            updater.sceneController = this.gameObject;
            updater.SetupVars();

            updater.UpdateUI();

            shownLeftCollectionTiles[i].transform.SetParent(collectionLeftUIGrid.transform);
            shownLeftCollectionTiles[i].transform.localScale = new Vector3(1f, 1f, 1f); // The prefabs scale up to 1.2851, so thi scales them back down
        }

        for (int j = 0; j < tilesOnRight; j++)
        {
            shownRightCollectionTiles[j] = Instantiate(tileInfoPrefab);

            TileInfoUpdater updater = shownRightCollectionTiles[j].GetComponent<TileInfoUpdater>();

            if (updater == null)
            {
                Debug.LogError("UI Updater could not be found on tile when adding to colleciton UI: " + shownLeftCollectionTiles[j].name);
            }

            updater.tilePrefab = collection[pageMinTile + 16 + j - 1];   // Sets the tilePrefab to the correct tile in the collection
            updater.sceneController = this.gameObject;
            updater.SetupVars();

            updater.UpdateUI();

            shownRightCollectionTiles[j].transform.SetParent(collectionRightUIGrid.transform);
            shownRightCollectionTiles[j].transform.localScale = new Vector3(1f, 1f, 1f); // The prefabs scale up to 1.2851, so thi scales them back down
        }
    }

    public void RemoveDeckTile(GameObject tile)
    {
        DeckTileInfoUpdate updater = tile.GetComponent<DeckTileInfoUpdate>();
        if (updater == null)
        {
            Debug.LogError("No tile Info Updater Script Found");
        }

        bool found = false;
        for (int i = 0; i < 20; i++)
        {
            if (deckList[i] == updater.tilePrefab)
            {
                deckList[i] = null;
                found = true;
                break;
            }
        }
        if (found == false)
        {
            Debug.LogError("Tile not found in deck");
        }

        // Save and Update
        TileDataMethods.SaveDeck(deckList, playerID);
        UpdateDeckUI();
    }

	public void AddDeckTile(GameObject tile)
	{
		TileInfoUpdater updater = tile.GetComponent<TileInfoUpdater>();
		if(updater == null)
		{
			Debug.LogError("No tile Info Updater Script Found");
		}

		// Find Empty Spot
		bool found = false;
		for (int i = 0; i < 20; i++)
		{
			if(deckList[i] == null)
			{
				deckList[i] = updater.tilePrefab;
				found = true;
				break;
			}
		}

		// If Full
		if (found == false)
		{
			Debug.LogError("Deck is Full");
		}

		// Save and Update
		TileDataMethods.SaveDeck(deckList,playerID);
		UpdateDeckUI();
	}
}