using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;
using System.Linq;
using DG.Tweening;
// using System.Collections;
public class PhotonManager : MonoBehaviourPunCallbacks // MonoBehaviourとPhotonの機能の継承
{
    // static 変数
    public static PhotonManager instance;

    public Canvas canvas;
    public Transform canvasSize;
    public GameObject eventSystemObjectWindows;
    public GameObject eventSystemObjectOculus;
    public GameObject mainCamera;
    public GameObject oVRCameraRig;
    public Camera centerEyeAnchor;

    // タイトルイメージ
    public GameObject titleImage;
    public GameObject titleText;

    // ロードパネル
    public GameObject loadingPanel;

    // ロードテキスト
    public TextMeshProUGUI loadingText;

    // ボタンの親のオブジェクト
    public GameObject buttons;

    // ルームパネル
    public GameObject createRoomPanel;

    // ルーム名の入力テキスト
    public TMP_InputField enterRoomNameInputField;
    public TextMeshProUGUI enterRoomName;

    // ルームパネル
    public GameObject roomPanel;

    // ルームネーム
    public TextMeshProUGUI roomName;

    // エラーパネル
    public GameObject errorPanel;

    // エラーネーム
    public TextMeshProUGUI errorText;

    // ルーム一覧
    public GameObject roomListPanel;

    // ルームボタン格納
    public Room originalRoomButton;

    // ルームボタンの親オブジェクト
    public GameObject roomButtonContent;

    // ルーム情報を扱う辞書（ルーム名：情報）
    Dictionary<string, RoomInfo> roomsList = new Dictionary<string, RoomInfo>();

    // ルームボタンを扱うリスト
    private List<Room> allRoomButtons = new List<Room>();

    // 名前テキスト
    public TextMeshProUGUI playerNameText;

    // 名前テキスト格納リスト
    private List<TextMeshProUGUI> allPlayerNames = new List<TextMeshProUGUI>();

    // 名前テキストの親オブジェクト
    public GameObject playerNameContent;

    // 名前入力パネル
    public GameObject nameInputPanel;

    // 名前入力表示テキスト
    public TextMeshProUGUI placeholderText;

    // 入力フィールド
    public TMP_InputField nameInput;

    public Image nameInputBackImage;

    // 名前を入力したか判定
    private bool setName;

    // ボタン格納
    public GameObject startButton;

    // 遷移シーン名
    public string levelToPlay;

    public List<GameObject> buttonsBlackImages;
    GameObject black;
    private PointerEventData pointerEventData;
    private OVRPointerEventData oVRPointerEventData;
    public EventSystem eventSystemWindows;
    public EventSystem eventSystemOculus;
    public GraphicRaycaster raycaster;

    GameObject hoverUI;

    public RectTransform LobbyButtons;


    public OVRRaycaster oVRRaycaster; // Inspectorでアサインする

    public LaserPointer laserPointer;

    public GameObject targetNumber;
    public TextMeshProUGUI killNumberText;
    public Button leftButton;
    public Button rightButton;
    public int killNumber = 1;

    public GameObject aimModeButton;
    public AimMode aimMode = AimMode.RightHand;
    public TextMeshProUGUI aimModeText;

    // bool allowInput = true;
    // IEnumerator InputInterval()
    // {
    //     allowInput = false;
    //     yield return new WaitForSeconds(0.15f);
    //     allowInput = true;
    // }
    public void ChangeAimMode()
    {
        if (aimMode == AimMode.RightHand)
        {
            aimMode = AimMode.Screen;
        }
        else
        {
            aimMode = AimMode.RightHand;
        }
    }

    private void Awake()
    {
        // static 変数に格納
        instance = this;
        if (PlatformManager.Instance.Platform == "Windows")
        {
            pointerEventData = new PointerEventData(eventSystemWindows);
            aimModeButton.SetActive(true);
        }
        else if (PlatformManager.Instance.Platform == "Oculus")
        {
            oVRPointerEventData = new OVRPointerEventData(eventSystemOculus);
            aimModeButton.SetActive(true);
        }
    }

