using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private Transform[] positions;
    private int position = 1; // 1, 2, 3, ....
    public Camera mainCamera;
    public float timeToMove = 2.0f;
    public float maxYrotation = 5f;

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
            //float mouseX = Input.GetAxisRaw("Mouse X") * timeToMove * Time.deltaTime;
            //float yRotation = Mathf.Clamp(mouseX, 85f, 95f);
            //Debug.Log("x-axis: " + mouseX + ", final-rotation: " + yRotation);
            //Quaternion newRotation = new Quaternion(positions[position].rotation.x, yRotation, positions[position].rotation.z, positions[position].rotation.w);

            mainCamera.transform.position = Vector3.Lerp(mainCamera.transform.position, positions[position].position, timeToMove * Time.deltaTime);
            mainCamera.transform.rotation = Quaternion.Lerp(mainCamera.transform.rotation, positions[position].rotation, timeToMove * Time.deltaTime);
        }
    }
}
