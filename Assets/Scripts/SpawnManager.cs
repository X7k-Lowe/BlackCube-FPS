using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

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
            SpawnPlayer();
            SpawnHealingCubes();
        }
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
