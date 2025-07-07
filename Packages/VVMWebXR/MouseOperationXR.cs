using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebXR;
using WebXR.Interactions;

namespace UserHandleSpace
{
    [RequireComponent(typeof(Rigidbody))]
    public class MouseOperationXR : MonoBehaviour
    {
        private Camera m_currentCamera;
        private Rigidbody m_rigidbody;
        private Vector3 m_screenPoint;
        private Vector3 m_offset;
        private Vector3 m_currentVelocity;
        private Vector3 m_previousPos;

        private WebXRCamera m_xrcam;
        private WebXRManager m_xrman;
        public CameraManagerXR m_xrmanManager;     

        private bool m_emergencyStop;
        public bool UseTouchMode = false; // ← フラグ追加：モバイル用処理を使うかどうか

        private enum ControlMode { None, Move, Rotate }
        private ControlMode m_controlMode = ControlMode.None;
        public enum RotateAxis { X, Y, Z }
        public RotateAxis rotateAxis = RotateAxis.Y;

        private Vector2 m_touchStartPos; // 1点目のタッチ位置（スクリーン座標）
        private float m_twoTouchAngle;   // 2点間の角度（ラジアン）

        private float m_startAngle; // 掴み始めた時の2点間角度
        private float m_currentAngle; // 毎フレームの2点間角度
        private Quaternion m_initialRotation; // 掴み始めたときのオブジェクトの回転

        private static MouseOperationXR s_currentGrabbed = null;

        /// <summary>
        /// last tapped MouseOeprationXR object
        /// </summary>
        private static MouseOperationXR s_curgrab = null;
        /// <summary>
        /// last tapped MouseOeprationXR object get property
        /// </summary>
        public static MouseOperationXR CurrentGrabbed => s_curgrab;


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
            m_xrmanManager = FindObjectOfType<CameraManagerXR>();
        }

