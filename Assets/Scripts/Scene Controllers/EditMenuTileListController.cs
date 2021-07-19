using System.Collections.Generic;
using UnityEngine;
using System.Data;
using UnityEngine.UI;

public class EditMenuTileListController : MonoBehaviour
{
    // Prev & Next Buttons for Stopping Interactions when required
    public GameObject prevButton;
    public GameObject nextButton;

    private Button prevButtonComponent;
    private Button nextButtonComponent;

    // Main Tile Arrays/Lists    
    public DataTable allTiles;
    public DataTable deckList;
    private DataTable collection;
    private Dictionary<int, GameObject> tile_prefabs = new Dictionary<int, GameObject>(); // key is tile id (from first 4 chars of gameobject name), should be same as in database

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

    // Deck UI
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

        // load all tile prefabs from Resources folder to tile_prefabs dict
        LoadTilePrefabs();

         // Load all tiles and collection data from database
        DatabaseReader dbReader = new DatabaseReader();

        allTiles = dbReader.query("SELECT * FROM tiles_library");

        dbReader.close();

        // load collection 
        loadCollection();

        // Set prev and next page buttons' interactability (always starts on first page)
        prevButtonComponent.interactable = false;

        // Allow next button interactability if more pages are needed
        if (collection.Rows.Count > pageMaxTile)
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
        if (collection.Rows.Count > pageMaxTile) // Only show pages needed, dont show empty pages
        {
            collectionPage++;
            pageMaxTile = pageMaxTile + 28;
            pageMinTile = pageMinTile + 28;

            UpdateCollectionUI();
        }

        // Allow button interactability if more pages are needed
        if (collection.Rows.Count > pageMaxTile)
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

    private void LoadTilePrefabs() {
        Object[] loaded = Resources.LoadAll("", typeof(GameObject));

        foreach (GameObject tile in loaded)
        {

            int id;
            bool success = int.TryParse(tile.gameObject.name.Substring(0, 4), out id);

            if (!success) {
                Debug.LogError("Failed to parse tile ID from prefab name");
            }

            tile_prefabs.Add(id, tile);
        }
    }


    public void UpdateDeckUI()
    {
        // reload deck incase of change
        loadDeck(playerID);

        // remove old tileinfo prefabs
        foreach (GameObject infoPanel in shownDeckTiles)
        {
            Destroy(infoPanel);
        }

        // Add new tileInfo prefabs
        for (int i = 0; i < deckList.Rows.Count; i++)   // deckList.Length should be 20, bu this saves me time if I change it in the future
        {
            shownDeckTiles[i] = Instantiate(tileInfoDeckPrefab);

            DeckTileInfoUpdate updater = shownDeckTiles[i].GetComponent<DeckTileInfoUpdate>();

            if (updater == null)
            {
                Debug.LogError("UI Updater could not be found on tile when adding to deck UI: " + shownDeckTiles[i].name);
            }

            updater.tilePrefab = tile_prefabs[System.Convert.ToInt32(deckList.Rows[i]["id"])];   // Sets the tilePrefab to the correct tile in the deck
            updater.SetTileData(deckList.Rows[i]);
            updater.sceneController = this.gameObject;
            updater.SetupVars();

            updater.UpdateUI();

            shownDeckTiles[i].transform.SetParent(deckUIGrid.transform);
            shownDeckTiles[i].transform.localScale = new Vector3(1f, 1f, 1f); // The prefabs scale up to 1.2851, so this scales them back down
        }
    }

    private void UpdateCollectionUI()
    {
        // reload collection
        loadCollection();

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
        int tilesOnPage = collection.Rows.Count - (12 + 28 * (collectionPage - 2));  // calculates how many tiles there are left to show

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

            updater.tilePrefab = tile_prefabs[System.Convert.ToInt32(collection.Rows[pageMinTile + i - 1]["id"])];   // Sets the tilePrefab to the correct tile in the deck
            updater.SetTileData(collection.Rows[pageMinTile + i - 1]);
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

            updater.tilePrefab = tile_prefabs[System.Convert.ToInt32(collection.Rows[pageMinTile + 16 + j - 1]["id"])];   // Sets the tilePrefab to the correct tile in the deck
            updater.SetTileData(collection.Rows[pageMinTile + 16 + j - 1]);
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

        // delete from database
        DatabaseReader db = new DatabaseReader();

        if(playerID == 0) db.nonQuery("DELETE FROM deck_player1 WHERE num = " + updater.GetTileData()["num"]);
        else db.nonQuery("DELETE FROM deck_player2 WHERE num = " + updater.GetTileData()["num"]);

        db.close();

        UpdateDeckUI();
    }

	public void AddDeckTile(GameObject tile)
	{
		TileInfoUpdater updater = tile.GetComponent<TileInfoUpdater>();
		if(updater == null)
		{
			Debug.LogError("No tile Info Updater Script Found");
		}

        if(deckList.Rows.Count < 20)
        {
            // Add to database
            DatabaseReader db = new DatabaseReader();

            if(playerID == 0) db.nonQuery("INSERT INTO deck_player1 (tile_id) VALUES (" + updater.GetTileData()["id"] + ")");
            else  db.nonQuery("INSERT INTO deck_player2 (tile_id) VALUES (" + updater.GetTileData()["id"] + ")");

            db.close();
        }
        else {
			Debug.LogError("Deck is Full");
		}

		UpdateDeckUI();
	}

        private void loadDeck(int playerID) {
            // Read deck from database
            DatabaseReader db = new DatabaseReader();

            if(playerID == 0) deckList = db.query("SELECT deck_player1.num, tiles_library.* FROM tiles_library INNER JOIN deck_player1 ON tiles_library.id = deck_player1.tile_id");
            else deckList = db.query("SELECT deck_player2.num, tiles_library.* FROM tiles_library INNER JOIN deck_player2 ON tiles_library.id = deck_player2.tile_id");

            db.close();
    }

        private void loadCollection() {
            // Read collection from database
            DatabaseReader db = new DatabaseReader();

            collection = db.query("SELECT tiles_library.* FROM tiles_library INNER JOIN collection ON tiles_library.id = collection.tile_id");

            db.close();
    }
}