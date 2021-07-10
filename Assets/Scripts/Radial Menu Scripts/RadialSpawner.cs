using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadialSpawner : MonoBehaviour
{
    public static RadialSpawner ins;
    public RadialMenu menuPrefab;

    private void Awake()
    {
        ins = this;
    }

    public void SpawnMenu(RadialTile obj)
    {
        RadialMenu newMenu = Instantiate(menuPrefab) as RadialMenu;
        newMenu.ogRadialTile = obj;
        newMenu.transform.SetParent(transform, false);
        newMenu.transform.position = Input.mousePosition;
        newMenu.label.text = obj.label;
        newMenu.SpawnButtons(obj);
    }
}
