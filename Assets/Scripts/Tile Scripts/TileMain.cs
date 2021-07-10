using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Class all Tiles are instances of
[RequireComponent(typeof(BoxCollider))]
public class TileMain : MonoBehaviour
{
    // REOSURCE ARRAYS
    // Array order: Wood, Food, Iron, Weapons, Energy, Alloys
    public int[] resourceCosts = new int[6];            // Cost When Placed
    public int[] resourceTurnGain = new int[6];         // Gain Per Turn

    // Other variables set for specific Tiles
    public string setName;

    public string tileName;

    public string description;

    public bool isBaseTile;

    public GameObject unitToSpawn;

    // Game State Variables
    public PlayerController owner;

    public bool placed = false;
    public bool updateResourceGain = false;

    public UnitMain unitOnTile;

    // Event and Trigger Variables
    public delegate void eventFunc();
    public delegate bool triggerFunc(bool isTurnEnd = false,
        bool isTileDrawn = false, TileMain tileDrawn = null,
        bool isTileLocked = false, TileMain tileLocked = null,
        bool isRadialButtonSelected = false, string radialButtonSelectedTitle = null, TileMain radialmenuTile = null);

    public List<eventFunc> eventsList = new List<eventFunc>();
    public List<triggerFunc> allTriggersList = new List<triggerFunc>();
    public List<triggerFunc> activeTriggersList = new List<triggerFunc>();

    // Drag Variables
    private bool dragging = false;
    private Vector3 dragOffset = new Vector3(0f, 0f, 0f);

    public bool locked = false; // Stops movement once placed and turn has ended

    public bool nextToTile = false;

    private List<TileMain> checkQueue = new List<TileMain>();

    // Snap Variables
    public GameObject snappedTo = null;
    private GameObject newSnappedTo;
    private Vector3 snapOffset = new Vector3(0.98f,  -0.45f, 0f);

    // References
    public GameController gameController;


    // FUNCTIONS
    public void InheritedStart()
    {
        eventsList.Add(EventMove);
        allTriggersList.Add(TriggerMove);
    }

    public GameObject[] GetAdjacentTiles(GameObject snapCube)
    {
        GameObject[] adjTiles = new GameObject[4];
        snapCubeData scScript = snapCube.GetComponent<snapCubeData>();

        if (snappedTo != null && scScript.isBoardCube)
        {
            if (scScript.boardIndexColumn < 5) {
                GameObject upTile = owner.gameController.boardPlacedTiles[scScript.boardIndexColumn + 1][scScript.boardIndexRow];
                adjTiles[0] = upTile;
            }

            if (scScript.boardIndexColumn > 0) {
                GameObject downTile = owner.gameController.boardPlacedTiles[scScript.boardIndexColumn - 1][scScript.boardIndexRow];
                adjTiles[1] = downTile;
            }


            if (scScript.boardIndexRow < 5) {
                GameObject rightTile = owner.gameController.boardPlacedTiles[scScript.boardIndexColumn][scScript.boardIndexRow + 1];
                adjTiles[2] = rightTile;
            }

            if (scScript.boardIndexRow > 0) {
                GameObject leftTile = owner.gameController.boardPlacedTiles[scScript.boardIndexColumn][scScript.boardIndexRow - 1];
                adjTiles[3] = leftTile;
            }

            return adjTiles;
        }

        return adjTiles;
    }

    public void SnapTo(GameObject snapObj)
    {

        if (snappedTo != snapObj && snappedTo != null)
        {
            snappedTo.SetActive(true);
            snapObj.SetActive(false);

            snappedTo = snapObj;

        }
        else if (snappedTo == null)
        {
            snapObj.SetActive(false);

            snappedTo = snapObj;
        }

        Vector3 newPos = snapObj.transform.position + snapOffset;
        transform.position = newPos;

    }