    void Update()
    {
        HandleHoverUI();
        if (PlatformManager.Instance.Platform == "Oculus")
        {
            if (aimMode == AimMode.RightHand) aimModeText.text = "RIGHT HAND";
            else aimModeText.text = "SCREEN";
        }

        if (titleText.activeInHierarchy)
        {
            if (Input.GetMouseButtonDown(0))
            {
                LobbyMenuDisplay();
            }
        }
    }
    void HandleHoverUI()
    {
        List<RaycastResult> results = new List<RaycastResult>();

        if (PlatformManager.Instance.Platform == "Windows")
        {
            pointerEventData.position = Input.mousePosition;
            raycaster.Raycast(pointerEventData, results);
        }
        else if (PlatformManager.Instance.Platform == "Oculus")
        {
            oVRPointerEventData.worldSpaceRay = new Ray(laserPointer.StartPoint, (laserPointer.EndPoint - laserPointer.StartPoint).normalized);
            oVRRaycaster.Raycast(oVRPointerEventData, results);
        }

        hoverUI = results.FirstOrDefault(result => result.gameObject.tag == "HoverUI").gameObject;

        black = hoverUI != null ? hoverUI.transform.GetChild(1).gameObject : null;

        foreach (GameObject blackImage in buttonsBlackImages)
        {
            blackImage.SetActive(black != blackImage);
        }
    }

    private void Start()
    {
        if (PlatformManager.Instance.Platform == "Windows")
        {
            titleImage.SetActive(true);
            mainCamera.SetActive(true);
            oVRCameraRig.SetActive(false);
            eventSystemObjectWindows.SetActive(true);
            eventSystemObjectOculus.SetActive(false);
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = mainCamera.GetComponent<Camera>();
            canvasSize.localScale = Vector3.one;
            aimModeButton.SetActive(false);
        }
        else if (PlatformManager.Instance.Platform == "Oculus")
        {
            titleImage.SetActive(false);
            nameInputBackImage.color = Color.clear;
            oVRCameraRig.SetActive(true);
            mainCamera.SetActive(false);
            eventSystemObjectOculus.SetActive(true);
            eventSystemObjectWindows.SetActive(false);
            LobbyButtons.localPosition = new Vector3(
                LobbyButtons.localPosition.x,
                LobbyButtons.localPosition.y,
                -200
            );
            aimModeButton.SetActive(true);
        }
        titleText.SetActive(false);

        // メニューUIをすべて閉じる関数
        CloseMenuUI();

        // パネルとテキスト更新
        loadingPanel.SetActive(true);
        loadingText.text = "ネットワークに接続中…";

        // ネットワーク未接続なら
        if (!PhotonNetwork.IsConnected)
        {
            // ネットワークに接続する
            PhotonNetwork.ConnectUsingSettings();
            Debug.Log("ネットワーク接続");
        }
    }
    // メニューUIをすべて閉じる関数
    public void CloseMenuUI()
    {
        loadingPanel.SetActive(false);
        buttons.SetActive(false);
        createRoomPanel.SetActive(false);
        roomPanel.SetActive(false);
        errorPanel.SetActive(false);
        roomListPanel.SetActive(false);
        nameInputPanel.SetActive(false);
    }

