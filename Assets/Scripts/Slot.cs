using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InnerPosition
{
    #region VARS

    public Vector3 location;
    public GameObject player;

    #endregion

    public InnerPosition(Vector3 location)
    {
        this.location = location;
        this.player = null;
    }
}

public class Slot : MonoBehaviour
{
    #region VARS

    public int playersCount = 4;
    public InnerPosition[] positions;
    public Card card = null;

    #endregion

    private void Awake()
    {
        positions = new InnerPosition[playersCount];
        GameObject[] objects = GameObject.FindGameObjectsWithTag("player_position");
        for (int i = 0; i < playersCount; i++)
        {
            positions[i] = new InnerPosition(objects[i].transform.position);
        }
    }

    #region Positions

    public int getFreePosition()
    {
        bool freePosition = false;
        int index = 0;
        do
        {
            if (positions[index].player == null)
            {
                freePosition = true;
            }
            else
            {
                index++;
            }
        } while (index < playersCount && freePosition == false);

        return index;
    }

    public Vector3 getLocationByIndex(int index)
    {
        return positions[index].location;
    }

    public Vector3 getPlayerLocation(int playerIndex)
    {
        return positions[playerIndex].location;
    }

    public void setPlayerInPosition(int index, GameObject player)
    {
        positions[index].player = player;
    }

    public int getPlayerLocation(GameObject player)
    {
        bool founded = false;
        int index = 0;
        for (index = 0; index < positions.Length && founded == false; index++)
        {
            if (positions[index].player == player)
            {
                founded = true;
            }
        }
        return index;
    }

    public void removePlayerFromLocationByIndex(int index)
    {
        positions[index].player = null;
    }

    #endregion
}
