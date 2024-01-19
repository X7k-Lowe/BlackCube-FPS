using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class EyeArea : MonoBehaviour
{
    [SerializeField] PlayerController playerController;
    public bool triggerEnter { get; set; } = false;
    public bool triggerExit { get; set; } = false;

    void OnTriggerEnter(Collider other)
    {
        if (!playerController.photonView.IsMine) return;

        if (other.gameObject.CompareTag("Player") && other.transform.parent == null)
        {
            playerController.AddWorldObject(other.gameObject);
            playerController.EnemyPhotonViews.Add(other.gameObject.GetComponent<PhotonView>());
            Debug.Log("Player ADD: " + other.gameObject.GetComponent<PhotonView>().Owner.NickName);
            other.gameObject.GetComponent<PhotonView>().RPC("IncrementEyeAreaCounter", RpcTarget.All);
        }
        else if (other.gameObject.CompareTag("HealingCube"))
        {
            playerController.AddWorldObject(other.gameObject);
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (!playerController.photonView.IsMine) return;

        if (other.gameObject.CompareTag("Player") && other.transform.parent == null)
        {
            playerController.RemoveWorldObject(other.gameObject);
            playerController.EnemyPhotonViews.Remove(other.gameObject.GetComponent<PhotonView>());
            Debug.Log("Player RMV: " + other.gameObject.GetComponent<PhotonView>().Owner.NickName);
            other.gameObject.GetComponent<PhotonView>().RPC("DecrementEyeAreaCounter", RpcTarget.All);
        }
    }
}

[System.Serializable]
public class ObservableList<T> : List<T>
{
    public delegate void OnAdd(T item);
    public event OnAdd onAdd;

    public delegate void OnRemove(T item);
    public event OnRemove onRemove;

    public new void Add(T item)
    {
        base.Add(item);
        onAdd?.Invoke(item);
    }

    public new void Remove(T item)
    {
        if (base.Contains(item))
        {
            base.Remove(item);
            onRemove?.Invoke(item);
        }
    }
}

