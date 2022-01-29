using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public int life = 0;
    public int shields = 0;
    public string name = "name";
    public int order;
    public int attack = 10;
    public bool localHost = false;
    public Texture imgProfile = null;

    public bool gameOver = false;

    public bool win = false;

    public int PA = 0;

    public int laps = 0;

    public int slotPosition = 0;

    public int[] statsValue =  new int[5];

    public int ranking;
    void Start()
    {

        updateStats();
    }

    // Update is called once per frame
    void Update()
    {
        updateStats();
    }
    public void move()
    {

    }
    public void setLife(int a)
    {
        if ((life + a) <= 100)
        {
            life = life + a;
        }
        else
        {
            if ((life + a) > 100)
            {
                life = 100;
            }
            else
            {
                if ((life + a) <= 0)
                {
                    life = 0;
                    death();
                }
            }
        }
    }
    public void death()
    {
        gameOver = true;
    }

    public void updateStats()
    {
        statsValue =  new int[5];
        statsValue[0] = laps*100 + slotPosition;
        statsValue[1] = life;
        statsValue[2] = shields;
        statsValue[3] = attack;
        statsValue[4] = 3 - ranking;
    }

    #region Public files

    #endregion
}
