using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //Reference Variables
    public GameController gameController;

    public GameObject tilesParentObject;
    public GameObject unitsParentObject;

    public Text tilesLeftText;

    public GameObject baseTile;
    public GameObject baseTileSnapCube;

    public Text[] resourceUITexts = new Text[6];

    // Control Variables
    public int playerID;

    public Color PlayerUIColour;

    public bool moveSelecting = false;

    // Resource Variables
    // Array order: Wood, Food, Iron, Weapons, Energy, Alloys
    public int[] resourceTotalAmts = new int[6];
    public int[] resourceTurnAmts = new int[6];

    // Game State Variables
    public bool activePlayer = false;

    private GameObject[] allTiles;
    public GameObject[] deckList = new GameObject[20];  // Original Deck Before Drawing

    public GameObject[] hand = new GameObject[5];
    public List<GameObject> gameDeck = new List<GameObject>();  // Cards still in deck, List for random drawing

    private void Awake()
    {
        // Load all Tile prefabs from Resources and sort them by code
        Object[] loaded = Resources.LoadAll("", typeof(GameObject));
        allTiles = new GameObject[loaded.Length];

        int x = 0;
        foreach (GameObject tile in loaded)
        {
            allTiles[x] = tile;
            x++;
        }

        // Load Deck
        deckList = TileDataMethods.LoadDeck(allTiles, playerID, gameLoading:true);

        if (deckList == null)
        {
            Debug.LogError("Attempt To Load Player " + playerID + " Deck Failed");
        }

        // Add Deck to gameDeck List
        foreach(GameObject tile in deckList)
        {
            if(tile != null)
            {
                gameDeck.Add(tile);
            }
        }

        // Set Tiles Left UI Text
        tilesLeftText.text = "Tiles Left: " + gameDeck.Count;
    }

    private void Update()
    {
        for (int i = 0; i < 6; i++) // Check if Resource UI Needs Updating
        {
            string newText;

            if(resourceTurnAmts[i] >= 0)
            {
                newText = resourceTotalAmts[i] + " (+" + resourceTurnAmts[i] + ")";
            }
            else
            {
                newText = resourceTotalAmts[i] + " (" + resourceTurnAmts[i] + ")";
            }

            if (resourceUITexts[i].text != newText)
            {
                resourceUITexts[i].text = newText;
            }
        }
    }

    public void DrawTiles(int Amnt)
    {
        for(int i = 0; i < Amnt; i++)
        {

            int emptyPos = DeckMethods.GetFreeHandPosition(hand);
            if (emptyPos != -1)
            {
                if (gameDeck.Count != 0)
                {
                    if (playerID == 0)
                    {
                        hand[emptyPos] = DeckMethods.InstantiateTile(gameDeck[0], gameController.p0HandSnapCubes[emptyPos], this, parent: tilesParentObject);
                    }
                    else if (playerID == 1)
                    {
                        hand[emptyPos] = DeckMethods.InstantiateTile(gameDeck[0], gameController.p1HandSnapCubes[emptyPos], this, parent: tilesParentObject);
                    }

                    gameDeck.RemoveAt(0);

                    tilesLeftText.text = "Tiles Left: " + gameDeck.Count;

                    Debug.Log(playerID + ": Card Drawn");

                    // Trigger Events if Trigger is Tile Drawn
                    TileMain tileScript = hand[emptyPos].GetComponent<TileMain>();
                    gameController.CheckTriggers(IsTileDrawn: true, TileDrawn: tileScript);
                }
                else
                {
                    Debug.Log(playerID + "'s Deck is Empty, Couldn't Draw");
                }
            }
            else
            {
                Debug.Log(playerID + "'s Hand is Full, Couldn't Draw");
                break;
            }
        }
    }

    public void UnitMove(TileMain unitsTile)
    {
        StartCoroutine(UnitMoveCoroutine(unitsTile));
    }

    IEnumerator UnitMoveCoroutine(TileMain unitsTile)
    {
        TileMain targetTile = null;

        if (unitsTile.unitOnTile != null)
        {
            moveSelecting = true;
            bool failed = false;

            // Check unit's Speed Points
            if(unitsTile.unitOnTile.speedPoints <= 0)
            {
                Debug.Log("Unit doesn't have a Speed Point Left");
                moveSelecting = false;
                failed = true;
            }

            // Change Cursor
            Cursor.SetCursor(gameController.moveUnitCursorTexture, gameController.hotSpot, gameController.cursorMode);

            // Get click
            while (moveSelecting)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, 100))
                    {
                        targetTile = hit.transform.gameObject.GetComponent<TileMain>();
                        if (targetTile != null)
                        {
                            if (targetTile.locked)
                            {
                                moveSelecting = false;
                                yield return null;
                            }
                            else
                            {
                                Debug.Log("Didnt Click on Locked Board Tile");
                                moveSelecting = false;
                                failed = true;
                                yield return null;
                            }

                        }
                        else
                        {
                            Debug.Log("Didnt Click on Tile / Couldnt Get TileMain");
                            moveSelecting = false;
                            failed = true;
                            yield return null;
                        }
                    }
                    else
                    {
                        Debug.Log("Didnt Find Object");
                        moveSelecting = false;
                        failed = true;
                        yield return null;
                    }

                }
                else
                {
                    yield return null;
                }
            }
            // Change cursor to default
            Cursor.SetCursor(null, Vector2.zero, gameController.cursorMode);

            if (targetTile != null)
            {
                // Do Move
                if (!failed && targetTile.unitOnTile == null && !targetTile.isBaseTile)
                {
                    bool nextTo = false;
                    foreach (GameObject tile in unitsTile.GetAdjacentTiles(unitsTile.snappedTo))
                    {
                        if (tile != null)
                        {
                            TileMain tileScript = tile.GetComponent<TileMain>();
                            if (tileScript == targetTile)
                            {
                                nextTo = true;
                                break;
                            }
                        }
                    }

                    if (nextTo)
                    {
                        targetTile.unitOnTile = unitsTile.unitOnTile;
                        unitsTile.unitOnTile = null;

                        targetTile.unitOnTile.transform.position = targetTile.transform.position;

                        targetTile.unitOnTile.speedPoints -= 1;
                    }
                    else
                    {
                        Debug.Log("Target Tile is not adjacent to Unit's Tile");
                    }
                }
                //Do attack unit if moving onto tile with an enemy unit on
                else if (!failed && targetTile.unitOnTile != null && targetTile.unitOnTile.playerOwner != playerID)
                {
                    Debug.Log("Attacking Enemy Unit");

                    bool nextTo = false;
                    foreach (GameObject tile in unitsTile.GetAdjacentTiles(unitsTile.snappedTo))
                    {
                        if (tile != null)
                        {
                            TileMain tileScript = tile.GetComponent<TileMain>();
                            if (tileScript == targetTile)
                            {
                                nextTo = true;
                                break;
                            }
                        }
                    }

                    if (nextTo)
                    {
                        UnitMain attackingUnit = unitsTile.unitOnTile;
                        UnitMain defendingUnit = targetTile.unitOnTile;

                        defendingUnit.healthPoints -= attackingUnit.atkStrength;

                        attackingUnit.healthPoints -= attackingUnit.atkStrength;

                        attackingUnit.speedPoints -= 1;

                        unitsTile.unitOnTile.speedPoints -= 1;
                    }
                    else
                    {
                        Debug.Log("Target Unit is not adjacent to Unit's Tile");
                    }
                }
                // Do attack base tile if it is the enemies base tile
                else if (targetTile.isBaseTile && targetTile.owner != this)
                {
                    BaseTile baseTileScript = targetTile.gameObject.GetComponent<BaseTile>();

                    baseTileScript.Damage(unitsTile.unitOnTile.atkStrength);
                    unitsTile.unitOnTile.speedPoints -= 1;
                }
                else if (targetTile.isBaseTile && targetTile.owner == this)
                {
                    Debug.Log("Can't attack friendly Base Tile");
                }
            }
        }
    }
}
