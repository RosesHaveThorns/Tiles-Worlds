using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialTile : MonoBehaviour
{
    // Options Variables
    [System.Serializable]
    public class Action
    {
        public Color colour;
        public Sprite sprite;
        public string title;
        public bool useableByAll;
        public bool useableWhenUnitOnTile;
    }

    public Action[] options;
    public string label;

    // References
    public TileMain thisTileScript;

    private void Start()
    {
        thisTileScript = this.gameObject.GetComponent<TileMain>();
        label = thisTileScript.tileName;
    }

    private void OnMouseDown()
    {
        if (thisTileScript.locked)
        {
            if (thisTileScript.owner.activePlayer || thisTileScript.unitOnTile != null && thisTileScript.unitOnTile.playerOwner == thisTileScript.owner.gameController.activePlayerID)
            {
                RadialSpawner.ins.SpawnMenu(this);
            }
        }
    }
}
