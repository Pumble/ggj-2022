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
    public GameObject[] slotPositions;
    public int slots = 40;

    [Header("Variables sobre los jugadores")]
    public int playersCount = 4;
    public GameObject playerPrefab;
    public GameObject[] players;
    public SortedDictionary<int, GameObject> sortedPlayers = new SortedDictionary<int, GameObject>();

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        slotPositions = new GameObject[slots];
        players = new GameObject[playersCount];
        generatBoard();
        positionatePlayers();
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
        for (int i = 0; i < slots; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, new Vector3(x, y, z), Quaternion.identity);
            newSlot.name = "Slot_" + i;

            if (i > 28)
            {
                x = -1;
                z++;
            }
            else if (i > 18)
            {
                x--;
                z = -10;
            }
            else if (i > 8)
            {
                z--;
                x = 9;
            }
            else
            {
                x++;
            }

            slotPositions[i] = newSlot;
        }
    }

    /// <summary>
    /// Instantiate every player in the slot #0
    /// </summary>
    private void positionatePlayers()
    {
        Positions positions = slotPositions[0].GetComponent<Positions>();
        for (int i = 0; i < playersCount; i++)
        {
            Vector3 position = positions.getPosition(i) + slotPositions[0].transform.position;
            GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
            string playerName = "Player_" + i;
            player.name = playerName;
            Player playerClass = player.GetComponent<Player>();
            playerClass.name = playerName;
            playerClass.order = UnityEngine.Random.Range(1, 100);
            sortedPlayers.Add(playerClass.order, players[i]);
            players[i] = player;

            Debug.Log(playerClass.name + ": " + playerClass.order);
        }

        Debug.Log(sortedPlayers.Values);

    }

    public void movePlayerPosition(int player, int position)
    {

    }

    #endregion
}
