using UnityEngine;
using Photon.Pun;

public class Gun : MonoBehaviour
{
    // 射撃間隔
    [Tooltip("射撃間隔")]
    public float shootInterval = 0.1f;

    // 威力
    [Tooltip("威力")]
    public int shootDamage;

    // 覗き込み時のズーム
    [Tooltip("覗き込み時のズーム")]
    public float adsZoom;

    // 覗き込み時の速度
    [Tooltip("覗き込み時の速度")]
    public float acsSpeed;

    // 当たり判定
    [Tooltip("当たり判定")]
    public float hitBox;

    // 弾痕オブジェクト
    public GameObject bulletImpact;

    public AudioSource shotSound;

    public PhotonView playerPhotonView;


    // シングル銃音
    public void SoundGunShot()
    {
        shotSound.Play();
    }

    // マシンガン音
    public void SoundLoopOnMachineGun()
    {
        if (!shotSound.isPlaying)
        {
            shotSound.loop = true;
            shotSound.Play();
        }
    }

    // マシンガン音ストップ
    public void SoundLoopOffMachineGun()
    {
        shotSound.loop = false;
        shotSound.Stop();
    }
}
