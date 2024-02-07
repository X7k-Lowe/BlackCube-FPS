using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.EventSystems;
using DG.Tweening;


public enum Wall
{
    Front,
    Right,
    Left,
    Ground,
}

public enum AimMode
{
    RightHand,
    HeadSet,
}
public class PlayerController : MonoBehaviourPunCallbacks
{

    // カメラの親オブジェクト
    public Transform viewPoint;
    public Transform playerCanvasPoint;
    public Transform eyePoint;

    // 視点移動の速度
    public float mouseSensitivity = 1f;
    public float hmdSensitivity = 1f;

    // ユーザーのマウス入力の格納
    private Vector2 mouseInput;

    //y軸の回転格納
    private float verticalInput = 0f;

    // カメラ
    private Camera mainCamera;
    private Camera postProcessCamera;
    private Camera centerEyeAnchor;
    private GameObject oVRCameraRig;
    private GameObject cameraRigPlayerRotatePoint;

    // プレイヤーキャンバスUI
    public Billboard billboard;
    public Canvas playerCanvas;
    public GameObject aimIconsUI;
    public GameObject hpUI;
    public GameObject helpUI;
    public GameObject mapUI;
    public GameObject panelsUI;
    public GameObject playerCubeIcon;


    //入力された値格納
    private Vector3 moveDir;

    // 進む方向格納
    private Vector3 movement;

    // 実際の移動速度
    private float activeMoveSpeed = 4f;

    // 速度反映までの時間
    private float currentLerpTime = 0;

    // ジャンプ力
    public Vector3 jumpForce = new Vector3(0, 40, 0);

    // ジャンプしてるかどうか判定
    public bool isJumping = true;

    // ジャンプ間隔時間
    public float jumpInterval;

    // ジャンプした瞬間からの時間
    public float impactTime = 0;

    // レイを飛ばす地面判定、壁判定のオブジェクトの位置
    public Transform groundCheckPoint;
    public Transform wallCheckPoint;

    // レイを飛ばしてヒットしたオブジェクトの法線
    public Vector3 wallNormal;

    // ヒットした壁の向き
    public Wall hitWall;

    // 地面レイヤー
    public LayerMask groundLayers;

    // 視界レイヤー
    public LayerMask eyeAreaLayer;

    // カプセルコライダー
    public Collider playerCollider;
    public Collider myEyeAreaCollider;

    public EyeArea myEyeArea;
    public List<PhotonView> EnemyPhotonViews { get; set; } = new List<PhotonView>();

    // 剛体
    private Rigidbody rb;

    // 歩くスピード
    public float walkSpeed = 7f;

    // 走るスピード
    public float runSpeed = 15f;

    // カーソルの表示判定
    private bool cursorLock = true;



    // 武器の格納リスト
    public List<Gun> guns = new List<Gun>();


    // 選択中の武器管理用数値
    private int selectedGun = 0;

    // 射撃間隔
    private float shotTimer;

    [Tooltip("所持弾薬")]
    public int[] ammunition;

    [Tooltip("最高所持弾薬数")]
    public int[] maxAmmunition;

    [Tooltip("マガジン内の弾数")]
    public int[] ammoClip;

    [Tooltip("マガジンに入る最大数")]
    public int[] maxAmmoClip;

    int layerMaskGround;
    bool allowSwitchGuns = true;
    float switchGunsTime = 1.0f;
    const float SwitchGunsTime = 0.8f;
    const float ReloadingTime = 1.9f;
    bool isReloading = false;


    // uIManager
    public UIManager uIManager { get; set; }

    // SpawnManager
    private SpawnManager spawnManager;


    // アニメーター
    public Animator animator;

    // プレイヤーモデル
    public GameObject[] playerModel;

    // 銃ホルダー（自分用、他人用）
    public Gun[] gunsHolder, otherGunsHolder;


    // 最大HP
    public int maxHP = 100;

    // 現在HP
    private int currentHP;

    // 血のエフェクト
    public GameObject hitEffect;


    GameManager gameManager;

    public string platform;
    GameObject myIcon;
    public Material whiteMaterial;
    SpriteFacesCamera myIconSpriteFacesCamera;
    public int EyeAreaCounter { get; set; } = 0; // "EyeArea"コライダーに触れている数を数えるカウンター

    const float healingCubeReSpawnTime = 5f;

    // InputDevices.GetDeviceAtXRNodeを使ってHMDデバイスを取得
    InputDevice headDevice;




    GameObject rightController;
    public GameObject oculusGunsHolder;
    public List<Transform> gunModeTransforms;
    public AimMode AimMode { get; set; } = AimMode.RightHand;

    public GameObject gunModeAimIcon;
    GameObject laserPoint;
    public LineRenderer laserSight { get; set; }

    public Vector3 viewPointInitLocalPosition;

    public Material whiteOutMaterial;
    bool isWhiteOut = true;

    private float horizontalInput;

    public AudioSource healSE;

    public bool onZoom = false;
    bool isCameraMoving = false;
    float zoomTime;

    bool isGetDown = false;
    bool isGetUp = false;
    Vector3 targetPos;
    Vector3 prevTargetPos;
    float prevDistance;

    bool isZoomingHit = false;
    Vector3 hitObjectNormal;

