using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using DG.Tweening;

using UniGLTF;
using UniHumanoid;
using UniVRM10;

namespace UserHandleSpace
{


    /// <summary>
    /// Convertion DB Item for HumanBodyBones to Muscle 
    /// </summary>
    [Serializable]
    public class VVMMotionMuscleDBItem
    {
        public HumanBodyBones hbb;
        public string vector;
        public string muscleName;
        public float minRotation;
        public float maxRotation;

        public float ConvertFloat (float rotation)
        {
            float ret = 0;

            if (rotation > 0)
            {
                ret = rotation / maxRotation;
            }
            else
            {
                ret = rotation / minRotation;
            }

            return ret;
        }
    }
    public class VVMMotionMuscleDB
    {
        public static string[] CNS_VVMMuscleCSV =
        {
            "7,Z,Spine Front-Back,-40,40",
            "7,Y,Spine Left-Right,-40,40",
            "7,X,Spine Twist Left-Right,-40,40",
            "8,Z,Chest Front-Back,-40,40",
            "8,Y,Chest Left-Right,-40,40",
            "8,X,Chest Twist Left-Right,-40,40",
            "54,Z,UpperChest Front-Back,-20,20",
            "54,Y,UpperChest Left-Right,-20,20",
            "54,X,UpperChest Twist Left-Right,-20,20",
            "9,Z,Neck Nod Down-Up,-40,40",
            "9,Y,Neck Tilt Left-Right,-40,40",
            "9,X,Neck Turn Left-Right,-40,40",
            "10,Z,Head Nod Down-Up,-40,40",
            "10,Y,Head Tilt Left-Right,-40,40",
            "10,X,Head Turn Left-Right,-40,40",
            "21,Z,Left Eye Down-Up,-10,15",
            "21,X,Left Eye In-Out,-20,20",
            "22,Z,Right Eye Down-Up,-10,15",
            "22,X,Right Eye In-Out,-20,20",
            "23, ,Jaw Close,-1,-1",
            "23, ,Jaw Left-Right,-1,-1",
            "1,X,Left Upper Leg Front-Back,-90,50",
            "1,Y,Left Upper Leg In-Out,-60,60",
            "1,X,Left Upper Leg Twist In-Out,-60,60",
            "3,Z,Left Lower Leg Stretch,-80,80",
            "3,X,Left Lower Leg Twist In-Out,-90,90",
            "5,Z,Left Foot Up-Down,-50,50",
            "5,X,Left Foot Twist In-Out,-30,30",
            "19,Z,Left Toes Up-Down,-50,50",
            "2,X,Right Upper Leg Front-Back,-90,50",
            "2,Y,Right Upper Leg In-Out,-60,60",
            "2,X,Right Upper Leg Twist In-Out,-60,60",
            "4,Z,Right Lower Leg Stretch,-80,80",
            "4,X,Right Lower Leg Twist In-Out,-90,90",
            "6,Z,Right Foot Up-Down,-50,50",
            "6,X,Right Foot Twist In-Out,-30,30",
            "20,Z,Right Toes Up-Down,-50,50",
            "11,Z,Left Shoulder Down-Up,-15,30",
            "11,Y,Left Shoulder Front-Back,-15,15",
            "13,Z,Left Arm Down-Up,-60,100",
            "13,Y,Left Arm Front-Back,-100,100",
            "13,X,Left Arm Twist In-Out,-90,90",
            "15,Z,Left Forearm Stretch,-80,80",
            "15,X,Left Forearm Twist In-Out,-90,90",
            "17,Z,Left Hand Down-Up,-80,80",
            "17,Y,Left Hand In-Out,-40,40",
            "12,Z,Right Shoulder Down-Up,-15,30",
            "12,Y,Right Shoulder Front-Back,-15,15",
            "14,Z,Right Arm Down-Up,-60,100",
            "14,Y,Right Arm Front-Back,-100,100",
            "14,X,Right Arm Twist In-Out,-90,90",
            "16,Z,Right Forearm Stretch,-80,80",
            "16,X,Right Forearm Twist In-Out,-90,90",
            "18,Z,Right Hand Down-Up,-80,80",
            "18,Y,Right Hand In-Out,-40,40",
            "24,Z,Left Thumb 1 Stretched,-20,20",
            "24,Y,Left Thumb Spread,-25,25",
            "25,Z,Left Thumb 2 Stretched,-40,35",
            "26,Z,Left Thumb 3 Stretched,-40,35",
            "27,Z,Left Index 1 Stretched,-50,50",
            "27,Y,Left Index Spread,-20,20",
            "28,Z,Left Index 2 Stretched,-45,45",
            "29,Z,Left Index 3 Stretched,-45,45",
            "30,Z,Left Middle 1 Stretched,-50,50",
            "30,Y,Left Middle Spread,-7.5,7.5",
            "31,Z,Left Middle 2 Stretched,-45,45",
            "32,Z,Left Middle 3 Stretched,-45,45",
            "33,Z,Left Ring 1 Stretched,-50,50",
            "33,Y,Left Ring Spread,-7.5,7.5",
            "34,Z,Left Ring 2 Stretched,-45,45",
            "35,Z,Left Ring 3 Stretched,-45,45",
            "36,Z,Left Little 1 Stretched,-50,50",
            "36,Y,Left Little Spread,-20,20",
            "37,Z,Left Little 2 Stretched,-45,45",
            "38,Z,Left Little 3 Stretched,-45,45",
            "39,Z,Right Thumb 1 Stretched,-20,20",
            "39,Y,Right Thumb Spread,-25,25",
            "40,Z,Right Thumb 2 Stretched,-40,35",
            "41,Z,Right Thumb 3 Stretched,-40,35",
            "42,Z,Right Index 1 Stretched,-50,50",
            "42,Y,Right Index Spread,-20,20",
            "43,Z,Right Index 2 Stretched,-45,45",
            "44,Z,Right Index 3 Stretched,-45,45",
            "45,Z,Right Middle 1 Stretched,-50,50",
            "45,Y,Right Middle Spread,-7.5,7.5",
            "46,Z,Right Middle 2 Stretched,-45,45",
            "47,Z,Right Middle 3 Stretched,-45,45",
            "48,Z,Right Ring 1 Stretched,-50,50",
            "48,Y,Right Ring Spread,-7.5,7.5",
            "49,Z,Right Ring 2 Stretched,-45,45",
            "50,Z,Right Ring 3 Stretched,-45,45",
            "51,Z,Right Little 1 Stretched,-50,50",
            "51,Y,Right Little Spread,-20,20",
            "52,Z,Right Little 2 Stretched,-45,45",
            "53,Z,Right Little 3 Stretched,-45,45"
        };

