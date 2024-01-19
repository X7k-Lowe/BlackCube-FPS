using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class HealingCube : MonoBehaviour
{
    public int hearingPoint = 50;
    public PhotonView photonView;
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerController>().HealingHP(hearingPoint, photonView.ViewID);
        }
    }
}