    private void OnMouseEnter()
    {
        if(unitOnTile != null)
        {
            owner.gameController.UnitInfoUI.Show(this);
        }

        owner.gameController.TileInfoUI.Show(this);

        if (owner.activePlayer || unitOnTile != null && unitOnTile.playerOwner == owner.gameController.activePlayerID)
        {
            if (!owner.gameController.player0.moveSelecting && !owner.gameController.player1.moveSelecting)   // leave cursor as move cursor if a player selecting a move target
            {
                // set to grab mouse icon
                Cursor.SetCursor(gameController.selectableCursorTexture, gameController.hotSpot, gameController.cursorMode);
            }
        }
    }

    private void OnMouseExit()
    {
        owner.gameController.UnitInfoUI.Hide();
        owner.gameController.TileInfoUI.Hide();

        if (owner.activePlayer || unitOnTile != null && unitOnTile.playerOwner == owner.gameController.activePlayerID)
        {
            if (!owner.gameController.player0.moveSelecting && !owner.gameController.player1.moveSelecting) // leave cursor as move cursor if a player is selecting a move target
            {
                Cursor.SetCursor(null, Vector2.zero, gameController.cursorMode);
            }
        }
    }

    void OnMouseDown()
    {
        if (!locked && owner.activePlayer)
        {
            dragging = true;
            placed = false;

            // Check if was placed on board
            snapCubeData scData = snappedTo.GetComponent<snapCubeData>();
            if (scData.isBoardCube)
            {
                // Remove From Placed Tiles 2D Array
                gameController.boardPlacedTiles[scData.boardIndexColumn][scData.boardIndexRow] = null;

                // Check all adjacent tiles will still be connected to the base tile after this one is moved
                GameObject[] adjTiles = GetAdjacentTiles(snappedTo);
                foreach (GameObject tile in adjTiles)
                {
                    if (tile != null)
                    {
                        TileMain adjTileScript = tile.GetComponent<TileMain>();
                        if (!TileDataMethods.checkConnectedToBase(tile, owner) && adjTileScript.owner == owner)
                        {
                            // If not, stop dragging and return to original position
                            gameController.boardPlacedTiles[scData.boardIndexColumn][scData.boardIndexRow] = gameObject;
                            dragging = false;
                            SnapTo(snappedTo);
                            Debug.Log(owner.playerID.ToString() + ": Drag cancelled as adjacent tiles would be disconnected");
                        }
                    }
                }

                // refund resource costs
                for (int i = 0; i < 6; i++)
                {
                    owner.resourceTotalAmts[i] += resourceCosts[i];
                }
            }
            // Check if was placed in hand
            else if (scData.isHandCube)
            {
                owner.hand[scData.handIndex] = null;
            }

            // Get Drag Offset
            Plane plane = new Plane(Vector3.up, new Vector3(0, -1, 0));
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            float distance;
            if (plane.Raycast(ray, out distance))
            {
                Vector3 startRayPos = ray.GetPoint(distance);

                dragOffset = transform.position - startRayPos;
            }
        }
    }

    void OnMouseUp()
    {
        if (dragging && !locked && owner.activePlayer)
        {
            dragging = false;

            SnapTo(newSnappedTo);

            // Check if placed on board
            snapCubeData scData = snappedTo.GetComponent<snapCubeData>();
            if (scData.isBoardCube)
            {
                placed = true;

                // Add To Placed Tiles 2D Array
                gameController.boardPlacedTiles[scData.boardIndexColumn][scData.boardIndexRow] = gameObject;

                // pay resource costs
                for (int i = 0; i < 6; i++)
                {
                    owner.resourceTotalAmts[i] -= resourceCosts[i];
                }
            }
            // Check if placed on Tile
            else if (scData.isHandCube)
            {
                owner.hand[scData.handIndex] = gameObject;
            }
        }
    }

