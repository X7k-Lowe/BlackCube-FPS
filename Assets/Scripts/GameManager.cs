using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime; // IOnEventCallback
using ExitGames.Client.Photon; // IOnEventCallback
using System.Linq;
using UnityEngine.EventSystems;

public enum GameState
{
    Playing,
    Ending,
}

public class GameManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    // プレイヤー情報を格納するリスト
    public List<PlayerInfo> playerList = new List<PlayerInfo>();

    // イベント
    public enum EventCodes : byte // Photonではbyte型にする
    {
        NewPlayer,
        ListPlayers,
        UpdateState,
        SetTargetNumber,
    }


    // ゲームの状態
    public GameState state;

    public UIManager uIManager { get; set; }

    private List<PlayerInfomation> playerInfoList = new List<PlayerInfomation>();



    // クリアまでのキル数
    public int targetNumber = 2;

    // クリアパネルを表示している時間
    public float waitAfterEnding = 12f;

    public SpawnManager spawnManager;
    public GameObject mainCamera;
    public GameObject oVRCameraRig;
    public GameObject uIHelper;

    public bool AllowLeaveRoom { get; set; } = false;

    public LaserPointer laserPointer;
    public SkinnedMeshRenderer rightControllerRenderer;
    public SkinnedMeshRenderer leftControllerRenderer;

    public AimMode AimMode = AimMode.RightHand;

    public bool isStart { get; set; } = false;

    public bool onSetKills { get; set; } = false;

    public bool isPracticeMode { get; set; } = false;
    public bool allowInput { get; set; } = false;
    float waitTime = 1f;

    private void Awake()
    {
        mainCamera.SetActive(true);
        oVRCameraRig.SetActive(true);
        AllowLeaveRoom = false;
        allowInput = false;
        // uIManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();
        if (!PhotonNetwork.IsConnected)
        {
            // 0番目のシーンに遷移
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerGet(PhotonNetwork.NickName);

            // if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("KillNumber"))
            // {
            //     targetNumber = (int)PhotonNetwork.CurrentRoom.CustomProperties["KillNumber"];
            //     Debug.Log("GameManager KillNumber: " + targetNumber);
            //     KillNumberGet(targetNumber);
            // }

            // if (PlatformManager.Instance.Platform == "Oculus")
            // {
            //     if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("AimMode"))
            //     {
            //         AimMode = (AimMode)PhotonNetwork.CurrentRoom.CustomProperties["AimMode"];
            //     }
            // }

            if (PhotonNetwork.IsMasterClient)
            {
                targetNumber = PlayerPrefs.GetInt("KillNumber");
                PlayerPrefs.DeleteKey("KillNumber");
                KillNumberGet(targetNumber);
            }

            if (PlatformManager.Instance.Platform == "Oculus")
            {
                ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;
                AimMode = (AimMode)(int)customProperties["AimMode"];
                // PlayerPrefs.DeleteKey("AimMode");
            }

            if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
                isPracticeMode = true;
                onSetKills = true;
            }

            state = GameState.Playing;
        }
    }

    void Start()
    {
    }


    void Update()
    {
        // if (PlatformManager.Instance.Platform == "Windows")
        // {
        //     PlatformManager.Instance.WindowAspectRatio();
        // }

        if (!isStart)
        {
            return;
        }

        if (!allowInput)
        {
            waitTime -= Time.deltaTime;
            if (waitTime <= 0)
            {
                allowInput = true;
            }
            return;
        }

        if (state == GameState.Playing)
        {
            // タブキー検知でスコアボード表示切り替え
            if (Input.GetKeyDown(KeyCode.Tab)
            || OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.LTouch)) // X
            {
                // 内容を更新しつつスコアボードを開く
                ShowScoreboard();
            }
            else if (Input.GetKeyUp(KeyCode.Tab)
            || OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.LTouch)) // X
            {
                uIManager.ChangeScoreUI(false);
                uIManager.ShowHelpBox();
            }

            // ヘルプレコード表示切替
            if (Input.GetKeyDown(KeyCode.Q)
            || OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.LTouch)) // Y
            {
                uIManager.ChangeHelpRecord();
            }
        }

        if (AllowLeaveRoom)
        {
            if (Input.GetMouseButton(0)
            || OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)
            || OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch)
            || Input.GetKeyDown(KeyCode.Escape)
            || Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.Space)
            || OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.LTouch))
            {
                ProcessingAfterCompletion();
            }
        }
    }

    // コールバック関数(イベントが発生したときに実行される)
    public void OnEvent(EventData photonEvent)
    {
        // カスタムイベントなのか
        if (photonEvent.Code < 200) // Photon独自イベントは２００以上
        {
            // イベントコード格納
            EventCodes eventCode = (EventCodes)photonEvent.Code;

            // 送られてきたイベントデータを格納
            object[] data = (object[])photonEvent.CustomData;

            switch (eventCode)
            {
                case EventCodes.NewPlayer:
                    NewPlayerSet(data);
                    break;

                case EventCodes.ListPlayers:
                    ListPlayersSet(data);
                    break;

                case EventCodes.UpdateState:
                    ScoreSet(data);
                    break;

                case EventCodes.SetTargetNumber:
                    TargetNumberSet(data);
                    break;
            }
        }
    }

    // コールバック登録・解除
    public override void OnEnable() // コンポーネントがONになったら呼ばれる
    {
        // GameManagerをコールバックに登録
        PhotonNetwork.AddCallbackTarget(this);
    }

    // コールバック解除
    public override void OnDisable() // コンポーネントがOFFになったら呼ばれる
    {
        // GameManagerをコールバックから解除
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // 新規ユーザーがネットワーク経由でマスターに自分の情報を送る
    public void NewPlayerGet(string name)
    {
        object[] info = new object[4];
        info[0] = name;
        info[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        info[2] = 0;
        info[3] = 0;

        // 新規ユーザー発生イベント
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NewPlayer,
            info,
            // マスターのみに送る
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true });
    }

    // 送られてきた新プレイヤー情報をリストに格納 (マスターだけが呼ぶ)
    public void NewPlayerSet(object[] data)
    {
        // プレイヤー情報を変数に格納
        PlayerInfo player = new PlayerInfo((string)data[0], (int)data[1], (int)data[2], (int)data[3]);

        playerList.Add(player);

        // 取得したプレイヤー情報をルーム内の全プレイヤーに送信する
        ListPlayersGet();
    }

    public void ListPlayersGet()
    {
        // 送信するユーザー情報を格納
        object[] info = new object[playerList.Count + 1];

        info[0] = state;

        for (int i = 0; i < playerList.Count; i++)
        {
            object[] temp = new object[4];

            // ユーザー情報を格納
            temp[0] = playerList[i].name;
            temp[1] = playerList[i].actor;
            temp[2] = playerList[i].kills;
            temp[3] = playerList[i].deaths;

            info[i + 1] = temp;
        }

        // 情報共有イベントを発生させる
        PhotonNetwork.RaiseEvent(
           (byte)EventCodes.ListPlayers,
           info,
           // 全員に送る
           new RaiseEventOptions { Receivers = ReceiverGroup.All },
           new SendOptions { Reliability = true });
    }

    // 新しいプレイヤー情報をリストに格納
    public void ListPlayersSet(object[] data)
    {
        // プレイヤー情報初期化
        playerList.Clear();

        state = (GameState)data[0];

        for (int i = 1; i < data.LongLength; i++)
        {
            object[] info = (object[])data[i];

            PlayerInfo player = new PlayerInfo((string)info[0], (int)info[1], (int)info[2], (int)info[3]);

            playerList.Add(player);
        }

        // ゲームの状態を判定する関数
        StateCheck();
    }

    // キル数やデス数を取得してイベントを発生させる
    public void ScoreGet(int actor, int state, int amount)
    {
        // 引数の値を配列に格納
        object[] package = new object[] { actor, state, amount };

        // キルデスイベント発生
        PhotonNetwork.RaiseEvent(
          (byte)EventCodes.UpdateState,
          package,
          // 全員に送る
          new RaiseEventOptions { Receivers = ReceiverGroup.All },
          new SendOptions { Reliability = true });
    }

    public void ScoreSet(object[] data)
    {
        int actor = (int)data[0];
        int state = (int)data[1]; // 0:キル 1:デス
        int amount = (int)data[2];

        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].actor == actor)
            {
                switch (state)
                {
                    case 0:
                        playerList[i].kills += amount;
                        break;
                    case 1:
                        playerList[i].deaths += amount;
                        break;
                }
                break;
            }
        }

        // ゲームクリア条件を達成したか確認する関数
        TargetScoreCheck();
    }

    public void KillNumberGet(int killNumber)
    {
        object[] data = new object[] { killNumber };

        PhotonNetwork.RaiseEvent(
         (byte)EventCodes.SetTargetNumber,
         data,
         // 全員に送る
         new RaiseEventOptions { Receivers = ReceiverGroup.All },
         new SendOptions { Reliability = true });
    }
    public void TargetNumberSet(object[] data)
    {
        targetNumber = (int)data[0];
        onSetKills = true;
        Debug.Log("TtargetNumber: " + targetNumber);
    }

    // 内容を更新しつつスコアボードを開く
    public void ShowScoreboard()
    {
        // ヘルプボックス非表示
        uIManager.HideHelpBox();

        // スコアボードを開く
        uIManager.ChangeScoreUI(true);

        // 表示されているスコアボードを一旦すべて削除
        foreach (PlayerInfomation info in playerInfoList)
        {
            Destroy(info.gameObject);
        }
        playerInfoList.Clear();

        playerList = playerList.OrderByDescending(p => p.kills).ThenBy(p => p.deaths).ThenBy(p => p.name).ToList();

        foreach (PlayerInfo player in playerList)
        {
            // スコアボードを作成して格納
            PlayerInfomation newPlayerDisplay = Instantiate(uIManager.info, uIManager.info.transform.parent);

            // UIに情報を格納
            newPlayerDisplay.SetPlayerDetails(player.name, player.kills, player.deaths, state);

            // 表示
            newPlayerDisplay.gameObject.SetActive(true);

            // リストに追加
            playerInfoList.Add(newPlayerDisplay);
        }

        if (state == GameState.Ending)
        {
            PlayerInfomation topPlayer = playerInfoList.OrderByDescending(info => int.Parse(info.killsText.text)).First();
            topPlayer.SetTopPlayer();
            if (PhotonNetwork.LocalPlayer.NickName == topPlayer.playerNameText.text)
            {
                uIManager.UpdateEndText();
            }
        }
    }

    // ゲームクリア条件を達成したか確認する関数
    public void TargetScoreCheck()
    {
        bool clear = false;
        foreach (PlayerInfo player in playerList)
        {
            //キル数判定
            if (player.kills >= targetNumber && targetNumber > 0)
            {
                clear = true;
                break;
            }
        }

        if (clear)
        {
            if (PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                state = GameState.Ending;

                // 取得したプレイヤー情報をルーム内の全プレイヤーに送信する
                ListPlayersGet();
            }
        }
    }

    // ゲームの状態を判定する関数
    public void StateCheck()
    {
        if (state == GameState.Ending)
        {
            // ゲーム終了関数
            EndGame();
        }
    }

    // ゲーム終了関数
    public void EndGame()
    {
        // 全てのネットワークオブジェクトを削除
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }

        uIManager.IsEnd = true;
        spawnManager.IsEnd = true;

        Invoke("ShowEndPanel", 4.5f);
    }

    private void ShowEndPanel()
    {
        if (PlatformManager.Instance.Platform == "Oculus")
        {
            uIManager.scoreboardRect.localPosition = new Vector3(uIManager.scoreboardRect.localPosition.x, uIManager.scoreboardRect.localPosition.y, 0);
        }
        // スコアパネル表示
        ShowScoreboard();

        // ゲーム終了パネル表示
        uIManager.OpenEndPanel(waitAfterEnding);

        // // 全ての"Player"タグのオブジェクトを非表示
        // GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        // foreach (GameObject player in players)
        // {
        //     player.SetActive(false);
        // }

        // カーソルの表示
        Cursor.lockState = CursorLockMode.None;

        // 終了後の処理
        Invoke("ProcessingAfterCompletion", waitAfterEnding);
    }

    // 終了後の処理
    private void ProcessingAfterCompletion()
    {
        if (!PhotonNetwork.IsConnectedAndReady) return;

        // シーンの同期解除
        PhotonNetwork.AutomaticallySyncScene = false;

        // 自分のPlayerPrefs.GetString("playerName")を削除
        if (PlayerPrefs.HasKey("playerName"))
        {
            PlayerPrefs.DeleteKey("playerName");
        }

        // ルームを抜ける
        PhotonNetwork.LeaveRoom();
    }


    // ルームを抜けたときに呼ばれる
    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    // プレイヤー情報を管理しているリストから、プレイヤー情報を削除して共有
    public void OutPlayerGet(int actor)
    {
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].actor == actor)
            {
                playerList.RemoveAt(i);

                break;
            }
        }

        ListPlayersGet();
    }

    // プレイヤーがルームに入ったときに呼び出される関数（継承：コールバック）
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        NewPlayerGet(newPlayer.NickName);
        state = GameState.Playing;
    }
}


// プレイヤーの情報を扱うクラス
[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor, kills, deaths;

    public PlayerInfo(string name, int actor, int kills, int deaths)
    {
        this.name = name;
        this.actor = actor;
        this.kills = kills;
        this.deaths = deaths;
    }
}