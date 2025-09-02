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
    private ManageAnimation manim;
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
                camoperator.TranslateCameraPos(move_value.x, 0, 0);
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
                camoperator.RotateCameraPos(rotate_value.x, rotate_value.y, 0);
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
                camoperator.TranslateCameraPos(0, dig_value.y, 0);
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
            camoperator.TranslateCameraPos(fx, 0, 0);
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
            camoperator.RotateCameraPos(fx, fy, 0);
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
            camoperator.TranslateCameraPos(0, fy, 0);
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

    //####################################################################################################################3
    // Function for key, device operation
    //####################################################################################################################3
    public void OnMove(InputAction.CallbackContext context)
    {        
        move_value = context.ReadValue<Vector2>();
        Debug.Log("OnMove");
    }
    public void OnRotate(InputAction.CallbackContext context)
    {
        rotate_value = context.ReadValue<Vector2>();
    }
    public void OnUpDown(InputAction.CallbackContext context)
    {
        dig_value = context.ReadValue<Vector2>();
    }
    public void OnKeyRotateForward(InputAction.CallbackContext context)
    {
        camoperator.RotateCameraPos(Vector3.left.x * manim.cfg_keymove_speed_rot, 0, 0);
    }
    public void OnKeyRotateBackward(InputAction.CallbackContext context)
    {
        camoperator.RotateCameraPos(Vector3.right.x * manim.cfg_keymove_speed_rot, 0, 0);
    }
    public void OnKeyRotateLeft(InputAction.CallbackContext context)
    {
        camoperator.RotateCameraPos(0, Vector3.down.y * manim.cfg_keymove_speed_rot, 0);
    }
    public void OnKeyRotateRight(InputAction.CallbackContext context)
    {
        camoperator.RotateCameraPos(0, Vector3.up.y * manim.cfg_keymove_speed_rot, 0);
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
            keyOperationMode = manim.keyOperationMode;
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
            keyOperationMode = manim.keyOperationMode;
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
    public void OnKeyChangeTarget(InputAction.CallbackContext context)
    {

        if (manim.keyOperationMode == KeyOperationMode.MoveAvatar)
        {
            camoperator.ChangeOperateTarget(UserHandleSpace.KeyOperationMode.MoveCamera);
        }
        else
        {
            camoperator.ChangeOperateTarget(UserHandleSpace.KeyOperationMode.MoveAvatar);
        }
        keyOperationMode = manim.keyOperationMode;
    }
}
