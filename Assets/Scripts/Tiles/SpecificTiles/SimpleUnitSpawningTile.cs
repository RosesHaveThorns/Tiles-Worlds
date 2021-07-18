using UnityEngine;
using System.Collections.Generic;

public class SimpleUnitSpawningTile : TileMain
{
    public int[] unitSpawnCost = new int[6];
    public GameObject unitToSpawn;

    private void Start()
    {
        InheritedStart();
        eventsList.Add(Event0);

        allTriggersList.Add(Trigger0);
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

    public override void Event0()
    {
        for (int i = 0; i < 6; i++)
        {
            owner.resourceTotalAmts[i] -= unitSpawnCost[i];
        }

        SpawnUnit(unitToSpawn);
    }

    public override bool Trigger0(bool isTurnEnd= false,
        bool isTileDrawn = false, TileMain tileDrawn = null,
        bool isTileLocked = false, TileMain tileLocked = null,
        bool isRadialButtonSelected = false, string radialButtonSelectedTitle = null, TileMain radialmenuTile = null)
    {
        if (radialmenuTile == this && isRadialButtonSelected && radialButtonSelectedTitle == "spawn")
        {
            for (int i = 0; i < 6; i++)
            {
                if (owner.resourceTotalAmts[i] < unitSpawnCost[i])
                {
                    return false;
                }
            }
            return true;
        }

        return false;
    }

}