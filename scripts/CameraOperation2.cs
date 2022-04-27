using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraOperation2 : MonoBehaviour
{
    public GameObject MainCamera;
    public GameObject TargetObject;

    public bool isRotateMode;
    public float RotateSpeed;

    private Vector3 lastMousePosition;
    private Vector3 newAngle = new Vector3(0, 0, 0);

    void Start()
    {
        DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
    }
    void Update()
    {


        if (isRotateMode)
        {
            transform.RotateAround
            (
                TargetObject.transform.position,
                Vector3.up,
                RotateSpeed * Time.deltaTime
            );
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                newAngle = MainCamera.transform.localEulerAngles;
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                newAngle.y -= (Input.mousePosition.x - lastMousePosition.x) * 0.1f;
                newAngle.x -= (Input.mousePosition.y - lastMousePosition.y) * 0.1f;
                MainCamera.gameObject.transform.localEulerAngles = newAngle;

                lastMousePosition = Input.mousePosition;
            }

            if (Input.GetMouseButtonUp(1))
            {
                var rotation = Quaternion.LookRotation(TargetObject.transform.position - MainCamera.transform.position);

                MainCamera.transform.DORotateQuaternion(rotation, 0.5f)
                    .SetEase(Ease.InOutBounce)
                    .OnComplete(() => Debug.Log("Finished"));
            }
        }
    }
}
