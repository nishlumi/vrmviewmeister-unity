using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UserHandleSpace;
using RootMotion.FinalIK;
using VRM;
using DG.Tweening;


namespace UserHandleSpace
{
    /// <summary>
    /// Attach for VRM game object
    /// </summary>
    public class OperateLoadedVRM : OperateLoadedBase
    {

        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);

        [Serializable]
        public class MaterialProperties
        {
            public string shaderName = "";
            public Color color = Color.white;
            public float cullmode = 0;
            public float blendmode = 0;
            public Color emissioncolor = Color.white;
            public Color shadetexcolor = Color.white;
            public float shadingtoony = 0;
            public Color rimcolor = Color.white;
            public float rimfresnel = 0;
            public float srcblend = 0;
            public float dstblend = 0;
        }

        /**
         *  This class is manager function, all VRM
         *  Variables only...
         */
        //public GameObject relatedHandleParent;
        private VRMImporterContext context;
        private Bounds bodyInfoTPose;
        private List<Vector3> bodyinfoList;
        private SkinnedMeshRenderer BSFace;

        public List<BasicStringFloatList> blendShapeList;

        public bool isMoveMode;

        public int equipType; //Flag for animation: -1 - unequip, 0 - no change, 1 - to equip
        public AvatarEquipmentClass equipDestinations;
        public AvatarGravityClass gravityList;
        public List<AvatarIKMappingClass> ikMappingList;

        private Blinker blink;
        public MaterialProperties userSharedProperties;

        private ManageAnimation manim;

        //private float[] bodyInfoFloat;
        /*
        private bool isFixMoving;

        private Vector3 oldPosition;
        private Quaternion oldRotation;
        private Vector3 defaultPosition;
        private Quaternion defaultRotation;
        private Vector3 defaultColliderPosition;

        private Vector3 oldikposition;
        */

        // Start is called before the first frame update
        private void Awake()
        {
            SaveDefaultTransform(true, true);
            SetActiveFace();

            targetType = AF_TARGETTYPE.VRM;

            bodyinfoList = new List<Vector3>();
            blendShapeList = new List<BasicStringFloatList>();
            gravityList = new AvatarGravityClass();
            equipType = 0;
            equipDestinations = new AvatarEquipmentClass();
            ikMappingList = new List<AvatarIKMappingClass>();
            userSharedProperties = new MaterialProperties();
        }
        void Start()
        {
            
            manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

            blink = GetComponent<Blinker>();
            blendShapeList.Clear();
            
            List<string> lst = ListAvatarBlendShape();
            lst.ForEach(item =>
            {
                string[] arr = item.Split('=');
                float value = -1f; // float.TryParse(arr[1], out value) ? value : -1f;
                BasicStringFloatList lsf = new BasicStringFloatList(arr[0], value);
                blendShapeList.Add(lsf);

            });
        }

        // Update is called once per frame
        void Update()
        {
            //--if move ik handle parent, move CapsuleCollider of active avatar to same position.
            //GameObject ikhp = GameObject.Find("IKHandleParent");
            //OperateActiveVRM oavrm;
            //if (ikhp.TryGetComponent<OperateActiveVRM>(out oavrm))
            //{
            if (relatedHandleParent)
            {
                Transform iktran = relatedHandleParent.transform;
                if (iktran.position != oldikposition)
                {
                    CapsuleCollider cap = GetComponent<CapsuleCollider>();
                    cap.center = new Vector3(iktran.position.x, iktran.position.y + (defaultColliderPosition.y), iktran.position.z);

                    oldikposition = iktran.position;
                }

            }
            //}


        }
        private void LateUpdate()
        {

        }
        public VRMImporterContext GetContext()
        {
            return context;
        }
        public void SetContext(VRMImporterContext cont)
        {
            context = cont;
        }
        public Bounds GetTPoseBodyInfo()
        {
            return bodyInfoTPose;
        }
        public void SetTPoseBodyInfo(Bounds pose)
        {
            bodyInfoTPose = new Bounds(pose.center, pose.size);
            bodyInfoTPose.extents = new Vector3(pose.extents.x, pose.extents.y, pose.extents.z);

            //Array.Copy(basebodyInfo, bodyInfoFloat, basebodyInfo.Length);
        }
        public List<Vector3> GetTPoseBodyList(bool iscopy = false)
        {
            if (iscopy)
            {
                List<Vector3> lst = new List<Vector3>();
                for (int i = 0; i < bodyinfoList.Count; i++)
                {
                    lst.Add(new Vector3(  bodyinfoList[i].x, bodyinfoList[i].y, bodyinfoList[i].z ));
                }
                return lst;
            }
            else
            {
                return bodyinfoList;
            }
        }
        public void SetTPoseBodyList(List<Vector3> lst)
        {
            bodyinfoList.Clear();
            for (int i = 0; i < lst.Count; i++)
            {
                bodyinfoList.Add(new Vector3(lst[i].x, lst[i].y, lst[i].z));
            }
        }
        public override void SetEnableWholeIK(int intflag)
        {
            bool flag = intflag == 1 ? true : false;

            BipedIK bik = transform.GetComponent<BipedIK>();
            CCDIK cik = transform.GetComponent<CCDIK>();

            if (bik != null) bik.enabled = flag;
            if (cik != null) cik.enabled = flag;
        }
        public override void SetFixMoving(bool flag)
        {
            isFixMoving = flag;
            if (flag)
            {
                transform.gameObject.GetComponent<CapsuleCollider>().enabled = false;
                relatedHandleParent.SetActive(false);
            }
            else
            {
                transform.gameObject.GetComponent<CapsuleCollider>().enabled = true;
                relatedHandleParent.SetActive(true);
            }
        }

        public override void SetScale(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;

            //---Different to OperateActiveVRM.SetScale:
            // OperateActiveVRM: base point is child object with collider. refer to transform.parent.
            // At here: transform.gameObject IS Other Object own.
            transform.DOScale(new Vector3(x, y, z), 0.2f);

        }
        //===============================================================================================================================
        //  IK operation 

