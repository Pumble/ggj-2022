using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    public GameObject[] players;
    public SortedDictionary<int, Player> sortedPlayers = new SortedDictionary<int, Player>();

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        slots = new GameObject[slotsNumber];
        players = new GameObject[playersCount];
        generatBoard();
        setPlayersInInitialPosition(0);
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

            int freePosition = initialSlot.getFreePosition();
            Vector3 position = initialSlot.getLocationByIndex(freePosition) + slots[initialPosition].transform.position;
            GameObject avatar = Instantiate(playerPrefab, position, Quaternion.identity);
            avatar.name = playerName;

            Player player = avatar.GetComponent<Player>();
            player.nickname = playerName;
            player.order = UnityEngine.Random.Range(1, 100);
            player.gameManager = this;
            player.positionInSlot = freePosition;

            players[i] = avatar;
            initialSlot.setPlayerInPosition(freePosition, avatar);
            sortedPlayers.Add(player.order, player);
            Debug.Log(player.nickname + ": " + player.order);
        }
    }

    #endregion
}
