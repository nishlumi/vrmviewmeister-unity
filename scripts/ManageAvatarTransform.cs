using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using VRM;
using DG.Tweening;


namespace UserHandleSpace
{
    

    /// <summary>
    /// To manage transform of all avatar object
    /// </summary>
    public class ManageAvatarTransform : MonoBehaviour
    {


        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);

        [DllImport("__Internal")]
        private static extern void sendBackupTransform(byte[] thumbnail, int size, string info);


        public string[] IKbones = { "IKParent", "EyeViewHandle",
                    "Head", "LookAt", "Aim", "Chest", "Pelvis",
                    "LeftShoulder","LeftLowerArm", "LeftHand",
                    "RightShoulder","RightLowerArm","RightHand",
                    "LeftLowerLeg","LeftLeg",
                    "RightLowerLeg","RightLeg",
        };
        /*
        public string[] IKbones = { "IKParent", "EyeViewHandle", "Head", "LookAt", "Aim", "Chest", "Pelvis", "LeftShoulder", "LeftLowerArm", "LeftHand",
            "RightShoulder","RightLowerArm","RightHand","LeftUpperLeg","LeftLowerLeg","LeftLeg","RightUpperLeg","RightLowerLeg","RightLeg"
        };
        */
        const int IKbonesCount = 17;

        const int BODYINFO_COUNT = 9;
        const int HEIGHT_X = 0;
        const int HEIGHT_Y = 1;
        const int HEIGHT_Z = 2;
        const int CHEST_X = 3;
        const int CHEST_Y = 4;
        const int CHEST_Z = 5;
        const int PELVIS_X = 6;
        const int PELVIS_Y = 7;
        const int PELVIS_Z = 8;

        public bool isOpenSaveMode;

        private ManageAnimation maa;

        //TEST CODE
        public List<string> PoseSaveList = new List<string>();

