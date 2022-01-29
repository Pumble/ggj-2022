using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    #region VARS

    [Header("Variables sobre slots")]
    public GameObject slotPrefab;
    public GameObject[] slotPositions;
    public int slots = 40;

    [Header("Variables sobre los jugadores")]
    public int playersCount = 4;
    public GameObject playerPrefab;
    public GameObject[] players;

    // Nuevas variables, son para las estadisticas del juegador y juego
    //Juego
    public int gameTurnTime = 0;
    public Text txtGameTurnTime;

    public Text txtGameResult;

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
        updatePlayerStats();
        startCountDown();
        //
    }

    // Update is called once per frame
    void Update()
    {
        updatePlayerStats();
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
        Positions positions = slotPositions[38].GetComponent<Positions>();
        for (int i = 0; i < playersCount; i++)
        {
            Vector3 position = positions.getPosition(i);
            GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
            player.name = "Player_" + i;

            // Solo es para poner al primer jugador como local
            if (i == 0)
            {
                player.GetComponent<Player>().localHost = true;
                player.GetComponent<Player>().name = "JugadorLocal";
                player.GetComponent<Player>().life = 50;
            }
            // fin del cambio

            players[i] = player;
        }
    }
    // Nuevas funciones
    private void updatePlayerStats()
    {
        txtNamePlayer.text = playerLocalHost.GetComponent<Player>().name;
        sliderLife.value = playerLocalHost.GetComponent<Player>().life;
        txtPA.text = playerLocalHost.GetComponent<Player>().PA.ToString();
        txtShield.text = playerLocalHost.GetComponent<Player>().shields.ToString();
        imgProfile.GetComponent<RawImage>().texture = playerLocalHost.GetComponent<Player>().imgProfile;
        txtGameTurnTime.text = gameTurnTime.ToString();
        txtGameResult.text = gameResult(playerLocalHost);
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
