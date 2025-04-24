using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using WebXR;

namespace UserHandleSpace
{
    public class CameraManagerXR : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);


        [SerializeField]
        private WebXRManager xrman;
        [SerializeField]
        //WebXRCamera cameras;
        VVMWebXRCamera cameras;
        [SerializeField]
        WebXRController conleft;
        [SerializeField]
        WebXRController conright;

        public float MoveRate = 0.01f;
        public float RotateRate = 0.1f;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //if (xrman.XRState != WebXRState.NORMAL)
            {
                CamMoving();
                CamRotating();
            }

        }
        void CamMoving()
        {
            float realmoverate = MoveRate;
            Vector2 v2 = conleft.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick);
            if (conright.GetButton(WebXRController.ButtonTypes.ButtonA))
            {
                //transform.Translate(v2.x * moverate, v2.y * moverate, 0, Space.Self);
                realmoverate = MoveRate * 2.0f;
            }

            //Debug.Log($"CamMoving {v2.x} , {v2.y}");
            {
                transform.Translate(v2.x * realmoverate, 0, v2.y * realmoverate, Space.Self);
            }

        }
        void CamRotating()
        {
            float realmoverate = MoveRate;
            float realrotaterate = RotateRate; 

            Vector2 v2 = conright.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick);
            if (conright.GetButton(WebXRController.ButtonTypes.ButtonA))
            {
                realmoverate = MoveRate * 2.0f;
                realrotaterate = RotateRate * 2.0f;
            }
            //Debug.Log($"CamRotating {v2.x} , {v2.y}");
            transform.Rotate(0, v2.x * realrotaterate, 0);
            transform.Translate(0, v2.y * realmoverate, 0, Space.Self);
        }
        
        public bool isActiveNormal()
        {
            return xrman.XRState == WebXRState.NORMAL;
        }
        public bool isActiveVR()
        {
            return xrman.XRState == WebXRState.VR;
        }
        public bool isActiveAR()
        {
            return xrman.XRState == WebXRState.AR;
        }
        public void isActiveVRFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(xrman.XRState == WebXRState.VR ? 1 : 0);
#endif
        }
        public void isActiveARFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(xrman.XRState == WebXRState.AR ? 1 : 0);
#endif
        }
        public void ToggleVR(bool flag = true)
        {
            xrman.ToggleVR();
        }
        public void ToggleAR(bool flag = true)
        {
            xrman.ToggleAR();
        }
        public bool isSupportVR()
        {
            return xrman.isSupportedVR;
        }
        public bool isSupportAR()
        {
            return xrman.isSupportedAR;
        }
        public void isSupportVRFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(xrman.isSupportedVR ? 1 : 0);
#endif
        }
        public void isSupportARFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(xrman.isSupportedAR ? 1 : 0);
