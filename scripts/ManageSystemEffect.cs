using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using DG.Tweening;

namespace UserHandleSpace
{
    public class ManageSystemEffect : MonoBehaviour
    {

        [DllImport("__Internal")]
        private static extern void ReceiveObjectVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);



        public GameObject PostProcessingArea;
        public GameObject SystemViewArea;
        public GameObject FrontCamera;

        public string[] ProcessNames = { "Bloom", "Chromatic", "ColorGrading", "DepthOfField", "Grain", "MotionBlur", "Vignette" };

        private PostProcessVolume ppvol;

        private void Awake()
        {
            ppvol = PostProcessingArea.GetComponent<PostProcessVolume>();


            
        }
        // Start is called before the first frame update
        void Start()
        {
            ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            //mana.FirstAddFixedAvatar(gameObject.name, gameObject, gameObject, "SystemEffect", AF_TARGETTYPE.SystemEffect);


        }

        // Update is called once per frame
        void Update()
        {

        }
        /// <summary>
        /// To pack all properties for HTML-UI
        /// </summary>
        /// <param name="type">t</param>
        public void GetIndicatedPropertyFromOuter(int type)
        {
            string ret = "";

            List<string> arr = new List<string>();
            arr.Add(GetEnablePostProcessing("bloom,0").ToString() + "," + GetBloomSetting("intensity,0").ToString());
            arr.Add(GetEnablePostProcessing("chromatic,0").ToString() + "," + GetChromaticSetting("intensity,0"));
            arr.Add(GetEnablePostProcessing("colorgrading,0").ToString() + ",#" + ColorUtility.ToHtmlStringRGBA(GetColorGradingSettingColor("colorfilter,0")) + "," + GetColorGradingSettingFloat("temperature,0").ToString() + "," + GetColorGradingSettingFloat("tint,0").ToString());
            arr.Add(GetEnablePostProcessing("depthoffield,0").ToString() + "," + GetDepthOfFieldSetting("aperture,0").ToString() + "," + GetDepthOfFieldSetting("focallength,0").ToString());
            arr.Add(GetEnablePostProcessing("grain,0").ToString() + "," + GetGrainSetting("intensity,0").ToString() + "," + GetGrainSetting("size,0").ToString());
            arr.Add(GetEnablePostProcessing("vignette,0").ToString() + "," + GetVignetteSetting("intensity,0").ToString());
            arr.Add(GetEnablePostProcessing("motionblur,0").ToString() + "," + GetMotionBlurSetting("shutterangle,0").ToString() + "," + GetMotionBlurSetting("samplecount,0").ToString());

            ret = String.Join("\t", arr);

            //0 - Bloom(on/off, intensity)
            //1 - Chromatic(on/off, intensity)
            //2 - Color grading(on/off, colorfilter, temperature, tint)
            //3 - depth of field(on/off, aperture, focallength)
            //4 - Grain(on/off, intensity, size)
            //5 - Vignette(on/off, intensity)
            //6 - MotionBlur(on/off, shutterangle, samplecount)

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }

        //===========================================================================================================================
        //  Effect operational functions
        //===========================================================================================================================
        public void EnableEffectIK(int index)
        {
            GameObject[] efs = GameObject.FindGameObjectsWithTag("EffectDestination");
            if ((0 <= index) && (index < efs.Length))
            {
                efs[index].GetComponent<OperateLoadedEffect>().SetEnable(1);
            }
        }
        public string GetEffectGenre()
        {
            string ret = "";
            GameObject[] efs = GameObject.FindGameObjectsWithTag("EffectSystem");
            string[] arr = new string[efs.Length];
            for (int i = 0; i < efs.Length; i++)
            {
                arr[i] = efs[i].name;
            }
            ret = string.Join(",", arr);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
            return ret;
        }

