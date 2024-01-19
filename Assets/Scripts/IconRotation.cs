using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IconRotation : MonoBehaviour
{
    // 回転速度（度/秒）
    public float rotationSpeed = 70.0f;

    // Updateメソッドは毎フレーム呼ばれる
    void Update()
    {
        // Time.deltaTimeで前のフレームからの経過時間を取得
        float deltaRotation = rotationSpeed * Time.deltaTime;

        // y軸を中心にオブジェクトを回転させる
        transform.Rotate(0.0f, deltaRotation, 0.0f, Space.World);
    }
}
