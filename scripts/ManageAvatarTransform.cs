using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UniVRM10;
using DG.Tweening;
using RootMotion.FinalIK;

namespace UserHandleSpace
{
    

    /// <summary>
    /// To manage transform of all avatar object
    /// </summary>
    public partial class ManageAvatarTransform : MonoBehaviour
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

        private Animator animator;
        private GameObject TmpPointer;

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
            TmpPointer = GameObject.Find("TmpPointer");

            BaseBik = gameObject.GetComponent<BipedIK>();
            animator = gameObject.GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        public static Vector3 GetTwoPointAngleDirection(Vector3 point1, Vector3 point2, Vector3 axis)
        {
            Vector3 ret = Vector3.zero;
            /*
            if (axis == Vector3.right)
            { //axis is X, rotate Y and Z
                float angle_z2y = Mathf.Atan2(point2.z - point1.z, point2.x - point1.x) * Mathf.Rad2Deg * -1;
                float angle_y2z = Mathf.Atan2(point2.y - point1.y, point2.x - point1.x) * Mathf.Rad2Deg;

                ret.y = angle_z2y;
                ret.z = angle_y2z;
            }
            else if (axis == Vector3.up)
            { //axis is Y, rotate X and Z
                float angle_x2z = Mathf.Atan2(point2.x - point1.x, point2.y - point2.y);
                float angle_z2x = Mathf.Atan2(point2.z - point1.z, point2.y - point2.y);

                ret.x = angle_z2x;
                ret.z = angle_x2z;
            }
            else if (axis == Vector3.forward)
            { //axis is Z, rotate X and Y
                float angle_x2y = Mathf.Atan2(point2.x - point1.x, point2.z - point1.z) * Mathf.Rad2Deg * -1;
                float angle_y2x = Mathf.Atan2(point2.y - point1.y, point2.z - point1.z) * Mathf.Rad2Deg;

                ret.x = angle_y2x;
                ret.y = angle_x2y;
            }
            */
            //ret = Quaternion.LookRotation(point1, point2).eulerAngles;
            ret = Quaternion.LookRotation(point1 - point2).eulerAngles;
            return ret;
        }

        /// <summary>
        /// calculate one object direction from position of two object.
        /// </summary>
        /// <param name="left">1st object position</param>
        /// <param name="right">2nd object position</param>
        /// <param name="keisu"></param>
        /// <returns></returns>
        public static Vector3 CalcFromTwoPointToObjectDirection(Vector3 left, Vector3 right, float keisu)
        {
            Vector3 ret = Vector3.zero;

            Vector3 center = Vector3.Lerp(left, right, keisu);
            Vector3 tmpleft = left - center;
            Vector3 tmpright = right - center;

            Vector3 slerp = Vector3.Slerp(tmpleft, tmpright, keisu);
            slerp += center;

            ret = slerp;

            return ret;
        }
        public static Vector3 TranslateDummyPoint(Transform tmpobj, Vector3 rot, Vector3 left, Vector3 right, Vector3 pelvis)
        {
            Vector3 objcenter = Vector3.Lerp(left,right,0.5f);
            tmpobj.position = new Vector3(objcenter.x, Mathf.Abs(objcenter.y) + pelvis.y, objcenter.z);
            tmpobj.localRotation = Quaternion.Euler(rot);

            return tmpobj.position;
        }
        public static Quaternion TranslateDummyRotate(Vector3 rot)
        {
            return Quaternion.Euler(rot);
        }