        public List<VVMMotionMuscleDBItem> items;

        public void Load(string[] datas)
        {
            items = new List<VVMMotionMuscleDBItem>();

            for (int i = 0; i < datas.Length; i++)
            {
                string dt = datas[i];
                string[] arr = dt.Split(",");

                VVMMotionMuscleDBItem dbitem = new VVMMotionMuscleDBItem();

                int hbb = int.TryParse(arr[0], out hbb) ? hbb : -1;
                dbitem.hbb = (HumanBodyBones)hbb;
                dbitem.vector = arr[1];
                dbitem.muscleName = arr[2];
                float minr = float.TryParse(arr[3], out minr) ? minr : -1;
                dbitem.minRotation = minr;
                float maxr = float.TryParse(arr[4], out maxr) ? maxr : -1;
                dbitem.maxRotation = maxr;

                items.Add(dbitem);
            }
        }
        public VVMMotionMuscleDBItem Search(HumanBodyBones hbb, string vector)
        {
            VVMMotionMuscleDBItem ret = null;
            items.Find(match =>
            {
                if ((match.hbb == hbb) && (match.vector == vector)) return true;
                return false;
            });
            return ret;
        }
    }
    [Serializable]
    public class VVMDummyAnimCurveItem
    {
        public float serializedVersion = 3;
        public float time = 0;
        public float value = 0f;
        public float inSlope = 0;
        public float outSlope = 0;
        public float tangentMode = 0;
        public float weightedMode = 0;
        public float inWeight = 0;
        public float outWeight = 0;
    }
    [Serializable]
    public class VVMDummyAnimCurve
    {
        public int serializedVersion = 2;
        public List<VVMDummyAnimCurveItem> m_Curve = new List<VVMDummyAnimCurveItem>();
        public int m_PreInfinity = 2;
        public int m_PostInfinity = 2;
        public int m_RotationOrder = 4;

        public VVMDummyAnimCurve()
        {
            m_Curve = new List<VVMDummyAnimCurveItem>();
        }
    }
    [Serializable]
    public class VVMDummyIDObject
    {
        public int fileID = 0;
    }

