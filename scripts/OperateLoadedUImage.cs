using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


namespace UserHandleSpace
{
    public class OperateLoadedUImage : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void ChangeTransformOnUpdate(string val);

        [DllImport("__Internal")]
        private static extern void ReceiveObjectVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);

        public string Title;
        protected Vector2 oldPosition;
        protected Quaternion oldRotation;
        protected Vector2 defaultPosition;
        protected Quaternion defaultRotation;

        protected Vector2 oldikposition;
        protected AF_TARGETTYPE targetType;

        private ManageAnimation animarea;
        private BasicTransformInformation bti;

        private void Awake()
        {
            targetType = AF_TARGETTYPE.UImage;
            bti = new BasicTransformInformation();
            bti.dimension = "2d";
            animarea = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

        }
        // Start is called before the first frame update
        void Start()
        {
            RectTransform rect = GetRectTransform();
            rect.anchoredPosition = oldPosition;
            rect.rotation = oldRotation;

        }

        // Update is called once per frame
        void Update()
        {
            RectTransform rect = GetRectTransform();
            if ((rect.anchoredPosition != oldPosition) || (rect.rotation != oldRotation))
            {
            }

            oldPosition = rect.anchoredPosition;
            oldRotation = rect.rotation;

        }

        public RectTransform GetRectTransform()
        {
            return transform.gameObject.GetComponent<RectTransform>();
        }
        public void SaveDefaultTransform(bool ispos, bool isrotate)
        {
            RectTransform rect = GetRectTransform();
            if (ispos) defaultPosition = rect.anchoredPosition;
            if (isrotate) defaultRotation = rect.rotation;
        }
        public void LoadDefaultTransform(bool ispos, bool isrotate)
        {
            RectTransform rect = GetRectTransform();
            if (ispos) rect.anchoredPosition = defaultPosition;
            if (isrotate) rect.rotation = defaultRotation;
        }
        public void SetObjectTitle(string title)
        {
            Title = title;
        }
        public void GetObjectTitle()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(Title);
