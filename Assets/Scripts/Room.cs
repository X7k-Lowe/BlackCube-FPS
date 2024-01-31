using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;

public class Room : MonoBehaviour
{
    // ルーム名テキスト
    public TextMeshProUGUI buttonText;

    // ルーム情報
    private RoomInfo info;

    private PhotonManager photonManager;

    void Awake()
    {
        photonManager = GameObject.Find("Canvas").GetComponent<PhotonManager>();
    }

    // このボタンの変数にルーム情報を格納
    public void RegisterRoomDetails(RoomInfo info)
    {
        // ルーム情報格納
        this.info = info;

        // UI
        buttonText.text = this.info.Name;
    }

    // このルームボタンが管理しているルームに入室する
    public void OpenRoom()
    {
        if (photonManager.allowInput)
        {
            photonManager.RoomButtonSE();
            // ルーム入室関数を呼び出す
            PhotonManager.instance.JoinRoom(info);
        }
    }
}