        //===========================================================================================================================
        //  Post-Processing operational functions
        //===========================================================================================================================

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">0 - effect name, 1 - 1=true,0=false</param>
        public void EnablePostProcessing(string param)
        {
            string[] prm = param.Split(',');
            string effectName = prm[0];
            bool ena = prm[1] == "1" ? true : false;

            if (effectName.ToLower() == "bloom")
            {
                Bloom st = ppvol.profile.GetSetting<Bloom>();
                st.enabled.Override(ena);
            }
            else if (effectName.ToLower() == "chromatic")
            {
                ChromaticAberration st = ppvol.profile.GetSetting<ChromaticAberration>();
                st.enabled.Override(ena);
            }
            else if (effectName.ToLower() == "colorgrading")
            {
                ColorGrading st = ppvol.profile.GetSetting<ColorGrading>();
                st.enabled.Override(ena);
            }
            else if (effectName.ToLower() == "depthoffield")
            {
                DepthOfField st = ppvol.profile.GetSetting<DepthOfField>();
                st.enabled.Override(ena);
            }
            else if (effectName.ToLower() == "grain")
            {
                Grain st = ppvol.profile.GetSetting<Grain>();
                st.enabled.Override(ena);
            }
            else if (effectName.ToLower() == "motionblur")
            {
                MotionBlur st = ppvol.profile.GetSetting<MotionBlur>();
                st.enabled.Override(ena);
            }
            else if (effectName.ToLower() == "vignette")
            {
                Vignette st = ppvol.profile.GetSetting<Vignette>();
                st.enabled.Override(ena);
            }
            else if (effectName.ToLower() == "fxaa")
            {
                PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                ppl.antialiasingMode = ena ? PostProcessLayer.Antialiasing.FastApproximateAntialiasing : PostProcessLayer.Antialiasing.None;
            }
            else if (effectName.ToLower() == "smaa")
            {
                PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                ppl.antialiasingMode = ena ? PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing : PostProcessLayer.Antialiasing.None;
            }
            else if (effectName.ToLower() == "taa")
            {
                PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                ppl.antialiasingMode = ena ? PostProcessLayer.Antialiasing.TemporalAntialiasing : PostProcessLayer.Antialiasing.None;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">0 - effect name, 1 - is contact html(1=true, 0=false)</param>
        /// <returns></returns>
        public int GetEnablePostProcessing(string param)
        {
            int ret = 0;
            string[] prm = param.Split(',');
            string effectName = prm[0];
            bool is_contacthtml = prm[1] == "1" ? true : false;

            if (effectName.ToLower() == "bloom")
            {
                Bloom st = ppvol.profile.GetSetting<Bloom>();
                ret = st.enabled.value ? 1 : 0;
            }
            else if (effectName.ToLower() == "chromatic")
            {
                ChromaticAberration st = ppvol.profile.GetSetting<ChromaticAberration>();
                ret = st.enabled.value ? 1 : 0;
            }
            else if (effectName.ToLower() == "colorgrading")
            {
                ColorGrading st = ppvol.profile.GetSetting<ColorGrading>();
                ret = st.enabled.value ? 1 : 0;
            }
            else if (effectName.ToLower() == "depthoffield")
            {
                DepthOfField st = ppvol.profile.GetSetting<DepthOfField>();
                ret = st.enabled.value ? 1 : 0;
            }
            else if (effectName.ToLower() == "grain")
            {
                Grain st = ppvol.profile.GetSetting<Grain>();
                ret = st.enabled.value ? 1 : 0;
            }
            else if (effectName.ToLower() == "motionblur")
            {
                MotionBlur st = ppvol.profile.GetSetting<MotionBlur>();
                ret = st.enabled.value ? 1 : 0;
            }
            else if (effectName.ToLower() == "vignette")
            {
                Vignette st = ppvol.profile.GetSetting<Vignette>();
                ret = st.enabled.value ? 1 : 0;
            }
            else if (effectName.ToLower() == "fxaa")
            {
                PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                ret = (ppl.antialiasingMode == PostProcessLayer.Antialiasing.FastApproximateAntialiasing) ? 1 : 0;
            }
            else if (effectName.ToLower() == "smaa")
            {
                PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                ret = (ppl.antialiasingMode == PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing) ? 1 : 0;
            }
            else if (effectName.ToLower() == "taa")
            {
                PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                ret = (ppl.antialiasingMode == PostProcessLayer.Antialiasing.TemporalAntialiasing) ? 1 : 0;
            }

            return ret;
        }
        public List<float> PackEffectValue(string effectName)
        {
            List<float> ret = new List<float>();


            if (effectName.ToLower() == "bloom")
            {
                Bloom st = ppvol.profile.GetSetting<Bloom>();
                ret.Add(st.intensity.value);
            }
            else if (effectName.ToLower() == "chromatic")
            {
                ChromaticAberration st = ppvol.profile.GetSetting<ChromaticAberration>();
                ret.Add(st.intensity.value);
            }
            else if (effectName.ToLower() == "colorgrading")
            {
                ColorGrading st = ppvol.profile.GetSetting<ColorGrading>();
                ret.Add(st.temperature.value);
                ret.Add(st.tint.value);
                ret.Add(st.colorFilter.value.r);
                ret.Add(st.colorFilter.value.g);
                ret.Add(st.colorFilter.value.b);
                ret.Add(st.colorFilter.value.a);
            }
            else if (effectName.ToLower() == "depthoffield")
            {
                DepthOfField st = ppvol.profile.GetSetting<DepthOfField>();
                ret.Add(st.aperture.value);
                ret.Add(st.focalLength.value);
            }
            else if (effectName.ToLower() == "grain")
            {
                Grain st = ppvol.profile.GetSetting<Grain>();
                ret.Add(st.intensity.value);
                ret.Add(st.size.value);
            }
            else if (effectName.ToLower() == "motionblur")
            {
                MotionBlur st = ppvol.profile.GetSetting<MotionBlur>();
                ret.Add(st.shutterAngle.value);
                ret.Add(st.sampleCount.value);
            }
            else if (effectName.ToLower() == "vignette")
            {
                Vignette st = ppvol.profile.GetSetting<Vignette>();
                ret.Add(st.intensity.value);
            }
            else if (effectName.ToLower() == "fxaa")
            {
                PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                ret.Add(ppl.fastApproximateAntialiasing.fastMode ? 1f : 0f);
                ret.Add(ppl.fastApproximateAntialiasing.keepAlpha ? 1f : 0f);
            }
            else if (effectName.ToLower() == "smaa")
            {
                PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                ret.Add((float)ppl.subpixelMorphologicalAntialiasing.quality);
            }
            else if (effectName.ToLower() == "taa")
            {
                PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                ret.Add(ppl.temporalAntialiasing.sharpness);
            }

            return ret;
        }
        public Sequence SetEffectValues(Sequence seq, string effectName, List<float> values, bool isEnable, bool useDOTween, float duration)
        {

            if (effectName.ToLower() == "bloom")
            {
                Bloom st = ppvol.profile.GetSetting<Bloom>();

                seq.Join(DOVirtual.DelayedCall(duration, () => st.enabled.Override(isEnable), false));
                
                if (isEnable)
                {
                    if (useDOTween) seq.Join(DOTween.To(() => st.intensity.value, x => st.intensity.Override(x), values[0], duration));
                    else st.intensity.Override(values[0]);
                }
                
            }
            else if (effectName.ToLower() == "chromatic")
            {
                ChromaticAberration st = ppvol.profile.GetSetting<ChromaticAberration>();

                seq.Join(DOVirtual.DelayedCall(duration, () => st.enabled.Override(isEnable), false));
                
                if (isEnable)
                {
                    if (useDOTween) seq.Join(DOTween.To(() => st.intensity.value, x => st.intensity.Override(x), values[0], duration));
                    else st.intensity.Override(values[0]);
                }
                
            }
            else if (effectName.ToLower() == "colorgrading")
            {
                ColorGrading st = ppvol.profile.GetSetting<ColorGrading>();

                seq.Join(DOVirtual.DelayedCall(duration, () => st.enabled.Override(isEnable), false));
                if (isEnable)
                {
                    if (useDOTween) seq.Join(DOTween.To(() => st.temperature.value, x => st.temperature.Override(x), values[0], duration));
                    else st.temperature.Override(values[0]);

                    if (useDOTween) seq.Join(DOTween.To(() => st.tint.value, x => st.tint.Override(x), values[1], duration));
                    else st.tint.Override(values[1]);

                    Color col = new Color(values[2], values[3], values[4], values[5]);
                    if (useDOTween) seq.Join(DOTween.To(() => st.colorFilter.value, x => st.colorFilter.Override(x), col, duration));
                    else st.colorFilter.Override(col);


                }
            }
            else if (effectName.ToLower() == "depthoffield")
            {
                DepthOfField st = ppvol.profile.GetSetting<DepthOfField>();

                seq.Join(DOVirtual.DelayedCall(duration, () => st.enabled.Override(isEnable), false));
                if (isEnable)
                {
                    if (useDOTween) seq.Join(DOTween.To(() => st.aperture.value, x => st.aperture.Override(x), values[0], duration));
                    else st.aperture.Override(values[0]);

                    if (useDOTween) seq.Join(DOTween.To(() => st.focalLength.value, x => st.focalLength.Override(x), values[1], duration));
                    else st.focalLength.Override(values[1]);

                }

            }
            else if (effectName.ToLower() == "grain")
            {
                Grain st = ppvol.profile.GetSetting<Grain>();

                seq.Join(DOVirtual.DelayedCall(duration, () => st.enabled.Override(isEnable), false));
                if (isEnable)
                {
                    if (useDOTween) seq.Join(DOTween.To(() => st.intensity.value, x => st.intensity.Override(x), values[0], duration));
                    else st.intensity.Override(values[0]);

                    if (useDOTween) seq.Join(DOTween.To(() => st.size.value, x => st.size.Override(x), values[1], duration));
                    else st.size.Override(values[1]);

                }

            }
            else if (effectName.ToLower() == "motionblur")
            {
                MotionBlur st = ppvol.profile.GetSetting<MotionBlur>();

                seq.Join(DOVirtual.DelayedCall(duration, () => st.enabled.Override(isEnable), false));
                if (isEnable)
                {
                    if (useDOTween) seq.Join(DOTween.To(() => st.shutterAngle.value, x => st.shutterAngle.Override(x), values[0], duration));
                    else st.shutterAngle.Override(values[0]);

                    if (useDOTween) seq.Join(DOTween.To(() => (int)st.sampleCount.value, x => st.sampleCount.Override((int)x), values[1], duration));
                    else st.sampleCount.Override((int)values[1]);

                }

            }
            else if (effectName.ToLower() == "vignette")
            {
                Vignette st = ppvol.profile.GetSetting<Vignette>();

                seq.Join(DOVirtual.DelayedCall(duration, () => st.enabled.Override(isEnable), false));
                if (isEnable)
                {
                    if (useDOTween) seq.Join(DOTween.To(() => st.intensity.value, x => st.intensity.Override(x), values[0], duration));
                    else st.intensity.Override(values[0]);

                }
            }
            else if (effectName.ToLower() == "fxaa")
            {
                PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                ppl.antialiasingMode = isEnable ? PostProcessLayer.Antialiasing.FastApproximateAntialiasing : PostProcessLayer.Antialiasing.None;
                if (isEnable)
                {
                    ppl.fastApproximateAntialiasing.fastMode = values[0] == 1f ? true : false;
                }

            }
            else if (effectName.ToLower() == "smaa")
            {
                PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                ppl.antialiasingMode = isEnable ? PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing : PostProcessLayer.Antialiasing.None;
                if (isEnable)
                {
                    ppl.subpixelMorphologicalAntialiasing.quality = (SubpixelMorphologicalAntialiasing.Quality)values[0];
                }

            }
            else if (effectName.ToLower() == "taa")
            {
                PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                ppl.antialiasingMode = isEnable ? PostProcessLayer.Antialiasing.TemporalAntialiasing : PostProcessLayer.Antialiasing.None;
                if (isEnable)
                {
                    ppl.temporalAntialiasing.sharpness = values[0];
                }

            }

            return seq;
        }


        public void BloomSetting(string param)
        {
            string[] prm = param.Split(',');
            string name = prm[0];
            float v = float.TryParse(prm[1], out v) ? v : 0f;
            Bloom st = ppvol.profile.GetSetting<Bloom>();
            if (name.ToLower() == "intensity")
            {
                st.intensity.Override(v);
            }
        }
        public float GetBloomSetting(string param)
        {
            float ret = 0f;
            Bloom st = ppvol.profile.GetSetting<Bloom>();

            string[] prm = param.Split(',');
            string name = prm[0];
            bool is_contacthtml = prm[1] == "1" ? true : false;

            if (name.ToLower() == "intensity")
            {
                ret = st.intensity.value;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml) ReceiveFloatVal(ret);
#endif
            return ret;
        }

        public void ChromaticSetting(string param)
        {
            string[] prm = param.Split(',');
            string name = prm[0];
            float v = float.TryParse(prm[1], out v) ? v : 0f;
            ChromaticAberration st = ppvol.profile.GetSetting<ChromaticAberration>();
            if (name.ToLower() == "intensity")
            {
                st.intensity.Override(v);
            }
        }
        public float GetChromaticSetting(string param)
        {
            float ret = 0f;
            ChromaticAberration st = ppvol.profile.GetSetting<ChromaticAberration>();

            string[] prm = param.Split(',');
            string name = prm[0];
            bool is_contacthtml = prm[1] == "1" ? true : false;

            if (name.ToLower() == "intensity")
            {
                ret = st.intensity.value;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml) ReceiveFloatVal(ret);
#endif
            return ret;
        }

        public void ColorGradingSetting(string param)
        {
            string[] prm = param.Split(',');
            string name = prm[0];

            ColorGrading st = ppvol.profile.GetSetting<ColorGrading>();
            if (name.ToLower() == "temperature")
            {
                float v = float.TryParse(prm[1], out v) ? v : 0f;
                st.temperature.Override(v);
            }
            else if (name.ToLower() == "tint")
            {
                float v = float.TryParse(prm[1], out v) ? v : 0f;
                st.tint.Override(v);
            }
            else if (name.ToLower() == "colorfilter")
            {
                Color col = ColorUtility.TryParseHtmlString(prm[1], out col) ? col : Color.white;
                st.colorFilter.Override(col);
            }
        }
        public float GetColorGradingSettingFloat(string param)
        {
            float ret = 0f;
            ColorGrading st = ppvol.profile.GetSetting<ColorGrading>();

            string[] prm = param.Split(',');
            string name = prm[0];
            bool is_contacthtml = prm[1] == "1" ? true : false;

            if (name.ToLower() == "temperature")
            {
                ret = st.temperature.value;
            }
            else if (name.ToLower() == "tint")
            {
                ret = st.tint.value;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml) ReceiveFloatVal(ret);
#endif
            return ret;
        }
        public Color GetColorGradingSettingColor(string param)
        {
            Color ret = Color.white;
            ColorGrading st = ppvol.profile.GetSetting<ColorGrading>();

            string[] prm = param.Split(',');
            string name = prm[0];
            bool is_contacthtml = prm[1] == "1" ? true : false;

            if (name.ToLower() == "colorfilter")
            {
                ret = st.colorFilter.value;
            }

#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml) ReceiveStringVal(ColorUtility.ToHtmlStringRGBA(st.colorFilter.value));
#endif
            return ret;
        }


        public void DepthOfFieldSetting(string param)
        {
            string[] prm = param.Split(',');
            string name = prm[0];
            float v = float.TryParse(prm[1], out v) ? v : 0f;
            DepthOfField st = ppvol.profile.GetSetting<DepthOfField>();
            if (name.ToLower() == "aperture")
            {
                st.aperture.Override(v);
            }
            else if (name.ToLower() == "focallength")
            {
                st.focalLength.Override(v);
            }
        }
        public float GetDepthOfFieldSetting(string param)
        {
            float ret = 0f;
            DepthOfField st = ppvol.profile.GetSetting<DepthOfField>();

            string[] prm = param.Split(',');
            string name = prm[0];
            bool is_contacthtml = prm[1] == "1" ? true : false;

            if (name.ToLower() == "aperture")
            {
                ret = st.aperture.value;
            }
            else if (name.ToLower() == "focallength")
            {
                ret = st.focalLength.value;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml) ReceiveFloatVal(ret);
#endif
            return ret;
        }


        public void GrainSetting(string param)
        {
            string[] prm = param.Split(',');
            string name = prm[0];
            float v = float.TryParse(prm[1], out v) ? v : 0f;
            Grain st = ppvol.profile.GetSetting<Grain>();
            if (name.ToLower() == "intensity")
            {
                st.intensity.Override(v);
            }
            else if (name.ToLower() == "size")
            {
                st.size.Override(v);
            }
        }
        public float GetGrainSetting(string param)
        {
            float ret = 0f;
            Grain st = ppvol.profile.GetSetting<Grain>();

            string[] prm = param.Split(',');
            string name = prm[0];
            bool is_contacthtml = prm[1] == "1" ? true : false;

            if (name.ToLower() == "intensity")
            {
                ret = st.intensity.value;
            }
            if (name.ToLower() == "size")
            {
                ret = st.size.value;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml) ReceiveFloatVal(ret);
#endif
            return ret;
        }


        public void VignetteSetting(string param)
        {
            string[] prm = param.Split(',');
            string name = prm[0];
            float v = float.TryParse(prm[1], out v) ? v : 0f;
            Vignette st = ppvol.profile.GetSetting<Vignette>();
            if (name.ToLower() == "intensity")
            {
                st.intensity.Override(v);
            }
        }
        public float GetVignetteSetting(string param)
        {
            float ret = 0f;
            Vignette st = ppvol.profile.GetSetting<Vignette>();

            string[] prm = param.Split(',');
            string name = prm[0];
            bool is_contacthtml = prm[1] == "1" ? true : false;

            if (name.ToLower() == "intensity")
            {
                ret = st.intensity.value;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml) ReceiveFloatVal(ret);
#endif
            return ret;
        }


        public void MotionBlurSetting(string param)
        {
            string[] prm = param.Split(',');
            string name = prm[0];
            float v = float.TryParse(prm[1], out v) ? v : 0f;
            MotionBlur st = ppvol.profile.GetSetting<MotionBlur>();
            if (name.ToLower() == "shutterangle")
            {
                st.shutterAngle.Override(v);
            }
            else if (name.ToLower() == "samplecount")
            {
                st.sampleCount.Override((int)v);
            }
        }
        public float GetMotionBlurSetting(string param)
        {
            float ret = 0f;
            MotionBlur st = ppvol.profile.GetSetting<MotionBlur>();

            string[] prm = param.Split(',');
            string name = prm[0];
            bool is_contacthtml = prm[1] == "1" ? true : false;

            if (name.ToLower() == "shutterangle")
            {
                ret = st.shutterAngle.value;
            }
            else if (name.ToLower() == "samplecount")
            {
                ret = st.sampleCount.value;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml) ReceiveFloatVal(ret);
#endif
            return ret;
        }

        public void SetAntiAlias(string param)
        {
            string[] prm = param.Split(',');
            string name = prm[0];
            
            PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
            if (ppl.antialiasingMode == PostProcessLayer.Antialiasing.FastApproximateAntialiasing)
            {
                if (name.ToLower() == "fastmode")
                {
                    ppl.fastApproximateAntialiasing.fastMode = prm[2] == "1" ? true : false;
                }
                else if (name.ToLower () == "keepalpha")
                {
                    ppl.fastApproximateAntialiasing.keepAlpha = prm[2] == "1" ? true : false;
                }
            }
            else if (ppl.antialiasingMode == PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing)
            {
                float v = float.TryParse(prm[1], out v) ? v : 0f;
                ppl.subpixelMorphologicalAntialiasing.quality = (SubpixelMorphologicalAntialiasing.Quality)v;

            }
            else if (ppl.antialiasingMode == PostProcessLayer.Antialiasing.TemporalAntialiasing)
            {
                float v = float.TryParse(prm[1], out v) ? v : 0f;
                if (name.ToLower() == "sharpness")
                {
                    ppl.temporalAntialiasing.sharpness = v;
                }
            }
        }
        public float GetAntiAlias(string param)
        {
            float ret = 0f;
            PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();

            string[] prm = param.Split(',');
            string name = prm[0];
            bool is_contacthtml = prm[1] == "1" ? true : false;

            if (ppl.antialiasingMode == PostProcessLayer.Antialiasing.FastApproximateAntialiasing)
            {
                if (name.ToLower() == "fastmode")
                {
                    ret = ppl.fastApproximateAntialiasing.fastMode ? 1f : 0f;
                }
                else if (name.ToLower() == "keepalpha")
                {
                    ret = ppl.fastApproximateAntialiasing.keepAlpha ? 1f : 0f;
                }
            }
            else if (ppl.antialiasingMode == PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing)
            {
                ret = (float)ppl.subpixelMorphologicalAntialiasing.quality;
            }
            else if (ppl.antialiasingMode == PostProcessLayer.Antialiasing.TemporalAntialiasing)
            {
                if (name.ToLower() == "sharpness")
                {
                    ret = ppl.temporalAntialiasing.sharpness;
                }
            }

#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml) ReceiveFloatVal(ret);
#endif
            return ret;
        }
    }

}