#endif
        }

        /// <summary>
        /// To pack all properties for HTML-UI
        /// </summary>
        /// <param name="type">1 - normal light, 0 - directional light</param>
        public virtual void GetIndicatedPropertyFromOuter(int type)
        {
            string ret = "";
            RectTransform rect = transform.GetComponent<RectTransform>();
            string apos = GetAnchorPos();
            Color color = GetImageBaseColor();

            ret = apos + "\t" + ColorUtility.ToHtmlStringRGBA(color)
            ;

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }

        //---Transform for manual operation--------------------------------------------=============
        public string GetAnchorPos()
        {
            string ret = "";
            RectTransform rect = transform.GetComponent<RectTransform>();

            if ((rect.pivot.x == 0f) && (rect.pivot.y == 1f))
            {
                ret = "tl";
            }
            else if ((rect.pivot.x == 0f) && (rect.pivot.y == 0.5f))
            {
                ret = "ml";
            }
            else if ((rect.pivot.x == 0f) && (rect.pivot.y == 0f))
            {
                ret = "bl";
            }
            else if ((rect.pivot.x == 0.5f) && (rect.pivot.y == 1f))
            {
                ret = "tm";
            }
            else if ((rect.pivot.x == 0.5f) && (rect.pivot.y == 0.5f))
            {
                ret = "mm";
            }
            else if ((rect.pivot.x == 0.5f) && (rect.pivot.y == 0f))
            {
                ret = "bm";
            }
            else if ((rect.pivot.x == 1f) && (rect.pivot.y == 1f))
            {
                ret = "tr";
            }
            else if ((rect.pivot.x == 1f) && (rect.pivot.y == 0.5f))
            {
                ret = "mr";
            }
            else if ((rect.pivot.x == 1f) && (rect.pivot.y == 0f))
            {
                ret = "br";
            }

            return ret;
        }
        public string GetAnchorPosFromOuter()
        {
            string ret = GetAnchorPos();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
            return ret;
        }
        public void SetAnchorPos(string anchorpos)
        {
            RectTransform rect = transform.GetComponent<RectTransform>();

            if (anchorpos == "tl")
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
            }
            else if (anchorpos == "ml")
            {
                rect.anchorMin = new Vector2(0f, 0.5f);
                rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);
            }
            else if (anchorpos == "bl")
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 0f);
            }
            else if (anchorpos == "tm")
            {
                rect.anchorMin = new Vector2(0.5f, 1f);
                rect.anchorMax = new Vector2(0.5f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
            }
            else if (anchorpos == "mm")
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
            }
            else if (anchorpos == "bm")
            {
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
            }
            else if (anchorpos == "tr")
            {
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
            }
            else if (anchorpos == "mr")
            {
                rect.anchorMin = new Vector2(1f, 0.5f);
                rect.anchorMax = new Vector2(1f, 0.5f);
                rect.pivot = new Vector2(1f, 0.5f);
            }
            else if (anchorpos == "br")
            {
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(1f, 0f);
            }

        }

        //--- 2D transform -------------------------------------------------------------------------------------------
        public void GetCommonTransformFromOuter()
        {
            RectTransform rect = GetRectTransform();

            Vector2 pos = rect.anchoredPosition;
            Vector3 rot = rect.rotation.eulerAngles;
            Vector2 siz = rect.sizeDelta;
            Vector2 sca = new Vector2(rect.localScale.x, rect.localScale.y);
            string ret = "";
            ret = pos.x + "," + pos.y + "," + 0f + "%"
                + rot.x + "," + rot.y + "," + rot.z + "%"
                + siz.x + "," + siz.y + "," + 0f + "%"
                + sca.x + "," + sca.y + "," + 0f;

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public Vector2 GetSizeFromOuter()
        {
            RectTransform rect = GetRectTransform();

            Vector2 ret;
            ret = rect.sizeDelta;

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(JsonUtility.ToJson(ret));
#endif

            return ret;
        }
        public void SetSizeFromOuter(string param)
        {
            RectTransform rect = GetRectTransform();

            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            //float z = float.TryParse(prm[2], out z) ? z : 0f;

            rect.sizeDelta = new Vector2(x, y);

        }
        public Vector2 GetScaleFromOuter()
        {
            RectTransform rect = GetRectTransform();

            Vector2 ret = new Vector2(rect.localScale.x, rect.localScale.y);

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(JsonUtility.ToJson(ret));
#endif

            return ret;
        }
        public void SetScaleFromOuter(string param)
        {
            RectTransform rect = GetRectTransform();

            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = 1f; // float.TryParse(prm[2], out z) ? z : 0f;

            rect.localScale = new Vector3(x, y, z);

        }

        public Vector2 GetPositionFromOuter()
        {
            RectTransform rect = GetRectTransform();

            Vector2 ret;
            ret = rect.anchoredPosition;

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(JsonUtility.ToJson(ret));
#endif

            return ret;
        }
        public void SetPositionFromOuter(string param)
        {
            RectTransform rect = GetRectTransform();

            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            //float z = float.TryParse(prm[2], out z) ? z : 0f;
            bool isabs = prm[2] == "1" ? true : false;

            //rect.anchoredPosition = new Vector2(x, y);
            rect.DOAnchorPos(new Vector2(x, y), 0.1f).SetRelative(!isabs);

        }
        public Vector3 GetRotationFromOuter()
        {
            RectTransform rect = GetRectTransform();

            Vector3 ret;
            ret = rect.rotation.eulerAngles;

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(JsonUtility.ToJson(ret));
#endif

            return ret;

        }
        public void SetRotationFromOuter(string param)
        {
            RectTransform rect = GetRectTransform();

            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;

            //rect.rotation = Quaternion.Euler(new Vector3(x, y, z));
            rect.DORotateQuaternion(Quaternion.Euler(new Vector3(x, y, z)), 0.1f);
        }
        public void SetRotationZFromOuter(float z)
        {
            RectTransform rect = GetRectTransform();
            Vector3 rot = rect.rotation.eulerAngles;
            rot.z = z;
            rect.DORotateQuaternion(Quaternion.Euler(rot), 0.1f);
        }
        public void SetImageBaseColor(Color col)
        {
            Image img = gameObject.GetComponent<Image>();
            img.color = col;
        }
        public void SetImageBaseColorFromOuter(string param)
        {
            Color col = ColorUtility.TryParseHtmlString(param, out col) ? col : Color.white;
            SetImageBaseColor(col);
        }
        public Color GetImageBaseColor()
        {
            Image img = gameObject.GetComponent<Image>();
            return img.color;
        }
        public void  GetImageBaseColorFromOuter()
        {
            Color col = GetImageBaseColor();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ColorUtility.ToHtmlStringRGBA(col));
#endif
        }
    }

}