#endif
        }
        public void ChangeIKMarkerStateWhenVRAR(bool flag)
        {
            if (flag)
            {
                if (isActiveVR()) LayerCullingShow(cameras.GetCamera(WebXRCamera.CameraID.LeftVR), "Handle");
                if (isActiveVR()) LayerCullingShow(cameras.GetCamera(WebXRCamera.CameraID.RightVR), "Handle");
                if (isActiveAR()) LayerCullingShow(cameras.GetCamera(WebXRCamera.CameraID.LeftAR), "Handle");
                if (isActiveAR()) LayerCullingShow(cameras.GetCamera(WebXRCamera.CameraID.RightAR), "Handle");
            }
            else
            {
                if (isActiveVR()) LayerCullingHide(cameras.GetCamera(WebXRCamera.CameraID.LeftVR), "Handle");
                if (isActiveVR()) LayerCullingHide(cameras.GetCamera(WebXRCamera.CameraID.RightVR), "Handle");
                if (isActiveAR()) LayerCullingHide(cameras.GetCamera(WebXRCamera.CameraID.LeftAR), "Handle");
                if (isActiveAR()) LayerCullingHide(cameras.GetCamera(WebXRCamera.CameraID.RightAR), "Handle");

            }
        }
        public bool ButtonA_ControllerLeft(bool isdown, bool isup, bool isduring)
        {
            if (isdown)
            {
                return conleft.GetButtonDown(WebXRController.ButtonTypes.ButtonA);
            }
            else if (isup)
            {
                return conleft.GetButtonUp(WebXRController.ButtonTypes.ButtonA);
            }
            else if (isduring)
            {
                return conleft.GetButton(WebXRController.ButtonTypes.ButtonA);
            }
            else
            {
                return false;
            }
        }
        public bool ButtonB_ControllerLeft(bool isdown, bool isup, bool isduring)
        {
            if (isdown)
            {
                return conleft.GetButtonDown(WebXRController.ButtonTypes.ButtonB);
            }
            else if (isup)
            {
                return conleft.GetButtonUp(WebXRController.ButtonTypes.ButtonB);
            }
            else if (isduring)
            {
                return conleft.GetButton(WebXRController.ButtonTypes.ButtonB);
            }
            else
            {
                return false;
            }
        }
        public bool ButtonA_ControllerRight(bool isdown, bool isup, bool isduring)
        {
            if (isdown)
            {
                return conright.GetButtonDown(WebXRController.ButtonTypes.ButtonA);
            }
            else if (isup)
            {
                return conright.GetButtonUp(WebXRController.ButtonTypes.ButtonA);
            }
            else if (isduring)
            {
                return conright.GetButton(WebXRController.ButtonTypes.ButtonA);
            }
            else
            {
                return false;
            }
        }
        public bool ButtonB_ControllerRight(bool isdown, bool isup, bool isduring)
        {
            if (isdown)
            {
                return conright.GetButtonDown(WebXRController.ButtonTypes.ButtonB);
            }
            else if (isup)
            {
                return conright.GetButtonUp(WebXRController.ButtonTypes.ButtonB);
            }
            else if (isduring)
            {
                return conright.GetButton(WebXRController.ButtonTypes.ButtonB);
            }
            else
            {
                return false;
            }
        }
        public void ControllerLeftButtonAction(System.Action actionA, System.Action actionB = null)
        {
            if (conleft.GetButtonUp(WebXRController.ButtonTypes.ButtonA))
            {
                actionA();
            }
            if (actionB != null)
            {
                if (conleft.GetButtonUp(WebXRController.ButtonTypes.ButtonB))
                {
                    actionB();
                }
            }
            
        }
        public void  ControllerRightButtonAction(System.Action actionA, System.Action actionB = null)
        {
            if (conleft.GetButtonUp(WebXRController.ButtonTypes.ButtonA))
            {
                actionA();
            }
            if (actionB != null)
            {
                if (conleft.GetButtonUp(WebXRController.ButtonTypes.ButtonB))
                {
                    actionB();
                }
            }
            
        }
    

        //==================================================================================
        //---Camera extension
        public static void LayerCullingShow(Camera cam, int layerMask)
        {
            cam.cullingMask |= layerMask;
        }

        public static void LayerCullingShow(Camera cam, string layer)
        {
            LayerCullingShow(cam, 1 << LayerMask.NameToLayer(layer));
        }

        public static void LayerCullingHide(Camera cam, int layerMask)
        {
            cam.cullingMask &= ~layerMask;
        }

        public static void LayerCullingHide(Camera cam, string layer)
        {
            LayerCullingHide(cam, 1 << LayerMask.NameToLayer(layer));
        }

        public static void LayerCullingToggle(Camera cam, int layerMask)
        {
            cam.cullingMask ^= layerMask;
        }

        public static void LayerCullingToggle(Camera cam, string layer)
        {
            LayerCullingToggle(cam, 1 << LayerMask.NameToLayer(layer));
        }

        public static bool LayerCullingIncludes(Camera cam, int layerMask)
        {
            return (cam.cullingMask & layerMask) > 0;
        }

        public static bool LayerCullingIncludes(Camera cam, string layer)
        {
            return LayerCullingIncludes(cam, 1 << LayerMask.NameToLayer(layer));
        }

        public static void LayerCullingToggle(Camera cam, int layerMask, bool isOn)
        {
            bool included = LayerCullingIncludes(cam, layerMask);
            if (isOn && !included)
            {
                LayerCullingShow(cam, layerMask);
            }
            else if (!isOn && included)
            {
                LayerCullingHide(cam, layerMask);
            }
        }

        public static void LayerCullingToggle(Camera cam, string layer, bool isOn)
        {
            LayerCullingToggle(cam, 1 << LayerMask.NameToLayer(layer), isOn);
        }

    }

}
