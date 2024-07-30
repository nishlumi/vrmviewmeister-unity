using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UserHandleSpace;

public class CtrlOperateVpad : MonoBehaviour
{
    [SerializeField]
    private CameraOperation1 camoperator;
    [SerializeField]
    private OperateActiveVRM oavrm;
    //[SerializeField]
    //private PlayerInput pinput;

    private UserHandleSpace.KeyOperationMode keyOperationMode;

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
       
    }
    public void VpadLeftStickFromOuter(string param)
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
    public void VpadRightStickFromOuter(string param)
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
    public void VpadDpadFromOuter(string param)
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
    public void VpadKeyFromOuter(string param)
    {
        if (param == "L1")
        {
            //camoperator.ChangeOperateTarget(UserHandleSpace.KeyOperationMode.MoveCamera);
            keyOperationMode = UserHandleSpace.KeyOperationMode.MoveCamera;
        }
        else if (param == "R1")
        {
            //camoperator.ChangeOperateTarget(UserHandleSpace.KeyOperationMode.MoveAvatar);
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
}
