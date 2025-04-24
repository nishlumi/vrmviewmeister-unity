using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebXR;
using WebXR.Interactions;


namespace UserHandleSpace
{
    [RequireComponent(typeof(Rigidbody))]
    public class MouseOperationXR2 : MonoBehaviour
    {
        private Camera m_currentCamera;
        private Rigidbody m_rigidbody;
        private Vector3 m_screenPoint;
        private Vector3 m_offset;
        private Vector3 m_currentVelocity;
        private Vector3 m_previousPos;

        private WebXRCamera m_xrcam;
        private WebXRManager m_xrman;

        private bool m_emergencyStop;

        // Start is called before the first frame update
        void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody>();
            m_xrcam = FindObjectOfType<WebXRCamera>();
            m_xrman = FindObjectOfType<WebXRManager>();
        }

        void Start()
        {
            m_emergencyStop = false;
        }

        // Update is called once per frame
        void Update()
        {

        }
        void OnMouseDown()
        {
            if (IsNormal()) return;
            if (m_emergencyStop) return;

            m_currentCamera = FindCamera();
            if (m_currentCamera != null)
            {
                m_screenPoint = m_currentCamera.WorldToScreenPoint(gameObject.transform.position);
                m_offset = gameObject.transform.position - m_currentCamera.ScreenToWorldPoint(GetMousePosWithScreenZ(m_screenPoint.z));
            }
        }

        void OnMouseUp()
        {
            if (IsNormal()) return;
            if (m_emergencyStop) return;

            m_rigidbody.velocity = m_currentVelocity;
            m_currentCamera = null;
        }

        void FixedUpdate()
        {
            if (IsNormal()) return;
            if (m_emergencyStop) return;

            if (m_currentCamera != null)
            {
                Vector3 currentScreenPoint = GetMousePosWithScreenZ(m_screenPoint.z);
                m_rigidbody.velocity = Vector3.zero;
                m_rigidbody.MovePosition(m_currentCamera.ScreenToWorldPoint(currentScreenPoint) + m_offset);
                transform.position = m_currentCamera.ScreenToWorldPoint(currentScreenPoint) + m_offset;
                //transform.Translate(m_offset, Space.World);
                m_currentVelocity = (transform.position - m_previousPos) / Time.deltaTime;
                m_previousPos = transform.position;
            }
        }

        Vector3 GetMousePosWithScreenZ(float screenZ)
        {
            return new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenZ);
        }

        Camera FindCamera()
        {
            //Camera[] cameras = FindObjectsOfType<Camera>();
            Camera result = null;
            int camerasSum = 0;
            /*foreach (var camera in cameras)
            {
                if (camera.enabled)
                {
                    result = camera;
                    camerasSum++;
                }
            }*/
            for (int i = 0; i < (int)WebXRCamera.CameraID.RightAR; i++)
            {
                WebXRCamera.CameraID cameraID = (WebXRCamera.CameraID)i;
                var camera = m_xrcam.GetCamera(cameraID);
                if (camera.enabled)
                {
                    result = camera;
                    camerasSum++;
                }
            }

            if (camerasSum > 1)
            {
                result = null;
            }
            return result;
        }
        public bool IsNormal()
        {
            if (m_xrman.XRState == WebXRState.VR)
            {
                return false;
            }
            else if (m_xrman.XRState == WebXRState.AR)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public bool EmergencyStop
        {
            get
            {
                return m_emergencyStop;
            }
            set
            {
                m_emergencyStop = value;
            }

        }
    }
}