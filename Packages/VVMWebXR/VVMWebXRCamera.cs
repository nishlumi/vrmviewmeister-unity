using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebXR;

namespace UserHandleSpace
{
    public class VVMWebXRCamera : MonoBehaviour
    {
        private static readonly string mainCameraTag = "MainCamera";
        private static readonly string untaggedTag = "Untagged";

        [SerializeField]
        private Camera cameraMain = null, cameraL = null, cameraR = null, cameraARL = null, cameraARR = null;
        [SerializeField]
        private Transform cameraFollower = null;

        [SerializeField]
        private Camera eventsCamera = null;

        private WebXRState xrState = WebXRState.NORMAL;
        private Rect leftRect, rightRect;

        private int viewsCount = 1;

        private bool hasFollower = false;
        private bool hasEventsCamera = false;

        [SerializeField]
        private bool updateCameraTag = false;

        private void OnEnable()
        {
            WebXRManager.OnXRChange += OnXRChange;
            WebXRManager.OnHeadsetUpdate += OnHeadsetUpdate;
            hasFollower = cameraFollower != null;
            hasEventsCamera = eventsCamera != null;
            OnXRChange(WebXRManager.Instance.XRState,
                        WebXRManager.Instance.ViewsCount,
                        WebXRManager.Instance.ViewsLeftRect,
                        WebXRManager.Instance.ViewsRightRect);
        }

        private void OnDisable()
        {
            WebXRManager.OnXRChange -= OnXRChange;
            WebXRManager.OnHeadsetUpdate -= OnHeadsetUpdate;
        }

        private void Update()
        {
            UpdateFollower();
        }

        private void SwitchXRState()
        {
            switch (xrState)
            {
                case WebXRState.AR:
                    //cameraMain.enabled = false;
                    SearchCameraAndEnable(cameraMain, false);
                    //cameraL.enabled = false;
                    SearchCameraAndEnable(cameraL, false);
                    //cameraR.enabled = false;
                    SearchCameraAndEnable(cameraR, false);
                    //cameraARL.enabled = viewsCount > 0;
                    SearchCameraAndEnable(cameraARL, viewsCount > 0);
                    cameraARL.rect = leftRect;
                    //cameraARR.enabled = viewsCount > 1;
                    SearchCameraAndEnable(cameraARR, viewsCount > 1);
                    cameraARR.rect = rightRect;
                    if (updateCameraTag)
                    {
                        cameraMain.tag = untaggedTag;
                        cameraL.tag = untaggedTag;
                        cameraARL.tag = mainCameraTag;
                    }
                    break;
                case WebXRState.VR:
                    //cameraMain.enabled = false;
                    SearchCameraAndEnable(cameraMain, false);
                    //cameraL.enabled = viewsCount > 0;
                    SearchCameraAndEnable(cameraL, viewsCount > 0);
                    cameraL.rect = leftRect;
                    //cameraR.enabled = viewsCount > 1;
                    SearchCameraAndEnable(cameraR, viewsCount > 1);
                    cameraR.rect = rightRect;
                    //cameraARL.enabled = false;
                    SearchCameraAndEnable(cameraARL, false);
                    //cameraARR.enabled = false;
                    SearchCameraAndEnable(cameraARR, false);
                    if (updateCameraTag)
                    {
                        cameraMain.tag = untaggedTag;
                        cameraL.tag = mainCameraTag;
                        cameraARL.tag = untaggedTag;
                    }
                    break;
                case WebXRState.NORMAL:
                    //cameraMain.enabled = true;
                    SearchCameraAndEnable(cameraMain, true);
                    //cameraL.enabled = false;
                    SearchCameraAndEnable(cameraL, false);
                    //cameraR.enabled = false;
                    SearchCameraAndEnable(cameraR, false);
                    //cameraARL.enabled = false;
                    SearchCameraAndEnable(cameraARL, false);
                    //cameraARR.enabled = false;
                    SearchCameraAndEnable(cameraARR, false);
                    if (updateCameraTag)
                    {
                        cameraMain.tag = mainCameraTag;
                        cameraL.tag = untaggedTag;
                        cameraARL.tag = untaggedTag;
                    }
                    break;
            }
            if (hasEventsCamera)
            {
                eventsCamera.projectionMatrix = cameraMain.projectionMatrix;
            }
        }
        protected void SearchCameraAndEnable(Camera cam, bool flag)
        {
            cam.enabled = flag;
            int childCount = cam.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
               Transform child = cam.transform.GetChild(i);
                child.GetComponent<Camera>().enabled = flag;
            }
        }

