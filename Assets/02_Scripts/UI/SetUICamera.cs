using System;
using UnityEngine;

public class SetUICamera : MonoBehaviour
{
    private void Start()
    {
        var uiCamera = GameObject.Find("UI Camera");
        GetComponent<Canvas>().worldCamera = uiCamera.GetComponent<Camera>();
    }
}
