using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.EventSystems;
using System.Linq;
using DG.Tweening;
using System.Collections;
using ExitGames.Client.Photon; // IOnEventCallback

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
    public TextMeshProUGUI titleText;
    public Material titleTextMaterial;
    public Material titleTextFlushMaterial;
    public CanvasGroup titleTextCanvasGroup;
    public TextMeshProUGUI titleStartText;
    public CanvasGroup titleStartTextCanvasGroup;
    public RectTransform titleStartTextRectTransform;
    public GameObject textUnderLine;
    public GameObject titleFlushImage;
    bool onTitleText = false;
    public CanvasGroup closeMenuUICanvasGroup;

    // ロードパネル
    public GameObject loadingPanel;
    public CanvasGroup loadingCanvasGroup;

    // ロードテキスト
    public TextMeshProUGUI loadingText;

    // ボタンの親のオブジェクト
    public GameObject buttons;
    public CanvasGroup masterButtonsCanvasGroup;

    // ルームパネル
    public GameObject createRoomPanel;
    public CanvasGroup createRoomCanvasGroup;

    // ルーム名の入力テキスト
    public TMP_InputField enterRoomNameInputField;
    public TextMeshProUGUI enterRoomName;

    // ルームパネル
    public GameObject roomPanel;
    public CanvasGroup roomPanelCanvasGroup;

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

    public List<Image> buttonsHighLightImages;
    public List<GameObject> buttonsBlackImages;
    GameObject black;
    GameObject previousBlack;
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
    public int killNumber = 3;

    public GameObject aimModeButton;
    public AimMode aimMode = AimMode.RightHand;
    public TextMeshProUGUI aimModeText;

    Sequence titleStartSequence;

    public AudioSource audioSource;
    public AudioClip hoverSE;
    public AudioClip nameInputSE;
    public AudioClip titleSE;
    public AudioClip clickToStartSE;
    public AudioClip enterSE;
    public AudioClip exitSE;


    public GameObject battleRoomHelp;
    public RectTransform battleRoomHelpLine1;
    public RectTransform battleRoomHelpLine2;
    public RectTransform battleRoomHelpLine3;
    public RectTransform battleRoomHelpBackground;
    public TextMeshProUGUI battleRoomHelpText;
    public TextMeshProUGUI battleRoomCreateHelpText;
    public TextMeshProUGUI battleRoomSelectHelpText;
    public CanvasGroup battleRoomHelpTextCanvasGroup;
    public CanvasGroup battleRoomCreateHelpTextCanvasGroup;
    public CanvasGroup battleRoomSelectHelpTextCanvasGroup;


    public GameObject aimModeHelp;
    public RectTransform aimModeHelpLine1;
    public RectTransform aimModeHelpLine2;
    public RectTransform aimModeHelpLine3;
    public RectTransform aimModeHelpBackground;
    public TextMeshProUGUI rightHandModeHelpText;
    public TextMeshProUGUI headSetModeHelpText;
    public CanvasGroup rightHandModeHelpTextCanvasGroup;
    public CanvasGroup headSetModeHelpTextCanvasGroup;


    string hoverUIParentName;
    string prevHoverUIParentName;

    public GameObject vRCautionPanel;
    public CanvasGroup vRCautionTextCanvasGroup;
    public GameObject oKTextObject;

    // private float lastServiceTime;
    // public float serviceInterval = 5.0f; // 5秒ごとにイベントを送信

    void OpenBattleRoomHelp()
    {
        StartCoroutine(InputInterval(1.2f));
        battleRoomHelpLine1.localScale = new Vector3(0, battleRoomHelpLine1.localScale.y, battleRoomHelpLine1.localScale.z);
        battleRoomHelpLine2.localScale = new Vector3(battleRoomHelpLine2.localScale.x, 0, battleRoomHelpLine2.localScale.z);
        battleRoomHelpLine3.localScale = new Vector3(0, battleRoomHelpLine3.localScale.y, battleRoomHelpLine3.localScale.z);
        battleRoomHelpBackground.localScale = new Vector3(battleRoomHelpBackground.localScale.x, 0, battleRoomHelpBackground.localScale.z);
        battleRoomHelpTextCanvasGroup.alpha = 0;
        battleRoomCreateHelpTextCanvasGroup.alpha = 0;
        battleRoomSelectHelpTextCanvasGroup.alpha = 0;
        battleRoomHelpText.gameObject.SetActive(true);
        battleRoomCreateHelpText.gameObject.SetActive(false);
        battleRoomSelectHelpText.gameObject.SetActive(false);
        battleRoomHelp.SetActive(true);
        Sequence helpSequence = DOTween.Sequence();
        helpSequence.Append(battleRoomHelpLine1.DOScaleX(1, 0.2f))
                     .Append(battleRoomHelpLine2.DOScaleY(1, 0.4f))
                     .Append(battleRoomHelpLine3.DOScaleX(1, 0.2f))
                     .Append(battleRoomHelpBackground.DOScaleY(1, 0.5f))
                     .Append(battleRoomHelpTextCanvasGroup.DOFade(1, 0.7f));
    }
    void OpenAimModeHelp()
    {
        StartCoroutine(InputInterval(1.2f));
        aimModeHelpLine1.localScale = new Vector3(0, aimModeHelpLine1.localScale.y, aimModeHelpLine1.localScale.z);
        aimModeHelpLine2.localScale = new Vector3(aimModeHelpLine2.localScale.x, 0, aimModeHelpLine2.localScale.z);
        aimModeHelpLine3.localScale = new Vector3(0, aimModeHelpLine3.localScale.y, aimModeHelpLine3.localScale.z);
        aimModeHelpBackground.localScale = new Vector3(aimModeHelpBackground.localScale.x, 0, aimModeHelpBackground.localScale.z);
        rightHandModeHelpTextCanvasGroup.alpha = 0;
        headSetModeHelpTextCanvasGroup.alpha = 0;
        aimModeHelp.SetActive(true);
        Sequence helpSequence = DOTween.Sequence();
        helpSequence.Append(aimModeHelpLine1.DOScaleX(1, 0.2f))
                     .Append(aimModeHelpLine2.DOScaleY(1, 0.4f))
                     .Append(aimModeHelpLine3.DOScaleX(1, 0.2f))
                     .Append(aimModeHelpBackground.DOScaleY(1, 0.5f))
                     .Append(rightHandModeHelpTextCanvasGroup.DOFade(aimMode == AimMode.RightHand ? 1 : 0, 0.7f))
                     .Join(headSetModeHelpTextCanvasGroup.DOFade(aimMode == AimMode.HeadSet ? 1 : 0, 0.7f));
    }

    void UpdateAimModeHelpText()
    {
        rightHandModeHelpTextCanvasGroup.alpha = 0;
        headSetModeHelpTextCanvasGroup.alpha = 0;
        Sequence helpSequence = DOTween.Sequence();
        helpSequence.Append(rightHandModeHelpTextCanvasGroup.DOFade(aimMode == AimMode.RightHand ? 1 : 0, 1.0f))
                     .Join(headSetModeHelpTextCanvasGroup.DOFade(aimMode == AimMode.HeadSet ? 1 : 0, 1.0f));
    }

    void UpdateBattleRoomHelpText(string hoverUIParentName)
    {
        if (!battleRoomHelp.activeSelf || !allowInput
        || (hoverUIParentName == null && prevHoverUIParentName == null)
        || (hoverUIParentName == prevHoverUIParentName)) return;

        battleRoomHelpText.gameObject.SetActive(false);
        battleRoomCreateHelpText.gameObject.SetActive(false);
        battleRoomSelectHelpText.gameObject.SetActive(false);

        battleRoomHelpTextCanvasGroup.alpha = 0;
        battleRoomCreateHelpTextCanvasGroup.alpha = 0;
        battleRoomSelectHelpTextCanvasGroup.alpha = 0;

        if (hoverUIParentName == null)
        {
            battleRoomHelpText.gameObject.SetActive(true);
            Sequence helpSequence = DOTween.Sequence();
            helpSequence.Append(battleRoomHelpTextCanvasGroup.DOFade(1, 0.7f));
        }
        else if (hoverUIParentName == "CreateRoomButton")
        {
            battleRoomCreateHelpText.gameObject.SetActive(true);
            Sequence helpSequence = DOTween.Sequence();
            helpSequence.Append(battleRoomCreateHelpTextCanvasGroup.DOFade(1, 0.7f));
        }
        else if (hoverUIParentName == "FindRoomButton")
        {
            battleRoomSelectHelpText.gameObject.SetActive(true);
            Sequence helpSequence = DOTween.Sequence();
            helpSequence.Append(battleRoomSelectHelpTextCanvasGroup.DOFade(1, 0.7f));
        }
        else
        {
            battleRoomHelpTextCanvasGroup.alpha = 1;
            battleRoomHelpText.gameObject.SetActive(true);
        }
    }

    public bool allowInput { get; private set; } = true;
    public void ChangeAimMode()
    {
        if (!allowInput) return;

        if (aimMode == AimMode.RightHand)
        {
            aimMode = AimMode.HeadSet;
        }
        else
        {
            aimMode = AimMode.RightHand;
        }
        audioSource.PlayOneShot(enterSE);
        UpdateAimModeHelpText();
    }
    private void OnDestroy()
    {
        DOTween.KillAll();
    }
    private void Awake()
    {
        // static 変数に格納
        instance = this;
        allowInput = true;
        battleRoomHelp.SetActive(false);
        aimModeHelp.SetActive(false);

        titleStartSequence = DOTween.Sequence()
       .Append(titleStartTextCanvasGroup.DOFade(0, 0.7f))
       .Append(titleStartTextCanvasGroup.DOFade(1, 0.7f))
       .SetLoops(-1);

        if (PlatformManager.Instance.Platform == "Windows")
        {
            pointerEventData = new PointerEventData(eventSystemWindows);
            aimModeButton.SetActive(true);
            titleStartText.text = "Click to Start";
        }
        else if (PlatformManager.Instance.Platform == "Oculus")
        {
            oVRPointerEventData = new OVRPointerEventData(eventSystemOculus);
            aimModeButton.SetActive(true);
            titleStartText.text = "Press to Start";
        }
    }

    public IEnumerator InputInterval(float seconds = 1.5f)
    {
        allowInput = false;
        yield return new WaitForSeconds(seconds);
        allowInput = true;
    }

    void Update()
    {

        // if (PhotonNetwork.IsConnected && Time.time - lastServiceTime > serviceInterval)
        // {
        //     // イベントコードを定義（ここでは1を使用）
        //     byte eventCode = 1;
        //     // イベントデータ（ここではnullを使用）
        //     object content = null;
        //     // イベントの送信オプション
        //     RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        //     // 信頼性のある送信方法を選択
        //     SendOptions sendOptions = new SendOptions { Reliability = true };

        //     // イベントを送信
        //     PhotonNetwork.RaiseEvent(eventCode, content, raiseEventOptions, sendOptions);

        //     // 最後のサービス時刻を更新
        //     lastServiceTime = Time.time;

        //     Debug.Log("KeepAlive event sent");
        // }

        HandleHoverUI();
        if (PlatformManager.Instance.Platform == "Oculus" && buttons.activeSelf)
        {
            if (aimMode == AimMode.RightHand) aimModeText.text = "RIGHT HAND";
            else aimModeText.text = "HEAD SET";

            // カスタムプロパティにプラットフォーム情報を設定
            ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
            {
                { "Platform", PlatformManager.Instance.Platform },
                { "AimMode", (int)aimMode }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
        }

        if (onTitleText && allowInput)
        {
            if (Input.GetMouseButtonDown(0)
            || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)
            || OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch))
            {
                StartCoroutine(InputInterval());
                titleStartSequence.Pause();
                audioSource.PlayOneShot(clickToStartSE);
                StartCoroutine(TitleFlushText());
                onTitleText = false;
            }
        }

        if (roomPanel.activeSelf)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                SetKillNumber();
            }
        }
    }
    IEnumerator TitleFlushText()
    {
        titleText.fontMaterial = titleTextFlushMaterial;
        // yield return new WaitForSeconds(0.1f);
        yield return LobbyMenuDisplay();
        titleText.fontMaterial = titleTextMaterial;
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

        if (hoverUI != null)
        {
            if (hoverUI.transform.parent.name == "CreateRoomButton"
            || hoverUI.transform.parent.name == "FindRoomButton")
            {
                if (!battleRoomHelp.activeSelf)
                {
                    OpenBattleRoomHelp();
                }
            }
            else if (hoverUI.transform.parent.name == "AimModeButton")
            {
                if (!aimModeHelp.activeSelf)
                {
                    OpenAimModeHelp();
                }
            }
            hoverUIParentName = hoverUI.transform.parent.name;
        }
        else
        {
            hoverUIParentName = null;
        }

        UpdateBattleRoomHelpText(hoverUIParentName);

        black = hoverUI != null ? hoverUI.transform.GetChild(1).gameObject : null;

        if (black != null && black != previousBlack)
        {
            audioSource.PlayOneShot(hoverSE);
        }

        foreach (GameObject blackImage in buttonsBlackImages)
        {
            if (black == blackImage)
            {
                if (allowInput) blackImage.SetActive(false);
                else blackImage.SetActive(true);
            }
            else
            {
                blackImage.SetActive(true);
            }
        }

        previousBlack = black;
        prevHoverUIParentName = hoverUIParentName;
    }

    private void Start()
    {
        if (PlatformManager.Instance.Platform == "Windows")
        {
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
            nameInputBackImage.color = Color.clear;
            oVRCameraRig.SetActive(true);
            mainCamera.SetActive(false);
            eventSystemObjectOculus.SetActive(true);
            eventSystemObjectWindows.SetActive(false);
            aimModeButton.SetActive(true);

            if (PlatformManager.Instance.IsEditor)
            {
                eventSystemObjectOculus.SetActive(false);
                eventSystemObjectWindows.SetActive(true);
            }
        }
        titleImage.SetActive(false);
        titleText.gameObject.SetActive(false);
        textUnderLine.SetActive(false);
        titleStartTextCanvasGroup.gameObject.SetActive(false);

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
        titleText.gameObject.SetActive(false);
        textUnderLine.SetActive(false);
        titleStartTextCanvasGroup.gameObject.SetActive(false);
        titleFlushImage.SetActive(false);
    }

    public IEnumerator FadeOutMenuUI(CanvasGroup canvasGroup, float times = 1f)
    {
        foreach (var image in buttonsHighLightImages)
        {
            image.enabled = false;
        }
        canvasGroup.alpha = 1;
        canvasGroup.DOFade(0, times);
        yield return new WaitForSeconds(times);
        canvasGroup.alpha = 1;
        foreach (var image in buttonsHighLightImages)
        {
            image.enabled = true;
        }
    }

    public IEnumerator FadeInMenuUI(CanvasGroup canvasGroup, float times = 1f, bool inQuad = false)
    {
        canvasGroup.alpha = 0;
        if (!inQuad) canvasGroup.DOFade(1, times);
        else canvasGroup.DOFade(1, times).SetEase(Ease.OutQuad);
        yield return new WaitForSeconds(times);
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

    public IEnumerator ShowTitle()
    {
        CloseMenuUI();
        textUnderLine.transform.localScale = new Vector3(0, textUnderLine.transform.localScale.y, textUnderLine.transform.localScale.z);
        textUnderLine.SetActive(true);
        titleImage.SetActive(true);
        titleText.gameObject.SetActive(true);
        titleFlushImage.SetActive(true);
        titleText.GetComponent<CanvasGroup>().alpha = 0;

        yield return new WaitForSeconds(1.0f);

        yield return FadeOutMenuUI(closeMenuUICanvasGroup, 1.5f);
        titleFlushImage.SetActive(false);

        DOTween.Sequence()
            .Append(textUnderLine.transform.DOScaleX(1, 0.8f))
            .Append(titleTextCanvasGroup.DOFade(1, 1.5f).SetEase(Ease.InOutCubic));
        // audioSource.PlayOneShot(titleSE);

        yield return new WaitForSeconds(2.7f);
        titleStartTextCanvasGroup.alpha = 0;
        titleStartTextCanvasGroup.gameObject.SetActive(true);
        titleStartSequence.Play();

        yield return new WaitForSeconds(0.3f);
        onTitleText = true;
    }
    // ロビーUIを表示する関数
    public IEnumerator LobbyMenuDisplay(float times = 1.0f)
    {
        yield return FadeOutMenuUI(closeMenuUICanvasGroup, times);

        CloseMenuUI();
        battleRoomHelp.SetActive(false);
        aimModeHelp.SetActive(false);
        allowInput = false;
        buttons.SetActive(true);
        yield return FadeInMenuUI(closeMenuUICanvasGroup);
    }

    public void RoomButtonSE()
    {
        audioSource.PlayOneShot(enterSE);
    }

    public void LobbyMenuDisplayButton()
    {
        if (allowInput)
        {
            StartCoroutine(InputInterval());
            audioSource.PlayOneShot(exitSE);
            StartCoroutine(LobbyMenuDisplay(0.5f));
        }

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

        // 辞書の初期化
        roomsList.Clear();

        // NickNameは参加中のユーザー名
        PhotonNetwork.NickName = Random.Range(0, 1000).ToString();

        if (PlatformManager.Instance.Platform == "Windows")
        {
            // 名前が入力済みか確認してUI更新
            ConfirmationName();
        }
        else if (PlatformManager.Instance.Platform == "Oculus")
        {
            if (PlatformManager.Instance.IsStarted)
            {
                ConfirmationName();
            }
            else
            {
                OpenVRCautionPanel();
            }
        }
    }

    void OpenVRCautionPanel()
    {
        StartCoroutine(OpenVRCautionPanelCoroutine());
    }

    IEnumerator OpenVRCautionPanelCoroutine()
    {
        CloseMenuUI();
        vRCautionTextCanvasGroup.alpha = 0;
        vRCautionPanel.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        yield return FadeInMenuUI(vRCautionTextCanvasGroup, 2.0f);
        yield return new WaitForSeconds(0.5f);
        oKTextObject.SetActive(true);
    }

    public void CloseVRCautionPanelButton()
    {
        StartCoroutine(CloseVRCautionPanelCoroutine());
    }

    IEnumerator CloseVRCautionPanelCoroutine()
    {
        audioSource.PlayOneShot(nameInputSE);
        yield return FadeOutMenuUI(vRCautionTextCanvasGroup, 1.0f);
        vRCautionPanel.SetActive(false);
        PlatformManager.Instance.IsStarted = true;
        ConfirmationName();
    }
    // ルームを作るボタン用の関数
    public void OpenCreateRoomPanelButton()
    {
        if (allowInput)
        {
            StartCoroutine(InputInterval());
            StartCoroutine(OpenCreateRoomPanel());
        }
    }
    public IEnumerator OpenCreateRoomPanel()
    {
        audioSource.PlayOneShot(enterSE);
        yield return FadeOutMenuUI(closeMenuUICanvasGroup, 0.8f);
        CloseMenuUI();
        createRoomPanel.SetActive(true);
        yield return FadeInMenuUI(closeMenuUICanvasGroup, 0.5f);
    }

    // ルーム作成ボタン用の関数
    public void CreateRoomButton()
    {
        // 入力されたとき
        if (!string.IsNullOrEmpty(enterRoomName.text) && allowInput)
        {
            StartCoroutine(InputInterval());

            audioSource.PlayOneShot(enterSE);

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
        StartCoroutine(JoinRoom());
    }

    public IEnumerator JoinRoom()
    {
        StartCoroutine(InputInterval());

        // 入室中のルーム名を取得
        roomName.text = PhotonNetwork.CurrentRoom.Name;

        // ルームにいるプレイヤー情報を取得
        GetAllPlayer();

        roomPanel.SetActive(true);
        masterButtonsCanvasGroup.alpha = 0;

        StartCoroutine(FadeInMenuUI(roomPanelCanvasGroup, 0.5f));

        StartCoroutine(FadeOutMenuUI(loadingCanvasGroup));
        // CloseMenuUI();
        yield return new WaitForSeconds(0.5f);
        loadingPanel.SetActive(false);

        // マスターか判定してボタン表示
        CheckRoomMaster();
        yield return FadeInMenuUI(masterButtonsCanvasGroup, 1f, true);
    }

    // ルーム退出の関数
    public void LeaveRoom()
    {
        if (allowInput)
        {
            StartCoroutine(InputInterval());
            audioSource.PlayOneShot(exitSE);
            // ルームから退出
            PhotonNetwork.LeaveRoom();

            enterRoomNameInputField.text = "";

            // UI
            CloseMenuUI();
            loadingText.text = "退出中…";
            loadingPanel.SetActive(true);
        }
    }

    // ルーム退出時に呼ばれる関数
    public override void OnLeftRoom()
    {
        // ロビーUI表示
        StartCoroutine(LobbyMenuDisplay());
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
    public void FindRoomButton()
    {
        if (allowInput)
        {
            StartCoroutine(InputInterval());
            StartCoroutine(FindRoom());
        }
    }
    public IEnumerator FindRoom()
    {
        audioSource.PlayOneShot(enterSE);
        yield return FadeOutMenuUI(closeMenuUICanvasGroup, 0.8f);
        CloseMenuUI();
        roomListPanel.SetActive(true);
        yield return FadeInMenuUI(closeMenuUICanvasGroup, 0.5f);
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
            StartCoroutine(InputInterval());

            // ユーザー名登録
            PhotonNetwork.NickName = nameInput.text;

            // "playerName"キーで保存
            PlayerPrefs.SetString("playerName", nameInput.text);

            audioSource.PlayOneShot(nameInputSE);
            // UI
            // LobbyMenuDisplay();
            StartCoroutine(ShowTitle());

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
            killNumberText.text = killNumber.ToString();
            targetNumber.SetActive(true);
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
            audioSource.PlayOneShot(nameInputSE);
        }
        killNumberText.text = killNumber.ToString();
        BroadcastKillNumber();
    }

    // killNumberをデクリメントして、テキストに反映する関数
    public void DecrementKillNumber()
    {
        if (killNumber > 1)
        {
            killNumber--;
            audioSource.PlayOneShot(nameInputSE);
        }
        killNumberText.text = killNumber.ToString();
        BroadcastKillNumber();
    }

    // ルームマスターのkillNumberを全てのローカルユーザーに送信する関数
    public void BroadcastKillNumber()
    {
        // カスタムプロパティにkillNumberを設定
        ExitGames.Client.Photon.Hashtable customProperties = new ExitGames.Client.Photon.Hashtable
        {
            { "KillNumber", killNumber }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
    }

    public void SetKillNumber()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("KillNumber"))
        {
            killNumber = (int)PhotonNetwork.CurrentRoom.CustomProperties["KillNumber"];
            killNumberText.text = killNumber.ToString();
        }
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
        if (allowInput)
        {
            allowInput = false;
            StartCoroutine(Play());
        }
    }

    IEnumerator Play()
    {
        audioSource.PlayOneShot(clickToStartSE);

        // 入室中のルームを満室にして閉鎖する
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        if (PhotonNetwork.IsMasterClient)
        {
            PlayerPrefs.SetInt("KillNumber", killNumber);
        }

        yield return new WaitForSeconds(1.3f);

        // ステージをよみこむ
        PhotonNetwork.LoadLevel(levelToPlay);
    }
}
