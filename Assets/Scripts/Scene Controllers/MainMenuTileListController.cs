using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class MainMenuTileListController : MonoBehaviour
{
    // Main Tile Arrays/Lists
    public DataTable allTiles;  // Set length to amount of tiles to be added
    public DataTable deckList;
    private DataTable collection;
    private Dictionary<int, GameObject> tile_prefabs = new Dictionary<int, GameObject>(); // key is tile id (from first 4 chars of gameobject name), should be same as in database

    // Collection UI
    public GameObject collectionUI;
    private GameObject collectionUIGrid;
    private GameObject[] shownCollectionTiles = new GameObject[12]; //cant have more than 12 total tiles shown
    public GameObject tileInfoCollectionPrefab;

    // Deck UI
    public GameObject deckUI;
    private GameObject deckUIGrid;
    private GameObject[] shownDeckTiles = new GameObject[20]; // used for keeping track of gameobjects for deleting
    public GameObject tileInfoDeckPrefab;

    public int playerID = 0;

    //Collection Page Variables
    private int pageMinTile = 1;    // The first page begins with collection tile 1

    // Start is called before the first frame update
    void Start()
    {

        // Get UI grids
        collectionUIGrid = collectionUI.transform.Find("P_Grid").gameObject;
        deckUIGrid = deckUI.transform.Find("P_Grid").gameObject;

        // load all tile prefabs from Resources folder to tile_prefabs dict
        LoadTilePrefabs();

        // Load all tiles and collection data from database

        DatabaseReader dbReader = new DatabaseReader();

        allTiles = dbReader.query("SELECT * FROM tiles_library");

        dbReader.close();

        // Update UI lists after loading

        UpdateCollectionUI();
        UpdateDeckUI();
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

        // Remove old tileinfo prefabs
        foreach (GameObject infoPanel in shownDeckTiles)
        {
            Destroy(infoPanel);
        }
        
        // Add new tileInfo prefabs
        for (int i = 0; i < deckList.Rows.Count; i++)
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

        // Remove old tileinfo prefabs
        foreach (GameObject infoPanel in shownCollectionTiles)
        {
            Destroy(infoPanel);
        }

        // Add new tileInfo prefabs
        int tilesOnPage = collection.Rows.Count;  // calculates how many tiles there are left to show


        if (tilesOnPage > 12)   // If there are more than 12 tiles left to show, show 12
        {
            tilesOnPage = 12;
        }


        for (int i = 0; i < tilesOnPage; i++)
        {
            shownCollectionTiles[i] = Instantiate(tileInfoCollectionPrefab);

            TileInfoUpdater updater = shownCollectionTiles[i].GetComponent<TileInfoUpdater>();

            if (updater == null)
            {
                Debug.LogError("UI Updater could not be found on tile when adding to collection UI: " + shownCollectionTiles[i].name);
            }

            updater.tilePrefab = tile_prefabs[System.Convert.ToInt32(collection.Rows[pageMinTile + i - 1]["id"])];   // Sets the tilePrefab to the correct tile in the deck
            updater.SetTileData(collection.Rows[pageMinTile + i - 1]);
            updater.sceneController = this.gameObject;
            updater.SetupVars();

            updater.UpdateUI();

            shownCollectionTiles[i].transform.SetParent(collectionUIGrid.transform);
            shownCollectionTiles[i].transform.localScale = new Vector3(1f, 1f, 1f); // The prefabs scale up to 1.2851 for some reason, so this scales them back down
        }
    }

    public void RemoveDeckTile(GameObject deckTile)
    {
        DeckTileInfoUpdate updater = deckTile.GetComponent<DeckTileInfoUpdate>();
        if(updater == null)
        {
            Debug.LogError("No tile Info Updater Script Found");
        }

		// // Find tile in deck and remove
        // bool found = false;
        // for (int i = 0; i < 20 && i < deckList.Rows.Count && !found; i++)
        // {

        //     if(tile_prefabs[System.Convert.ToInt32(deckList.Rows[i]["id"])] == )
        //     {
        //         DataRow tiledata = deckList.Rows[i];

                // delete from database
                DatabaseReader db = new DatabaseReader();

                if(playerID == 0) db.nonQuery("DELETE FROM deck_player1 WHERE num = " + updater.GetTileData()["num"]);
                else db.nonQuery("DELETE FROM deck_player2 WHERE num = " + updater.GetTileData()["num"]);

                db.close();

        //         found = true;
        //     }
        // }
        // if (found == false)
        // {
        //     Debug.LogError("Tile not found in deck");
        // }

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
			Debug.Log("Deck is Full");
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