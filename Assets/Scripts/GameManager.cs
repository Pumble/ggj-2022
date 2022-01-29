using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;
using Photon.Pun;

public enum Elements : ushort { Fire = 1, Water = 2, Earth = 3, Wind = 4 };

public class GameManager : MonoBehaviourPun
{
    #region VARS

    [Header("Game Manager del nivel")]

    [Header("Variables sobre slots")]
    public GameObject slotPrefab;
    public GameObject[] slots;
    public int slotsNumber = 40;

    [Header("Variables sobre los jugadores")]
    public int playersCount = 4;
    public GameObject playerPrefab;
    public GameObject[] players;
    public SortedDictionary<int, Player> sortedPlayers = new SortedDictionary<int, Player>();

    [Header("Variables sobre la musica")]
    public GameObject musicList;

    [Header("Variables sobre la UI")]
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

    public bool MatchInCourse = false;

    // Start is called before the first frame update
    void Start()
    {
        slots = new GameObject[slotsNumber];
        players = new GameObject[playersCount];

        setPlayersInInitialPosition(0);

        //LLamando la nueva funcion
        playerLocalHost = findLocalPlayer();
        updateStatsGui();
        startCountDown();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        updateStatsGui();
    }

    #region Methods

    void StarMusic()
    {
        try
        {
            AudioSource[] audios = musicList.GetComponents<AudioSource>();
            int musicRandom = Random.Range(0, audios.Length - 1);
            audios[musicRandom].Play();
        }
        catch (System.Exception)
        {
            Debug.LogError("Algo paso con la musica");
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


    #region PUN EVENTS

    [PunRPC]
    void attack(Player from, Player to)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            to.life = to.life - from.attack;
        }
    }

    #endregion
}
