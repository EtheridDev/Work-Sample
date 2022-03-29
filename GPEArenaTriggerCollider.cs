using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPEArenaTriggerCollider : MonoBehaviour
{
    public GPEArenaManager arenaManager;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            arenaManager.LaunchArena();
        }
    }

}
