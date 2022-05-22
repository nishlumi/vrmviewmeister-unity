using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using DG.Tweening;


namespace UserHandleSpace
{
    public class OperateLoadedLight : OperateLoadedBase
    {

        [DllImport("__Internal")]
        private static extern void ReceiveObjectVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);


        private void Awake()
        {
            targetType = AF_TARGETTYPE.Light;
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public override void SetEnableWholeIK(int intflag)
        {
            bool flag = intflag == 1 ? true : false;

            relatedHandleParent.SetActive(flag);

        }
        /// <summary>
        /// To pack all properties for HTML-UI
        /// </summary>
        /// <param name="type">1 - normal light, 0 - directional light</param>
        public void GetIndicatedPropertyFromOuter(int type)
        {
            string ret = "";
            Light lt = transform.gameObject.GetComponent<Light>();
            string ltype = "";
            if (lt.type == LightType.Spot)
            {
                ltype = "s";
            }
            else if(lt.type == LightType.Point)
            {
                ltype = "p";
            }
            else if (lt.type == LightType.Directional)
            {
                ltype = "d";
            }

            ret = ltype + "\t"
                + lt.range.ToString() + "\t"
                + "#" + ColorUtility.ToHtmlStringRGBA(lt.color)
                + "\t" + lt.intensity.ToString()
                + "\t"+ lt.spotAngle.ToString()
                + "\t" + ((int)lt.renderMode).ToString()
            ;
            if (ltype == "d") ret += "\t" + lt.shadowStrength.ToString();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }

        //--------------------------------------------------------------------
        public void SetLightType ( LightType type )
        {
            Light lt = transform.gameObject.GetComponent<Light>();
            lt.type = type;
        }
        public void SetLightTypeFromOuter(int param)
        {
            SetLightType((LightType)param);
        }
        public LightType GetLightType()
        {
            Light lt = transform.gameObject.GetComponent<Light>();
            return lt.type;
        }
        public void GetLightTypeFromOuter()
        {
            LightType ret = GetLightType();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal((int)ret);
#endif
        }
        //--------------------------------------------------------------------
        public float GetRange()
        {
            Light lt = transform.gameObject.GetComponent<Light>();

            return lt.range;
        }
        public void GetRangeFromOuter()
        {
            float ret = GetRange();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
        }
        public void SetRange(float range)
        {
            Light lt = transform.gameObject.GetComponent<Light>();

            lt.range = range;
        }

        public Color GetColor()
        {
            Light lt = transform.gameObject.GetComponent<Light>();

            return lt.color;
        }
        public void GetColorFromOuter()
        {
            Color ret = GetColor();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ColorUtility.ToHtmlStringRGBA(ret));
#endif
        }
        public void SetColor(Color col)
        {
            Light lt = transform.gameObject.GetComponent<Light>();

            lt.color = col;
        }
        public void SetColorFromOuter(string param)
        {
            Light lt = transform.gameObject.GetComponent<Light>();
            Color col;
            if (ColorUtility.TryParseHtmlString(param, out col))
            {
                lt.color = col;
            }
        }

        public float GetPower()
        {
            Light lt = transform.gameObject.GetComponent<Light>();

            return lt.intensity;
        }
        public void GetPowerFromOuter()
        {
            float ret = GetPower();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
        }
        public void SetPower(float power)
        {
            Light lt = transform.gameObject.GetComponent<Light>();

            lt.intensity = power;
        }

        public float GetSpotAngle()
        {
            float ret = 0f;
            Light lt = transform.gameObject.GetComponent<Light>();
            ret = lt.spotAngle;

            return ret;

        }
        public void GetSpotAngleFromOuter()
        {
            float ret = GetSpotAngle();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
        }
        public void SetSpotAngle(float angle)
        {
            Light lt = transform.gameObject.GetComponent<Light>();

            lt.spotAngle = angle;
        }

        public float GetShadowPower()
        {
            Light lt = transform.gameObject.GetComponent<Light>();

            return lt.shadowStrength;
        }
        public void GetShadowPowerFromOuter()
        {
            float ret = GetShadowPower();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
        }
        public void SetShadowPower(float intensity)
        {
            Light lt = transform.gameObject.GetComponent<Light>();
            lt.shadowStrength = intensity;
        }

        public LightRenderMode GetRenderMode()
        {
            Light lt = transform.gameObject.GetComponent<Light>();
            return lt.renderMode;
        }
        public void GetRenderModeFromOuter()
        {
            Light lt = transform.gameObject.GetComponent<Light>();
            int ret = (int)lt.renderMode;
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret);
#endif
        }

        /// <summary>
        /// Set the render mode for Light.
        /// </summary>
        /// <param name="val">0 - Auto, 1 - Important, 2 - NoImportant</param>
        public void SetRenderMode(int val)
        {
            Light lt = transform.gameObject.GetComponent<Light>();
            LightRenderMode ren = (LightRenderMode)val;
            lt.renderMode = ren;
        }
    }

}
