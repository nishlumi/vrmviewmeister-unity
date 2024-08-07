using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

using UserHandleSpace;
using RootMotion.FinalIK;
using VRM;
using VRMShaders;
using UniVRM10;
using DG.Tweening;
using LumisIkApp;
using UniGLTF;
using TriLibCore.Dae.Schema;
using RootMotion.Demos;

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

        const string PREFIX_PROXY = "PROX:";


        /**
         *  This class is manager function, all VRM
         *  Variables only...
         */

        

        //private VRMImporterContext context;
        private Bounds bodyInfoTPose;
        private List<Vector3> bodyinfoList;
        private SkinnedMeshRenderer BSFace;
        private List<SkinnedMeshRenderer> BSMeshs;

        public List<BasicBlendShapeKey> blendShapeList;

        public bool isMoveMode;

        public int equipType; //Flag for animation: -1 - unequip, 0 - no change, 1 - to equip
        public AvatarEquipmentClass equipDestinations;
        public AvatarGravityClass gravityList;
        public List<AvatarIKMappingClass> ikMappingList;
        public bool IsGoalRotationNatural;

        private Vrm10Instance vrminstance;
        private UniVRM10.VRM10Viewer.VRM10Blinker blink;
        public MaterialProperties userSharedProperties;

        private ManageAnimation manim;

        public bool isHandPosing = true;
        public LeftHandPoseController LeftHandCtrl;
        public RightHandPoseController RightHandCtrl;
        public AvatarFingerForHPC LeftFingerBackupHPC;
        public AvatarFingerForHPC RightFingerBackupHPC;

        public int LeftCurrentHand = 0;
        public float LeftHandValue = 0;
        public int RightCurrentHand = 0;
        public float RightHandValue = 0;
        public AvatarFingerInHand LeftFingerBkup;
        public AvatarFingerInHand RightFingerBkup;

        private OperateVRMExporter ovrmex;
        private Vrm10AnimationInstance vrmaInst;
        private Animation VrmaNativeAnimation;
        public UserAnimationState animationStartFlag;  //1 - play, 0 - stop, 2 - playing, 3 - seeking, 4 - pause
        public float animationRemainTime;
        private float local_animRemainTime;
        protected bool triggerCurrentAnimOnFinished;
        private string VrmaFileName;
        private string animationClipName;

        //---stocked sequence for auto blendshape
        Coroutine m_coroutine;
        private Sequence autoBlendShapeSequence;
        private int IsAutoBlendShape = 0;
        private float autoBlendShapeOpeningSeconds = 0.1f;
        private float autoBlendShapeClosingSeconds = 0.1f;
        private float autoBlendShapeInterval = 0.2f;
        private bool IsKillAutoBlendShape;
        public UserAnimationState AutoBlendShapeFlag;

 

        // Start is called before the first frame update
        override protected void Awake()
        {
            base.Awake();


            SaveDefaultTransform(true, true);
            BSMeshs = new List<SkinnedMeshRenderer>();

            targetType = AF_TARGETTYPE.VRM;

            bodyinfoList = new List<Vector3>();
            blendShapeList = new List<BasicBlendShapeKey>();
            gravityList = new AvatarGravityClass();
            equipType = 0;
            equipDestinations = new AvatarEquipmentClass();
            ikMappingList = new List<AvatarIKMappingClass>();
            //userSharedProperties = new MaterialProperties();

            LeftFingerBkup = new AvatarFingerInHand();
            RightFingerBkup = new AvatarFingerInHand();
            vrminstance = GetComponent<Vrm10Instance>();

        }
        override protected void Start() 
        {
            base.Start();
            
            manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

            blink = GetComponent<UniVRM10.VRM10Viewer.VRM10Blinker>();
            /*blendShapeList.Clear();
            
            List<string> lst = ListAvatarBlendShape();
            lst.ForEach(item =>
            {
                string[] arr = item.Split('=');
                float value = -1f; // float.TryParse(arr[1], out value) ? value : -1f;
                BasicStringFloatList lsf = new BasicStringFloatList(arr[0], value);
                blendShapeList.Add(lsf);

            });*/

            //---set  up IKMapping.
            constructIKMappingList();

            //---set up exporter
            ovrmex = new OperateVRMExporter();

            triggerCurrentAnimOnFinished = false;
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
            if (vrminstance != null)
            {
                //vrminstance.Runtime.Process();
                //BSMeshs.ForEach(action => actio);
                //vrminstance.Runtime.ReconstructSpringBone();
            }

        }
        private void LateUpdate()
        {
            if (vrminstance != null)
            {
                //vrminstance.Runtime.ReconstructSpringBone();
                //vrminstance.SpringBone.Springs[0].EnumHeadTail(
            }
        }
        override protected void OnDestroy()
        {
            base.OnDestroy();

            foreach (KeyValuePair<string, Material> kvp in userSharedMaterials)
            {
                Material mat = kvp.Value;
                { //---nullize old texture ( not destroy here )
                    if (userSharedTextureFiles.ContainsKey(kvp.Key))
                    {
                        if ((userSharedTextureFiles[kvp.Key].textureIsCamera == 0) && (userSharedTextureFiles[kvp.Key].texturePath != ""))
                        {
                            manim.UnReferMaterial(OneMaterialType.Texture, userSharedTextureFiles[kvp.Key].texturePath);
                        }

                        mat.SetTexture("_MainTex", null);
                    }

                }
                mat = null;
            }
            userSharedMaterials.Clear();
            userSharedTextureFiles.Clear();
            backupTextureFiles.Clear();
            Renderer[] mrs = GetComponentsInChildren<Renderer>();
            foreach (var mr in mrs)
            {
                foreach (var mat in mr.sharedMaterials)
                {
                    Destroy(mat);
                }
            }
            foreach (var mr in mrs)
            {
                foreach (var mat in mr.materials)
                {
                    Destroy(mat);
                }
            }
        }
        /*
        public VRMImporterContext GetContext()
        {
            return context;
        }
        public void SetContext(VRMImporterContext cont)
        {
            context = cont;
        }
        */
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
            VvmIk vik = transform.GetComponent<VvmIk>();

            if (bik != null) bik.enabled = flag;
            if (cik != null) cik.enabled = flag;
            if (vik != null) vik.enabled = flag;
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

        /// <summary>
        /// Change Move-Mode (-1 is no change)
        /// </summary>
        /// <param name="flag"></param>
        public void ChangeToggleAvatarMoveFromOuter(int flag)
        {
            if (flag >= 0) //---flag is 1-true, 0-false, -1 - flag no change
            {
                isMoveMode = (flag == 1) ? true : false;

                if (relatedHandleParent == null) return;
                if (relatedTrueIKParent == null) return;
            }
            //relatedHandleParent.GetComponent<BoxCollider>().enabled = isMoveMode;
            relatedTrueIKParent.GetComponent<BoxCollider>().enabled = isMoveMode;
        }
        public GameObject GetIKHandle(string name)
        {
            GameObject ret = null;

            /*int cnt = relatedHandleParent.transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                if (relatedHandleParent.transform.GetChild(i).name == name)
                {
                    ret = relatedHandleParent.transform.GetChild(i).gameObject;
                    break;
                }
            }*/
            Transform[] children = relatedHandleParent.transform.GetComponentsInChildren<Transform>();
            foreach (var child in children)
            {
                if (child.name == name)
                {
                    ret = child.gameObject;
                    break;
                }
            }
            //ret = relatedHandleParent.transform.Find(name).gameObject;
            return ret;
        }
        public GameObject GetIKHandleByPartsName(string name)
        {
            GameObject ret = null;

            /*int cnt = relatedHandleParent.transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                if (relatedHandleParent.transform.GetChild(i).name == name)
                {
                    ret = relatedHandleParent.transform.GetChild(i).gameObject;
                    break;
                }
            }*/
            UserHandleOperation[] children = relatedHandleParent.transform.GetComponentsInChildren<UserHandleOperation>();
            foreach (var child in children)
            {
                if (child.PartsName == name)
                {
                    ret = child.gameObject;
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
            VvmIk vik = gameObject.TryGetComponent<VvmIk>(out vik) ? vik : null;
            LeftHandPoseController lhand = gameObject.TryGetComponent<LeftHandPoseController>(out lhand) ? lhand : null;
            RightHandPoseController rhand = gameObject.TryGetComponent<RightHandPoseController>(out rhand) ? rhand : null;

            if (rhand != null) rhand.enabled = flag;
            if (lhand != null) lhand.enabled = flag;
            if (bik != null) bik.enabled = flag;
            if (cik != null) cik.enabled = flag;
            if (vik != null) vik.enabled = flag;

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

            if (leftlowerarm.GetComponent<RotationLimitHinge>() != null) leftlowerarm.GetComponent<RotationLimitHinge>().enabled = flag == 1 ? true : false;
            if (rightlowerarm.GetComponent<RotationLimitHinge>() != null) rightlowerarm.GetComponent<RotationLimitHinge>().enabled = flag == 1 ? true : false;
            if (leftlowerleg.GetComponent<RotationLimitHinge>() != null) leftlowerleg.GetComponent<RotationLimitHinge>().enabled = flag == 1 ? true : false;
            if (rightlowerleg.GetComponent<RotationLimitHinge>() != null) rightlowerleg.GetComponent<RotationLimitHinge>().enabled = flag == 1 ? true : false;
            if (leftfoot.GetComponent<RotationLimitHinge>() != null) leftfoot.GetComponent<RotationLimitHinge>().enabled = flag == 1 ? true : false;
            if (rightfoot.GetComponent<RotationLimitHinge>() != null) rightfoot.GetComponent<RotationLimitHinge>().enabled = flag == 1 ? true : false;

        }

        /// <summary>
        /// Change Enable/Disable of IK-system
        /// </summary>
        /// <param name="flag"></param>
        public IEnumerator EnableIKOperationMode(bool flag)
        {
            float weight = 0f;
            if (flag) weight = 1.0f;

            BipedIK bik = GetComponent<BipedIK>();
            CCDIK cik = GetComponent<CCDIK>();
            VvmIk vik = GetComponent<VvmIk>();

            if (bik != null)
            {                
                //---change IK to disable
                if (flag == false)
                {

                    bik.fixTransforms = flag;
                    cik.fixTransforms = flag;
                    yield return null;

                    UserHandleOperation[] uhos = relatedHandleParent.transform.GetComponentsInChildren<UserHandleOperation>();
                    foreach (var uho in uhos)
                    {
                        uho.IsFixTransform = flag;
                    }
                    //GetIKHandle("Chest/LeftShoulder").GetComponent<UserHandleOperation>().IsFixTransform = flag;
                    //GetIKHandle("Chest/RightShoulder").GetComponent<UserHandleOperation>().IsFixTransform = flag;
                    bik.references.leftForearm.GetComponent<RotationLimitHinge>().enabled = flag;
                    bik.references.rightForearm.GetComponent<RotationLimitHinge>().enabled = flag;
                    bik.references.leftCalf.GetComponent<RotationLimitHinge>().enabled = flag;
                    bik.references.rightCalf.GetComponent<RotationLimitHinge>().enabled = flag;

                }

                //---head
                cik.solver.SetIKPositionWeight(weight);

                //---lookAt
                bik.solvers.lookAt.headWeight = weight;
                bik.solvers.lookAt.head.weight = weight;
                bik.solvers.lookAt.IKPositionWeight = weight;

                //---Aim
                bik.solvers.aim.IKPositionWeight = weight;

                //---chest
                bik.solvers.spine.IKPositionWeight = weight;

                //---left lower arm
                bik.solvers.leftHand.bendModifierWeight = weight;

                //---left hand
                bik.solvers.leftHand.IKPositionWeight = weight;
                bik.solvers.leftHand.IKRotationWeight = weight;

                //---right lower arm
                bik.solvers.rightHand.bendModifierWeight = weight;

                //---right hand
                bik.solvers.rightHand.IKPositionWeight = weight;
                bik.solvers.rightHand.IKRotationWeight = weight;

                //---pelvis
                bik.solvers.pelvis.positionWeight = weight;
                bik.solvers.pelvis.rotationWeight = weight;


                //---left lower leg
                bik.solvers.leftFoot.bendModifierWeight = weight;

                //---left foot
                bik.solvers.leftFoot.IKPositionWeight = weight;
                bik.solvers.leftFoot.IKRotationWeight = weight;

                //---right lower leg
                bik.solvers.rightFoot.bendModifierWeight = weight;

                //---right foot
                bik.solvers.rightFoot.IKPositionWeight = weight;
                bik.solvers.rightFoot.IKRotationWeight = weight;

                //---Change IK to enable
                if (flag == true)
                {
                    bik.fixTransforms = flag;
                    cik.fixTransforms = flag;


                    UserHandleOperation[] uhos = relatedHandleParent.transform.GetComponentsInChildren<UserHandleOperation>();
                    foreach (var uho in uhos)
                    {
                        uho.IsFixTransform = flag;
                    }
                    bik.references.leftCalf.GetComponent<RotationLimitHinge>().enabled = flag;
                    bik.references.rightCalf.GetComponent<RotationLimitHinge>().enabled = flag;
                    bik.references.leftForearm.GetComponent<RotationLimitHinge>().enabled = flag;
                    bik.references.rightForearm.GetComponent<RotationLimitHinge>().enabled = flag;
                    yield return null;

                }
            }
            else if (vik != null)
            {
                //---change IK to disable
                if (flag == false)
                {
                    vik.IsApplyIK = false;
                }

                //---head
                

                //---lookAt
                vik.lookAtHeadWeight = weight;
                vik.lookAtWeight = weight;

                //---Aim

                //---chest

                //---pelvis

                //---left lower arm
                vik.LeftLowerArmWeight = weight;

                //---left hand
                vik.LeftHandPositionWeight = weight;
                vik.LeftHandRotationWeight = weight;

                //---right lower arm
                vik.RightLowerArmWeight = weight;

                //---right hand
                vik.RightHandPositionWeight = weight;
                vik.RightHandRotationWeight = weight;

                //---pelvis
                bik.solvers.pelvis.positionWeight = weight;
                bik.solvers.pelvis.rotationWeight = weight;


                //---left lower leg
                vik.LeftLowerLegWeight = weight;

                //---left foot
                vik.LeftFootPositionWeight = weight;
                vik.LeftFootRotationWeight = weight;

                //---right lower leg
                vik.RightLowerLegWeight = weight;

                //---right foot
                vik.RightFootPositionWeight = weight;
                vik.RightFootRotationWeight = weight;

                //---Change IK to enable
                if (flag == true)
                {
                    vik.IsApplyIK = flag;
                }
            }
            
        }

        /// <summary>
        /// Convert HumanBodyBones transform to This app IK transform
        /// </summary>
        /// <returns></returns>
        public IEnumerator ApplyBoneTransformToIKTransform(Animator animator)
        {
            const int BIPEDIK = 1;
            const int VVMIK = 2;
            //Animator animator = GetComponent<Animator>();
            BipedIK bik = GetComponent<BipedIK>();
            CCDIK cik = GetComponent<CCDIK>();
            VvmIk vik = GetComponent<VvmIk>();
            int isIK = 0;
            if (bik != null) isIK = BIPEDIK;
            if (vik != null) isIK = VVMIK;

            yield return null;

            //---start condition: a weight of each IK target == 0

            Transform bleye = animator.GetBoneTransform(HumanBodyBones.LeftEye);
            Transform breye = animator.GetBoneTransform(HumanBodyBones.RightEye);
            Transform bhead = animator.GetBoneTransform(HumanBodyBones.Head);
            Transform bneck = animator.GetBoneTransform(HumanBodyBones.Neck);
            Transform bupperchest = animator.GetBoneTransform(HumanBodyBones.UpperChest);
            Transform bchest = animator.GetBoneTransform(HumanBodyBones.Chest);
            Transform bspine = animator.GetBoneTransform(HumanBodyBones.Spine);
            Transform bleftshoulder = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
            Transform brightshoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
            Transform bleftlowerarm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            Transform brightlowerarm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            Transform blefthand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            Transform brighthand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            Transform bpelvis = animator.GetBoneTransform(HumanBodyBones.Hips);
            Transform bleftlowerleg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            Transform brightlowerleg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            Transform bleftfoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            Transform brightfoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);


            //---spine and hips---------------------------------------------------------------------------
            Transform pelvis = relatedHandleParent.transform.Find("Pelvis");
            if (pelvis != null)
            {
                if (isIK > 0)
                {
                    pelvis.position = bpelvis.position;
                    yield return null;
                    Vector3 bpeltmp1 = bpelvis.rotation.eulerAngles;
                    //bpeltmp1.y += -180f;
                    //bpeltmp1.y = Mathf.Repeat(bpeltmp1.y, 360f);
                    pelvis.rotation = Quaternion.Euler(bpeltmp1);
                    //pelvis.Rotate(0, 180, 0);
                    if (isIK == VVMIK)
                    {
                        Vector3 rot = bpelvis.rotation.eulerAngles + new Vector3(0, 180f, 0);
                        rot.y = Mathf.Repeat(rot.y + 180f, 360f) - 180f;
                        pelvis.rotation = Quaternion.Euler( rot );
                        
                    }
                    yield return null;
                }
                
            }
            //---UpperChest, Chest and Aim------------------------------------------------------------------
            Transform aim = relatedHandleParent.transform.Find("Aim");
            if (aim != null)
            {
                Vector3 pos = Vector3.zero;
                Vector3 rot = Vector3.zero;
                /*if (bupperchest != null)
                {
                    pos = bupperchest.position;
                    rot = bupperchest.rotation.eulerAngles;
                }
                else */
                if (isIK == BIPEDIK)
                {
                    if (bupperchest != null)
                    {
                        pos = bupperchest.position;
                        rot = bupperchest.rotation.eulerAngles;

                        aim.position = bupperchest.TransformPoint(bupperchest.localPosition - Vector3.back);
                    }
                    else if (bchest != null) 
                    {
                        pos = bchest.position;
                        rot = bchest.rotation.eulerAngles;

                        aim.position = bchest.TransformPoint(bchest.localPosition - Vector3.back);
                    }
                    

                    //aim.position = pos;
                    //aim.Translate(Vector3.back, Space.Self);
                    //aim.position = bchest.TransformPoint(bchest.localPosition + Vector3.back);
                    ///yield return null;

                    aim.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
                    yield return null;
                }
                else if (isIK == VVMIK)
                {
                    if (bchest != null)
                    {
                        pos = bchest.position;
                        rot = bchest.rotation.eulerAngles;
                    }
                    else
                    {
                        pos = bspine.position;
                        rot = bspine.rotation.eulerAngles;
                    }
                    aim.rotation = Quaternion.Euler(rot);
                    yield return null;
                    aim.position = pos;
                    yield return null;

                }
                
            }
            Transform chest = relatedHandleParent.transform.Find("Chest");
            if (chest != null)
            {
                if (bchest != null)
                {
                    chest.position = bchest.position;
                    chest.rotation = bchest.rotation;
                }
                else
                {
                    chest.position = bspine.position;
                    chest.rotation = bspine.rotation;
                }
                //Vector3 bnecktmp1 = bneck.localRotation.eulerAngles;
                ////bnecktmp1.y += -180f;
                //chest.position = bneck.position;
                //chest.rotation = Quaternion.Euler(bnecktmp1.x, bnecktmp1.y, bnecktmp1.z);
                yield return null;
            }

            //---Eye and Head----------------------------------------------------------------------------
            /*Transform eye = relatedHandleParent.transform.Find("EyeViewHandle");
            if (eye != null)
            {
                Vector3 pos = (bleye.position + breye.position) / 2f;
                //Vector3 rot = (bleye.rotation.eulerAngles + breye.rotation.eulerAngles) / 2f;
                pos = bleye.TransformPoint(bleye.localPosition - Vector3.back);

                //eye.rotation = Quaternion.Euler(rot);
                //yield return null;

                //pos.z--;
                eye.position = pos - new Vector3(0,0,0.15f);
                yield return null;

            }*/
            Transform head = relatedHandleParent.transform.Find("Head");
            if (head != null)
            {
                head.position = bhead.position;
                head.rotation = bhead.rotation;
                yield return null;
            }

            Transform lookat = relatedHandleParent.transform.Find("LookAt");
            if (lookat != null)
            {
                Vector3 pos = bhead.TransformPoint(bhead.localPosition - Vector3.back);
                //Vector3 rot = bhead.rotation.eulerAngles;
                //lookat.rotation = Quaternion.Euler(rot);
                yield return null;

                //pos.z--;
                lookat.position = pos - new Vector3(0, 0, 0.15f);
                yield return null;
            }


            //---arm and hand------------------------------------------------------------------------------
            Transform lhand = relatedHandleParent.transform.Find("LeftHand");
            if (lhand != null)
            {
                lhand.position = new Vector3(blefthand.position.x, blefthand.position.y, blefthand.position.z);
                yield return null;

                Vector3 rot = blefthand.rotation.eulerAngles;
                //rot.y += -180f;
                //rot.y = Mathf.Repeat(rot.y, 360f);
                lhand.rotation = Quaternion.Euler(rot);
                yield return null;
            }
            Transform lla = relatedHandleParent.transform.Find("LeftLowerArm");
            if (lla != null)
            {
                lla.position = bleftlowerarm.position;
                yield return null;
            }
            Transform lsho = relatedHandleParent.transform.Find("LeftShoulder");
            if (lsho != null)
            {
                lsho.position = bleftshoulder.position;
                lsho.rotation = bleftshoulder.rotation;
                yield return null;
            }
            Transform rhand = relatedHandleParent.transform.Find("RightHand");
            if (rhand != null)
            {
                rhand.position = new Vector3(brighthand.position.x, brighthand.position.y, brighthand.position.z);
                yield return null;

                Vector3 rot = brighthand.rotation.eulerAngles;
                //rot.y += -180f;
                //rot.y = Mathf.Repeat(rot.y, 360f);
                rhand.rotation = Quaternion.Euler(rot);
                yield return null;
            }
            Transform rla = relatedHandleParent.transform.Find("RightLowerArm");
            if (rla != null)
            {
                rla.position = brightlowerarm.position;
                yield return null;
            }
            Transform rsho = relatedHandleParent.transform.Find("RightShoulder");
            if (rsho != null)
            {
                rsho.position = brightshoulder.position;
                rsho.rotation = brightshoulder.rotation;
                yield return null;
            }


            //---leg and foot------------------------------------------------------------------------------
            Transform lfoot = relatedHandleParent.transform.Find("LeftLeg");
            if (lfoot != null)
            {
                lfoot.position = new Vector3(bleftfoot.position.x, bleftfoot.position.y, bleftfoot.position.z);
                yield return null;

                Vector3 rot = bleftfoot.rotation.eulerAngles;
                //rot.y += 180;
                lfoot.rotation = Quaternion.Euler(rot);
                
                yield return null;
            }
            Transform lllg = relatedHandleParent.transform.Find("LeftLowerLeg");
            if (lllg != null)
            {
                lllg.position = bleftlowerleg.position + new Vector3(0,0,-0.1f);
                yield return null;
            }

            Transform rfoot = relatedHandleParent.transform.Find("RightLeg");
            if (rfoot != null)
            {
                rfoot.position = new Vector3(brightfoot.position.x, brightfoot.position.y, brightfoot.position.z);
                yield return null;

                Vector3 rot = brightfoot.rotation.eulerAngles;
                //rot.y += 180;
                rfoot.rotation = Quaternion.Euler(rot);
                yield return null;
            }
            Transform rllg = relatedHandleParent.transform.Find("RightLowerLeg");
            if (rllg != null)
            {
                rllg.position = brightlowerleg.position + new Vector3(0, 0, -0.1f);
                yield return null;
            }


            //---finally, already recover IK weight and FixTransform
            //   as preparing, proceed Frame
            yield return null;
            
            //EnableIK(true);
            StartCoroutine(EnableIKOperationMode(true));
        }
        public void ApplyNaturalRotationGoal(string partsname)
        {
            GameObject leftarm = GetIKHandleByPartsName("leftarm");
            GameObject leftlowerarm = GetIKHandleByPartsName("leftlowerarm");
            GameObject rightarm = GetIKHandleByPartsName("rightarm");
            GameObject rightlowerarm = GetIKHandleByPartsName("rightlowerarm");
            GameObject leftleg = GetIKHandleByPartsName("leftleg");
            GameObject leftlowerleg = GetIKHandleByPartsName("leftlowerleg");
            GameObject rightleg = GetIKHandleByPartsName("rightleg");
            GameObject rightlowerleg = GetIKHandleByPartsName("rightlowerleg");
            if ((partsname == "leftarm") && leftarm.GetComponent<UserHandleOperation>().is_current_marker)
            {
                leftarm.transform.localRotation = leftlowerarm.transform.localRotation;
            }
            else if ((partsname == "rightarm") && rightarm.GetComponent<UserHandleOperation>().is_current_marker)
            {
                rightarm.transform.localRotation = rightlowerarm.transform.localRotation;
            }
            else if ((partsname == "leftleg") && leftleg.GetComponent<UserHandleOperation>().is_current_marker)
            {
                leftleg.transform.localRotation = leftlowerleg.transform.localRotation;
            }
            else if ((partsname == "rightleg") && rightleg.GetComponent<UserHandleOperation>().is_current_marker)
            {
                rightleg.transform.localRotation = rightlowerleg.transform.localRotation;
            }
        }
        //===============================================================================================================================
        //  Gravity 

        /// <summary>
        /// Initial enumrate Gravity information
        /// </summary>
        public void ListGravityInfo()
        {
            gravityList.list.Clear();
            /*
            VRMSpringBone[] bones = transform.GetComponentsInChildren<VRMSpringBone>();
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
            */
            //1.x
            List<Vrm10InstanceSpringBone.Spring> springs = vrminstance.SpringBone.Springs;
            for (int spi = 0; spi < springs.Count; spi++)
            {
                if (springs[spi].Joints.Count > 0)
                {                    
                    //---0.xとの互換性のため、Jointsの0番目のみ取得し、後続のJointsにもその値を反映するようにする。
                    VRM10SpringBoneJoint bone = springs[spi].Joints[0];
                    if (!bone.gameObject.name.EndsWith("_end"))
                    { //--- 0.xモデルは_endが末尾についているものがあり、それだけのJointsもあるためそれは省く
                        VRMGravityInfo vgi = new VRMGravityInfo();
                        vgi.comment = springs[spi].Name;
                        vgi.rootBoneName = bone.gameObject.name;
                        vgi.power = bone.m_gravityPower;
                        vgi.dir.x = bone.m_gravityDir.x;
                        vgi.dir.y = bone.m_gravityDir.y;
                        vgi.dir.z = bone.m_gravityDir.z;
                        gravityList.list.Add(vgi);
                    }
                    
                }
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
        public AvatarGravityClass GetGravityInfo(string param)
        {
            VRMGravityInfo ret = null;

            string[] arr = param.Split(',');
            /*
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
            */
            //---1.x
            AvatarGravityClass vgilist = new AvatarGravityClass();
            List<Vrm10InstanceSpringBone.Spring> springs = vrminstance.SpringBone.Springs;
            springs.ForEach(item =>
            {
                VRMGravityInfo vgi = new VRMGravityInfo();
                if ((item.Name == arr[0]) && (item.Joints[0].gameObject.name == arr[1]))
                {
                    ret = gravityList.list.Find(match =>
                    {
                        if ((match.comment == arr[0]) && (match.rootBoneName == arr[1])) return true;
                        return false;
                    });
                    if (ret != null)
                    {
                        ret.power = item.Joints[0].m_gravityPower;
                        ret.dir.x = item.Joints[0].m_gravityDir.x;
                        ret.dir.y = item.Joints[0].m_gravityDir.y;
                        ret.dir.z = item.Joints[0].m_gravityDir.z;
                        vgilist.list.Add(ret);
                    }
                }
            });

            return vgilist;
        }
        public void GetGravityInfoFromOuter(string param)
        {
            AvatarGravityClass ret = GetGravityInfo(param);
            string js = "";
            if (ret != null) js = JsonUtility.ToJson(ret);
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
                /*
                VRMSpringBone[] bones = transform.GetComponentsInChildren<VRMSpringBone>();
                for (int i = 0; i < bones.Length; i++)
                {
                    //Debug.Log(arr[0] + "," + arr[1] + " vs bones[" + i + "]-" + bones[i].m_comment + "," + bones[i].RootBones[0].gameObject.name);
                    string bonecomment = bones[i].m_comment == null ? "" : bones[i].m_comment;
                    if ((bonecomment == arr[0]) && (bones[i].RootBones[0].gameObject.name == arr[1]))
                    {
                        //Debug.Log(";[" + arr[0] + "," + arr[1] + ":" + bones[i].m_comment + "," + bones[i].RootBones[0].gameObject.name + "]");
                        bones[i].m_gravityPower = val;
                        VRMGravityInfo vgi = GetGravityInfo(arr[0] + "," + arr[1]);
                        if (vgi != null) vgi.power = val;
                        break;
                    }
                }
                */
                //---1.x
                List<Vrm10InstanceSpringBone.Spring> springs = vrminstance.SpringBone.Springs;
                springs.ForEach(spring =>
                {
                    string bonecomment = spring.Name == null ? "" : spring.Name;
                    if ((bonecomment == arr[0]) && (spring.Joints[0].gameObject.name == arr[1]))
                    {
                        //Debug.Log(";[" + arr[0] + "," + arr[1] + ":" + bones[i].m_comment + "," + bones[i].RootBones[0].gameObject.name + "]");
                        spring.Joints.ForEach(bone =>
                        {
                            bone.m_gravityPower = val;
                        });

                        AvatarGravityClass ags = GetGravityInfo(arr[0] + "," + arr[1]);
                        if (ags.list.Count > 0) ags.list.ForEach(gs => gs.power = val);
                    }
                });
                vrminstance.Runtime.ReconstructSpringBone();
            }
            
        }
        public void SetAnimationGravityPower(string comment, string bonename, Sequence seq, float val_power, float duration)
        {
            /*
            VRMSpringBone[] bones = transform.GetComponentsInChildren<VRMSpringBone>();
            for (int i = 0; i < bones.Length; i++)
            {
                if ((bones[i].m_comment == comment) && (bones[i].RootBones.Count > 0) && (bones[i].RootBones[0].gameObject.name == bonename))
                {
                    seq.Join(DOTween.To(() => bones[i].m_gravityPower, x => bones[i].m_gravityPower = x, val_power, duration));
                    break;
                }
            }
            */
            //---1.x
            List<Vrm10InstanceSpringBone.Spring> springs = vrminstance.SpringBone.Springs;
            springs.ForEach(spring =>
            {
                string bonecomment = spring.Name == null ? "" : spring.Name;
                if ((bonecomment == comment) && (spring.Joints[0].gameObject.name == bonename))
                {
                    spring.Joints.ForEach(bone =>
                    {
                        seq.Join(
                            DOTween.To(() => bone.m_gravityPower, x => bone.m_gravityPower = x, val_power, duration)
                        );
                    });
                    seq.Join(DOVirtual.DelayedCall(duration, () =>
                    {
                        vrminstance.Runtime.ReconstructSpringBone();

                    }));
                }
            });
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
                SetGravityDir(arr[0], arr[1], x, y, z);
                /*
                VRMSpringBone[] bones = transform.GetComponentsInChildren<VRMSpringBone>();
                for (int i = 0; i < bones.Length; i++)
                {
                    if ((bones[i].m_comment == arr[0]) && (bones[i].RootBones.Count > 0) && (bones[i].RootBones[0].gameObject.name == arr[1]))
                    {
                        bones[i].m_gravityDir.x = x;
                        bones[i].m_gravityDir.y = y;
                        bones[i].m_gravityDir.z = z;
                        VRMGravityInfo vgi = GetGravityInfo(arr[0] + "," + arr[1]);
                        if (vgi != null)
                        {
                            vgi.dir.x = x;
                            vgi.dir.y = y;
                            vgi.dir.z = z;
                        }
                        
                        break;
                    }
                }
                */
            }

        }
        public void SetGravityDir(string comment, string bonename, float x, float y, float z)
        {
            /*
            VRMSpringBone[] bones = transform.GetComponentsInChildren<VRMSpringBone>();
            for (int i = 0; i < bones.Length; i++)
            {
                if ((bones[i].m_comment == comment) && (bones[i].RootBones.Count > 0) && (bones[i].RootBones[0].gameObject.name == bonename))
                {
                    bones[i].m_gravityDir.x = x;
                    bones[i].m_gravityDir.y = y;
                    bones[i].m_gravityDir.z = z;
                    VRMGravityInfo vgi = GetGravityInfo(comment + "," + bonename);
                    if (vgi != null)
                    {
                        vgi.dir.x = x;
                        vgi.dir.y = y;
                        vgi.dir.z = z;
                    }
                    
                    break;
                }
            }*/
            //---1.x
            List<Vrm10InstanceSpringBone.Spring> springs = vrminstance.SpringBone.Springs;
            Vrm10InstanceSpringBone.Spring ret = springs.Find(item =>
            {
                string bonecomment = item.Name == null ? "" : item.Name;
                if ((bonecomment == comment) && (item.Joints[0].gameObject.name == bonename))
                {
                    
                    return true;
                }
                return false;
            });
            if (ret != null)
            {
                ret.Joints.ForEach(bone =>
                {
                    bone.m_gravityDir.x = x;
                    bone.m_gravityDir.y = y;
                    bone.m_gravityDir.z = z;
                });
                AvatarGravityClass ags = GetGravityInfo(comment + "," + bonename);
                ags.list.ForEach(vgi =>
                {
                    vgi.dir.x = x;
                    vgi.dir.y = y;
                    vgi.dir.z = z;
                });
                vrminstance.Runtime.ReconstructSpringBone();
            }
        }

        /// <summary>
        /// To pack all properties for HTML-UI
        /// </summary>
        public void GetIndicatedPropertyFromOuter()
        {
            const string SEPSTR = "\t";
            string ret = "";

            LeftHandPoseController ctl = LeftHandCtrl;
            RightHandPoseController ctr = RightHandCtrl;

            List<string> matlist = ListUserMaterial();
            string matjs = string.Join("\r\n", matlist.ToArray());

            List<string> lst = ListBackupAvatarBlendShape();
            string blendshape = string.Join(",", lst.ToArray());
            int eflag = GetEquipFlag();
            string equipjs = JsonUtility.ToJson(equipDestinations);
            string gravityjs = JsonUtility.ToJson(gravityList);

            string movemode = (isMoveMode == true) ? "1" : "0";

            List<string> animclips = ListAnimationClips();            
            string jsclip = string.Join('=', animclips);
            int pflag = IsPlayingAnimation();
            int playflag = (int)animationStartFlag;
            float seek = animationRemainTime;

            // 1st sep ... \t
            // 2nd sep ... =
            //             ,
            // blendshape
            //             ,
            //            name=value
            /*
            ret = "l," + LeftCurrentHand.ToString() + "," + LeftHandValue.ToString()
                    + "=" +
                    "r," + RightCurrentHand.ToString() + "," + RightHandValue.ToString()
                + SEPSTR + 
                blendshape
                + SEPSTR + 
                eflag.ToString()
                + SEPSTR + 
                equipjs
                + SEPSTR + 
                gravityjs
                + SEPSTR +
                GetHeadLock().ToString() 
                + SEPSTR +
                matjs
                + SEPSTR +
                movemode

            ;
            */
            string[] retarr =
            {
                "l," + LeftCurrentHand.ToString() + "," + LeftHandValue.ToString()
                    + "=" +
                    "r," + RightCurrentHand.ToString() + "," + RightHandValue.ToString(),
                blendshape,
                eflag.ToString(),
                equipjs,
                gravityjs,
                GetHeadLock().ToString(),
                matjs,
                movemode,
                //---VRMAnimation
                pflag.ToString(),
                playflag.ToString(),
                seek.ToString(),
                GetSpeedAnimation().ToString(),
                GetMaxPosAnimation().ToString(),
                GetWrapMode().ToString(),
                jsclip,
                GetTargetClip(),
                VrmaFileName,
                IsEnableVRMA() ? "1" : "0",
            };
            ret = string.Join(SEPSTR, retarr);

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
        public void ListFingerPoseClass()
        {
            BackupFingerNative();

            string[] js = { JsonUtility.ToJson(LeftFingerBackupHPC), JsonUtility.ToJson(RightFingerBackupHPC) };

            string ret = string.Join("\t", js);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
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

            //LeftHandPoseController ctl = GetComponent<LeftHandPoseController>();
            //RightHandPoseController ctr = GetComponent<RightHandPoseController>();

            if (handtype == "l")
            {
                LeftHandCtrl.ResetPose();
                LeftHandCtrl.SetPose(posetype, value);
                LeftCurrentHand = LeftHandCtrl.currentPose;
            }
            else
            {
                RightHandCtrl.ResetPose();
                RightHandCtrl.SetPose(posetype, value);
                RightCurrentHand = RightHandCtrl.currentPose;
            }
        }
        public void SetBackupHandPosing(string hand, int currentpose, float handvalue, AvatarFingerForHPC hpc)
        {
            if (hand == "r")
            {
                RightCurrentHand = currentpose;
                RightHandValue = handvalue;
                if (RightFingerBackupHPC != null) RightFingerBackupHPC.Copy(hpc);
            }
            else if (hand == "l")
            {
                LeftCurrentHand = currentpose;
                LeftHandValue = handvalue;
                if (LeftFingerBackupHPC != null) LeftFingerBackupHPC.Copy(hpc);
            }
        }
        public void BackupFingerNative()
        {
            LeftFingerBackupHPC = LeftHandCtrl.BackupFinger();
            RightFingerBackupHPC = RightHandCtrl.BackupFinger();
        }
        //=== Hand(Finger) new functions =============================****************************************-----------------------
        public void SetRelateHandController()
        {
            LeftHandCtrl = GetComponent<LeftHandPoseController>();
            RightHandCtrl = GetComponent<RightHandPoseController>();

        }
        public void SetHandFingerMode(string param)
        {
            if (param == "1")
            { //1 - handposing
                isHandPosing = true;
                LeftHandCtrl.enabled = true;
                RightHandCtrl.enabled = true;
            }
            else if (param == "2")
            { //2 - finger manually posing
                isHandPosing = false;
                LeftHandCtrl.enabled = false;
                RightHandCtrl.enabled = false;
            }
        }
        public Transform GetTargetFinger(string hand, string finger, int section)
        {
            Animator anim = GetComponent<Animator>();
            Transform ret = null;

            HumanBodyBones hbb = HumanBodyBones.LastBone;
            if (hand == "r")
            {
                if ((finger == "t") && (section == 0)) hbb = HumanBodyBones.RightThumbProximal;
                if ((finger == "t") && (section == 1)) hbb = HumanBodyBones.RightThumbIntermediate;
                if ((finger == "t") && (section == 2)) hbb = HumanBodyBones.RightThumbDistal;

                if ((finger == "i") && (section == 0)) hbb = HumanBodyBones.RightIndexProximal;
                if ((finger == "i") && (section == 1)) hbb = HumanBodyBones.RightIndexIntermediate;
                if ((finger == "i") && (section == 2)) hbb = HumanBodyBones.RightIndexDistal;

                if ((finger == "m") && (section == 0)) hbb = HumanBodyBones.RightMiddleProximal;
                if ((finger == "m") && (section == 1)) hbb = HumanBodyBones.RightMiddleIntermediate;
                if ((finger == "m") && (section == 2)) hbb = HumanBodyBones.RightMiddleDistal;

                if ((finger == "r") && (section == 0)) hbb = HumanBodyBones.RightRingProximal;
                if ((finger == "r") && (section == 1)) hbb = HumanBodyBones.RightRingIntermediate;
                if ((finger == "r") && (section == 2)) hbb = HumanBodyBones.RightRingDistal;

                if ((finger == "l") && (section == 0)) hbb = HumanBodyBones.RightLittleProximal;
                if ((finger == "l") && (section == 1)) hbb = HumanBodyBones.RightLittleIntermediate;
                if ((finger == "l") && (section == 2)) hbb = HumanBodyBones.RightLittleDistal;

            }
            else if (hand == "l")
            {
                if ((finger == "t") && (section == 0)) hbb = HumanBodyBones.LeftThumbProximal;
                if ((finger == "t") && (section == 1)) hbb = HumanBodyBones.LeftThumbIntermediate;
                if ((finger == "t") && (section == 2)) hbb = HumanBodyBones.LeftThumbDistal;

                if ((finger == "i") && (section == 0)) hbb = HumanBodyBones.LeftIndexProximal;
                if ((finger == "i") && (section == 1)) hbb = HumanBodyBones.LeftIndexIntermediate;
                if ((finger == "i") && (section == 2)) hbb = HumanBodyBones.LeftIndexDistal;

                if ((finger == "m") && (section == 0)) hbb = HumanBodyBones.LeftMiddleProximal;
                if ((finger == "m") && (section == 1)) hbb = HumanBodyBones.LeftMiddleIntermediate;
                if ((finger == "m") && (section == 2)) hbb = HumanBodyBones.LeftMiddleDistal;

                if ((finger == "r") && (section == 0)) hbb = HumanBodyBones.LeftRingProximal;
                if ((finger == "r") && (section == 1)) hbb = HumanBodyBones.LeftRingIntermediate;
                if ((finger == "r") && (section == 2)) hbb = HumanBodyBones.LeftRingDistal;

                if ((finger == "l") && (section == 0)) hbb = HumanBodyBones.LeftLittleProximal;
                if ((finger == "l") && (section == 1)) hbb = HumanBodyBones.LeftLittleIntermediate;
                if ((finger == "l") && (section == 2)) hbb = HumanBodyBones.LeftLittleDistal;
            }
            if (hbb != HumanBodyBones.LastBone)
            {
                ret = anim.GetBoneTransform(hbb);
            }

            return ret;
        }
        public void RotateFingerFromOuter(string param)
        {
            /* sep= , -> &
             * [0] - 左右：r, l
             * [1] - 親指、人差し指、中指、薬指、小指：t, i, m, r, l
             * [2] - 値：0.xxf (& sep)
             */
            //Animator anim = GetComponent<Animator>();

            string[] arr = param.Split(",");
            string hand = arr[0];
            string finger = arr[1];
            List<float> fval = new List<float>();
            string [] vals = arr[2].Split("&");
            foreach (string v in vals)
            {
                float fv = float.TryParse(v, out fv) ? fv : 0;
                fval.Add(fv);
            }

            RotateFinger(hand, finger, fval.ToArray());
            
        }
        public void RotateFinger(string handRL, string finger, float[] values)
        {
            /*
            Transform tran = GetTargetFinger(handRL, finger, section); //anim.GetBoneTransform(hbb);
            Vector3 rot = new Vector3(tran.localRotation.eulerAngles.x, tran.localRotation.eulerAngles.y, tran.localRotation.eulerAngles.z);
            if (dir == "x") rot.x = value;
            if (dir == "y") rot.y = value;
            if (dir == "z") rot.z = value;
            tran.localRotation = Quaternion.Euler(rot);
            */

            if (handRL == "r")
            {
                RightHandCtrl.PoseFinger(finger, values.Length, values);
            }
            else if (handRL == "l")
            {
                LeftHandCtrl.PoseFinger(finger, values.Length, values);
            }
        }
        public void SetBackupFinger(string hand, AvatarFingerInHand fingerCls)
        {
            if (hand == "r")
            {
                RightFingerBkup.Copy(fingerCls);
            }
            else if (hand == "l")
            {
                LeftFingerBkup.Copy(fingerCls);
            }
        }
        public Vector3 GetFingerRotation(string hand, string finger, int section)
        {
            Vector3 ret = Vector3.zero;

            Transform tran = GetTargetFinger(hand, finger, section);
            ret = tran.localRotation.eulerAngles;

            return ret;
        }
        public AvatarFingerInHand GetFingerRotationByClass(string hand)
        {
            AvatarFingerInHand ret = new AvatarFingerInHand();
            Animator anim = GetComponent<Animator>();
            if (hand == "r")
            {
                ret.Thumbs.Add(anim.GetBoneTransform(HumanBodyBones.RightThumbProximal).localRotation.eulerAngles);
                ret.Thumbs.Add(anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate).localRotation.eulerAngles);
                ret.Thumbs.Add(anim.GetBoneTransform(HumanBodyBones.RightThumbDistal).localRotation.eulerAngles);

                ret.Index.Add(anim.GetBoneTransform(HumanBodyBones.RightIndexProximal).localRotation.eulerAngles);
                ret.Index.Add(anim.GetBoneTransform(HumanBodyBones.RightIndexIntermediate).localRotation.eulerAngles);
                ret.Index.Add(anim.GetBoneTransform(HumanBodyBones.RightIndexDistal).localRotation.eulerAngles);

                ret.Middle.Add(anim.GetBoneTransform(HumanBodyBones.RightMiddleProximal).localRotation.eulerAngles);
                ret.Middle.Add(anim.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate).localRotation.eulerAngles);
                ret.Middle.Add(anim.GetBoneTransform(HumanBodyBones.RightMiddleDistal).localRotation.eulerAngles);

                ret.Ring.Add(anim.GetBoneTransform(HumanBodyBones.RightRingProximal).localRotation.eulerAngles);
                ret.Ring.Add(anim.GetBoneTransform(HumanBodyBones.RightRingIntermediate).localRotation.eulerAngles);
                ret.Ring.Add(anim.GetBoneTransform(HumanBodyBones.RightRingDistal).localRotation.eulerAngles);

                ret.Little.Add(anim.GetBoneTransform(HumanBodyBones.RightLittleProximal).localRotation.eulerAngles);
                ret.Little.Add(anim.GetBoneTransform(HumanBodyBones.RightLittleIntermediate).localRotation.eulerAngles);
                ret.Little.Add(anim.GetBoneTransform(HumanBodyBones.RightLittleDistal).localRotation.eulerAngles);
            }
            else if (hand == "l")
            {
                ret.Thumbs.Add(anim.GetBoneTransform(HumanBodyBones.LeftThumbProximal).localRotation.eulerAngles);
                ret.Thumbs.Add(anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate).localRotation.eulerAngles);
                ret.Thumbs.Add(anim.GetBoneTransform(HumanBodyBones.LeftThumbDistal).localRotation.eulerAngles);

                ret.Index.Add(anim.GetBoneTransform(HumanBodyBones.LeftIndexProximal).localRotation.eulerAngles);
                ret.Index.Add(anim.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate).localRotation.eulerAngles);
                ret.Index.Add(anim.GetBoneTransform(HumanBodyBones.LeftIndexDistal).localRotation.eulerAngles);

                ret.Middle.Add(anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal).localRotation.eulerAngles);
                ret.Middle.Add(anim.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate).localRotation.eulerAngles);
                ret.Middle.Add(anim.GetBoneTransform(HumanBodyBones.LeftMiddleDistal).localRotation.eulerAngles);

                ret.Ring.Add(anim.GetBoneTransform(HumanBodyBones.LeftRingProximal).localRotation.eulerAngles);
                ret.Ring.Add(anim.GetBoneTransform(HumanBodyBones.LeftRingIntermediate).localRotation.eulerAngles);
                ret.Ring.Add(anim.GetBoneTransform(HumanBodyBones.LeftRingDistal).localRotation.eulerAngles);

                ret.Little.Add(anim.GetBoneTransform(HumanBodyBones.LeftLittleProximal).localRotation.eulerAngles);
                ret.Little.Add(anim.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate).localRotation.eulerAngles);
                ret.Little.Add(anim.GetBoneTransform(HumanBodyBones.LeftLittleDistal).localRotation.eulerAngles);
            }
            return ret;
        }
        public Sequence AnimationFinger(string hand, string finger, AvatarFingerInHand fingerCls, float duration, Sequence seq)
        {
            Animator anim = GetComponent<Animator>();
            Transform[] ft = new Transform[3] { null, null, null };
            Transform[] fi = new Transform[3] { null, null, null };
            Transform[] fm = new Transform[3] { null, null, null };
            Transform[] fr = new Transform[3] { null, null, null };
            Transform[] fl = new Transform[3] { null, null, null };
            if (hand == "r")
            {
                ft[0] = anim.GetBoneTransform(HumanBodyBones.RightThumbProximal);
                ft[1] = anim.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);
                ft[2] = anim.GetBoneTransform(HumanBodyBones.RightThumbDistal);

                fi[0] = anim.GetBoneTransform(HumanBodyBones.RightIndexProximal);
                fi[1] = anim.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
                fi[2] = anim.GetBoneTransform(HumanBodyBones.RightIndexDistal);

                fm[0] = anim.GetBoneTransform(HumanBodyBones.RightMiddleProximal);
                fm[1] = anim.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
                fm[2] = anim.GetBoneTransform(HumanBodyBones.RightMiddleDistal);

                fr[0] = anim.GetBoneTransform(HumanBodyBones.RightRingProximal);
                fr[1] = anim.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
                fr[2] = anim.GetBoneTransform(HumanBodyBones.RightRingDistal);

                fl[0] = anim.GetBoneTransform(HumanBodyBones.RightLittleProximal);
                fl[1] = anim.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
                fl[2] = anim.GetBoneTransform(HumanBodyBones.RightLittleDistal);
            }
            else if (hand == "l")
            {
                ft[0] = anim.GetBoneTransform(HumanBodyBones.LeftThumbProximal);
                ft[1] = anim.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);
                ft[2] = anim.GetBoneTransform(HumanBodyBones.LeftThumbDistal);

                fi[0] = anim.GetBoneTransform(HumanBodyBones.LeftIndexProximal);
                fi[1] = anim.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
                fi[2] = anim.GetBoneTransform(HumanBodyBones.LeftIndexDistal);

                fm[0] = anim.GetBoneTransform(HumanBodyBones.LeftMiddleProximal);
                fm[1] = anim.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
                fm[2] = anim.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);

                fr[0] = anim.GetBoneTransform(HumanBodyBones.LeftRingProximal);
                fr[1] = anim.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
                fr[2] = anim.GetBoneTransform(HumanBodyBones.LeftRingDistal);

                fl[0] = anim.GetBoneTransform(HumanBodyBones.LeftLittleProximal);
                fl[1] = anim.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
                fl[2] = anim.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
            }
            for (int i = 0; i < ft.Length; i++) seq.Join(ft[i].DOLocalRotate(fingerCls.Thumbs[i], duration));
            for (int i = 0; i < fi.Length; i++) seq.Join(fi[i].DOLocalRotate(fingerCls.Index[i], duration));
            for (int i = 0; i < fm.Length; i++) seq.Join(fm[i].DOLocalRotate(fingerCls.Middle[i], duration));
            for (int i = 0; i < fr.Length; i++) seq.Join(fr[i].DOLocalRotate(fingerCls.Ring[i], duration));
            for (int i = 0; i < fl.Length; i++) seq.Join(fl[i].DOLocalRotate(fingerCls.Little[i], duration));            

            return seq;
        }
        public static string StringifyFingerRotation(AvatarFingerInHand fingerCls, string finger)
        {
            string ret = "";

            if (finger == "t")
            {
                string sect1 = string.Join("&", new string[] { fingerCls.Thumbs[0].x.ToString(), fingerCls.Thumbs[0].y.ToString(), fingerCls.Thumbs[0].z.ToString() });
                string sect2 = fingerCls.Thumbs[1].y.ToString();
                string sect3 = fingerCls.Thumbs[2].y.ToString();

                ret = sect1 + ":" + sect2 + ":" + sect3;
            }
            else if (finger == "i")
            {
                string sect1 = string.Join("&", new string[] { fingerCls.Index[0].y.ToString(), fingerCls.Index[0].z.ToString() });
                string sect2 = fingerCls.Index[1].z.ToString();
                string sect3 = fingerCls.Index[2].z.ToString();

                ret = sect1 + ":" + sect2 + ":" + sect3;
            }
            else if (finger == "m")
            {
                string sect1 = string.Join("&", new string[] { fingerCls.Middle[0].y.ToString(), fingerCls.Middle[0].z.ToString() });
                string sect2 = fingerCls.Middle[1].z.ToString();
                string sect3 = fingerCls.Middle[2].z.ToString();

                ret = sect1 + ":" + sect2 + ":" + sect3;
            }
            else if (finger == "r")
            {
                string sect1 = string.Join("&", new string[] { fingerCls.Ring[0].y.ToString(), fingerCls.Ring[0].z.ToString() });
                string sect2 = fingerCls.Ring[1].z.ToString();
                string sect3 = fingerCls.Ring[2].z.ToString();

                ret = sect1 + ":" + sect2 + ":" + sect3;
            }
            else if (finger == "l")
            {
                string sect1 = string.Join("&", new string[] { fingerCls.Little[0].y.ToString(), fingerCls.Little[0].z.ToString() });
                string sect2 = fingerCls.Little[1].z.ToString();
                string sect3 = fingerCls.Little[2].z.ToString();

                ret = sect1 + ":" + sect2 + ":" + sect3;
            }

            return ret;
        }
        public static List<Vector3> ParseFingerRotation(string rottext, string finger)
        {
            List<Vector3> ret = new List<Vector3>();

            if (finger == "t")
            {
                string[] sections = rottext.Split(":");
                string[] sect0 = sections[0].Split("&");
                float x0 = float.TryParse(sect0[0], out x0) ? x0 : 0;
                float y0 = float.TryParse(sect0[1], out y0) ? y0 : 0;
                float z0 = float.TryParse(sect0[2], out z0) ? z0 : 0;
                ret.Add(new Vector3(x0, y0, z0));

                float y1 = float.TryParse(sections[1], out y1) ? y1 : 0;
                ret.Add(new Vector3(0, y1, 0));

                float y2 = float.TryParse(sections[1], out y2) ? y2 : 0;
                ret.Add(new Vector3(0, y2, 0));
            }
            else if ((finger == "i") || (finger == "m") || (finger == "r") || (finger == "l"))
            {
                string[] sections = rottext.Split(":");
                string[] sect0 = sections[0].Split("&");
                float y0 = float.TryParse(sect0[1], out y0) ? y0 : 0;
                float z0 = float.TryParse(sect0[2], out z0) ? z0 : 0;
                ret.Add(new Vector3(0, y0, z0));

                float z1 = float.TryParse(sections[1], out z1) ? z1 : 0;
                ret.Add(new Vector3(0, 0, z1));

                float z2 = float.TryParse(sections[1], out z2) ? z2 : 0;
                ret.Add(new Vector3(0, 0, z2));
            }
            

            return ret;
        }


        //===============================================================================================================================
        //  Blend Shape 
        public SkinnedMeshRenderer GetBlendShapeTarget()
        {
            return BSFace;
        }
        public List<SkinnedMeshRenderer> GetBlendShapeTargets()
        {
            return BSMeshs;
        }
        public SkinnedMeshRenderer SetActiveFace()
        {
            SkinnedMeshRenderer[] skns = transform.GetComponentsInChildren<SkinnedMeshRenderer>();
            int cnt = skns.Length;
            SkinnedMeshRenderer mesh = null;
            for (int i = 0; i < cnt; i++)
            {
                mesh = skns[i];
                if (mesh != null)
                {
                    if (mesh.sharedMesh.blendShapeCount > 0)
                    {
                        //BSFace = mesh;
                        BSMeshs.Add(mesh);
                        //break;
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
            /*if (BSFace)
            {
                int bscnt = BSFace.sharedMesh.blendShapeCount;
                for (int i = 0; i < bscnt; i++)
                {
                    ret.Add(BSFace.sharedMesh.GetBlendShapeName(i) + "=" + BSFace.GetBlendShapeWeight(i).ToString());
                }
            }*/
            //---1.x
            BSMeshs.ForEach(mesh =>
            {
                for (int i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
                {
                    ret.Add(mesh.gameObject.name + ":" + mesh.sharedMesh.GetBlendShapeName(i) + "=" + mesh.GetBlendShapeWeight(i).ToString());
                }
            });

            //---custom clip version
            vrminstance.Vrm.Expression.CustomClips.ForEach(item =>
            {
                ret.Add(item.name + "=" + getProxyBlendShape(item.name).ToString());
            });


            return ret;
        }
        public List<BasicStringFloatList> ListAvatarBlendShapeList()
        {
            List<BasicStringFloatList> ret = new List<BasicStringFloatList>();
            BSMeshs.ForEach(mesh =>
            {
                for (int i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
                {
                    BasicStringFloatList bsf = new BasicStringFloatList(mesh.gameObject.name + ":" + mesh.sharedMesh.GetBlendShapeName(i),mesh.GetBlendShapeWeight(i));
                    ret.Add(bsf);
                }
            });


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
        public Vrm10RuntimeExpression RunExpression => vrminstance.Runtime.Expression;
        public void ReloadProxyCustomExpression()
        {
            RunExpression.ReloadCustomClip(vrminstance);
        }

        /// <summary>
        /// Gert all blend shape proxy, the avatar has.
        /// </summary>
        /// <returns></returns>
        public List<BasicBlendShapeKey> ListProxyBlendShape()
        {
            List<BasicBlendShapeKey> ret = new List<BasicBlendShapeKey>();
            /*
            VRMBlendShapeProxy prox = GetComponent<VRMBlendShapeProxy>();

            IEnumerable<KeyValuePair<BlendShapeKey, float>> bsk = prox.GetValues();
            IEnumerator<KeyValuePair<BlendShapeKey, float>> bslist = bsk.GetEnumerator();
            while (bslist.MoveNext())
            {
                KeyValuePair<BlendShapeKey, float> bs = bslist.Current;
                BasicStringFloatList bsf = new BasicStringFloatList(PREFIX_PROXY + bs.Key.Name, bs.Value);
                ret.Add(bsf);
            }
            */
            //---1.x
            IReadOnlyList<ExpressionKey> eklist = vrminstance.Runtime.Expression.ExpressionKeys;
            for (int i = 0; i < eklist.Count; i++)
            {
                BasicBlendShapeKey bsf = new BasicBlendShapeKey(PREFIX_PROXY + eklist[i].Name, vrminstance.Runtime.Expression.GetWeight(eklist[i]));
                ret.Add(bsf);
            }

            return ret;
        }
        public void ListProxyBlendShapeFromOuter()
        {
            List<BasicBlendShapeKey> lst = ListProxyBlendShape();
            List<string> retlst = new List<string>();

            foreach (BasicBlendShapeKey bsf in lst)
            {
                retlst.Add(bsf.text + "=" + bsf.value.ToString());
            }

            string ret = string.Join(",", retlst.ToArray());
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        //----- backup blendshape ---------------------=============================
        public List<string> ListBackupAvatarBlendShape()
        {
            List<string> ret = new List<string>();
            for (int i = 0; i < blendShapeList.Count; i++)
            {
                ret.Add(blendShapeList[i].text + "=" + blendShapeList[i].value.ToString() + "=" + blendShapeList[i].is_changed.ToString());
                //Debug.Log(blendShapeList[i].text + "=" + blendShapeList[i].value.ToString());
            }
            
            return ret;
        }
        public void InitializeBlendShapeList()
        {
            blendShapeList.Clear();
            /*List<string> lst = ListAvatarBlendShape();
            lst.ForEach(item =>
            {
                string[] arr = item.Split('=');
                float value = -1f; // float.TryParse(arr[1], out value) ? value : -1f;
                BasicStringFloatList lsf = new BasicStringFloatList(arr[0], value);
                blendShapeList.Add(lsf);
            });*/
            //---1.x
            //List<BasicStringFloatList> lst_skn = ListAvatarBlendShapeList();
            //lst_skn.ForEach(item =>
            //{
            //    blendShapeList.Add(item);
            //});

            //---Expression only
            List<BasicBlendShapeKey> lst_px = ListProxyBlendShape();
            lst_px.ForEach(item =>
            {
                blendShapeList.Add(item);
            });
            //Debug.Log(blendShapeList.Count);
        }
        public void SetBlendShapeToBackup(string name, float value, int is_changed = 0)
        {
            BasicBlendShapeKey bs = blendShapeList.Find(item =>
            {
                if (item.text == name) return true;
                return false;
            });
            if (bs != null)
            {
                bs.value = value;
                bs.is_changed = is_changed;
            }
        }
        //------ getting blendshape ----------------------==========================
/*        public float getAvatarBlendShape(string param)
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
        }*/
/*        public float getAvatarBlendShapeValue(string param)
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
        }*/
        public float getAvatarBlendShapeValue(string shapename)
        {
            float ret = 0;
            foreach (SkinnedMeshRenderer mesh in BSMeshs)
            {
                int index = getAvatarBlendShapeIndex(mesh, shapename);
                if (index > -1)
                {
                    ret = mesh.GetBlendShapeWeight(index);
                    break;
                }
            }
            return ret;
        }
        public int getAvatarBlendShapeIndex(SkinnedMeshRenderer mesh, string name)
        {
            //return BSFace.sharedMesh.GetBlendShapeIndex(name);
            //---1.x
            int result = -1;
            string[] names = name.Split(':');
            if (mesh.gameObject.name == names[0])
            {
                result = mesh.sharedMesh.GetBlendShapeIndex(names[1]);
            }
            return result;
        }
        public float getProxyBlendShape(string param)
        {
            //VRMBlendShapeProxy prox = GetComponent<VRMBlendShapeProxy>();

            float ret = 0f;
            List<BasicBlendShapeKey> lst = ListProxyBlendShape();
            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].text == param)
                {
                    /*
                    BlendShapeClip bsclip = prox.BlendShapeAvatar.Clips.Find(cl =>
                    {
                        if (cl.Key.Name == param.Replace(PREFIX_PROXY, ""))
                        {
                            return true;
                        }
                        return false;
                    });

                    if (bsclip != null)
                    {
                        ret = prox.GetValue(bsclip.Key);
                    }*/
                    //---1.x
                    for (int e = 0; e < vrminstance.Runtime.Expression.ExpressionKeys.Count; e++)
                    {
                        if (vrminstance.Runtime.Expression.ExpressionKeys[e].Name == param.Replace(PREFIX_PROXY, ""))
                        {
                            ret = vrminstance.Runtime.Expression.GetWeight(vrminstance.Runtime.Expression.ExpressionKeys[i]);
                        }
                    }
                    break;
                }
            }
            

            return ret;
        }
        public void getProxyBlendShapeFromOuter(string param)
        {
            float ret = getProxyBlendShape(param);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
        }
        /*
        public BlendShapeKey getProxyBlendShapeKey(string param)
        {
            VRMBlendShapeProxy prox = GetComponent<VRMBlendShapeProxy>();
            BlendShapeKey ret = BlendShapeKey.CreateUnknown("d%d"); //null代わりのダミーキー

            BlendShapeClip bsclip = prox.BlendShapeAvatar.Clips.Find(cl =>
            {
                if (cl.Key.Name == param.Replace(PREFIX_PROXY, ""))
                {
                    return true;
                }
                return false;
            });

            if (bsclip != null)
            {
                ret = bsclip.Key;
            }

            return ret;
        }
        */
        public ExpressionKey getVrm10ExpressionKey(string param)
        {
            ExpressionKey ret = new ExpressionKey(ExpressionPreset.custom,"d%d"); //null代わりのダミーキー
            
            for (int e = 0; e < vrminstance.Runtime.Expression.ExpressionKeys.Count; e++)
            {
                if (vrminstance.Runtime.Expression.ExpressionKeys[e].Name == param.Replace(PREFIX_PROXY, ""))
                {
                    ret = vrminstance.Runtime.Expression.ExpressionKeys[e];
                    break;
                }
            }
            return ret;
        }
        //------ setting blendshape ----------------------==========================
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

            changeAvatarBlendShapeByName(shapename, value);

            /*int index = getAvatarBlendShapeIndex(shapename);
            if (index > -1)
            {
                BSFace.SetBlendShapeWeight(index, value);
            }*/
            //---1.x
            /*foreach (SkinnedMeshRenderer mesh in BSMeshs)
            {
                int index = getAvatarBlendShapeIndex(mesh, shapename);
                if (index > -1)
                {
                    mesh.SetBlendShapeWeight(index, value);
                    vrminstance.Runtime.Process();
                }
            }*/
        }
        public void changeAvatarBlendShapeByName(string shapename, float value)
        {
            /*int index = getAvatarBlendShapeIndex(shapename);
            if (index > -1)
            {
                BSFace.SetBlendShapeWeight(index, value);
            }*/
            //---1.x
            foreach (SkinnedMeshRenderer mesh in BSMeshs)
            {
                int index = getAvatarBlendShapeIndex(mesh, shapename);
                if (index > -1)
                {
                    mesh.SetBlendShapeWeight(index, value);
                    //vrminstance.Runtime.Process();
                    break;
                    
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">equal separated string: [0] - key name, [1] - float value(already 0.xxf) </param>
        public void changeProxyBlendShapeByName(string param)
        {
            //VRMBlendShapeProxy prox = GetComponent<VRMBlendShapeProxy>();

            string[] prm = param.Split(',');
            string shapename = prm[0];
            float value = float.TryParse(prm[1], out value) ? value : 0f;

            //BlendShapeKey bskey = getProxyBlendShapeKey(shapename);
            ExpressionKey bskey = getVrm10ExpressionKey(shapename);
            if (bskey.Name != "d%d")
            {
                //prox.AccumulateValue(bskey, value);
                //prox.Apply();
                
                //---1.x
                changeProxyBlendShapeByName(bskey, value);

                SetBlendShapeToBackup(shapename, value);
            }
        }
        /*
        public void changeProxyBlendShapeByName(BlendShapeKey shape, float value)
        {
            VRMBlendShapeProxy prox = GetComponent<VRMBlendShapeProxy>();

            prox.AccumulateValue(shape, value);
            prox.Apply();

        }
        */
        public void changeProxyBlendShapeByName(ExpressionKey shape, float value)
        {
            vrminstance.Runtime.Expression.SetWeight(shape, value);
        }
        //---For animation
        public Sequence AnimationBlendShape(Sequence seq, string shapename, float value, float duration)
        {
            foreach (SkinnedMeshRenderer mesh in BSMeshs)
            {
                int index = getAvatarBlendShapeIndex(mesh, shapename);
                if (index > -1)
                {
                    seq.Join(DOTween.To(() => mesh.GetBlendShapeWeight(index), x => mesh.SetBlendShapeWeight(index, x), value, duration));
                }
            }

            
            return seq;
        }
        //===Auto Blendshape (lip sync) =====================================
        public void GetAutoBlendShapeFromOuter()
        {
            string ret = "";
            if (blink != null)
            {
                ret = (IsAutoBlendShape.ToString()) + "," + autoBlendShapeInterval.ToString() + "," + autoBlendShapeOpeningSeconds.ToString() + "," + autoBlendShapeClosingSeconds.ToString();
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public void AutoBlendShape(float duration)
        {
            autoBlendShapeSequence = DOTween.Sequence();
            autoBlendShapeSequence.SetAutoKill(false);
            IsKillAutoBlendShape = false;

            autoBlendShapeSequence.Join(
                DOTween.To(
                    () => vrminstance.Runtime.Expression.GetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.aa)),
                    x => vrminstance.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.aa), x),
                    0.5f, autoBlendShapeOpeningSeconds
                )
            );
            autoBlendShapeSequence.Join(
                DOTween.To(
                    () => vrminstance.Runtime.Expression.GetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.aa)),
                    x => vrminstance.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.aa), x),
                    0f, autoBlendShapeClosingSeconds
                )
            );
            autoBlendShapeSequence.AppendInterval(autoBlendShapeInterval);

            autoBlendShapeSequence.SetLoops(-1);

            autoBlendShapeSequence.Pause();
        }
        IEnumerator AutoBlendShapeRoutine()
        {
            while (true)
            {
                if (IsAutoBlendShape != 1) break;
                //yield return new WaitForSeconds(1.0f);

                //var velocity = 0.1f;

                for (var value = 0.0f; value <= 0.5f; value += autoBlendShapeOpeningSeconds)
                {
                    vrminstance.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.aa), value);
                    yield return null;
                }
                vrminstance.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.aa), 1.0f);
                //yield return new WaitForSeconds(wait);
                for (var value = 0.5f; value >= 0; value -= autoBlendShapeClosingSeconds)
                {
                    vrminstance.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.aa), value);
                    yield return null;
                }
                vrminstance.Runtime.Expression.SetWeight(ExpressionKey.CreateFromPreset(ExpressionPreset.aa), 0);
                yield return new WaitForSeconds(autoBlendShapeInterval);
            }
        }
        public void PlayAutoBlendShape()
        {
            //autoBlendShapeSequence.Rewind();
            //autoBlendShapeSequence.Play();

            if (IsAutoBlendShape == 1) 
                m_coroutine = StartCoroutine(AutoBlendShapeRoutine());
        }
        public void PauseAutoBlendShape()
        {
            if (autoBlendShapeSequence != null)
            {
                if (autoBlendShapeSequence.IsPlaying()) autoBlendShapeSequence.Pause();
            }
            
        }
        public void StopAutoBlendShape()
        {
            IsKillAutoBlendShape = true;
            /*if (autoBlendShapeSequence != null)
            {
                DOTween.Kill(autoBlendShapeSequence, true);
                autoBlendShapeSequence = null;
            }*/
            if (m_coroutine != null)
            {
                StopCoroutine(m_coroutine);
            }
            
        }
        public void SetPlayFlagAutoBlendShape(int flag)
        {
            IsAutoBlendShape = flag;
        }

        /// <summary>
        /// Get Play flag for Auto blendshape
        /// </summary>
        /// <returns>int</returns>
        public int GetPlayFlagAutoBlendShape()
        {
            return IsAutoBlendShape;
        }
        public void GetPlayFlagAutoBlendShapeFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(IsAutoBlendShape);
