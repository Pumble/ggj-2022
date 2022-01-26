using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Positions : MonoBehaviour
{
    private Transform[] positions;

    private void Awake()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("player_position");
        positions = new Transform[objects.Length];
        for (int i = 0; i < objects.Length; i++)
        {
            positions[i] = objects[i].transform;
        }
    }

    public Vector3 getPosition(int position)
    {
        return new Vector3(positions[position].position.x, positions[position].position.y, positions[position].position.z);
    }
}
