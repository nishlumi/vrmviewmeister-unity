﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using RuntimeGizmos;
using UserHandleSpace;

public class CameraOperation1 : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ReceiveStringVal(string val);

    [DllImport("__Internal")]
    private static extern void ReceiveIntVal(int val);
    [DllImport("__Internal")]
    private static extern void ReceiveFloatVal(float val);

    public GameObject targetObject;
    public Vector2 rotationSpeed;
    public float zoomSpeed;
    public bool reverse;

    public GameObject rotateTargetObject;
    public float rotateTargetSpeed;
    public bool isRotateMode;

    public Camera Front3DCam;
    public Camera Front2DCam;
    public Camera FrontIKCam;
    public Camera VRCameraL;
    public Camera VRCameraR;
    public Camera ARCameraL;
    public Camera ARCameraR;

    [SerializeField]
    private GameObject mainCamera;
    private Vector3 lastMousePos;
    private Vector3 lastDragPos;
    private Vector3 newAngle = Vector3.zero;
    private bool virtualRightClick;

    private Text Dbg_diff;
    private Text Dbg_mouse;

    private ConfigSettingLabs configLab;
    private float distance_camera2viewpoint;
    private float camera_keymove_speed;
    private float camera_keymove_speed_t;

    //---for backup variables
    private Color bkup_skyColor;
    private CameraClearFlags bkup_clearFlags;
    private string bkup_skyShader;
    private List<BasicStringFloatList> bkup_skyMaterialFloat;
    private List<BasicStringColorList> bkup_skyMaterialColor;

    [SerializeField]
    private Material[] skyMaterials = { };

    private ManageAnimation manim;
    [SerializeField]
    private CameraManagerXR camxr;

    public TMPro.TextMeshProUGUI KeyOperationModeView;
    public TMPro.TextMeshProUGUI KeyObjGlobalLocal;

    // Start is called before the first frame update
    void Start()
    {
        virtualRightClick = false;

        manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();


        //Debug.Log(RenderSettings.skybox.shader.name);

        configLab = GameObject.Find("Canvas").GetComponent<ConfigSettingLabs>();
        //mainCamera = GetComponent<Camera>(); // Camera.main;
        //Dbg_diff = GameObject.Find("Dbg_diff").GetComponent<Text>();
        //Dbg_mouse = GameObject.Find("Dbg_mouse").GetComponent<Text>();

        distance_camera2viewpoint = configLab.GetFloatVal("distance_camera_viewpoint", 2.5f);

        //### [Hidden the editor only UI] ###
#if !UNITY_EDITOR && UNITY_WEBGL
        WebGLInput.captureAllKeyboardInput = false;
        //GameObject futureHide = GameObject.Find("Canvas").transform.Find("futureHide").gameObject;
        //if (futureHide != null) futureHide.SetActive(false);

        GameObject newUI = GameObject.Find("newUI").gameObject;
        if (newUI != null) newUI.SetActive(false);
#endif

        bkup_skyMaterialFloat = new List<BasicStringFloatList>();
        bkup_skyMaterialColor = new List<BasicStringColorList>();

        GetDefaultSky();
    }

    // Update is called once per frame
    void Update()
    {
        if (!camxr.isActiveNormal()) return;
        if (mainCamera == null) return;

        //distance_camera2viewpoint = configLab.GetFloatVal("distance_camera_viewpoint", 2.5f);
        //camera_keymove_speed = configLab.GetFloatVal("camera_keymove_speed", 0.1f);
        //camera_keymove_speed_t = camera_keymove_speed / 10;

        if (isRotateMode)
        {
            execCameraRotation();
            return;
        }
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
        if (scrollWheel != 0.0f)
        {
            mainCamera.transform.position += mainCamera.transform.forward * scrollWheel * zoomSpeed;
            targetObject.transform.position += mainCamera.transform.forward * scrollWheel * zoomSpeed;
        }
        
        //---special shortcut key---
        if (manim.keyOperationMode == KeyOperationMode.MoveCamera)
        {
            if (Input.GetKey(KeyCode.W))
            {
                
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    //rotate to up
                    mainCamera.transform.Rotate(Vector3.left * manim.cfg_keymove_speed_rot);
                    targetObject.transform.Rotate(Vector3.left * manim.cfg_keymove_speed_rot);
                    targetObject.transform.position = mainCamera.transform.position;
                    targetObject.transform.Translate(Vector3.forward * manim.cfg_dist_cam2view);
                }
                else
                {
                    //to front
                    mainCamera.transform.Translate(Vector3.forward * manim.cfg_keymove_speed_trans);
                    targetObject.transform.Translate(Vector3.forward * manim.cfg_keymove_speed_trans);
                }

            }
            if (Input.GetKey(KeyCode.S))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    //rotate to down
                    mainCamera.transform.Rotate(Vector3.right * manim.cfg_keymove_speed_rot);
                    targetObject.transform.Rotate(Vector3.right * manim.cfg_keymove_speed_rot);
                    targetObject.transform.position = mainCamera.transform.position;
                    targetObject.transform.Translate(Vector3.forward * manim.cfg_dist_cam2view);
                }
                else
                {
                    //to back
                    mainCamera.transform.Translate(Vector3.back * manim.cfg_keymove_speed_trans);
                    targetObject.transform.Translate(Vector3.back * manim.cfg_keymove_speed_trans);
                }

            }
            if (Input.GetKey(KeyCode.A))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    //rotate to left
                    mainCamera.transform.Rotate(Vector3.down * manim.cfg_keymove_speed_rot, Space.World);
                    targetObject.transform.Rotate(Vector3.down * manim.cfg_keymove_speed_rot, Space.World);
                    targetObject.transform.position = mainCamera.transform.position;
                    targetObject.transform.Translate(Vector3.forward * manim.cfg_dist_cam2view);
                }
                else
                {
                    //to left
                    mainCamera.transform.Translate(Vector3.left * manim.cfg_keymove_speed_trans);
                    targetObject.transform.Translate(Vector3.left * manim.cfg_keymove_speed_trans);
                }

            }
            if (Input.GetKey(KeyCode.D))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    //rotate to right
                    mainCamera.transform.Rotate(Vector3.up * manim.cfg_keymove_speed_rot, Space.World);
                    targetObject.transform.Rotate(Vector3.up * manim.cfg_keymove_speed_rot, Space.World);
                    targetObject.transform.position = mainCamera.transform.position;
                    targetObject.transform.Translate(Vector3.forward * manim.cfg_dist_cam2view);
                }
                else
                {
                    //to right
                    mainCamera.transform.Translate(Vector3.right * manim.cfg_keymove_speed_trans);
                    targetObject.transform.Translate(Vector3.right * manim.cfg_keymove_speed_trans);
                }

            }
            if (Input.GetKey(KeyCode.F))
            { //to up
                mainCamera.transform.Translate(Vector3.up * manim.cfg_keymove_speed_trans);
                targetObject.transform.Translate(Vector3.up * manim.cfg_keymove_speed_trans);
            }
            if (Input.GetKey(KeyCode.V))
            { //to down
                mainCamera.transform.Translate(Vector3.down * manim.cfg_keymove_speed_trans);
                targetObject.transform.Translate(Vector3.down * manim.cfg_keymove_speed_trans);
            }
            if (Input.GetKey(KeyCode.Q))
            {
                //reset rotation
                mainCamera.transform.rotation = Quaternion.Euler(new Vector3(mainCamera.transform.rotation.eulerAngles.x, mainCamera.transform.rotation.eulerAngles.y, 0));
                ResetCenterTarget();
            }
            if (Input.GetKey(KeyCode.E))
            {
            }
            if (Input.GetKey(KeyCode.G))
            {
                //---already used
            }
            if (Input.GetKey(KeyCode.B))
            {

            }
            if (Input.GetKey(KeyCode.X))
            {

            }
            if (Input.GetKey(KeyCode.R))
            {
                ResetCameraFromOuter();
            }
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (manim.keyOperationMode == KeyOperationMode.MoveCamera)
            {
                manim.keyOperationMode = KeyOperationMode.MoveAvatar;
                KeyOperationModeView.text = "O";
                KeyObjGlobalLocal.gameObject.SetActive(true);
            }
            else if (manim.keyOperationMode == KeyOperationMode.MoveAvatar)
            {
                manim.keyOperationMode = KeyOperationMode.MoveCamera;
                KeyOperationModeView.text = "C";
                KeyObjGlobalLocal.gameObject.SetActive(false);
            }
            GetComponent<CtrlOperateCamera>().keyOperationMode = manim.keyOperationMode;
        }
        if (Input.GetKey(KeyCode.I))
        {
            //manim.cfg_dist_cam2view += 0.1f;
            float dist = Vector3.Distance(transform.position, targetObject.transform.position);
            if (dist <= 5f)
            {
                targetObject.transform.Translate(Vector3.forward * 0.01f);
                manim.cfg_dist_cam2view = dist;
            }
            else if (dist > 5f) 
            {
                manim.cfg_dist_cam2view = 5f;
                targetObject.transform.Translate(Vector3.back * 0.01f);
            }
            
        }
        if (Input.GetKey(KeyCode.O))
        {
            float dist = Vector3.Distance(transform.position, targetObject.transform.position);
            //manim.cfg_dist_cam2view -= 0.1f;
            if (0.3f <= dist)
            {
                targetObject.transform.Translate(Vector3.back * 0.01f);
                manim.cfg_dist_cam2view = dist;
            }
            else if (dist < 0.3f)
            {
                manim.cfg_dist_cam2view = 0.3f;
                targetObject.transform.Translate(Vector3.forward * 0.01f);
            }
            
        }


        //---Mouse Left button
        //---Same as mouse right button 
        if (Input.GetMouseButtonDown(0))
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {

                lastMousePos = Input.mousePosition;
                newAngle = transform.localEulerAngles;
                virtualRightClick = true;
            }
            
        }
        else  if (Input.GetMouseButton(0))
        {

            if (virtualRightClick)
            {
                execCameraRotater(Input.mousePosition);

            }
            
        }
        if ((Input.GetKeyUp(KeyCode.LeftControl)) || (Input.GetKeyUp(KeyCode.RightControl)))
        {
            virtualRightClick = false;
        }
        

        //---Same as wheel drag
        if (Input.GetKey(KeyCode.Space))
        {
            
            if (Input.GetMouseButtonDown(0))
            {
                lastDragPos = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                
                //---Same as wheel drag
                if (Input.GetKey(KeyCode.Space))
                {
                    /*var delta = lastDragPos - Input.mousePosition;
                    mainCamera.transform.Translate(delta * Time.deltaTime * 0.25f);
                    targetObject.transform.Translate(delta * Time.deltaTime * 0.25f);
                    lastDragPos = Input.mousePosition;
                    */
                    execCameraMover(Input.mousePosition);
                }
            }
        }
        

        //---Mouse middle wheel
        if (Input.GetMouseButtonDown(2))
        {
            lastDragPos = Input.mousePosition;
        }else if (Input.GetMouseButton(2))
        {
            /*
            var delta = lastDragPos - Input.mousePosition;
            mainCamera.transform.Translate(delta * Time.deltaTime * 0.25f);
            targetObject.transform.Translate(delta * Time.deltaTime * 0.25f);
            lastDragPos = Input.mousePosition;
            */
            execCameraMover(Input.mousePosition);
        }

        //---Mouse Right button
        if (Input.GetMouseButtonDown(1))
        {
            lastMousePos = Input.mousePosition;
            newAngle = transform.localEulerAngles;
        }else if ( Input.GetMouseButton(1))
        {
            execCameraRotater(Input.mousePosition);
        }

        
    }
    Vector3 execCameraMover(Vector3 inputPosition)
    {
        
        var delta = lastDragPos - inputPosition;
        mainCamera.transform.Translate(delta * Time.deltaTime * 0.25f);
        targetObject.transform.Translate(delta * Time.deltaTime * 0.25f);
        lastDragPos = inputPosition;

        return inputPosition;
    }
    Vector3 execCameraRotater(Vector3 inputPosition)
    {
        var x = 0f;
        var y = 0f;
        Vector3 diff;
        if (!reverse)
        {
            x = lastMousePos.x - inputPosition.x;
            y = inputPosition.y - lastMousePos.y;
            diff = lastMousePos - inputPosition;

        }
        else
        {

            x = inputPosition.x - lastMousePos.x;
            y = lastMousePos.y - inputPosition.y;
            diff = inputPosition - lastMousePos;


        }
        if (diff.magnitude < Vector3.kEpsilon)
            return inputPosition;
        if (Mathf.Abs(x) < Mathf.Abs(y))
        {
            x = 0;
        }
        else
        {
            y = 0;
        }
        

        newAngle.x = x * rotationSpeed.x;
        newAngle.y = y * rotationSpeed.y;

        Vector3 tarpos = targetObject.transform.position;
        //tarpos.y = tarpos.y + (targetObject.transform.localScale.y * 0.15f);
        //tarpos.x = tarpos.x + (targetObject.transform.localScale.x * 0.15f);
        targetObject.transform.rotation = mainCamera.transform.rotation;

        if ((Input.GetKey(KeyCode.LeftShift)) || (Input.GetKey(KeyCode.RightShift))
            )
        {
            mainCamera.transform.Rotate(Vector3.up, diff.x);
            mainCamera.transform.Rotate(Vector3.right, -diff.y); //mainCamera.transform.right
            targetObject.transform.position = mainCamera.transform.position;
            targetObject.transform.Translate(Vector3.forward * manim.cfg_dist_cam2view);

        }
        else
        {
            mainCamera.transform.RotateAround(tarpos, Vector3.up, diff.x);
            mainCamera.transform.RotateAround(tarpos, mainCamera.transform.right, -diff.y);
        }

        //mainCamera.transform.Rotate(mainCamera.transform.rotation.x, mainCamera.transform.rotation.y, 0);


        //mainCamera.transform.LookAt(tarpos);


        //Debug.Log("transform.rotation=" + transform.rotation);
        lastMousePos = inputPosition;

        return inputPosition;
    }
    public void execCameraRotation()
    {
        if (rotateTargetObject == null) return;
        if (rotateTargetSpeed == 0f) return;

        transform.RotateAround
        (
            rotateTargetObject.transform.position,
            Vector3.up,
            rotateTargetSpeed * Time.deltaTime
        );
    }
    public void StartRotate360(float speed)
    {
        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        rotateTargetObject = ovrm.ActiveAvatar;

        rotateTargetSpeed = speed;
        isRotateMode = true;
    }
    public void StopRotate360()
    {
        isRotateMode = false;
    }

    /// <summary>
    /// Reset camera first position and rotation
    /// </summary>
    public void ResetCameraFromOuter()
    {
        mainCamera.transform.position = new Vector3(0f, 1f, -3.5f);
        mainCamera.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        ResetCenterTarget();

        mainCamera.transform.parent.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }

    /// <summary>
    /// Reset z-dimension to 0
    /// </summary>
    public void ResetCenterTarget()
    {
        targetObject.transform.position = new Vector3(0f, 1f, 0f);
        targetObject.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
    public void ResetCenterTargetFromOuter()
    {
        mainCamera.transform.rotation = Quaternion.Euler(new Vector3(mainCamera.transform.rotation.eulerAngles.x, mainCamera.transform.rotation.eulerAngles.y, 0));
        ResetCenterTarget();
    }
    public void MoveCamera2TargetDistance (int flag)
    {
        if (flag  == 1)
        { // I - key
            float dist = Vector3.Distance(transform.position, targetObject.transform.position);
            if (dist <= 5f)
            {
                targetObject.transform.Translate(Vector3.forward * 0.01f);
                manim.cfg_dist_cam2view = dist;
            }
            else if (dist > 5f)
            {
                manim.cfg_dist_cam2view = 5f;
                targetObject.transform.Translate(Vector3.back * 0.01f);
            }
        }
        else if (flag == -1)
        { // O - key
            float dist = Vector3.Distance(transform.position, targetObject.transform.position);
            //manim.cfg_dist_cam2view -= 0.1f;
            if (0.3f <= dist)
            {
                targetObject.transform.Translate(Vector3.back * 0.01f);
                manim.cfg_dist_cam2view = dist;
            }
            else if (dist < 0.3f)
            {
                manim.cfg_dist_cam2view = 0.3f;
                targetObject.transform.Translate(Vector3.forward * 0.01f);
            }
        }
    }
    public void SetTargetAndCameraDistance(float param)
    {
        targetObject.transform.position = new Vector3(mainCamera.transform.position.x, mainCamera.transform.position.y, mainCamera.transform.position.z);
        targetObject.transform.Translate(new Vector3(0, 0, param));
        manim.cfg_dist_cam2view = param;
    }
    
    //===========================================================================
    // Translate/Rotate Operation for external functions
    //===========================================================================

    /// <summary>
    /// Move main camera to center position of VRM
    /// </summary>
    /// <param name="param"></param>
    public void FocusCameraToVRMFromOuter(string param)
    {
        ManageAnimation manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

        for (int i = 0; i < manim.currentProject.casts.Count; i++)
        {
            NativeAnimationAvatar nav = manim.currentProject.casts[i];
            if ((nav.avatar != null) && (nav.avatar.name == param))
            {
                ResetCameraFromOuter();

                Animator animator = nav.avatar.GetComponent<Animator>();
                Vector3 newpos = new Vector3(nav.avatar.transform.position.x, nav.avatar.transform.position.y, nav.avatar.transform.position.z);
                mainCamera.transform.position = newpos;
                mainCamera.transform.position = new Vector3(newpos.x, newpos.y, newpos.z + (-1 * manim.cfg_dist_cam2view));

                targetObject.transform.position = newpos;
                targetObject.transform.rotation = mainCamera.transform.rotation;



                if (nav.type == AF_TARGETTYPE.VRM)
                {
                    Camera.main.transform.DOLookAt(animator.GetBoneTransform(HumanBodyBones.Head).position, 0.5f);
                }
                else
                {
                    Camera.main.transform.DOLookAt(nav.avatar.transform.position, 0.5f);
                }
                
                
                break;
            }
        }
    }
    public void CenteringCameraForAvatar(string param)
    {
        ManageAnimation manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

        for (int i = 0; i < manim.currentProject.casts.Count; i++)
        {
            NativeAnimationAvatar nav = manim.currentProject.casts[i];
            if ((nav.avatar != null) && (nav.avatar.name == param))
            {
                OperateLoadedBase ovrm = nav.avatar.GetComponent<OperateLoadedBase>();

                Vector3 newpos = new Vector3(
                    ovrm.relatedHandleParent.transform.position.x, ovrm.relatedHandleParent.transform.position.y, ovrm.relatedHandleParent.transform.position.z
                );

                if (ovrm.targetType == AF_TARGETTYPE.VRM)
                {
                    ManageAvatarTransform mat = nav.avatar.GetComponent<ManageAvatarTransform>();
                    //GameObject bodyobj = mat.GetBodyMesh();
                    //Bounds bnd = bodyobj.GetComponent<SkinnedMeshRenderer>().bounds;

                    newpos.y += (mat.GetMaximumHeightRenderer(mat.CheckSkinnedMeshAvailable()) / 2f); // (bnd.size.y / 2f);
                }
                targetObject.transform.position = newpos;
                mainCamera.transform.position = newpos;
                mainCamera.transform.Translate(Vector3.back * manim.cfg_dist_cam2view);
                
                
                //newpos.z += manim.cfg_dist_cam2view;
                //targetObject.transform.position = newpos;
                mainCamera.transform.DOLookAt(targetObject.transform.position, 0.1f);
                //SetTargetAndCameraDistance(manim.cfg_dist_cam2view);
                
                break;
            }
        }
    }
    public void ConvertLastPosOnMouse2Pad()
    {
        lastDragPos = Vector3.zero;
        lastMousePos = Vector3.zero;
    }
    public void GetRotationCameraFromOuter()
    {
        string ret = transform.rotation.eulerAngles.x.ToString() + "," + transform.rotation.eulerAngles.y.ToString() + "," + transform.rotation.eulerAngles.z.ToString();
#if !UNITY_EDITOR && UNITY_WEBGL
        ReceiveStringVal(ret);
#endif
    }
    public void GetTranslationCameraFromOuter()
    {
        string ret = transform.position.x.ToString() + "," + transform.position.y.ToString() + "," + transform.position.z.ToString();
#if !UNITY_EDITOR && UNITY_WEBGL
        ReceiveStringVal(ret);
#endif
    }
    public void GetLastMousePositionFromOuter()
    {
        string ret = lastMousePos.x.ToString() + "," + lastMousePos.y.ToString() + "," + lastMousePos.z.ToString();
#if !UNITY_EDITOR && UNITY_WEBGL
        ReceiveStringVal(ret);
#endif
    }

    /// <summary>
    /// Rotate by user keyboard input
    /// </summary>
    /// <param name="param"></param>
    public void RotateCameraPosFromOuter(string param)
    {
        string[] prm = param.Split(',');
        //-1 ~ 1
        float x = float.TryParse(prm[0], out x) ? x : 0f;
        float y = float.TryParse(prm[1], out y) ? y : 0f;
        float z = 0f;
        Vector3 pos = new Vector3(x, y, z);

        /*
        var delta = lastDragPos - pos;
        mainCamera.transform.Translate(delta * Time.deltaTime * 0.25f);
        lastDragPos = pos;
        */
        //Vector3 vec = execCameraRotater(pos);

        /*if (pos.y != 0)
        { //Y is change
            mainCamera.transform.Rotate(pos * manim.cfg_keymove_speed_rot);
            targetObject.transform.Rotate(pos * manim.cfg_keymove_speed_rot);
        }
        else
        { //X, Z is change
            mainCamera.transform.Rotate(pos * manim.cfg_keymove_speed_rot, Space.World);
            targetObject.transform.Rotate(pos * manim.cfg_keymove_speed_rot, Space.World);
        }*/
        targetObject.transform.rotation = mainCamera.transform.rotation;

        mainCamera.transform.RotateAround(targetObject.transform.position, Vector3.up, pos.x);
        mainCamera.transform.RotateAround(targetObject.transform.position, mainCamera.transform.right, -pos.y);

        //targetObject.transform.position = mainCamera.transform.position;
        //targetObject.transform.Translate(Vector3.forward * manim.cfg_dist_cam2view);


    }

    /// <summary>
    /// move by user keyboard input
    /// </summary>
    /// <param name="param"></param>
    public void TranslateCameraPosFromOuter(string param)
    {
        string[] prm = param.Split(',');
        //-1 ~ 1
        float x = float.TryParse(prm[0], out x) ? x : 0f;
        float y = float.TryParse(prm[1], out y) ? y : 0f;
        float z = float.TryParse(prm[2], out z) ? z : 0f;
        Vector3 pos = new Vector3(x, y, z);

        //Vector3 vec = execCameraMover(pos);


        mainCamera.transform.Translate(pos * manim.cfg_keymove_speed_trans);
        targetObject.transform.Translate(pos * manim.cfg_keymove_speed_trans);


    }

    /// <summary>
    /// progress/back by user keyboard input
    /// </summary>
    /// <param name="zpos"></param>
    public void ProgressCameraPosFromOuter(float zpos)
    {
        Vector3 pos = new Vector3(0, 0, zpos);

        //Vector3 vec = execCameraMover(pos);

        mainCamera.transform.Translate(pos * manim.cfg_keymove_speed_trans);
        targetObject.transform.Translate(pos * manim.cfg_keymove_speed_trans);


    }
    public void ShowTargetObject(string param)
    {
        MeshRenderer mr = targetObject.GetComponent<MeshRenderer>();
        Color col = mr.sharedMaterial.GetColor("_Color");

        if (param == "1")
        {
            col.a = 0.2f;
        }
        else
        {
            col.a = 0f;
        }
        mr.sharedMaterial.SetColor("_Color", col);
    }
    public void ChangeOperateTarget(KeyOperationMode kmode)
    {
        if (kmode == KeyOperationMode.MoveAvatar)
        {
            manim.keyOperationMode = KeyOperationMode.MoveAvatar;
            KeyOperationModeView.text = "O";
            KeyObjGlobalLocal.gameObject.SetActive(true);
        }
        else if (kmode == KeyOperationMode.MoveCamera)
        {
            manim.keyOperationMode = KeyOperationMode.MoveCamera;
            KeyOperationModeView.text = "C";
            KeyObjGlobalLocal.gameObject.SetActive(false);
        }
    }
    public KeyOperationMode GetCurrentOperationTarget()
    {
        return manim.keyOperationMode;
    }

    //*********************************************************************************************************
    //  Each settings method
    public void EnableHandleShowCamera(int param)
    {
        Camera cam = FrontIKCam.GetComponent<Camera>();
        cam.enabled = param == 1 ? true : false;
        GameObject canvas = GameObject.Find("Canvas");
        if (param == 1)
        {
            canvas.transform.Find("GizmoRenderer").gameObject.SetActive(true);
            canvas.transform.Find("ObjectInfoView").gameObject.SetActive(true);
            //RectTransform rt = GameObject.Find("GizmoRenderer").GetComponent<RectTransform>();
            //rt.anchoredPosition = new Vector2(40, rt.anchoredPosition.y);
        }
        else
        {
            canvas.transform.Find("GizmoRenderer").gameObject.SetActive(false);
            canvas.transform.Find("ObjectInfoView").gameObject.SetActive(false);
            //RectTransform rt = GameObject.Find("GizmoRenderer").GetComponent<RectTransform>();
            //rt.anchoredPosition = new Vector2(-40, rt.anchoredPosition.y);
        }
    }
    public bool GetEnableHandleShowCamera()
    {
        Camera cam = FrontIKCam.GetComponent<Camera>();
#if !UNITY_EDITOR && UNITY_WEBGL
        ReceiveIntVal(cam.enabled ? 1 : 0);
#endif
        return cam.enabled;
    }
    public void SetInternalResolutionFromOuter(string param)
    {
        string[] prm = param.Split(',');
        int x = int.TryParse(prm[0], out x) ? x : 960;
        int y = int.TryParse(prm[1], out y) ? y : 600;
        bool isfull = prm[2] == "1" ? true : false;

        Screen.SetResolution(x, y, isfull);
        
    }
    public void GetInternalResolutionFromOuter()
    {
        string ret = Screen.currentResolution.width.ToString() + "," + Screen.currentResolution.height.ToString();
#if !UNITY_EDITOR && UNITY_WEBGL
        ReceiveStringVal(ret);
#endif
    }
    public void SetFlagWebGLInputFromOuter(string param)
    {
        bool flag = param == "1" ? true : false;

#if !UNITY_EDITOR && UNITY_WEBGL
        WebGLInput.captureAllKeyboardInput = flag;
#endif
    }
    public void SetZoomSpeed(float speed)
    {
        zoomSpeed = speed;
    }
    public float GetZoomSpeed()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
        ReceiveFloatVal(zoomSpeed);
#endif
        return zoomSpeed;
    }
    //------------------------------------------------------------------------
    //---------------------------------------------------------------------------------------
    // Sky mode settings
    //---------------------------------------------------------------------------------------
    public void GetDefaultSky()
    {
        bkup_skyColor = GetSkyColor();
        bkup_clearFlags = GetClearFlag();
        bkup_skyShader = GetSkyShader();
        bkup_skyMaterialFloat = ListSkyMaterialFloat();
        bkup_skyMaterialColor = ListSkyMaterialColor();
    }
    public void SetDefaultSky()
    {
        SetSkyColor(bkup_skyColor);
        SetClearFlag(bkup_clearFlags);
        SetSkyShader(bkup_skyShader);
        bkup_skyMaterialFloat.ForEach(item =>
        {
            SetSkyMaterial(item.text + "," + item.value.ToString());
        });
        bkup_skyMaterialColor.ForEach(item =>
        {
            SetSkyMaterial(item.text + ",#" + ColorUtility.ToHtmlStringRGBA(item.value));
        });
    }
    public void GetIndicatedPropertyFromOuter()
    {
        string ret = "{";
        ret += "\"skymodeselected\": " + ((int)GetClearFlag()).ToString() + ", ";
        ret += "\"skyshaderselected\": \"" + GetSkyShader() + "\", ";
        ret += "\"skycolorselected\": \"#" + ColorUtility.ToHtmlStringRGBA(Front3DCam.backgroundColor) + "\", ";

        List<BasicStringFloatList> flst =  ListSkyMaterialFloat();
        flst.ForEach(item =>
        {
            string label = "";
            if (item.text == "_SunSize")
            {
                label = "sunsize";
            }
            else if (item.text == "_SunSizeConvergence")
            {
                label = "sunsize_convergence";
            }
            else if (item.text == "_AtmosphereThickness")
            {
                label = "atmosphere_thickness";
            }
            else if (item.text == "_Exposure")
            {
                label = "exposure";
            }
            ret += "\"" + label + "\": " + item.value.ToString() + ", ";
        });
        List<BasicStringColorList> clst = ListSkyMaterialColor();
        clst.ForEach(item =>
        {
            string label = "";
            if (item.text == "_SkyTint")
            {
                label = "tint";
            }
            else if (item.text == "_GroundColor")
            {
                label = "ground_color";
            }
            ret += "\"" + label + "\": \"#" + ColorUtility.ToHtmlStringRGBA(item.value) + "\",";
        });

        ret += "\"EOF\": \"null\"}";
#if !UNITY_EDITOR && UNITY_WEBGL
        ReceiveStringVal(ret);
#endif
    }
    public void SetClearFlag(CameraClearFlags param)
    {
        if (Front3DCam.enabled) Front3DCam.clearFlags = param;
        
        {
            VRCameraL.clearFlags = param;
            VRCameraR.clearFlags = param;
        }

        if (camxr.isActiveNormal())
        {
            List<NativeAnimationAvatar> list = manim.GetCastsByRoleType(AF_TARGETTYPE.Camera);
            list.ForEach(item =>
            {
                item.avatar.GetComponent<OperateLoadedCamera>().SetClearFlag((int)param);
            });
        }
        
    }
    public void SetClearFlagFromOuter(int param)
    {
        SetClearFlag((CameraClearFlags)param);
        
    }
    public CameraClearFlags GetClearFlag()
    {
        return Front3DCam.clearFlags;
    }
    public int GetClearFlagFromOuter()
    {
        int ret = 0;
        ret = (int)Front3DCam.clearFlags;
#if !UNITY_EDITOR && UNITY_WEBGL
        ReceiveIntVal(ret);
#endif
        return ret;
    }

    public void SetSkyColor(Color param)
    {
        if (Front3DCam.enabled) Front3DCam.backgroundColor = param;
        //if (camxr.isActiveVR())
        {
            VRCameraL.backgroundColor = param;
            VRCameraR.backgroundColor = param;
        }
        //Front3DCam.transform.GetChild(0).gameObject.GetComponent<Camera>().backgroundColor = param;

        if (camxr.isActiveNormal())
        {
            List<NativeAnimationAvatar> list = manim.GetCastsByRoleType(AF_TARGETTYPE.Camera);
            list.ForEach(item =>
            {
                item.avatar.GetComponent<Camera>().backgroundColor = param;
            });
        }
        

    }
    public void SetSkyColorFromOuter(string param)
    {
        Color col;
        if (ColorUtility.TryParseHtmlString(param, out col))
        {
            SetSkyColor(col);
        }
    }
    public Sequence SetSkyColorTween(Sequence seq, Color param, float duration)
    {
        if (Front3DCam.enabled) seq.Join(Front3DCam.DOColor(param, duration));
        //if (camxr.isActiveVR())
        {
            seq.Join(VRCameraL.DOColor(param, duration));
            seq.Join(VRCameraR.DOColor(param, duration));
        }

        if (camxr.isActiveNormal())
        {
            List<NativeAnimationAvatar> list = manim.GetCastsByRoleType(AF_TARGETTYPE.Camera);
            for (int i = 0; i < list.Count; i++)
            {
                list[i].avatar.GetComponent<Camera>().backgroundColor = param;
            }
        }
        

        return seq;
    }
    public Color GetSkyColor()
    {
        return Front3DCam.backgroundColor;
    }
    public string GetSkyColorFromOuter()
    {
        string ret = "";
        ret = ColorUtility.ToHtmlStringRGBA(Front3DCam.backgroundColor);
#if !UNITY_EDITOR && UNITY_WEBGL
        ReceiveStringVal(ret);
#endif
        return ret;
    }

    public Material skyboxMaterial
    {
        get
        {
            return RenderSettings.skybox;
        }
    }
    public string GetSkyShader()
    {
        if (RenderSettings.skybox.shader.name == "Skybox/Procedural")
        {
            return "procedural";
        }
        else
        {
            if (RenderSettings.skybox.name.IndexOf("Blue") > -1)
            {
                return "bluesky";
            }
            else if (RenderSettings.skybox.name.IndexOf("Purple") > -1)
            {
                return "purplesky";
            }
        }
        return "";
    }
    public void SetSkyShader(string param)
    {
        //---search public shader name
        for (int i = 0; i < skyMaterials.Length; i++)
        {
            if (skyMaterials[i].name == param)
            {
                RenderSettings.skybox = skyMaterials[i];
                return;
            }
        }

        //---search shader from spp shader name
        if (param == "procedural")
        {
            RenderSettings.skybox = skyMaterials[0];
        }
        else if (param == "bluesky")
        {
            RenderSettings.skybox = skyMaterials[1];
        }
        else if (param == "purplesky")
        {
            RenderSettings.skybox = skyMaterials[2];
        }
    }
    public List<BasicStringFloatList> ListSkyMaterialFloat()
    {
        List<BasicStringFloatList> ret = new List<BasicStringFloatList>();

        if (RenderSettings.skybox.shader.name == "Skybox/Procedural")
        {
            ret.Add(new BasicStringFloatList("_SunSize", RenderSettings.skybox.GetFloat("_SunSize")));
            ret.Add(new BasicStringFloatList("_SunSizeConvergence", RenderSettings.skybox.GetFloat("_SunSizeConvergence")));
            ret.Add(new BasicStringFloatList("_AtmosphereThickness", RenderSettings.skybox.GetFloat("_AtmosphereThickness")));
            ret.Add(new BasicStringFloatList("_Exposure", RenderSettings.skybox.GetFloat("_Exposure")));
        }
        else if (RenderSettings.skybox.shader.name == "Skybox/6 Sided")
        {
            ret.Add(new BasicStringFloatList("_Exposure", RenderSettings.skybox.GetFloat("_Exposure")));
            ret.Add(new BasicStringFloatList("_Rotation", RenderSettings.skybox.GetFloat("_Rotation")));

        }
        return ret;
    }
    public List<BasicStringColorList> ListSkyMaterialColor()
    {
        List<BasicStringColorList> ret = new List<BasicStringColorList>();

        if (RenderSettings.skybox.shader.name == "Skybox/Procedural")
        {
            ret.Add(new BasicStringColorList("_SkyTint", RenderSettings.skybox.GetColor("_SkyTint")));
            ret.Add(new BasicStringColorList("_GroundColor", RenderSettings.skybox.GetColor("_GroundColor")));
        }
        else if (RenderSettings.skybox.shader.name == "Skybox/6 Sided")
        {
            ret.Add(new BasicStringColorList("_Tint", RenderSettings.skybox.GetColor("_Tint")));

        }

        return ret;
    }

    public void SetSkyMaterial(string param)
    {
        string[] arr = param.Split(',');
        string prm = arr[0];
        float val = float.TryParse(arr[1], out val) ? val : 0;
        if (RenderSettings.skybox.shader.name == "Skybox/Procedural")
        {
            if (prm == "sunsize")
            {
                RenderSettings.skybox.SetFloat("_SunSize", val);
            }
            else if (prm == "sunconvergence")
            {
                RenderSettings.skybox.SetFloat("_SunSizeConvergence", val);
            }
            else if (prm == "atmosphere")
            {
                RenderSettings.skybox.SetFloat("_AtmosphereThickness", val);
            }
            else if (prm == "exposure")
            {
                RenderSettings.skybox.SetFloat("_Exposure", val);
            }
            else if (prm == "skytint")
            {
                Color col = ColorUtility.TryParseHtmlString(arr[1], out col) ? col : new Color(0.5f, 0.5f, 0.5f, 1f);
                RenderSettings.skybox.SetColor("_SkyTint", col);
            }
            else if (prm == "groundcolor")
            {
                Color col = ColorUtility.TryParseHtmlString(arr[1], out col) ? col : new Color(0.369f, 0.349f, 0.341f, 1f);
                RenderSettings.skybox.SetColor("_GroundColor", col);
            }
        }
        else if (RenderSettings.skybox.shader.name == "Skybox/6 Sided")
        {
            if (prm == "exposure")
            {
                RenderSettings.skybox.SetFloat("_Exposure", val);
            }
            else if (prm == "rotation")
            {
                RenderSettings.skybox.SetFloat("_Rotation", val);
            }
            else if (prm == "skytint")
            {
                Color col = ColorUtility.TryParseHtmlString(arr[1], out col) ? col : new Color(0.5f, 0.5f, 0.5f, 1f);
                RenderSettings.skybox.SetColor("_Tint", col);
            }
        }

    }
    public void SetGizmoTranslation()
    {
        TransformGizmo gizmo = transform.gameObject.GetComponent<TransformGizmo>();

        gizmo.transformType = TransformType.Move;
    }
    public void SetGizmoRotation()
    {
        TransformGizmo gizmo = transform.gameObject.GetComponent<TransformGizmo>();

        gizmo.transformType = TransformType.Rotate;
    }
    public void SetGizmoAllType()
    {
        TransformGizmo gizmo = transform.gameObject.GetComponent<TransformGizmo>();

        gizmo.transformType = TransformType.All;
    }
    public void SetGizmoHandle(string param)
    {
        string[] arr = param.Split(',');
        float allMoveHandle = float.TryParse(arr[0], out allMoveHandle) ? allMoveHandle : 0.6f;
        float allRotateHandle = float.TryParse(arr[1], out allRotateHandle) ? allRotateHandle : 0.6f;
        float handleWidth = float.TryParse(arr[2], out handleWidth) ? handleWidth : 0.0015f;
        float planeSize = float.TryParse(arr[3], out planeSize) ? planeSize : 0.02f;
        float triangleSize = float.TryParse(arr[4], out triangleSize) ? triangleSize : 0.0015f;

        TransformGizmo gizmo = transform.gameObject.GetComponent<TransformGizmo>();

        gizmo.allMoveHandleLengthMultiplier = allMoveHandle;
        gizmo.allRotateHandleLengthMultiplier = allRotateHandle;
        gizmo.handleWidth = handleWidth;
        gizmo.planeSize = planeSize;
        gizmo.triangleSize = triangleSize;
    }
    public void GetGizmoHandle()
    {
        string ret = "";
        TransformGizmo gizmo = transform.gameObject.GetComponent<TransformGizmo>();

        ret = gizmo.allMoveHandleLengthMultiplier.ToString() + "," + gizmo.allRotateHandleLengthMultiplier.ToString() + "," + gizmo.handleWidth.ToString() + "," + gizmo.planeSize.ToString() + "," + gizmo.triangleSize.ToString();

#if !UNITY_EDITOR && UNITY_WEBGL
        ReceiveStringVal(ret);
#endif
    }

    //=====================================================================================================================
    /// <summary>
    /// Change camera from Main to User (or User to Main)
    /// </summary>
    /// <param name="param">main, camera object ID</param>
    public void ChangeMainCamera(string param)
    {
        GameObject[] cameras = GameObject.FindGameObjectsWithTag("CameraPlayer");
        if (param == "main")
        {
            Camera.main.enabled = true;
            Front2DCam.enabled = true;
            Front3DCam.enabled = true;
            return;
        }
        if (cameras.Length > 0)
        {
            bool ischanged = false;
            for (int i = 0; i < cameras.Length; i++)
            {
                if (cameras[i].name == param)
                {
                    cameras[i].GetComponent<Camera>().enabled = true;
                    ischanged = true;
                }
                else
                {
                    cameras[i].GetComponent<Camera>().enabled = false;
                }
            }
            if (ischanged)
            {
                Camera.main.enabled = false;
                Front2DCam.enabled = false;
                Front3DCam.enabled = false;
            }
        }
        else
        {
            Camera.main.enabled = true;
            Front2DCam.enabled = true;
            Front3DCam.enabled = true;
        }
    }
}
