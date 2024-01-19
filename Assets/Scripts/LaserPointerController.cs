using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
public class LaserPointerController : MonoBehaviour
{
    public Transform handAnchor; // VRコントローラーの位置
    public LineRenderer laser; // LineRendererコンポーネント
    public float laserMaxLength; // レーザーの最大長
    public RaycastHit hit;
    public GameObject hitObject;
    public EventSystem eventSystem;

    void Update()
    {
        UpdateLaser();
        if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) != 0)
        {
            EventSystem.current.SetSelectedGameObject(hitObject);
        }
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            if (eventSystem.currentSelectedGameObject != null)
            {
                Button button = eventSystem.currentSelectedGameObject.GetComponent<Button>();

                // もしButtonコンポーネントがあれば、OnClickイベントを発動する
                if (button != null)
                {
                    button.onClick.Invoke();
                    button.OnSubmit(null);
                }
            }
        }
    }

    void UpdateLaser()
    {
        // レイキャストをコントローラーの位置から前方に発射
        if (Physics.Raycast(handAnchor.position, handAnchor.forward, out hit, laserMaxLength))
        {
            // ヒットした場合、レーザーの長さをヒット地点までにする
            laser.SetPosition(0, handAnchor.position);
            laser.SetPosition(1, hit.point);
            hitObject = hit.collider.gameObject;
        }
        else
        {
            // ヒットしない場合、レーザーの長さを最大にする
            laser.SetPosition(0, handAnchor.position);
            laser.SetPosition(1, handAnchor.position + handAnchor.forward * laserMaxLength);
        }
    }
}
