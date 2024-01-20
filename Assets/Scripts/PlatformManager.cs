using UnityEngine;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager Instance { get; private set; }

    public string Platform { get; private set; }

    private void Awake()
    {
        // 既にインスタンスが存在する場合は、新たに生成されたオブジェクトを破棄する
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        // このオブジェクトをシーン間で維持する
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // プラットフォームの判定を行う
        DetectPlatform();
    }

    void Update()
    {
        if (Platform == "Windows")
        {
            WindowAspectRatio();
        }
    }

    public void WindowAspectRatio()
    {
        // 現在のウィンドウサイズを取得
        var windowSize = new Vector2(Screen.width, Screen.height);
        // 目的のアスペクト比を計算
        float targetAspectRatio = 16f / 9f;
        // 現在のアスペクト比を計算
        var currentAspectRatio = windowSize.x / windowSize.y;

        // ウィンドウのアスペクト比が目的のアスペクト比と異なる場合にサイズ調整
        if (currentAspectRatio != targetAspectRatio)
        {
            // ウィンドウサイズを調整
            Screen.SetResolution((int)(windowSize.y * targetAspectRatio), (int)windowSize.y, false);
        }
    }
    private void DetectPlatform()
    {
#if UNITY_EDITOR
        Platform = "Windows"; // エディター上でWindowsとして模倣する場合
        // Platform = "Oculus"; // エディター上でQuest2を模倣する場合
#elif UNITY_STANDALONE_WIN
        Platform = "Windows";
#elif UNITY_ANDROID
        Platform = "Oculus";
#else
        Platform = "Unknown";
#endif
    }
}