        // Update is called once per frame
        void Update()
        {
            if ((m_xrmanManager.DeviceFlagStr == "m") && (m_xrmanManager.ChangedVRAR)) //&& m_xrmanManager.IsStartTapControl
            {
                if (Input.touchCount == 1)
                {
                    Touch touch = Input.GetTouch(0);
                    if (touch.phase == TouchPhase.Began)
                    {
                        TryGrabForMove(touch.position);
                    }
                }
                else if (Input.touchCount == 2)
                {
                    Touch touch0 = Input.GetTouch(0);
                    Touch touch1 = Input.GetTouch(1);

                    if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                    {
                        TryGrabForRotate(touch0.position, touch1.position);
                    }
                }
                /*else if (Input.touchCount == 3)
                {
                    if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began || Input.GetTouch(2).phase == TouchPhase.Began)
                    {
                        CycleRotateAxis();
                    }
                    
                }*/
                else if (Input.touchCount == 0 && s_currentGrabbed == this)
                {
                    ForceRelease();
                }
            }
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
                s_currentGrabbed = this;
                s_curgrab = this;
                m_controlMode = ControlMode.Move;

                ShowCurrentCycleRotateAxis();
            }
        }

        void OnMouseUp()
        {
            if (IsNormal()) return;
            if (m_emergencyStop) return;

            //m_rigidbody.velocity = m_currentVelocity;
            m_rigidbody.velocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
            m_currentCamera = null;
        }
        void OnTouchEnd()
        {
            m_rigidbody.velocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
        }

        void FixedUpdate()
        {
            if (IsNormal()) return;
            if (m_emergencyStop) return;

            if (m_xrmanManager.DeviceFlagStr == "m")
            {
                /*if (m_currentCamera != null && Input.touchCount == 2)
                {
                    Vector3 currentScreenPoint = new Vector3(m_touchStartPos.x, m_touchStartPos.y, m_screenPoint.z);
                    Touch t1 = Input.GetTouch(0);
                    currentScreenPoint.x = t1.position.x;
                    currentScreenPoint.y = t1.position.y;

                    m_rigidbody.velocity = Vector3.zero;
                    m_rigidbody.MovePosition(m_currentCamera.ScreenToWorldPoint(currentScreenPoint) + m_offset);
                    m_currentVelocity = (transform.position - m_previousPos) / Time.deltaTime;
                    m_previousPos = transform.position;

                }*/
                if (m_currentCamera != null)
                {
                    switch (m_controlMode)
                    {
                        case ControlMode.Move:
                            HandleMove();
                            break;
                        case ControlMode.Rotate:
                            HandleRotate();
                            break;
                    }
                }
                
            }
            else
            {
                // VR用の従来処理がここに入る（必要に応じて追記）
                if (m_currentCamera != null)
                {
                    Vector3 currentScreenPoint = GetMousePosWithScreenZ(m_screenPoint.z);
                    //Vector3 currentScreenPoint = GetTouchMidPosWithScreenZ(m_screenPoint.z);
                    //Vector3 currentScreenPoint = new Vector3(m_touchStartPos.x, m_touchStartPos.y, m_screenPoint.z);
                    m_rigidbody.velocity = Vector3.zero;
                    m_rigidbody.MovePosition(m_currentCamera.ScreenToWorldPoint(currentScreenPoint) + m_offset);
                    m_currentVelocity = (transform.position - m_previousPos) / Time.deltaTime;
                    m_previousPos = transform.position;
                }
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

        void TryGrabAtPrimaryTouch(Vector2 pos1, Vector2 pos2)
        {
            if (IsNormal()) return;
            if (m_emergencyStop) return;
            if (s_currentGrabbed != null) return; // 誰かが既に掴まれていたら無視

            m_currentCamera = FindCamera();
            if (m_currentCamera == null) return;

            m_touchStartPos = pos1; // 1点目の位置を基準とする
            Vector2 dir = pos2 - pos1;
            //m_twoTouchAngle = Mathf.Atan2(dir.y, dir.x); // ラジアンで角度を保存
            m_startAngle = Mathf.Atan2(dir.y, dir.x); // 初期角度を保存
            m_initialRotation = transform.rotation;  // オブジェクト初期回転保存

            Ray ray = m_currentCamera.ScreenPointToRay(pos1);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if ((hit.collider.gameObject == this.gameObject))
                {
                    m_screenPoint = m_currentCamera.WorldToScreenPoint(transform.position);
                    m_offset = transform.position - m_currentCamera.ScreenToWorldPoint(new Vector3(pos1.x, pos1.y, m_screenPoint.z));
                    s_currentGrabbed = this; // 自分を掴んだと記録
                }
                else
                {
                    m_currentCamera = null;
                }
            }
        }
        void TryGrabForMove(Vector2 pos)
        {
            if (IsNormal()) return;
            if (m_emergencyStop) return;
            if (s_currentGrabbed != null && s_currentGrabbed != this) return;

            m_currentCamera = FindCamera();
            if (m_currentCamera == null) return;


            int layers = 1 << LayerMask.NameToLayer("Player");
            layers = layers | (1 << LayerMask.NameToLayer("Handle"));

            Ray ray = m_currentCamera.ScreenPointToRay(pos);

            /*RaycastHit[] hits = Physics.RaycastAll(ray.origin, ray.direction, 10f, layers);
            foreach (var h in hits)
            {
                Debug.Log("???MouseOperationXR hit:" + h.collider.gameObject.name);
            }*/

            if (Physics.Raycast(ray, out RaycastHit hit, 10f, layers))
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    Debug.Log("***MouseOperationXR real hit:" + hit.collider.gameObject.name);
                    m_screenPoint = m_currentCamera.WorldToScreenPoint(transform.position);
                    m_offset = transform.position - m_currentCamera.ScreenToWorldPoint(new Vector3(pos.x, pos.y, m_screenPoint.z));
                    s_currentGrabbed = this;
                    s_curgrab = this;
                    m_controlMode = ControlMode.Move;

                    ShowCurrentCycleRotateAxis();
                }
                else
                {
                    m_currentCamera = null;
                }
            }
        }

        void TryGrabForRotate(Vector2 pos1, Vector2 pos2)
        {
            if (IsNormal()) return;
            if (m_emergencyStop) return;
            if (s_currentGrabbed != null && s_currentGrabbed != this) return;

            m_currentCamera = FindCamera();
            if (m_currentCamera == null) return;

            Vector2 midPoint = (pos1 + pos2) * 0.5f;
            Ray ray = m_currentCamera.ScreenPointToRay(midPoint);

            int layers = 1 << LayerMask.NameToLayer("Player");
            layers = layers | (1 << LayerMask.NameToLayer("Handle"));

            
            if (Physics.Raycast(ray, out RaycastHit hit, 10f, layers))
            {
                if (hit.collider.gameObject == this.gameObject)
                {
                    m_screenPoint = m_currentCamera.WorldToScreenPoint(transform.position);
                    m_offset = transform.position - m_currentCamera.ScreenToWorldPoint(new Vector3(midPoint.x, midPoint.y, m_screenPoint.z));
                    s_currentGrabbed = this;
                    s_curgrab = this;
                    m_controlMode = ControlMode.Rotate;

                    Vector2 dir = pos2 - pos1;
                    m_startAngle = Mathf.Atan2(dir.y, dir.x);
                    m_initialRotation = transform.rotation;

                    ShowCurrentCycleRotateAxis();
                }
                else
                {
                    m_currentCamera = null;
                }
            }
        }

        void HandleMove()
        {
            if (Input.touchCount != 1) return;

            Vector2 touchPos = Input.GetTouch(0).position;
            Vector3 currentScreenPoint = new Vector3(touchPos.x, touchPos.y, m_screenPoint.z);
            m_rigidbody.velocity = Vector3.zero;
            m_rigidbody.MovePosition(m_currentCamera.ScreenToWorldPoint(currentScreenPoint) + m_offset);
            m_currentVelocity = (transform.position - m_previousPos) / Time.deltaTime;
            m_previousPos = transform.position;
        }

        void HandleRotate()
        {
            if (Input.touchCount != 2) return;

            Vector2 nowPos0 = Input.GetTouch(0).position;
            Vector2 nowPos1 = Input.GetTouch(1).position;
            Vector2 nowDir = nowPos1 - nowPos0;
            m_currentAngle = Mathf.Atan2(nowDir.y, nowDir.x);

            float angleDelta = Mathf.Rad2Deg * (m_currentAngle - m_startAngle);
            Quaternion rotationDelta = Quaternion.identity;

            switch (rotateAxis)
            {
                case RotateAxis.X:
                    rotationDelta = Quaternion.Euler(angleDelta, 0f, 0f);
                    break;
                case RotateAxis.Y:
                    rotationDelta = Quaternion.Euler(0f, -angleDelta, 0f);
                    break;
                case RotateAxis.Z:
                    rotationDelta = Quaternion.Euler(0f, 0f, angleDelta);
                    break;
            }

            m_rigidbody.MoveRotation(m_initialRotation * rotationDelta);
        }

        void ForceRelease()
        {
            if (s_currentGrabbed == this)
            {
                s_currentGrabbed = null;
            }
            //m_rigidbody.velocity = m_currentVelocity;
            m_rigidbody.velocity = Vector3.zero;
            m_rigidbody.angularVelocity = Vector3.zero;
            m_currentCamera = null;
            m_controlMode = ControlMode.None;
        }
        void ShowCurrentCycleRotateAxis()
        {
            switch (rotateAxis)
            {
                case RotateAxis.X:
                    m_xrmanManager.receiver1.label = "X";
                    break;
                case RotateAxis.Y:
                    m_xrmanManager.receiver1.label = "Y";
                    break;
                case RotateAxis.Z:
                    m_xrmanManager.receiver1.label = "Z";
                    break;
            }
            m_xrmanManager.receiver1.partsname = this.gameObject.name;
            m_xrmanManager.receiver1.IsShow = true;
        }
        void CycleRotateAxis()
        { //---NOT USE
            if (IsNormal()) return;
            if (m_emergencyStop) return;
            if (s_currentGrabbed != null && s_currentGrabbed != this) return;

            m_currentCamera = FindCamera();
            if (m_currentCamera == null) return;

            Vector2 touchPos0 = Input.GetTouch(0).position;
            Vector2 touchPos1 = Input.GetTouch(1).position;
            Vector2 touchPos2 = Input.GetTouch(2).position;

            Vector2 midPoint = (touchPos0 + touchPos1 + touchPos2) / 3f;
            Ray ray = m_currentCamera.ScreenPointToRay(midPoint);

            if (Physics.Raycast(ray, out RaycastHit hit, 10f))
            {
                if (hit.collider.gameObject != this.gameObject)
                {
                    return;
                }
            }
            else
            {
                return;
            }

            switch (rotateAxis)
            {
                case RotateAxis.X:
                    rotateAxis = RotateAxis.Y;
                    m_xrmanManager.receiver1.label = "Y";
                    break;
                case RotateAxis.Y:
                    rotateAxis = RotateAxis.Z;
                    m_xrmanManager.receiver1.label = "Z";
                    break;
                case RotateAxis.Z:
                    rotateAxis = RotateAxis.X;
                    m_xrmanManager.receiver1.label = "X";
                    break;
            }

            Debug.Log("[MouseOperationXR] Rotate Axis Changed to: " + rotateAxis);
            //---calculate popup position and set contents.
            Vector3 pos = Input.GetTouch(0).position;
            Vector3 popupsize = new Vector3(Screen.width * 0.05f, Screen.height * 0.05f, 0f);
            Vector3 spos = new Vector3(pos.x, pos.y - popupsize.y, 0);
            if (pos.y < popupsize.y)
            { //---y pos near 0, move below popup .
                spos.y = (popupsize.y * 2f);
            }
            m_xrmanManager.receiver1.pos = spos;
            //--- Continue with window linking processing
            m_xrmanManager.receiver1.IsShow = true;

            // ★回転基準のリセット（3点重心を考慮した改訂版）
            if (Input.touchCount >= 2)
            {
                Vector2 pos0 = Input.GetTouch(0).position;
                Vector2 pos1 = Input.GetTouch(1).position;
                Vector2 pos2 = Input.GetTouch(2).position;

                // 3点の平均（重心）を計算
                Vector2 center = (pos0 + pos1 + pos2) / 3f;

                // 代表として最初の2点間のベクトルを使う
                Vector2 dir = pos1 - pos0;

                // 中心ベースで初期角度を再設定
                m_startAngle = Mathf.Atan2(dir.y, dir.x);
                m_initialRotation = transform.rotation;
            }

        }

    }
}

