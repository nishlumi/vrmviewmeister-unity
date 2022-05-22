using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UserHandleSpace;
using RootMotion.FinalIK;


namespace UserHandleSpace
{
    /// <summary>
    /// Attach for IKHandleParent(parent of the ik handle game object)
    /// </summary>
    public class OperateLoadedObj : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);


        public bool enablePositionHandle = true;
        public bool enableRotationHandle = true;
        public bool enableScaleHandle = true;

        [SerializeField]
        private ManageAnimation manim;

        private Vector3 ikmarker_size;

        private ConfigSettingLabs conf;

        // Start is called before the first frame update
        void Start()
        {
            manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            conf = GameObject.Find("Canvas").GetComponent<ConfigSettingLabs>();
            ikmarker_size = new Vector3(0.1f, 0.1f, 0.1f);
        }

        // Update is called once per frame
        void Update()
        {
            if (false)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit = new RaycastHit();
                    if (Physics.Raycast(ray, out hit))
                    {
                        Debug.Log("collider hit name=" + hit.collider.gameObject.name + ", " + hit.collider.gameObject.tag);
                        ChangeColliderState(hit.collider.gameObject);

                    }
                }
            }
            
        }
        void LastUpdate()
        {

        }
        public void ChangeColliderState(GameObject hitObject)
        {
            if (hitObject.tag == "Ground")
            {
                /*GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
                OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
                ovrm.ActivateHandleView(false);

                if (ovrm.ActiveAvatar) {
                    CapsuleCollider col = ovrm.ActiveAvatar.GetComponent<CapsuleCollider>();
                    col.enabled = true;
                }*/
                //ChangeAllOtherObjectHandleState(false);

            }
            else if (hitObject.tag == "IKHandle")
            {
                //ChangeAllOtherObjectHandleState(false);
                /*if ((hitObject.name == "Head") ||
                    (hitObject.name == "LookAt") ||
                    (hitObject.name == "Aim") ||
                    (hitObject.name == "LeftLowerLeg") ||
                    (hitObject.name == "RightLowerLeg") ||
                    (hitObject.name == "EyeViewHandle")
                )
                {
                    Camera.main.GetComponent<CameraOperation1>().SetGizmoTranslation();
                }
                else
                {
                    Camera.main.GetComponent<CameraOperation1>().SetGizmoAllType();
                }*/
                /*
                UserHandleOperation uho;
                OtherObjectDummyIK ooik;
                GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
                OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

                //---Set related avatar object as the active object
                if (hitObject.TryGetComponent<UserHandleOperation>(out uho))
                {
                    ovrm.ActiveAvatar = uho.relatedAvatar;
                    OperateLoadedVRM hitolvrm = uho.relatedAvatar.GetComponent<OperateLoadedVRM>();
                    ovrm.ActiveIKHandle = hitolvrm.relatedHandleParent;
                    ovrm.SetActiveFace();
                    ovrm.ActiveType = "vrm";
                }
                if (hitObject.TryGetComponent<OtherObjectDummyIK>(out ooik)) 
                {
                    int childcnt = ooik.relatedAvatar.transform.childCount;
                    for (int i = 0; i < childcnt; i++)
                    {
                        GameObject child = ooik.relatedAvatar.transform.GetChild(i).gameObject;
                        BoxCollider boc;
                        if (child.TryGetComponent<BoxCollider>(out boc))
                        {
                            OperateLoadedOther hitoloth = ooik.relatedAvatar.GetComponent<OperateLoadedOther>();

                            ovrm.ActiveAvatar = child; //.transform.parent.gameObject;
                            ovrm.ActiveIKHandle = hitoloth.relatedHandleParent;
                            ovrm.ActiveFace = null;
                            //ChangeTargetOtherObjectHandleState(hitObject, 1);
                            //ActivateAvatar(hitoloth.objectType);
                            ovrm.ActiveType = "otherobject";
                        }
                    }
                }
                */
            }
            else if ((hitObject.tag == "Player") || (hitObject.tag == "SampleData") || (hitObject.tag == "OtherPlayerCollider")
                || (hitObject.tag == "OtherPlayer")
                || (hitObject.tag == "LightPlayer")
                || (hitObject.tag == "CameraPlayer")
                || (hitObject.tag == "EffectDestination")
                )
            {
                //ChangeAllOtherObjectHandleState(false);
                //---equipping item
                OtherObjectDummyIK ooik;
                if (hitObject.tag != "Player")
                {
                    if (hitObject.tag == "OtherPlayerCollider")
                    {
                        //---collider child object to parent object
                        GameObject ikhandle = hitObject.transform.parent.GetComponent<OperateLoadedOther>().relatedHandleParent;
                        if (ikhandle.TryGetComponent<OtherObjectDummyIK>(out ooik))
                        {
                            if (ooik.isEquipping)
                            {
                                return;
                            }
                        }
                    }
                    else //if (hitObject.tag == "OtherPlayer")
                    {
                        //---collider child object to parent object
                        GameObject ikhandle = hitObject.transform.GetComponent<OperateLoadedBase>().relatedHandleParent;
                        if (ikhandle.TryGetComponent<OtherObjectDummyIK>(out ooik))
                        {
                            if (ooik.isEquipping)
                            {
                                return;
                            }
                        }
                    }
                }


                GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
                OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

                if (hitObject == ovrm.ActiveAvatar)
                {

                }
                else
                {
                    //---To activate different VRM/OtherObject

                    ovrm._ActivateAvatarBody(hitObject);

                }

                //---test: change position of ViewPointObj
                GameObject vpo = GameObject.Find("ViewPointObj");
                vpo.transform.position = ovrm.ActiveAvatar.transform.position;


                GameObject.Find("Canvas").GetComponent<OperateAvatarIK>().SetInpAvatarScale(ovrm.ActiveAvatar.transform.localScale.x);
            }
        }
        public Vector3 GetIKMarkerStyle()
        {
            Vector3 ret = Vector3.zero;

            ret = ikmarker_size;

            string js = JsonUtility.ToJson(ret);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif

            return ret;
        }

        /// <summary>
        /// Change scale of all IK-markers
        /// (*) the scale is commonly for all IK-markers.
        /// </summary>
        /// <param name="param"></param>
        public void ChangeIKMarkerStyle(float param)
        {

            GameObject[] objects = GameObject.FindGameObjectsWithTag("IKHandle");
            int childCount = objects.Length;

            float commonSize = param < 0f ? 0.01f : param; // float.TryParse(param, out commonSize) ? commonSize : 0.1f;
            if (commonSize > 1f) commonSize = 1f;
            ikmarker_size = new Vector3(commonSize, commonSize, commonSize);

            for (var i = 0; i < childCount; i++)
            {
                GameObject child = objects[i]; // gameObject.transform.GetChild(i).gameObject;

                child.transform.localScale = ikmarker_size;

                /*
                UserGroundOperation ugo;
                OtherObjectDummyIK ooik;

                if (child.name != "DLightIK")
                {
                    if (child.TryGetComponent(out ugo))
                    { //---For VRM
                      //Not change IKParent of VRM.

                        int bodyChildCount = child.transform.childCount;
                        for (var b = 0; b < bodyChildCount; b++)
                        {
                            GameObject bodyChild = child.transform.GetChild(b).gameObject;
                            bodyChild.transform.localScale = new Vector3(commonSize, commonSize, commonSize);
                        }

                    }
                    else if (child.TryGetComponent(out ooik))
                    { //---For OtherObject, Camera, Light, Effect, Image
                        child.transform.localScale = new Vector3(commonSize, commonSize, commonSize);
                    }
                }
                */
            }
        }
        /*
            /// <summary>
            /// Duplicate check for loaded object
            /// </summary>
            /// <param name="target"></param>
            /// <param name="objtype">Player, OtherPlayer, etc...</param>
            /// <returns></returns>
            public bool CheckLoadObjectExist(GameObject target, string objtype)
            {
                bool ret = false;

                GameObject[] objs = GameObject.FindGameObjectsWithTag(objtype);
                foreach (GameObject oo in objs)
                {
                    if (oo.name == target.name)
                    {
                        ret = true;
                    }
                }

                return ret;
            }

            /// <summary>
            /// Duplicate check and to return rename object name
            /// </summary>
            /// <param name="target"></param>
            /// <param name="objtype">Player, OtherPlayer, etc...</param>
            /// <returns></returns>
            public string AsistCheckLoadObjectExist(GameObject target, string objtype)
            {
                string ret = "";

                GameObject[] objs = GameObject.FindGameObjectsWithTag(objtype);
                foreach (GameObject oo in objs)
                {
                    if (oo.name == target.name)
                    {
                        ret = oo.name + " (1)";
                    }
                }

                return ret;
            }
            private void ChangeAllOtherObjectHandleState(bool flag)
            {
                GameObject[] objects = GameObject.FindGameObjectsWithTag("OtherPlayer");
                for (int i = 0; i < objects.Length; i++)
                {
                    Debug.Log(objects[i].GetComponent<OperateLoadedOther>().relatedHandleParent);
                }
            }

            public void ChangeTargetOtherObjectHandleState(GameObject hitObject, int flag)
            {
                GameObject parentobj = hitObject.transform.parent.gameObject;
                if (enableRotationHandle)
                {
                    parentobj.GetComponent<RotationHandle>().enabled = flag == 1 ? true : false;
                }
                else
                {
                    parentobj.GetComponent<RotationHandle>().enabled = false;
                }
                if (enablePositionHandle)
                {
                    parentobj.GetComponent<PositionHandle>().enabled = flag == 1 ? true : false;
                }
                else
                {
                    parentobj.GetComponent<PositionHandle>().enabled = false;
                }
                if (enableScaleHandle)
                {
                    parentobj.GetComponent<ScaleHandle>().enabled = flag == 1 ? true : false;
                }
                else
                {
                    parentobj.GetComponent<ScaleHandle>().enabled = false;
                }
            }

            public void ShowAllOtherObjectHandle(int isrotate, int isposition, int isscale)
            {
                enableRotationHandle = isrotate == 1 ? true : false;
                enablePositionHandle = isposition == 1 ? true : false;
                enableScaleHandle = isscale == 1 ? true : false;
            }
            public void ShowAllOtherObjectHandleFromOuter(string param)
            {
                string[] prm = param.Split(',');
                if (prm[0] == "1") enablePositionHandle = true;
                else enablePositionHandle = false;

                if (prm[1] == "1") enableRotationHandle = true;
                else enableRotationHandle = false;

                if (prm[1] == "2") enableScaleHandle = true;
                else enableScaleHandle = false;
            }*/

        public GameObject CreateIKHandle(GameObject avatar)
        {
            GameObject ikhp = manim.ikArea; //GameObject.FindGameObjectWithTag("IKHandleWorld");
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
            OperateLoadedVRM avatar_olvrm = avatar.GetComponent<OperateLoadedVRM>();
            Animator animator = avatar.GetComponent<Animator>();
            GameObject ikparent = Instantiate((GameObject)Resources.Load("IKParentBase"));
            ikparent.name = "ikparent_" + avatar.name;
            ikparent.tag = "IKHandleParent";
            ikparent.layer = LayerMask.NameToLayer("Handle");
            //ikparent.AddComponent<PositionHandle>().enabled = false;
            //ikparent.AddComponent<RotationHandle>().enabled = false;
            ikparent.transform.SetParent(ikhp.transform);

            if (!ovrm.isMoveMode)
            {
                ovrm.ShowHandleBody(false, ikparent);
            }

            avatar_olvrm.relatedHandleParent = ikparent;

            GameObject copycamera = (GameObject)Resources.Load("EyeViewHandleSphere");
            GameObject camera = Instantiate(copycamera, copycamera.transform.position, Quaternion.identity, ikparent.transform);
            camera.name = "EyeViewHandle"; //+ avatar.name;
            Vector3 pos = animator.GetBoneTransform(HumanBodyBones.Head).transform.position;
            pos.Set(pos.x, pos.y, pos.z - 0.05f);
            camera.transform.position = new Vector3(pos.x, pos.y, pos.z - 1.2f);
            camera.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            //camera.transform.SetParent(ikparent.transform);


            GameObject copyhead = (GameObject)Resources.Load("IKHandleSphereHead");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject head = Instantiate(copyhead, copyhead.transform.position, Quaternion.identity, ikparent.transform);
            head.name = "Head";// + avatar.name;
            head.transform.rotation = new Quaternion(0f, 180f, 0f, 0f);
            head.tag = "IKHandle";
            head.GetComponent<UserHandleOperation>().PartsName = "head";
            head.transform.position = animator.GetBoneTransform(HumanBodyBones.Head).transform.position;
            //head.transform.SetParent(ikparent.transform);


            GameObject copychest = (GameObject)Resources.Load("IKHandleSphereChest");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject chest = Instantiate(copychest, copychest.transform.position, Quaternion.identity, ikparent.transform);
            chest.name = "Chest";// + avatar.name;
            chest.tag = "IKHandle";
            chest.GetComponent<UserHandleOperation>().PartsName = "chest";
            chest.transform.position = animator.GetBoneTransform(HumanBodyBones.Chest).transform.position;
            chest.transform.rotation = animator.GetBoneTransform(HumanBodyBones.Chest).transform.rotation;
            //chest.transform.SetParent(ikparent.transform);

            GameObject copypelvis = (GameObject)Resources.Load("IKHandleSpherePelvis");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject pelvis = Instantiate(copypelvis, copypelvis.transform.position, Quaternion.identity, ikparent.transform);
            pelvis.name = "Pelvis";// + avatar.name;
            Vector3 vrot = new Vector3(0f, 180f, 0f);
            Quaternion rot = Quaternion.Euler(vrot);
            pelvis.transform.rotation = rot;
            pelvis.tag = "IKHandle";
            pelvis.GetComponent<UserHandleOperation>().PartsName = "pelvis";
            pelvis.transform.position = animator.GetBoneTransform(HumanBodyBones.Spine).transform.position;
            //pelvis.transform.SetParent(ikparent.transform);

            //---Hand, Arm
            GameObject copyleftlowerarm = (GameObject)Resources.Load("IKHandleSphere");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject leftlowerarm = Instantiate(copyleftlowerarm, copyleftlowerarm.transform.position, Quaternion.identity, ikparent.transform);
            leftlowerarm.name = "LeftLowerArm";// + avatar.name;
            leftlowerarm.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            leftlowerarm.tag = "IKHandle";
            leftlowerarm.GetComponent<UserHandleOperation>().PartsName = "leftlowerarm";
            leftlowerarm.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).transform.position;

            GameObject copyrightlowerarm = (GameObject)Resources.Load("IKHandleSphere");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject rightlowerarm = Instantiate(copyrightlowerarm, copyrightlowerarm.transform.position, Quaternion.identity, ikparent.transform);
            rightlowerarm.name = "RightLowerArm";// + avatar.name;
            rightlowerarm.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rightlowerarm.tag = "IKHandle";
            rightlowerarm.GetComponent<UserHandleOperation>().PartsName = "rightlowerarm";
            rightlowerarm.transform.position = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).transform.position;

            GameObject copylefthand = (GameObject)Resources.Load("IKHandleSphere");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject lefthand = Instantiate(copylefthand, copylefthand.transform.position, Quaternion.identity, ikparent.transform);
            lefthand.name = "LeftHand";// + avatar.name;
            lefthand.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            lefthand.tag = "IKHandle";
            lefthand.GetComponent<UserHandleOperation>().PartsName = "leftarm";
            lefthand.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftHand).transform.position;
            //lefthand.transform.SetParent(ikparent.transform);

            GameObject copyrighthand = (GameObject)Resources.Load("IKHandleSphere");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject righthand = Instantiate(copyrighthand, copyrighthand.transform.position, Quaternion.identity, ikparent.transform);
            righthand.name = "RightHand";// + avatar.name;
            righthand.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            righthand.tag = "IKHandle";
            righthand.GetComponent<UserHandleOperation>().PartsName = "rightarm";
            righthand.transform.position = animator.GetBoneTransform(HumanBodyBones.RightHand).transform.position;
            //righthand.transform.SetParent(ikparent.transform);

            //---Leg
            GameObject copyleftlowerleg = (GameObject)Resources.Load("IKHandleSphere");
            GameObject leftlowerleg = Instantiate(copyleftlowerleg, copyleftlowerleg.transform.position, Quaternion.identity, ikparent.transform);
            leftlowerleg.name = "LeftLowerLeg"; // + avatar.name;
            leftlowerleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            leftlowerleg.tag = "IKHandle";
            leftlowerleg.GetComponent<UserHandleOperation>().PartsName = "leftlowerleg";
            leftlowerleg.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).transform.position;

            GameObject copyrightlowerleg = (GameObject)Resources.Load("IKHandleSphere");
            GameObject rightlowerleg = Instantiate(copyrightlowerleg, copyrightlowerleg.transform.position, Quaternion.identity, ikparent.transform);
            rightlowerleg.name = "RightLowerLeg"; // + avatar.name;
            rightlowerleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rightlowerleg.tag = "IKHandle";
            rightlowerleg.GetComponent<UserHandleOperation>().PartsName = "rightlowerleg";
            rightlowerleg.transform.position = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).transform.position;

            GameObject copyleftleg = (GameObject)Resources.Load("IKHandleSphere");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject leftleg = Instantiate(copyleftleg, copyleftleg.transform.position, Quaternion.identity, ikparent.transform);
            leftleg.name = "LeftLeg"; // + avatar.name;
            leftleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            leftleg.tag = "IKHandle";
            leftleg.GetComponent<UserHandleOperation>().PartsName = "leftleg";
            leftleg.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftFoot).transform.position;
            //leftleg.transform.SetParent(ikparent.transform);

            GameObject copyrightleg = (GameObject)Resources.Load("IKHandleSphere");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject rightleg = Instantiate(copyrightleg, copyrightleg.transform.position, Quaternion.identity, ikparent.transform);
            rightleg.name = "RightLeg";// + avatar.name;
            rightleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rightleg.tag = "IKHandle";
            rightleg.GetComponent<UserHandleOperation>().PartsName = "rightleg";
            rightleg.transform.position = animator.GetBoneTransform(HumanBodyBones.RightFoot).transform.position;
            //rightleg.transform.SetParent(ikparent.transform);

            return ikparent;
        }
        public GameObject CreateFullBodyIKHandle(GameObject avatar)
        {
            GameObject ikhp = manim.ikArea; //GameObject.FindGameObjectWithTag("IKHandleWorld");
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
            OperateLoadedVRM avatar_olvrm = avatar.GetComponent<OperateLoadedVRM>();
            Animator animator = avatar.GetComponent<Animator>();
            GameObject ikparent = Instantiate((GameObject)Resources.Load("IKParentBase"));
            ikparent.name = "ikparent_" + avatar.name;
            ikparent.tag = "IKHandleParent";
            ikparent.layer = LayerMask.NameToLayer("Handle");
            //ikparent.AddComponent<PositionHandle>().enabled = false;
            //ikparent.AddComponent<RotationHandle>().enabled = false;
            ikparent.transform.SetParent(ikhp.transform);

            if (!ovrm.isMoveMode)
            {
                ovrm.ShowHandleBody(false, ikparent);
            }

            avatar_olvrm.relatedHandleParent = ikparent;

            GameObject copycamera = (GameObject)Resources.Load("EyeViewHandleSphere");
            GameObject camera = Instantiate(copycamera, copycamera.transform.position, Quaternion.identity, ikparent.transform);
            camera.name = "EyeViewHandle"; //+ avatar.name;
            Vector3 pos = animator.GetBoneTransform(HumanBodyBones.Head).transform.position;
            pos.Set(pos.x, pos.y, pos.z - 0.05f);
            camera.tag = "IKHandle";
            camera.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            camera.transform.position = new Vector3(pos.x, pos.y, -0.5f);
            camera.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            //camera.transform.SetParent(ikparent.transform);

            GameObject copyhead = (GameObject)Resources.Load("IKHandleSphereHead");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject head = Instantiate(copyhead, copyhead.transform.position, Quaternion.identity, ikparent.transform);
            head.name = "Head";// + avatar.name;
            head.transform.rotation = new Quaternion(0f, 180f, 0f, 0f);
            head.tag = "IKHandle";
            head.GetComponent<UserHandleOperation>().PartsName = "head";
            head.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            head.transform.position = animator.GetBoneTransform(HumanBodyBones.Head).transform.position;

            /*GameObject copyneck = (GameObject)Resources.Load("IKHandleSphereHeadFull");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject neck = Instantiate(copyhead, copyhead.transform.position, Quaternion.identity, ikparent.transform);
            neck.name = "Neck";// + avatar.name;
            neck.transform.rotation = new Quaternion(0f, 180f, 0f, 0f);
            neck.tag = "IKHandle";
            neck.GetComponent<UserLookAtOperation>().PartsName = "neck";
            neck.GetComponent<UserLookAtOperation>().SetRelatedAvatar(avatar, HumanBodyBones.Neck);
            neck.transform.position = animator.GetBoneTransform(HumanBodyBones.Neck).transform.position;*/

            GameObject copyaim = (GameObject)Resources.Load("IKHandleSphereChest");
            GameObject aim = Instantiate(copyaim, copyaim.transform.position, Quaternion.identity, ikparent.transform);
            aim.name = "Aim";
            aim.tag = "IKHandle";
            aim.GetComponent<UserHandleOperation>().PartsName = "aim";
            aim.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            aim.transform.position = animator.GetBoneTransform(HumanBodyBones.Neck).transform.position;
            

            GameObject copypelvis = (GameObject)Resources.Load("IKHandleSpherePelvis");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject pelvis = Instantiate(copypelvis, copypelvis.transform.position, Quaternion.identity, ikparent.transform);
            pelvis.name = "Chest";// + avatar.name;
            Vector3 vrot = new Vector3(0f, 180f, 0f);
            Quaternion rot = Quaternion.Euler(vrot);
            pelvis.transform.rotation = rot;
            pelvis.tag = "IKHandle";
            pelvis.GetComponent<UserHandleOperation>().PartsName = "chest";
            pelvis.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            pelvis.transform.position = animator.GetBoneTransform(HumanBodyBones.Spine).transform.position;



            //---Hand, Arm
            GameObject copyleftshoulder = (GameObject)Resources.Load("IKHandleSphereLeft");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject leftshoulder = Instantiate(copyleftshoulder, copyleftshoulder.transform.position, Quaternion.identity, ikparent.transform);
            leftshoulder.name = "LeftShoulder";// + avatar.name;
            leftshoulder.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            leftshoulder.tag = "IKHandle";
            leftshoulder.GetComponent<UserHandleOperation>().PartsName = "leftshoulder";
            leftshoulder.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            leftshoulder.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftShoulder).transform.position;

            GameObject copyleftlowerarm = (GameObject)Resources.Load("IKHandleSphereLeft");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject leftlowerarm = Instantiate(copyleftlowerarm, copyleftlowerarm.transform.position, Quaternion.identity, ikparent.transform);
            leftlowerarm.name = "LeftLowerArm";// + avatar.name;
            leftlowerarm.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            leftlowerarm.tag = "IKHandle";
            leftlowerarm.GetComponent<UserHandleOperation>().PartsName = "leftlowerarm";
            leftlowerarm.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            leftlowerarm.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).transform.position;

            GameObject copylefthand = (GameObject)Resources.Load("IKHandleSphereLeft");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject lefthand = Instantiate(copylefthand, copylefthand.transform.position, Quaternion.identity, ikparent.transform);
            lefthand.name = "LeftHand";// + avatar.name;
            lefthand.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            lefthand.tag = "IKHandle";
            lefthand.GetComponent<UserHandleOperation>().PartsName = "leftarm";
            lefthand.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            lefthand.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftHand).transform.position;



            GameObject copyrightshoulder = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject rightshoulder = Instantiate(copyrightshoulder, copyrightshoulder.transform.position, Quaternion.identity, ikparent.transform);
            rightshoulder.name = "RightShoulder";// + avatar.name;
            rightshoulder.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rightshoulder.tag = "IKHandle";
            rightshoulder.GetComponent<UserHandleOperation>().PartsName = "rightshoulder";
            rightshoulder.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            rightshoulder.transform.position = animator.GetBoneTransform(HumanBodyBones.RightShoulder).transform.position;

            GameObject copyrightlowerarm = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject rightlowerarm = Instantiate(copyrightlowerarm, copyrightlowerarm.transform.position, Quaternion.identity, ikparent.transform);
            rightlowerarm.name = "RightLowerArm";// + avatar.name;
            rightlowerarm.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rightlowerarm.tag = "IKHandle";
            rightlowerarm.GetComponent<UserHandleOperation>().PartsName = "rightlowerarm";
            rightlowerarm.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            rightlowerarm.transform.position = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).transform.position;

            GameObject copyrighthand = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject righthand = Instantiate(copyrighthand, copyrighthand.transform.position, Quaternion.identity, ikparent.transform);
            righthand.name = "RightHand";// + avatar.name;
            righthand.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            righthand.tag = "IKHandle";
            righthand.GetComponent<UserHandleOperation>().PartsName = "rightarm";
            righthand.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            righthand.transform.position = animator.GetBoneTransform(HumanBodyBones.RightHand).transform.position;



            //---Leg
            GameObject copyleftupperleg = (GameObject)Resources.Load("IKHandleSphereLeft");
            GameObject leftupperleg = Instantiate(copyleftupperleg, copyleftupperleg.transform.position, Quaternion.identity, ikparent.transform);
            leftupperleg.name = "LeftUpperLeg"; // + avatar.name;
            leftupperleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            leftupperleg.tag = "IKHandle";
            leftupperleg.GetComponent<UserHandleOperation>().PartsName = "leftupperleg";
            leftupperleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            leftupperleg.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).transform.position;

            GameObject copyleftlowerleg = (GameObject)Resources.Load("IKHandleSphereLeft");
            GameObject leftlowerleg = Instantiate(copyleftlowerleg, copyleftlowerleg.transform.position, Quaternion.identity, ikparent.transform);
            leftlowerleg.name = "LeftLowerLeg"; // + avatar.name;
            leftlowerleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            leftlowerleg.tag = "IKHandle";
            leftlowerleg.GetComponent<UserHandleOperation>().PartsName = "leftlowerleg";
            leftlowerleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            leftlowerleg.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).transform.position;

            GameObject copyleftleg = (GameObject)Resources.Load("IKHandleSphereLeft");
            GameObject leftleg = Instantiate(copyleftleg, copyleftleg.transform.position, Quaternion.identity, ikparent.transform);
            leftleg.name = "LeftLeg"; // + avatar.name;
            leftleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            leftleg.tag = "IKHandle";
            leftleg.GetComponent<UserHandleOperation>().PartsName = "leftleg";
            leftleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            leftleg.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftFoot).transform.position;
            //leftleg.transform.SetParent(ikparent.transform);



            GameObject copyrightupperleg = (GameObject)Resources.Load("IKHandleSphereRight");
            GameObject rightupperleg = Instantiate(copyrightupperleg, copyrightupperleg.transform.position, Quaternion.identity, ikparent.transform);
            rightupperleg.name = "RightUpperLeg"; // + avatar.name;
            rightupperleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rightupperleg.tag = "IKHandle";
            rightupperleg.GetComponent<UserHandleOperation>().PartsName = "rightupperleg";
            rightupperleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            rightupperleg.transform.position = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).transform.position;

            GameObject copyrightlowerleg = (GameObject)Resources.Load("IKHandleSphereRight");
            GameObject rightlowerleg = Instantiate(copyrightlowerleg, copyrightlowerleg.transform.position, Quaternion.identity, ikparent.transform);
            rightlowerleg.name = "RightLowerLeg"; // + avatar.name;
            rightlowerleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rightlowerleg.tag = "IKHandle";
            rightlowerleg.GetComponent<UserHandleOperation>().PartsName = "rightlowerleg";
            rightlowerleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            rightlowerleg.transform.position = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).transform.position;

            GameObject copyrightleg = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject rightleg = Instantiate(copyrightleg, copyrightleg.transform.position, Quaternion.identity, ikparent.transform);
            rightleg.name = "RightLeg";// + avatar.name;
            rightleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rightleg.tag = "IKHandle";
            rightleg.GetComponent<UserHandleOperation>().PartsName = "rightleg";
            rightleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            rightleg.transform.position = animator.GetBoneTransform(HumanBodyBones.RightFoot).transform.position;
            //rightleg.transform.SetParent(ikparent.transform);

            return ikparent;


        }
        public GameObject CreateBipedIKHandle(GameObject avatar)
        {
            GameObject ikhp = manim.ikArea; //GameObject.FindGameObjectWithTag("IKHandleWorld");
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
            OperateLoadedVRM avatar_olvrm = avatar.GetComponent<OperateLoadedVRM>();
            Animator animator = avatar.GetComponent<Animator>();
            GameObject ikparent = Instantiate((GameObject)Resources.Load("IKParentBase"));
            ikparent.name = "ikparent_" + avatar.name;
            ikparent.tag = "IKHandleParent";
            ikparent.layer = LayerMask.NameToLayer("Handle");
            ikparent.transform.Rotate(new Vector3(0f, 0f, 0f));
            //ikparent.AddComponent<PositionHandle>().enabled = false;
            //ikparent.AddComponent<RotationHandle>().enabled = false;
            ikparent.transform.SetParent(ikhp.transform);

            if (!ovrm.isMoveMode)
            {
                ovrm.ShowHandleBody(false, ikparent);
            }

            avatar_olvrm.relatedHandleParent = ikparent;

            GameObject copycamera = (GameObject)Resources.Load("EyeViewHandleSphere");
            GameObject camera = Instantiate(copycamera, copycamera.transform.position, Quaternion.identity, ikparent.transform);
            camera.name = "EyeViewHandle"; //+ avatar.name;
            Vector3 pos = animator.GetBoneTransform(HumanBodyBones.Head).transform.position;
            pos.Set(pos.x, pos.y, pos.z - 0.05f);
            camera.tag = "IKHandle";
            camera.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            camera.transform.position = new Vector3(pos.x, pos.y, -0.5f);
            camera.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            camera.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copyhead = (GameObject)Resources.Load("IKHandleSphereHead");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject head = Instantiate(copyhead, copyhead.transform.position, Quaternion.identity, ikparent.transform);
            head.name = "Head";// + avatar.name;
            head.transform.rotation = new Quaternion(0f, 180f, 0f, 0f);
            head.tag = "IKHandle";
            head.GetComponent<UserHandleOperation>().PartsName = "head";
            head.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 headpos = animator.GetBoneTransform(HumanBodyBones.Head).transform.position;
            head.transform.position = new Vector3(headpos.x, headpos.y, 0f);
            head.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copylookat = (GameObject)Resources.Load("IKHandleSphereHead");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject lookat = Instantiate(copylookat, copylookat.transform.position, Quaternion.identity, ikparent.transform);
            lookat.name = "LookAt";// + avatar.name;
            lookat.transform.rotation = new Quaternion(0f, 180f, 0f, 0f);
            lookat.tag = "IKHandle";
            lookat.GetComponent<UserHandleOperation>().PartsName = "lookat";
            lookat.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 lookatpos = animator.GetBoneTransform(HumanBodyBones.Head).transform.position;
            lookat.transform.position = new Vector3(headpos.x, headpos.y, -1f);
            lookat.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copyaim = (GameObject)Resources.Load("IKHandleSphereChest");
            GameObject aim = Instantiate(copyaim, copyaim.transform.position, Quaternion.identity, ikparent.transform);
            aim.name = "Aim";
            aim.tag = "IKHandle";
            aim.GetComponent<UserHandleOperation>().PartsName = "aim";
            aim.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);

            //---Chest is optional======
            Transform upperchesttrans = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (upperchesttrans == null)
            {
                upperchesttrans = animator.GetBoneTransform(HumanBodyBones.Spine);
            }
            aim.transform.position = new Vector3(upperchesttrans.position.x, upperchesttrans.position.y, upperchesttrans.position.z);
            aim.transform.rotation = upperchesttrans.transform.rotation;
            aim.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copychest = (GameObject)Resources.Load("IKHandleSphereChest");
            GameObject chest = Instantiate(copychest, copychest.transform.position, Quaternion.identity, ikparent.transform);
            chest.name = "Chest";
            chest.tag = "IKHandle";
            chest.GetComponent<UserHandleOperation>().PartsName = "chest";
            chest.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            chest.transform.position = upperchesttrans.transform.position;
            chest.transform.rotation = upperchesttrans.transform.rotation;
            chest.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copypelvis = (GameObject)Resources.Load("IKHandleSpherePelvis");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject pelvis = Instantiate(copypelvis, copypelvis.transform.position, Quaternion.identity, ikparent.transform);
            pelvis.name = "Pelvis";// + avatar.name;
            Vector3 vrot = new Vector3(0f, 180f, 0f);
            Quaternion rot = Quaternion.Euler(vrot);
            pelvis.transform.rotation = rot;
            pelvis.tag = "IKHandle";
            pelvis.GetComponent<UserHandleOperation>().PartsName = "pelvis";
            pelvis.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            pelvis.transform.position = animator.GetBoneTransform(HumanBodyBones.Hips).transform.position;
            pelvis.GetComponent<UserHandleOperation>().SaveDefaultTransform();



            //---Hand, Arm
            GameObject copyleftshoulder = (GameObject)Resources.Load("IKHandleTriangleLeft");
            GameObject leftshoulder = Instantiate(copyleftshoulder, copyleftshoulder.transform.position, Quaternion.identity, ikparent.transform);
            leftshoulder.name = "LeftShoulder";
            leftshoulder.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            UserHandleOperation leftsld = leftshoulder.GetComponent<UserHandleOperation>();
            leftsld.PartsName = "leftshoulder";
            leftsld.SetRelatedAvatar(avatar);
            leftshoulder.transform.position = new Vector3(
                animator.GetBoneTransform(HumanBodyBones.LeftShoulder).transform.position.x+0.09f,
                animator.GetBoneTransform(HumanBodyBones.LeftShoulder).transform.position.y,
                animator.GetBoneTransform(HumanBodyBones.LeftShoulder).transform.position.z
            );
            leftsld.SaveDefaultTransform();


            GameObject copyleftlowerarm = (GameObject)Resources.Load("IKHandleSphereLeft");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject leftlowerarm = Instantiate(copyleftlowerarm, copyleftlowerarm.transform.position, Quaternion.identity, ikparent.transform);
            leftlowerarm.name = "LeftLowerArm";// + avatar.name;
            leftlowerarm.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            leftlowerarm.tag = "IKHandle";
            leftlowerarm.GetComponent<UserHandleOperation>().PartsName = "leftlowerarm";
            leftlowerarm.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            leftlowerarm.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).transform.position;
            leftlowerarm.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copylefthand = (GameObject)Resources.Load("IKHandleSphereLeft");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject lefthand = Instantiate(copylefthand, copylefthand.transform.position, Quaternion.identity, ikparent.transform);
            lefthand.name = "LeftHand";// + avatar.name;
            lefthand.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            lefthand.tag = "IKHandle";
            lefthand.GetComponent<UserHandleOperation>().PartsName = "leftarm";
            lefthand.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            lefthand.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftHand).transform.position;
            lefthand.GetComponent<UserHandleOperation>().SaveDefaultTransform();



            GameObject copyrightshoulder = (GameObject)Resources.Load("IKHandleTriangleRight");
            GameObject rightshoulder = Instantiate(copyrightshoulder, copyrightshoulder.transform.position, Quaternion.identity, ikparent.transform);
            rightshoulder.name = "RightShoulder";
            rightshoulder.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            UserHandleOperation rightsld = rightshoulder.GetComponent<UserHandleOperation>();
            rightsld.PartsName = "rightshoulder";
            rightsld.SetRelatedAvatar(avatar);
            rightshoulder.transform.position = new Vector3(
                animator.GetBoneTransform(HumanBodyBones.RightShoulder).transform.position.x-0.09f,
                animator.GetBoneTransform(HumanBodyBones.RightShoulder).transform.position.y,
                animator.GetBoneTransform(HumanBodyBones.RightShoulder).transform.position.z
            );
            rightsld.SaveDefaultTransform();


            GameObject copyrightlowerarm = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject rightlowerarm = Instantiate(copyrightlowerarm, copyrightlowerarm.transform.position, Quaternion.identity, ikparent.transform);
            rightlowerarm.name = "RightLowerArm";// + avatar.name;
            rightlowerarm.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rightlowerarm.tag = "IKHandle";
            rightlowerarm.GetComponent<UserHandleOperation>().PartsName = "rightlowerarm";
            rightlowerarm.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            rightlowerarm.transform.position = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).transform.position;
            rightlowerarm.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copyrighthand = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject righthand = Instantiate(copyrighthand, copyrighthand.transform.position, Quaternion.identity, ikparent.transform);
            righthand.name = "RightHand";// + avatar.name;
            righthand.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            righthand.tag = "IKHandle";
            righthand.GetComponent<UserHandleOperation>().PartsName = "rightarm";
            righthand.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            righthand.transform.position = animator.GetBoneTransform(HumanBodyBones.RightHand).transform.position;
            righthand.GetComponent<UserHandleOperation>().SaveDefaultTransform();



            //---Leg
            GameObject copyleftlowerleg = (GameObject)Resources.Load("IKHandleSphereLeft");
            GameObject leftlowerleg = Instantiate(copyleftlowerleg, copyleftlowerleg.transform.position, Quaternion.identity, ikparent.transform);
            leftlowerleg.name = "LeftLowerLeg"; // + avatar.name;
            leftlowerleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            leftlowerleg.tag = "IKHandle";
            leftlowerleg.GetComponent<UserHandleOperation>().PartsName = "leftlowerleg";
            leftlowerleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            leftlowerleg.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).transform.position;
            leftlowerleg.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copyleftleg = (GameObject)Resources.Load("IKHandleSphereLeft");
            GameObject leftleg = Instantiate(copyleftleg, copyleftleg.transform.position, Quaternion.identity, ikparent.transform);
            leftleg.name = "LeftLeg"; // + avatar.name;
            leftleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            leftleg.tag = "IKHandle";
            leftleg.GetComponent<UserHandleOperation>().PartsName = "leftleg";
            leftleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            leftleg.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftFoot).transform.position;
            leftleg.GetComponent<UserHandleOperation>().SaveDefaultTransform();



            GameObject copyrightlowerleg = (GameObject)Resources.Load("IKHandleSphereRight");
            GameObject rightlowerleg = Instantiate(copyrightlowerleg, copyrightlowerleg.transform.position, Quaternion.identity, ikparent.transform);
            rightlowerleg.name = "RightLowerLeg"; // + avatar.name;
            rightlowerleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rightlowerleg.tag = "IKHandle";
            rightlowerleg.GetComponent<UserHandleOperation>().PartsName = "rightlowerleg";
            rightlowerleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            rightlowerleg.transform.position = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).transform.position;
            rightlowerleg.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copyrightleg = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject rightleg = Instantiate(copyrightleg, copyrightleg.transform.position, Quaternion.identity, ikparent.transform);
            rightleg.name = "RightLeg";// + avatar.name;
            rightleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rightleg.tag = "IKHandle";
            rightleg.GetComponent<UserHandleOperation>().PartsName = "rightleg";
            rightleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            rightleg.transform.position = animator.GetBoneTransform(HumanBodyBones.RightFoot).transform.position;
            rightleg.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            return ikparent;


        }
        /// <summary>
        /// create light object
        /// </summary>
        /// <returns>0 - main object, 1 - ik object</returns>
        public GameObject[] CreateLight(string type)
        {
            GameObject copylight = null;
            GameObject copyikLight = null;
            GameObject ikhp = manim.ikArea; //GameObject.FindGameObjectWithTag("IKHandleWorld");
            GameObject viewBody = manim.AvatarArea; // GameObject.Find("View Body");

            copyikLight = (GameObject)Resources.Load("IKHandleLight");

            if (type == "spot")
            {
                copylight = (GameObject)Resources.Load("UserSpotLight");

            }
            else if (type == "point")
            {
                copylight = (GameObject)Resources.Load("UserPointLight");

            }

            GameObject[] ret = new GameObject[2];

            ret[0] = Instantiate(copylight, copylight.transform.position, Quaternion.identity, viewBody.transform);
            ret[1] = Instantiate(copyikLight, copyikLight.transform.position, Quaternion.identity, ikhp.transform);
            //ret[0].GetComponent<Light>()

            ret[0].transform.SetParent(viewBody.transform);
            ret[1].transform.SetParent(ikhp.transform);
            //---re-positionning IK to real object
            ret[1].transform.position = new Vector3(ret[0].transform.position.x, ret[0].transform.position.y, ret[0].transform.position.z);
            ret[1].transform.rotation = Quaternion.Euler(ret[0].transform.eulerAngles);
            OtherObjectDummyIK oodk = ret[1].GetComponent<OtherObjectDummyIK>();
            oodk.relatedAvatar = ret[0];

            return ret;
        }
        public GameObject CreateText(string text, string anchorpos)
        {
            GameObject msgarea = manim.MsgArea; //GameObject.Find("MsgArea");
            GameObject copytex = null;
            copytex = (GameObject)Resources.Load("UserTextTL");
            GameObject tex = Instantiate(copytex, copytex.transform.position, Quaternion.identity, msgarea.transform);
            RectTransform rect = tex.GetComponent<RectTransform>();

            if (anchorpos == "tl")
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
            }
            else if (anchorpos == "bl")
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 0f);
            }
            else if (anchorpos == "tr")
            {
                rect.anchorMin = new Vector2(1f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
            }
            else if (anchorpos == "br")
            {
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(1f, 0f);
            }

            tex.transform.SetParent(msgarea.transform, false);
            tex.GetComponent<Text>().text = text;
            tex.AddComponent<OperateLoadedText>();

            return tex;
        }

        /// <summary>
        /// create camera object
        /// </summary>
        /// <returns>0 - main object, 1 - ik object</returns>
        public GameObject[] CreateCamera()
        {
            GameObject animarea = manim.AvatarArea; //GameObject.Find("View Body");
            GameObject ikhp = manim.ikArea; //GameObject.FindGameObjectWithTag("IKHandleWorld");

            //GameObject campar = new GameObject();
            GameObject copycam;
            copycam = (GameObject)Resources.Load("UserCamera");
            GameObject ikcopycam = (GameObject)Resources.Load("IKHandleCamera");

            GameObject[] ret = new GameObject[2];
            ret[0] = Instantiate(copycam, copycam.transform.position, Quaternion.identity, animarea.transform);
            ret[1] = Instantiate(ikcopycam, ikcopycam.transform.position, Quaternion.identity, ikhp.transform);

            //---connect camera transform to Post-process-layer
            PostProcessLayer ppl = ret[0].GetComponent<PostProcessLayer>();
            ppl.volumeTrigger = ret[0].transform;

            //---re-positionning IK to real object
            ret[1].transform.position = new Vector3(ret[0].transform.position.x, ret[0].transform.position.y, ret[0].transform.position.z);
            ret[1].transform.rotation = Quaternion.Euler(ret[0].transform.eulerAngles);
            OtherObjectDummyIK oodk = ret[1].GetComponent<OtherObjectDummyIK>();
            oodk.relatedAvatar = ret[0];


            return ret;
        }
        /// <summary>
        /// create effect object
        /// </summary>
        /// <returns>0 - main object, 1 - ik object</returns>
        public GameObject[] CreateEffect()
        {
            GameObject animarea = manim.AvatarArea; //GameObject.Find("View Body");
            GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");

            //GameObject campar = new GameObject();
            GameObject copyeff;
            copyeff = (GameObject)Resources.Load("UserEffect");
            GameObject ikcopyeff = (GameObject)Resources.Load("IKEffect");

            GameObject[] ret = new GameObject[2];
            ret[0] = Instantiate(copyeff, copyeff.transform.position, Quaternion.identity, animarea.transform);
            ret[1] = Instantiate(ikcopyeff, ikcopyeff.transform.position, Quaternion.identity, ikhp.transform);

            //GameObject ret = Instantiate(copyeff, copyeff.transform.position, Quaternion.identity, animarea.transform);

            ret[0].transform.GetComponent<OperateLoadedEffect>().previewColliderSphere = ret[0].transform.GetChild(0).gameObject;

            OtherObjectDummyIK oodk = ret[1].GetComponent<OtherObjectDummyIK>();
            oodk.relatedAvatar = ret[0];

            return ret;
        }
    }

}