        /// <summary>
        /// Decide the aim position from left and right object positions
        /// </summary>
        /// <param name="BasePos"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Vector3 DecideAimPlateDirection(Vector3 BasePos, Vector3 left, Vector3 right)
        {
            Vector3 ret = BasePos;

            Vector3 lrdist = Quaternion.LookRotation(left, right).eulerAngles;
            float zsa = lrdist.normalized.z * 100 / 90f;
            float xsa = lrdist.normalized.x * 100 / 90f;

            if (right.x < left.x)
            { //---front

                if (left.z < right.z)
                { //---left
                    ret = ret + Vector3.left * xsa;
                }
                else if (left.z > right.z)
                { //---right
                    ret = ret + Vector3.right * xsa;
                }
                ret = ret + Vector3.back * zsa;
            }
            else if (right.x > left.x)
            { //---back
                if (left.z < right.z)
                { //---left
                    ret = ret + Vector3.left * xsa;
                }
                else if (left.z > right.z)
                { //---right
                    ret = ret + Vector3.right * xsa;
                }
                ret = ret + Vector3.forward * zsa;
            }
            return ret;
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

        /// <summary>
        /// Calculate an encapsulated bound information
        /// </summary>
        /// <param name="glist"></param>
        /// <returns></returns>
        public Bounds CalculateAllBounds(List<GameObject> glist)
        {
            Bounds maxbounds = new Bounds(Vector3.zero, Vector3.zero);
            foreach (GameObject gobj in glist)
            {
                SkinnedMeshRenderer smr = null;

                if (gobj.TryGetComponent<SkinnedMeshRenderer>(out smr))
                {
                    
                    maxbounds.Encapsulate(smr.bounds);
                }
            }
            return maxbounds;
        }
        public float GetMaximumHeightRenderer(List<GameObject> glist)
        {
            List<float> yarr = new List<float>();

            foreach (GameObject gobj in glist)
            {
                SkinnedMeshRenderer smr = null;

                if (gobj.TryGetComponent<SkinnedMeshRenderer>(out smr))
                {
                    yarr.Add(smr.bounds.max.y);
                }
            }
            
            float ishit = Mathf.Max(yarr.ToArray());

            return ishit;
        }
        public float GetMaximumWidthRenderer(List<GameObject> glist)
        {
            List<float> yarr = new List<float>();

            foreach (GameObject gobj in glist)
            {
                SkinnedMeshRenderer smr = null;

                if (gobj.TryGetComponent<SkinnedMeshRenderer>(out smr))
                {
                    yarr.Add(smr.bounds.max.x);
                }
            }

            float ishit = Mathf.Max(yarr.ToArray());

            return ishit;
        }
        public float GetMaximumDepthRenderer(List<GameObject> glist)
        {
            List<float> yarr = new List<float>();

            foreach (GameObject gobj in glist)
            {
                SkinnedMeshRenderer smr = null;

                if (gobj.TryGetComponent<SkinnedMeshRenderer>(out smr))
                {
                    yarr.Add(smr.bounds.max.z);
                }
            }

            float ishit = Mathf.Max(yarr.ToArray());

            return ishit;
        }
        /// <summary>
        /// Get SkinnedMeshRenderere with BlendShape as Face mainly.
        /// </summary>
        /// <param name="partsname"></param>
        /// <returns></returns>
        public GameObject GetFaceMesh(string partsname = "Face")
        {
            //return _GetMesh(partsname, transform);
            GameObject ret = null;


            List<GameObject> glist = CheckSkinnedMeshAvailable();
            foreach (GameObject gobj in glist)
            {
                SkinnedMeshRenderer smr = null;

                if (gobj.TryGetComponent<SkinnedMeshRenderer>(out smr))
                {
                    if (smr.sharedMesh.blendShapeCount > 0)
                    {
                        ret = gobj;
                        break;
                    }
                }
            }
            if (ret == null)
            {
                //---if GameObject with blendshape is none, default is first SkinedMesheRenderer GameObject.
                ret = glist[0];
            }
            /*
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
            */
            return ret;
        }
        public GameObject GetBodyMesh(string partsname = "Body")
        {
            GameObject ret =  _GetMesh(partsname, transform);

            if (ret == null)
            {
                List<GameObject> glist = CheckSkinnedMeshAvailable();
                foreach (GameObject gobj in glist)
                {
                    SkinnedMeshRenderer smr = null;

                    if (gobj.TryGetComponent<SkinnedMeshRenderer>(out smr))
                    {
                        if (smr.sharedMesh.blendShapeCount == 0)
                        {
                            ret = gobj;
                            break;
                        }
                    }
                }
                /*
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
                */
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

            List<GameObject> glist = CheckSkinnedMeshAvailable();

            //---repair code---
            Bounds maxbounds = CalculateAllBounds(glist);

            ret[0] = maxbounds.size.x; // GetMaximumWidthRenderer(glist) * 2f;
            ret[1] = maxbounds.size.y; //GetMaximumHeightRenderer(glist);
            ret[2] = maxbounds.size.z; //GetMaximumDepthRenderer(glist);

            Debug.Log("max ren=" + ret[0].ToString() + "," +  ret[1].ToString() + "," +  ret[2].ToString());

            /*
            Animator anim = transform.GetComponent<Animator>();
            Vector3 lefthandpos = anim.GetBoneTransform(HumanBodyBones.LeftHand).position;
            Vector3 righthandpos = anim.GetBoneTransform(HumanBodyBones.RightHand).position;
            Vector3 headpos = anim.GetBoneTransform(HumanBodyBones.Head).position;
            Vector3 footpos = anim.GetBoneTransform(HumanBodyBones.LeftFoot).position;
            Debug.Log(lefthandpos);
            Debug.Log(righthandpos);
            Debug.Log(headpos);
            Debug.Log(footpos);
            */

            /*
            //---original code---
            GameObject part = GetBodyMesh();
            if (part != null)
            {
                Vector3 bounds = part.GetComponent<SkinnedMeshRenderer>().bounds.extents;
                ret[0] = bounds.x * 2f;
                ret[1] = bounds.y * 2f;
                ret[2] = bounds.z;

                Debug.Log("body mesh=" + ret[0].ToString() + "," + ret[1].ToString() + "," + ret[2].ToString());

            }
            */


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

        /// <summary>
        /// Save and parse T-pose body infomation.
        /// </summary>
        /// <param name="bnd"></param>
        /// <param name="ikparent"></param>
        /// <returns></returns>
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
                        UserHandleOperation uho = null; // child.GetComponent<UserHandleOperation>();
                        if (child.TryGetComponent<UserHandleOperation>(out uho))
                        {
                            ret.Add(new Vector3(Mathf.Round(uho.defaultPosition.x*1000f)/1000f, Mathf.Round(uho.defaultPosition.y*1000f)/1000f, Mathf.Round(uho.defaultPosition.z*1000f)/1000f));
                            //ret.Add(new Vector3(child.transform.position.x, child.transform.position.y, child.transform.position.z));
                        }
                        
                    }
                }
                
            }

