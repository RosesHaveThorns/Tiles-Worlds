using System.Collections.Generic;
using UnityEngine;

public class MainMenuTileListController : MonoBehaviour
{
    // Main Tile Arrays/Lists
    public GameObject[] allTiles;  // Set length to amount of tiles to be added
    public GameObject[] deckList = new GameObject[20];
    public List<GameObject> collection;

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
    private int pageMinTile = 1;    // The first page begisn with collection tile 1

    // Start is called before the first frame update
    void Start()
    {
        // Get UI grids
        collectionUIGrid = collectionUI.transform.Find("P_Grid").gameObject;
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

        allTiles = TileDataMethods.GameObjectArrayInsertSort(allTiles);

        // load deck from file, try twice, if faled first time, attempt to create an empty deck file
        deckList = TileDataMethods.LoadDeck(allTiles, playerID);

        if (deckList == null)
        {
            deckList = TileDataMethods.LoadDeck(allTiles, playerID); // Attempt a second try, as LoadDeck should have created a new save file
            if (deckList == null)
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

            // Add tiles for testing when collection is empty
            for (int i = 0; i < allTiles.Length; i++)
            {
                collection.Add(allTiles[i]);
            }

            TileDataMethods.SaveCollection(collection);
        }

        // Update UI lists after loading

        UpdateCollectionUI();
        UpdateDeckUI();

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
                updater.SetupControllerVars();

                updater.UpdateUI();

                shownDeckTiles[i].transform.SetParent(deckUIGrid.transform);
                shownDeckTiles[i].transform.localScale = new Vector3(1f, 1f, 1f); // The prefabs scale up to 1.2851, so this scales them back down

            }
        }
    }

    private void UpdateCollectionUI()
    {

        // Remove old tileinfo prefabs
        foreach (GameObject infoPanel in shownCollectionTiles)
        {
            Destroy(infoPanel);
        }

        // Add new tileInfo prefabs
        int tilesOnPage = collection.Count;  // calculates how many tiles there are left to show

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
                Debug.LogError("UI Updater could not be found on tile when adding to colleciton UI: " + shownCollectionTiles[i].name);
            }

            // Setup info prefab variables
            updater.tilePrefab = collection[pageMinTile + i - 1];   // Sets the tilePrefab to the correct tile in the collection
            updater.sceneController = this.gameObject;
            updater.SetupControllerVars();

            updater.UpdateUI();

            shownCollectionTiles[i].transform.SetParent(collectionUIGrid.transform);
            shownCollectionTiles[i].transform.localScale = new Vector3(1f, 1f, 1f); // The prefabs scale up to 1.2851, so thi scales them back down
        }
    }

    public void RemoveDeckTile(GameObject tile)
    {
        DeckTileInfoUpdate updater = tile.GetComponent<DeckTileInfoUpdate>();
        if(updater == null)
        {
            Debug.LogError("No tile Info Updater Script Found");
        }

		// Find tile in deck and remove
        bool found = false;
        for (int i = 0; i < 20; i++)
        {
            if(deckList[i] == updater.tilePrefab)
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
			Debug.Log("Deck is Full");
		}

		// Save and Update
		TileDataMethods.SaveDeck(deckList, playerID);

		UpdateDeckUI();
	}
}