    void SetPlayerPlatformProperty()
    {
        // PlatformManagerを通じて現在のプラットフォームを取得
        string platform = PlatformManager.Instance.Platform;

        // カスタムプロパティにプラットフォーム情報を設定
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "Platform", platform }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
    }

    public void ShowTitle()
    {
        CloseMenuUI();
        titleImage.SetActive(true);
        titleText.SetActive(true);
    }
    // ロビーUIを表示する関数
    public void LobbyMenuDisplay()
    {
        CloseMenuUI();
        titleText.SetActive(false);
        buttons.SetActive(true);
    }

    // マスターサーバーに接続されたときに呼ばれる関数（継承：コールバック）
    public override void OnConnectedToMaster()
    {
        Debug.Log("マスターサーバー接続");

        // ロビーに参加する前にプラットフォーム情報を設定する
        SetPlayerPlatformProperty();

        // ロビーに接続
        PhotonNetwork.JoinLobby();

        // テキスト更新
        loadingText.text = "ロビーへ参加中…";

        // マスタークライアントと同じレベルをロード
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // ロビーに接続されたときに呼ばれる関数（継承：コールバック）
    public override void OnJoinedLobby()
    {
        Debug.Log("ロビー接続");
        LobbyMenuDisplay();

        // 辞書の初期化
        roomsList.Clear();

        // NickNameは参加中のユーザー名
        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        // 名前が入力済みか確認してUI更新
        ConfirmationName();
    }

    // ルームを作るボタン用の関数
    public void OpenCreateRoomPanel()
    {
        CloseMenuUI();
        createRoomPanel.SetActive(true);
    }

    // ルーム作成ボタン用の関数
    public void CreateRoomButton()
    {
        // 入力されたとき
        if (!string.IsNullOrEmpty(enterRoomName.text))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = 7;

            // ルーム作成
            PhotonNetwork.CreateRoom(enterRoomName.text, options);

            CloseMenuUI();

            // ロードパネル表示
            loadingText.text = "ルーム作成中…";
            loadingPanel.SetActive(true);
        }
    }

    // ルームに参加時に呼ばれる関数（継承；コールバック）
    public override void OnJoinedRoom()
    {
        CloseMenuUI();
        roomPanel.SetActive(true);

        // 入室中のルーム名を取得
        roomName.text = PhotonNetwork.CurrentRoom.Name;

        // ルームにいるプレイヤー情報を取得
        GetAllPlayer();

        // マスターか判定してボタン表示
        CheckRoomMaster();

    }

    // ルーム退出の関数
    public void LeaveRoom()
    {
        // ルームから退出
        PhotonNetwork.LeaveRoom();

        enterRoomNameInputField.text = "";

        // UI
        CloseMenuUI();
        loadingText.text = "退出中…";
        loadingPanel.SetActive(true);
    }

    // ルーム退出時に呼ばれる関数
    public override void OnLeftRoom()
    {
        // ロビーUI表示
        LobbyMenuDisplay();
    }

    // ルーム作成できなかった時に呼ばれる関数（継承：コールバック）
    public override void OnCreateRoomFailed(short returnCode, string messege)
    {
        // UIの表示を変える
        CloseMenuUI();
        errorText.text = "ルーム名が重複しています";

        errorPanel.SetActive(true);
    }

    // ルーム一覧パネルを開く関数
    public void FindRoom()
    {
        CloseMenuUI();
        roomListPanel.SetActive(true);
    }

    // ルームリストに更新があったときに呼ばれる関数（継承；コールバック）
    // ユーザーがロビーにいるときにルームリストが更新されたとき自動で呼ばれる関数
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // ルームボタンUI初期化
        RoomUIinitialization();

        // 辞書に登録
        UpdateRoomLIst(roomList);
    }

    public void UpdateRoomLIst(List<RoomInfo> roomList)
    {
        // 辞書にルーム登録
        for (int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];

            if (info.RemovedFromList) // RemovedFromListは満室か閉鎖ならtrue
            {
                roomsList.Remove(info.Name);
            }
            else
            {
                roomsList[info.Name] = info;
            }
        }

        // ルームボタン表示関数
        RoomListDisplay(roomsList);
    }

    // ルームボタンを作成して表示する関数
    public void RoomListDisplay(Dictionary<string, RoomInfo> cachedRoomList)
    {
        foreach (var roomInfo in cachedRoomList)
        {
            // ボタン作成
            Room newButton = Instantiate(originalRoomButton);

            // ボタンにルーム情報を設定
            newButton.RegisterRoomDetails(roomInfo.Value);

            // 親の設定
            newButton.transform.SetParent(roomButtonContent.transform);


            newButton.transform.localPosition = new Vector3(newButton.transform.localPosition.x, newButton.transform.localPosition.y, 0);
            newButton.transform.localScale = Vector3.one;


            allRoomButtons.Add(newButton);

            // newButtonの子０番目の子１番目オブジェクトをbuttonBlackImageに追加
            GameObject childObject = newButton.gameObject.transform.GetChild(0).GetChild(1).gameObject;
            buttonsBlackImages.Add(childObject);
        }
    }

    public void RoomUIinitialization()
    {
        // ルームUIをすべて削除
        foreach (Room room in allRoomButtons)
        {
            // 削除
            Destroy(room.gameObject);
            GameObject childObject = room.gameObject.transform.GetChild(0).GetChild(1).gameObject;
            if (buttonsBlackImages.Contains(childObject))
            {
                buttonsBlackImages.Remove(childObject);
            }
        }

        // リスト初期化
        allRoomButtons.Clear();
    }

    // 引数のルームに入室する関数
    public void JoinRoom(RoomInfo roomInfo)
    {
        // ルーム入室
        PhotonNetwork.JoinRoom(roomInfo.Name);

        // UI
        CloseMenuUI();

        loadingText.text = "ルーム入室中…";
        loadingPanel.SetActive(true);
    }

    // ルームにいるプレイヤー情報を取得
    public void GetAllPlayer()
    {
        // 名前テキストUI初期化
        InitializePlayerList();

        // プレイヤー表示関数
        PlayerDisplay();
    }

    // 名前テキスト初期化
    public void InitializePlayerList()
    {
        foreach (TextMeshProUGUI playerName in allPlayerNames)
        {
            Destroy(playerName.gameObject);
        }
        allPlayerNames.Clear();

    }

    // プレイヤー表示関数
    public void PlayerDisplay()
    {
        // ルームに参加中の人数分UI作成
        foreach (Player players in PhotonNetwork.PlayerList)
        {
            //UI生成関数
            PlayerTextGeneration(players);
        }
    }

    public void PlayerTextGeneration(Player players)
    {
        // UI生成
        TextMeshProUGUI newPlayerText = Instantiate(playerNameText);

        // テキストに名前反映
        newPlayerText.text = players.NickName;

        // 親オブジェクトの設定
        newPlayerText.transform.SetParent(playerNameContent.transform);

        if (PlatformManager.Instance.Platform == "Windows")
        {
            newPlayerText.transform.localPosition = new Vector3(
                newPlayerText.transform.localPosition.x,
                newPlayerText.transform.localPosition.y,
                0
            );
            newPlayerText.transform.localScale = Vector3.one;
        }
        else if (PlatformManager.Instance.Platform == "Oculus")
        {
            newPlayerText.transform.localPosition = new Vector3(
                newPlayerText.transform.localPosition.x,
                newPlayerText.transform.localPosition.y,
                0
            );
            newPlayerText.transform.localScale = Vector3.one;
        }

        // リストに登録
        allPlayerNames.Add(newPlayerText);
    }

    // 名前が入力済みか確認してUI更新
    public void ConfirmationName()
    {
        if (!setName)
        {
            CloseMenuUI();
            nameInputPanel.SetActive(true);

            if (PlayerPrefs.HasKey("playerName"))
            {
                // placeholderText.text = PlayerPrefs.GetString("playerName");
                // nameInput.text = PlayerPrefs.GetString("playerName");
                PlayerPrefs.DeleteKey("playerName");
            }
        }
        else
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
        }
    }

    public void SetName()
    {
        // 入力フィールドに文字が入力されているか
        if (!string.IsNullOrEmpty(nameInput.text))
        {
            // ユーザー名登録
            PhotonNetwork.NickName = nameInput.text;

            // "playerName"キーで保存
            PlayerPrefs.SetString("playerName", nameInput.text);

            // UI
            // LobbyMenuDisplay();
            ShowTitle();

            setName = true;
        }
    }

    // プレイヤーがルームに入ったときに呼び出される関数（継承：コールバック）
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        PlayerTextGeneration(newPlayer);
    }

    // プレイヤーがルームから離れるか、非アクティブになったときに呼び出される関数（継承：コールバック）
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GetAllPlayer();
    }

    // マスターか判定してボタン表示
    public void CheckRoomMaster()
    {
        // 自分がマスターならTrue
        if (PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(true);
            killNumberText.text = killNumber.ToString();
            targetNumber.SetActive(true);
            leftButton.gameObject.SetActive(true);
            rightButton.gameObject.SetActive(true);

        }
        else
        {
            startButton.SetActive(false);
            targetNumber.SetActive(false);
            leftButton.gameObject.SetActive(false);
            rightButton.gameObject.SetActive(false);
        }
    }

    // killNumberをインクリメントして、テキストに反映する関数
    public void IncrementKillNumber()
    {
        if (killNumber < 5)
        {
            killNumber++;
        }
        killNumberText.text = killNumber.ToString();
    }

    // killNumberをデクリメントして、テキストに反映する関数
    public void DecrementKillNumber()
    {
        if (killNumber > 1)
        {
            killNumber--;
        }
        killNumberText.text = killNumber.ToString();
    }

    // マスターが切り替わったときに呼ばれる関数（継承：コールバック）
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CheckRoomMaster();
        }
    }

    public void PlayGame()
    {
        // 入室中のルームを満室にして閉鎖する
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        // killSetの情報をカスタムプロパティとして設定
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "KillNumber", killNumber },
            { "AimMode", (int)aimMode }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);

        // ステージをよみこむ
        PhotonNetwork.LoadLevel(levelToPlay);
    }
}