    /// <summary>
    /// Serialized main body of AnimationCurve of text file version
    /// </summary>
    [Serializable]
    public class VVMDummyFloatAnimCurve
    {
        public VVMDummyAnimCurve curve = new VVMDummyAnimCurve();
        public string attribute = "";
        public string path = "";
        public int classID = 95;
        public VVMDummyIDObject script = new VVMDummyIDObject();
        public int[] pptrCurveMapping = new int[] { };
    }
    [Serializable]
    public class VVMDummyAnimationClipSettings 
    {
        public int serializedVersion = 2;
        public VVMDummyIDObject m_AdditiveReferencePoseClip = new VVMDummyIDObject();
        public float m_AdditiveReferencePoseTime = 0;
        public float m_StartTime = 0;
        public float m_StopTime = 0;
        public float m_OrientationOffsetY = 0;
        public float m_Level = 0;
        public float m_CycleOffset = 0;
        public float m_HasAdditiveReferencePose = 0;
        public float m_LoopTime = 0;
        public float m_LoopBlend = 0;
        public float m_LoopBlendOrientation = 0;
        public float m_LoopBlendPositionY = 0;
        public float m_LoopBlendPositionXZ = 0;
        public float m_KeepOriginalOrientation = 0;
        public float m_KeepOriginalPositionY = 0;
        public float m_KeepOriginalPositionXZ = 0;
        public float m_HeightFromFeet = 0;
        public float m_Mirror = 0;
    }
    [Serializable]
    public class VVMDummyAnimationClip
    {
        public int m_ObjectHideFlags = 0;
        public VVMDummyIDObject m_CorrespondingSourceObject = new VVMDummyIDObject();
        public VVMDummyIDObject m_PrefabInstance = new VVMDummyIDObject();
        public VVMDummyIDObject m_PrefabAsset = new VVMDummyIDObject();
        public string m_Name = "";
        public int serializedVersion = 6;
        public int m_Legacy = 0;
        public int m_Compressed = 0;
        public int m_UseHighQualityCurve = 1;
        public int[] m_RotationCurves = new int[] { };
        public int[] m_CompressedRotationCurves = new int[] { };
        public int[] m_EulerCurves = new int[] { };
        public int[] m_PositionCurves = new int[] { };
        public int[] m_ScaleCurves = new int[] { };
        public List<VVMDummyFloatAnimCurve> m_FloatCurves = new List<VVMDummyFloatAnimCurve>();
        public VVMDummyAnimationClipSettings m_AnimationClipSettings = new VVMDummyAnimationClipSettings();
        public int[] m_EditorCurves = new int[] { };
        public int[] m_EulerEditorCurves = new int[] { };
        public int m_HasGenericRootTransform = 0;
        public int m_HasMotionFloatCurves = 0;
        public string[] m_Events =  new string[] { };

    }
    [Serializable]
    public class VVMDummyVrmaOutput
    {
        public byte[] data;
    }


    //==================================================================================================================================================//

    public class VVMTransform
    {
        public HumanBodyBones bone;
        public Vector3 position;
        public Vector3 rotation;
        

        public VVMTransform()
        {
            bone = HumanBodyBones.Hips;
            position = new Vector3();
            rotation = new Vector3();
            
        }
        public VVMTransform(HumanBodyBones bone, Vector3 pos, Vector3 rot)
        {
            this.bone = bone;
            position = pos;
            rotation = rot;
        }
    }
    public class VVMMotionRecorderFrame
    {
        public int frameIndex;
        public float duration;
        public Ease ease;
        public List<VVMTransform> transforms;
        public List<float> muscles;
        public VVMMotionRecorderFrame()
        {
            frameIndex = 0;
            duration = 0f;
            ease = Ease.Linear;
            transforms = new List<VVMTransform>();
            muscles = new List<float>();
        }
        public VVMMotionRecorderFrame(int frame)
        {
            frameIndex = frame;
            duration = 0f;
            ease = Ease.Linear;
            transforms = new List<VVMTransform>();
            muscles = new List<float>();
        }
    }
    public class VVMAnimationCurve
    {
        public Dictionary<string, AnimationCurve> curve;
        public VVMAnimationCurve()
        {
            curve["p.x"] = new AnimationCurve();
            curve["p.y"] = new AnimationCurve();
            curve["p.z"] = new AnimationCurve();
            curve["r.x"] = new AnimationCurve();
            curve["r.y"] = new AnimationCurve();
            curve["r.z"] = new AnimationCurve();
            curve["r.w"] = new AnimationCurve();
        }
    }
    //==================================================================================================================================================//
    

