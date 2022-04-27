using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using DG.Tweening;
using VRM;

namespace UserHandleSpace
{
    public class OperateLoadedWindzone : OperateLoadedBase
    {
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);

        private ManageAnimation manim;

        public WindZone wind;
        private float[] wind_durations = { 0.01f, 0.02f };
        private float tmp_windpower = 0f;
        private float old_tmp_windpower = 0f;
        private string[] exWindBoneComment = { "Bust" };
        private string[] exWindBoneName = { "Bust" };

        // Start is called before the first frame update
        void Start()
        {
            manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
        }

        // Update is called once per frame
        void Update()
        {
            if (old_tmp_windpower != 0)
            {
                DOVirtual.DelayedCall(UnityEngine.Random.Range(wind_durations[0], wind_durations[1]), () =>
                {
                    tmp_windpower = wind.windMain * UnityEngine.Random.Range(wind.windPulseFrequency * 0, wind.windPulseFrequency);
                    ApplyAllVRMSpringBone();
                });
            }
            old_tmp_windpower = wind.windMain;
            
            
        }
        public void GetIndicatedPropertyFromOuter()
        {
            string ret = windPower.ToString() + "," + windFrequency.ToString() + "," + windDurationMin.ToString() + "," + windDurationMax.ToString();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        //----------------------------------------------------------------------
        public float windDurationMin
        {
            set
            {
                wind_durations[0] = value;
            }
            get
            {
                return wind_durations[0];
            }
        }
        public float windDurationMax
        {
            set
            {
                wind_durations[1] = value;
            }
            get
            {
                return wind_durations[1];
            }
        }
        public void SetWindDurationMin(float value)
        {
            wind_durations[0] = value;
        }
        public void SetWindDurationMax(float value)
        {
            wind_durations[1] = value;
        }
        public void GetWindDurations()
        {
            string js = wind_durations[0].ToString() + "," + wind_durations[1].ToString();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }
        //-----------------------------------------------------------
        public float windPower
        {
            set
            {
                wind.windMain = value;
            }
            get
            {
                return wind.windMain;
            }
        }
        public void GetWindPowerFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(wind.windMain);
#endif
        }
        public void SetWindPowerFromOuter(float value)
        {
            wind.windMain = value;
        }
        //---------------------------------------------------------------
        public float windFrequency
        {
            set
            {
                wind.windPulseFrequency = value;
            }
            get
            {
                return wind.windPulseFrequency;
            }
        }
        public void GetWindFrequencyFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(wind.windPulseFrequency);
#endif
        }
        public void SetWindFrequency(float value)
        {
            wind.windPulseFrequency = value;
        }
        //-------------------------------------------------------------------
        public void SetWindDir(Vector3 dir)
        {
            transform.Rotate(dir);
        }
        public void SetWindDirHorizontal(float value)
        {
            transform.Rotate(transform.rotation.eulerAngles.x, value, transform.rotation.eulerAngles.z);
        }
        public void SetWindDirVertical(float value)
        {
            transform.Rotate(value, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
        public Vector3 GetWindDir()
        {
            return transform.rotation.eulerAngles;
        }
        public void GetWindDirFromOuter()
        {
            string js = JsonUtility.ToJson(transform.rotation.eulerAngles);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }
        //------------------------------------------------------------------------
        public void ResetAllVRMSpringBone()
        {
            foreach (NativeAnimationAvatar cast in manim.currentProject.casts)
            {
                if (cast.type == AF_TARGETTYPE.VRM)
                {
                    VRMSpringBone[] vsbones = cast.avatar.transform.GetComponentsInChildren<VRMSpringBone>();
                    foreach (VRMSpringBone bone in vsbones)
                    {
                        bone.m_gravityPower = 0;
                        bone.m_gravityDir = new Vector3(0, -1, 0);
                    }
                }

            }
        }
        private void ApplyAllVRMSpringBone()
        {
            if (wind.windMain == 0)
            {
                ResetAllVRMSpringBone();
                return;
            }

            foreach (NativeAnimationAvatar cast in manim.currentProject.casts)
            {
                if (cast.type == AF_TARGETTYPE.VRM)
                {
                    VRMSpringBone[] vsbones = cast.avatar.transform.GetComponentsInChildren<VRMSpringBone>();
                    foreach (VRMSpringBone bone in vsbones)
                    {
                        bool ishit = false;
                        foreach(string com in exWindBoneComment)
                        {
                            if (com == bone.m_comment)
                            {
                                ishit = true;
                                break;
                            }
                        }
                        if (!ishit)
                        {
                            foreach (string name in exWindBoneName)
                            {
                                if (bone.RootBones[0].gameObject.name.IndexOf(name) > -1)
                                {
                                    ishit = true;
                                    break;
                                }
                            }
                        }
                        
                        if (!ishit)
                        {
                            /*
                             * VRM  x:0, y:-1, z:0
                             * Wind x:0, y:0,  z:0
                             * 
                             */
                            bone.m_gravityPower = tmp_windpower;

                            //---Wind Y -> VRM X, Z
                            float forZ = 1f - (wind.transform.rotation.eulerAngles.y / 90f);

                            if (wind.transform.rotation.eulerAngles.y > 180f)
                            {
                                forZ = ((wind.transform.rotation.eulerAngles.y - 180f) / 90f) - 1f;
                            }

                            float rotX = wind.transform.rotation.eulerAngles.y - 90f;
                            if (rotX < 0) rotX *= -1;

                            float forX = 1f - (rotX / 90f);
                            if (rotX > 180f)
                            {
                                forX = ((rotX - 180f) / 90f) - 1f;
                            }

                            bone.m_gravityDir.x = forX;
                            bone.m_gravityDir.z = forZ;

                            //---Wind X -> VRM Y
                            float rotY = wind.transform.rotation.eulerAngles.x + 90f;
                            if (rotY > 360f)
                            {
                                rotY = (rotY - 90f) * -1f; //---effective > 270
                            }
                            if (rotY > 270f) rotY = 90f;  //---effective > 360

                            float forY = 1f - (rotY / 90f);
                            if (rotY > 180f)
                            {
                                forY = ((rotY - 180f) / 90f) - 1f;
                            }
                            bone.m_gravityDir.y = forY;
                        }
                        

                    }
                }
                
            }
        }
    }
}

