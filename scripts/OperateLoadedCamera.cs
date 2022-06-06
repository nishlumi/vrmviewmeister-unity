using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using DG.Tweening;

namespace UserHandleSpace
{
    public class OperateLoadedCamera : OperateLoadedBase
    {

        [DllImport("__Internal")]
        private static extern void ReceiveObjectVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);

        public UserAnimationState cameraPlayFlag;  //1 - play, 0 - stop, 2 - playing, 3 - seeking, 4 - pause

        public int cameraRenderFlag;    //1 - renderTexture, 0 - camera self
        public Vector2 RenderSize;
        protected RenderTexture availableRenderTexture;

        ManageAnimation manim;


        private void Awake()
        {
            effectPunch = new AvatarPunchEffect();
            effectShake = new AvatarShakeEffect();


            cameraPlayFlag = UserAnimationState.Stop;

            targetType = AF_TARGETTYPE.Camera;
            RenderSize = new Vector2(200f, 200f);
            availableRenderTexture = null;
        }
        // Start is called before the first frame update
        void Start()
        {
            manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        /// <summary>
        /// To pack all properties for HTML-UI
        /// </summary>
        /// <param name="type">1 - normal light, 0 - directional light</param>
        public void GetIndicatedPropertyFromOuter(int type)
        {
            string ret = "";
            Camera lt = transform.gameObject.GetComponent<Camera>();

            int pflag = (int)GetCameraPlaying(0);
            string js = lt.rect.x.ToString() + "," + lt.rect.y.ToString() + "," + lt.rect.width.ToString() + "," + lt.rect.height.ToString();

            ret = pflag.ToString() + "\t" + lt.fieldOfView.ToString() + "\t" + lt.depth.ToString() + "\t" + js + "\t" + ((int)lt.clearFlags).ToString() + 
                "\t" + cameraRenderFlag.ToString() + "\t" + RenderSize.x.ToString() + "/" + RenderSize.y.ToString()
            ;

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        //----------------------------------------------------------------------------------------------------------
        public override void SetEnableWholeIK(int intflag)
        {
            bool flag = intflag == 1 ? true : false;

            relatedHandleParent.SetActive(flag);

        }

        public void PreviewCamera()
        {
            manim.currentProject.casts.ForEach(action =>
            {
                Camera c = action.avatar.GetComponent<Camera>();
                if (c != null)
                {
                    c.enabled = false;
                }
            });
            Camera cam = gameObject.GetComponent<Camera>();
            cam.enabled = true;
            ScreenShot ss = Camera.main.GetComponent<ScreenShot>();
            if (ss != null)
            {
                ss.SetCameraForScreenshot(gameObject.name);
            }
        }
        public void EndPreview()
        {
            Camera cam = gameObject.GetComponent<Camera>();
            cam.enabled = false;

            ScreenShot ss = Camera.main.GetComponent<ScreenShot>();
            if (ss != null)
            {
                ss.SetCameraForScreenshot("FrontMainCamera");
            }
        }
        public UserAnimationState GetCameraPlaying(int is_contacthtml = 1)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml == 1) {
            ReceiveIntVal((int)cameraPlayFlag);
        }
#endif
            return cameraPlayFlag;
        }
        public void SetCameraPlaying(int flag)
        {
            cameraPlayFlag = (UserAnimationState)flag;
        }
        public void SetClearFlag(int param)
        {
            Camera cam = gameObject.GetComponent<Camera>();
            cam.clearFlags = (CameraClearFlags)param;
        }
        public int GetClearFlag()
        {
            int ret = 0;
            Camera cam = gameObject.GetComponent<Camera>();
            ret = (int)cam.clearFlags;
#if !UNITY_EDITOR && UNITY_WEBGL
        ReceiveIntVal(ret);
#endif
            return ret;
        }

        public float GetFoV()
        {
            Camera lt = transform.gameObject.GetComponent<Camera>();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(lt.fieldOfView);
#endif
            return lt.fieldOfView;
        }
        public void SetFoV(float fov)
        {
            Camera lt = transform.gameObject.GetComponent<Camera>();

            lt.fieldOfView = fov;
        }

        public float GetDepth()
        {
            Camera lt = transform.gameObject.GetComponent<Camera>();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(lt.depth);
#endif
            return lt.depth;
        }
        public void SetDepth(float dep)
        {
            Camera lt = transform.gameObject.GetComponent<Camera>();

            lt.depth = dep;
        }

        public Rect GetViewport()
        {
            Camera lt = transform.gameObject.GetComponent<Camera>();

            string js = lt.rect.x + "," + lt.rect.y + "," + lt.rect.width + "," + lt.rect.height;
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
            return lt.rect;
        }
        public void SetViewport(Rect rect)
        {
            Camera lt = transform.gameObject.GetComponent<Camera>();

            lt.rect = rect;
        }
        public void SetViewportFromOuter(string param)
        {
            /*
             * param must to has: x, y, width: height
             */
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float width = float.TryParse(prm[2], out width) ? width : 1f;
            float height = float.TryParse(prm[3], out height) ? height : 1f;


            Camera lt = transform.gameObject.GetComponent<Camera>();
            Rect rec = new Rect(x, y, width, height);

            lt.rect = rec;
        }
        public void SetViewportPosition(string param)
        {
            Camera lt = transform.gameObject.GetComponent<Camera>();
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[0], out y) ? y : 0f;

            lt.DORect(new Rect(x, y, lt.rect.width, lt.rect.height), 0.1f);
        }
        //---------------------------------------------------------------------------------
        public int GetCameraRenderFlag()
        {
            int ret = cameraRenderFlag;

            return ret;
        }
        public void GetCameraRenderFlagFromOuter()
        {
            int ret = cameraRenderFlag;

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">1 - renderTexture, 0 - camera self</param>
        public void SetCameraRenderFlag(int param)
        {
            if (cameraRenderFlag == param) return;

            cameraRenderFlag = param;
            if (cameraRenderFlag == 1)
            {
                ApplyRenderTexture();
            }
            else
            {
                DestroyRenderTexture();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">csv: 0 - size X, 1 - size Y</param>
        public void SetRenderTexture(string param)
        {
            string[] prm = param.Split(',');
            int x = int.TryParse(prm[0], out x) ? x : 100;
            int y = int.TryParse(prm[1], out y) ? y : 100;

            RenderSize.x = (float)x;
            RenderSize.y = (float)y;

        }
        public void SetRenderTexture(Vector2 val)
        {
            RenderSize = val;
        }
        public void ApplyRenderTexture()
        {
            Camera lt = transform.gameObject.GetComponent<Camera>();
            RenderTexture rt = new RenderTexture((int)RenderSize.x, (int)RenderSize.y, 32);
            rt.dimension = UnityEngine.Rendering.TextureDimension.Tex2D;

            lt.targetTexture = rt;

            DestroyRenderTexture();
            availableRenderTexture = rt;
        }
        public void DestroyRenderTexture()
        {
            if (availableRenderTexture != null)
            {
                availableRenderTexture.Release();
                //Destroy(availableRenderTexture);
            }
            availableRenderTexture = null;
        }
        public RenderTexture GetRenderTexture()
        {
            return availableRenderTexture;
        }
    }

}
