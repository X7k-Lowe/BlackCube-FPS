using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Billboard : MonoBehaviour
{
    public Camera useCamera { get; set; }
    public TextMeshProUGUI nameText; // 名前を表示するTextコンポーネント
    public RectTransform background; // 名前の背景
    public Slider hpSlider; // HPバーのSliderコンポーネント

    void Start()
    {

    }

    public void UpdateHP(int maxHP, int currentHP)
    {
        hpSlider.maxValue = maxHP;
        hpSlider.value = currentHP;
    }
    void Update()
    {
        // Canvasをカメラに向ける
        if (gameObject != null && useCamera != null)
            transform.LookAt(transform.position + useCamera.transform.rotation * Vector3.forward,
                useCamera.transform.rotation * Vector3.up);
    }
}