    private void Awake()
    {
        // uIManager格納
        uIManager = GameObject.FindGameObjectWithTag("UIManager").GetComponent<UIManager>();

        // SpawnManager格納
        spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        gameManager.uIManager = uIManager;

        rightController = GameObject.Find("RightHandAnchor");
        cameraRigPlayerRotatePoint = GameObject.Find("PlayerRotatePoint");

        laserSight = gameManager.laserSight;

        layerMaskGround = 1 << LayerMask.NameToLayer("Ground");

        gameManager.isDead = false;
        onZoom = false;
        isCameraMoving = false;


        if (photonView.IsMine)
        {
            uIManager.playerCanvas = playerCanvas;
            uIManager.playerAimIconsUI = aimIconsUI;
            uIManager.playerHpUI = hpUI;
            uIManager.playerHelpUI = helpUI;
            uIManager.playerMapUI = mapUI;
            uIManager.playerPanelsUI = panelsUI;
        }


        myIcon = uIManager.GetMyMapIcon();

        myIcon.transform.GetChild(0).GetComponent<Image>().material = whiteMaterial;

        if (!gameManager.isStart)
        {
            whiteOutMaterial.color = new Color(whiteOutMaterial.color.r, whiteOutMaterial.color.g, whiteOutMaterial.color.b, 1);
        }
        else
        {
            isWhiteOut = false;
        }
    }
    void InitializePlatformSpecificFeatures()
    {
        if (platform == "Windows")
        {
            mainCamera = gameManager.mainCamera.GetComponent<Camera>();
            postProcessCamera = GameObject.Find("PostProcess").GetComponent<Camera>();
            mainCamera.gameObject.SetActive(true);
            gameManager.oVRCameraRig.SetActive(false);
            laserSight.gameObject.SetActive(false);
            uIManager.WindowsCanvas(postProcessCamera);
            myIconSpriteFacesCamera.useCamera = postProcessCamera;
            billboard.useCamera = postProcessCamera;
            playerCanvas.enabled = false;
        }
        else if (platform == "Oculus")
        {
            horizontalInput = transform.eulerAngles.y;
            centerEyeAnchor = GameObject.Find("CenterEyeAnchor").GetComponent<Camera>();
            oVRCameraRig = GameObject.Find("OVRCameraRig");
            oVRCameraRig.SetActive(true);
            gameManager.mainCamera.SetActive(false);
            uIManager.OculusCanvas();
            myIconSpriteFacesCamera.useCamera = centerEyeAnchor;
            billboard.useCamera = centerEyeAnchor;

            if (AimMode == AimMode.HeadSet && photonView.IsMine)
            {
                laserSight.gameObject.SetActive(false);
                oculusGunsHolder.transform.SetParent(oVRCameraRig.transform);
                oculusGunsHolder.transform.localPosition = Vector3.zero;
                oculusGunsHolder.transform.localRotation = Quaternion.identity;
            }
            else if (AimMode == AimMode.RightHand && photonView.IsMine)
            {
                oculusGunsHolder.transform.SetParent(rightController.transform);
                oculusGunsHolder.transform.localPosition = Vector3.zero;
                oculusGunsHolder.transform.localRotation = Quaternion.identity;

                for (int i = 0; i < gunsHolder.Length; i++)
                {
                    gunsHolder[i].transform.localPosition = gunModeTransforms[i].localPosition;
                    gunsHolder[i].transform.localRotation = gunModeTransforms[i].localRotation;
                }

                gameManager.rightControllerRenderer.enabled = false;
                gameManager.leftControllerRenderer.enabled = false;

                laserPoint = Instantiate(gunModeAimIcon);
                laserPoint.SetActive(false);
            }

            // UIをプレイヤーキャンバスに配置
            if (photonView.IsMine)
            {
                if (!gameManager.isStart)
                {
                    uIManager.SetUIAsChildOfPlayerCanvas();
                    playerCanvas.enabled = false;
                    uIManager.cameraRigCanvas.enabled = false;
                }
                else
                {
                    uIManager.hpUI.SetActive(true);
                }
            }
        }
    }
    private void Start()
    {
        // InputDevices.GetDeviceAtXRNodeを使ってHMDデバイスを取得
        headDevice = InputDevices.GetDeviceAtXRNode(XRNode.Head);

        // カスタムプロパティからプラットフォーム情報を取得して保存する

        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;

        uIManager.platform = platform = (string)customProperties["Platform"];

        // Debug.Log(gameManager == null ? "gameManagerはnullです" : "gameManagerはnullではありません");
        AimMode = gameManager.AimMode;

        uIManager.AimMode = AimMode;
        if (platform == "Oculus" && AimMode == AimMode.RightHand)
        {
            uIManager.aimIcon.SetActive(false);
            laserSight.enabled = false;
        }

        //カメラ格納 プラットフォームごとの初期化処理を行う

        myIconSpriteFacesCamera = myIcon.GetComponent<SpriteFacesCamera>();

        InitializePlatformSpecificFeatures();

        uIManager.myPlayerObject = this.gameObject;

        // 現在HPに最大HPを代入

        currentHP = maxHP;

        rb = GetComponent<Rigidbody>();

        // カーソルの表示判定関数

        UpdateCursorLock();

        // 銃を扱うリスト初期化
        guns.Clear();

        // モデルや銃の表示切替
        if (photonView.IsMine)
        {
            foreach (GameObject model in playerModel)
            {
                model.SetActive(false);
            }

            foreach (Gun gun in gunsHolder)
            {
                guns.Add(gun);
            }

            // HPをスライダーに反映
            uIManager.UpdateHP(maxHP, currentHP);
            photonView.RPC("UpdateHPSlider", RpcTarget.All, maxHP, currentHP);

            billboard.nameText.text = PhotonNetwork.NickName;
            float textWidth = billboard.nameText.preferredWidth;
            billboard.background.sizeDelta = new Vector2(textWidth + 20, billboard.background.sizeDelta.y + 5);

            if (platform == "Oculus")
            {
                // キャンバスの表示を切り替える
                uIManager.playerCanvas.enabled = true;
                uIManager.cameraRigCanvas.enabled = true;
            }

            billboard.gameObject.SetActive(false);
            playerCubeIcon.SetActive(false);
        }
        else
        {
            foreach (Gun gun in otherGunsHolder)
            {
                guns.Add(gun);
            }

            billboard.nameText.text = photonView.Owner.NickName;
            float textWidth = billboard.nameText.preferredWidth;
            billboard.background.sizeDelta = new Vector2(textWidth + 20, billboard.background.sizeDelta.y + 5);
        }


        // 全プレーヤー同期の銃切り替え
        photonView.RPC("SetGun", RpcTarget.All, selectedGun);



        if (photonView.IsMine)
        {
            InitPlayerRotate();
            if (!gameManager.isStart) StartCoroutine(FadeInStartDisplay());
        }
    }

    [PunRPC]
    public void UpdateHPSlider(int maxHP, int currentHP)
    {
        billboard.UpdateHP(maxHP, currentHP);
    }

    public void HealingHP(int healingPoint, int viewID)
    {
        if (photonView.IsMine)
        {
            healSE.Play();
            currentHP += healingPoint;

            currentHP = Mathf.Clamp(currentHP, 0, maxHP);

            // HPをスライダーに反映
            uIManager.UpdateHP(maxHP, currentHP);
            photonView.RPC("UpdateHPSlider", RpcTarget.All, maxHP, currentHP);

            photonView.RPC("DestroyHealingCube", RpcTarget.MasterClient, viewID);
            photonView.RPC("RemoveHealingCube", RpcTarget.All, viewID);
        }
    }

