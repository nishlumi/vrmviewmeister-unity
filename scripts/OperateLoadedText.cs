using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace UserHandleSpace
{
    public class OperateLoadedText : OperateLoadedUImage
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


        private BasicTransformInformation bti;

        private ManageAnimation animarea;

        private void Awake()
        {
            targetType = AF_TARGETTYPE.Text;
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

        /// <summary>
        /// To pack all properties for HTML-UI
        /// </summary>
        /// <param name="type">1 - normal light, 0 - directional light</param>
        public override void GetIndicatedPropertyFromOuter(int type)
        {
            string ret = "";
            Text tex = transform.GetComponent<Text>();
            string apos = GetAnchorPos();

            ret = tex.text + "\t" + apos + "\t" + tex.fontSize.ToString() + "\t" + ((int)tex.fontStyle).ToString() + "\t" + ColorUtility.ToHtmlStringRGBA(tex.color)
            ;

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }

        //---Transform for manual operation--------------------------------------------=============

        //---method for Text UI----------------------------------------=======================
        public int GetFontSize()
        {
            RectTransform rect = GetRectTransform();
            Text tex = transform.GetComponent<Text>();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(tex.fontSize);
#endif
            return tex.fontSize;

        }
        public void SetFontSize(int size)
        {
            Text tex = transform.GetComponent<Text>();
            tex.fontSize = size;
        }
        public int GetFontStyle()
        {
            Text tex = transform.GetComponent<Text>();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal((int)tex.fontStyle);
#endif
            return (int)tex.fontStyle;
        }
        public void SetFontStyle(int style)
        {
            Text tex = transform.GetComponent<Text>();
            tex.fontStyle = (FontStyle)style;
        }

        public string GetText()
        {
            Text tex = transform.GetComponent<Text>();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(tex.text);
#endif
            return tex.text;
        }
        public void SetText(string text)
        {
            Text tex = transform.GetComponent<Text>();
            tex.text = text;
        }

        public Color GetFontColor()
        {
            Text tex = transform.GetComponent<Text>();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ColorUtility.ToHtmlStringRGBA(tex.color));
#endif
            return tex.color;
        }
        public void SetFontColor(string param)
        {
            Text tex = transform.GetComponent<Text>();
            Color col = ColorUtility.TryParseHtmlString(param, out col) ? col : Color.black;

            tex.color = col;
        }
    }

}