    void OnMouseDrag()
    {
        if (dragging && !locked && owner.activePlayer)
        {
            // Set Drag Position using raycast from screen
            Plane plane = new Plane(Vector3.up, new Vector3(0, -1, 0));
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float distance;

            if (plane.Raycast(ray, out distance))
            {
                Vector3 rayPos = ray.GetPoint(distance);

                Vector3 tilePos = rayPos + dragOffset;
                transform.position = tilePos;
            }

            // Check if on SnapCube using a raycast, if so snap to it
            int layerMask = 1 << 9; // Only check if Ray has hit a SnapCube (i.e. Layer 9)
            RaycastHit hit;

            if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity, layerMask))
            {
                // Check if hit is a board SC or or of the owner's hand's SCs
                snapCubeData scData = hit.transform.gameObject.GetComponent<snapCubeData>();
                bool snappable = true;

                if (scData == null)
                {
                    Debug.LogError("No snapCubeData Script found on Snap Cube");
                }

                if (scData.isHandCube)
                {
                    if (scData.handPlayerID != owner.playerID) // Check it is not the owners hand
                    {
                        snappable = false;
                    }
                }

                //Check if player can afford to place tile
                if (owner.resourceTotalAmts[0] < resourceCosts[0] || owner.resourceTotalAmts[1] < resourceCosts[1] || owner.resourceTotalAmts[2] < resourceCosts[2] || owner.resourceTotalAmts[3] < resourceCosts[3] || owner.resourceTotalAmts[4] < resourceCosts[4] || owner.resourceTotalAmts[5] < resourceCosts[5])
                {
                    snappable = false;
                }

                // Set new Snap Location to the SnapCube if Not already snapped & can snap to it
                if (newSnappedTo != hit.transform.gameObject && snappable)
                {
                    if (scData.isBoardCube)
                    {
                        // Check If Placed Position is Next To Another Tile
                        GameObject[] adjTiles = GetAdjacentTiles(hit.transform.gameObject);
                        for (int i = 0; i < 4; i++)
                        {
                            if (adjTiles[i] != null) // Check if That Tile is Owned By The Same Player
                            {
                                TileMain tileScript = adjTiles[i].GetComponent<TileMain>();
                                if (tileScript.owner == owner)
                                {
                                    newSnappedTo = hit.transform.gameObject;
                                }
                            }
                        }
                    }
                    else
                    {
                        newSnappedTo = hit.transform.gameObject;
                    }

                }
            }
            else
            {
                if (newSnappedTo != snappedTo)
                {
                    newSnappedTo = snappedTo;   // Return to orginal snapCube if not over a new snappable snapCube
                }
            }
        }
    }

    public void SpawnUnit(GameObject unitPrefab)
    {
        if (unitOnTile != null)
        {
            Debug.Log("Couldnt Spawn Unit: Unit on Tile");
            return;
        }
        GameObject newUnit = Instantiate(unitPrefab);
        UnitMain unitScript = newUnit.GetComponent<UnitMain>();

        if(unitScript == null)
        {
            Debug.LogError("No Unit Script Found on Prefab");
        }

        unitOnTile = unitScript;

        newUnit.transform.parent = owner.unitsParentObject.transform;
        newUnit.transform.position = transform.position;
        unitOnTile.playerOwner = owner.playerID;
    }


    // VIRTUAL FUNCTIONS (Event and Trigger Functions)

    // MOVE/ATTACK FUNCTION
    public virtual bool TriggerMove(bool isTurnEnd = false,
        bool isTileDrawn = false, TileMain tileDrawn = null,
        bool isTileLocked = false, TileMain tileLocked = null,
        bool isRadialButtonSelected = false, string radialButtonSelectedTitle = null, TileMain radialmenuTile = null)
    {
        if (isRadialButtonSelected && radialmenuTile == this && radialButtonSelectedTitle == "move")
        {
            return true;
        }
        return false;
    }

    public virtual void EventMove()
    {
        owner.UnitMove(this);
    }

    public virtual void Event0()
    {
    }

    // All given variables are defaulted to null/false, and are checked by triggers as required to check for specific trigger events
    // KEEP THESE VARIABLES AND THE ONES PASSED TO "GAMECONTROLLER.CHECKTRIGGERS()" THE SAME
    public virtual bool Trigger0(bool isTurnEnd= false,
        bool isTileDrawn = false, TileMain tileDrawn = null,
        bool isTileLocked = false, TileMain tileLocked = null,
        bool isRadialButtonSelected = false, string radialButtonSelectedTitle = null, TileMain radialmenuTile = null)
    {
        return false;
    }
}