    [PunRPC]
    void DestroyHealingCube(int viewID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            GameObject healingCube = PhotonView.Find(viewID).gameObject;
            PhotonNetwork.Destroy(healingCube);

            Invoke("ReSpawnHealingCube", healingCubeReSpawnTime);
        }
    }

    void ReSpawnHealingCube()
    {
        spawnManager.SpawnHealingCube();
    }

    [PunRPC]
    public void RemoveHealingCube(int viewID)
    {
        GameObject healingCube = uIManager.worldObjects.Find(x => x.GetPhotonView().ViewID == viewID);
        RemoveWorldObject(healingCube);
    }

    IEnumerator FadeInStartDisplay()
    {
        uIManager.hpUI.SetActive(false);
        gameManager.ShowScoreboard();
        uIManager.scoreboard.SetActive(false);
        yield return new WaitForSeconds(1.0f);

        while (!gameManager.onSetKills)
        {
            yield return null;
        }

        if (gameManager.isPracticeMode)
        {
            yield return uIManager.PracticeModeSetStartText();
        }
        else
        {
            if (platform == "Oculus")
            {
                uIManager.panelsUIRect.localPosition = new Vector3(uIManager.panelsUIRect.localPosition.x, -150, uIManager.panelsUIRect.localPosition.z);
            }
            uIManager.scoreboard.SetActive(true);
            uIManager.ChangeScoreUI(false);
            uIManager.ShowHelpBox();
            gameManager.ShowScoreboard();
            yield return uIManager.ShowStartPanel();
            yield return new WaitForSeconds(0.5f);
        }

        yield return new WaitForSeconds(1.3f);

        whiteOutMaterial.DOFade(0, 3f).SetEase(Ease.InQuad);
        yield return new WaitForSeconds(2.5f);

        if (gameManager.isPracticeMode)
        {
            uIManager.practicePanel.SetActive(false);
            uIManager.scoreboard.SetActive(true);
        }
        else
        {
            uIManager.startPanel.SetActive(false);
            if (platform == "Oculus")
            {
                uIManager.panelsUIRect.localPosition = new Vector3(uIManager.panelsUIRect.localPosition.x, 0, uIManager.panelsUIRect.localPosition.z);
            }
        }
        uIManager.ChangeScoreUI(false);
        uIManager.ShowHelpBox();
        uIManager.hpUI.SetActive(true);
        if (platform == "Oculus")
        {
            uIManager.scoreboardRect.localPosition = new Vector3(uIManager.scoreboardRect.localPosition.x, uIManager.scoreboardRect.localPosition.y, 200);
        }

        yield return new WaitForSeconds(1.0f);

        isWhiteOut = false;

        if (gameManager.isPracticeMode)
        {
            gameManager.isStart = true;
        }
        else
        {
            uIManager.readyCountdown = 3.0f;
            uIManager.isReady = true;
        }
    }
    private void Update()
    {
        // 自分じゃなければ
        if (!photonView.IsMine) return;

        if (isWhiteOut)
        {
            return;
        }

        CheckEyeAreaStatus();
        uIManager.UpdateMapIconPos(this.gameObject, myIcon);

        //視点移動関数の呼び出し
        PlayerRotate();

        // 走る関数の呼び出し
        Run();

        // 移動関数を呼ぶ
        PlayerMove();



        if (CanJump() && (IsGround() || IsWall()))
        {
            // ジャンプ関数を呼ぶ
            jump();
        }

        // 覗き込み関数
        Aim();

        // カーソルの表示判定関数
        UpdateCursorLock();
        // UpdateLaserPoint();

        if (!gameManager.isStart)
        {
            return;
        }

        if (allowSwitchGuns)
        {
            // 射撃ボタン検知関数を呼ぶ
            Fire();

            // 武器の変更キー検知関数
            SwitchingGuns();

            // リロード関数を呼ぶ
            Reload();
        }
        else
        {
            if (switchGunsTime >= 0)
            {
                switchGunsTime -= Time.deltaTime;
                if (isReloading)
                {
                    uIManager.gunChangeSlider.value = switchGunsTime / ReloadingTime;
                }
                else
                {
                    uIManager.gunChangeSlider.value = switchGunsTime / SwitchGunsTime;
                }
                uIManager.changeIcon.localRotation
                = Quaternion.Euler(0, 0, switchGunsTime * 360);
                uIManager.changeAimIcon.localRotation
             = Quaternion.Euler(0, 0, switchGunsTime * 360);
            }
            if (switchGunsTime <= 0)
            {
                uIManager.HideGunChangeUI();
                allowSwitchGuns = true;
                isReloading = false;
            }
        }


        uIManager.IsChanging = isReloading || !allowSwitchGuns;

        // アニメーションの関数呼び出し
        AnimatorSet();

        if (platform == "Windows")
        {
            if (Input.GetMouseButtonUp(0) || ammoClip[2] <= 0 || !allowSwitchGuns)
            {
                photonView.RPC("SoundStop", RpcTarget.All);
            }
        }
        else if (platform == "Oculus")
        {
            if (OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) // 右人差し指
           || ammoClip[2] <= 0 || !allowSwitchGuns)
            {
                photonView.RPC("SoundStop", RpcTarget.All);
            }
        }

        // 特定のボタンが押されたらメニューに戻る
        if (gameManager.isPracticeMode)
            OutGame();

    }

    private void FixedUpdate()
    {
        // 自分じゃなければ
        if (!photonView.IsMine) return;

        // テキスト更新関数の呼び出し
        uIManager.SettingBulletsText(ammoClip[selectedGun], ammunition[selectedGun]);
    }
    private void LateUpdate()
    {
        // 自分じゃなければ
        if (!photonView.IsMine) return;

        if (platform == "Windows")
        {
            //カメラの位置調整
            mainCamera.transform.position = viewPoint.position;
            // 回転
            mainCamera.transform.rotation = viewPoint.rotation;
        }
        else if (platform == "Oculus")
        {
            UpdateLaserPoint();
        }
    }

    [PunRPC]
    public void IncrementEyeAreaCounter()
    {
        EyeAreaCounter++;
    }

    [PunRPC]
    public void DecrementEyeAreaCounter()
    {
        Debug.Log("DecrementEyeAreaCounter : " + EyeAreaCounter);
        EyeAreaCounter--;
        Debug.Log("DecrementEyeAreaCounter : " + EyeAreaCounter);

    }
    private void CheckEyeAreaStatus()
    {
        if (EyeAreaCounter > 0)
        {
            AddWorldObject(this.gameObject);
        }
        else
        {
            RemoveWorldObject(this.gameObject);
        }
    }
    public void AddWorldObject(GameObject worldObject)
    {
        if (!uIManager.worldObjects.Contains(worldObject))
        {
            uIManager.allowUpdateMapIcon = false;
            uIManager.worldObjects.Add(worldObject);
            uIManager.allowUpdateMapIcon = true;
        }
    }

    public void RemoveWorldObject(GameObject worldObject)
    {
        if (uIManager.worldObjects.Contains(worldObject))
        {
            uIManager.allowUpdateMapIcon = false;
            uIManager.worldObjects.Remove(worldObject);
            uIManager.allowUpdateMapIcon = true;
        }
    }

    public void RemoveAllEnemyPlayerObject()
    {
        Debug.Log("RemoveAllEnemyPlayerObject");
        uIManager.allowUpdateMapIcon = false;

        foreach (var enemyPlayer in uIManager.worldObjects.ToArray())
        {
            uIManager.worldObjects.Remove(enemyPlayer);
        }

        uIManager.allowUpdateMapIcon = true;
    }

    void InitPlayerRotate()
    {
        if (platform == "Oculus")
        {
            transform.rotation = Quaternion.identity;
            viewPoint.rotation = Quaternion.identity;
            playerCanvasPoint.rotation = Quaternion.identity;
            oVRCameraRig.transform.rotation = Quaternion.identity;

            viewPoint.localPosition = viewPointInitLocalPosition;
            playerCanvasPoint.localPosition = viewPointInitLocalPosition;
            oVRCameraRig.transform.position = viewPoint.position;
        }
    }

    // 視点移動関数
    public void PlayerRotate()
    {
        if (platform == "Windows")
        {
            // 変数にユーザーのマウスの動きを格納
            mouseInput = new Vector2(Input.GetAxisRaw("Mouse X") * mouseSensitivity,
            Input.GetAxisRaw("Mouse Y") * mouseSensitivity);

            // マウスのx軸の動きを反映
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x,
            transform.eulerAngles.y + mouseInput.x,
            transform.eulerAngles.z);

            // y軸の値に現在の値を足す
            verticalInput += mouseInput.y;
            // 数値を丸める
            verticalInput = Mathf.Clamp(verticalInput, -60f, 60f);

            // viewPointに数値を反映
            viewPoint.rotation = Quaternion.Euler(-verticalInput,
            viewPoint.transform.rotation.eulerAngles.y,
            viewPoint.transform.rotation.eulerAngles.z);
        }
        else if (platform == "Oculus")
        {
            // 変数にユーザーのサムスティックの動きを格納
            Vector2 rStickInput = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch);

            horizontalInput += rStickInput.x * hmdSensitivity;
            if (horizontalInput < 0) horizontalInput = 360 + horizontalInput;
            if (horizontalInput > 360) horizontalInput = horizontalInput - 360;

            // サムスティックのx軸の動きを反映
            if (oVRCameraRig != null)
            {
                oVRCameraRig.transform.rotation = Quaternion.Euler(oVRCameraRig.transform.eulerAngles.x,
                oVRCameraRig.transform.eulerAngles.y + rStickInput.x * hmdSensitivity,
                oVRCameraRig.transform.eulerAngles.z);
            }

            // y軸の値に現在の値を足す
            verticalInput += rStickInput.y * hmdSensitivity * 0.25f;
            // 数値を丸める
            verticalInput = Mathf.Clamp(verticalInput, -30f, 30f);

            // viewPointに数値を反映
            viewPoint.rotation = Quaternion.Euler(-verticalInput,
            viewPoint.transform.rotation.eulerAngles.y,
            viewPoint.transform.rotation.eulerAngles.z);

            // HMDの回転を取得
            if (headDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion headRotation))
            {
                // プレイヤーの水平回転（y軸）をHMDの回転に合わせる
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, horizontalInput + headRotation.eulerAngles.y, transform.eulerAngles.z);
                Debug.Log("headRotation : " + headRotation.eulerAngles.y);
                // 垂直回転（x軸）のためにHMDのPitch値を使用して、viewPointを回転させる
                // ここでは範囲を-60fから60fに制限している
                float headPitch = headRotation.eulerAngles.x;
                if (headPitch > 180)
                {
                    headPitch -= 360; // 0-360から-180から180に変換
                }

                // viewPointの回転を設定
                viewPoint.rotation = Quaternion.Euler(headPitch, viewPoint.eulerAngles.y, viewPoint.eulerAngles.z);
                if (AimMode == AimMode.HeadSet)
                {
                    oculusGunsHolder.transform.rotation = Quaternion.Euler(headPitch, viewPoint.eulerAngles.y, viewPoint.eulerAngles.z);
                }
            }

            cameraRigPlayerRotatePoint.transform.rotation = Quaternion.Euler(0, viewPoint.eulerAngles.y, 0);
        }
    }

    public void PlayerMove()
    {
        if (platform == "Windows")
        {
            // 移動用キーの入力を検知して値を格納
            moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        }
        else if (platform == "Oculus")
        {
            moveDir = new Vector3(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch).x, 0, OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.LTouch).y); // 左スティック
        }

        // ジャンプ中ならジャンプした瞬間から少しの間、壁方向の入力を０にする
        if (isJumping)
        {
            impactTime += Time.deltaTime;
            if (impactTime > 0.25f)
            {
                impactTime = 0;
                if (hitWall == Wall.Front) moveDir.z = Mathf.Clamp(moveDir.z, -1, 0);
                else if (hitWall == Wall.Right) moveDir.x = Mathf.Clamp(moveDir.x, -1, 0);
                else if (hitWall == Wall.Left) moveDir.x = Mathf.Clamp(moveDir.x, 0, 1);
            }
        }

        // 進む方向を出して変数に格納
        if (platform == "Windows")
        {
            movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized;
        }
        else if (platform == "Oculus")
        {
            movement = ((oVRCameraRig.transform.forward * moveDir.z) + (oVRCameraRig.transform.right * moveDir.x)).normalized;

            if (isZoomingHit)
            {
                if (Vector3.Distance(this.transform.position, oVRCameraRig.transform.position) <= 2.0f)
                {
                    float velocityAlongNormal = Vector3.Dot(movement, hitObjectNormal);
                    if (velocityAlongNormal < 0)
                    {
                        Vector3 normalComponent = hitObjectNormal * velocityAlongNormal;
                        movement = movement - normalComponent;
                    }
                }
            }
        }

        // 状態ごとに移動量を変更
        if (isJumping && hitWall != Wall.Ground) // 壁からのジャンプ中
        {

            if (hitWall == Wall.Front && moveDir.z >= 0
            || hitWall == Wall.Right && moveDir.x >= 0
            || hitWall == Wall.Left && moveDir.x <= 0)
            {
                activeMoveSpeed = walkSpeed;
                Vector3 newVelocity = new Vector3(movement.x * activeMoveSpeed, rb.velocity.y, movement.z * activeMoveSpeed);

                if (newVelocity != rb.velocity)
                {
                    currentLerpTime += Time.deltaTime / 1.5f;
                    rb.velocity = new Vector3(
                        Mathf.Lerp(rb.velocity.x, newVelocity.x, currentLerpTime),
                        rb.velocity.y,
                        Mathf.Lerp(rb.velocity.z, newVelocity.z, currentLerpTime)
                    );
                }
                else
                {
                    currentLerpTime = 0;
                }
            }
            else rb.velocity = new Vector3(movement.x * activeMoveSpeed, rb.velocity.y, movement.z * activeMoveSpeed);
        }
        else if (!isJumping && IsWall()) // 壁についているとき
        {
            {
                // 壁の法線の反対方向のベクトルの成分を打ち消す
                float velocityAlongNormal = Vector3.Dot(movement, wallNormal);
                if (velocityAlongNormal < 0)
                {
                    Vector3 normalComponent = wallNormal * velocityAlongNormal;
                    movement = movement - normalComponent;
                }
            }

            if (hitWall == Wall.Ground)
            {
                rb.velocity = new Vector3(movement.x * activeMoveSpeed, rb.velocity.y, movement.z * activeMoveSpeed);
            }
            else if (hitWall == Wall.Front)
            {
                rb.velocity = new Vector3(movement.x * activeMoveSpeed * 0.3f, 0, movement.z * activeMoveSpeed * 0.3f);
            }
            else if (hitWall == Wall.Left || hitWall == Wall.Right)
            {
                rb.velocity = new Vector3(movement.x * activeMoveSpeed, 0, movement.z * activeMoveSpeed);
            }
        }
        // 地面についているか、その他ジャンプ中
        else if (IsGround() || isJumping)
        {
            rb.velocity = new Vector3(movement.x * activeMoveSpeed, rb.velocity.y, movement.z * activeMoveSpeed);
        }
    }

    public void jump()
    {
        // スペースキーが押された時
        if (Input.GetKeyDown(KeyCode.Space)
        || OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) // 左人差し指
        {
            isJumping = true;

            // Y軸の速度を０にする
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);

            // 壁についているなら
            if (IsWall())
            {
                if (hitWall == Wall.Front)
                {
                    rb.velocity = (transform.up - (transform.forward * 2)).normalized * 20;
                }
                else if (hitWall == Wall.Left)
                {
                    rb.velocity = (transform.up + (transform.right * 2)).normalized * 20;
                }
                else if (hitWall == Wall.Right)
                {
                    rb.velocity = (transform.up - (transform.right * 2)).normalized * 20;
                }
                else rb.AddForce(jumpForce, ForceMode.Impulse);
            }
            // 地面についているなら
            else if (hitWall == Wall.Ground) rb.AddForce(jumpForce, ForceMode.Impulse); // Impulseは力を瞬時に加えるモード
        }
    }

    // ジャンプできるかどうかの判定（ジャンプしたら一定時間、再ジャンプできない）
    public bool CanJump()
    {
        if (isJumping)
        {
            impactTime = 0;
            jumpInterval += Time.deltaTime;

            if (jumpInterval > 0.5f)
            {
                jumpInterval = 0;
                isJumping = false;
            }
        }
        return !isJumping;
    }

    // 地面についていればtrue
    public bool IsGround()
    {
        // 判定してboolを返す(レイを飛ばすポジション、方向、距離、地面判定するレイヤー)
        return Physics.Raycast(groundCheckPoint.position, Vector3.down, 1f, groundLayers);
    }

    // 壁についていればtrue
    public bool IsWall()
    {
        bool isWall = false;

        // プレイヤーの前方にレイを飛ばす
        if (Physics.Raycast(wallCheckPoint.position, transform.TransformDirection(Vector3.forward), out RaycastHit hitFront, 3f)
        && hitFront.collider.gameObject.tag == "Wall")
        {
            wallNormal = hitFront.normal;
            hitWall = Wall.Front;
            isWall = true;
        }
        // プレイヤーの左にレイを飛ばす
        else if (Physics.Raycast(wallCheckPoint.position, transform.TransformDirection(Vector3.left), out RaycastHit hitLeft, 2.5f)
        && hitLeft.collider.gameObject.tag == "Wall")
        {
            hitWall = Wall.Left;
            isWall = true;
        }
        // プレイヤーの右にレイを飛ばす
        else if (Physics.Raycast(wallCheckPoint.position, transform.TransformDirection(Vector3.right), out RaycastHit hitRight, 2.5f)
        && hitRight.collider.gameObject.tag == "Wall")
        {
            hitWall = Wall.Right;
            isWall = true;
        }

        if (IsGround()) hitWall = Wall.Ground;
        return isWall;
    }

    public void Run()
    {
        // シフトが押されているときにスピードを切り替える
        if (Input.GetKey(KeyCode.LeftShift)
        || OVRInput.Get(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch))  // 左中指
        {
            // if (hitWall == Wall.Ground || !isJumping && (hitWall == Wall.Right || hitWall == Wall.Left))
            activeMoveSpeed = runSpeed;
            // else activeMoveSpeed = walkSpeed;
            hmdSensitivity = 2.5f;
        }
        else
        {
            activeMoveSpeed = walkSpeed;
            hmdSensitivity = 1f;

        }
    }

    public void UpdateCursorLock()
    {
        // boolを切り替える
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            cursorLock = false;
        }
        else if (Input.GetMouseButton(0))
        {
            cursorLock = true;
        }

        if (cursorLock)
        {
            // カーソルを中央に固定して非表示
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            // カーソル表示
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void SwitchingGuns()
    {
        // マウスホイールを回して銃の切り替え
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0
    || OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch)) // B
        {
            selectedGun++;

            if (selectedGun >= guns.Count)
            {
                selectedGun = 0;
            }

            // 銃を切り替える関数
            // SwitchGun();
            // 全プレーヤー同期の銃切り替え
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);

            allowSwitchGuns = false;
            switchGunsTime = SwitchGunsTime;
            uIManager.ShowGunChangeUI();
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") < 0)
        {
            selectedGun--;

            if (selectedGun < 0)
            {
                selectedGun = guns.Count - 1;
            }

            // 銃を切り替える関数
            // SwitchGun();
            // 全プレーヤー同期の銃切り替え
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);

            allowSwitchGuns = false;
            switchGunsTime = SwitchGunsTime;
            uIManager.ShowGunChangeUI();
        }

        // 数値キー入力で銃切り替え
        for (int i = 0; i < guns.Count; i++)
        {
            // 数値キーの入力判定
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                selectedGun = i;

                // 銃を切り替える関数
                // SwitchGun();
                // 全プレーヤー同期の銃切り替え
                photonView.RPC("SetGun", RpcTarget.All, selectedGun);

                allowSwitchGuns = false;
                switchGunsTime = SwitchGunsTime;
                uIManager.ShowGunChangeUI();
            }
        }
    }

    public void SwitchGun()
    {
        // 全ての銃を非表示にする
        foreach (Gun gun in guns)
        {
            gun.gameObject.SetActive(false);
        }

        // 選択中の銃のみ表示する
        guns[selectedGun].gameObject.SetActive(true);
    }

    public void Aim()
    {
        if (platform == "Windows")
        {
            float targetFOV;
            if (Input.GetMouseButton(1))
            {
                targetFOV = guns[selectedGun].adsZoom;
            }
            else
            {
                targetFOV = 60f;
            }

            // mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, guns[selectedGun].acsSpeed * Time.deltaTime);
            postProcessCamera.fieldOfView = Mathf.Lerp(postProcessCamera.fieldOfView, targetFOV, guns[selectedGun].acsSpeed * Time.deltaTime);
        }
        else if (platform == "Oculus")
        {
            if (allowSwitchGuns && OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch) == 1
            || Input.GetMouseButton(1)) // 右中指
            {
                if (!isGetDown)
                {
                    zoomTime = 1.5f / guns[selectedGun].acsSpeed + 0.3f;
                    isGetDown = true;
                    isGetUp = false;
                    isCameraMoving = true;
                }
                zoomTime -= Time.deltaTime;
                if (zoomTime <= 0)
                {
                    isCameraMoving = false;
                }

                if (AimMode == AimMode.HeadSet) uIManager.ZoomIn();
                else if (AimMode == AimMode.RightHand) laserSight.enabled = true;

                onZoom = true;
            }
            else
            {
                if (!isGetUp)
                {
                    zoomTime = 1.5f / guns[selectedGun].acsSpeed + 0.3f;
                    isGetUp = true;
                    isGetDown = false;
                    isCameraMoving = true;
                }
                zoomTime -= Time.deltaTime;
                if (zoomTime <= 0)
                {
                    isCameraMoving = false;
                }

                if (AimMode == AimMode.HeadSet) uIManager.ZoomOut();
                else if (AimMode == AimMode.RightHand) laserSight.enabled = false;

                onZoom = false;
            }
        }
    }

    void SetOVRCameraRigPos()
    {
        Vector3 startPos = viewPoint.position;
        Vector3 forwardDirection = viewPoint.forward;
        forwardDirection.y = 0; // 上下の向きを無視
        float distance;
        float zoom = 0.1f;
        float zoomScale = 5.0f;

        if (AimMode == AimMode.HeadSet)
        {
            zoom = 0.3f;
            zoomScale = 15.0f;
        }

        if (onZoom)
        {
            RaycastHit hit;
            if (Physics.SphereCast(startPos, zoomScale, forwardDirection, out hit, guns[selectedGun].adsZoom * zoom))
            {
                if (hit.collider.gameObject.tag == "Player")
                {
                    targetPos = hit.point - forwardDirection * 4;
                    isZoomingHit = true;
                    hitObjectNormal = new Vector3(hit.normal.x, 0, hit.normal.z); // 水平方向のみにする
                }
                else if (Physics.SphereCast(startPos, 0.5f, forwardDirection, out hit, guns[selectedGun].adsZoom * zoom))
                {
                    if (hit.collider.gameObject.tag == "Wall"
                    || hit.collider.gameObject.tag == "HealingCube")
                    {
                        targetPos = hit.point - forwardDirection * 4;
                        isZoomingHit = true;
                        hitObjectNormal = new Vector3(hit.normal.x, 0, hit.normal.z); // 水平方向のみにする
                    }
                }
                else
                {
                    targetPos = startPos + forwardDirection * guns[selectedGun].adsZoom * zoom;
                    isZoomingHit = false;
                }
            }
            else
            {
                targetPos = startPos + forwardDirection * guns[selectedGun].adsZoom * zoom;
                isZoomingHit = false;
            }

            distance = Vector3.Distance(startPos, targetPos);
        }
        else
        {
            targetPos = viewPoint.position;
            distance = 0;
            isZoomingHit = false;
        }

        if (Mathf.Abs(distance - prevDistance) > 0.1f)
        {
            zoomTime = 1.5f / guns[selectedGun].acsSpeed + 0.3f;
            isCameraMoving = true;
        }

        prevDistance = distance;

        if (isCameraMoving)
        {
            float step = (targetPos - oVRCameraRig.transform.position).magnitude / (1.5f / guns[selectedGun].acsSpeed) * Time.deltaTime; // 0.3秒で移動を完了させる
            oVRCameraRig.transform.position = Vector3.MoveTowards(oVRCameraRig.transform.position, targetPos, step);
        }
        else
        {
            oVRCameraRig.transform.position = targetPos;
        }
    }
    public void Fire()
    {
        // 撃ちだせるのかの判定
        if ((Input.GetMouseButton(0)
        || OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))  // 右人差し指
        && Time.time > shotTimer)
        {
            if (ammoClip[selectedGun] > 0)
            {
                // 弾を撃ちだす関数
                FiringBullet();
            }
            else
            {
                Reloading();
            }
        }
    }

    public bool IsTouchingEyeArea()
    {
        Collider[] hitColliders = Physics.OverlapBox(playerCollider.bounds.center, playerCollider.bounds.extents, playerCollider.transform.rotation, eyeAreaLayer);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != myEyeAreaCollider.gameObject)
            {
                return true;
            }
        }
        return false;
    }
    public void FiringBullet()
    {
        // 弾を減らす
        ammoClip[selectedGun]--;

        Ray ray;

        // カメラの中央からレイを飛ばす
        if (platform == "Windows")
        {
            ray = mainCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            float hitBox = guns[selectedGun].hitBox * 0.25f; // 球体の半径を設定します。これが「太さ」になります。

            if (Physics.SphereCast(ray, hitBox, out RaycastHit hit, Mathf.Infinity, ~eyeAreaLayer.value))
            {
                // 当たったのがプレイヤーならHit関数
                if (hit.collider.gameObject.tag == "Player"
                && hit.collider.gameObject != photonView.gameObject)
                {
                    // 血のエフェクト生成
                    PhotonNetwork.Instantiate(hitEffect.name, hit.point, Quaternion.identity);

                    hit.collider.gameObject.GetPhotonView().RPC(
                        "Hit",
                        RpcTarget.All,
                        guns[selectedGun].shootDamage,
                        photonView.Owner.NickName,
                        PhotonNetwork.LocalPlayer.ActorNumber);
                }
                else
                {
                    if (Physics.Raycast(ray, out RaycastHit hit2, Mathf.Infinity, ~eyeAreaLayer.value))
                    {
                        // 当たったのがプレイヤーならHit関数
                        if (hit2.collider.gameObject.tag == "Player"
                        && hit2.collider.gameObject != photonView.gameObject)
                        {
                            // 血のエフェクト生成
                            PhotonNetwork.Instantiate(hitEffect.name, hit2.point, Quaternion.identity);

                            hit2.collider.gameObject.GetPhotonView().RPC(
                                "Hit",
                                RpcTarget.All,
                                guns[selectedGun].shootDamage,
                                photonView.Owner.NickName,
                                PhotonNetwork.LocalPlayer.ActorNumber);
                        }
                        else if (hit2.collider.gameObject.tag != "HealingCube"
                        && hit2.collider.gameObject != photonView.gameObject)
                        // 当たったのがプレイヤー以外なら弾痕を生成
                        {
                            // 当たった場所に弾痕を生成
                            GameObject bulletImpact = Instantiate(
                                guns[selectedGun].bulletImpact,
                                hit2.point + (hit2.normal * 0.02f),
                                // hit.normal : ヒットしたコライダーの９０度, Vector3.up : Y軸を上とする
                                Quaternion.LookRotation(hit2.normal, Vector3.up));

                            Destroy(bulletImpact, 10f);
                        }
                    }
                }
            }
        }
        else if (platform == "Oculus")
        {
            ray = new Ray();
            float hitBox = guns[selectedGun].hitBox; // 球体の半径を設定します。これが「太さ」になります。

            if (AimMode == AimMode.HeadSet) ray = centerEyeAnchor.ViewportPointToRay(new Vector2(0.5f, 0.5f));
            else if (AimMode == AimMode.RightHand) ray = new Ray(rightController.transform.position, rightController.transform.forward);

            if (Physics.SphereCast(ray, hitBox, out RaycastHit hit, Mathf.Infinity, ~eyeAreaLayer.value))
            {
                // 当たったのがプレイヤーならHit関数
                if (hit.collider.gameObject.tag == "Player"
                && hit.collider.gameObject != photonView.gameObject)
                {
                    // 血のエフェクト生成
                    PhotonNetwork.Instantiate(hitEffect.name, hit.point, Quaternion.identity);

                    hit.collider.gameObject.GetPhotonView().RPC(
                        "Hit",
                        RpcTarget.All,
                        guns[selectedGun].shootDamage,
                        photonView.Owner.NickName,
                        PhotonNetwork.LocalPlayer.ActorNumber);
                }
                else
                {
                    if (Physics.Raycast(ray, out RaycastHit hit2, Mathf.Infinity, ~eyeAreaLayer.value))
                    {
                        // 当たったのがプレイヤーならHit関数
                        if (hit2.collider.gameObject.tag == "Player"
                        && hit2.collider.gameObject != photonView.gameObject)
                        {
                            // 血のエフェクト生成
                            PhotonNetwork.Instantiate(hitEffect.name, hit2.point, Quaternion.identity);

                            hit2.collider.gameObject.GetPhotonView().RPC(
                                "Hit",
                                RpcTarget.All,
                                guns[selectedGun].shootDamage,
                                photonView.Owner.NickName,
                                PhotonNetwork.LocalPlayer.ActorNumber);
                        }
                        else if (hit2.collider.gameObject.tag != "HealingCube"
                        && hit2.collider.gameObject != photonView.gameObject)
                        // 当たったのがプレイヤー以外なら弾痕を生成
                        {
                            // 当たった場所に弾痕を生成
                            GameObject bulletImpact = Instantiate(
                                guns[selectedGun].bulletImpact,
                                hit2.point + (hit2.normal * 0.02f),
                                // hit.normal : ヒットしたコライダーの９０度, Vector3.up : Y軸を上とする
                                Quaternion.LookRotation(hit2.normal, Vector3.up));

                            Destroy(bulletImpact, 10f);
                        }
                    }
                }
            }
        }

        // 射撃間隔の設定
        shotTimer = Time.time + guns[selectedGun].shootInterval;

        // 音を鳴らす
        photonView.RPC("SoundGeneration", RpcTarget.All);
    }

    void UpdateLaserPoint()
    {
        if (platform != "Oculus") return;

        SetOVRCameraRigPos();

        if (AimMode != AimMode.RightHand) return;

        Ray ray = new Ray(rightController.transform.position, rightController.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, ~eyeAreaLayer.value))
        {
            if (hit.collider.gameObject != photonView.gameObject)
            {
                laserPoint.transform.position = hit.point + (hit.normal * 0.03f);
                laserPoint.transform.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
                if (allowSwitchGuns) laserPoint.SetActive(true);
                else laserPoint.SetActive(false);

                laserSight.SetPosition(0, rightController.transform.position);
                laserSight.SetPosition(1, hit.point);
            }
            else
            {
                laserSight.SetPosition(0, rightController.transform.position);
                laserSight.SetPosition(1, rightController.transform.position + rightController.transform.forward * 10000);
                laserPoint.SetActive(false);
            }
        }
        else
        {
            laserSight.SetPosition(0, rightController.transform.position);
            laserSight.SetPosition(1, rightController.transform.position + rightController.transform.forward * 10000);
            laserPoint.SetActive(false);
        }
    }
    private void Reload()
    {
        // ボタン判定
        if (Input.GetKeyDown(KeyCode.R)
        || OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch)) // A
        {
            Reloading();
        }
    }

    private void Reloading()
    {
        if (isReloading) return;

        // リロードで補充する弾数を取得
        int amountNeed = maxAmmoClip[selectedGun] - ammoClip[selectedGun];

        // 補充する弾数が所持弾薬を超えていなければ変数に格納、超えていれば所持弾薬全て格納
        int ammoAvailable = amountNeed < ammunition[selectedGun] ? amountNeed : ammunition[selectedGun];

        // 弾薬が満タンの時はリロードできない ＆ 弾薬を所持しているとき
        if (amountNeed != 0 && ammunition[selectedGun] != 0)
        {
            // 所持弾薬からリロードする弾薬を引く
            ammunition[selectedGun] -= ammoAvailable;

            // 銃に弾薬をセット
            ammoClip[selectedGun] += ammoAvailable;

            allowSwitchGuns = false;
            switchGunsTime = ReloadingTime;
            isReloading = true;
            uIManager.ShowGunChangeUI();
        }
    }

    // アニメーションの関数呼び出し
    public void AnimatorSet()
    {
        // Walk判定
        if (moveDir != Vector3.zero)
        {
            animator.SetBool("walk", true);
        }
        else
        {
            animator.SetBool("walk", false);
        }

        // run判定
        if (Input.GetKey(KeyCode.LeftShift))
        {
            animator.SetBool("run", true);
        }
        else
        {
            animator.SetBool("run", false);
        }
    }

    // 銃切り替え関数
    [PunRPC] // リモート呼び出し可能にする、ほかのユーザーからも呼び出し可能にする
    public void SetGun(int gunNo)
    {
        if (gunNo < guns.Count)
        {
            selectedGun = gunNo;

            SwitchGun();
        }
    }

    // 被弾関数（全プレーヤー共有）
    [PunRPC]
    public void Hit(int damage, string killerName, int actor) // (ダメージ、撃ってきた相手の名前、プレイヤーの管理番号)
    {
        // HPを減らす関数
        ReceiveDamage(damage, killerName, actor);
    }

    // HPを減らす関数
    public void ReceiveDamage(int damage, string killerName, int actor)
    {
        if (photonView.IsMine)
        {
            currentHP -= damage;

            if (currentHP <= 0)
            {
                // 死亡関数
                Death(killerName, actor);
            }

            currentHP = Mathf.Clamp(currentHP, 0, maxHP);

            // HPをスライダーに反映
            uIManager.UpdateHP(maxHP, currentHP);
            photonView.RPC("UpdateHPSlider", RpcTarget.All, maxHP, currentHP);
        }
    }

    // 銃の音を鳴らす
    [PunRPC]
    public void SoundGeneration()
    {
        if (selectedGun == 2)
        {
            guns[selectedGun].SoundLoopOnMachineGun();
        }
        else
        {
            guns[selectedGun].SoundGunShot();
        }
    }

    // 銃の音を止める
    [PunRPC]
    public void SoundStop()
    {
        guns[2].SoundLoopOffMachineGun();
    }

    public void OculusResetUI()
    {
        if (platform == "Oculus")
        {
            uIManager.ResetUICanvas();
            oculusGunsHolder.transform.SetParent(this.gameObject.transform);
        }
    }
    // 死亡関数
    public void Death(string killerName, int actor)
    {
        Debug.Log("Death");

        gameManager.isDead = true;
        laserSight.enabled = false;

        currentHP = 0;

        OculusResetUI();

        Destroy(myIcon);
        photonView.RPC("AllDestroyThisPlayerMapIcon", RpcTarget.All, photonView.Owner.ActorNumber);
        photonView.RPC("AllDestroyThisPlayerMapIcon", RpcTarget.All, actor);
        Debug.Log("EnemyPlayers.Count : " + EnemyPhotonViews.Count);
        for (int i = 0; i < EnemyPhotonViews.Count; i++)
        {
            Debug.Log("for内 : " + EnemyPhotonViews[i].Owner.NickName);
            EnemyPhotonViews[i].RPC("DecrementEyeAreaCounter", EnemyPhotonViews[i].Owner);
        }
        Debug.Log("EnemyPlayers.Count : " + EnemyPhotonViews.Count);
        RemoveAllEnemyPlayerObject();


        Kill(photonView.Owner.NickName, actor);
        photonView.RPC("Kill", RpcTarget.All, photonView.Owner.NickName, actor);

        float reSpawnTime = 5f;
        if (PhotonNetwork.PlayerList.Length > 2)
        {
            float addTime = (PhotonNetwork.PlayerList.Length - 2) * 5;
            reSpawnTime += addTime;
        }

        uIManager.UpdateDeathUI(killerName, reSpawnTime);

        spawnManager.Die(reSpawnTime);

        // キルデスイベント呼び出し (actor, state(0:キル 1:デス), amount)
        gameManager.ScoreGet(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1); // 自分が死 デス+1
        gameManager.ScoreGet(actor, 0, 1); // 自分を倒した相手 キル+1
    }

    [PunRPC]
    public void Kill(string deathName, int actor)
    {
        Debug.Log("Kill");
        if (PhotonNetwork.LocalPlayer.ActorNumber == actor)
        {
            uIManager.UpdateKillUI(deathName);
        }
    }

    [PunRPC]
    public void AllDestroyThisPlayerMapIcon(int actor)
    {
        Debug.Log("AllDestroyThisPlayerMapIcon");

        for (int i = uIManager.worldObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = uIManager.worldObjects[i];
            if (obj == null)
            {
                uIManager.worldObjects.RemoveAt(i); // nullのオブジェクトをリストから削除
            }
            else
            {
                PhotonView photonView = obj.GetPhotonView();
                if (photonView != null && photonView.Owner.ActorNumber == actor)
                {
                    RemoveWorldObject(obj); // オブジェクトを削除するメソッドを呼び出す
                    break; // 該当するオブジェクトが見つかったらループを抜ける
                }
            }
        }
    }

    // このコンポーネントがオフになったとき呼ばれる
    public override void OnDisable()
    {
        // マウス表示
        cursorLock = false;
        Cursor.lockState = CursorLockMode.None;
    }

    // 特定のボタンが押されたらメニューに戻る
    public void OutGame()
    {
        if (Input.GetKeyDown(KeyCode.M)
        || OVRInput.GetDown(OVRInput.Button.Start))
        {
            if (platform == "Oculus")
            {
                uIManager.ResetUICanvas();
            }

            photonView.RPC("AllDestroyThisPlayerMapIcon", RpcTarget.All, photonView.Owner.ActorNumber);
            RemoveAllEnemyPlayerObject();

            // 自分のPlayerPrefs.GetString("playerName")を削除
            if (PlayerPrefs.HasKey("playerName"))
            {
                PlayerPrefs.DeleteKey("playerName");
            }

            // ローカルプレイヤーのカスタムプロパティの内容をすべて削除
            ExitGames.Client.Photon.Hashtable properties = PhotonNetwork.LocalPlayer.CustomProperties;
            properties.Clear();
            PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

            // プレイヤーリストからプレイヤー削除
            gameManager.OutPlayerGet(PhotonNetwork.LocalPlayer.ActorNumber);

            // シーン同期の解除
            PhotonNetwork.AutomaticallySyncScene = false;

            // ルームを抜ける
            PhotonNetwork.LeaveRoom();
        }
    }
}