        private void UpdateFollower()
        {
            if (!hasFollower)
            {
                return;
            }
            switch (xrState)
            {
                case WebXRState.AR:
#if HAS_POSITION_AND_ROTATION
          cameraFollower.SetLocalPositionAndRotation(viewsCount > 1 ? (cameraARL.transform.localPosition + cameraARR.transform.localPosition) * 0.5f : cameraARL.transform.localPosition,
            cameraARL.transform.localRotation);
#else
                    cameraFollower.localPosition = viewsCount > 1 ? (cameraARL.transform.localPosition + cameraARR.transform.localPosition) * 0.5f : cameraARL.transform.localPosition;
                    cameraFollower.localRotation = cameraARL.transform.localRotation;
#endif
                    return;
                case WebXRState.VR:
#if HAS_POSITION_AND_ROTATION
          cameraFollower.SetLocalPositionAndRotation((cameraL.transform.localPosition + cameraR.transform.localPosition) * 0.5f, 
            cameraL.transform.localRotation);
#else
                    cameraFollower.localPosition = (cameraL.transform.localPosition + cameraR.transform.localPosition) * 0.5f;
                    cameraFollower.localRotation = cameraL.transform.localRotation;
#endif
                    return;
            }
#if HAS_POSITION_AND_ROTATION
      cameraFollower.SetLocalPositionAndRotation(cameraMain.transform.localPosition, 
        cameraMain.transform.localRotation);
#else
            cameraFollower.localRotation = cameraMain.transform.localRotation;
            cameraFollower.localPosition = cameraMain.transform.localPosition;
#endif
        }

        public Quaternion GetLocalRotation()
        {
            switch (xrState)
            {
                case WebXRState.AR:
                    return cameraARL.transform.localRotation;
                case WebXRState.VR:
                    return cameraL.transform.localRotation;
            }
            return cameraMain.transform.localRotation;
        }

        public Vector3 GetLocalPosition()
        {
            switch (xrState)
            {
                case WebXRState.AR:
                    if (viewsCount > 1)
                    {
                        return (cameraARL.transform.localPosition + cameraARR.transform.localPosition) * 0.5f;
                    }
                    return cameraARL.transform.localPosition;
                case WebXRState.VR:
                    return (cameraL.transform.localPosition + cameraR.transform.localPosition) * 0.5f;
            }
            return cameraMain.transform.localPosition;
        }

        public Camera GetCamera(WebXRCamera.CameraID cameraID)
        {
            switch (cameraID)
            {
                case WebXRCamera.CameraID.LeftVR:
                    return cameraL;
                case WebXRCamera.CameraID.RightVR:
                    return cameraR;
                case WebXRCamera.CameraID.LeftAR:
                    return cameraARL;
                case WebXRCamera.CameraID.RightAR:
                    return cameraARR;
            }
            return cameraMain;
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            xrState = state;
            this.viewsCount = viewsCount;
            this.leftRect = leftRect;
            this.rightRect = rightRect;
            SwitchXRState();
        }

        private void OnHeadsetUpdate(
            Matrix4x4 leftProjectionMatrix,
            Matrix4x4 rightProjectionMatrix,
            Quaternion leftRotation,
            Quaternion rightRotation,
            Vector3 leftPosition,
            Vector3 rightPosition)
        {
            if (xrState == WebXRState.VR)
            {
#if HAS_POSITION_AND_ROTATION
        cameraL.transform.SetLocalPositionAndRotation(leftPosition, leftRotation);
#else
                cameraL.transform.localPosition = leftPosition;
                cameraL.transform.localRotation = leftRotation;
#endif
                cameraL.projectionMatrix = leftProjectionMatrix;

#if HAS_POSITION_AND_ROTATION
        cameraR.transform.SetLocalPositionAndRotation(rightPosition, rightRotation);
#else
                cameraR.transform.localPosition = rightPosition;
                cameraR.transform.localRotation = rightRotation;
#endif
                cameraR.projectionMatrix = rightProjectionMatrix;
            }
            else if (xrState == WebXRState.AR)
            {
#if HAS_POSITION_AND_ROTATION
        cameraARL.transform.SetLocalPositionAndRotation(leftPosition, leftRotation);
#else
                cameraARL.transform.localPosition = leftPosition;
                cameraARL.transform.localRotation = leftRotation;
#endif
                cameraARL.projectionMatrix = leftProjectionMatrix;
#if HAS_POSITION_AND_ROTATION
        cameraARR.transform.SetLocalPositionAndRotation(rightPosition, rightRotation);
#else
                cameraARR.transform.localPosition = rightPosition;
                cameraARR.transform.localRotation = rightRotation;
#endif
                cameraARR.projectionMatrix = rightProjectionMatrix;
                if (hasEventsCamera && viewsCount == 1)
                {
                    eventsCamera.projectionMatrix = leftProjectionMatrix;
                }
            }
        }
    }

}