            return ret;
        }
        public List<AvatarSingleIKTransform> GetIKTransformAll_body()
        {
            List<AvatarSingleIKTransform> ret = new List<AvatarSingleIKTransform>();
            OperateLoadedVRM ovrm = gameObject.GetComponent<OperateLoadedVRM>();
            Transform[] bts = ovrm.relatedHandleParent.GetComponentsInChildren<Transform>();

            for (int i = 0; i < IKbones.Length; i++)
            {
                if (i == 0)
                {
                    GameObject ikparent = ovrm.relatedHandleParent;
                    GameObject trueik = ovrm.relatedTrueIKParent;
                    Collider col = trueik.GetComponent<Collider>();
                    Rigidbody rig = trueik.GetComponent<Rigidbody>();

                    //ret.Add(new Vector3(  ikparent.transform.position.x, ikparent.transform.position.y, ikparent.transform.position.z));
                    ret.Add(new AvatarSingleIKTransform(IKbones[i], ikparent.transform.position, ikparent.transform.rotation.eulerAngles, 
                        col.isTrigger ? 0 : 1, rig.useGravity ? 1 : 0, rig.drag, rig.angularDrag
                    ));
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
                        Collider col = child.GetComponent<Collider>();
                        Rigidbody rig = child.GetComponent<Rigidbody>();

                        //ret.Add(new Vector3(  child.transform.localPosition.x, child.transform.localPosition.y, child.transform.localPosition.z ));
                        ret.Add(new AvatarSingleIKTransform(IKbones[i], child.transform.localPosition, child.transform.localRotation.eulerAngles,
                            col.isTrigger ? 0 : 1, rig.useGravity ? 1 : 0, rig.drag, rig.angularDrag
                        ));

                    }
                    else
                    {
                        ret.Add(new AvatarSingleIKTransform(IKbones[i], Vector3.zero, Vector3.zero, 0, 0, 10, 10));
                    }

                }

            }

            return ret;
        }
        public List<AvatarSingleIKTransform> GetIKTransformAll()
        {
            AvatarAllIKParts aai = new AvatarAllIKParts();
            aai.list = GetIKTransformAll_body();
            string js = JsonUtility.ToJson(aai);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
            return aai.list;
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
        public void SetIKTransformAll2(string param)
        {
            AvatarAllIKParts aai = JsonUtility.FromJson<AvatarAllIKParts>(param);
            if (aai != null)
            {
                StartCoroutine(SetIKTransformAll_Body2(aai));
            }
        }
        private IEnumerator SetIKTransformAll_Body(AvatarAllIKParts aai)
        {
            OperateLoadedVRM ovrm = gameObject.GetComponent<OperateLoadedVRM>();
            string[] sortedBones = {
                    "IKParent",
                    "Pelvis", "Aim", "LookAt", "Chest","Head",
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
                            Collider ccol = child.GetComponent<Collider>();
                            Rigidbody crig = child.GetComponent<Rigidbody>();

                            if ((asit.ikname.ToLower() != "leftshoulder") && (asit.ikname.ToLower() != "rightshoulder"))
                            { // both shoulder is not neccessary positioning (rotation only)
                                child.transform.localPosition = asit.position;
                            }
                            
                            if (asit.ikname.ToLower() == "xxxchest")
                            {
                                AvatarSingleIKTransform asit_leftshld = aai.list.Find(match =>
                                {
                                    if (match.ikname.ToLower() == "leftshoulder") return true;
                                    return false;
                                });
                                AvatarSingleIKTransform asit_rightshld = aai.list.Find(match =>
                                {
                                    if (match.ikname.ToLower() == "rightshoulder") return true;
                                    return false;
                                });
                                if ((asit_leftshld != null) && (asit_rightshld != null)) {
                                    Quaternion fnlrot = Quaternion.FromToRotation(asit_leftshld.position, asit_rightshld.position);
                                    child.transform.localRotation = fnlrot;
                                }
                            }
                            else
                            {
                                child.transform.localRotation = Quaternion.Euler(asit.rotation);
                            }
                            //seq.Join(child.transform.DOLocalMove(asit.position, 0.1f));
                            //seq.Join(child.transform.DOLocalRotate(asit.rotation, 0.1f));

                            ccol.isTrigger = asit.useCollision == 1 ? false : true;
                            crig.useGravity = asit.useGravity == 1 ? true : false;
                            crig.drag = asit.drag;
                            crig.angularDrag = asit.anglarDrag;


                            yield return null;
                            
                        }

                    }
                }
            }
            //---IKParent
            GameObject ikparent = ovrm.relatedTrueIKParent;
            Collider col = ikparent.GetComponent<Collider>();
            Rigidbody rig = ikparent.GetComponent<Rigidbody>();

            ikparent.transform.position = aai.list[0].position; // new Vector3(aai.list[i].x, aai.list[i].y, aai.list[i].z);
            ikparent.transform.rotation = Quaternion.Euler(aai.list[0].rotation);
            col.isTrigger = aai.list[0].useCollision == 1 ? false : true;
            rig.useGravity = aai.list[0].useGravity == 1 ? true : false;
            rig.drag = aai.list[0].drag;
            rig.angularDrag = aai.list[0].anglarDrag;
            //seq.Join(ikparent.transform.DOMove(aai.list[0].position, 0.01f));
            //seq.Join(ikparent.transform.DORotate(aai.list[0].rotation, 0.01f));
        }
        private bool SIKTAB2_calc_mindistance(Vector3 body)
        {
            bool ret = false;
            const float MINDISTANCE = 0.001f;

            if (body.x < MINDISTANCE) ret = true;
            if (body.y < MINDISTANCE) ret = true;
            if (body.z < MINDISTANCE) ret = true;

            return ret;
        }

        /// <summary>
        /// Analyze and apply pose data from MediaPipe-format
        /// </summary>
        /// <param name="aai">pose data</param>
        /// <returns></returns>
        private IEnumerator SetIKTransformAll_Body2(AvatarAllIKParts aai)
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
            List<Vector3> tpose = ovrm.GetTPoseBodyList();
            Vector3 t_pelvis = tpose[6];

            //Transform[] bts = ovrm.relatedHandleParent.GetComponentsInChildren<Transform>();
            GameObject tmpobj = new GameObject();

            //---common using
            Vector3 chestcenter = Vector3.Lerp(aai.list[12].position, aai.list[11].position, 0.5f);

            Vector3 lhipplus = aai.list[23].position;
            Vector3 rhipplus = aai.list[24].position;
            Vector3 hipcenter = Vector3.Lerp(rhipplus, lhipplus, 0.5f);

            Vector3 eyerot = GetTwoPointAngleDirection(aai.list[2].position, aai.list[5].position, Vector3.right);
            Vector3 aimrot = GetTwoPointAngleDirection(aai.list[12].position, aai.list[11].position, Vector3.up);
            Vector3 hiprot = GetTwoPointAngleDirection(aai.list[24].position, aai.list[23].position, Vector3.right);
            Vector3 hipXrot = GetTwoPointAngleDirection(hipcenter, chestcenter, Vector3.up);

            //---new code for center of 2-pos
            Vector3 lshoul = new Vector3(aai.list[11].position.x, Mathf.Abs(aai.list[11].position.y), aai.list[11].position.z);
            Vector3 rshoul = new Vector3(aai.list[12].position.x, Mathf.Abs(aai.list[12].position.y), aai.list[12].position.z);

            Vector3 shoulder_rot = GetTwoPointAngleDirection(lshoul, rshoul, Vector3.up);
            Vector3 shouldercenter = Vector3.Lerp(lshoul, rshoul, 0.5f);


            //---Pelvis
            Transform pelvis = ovrm.relatedHandleParent.transform.Find("Pelvis");
            {
                Vector3 tmppos = TranslateDummyPoint(tmpobj.transform, hiprot, aai.list[23].position, aai.list[24].position, t_pelvis);
                //Quaternion tmprot = TranslateDummyRotate(hiprot);
                yield return null;

                //---new code for center of 2-pos
                tmppos = CalcFromTwoPointToObjectDirection(aai.list[23].position, aai.list[24].position, 0.5f);
                tmppos.x = tmppos.x + t_pelvis.x;
                tmppos.y = Mathf.Abs(tmppos.y) + t_pelvis.y;

                
                pelvis.localPosition = tmppos; // new Vector3(tmpobj.transform.position.x, tmpobj.transform.position.y, tmpobj.transform.position.z);
                pelvis.localRotation = Quaternion.Euler(hiprot.x, hiprot.y-180f, 0);
                
                
            }
            yield return null;



            //---Aim
            Transform aim = ovrm.relatedHandleParent.transform.Find("Aim");
            aimrot.z = Mathf.Abs(aimrot.z);
            {
                //Vector3 aimcenter = Vector3.Leap(aai.list[11].position, aai.list[24].position, 0.5f);

                Vector2 aimcenter = Vector3.Lerp(shouldercenter, hipcenter, 0.5f);

                Vector3 aimdir = hipcenter - shouldercenter;
                aimdir.Normalize();

                /*
                Vector3 tmppos = CalcFromTwoPointToObjectDirection(shouldercenter, hipcenter, 0.5f);
                //Debug.Log("aim tmppos 1=" + tmppos.ToString());
                //tmppos.x = (Mathf.Abs(tmppos.x) + t_pelvis.x);

                ////tmppos.z = (Mathf.Abs(tmppos.z) + t_pelvis.z);
                //Debug.Log("aim tmppos 2=" + tmppos.ToString());

                tmppos = DecideAimPlateDirection(shouldercenter, lshoul, rshoul);
                tmppos.y = (Mathf.Abs(tmppos.y) + t_pelvis.y);

                //---肩中心とケツのz位置の関係性から、態勢を決める（前かがみ・のけぞり）
                float sa = Mathf.Abs(hipcenter.z - shouldercenter.z);
                if (rshoul.x < lshoul.x)
                { //---front
                    if (shouldercenter.z < hipcenter.z)
                    { //mae kagami
                        tmppos.y = tmppos.y - sa;
                    }
                    else if (shouldercenter.z > hipcenter.z)
                    { //noke zori
                        tmppos.y = tmppos.y + sa;
                    }
                }
                else if (rshoul.x > lshoul.x)
                { //---back
                    if (shouldercenter.z < hipcenter.z)
                    { //mae kagami
                        tmppos.y = tmppos.y - sa;
                    }
                    else if (shouldercenter.z > hipcenter.z)
                    { //noke zori
                        tmppos.y = tmppos.y + sa;
                    }
                }
                
                

                */
                //Vector3 shoulder_hip_center = Vector3.Lerp(shouldercenter, hipcenter, 0.5f);

                //Quaternion to_tmppos_rot = Quaternion.LookRotation(shoulder_hip_center, tmppos);


                yield return null;
                //tmppos.z += Vector3.back.z;
                //aim.localPosition = tmppos; // new Vector3(tmpobj.transform.position.x, tmpobj.transform.position.y, tmpobj.transform.position.z);
                aim.localPosition = aimcenter;
                aim.forward = aimdir;
                                
            }
            yield return null;

            //---Chest
            Transform chest = ovrm.relatedHandleParent.transform.Find("Chest");
            {
                Vector3 chestpos = new Vector3(chestcenter.x, chestcenter.y, chestcenter.z);
                chestpos.y = t_pelvis.y + Mathf.Abs(chestpos.y);
                //chestcenter.x = chestcenter.x + chest.localPosition.x;
                //Vector3 distance = chest.localPosition - chestcenter;
                Vector3 chestrot = chest.localRotation.eulerAngles;

                Quaternion chestlookat_aim = Quaternion.LookRotation(chestcenter - aim.position);
                

                chest.localPosition = chestpos;
                chest.localRotation = chestlookat_aim; // Quaternion.Euler(shoulder_rot); //chestrot.x, aimrot.y, aimrot.z);


            }
            yield return null;


            //---LookAt
            Transform lookat = ovrm.relatedHandleParent.transform.Find("LookAt");
            {
                //Vector3 tmppos = GetTwoPointAngleDirection(aai.list[2].position, aai.list[5].position, Vector3.right);
                //lookat.localPosition = new Vector3(aai.list[0].position.x, t_pelvis.y + MathF.Abs(aai.list[0].position.y), tpose[3].z);
                //tmpobj.transform.position = lookat.position;
                //tmpobj.transform.localRotation = Quaternion.Euler(tmppos.x, tmppos.y, tmppos.z);

                Vector3 leye = new Vector3(aai.list[2].position.x, Mathf.Abs(aai.list[2].position.y), aai.list[2].position.z);
                Vector3 reye = new Vector3(aai.list[5].position.x, Mathf.Abs(aai.list[5].position.y), aai.list[5].position.z);

                Vector3 eyecenter_lr = Vector3.Lerp(leye, reye, 0.5f);

                Vector3 tmppos = CalcFromTwoPointToObjectDirection(reye, leye, 0.5f);
                //tmppos.x = (Mathf.Abs(tmppos.x) + t_pelvis.x);
                //tmppos.y = (Mathf.Abs(tmppos.y) + t_pelvis.y);
                //tmppos.z = (Mathf.Abs(tmppos.z) + t_pelvis.z);
                tmppos = DecideAimPlateDirection(eyecenter_lr, leye, reye);
                tmppos.y = (Mathf.Abs(tmppos.y) + t_pelvis.y);

                //Quaternion to_tmppos_rot = Quaternion.LookRotation(eyecenter_lr, tmppos);

                //tmpobj.transform.Translate(Vector3.back);
                lookat.localPosition = tmppos; // new Vector3(tmpobj.transform.position.x, tmpobj.transform.position.y, tpose[3].z);
                //lookat.localRotation = to_tmppos_rot;
                yield return null;

                //lookat.localPosition = new Vector3(aim.localPosition.x, aim.localPosition.y, tmppos.z + Vector3.back.z);
            }
            yield return null;

            //---EyeViewHandle
            Transform eye = ovrm.relatedHandleParent.transform.Find("EyeViewHandle");
            {
                //Vector3 tmppos = Vector3.Lerp(aai.list[2].position, aai.list[5].position, 0.5f);
                //tmpobj.transform.position = new Vector3(tmppos.x, tmppos.y + t_pelvis.y, tmppos.z);
                //tmpobj.transform.localRotation = Quaternion.Euler(eyerot);

                Vector3 leye = new Vector3(aai.list[2].position.x, Mathf.Abs(aai.list[2].position.y), aai.list[2].position.z);
                Vector3 reye = new Vector3(aai.list[5].position.x, Mathf.Abs(aai.list[5].position.y), aai.list[5].position.z);

                Vector3 eyecenter_lr = Vector3.Lerp(leye, reye, 0.5f);

                Vector3 tmppos = CalcFromTwoPointToObjectDirection(reye, leye, 0.5f);
                //tmppos.x = (Mathf.Abs(tmppos.x) + t_pelvis.x);
                //tmppos.y = (Mathf.Abs(tmppos.y) + t_pelvis.y);
                //tmppos.z = (Mathf.Abs(tmppos.z) + t_pelvis.z);
                tmppos = DecideAimPlateDirection(eyecenter_lr, leye, reye);
                tmppos.y = (Mathf.Abs(tmppos.y) + t_pelvis.y);

                //Quaternion to_tmppos_rot = Quaternion.LookRotation(eyecenter_lr, tmppos);
                

                //tmpobj.transform.Translate(Vector3.back);
                eye.localPosition = tmppos; // new Vector3(tmppos.x, tmppos.y, tpose[1].z);
                //lookat.localRotation = to_tmppos_rot;
                yield return null;

                //lookat.localPosition = new Vector3(aim.localPosition.x, aim.localPosition.y, tmppos.z + Vector3.back.z);
            }
            yield return null;

            //---Head
            Transform head = ovrm.relatedHandleParent.transform.Find("Head");
            {
                head.localPosition = new Vector3(aai.list[0].position.x, t_pelvis.y + Mathf.Abs(aai.list[0].position.y), aai.list[0].position.z);
                //head.localRotation = Quaternion.Euler(eyerot.x, head.localRotation.eulerAngles.y, head.localRotation.eulerAngles.z);
            }
            yield return null;


            //---left hand
            Transform lefthand = ovrm.relatedHandleParent.transform.Find("LeftHand");
            {
                //te no hira
                Vector3 handrot = GetTwoPointAngleDirection(aai.list[21].position, aai.list[17].position, Vector3.up);
                //te kara ude
                Vector3 handbrarot = GetTwoPointAngleDirection(aai.list[19].position, aai.list[15].position, Vector3.right);

                Vector3 lh = aai.list[15].position;
                lh.y = Mathf.Abs(lh.y);

                lefthand.localPosition = lh + t_pelvis; // new Vector3(aai.list[15].position.x, Mathf.Abs(aai.list[15].position.y) + t_pelvis.y, aai.list[15].position.z);
                lefthand.localRotation = Quaternion.Euler(handrot.x, lefthand.localRotation.eulerAngles.y, handbrarot.z+180f); //aai.list[15].rotation.x, aai.list[15].rotation.y + 180f, aai.list[15].rotation.z);
            }
            yield return null;

            //---right hand
            Transform righthand = ovrm.relatedHandleParent.transform.Find("RightHand");
            {
                //te no hira
                Vector3 handrot = GetTwoPointAngleDirection(aai.list[22].position, aai.list[18].position, Vector3.up);
                //te kara ude
                Vector3 handbrarot = GetTwoPointAngleDirection(aai.list[20].position, aai.list[16].position, Vector3.right);

                Vector3 rh = aai.list[16].position;
                rh.y = Mathf.Abs(rh.y);

                righthand.localPosition = rh + t_pelvis; //  new Vector3(aai.list[16].position.x+t_pelvis.x, aai.list[16].position.y + t_pelvis.y, aai.list[16].position.z);
                righthand.localRotation = Quaternion.Euler(handrot.x, righthand.localRotation.eulerAngles.y, handbrarot.z+180f); //aai.list[16].rotation.x, aai.list[16].rotation.y + 180f, aai.list[16].rotation.z);
            }
            yield return null;

            //---left lower arm
            Transform leftlowerarm = ovrm.relatedHandleParent.transform.Find("LeftLowerArm");
            {
                Vector3 lh = aai.list[13].position;
                lh.y = Mathf.Abs(lh.y);

                leftlowerarm.localPosition = lh + t_pelvis; //new Vector3(aai.list[13].position.x, Mathf.Abs(aai.list[13].position.y) + t_pelvis.y, aai.list[13].position.z);
            }
            yield return null;

            //---right lower arm
            Transform rightlowerarm = ovrm.relatedHandleParent.transform.Find("RightLowerArm");
            {
                Vector3 rh = aai.list[14].position;
                rh.y = Mathf.Abs(rh.y);

                rightlowerarm.localPosition = rh + t_pelvis; //new Vector3(aai.list[14].position.x, Mathf.Abs(aai.list[14].position.y) + t_pelvis.y, aai.list[14].position.z);
            }
            yield return null;


            //---left lower leg
            Transform leftlowerleg = ovrm.relatedHandleParent.transform.Find("LeftLowerLeg");
            {
                Vector3 lll = t_pelvis - aai.list[25].position;
                lll.x = aai.list[25].position.x;
                lll.z *= -1f;
                leftlowerleg.localPosition = lll; //new Vector3(aai.list[25].position.x, t_pelvis.y - Mathf.Abs(aai.list[25].position.y), aai.list[25].position.z);
            }
            yield return null;

            //---right lower leg
            Transform rightlowerleg = ovrm.relatedHandleParent.transform.Find("RightLowerLeg");
            {
                Vector3 rll = t_pelvis - aai.list[26].position;
                rll.x = aai.list[26].position.x;
                rll.z *= -1f;
                rightlowerleg.localPosition = rll; //new Vector3(aai.list[26].position.x, t_pelvis.y - Mathf.Abs(aai.list[26].position.y), aai.list[26].position.z);
            }
            yield return null;

            //---left foot
            Transform leftfoot = ovrm.relatedHandleParent.transform.Find("LeftLeg");
            //Vector3 leftfootrot = GetTwoPointAngleDirection(aai.list[29].position, aai.list[31].position, Vector3.forward);
            {
                Vector3 indexheel = Vector3.Lerp(aai.list[31].position, aai.list[29].position, 0.5f);
                Vector3 rot_ankle = GetTwoPointAngleDirection(aai.list[27].position, indexheel, Vector3.up);
                Vector3 rot_indexheel = GetTwoPointAngleDirection(aai.list[31].position, aai.list[29].position, Vector3.up);
                Vector3 leftfoot_rot = leftfoot.localRotation.eulerAngles;

                Vector3 fnllocpos = t_pelvis - aai.list[27].position;
                fnllocpos.z *= -1f;
                fnllocpos.x *= -1f;
                leftfoot.localPosition = fnllocpos; // new Vector3(aai.list[27].position.x, t_pelvis.y - Mathf.Abs(aai.list[27].position.y), aai.list[27].position.z);
                leftfoot.localRotation = Quaternion.Euler(leftfoot_rot.x, rot_indexheel.y, leftfoot_rot.z); //leftfootrot.x, leftfootrot.y, leftfootrot.z+180f);
            }
            yield return null;

            //---right foot
            Transform rightfoot = ovrm.relatedHandleParent.transform.Find("RightLeg");
            //Vector3 rightfootrot = GetTwoPointAngleDirection(aai.list[30].position, aai.list[32].position, Vector3.forward);
            {
                Vector3 indexheel = Vector3.Lerp(aai.list[32].position, aai.list[30].position, 0.5f);
                Vector3 rot_ankle = GetTwoPointAngleDirection(aai.list[28].position, indexheel, Vector3.up);
                Vector3 rot_indexheel = GetTwoPointAngleDirection(aai.list[32].position, aai.list[30].position, Vector3.up);
                Vector3 rightfoot_rot = rightfoot.localRotation.eulerAngles;

                Vector3 fnllocpos = t_pelvis - aai.list[28].position;
                fnllocpos.z *= -1f;
                fnllocpos.x *= -1f;
                rightfoot.localPosition = fnllocpos; // new Vector3(aai.list[28].position.x, t_pelvis.y - Mathf.Abs(aai.list[28].position.y), aai.list[28].position.z);
                rightfoot.localRotation = Quaternion.Euler(rightfoot_rot.x, rot_indexheel.y, rightfoot_rot.z); //rightfootrot.x, rightfootrot.y, rightfootrot.z+180f);
            }
            yield return null;


            //---IKParent
            GameObject ikparent = ovrm.relatedTrueIKParent;
            ikparent.transform.position = tpose[0];
            //ikparent.transform.rotation = Quaternion.Euler(aai.list[0].rotation);
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
                parts = ovrm.relatedTrueIKParent;
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

        /// <summary>
        /// flip left-right posing
        /// </summary>
        public void MirrorPose()
        {
            AvatarSingleIKTransform FindParts(List<AvatarSingleIKTransform> list, string name)
            {
                return list.Find(match =>
                {
                    if (match.ikname == name) return true;
                    return false;
                });
            }
            
            AvatarSingleIKTransform mirrorLR(AvatarSingleIKTransform tleft, AvatarSingleIKTransform tright)
            {
                AvatarSingleIKTransform asik = new AvatarSingleIKTransform(tleft.ikname, tleft.position, tleft.rotation, tleft.useCollision, tleft.useGravity, tleft.drag, tleft.anglarDrag);

                asik.position.x = tright.position.x * -1;
                asik.position.y = tright.position.y;
                asik.position.z = tright.position.z;
                asik.rotation.x = tright.rotation.x;
                asik.rotation.y = tright.rotation.y * -1;
                asik.rotation.z = tright.rotation.z * -1;

                return asik;
            }

            AvatarAllIKParts aai = new AvatarAllIKParts();
            List<AvatarSingleIKTransform> list = GetIKTransformAll();
            aai.list.Add(list[0]);
            
            

            for (int i = 1; i < IKbones.Length; i++)
            {
                AvatarSingleIKTransform hit = FindParts(list, IKbones[i]);
                if ((i >= 1) && (i <= 6))
                { 
                    aai.list.Add(mirrorLR(hit, hit));
                }
                //---arm ~ hand
                else if (i == (int)IKBoneType.LeftShoulder)
                {
                    aai.list.Add(mirrorLR(hit, FindParts(list, IKbones[(int)IKBoneType.RightShoulder])));
                }
                else if (i == (int)IKBoneType.LeftLowerArm)
                {
                    aai.list.Add(mirrorLR(hit, FindParts(list, IKbones[(int)IKBoneType.RightLowerArm])));
                }
                else if (i == (int)IKBoneType.LeftHand)
                {
                    aai.list.Add(mirrorLR(hit, FindParts(list, IKbones[(int)IKBoneType.RightHand])));
                }
                else if (i == (int)IKBoneType.RightShoulder)
                {
                    aai.list.Add(mirrorLR(hit, FindParts(list, IKbones[(int)IKBoneType.LeftShoulder])));
                }
                else if (i == (int)IKBoneType.RightLowerArm)
                {
                    aai.list.Add(mirrorLR(hit, FindParts(list, IKbones[(int)IKBoneType.LeftLowerArm])));
                }
                else if (i == (int)IKBoneType.RightHand)
                {
                    aai.list.Add(mirrorLR(hit, FindParts(list, IKbones[(int)IKBoneType.LeftHand])));
                }
                //---leg ~ foot
                else if (i == (int)IKBoneType.LeftLowerLeg)
                {
                    aai.list.Add(mirrorLR(hit, FindParts(list, IKbones[(int)IKBoneType.RightLowerLeg])));
                }
                else if (i == (int)IKBoneType.LeftLeg)
                {
                    aai.list.Add(mirrorLR(hit, FindParts(list, IKbones[(int)IKBoneType.RightLeg])));
                }
                else if (i == (int)IKBoneType.RightLowerLeg)
                {
                    aai.list.Add(mirrorLR(hit, FindParts(list, IKbones[(int)IKBoneType.LeftLowerLeg])));
                }
                else if (i == (int)IKBoneType.RightLeg)
                {
                    aai.list.Add(mirrorLR(hit, FindParts(list, IKbones[(int)IKBoneType.LeftLeg])));
                }

            }
            foreach (AvatarSingleIKTransform sik in aai.list)
            {
                int ishit = -1;
                for (int i = 0; i < IKbones.Length; i++)
                {
                    if (sik.ikname == IKbones[i])
                    {
                        ishit = i;
                        break;
                    }
                }
                if (ishit > -1)
                {

                }
                
            }
            StartCoroutine(SetIKTransformAll_Body(aai));

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
            atran.version = ManageAnimation.PROJECT_VERSION;

            if (param.ToLower() == "vrm")
            {
                
                atran.sampleavatar = transform.gameObject.GetComponent<Vrm10Instance>().Vrm.Meta.Name;

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

            List<int> regBones = new List<int>();
            for (int i = (int)ParseIKBoneType.EyeViewHandle; i < (int)ParseIKBoneType.LeftHandPose; i++)
            {
                aro.registerBoneTypes.Add(i);
            }
            aro.registerMoveTypes.Add((int)AF_MOVETYPE.Translate);
            aro.registerMoveTypes.Add((int)AF_MOVETYPE.NormalTransform);
            aro.registerMoveTypes.Add((int)AF_MOVETYPE.AllProperties);
            NativeAnimationFrameActor savedActor = new NativeAnimationFrameActor();
            NativeAnimationFrameActor actor = maa.GetFrameActorFromObjectID(gameObject.name, AF_TARGETTYPE.VRM);
            savedActor = actor.SCopy();
            Array.Copy(actor.bodyHeight, savedActor.bodyHeight, actor.bodyHeight.Length);

            NativeAnimationFrame fr = maa.SaveFrameData(1, null, -1, actor, aro);
            //savedActor.frames.Add(fr);

            AnimationFrameActor afactor = new AnimationFrameActor();
            afactor.SetFromNative(savedActor);

            
            AnimationFrame afg = new AnimationFrame();
            afg.duration = fr.duration;
            afg.finalizeIndex = fr.finalizeIndex;
            afg.index = fr.index;
            afg.key = fr.key;

            //---translate to csv
            foreach (AnimationTranslateTargetParts tmv in fr.translateMovingData)
            {
                foreach (Vector3 vv in tmv.values)
                {
                    afg.movingData.Add(maa.TranslateDataToCSV(afactor.targetType, tmv, vv));
                }

            }
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
            { //---if not found, add newly
                afactor.frames.Add(afg);
            }
            else
            { //---overwrite
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
            for (int i = 0; i < atran.frameData.bodyInfoList.Count; i++)
            {
                Vector3 fi = atran.frameData.bodyInfoList[i];

                Debug.Log(IKbones[i] + "=" + fi.x.ToString() + " - " + fi.y.ToString() + " - " + fi.z.ToString());
            }

            //---open new code
            AnimationParsingOptions aro = new AnimationParsingOptions();
            aro.index = 1;
            aro.finalizeIndex = 1;
            aro.endIndex = 1;
            aro.isBuildDoTween = 0;
            aro.isExecuteForDOTween = 1;
            aro.targetType = AF_TARGETTYPE.VRM;
            
            //---extract frame data from text file
            NativeAnimationFrameActor nact = new NativeAnimationFrameActor();
            nact.SetFromRaw(atran.frameData);
            //---get cast to action
            NativeAnimationAvatar nav = maa.GetCastByAvatar(gameObject.name); 
            /*maa.currentProject.casts.Find(match =>
            {
                if (match.avatarId == gameObject.name) return true;
                return false;
            });*/
            //---apply to frame actor
            nact.avatar = nav;

            //---construct frame data
            foreach (AnimationFrame fr in atran.frameData.frames)
            {
                NativeAnimationFrame nframe = maa.ParseEffectiveFrame(nact, fr, atran.version);
                nact.frames.Add(nframe);
            }
            for (int di = 0; di < nact.bodyHeight.Length; di++)
            {
                Debug.Log("nact=" + nact.bodyHeight[di].ToString());
            }
            //---calculate difference of the height
            maa.CalculateAllFrameForCurrent(nav, nact);
            Array.Copy(nav.bodyHeight, nact.bodyHeight, nav.bodyHeight.Length);

            Debug.Log("mat calculate end!");
            aro.targetRole = nact.targetRole;

            //---at this time, cast : frame actor = 1 : 1
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

