using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Change
{

    public static Vector2 X(this Vector2 v, float x)
    {
        v.x = x;
        return v;
    }

    public static Vector2 Y(this Vector2 v, float y)
    {
        v.y = y;
        return v;
    }

    public static Vector3 X(this Vector3 v, float x)
    {
        v.x = x;
        return v;
    }

    public static Vector3 Y(this Vector3 v, float y)
    {
        v.y = y;
        return v;
    }

    public static Vector3 Z(this Vector3 v, float z)
    {
        v.z = z;
        return v;
    }

    public static Color R(this Color c, float r)
    {
        c.r = r;
        return c;
    }

    public static Color G(this Color c, float g)
    {
        c.g = g;
        return c;
    }

    public static Color B(this Color c, float b)
    {
        c.b = b;
        return c;
    }

    public static Color A(this Color c, float a)
    {
        c.a = a;
        return c;
    }
}


public class GameManager : MonoBehaviour
{
    #region VARS

    public enum Elements { Fire, Water, Earth, Wind };

    [Header("Variables sobre slots")]
    public GameObject slotPrefab;
    public GameObject[] slots;
    public int slotsNumber = 40;

    [Header("Variables sobre los jugadores")]
    public int playersCount = 4;
    public GameObject playerPrefab;
    public Player[] players;
    public SortedDictionary<int, Player> sortedPlayers = new SortedDictionary<int, Player>();

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        slots = new GameObject[slotsNumber];
        players = new Player[playersCount];
        generatBoard();
        setPlayersInInitialPosition(0);
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Methods

    /// <summary>
    /// Generate the board, make it with slots
    /// </summary>
    private void generatBoard()
    {
        float x = 0, y = 0.0f, z = 0f;
        int splitIn = slotsNumber / 4;
        for (int i = 0, slotIndex = 1; i < slotsNumber; i++, slotIndex++)
        {
            GameObject newSlot = Instantiate(slotPrefab, new Vector3(x, y, z), Quaternion.identity);
            newSlot.name = "Slot_" + slotIndex;

            if (slotIndex >= splitIn * 3)
            {
                x = -1;
                z++;
            }
            else if (slotIndex >= splitIn * 2)
            {
                x--;
                z = -10;
            }
            else if (slotIndex >= splitIn)
            {
                z--;
                x = 9;
            }
            else
            {
                x++;
            }

            slots[i] = newSlot;
        }
    }

    /// <summary>
    /// Instantiate every player in the slot #0
    /// </summary>
    private void setPlayersInInitialPosition(int initialPosition)
    {
        Slot initialSlot = slots[initialPosition].GetComponent<Slot>();
        for (int i = 0; i < playersCount; i++)
        {
            string playerName = "Player_" + i;

            Player player = new Player();
            player.nickname = playerName;
            player.order = UnityEngine.Random.Range(1, 100);

            Vector3 innerLocation = initialSlot.setPlayer(player);
            Vector3 position = innerLocation + slots[initialPosition].transform.position;
            GameObject avatar = Instantiate(playerPrefab, position, Quaternion.identity);
            avatar.name = playerName;

            player.avatar = avatar;
            players[i] = player;
            sortedPlayers.Add(player.order, player);
            Debug.Log(player.nickname + ": " + player.order);
        }

        // Debug.Log(sortedPlayers.Values);

    }    

    #endregion

    
}