        public GameObject GetIKHandle(string name)
        {
            GameObject ret = null;

            int cnt = relatedHandleParent.transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                if (relatedHandleParent.transform.GetChild(i).name == name)
                {
                    ret = relatedHandleParent.transform.GetChild(i).gameObject;
                    break;
                }
            }
            return ret;
        }
        /// <summary>
        /// To recover the IK transform(position, rotation) by HumanBodyBones of avatar self.
        /// </summary>
        public void RecoverIKTransform()
        {
            Animator animator = GetComponent<Animator>();
            int childCount = relatedHandleParent.transform.childCount;

            //---Head <= Head
            Transform head = relatedHandleParent.transform.Find("Head");
            head.position = animator.GetBoneTransform(HumanBodyBones.Head).transform.position;
            head.rotation = animator.GetBoneTransform(HumanBodyBones.Head).transform.rotation;

        }
        public override void EnableIK(bool flag)
        {
            BipedIK bik = gameObject.TryGetComponent<BipedIK>(out bik) ? bik : null;
            CCDIK cik = gameObject.TryGetComponent<CCDIK>(out cik) ? cik : null;
            LeftHandPoseController lhand = gameObject.TryGetComponent<LeftHandPoseController>(out lhand) ? lhand : null;
            RightHandPoseController rhand = gameObject.TryGetComponent<RightHandPoseController>(out rhand) ? rhand : null;

            if (flag)
            {
                if (rhand != null) rhand.enabled = flag;
                if (lhand != null) lhand.enabled = flag;
                if (bik != null) bik.enabled = flag;
                if (cik != null) cik.enabled = flag;
            }
            else
            {
                if (cik != null) cik.enabled = flag;
                if (bik != null) bik.enabled = flag;
                if (lhand != null) lhand.enabled = flag;
                if (rhand != null) rhand.enabled = flag;
            }

        }

        /// <summary>
        /// Change enable/disable RotationLimitedHinge of LowerLeg / Foot
        /// </summary>
        /// <param name="flag"></param>
        public void EnableRotationLimit(int flag)
        {
            Animator animator = GetComponent<Animator>();

            Transform leftlowerarm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            Transform rightlowerarm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            Transform leftlowerleg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            Transform rightlowerleg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            Transform leftfoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform rightfoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

            leftlowerarm.GetComponent<RotationLimitHinge>().enabled = flag == 1 ? true : false;
            rightlowerarm.GetComponent<RotationLimitHinge>().enabled = flag == 1 ? true : false;
            leftlowerleg.GetComponent<RotationLimitHinge>().enabled = flag == 1 ? true : false;
            rightlowerleg.GetComponent<RotationLimitHinge>().enabled = flag == 1 ? true : false;
            leftfoot.GetComponent<RotationLimitHinge>().enabled = flag == 1 ? true : false;
            rightfoot.GetComponent<RotationLimitHinge>().enabled = flag == 1 ? true : false;

        }

        //===============================================================================================================================
        //  Gravity 

        /// <summary>
        /// Initial enumrate Gravity information
        /// </summary>
        public void ListGravityInfo()
        {
            VRMSpringBone[] bones = transform.GetComponentsInChildren<VRMSpringBone>();
            gravityList.list.Clear();
            for (int i = 0; i < bones.Length; i++)
            {
                VRMGravityInfo vgi = new VRMGravityInfo();
                vgi.comment = bones[i].m_comment;
                if (bones[i].RootBones.Count > 0) vgi.rootBoneName = bones[i].RootBones[0].gameObject.name;
                vgi.power = bones[i].m_gravityPower;
                vgi.dir.x = bones[i].m_gravityDir.x;
                vgi.dir.y = bones[i].m_gravityDir.y;
                vgi.dir.z = bones[i].m_gravityDir.z;
                gravityList.list.Add(vgi);
            }
        }
        public void ListGravityInfoFromOuter()
        {
            ListGravityInfo();
            string ret = JsonUtility.ToJson(gravityList);

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">csv-string: 0 - comment, 1 - root bone[0] name</param>
        /// <returns></returns>
        public VRMGravityInfo GetGravityInfo(string param)
        {
            VRMGravityInfo ret = null;

            string[] arr = param.Split(',');

            VRMSpringBone[] bones = transform.GetComponentsInChildren<VRMSpringBone>();
            for (int i = 0; i < bones.Length; i++)
            {
                if ((bones[i].m_comment == arr[0]) && (bones[i].RootBones[0].gameObject.name == arr[1]))
                {
                    ret = gravityList.list.Find(match =>
                    {
                        if ((match.comment == arr[0]) && (match.rootBoneName == arr[1])) return true;
                        return false;
                    });
                    //ret.comment = bones[i].m_comment;
                    //if (bones[i].RootBones.Count > 0) ret.rootBoneName = bones[i].RootBones[0].gameObject.name;
                    if (ret != null)
                    {
                        ret.power = bones[i].m_gravityPower;
                        ret.dir.x = bones[i].m_gravityDir.x;
                        ret.dir.y = bones[i].m_gravityDir.y;
                        ret.dir.z = bones[i].m_gravityDir.z;
                    }
                }
            }

            return ret;
        }
        public void GetGravityInfoFromOuter(string param)
        {
            VRMGravityInfo ret = GetGravityInfo(param);
            string js = JsonUtility.ToJson(ret);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">csv-string: 0 - comment, 1 - root bone[0] name, 2 - power</param>
        public void SetGravityPower(string param)
        {
            VRMGravityInfo ret = new VRMGravityInfo();

            string[] arr = param.Split(',');
            if (arr.Length > 2)
            {
                float val = float.TryParse(arr[2], out val) ? val : 0f;

                VRMSpringBone[] bones = transform.GetComponentsInChildren<VRMSpringBone>();
                for (int i = 0; i < bones.Length; i++)
                {
                    if ((bones[i].m_comment == arr[0]) && (bones[i].RootBones[0].gameObject.name == arr[1]))
                    {
                        bones[i].m_gravityPower = val;
                        VRMGravityInfo vgi = GetGravityInfo(arr[0] + "," + arr[1]);
                        vgi.power = val;
                        break;
                    }
                }
            }
            
        }
        public void SetAnimationGravityPower(string comment, string bonename, Sequence seq, float val_power, float duration)
        {

            VRMSpringBone[] bones = transform.GetComponentsInChildren<VRMSpringBone>();
            for (int i = 0; i < bones.Length; i++)
            {
                if ((bones[i].m_comment == comment) && (bones[i].RootBones[0].gameObject.name == bonename))
                {
                    seq.Join(DOTween.To(() => bones[i].m_gravityPower, x => bones[i].m_gravityPower = x, val_power, duration));
                    break;
                }
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">csv-string: 0 - comment, 1 - root bone[0] name, 2 - x, 3 - y, 4 - z</param>
        public void SetGravityDirFromOuter(string param)
        {
            VRMGravityInfo ret = new VRMGravityInfo();

            string[] arr = param.Split(',');
            if (arr.Length > 4)
            {
                float x = float.TryParse(arr[2], out x) ? x : 0f;
                float y = float.TryParse(arr[3], out y) ? y : 0f;
                float z = float.TryParse(arr[4], out z) ? z : 0f;

                VRMSpringBone[] bones = transform.GetComponentsInChildren<VRMSpringBone>();
                for (int i = 0; i < bones.Length; i++)
                {
                    if ((bones[i].m_comment == arr[0]) && (bones[i].RootBones[0].gameObject.name == arr[1]))
                    {
                        bones[i].m_gravityDir.x = x;
                        bones[i].m_gravityDir.y = y;
                        bones[i].m_gravityDir.z = z;
                        VRMGravityInfo vgi = GetGravityInfo(arr[0] + "," + arr[1]);
                        vgi.dir.x = x;
                        vgi.dir.y = y;
                        vgi.dir.z = z;
                        break;
                    }
                }
            }

        }
        public void SetGravityDir(string comment, string bonename, float x, float y, float z)
        {
            VRMSpringBone[] bones = transform.GetComponentsInChildren<VRMSpringBone>();
            for (int i = 0; i < bones.Length; i++)
            {
                if ((bones[i].m_comment == comment) && (bones[i].RootBones[0].gameObject.name == bonename))
                {
                    bones[i].m_gravityDir.x = x;
                    bones[i].m_gravityDir.y = y;
                    bones[i].m_gravityDir.z = z;
                    VRMGravityInfo vgi = GetGravityInfo(comment + "," + bonename);
                    vgi.dir.x = x;
                    vgi.dir.y = y;
                    vgi.dir.z = z;
                    break;
                }
            }
        }

        /// <summary>
        /// To pack all properties for HTML-UI
        /// </summary>
        public void GetIndicatedPropertyFromOuter()
        {
            string ret = "";

            LeftHandPoseController ctl = GetComponent<LeftHandPoseController>();
            RightHandPoseController ctr = GetComponent<RightHandPoseController>();

            List<string> lst = ListAvatarBlendShape();
            string blendshape = string.Join(",", lst.ToArray());
            int eflag = GetEquipFlag();
            string js = JsonUtility.ToJson(equipDestinations);
            string grajs = JsonUtility.ToJson(gravityList);

            ret = "l," + ctl.currentPose.ToString() + "," + ctl.handPoseValue.ToString()
                + "%" +
                "r," + ctr.currentPose.ToString() + "," + ctr.handPoseValue.ToString()
                + "\t" + 
                blendshape
                + "\t" + 
                eflag.ToString()
                + "\t" + 
                js
                + "\t" + 
                grajs
                + "\t" +
                GetHeadLock().ToString()
            ;
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        //===============================================================================================================***
        //  Hand Pose
        public string ListHandPose()
        {
            string ret = "";
            string[] arms = new string[2];
            LeftHandPoseController ctl = GetComponent<LeftHandPoseController>();
            RightHandPoseController ctr = GetComponent<RightHandPoseController>();
            
            ret = "l," + ctl.currentPose.ToString() + "," + ctl.handPoseValue.ToString()// + "," + ctl.handPose2Value.ToString() + "," + ctl.handPose3Value.ToString() + "," + ctl.handPose4Value.ToString() + "," + ctl.handPose5Value.ToString() + "," + ctl.handPose6Value.ToString()
                + "%" +
                "r," + ctr.currentPose.ToString() + "," + ctr.handPoseValue.ToString()// + "," + ctr.handPose2Value.ToString() + "," + ctr.handPose3Value.ToString() + "," + ctr.handPose4Value.ToString() + "," + ctr.handPose5Value.ToString() + "," + ctr.handPose6Value.ToString();
            ;
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
            return ret;
        }
        /// <summary>
        /// Change Hand Pose
        /// </summary>
        /// <param name="param">CSV-string : 0 = r(right), l(left), 1 = pose type(1-6), 2 = pose weight</param>
        public void PosingHandFromOuter(string param)
        {

            string[] prm = param.Split(',');
            string handtype = prm[0];
            int posetype = int.TryParse(prm[1], out posetype) ? posetype : 1;
            float value = float.TryParse(prm[2], out value) ? value : 0f;
            //Debug.Log("unity . int=" + posetype);
            //Debug.Log("unity . float=" + value);

            LeftHandPoseController ctl = GetComponent<LeftHandPoseController>();
            RightHandPoseController ctr = GetComponent<RightHandPoseController>();

            if (handtype == "l")
            {
                ctl.ResetPose();
                ctl.SetPose(posetype, value);
            }
            else
            {
                ctr.ResetPose();
                ctr.SetPose(posetype, value);
            }
        }


        //===============================================================================================================================
        //  Blend Shape 
        public SkinnedMeshRenderer GetBlendShapeTarget()
        {
            return BSFace;
        }
        public SkinnedMeshRenderer SetActiveFace()
        {
            int cnt = transform.childCount;
            SkinnedMeshRenderer mesh = null;
            for (int i = 0; i < cnt; i++)
            {
                mesh = transform.GetChild(i).GetComponent<SkinnedMeshRenderer>();
                if (mesh != null)
                {
                    if (mesh.sharedMesh.blendShapeCount > 0)
                    {
                        BSFace = mesh;
                        break;
                    }
                }

            }
            return mesh;
        }
        /// <summary>
        /// Get all blend shapes, the avatar has.
        /// </summary>
        /// <returns>List string Blend shape lis</returns>
        public List<string> ListAvatarBlendShape()
        {
            List<string> ret = new List<string>();
            if (BSFace)
            {
                int bscnt = BSFace.sharedMesh.blendShapeCount;
                for (int i = 0; i < bscnt; i++)
                {
                    ret.Add(BSFace.sharedMesh.GetBlendShapeName(i) + "=" + BSFace.GetBlendShapeWeight(i));
                }
            }

            return ret;
        }
        /// <summary>
        /// Get all blend shapes, the avatar has. When user call from HTML
        /// </summary>
        /// <returns>string Blend shape list</returns>
        public string ListAvatarBlendShapeFromOuter()
        {
            List<string> lst = ListAvatarBlendShape();
            string ret = string.Join(",", lst.ToArray());
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
            return ret;
        }
        public void InitializeBlendShapeList()
        {
            blendShapeList.Clear();
            List<string> lst = ListAvatarBlendShape();
            lst.ForEach(item =>
            {
                string[] arr = item.Split('=');
                float value = -1f; // float.TryParse(arr[1], out value) ? value : -1f;
                BasicStringFloatList lsf = new BasicStringFloatList(arr[0], value);
                blendShapeList.Add(lsf);

            });
        }
        public float getAvatarBlendShape(string param)
        {
            float ret = 0f;
            List<string> lst = ListAvatarBlendShape();
            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].IndexOf(param) > -1)
                {
                    ret = BSFace.GetBlendShapeWeight(i);
                    break;
                }
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
            return ret;
        }
        public float getAvatarBlendShapeValue(string param)
        {
            float ret = 0f;
            List<string> lst = ListAvatarBlendShape();
            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].IndexOf(param) > -1)
                {
                    ret = BSFace.GetBlendShapeWeight(i);
                    break;
                }
            }
            return ret;
        }
        public int getAvatarBlendShapeIndex(string name)
        {
            return BSFace.sharedMesh.GetBlendShapeIndex(name);
        }
        public void changeAvatarBlendShape(string param)
        {
            string[] prm = param.Split(',');
            int index = int.TryParse(prm[0], out index) ? index : 0;
            float value = float.TryParse(prm[1], out value) ? value : 0f;
            BSFace.SetBlendShapeWeight(index, value);
        }
        public void changeAvatarBlendShape(int index, float value)
        {
            BSFace.SetBlendShapeWeight(index, value);
        }
        public void changeAvatarBlendShapeByName(string param)
        {
            string[] prm = param.Split(',');
            string shapename = prm[0];
            float value = float.TryParse(prm[1], out value) ? value : 0f;
            int index = getAvatarBlendShapeIndex(shapename);
            if (index > -1)
            {
                BSFace.SetBlendShapeWeight(index, value);
            }
        }
        public void changeAvatarBlendShapeByName(string shapename, float value)
        {
            int index = getAvatarBlendShapeIndex(shapename);
            if (index > -1)
            {
                BSFace.SetBlendShapeWeight(index, value);
            }
        }
        public void GetBlinkEye()
        {
            string ret = "";
            if (blink != null)
            {
                ret = (blink.enabled ? "1" : "0") + "," + blink.Interval.ToString() + "," + blink.OpeningSeconds.ToString() + "," + blink.CloseSeconds.ToString() + "," + blink.ClosingTime.ToString();
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public int GetBlinkFlag()
        {
            return blink.enabled ? 1 : 0;
        }
        public Blinker BlinkEye
        {
            get
            {
                return blink;
            }
        }
        public float GetBlinkInterval()
        {
            return blink.Interval;
        }
        public float GetBlinkOpeningSeconds()
        {
            return blink.OpeningSeconds;
        }
        public float GetBlinkCloseSeconds()
        {
            return blink.CloseSeconds;
        }
        public float GetBlinkClosingTime()
        {
            return blink.ClosingTime;
        }
        public void SetBlinkFlag(int flag)
        {
            blink.enabled = flag == 1 ? true : false;
        }
        public void SetBlinkInterval(float v)
        {
            blink.Interval = v;
        }
        public void SetBlinkOpeningSeconds(float v)
        {
            blink.OpeningSeconds = v;
        }
        public void SetBlinkCloseSeconds(float v)
        {
            blink.CloseSeconds = v;
        }
        public void SetBlinkClosingTime(float v)
        {
            blink.ClosingTime = v;
        }


        //===============================================================================================================================
        //  Equip 

        public void SetEquipFlag(int flag)
        {
            equipType = flag;
        }
        public int GetEquipFlag()
        {
            return equipType;
        }
        public int GetEquipFlagFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(equipType);
#endif
            return equipType;
        }
        public List<AvatarEquipSaveClass> GetEquipmentInformation()
        {
            return equipDestinations.list;
        }
        public void GetEquipmentInformationFromOuter()
        {
            string js = JsonUtility.ToJson(equipDestinations);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }
        /// <summary>
        /// To apply equipment object position to destination body parts position
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="equipment"></param>
        public void EquipPositioning(HumanBodyBones parts, NativeAnimationAvatar equipment)
        {
            Transform dest = this.gameObject.GetComponent<Animator>().GetBoneTransform(parts);
            equipment.avatar.transform.position = dest.position;
        }

        /// <summary>
        /// To equip other item object
        /// (*) if already equped, nothing function
        /// vi devas sxargi position kaj rotation antaux cxi tiu funkcio
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="equipment">animation avatar</param>
        public void EquipObject(HumanBodyBones parts, NativeAnimationAvatar equipment)
        {
            Transform dest = this.gameObject.GetComponent<Animator>().GetBoneTransform(parts);
            //ManageAnimation manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            OperateLoadedBase olo = equipment.avatar.GetComponent<OperateLoadedBase>();
            OtherObjectDummyIK ooik = olo.relatedHandleParent.GetComponent<OtherObjectDummyIK>();

            int isHit = equipDestinations.list.FindIndex(match =>
            {
                if ((match.bodybonename == parts) && (match.equipitem == equipment.roleName)) return true;
                return false;
            });

            if (isHit == -1)
            {
                equipment.avatar.transform.SetParent(dest);
                //equipment.transform.position = dest.position;


                //---set up DummyIK
                ooik.isEquipping = true;
                ooik.equippedAvatar = this.gameObject;

                //---set up for animation
                //string[] roles = manim.GetRoleSpecifiedAvatar(equipment.name);
                AvatarEquipSaveClass ave = new AvatarEquipSaveClass();
                ave.bodybonename = parts;
                ave.equipitem = equipment.roleName;  //roles[0];
                ave.position = olo.GetPosition();
                ave.rotation = olo.GetRotation();
                equipDestinations.list.Add(ave);

                //---set up IK marker
                GameObject ikhandle = olo.relatedHandleParent;
                //ikhandle.transform.position = dest.position;
                ikhandle.SetActive(false);

                //---set up object self
                olo.SetColliderAvailable(false);
                equipment.avatar.layer = LayerMask.NameToLayer("Default");

            }
        }

        /// <summary>
        /// To exchange before equipment and after equipment.
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="equipment">animation avatar</param>
        public void EquipExchange(HumanBodyBones parts, NativeAnimationAvatar equipment)
        {
            int isHit = equipDestinations.list.FindIndex(match =>
            {
                if ((match.bodybonename == parts) && (match.equipitem == equipment.roleName)) return true;
                return false;
            });

            if (isHit > -1)
            {
                UnequipObject(parts, equipment.roleName);
            }

            EquipObject(parts, equipment);
        }

        /// <summary>
        /// To equip an object to body part of specified NAME
        /// </summary>
        /// <param name="parts"></param>
        /// <param name="equipment">animation avatar</param>
        public void EquipObjectByName(string parts, NativeAnimationAvatar equipment)
        {
            foreach (HumanBodyBones value in Enum.GetValues(typeof(HumanBodyBones)))
            {
                string name = Enum.GetName(typeof(HumanBodyBones), value);
                if (parts == name)
                {
                    EquipObject(value, equipment);
                    break;
                }
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">CSV-string: [0] - HumanBodyBones position index, [1] - role name, [2] - isquip flag(</param>
        public void EquipObjectFromOuter(string param)
        {
            string[] prm = param.Split(',');
            int index = int.TryParse(prm[0], out index) ? index : 0;
            string name = prm[1];
            string isequip = prm[2];

            NativeAnimationAvatar nav = manim.GetCastInProject(name);
            if (nav != null)
            {
                if (isequip == "1")
                {
                    EquipExchange((HumanBodyBones)index, nav);
                }
                else
                { //---effectively NOT USE
                    EquipPositioning((HumanBodyBones)index, nav);
                }
            }
            /*
            GameObject pt = GameObject.Find(name);
            if (pt != null)
            {
                if (isequip == "1")
                {
                    //EquipObject((HumanBodyBones)index, pt);
                    EquipExchange((HumanBodyBones)index, pt);
                }
                else
                {
                    EquipPositioning((HumanBodyBones)index, pt);
                }
            }
            */
            
        }

        /// <summary>
        /// Un-equip the object
        /// </summary>
        /// <param name="parts">body bone name</param>
        /// <param name="equipmentName">role name of the object</param>
        public void UnequipObject(HumanBodyBones parts, string equipmentName)
        {
            //ManageAnimation manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

            Transform dest = this.gameObject.GetComponent<Animator>().GetBoneTransform(parts);

            AvatarEquipSaveClass aes =  equipDestinations.list.Find(match =>
            {
                if ((match.bodybonename == parts) && (match.equipitem == equipmentName)) return true;
                return false;
            });
            if (aes != null)
            {
                NativeAnimationAvatar naa =  manim.GetCastInProject(aes.equipitem);
                GameObject equip = naa.avatar; // dest.Find(equipmentName).gameObject;
                if (equip)
                {
                    OperateLoadedBase olb = equip.GetComponent<OperateLoadedBase>();
                    GameObject ikhandle = olb.relatedHandleParent;

                    equip.transform.SetParent(GameObject.Find("View Body").transform);
                    equip.layer = LayerMask.NameToLayer("Player");
                    olb.SetColliderAvailable(true);

                    if (ikhandle != null)
                    {
                        OtherObjectDummyIK ooik = ikhandle.GetComponent<OtherObjectDummyIK>();
                        ooik.isEquipping = false;
                        ooik.equippedAvatar = null;

                        //---recover transform of ik-marker to effective object transform.
                        //   because transform of ik-market and effective object don't link during equipment.
                        ikhandle.transform.position = equip.transform.position;
                        ikhandle.transform.rotation = equip.transform.rotation;
                        ikhandle.SetActive(true);

                        equipDestinations.list.Remove(aes);
                    }
                }
            }
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">csv string: [0] body bone name, [1] - role name</param>
        public void UnequipObjectFromOuter(string param)
        {
            string[] prm = param.Split(',');
            int index = int.TryParse(prm[0], out index) ? index : 0;
            string name = prm[1];

            UnequipObject((HumanBodyBones)index, name);
        }
        /*
        public int GetFixMoving()
        {
            return isFixMoving ? 1 : 0;
        }
        public void SetFixMoving(bool flag)
        {
            isFixMoving = flag;
            if (flag)
            {
                transform.gameObject.GetComponent<CapsuleCollider>().enabled = false;
                relatedHandleParent.SetActive(false);
            }
            else
            {
                transform.gameObject.GetComponent<CapsuleCollider>().enabled = true;
                relatedHandleParent.SetActive(true);
            }
        }
        public void SetFixMovingFromOuter(string param)
        {
            SetFixMoving(param == "1" ? true : false);
        }
        */

        //===============================================================================================================================
        //  IK handle
        //  
        public void ResetIKMappingList ()
        {
            ikMappingList.Clear();
        }
        public string GetIKTarget(IKBoneType parts)
        {
            string js = "";

            BipedIK bik = gameObject.GetComponent<BipedIK>();
            if (parts == IKBoneType.EyeViewHandle)
            {
                VRMLookAtHead vlook = gameObject.GetComponent<VRMLookAtHead>();
                js = vlook.Target.gameObject.name;
            }
            else if (parts == IKBoneType.LookAt)
            {
                js = bik.solvers.lookAt.target.gameObject.name;
            }
            else if (parts == IKBoneType.Aim)
            {
                js = bik.solvers.aim.target.gameObject.name;
            }
            else if (parts == IKBoneType.Chest)
            {
                js = bik.solvers.spine.target.gameObject.name;
            }
            else if (parts == IKBoneType.Pelvis)
            {
                js = bik.solvers.pelvis.target.gameObject.name;
            }
            //-------------
            else if (parts == IKBoneType.LeftLowerArm)
            {
                js = bik.solvers.leftHand.bendGoal.gameObject.name;
            }
            else if (parts == IKBoneType.LeftHand)
            {
                js = bik.solvers.leftHand.target.gameObject.name;
            }
            else if (parts == IKBoneType.RightLowerArm)
            {
                js = bik.solvers.rightHand.bendGoal.gameObject.name;
            }
            else if (parts == IKBoneType.RightHand)
            {
                js = bik.solvers.rightHand.target.gameObject.name;
            }
            //---------------
            else if (parts == IKBoneType.LeftLowerLeg)
            {
                js = bik.solvers.leftFoot.bendGoal.gameObject.name;
            }
            else if (parts == IKBoneType.LeftLeg)
            {
                js = bik.solvers.leftFoot.target.gameObject.name;
            }
            else if (parts == IKBoneType.RightLowerLeg)
            {
                js = bik.solvers.rightFoot.bendGoal.gameObject.name;
            }
            else if (parts == IKBoneType.RightLeg)
            {
                js = bik.solvers.rightFoot.target.gameObject.name;
            }

            //---if js exists in IKBoneType, set "self"
            IKBoneType jud = ManageAnimation.GetVRMIKBoneEnum(js);
            if (jud == IKBoneType.IKParent)
            {
                if (js == "Main Camera")
                {
                    js = "maincamera";
                }
                else
                {
                    NativeAnimationAvatar nav = manim.GetCastByAvatar(js);
                    if (nav != null)
                    {
                        js = nav.roleName;
                    }
                }
                
            }
            else
            {
                js = "self";
            }
            

            

            return js;
        }
        public void GetIKTargetFromOuter(int parts)
        {
            IKBoneType ikparts = (IKBoneType)parts;


            string js = GetIKTarget(ikparts);

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }
        public void SetIKTarget(IKBoneType parts, string name, bool isPlayMode = false)
        {
            BipedIK bik = gameObject.GetComponent<BipedIK>();
            Transform target = null;
            if (name.ToLower() == "self")
            {
                target = GetIKHandle(ManageAnimation.GetVRMIKBoneName(parts)).transform;
            }
            else if (name.ToLower() == "maincamera")
            {
                target = Camera.main.transform;
            }
            else
            {
                NativeAnimationAvatar nav = manim.GetCastInProject(name);
                if (nav == null) return;
                if (nav.avatar == null) return;

                target = nav.avatar.transform;
            }

            if (target == null) return;

            if (parts == IKBoneType.EyeViewHandle)
            {
                VRMLookAtHead vlook = gameObject.GetComponent<VRMLookAtHead>();
                vlook.Target = target;
            }
            else if (parts == IKBoneType.LookAt)
            {
                bik.solvers.lookAt.target = target;
            }
            else if (parts == IKBoneType.Aim)
            {
                bik.solvers.aim.target = target;
            }
            else if (parts == IKBoneType.Chest)
            {
                bik.solvers.spine.target = target;
            }
            else if (parts == IKBoneType.Pelvis)
            {
                bik.solvers.pelvis.target = target;
            }
            //-------------------
            else if (parts == IKBoneType.LeftLowerArm)
            {
                bik.solvers.leftHand.bendGoal = target;
            }
            else if (parts == IKBoneType.LeftHand)
            {
                bik.solvers.leftHand.target = target;
            }
            else if (parts == IKBoneType.RightLowerArm)
            {
                bik.solvers.rightHand.bendGoal = target;
            }
            else if (parts == IKBoneType.RightHand)
            {
                bik.solvers.rightHand.target = target;
            }
            //------------------
            else if (parts == IKBoneType.LeftLowerLeg)
            {
                bik.solvers.leftFoot.bendGoal = target;
            }
            else if (parts == IKBoneType.LeftLeg)
            {
                bik.solvers.leftFoot.target = target;
            }
            else if (parts == IKBoneType.RightLowerLeg)
            {
                bik.solvers.rightFoot.bendGoal = target;
            }
            else if (parts == IKBoneType.RightLeg)
            {
                bik.solvers.rightFoot.target = target;
            }

            if (!isPlayMode)
            { //---when animation play mode, don't save information.
                int inx = ikMappingList.FindIndex(item =>
                {
                    if (item.parts == parts) return true;
                    return false;
                });

                if (name.ToLower() == "self")
                { //---if ik handle is self(original handle), remove this handle list.
                    ikMappingList.RemoveAt(inx);
                }
                else
                { //---if it is not self, set up as special IK handle
                    AvatarIKMappingClass aik = new AvatarIKMappingClass();
                    aik.parts = parts;
                    aik.name = name;
                    if (inx == -1)
                    {
                        ikMappingList.Add(aik);
                    }
                    else
                    {
                        ikMappingList[inx].name = name;
                    }
                }
            }
            
        }
        public void SetIKTargetFromOuter(string param)
        {
            string[] arr = param.Split(',');
            if (arr.Length < 2) return;

            int iparts = int.TryParse(arr[0], out iparts) ? iparts : 99;
            if (iparts != 99)
            {
                IKBoneType ikbonetype = (IKBoneType)iparts;
                SetIKTarget(ikbonetype, arr[1]);
            }
            
        }

        public void SetHeadLock(int flag)
        {
            CCDIK cik = GetComponent<CCDIK>();
            cik.solver.maxIterations = flag;
        }
        public int GetHeadLock()
        {
            CCDIK cik = GetComponent<CCDIK>();
            return cik.solver.maxIterations;
        }
        public void RegetTextureConfig()
        {
            ManageAvatarTransform mat = GetComponent<ManageAvatarTransform>();
            List<GameObject> meshcnt = mat.CheckSkinnedMeshAvailable();
            List<string> retarr = new List<string>();
            GameObject item = meshcnt[0];
            {
                SkinnedMeshRenderer skn;
                MeshRenderer mr;
                Material[] mats = null;
                if (item.TryGetComponent<SkinnedMeshRenderer>(out skn))
                {
                    mats = skn.materials;
                }
                else if (item.TryGetComponent<MeshRenderer>(out mr))
                {
                    mats = mr.materials;
                }
                if (mats != null)
                {
                    for (int i = 0; i < mats.Length; i++)
                    {
                        Material mate = mats[i];

                        userSharedProperties.shaderName = mate.shader.name;
                        userSharedProperties.color = mate.GetColor("_Color");
                        userSharedProperties.blendmode = mate.GetFloat("_BlendMode");
                        userSharedProperties.cullmode = mate.GetFloat("_CullMode");
                        userSharedProperties.emissioncolor = mate.GetColor("_EmissionColor");
                        userSharedProperties.shadetexcolor = mate.GetColor("_ShadeColor");
                        userSharedProperties.shadingtoony = mate.GetFloat("_ShadeToony");
                        userSharedProperties.rimcolor = mate.GetColor("_RimColor");
                        userSharedProperties.rimfresnel = mate.GetFloat("_RimFresnelPower");
                        userSharedProperties.srcblend = mate.GetFloat("_SrcBlend");
                        userSharedProperties.dstblend = mate.GetFloat("_DstBlend");
                    }
                }
                
            }
        }
        public void GetTextureConfig(string param)
        {
            string[] arr = param.Split(",");
            string name = arr[0];
            string type = arr[1]; //float, color
            /*
            ManageAvatarTransform mat = GetComponent<ManageAvatarTransform>();
            List<GameObject> meshcnt = mat.CheckSkinnedMeshAvailable();
            List<string> retarr = new List<string>();
            meshcnt.ForEach(item =>
            {
                SkinnedMeshRenderer skn = item.GetComponent<SkinnedMeshRenderer>();
                Material[] mats = skn.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    float v = 0f;
                    Color cv = Color.white;
                    if (type == "float")
                    {
                        v = mats[i].GetFloat(name);
                        retarr.Add(skn.name + ":" + mats[i].name + ":" + param + "=" + v.ToString());
                    }
                    else if (type == "color")
                    {
                        cv = mats[i].GetColor(name);
                        retarr.Add(skn.name + ":" + mats[i].name + ":" + param + "=#" + ColorUtility.ToHtmlStringRGBA(cv));
                    }
                    
                    
                }
            });
            */
            string ret = JsonUtility.ToJson(userSharedProperties);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public void SetTextureConfig(MaterialProperties vmat, bool isSaveOnly = false)
        {
            ManageAvatarTransform mat = GetComponent<ManageAvatarTransform>();
            List<GameObject> meshcnt = mat.CheckSkinnedMeshAvailable();

            meshcnt.ForEach(item =>
            {
                SkinnedMeshRenderer skn = item.GetComponent<SkinnedMeshRenderer>();
                Material[] mats = skn.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (!isSaveOnly) mats[i].SetFloat("_SrcBlend", vmat.srcblend);
                    userSharedProperties.srcblend = vmat.srcblend;

                    if (!isSaveOnly) mats[i].SetFloat("_DstBlend", vmat.dstblend);
                    userSharedProperties.dstblend = vmat.dstblend;

                    if (!isSaveOnly) mats[i].SetColor("_Color", vmat.color);
                    userSharedProperties.color = vmat.color;

                    if (!isSaveOnly) mats[i].SetFloat("_BlendMode", vmat.blendmode);
                    userSharedProperties.blendmode = vmat.blendmode;

                    if (!isSaveOnly) mats[i].SetFloat("_CullMode", vmat.cullmode);
                    userSharedProperties.cullmode = vmat.cullmode;

                    if (!isSaveOnly) mats[i].EnableKeyword("EMISSION");
                    if (!isSaveOnly) mats[i].SetColor("_EmissionColor", vmat.emissioncolor);
                    userSharedProperties.emissioncolor = vmat.emissioncolor;

                    if (!isSaveOnly) mats[i].SetColor("_ShadeColor", vmat.shadetexcolor);
                    userSharedProperties.shadetexcolor = vmat.shadetexcolor;

                    if (!isSaveOnly) mats[i].SetFloat("_ShadeToony", vmat.shadingtoony);
                    userSharedProperties.shadingtoony = vmat.shadingtoony;

                    if (!isSaveOnly) mats[i].SetColor("_RimColor", vmat.rimcolor);
                    userSharedProperties.rimcolor = vmat.rimcolor;

                    if (!isSaveOnly) mats[i].SetFloat("_RimFresnelPower", vmat.rimfresnel);
                    userSharedProperties.rimfresnel = vmat.rimfresnel;

                }
            });
        }
        public void SetTextureConfigFromOuter(string param)
        {
            string[] arr = param.Split(",");
            string name = arr[0];
            //string type = arr[1]; //float, color
            float val = float.TryParse(arr[1], out val) ? val : 0f;
            Color cval = ColorUtility.TryParseHtmlString(arr[1], out cval) ? cval : Color.white;

            ManageAvatarTransform mat = GetComponent<ManageAvatarTransform>();
            List<GameObject> meshcnt = mat.CheckSkinnedMeshAvailable();

            meshcnt.ForEach(item =>
            {
                SkinnedMeshRenderer skn = item.GetComponent<SkinnedMeshRenderer>();
                Material[] mats = skn.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (name.ToLower() == "srcblend")
                    {
                        mats[i].SetFloat("_SrcBlend", val);
                        userSharedProperties.srcblend = val;
                    }
                    else if (name.ToLower() == "dstblend")
                    {
                        mats[i].SetFloat("_DstBlend", val);
                        userSharedProperties.dstblend = val;
                    }
                    else if (name.ToLower() == "color")
                    {
                        mats[i].SetColor("_Color", cval);
                        userSharedProperties.color = cval;
                    }
                    else if (name.ToLower() == "renderingtype")
                    {
                        mats[i].SetFloat("_BlendMode", val);
                        userSharedProperties.blendmode = val;
                    }
                    else if (name.ToLower() == "cullmode")
                    { //0 - off, 1 - front, 2 - back
                        mats[i].SetFloat("_CullMode", val);
                        userSharedProperties.cullmode = val;
                    }
                    else if (name.ToLower() == "emissioncolor")
                    {
                        mats[i].EnableKeyword("EMISSION");
                        mats[i].SetColor("_EmissionColor", cval);
                        userSharedProperties.emissioncolor = cval;
                    }
                    else if (name.ToLower() == "shadetexcolor")
                    {
                        mats[i].SetColor("_ShadeColor", cval);
                        userSharedProperties.shadetexcolor = cval;
                    }
                    else if (name.ToLower() == "shadingtoony")
                    {
                        mats[i].SetFloat("_ShadeToony", val);
                        userSharedProperties.shadingtoony = val;
                    }
                    else if (name.ToLower() == "rimcolor")
                    {
                        mats[i].SetColor("_RimColor", cval);
                        userSharedProperties.rimcolor = cval;
                    }
                    else if (name.ToLower() == "rimfresnel")
                    {
                        mats[i].SetFloat("_RimFresnelPower", val);
                        userSharedProperties.rimfresnel = val;
                    }
                }
            });
        }
        public Sequence SetMaterialTween(Sequence seq, string propname, MaterialProperties value, float duration)
        {
            //string[] prm = param.Split(',');
            string mat_name = name;
            //string propname = prm[1];

            ManageAvatarTransform mat = GetComponent<ManageAvatarTransform>();
            List<GameObject> meshcnt = mat.CheckSkinnedMeshAvailable();
            meshcnt.ForEach(item =>
            {
                SkinnedMeshRenderer skn = item.GetComponent<SkinnedMeshRenderer>();
                Material[] mats = skn.materials;
                for (int i = 0; i < mats.Length; i++)
                {

                    if (propname.ToLower() == "srcblend")
                    { //SrcBlend

                        seq.Join(mats[i].DOFloat(value.srcblend, "_SrcBlend", duration));
                    }
                    else if (propname.ToLower() == "dstblend")
                    { //DstBlend

                        seq.Join(mats[i].DOFloat(value.dstblend, "_DstBlend", duration));

                    }
                    else if (propname.ToLower() == "color")
                    {
                        seq.Join(mats[i].DOColor(value.color, duration));
                    }
                    else if (propname.ToLower() == "renderingtype")
                    {
                        if (mats[i].shader.name.ToLower() == "standard")
                        {
                            seq.Join(mats[i].DOFloat(value.blendmode, "_Mode", duration));
                        }
                        else if (mats[i].shader.name.ToLower() == "vrm/mtoon")
                        {
                            seq.Join(mats[i].DOFloat(value.blendmode, "_BlendMode", duration));
                        }
                    }
                    else if (propname.ToLower() == "cullmode")
                    { //0 - off, 1 - front, 2 - back
                        if (mats[i].shader.name.ToLower() == "vrm/mtoon")
                        {
                            seq.Join(mats[i].DOFloat(value.cullmode, "_CullMode", duration));
                        }
                    }
                    else if (propname.ToLower() == "emissioncolor")
                    { //emission color
                        seq.Join(DOVirtual.DelayedCall(duration, () => mats[i].EnableKeyword("EMISSION")));
                        seq.Join(mats[i].DOColor(value.emissioncolor, "_EmissionColor", duration));
                    }
                    else if (propname.ToLower() == "shadetexcolor")
                    { //shade texture color
                        if (mats[i].shader.name.ToLower() == "vrm/mtoon")
                        {
                            seq.Join(mats[i].DOColor(value.shadetexcolor, "_ShadeColor", duration));
                        }
                    }
                    else if (propname.ToLower() == "shadingtoony")
                    { //shading toony
                        seq.Join(mats[i].DOFloat(value.shadingtoony, "_ShadeToony", duration));
                    }
                    else if (propname.ToLower() == "rimcolor")
                    { //rim color
                        if (mats[i].shader.name.ToLower() == "vrm/mtoon")
                        {
                            seq.Join(mats[i].DOColor(value.rimcolor, "_RimColor", duration));
                        }
                    }
                    else if (propname.ToLower() == "rimfresnel")
                    { //rim fresnel power
                        seq.Join(mats[i].DOFloat(value.rimfresnel, "_RimFresnelPower", duration));
                    }
                }
            });

            return seq;
        }
    }

}
