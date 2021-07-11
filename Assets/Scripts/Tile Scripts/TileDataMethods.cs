using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class TileDataMethods
{
    // ========================= DECK FILE IO METHODS ==========================

    public static void SaveDeck(GameObject[] deck, int playerID)
    {
        string fileName = "deck" + playerID + ".dk";
        string path = Application.persistentDataPath + "/" + fileName;

        BinaryFormatter formatter = new BinaryFormatter();

        string[] data = new string[20];

        if (deck.Length == 0)
        {
            // deserialize function cant work if file empty, so must avoid that here
            data[0] = "null";
        }
        else
        {
            data = DeckToStringArray(deck);
        }

        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, data);

        stream.Close();
    }

    public static GameObject[] LoadDeck(GameObject[] allTiles, int playerID, bool gameLoading = false)
    {
        string fileName = "deck" + playerID + ".dk";
        string path = Application.persistentDataPath + "/" + fileName;
		Debug.Log (path);

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            string[] data = formatter.Deserialize(stream) as string[];
            stream.Close();

            if (data[0] == "null")   // if the deck is 'empty' (ie first value is string "null"), return deck as an empty array
            {
                GameObject[] emptyDeck = new GameObject[20];
                return emptyDeck;
            }
            else
            {
                GameObject[] deck = StringArrayToDeck(data, allTiles);
                return deck;
            }

        }
        else
        {
            if (!gameLoading)
            {
                Debug.Log("Deck Save File Not Found In " + path + ", creating empty Deck Save File");

                GameObject[] deck = new GameObject[20];
                SaveDeck(deck, playerID);

                return null;
            }
            else
            {
                return null;
            }

        }
    }

    // ==================== COLLECTION FILE IO METHODS =========================

    // Save Collection as String to file (collection.dk)
    public static void SaveCollection(List<GameObject> collection)
    {
        string fileName = "collection.dk";
        string path = Application.persistentDataPath + "/" + fileName;

        BinaryFormatter formatter = new BinaryFormatter();

        string[] data;

        if (collection.Count == 0)
        {
            // deserialize fucntion cant get work if file empty, so must avoid that here
            data = new string[1]; // data must be created here, as cretaing it earlier using collection.Count as its length makes an array with 0 elements
            data[0] = "null";
        }
        else
        {
            data = new string[collection.Count];
            data = CollectionToStringArray(collection);
        }

        FileStream stream = new FileStream(path, FileMode.Create);

        formatter.Serialize(stream, data);

        stream.Close();
    }

    // Load Collection from file (collection.dk) as String
    public static List<GameObject> LoadCollection(GameObject[] allTiles)
    {
        string fileName = "collection.dk";
        string path = Application.persistentDataPath + "/" + fileName;

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);

            string[] data = formatter.Deserialize(stream) as string[];
            stream.Close();


            if (data[0] == "null")   // if the deck is 'empty' (ie first value is string "null"), save deck as an empty array
            {
                List<GameObject> emptyCollection = new List<GameObject>();
                return emptyCollection;
            }
            else
            {
                List<GameObject> deck = StringArrayToCollection(data, allTiles);
                return deck;
            }

        }
        else
        {
            Debug.Log("Colelction Save File Not Found In " + path + ", creating empty Collection Save File");

            List<GameObject> collection = new List<GameObject>();
            SaveCollection(collection);

            return null;
        }
    }

    // ==================== ARRAY MANIPULATION METHODS =========================

    // Check if all elements in an array are Null, if so, returns >> TRUE <<, this is basicly a linear search
    public static bool CheckArrayEmpty(object[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != null)
            {
                return false;
            }

        }
        return true;
    }

    // Converting the deckList from agme objects to strings and back using thir name
    public static string[] DeckToStringArray(GameObject[] deck)
    {
        string[] deckStrings = new string[20];

        for (int i = 0; i < 20; i++)
        {
            // If no tile in element then store as "empty"
            if (deck[i] != null)
            {
                deckStrings[i] = deck[i].name;
            }
            else
            {
                deckStrings[i] = "empty";
            }
        }
        return deckStrings;
    }

    // Converting strings of tile prefab names to an array of those gameobjects
    public static GameObject[] StringArrayToDeck(string[] deckStrings, GameObject[] allTiles)
    {
        GameObject[] deck = new GameObject[20];
        for (int i = 0; i < 20; i++)
        {
            if (deckStrings[i] != "empty")
            {
                deck[i] = allTiles[GameObjectArrayBiSearch(allTiles, deckStrings[i])];
            }
            else
            {
                deck[i] = null;
            }

        }
        return deck;
    }

    // Swaps 2 elements in a array of Objects, used for the sorting of allTiles
    public static GameObject[] SwapElement(GameObject[] data, int x, int y)
    {
        GameObject temp = data[x];
        data[x] = data[y];
        data[y] = temp;
        return data;
    }

    // Insertion Sort on an array of gameObjects, used for sorting of allTiles
    public static GameObject[] GameObjectArrayInsertSort(GameObject[] data)
    {
        int i, j;
        int n = data.Length;

        for (j = 1; j < n; j++)
        {
            for (i = j; i > 0 && int.Parse(data[i].name.Split('_')[0]) < int.Parse(data[i - 1].name.Split('_')[0]); i--)
            {
                data = SwapElement(data, i, i - 1);
            }
        }

        return data;
    }

    // Binary search on Array of game objects looking for same name, sorted by first 4 characters as integers, returns index of element found, or null if failed
    public static int GameObjectArrayBiSearch(GameObject[] data, string keyTileName)
    {
        int min = 0;
        int max = data.Length - 1;

        int keyTileID = int.Parse(keyTileName.Split('_')[0]);   // Gets the first 4 charcters of the tilews nae as a integer

        while (min <= max)
        {
            int mid = (min + max) / 2;

            int midTileID = int.Parse(data[mid].name.Split('_')[0]);   // Gets the first 4 charcters of the tilews nae as a integer

            if (keyTileName == data[mid].name)
            {
                return mid;
            }
            else if (keyTileID < midTileID)
            {
                max = mid - 1;
            }
            else
            {
                min = mid + 1;
            }
        }
        Debug.LogError("keyTile Not Found in Data, Returning -1");
        return -1;
    }

    // ==================== LIST MANIPULATION METHODS ==========================

    // Converting the collectionfrom game objects list to array of strings and back using their name
    public static string[] CollectionToStringArray(List<GameObject> collection)
    {
        string[] collectionStrings = new string[collection.Count];

        for (int i = 0; i < collection.Count; i++)
        {
            collectionStrings[i] = collection[i].name;
        }
        return collectionStrings;
    }

    // Converting strings of tile prefab names to a list of those gameobjects
    public static List<GameObject> StringArrayToCollection(string[] collectionStrings, GameObject[] all)
    {
        List<GameObject> collection = new List<GameObject>();
        for (int i = 0; i < collectionStrings.Length; i++)
        {
            collection.Add(all[GameObjectArrayBiSearch(all, collectionStrings[i])]);
        }
        return collection;
    }

    // ======================= TILE GRAPH METHODS =============================

    public static bool checkConnectedToBase(GameObject startTile, PlayerController player)
    {
        TileMain startTileScript = startTile.GetComponent<TileMain>();
        TileMain baseTileScript = player.baseTile.GetComponent<TileMain>();
        var visited = new HashSet<TileMain>();
        var queue = new Queue<TileMain>();

        // Enqueue source node
        queue.Enqueue(startTileScript);

        while (queue.Count > 0)
        {
            var vertex = queue.Dequeue();

            if (visited.Contains(vertex))
                continue;

            visited.Add(vertex);

            GameObject[] adjTiles = vertex.GetAdjacentTiles(vertex.snappedTo);

            foreach (var neighbor in adjTiles)
            {
                if (neighbor != null)
                {
                    TileMain neighborTileScript = neighbor.GetComponent<TileMain>();
                    
                    if (neighborTileScript.owner == player)
                    {

                        if (!visited.Contains(neighborTileScript))
                        {
                            queue.Enqueue(neighborTileScript);
                        }
                    }
                }
            }
        }
        if(!visited.Contains(baseTileScript))
        {
            return false;
        } else
        {
            return true;
        }
    }
}
