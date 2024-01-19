using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteFacesCamera : MonoBehaviour
{
    public Camera useCamera { get; set; }
    void Update()
    {
        if (useCamera != null)
            transform.rotation = useCamera.transform.rotation;
    }
}
