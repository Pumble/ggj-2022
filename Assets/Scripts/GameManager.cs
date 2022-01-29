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
    public GameObject[] slotPositions;
    public int slots = 40;

    [Header("Variables sobre los jugadores")]
    public int playersCount = 4;
    public GameObject playerPrefab;
    public GameObject[] players;
    public SortedDictionary<int, GameObject> sortedPlayers = new SortedDictionary<int, GameObject>();

    // Nuevas variables, son para las estadisticas del juegador y juego
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
        slotPositions = new GameObject[slots];
        players = new GameObject[playersCount];
        generatBoard();
        positionatePlayers();

        //LLamando la nueva funcion
        playerLocalHost = findLocalPlayer();
        playersRanking = players;

        updateStatsGui();
        startCountDown();
        //
    }

    // Update is called once per frame
    void Update()
    {
        sortRanking();
        updateStatsGui();
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
            player.name = "Player_" + i;

            string playerName = "Player_" + i;
            player.name = playerName;
            Player playerClass = player.GetComponent<Player>();
            playerClass.name = playerName;
            playerClass.order = UnityEngine.Random.Range(1, 100);
            sortedPlayers.Add(playerClass.order, players[i]);

            // Solo es para poner al primer jugador como local y dejar el ranking por defecto a todos
            player.GetComponent<Player>().ranking = i;
            if (i == 0)
            {

                player.GetComponent<Player>().slotPosition = 0;
                player.GetComponent<Player>().localHost = true;
                player.GetComponent<Player>().name = "JugadorLocal";
                player.GetComponent<Player>().life = 100;
            }
            player.GetComponent<Player>().updateStats();
            // fin del cambio

            players[i] = player;

            Debug.Log(playerClass.name + ": " + playerClass.order);
        }

        Debug.Log(sortedPlayers.Values);

    }

    public void movePlayerPosition(int player, int position)
    {

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
    private void sortRanking()
    {
        int a = 0;
        GameObject aux = null;
        int mx = 0;
        int index = 0;
        bool sort = true;
        bool equal = false;

        while (sort)
        {
            sort = false;
            equal = false;
            if (index != 5)
            {
                sortMaximum(a, aux, mx, index);
                int k = a;
                while ((k < 3) && (!equal))
                {
                    if (playersRanking[k].GetComponent<Player>().statsValue[index] ==
                                            playersRanking[k + 1].GetComponent<Player>().statsValue[index])
                    {
                        a = k;
                        sort = true;
                        index++;
                        equal = true;
                    }
                    else
                    {
                        k++;
                    }
                }
            }
        }
    }
    private void sortMaximum(int a, GameObject aux, int mx, int index)
    {

        for (int i = a; i < 4; i++)
        {
            mx = i;
            for (int j = i + 1; j < 4; j++)
            {
                if (playersRanking[j].GetComponent<Player>().statsValue[index] >
                                        playersRanking[mx].GetComponent<Player>().statsValue[index])
                {
                    mx = j;
                }
            }
            aux = playersRanking[i];
            playersRanking[i] = playersRanking[mx];
            playersRanking[mx] = aux;
        }
    }
    // fin de nuevas funciones
    #endregion
}
