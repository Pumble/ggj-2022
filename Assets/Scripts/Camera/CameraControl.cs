using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Transform[] positions;
    private int position = 1; // 1, 2, 3, ....
    public Camera mainCamera;
    public float timeToMove = 2.0f;

    private void Awake()
    {
        positions = GetComponentsInChildren<Transform>();

        if (mainCamera != null)
        {
            mainCamera.transform.position = positions[position].position;
            mainCamera.transform.rotation = positions[position].rotation;
        }
    }

    private void Update()
    {
        Vector2 scrollMovement = Input.mouseScrollDelta;
        if (scrollMovement.y > 0 && position >= 1 && position < positions.Length - 1)
        {
            position++;
        }
        else if (scrollMovement.y < 0 && position < positions.Length && position > 1)
        {
            position--;
        }

        if (mainCamera != null)
        {
            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, positions[position].position, timeToMove * Time.deltaTime);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, positions[position].rotation, timeToMove * Time.deltaTime);
        }
    }
}