    //#################################################################################################
    /// <summary> 
    /// Animation Clip expoter 
    /// </summary>
    public class VVMMotionRecorder : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);

        //Target object: attached VRM, OtherObject self.
        public AF_TARGETTYPE TargetType;

        public List<VVMMotionRecorderFrame> BodyBonesCurves;

        //public VVMMotionMuscleDB nameDB;

        /*
         * Specification of Motion
         * 1-keyframe : {
         *   time : float,
         *   value : float 
         * }
         * 
         * 1-curve : n-keyframe of each HumanBodyBones.position/rotation.x/y/w/z
         *   hip  : position, rotation
         *   other: rotation
         * 
         * 1-clip : n-curve (All of each Vector3,Quaternion of each HumanBodyBones
         * 
         */
        public AnimationClip clip;

        //===VRMAnimation
        //ExportingGltfData exportingGltfData;
        //VrmAnimationExporter exportingGltfExporter;
        CustomVrmaDataStore customVrmaDataStore;

        

        protected void Awake()
        {
            //nameDB = new VVMMotionMuscleDB();
            //nameDB.Load(VVMMotionMuscleDB.CNS_VVMMuscleCSV);
            //exportingGltfData = new ExportingGltfData();
            //exportingGltfExporter = new VrmAnimationExporter(exportingGltfData, new GltfExportSettings());

            customVrmaDataStore = new CustomVrmaDataStore();
            //customExporter = new CustomVrmaExporter(exportingGltfData, new GltfExportSettings());

        }
        protected void Start()
        {
            BodyBonesCurves = new List<VVMMotionRecorderFrame>();
            //clip = new AnimationClip();

        }

        public void SetupTargetType(AF_TARGETTYPE type)
        {
            if (type == AF_TARGETTYPE.VRM)
            {

            }
        }
        public void ClearCurves()
        {
            BodyBonesCurves.Clear();
        }
        public float ConvertFloat(int muscle, float rotation)
        {
            float ret = 0;
            float maxr = HumanTrait.GetMuscleDefaultMax(muscle);
            float minr = HumanTrait.GetMuscleDefaultMin(muscle);

            float ret360 = Mathf.Repeat(rotation - 0, 360 - 0) + 0;

            ret = Mathf.Repeat(ret360 - minr, maxr - minr) + minr;

            if (rotation > 0)
            {
                
                //ret = rotation / HumanTrait.GetMuscleDefaultMax(muscle);
            }
            else
            {
                //ret = rotation / HumanTrait.GetMuscleDefaultMin(muscle);
            }

            return ret;
        }
        public string GenerateAnimationCurve()
        {

            Animator anim = GetComponent<Animator>();

            VVMDummyAnimationClip vmaniclip = new VVMDummyAnimationClip();

            Dictionary<string, VVMDummyFloatAnimCurve> dic_vmfcurve = new Dictionary<string, VVMDummyFloatAnimCurve>();

            //Dictionary<HumanBodyBones, VVMAnimationCurve> curves = new Dictionary<HumanBodyBones, VVMAnimationCurve>();
            //foreach (HumanBodyBones value in Enum.GetValues(typeof(HumanBodyBones)))
            for (int i = 0; i < HumanTrait.MuscleCount; i++)
            {
                string value = HumanTrait.MuscleName[i];

                VVMDummyFloatAnimCurve vmfcurve = new VVMDummyFloatAnimCurve();
                vmfcurve.attribute = value;
                dic_vmfcurve[value] = vmfcurve;
                vmaniclip.m_FloatCurves.Add(vmfcurve);

                /*
                //if ((value != HumanBodyBones.Jaw) && (value != HumanBodyBones.LastBone))
                {
                    //curves[value] = new VVMAnimationCurve();

                    int nx = HumanTrait.MuscleFromBone((int)value, 0);
                    //VVMMotionMuscleDBItem dbitem = nameDB.Search(value, "X");
                    if (nx > -1)
                    {
                        VVMDummyFloatAnimCurve vmfcurve = new VVMDummyFloatAnimCurve();
                        vmfcurve.attribute = HumanTrait.MuscleName[nx];
                        dic_vmfcurve[((int)value).ToString() + "_" + "X"] = vmfcurve;
                        vmaniclip.m_FloatCurves.Add(vmfcurve);
                    }
                    int ny = HumanTrait.MuscleFromBone((int)value, 1);
                    //VVMMotionMuscleDBItem dbitem2 = nameDB.Search(value, "Y");
                    if (ny > -1)
                    {
                        VVMDummyFloatAnimCurve vmfcurve = new VVMDummyFloatAnimCurve();
                        vmfcurve.attribute = HumanTrait.MuscleName[ny];
                        dic_vmfcurve[((int)value).ToString() + "_" + "Y"] = vmfcurve;
                        vmaniclip.m_FloatCurves.Add(vmfcurve);
                    }

                    int nz = HumanTrait.MuscleFromBone((int)value, 2);
                    //VVMMotionMuscleDBItem dbitem3 = nameDB.Search(value, "Z");
                    if (nz > -1)
                    {
                        VVMDummyFloatAnimCurve vmfcurve = new VVMDummyFloatAnimCurve();
                        vmfcurve.attribute = HumanTrait.MuscleName[nz];
                        dic_vmfcurve[((int)value).ToString() + "_" + "Z"] = vmfcurve;
                        vmaniclip.m_FloatCurves.Add(vmfcurve);
                    }
                }
                */
            }

            float fulltime = 0;
            int maxframe = BodyBonesCurves[BodyBonesCurves.Count - 1].frameIndex;
            for (int i = 0; i < BodyBonesCurves.Count; i++)
            {
                
                
                VVMMotionRecorderFrame vframe = BodyBonesCurves[i];

                if (i > 0)
                {
                    fulltime += vframe.duration;
                }

                for (int mf = 0; mf < vframe.muscles.Count; mf++)
                {
                    VVMDummyAnimCurveItem curitemX = new VVMDummyAnimCurveItem();
                    curitemX.time = fulltime;
                    curitemX.value = vframe.muscles[mf];
                    dic_vmfcurve[HumanTrait.MuscleName[mf]].curve.m_Curve.Add(curitemX);
                }
                
                /*
                vframe.transforms.ForEach(item =>
                {
                    int nx = HumanTrait.MuscleFromBone((int)item.bone, 0);
                    //VVMMotionMuscleDBItem dbitem = nameDB.Search(item.bone, "X");
                    if (nx > -1)
                    {
                        VVMDummyAnimCurveItem curitemX = new VVMDummyAnimCurveItem();
                        curitemX.time = fulltime;
                        curitemX.value = ConvertFloat(nx, item.rotation.x);
                        dic_vmfcurve[((int)item.bone).ToString() + "_X"].curve.m_Curve.Add(curitemX);
                    }

                    int ny = HumanTrait.MuscleFromBone((int)item.bone, 1);
                    //VVMMotionMuscleDBItem dbitem2 = nameDB.Search(item.bone, "Y");
                    if (ny > -1)
                    {
                        VVMDummyAnimCurveItem curitemY = new VVMDummyAnimCurveItem();
                        curitemY.time = fulltime;
                        curitemY.value = ConvertFloat(ny, item.rotation.y);
                        dic_vmfcurve[((int)item.bone).ToString() + "_Y"].curve.m_Curve.Add(curitemY);
                    }

                    int nz = HumanTrait.MuscleFromBone((int)item.bone, 2);
                    //VVMMotionMuscleDBItem dbitem3 = nameDB.Search(item.bone, "Z");
                    if (nz > -1)
                    {
                        VVMDummyAnimCurveItem curitemZ = new VVMDummyAnimCurveItem();
                        curitemZ.time = fulltime;
                        curitemZ.value = ConvertFloat(nz, item.rotation.z);
                        dic_vmfcurve[((int)item.bone).ToString() + "_Z"].curve.m_Curve.Add(curitemZ);
                    }
                });
                */
            }
            vmaniclip.m_AnimationClipSettings.m_StopTime = fulltime;
            /*
            Dictionary<string, VVMDummyFloatAnimCurve>.Enumerator cenum = dic_vmfcurve.GetEnumerator();
            while (cenum.MoveNext())
            {
                var cur = cenum.Current;
                
                clip.SetCurve(anim.GetBoneTransform(cur.Key).name, typeof(Transform), "localPosition.x", curves[cur.Key].curve["p.x"]);
                clip.SetCurve(anim.GetBoneTransform(cur.Key).name, typeof(Transform), "localPosition.y", curves[cur.Key].curve["p.y"]);
                clip.SetCurve(anim.GetBoneTransform(cur.Key).name, typeof(Transform), "localPosition.z", curves[cur.Key].curve["p.z"]);
                clip.SetCurve(anim.GetBoneTransform(cur.Key).name, typeof(Transform), "localRotation.x", curves[cur.Key].curve["r.x"]);
                clip.SetCurve(anim.GetBoneTransform(cur.Key).name, typeof(Transform), "localRotation.y", curves[cur.Key].curve["r.y"]);
                clip.SetCurve(anim.GetBoneTransform(cur.Key).name, typeof(Transform), "localRotation.z", curves[cur.Key].curve["r.z"]);
                clip.SetCurve(anim.GetBoneTransform(cur.Key).name, typeof(Transform), "localRotation.w", curves[cur.Key].curve["r.w"]);
            }
            */

            string ret = JsonUtility.ToJson(vmaniclip);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
            return ret;
        }

        Vector3 vrmaInverted = new Vector3(1, -1, -1);

        public void AddKeyFrame(int frame, Ease ease, float duration)
        {
            Animator anim = GetComponent<Animator>();
            if (TargetType == AF_TARGETTYPE.VRM)
            {
                HumanPoseHandler hph = new HumanPoseHandler(anim.avatar, anim.transform);
                HumanPose hp = new HumanPose();
                hph.GetHumanPose(ref hp);

                VVMMotionRecorderFrame vfr = new VVMMotionRecorderFrame(frame);
                vfr.duration = duration;
                vfr.ease = ease;
                vfr.muscles = new List<float>(hp.muscles);
                /*
                foreach (HumanBodyBones value in Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if ((value != HumanBodyBones.Jaw) && (value != HumanBodyBones.LastBone))
                    {
                        Transform bonetran = anim.GetBoneTransform(value);
                        if (bonetran != null)
                        {
                            VVMTransform vtran = new VVMTransform();
                            vtran.bone = value;
                            vtran.position = bonetran.localPosition;
                            vtran.rotation = bonetran.localRotation.eulerAngles;
                            vfr.transforms.Add(vtran);
                        }
                    }
                    

                }
                */
                BodyBonesCurves.Add(vfr);

                
            }
        }
        public void AddKeyFrameVRMA(int frame, Ease ease, float duration)
        {
            //---VRMAnimation
            string decimalstr = duration.ToString("0.0000");
            string[] decstrs = decimalstr.Split(".");
            //var time = default(TimeSpan);
            //time.Add(new TimeSpan(0, 0, 0, int.Parse(decstrs[0]), int.Parse(decstrs[1])));
            TimeSpan time = TimeSpan.FromMilliseconds(duration * 1000);

            //Debug.Log(time.ToString());
            //exportingGltfExporter.AddFrame(time, -1);
            customVrmaDataStore.AddFrame(frame, duration, new Vector3(1, -1, -1));
        }
        public void InsertKeyFrame(int frame, float duration, int nearRightFrame)
        {
            string decimalstr = duration.ToString("0.0000");
            string[] decstrs = decimalstr.Split(".");
            //var time = default(TimeSpan);
            //time.Add(new TimeSpan(0, 0, 0, int.Parse(decstrs[0]), int.Parse(decstrs[1])));
            TimeSpan time = TimeSpan.FromMilliseconds(duration * 1000);
            //exportingGltfExporter.InsertFrame(frame, time, 1);

            
        }
        public void InsertKeyFrameVRMA(int frame, float duration, int nearRightFrame)
        {
            customVrmaDataStore.InsertFrame(frame, duration, vrmaInverted);
        }
        public void ModifyKeyFrame(int frame, Ease ease, float duration)
        {
            Animator anim = GetComponent<Animator>();
            if (TargetType == AF_TARGETTYPE.VRM)
            {
                HumanPoseHandler hph = new HumanPoseHandler(anim.avatar, anim.transform);
                HumanPose hp = new HumanPose();
                hph.GetHumanPose(ref hp);

                VVMMotionRecorderFrame vfr = new VVMMotionRecorderFrame(frame);
                vfr.duration = duration;
                vfr.ease = ease;
                vfr.muscles = new List<float>(hp.muscles);
                /*
                foreach (HumanBodyBones value in Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if ((value != HumanBodyBones.Jaw) && (value != HumanBodyBones.LastBone))
                    {
                        Transform bonetran = anim.GetBoneTransform(value);
                        if (bonetran != null)
                        {
                            VVMTransform vtran = new VVMTransform();
                            vtran.bone = value;
                            vtran.position = bonetran.localPosition;
                            vtran.rotation = bonetran.localRotation.eulerAngles;
                            vfr.transforms.Add(vtran);
                        }
                    }
                }
                */
                int index = BodyBonesCurves.FindIndex(m =>
                {
                    if (m.frameIndex == frame) return true;
                    return false;
                });
                if (index > -1)
                {
                    BodyBonesCurves[index] = vfr;
                }

                
            }
        }
        public void ModifyFrameVRMA(int frame, Ease ease, float duration)
        {
            if (TargetType == AF_TARGETTYPE.VRM)
            {
                //---VRMAnimation
                /*string decimalstr = duration.ToString("0.0000");
                string[] decstrs = decimalstr.Split(".");
                //var time = default(TimeSpan);
                //time.Add(new TimeSpan(0, 0, 0, int.Parse(decstrs[0]), int.Parse(decstrs[1])));
                TimeSpan time = TimeSpan.FromMilliseconds(duration * 1000);
                //exportingGltfExporter.ModifyFrame(index, time, -1);
                */
                customVrmaDataStore.ModifyFrame(frame, duration, vrmaInverted);
            }
        }
        public void RemoveKeyFrame(int frame, bool toleft = false)
        {
            int index = -1;
            for (int i = 0; i < BodyBonesCurves.Count; i++)
            {
                if ((index > -1) && (toleft))
                { //---decrease frame index (to left) after target frame index.
                    BodyBonesCurves[i].frameIndex -= -1;
                }
                if (BodyBonesCurves[i].frameIndex == frame)
                {
                    index = i;
                }
            }
            
            if (index > -1)
            {
                BodyBonesCurves.RemoveAt(index);
            }

            
        }
        public void RemoveKeyFrameVRMA(int frame, bool toleft = false)
        {
            //---VRMAnimation
            //exportingGltfExporter.DeleteFrame(index);
            if (toleft == true)
            {
                customVrmaDataStore.DeleteRowFrame(frame);
            }
            else
            {
                customVrmaDataStore.DeleteFrame(frame);
            }
            
        }
        public void ClearKeyFrameVRMA()
        {
            customVrmaDataStore.ClearFrames();
        }
        public void InsertBlankFrame(int frame)
        {
            int index = -1;
            for (int i = 0; i < BodyBonesCurves.Count; i++)
            {
                if ((index > -1))
                { //---increase frame index (to right) after target frame index.
                    BodyBonesCurves[i].frameIndex += -1;
                }
                if (BodyBonesCurves[i].frameIndex == frame)
                {
                    index = i;
                    BodyBonesCurves[i].frameIndex += 1;
                }
            }
            
        }
        public void InsertBlankFrameVRMA(int frame)
        {
            //---VRMAnimation
            customVrmaDataStore.InsertFrame(frame, 0, vrmaInverted);
        }
        public bool IsExistFrame(int frame)
        {
            int index = BodyBonesCurves.FindIndex(m =>
            {
                if (m.frameIndex == frame) return true;
                return false;
            });
            return index > -1;
        }
        public bool IsExistFrameVRMA(int frame)
        {
            int ishit = customVrmaDataStore.GetFrameByIndex(frame);
            return (ishit > -1);
        }
        public void CopyHierarchy(Transform source, Transform dest)
        {
            // コピー元のGameObjectの子オブジェクトを取得
            Transform[] sourceChildren = source.gameObject.GetComponentsInChildren<Transform>();
            

            // コピー元のGameObjectの子オブジェクトをループ処理
            for (int i = 0; i < sourceChildren.Length; i++)
            {
                // コピー元のGameObjectの子オブジェクトをコピー
                GameObject child = GameObject.Instantiate(sourceChildren[i].gameObject);
                //dest.transform.GetChildren().Add(child);
                child.transform.SetParent(dest);

                // 再帰処理
                CopyHierarchy(child.transform, dest);
            }
        }
        public void PrepareExportVRMA(Transform rootparent, Vrm10Instance vinst)
        {
            Transform GetParentBone(Dictionary<HumanBodyBones, Transform> map, Vrm10HumanoidBones bone)
            {
                while (true)
                {
                    if (bone == Vrm10HumanoidBones.Hips)
                    {
                        break;
                    }
                    var parentBone = Vrm10HumanoidBoneSpecification.GetDefine(bone).ParentBone.Value;
                    var unityParentBone = Vrm10HumanoidBoneSpecification.ConvertToUnityBone(parentBone);
                    if (map.TryGetValue(unityParentBone, out var found))
                    {
                        return found;
                    }
                    bone = parentBone;
                    //Debug.Log(bone);
                }

                // hips has no parent
                return null;
            }
            GameObject tmpobj = new GameObject();
            tmpobj.name = "vrma_" + transform.gameObject.name;
            tmpobj.transform.SetParent(rootparent);
            tmpobj.transform.rotation = Quaternion.Euler(new Vector3(0, -180f, 0));
            Debug.Log(vinst.Runtime.ControlRig.Bones);
            Transform hips = vinst.Runtime.ControlRig.GetBoneTransform(HumanBodyBones.Hips);
            Debug.Log(hips.parent.name);
            Transform tmpvrmroot = hips.parent;
            //GameObject[] srcChildren = tmpvrmroot.GetComponentsInChildren<GameObject>();
            GameObject copyedHipsParent = GameObject.Instantiate(hips.parent.gameObject);
            copyedHipsParent.transform.SetParent(tmpobj.transform);

            //CopyHierarchy(tmpvrmroot, tmpobj.transform);



            //exportingGltfExporter.Prepare(tmpobj);
            customVrmaDataStore.pointerObject = tmpobj;
            //customExporter.Prepare(tmpobj);

            //
            // setup
            //
            var map = new Dictionary<HumanBodyBones, Transform>();
            Animator anim = GetComponent<Animator>();
            foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if ((bone == HumanBodyBones.LastBone) 
                    || (bone == HumanBodyBones.UpperChest) //---if not neccesary...?
                )
                {
                    continue;
                }
                var t = anim.GetBoneTransform(bone);
                if (t == null)
                {
                    continue;
                }
                map.Add(bone, t);
            }

            //exportingGltfExporter.SetPositionBoneAndParent(map[HumanBodyBones.Hips], transform);
            customVrmaDataStore.SetPositionBoneAndParent(map[HumanBodyBones.Hips], transform);

            foreach (var kv in map)
            {
                var vrmBone = Vrm10HumanoidBoneSpecification.ConvertFromUnityBone(kv.Key);
                var parent = GetParentBone(map, vrmBone) ?? transform;
                //exportingGltfExporter.AddRotationBoneAndParent(kv.Key, kv.Value, parent);
                customVrmaDataStore.AddRotationBoneAndParent(kv.Key, kv.Value, parent);
            }
            //---Look At bone
            var lookatmap = new Dictionary<HumanBodyBones, Transform>();
            lookatmap[HumanBodyBones.LeftEye] = vinst.Humanoid.LeftEye.transform;
            lookatmap[HumanBodyBones.RightEye] = vinst.Humanoid.RightEye.transform;
            foreach (var kv in lookatmap) {
                var vrmBone = Vrm10HumanoidBoneSpecification.ConvertFromUnityBone(kv.Key);
                var parent = GetParentBone(lookatmap, vrmBone) ?? transform;
                customVrmaDataStore.AddRotationLookAtBone(kv.Key, kv.Value, parent);
            }
            
            
        }
        public void ExportVRMA(string param)
        {
            ExportingGltfData edata = new ExportingGltfData();
            CustomVrmaExporter vexport = new CustomVrmaExporter(edata, new GltfExportSettings());
            vexport.Prepare(customVrmaDataStore.pointerObject);
            vexport.ExportCustom(customVrmaDataStore, customVrmaDataStore.m_frames, param);

            Byte[] outdata = edata.ToGlbBytes();


            /*exportingGltfExporter.Export((VrmAnimationExporter vrma) =>
            {
                int count = vrma.GetFrameCount();
                for (int i = 0; i < count; i++)
                {
                    Debug.Log($"Frm: {i} = {vrma.GetFrameTime(i)}");
                }
                vrma.GenerateTotalSeconds();
            });*/
            /*
            customExporter.ExportCustom();

            Byte[] outdata = exportingGltfData.ToGlbBytes();
            */            
            VVMDummyVrmaOutput vvmao = new();
            vvmao.data = outdata;
            

            string ret = JsonUtility.ToJson(vvmao);

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public void ClearAllFrames4VRMA()
        {
            customVrmaDataStore.ClearFrames();
            
        }
    }
}

