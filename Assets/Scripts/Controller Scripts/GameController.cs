using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{

    // Reference Variables
    public GameObject p0ActiveImg;
    public GameObject p1ActiveImg;

    public PlayerController player0;
    public PlayerController player1;

    public GameObject[] p0HandSnapCubes = new GameObject[5];
    public GameObject[] p1HandSnapCubes = new GameObject[5];

    public GameObject[] row0SnapCubes = new GameObject[6];
    public GameObject[] row1SnapCubes = new GameObject[6];
    public GameObject[] row2SnapCubes = new GameObject[6];
    public GameObject[] row3SnapCubes = new GameObject[6];
    public GameObject[] row4SnapCubes = new GameObject[6];
    public GameObject[] row5SnapCubes = new GameObject[6];

    public GameObject[][] boardSnapCubes = new GameObject[6][];

    public UnitInfoUIUpdater UnitInfoUI;
    public TileInfoUIUpdater TileInfoUI;

    public GameObject gameOverUIPanel;
    public Text gameOverUIWinnerText;

    public string mainMenuSceneName;

    // Cursor Variables
    public Texture2D moveUnitCursorTexture;
    public Texture2D attackUnitCursorTexture;
    public Texture2D selectableCursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    // Game State Variables
    public int activePlayerID = 0;
    public int begginingPlayerID = 0;

    private int turn = 1;

    // 2D Array Storing Tiles and Where They Are Placed
    public GameObject[] row0PlacedTiles = new GameObject[6];
    public GameObject[] row1PlacedTiles = new GameObject[6];
    public GameObject[] row2PlacedTiles = new GameObject[6];
    public GameObject[] row3PlacedTiles = new GameObject[6];
    public GameObject[] row4PlacedTiles = new GameObject[6];
    public GameObject[] row5PlacedTiles = new GameObject[6];

    public GameObject[][] boardPlacedTiles = new GameObject[6][];

    // Start is called before the first frame update
    void Start()
    {
        gameOverUIPanel.SetActive(false);

        // Setup Board Snap Cubes 2D Array    (Column, Row)
        boardSnapCubes[0] = row0SnapCubes;
        boardSnapCubes[1] = row1SnapCubes;
        boardSnapCubes[2] = row2SnapCubes;
        boardSnapCubes[3] = row3SnapCubes;
        boardSnapCubes[4] = row4SnapCubes;
        boardSnapCubes[5] = row5SnapCubes;

        // Setup Board Placed Tiles 2D Array  (Column, Row)
        boardPlacedTiles[0] = row0PlacedTiles;
        boardPlacedTiles[1] = row1PlacedTiles;
        boardPlacedTiles[2] = row2PlacedTiles;
        boardPlacedTiles[3] = row3PlacedTiles;
        boardPlacedTiles[4] = row4PlacedTiles;
        boardPlacedTiles[5] = row5PlacedTiles;

        // Begin Game
        BeginGame();
    }

    private void BeginGame()
    {
        // Setup BaseTiles
        TileMain baseTileScript;
        snapCubeData scScript;

        baseTileScript = player0.baseTile.GetComponent<TileMain>();
        baseTileScript.SnapTo(player0.baseTileSnapCube);
        scScript = baseTileScript.snappedTo.GetComponent<snapCubeData>();
        boardPlacedTiles[scScript.boardIndexColumn][scScript.boardIndexRow] = baseTileScript.gameObject;

        baseTileScript = player1.baseTile.GetComponent<TileMain>();
        baseTileScript.SnapTo(player1.baseTileSnapCube);
        scScript = baseTileScript.snappedTo.GetComponent<snapCubeData>();
        boardPlacedTiles[scScript.boardIndexColumn][scScript.boardIndexRow] = baseTileScript.gameObject;

        // Shuffle Decks
        player0.gameDeck = DeckMethods.ShuffleDeck(player0.gameDeck);
        player1.gameDeck = DeckMethods.ShuffleDeck(player1.gameDeck);

        // Select Starting Player
        System.Random random = new System.Random();

        activePlayerID = random.Next(0, 2);
        SetPlayerActiveVars();

        begginingPlayerID = activePlayerID;

        // Draw 2 Tiles For Each Player
        player0.DrawTiles(2);
        player1.DrawTiles(2);
    }

    private void EndGame(int winner)
    {
        gameOverUIWinnerText.text = (winner + 1).ToString();

        gameOverUIPanel.SetActive(true);
    }

    public void ExitGame()
    {
        SceneManager.LoadScene(mainMenuSceneName);  //Change to Main Menu Scene
    }

    private void SetPlayerActiveVars()  // Updates PlayerData scripts and UI with who is currently active
    {
        if(activePlayerID == 0)
        {
            player0.activePlayer = true;
            player1.activePlayer = false;

            // Update UI
            p0ActiveImg.SetActive(true);
            p1ActiveImg.SetActive(false);
        }
        else if (activePlayerID == 1)
        {
            player1.activePlayer = true;
            player0.activePlayer = false;

            // Update UI
            p1ActiveImg.SetActive(true);
            p0ActiveImg.SetActive(false);
        }
        else
        {
            Debug.LogError("Unexpected active player ID. Expected 0 or 1, got " + activePlayerID);
        }
    }

    // TRIGGER/EVENT FUCNTIONS - KEEP VARIBALES PASSED UP TO DATE
    public void CheckTriggers(bool IsTurnEnd = false, 
        bool IsTileDrawn = false, TileMain TileDrawn = null, 
        bool IsTileLocked = false, TileMain TileLocked = null, 
        bool IsRadialButtonSelected = false, string RadialButtonSelectedTitle = null, TileMain RadialMenuTile = null)
    {
        for (int i = 0; i < 6; i++)
        {
            foreach (GameObject tile in boardPlacedTiles[i])
            {
                if (tile != null)
                {
                    TileMain tileScript = tile.GetComponent<TileMain>();

                    for(int triggerNum = 0; triggerNum < tileScript.activeTriggersList.Count; triggerNum++)
                    {
                        bool triggered = tileScript.activeTriggersList[triggerNum](isTurnEnd: IsTurnEnd, 
                            isTileDrawn: IsTileDrawn, tileDrawn: TileDrawn, 
                            isTileLocked: IsTileLocked, tileLocked: TileLocked,
                            isRadialButtonSelected: IsRadialButtonSelected, radialButtonSelectedTitle: RadialButtonSelectedTitle, radialmenuTile: RadialMenuTile);

                        if (triggered)
                        {
                            tileScript.eventsList[triggerNum]();
                        }
                    }
                }
            }
        }
    }

    public void ResetActiveTriggerList(int playerID)
    {
        if (playerID == 0)
        {
            for (int i = 0; i < 6; i++)
            {
                foreach (GameObject tile in boardPlacedTiles[i])
                {
                    if (tile != null)
                    {
                        TileMain tileScript = tile.GetComponent<TileMain>();

                        if (tileScript.owner == player0)
                        {
                            tileScript.activeTriggersList = tileScript.allTriggersList;
                        }
                    }
                }
            }
        }
        else if (playerID == 1)
        {
            for (int i = 0; i < 6; i++)
            {
                foreach (GameObject tile in boardPlacedTiles[i])
                {
                    if (tile != null)
                    {
                        TileMain tileScript = tile.GetComponent<TileMain>();

                        if (tileScript.owner == player1)
                        {
                            tileScript.activeTriggersList = tileScript.allTriggersList;
                        }
                    }
                }
            }
        }
    }

    // SETUP FOR NEXT TURN
    public void EndTurn()
    {
        // 0: Check triggers for turn ended
        CheckTriggers(IsTurnEnd: true);

        // 1: Increase Turn Counter by 1
        turn++;

        // 2: Lock Placed Tiles In Position
        for (int i = 0; i < boardPlacedTiles.Length; i++)
        {
            for (int j = 0; j < boardPlacedTiles[i].Length; j++)
            {
                if(boardPlacedTiles[i][j] != null)
                {
                    TileMain tileScript = boardPlacedTiles[i][j].GetComponent<TileMain>();
                    if (!tileScript.locked)
                    {
                        tileScript.locked = true;
                        tileScript.updateResourceGain = true;
                        CheckTriggers(IsTileLocked: true, TileLocked: tileScript);
                    }
                }
            }
        }

        // 3: Reset Unit Speed Points
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (boardPlacedTiles[i][j] != null)
                {
                    TileMain tileScript = boardPlacedTiles[i][j].GetComponent<TileMain>();

                    if (tileScript.unitOnTile != null)
                    {
                        tileScript.unitOnTile.speedPoints = tileScript.unitOnTile.speedMax;
                    }
                }
            }
        }

        // 4: Check if Either Base Tile <= 0, if so end game, work our winner
        BaseTile p0BaseTileScript = player0.baseTile.GetComponent<BaseTile>();
        BaseTile p1BaseTileScript = player1.baseTile.GetComponent<BaseTile>();

        if (p0BaseTileScript.GetHealth() <= 0 && p1BaseTileScript.GetHealth() <= 0)
        {
            if(p0BaseTileScript.GetHealth() > p1BaseTileScript.GetHealth()){
                EndGame(0);
            }
            else if (p1BaseTileScript.GetHealth() > p0BaseTileScript.GetHealth())
            {
                EndGame(1);
            }
        }
        else if (p0BaseTileScript.GetHealth() <= 0) {
            EndGame(1);
        }
        else if (p1BaseTileScript.GetHealth() <= 0)
        {
            EndGame(0);
        }

        // 5: Currently Active Player Draws Card
        if (activePlayerID == 0)
        {
            player0.DrawTiles(1);
        } else if (activePlayerID == 1)
        {
            player1.DrawTiles(1);
        }

        // 6: Reset Both ActiveTriggers Lists
        ResetActiveTriggerList(0);
        ResetActiveTriggerList(1);

        // 7: Swap Active Player
        if (activePlayerID == 0)
        {
            activePlayerID = 1;
        }
        else if (activePlayerID == 1)
        {
            activePlayerID = 0;
        }
        SetPlayerActiveVars();

        // 8: Update resource gain per turn for new tiles

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (boardPlacedTiles[i][j] != null)
                {
                    TileMain tileScript = boardPlacedTiles[i][j].GetComponent<TileMain>();

                    if (tileScript.updateResourceGain)
                    {
                        tileScript.owner.resourceTurnAmts[0] += tileScript.resourceTurnGain[0];
                        tileScript.owner.resourceTurnAmts[1] += tileScript.resourceTurnGain[1];
                        tileScript.owner.resourceTurnAmts[2] += tileScript.resourceTurnGain[2];
                        tileScript.owner.resourceTurnAmts[3] += tileScript.resourceTurnGain[3];
                        tileScript.owner.resourceTurnAmts[4] += tileScript.resourceTurnGain[4];
                        tileScript.owner.resourceTurnAmts[5] += tileScript.resourceTurnGain[5];

                        tileScript.updateResourceGain = false;
                    }
                }
            }
        }

        // 9: Update resource total for currently active user
        if (activePlayerID == 0)
        {
            player0.resourceTotalAmts[0] += player0.resourceTurnAmts[0];
            player0.resourceTotalAmts[1] += player0.resourceTurnAmts[1];
            player0.resourceTotalAmts[2] += player0.resourceTurnAmts[2];
            player0.resourceTotalAmts[3] += player0.resourceTurnAmts[3];
            player0.resourceTotalAmts[4] += player0.resourceTurnAmts[4];
            player0.resourceTotalAmts[5] += player0.resourceTurnAmts[5];
        } else if (activePlayerID == 1)
        {
            player1.resourceTotalAmts[0] += player1.resourceTurnAmts[0];
            player1.resourceTotalAmts[1] += player1.resourceTurnAmts[1];
            player1.resourceTotalAmts[2] += player1.resourceTurnAmts[2];
            player1.resourceTotalAmts[3] += player1.resourceTurnAmts[3];
            player1.resourceTotalAmts[4] += player1.resourceTurnAmts[4];
            player1.resourceTotalAmts[5] += player1.resourceTurnAmts[5];
        }
    }
}
