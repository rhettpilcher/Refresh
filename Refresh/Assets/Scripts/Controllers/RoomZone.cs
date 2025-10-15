using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomZone : MonoBehaviour
{
    //switch camera to view this room
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }
}
