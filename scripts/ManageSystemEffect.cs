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
        public WipePostEffect WipeObject;

        public string[] ProcessNames = { "Bloom", "Chromatic", "ColorGrading", "DepthOfField", "Grain", "MotionBlur", "Vignette" };

        private PostProcessVolume ppvol;

        private List<string> bkuplist;
        private BasicNamedFloatList bkupProcessingList;

        private ManageAnimation manim;

        private void Awake()
        {
            ppvol = PostProcessingArea.GetComponent<PostProcessVolume>();


            
        }
        // Start is called before the first frame update
        void Start()
        {
            manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            //mana.FirstAddFixedAvatar(gameObject.name, gameObject, gameObject, "SystemEffect", AF_TARGETTYPE.SystemEffect);

            bkuplist = new List<string>();
            GetDefault();
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnDestroy()
        {
            RuntimeUtilities.DestroyProfile(ppvol.profile, true);
        }
        public void GetDefault()
        {
            bkuplist.Clear();

            bkuplist.Add(GetEnablePostProcessing("bloom,0").ToString() + "," + GetBloomSetting("intensity,0").ToString());
            bkuplist.Add(GetEnablePostProcessing("chromatic,0").ToString() + "," + GetChromaticSetting("intensity,0"));
            bkuplist.Add(
                GetEnablePostProcessing("colorgrading,0").ToString() + "," + 
                "#" + ColorUtility.ToHtmlStringRGBA(GetColorGradingSettingColor("colorfilter,0")) + "," + 
                GetColorGradingSettingFloat("temperature,0").ToString() + "," + 
                GetColorGradingSettingFloat("tint,0").ToString()
            );
            bkuplist.Add(
                GetEnablePostProcessing("depthoffield,0").ToString() + "," + 
                GetDepthOfFieldSetting("aperture,0").ToString() + "," + 
                GetDepthOfFieldSetting("focallength,0").ToString() + "," + 
                GetDepthOfFieldSetting("focusdistance,0").ToString()
            );
            bkuplist.Add(GetEnablePostProcessing("grain,0").ToString() + "," + GetGrainSetting("intensity,0").ToString() + "," + GetGrainSetting("size,0").ToString());

            Vector2 v2 = GetVignetteSettingVector2("center,0");
            bkuplist.Add(
                GetEnablePostProcessing("vignette,0").ToString() + "," + 
                GetVignetteSetting("intensity,0").ToString() + "," +
                GetVignetteSetting("smoothness,0").ToString() + "," +
                GetVignetteSetting("roundness,0").ToString() + "," +
                "#" + ColorUtility.ToHtmlStringRGBA(GetVignetteSettingColor("color,0")) + "," +
                v2.x.ToString() + "," + v2.y.ToString()
            );
            bkuplist.Add(GetEnablePostProcessing("motionblur,0").ToString() + "," + GetMotionBlurSetting("shutterangle,0").ToString() + "," + GetMotionBlurSetting("samplecount,0").ToString());

        }
        public void SetDefault()
        {
            //---bloom
            string[] bloom = bkuplist[0].Split(",");
            EnablePostProcessing("bloom," + bloom[0]);
            BloomSetting("intensity," + bloom[1]);

            //---chromatic
            string[] chromatic = bkuplist[1].Split(",");
            EnablePostProcessing("chromatic," + chromatic[0]);
            ChromaticSetting("intensity," + chromatic[1]);

            //---colorgrading
            string[] colorgrad = bkuplist[2].Split(",");
            EnablePostProcessing("colorgrading," + colorgrad[0]);
            ColorGradingSetting("colorfilter," + colorgrad[1]);
            ColorGradingSetting("temperature," + colorgrad[2]);
            ColorGradingSetting("tint," + colorgrad[3]);

            //---depthoffield
            string[] depthoffield = bkuplist[3].Split(",");
            EnablePostProcessing("depthoffield," + depthoffield[0]);
            DepthOfFieldSetting("aperture," + depthoffield[1]);
            DepthOfFieldSetting("focallength," + depthoffield[2]);
            DepthOfFieldSetting("focusdistance," + depthoffield[3]);

            //---grain
            string[] grain = bkuplist[4].Split(",");
            EnablePostProcessing("grain," + grain[0]);
            GrainSetting("intensity," + grain[1]);
            GrainSetting("size," + grain[2]);

            //---vignette
            string[] vignette = bkuplist[5].Split(",");
            EnablePostProcessing("vignette," + vignette[0]);
            VignetteSetting("intensity," + vignette[1]);
            VignetteSetting("smoothness," + vignette[2]);
            VignetteSetting("roundness," + vignette[3]);
            VignetteSetting("color," + vignette[4]);
            VignetteSetting("center," + vignette[5] + "," + vignette[6]);

            //---motionblur
            string[] motionblur = bkuplist[6].Split(",");
            EnablePostProcessing("motionblur," + motionblur[0]);
            MotionBlurSetting("shutterangle," + motionblur[1]);
            MotionBlurSetting("samplecount," + motionblur[2]);


        }
        /// <summary>
        /// To pack all properties for HTML-UI
        /// </summary>
        /// <param name="type">t</param>
        public void GetIndicatedPropertyFromOuter(int type)
        {
            string ret = "";

            /*
            List<string> arr = new List<string>();
            arr.Add(
                GetEnablePostProcessing("bloom,0").ToString() + "," + 
                GetBloomSetting("intensity,0").ToString()
            );
            arr.Add(
                GetEnablePostProcessing("chromatic,0").ToString() + "," + 
                GetChromaticSetting("intensity,0")
            );
            arr.Add(
                GetEnablePostProcessing("colorgrading,0").ToString() + "," + 
                "#" + ColorUtility.ToHtmlStringRGBA(GetColorGradingSettingColor("colorfilter,0")) + "," + 
                GetColorGradingSettingFloat("temperature,0").ToString() + "," + 
                GetColorGradingSettingFloat("tint,0").ToString()
            );
            arr.Add(
                GetEnablePostProcessing("depthoffield,0").ToString() + "," + 
                GetDepthOfFieldSetting("aperture,0").ToString() + "," + 
                GetDepthOfFieldSetting("focallength,0").ToString() + "," + 
                GetDepthOfFieldSetting("focusdistance,0").ToString()
            );
            arr.Add(
                GetEnablePostProcessing("grain,0").ToString() + "," + 
                GetGrainSetting("intensity,0").ToString() + "," + 
                GetGrainSetting("size,0").ToString()
            );
            Vector2 v2 = GetVignetteSettingVector2("center,0");
            arr.Add(
                GetEnablePostProcessing("vignette,0").ToString() + "," + 
                GetVignetteSetting("intensity,0").ToString() + "," +
                GetVignetteSetting("smoothness,0").ToString() + "," +
                GetVignetteSetting("roundness,0").ToString() + "," +
                "#" + ColorUtility.ToHtmlStringRGBA(GetVignetteSettingColor("color,0")) + "," +
                v2.x.ToString() + "," + 
                v2.y.ToString()
            );
            arr.Add(
                GetEnablePostProcessing("motionblur,0").ToString() + "," + 
                GetMotionBlurSetting("shutterangle,0").ToString() + "," + 
                GetMotionBlurSetting("samplecount,0").ToString()
            );
            */
            //ret = String.Join("\t", arr);
            ret = String.Join("\t", bkuplist);

            //0 - Bloom(on/off, intensity)
            //1 - Chromatic(on/off, intensity)
            //2 - Color grading(on/off, colorfilter, temperature, tint)
            //3 - depth of field(on/off, aperture, focallength, focusdistance)
            //4 - Grain(on/off, intensity, size)
            //5 - Vignette(on/off, intensity, smoothness, roundness, color, center)
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
                Bloom st = null;// ppvol.profile.GetSetting<Bloom>();
                
                if (!ppvol.profile.TryGetSettings<Bloom>(out st))
                {
                    st = ScriptableObject.CreateInstance<Bloom>();
                    ppvol.profile.AddSettings(st);
                }
                
                
                st.enabled.Override(ena);
                
                

            }
            else if (effectName.ToLower() == "chromatic")
            {
                ChromaticAberration st = null;
                
                if (!ppvol.profile.TryGetSettings<ChromaticAberration>(out st))
                {
                    st = ScriptableObject.CreateInstance<ChromaticAberration>();
                    ppvol.profile.AddSettings(st);
                }
                st.enabled.Override(ena);
            }
            else if (effectName.ToLower() == "colorgrading")
            {
                ColorGrading st = null;
                if (!ppvol.profile.TryGetSettings<ColorGrading>(out st))
                {
                    st = ScriptableObject.CreateInstance<ColorGrading>();
                    ppvol.profile.AddSettings(st);
                }
                st.enabled.Override(ena);
            }
            else if (effectName.ToLower() == "depthoffield")
            {
                DepthOfField st = null;
                if (!ppvol.profile.TryGetSettings<DepthOfField>(out st))
                {
                    st = ScriptableObject.CreateInstance<DepthOfField>();
                    ppvol.profile.AddSettings(st);
                }
                st.enabled.Override(ena);
            }
            else if (effectName.ToLower() == "grain")
            {
                Grain st = null;
                if (!ppvol.profile.TryGetSettings<Grain>(out st))
                {
                    st = ScriptableObject.CreateInstance<Grain>();
                    ppvol.profile.AddSettings(st);
                }
                st.enabled.Override(ena);
            }
            else if (effectName.ToLower() == "motionblur")
            {
                MotionBlur st = null;
                if (!ppvol.profile.TryGetSettings<MotionBlur>(out st))
                {
                    st = ScriptableObject.CreateInstance<MotionBlur>();
                    ppvol.profile.AddSettings(st);
                }
                st.enabled.Override(ena);
            }
            else if (effectName.ToLower() == "vignette")
            {
                Vignette st = null;
                if (!ppvol.profile.TryGetSettings<Vignette>(out st))
                {
                    st = ScriptableObject.CreateInstance<Vignette>();
                    ppvol.profile.AddSettings(st);
                }
                st.enabled.Override(ena);
            }
            else if (effectName.ToLower() == "fxaa")
            {
                List<PostProcessLayer> arr_ppl = new List<PostProcessLayer>();
                arr_ppl.Add(FrontCamera.GetComponent<PostProcessLayer>());

                List<NativeAnimationAvatar> navs = manim.GetCastsByRoleType(AF_TARGETTYPE.Camera);
                navs.ForEach(nav =>
                {
                    arr_ppl.Add(nav.avatar.GetComponent<PostProcessLayer>());
                });

                //PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                arr_ppl.ForEach(ppl =>
                {
                    ppl.antialiasingMode = ena ? PostProcessLayer.Antialiasing.FastApproximateAntialiasing : PostProcessLayer.Antialiasing.None;
                });
                
            }
            else if (effectName.ToLower() == "smaa")
            {
                List<PostProcessLayer> arr_ppl = new List<PostProcessLayer>();
                arr_ppl.Add(FrontCamera.GetComponent<PostProcessLayer>());

                List<NativeAnimationAvatar> navs = manim.GetCastsByRoleType(AF_TARGETTYPE.Camera);
                navs.ForEach(nav =>
                {
                    arr_ppl.Add(nav.avatar.GetComponent<PostProcessLayer>());
                });

                //PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                arr_ppl.ForEach(ppl =>
                {
                    ppl.antialiasingMode = ena ? PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing : PostProcessLayer.Antialiasing.None;
                });

            }
            else if (effectName.ToLower() == "taa")
            {
                List<PostProcessLayer> arr_ppl = new List<PostProcessLayer>();
                arr_ppl.Add(FrontCamera.GetComponent<PostProcessLayer>());

                List<NativeAnimationAvatar> navs = manim.GetCastsByRoleType(AF_TARGETTYPE.Camera);
                navs.ForEach(nav =>
                {
                    arr_ppl.Add(nav.avatar.GetComponent<PostProcessLayer>());
                });

                //PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                arr_ppl.ForEach(ppl =>
                {
                    ppl.antialiasingMode = ena ? PostProcessLayer.Antialiasing.TemporalAntialiasing : PostProcessLayer.Antialiasing.None;
                });
                
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
                Bloom st = null;
                if (ppvol.profile.TryGetSettings<Bloom>(out st))
                {
                    ret.Add(st.intensity.value);
                }
                
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
                ret.Add(st.focusDistance.value);
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
                ret.Add(st.smoothness.value);
                ret.Add(st.roundness.value);
                ret.Add(st.color.value.r);
                ret.Add(st.color.value.g);
                ret.Add(st.color.value.b);
                ret.Add(st.color.value.a);
                ret.Add(st.center.value.x);
                ret.Add(st.center.value.y);
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
                string enastr = bkuplist[0].Split(',')[0];
                Bloom st = null;
                
                if (ppvol.profile.TryGetSettings<Bloom>(out st))
                {
                    if (useDOTween)
                    {
                        if (st.enabled.value != isEnable)
                        {
                            seq.Join(DOVirtual.DelayedCall(duration, () =>
                            {
                                st.enabled.Override(isEnable);
                                SetEffectValues(seq, effectName, values, isEnable, useDOTween, duration);

                            }, false));
                        }
                        
                        if (values.Count > 0)
                        {
                            seq.Join(DOTween.To(() => st.intensity.value, x => st.intensity.Override(x), values[0], duration));

                        }
                    }
                    else
                    {
                        
                        if (isEnable)
                        {
                            st.intensity.Override(values[0]);
                        }
                    }
                    
                }

                
                
            }
            else if (effectName.ToLower() == "chromatic")
            {
                ChromaticAberration st = ppvol.profile.GetSetting<ChromaticAberration>();

                if (useDOTween)
                {
                    if (st.enabled.value != isEnable)
                    {
                        seq.Join(DOVirtual.DelayedCall(duration, () =>
                        {
                            st.enabled.Override(isEnable);
                            SetEffectValues(seq, effectName, values, isEnable, useDOTween, duration);

                        }, false));
                    }
                    if (values.Count > 0)
                    {
                        seq.Join(DOTween.To(() => st.intensity.value, x => st.intensity.Override(x), values[0], duration));

                    }
                }
                else
                {
                    st.enabled.Override(isEnable);
                    if (isEnable) st.intensity.Override(values[0]);
                }
                
                
                
            }
            else if (effectName.ToLower() == "colorgrading")
            {
                ColorGrading st = ppvol.profile.GetSetting<ColorGrading>();

                if (useDOTween)
                {
                    if (st.enabled.value != isEnable)
                    {
                        seq.Join(DOVirtual.DelayedCall(duration, () =>
                        {
                            st.enabled.Override(isEnable);
                            SetEffectValues(seq, effectName, values, isEnable, useDOTween, duration);

                        }, false));
                    }
                    if (values.Count > 5)
                    {
                        seq.Join(DOTween.To(() => st.temperature.value, x => st.temperature.Override(x), values[0], duration));


                        seq.Join(DOTween.To(() => st.tint.value, x => st.tint.Override(x), values[1], duration));


                        Color col = new Color(values[2], values[3], values[4], values[5]);
                        seq.Join(DOTween.To(() => st.colorFilter.value, x => st.colorFilter.Override(x), col, duration));

                    }
                }
                else
                {
                    if (isEnable)
                    {
                        st.temperature.Override(values[0]);

                        st.tint.Override(values[1]);

                        Color col = new Color(values[2], values[3], values[4], values[5]);
                        st.colorFilter.Override(col);
                    }
                }
                
            }
            else if (effectName.ToLower() == "depthoffield")
            {
                DepthOfField st = ppvol.profile.GetSetting<DepthOfField>();

                if (useDOTween)
                {
                    if (st.enabled.value != isEnable)
                    {
                        seq.Join(DOVirtual.DelayedCall(duration, () =>
                        {
                            st.enabled.Override(isEnable);
                            SetEffectValues(seq, effectName, values, isEnable, useDOTween, duration);

                        }, false));
                    }
                    if (values.Count > 2)
                    {
                        seq.Join(DOTween.To(() => st.aperture.value, x => st.aperture.Override(x), values[0], duration));


                        seq.Join(DOTween.To(() => st.focalLength.value, x => st.focalLength.Override(x), values[1], duration));


                        if (values.Count > 2)
                        { //---add 2023.05.05
                            seq.Join(DOTween.To(() => st.focusDistance.value, x => st.focusDistance.Override(x), values[2], duration));

                        }

                    }
                }
                else
                {
                    if (isEnable)
                    {
                        st.aperture.Override(values[0]);

                        st.focalLength.Override(values[1]);

                        if (values.Count > 2)
                        { //---add 2023.05.05
                            st.focusDistance.Override(values[2]);
                        }

                    }
                }
                

            }
            else if (effectName.ToLower() == "grain")
            {
                Grain st = ppvol.profile.GetSetting<Grain>();

                if (useDOTween)
                {
                    if (st.enabled.value != isEnable)
                    {
                        seq.Join(DOVirtual.DelayedCall(duration, () =>
                        {
                            st.enabled.Override(isEnable);
                            SetEffectValues(seq, effectName, values, isEnable, useDOTween, duration);

                        }, false));
                    }
                    if (values.Count > 1)
                    {
                        seq.Join(DOTween.To(() => st.intensity.value, x => st.intensity.Override(x), values[0], duration));


                        seq.Join(DOTween.To(() => st.size.value, x => st.size.Override(x), values[1], duration));
                    }
                }
                else
                {
                    if (isEnable)
                    {
                        st.intensity.Override(values[0]);

                        st.size.Override(values[1]);

                    }
                }

            }
            else if (effectName.ToLower() == "motionblur")
            {
                MotionBlur st = ppvol.profile.GetSetting<MotionBlur>();

                if (useDOTween)
                {
                    if (st.enabled.value != isEnable)
                    {
                        seq.Join(DOVirtual.DelayedCall(duration, () =>
                        {
                            st.enabled.Override(isEnable);
                            SetEffectValues(seq, effectName, values, isEnable, useDOTween, duration);

                        }, false));
                    }
                    if (values.Count > 1)
                    {
                        seq.Join(DOTween.To(() => st.shutterAngle.value, x => st.shutterAngle.Override(x), values[0], duration));


                        seq.Join(DOTween.To(() => (int)st.sampleCount.value, x => st.sampleCount.Override((int)x), values[1], duration));


                    }
                }
                else
                {
                    if (isEnable)
                    {
                        st.shutterAngle.Override(values[0]);

                        st.sampleCount.Override((int)values[1]);

                    }
                }
            }
            else if (effectName.ToLower() == "vignette")
            {
                Vignette st = ppvol.profile.GetSetting<Vignette>();

                if (useDOTween)
                {
                    if (st.enabled.value != isEnable)
                    {
                        seq.Join(DOVirtual.DelayedCall(duration, () =>
                        {
                            st.enabled.Override(isEnable);
                            SetEffectValues(seq, effectName, values, isEnable, useDOTween, duration);

                        }, false));
                    }
                    if (values.Count > 8)
                    {
                        seq.Join(DOTween.To(() => st.intensity.value, x => st.intensity.Override(x), values[0], duration));


                        if (values.Count > 8)
                        { //---add 2023.05.05
                            seq.Join(DOTween.To(() => st.smoothness.value, x => st.smoothness.Override(x), values[1], duration));


                            seq.Join(DOTween.To(() => st.roundness.value, x => st.roundness.Override(x), values[2], duration));


                            Color col = new Color(values[3], values[4], values[5], values[6]);

                            seq.Join(DOTween.To(() => st.color.value.r, x => st.color.value.r = x, col.r, duration));
                            seq.Join(DOTween.To(() => st.color.value.g, x => st.color.value.g = x, col.g, duration));
                            seq.Join(DOTween.To(() => st.color.value.b, x => st.color.value.b = x, col.b, duration));
                            seq.Join(DOTween.To(() => st.color.value.a, x => st.color.value.a = x, col.a, duration));


                            Vector2 v2 = new Vector2(values[7], values[8]);

                            seq.Join(DOTween.To(() => st.center.value.x, x => st.center.value.x = v2.x, v2.x, duration));
                            seq.Join(DOTween.To(() => st.center.value.y, x => st.center.value.y = v2.y, v2.y, duration));

                        }
                    }
                }
                else
                {
                    if (isEnable)
                    {
                        st.intensity.Override(values[0]);

                        if (values.Count > 1)
                        { //---add 2023.05.05
                            st.smoothness.Override(values[1]);

                            st.roundness.Override(values[2]);

                            Color col = new Color(values[3], values[4], values[5], values[6]);
                            st.color.Override(col);

                            Vector2 v2 = new Vector2(values[7], values[8]);
                            st.center.Override(v2);
                        }


                    }
                }
            }
            else if (effectName.ToLower() == "fxaa")
            {
                List<PostProcessLayer> arr_ppl = new List<PostProcessLayer>();
                arr_ppl.Add(FrontCamera.GetComponent<PostProcessLayer>());

                List<NativeAnimationAvatar> navs = manim.GetCastsByRoleType(AF_TARGETTYPE.Camera);
                navs.ForEach(nav =>
                {
                    arr_ppl.Add(nav.avatar.GetComponent<PostProcessLayer>());
                });

                //PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                arr_ppl.ForEach(ppl =>
                {
                    ppl.antialiasingMode = isEnable ? PostProcessLayer.Antialiasing.FastApproximateAntialiasing : PostProcessLayer.Antialiasing.None;
                    if (isEnable)
                    {
                        ppl.fastApproximateAntialiasing.fastMode = values[0] == 1f ? true : false;
                    }
                });
                

            }
            else if (effectName.ToLower() == "smaa")
            {
                List<PostProcessLayer> arr_ppl = new List<PostProcessLayer>();
                arr_ppl.Add(FrontCamera.GetComponent<PostProcessLayer>());

                List<NativeAnimationAvatar> navs = manim.GetCastsByRoleType(AF_TARGETTYPE.Camera);
                navs.ForEach(nav =>
                {
                    arr_ppl.Add(nav.avatar.GetComponent<PostProcessLayer>());
                });

                //PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                arr_ppl.ForEach(ppl =>
                {
                    ppl.antialiasingMode = isEnable ? PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing : PostProcessLayer.Antialiasing.None;
                    if (isEnable)
                    {
                        ppl.subpixelMorphologicalAntialiasing.quality = (SubpixelMorphologicalAntialiasing.Quality)values[0];
                    }
                });
                

            }
            else if (effectName.ToLower() == "taa")
            {
                List<PostProcessLayer> arr_ppl = new List<PostProcessLayer>();
                arr_ppl.Add(FrontCamera.GetComponent<PostProcessLayer>());

                List<NativeAnimationAvatar> navs = manim.GetCastsByRoleType(AF_TARGETTYPE.Camera);
                navs.ForEach(nav =>
                {
                    arr_ppl.Add(nav.avatar.GetComponent<PostProcessLayer>());
                });

                //PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                arr_ppl.ForEach(ppl =>
                {
                    ppl.antialiasingMode = isEnable ? PostProcessLayer.Antialiasing.TemporalAntialiasing : PostProcessLayer.Antialiasing.None;
                    if (isEnable)
                    {
                        ppl.temporalAntialiasing.sharpness = values[0];
                    }
                });
                

            }

            return seq;
        }
        public void SetEffectBackup(string effectName, List<float> values, bool isEnable)
        {
            //List<string> efarr = new List<string>(new string[] { "bloom", "chromatic", "colorgrading", "depthoffield", "grain", "vignette", "motionblur" });
            //int hit = efarr.FindIndex(mat => { return mat == effectName; });

            string enableStr = (isEnable ? "1" : "0");
            string vals = "";
            if (effectName.ToLower() == "bloom")
            {
                if (values.Count > 0)
                {
                    vals = enableStr + "," + values[0].ToString();
                    bkuplist[0] = vals;
                }
                
            }
            else if (effectName.ToLower() == "chromatic")
            {
                if (values.Count > 0)
                {
                    vals = enableStr + "," + values[0].ToString();
                    bkuplist[1] = vals;
                }
                
            }
            else if (effectName.ToLower() == "colorgrading")
            {
                if (values.Count > 5)
                {
                    Color col = new Color(values[2], values[3], values[4], values[5]);
                    vals = enableStr + "," +
                    "#" + ColorUtility.ToHtmlStringRGBA(col) + "," +
                    values[0].ToString() + "," +
                    values[1].ToString();

                    bkuplist[2] = vals;
                }
                
            }
            else if (effectName.ToLower() == "depthoffield")
            {
                if (values.Count > 2)
                {
                    vals = enableStr + "," + values[0].ToString() + "," + values[1].ToString() + "," + values[2].ToString();

                    bkuplist[3] = vals;
                }
                
            }
            else if (effectName.ToLower() == "grain")
            {
                if (values.Count > 1)
                {
                    vals = enableStr + "," + values[0].ToString() + "," + values[1].ToString();

                    bkuplist[4] = vals;
                }
                
            }
            else if (effectName.ToLower() == "vignette")
            {
                if (values.Count > 8)
                {
                    Color col = new Color(values[3], values[4], values[5], values[6]);

                    vals = enableStr + "," +
                    values[0].ToString().ToString() + "," +
                    values[1].ToString().ToString() + "," +
                    values[2].ToString().ToString() + "," +
                    "#" + ColorUtility.ToHtmlStringRGBA(col) + "," +
                    values[7].ToString() + "," + values[8].ToString();

                    bkuplist[5] = vals;
                }
                
            }
            else if (effectName.ToLower() == "motionblur")
            {
                if (values.Count > 1)
                {
                    vals = enableStr + "," + values[0].ToString() + "," + values[1].ToString();

                    bkuplist[6] = vals;
                }
                
            }
            
        }


        public void BloomSetting(string param)
        {
            string[] prm = param.Split(',');
            string name = prm[0];
            float v = float.TryParse(prm[1], out v) ? v : 0f;
            Bloom st = null;// ppvol.profile.GetSetting<Bloom>();
            if (ppvol.profile.TryGetSettings<Bloom>(out st))
            {
                if (name.ToLower() == "intensity")
                {
                    st.intensity.Override(v);
                    //st.intensity.value = v;
                }

            }
        }
        public float GetBloomSetting(string param)
        {
            float ret = 0f;
            string[] prm = param.Split(',');
            string name = prm[0];
            bool is_contacthtml = prm[1] == "1" ? true : false;
            Bloom st = null;
            
            if (ppvol.profile.TryGetSettings<Bloom>(out st))
            {

                if (name.ToLower() == "intensity")
                {
                    ret = st.intensity.value;
                }
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
            else if (name.ToLower() == "focusdistance")
            {
                st.focusDistance.Override(v);
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
            else if (name.ToLower() == "focusdistance")
            {
                ret = st.focusDistance.value;
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
            Color col = ColorUtility.TryParseHtmlString(prm[1], out col) ? col : Color.black;

            Vector2 v2 = new Vector2(0.5f, 0.5f);
            if (prm.Length > 2)
            {
                float vy = float.TryParse(prm[2], out vy) ? vy : 0.5f;
                v2.x = v;
                v2.y = vy;
            }
            
            Vignette st = ppvol.profile.GetSetting<Vignette>();
            if (name.ToLower() == "intensity")
            {
                st.intensity.Override(v);
            }
            else if (name.ToLower() == "smoothness")
            {
                st.smoothness.Override(v);
            }
            else if (name.ToLower() == "roundness")
            {
                st.roundness.Override(v);
            }            
            else if (name.ToLower() == "color")
            {
                st.color.Override(col);
            }
            else if (name.ToLower() == "center")
            {
                st.center.Override(v2);
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
            else if (name.ToLower() == "smoothness")
            {
                ret = st.smoothness.value;
            }
            else if (name.ToLower() == "roundness")
            {
                ret = st.roundness.value;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml) ReceiveFloatVal(ret);
#endif
            return ret;
        }
        public Color GetVignetteSettingColor(string param)
        {
            Color ret = Color.black;
            Vignette st = ppvol.profile.GetSetting<Vignette>();

            string[] prm = param.Split(',');
            string name = prm[0];
            bool is_contacthtml = prm[1] == "1" ? true : false;

            if (name.ToLower() == "color")
            {
                ret = st.color.value;
            }
            string js = "#" + ColorUtility.ToHtmlStringRGBA(ret);
#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml) ReceiveStringVal(js);
#endif
            return ret;
        }
        public Vector2 GetVignetteSettingVector2(string param)
        {
            Vector2 ret = Vector2.zero;

            Vignette st = ppvol.profile.GetSetting<Vignette>();

            string[] prm = param.Split(',');
            string name = prm[0];
            bool is_contacthtml = prm[1] == "1" ? true : false;

            if (name.ToLower() == "center")
            {
                ret = st.center.value;
            }
            string js = JsonUtility.ToJson(ret);
#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml) ReceiveStringVal(js);
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

            List<PostProcessLayer> arr_ppl = new List<PostProcessLayer>();
            arr_ppl.Add(FrontCamera.GetComponent<PostProcessLayer>());

            List <NativeAnimationAvatar> navs =  manim.GetCastsByRoleType(AF_TARGETTYPE.Camera);
            navs.ForEach(nav =>
            {
                arr_ppl.Add(nav.avatar.GetComponent<PostProcessLayer>());
            });

            arr_ppl.ForEach(ppl =>
            {
                //PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
                if (ppl.antialiasingMode == PostProcessLayer.Antialiasing.FastApproximateAntialiasing)
                {
                    if (name.ToLower() == "fastmode")
                    {
                        ppl.fastApproximateAntialiasing.fastMode = prm[2] == "1" ? true : false;
                    }
                    else if (name.ToLower() == "keepalpha")
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
            });

            
        }
        public float GetAntiAlias(string param)
        {
            float ret = 0f;

            string[] prm = param.Split(',');
            string name = prm[0];
            bool is_contacthtml = prm[1] == "1" ? true : false;


            PostProcessLayer ppl = FrontCamera.GetComponent<PostProcessLayer>();
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
