using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InnerPosition
{
    #region VARS

    public Vector3 location;
    public Player player;

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

    public Vector3 setPlayer(Player player)
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

        positions[index].player = player;
        return positions[index].location;
    }

    public Vector3 getPlayerLocation(int playerIndex)
    {
        return positions[playerIndex].location;
    }
}
