using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;
using UserHandleSpace;
using RootMotion.FinalIK;
using LumisIkApp;

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

        public List<Shader> shaders = new List<Shader>();

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
            /*if (false)
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
            }*/
            
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

                if (child.name.IndexOf("Shoulder") == -1)
                {
                    child.transform.localScale = ikmarker_size;
                }
                

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
            Quaternion CmnRotation = Quaternion.Euler(new Vector3(0, 180f, 0));

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
            head.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
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

            /*
            GameObject copyaim = (GameObject)Resources.Load("IKHandleSphereChest");
            GameObject aim = Instantiate(copyaim, copyaim.transform.position, Quaternion.identity, ikparent.transform);
            aim.name = "Aim";
            aim.tag = "IKHandle";
            aim.GetComponent<UserHandleOperation>().PartsName = "aim";
            aim.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            aim.transform.position = animator.GetBoneTransform(HumanBodyBones.Neck).transform.position;
            */
            GameObject copylookat = (GameObject)Resources.Load("IKHandleSphereHead");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject lookat = Instantiate(copylookat, copylookat.transform.position, Quaternion.identity, ikparent.transform);
            lookat.name = "LookAt";// + avatar.name;
            lookat.transform.rotation = CmnRotation;
            lookat.tag = "IKHandle";
            lookat.GetComponent<UserHandleOperation>().PartsName = "lookat";
            lookat.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 lookatpos = animator.GetBoneTransform(HumanBodyBones.Head).transform.position;
            lookat.transform.position = new Vector3(lookatpos.x, lookatpos.y, -1f);
            lookat.GetComponent<UserHandleOperation>().SaveDefaultTransform();


            GameObject copychest = (GameObject)Resources.Load("IKHandleSpherePelvis");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject chest = Instantiate(copychest, copychest.transform.position, Quaternion.identity, ikparent.transform);
            chest.name = "Chest";// + avatar.name;
            Vector3 vrot = new Vector3(0f, 180f, 0f);
            Quaternion rot = CmnRotation;
            chest.transform.rotation = rot;
            chest.tag = "IKHandle";
            chest.GetComponent<UserHandleOperation>().PartsName = "chest";
            chest.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            chest.transform.position = animator.GetBoneTransform(HumanBodyBones.Spine).transform.position;


            //---Hand, Arm
            GameObject copyleftshoulder = (GameObject)Resources.Load("IKHandleTriangleLeft");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject leftshoulder = Instantiate(copyleftshoulder, copyleftshoulder.transform.position, Quaternion.identity, ikparent.transform);
            leftshoulder.name = "LeftShoulder";// + avatar.name;
            leftshoulder.transform.rotation = CmnRotation; // Quaternion.Euler(0f, 180f, 0f);
            leftshoulder.tag = "IKHandle";
            UserHandleOperation uho_lshould = leftshoulder.GetComponent<UserHandleOperation>();
            uho_lshould.PartsName = "leftshoulder";
            uho_lshould.SetRelatedAvatar(avatar);
            leftshoulder.transform.position = new Vector3(
                animator.GetBoneTransform(HumanBodyBones.LeftShoulder).position.x + 0.09f,
                animator.GetBoneTransform(HumanBodyBones.LeftShoulder).position.y,
                animator.GetBoneTransform(HumanBodyBones.LeftShoulder).position.z
            );
            uho_lshould.SaveDefaultTransform();

            GameObject copyleftlowerarm = (GameObject)Resources.Load("IKHandleSphereLeft");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject leftlowerarm = Instantiate(copyleftlowerarm, copyleftlowerarm.transform.position, Quaternion.identity, ikparent.transform);
            leftlowerarm.name = "LeftLowerArm";// + avatar.name;
            leftlowerarm.transform.rotation = CmnRotation; //Quaternion.Euler(0f, 180f, 0f);
            leftlowerarm.tag = "IKHandle";
            leftlowerarm.GetComponent<UserHandleOperation>().PartsName = "leftlowerarm";
            leftlowerarm.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            leftlowerarm.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).transform.position;

            GameObject copylefthand = (GameObject)Resources.Load("IKHandleSphereLeft");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject lefthand = Instantiate(copylefthand, copylefthand.transform.position, Quaternion.identity, ikparent.transform);
            lefthand.name = "LeftHand";// + avatar.name;
            lefthand.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            lefthand.tag = "IKHandle";
            lefthand.GetComponent<UserHandleOperation>().PartsName = "leftarm";
            lefthand.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            lefthand.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftHand).transform.position;



            GameObject copyrightshoulder = (GameObject)Resources.Load("IKHandleTriangleRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject rightshoulder = Instantiate(copyrightshoulder, copyrightshoulder.transform.position, Quaternion.identity, ikparent.transform);
            rightshoulder.name = "RightShoulder";// + avatar.name;
            rightshoulder.transform.rotation = CmnRotation; //Quaternion.Euler(0f, 180f, 0f);
            rightshoulder.tag = "IKHandle";
            UserHandleOperation uho_rsho = rightshoulder.GetComponent<UserHandleOperation>();
            uho_rsho.PartsName = "rightshoulder";
            uho_rsho.SetRelatedAvatar(avatar);
            rightshoulder.transform.position = new Vector3(
                animator.GetBoneTransform(HumanBodyBones.RightShoulder).position.x - 0.09f,
                animator.GetBoneTransform(HumanBodyBones.RightShoulder).position.y,
                animator.GetBoneTransform(HumanBodyBones.RightShoulder).position.z
            );
            uho_rsho.SaveDefaultTransform();

            GameObject copyrightlowerarm = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject rightlowerarm = Instantiate(copyrightlowerarm, copyrightlowerarm.transform.position, Quaternion.identity, ikparent.transform);
            rightlowerarm.name = "RightLowerArm";// + avatar.name;
            rightlowerarm.transform.rotation = CmnRotation; //Quaternion.Euler(0f, 180f, 0f);
            rightlowerarm.tag = "IKHandle";
            rightlowerarm.GetComponent<UserHandleOperation>().PartsName = "rightlowerarm";
            rightlowerarm.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            rightlowerarm.transform.position = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).transform.position;

            GameObject copyrighthand = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject righthand = Instantiate(copyrighthand, copyrighthand.transform.position, Quaternion.identity, ikparent.transform);
            righthand.name = "RightHand";// + avatar.name;
            righthand.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            righthand.tag = "IKHandle";
            righthand.GetComponent<UserHandleOperation>().PartsName = "rightarm";
            righthand.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            righthand.transform.position = animator.GetBoneTransform(HumanBodyBones.RightHand).transform.position;


            //---new: upperchest
            GameObject copyupperchest = (GameObject)Resources.Load("IKHandleSphereChest");
            GameObject upperchest = Instantiate(copyupperchest, copyupperchest.transform.position, Quaternion.identity, ikparent.transform);
            upperchest.name = "UpperChest";
            upperchest.tag = "IKHandle";
            upperchest.GetComponent<UserHandleOperation>().PartsName = "upperchest";
            upperchest.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            if (animator.GetBoneTransform(HumanBodyBones.Neck) != null) upperchest.transform.position = animator.GetBoneTransform(HumanBodyBones.Neck).transform.position;
            else if (animator.GetBoneTransform(HumanBodyBones.UpperChest) != null) upperchest.transform.position = animator.GetBoneTransform(HumanBodyBones.UpperChest).transform.position;
            //### LeftSholuder + RightShoulder = Upper Chest
            leftshoulder.layer = 10;
            rightshoulder.layer = 10;
            leftshoulder.transform.SetParent(upperchest.transform);
            rightshoulder.transform.SetParent(upperchest.transform);



            //---Leg
            GameObject copyleftupperleg = (GameObject)Resources.Load("IKHandleSphereLeft");
            GameObject leftupperleg = Instantiate(copyleftupperleg, copyleftupperleg.transform.position, Quaternion.identity, ikparent.transform);
            leftupperleg.name = "LeftUpperLeg"; // + avatar.name;
            leftupperleg.transform.rotation = CmnRotation; //Quaternion.Euler(0f, 180f, 0f);
            leftupperleg.tag = "IKHandle";
            leftupperleg.GetComponent<UserHandleOperation>().PartsName = "leftupperleg";
            leftupperleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            leftupperleg.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).transform.position;

            GameObject copyleftlowerleg = (GameObject)Resources.Load("IKHandleSphereLeft");
            GameObject leftlowerleg = Instantiate(copyleftlowerleg, copyleftlowerleg.transform.position, Quaternion.identity, ikparent.transform);
            leftlowerleg.name = "LeftLowerLeg"; // + avatar.name;
            leftlowerleg.transform.rotation = CmnRotation; //Quaternion.Euler(0f, 180f, 0f);
            leftlowerleg.tag = "IKHandle";
            leftlowerleg.GetComponent<UserHandleOperation>().PartsName = "leftlowerleg";
            leftlowerleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            leftlowerleg.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).transform.position;

            GameObject copyleftleg = (GameObject)Resources.Load("IKHandleSphereLeft");
            GameObject leftleg = Instantiate(copyleftleg, copyleftleg.transform.position, Quaternion.identity, ikparent.transform);
            leftleg.name = "LeftLeg"; // + avatar.name;
            leftleg.transform.rotation = CmnRotation; //Quaternion.Euler(0f, 180f, 0f);
            leftleg.tag = "IKHandle";
            leftleg.GetComponent<UserHandleOperation>().PartsName = "leftleg";
            leftleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            leftleg.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftFoot).transform.position;
            //leftleg.transform.SetParent(ikparent.transform);



            GameObject copyrightupperleg = (GameObject)Resources.Load("IKHandleSphereRight");
            GameObject rightupperleg = Instantiate(copyrightupperleg, copyrightupperleg.transform.position, Quaternion.identity, ikparent.transform);
            rightupperleg.name = "RightUpperLeg"; // + avatar.name;
            rightupperleg.transform.rotation = CmnRotation; //Quaternion.Euler(0f, 180f, 0f);
            rightupperleg.tag = "IKHandle";
            rightupperleg.GetComponent<UserHandleOperation>().PartsName = "rightupperleg";
            rightupperleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            rightupperleg.transform.position = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).transform.position;

            GameObject copyrightlowerleg = (GameObject)Resources.Load("IKHandleSphereRight");
            GameObject rightlowerleg = Instantiate(copyrightlowerleg, copyrightlowerleg.transform.position, Quaternion.identity, ikparent.transform);
            rightlowerleg.name = "RightLowerLeg"; // + avatar.name;
            rightlowerleg.transform.rotation = CmnRotation; //Quaternion.Euler(0f, 180f, 0f);
            rightlowerleg.tag = "IKHandle";
            rightlowerleg.GetComponent<UserHandleOperation>().PartsName = "rightlowerleg";
            rightlowerleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            rightlowerleg.transform.position = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).transform.position;

            GameObject copyrightleg = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject rightleg = Instantiate(copyrightleg, copyrightleg.transform.position, Quaternion.identity, ikparent.transform);
            rightleg.name = "RightLeg";// + avatar.name;
            rightleg.transform.rotation = CmnRotation; //Quaternion.Euler(0f, 180f, 0f);
            rightleg.tag = "IKHandle";
            rightleg.GetComponent<UserHandleOperation>().PartsName = "rightleg";
            rightleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            rightleg.transform.position = animator.GetBoneTransform(HumanBodyBones.RightFoot).transform.position;
            //rightleg.transform.SetParent(ikparent.transform);

            //---new: Pelvis
            GameObject copypelvis = (GameObject)Resources.Load("IKHandleSpherePelvis");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject pelvis = Instantiate(copypelvis, copypelvis.transform.position, Quaternion.identity, ikparent.transform);
            pelvis.name = "Pelvis";
            pelvis.tag = "IKHandle";
            UserHandleOperation uho_pelvis = pelvis.GetComponent<UserHandleOperation>();
            uho_pelvis.PartsName = "pelvis";
            uho_pelvis.SetRelatedAvatar(avatar);
            pelvis.transform.position = animator.GetBoneTransform(HumanBodyBones.Hips).transform.position;
            uho_pelvis.SaveDefaultTransform();
            //### LeftUpperLeg + RightUpperLeg = Pelvis
            leftupperleg.layer = 10;
            rightupperleg.layer = 10;
            leftupperleg.transform.SetParent(pelvis.transform);
            rightupperleg.transform.SetParent(pelvis.transform);
            pelvis.transform.rotation = CmnRotation; //Quaternion.Euler(0f, 0f, 0f);

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
            //ikparent.transform.Rotate(new Vector3(0f, 0f, 0f));
            //ikparent.AddComponent<PositionHandle>().enabled = false;
            //ikparent.AddComponent<RotationHandle>().enabled = false;
            ikparent.transform.SetParent(ikhp.transform);

            if (!ovrm.isMoveMode)
            {
                ovrm.ShowHandleBody(false, ikparent);
            }

            Quaternion CmnRotation = Quaternion.Euler(new Vector3(0, 0f, 0));
            Quaternion CmnRotation180 = Quaternion.Euler(new Vector3(0, 180f, 0));
            Quaternion CmnRotation90180 = Quaternion.Euler(new Vector3(-90f, 0f, 0f));

            avatar_olvrm.relatedHandleParent = ikparent;
            UserGroundOperation ugo = ikparent.GetComponent<UserGroundOperation>();
            ugo.SetRelatedAvatar(avatar);

            GameObject copycamera = (GameObject)Resources.Load("EyeViewHandleSphere"); //---showable IK marker
            GameObject camera = Instantiate(copycamera, copycamera.transform.position, Quaternion.identity, ikparent.transform);
            camera.name = "EyeViewHandle"; //+ avatar.name;
            Vector3 pos = animator.GetBoneTransform(HumanBodyBones.Head).transform.position;
            pos.Set(pos.x, pos.y, pos.z - 0.05f);
            camera.tag = "IKHandle";
            //camera.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            camera.transform.position = new Vector3(pos.x, pos.y, -0.5f);
            camera.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
            ///camera.GetComponent<UserHandleOperation>().SaveDefaultTransform();
            //---For VRM1.0 (for effective transform)
            /*
            GameObject realcamera = Instantiate(copycamera, copycamera.transform.position, Quaternion.identity, ikparent.transform);
            realcamera.name = "REAL:EyeViewHandle";
            realcamera.layer = camera.layer;
            realcamera.tag = camera.tag;
            realcamera.transform.position = new Vector3(camera.transform.position.x * -1, camera.transform.position.y, camera.transform.position.z);
            realcamera.transform.SetParent(ikparent.transform);
            UserHandleOperation realcamera_uho = realcamera.AddComponent<UserHandleOperation>();
            realcamera_uho.SetRelatedAvatar(avatar);
            realcamera_uho.SaveDefaultTransform();
            UserVRM10IK uvik_camera = camera.GetComponent<UserVRM10IK>();
            uvik_camera.relatedOriginalIK = realcamera.transform;
            uvik_camera.originalIKName = camera.name;
            uvik_camera.asistType = UserVRM10IK.AsistType.EyeViewHandle;
            */


            GameObject copyhead = (GameObject)Resources.Load("IKHandleSphereHead");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject head = Instantiate(copyhead, copyhead.transform.position, Quaternion.identity, ikparent.transform);
            head.name = "Head";// + avatar.name;
            head.transform.rotation = Quaternion.Euler(0, 180f, 0);
            head.tag = "IKHandle";
            head.GetComponent<UserHandleOperation>().PartsName = "head";
            head.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 headpos = animator.GetBoneTransform(HumanBodyBones.Head).transform.position;
            head.transform.position = new Vector3(headpos.x, headpos.y, 0f);
            head.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copylookat = (GameObject)Resources.Load("IKHandleSphereHead");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject lookat = Instantiate(copylookat, copylookat.transform.position, Quaternion.identity, ikparent.transform);
            lookat.name = "LookAt";// + avatar.name;
            lookat.transform.rotation = CmnRotation;
            lookat.tag = "IKHandle";
            lookat.GetComponent<UserHandleOperation>().PartsName = "lookat";
            lookat.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 lookatpos = animator.GetBoneTransform(HumanBodyBones.Head).transform.position;
            lookat.transform.position = new Vector3(headpos.x*-1, headpos.y, -1f);
            lookat.GetComponent<UserHandleOperation>().SaveDefaultTransform();
            //---For VRM1.0 (for effective transform)
            /*
            GameObject reallookat = Instantiate(copylookat, copylookat.transform.position, Quaternion.identity, ikparent.transform);
            UserVRM10IK uvik_lookat = lookat.AddComponent<UserVRM10IK>();
            uvik_lookat.relatedOriginalIK = reallookat.transform;
            uvik_lookat.originalIKName = lookat.name;
            uvik_lookat.asistType = UserVRM10IK.AsistType.LookAt;

            reallookat.name = "REAL:LookAt";
            reallookat.tag = lookat.tag;
            reallookat.layer = lookat.layer;
            reallookat.transform.position = new Vector3(lookat.transform.position.x * -1, lookat.transform.position.y, lookat.transform.position.z*-1);
            reallookat.transform.SetParent(ikparent.transform);
            UserHandleOperation reallookat_uho = reallookat.GetComponent<UserHandleOperation>();
            reallookat_uho.SetRelatedAvatar(avatar);
            reallookat_uho.PartsName = "lookat";
            reallookat_uho.SaveDefaultTransform();
            */

            GameObject copyaim = (GameObject)Resources.Load("IKHandleSphereChest");
            GameObject aim = Instantiate(copyaim, copyaim.transform.position, Quaternion.identity, ikparent.transform);
            aim.name = "Aim";
            aim.tag = "IKHandle";
            aim.GetComponent<UserHandleOperation>().PartsName = "aim";
            aim.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            ///aim.GetComponent<UserHandleOperation>().enabled = false;
            //---For VRM1.0 (for effective transform)
            /*
            GameObject realaim = Instantiate(copyaim, copyaim.transform.position, Quaternion.identity, ikparent.transform);
            realaim.name = "REAL:Aim";
            realaim.tag = aim.tag;
            realaim.layer = aim.layer;
            realaim.transform.SetParent(ikparent.transform);
            UserHandleOperation realaim_uho = realaim.GetComponent<UserHandleOperation>();
            realaim_uho.SetRelatedAvatar(avatar);
            realaim_uho.PartsName = "aim";
            realaim_uho.SaveDefaultTransform();
            UserVRM10IK uvik_aim = aim.AddComponent<UserVRM10IK>();
            uvik_aim.relatedOriginalIK = realaim.transform;
            uvik_aim.originalIKName = aim.name;
            uvik_aim.asistType = UserVRM10IK.AsistType.Aim;
            */


            //---Chest is optional======
            Transform upperchesttrans = animator.GetBoneTransform(HumanBodyBones.Chest);
            if (upperchesttrans == null)
            {
                upperchesttrans = animator.GetBoneTransform(HumanBodyBones.Spine);
            }
            aim.transform.position = new Vector3(upperchesttrans.position.x, upperchesttrans.position.y, upperchesttrans.position.z);
            aim.transform.rotation = upperchesttrans.transform.rotation;
            aim.GetComponent<UserHandleOperation>().SaveDefaultTransform();
            //realaim.transform.position = new Vector3(aim.transform.position.x * -1, aim.transform.position.y, aim.transform.position.z*-1);

            GameObject copychest = (GameObject)Resources.Load("IKHandleSphereChest");
            GameObject chest = Instantiate(copychest, copychest.transform.position, Quaternion.identity, ikparent.transform);
            chest.name = "Chest";
            chest.tag = "IKHandle";
            chest.GetComponent<UserHandleOperation>().PartsName = "chest";
            chest.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            chest.transform.position = upperchesttrans.transform.position;
            chest.transform.rotation = CmnRotation;
            chest.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copypelvis = (GameObject)Resources.Load("IKHandleSpherePelvis");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject pelvis = Instantiate(copypelvis, copypelvis.transform.position, Quaternion.identity, ikparent.transform);
            pelvis.name = "Pelvis";// + avatar.name;
            //Vector3 vrot = new Vector3(0f, 0f, 0f);
            //Quaternion rot = Quaternion.Euler(vrot);
            pelvis.transform.rotation = CmnRotation;
            pelvis.tag = "IKHandle";
            pelvis.GetComponent<UserHandleOperation>().PartsName = "pelvis";
            pelvis.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            pelvis.transform.position = animator.GetBoneTransform(HumanBodyBones.Hips).transform.position;
            pelvis.GetComponent<UserHandleOperation>().SaveDefaultTransform();



            //---Hand, Arm
            Vector3 animLeftShoulderPos = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).transform.position;
            GameObject copyleftshoulder = (GameObject)Resources.Load("IKHandleTriangleLeft");
            GameObject leftshoulder = Instantiate(copyleftshoulder, copyleftshoulder.transform.position, Quaternion.identity, chest.transform);

            leftshoulder.name = "LeftShoulder"; 
            leftshoulder.transform.rotation = CmnRotation180;
            leftshoulder.transform.localScale = new Vector3(1f, 1f, 1f);
            UserHandleOperation leftsld = leftshoulder.GetComponent<UserHandleOperation>();
            leftsld.PartsName = "leftshoulder";
            leftsld.SetRelatedAvatar(avatar);
            leftshoulder.transform.position = new Vector3(
                (animLeftShoulderPos.x*0.5f) * -1f,
                chest.transform.position.y,
                animLeftShoulderPos.z
            );
            leftsld.SaveDefaultTransform();
            ugo.LeftShoulderIK = leftshoulder.transform;


            GameObject copyleftlowerarm = (GameObject)Resources.Load("IKHandleSphereLeft");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject leftlowerarm = Instantiate(copyleftlowerarm, copyleftlowerarm.transform.position, Quaternion.identity, ikparent.transform);
            leftlowerarm.name = "LeftLowerArm";// + avatar.name;
            leftlowerarm.transform.rotation = CmnRotation;
            leftlowerarm.tag = "IKHandle";
            leftlowerarm.GetComponent<UserHandleOperation>().PartsName = "leftlowerarm";
            leftlowerarm.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 leftlowervec = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).transform.position;
            leftlowervec.x *= -1f;
            leftlowervec.z += 0.01f;
            leftlowerarm.transform.position = leftlowervec;
            leftlowerarm.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            //leftshoulder.GetComponent<SkinnedMeshRenderer>().sharedMaterial = leftlowerarm.GetComponent<SkinnedMeshRenderer>().sharedMaterial;

            GameObject copylefthand = (GameObject)Resources.Load("IKHandleSphereLeft");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject lefthand = Instantiate(copylefthand, copylefthand.transform.position, Quaternion.identity, ikparent.transform);
            lefthand.name = "LeftHand";// + avatar.name;
            lefthand.transform.rotation = CmnRotation;
            lefthand.tag = "IKHandle";
            lefthand.GetComponent<UserHandleOperation>().PartsName = "leftarm";
            lefthand.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            lefthand.transform.position = animator.GetBoneTransform(HumanBodyBones.LeftHand).transform.position;
            lefthand.GetComponent<UserHandleOperation>().SaveDefaultTransform();


            Vector3 animRightShoulderPos = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).transform.position;
            GameObject copyrightshoulder = (GameObject)Resources.Load("IKHandleTriangleRight");
            GameObject rightshoulder = Instantiate(copyrightshoulder, copyrightshoulder.transform.position, Quaternion.identity, chest.transform);
            rightshoulder.name = "RightShoulder";
            rightshoulder.transform.rotation = CmnRotation180;
            rightshoulder.transform.localScale = new Vector3(1f, 1f, 1f);
            UserHandleOperation rightsld = rightshoulder.GetComponent<UserHandleOperation>();
            rightsld.PartsName = "rightshoulder";
            rightsld.SetRelatedAvatar(avatar);
            rightshoulder.transform.position = new Vector3(
                (animRightShoulderPos.x*0.5f) * -1f,
                chest.transform.position.y,
                animRightShoulderPos.z
            );
            rightsld.SaveDefaultTransform();
            ugo.RightShoulderIK = rightshoulder.transform;


            GameObject copyrightlowerarm = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject rightlowerarm = Instantiate(copyrightlowerarm, copyrightlowerarm.transform.position, Quaternion.identity, ikparent.transform);
            rightlowerarm.name = "RightLowerArm";// + avatar.name;
            rightlowerarm.transform.rotation = CmnRotation;
            rightlowerarm.tag = "IKHandle";
            rightlowerarm.GetComponent<UserHandleOperation>().PartsName = "rightlowerarm";
            rightlowerarm.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 rightlowervec = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).transform.position;
            rightlowervec.x *= -1f;
            rightlowervec.z += 0.01f;
            rightlowerarm.transform.position = rightlowervec;
            rightlowerarm.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            //rightshoulder.GetComponent<SkinnedMeshRenderer>().sharedMaterial = rightlowerarm.GetComponent<SkinnedMeshRenderer>().sharedMaterial;

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
            Vector3 animLeftLowerLegPos = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).transform.position;
            animLeftLowerLegPos.x *= -1f;
            leftlowerleg.transform.position = animLeftLowerLegPos;
            leftlowerleg.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copyleftleg = (GameObject)Resources.Load("IKHandleSphereLeft");
            GameObject leftleg = Instantiate(copyleftleg, copyleftleg.transform.position, Quaternion.identity, ikparent.transform);
            leftleg.name = "LeftLeg"; // + avatar.name;
            leftleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            leftleg.tag = "IKHandle";
            leftleg.GetComponent<UserHandleOperation>().PartsName = "leftleg";
            leftleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 animLeftFootPos = animator.GetBoneTransform(HumanBodyBones.LeftFoot).transform.position;
            animLeftFootPos.x *= -1f;
            leftleg.transform.position = animLeftFootPos;
            
            leftleg.GetComponent<UserHandleOperation>().SaveDefaultTransform();



            GameObject copyrightlowerleg = (GameObject)Resources.Load("IKHandleSphereRight");
            GameObject rightlowerleg = Instantiate(copyrightlowerleg, copyrightlowerleg.transform.position, Quaternion.identity, ikparent.transform);
            rightlowerleg.name = "RightLowerLeg"; // + avatar.name;
            rightlowerleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rightlowerleg.tag = "IKHandle";
            rightlowerleg.GetComponent<UserHandleOperation>().PartsName = "rightlowerleg";
            rightlowerleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 animRightLowerLegPos = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).transform.position;
            animRightLowerLegPos.x *= -1f;
            rightlowerleg.transform.position = animRightLowerLegPos;
            rightlowerleg.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copyrightleg = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject rightleg = Instantiate(copyrightleg, copyrightleg.transform.position, Quaternion.identity, ikparent.transform);
            rightleg.name = "RightLeg";// + avatar.name;
            rightleg.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            rightleg.tag = "IKHandle";
            rightleg.GetComponent<UserHandleOperation>().PartsName = "rightleg";
            rightleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 animRightFootPos = animator.GetBoneTransform(HumanBodyBones.RightFoot).transform.position;
            animRightFootPos.x *= -1f;
            rightleg.transform.position = animRightFootPos;
            rightleg.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            return ikparent;


        }
        
        /// <summary>
        /// Create IKHandle for VVMIK (original IK system)
        /// </summary>
        /// <param name="avatar"></param>
        /// <returns></returns>
        public GameObject CreateVVMIKHandle(GameObject avatar)
        {
            GameObject ikhp = manim.ikArea; 
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
            OperateLoadedVRM avatar_olvrm = avatar.GetComponent<OperateLoadedVRM>();
            Animator animator = avatar.GetComponent<Animator>();
            GameObject ikparent = Instantiate((GameObject)Resources.Load("IKParentBase"));
            ikparent.name = "ikparent_" + avatar.name;
            ikparent.tag = "IKHandleParent";
            ikparent.layer = LayerMask.NameToLayer("Handle");
            ikparent.transform.Rotate(new Vector3(0f, 0f, 0f));
            ikparent.transform.SetParent(ikhp.transform);

            if (!ovrm.isMoveMode)
            {
                ovrm.ShowHandleBody(false, ikparent);
            }

            Quaternion CmnRotation = Quaternion.Euler(new Vector3(0, 180, 0));
            Quaternion CmdZeroRotation = Quaternion.Euler(Vector3.zero);

            avatar_olvrm.relatedHandleParent = ikparent;
            UserGroundOperation ugo = ikparent.GetComponent<UserGroundOperation>();
            ugo.SetRelatedAvatar(avatar);

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
            head.transform.rotation = CmdZeroRotation;
            head.tag = "IKHandle";
            head.GetComponent<UserHandleOperation>().PartsName = "head";
            head.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 headpos = animator.GetBoneTransform(HumanBodyBones.Head).transform.position;
            head.transform.position = new Vector3(headpos.x, headpos.y, 0f);
            head.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copylookat = (GameObject)Resources.Load("IKHandleSphereHead");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject lookat = Instantiate(copylookat, copylookat.transform.position, Quaternion.identity, ikparent.transform);
            lookat.name = "LookAt";// + avatar.name;
            lookat.transform.rotation = CmnRotation;
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
            aim.transform.rotation = CmdZeroRotation; // upperchesttrans.transform.rotation;
            aim.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copychest = (GameObject)Resources.Load("IKHandleSphereChest");
            GameObject upperchest = Instantiate(copychest, copychest.transform.position, Quaternion.identity, ikparent.transform);
            upperchest.name = "Chest";
            upperchest.tag = "IKHandle";
            upperchest.GetComponent<UserHandleOperation>().PartsName = "chest";
            upperchest.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            upperchest.transform.position = upperchesttrans.transform.position;
            upperchest.transform.rotation = Quaternion.Euler(Vector3.zero);
            upperchest.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copypelvis = (GameObject)Resources.Load("IKHandleSpherePelvis");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject pelvis = Instantiate(copypelvis, copypelvis.transform.position, Quaternion.identity, ikparent.transform);
            pelvis.name = "Pelvis";// + avatar.name;
            Vector3 vrot = new Vector3(0f, 180f, 0f);
            Quaternion rot = CmnRotation;
            pelvis.transform.rotation = rot;
            pelvis.tag = "IKHandle";
            pelvis.GetComponent<UserHandleOperation>().PartsName = "pelvis";
            pelvis.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            pelvis.transform.position = animator.GetBoneTransform(HumanBodyBones.Chest).transform.position;
            pelvis.GetComponent<UserHandleOperation>().SaveDefaultTransform();



            //---Hand, Arm
            Vector3 animLeftShoulderPos = animator.GetBoneTransform(HumanBodyBones.LeftShoulder).transform.position;
            GameObject copyleftshoulder = (GameObject)Resources.Load("IKHandleTriangleLeft");
            GameObject leftshoulder = Instantiate(copyleftshoulder, copyleftshoulder.transform.position, Quaternion.identity, upperchest.transform);
            leftshoulder.name = "LeftShoulder";
            leftshoulder.transform.rotation = CmdZeroRotation;
            //leftshoulder.transform.localRotation = Quaternion.Euler(0f, 4.765f, 1.158f);
            leftshoulder.transform.localScale = new Vector3(1f, 1f, 1f);
            UserHandleOperation leftsld = leftshoulder.GetComponent<UserHandleOperation>();
            leftsld.PartsName = "leftshoulder";
            leftsld.SetRelatedAvatar(avatar);
            leftshoulder.transform.position = new Vector3(
                (animLeftShoulderPos.x) * 1f,
                upperchest.transform.position.y,
                animLeftShoulderPos.z
            );
            leftsld.SaveDefaultTransform();
            ugo.LeftShoulderIK = leftshoulder.transform;


            GameObject copyleftlowerarm = (GameObject)Resources.Load("IKHandleSphereLeft");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject leftlowerarm = Instantiate(copyleftlowerarm, copyleftlowerarm.transform.position, Quaternion.identity, ikparent.transform);
            leftlowerarm.name = "LeftLowerArm";// + avatar.name;
            leftlowerarm.transform.rotation = CmnRotation;
            leftlowerarm.tag = "IKHandle";
            leftlowerarm.GetComponent<UserHandleOperation>().PartsName = "leftlowerarm";
            leftlowerarm.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 animLeftLowerArmPos = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).transform.position;
            animLeftLowerArmPos.z = 0.01f;
            animLeftLowerArmPos.x *= 1f;
            leftlowerarm.transform.position = animLeftLowerArmPos;
            leftlowerarm.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copylefthand = (GameObject)Resources.Load("IKHandleSphereLeft");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject lefthand = Instantiate(copylefthand, copylefthand.transform.position, Quaternion.identity, ikparent.transform);
            lefthand.name = "LeftHand";// + avatar.name;
            lefthand.transform.rotation = Quaternion.Euler(new Vector3(0, 90f, 0));
            lefthand.tag = "IKHandle";
            lefthand.GetComponent<UserHandleOperation>().PartsName = "leftarm";
            lefthand.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 animLeftHandPos = animator.GetBoneTransform(HumanBodyBones.LeftHand).transform.position;
            animLeftHandPos.x *= -1f;
            lefthand.transform.position = animLeftHandPos;
            lefthand.GetComponent<UserHandleOperation>().SaveDefaultTransform();



            GameObject copyrightshoulder = (GameObject)Resources.Load("IKHandleTriangleRight");
            GameObject rightshoulder = Instantiate(copyrightshoulder, copyrightshoulder.transform.position, Quaternion.identity, upperchest.transform);
            rightshoulder.name = "RightShoulder";
            rightshoulder.transform.rotation = CmdZeroRotation;
            //rightshoulder.transform.localRotation = Quaternion.Euler(0f, -4.765f, -1.158f);
            rightshoulder.transform.localScale = new Vector3(1f, 1f, 1f);
            Vector3 animRightShoulderPos = animator.GetBoneTransform(HumanBodyBones.RightShoulder).transform.position;
            UserHandleOperation rightsld = rightshoulder.GetComponent<UserHandleOperation>();
            rightsld.PartsName = "rightshoulder";
            rightsld.SetRelatedAvatar(avatar);
            rightshoulder.transform.position = new Vector3(
                (animRightShoulderPos.x) * 1f,
                upperchest.transform.position.y,
                animRightShoulderPos.z
            );
            rightsld.SaveDefaultTransform();
            ugo.RightShoulderIK = rightshoulder.transform;


            GameObject copyrightlowerarm = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject rightlowerarm = Instantiate(copyrightlowerarm, copyrightlowerarm.transform.position, Quaternion.identity, ikparent.transform);
            rightlowerarm.name = "RightLowerArm";// + avatar.name;
            rightlowerarm.transform.rotation = CmnRotation;
            rightlowerarm.tag = "IKHandle";
            rightlowerarm.GetComponent<UserHandleOperation>().PartsName = "rightlowerarm";
            rightlowerarm.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 animRightLowerArmPos = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).transform.position;
            animRightLowerArmPos.z = 0.01f;
            animRightLowerArmPos.x *= 1f;
            rightlowerarm.transform.position = animRightLowerArmPos;
            rightlowerarm.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copyrighthand = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject righthand = Instantiate(copyrighthand, copyrighthand.transform.position, Quaternion.identity, ikparent.transform);
            righthand.name = "RightHand";// + avatar.name;
            righthand.transform.rotation = Quaternion.Euler(new Vector3(0, -90f, 0));
            righthand.tag = "IKHandle";
            righthand.GetComponent<UserHandleOperation>().PartsName = "rightarm";
            righthand.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);

            Vector3 animRightHandPos = animator.GetBoneTransform(HumanBodyBones.RightHand).transform.position;
            animRightHandPos.x *= 1f;
            righthand.transform.position = animRightHandPos;
            righthand.GetComponent<UserHandleOperation>().SaveDefaultTransform();



            //---Leg
            GameObject copyleftlowerleg = (GameObject)Resources.Load("IKHandleSphereLeft");
            GameObject leftlowerleg = Instantiate(copyleftlowerleg, copyleftlowerleg.transform.position, Quaternion.identity, ikparent.transform);
            leftlowerleg.name = "LeftLowerLeg"; // + avatar.name;
            leftlowerleg.transform.rotation = CmnRotation;
            leftlowerleg.tag = "IKHandle";
            leftlowerleg.GetComponent<UserHandleOperation>().PartsName = "leftlowerleg";
            leftlowerleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 animLeftLowerLegPos = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).transform.position;
            animLeftLowerLegPos.x *= -1;
            leftlowerleg.transform.position = animLeftLowerLegPos;
            leftlowerleg.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copyleftleg = (GameObject)Resources.Load("IKHandleSphereLeft");
            GameObject leftleg = Instantiate(copyleftleg, copyleftleg.transform.position, Quaternion.identity, ikparent.transform);
            leftleg.name = "LeftLeg"; // + avatar.name;
            leftleg.transform.rotation = CmnRotation;
            leftleg.tag = "IKHandle";
            leftleg.GetComponent<UserHandleOperation>().PartsName = "leftleg";
            leftleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 animLeftFootPos = animator.GetBoneTransform(HumanBodyBones.LeftFoot).transform.position;
            animLeftFootPos.x *= 1f;
            leftleg.transform.position = animLeftFootPos;
            leftleg.GetComponent<UserHandleOperation>().SaveDefaultTransform();



            GameObject copyrightlowerleg = (GameObject)Resources.Load("IKHandleSphereRight");
            GameObject rightlowerleg = Instantiate(copyrightlowerleg, copyrightlowerleg.transform.position, Quaternion.identity, ikparent.transform);
            rightlowerleg.name = "RightLowerLeg"; // + avatar.name;
            rightlowerleg.transform.rotation = CmnRotation;
            rightlowerleg.tag = "IKHandle";
            rightlowerleg.GetComponent<UserHandleOperation>().PartsName = "rightlowerleg";
            rightlowerleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 animRightLowerLegPos = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).transform.position;
            animRightLowerLegPos.x *= 1f;
            rightlowerleg.transform.position = animRightLowerLegPos;
            rightlowerleg.GetComponent<UserHandleOperation>().SaveDefaultTransform();

            GameObject copyrightleg = (GameObject)Resources.Load("IKHandleSphereRight");  //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            GameObject rightleg = Instantiate(copyrightleg, copyrightleg.transform.position, Quaternion.identity, ikparent.transform);
            rightleg.name = "RightLeg";// + avatar.name;
            rightleg.transform.rotation = CmnRotation;
            rightleg.tag = "IKHandle";
            rightleg.GetComponent<UserHandleOperation>().PartsName = "rightleg";
            rightleg.GetComponent<UserHandleOperation>().SetRelatedAvatar(avatar);
            Vector3 animRightFootPos = animator.GetBoneTransform(HumanBodyBones.RightFoot).transform.position;
            animRightFootPos.x *= 1f;
            rightleg.transform.position = animRightFootPos;
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
            Camera FrontMainCamera = GameObject.Find("FrontMainCamera").GetComponent<Camera>();

            //GameObject campar = new GameObject();
            GameObject copycam;
            copycam = (GameObject)Resources.Load("UserCamera");
            GameObject ikcopycam = (GameObject)Resources.Load("IKHandleCamera");

            GameObject[] ret = new GameObject[2];
            ret[0] = Instantiate(copycam, copycam.transform.position, Quaternion.identity, animarea.transform);
            ret[1] = Instantiate(ikcopycam, ikcopycam.transform.position, Quaternion.identity, ikhp.transform);

            /*Camera adcam = ret[0].AddComponent<Camera>();
            adcam.clearFlags = CameraClearFlags.SolidColor;
            adcam.fieldOfView = 30f;
            adcam.farClipPlane = 100f;
            adcam.nearClipPlane = 0.01f;
            adcam.depth = 11;
            adcam.cullingMask = FrontMainCamera.cullingMask;
            adcam.enabled = false;
            */

            //---connect camera transform to Post-process-layer
            PostProcessLayer ppl = ret[0].GetComponent<PostProcessLayer>();
            //ppl.volumeLayer ^= 1 << LayerMask.NameToLayer("SystemEffect");
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
