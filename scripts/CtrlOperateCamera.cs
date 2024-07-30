using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UserHandleSpace;

public class CtrlOperateCamera : MonoBehaviour
{

    private Vector2 dig_value;
    private Vector2 move_value;
    private Vector2 rotate_value;
    [SerializeField]
    private CameraOperation1 camoperator;
    [SerializeField]
    private OperateActiveVRM oavrm;
    [SerializeField]
    private PlayerInput pinput;

    public UserHandleSpace.KeyOperationMode keyOperationMode;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void FixedUpdate()
    {
        if (move_value.magnitude >= 0.1f)
        {
            if (keyOperationMode == UserHandleSpace.KeyOperationMode.MoveCamera)
            {
                camoperator.TranslateCameraPosFromOuter(move_value.x.ToString() + ",0,0");
                camoperator.ProgressCameraPosFromOuter(move_value.y);
            }
            else if (keyOperationMode == UserHandleSpace.KeyOperationMode.MoveAvatar) 
            {
                oavrm.TranslateByController(move_value);
            }

            
        }
        if (rotate_value.magnitude >= 0.1f)
        {
            if (keyOperationMode == UserHandleSpace.KeyOperationMode.MoveCamera)
            {
                camoperator.RotateCameraPosFromOuter(rotate_value.x.ToString() + "," + rotate_value.y.ToString());
            }
            else if (keyOperationMode == UserHandleSpace.KeyOperationMode.MoveAvatar)
            {
                oavrm.RotateByController(rotate_value);
            }
        }
        if (dig_value.magnitude >= 0.1f)
        {
            if (keyOperationMode == UserHandleSpace.KeyOperationMode.MoveCamera)
            {
                camoperator.TranslateCameraPosFromOuter("0," + dig_value.y.ToString() + ",0");
            }
            else if (keyOperationMode == UserHandleSpace.KeyOperationMode.MoveAvatar)
            {
                oavrm.UpDownByController(dig_value);
            }
        }
    }
    public void EnableGamePad(bool flag)
    {
        pinput.enabled = flag;
        
    }
    public void EnableGamePadFromOuter(string param)
    {
        if (param == "1")
        {
            EnableGamePad(true);
        }
        else
        {
            EnableGamePad(false);
        }
    }
    public void GamepadLeftStickFromOuter(string param)
    {
        string[] arr = param.Split(',');
        string x = arr[0];
        string y = arr[1];
        float fx = float.Parse(x);
        float fy = float.Parse(y);

        if (keyOperationMode == UserHandleSpace.KeyOperationMode.MoveCamera)
        {
            camoperator.TranslateCameraPosFromOuter(x + ",0,0");
            camoperator.ProgressCameraPosFromOuter(fy);
        }
        else if (keyOperationMode == UserHandleSpace.KeyOperationMode.MoveAvatar)
        {
            oavrm.TranslateByController(new Vector2(fx, fy));
        }
    }
    public void GamepadRightStickFromOuter(string param)
    {
        string[] arr = param.Split(',');
        string x = arr[0];
        string y = arr[1];
        float fx = float.Parse(x);
        float fy = float.Parse(y);

        if (keyOperationMode == UserHandleSpace.KeyOperationMode.MoveCamera)
        {
            camoperator.RotateCameraPosFromOuter(x + "," + y);
        }
        else if (keyOperationMode == UserHandleSpace.KeyOperationMode.MoveAvatar)
        {
            oavrm.RotateByController(new Vector2(fx, fy));
        }
    }
    public void GamepadDpadFromOuter(string param)
    {
        string[] arr = param.Split(',');
        string x = arr[0];
        string y = arr[1];
        float fx = float.Parse(x);
        float fy = float.Parse(y);

        if (keyOperationMode == UserHandleSpace.KeyOperationMode.MoveCamera)
        {
            camoperator.TranslateCameraPosFromOuter("0," + y + ",0");
        }
        else if (keyOperationMode == UserHandleSpace.KeyOperationMode.MoveAvatar)
        {
            oavrm.UpDownByController(new Vector2(fx, fy));
        }
    }
    public void GamepadKeyFromOuter(string param)
    {
        if (param == "L1")
        {
            camoperator.ChangeOperateTarget(UserHandleSpace.KeyOperationMode.MoveCamera);
            keyOperationMode = UserHandleSpace.KeyOperationMode.MoveCamera;
        }
        else if (param == "R1")
        {
            camoperator.ChangeOperateTarget(UserHandleSpace.KeyOperationMode.MoveAvatar);
            keyOperationMode = UserHandleSpace.KeyOperationMode.MoveAvatar;
        }
        else if (param == "select")
        {
            if (camoperator.GetCurrentOperationTarget() == KeyOperationMode.MoveAvatar)
            {
                oavrm.ChangeOperationSpaceFromController();
            }
        }
    }
    public void OnMove(InputAction.CallbackContext context)
    {        
        move_value = context.ReadValue<Vector2>();
    }
    public void OnRotate(InputAction.CallbackContext context)
    {
        rotate_value = context.ReadValue<Vector2>();
    }
    public void OnUpDown(InputAction.CallbackContext context)
    {
        dig_value = context.ReadValue<Vector2>();
    }
    /// <summary>
    /// Change operation target (MainCamera)
    /// </summary>
    /// <param name="context"></param>
    public void OnChangeTarget2Camera(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (context.interaction is PressInteraction)
        {
            camoperator.ChangeOperateTarget(UserHandleSpace.KeyOperationMode.MoveCamera);
        }
    }

    /// <summary>
    /// Change operation target (ActiveAvatar)
    /// </summary>
    /// <param name="context"></param>
    public void OnChangeTarget2Object(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (context.interaction is PressInteraction)
        {
            camoperator.ChangeOperateTarget(UserHandleSpace.KeyOperationMode.MoveAvatar);
        }
    }

    /// <summary>
    /// Change the operation space of the Avatar
    /// </summary>
    /// <param name="context"></param>
    public void OnSelectSpace(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (context.interaction is PressInteraction)
        {
            if (camoperator.GetCurrentOperationTarget() == KeyOperationMode.MoveAvatar)
            {
                oavrm.ChangeOperationSpaceFromController();
            }
        }
    }
}
