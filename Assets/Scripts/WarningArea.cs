using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WarningArea : MonoBehaviour
{
    GameObject myPlayer;
    GameObject enemyPlayer;
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // if(other.gameObject == )
        }
    }

    public void SetMyPlayerObject(GameObject player)
    {
        myPlayer = player;
    }
}
