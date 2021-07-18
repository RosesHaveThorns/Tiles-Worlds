using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckMethods : MonoBehaviour
{
    public static List<GameObject> ShuffleDeck(List<GameObject> deck)       // Shuffles deck using Fisher Yates Shuffle: i.e. takes the end index, swaps with a random one earlier or itself, takes index at end-1, swaps with a random one earlier or itself, repeats for each index but 0
    {

        System.Random random = new System.Random();

        GameObject temp;
        int n = deck.Count;

        for (int i = n - 1; i > 0; i--)
        {
            int rndVal = random.Next(0, i + 1);

            temp = deck[rndVal];
            deck[rndVal] = deck[i];
            deck[i] = temp;
        }

        return deck;
    }

    public static int GetFreeHandPosition(GameObject[] hand)
    {
        for (int i = 0; i < hand.Length; i++)
        {
            if (hand[i] == null)
            {
                return i;
                
            }
        }

        return -1;
    }

    public static GameObject InstantiateTile(GameObject tile, GameObject snapCube, PlayerController owner, GameObject parent = null)
    {
        GameObject newTile = GameObject.Instantiate(tile);

        TileMain tileScript = newTile.GetComponent<TileMain>();
        tileScript.SnapTo(snapCube);

        tileScript.owner = owner;
        tileScript.gameController = owner.gameController;

        if (parent != null)
        {
            newTile.transform.parent = parent.transform;
            newTile.transform.rotation = Quaternion.Euler(0, 45, 0);
        }

        return newTile;
    }
}