#endif
        }
        public float GetAutoBlendShapeOpeningSeconds()
        {
            return autoBlendShapeOpeningSeconds;
        }
        public void SetAutoBlendShapeOpeningSeconds(float v)
        {
            autoBlendShapeOpeningSeconds = v;
        }
        public float GetAutoBlendShapeCloseSeconds()
        {
            return autoBlendShapeClosingSeconds;
        }
        public void SetAutoBlendShapeCloseSeconds(float v)
        {
            autoBlendShapeClosingSeconds = v;
        }
        public float GetAutoBlendShapeInterval()
        {
            return autoBlendShapeInterval;
        }
        public void SetAutoBlendShapeInterval(float v)
        {
            autoBlendShapeInterval = v;
        }

        //----- blink ----------------------------------------==================
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
        public UniVRM10.VRM10Viewer.VRM10Blinker BlinkEye
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


        /// <summary>
        /// Flag for animation: -1 - unequip, 0 - no change, 1 - to equip
        /// </summary>
        /// <param name="flag"></param>
        public void SetEquipFlag(int flag)
        {
            equipType = flag;
        }

        /// <summary>
        /// Flag for animation: -1 - unequip, 0 - no change, 1 - to equip
        /// </summary>
        /// <returns></returns>
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
        /// (*) if already equiped, nothing function
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

            //---wheather already equiped the equipment avatar?
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
                ave.equipflag = 1;
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
        /// To exchange old equipment and new equipment.
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
            { //---unquip old equippable avatar
                UnequipObject(parts, equipDestinations.list[isHit].equipitem); // equipment.roleName);
            }

            //---equip new equippable avatar
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
                    //EquipExchange((HumanBodyBones)index, nav);
                    EquipObject((HumanBodyBones)index, nav);
                }
                else
                { //---effectively NOT USE
                    EquipPositioning((HumanBodyBones)index, nav);
                }
            }
            
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
        public IEnumerator SaveVRM()
        {
            //---Export VRM to byte array 
            var bytes = ovrmex.ExportSimple(transform.gameObject);
            

            yield return null;
            VVMDummyVrmaOutput vout = new VVMDummyVrmaOutput();
            vout.data = bytes;

            string ret = JsonUtility.ToJson(vout);

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif

        }

        //===============================================================================================================================
        //  IK handle
        //  
        /// <summary>
        /// To construct initial IKMappingList
        /// </summary>
        public void constructIKMappingList ()
        {
            for (int ik = (int)IKBoneType.EyeViewHandle; ik <= (int)IKBoneType.RightLeg; ik++)
            {
                AvatarIKMappingClass aik = new AvatarIKMappingClass();
                aik.parts = (IKBoneType)ik;
                aik.name = "self";
                ikMappingList.Add(aik);
            }
        }
        /// <summary>
        /// To reset all IK target marker to Default
        /// </summary>
        public void ResetIKMappingList ()
        {
            for (int i = ikMappingList.Count-1; i >= 0; i--)
            {
                AvatarIKMappingClass ik = ikMappingList[i];

                SetIKTarget(ik.parts, "self");
            }
            ikMappingList.Clear();
        }

        /// <summary>
        /// To reset ik-target, connected with specified object.
        /// </summary>
        /// <param name="param"></param>
        public void ResetIKMappingBySearchObject(string param)
        {
            AvatarIKMappingClass ik = ikMappingList.Find(item =>
            {
                //---role's roleTitle
                if (item.name == param) return true;
                return false;
            });
            if (ik != null)
            {
                SetIKTarget(ik.parts, "self");
            }
        }
        public void ResetIKMappingByBodyParts(int param)
        {
            AvatarIKMappingClass ik = ikMappingList.Find(item =>
            {
                if (item.parts == (IKBoneType)param) return true;
                return false;
            });
            if (ik != null)
            {
                SetIKTarget(ik.parts, "self");
            }
        }
        public string GetIKTarget(IKBoneType parts)
        {
            string js = "";

            BipedIK bik = gameObject.GetComponent<BipedIK>();
            VvmIk vik = gameObject.GetComponent<VvmIk>();
            int isIK = 0;
            if (bik != null) isIK = 1;
            if (vik != null) isIK = 2;

            if (parts == IKBoneType.EyeViewHandle)
            {
                //VRMLookAtHead vlook = gameObject.GetComponent<VRMLookAtHead>();
                //js = vlook.Target.gameObject.name;
                vrminstance.LookAtTargetType = VRM10ObjectLookAt.LookAtTargetTypes.CalcYawPitchToGaze;
                js = vrminstance.Gaze.gameObject.name;
            }
            else if (parts == IKBoneType.LookAt)
            {
                if (isIK == 1) js = bik.solvers.lookAt.target.gameObject.name;
                else if (isIK == 2) js = vik.lookAtObject.gameObject.name;
            }
            else if (parts == IKBoneType.Aim)
            {
                if (isIK == 1) js = bik.solvers.aim.target.gameObject.name;
                else if (isIK == 2) js = vik.Spine.gameObject.name;
            }
            else if (parts == IKBoneType.Chest)
            {
                if (isIK == 1) js = bik.solvers.spine.target.gameObject.name;
                else if (isIK == 2) js = vik.UpperChest.gameObject.name;
            }
            else if (parts == IKBoneType.Pelvis)
            {
                if (isIK == 1) js = bik.solvers.pelvis.target.gameObject.name;
                else if (isIK == 2) js = vik.waist.gameObject.name;
            }
            //-------------
            else if (parts == IKBoneType.LeftLowerArm)
            {
                if (isIK == 1) js = bik.solvers.leftHand.bendGoal.gameObject.name;
                else if (isIK == 2) js = vik.LeftLowerArm.gameObject.name;
            }
            else if (parts == IKBoneType.LeftHand)
            {
                if (isIK == 1) js = bik.solvers.leftHand.target.gameObject.name;
                else if (isIK == 2) js = vik.LeftHand.gameObject.name;
            }
            else if (parts == IKBoneType.RightLowerArm)
            {
                if (isIK == 1) js = bik.solvers.rightHand.bendGoal.gameObject.name;
                else if (isIK == 2) js = vik.RightLowerArm.gameObject.name;
            }
            else if (parts == IKBoneType.RightHand)
            {
                if (isIK == 1) js = bik.solvers.rightHand.target.gameObject.name;
                else if (isIK == 2) js = vik.RightHand.gameObject.name;
            }
            //---------------
            else if (parts == IKBoneType.LeftLowerLeg)
            {
                if (isIK == 1) js = bik.solvers.leftFoot.bendGoal.gameObject.name;
                else if (isIK == 2) js = vik.LeftLowerLeg.gameObject.name;
            }
            else if (parts == IKBoneType.LeftLeg)
            {
                if (isIK == 1) js = bik.solvers.leftFoot.target.gameObject.name;
                else if (isIK == 2) js = vik.LeftFoot.gameObject.name;
            }
            else if (parts == IKBoneType.RightLowerLeg)
            {
                if (isIK == 1) js = bik.solvers.rightFoot.bendGoal.gameObject.name;
                else if (isIK == 2) js = vik.RightLowerLeg.gameObject.name;
            }
            else if (parts == IKBoneType.RightLeg)
            {
                if (isIK == 1) js = bik.solvers.rightFoot.target.gameObject.name;
                else if (isIK == 2) js = vik.RightFoot.gameObject.name;
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
                        js = nav.roleTitle; //.roleName;
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

        /// <summary>
        /// Exchange maincamera IKTarget to between MainCamera and WebXRCameraSet
        /// </summary>
        /// <param name="isnormal"></param>
        public void ChangeNormalVRAR_IKTarget()
        {
            for (int i = 0; i < ikMappingList.Count; i++)
            {
                AvatarIKMappingClass aikmc = ikMappingList[i];
                if (aikmc.name == "maincamera")
                {
                    SetIKTarget(aikmc.parts, aikmc.name);
                }
            }
        }
        public void SetIKTarget(IKBoneType parts, string name, bool isPlayMode = false)
        {
            BipedIK bik = gameObject.GetComponent<BipedIK>();
            VvmIk vik = gameObject.GetComponent<VvmIk>();
            int isIK = 0;
            if (bik != null) isIK = 1;
            if (vik != null) isIK = 2;

            Transform target = null;
            if (name.ToLower() == "self")
            {
                target = GetIKHandle(ManageAnimation.GetVRMIKBoneName(parts)).transform;
            }
            else if (name.ToLower() == "maincamera")
            {
                if (manim.IsVRAR())
                {
                    target = manim.camxr.transform;
                }
                else
                {
                    target = Camera.main.transform;
                }
                
            }
            else
            {
                NativeAnimationAvatar nav = manim.GetCastByNameInProject(name); //.GetCastInProject(name);
                if (nav == null) return;
                if (nav.avatar == null) return;

                target = nav.avatar.transform;
            }

            if (target == null) return;

            if (parts == IKBoneType.EyeViewHandle)
            {
                //VRMLookAtHead vlook = gameObject.GetComponent<VRMLookAtHead>();
                //vlook.Target = target;
                vrminstance.Gaze = target;
                
            }
            else if (parts == IKBoneType.LookAt)
            {
                if (isIK == 1) bik.solvers.lookAt.target = target;
                else if (isIK == 2) vik.lookAtObject = target.transform;
            }
            else if (parts == IKBoneType.Aim)
            {
                if (isIK == 1) bik.solvers.aim.target = target;
                else if (isIK == 2)
                {
                    vik.Spine = target.transform;
                    if (name.ToLower() == "self")
                    {
                        vik.SpineReversed = Vector3.zero;
                    }
                    else
                    {
                        vik.SpineReversed = new Vector3(-1, 0, -1);
                    }
                }
            }
            else if (parts == IKBoneType.Chest)
            {
                if (isIK == 1) bik.solvers.spine.target = target;
                else if (isIK == 2)
                {
                    vik.UpperChest = target.transform;
                    if (name.ToLower() == "self")
                    {
                        vik.UpperChestReversed = Vector3.zero;
                    }
                    else
                    {
                        vik.UpperChestReversed = new Vector3(-1, 0, -1);
                    }
                    
                }
            }
            else if (parts == IKBoneType.Pelvis)
            {
                if (isIK == 1) bik.solvers.pelvis.target = target;
                else if (isIK == 2) vik.waist = target.transform;
            }
            //-------------------
            else if (parts == IKBoneType.LeftLowerArm)
            {
                if (isIK == 1) bik.solvers.leftHand.bendGoal = target;
                else if (isIK == 2) vik.LeftLowerArm = target.transform;
            }
            else if (parts == IKBoneType.LeftHand)
            {
                if (isIK == 1) bik.solvers.leftHand.target = target;
                else if (isIK == 2) vik.LeftHand = target.transform;
            }
            else if (parts == IKBoneType.RightLowerArm)
            {
                if (isIK == 1) bik.solvers.rightHand.bendGoal = target;
                else if (isIK == 2) vik.RightLowerArm = target.transform;
            }
            else if (parts == IKBoneType.RightHand)
            {
                if (isIK == 1) bik.solvers.rightHand.target = target;
                else if (isIK == 2) vik.RightHand = target.transform;
            }
            //------------------
            else if (parts == IKBoneType.LeftLowerLeg)
            {
                if (isIK == 1) bik.solvers.leftFoot.bendGoal = target;
                else if (isIK == 2) vik.LeftLowerLeg = target.transform;
            }
            else if (parts == IKBoneType.LeftLeg)
            {
                if (isIK == 1) bik.solvers.leftFoot.target = target;
                else if (isIK == 2) vik.LeftFoot = target.transform;
            }
            else if (parts == IKBoneType.RightLowerLeg)
            {
                if (isIK == 1) bik.solvers.rightFoot.bendGoal = target;
                else if (isIK == 2) vik.RightLowerLeg = target.transform;
            }
            else if (parts == IKBoneType.RightLeg)
            {
                if (isIK == 1) bik.solvers.rightFoot.target = target;
                else if (isIK == 2) vik.RightFoot = target.transform;
            }

            if (!isPlayMode)
            { //---when animation play mode, don't save information.
                int inx = ikMappingList.FindIndex(item =>
                {
                    if (item.parts == parts) return true;
                    return false;
                });

                //---set up as special IK handle. self, specified parts apply all.
                //   self is NOT remove from list!!
                AvatarIKMappingClass aik = new AvatarIKMappingClass();
                aik.parts = parts;
                aik.name = name;
                if (inx == -1)
                { //---if IK parts is not register, add it.
                    ikMappingList.Add(aik);
                }
                else
                {
                    ikMappingList[inx].name = name;
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

        /// <summary>
        /// to apply changes of role title.
        /// </summary>
        /// <param name="param">tab-separated csv-string: [0] - old role title, [1] - new role title</param>
        public void ApplyRenameIKTargetRoleTitle(string param)
        {
            string[] arr = param.Split('\t');
            string oldname = arr[0];
            string newname = arr[1];

            ikMappingList.ForEach(item =>
            {
                if (item.name == oldname)
                {
                    item.name = newname;
                }
            });
        }

        /// <summary>
        /// naturalize ik goal rotation
        /// </summary>
        /// <param name="isEnable"></param>
        public void SetIKGoalRotation2Natural(bool isEnable, string bonename)
        {
            IsGoalRotationNatural = isEnable;

            VvmIk vik = GetComponent<VvmIk>();
            if (vik != null)
            {
                if (isEnable == true)
                {
                    if (bonename == "leftarm") vik.LeftHandRotationWeight = 0;
                    if (bonename == "rightarm") vik.RightHandRotationWeight = 0;
                    if (bonename == "leftleg") vik.LeftFootRotationWeight = 0;
                    if (bonename == "rightleg") vik.RightFootRotationWeight = 0;
                }

                //---apply from HumanBodyBones to IK-marker---
                ApplyRotationHuman2IK(bonename);


                if (isEnable == false)
                {
                    if (bonename == "leftarm") vik.LeftHandRotationWeight = 1f;
                    if (bonename == "rightarm") vik.RightHandRotationWeight = 1f;
                    if (bonename == "leftleg") vik.LeftFootRotationWeight = 1f;
                    if (bonename == "rightleg") vik.RightFootRotationWeight = 1f;
                }
            }
        }
        public void SetIKGoalRotation2NaturalFromOuter(string param)
        {
            string[] arr = param.Split(",");
            SetIKGoalRotation2Natural(arr[0] == "1" ? true : false, arr[1]);
        }
        public void ApplyRotationHuman2IK(string bonename)
        {
            Animator anim = GetComponent<Animator>();
            if (bonename == "leftarm")
            {
                Transform lh = anim.GetBoneTransform(HumanBodyBones.LeftHand);
                GameObject ikla = GetIKHandleByPartsName("leftarm");
                ikla.transform.rotation = lh.rotation * Quaternion.Euler(new Vector3(0, 180f, 0)) * Quaternion.Euler(new Vector3(0, 90f, 0));
            }
            else if (bonename == "rightarm")
            {
                Transform rh = anim.GetBoneTransform(HumanBodyBones.RightHand);
                GameObject ikra = GetIKHandleByPartsName("rightarm");
                ikra.transform.rotation = rh.rotation * Quaternion.Euler(new Vector3(0, 180f, 0)) * Quaternion.Euler(new Vector3(0, -90f, 0));
            }
            else if (bonename == "leftleg")
            {
                Transform lf = anim.GetBoneTransform(HumanBodyBones.LeftFoot);
                GameObject iklf = GetIKHandleByPartsName("leftleg");
                iklf.transform.rotation = lf.rotation;
            }
            else if (bonename == "rightleg")
            {
                Transform rf = anim.GetBoneTransform(HumanBodyBones.RightFoot);
                GameObject ikrf = GetIKHandleByPartsName("rightleg");
                ikrf.transform.rotation = rf.rotation;
            }
        }
        public bool GetIKGoalRotation2Natural(string bonename)
        {
            bool flag = false;

            VvmIk vik = GetComponent<VvmIk>();
            if (bonename == "leftarm") flag = vik.LeftHandRotationWeight == 1 ? false : true;
            if (bonename == "rightarm") flag = vik.RightHandRotationWeight == 1 ? false : true;
            if (bonename == "leftleg") flag = vik.LeftFootRotationWeight == 1 ? false : true;
            if (bonename == "rightleg") flag = vik.RightFootRotationWeight == 1 ? false : true;

            return flag;
        }
        public void GetIKGoalRotation2NaturalFromOuter(string bonename)
        {
            int flag = GetIKGoalRotation2Natural(bonename) ? 1 : 0;
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(flag);
#endif
        }

        //---Head lock=========================================================
        public void SetHeadLock(int flag)
        {
            CCDIK cik = GetComponent<CCDIK>();
            if (cik != null)
            {
                cik.solver.maxIterations = flag;
            }
            
        }
        public int GetHeadLock()
        {
            CCDIK cik = GetComponent<CCDIK>();
            if (cik != null)
            {
                return cik.solver.maxIterations;
            }
            else
            {
                return -1;
            }
            
        }
        public bool IsEnableVRMA()
        {
            bool ret = false;
            
            if (vrminstance.Runtime.VrmAnimation != null)
            {
                ret = true;
            }                                
            
            return ret;
        }
        public string GetVRMAFileName()
        {
            return VrmaFileName;
        }
        public void SetVRMAFileName(string param)
        {
            VrmaFileName = param;
        }
        public void SetVRMAnimation(string param)
        {
            if (param == VrmaFileName) return;

            //Debug.Log("SetVRMA: begin ClearVRMA");
            ClearVRMA();
            //StartCoroutine(LoadVRMAbody(param));

            //---ManageExternalAnimation
            //Debug.Log("SetVRMA: begin GetVRMA");
            var vrma = manim.MexAnim.GetVRMA(param);
            if (vrma != null )
            {
                VrmaFileName = param;
                //Debug.Log("SetVRMA: begin vrminstance.Runtime.VrmAnimation");
                vrminstance.Runtime.VrmAnimation = vrma;
                //Debug.Log("SetVRMA: begin VrmaNativeAnimation");
                VrmaNativeAnimation = vrma.GetComponent<Animation>();
                VrmaNativeAnimation.wrapMode = WrapMode.Default;

                //Debug.Log("SetVRMA: begin CountAddVRMAReference");
                manim.MexAnim.CountAddVRMAReference(param);

                //Debug.Log("SetVRMA: begin relatedTrueIKParent");
                //---change target of trueik
                var oodik = relatedTrueIKParent.GetComponent<OtherObjectDummyIK>();
                oodik.relatedAvatar = gameObject;
                //oodik.transform.localRotation = Quaternion.Euler(oodik.transform.localRotation.eulerAngles + new Vector3(0, 180, 0));

                //---get initial information: clip list, length
                List<string> animclips = ListAnimationClips();
                SetTargetClip(animclips[0]);
                string jsclip = string.Join('=', animclips);
                string clipname = GetTargetClip();
                string[] retarr = {
                    IsEnableVRMA() ? "1" : "0",
                    jsclip,
                    clipname,
                    GetMaxPosAnimation().ToString()
                };
                string htmlret = string.Join("\t", retarr);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(htmlret);
#endif
            }
        }
        public void SetVRMAContinuously(string param)
        {
            if (param == VrmaFileName) return;
            //---ManageExternalAnimation
            var vrma = manim.MexAnim.GetVRMA(param);
            if (vrma != null)
            {
                VrmaFileName = param;
                vrminstance.Runtime.VrmAnimation = vrma;
                VrmaNativeAnimation = vrma.GetComponent<Animation>();

                manim.MexAnim.CountAddVRMAReference(param);

                //---change target of trueik
                var oodik = relatedTrueIKParent.GetComponent<OtherObjectDummyIK>();
                oodik.relatedAvatar = gameObject;
                //oodik.transform.localRotation = Quaternion.Euler(oodik.transform.localRotation.eulerAngles + new Vector3(0, 180, 0));

            }
        }
        public void AdjustVRMAInitialRotation()
        {
            var oodik = relatedTrueIKParent.GetComponent<OtherObjectDummyIK>();
            oodik.transform.localRotation = Quaternion.Euler(oodik.transform.localRotation.eulerAngles + new Vector3(0, 180, 0));
        }
        public IEnumerator LoadVRMAbody(string url)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();
                //if (www.isNetworkError || www.isHttpError)
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError(www.error);
                    yield break;
                }
                else
                {
                    //StartCoroutine(LoadVRM_body(www.downloadHandler.data));
                    StartLoadVRMAbody(www.downloadHandler.data).ConfigureAwait(false);
                }
            }
        }
        public async Task<Vrm10AnimationInstance> StartLoadVRMAbody(byte[] data)
        {
            using GltfData gdata = new GlbBinaryParser(data, "test").Parse();
            using var loader = new VrmAnimationImporter(gdata);
            var instance = await loader.LoadAsync(new ImmediateCaller());

            vrmaInst = instance.GetComponent<Vrm10AnimationInstance>();
            vrminstance.Runtime.VrmAnimation = vrmaInst;
            VrmaNativeAnimation = vrmaInst.GetComponent<Animation>();
            vrminstance.Runtime.VrmAnimation.ShowBoxMan(false);

            //---change target of trueik
            var oodik = relatedTrueIKParent.GetComponent<OtherObjectDummyIK>();
            oodik.relatedAvatar = gameObject;

            //---get initial information: clip list, length
            List<string> animclips = ListAnimationClips();
            SetTargetClip(animclips[0]);
            string jsclip = string.Join('=', animclips);
            string clipname = GetTargetClip();
            string[] retarr = {
                IsEnableVRMA() ? "1" : "0",
                jsclip,
                clipname,
                GetMaxPosAnimation().ToString()
            };
            string htmlret = string.Join("\t", retarr);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(htmlret);
#endif

            return vrmaInst;
        }
        public void ClearVRMA(int is_recover_pose = 0)
        {
            if (IsEnableVRMA())
            {
                if (is_recover_pose == 1) StartCoroutine("ApplyBoneTransformToIKTransform");

                manim.MexAnim.CountDownVRMAReference(VrmaFileName);

                //VrmaNativeAnimation = null;
                vrminstance.Runtime.VrmAnimation = null;
                VrmaFileName = "";
                //vrmaInst.Dispose();
                //vrmaInst = null;

                //---change target of trueik
                // VRM own transform -> handle ikparent, 
                relatedHandleParent.transform.position = gameObject.transform.position;
                relatedHandleParent.transform.rotation = gameObject.transform.rotation;
                gameObject.transform.position = Vector3.zero;
                gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

                var oodik = relatedTrueIKParent.GetComponent<OtherObjectDummyIK>();
                oodik.relatedAvatar = relatedHandleParent;
            }
        }

        /// <summary>
        /// Enable VRMAnimation. IK target change to VRM own.
        /// </summary>
        public void EnableVRMA()
        {
            //if (IsEnableVRMA())
            {
                vrminstance.Runtime.VrmAnimation = vrmaInst;

                //---change target of trueik
                var oodik = relatedTrueIKParent.GetComponent<OtherObjectDummyIK>();
                oodik.relatedAvatar = gameObject;
            }
        }

        /// <summary>
        /// Disable VRMAnimation (not delete). IK target change to relatedHandleParent.
        /// recover current pos/rot to it. VRM pos/rot set 0.
        /// </summary>
        public void DisableVRMA(int is_recover_pose = 0)
        {
            if (IsEnableVRMA())
            {
                if (is_recover_pose == 1) StartCoroutine("ApplyBoneTransformToIKTransform");

                vrminstance.Runtime.VrmAnimation = null;

                //---change target of trueik
                // VRM own transform -> handle ikparent, 
                relatedHandleParent.transform.position = gameObject.transform.position;
                relatedHandleParent.transform.rotation = gameObject.transform.rotation;
                gameObject.transform.position = Vector3.zero;
                gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);

                var oodik = relatedTrueIKParent.GetComponent<OtherObjectDummyIK>();
                oodik.relatedAvatar = relatedHandleParent;
            }
        }

        public void PlayAnimation(int isfirst = 0)
        {

            if (IsEnableVRMA())
            {
                if (!VrmaNativeAnimation.clip.isLooping)
                { //---manually finish flag is OFF
                    triggerCurrentAnimOnFinished = false;
                }
                foreach (AnimationState state in VrmaNativeAnimation)
                {
                    if (VrmaNativeAnimation.clip.name == state.clip.name)
                    {
                        state.time = animationRemainTime;
                    }

                }

                if (isfirst == 1) VrmaNativeAnimation.Rewind();
                VrmaNativeAnimation.Play(VrmaNativeAnimation.clip.name);
            }
        }
        public void PauseAnimation()
        {
            if (IsEnableVRMA())
            {
                if (VrmaNativeAnimation.isPlaying)
                {
                    foreach (AnimationState state in VrmaNativeAnimation)
                    {
                        if (VrmaNativeAnimation.clip.name == state.clip.name)
                        {
                            animationRemainTime = state.time;
                        }
                    }
                    VrmaNativeAnimation.Stop();
                }
                else
                {
                    foreach (AnimationState state in VrmaNativeAnimation)
                    {
                        if (VrmaNativeAnimation.clip.name == state.clip.name)
                        {
                            state.time = animationRemainTime;
                        }
                    }
                    VrmaNativeAnimation.Play();
                }

            }

        }
        public void PauseAnimationFromOuter()
        {
            PauseAnimation();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(animationRemainTime);
#endif
        }
        public void StopAnimation()
        {
            if (IsEnableVRMA())
            {
                VrmaNativeAnimation.Stop();
                animationRemainTime = 0f;
            }
        }
        //----------------------------------------------------------------------------
        public List<string> ListAnimationClips()
        {
            List<string> arr = new List<string>();

            if (IsEnableVRMA())
            {
                foreach (AnimationState state in VrmaNativeAnimation)
                {
                    arr.Add(state.clip.name);
                }
            }

            return arr;
        }
        public void ListAnimationClipsFromOuter()
        {
            List<string> arr = ListAnimationClips();
            string ret = string.Join(',', arr);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public void SetTargetClip(string param)
        {
            if (IsEnableVRMA())
            {
                if (VrmaNativeAnimation.clip == null)
                {
                    VrmaNativeAnimation.clip = VrmaNativeAnimation.GetClip(param);
                }
                else
                {
                    if (param != VrmaNativeAnimation.clip.name)
                    {
                        VrmaNativeAnimation.clip = VrmaNativeAnimation.GetClip(param);
                    }
                }


            }
        }
        public string GetTargetClip()
        {
            string ret = "";

            if (IsEnableVRMA())
            {
                if (VrmaNativeAnimation.clip != null)
                {
                    ret = VrmaNativeAnimation.clip.name;
                }

            }
            return ret;
        }
        public void GetTargetClipFromOuter()
        {
            string ret = GetTargetClip();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        //-----------------------------------------------------
        public int IsPlayingAnimation()
        {
            int ret = 0;

            if (IsEnableVRMA())
            {
                ret = VrmaNativeAnimation.isPlaying ? 1 : 0;
            }

            return ret;
        }
        public void IsPlayingAnimationFromOuter()
        {
            int ret = IsPlayingAnimation();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret);
#endif
        }
        //------------------------------------------------
        public void SetPlayFlagAnimation(UserAnimationState flag)
        {
            animationStartFlag = flag;
        }
        public void SetPlayFlagAnimationFromOuter(int flag)
        {
            animationStartFlag = (UserAnimationState)flag;
        }

        /// <summary>
        /// Get Play flag for Motion
        /// </summary>
        /// <param name="is_contacthtml"></param>
        /// <returns>UserAnimationState</returns>
        public UserAnimationState GetPlayFlagAnimation()
        {
            return animationStartFlag;
        }
        public void GetPlayFlagAnimationFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal((int)animationStartFlag);
#endif
        }
        //-----------------------------------------------------
        public void SetSpeedAnimation(float to)
        {
            if (IsEnableVRMA())
            {
                foreach (AnimationState state in VrmaNativeAnimation)
                {
                    if (VrmaNativeAnimation.clip.name == state.clip.name)
                    {
                        state.speed = to;
                    }
                }
            }
        }
        public float GetSpeedAnimation()
        {
            float ret = 0f;
            if (IsEnableVRMA())
            {
                if (VrmaNativeAnimation.clip != null)
                {
                    foreach (AnimationState state in VrmaNativeAnimation)
                    {
                        if (VrmaNativeAnimation.clip.name == state.clip.name)
                        {
                            ret = state.speed;
                        }
                    }
                }

            }

            return ret;
        }
        public void GetSpeedAnimationFromOuter()
        {
            float ret = GetSpeedAnimation();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
        }
        //-------------------------------------------------------
        public float SeekPosition
        {
            set
            {
                if (IsEnableVRMA())
                {
                    if (value > -1f)
                    {
                        VrmaNativeAnimation.clip.SampleAnimation(VrmaNativeAnimation.gameObject, value);
                        local_animRemainTime = value;
                    }

                }
            }
            get
            {
                return local_animRemainTime;
            }
        }
        public float GetSeekPosAnimation()
        {
            return animationRemainTime;
        }
        public void GetSeekPosAnimationFromOuter()
        {
            float pos = GetSeekPosAnimation();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(animationRemainTime);
#endif
        }
        public void SetSeekPosAnimation(float pos)
        {
            animationRemainTime = pos;
        }
        public virtual void SeekPlayAnimation(float value = -1f)
        {

            if (IsEnableVRMA())
            {
                if (value > -1f)
                {
                    animationRemainTime = value;
                    //if (VrmaNativeAnimation.clip != null)
                        VrmaNativeAnimation.clip.SampleAnimation(VrmaNativeAnimation.gameObject, animationRemainTime);
                }

            }
        }
        //-----------------------------------------------------
        public float GetMaxPosAnimation()
        {
            float ret = 0f;

            if (IsEnableVRMA())
            {
                if (VrmaNativeAnimation.clip != null)
                {
                    ret = VrmaNativeAnimation.clip.length;
                }


            }
            return ret;
        }
        public void GetMaxPosAnimationFromOuter()
        {
            float ret = GetMaxPosAnimation();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
        }
        //-------------------------------------------------------------
        public void SetWrapMode(int flag)
        {
            if (IsEnableVRMA())
            {
                VrmaNativeAnimation.wrapMode = (WrapMode)flag;
            }
        }
        public int GetWrapMode()
        {
            int ret = 0;

            if (IsEnableVRMA())
            {
                ret = (int)VrmaNativeAnimation.wrapMode;
            }
            return ret;
        }
        public void GetWrapModeFromOuter()
        {
            int ret = GetWrapMode();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret);
#endif
        }


        //======================================================================================================================

        public IEnumerator SetupAdditionalExpression ()
        {
            //VRM10Expression ex2 = new VRM10Expression();
            //ex2.MorphTargetBindings = new MorphTargetBinding[] {
            //    new MorphTargetBinding("Face", 6, 1.0f), //BRW_Angry
            //    //new MorphTargetBinding("Face", 17, 1.0f), //EYE_Joy
            //    new MorphTargetBinding("Face", 20, 1.0f) //EYE_Sorrow
            //};
            //ex2.name = "funangry";
            //Dictionary<ExpressionPreset, VRM10Expression>.Enumerator clips = 
            Dictionary<ExpressionKey, VRM10Expression> dic1 = vrminstance.Vrm.Expression.Clips.ToDictionary(x => vrminstance.Vrm.Expression.CreateKey(x.Clip),x => x.Clip);
            List<VRM10Expression> lstvrmexp = new List<VRM10Expression>();
            var dicenum = dic1.GetEnumerator();
            while (dicenum.MoveNext())
            {
                lstvrmexp.Add(dicenum.Current.Value);
            }


            List<VRM10Expression> exlist = new List<VRM10Expression>();
            //---all BlendShapes loop
            foreach (SkinnedMeshRenderer smr in BSMeshs)
            {                                
                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                { //---Loop of current blendshape exists in expression ?

                    int ishit = -1;
                    
                    foreach (VRM10Expression ve in lstvrmexp)
                    { //---Loop for existed expression
                        MorphTargetBinding[] mors = ve.MorphTargetBindings;

                        if (mors.Length > 1)
                        { //---MorphTarget is multi, add each item as single shape.
                            foreach (MorphTargetBinding mor in mors)
                            {
                                if (
                                    (vrminstance.transform.Find(mor.RelativePath) != null)
                                    &&
                                    (mor.Index == i)
                                )
                                { //---if shape exists, but other exists also...?
                                    ishit = -1;
                                }
                            }
                        }
                        else if (mors.Length == 1)
                        {

                            if (
                                (vrminstance.transform.Find(mors[0].RelativePath) != null)
                                &&
                                (mors[0].Index == i)
                            )
                            {
                                ishit = mors[0].Index;
                            }
                        }
                    }
                    if (ishit == -1)
                    { //---current shape not found in the expression[]
                        VRM10Expression exi = ScriptableObject.CreateInstance<VRM10Expression>();
                        Transform stran = smr.transform;
                        string stran_path = UniVRM10.UnityExtensions.RelativePathFrom(stran, vrminstance.transform);
                        exi.MorphTargetBindings = new MorphTargetBinding[] {
                            new MorphTargetBinding(stran_path, i, 1.0f)
                        };
                        exi.name = smr.sharedMesh.GetBlendShapeName(i);
                        int ccinx = vrminstance.Vrm.Expression.CustomClips.FindIndex(match =>
                        {
                            if (match.name == exi.name) return true;
                            return false;
                        });
                        if (ccinx == -1) vrminstance.Vrm.Expression.AddClip(ExpressionPreset.custom, exi);
                    }
                }
                    

                
                
                
            }
            

            //VRM10Expression exp = Resources.Load<VRM10Expression>("vrm/fun_mth"); //Angry_mth
            //ExpressionKey exp2 = ExpressionKey.CreateCustom("angry_mth");
            //ExpressionKey ek = vrminstance.Vrm.Expression.CreateKey(exp);
            //vrminstance.Vrm.Expression.AddClip(ExpressionPreset.custom, exp);
            //vrminstance.Vrm.Expression.AddClip(ExpressionPreset.custom, ex2);
            //Debug.Log(exp.name);
            ReloadProxyCustomExpression();

            yield return null;
        }
    }

}
