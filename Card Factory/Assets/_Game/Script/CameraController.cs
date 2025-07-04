using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public enum DeviceType
{
    NormalPhone,
    LongPhone,
    Tablet,
    FoldZ,
}

public class CameraController : Singleton<CameraController>
{
    public float WorldScaleManitude { get; private set; } = 1;
    public float UIScaleManitude { get; private set; } = 1;
    private float initOrthoSizeValue;
    private float initFOVValue;
    public DeviceType DeviceType { get; private set; }

    [Header("RoomSupport")]
    [SerializeField] private float zoomMagnitude = 1f;
    [SerializeField] private float camMoveTime = 1f;


    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        SetUpRatio();
    }

    private void SetUpRatio()
    {
        float originRatio = 1080f / 1920f;
        float deviceRatio = (float)Screen.width / (float)Screen.height;

        if (deviceRatio > originRatio)  //Tablet
        {
            initOrthoSizeValue = Camera.main.orthographicSize = 5.75f;
            initFOVValue = Camera.main.fieldOfView = 13;
            WorldScaleManitude = 1.32f;
            UIScaleManitude = 0.8f;
            DeviceType = DeviceType.Tablet;
        }
        if (deviceRatio == originRatio) //Normal Phone
        {
            initOrthoSizeValue = Camera.main.orthographicSize = 6.25f;
            initFOVValue = Camera.main.fieldOfView = 13;
            WorldScaleManitude = 1f;
            UIScaleManitude = 1f;
            DeviceType = DeviceType.NormalPhone;

        }
        if (deviceRatio < originRatio) //Long Phone
        {
            initOrthoSizeValue = Camera.main.orthographicSize = 7.5f;
            initFOVValue = Camera.main.fieldOfView = 18;
            WorldScaleManitude = 1.25f;
            UIScaleManitude = 1f;
            DeviceType = DeviceType.LongPhone;
        }
        if (deviceRatio < 0.365)
        {
            Debug.Log("Samsung z fold");
            initOrthoSizeValue = Camera.main.orthographicSize = 10f;
            initFOVValue = Camera.main.fieldOfView = 22;
            WorldScaleManitude = 1.25f;
            UIScaleManitude = 1f;
            DeviceType = DeviceType.FoldZ;
        }
    }

}
