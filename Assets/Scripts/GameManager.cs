using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    // Nuevas variables, son para las estadisticas del jugador y juego
    //Juego
    public int gameTurnTime = 0;
    public Text txtGameTurnTime;

    public Text txtGameResult;

    public GameObject[] playersRanking;
    public Text txtRanking1;
    public Text txtRanking2;
    public Text txtRanking3;
    public Text txtRanking4;

    //Jugador
    private GameObject playerLocalHost;
    public Text txtNamePlayer;
    public Slider sliderLife;

    public Text txtPA;

    public Text txtShield;

    public RawImage imgProfile;
    // fin
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        slots = new GameObject[slotsNumber];
        players = new GameObject[playersCount];
        generatBoard();
        setPlayersInInitialPosition(0);

        //LLamando la nueva funcion
        playerLocalHost = findLocalPlayer();
        updateStatsGui();
        startCountDown();
    }

    // Update is called once per frame
    void Update()
    {
        updateStatsGui();
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

            if (i == 0) {
                player.localHost = true;
            }

            players[i] = avatar;
            initialSlot.setPlayerInPosition(freePosition, avatar);
            sortedPlayers.Add(player.order, player);
            Debug.Log(player.nickname + ": " + player.order);
        }
    }

    // Nuevas funciones
    private void updateStatsGui()
    {
        //Player
        txtNamePlayer.text = playerLocalHost.GetComponent<Player>().name;
        sliderLife.value = playerLocalHost.GetComponent<Player>().life;
        txtPA.text = playerLocalHost.GetComponent<Player>().PA.ToString();
        txtShield.text = playerLocalHost.GetComponent<Player>().shields.ToString();
        imgProfile.GetComponent<RawImage>().texture = playerLocalHost.GetComponent<Player>().imgProfile;
        txtGameResult.text = gameResult(playerLocalHost);

        //Game
        txtGameTurnTime.text = gameTurnTime.ToString();

        txtRanking1.text = playersRanking[0].GetComponent<Player>().name;
        txtRanking2.text = playersRanking[1].GetComponent<Player>().name;
        txtRanking3.text = playersRanking[2].GetComponent<Player>().name;
        txtRanking4.text = playersRanking[3].GetComponent<Player>().name;
    }

    private GameObject findLocalPlayer()
    {
        int i = 0;
        while (players[i].GetComponent<Player>().localHost != true)
        {
            i++;
        }
        return players[i];
    }

    public void startCountDown()
    {
        StartCoroutine("setTime");
    }

    IEnumerator setTime()
    {
        yield return new WaitForSeconds(1);
        gameTurnTime -= 1;
        if (gameTurnTime > 0)
        {
            StartCoroutine("setTime");
        }
        else
        {
            StartCoroutine("endOfShift");
        }
    }

    IEnumerator endOfShift()
    { // Fin del turno
        yield return new WaitForSeconds(3);
        gameTurnTime = 30;
        StartCoroutine("setTime");
    }

    public string gameResult(GameObject ply)
    {
        string result = "En curso";
        if (ply.GetComponent<Player>().win)
        {
            result = "WIN";
        }
        else
        {
            if (ply.GetComponent<Player>().gameOver)
            {
                result = "GAME OVER";
            }
        }
        return result;
    }
    // fin de nuevas funciones

    #endregion
}
