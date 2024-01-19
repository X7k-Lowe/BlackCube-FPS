using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
public class UIManager : MonoBehaviour
{
    public Canvas canvas;


    // 弾薬テキスト
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI maxAmmoText;

    // HPスライダー
    public Slider hpSlider;

    // チェンジUI
    public GameObject gunChangeUI;
    public Slider gunChangeSlider;
    public RectTransform changeIcon;

    public GameObject aimIcon;
    public RectTransform changeAimIcon;

    // 死亡パネル
    public GameObject deathPanel;
    public Image deathPanelBackground;

    // 死亡テキスト
    public TextMeshProUGUI deathText;

    public GameObject killPanel;
    public TextMeshProUGUI killText;


    public GameObject scoreboard;

    public PlayerInfomation info;

    // ヘルプボックス
    public GameObject helpBox;
    public GameObject helpBoxBackground;
    public Image helpBoxBackgroundImage;
    public Sprite winHelpBackgroundSprite;
    public Color winHelpBackgroundColor;
    public Image helpRecord;
    public List<Image> helpRecords;
    public List<TextMeshProUGUI> actionTexts;
    public List<TextMeshProUGUI> commandTexts;
    public List<TextMeshProUGUI> keyHelpTexts;
    public Color keyHelpColor;
    public Color defaultColor;
    public Color highlightColor;
    Color currentKeyHelpTextColor;
    public float highlightDuration = 0.3f; // ハイライトを維持する時間（秒）
    private float mouseHighlightTimer = 0f;
    private float wheelHighlightTimer = 0f;
    private float triggerHighlightTimer = 0f;

    public GameObject mapIconHelpBox;


    // ゲーム終了パネル
    public GameObject endPanel;
    public TextMeshProUGUI endText;
    public Material victoryFontMaterial;

    // マップ
    public Transform mapBackground;
    public GameObject myPlayerObject { get; set; }
    public Material yellowMaterial;
    // GameObject myMapIcon;
    public ObservableList<GameObject> worldObjects = new ObservableList<GameObject>();
    public ObservableList<GameObject> healingCubes = new ObservableList<GameObject>();
    public GameObject mapIconPrefab;
    public GameObject healingCubeMapIconPrefab;
    public Dictionary<GameObject, GameObject> mapIconByWorldObject = new Dictionary<GameObject, GameObject>();
    public bool allowUpdateMapIcon { get; set; } = true;
    public string platform { get; set; }

    bool isHelpBoxActive;
    public bool IsEnd { get; set; } = false;


    public RectTransform canvasSize;
    public GameObject aimIconsParent;
    public GameObject hpUIParent;
    public GameObject helpUIParent;
    public GameObject mapUIParent;
    public RectTransform mapUIParentRect;
    public GameObject panelsUI;
    public GameObject aimIconsUI;
    public GameObject hpUI;
    public GameObject helpUI;
    public GameObject mapUI;

    // プレイヤーキャンバス
    public Canvas playerCanvas { get; set; }
    public RectTransform playerCanvasSize { get; set; }
    public GameObject playerAimIconsUI { get; set; }
    public GameObject playerHpUI { get; set; }
    public GameObject playerHelpUI { get; set; }
    public GameObject playerMapUI { get; set; }

    public bool IsChanging { get; set; } = false;

    public float GameExitCountdown { get; set; }
    bool onCountdown = false;
    public GameObject countdownBox;
    public TextMeshProUGUI countdownText;
    public GameManager gameManager;

    float reSpawnTime;
    float fiveSecond = 5f;

