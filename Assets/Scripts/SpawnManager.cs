using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    // SpawnPoints格納配列
    public Transform[] spawnPoints;
    public Transform[] healingCubeSpawnPoints;

    // 生成するプレイヤーオブジェクト
    public GameObject playerPrefab;

    public GameObject healingCubePrefab;

    // 生成したプレイヤーオブジェクト
    private GameObject player;


    // スポーンまでのインターバル
    public float respawnInterval = 5f;

    public bool IsEnd { get; set; } = false;
    private Player[] playerList;

    private List<int> usedSpawnPointIndexList;

    void Awake()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // マスタークライアントが利用可能なスポーンポイントのリストを初期化
            playerList = PhotonNetwork.PlayerList;
            for (int i = 0; i < playerList.Length; i++)
            {
                int spawnPointIndex = GetSpawnPointIndex();
                ExitGames.Client.Photon.Hashtable customProperties = playerList[i].CustomProperties;
                customProperties.Add("SpawnPointIndex", spawnPointIndex);
                playerList[i].SetCustomProperties(customProperties);
            }
        }
    }
    private void Start()
    {
        // スポーンオブジェクトすべて非表示
        foreach (Transform position in spawnPoints)
        {
            position.gameObject.SetActive(false);
        }
        // ヒーリングキューブのスポーンポイントも非表示
        foreach (Transform position in healingCubeSpawnPoints)
        {
            position.gameObject.SetActive(false);
        }

        if (PhotonNetwork.IsConnected)
        {
            // ネットワークオブジェクトとしてプレイヤーを生成する
            // SpawnPlayer();
            StartCoroutine(FirstSpawnPlayer());
            SpawnHealingCubes();
        }
    }

    // ランダムにスポーンポイントの一つを選択し、使用後にリストから削除する関数
    private int GetSpawnPointIndex()
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);
        while (usedSpawnPointIndexList.Contains(randomIndex))
        {
            randomIndex = Random.Range(0, spawnPoints.Length);
        }
        usedSpawnPointIndexList.Add(randomIndex);
        return randomIndex;
    }

    // ネットワークオブジェクトとしてプレイヤーを生成する
    public IEnumerator FirstSpawnPlayer()
    {
        if (IsEnd) yield break;

        ExitGames.Client.Photon.Hashtable customProperties = PhotonNetwork.LocalPlayer.CustomProperties;

        float timeout = 5.0f; // タイムアウト時間を5秒に設定
        float timer = 0.0f; // タイマーの初期化

        while (customProperties["SpawnPointIndex"] == null)
        {
            if (timer > timeout)
            {
                Debug.LogError("SpawnPointIndexの取得がタイムアウトしました。");
                yield break; // タイムアウトした場合はコルーチンを終了
            }

            timer += Time.deltaTime; // タイマーを更新
            yield return null;
        }

        int spawnPointIndex = (int)customProperties["SpawnPointIndex"];
        Transform spawnPoint = spawnPoints[spawnPointIndex];
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    // ランダムにスポーンポイントの一つを選択する関数
    public Transform GetSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    // ネットワークオブジェクトとしてプレイヤーを生成する
    public void SpawnPlayer()
    {
        if (IsEnd) return;

        // ランダムなスポーンポジションを格納
        Transform spawnPoint = GetSpawnPoint();

        // ネットワークオブジェクト生成
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
    }

    // ヒーリングキューブを生成する関数
    public void SpawnHealingCube()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // ランダムなスポーンポジションを格納
        Transform healingCubeSpawnPoint = healingCubeSpawnPoints[Random.Range(0, healingCubeSpawnPoints.Length)];

        // ネットワークオブジェクト生成
        PhotonNetwork.Instantiate(healingCubePrefab.name, healingCubeSpawnPoint.position, healingCubeSpawnPoint.rotation);
    }

    public void SpawnHealingCubes()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // int numberOfCubes = 10;
        int numberOfCubes = PhotonNetwork.PlayerList.Length;


        // スポーンポイントのリストを作成
        List<Transform> availableSpawnPoints = new List<Transform>(healingCubeSpawnPoints);

        for (int i = 0; i < numberOfCubes; i++)
        {
            if (availableSpawnPoints.Count == 0) break; // スポーンポイントがなくなったら終了

            // ランダムなスポーンポジションを取得
            int randomIndex = Random.Range(0, availableSpawnPoints.Count);
            Transform healingCubeSpawnPoint = availableSpawnPoints[randomIndex];

            // ネットワークオブジェクト生成
            PhotonNetwork.Instantiate(healingCubePrefab.name, healingCubeSpawnPoint.position, healingCubeSpawnPoint.rotation);

            // 使用したスポーンポイントをリストから削除
            availableSpawnPoints.RemoveAt(randomIndex);
        }
    }

    // 削除とリスポーン
    public void Die(float reSpawnTime)
    {
        Debug.Log("Die");
        if (player != null)
        {
            // リスポーン関数を呼ぶ
            Invoke("SpawnPlayer", reSpawnTime);
        }
        PhotonNetwork.Destroy(player);
    }
}
