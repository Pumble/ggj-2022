using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    public int life = 0;
    public int shields = 0;
    public string name = "name";

    public bool localHost = false;
    public Texture imgProfile = null;

    public bool gameOver = false;

    public bool win = false;

    public int PA = 0;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

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
                    deat();
                }
            }
        }
    }
    public void deat(){
        gameOver = true;
    }
    #region Public files

    public string Name;
    public int order;
    public int life = 100;
    public int attack = 10;

    #endregion
}