    public LineRenderer laserSight;
    public AimMode ShotMode { get; set; }
    void Update()
    {
        // if (!photonView.IsMine) return;
        UpdateHelpUI();
        // if (myPlayerObject != null && myMapIcon != null) UpdateMapIconPos(myPlayerObject, myMapIcon);
        UpdateAllMapIconsPos();



        if (onCountdown)
        {
            GameExitCountdown -= Time.deltaTime;
            if (GameExitCountdown <= 0)
            {
                GameExitCountdown = 0;
            }
            int count = Mathf.CeilToInt(GameExitCountdown);
            if (count <= 15)
            {
                countdownBox.SetActive(true);
                gameManager.AllowLeaveRoom = true;
            }

            countdownText.text = count.ToString();
        }
        else
        {
            reSpawnTime -= Time.deltaTime;
            fiveSecond -= Time.deltaTime;

            if (reSpawnTime <= 0)
            {
                reSpawnTime = 0;
            }
            int count = Mathf.CeilToInt(reSpawnTime);

            if (fiveSecond <= 0)
            {
                fiveSecond = 0;
                deathText.text = count.ToString();
            }
        }
    }

    public void UpdateEndText()
    {
        endText.text = "Congratulations !";
        endText.fontMaterial = victoryFontMaterial;
    }
    public void SetUIAsChildOfPlayerCanvas()
    {
        if (platform == "Oculus" && ShotMode == AimMode.RightHand)
        {
            aimIconsUI.transform.SetParent(playerAimIconsUI.transform);
            aimIconsUI.transform.localPosition = Vector3.zero;
            aimIconsUI.transform.localScale = Vector3.one;
            aimIconsUI.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        else if (platform == "Oculus" && ShotMode == AimMode.Screen)
        {
            aimIcon.transform.localPosition = new Vector3(aimIconsUI.transform.localPosition.x, aimIconsUI.transform.localPosition.y, 800);
            aimIcon.transform.localScale = Vector3.one * 1.5f;
        }

        panelsUI.transform.SetParent(playerCanvasSize.transform);
        hpUI.transform.SetParent(playerHpUI.transform);
        helpUI.transform.SetParent(playerHelpUI.transform);
        mapUI.transform.SetParent(playerMapUI.transform);

        panelsUI.transform.localPosition = Vector3.zero;
        hpUI.transform.localPosition = Vector3.zero;
        helpUI.transform.localPosition = Vector3.zero;
        mapUI.transform.localPosition = Vector3.zero;

        panelsUI.transform.localScale = Vector3.one;
        hpUI.transform.localScale = Vector3.one;
        helpUI.transform.localScale = Vector3.one;
        mapUI.transform.localScale = Vector3.one;

        panelsUI.transform.localRotation = Quaternion.Euler(0, 0, 0);
        hpUI.transform.localRotation = Quaternion.Euler(0, 0, 0);
        helpUI.transform.localRotation = Quaternion.Euler(0, 0, 0);
        mapUI.transform.localRotation = Quaternion.Euler(0, 0, 0);
        playerMapUI.transform.localScale = Vector3.one * 1.75f;

        deathPanelBackground.enabled = false;

        hpUI.SetActive(true);
    }

    public void ResetUICanvas()
    {
        Debug.Log("ResetUICanvas");
        if (platform == "Oculus" && ShotMode == AimMode.RightHand)
        {
            aimIconsUI.transform.SetParent(aimIconsParent.transform);
            aimIconsUI.transform.localPosition = Vector3.zero;
            aimIconsUI.transform.localScale = Vector3.one;
            aimIconsUI.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        playerMapUI.transform.localScale = Vector3.one;

        panelsUI.transform.SetParent(canvasSize.transform);
        hpUI.transform.SetParent(hpUIParent.transform);
        helpUI.transform.SetParent(helpUIParent.transform);
        mapUI.transform.SetParent(mapUIParent.transform);

        panelsUI.transform.localPosition = Vector3.zero;
        hpUI.transform.localPosition = Vector3.zero;
        helpUI.transform.localPosition = Vector3.zero;
        mapUI.transform.localPosition = Vector3.zero;

        panelsUI.transform.localScale = Vector3.one;
        hpUI.transform.localScale = Vector3.one;
        helpUI.transform.localScale = Vector3.one;
        mapUI.transform.localScale = Vector3.one;

        panelsUI.transform.localRotation = Quaternion.Euler(0, 0, 0);
        hpUI.transform.localRotation = Quaternion.Euler(0, 0, 0);
        helpUI.transform.localRotation = Quaternion.Euler(0, 0, 0);
        mapUI.transform.localRotation = Quaternion.Euler(0, 0, 0);

        hpUI.SetActive(false);
    }
    void Start()
    {
        // イベントにハンドラーを登録
        worldObjects.onAdd += WorldObjectAdded;
        worldObjects.onRemove += WorldObjectRemoved;
        IsEnd = false;
        onCountdown = false;
    }

    void WorldObjectAdded(GameObject worldObject)
    {
        InstantiateMapIcon(worldObject);
    }

    void WorldObjectRemoved(GameObject worldObject)
    {
        DestroyMapIcon(worldObject);
    }

    public void ShowGunChangeUI()
    {
        gunChangeSlider.value = 1.0f;
        changeIcon.localRotation = Quaternion.Euler(0, 0, 0);
        gunChangeUI.SetActive(true);
        changeAimIcon.localRotation = Quaternion.Euler(0, 0, 0);
        aimIcon.SetActive(false);
        changeAimIcon.gameObject.SetActive(true);
    }

    public void HideGunChangeUI()
    {
        gunChangeSlider.value = 0;
        gunChangeUI.SetActive(false);
        if (platform == "Windows" || ShotMode == AimMode.Screen) aimIcon.SetActive(true);
        changeAimIcon.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        // 忘れずにイベントハンドラーを解除
        worldObjects.onAdd -= WorldObjectAdded;
        worldObjects.onRemove -= WorldObjectRemoved;
    }

    public void InstantiateMapIcon(GameObject worldObject)
    {
        GameObject mapIcon = null;
        if (worldObject.CompareTag("Player"))
        {
            mapIcon = Instantiate(mapIconPrefab);
        }
        else if (worldObject.CompareTag("HealingCube"))
        {
            mapIcon = Instantiate(healingCubeMapIconPrefab);
        }

        if (mapIcon != null)
        {
            mapIcon.transform.SetParent(mapBackground);
            mapIcon.transform.localPosition = new Vector3(
                worldObject.transform.position.x * 125 / 160f,
                worldObject.transform.position.z * 125 / 160f,
                0
            );
            mapIcon.transform.localScale = Vector3.one;
            mapIcon.transform.localRotation = Quaternion.Euler(0, 0, 0);
            mapIconByWorldObject.Add(worldObject, mapIcon);
        }
    }

    public void DestroyMapIcon(GameObject worldObject)
    {
        // Destroy(mapIconByWorldObject[worldObject]);
        // mapIconByWorldObject.Remove(worldObject);

        if (worldObject != null && mapIconByWorldObject.ContainsKey(worldObject))
        {
            Destroy(mapIconByWorldObject[worldObject]);
            mapIconByWorldObject.Remove(worldObject);
        }
    }
    public void UpdateAllMapIconsPos()
    {
        if (!allowUpdateMapIcon) return;

        if (mapIconByWorldObject != null)
        {
            List<GameObject> keysToRemove = new List<GameObject>();

            foreach (KeyValuePair<GameObject, GameObject> kvp in mapIconByWorldObject)
            {
                if (kvp.Key == null) keysToRemove.Add(kvp.Key);
                else UpdateMapIconPos(kvp.Key, kvp.Value);
            }

            foreach (GameObject key in keysToRemove)
            {
                mapIconByWorldObject.Remove(key);
            }
        }
    }
    public GameObject GetMyMapIcon()
    {
        GameObject mapIcon = Instantiate(mapIconPrefab);
        mapIcon.transform.SetParent(mapBackground);

        mapIcon.transform.localScale = Vector3.one;
        mapIcon.transform.localRotation = Quaternion.Euler(0, 0, 0);
        return mapIcon;
    }
    public void UpdateMapIconPos(GameObject worldObject, GameObject mapIcon)
    {
        if (worldObject != null && mapIcon != null)
            mapIcon.transform.localPosition = new Vector3(
                worldObject.transform.position.x * 125 / 160f,
                worldObject.transform.position.z * 125 / 160f,
                0
            );
    }


    public void WindowsCanvas(Camera mainCamera)
    {
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = mainCamera;
        canvas.planeDistance = 0.31f;
        canvasSize.localScale = Vector3.one;
        commandTexts[0].text = "[Q]";
        commandTexts[1].text = "[マウス移動]";
        commandTexts[2].text = "[W A S D] (↑←↓→)";
        commandTexts[3].text = "[左Shift]";
        commandTexts[4].text = "[Space]";
        commandTexts[5].text = "[クリック]";
        commandTexts[6].text = "[右クリック]";
        commandTexts[7].text = "[R]";
        commandTexts[8].text = "[ホイール回転]";
        commandTexts[9].text = "[Tab]";
        helpBoxBackgroundImage.sprite = winHelpBackgroundSprite;
        helpBoxBackgroundImage.color = winHelpBackgroundColor;
        mapUIParentRect.localScale = Vector3.one * 1.6f;
    }

    public void OculusCanvas()
    {
        // canvas.planeDistance = f;
        commandTexts[0].text = "[Yボタン]";
        commandTexts[1].text = "R [スティック]";
        commandTexts[2].text = "L [スティック]";
        commandTexts[3].text = "L [下トリガー]";
        commandTexts[4].text = "L [上トリガー]";
        commandTexts[5].text = "R [上トリガー]";
        if (ShotMode == AimMode.RightHand) actionTexts[5].text = "レーザーサイト";
        commandTexts[6].text = "R [下トリガー]";
        commandTexts[7].text = "[Aボタン]";
        commandTexts[8].text = "[Bボタン]";
        commandTexts[9].text = "[Xボタン]";
    }


    void Awake()
    {
        currentKeyHelpTextColor = keyHelpTexts[0].color;
    }

    void UpdateHelpUI()
    {
        // QキーまたはYボタンの操作
        // UpdateHelpRecordAndActionText(Input.GetKey(KeyCode.Q) || OVRInput.Get(OVRInput.Touch.Two, OVRInput.Controller.LTouch), 0);
        //Q
        if (Input.GetKey(KeyCode.Q)
        || OVRInput.Get(OVRInput.Touch.Two, OVRInput.Controller.LTouch)) // Y
        {
            helpRecord.color = highlightColor;
            keyHelpTexts[0].color = Color.black;
        }
        else
        {
            helpRecord.color = defaultColor;
            keyHelpTexts[0].color = currentKeyHelpTextColor;
        }

        // マウス移動またはRスティックの操作
        bool isMouseMoving = platform == "Windows" && (Input.GetAxisRaw("Mouse X") != 0 || Input.GetAxisRaw("Mouse Y") != 0);
        UpdateHighlightTimer(ref isMouseMoving, ref mouseHighlightTimer);
        bool isRStickTouching = platform == "Oculus" && OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, OVRInput.Controller.RTouch);
        UpdateHelpRecordAndActionText(isMouseMoving || isRStickTouching, 0);

        // WASDキーまたは左スティックの操作
        bool isWASDKeyPressed = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);
        bool isArrowKeyPressed = Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);
        bool isLStickTouching = OVRInput.Get(OVRInput.Touch.PrimaryThumbstick, OVRInput.Controller.LTouch);
        UpdateHelpRecordAndActionText(isWASDKeyPressed || isArrowKeyPressed || isLStickTouching, 1);

