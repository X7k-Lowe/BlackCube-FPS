using UnityEngine;
using Photon.Pun; // Photonの名前空間を使用するのだ
using UnityEngine.UI; // UIコンポーネントの名前空間を使用するのだ

// スライダーの値を同期するためのクラスPhotonSliderViewを定義するのだ
public class PhotonSliderView : MonoBehaviourPunCallbacks, IPunObservable
{
    private Slider slider; // スライダーへの参照を保持する変数だよ

    // Startメソッドはオブジェクトの初期化に使われるのだ
    void Awake()
    {
        // このゲームオブジェクトにアタッチされたSliderコンポーネントを取得するのだ
        slider = GetComponent<Slider>();
    }

    // Photonのデータ同期のためのメソッドだよ
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // データを送信するプレイヤーの場合、スライダーの値をネットワークに送るのだ
            stream.SendNext(slider.value);
        }
        else
        {
            // 他のプレイヤーからデータを受信する場合、スライダーの値を更新するのだ
            slider.value = (float)stream.ReceiveNext();
        }
    }
}
