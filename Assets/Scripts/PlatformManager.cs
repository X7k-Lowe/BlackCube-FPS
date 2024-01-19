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
