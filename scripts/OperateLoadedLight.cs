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

        private Light OwnLight;
        public LensFlare OwnFlare;

        private LightType bkup_lightType;
        private Color bkup_lightColor;
        private float bkup_lightRange;
        private float bkup_lightIntensity;
        private float bkup_lightSpotAngle;
        private LightRenderMode bkup_lightRenderMode;
        private float bkup_halo;
        private int bkup_flareType;
        private Color bkup_flareColor;
        private float bkup_flareBrightness;
        private float bkup_flareFade;
        [SerializeField]
        protected List<Flare> flares;

        

        override protected void Awake()
        {
            base.Awake();

            OwnLight = GetComponent<Light>();
            OwnFlare = GetComponent<LensFlare>();
            SetFlare(0);
            

            targetType = AF_TARGETTYPE.Light;
            bkup_flareType = 0;
        }
        // Start is called before the first frame update
        override protected void Start()
        {
            base.Start();

            GetDefault();
        }

        // Update is called once per frame
        void Update()
        {

        }
        override protected void OnDestroy()
        {
            base.OnDestroy();
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

            List<string> arr = new List<string>();
            arr.Add(ltype);
            arr.Add(lt.range.ToString());
            arr.Add("#" + ColorUtility.ToHtmlStringRGBA(lt.color));
            arr.Add(lt.intensity.ToString());
            arr.Add(lt.spotAngle.ToString());
            arr.Add(((int)lt.renderMode).ToString());
            if (ltype == "d")
            {
                arr.Add(lt.shadowStrength.ToString());
                arr.Add(GetHalo().ToString());
            }
            arr.Add(GetFlare().ToString());
            arr.Add("#" + ColorUtility.ToHtmlStringRGBA(GetFlareColor()) );
            arr.Add(GetFlareBrightness().ToString());
            arr.Add(GetFlareFade().ToString());

            /*ret = ltype + "\t"
                + lt.range.ToString() + "\t"
                + "#" + ColorUtility.ToHtmlStringRGBA(lt.color)
                + "\t" + lt.intensity.ToString()
                + "\t"+ lt.spotAngle.ToString()
                + "\t" + ((int)lt.renderMode).ToString()
                + "\t" + (GetHalo().ToString())
            ;*/
            //if (ltype == "d") ret += "\t" + lt.shadowStrength.ToString();

            ret = String.Join('\t', arr);

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public void GetDefault()
        {
            bkup_lightType = GetLightType();
            bkup_lightColor = GetColor();
            bkup_lightRange = GetRange();
            bkup_lightIntensity = GetPower();
            bkup_lightSpotAngle = GetSpotAngle();
            bkup_lightRenderMode = GetRenderMode();

            bkup_halo = GetHalo();
            bkup_flareType = GetFlare();
            if (gameObject.name == "Directional Light") bkup_flareType = 0;
            bkup_flareColor = GetFlareColor();
            bkup_flareBrightness = GetFlareBrightness();
            bkup_flareFade = GetFlareFade();

        }
        public void SetDefault()
        {
            SetLightType(bkup_lightType);
            SetColor(bkup_lightColor);
            SetRange(bkup_lightRange);
            SetPower(bkup_lightIntensity);
            SetSpotAngle(bkup_lightSpotAngle);
            SetRenderMode((int)bkup_lightRenderMode);

            SetHalo(bkup_halo);
            SetFlare(bkup_flareType);
            SetFlareColor(bkup_flareColor);
            SetFlareBrightness(bkup_flareBrightness);
            SetFlareFade(bkup_flareFade);
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
        public void SetHalo(float flag)
        {
            RenderSettings.haloStrength = flag;
        }
        public void SetHaloFromOuter(float flag)
        {
            RenderSettings.haloStrength = flag;
        }
        public float GetHalo()
        {
            
            return RenderSettings.haloStrength;
        }
        public void GetHaloFromOuter()
        {
            float ret = RenderSettings.haloStrength;
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
        }
        public void SetFlare(int type)
        {
            if (type < 0)
            {
                OwnFlare.flare = null;
            }
            if (gameObject.name == "Directional Light")
            {
                OwnFlare.flare = flares[type];
            }
            else
            {
                OperateLoadedLight oll = GameObject.Find("Directional Light").GetComponent<OperateLoadedLight>();

                if (oll != null)
                {
                    if ((oll.flares.Count > 0) && (type < oll.flares.Count))
                    {
                        if (type > -1)
                        {
                            OwnFlare.flare = oll.flares[type];
                        }
                        else
                        {
                            OwnFlare.flare = oll.flares[0];
                        }

                    }
                    else
                    {
                        OwnFlare.flare = null;
                    }
                }
            }
            
        }
        public int GetFlare()
        {
            OperateLoadedLight oll = GameObject.Find("Directional Light").GetComponent<OperateLoadedLight>();
            int ret = -1;
            if (oll != null)
            {
                ret = oll.flares.FindIndex(match =>
                {
                    if (match.name == OwnFlare.name) return true;
                    return false;
                });
            }
            return ret;
        }
        public void GetFlareFromOuter()
        {
            int ret = -1;
            OperateLoadedLight oll = GameObject.Find("Directional Light").GetComponent<OperateLoadedLight>();
            if (oll != null)
            {
                ret = oll.flares.FindIndex(match =>
                {
                    if (match.name == OwnFlare.name) return true;
                    return false;
                });
            }

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret);
#endif
        }
        public void SetFlareColorFromOuter(string param)
        {
            OperateLoadedLight oll = GameObject.Find("Directional Light").GetComponent<OperateLoadedLight>();
            if (oll != null)
            {
                Color col;
                if (ColorUtility.TryParseHtmlString(param, out col))
                {
                    OwnFlare.color = col;
                }
            }
        }
        public void SetFlareColor(Color col)
        {
            OperateLoadedLight oll = GameObject.Find("Directional Light").GetComponent<OperateLoadedLight>();
            if (oll != null)
            {
                OwnFlare.color = col;
            }
        }
        public void GetFlareColorFromOuter()
        {
            string ret = "#" + ColorUtility.ToHtmlStringRGBA(OwnFlare.color);

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public Color GetFlareColor()
        {
            return OwnFlare.color;
        }
        public void SetFlareFade(float val)
        {
            OwnFlare.fadeSpeed = val;
        }
        public float GetFlareFade()
        {
            return OwnFlare.fadeSpeed;
        }
        public void GetFlareFadeFromOuter()
        {
            float ret = OwnFlare.fadeSpeed;
            
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
        }
        public void SetFlareBrightness(float val)
        {
            OwnFlare.brightness = val;
        }
        public float GetFlareBrightness()
        {
            return OwnFlare.brightness;
        }
        public void GetFlareBrightnessFromOuter()
        {
            float ret = OwnFlare.brightness;
            
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
        }
        public Sequence SetTweenLight(Sequence seq, Light lt, AnimationTargetParts movedata, float duration)
        {
            LensFlare flare = OwnFlare;

            //---Range
            seq.Join(DOTween.To(() => lt.range, x => lt.range = x, movedata.range, duration));

            //---Color
            seq.Join(lt.DOColor(movedata.color, duration));

            //---Power
            //seq.Join(lt.DOIntensity(movedata.power, duration));
            seq.Join(DOTween.To(() => lt.intensity, x => lt.intensity = x, movedata.power, duration));

            //---Angle (SpotLight only)
            if (lt.type == LightType.Spot)
            {
                seq.Join(DOTween.To(() => lt.spotAngle, x => lt.spotAngle = x, movedata.spotAngle, duration));
            }

            //---Halo
            ////seq.Join(DOTween.To(() => RenderSettings.haloStrength, x => RenderSettings.haloStrength = x, movedata.halo, duration));


            //---flareBrightness
            seq.Join(DOTween.To(() => flare.brightness, x => flare.brightness = x, movedata.flareBrightness, duration));

            //---flare fade
            seq.Join(DOTween.To(() => flare.fadeSpeed, x => flare.fadeSpeed = x, movedata.flareFade, duration));


            return seq;
        }
    }

}