        // 左Shiftキーまたは中指の操作
        bool isLeftShiftKeyPressed = platform == "Windows" && Input.GetKey(KeyCode.LeftShift);
        bool isMiddleFingerTriggered = platform == "Oculus" && (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch) > 0);
        UpdateHighlightTimer(ref isMiddleFingerTriggered, ref triggerHighlightTimer);
        UpdateHelpRecordAndActionText(isLeftShiftKeyPressed || isMiddleFingerTriggered, 2);

        // Spaceキーまたは左人差し指の操作
        UpdateHelpRecordAndActionText(Input.GetKey(KeyCode.Space) || OVRInput.Get(OVRInput.Touch.PrimaryIndexTrigger, OVRInput.Controller.LTouch), 3);

        // クリックまたは右人差し指の操作
        UpdateHelpRecordAndActionText(Input.GetMouseButton(0) || OVRInput.Get(OVRInput.Touch.PrimaryIndexTrigger, OVRInput.Controller.RTouch), 4);

        // 右クリックの操作
        UpdateHelpRecordAndActionText(Input.GetMouseButton(1) || OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) > 0, 5);

        // RキーまたはAボタンの操作
        UpdateHelpRecordAndActionText(Input.GetKey(KeyCode.R) || OVRInput.Get(OVRInput.Touch.One, OVRInput.Controller.RTouch), 6);

        // ホイール回転またはBボタンの操作
        bool isMouseWheelRotating = platform == "Windows" && Input.GetAxis("Mouse ScrollWheel") != 0f;
        UpdateHighlightTimer(ref isMouseWheelRotating, ref wheelHighlightTimer);
        bool isBButtonTouching = platform == "Oculus" && OVRInput.Get(OVRInput.Touch.Two, OVRInput.Controller.RTouch);
        UpdateHelpRecordAndActionText(isMouseWheelRotating || isBButtonTouching, 7);

        // TabキーまたはXボタンの操作
        UpdateHelpRecordAndActionText(Input.GetKey(KeyCode.Tab) || OVRInput.Get(OVRInput.Touch.One, OVRInput.Controller.LTouch), 8);
    }

    void UpdateHelpRecordAndActionText(bool isOperationActive, int index)
    {
        if (isOperationActive)
        {
            helpRecords[index].color = highlightColor;
            actionTexts[index].color = Color.black;
        }
        else
        {
            helpRecords[index].color = defaultColor;
            actionTexts[index].color = Color.white;
        }
    }

    void UpdateHighlightTimer(ref bool isOperationActive, ref float highlightTimer)
    {
        if (isOperationActive)
        {
            highlightTimer = highlightDuration;
        }
        else if (highlightTimer > 0)
        {
            highlightTimer -= Time.deltaTime;
            isOperationActive = true;
        }
    }

    // テキスト更新関数
    public void SettingBulletsText(int ammoClip, int ammunition)
    {
        // マガジン内の弾数 / 所持弾数
        ammoText.text = ammoClip.ToString();
        maxAmmoText.text = ammunition.ToString();
    }

    // HP更新関数
    public void UpdateHP(int maxHP, int currentHP)
    {
        hpSlider.maxValue = maxHP;
        hpSlider.value = currentHP;
    }

    // 死亡パネルを更新して開く
    public void UpdateDeathUI(string name, float reSpawnTime)
    {
        Debug.Log("UpdateDeathUI");
        this.reSpawnTime = reSpawnTime;
        fiveSecond = 5f;

        isHelpBoxActive = helpUI.activeSelf;

        HideHelpBox();
        deathPanel.SetActive(true);
        aimIcon.SetActive(false);
        changeAimIcon.gameObject.SetActive(false);
        mapUI.SetActive(false);

        deathText.text = name + " に倒された！";

        // ５秒後に死亡パネルを閉じる
        Invoke("CloseDeathUI", reSpawnTime);
    }

    // キルパネルを更新して開く
    public void UpdateKillUI(string name)
    {
        isHelpBoxActive = helpUI.activeSelf;

        HideHelpBox();
        killPanel.SetActive(true);
        aimIcon.SetActive(false);
        changeAimIcon.gameObject.SetActive(false);
        if (IsEnd) mapUI.SetActive(false);

        killText.text = name + " を倒した！";

        // 5秒後に死亡パネルを閉じる
        float uiTime = 3.5f;
        if (IsEnd) uiTime = 5.0f;
        Invoke("CloseKillUI", uiTime);
    }
    public void CloseDeathUI()
    {
        deathPanel.SetActive(false);

        if (!IsEnd)
        {
            if (!IsChanging)
            {
                if (platform == "Windows" || ShotMode == AimMode.Screen)
                {
                    aimIcon.SetActive(true);
                }
            }
            else changeAimIcon.gameObject.SetActive(true);
            helpUI.SetActive(isHelpBoxActive);
            mapUI.SetActive(true);
        }
    }

    public void CloseKillUI()
    {
        killPanel.SetActive(false);

        if (!IsEnd)
        {
            if (!IsChanging)
            {
                if (platform == "Windows" || ShotMode == AimMode.Screen)
                {
                    aimIcon.SetActive(true);
                }
            }
            else changeAimIcon.gameObject.SetActive(true);
            helpUI.SetActive(isHelpBoxActive);
            mapUI.SetActive(true);
        }
    }

    // スコアボードを開く関数
    public void ChangeScoreUI()
    {
        // 表示中なら非表示に、非表示中なら表示に切り替える 
        scoreboard.SetActive(!scoreboard.activeInHierarchy);
        if (scoreboard.activeInHierarchy)
        {
            aimIcon.SetActive(false);
            changeAimIcon.gameObject.SetActive(false);
            mapUI.SetActive(false);
        }
        else
        {
            if (!IsChanging)
            {
                if (platform == "Windows" || ShotMode == AimMode.Screen)
                {
                    aimIcon.SetActive(true);
                }
            }
            else changeAimIcon.gameObject.SetActive(true);
            mapUI.SetActive(true);
        }
    }

    // ヘルプボックス表示・非表示
    public void ShowHelpBox()
    {
        helpUI.SetActive(true);
    }
    public void HideHelpBox()
    {
        helpUI.SetActive(false);
    }

    // ヘルプレコード表示・非表示
    public void ChangeHelpRecord()
    {
        helpBoxBackground.SetActive(!helpBoxBackground.activeInHierarchy);
        foreach (Image record in helpRecords) record.gameObject.SetActive(!record.gameObject.activeInHierarchy);
        if (helpBoxBackground.activeInHierarchy)
        {
            foreach (TextMeshProUGUI helpText in keyHelpTexts) helpText.color = keyHelpColor;
            currentKeyHelpTextColor = keyHelpColor;
            keyHelpTexts[0].text = "ヘルプ OFF";
            mapIconHelpBox.SetActive(true);
            if (platform == "Windows")
            {
                mapUIParentRect.localScale = Vector3.one * 1.6f;
            }
        }
        else
        {
            foreach (TextMeshProUGUI helpText in keyHelpTexts) helpText.color = Color.black;
            currentKeyHelpTextColor = Color.black;
            keyHelpTexts[0].text = "ヘルプ ON";
            mapIconHelpBox.SetActive(false);
            if (platform == "Windows")
            {
                mapUIParentRect.localScale = Vector3.one;
            }
        }
    }

    // ゲーム終了パネル表示
    public void OpenEndPanel(float waitAfterEnding)
    {
        GameExitCountdown = waitAfterEnding;
        onCountdown = true;
        gunChangeUI.SetActive(false);
        deathPanel.SetActive(false);
        killPanel.SetActive(false);
        HideHelpBox();
        endPanel.SetActive(true);
        aimIcon.SetActive(false);
        changeAimIcon.gameObject.SetActive(false);
        hpUI.gameObject.SetActive(false);
    }
}