        private void Awake()
        {
            ChangeFullIKType(false);
        }
        // Start is called before the first frame update
        void Start()
        {

            isOpenSaveMode = false;

            maa = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void ChangeFullIKType(bool useFullIK)
        {
            if (useFullIK)
            {
                IKbones = new string [] {
                    "IKParent", "EyeViewHandle", "Head", "LookAt", "Chest",
                    "LeftShoulder", "LeftLowerArm", "LeftHand",
                    "RightShoulder","RightLowerArm","RightHand",
                    "LeftUpperLeg","LeftLowerLeg","LeftLeg",
                    "RightUpperLeg","RightLowerLeg","RightLeg"
                };
            }
            else
            {
                IKbones = new string[] {
                    "IKParent", "EyeViewHandle",
                    "Head", "LookAt", "Aim", "Chest", "Pelvis",
                    "LeftShoulder","LeftLowerArm", "LeftHand",
                    "RightShoulder","RightLowerArm","RightHand",
                    "LeftLowerLeg","LeftLeg",
                    "RightLowerLeg","RightLeg"
                };
            }
        }
        private GameObject _GetMesh(string partsname, Transform parent)
        {
            GameObject ret = null;
            for (int i = 0; i < parent.childCount; i++)
            {
                GameObject cld = parent.GetChild(i).gameObject;
                if (cld.name == partsname)
                {
                    SkinnedMeshRenderer smr;
                    if (cld.TryGetComponent<SkinnedMeshRenderer>(out smr))
                    {
                        ret = cld;
                    }
                    else
                    {
                        ret = null;
                    }
                    break;
                }
            }
            return ret;
        }
        public List<GameObject> CheckSkinnedMeshAvailable()
        {
            List<GameObject> ret = new List<GameObject>();
            SkinnedMeshRenderer [] smrs = transform.GetComponentsInChildren<SkinnedMeshRenderer>();

            for (int i = 0; i < smrs.Length; i++)
            {
                GameObject cld = smrs[i].gameObject;

                ret.Add(cld);
                
            }

            MeshRenderer[] mrs = transform.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < mrs.Length; i++)
            {
                GameObject cld = mrs[i].gameObject;

                ret.Add(cld);

            }

            return ret;
        }
        public GameObject GetFaceMesh(string partsname = "Face")
        {
            //return _GetMesh(partsname, transform);
            GameObject ret = null;
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject cld = transform.GetChild(i).gameObject;
                
                SkinnedMeshRenderer smr;
                if (cld.TryGetComponent<SkinnedMeshRenderer>(out smr))
                {
                    if (smr.sharedMesh.blendShapeCount > 0)
                    {
                        ret = cld;
                        break;
                    }
                }
                else
                {
                    ret = null;
                }
                
            }
            return ret;
        }
        public GameObject GetBodyMesh(string partsname = "Body")
        {
            GameObject ret =  _GetMesh(partsname, transform);

            if (ret == null)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    GameObject cld = transform.GetChild(i).gameObject;
                    SkinnedMeshRenderer smr = null;
                    if (cld.TryGetComponent<SkinnedMeshRenderer>(out smr))
                    {
                        if (smr.sharedMesh.blendShapeCount == 0)
                        {
                            ret = cld;
                            break;
                        }
                    }
                }
            }

            return ret;
        }
        public GameObject GetHairMesh(string partsname = "Hairs")
        {
            //---firstly, Hairs itself.
            GameObject ret =  _GetMesh(partsname, transform);
            if (ret == null)
            {
                //---secondly, children of Hairs.
                for (int i = 0; i < transform.childCount; i++)
                {
                    GameObject cld = transform.GetChild(i).gameObject;
                    if (cld.name == "Hairs")
                    {
                        for (var h = 0; h < cld.transform.childCount; h++)
                        {
                            GameObject haircld = cld.transform.GetChild(h).gameObject;
                            SkinnedMeshRenderer smr;
                            if (haircld.TryGetComponent<SkinnedMeshRenderer>(out smr))
                            {
                                ret = haircld;
                                break;
                            }
                            else
                            {
                                ret = null;
                            }
                        }
                        
                    }
                    
                }
            }
            return ret;
        }

        /// <summary>
        /// Parse body bounds information the difference for the pose and motion
        /// </summary>
        /// <returns></returns>
        public float [] ParseBodyInfo(SkinnedMeshRenderer skinnedmesh)
        {
            float[] ret = new float[3];

            OperateLoadedVRM olvrm = transform.gameObject.GetComponent<OperateLoadedVRM>();
            GameObject ikparent = olvrm.relatedHandleParent;

            GameObject part = GetBodyMesh();
            if (part != null)
            {
                Vector3 bounds = part.GetComponent<SkinnedMeshRenderer>().bounds.extents;
                ret[0] = bounds.x * 2f;
                ret[1] = bounds.y * 2f;
                ret[2] = bounds.z;
                /*ret[3] = ikparent.transform.Find("Chest").GetComponent<UserHandleOperation>().defaultPosition.x;
                ret[4] = ikparent.transform.Find("Chest").GetComponent<UserHandleOperation>().defaultPosition.y;
                ret[5] = ikparent.transform.Find("Chest").GetComponent<UserHandleOperation>().defaultPosition.z;
                ret[6] = ikparent.transform.Find("Pelvis").GetComponent<UserHandleOperation>().defaultPosition.x;
                ret[7] = ikparent.transform.Find("Pelvis").GetComponent<UserHandleOperation>().defaultPosition.y;
                ret[8] = ikparent.transform.Find("Pelvis").GetComponent<UserHandleOperation>().defaultPosition.z;*/
            }

            return ret;
        }
        public float[] ParseBodyInfo(Bounds bnd)
        {
            float[] ret = new float[3];

            OperateLoadedVRM olvrm = transform.gameObject.GetComponent<OperateLoadedVRM>();
            //GameObject ikparent = olvrm.relatedHandleParent;

            Vector3 bounds = bnd.extents;
            ret[0] = bounds.x * 2f;
            ret[1] = bounds.y * 2f;
            ret[2] = bounds.z * 2f;

            /*
            GameObject part = GetBodyMesh();
            if (part != null)
            {
                Vector3 bounds = bnd.extents;
                ret[0] = bounds.x * 2f;
                ret[1] = bounds.y * 2f;
                ret[2] = bounds.z;
                ret[3] = ikparent.transform.Find("Chest").GetComponent<UserHandleOperation>().defaultPosition.x;
                ret[4] = ikparent.transform.Find("Chest").GetComponent<UserHandleOperation>().defaultPosition.y;
                ret[5] = ikparent.transform.Find("Chest").GetComponent<UserHandleOperation>().defaultPosition.z;
                ret[6] = ikparent.transform.Find("Pelvis").GetComponent<UserHandleOperation>().defaultPosition.x;
                ret[7] = ikparent.transform.Find("Pelvis").GetComponent<UserHandleOperation>().defaultPosition.y;
                ret[8] = ikparent.transform.Find("Pelvis").GetComponent<UserHandleOperation>().defaultPosition.z;
            }*/

            return ret;
        }
        public List<Vector3> ParseBodyInfoList(Bounds bnd, GameObject ikparent)
        {
            Transform[] bts = ikparent.GetComponentsInChildren<Transform>();

            List<Vector3> ret = new List<Vector3>();
            //---height
            //ret.Add(new Vector3( bnd.extents.x * 2f, bnd.extents.y * 2f, bnd.extents.z * 2f ));

            //---each parts
            for (int i = 0; i < IKbones.Length; i++)
            {
                string name = IKbones[i];
                
                if (i == 0)
                {
                    ret.Add(new Vector3(  ikparent.transform.position.x, ikparent.transform.position.y, ikparent.transform.position.z ));
                }
                else
                {
                    //Debug.Log(name);
                    GameObject child = null; // ikparent.transform.Find(name).gameObject;
                    foreach (Transform bt in bts)
                    {
                        if (bt.name == name)
                        {
                            child = bt.gameObject;
                            break;
                        }
                    }

                    if (child == null)
                    {
                        ret.Add(Vector3.zero);
                    }
                    else
                    {
                        UserHandleOperation uho = child.GetComponent<UserHandleOperation>();

                        ret.Add(new Vector3( uho.defaultPosition.x, uho.defaultPosition.y, uho.defaultPosition.z ));
                    }
                }
                
            }

            return ret;
        }
        public List<AvatarSingleIKTransform> GetIKTransformAll()
        {

            List<AvatarSingleIKTransform> ret = new List<AvatarSingleIKTransform>();
            OperateLoadedVRM ovrm = gameObject.GetComponent<OperateLoadedVRM>();
            Transform[] bts = ovrm.relatedHandleParent.GetComponentsInChildren<Transform>();

            for (int i = 0; i < IKbones.Length; i++)
            {
                if (i == 0)
                {
                    GameObject ikparent = ovrm.relatedHandleParent;

                    //ret.Add(new Vector3(  ikparent.transform.position.x, ikparent.transform.position.y, ikparent.transform.position.z));
                    ret.Add(new AvatarSingleIKTransform(IKbones[i], ikparent.transform.position, ikparent.transform.rotation.eulerAngles));
                }
                else
                {
                    GameObject child = null;// ovrm.relatedHandleParent.transform.Find(IKbones[i]).gameObject;
                    foreach (Transform bt in bts)
                    {
                        if (bt.name == IKbones[i])
                        {
                            child = bt.gameObject;
                            break;
                        }
                    }

                    if (child != null)
                    {
                        //ret.Add(new Vector3(  child.transform.localPosition.x, child.transform.localPosition.y, child.transform.localPosition.z ));
                        ret.Add(new AvatarSingleIKTransform(IKbones[i], child.transform.localPosition, child.transform.localRotation.eulerAngles));

                    }
                    else
                    {
                        ret.Add(new AvatarSingleIKTransform(IKbones[i], Vector3.zero, Vector3.zero));
                    }
                    
                }
                
            }
            AvatarAllIKParts aai = new AvatarAllIKParts();
            aai.list = ret;
            string js = JsonUtility.ToJson(aai);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
            return ret;
        }
        public void GetIKPelvisAndHeight()
        {
            List<AvatarSingleIKTransform> ret = new List<AvatarSingleIKTransform>();
            OperateLoadedVRM ovrm = gameObject.GetComponent<OperateLoadedVRM>();
            List<Vector3> lst = ovrm.GetTPoseBodyList(true);


            string js = lst[6].x + "," + lst[6].y + "," + lst[6].z;
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
            
        }
        public void SetIKTransformAll(string param)
        {
            AvatarAllIKParts aai = JsonUtility.FromJson<AvatarAllIKParts>(param);
            if (aai != null)
            {
                StartCoroutine(SetIKTransformAll_Body(aai));
            }
        }
        private IEnumerator SetIKTransformAll_Body(AvatarAllIKParts aai)
        {
            OperateLoadedVRM ovrm = gameObject.GetComponent<OperateLoadedVRM>();
            string[] sortedBones = {
                    "IKParent",
                    "Pelvis","Chest","Head", "Aim", "LookAt",
                    "LeftLeg",
                    "RightLeg",
                    "LeftLowerLeg",
                    "RightLowerLeg",

                    "LeftShoulder",
                    "LeftHand", "LeftLowerArm",
                    "RightShoulder",
                    "RightHand", "RightLowerArm",

                    "EyeViewHandle"
                };
            sortedBones = IKbones;
            Transform[] bts = ovrm.relatedHandleParent.GetComponentsInChildren<Transform>();

            /*Sequence seq = DOTween.Sequence();
            TweenCallback cb_comp = () =>
            {
                maa.FinishPreviewMarker2();
            };
            seq.OnComplete(cb_comp);*/

            //maa.PreparePreviewMarker();
            for (int i = 1; i < sortedBones.Length; i++)
            {

                {
                    AvatarSingleIKTransform asit = aai.list.Find(match =>
                    {
                        if (match.ikname.ToLower() == sortedBones[i].ToLower()) return true;
                        return false;
                    });
                    if (asit != null)
                    {
                        GameObject child = null; // ovrm.relatedHandleParent.transform.Find(asit.ikname).gameObject;
                        foreach (Transform bt in bts)
                        {
                            if (bt.name == asit.ikname)
                            {
                                child = bt.gameObject;
                                break;
                            }
                        }

                        if (child != null)
                        {
                            child.transform.localPosition = asit.position;
                            child.transform.localRotation = Quaternion.Euler(asit.rotation);
                            //seq.Join(child.transform.DOLocalMove(asit.position, 0.1f));
                            //seq.Join(child.transform.DOLocalRotate(asit.rotation, 0.1f));
                            yield return null;
                            
                        }

                    }
                }
            }
            //---IKParent
            GameObject ikparent = ovrm.relatedHandleParent;
            ikparent.transform.position = aai.list[0].position; // new Vector3(aai.list[i].x, aai.list[i].y, aai.list[i].z);
            ikparent.transform.rotation = Quaternion.Euler(aai.list[0].rotation);
            //seq.Join(ikparent.transform.DOMove(aai.list[0].position, 0.01f));
            //seq.Join(ikparent.transform.DORotate(aai.list[0].rotation, 0.01f));
        }

        /// <summary>
        /// Set IK transform (single)
        /// </summary>
        /// <param name="param">CSV-string: [0] - ikname index, [1] - p=position, r=rotation, [2] - x, y, z, [3] - float value</param>
        public void SetIKTransformEach(string param)
        {
            string[] prm = param.Split(',');
            string ikname = prm[0];
            int ikpos = int.TryParse(prm[0], out ikpos) ? ikpos : -1;
            string transformType = prm[1];
            string vectorType = prm[2];
            float val = float.TryParse(prm[3], out val) ? val : 0f;

            if (ikpos == -1) return;

            OperateLoadedVRM ovrm = gameObject.GetComponent<OperateLoadedVRM>();
            Transform[] bts = ovrm.relatedHandleParent.GetComponentsInChildren<Transform>();

            GameObject parts = null;
            if (ikpos == 0)
            {
                parts = ovrm.relatedHandleParent;
            }
            else
            {
                //parts = ovrm.relatedHandleParent.transform.Find(IKbones[ikpos]).gameObject;
                foreach (Transform bt in bts)
                {
                    if (bt.name == IKbones[ikpos])
                    {
                        parts = bt.gameObject;
                        break;
                    }
                }
            }

            if (parts == null) return;

            if (transformType == "p")
            {
                if (vectorType == "x")
                {
                    if (ikpos == 0) parts.transform.position = new Vector3(val, parts.transform.position.y, parts.transform.position.z);
                    else parts.transform.localPosition = new Vector3(val, parts.transform.position.y, parts.transform.position.z);
                }
                else if (vectorType == "y")
                {
                    if (ikpos == 0) parts.transform.position = new Vector3(parts.transform.position.x, val, parts.transform.position.z);
                    else parts.transform.localPosition = new Vector3(parts.transform.position.x, val, parts.transform.position.z);
                }
                else if (vectorType == "z")
                {
                    if (ikpos == 0) parts.transform.position = new Vector3(parts.transform.position.x, parts.transform.position.y, val);
                    else parts.transform.localPosition = new Vector3(parts.transform.position.x, parts.transform.position.y, val);
                }
            }
            else if (transformType == "r")
            {
                if (vectorType == "x")
                {
                    if (ikpos == 0) parts.transform.rotation = Quaternion.Euler(new Vector3(val, parts.transform.rotation.y, parts.transform.rotation.z));
                    else parts.transform.localRotation = Quaternion.Euler(new Vector3(val, parts.transform.localRotation.y, parts.transform.localRotation.z));

                }
                else if (vectorType == "y")
                {
                    if (ikpos == 0) parts.transform.rotation = Quaternion.Euler(new Vector3(parts.transform.rotation.x, val, parts.transform.rotation.z));
                    else parts.transform.localRotation = Quaternion.Euler(new Vector3(parts.transform.localRotation.x, val, parts.transform.localRotation.z));
                }
                else if (vectorType == "z")
                {
                    if (ikpos == 0) parts.transform.rotation = Quaternion.Euler(new Vector3(parts.transform.rotation.x, parts.transform.rotation.y, val));
                    else parts.transform.localRotation = Quaternion.Euler(new Vector3(parts.transform.localRotation.x, parts.transform.localRotation.y, val));
                }
            }

        }
        //=============================================================================================================================================
        private List<Vector3> BackupAllPostion(GameObject ikparent, string type)
        {

            List<Vector3> ret = new List<Vector3>();
            /*Animator anime = ActiveAvatar.GetComponent<Animator>();
            foreach (HumanBodyBones value in Enum.GetValues(typeof(HumanBodyBones)))
            {
                Transform tran = anime.GetBoneTransform(value);
                ret.Add(tran.position);
                string name = Enum.GetName(typeof(HumanBodyBones), value);
            }*/
            Transform[] bts = ikparent.GetComponentsInChildren<Transform>();

            ret.Add(ikparent.transform.position);  //---IKHandleWorld
            for (int i = 1; i < IKbones.Length; i++)
            {
                GameObject child = null; //ikparent.transform.Find(IKbones[i])
                foreach (Transform bt in bts)
                {
                    if (bt.name == IKbones[i])
                    {
                        child = bt.gameObject;
                        break;
                    }
                }
                ret.Add(child.transform.localPosition);
            }



            return ret;
        }
        private List<Vector3> BackupAllRotation(GameObject ikparent, string type)
        {
            List<Vector3> ret = new List<Vector3>();

            /*Animator anime = ActiveAvatar.GetComponent<Animator>();
            foreach (HumanBodyBones value in Enum.GetValues(typeof(HumanBodyBones)))
            {
                Transform tran = anime.GetBoneTransform(value);
                ret.Add(tran.rotation);
                string name = Enum.GetName(typeof(HumanBodyBones), value);
            }*/
            Transform[] bts = ikparent.GetComponentsInChildren<Transform>();

            ret.Add(ikparent.transform.rotation.eulerAngles);  //---IKHandleWorld
            for (int i = 1; i < IKbones.Length; i++)
            {
                GameObject child = null; //ikparent.transform.Find(IKbones[i])
                foreach (Transform bt in bts)
                {
                    if (bt.name == IKbones[i])
                    {
                        child = bt.gameObject;
                        break;
                    }
                }
                ret.Add(child.transform.rotation.eulerAngles);
            }

            /*VRIK vik = ActiveAvatar.GetComponent<VRIK>();
            ret.Add(new Quaternion(vik.solver.leftArm.swivelOffset, 0f, 0f, 0f));
            ret.Add(new Quaternion(vik.solver.rightArm.swivelOffset, 0f, 0f, 0f));
            ret.Add(new Quaternion(vik.solver.leftLeg.swivelOffset, 0f, 0f, 0f));
            ret.Add(new Quaternion(vik.solver.rightLeg.swivelOffset, 0f, 0f, 0f));*/

            return ret;
        }
        public List<float> BackupHand(GameObject avatar, string type)
        {
            List<float> ret = new List<float>();
            if (type.ToLower() == "vrm")
            {
                LeftHandPoseController lhand = avatar.GetComponent<LeftHandPoseController>();
                RightHandPoseController rhand = avatar.GetComponent<RightHandPoseController>();

                ret.Add((float)lhand.currentPose);
                ret.Add(lhand.handPoseValue);

                ret.Add((float)rhand.currentPose);
                ret.Add(rhand.handPoseValue);

            }

            return ret;
        }
        private List<BasicStringFloatList> BackupBlendShape(GameObject avatar, string type)
        {
            List<BasicStringFloatList> ret = new List<BasicStringFloatList>();
            if (type.ToLower() == "vrm")
            {
                GameObject mainface = GetFaceMesh();
                SkinnedMeshRenderer face = mainface.GetComponent<SkinnedMeshRenderer>();
                int bscnt = face.sharedMesh.blendShapeCount;
                for (int i = 0; i < bscnt; i++)
                {
                    ret.Add(new BasicStringFloatList(face.sharedMesh.GetBlendShapeName(i),face.GetBlendShapeWeight(i)));

                }
            }

            return ret;
        }
        private List<Vector3> BackupBoneRotation(string type)
        {
            List<Vector3> ret = new List<Vector3>();

            if (type.ToLower() == "vrm")
            {
                Animator conanime = transform.gameObject.GetComponent<Animator>();
                foreach (HumanBodyBones hbb in Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if (hbb != HumanBodyBones.LastBone)
                    {
                        Transform tf = conanime.GetBoneTransform(hbb);
                        if (tf != null)
                        {
                            ret.Add(tf.rotation.eulerAngles);
                        }
                    }
                }
            }

            return ret;
        }
        /// <summary>
        /// Back up transform information of target avatar(VRM, OtherObject)
        /// </summary>
        /// <param name="param">avatar's type(VRM, etc...)</param>
        /// <returns></returns>
        public string BackupAvatarTransform(string param)
        {
            string ret = "";
            GameObject ikparent = null;
            AvatarTransformSaveClass atran = new AvatarTransformSaveClass();

            if (param.ToLower() == "vrm")
            {
                
                atran.sampleavatar = transform.gameObject.GetComponent<VRMMeta>().Meta.Title;

            }
            /*
            else
            {
                OperateLoadedOther olo = transform.gameObject.GetComponent<OperateLoadedOther>();
                ikparent = olo.relatedHandleParent;
            }
            if (ikparent != null)
            {
                atran.positions = BackupAllPostion(ikparent, param);
                atran.rotations = BackupAllRotation(ikparent, param);
            }
            atran.type = param;
            atran.handpose = BackupHand(transform.gameObject, param);
            atran.blendshapes = BackupBlendShape(transform.gameObject, param);
            //atran.bonerotations = BackupBoneRotation(param);

            //---options
            atran.thumbnail = "";
            atran.duration = 0f;
            atran.useik = 1;
            */

            //---save new code
            AnimationRegisterOptions aro = new AnimationRegisterOptions();
            aro.index = 1;
            aro.targetId = gameObject.name;
            aro.targetType = AF_TARGETTYPE.VRM;
            NativeAnimationFrameActor savedActor = new NativeAnimationFrameActor();
            NativeAnimationFrameActor actor = maa.GetFrameActorFromObjectID(gameObject.name, AF_TARGETTYPE.VRM);
            savedActor = actor.SCopy();

            NativeAnimationFrame fr = maa.SaveFrameData(1, -1, actor, aro);
            //savedActor.frames.Add(fr);

            AnimationFrameActor afactor = new AnimationFrameActor();
            afactor.SetFromNative(savedActor);

            
            AnimationFrame afg = new AnimationFrame();
            afg.duration = fr.duration;
            afg.finalizeIndex = fr.finalizeIndex;
            afg.index = fr.index;
            afg.key = fr.key;
            foreach (AnimationTargetParts mv in fr.movingData)
            {
                afg.movingData.Add(maa.DataToCSV(afactor.targetType, mv));
                //afg.movingData.Add(mv);
            }
            int isHit = afactor.frames.FindIndex(item =>
            {
                if (item.index == afg.index) return true;
                return false;
            });
            if (isHit < 0)
            {
                afactor.frames.Add(afg);
            }
            else
            {
                afactor.frames[isHit] = afg;
            }
                
            atran.frameData = afactor;

            //---Take thumbnail screenshot for This pose
            byte[] img = Camera.main.GetComponent<ScreenShot>().CaptureThumbnail(1, transform.gameObject);

            //---TEST CODE
#if UNITY_EDITOR
            Texture2D tex = new Texture2D(320, 240, TextureFormat.RGBA32, false);
            tex.LoadImage(img);
            GameObject.Find("testimage1").GetComponent<Image>().sprite = Sprite.Create(tex, new Rect(0, 0, 320, 240), Vector2.zero);
#endif


            ret = JsonUtility.ToJson(atran);

#if !UNITY_EDITOR && UNITY_WEBGL
            sendBackupTransform(img, img.Length, ret);
#endif
            return ret;
        }
        //=============================================================================================================================================
        private void RestoreAllPosition(GameObject ikparent, string type, float[] bounds, List<Vector3> lst)
        {
            ikparent.transform.position = lst[0];

            GameObject body = GetBodyMesh();
            Bounds bnd = body.GetComponent<SkinnedMeshRenderer>().bounds;

            Vector3 loadTargetPelvis = Vector3.zero;
            Vector3 loadTargetExtents = Vector3.zero;
            Vector3 currentTargetExtents = Vector3.zero;

            if (type.ToLower() == "vrm")
            {
                //---base is pelvis.
                loadTargetPelvis.y = bounds[PELVIS_Y] - lst[6].y;
                loadTargetExtents.x = bounds[HEIGHT_X];
                loadTargetExtents.y = bounds[HEIGHT_Y];
                loadTargetExtents.z = bounds[HEIGHT_Z];

                currentTargetExtents.x = bnd.extents.x * 2f;
                currentTargetExtents.y = bnd.extents.y * 2f;
                currentTargetExtents.z = bnd.extents.z * 2f;

            }
            Transform[] bts = ikparent.GetComponentsInChildren<Transform>();


            for (int i = 1; i < lst.Count; i++)
            {
                Transform boneTran = null;// ikparent.transform.Find(IKbones[i]);
                foreach (Transform bt in bts)
                {
                    if (bt.name == IKbones[i])
                    {
                        boneTran = bt;
                        break;
                    }
                }

                //---Absorb the difference in height.
                Vector3 fnl;

                fnl.x = currentTargetExtents.x * (lst[i].x / loadTargetExtents.x);
                if (i == 6)
                {
                    //---Pelvis only: add sample result of "rest pelvis - pose pelvis"
                    fnl.y = currentTargetExtents.y * (lst[i].y / loadTargetExtents.y) + loadTargetPelvis.y;
                }
                else
                {
                    fnl.y = currentTargetExtents.y * (lst[i].y / loadTargetExtents.y);
                }

                fnl.z = currentTargetExtents.x * (lst[i].z / loadTargetExtents.z);


                boneTran.localPosition = fnl;
                
            }

        }
        private void RestoreAllRotation(GameObject ikparent, string type, float[] bounds, List<Vector3> lst)
        {
            Transform[] bts = ikparent.GetComponentsInChildren<Transform>();
            ikparent.transform.rotation = Quaternion.Euler(lst[0]);

            for (int i = 1; i < lst.Count; i++)
            {
                Transform child = null; //ikparent.transform.Find(IKbones[i])
                foreach (Transform bt in bts)
                {
                    if (bt.name == IKbones[i])
                    {
                        child = bt;
                        break;
                    }
                }
                child.localRotation = Quaternion.Euler(lst[i]);
            }
        }
        private void RestoreHand(GameObject avatar, string type, List<float> lst)
        {
            LeftHandPoseController lhand = avatar.GetComponent<LeftHandPoseController>();
            RightHandPoseController rhand = avatar.GetComponent<RightHandPoseController>();

            //---left
            if (lst.Count > 0) lhand.currentPose = (int)lst[0];
            if (lst.Count > 1) lhand.handPoseValue = (int)lst[1];

            //---right
            if (lst.Count > 2) rhand.currentPose = (int)lst[2];
            if (lst.Count > 3) rhand.handPoseValue = (int)lst[3];

        }
        private void RestoreBlendShape(GameObject avatar, string type, List<BasicStringFloatList> lst)
        {
            if (type.ToLower() == "vrm")
            {
                GameObject mainface = GetFaceMesh();
                SkinnedMeshRenderer face = mainface.GetComponent<SkinnedMeshRenderer>();
                lst.ForEach((BasicStringFloatList val) =>
                {
                    float weight = val.value;
                    int bindex = face.sharedMesh.GetBlendShapeIndex(val.text);
                    if (bindex > -1) face.SetBlendShapeWeight(bindex, weight);

                });
            }
        }
        private void RestoreBoneRotation(string type, List<Vector3> lst)
        {

            Animator conanime = transform.gameObject.GetComponent<Animator>();
            for (int i = 0; i < lst.Count; i++)
            {
                conanime.GetBoneTransform((HumanBodyBones)i).rotation = Quaternion.Euler(lst[i]);
            }

        }
        public void RestoreAvatarTransform(string param)
        {

            AvatarTransformSaveClass atran = JsonUtility.FromJson<AvatarTransformSaveClass>(param);
            /*
            GameObject ikparent = null;
            if (atran.type.ToLower() == "vrm")
            {
                OperateLoadedVRM olvrm = transform.gameObject.GetComponent<OperateLoadedVRM>();
                ikparent = olvrm.relatedHandleParent;

                //---restore all transform as HumanBodyBone.
                //if (atran.useik != 1) olvrm.SetEnableWholeIK(0);
            }
            else
            {
                OperateLoadedOther olvrm = transform.gameObject.GetComponent<OperateLoadedOther>();
                ikparent = olvrm.relatedHandleParent;
            }

            
            if (atran.useik == 1)
            {
                RestoreAllPosition(ikparent, atran.type, atran.bodyinfo, atran.positions);
                RestoreAllRotation(ikparent, atran.type, atran.bodyinfo, atran.rotations);
            }
            else
            {

                RestoreBoneRotation(atran.type, atran.bonerotations);
            }
            
            RestoreBlendShape(ikparent, atran.type, atran.blendshapes);
            RestoreHand(transform.gameObject, atran.type, atran.handpose);
            */
        }
        //=============================================================================================================================================
        private void AnimateAllPosition(GameObject ikparent, string type, float[] bounds, List<Vector3> lst, float duration)
        {
            float distY = 0;
            float distX = 0;

            GameObject body = GetBodyMesh();
            Bounds bnd = body.GetComponent<SkinnedMeshRenderer>().bounds;
            float exHeight = bnd.extents.y * 2f;
            float asistWR = 0;
            float asistX_WR = 0;

            if (type.ToLower() == "vrm")
            {

                //---substract rest's pelvis - pose's pelvis
                float restWR = bounds[PELVIS_Y] * (bounds[HEIGHT_Y]);
                float poseWR = lst[6].y * (bounds[HEIGHT_Y]);
                asistWR = restWR - poseWR;

                asistX_WR = (
                    ((bounds[CHEST_X] * 2) * bounds[HEIGHT_X])
                    -
                    (lst[5].x * bounds[HEIGHT_X])
                    ) / 2f
                ;
                

                distY = exHeight - (bounds[HEIGHT_Y]);
                //distPelvis = bounds[PELVIS_Y] - lst[6].y;
                //distY -= distPelvis;

                distX = (bnd.extents.x*2f - bounds[HEIGHT_X]) / 2f;
            }

            Sequence seq = DOTween.Sequence();

            seq.Append(ikparent.transform.DOMove(lst[0], duration));

            Transform[] bts = ikparent.GetComponentsInChildren<Transform>();

            for (int i = 1; i < lst.Count; i++)
            {
                Transform boneTran = null;// ikparent.transform.Find(IKbones[i]);
                foreach (Transform bt in bts)
                {
                    if (bt.name == IKbones[i])
                    {
                        boneTran = bt;
                        break;
                    }
                }

                Vector3 defPos = boneTran.GetComponent<UserHandleOperation>().defaultPosition;

                //---Absorb the difference in height.
                float fnlX = (lst[i].x < 0 ? lst[i].x - distX : lst[i].x + distX);
                float fnlY = 0;
                if ((i == 12) || (i == 14))  //---Leg
                {
                    fnlY = lst[i].y;

                    fnlX = (lst[i].x < 0 ? lst[i].x - distX : lst[i].x + distX);
                    float eachPartsX_WR = (fnlX / (bnd.extents.x * 2f)) - asistX_WR;
                    fnlX = eachPartsX_WR * (bnd.extents.x * 2f);
                }
                else
                {
                    if ((1 <= i) && (i <= 6)) //---Arm, hand
                    {
                        fnlX = (lst[i].x < 0 ? lst[i].x - distX : lst[i].x + distX);
                        float eachPartsX_WR = (fnlX / (bnd.extents.x * 2f)) - asistX_WR;
                        fnlX = eachPartsX_WR * (bnd.extents.x * 2f);
                    }
                    //---各部位と身長を割り、割合を出す。
                    //   そこから腰で割り出した補正値を減算＝補正割合
                    //   最終的に補正割合と身長を割り、各部位の最終値を出す。
                    fnlY = lst[i].y + distY;
                    float eachPartsWR = (fnlY / exHeight) - asistWR;
                    fnlY = eachPartsWR * exHeight;
                }

                seq.Join(boneTran.DOLocalMove(new Vector3(fnlX, fnlY, lst[i].z), duration));

            }

        }
        private Sequence AnimateAllPosition2(Sequence seq, GameObject ikparent, string type, float[] bounds, List<Vector3> lst, float duration)
        {
            //GameObject body = GetBodyMesh();
            Bounds bnd = transform.GetComponent<OperateLoadedVRM>().GetTPoseBodyInfo(); //body.GetComponent<SkinnedMeshRenderer>().bounds;

            Vector3 loadTargetPelvis = Vector3.zero;
            Vector3 loadTargetExtents = Vector3.zero;
            Vector3 currentTargetExtents = Vector3.zero;

            if (type.ToLower() == "vrm")
            {
                //---base is pelvis.
                loadTargetPelvis.y = bounds[PELVIS_Y] - lst[6].y;
                loadTargetExtents.x = bounds[HEIGHT_X];
                loadTargetExtents.y = bounds[HEIGHT_Y];
                loadTargetExtents.z = bounds[HEIGHT_Z];

                currentTargetExtents.x = bnd.extents.x * 2f;
                currentTargetExtents.y = bnd.extents.y * 2f;
                currentTargetExtents.z = bnd.extents.z;

            }
            //Sequence seq = DOTween.Sequence();

            seq.Append(ikparent.transform.DOMove(lst[0], duration));

            Transform[] bts = ikparent.GetComponentsInChildren<Transform>();
            for (int i = 1; i < lst.Count; i++)
            {
                Transform boneTran = null;// ikparent.transform.Find(IKbones[i]);
                foreach (Transform bt in bts)
                {
                    if (bt.name == IKbones[i])
                    {
                        boneTran = bt;
                        break;
                    }
                }

                Vector3 fnl;
                
                //---Absorb the difference in height.
                fnl.x = currentTargetExtents.x * (lst[i].x / loadTargetExtents.x);
                if (i == 6)
                {
                    //---Pelvis only: add sample result of "rest pelvis - pose pelvis"
                    fnl.y = currentTargetExtents.y * (lst[i].y / loadTargetExtents.y) + loadTargetPelvis.y;
                }
                else
                {
                    fnl.y = currentTargetExtents.y * (lst[i].y / loadTargetExtents.y);
                }
                
                fnl.z = currentTargetExtents.z * (lst[i].z / loadTargetExtents.z);


                seq.Join(boneTran.DOLocalMove(new Vector3(fnl.x, fnl.y, lst[i].z), duration));
            }
            return seq;
        }
        private IEnumerator AnimateAllPosition3(Sequence seq, GameObject ikparent, string type, List<Vector3> bodyinfoList, List<Vector3> lst, float duration)
        {
            OperateLoadedVRM ovrm = gameObject.GetComponent<OperateLoadedVRM>();
            List<Vector3> curbodyList = ovrm.GetTPoseBodyList();

            /*
             * curbodyList, bodyinfoList ... [0]~[n]=Ik parts. calculate auto asist
             */
            int[] sortedIndex = new int[IKbonesCount] {
                (int)ParseIKBoneType.IKParent, 
                (int)ParseIKBoneType.EyeViewHandle, (int)ParseIKBoneType.Head,
                (int)ParseIKBoneType.LookAt, (int)ParseIKBoneType.Aim, 
                (int)ParseIKBoneType.Chest, (int)ParseIKBoneType.Pelvis,
                (int)ParseIKBoneType.LeftShoulder,
                (int)ParseIKBoneType.LeftHand, (int)ParseIKBoneType.LeftLowerArm,
                (int)ParseIKBoneType.RightShoulder,
                (int)ParseIKBoneType.RightHand, (int)ParseIKBoneType.RightLowerArm,
                (int)ParseIKBoneType.LeftLeg, (int)ParseIKBoneType.RightLeg,
                (int)ParseIKBoneType.LeftLowerLeg, (int)ParseIKBoneType.RightLowerLeg
            };

            Transform[] bts = ikparent.GetComponentsInChildren<Transform>();

            for (int srti = sortedIndex.Length-1; srti >= 0; srti--)
            {
                int i = sortedIndex[srti];

                int sI = i;
                Vector3 modlst = lst[i];

                Vector3 vec = Vector3.zero;
                Vector3 qv = Vector3.zero;
                Vector3 curElem = curbodyList[sI];
                Vector3 bodyElem = bodyinfoList[sI];

                qv = curElem - bodyElem;

                //fqv = (qv.x + qv.y + qv.z) / 3f;
                //fqv = (curElem.x + curElem.y + curElem.z) / (bodyElem.x + bodyElem.y + bodyElem.z);
                /*
                if (curElem.x < bodyElem.x)
                {
                    
                }
                else
                {
                    if (curElem.x != 0f) qv.x = bodyElem.x / curElem.x;
                }
                if (curElem.y < bodyElem.y)
                {
                    
                }
                else
                {
                    if (curElem.y != 0f) qv.y = bodyElem.y / curElem.y;
                }
                if (curElem.z < bodyElem.z)
                {
                    
                }
                else
                {
                    if (curElem.z != 0f) qv.z = bodyElem.z / curElem.z;
                }*/

                //---set finally
                if (i == 0)
                {
                    //Debug.Log("i=" + i + "[]=" + lst[0].x + "," + lst[0].y + "," + lst[0].z);
                    seq.Append(ikparent.transform.DOMove(modlst, duration));
                }
                else
                {

                    Transform boneTran = null;// ikparent.transform.Find(IKbones[i]);
                    foreach (Transform bt in bts)
                    {
                        if (bt.name == IKbones[i])
                        {
                            boneTran = bt;
                            break;
                        }
                    }

                    Vector3 fnlpos = Vector3.zero;

                    fnlpos = modlst + qv;


                    //Debug.Log("i=" + i + "[" + IKbones[i] + "]=" +  lst[i].x  + "," + lst[i].y + "," + lst[i].z );
                    //Debug.Log("      qc=" + qv.x + "," +  qv.y + "," +  qv.z);
                    //Debug.Log("   final=" + lst[i].x + qv.x + "," + lst[i].y + qv.y + "," + lst[i].z + qv.z);

                    //seq.Join(boneTran.DOLocalMove(new Vector3(lst[i].x * qv.x, lst[i].y * qv.y, lst[i].z * qv.z), duration));
                    seq.Join(boneTran.DOLocalMove(fnlpos, duration));
                }
                if (srti == 12)
                {
                    yield return null;
                }

            }
            yield return null;
            //return seq;
        }
        private Sequence AnimateAllRotation(Sequence seq, GameObject ikparent, string type, float[] bounds, List<Vector3> lst, float duration)
        {
            Transform[] bts = ikparent.GetComponentsInChildren<Transform>();

            //Sequence seq = DOTween.Sequence();
            seq.Append(ikparent.transform.DORotate(lst[0],duration));

            for (int i = 1; i < lst.Count; i++)
            {
                Transform child = null; //ikparent.transform.Find(IKbones[i])
                foreach (Transform bt in bts)
                {
                    if (bt.name == IKbones[i])
                    {
                        child = bt;
                        break;
                    }
                }
                seq.Join(child.DOLocalRotate(lst[i], duration));
            }
            return seq;
        }
        private void AnimateHand(GameObject avatar, string type, List<float> lst, float duration)
        {
            LeftHandPoseController lhand = avatar.GetComponent<LeftHandPoseController>();
            RightHandPoseController rhand = avatar.GetComponent<RightHandPoseController>();

            //---left
            if (lst.Count > 0) lhand.currentPose = (int)lst[0];
            if (lst.Count > 1) lhand.handPoseValue = (int)lst[1];

            //---right
            if (lst.Count > 2) rhand.currentPose = (int)lst[2];
            if (lst.Count > 3) rhand.handPoseValue = (int)lst[3];
        }
        private void AnimateBlendShape(GameObject avatar, string type, List<BasicStringFloatList> lst, float duration)
        {
            if (type.ToLower() == "vrm")
            {
                GameObject mainface = GetFaceMesh();
                SkinnedMeshRenderer face = mainface.GetComponent<SkinnedMeshRenderer>();
                lst.ForEach((BasicStringFloatList val) =>
                {
                    float weight = val.value;
                    int bindex = face.sharedMesh.GetBlendShapeIndex(val.text);
                    if (bindex > -1) face.SetBlendShapeWeight(bindex, weight);

                });
            }
        }
        private Sequence AnimateBoneRotation(Sequence seq, string type, List<Vector3> lst, float duration)
        {
            //Sequence seq = DOTween.Sequence();

            Animator conanime = transform.gameObject.GetComponent<Animator>();

            seq.Append(conanime.GetBoneTransform((HumanBodyBones)0).DORotate(lst[0], duration));
            for (int i = 1; i < lst.Count; i++)
            {
                seq.Join(conanime.GetBoneTransform((HumanBodyBones)i).DORotate(lst[i],duration));
            }
            return seq;
        }
        public void AnimateAvatarTransform(string param)
        {
            AvatarTransformSaveClass atran = JsonUtility.FromJson<AvatarTransformSaveClass>(param);
            GameObject ikparent = null;
            isOpenSaveMode = true;
            /*
            if (atran.type.ToLower() == "vrm")
            {
                OperateLoadedVRM olvrm = transform.gameObject.GetComponent<OperateLoadedVRM>();
                ikparent = olvrm.relatedHandleParent;

                
                olvrm.SetEnableWholeIK(atran.useik);
                
            }
            else
            {
                OperateLoadedOther olo = transform.gameObject.GetComponent<OperateLoadedOther>();
                ikparent = olo.relatedHandleParent;
            }

            Sequence seq = DOTween.Sequence();
            seq.OnComplete(() =>
            {
                isOpenSaveMode = false;
            });
            if (atran.useik == 1)
            {
                //seq = AnimateAllPosition2(seq, ikparent, atran.type, atran.bodyHeight, atran.positions, atran.duration);
                StartCoroutine(AnimateAllPosition3(seq, ikparent, atran.type, atran.bodyInfoList, atran.positions, atran.duration));
                seq = AnimateAllRotation(seq, ikparent, atran.type, atran.bodyinfo, atran.rotations, atran.duration);

            }
            else
            {
                seq = AnimateBoneRotation(seq, atran.type, atran.bonerotations, atran.duration);
            }

            AnimateHand(transform.gameObject, atran.type, atran.handpose, atran.duration);
            AnimateBlendShape(transform.gameObject, atran.type, atran.blendshapes, atran.duration);

            */

            //---open new code
            AnimationParsingOptions aro = new AnimationParsingOptions();
            aro.index = 1;
            aro.finalizeIndex = 1;
            aro.endIndex = 1;
            aro.isBuildDoTween = 0;
            aro.isExecuteForDOTween = 1;
            aro.targetType = AF_TARGETTYPE.VRM;
            
            NativeAnimationFrameActor nact = new NativeAnimationFrameActor();
            nact.SetFromRaw(atran.frameData);
            NativeAnimationAvatar nav = maa.currentProject.casts.Find(match =>
            {
                if (match.avatarId == gameObject.name) return true;
                return false;
            });
            nact.avatar = nav;

            foreach (AnimationFrame fr in atran.frameData.frames)
            {
                NativeAnimationFrame nframe = maa.ParseEffectiveFrame(nact, fr);
                nact.frames.Add(nframe);
            }
            aro.targetRole = nact.targetRole;

            maa.PreviewSingleFrame(nact,aro);
        }
    }

    public class BodyCalculator
    {
        public List<Vector3> Cast;
        public List<Vector3> Character;

        public List<Vector3> DistanceCast;
        public List<Vector3> DistanceCharacter;

        public BodyCalculator()
        {
            Cast = new List<Vector3>();
            Character = new List<Vector3>();

            DistanceCast = new List<Vector3>();
            DistanceCharacter = new List<Vector3>();
        }
        public void Clear()
        {
            Cast.Clear();
            Character.Clear();
            DistanceCast.Clear();
            DistanceCharacter.Clear();
        }
        public void SetCast(List<Vector3> lst)
        {
            Cast.Clear();
            DistanceCast.Clear();
            foreach (Vector3 v in lst)
            {
                Cast.Add(new Vector3(v.x, v.y, v.z));
            }
            DistanceCast.Add(lst[(int)IKBoneType.Head] - lst[(int)IKBoneType.Chest]);
            DistanceCast.Add(lst[(int)IKBoneType.Chest] - lst[(int)IKBoneType.Pelvis]);

            DistanceCast.Add(lst[(int)IKBoneType.Chest] - lst[(int)IKBoneType.LeftLowerArm]);
            DistanceCast.Add(lst[(int)IKBoneType.Chest] - lst[(int)IKBoneType.RightLowerArm]);

            DistanceCast.Add(lst[(int)IKBoneType.LeftLowerArm] - lst[(int)IKBoneType.LeftHand]);
            DistanceCast.Add(lst[(int)IKBoneType.RightLowerArm] - lst[(int)IKBoneType.RightHand]);

            DistanceCast.Add(lst[(int)IKBoneType.Pelvis] - lst[(int)IKBoneType.LeftLowerLeg]);
            DistanceCast.Add(lst[(int)IKBoneType.Pelvis] - lst[(int)IKBoneType.RightLowerLeg]);

            DistanceCast.Add(lst[(int)IKBoneType.LeftLowerLeg] - lst[(int)IKBoneType.LeftLeg]);
            DistanceCast.Add(lst[(int)IKBoneType.RightLowerLeg] - lst[(int)IKBoneType.RightLeg]);

        }
        public void SetCharacter(List<Vector3> lst)
        {
            Character.Clear();
            DistanceCharacter.Clear();
            foreach (Vector3 v in lst)
            {
                Character.Add(new Vector3(v.x, v.y, v.z));
            }
            DistanceCharacter.Add(lst[(int)IKBoneType.Head] - lst[(int)IKBoneType.Chest]);
            DistanceCharacter.Add(lst[(int)IKBoneType.Chest] - lst[(int)IKBoneType.Pelvis]);

            DistanceCharacter.Add(lst[(int)IKBoneType.Chest] - lst[(int)IKBoneType.LeftLowerArm]);
            DistanceCharacter.Add(lst[(int)IKBoneType.Chest] - lst[(int)IKBoneType.RightLowerArm]);

            DistanceCharacter.Add(lst[(int)IKBoneType.LeftLowerArm] - lst[(int)IKBoneType.LeftHand]);
            DistanceCharacter.Add(lst[(int)IKBoneType.RightLowerArm] - lst[(int)IKBoneType.RightHand]);

            DistanceCharacter.Add(lst[(int)IKBoneType.Pelvis] - lst[(int)IKBoneType.LeftLowerLeg]);
            DistanceCharacter.Add(lst[(int)IKBoneType.Pelvis] - lst[(int)IKBoneType.RightLowerLeg]);

            DistanceCharacter.Add(lst[(int)IKBoneType.LeftLowerLeg] - lst[(int)IKBoneType.LeftLeg]);
            DistanceCharacter.Add(lst[(int)IKBoneType.RightLowerLeg] - lst[(int)IKBoneType.RightLeg]);

        }
        public void Calculate(IKBoneType pid)
        {
            
        }
    }
}

