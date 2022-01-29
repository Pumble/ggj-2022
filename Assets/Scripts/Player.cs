using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player
{
    #region Public files

    public string nickname = null;
    public int order;
    public int life = 100;
    public int attack = 10;
    public int currentSlot = 0;
    public GameObject avatar = null;

    #endregion

    public Player()
    {
        this.nickname = null;
        this.order = 0;
        this.life = 100;
        this.attack = 10;
        this.currentSlot = 0;
        this.avatar = null;
    }
}
