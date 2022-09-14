using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RootMotion.FinalIK;
using VRM;
using DG.Tweening;

namespace UserHandleSpace
{

    /// <summary>
    /// Attach for IKHandleParent(parent of the ik handle game object)
    /// </summary>
    public class OperateActiveVRM : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void ChangeActiveAvatarSelection(string name, string type);

        [DllImport("__Internal")]
        private static extern void AfterActivateAvatar(string name);

        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);

        public GameObject ActiveAvatar;
        public GameObject ActiveIKHandle;
        public SkinnedMeshRenderer ActiveFace;
        [SerializeField] public GameObject OldActiveAvatar;
        public GameObject OldActiveIKHandle;
        public string ActivePartsName;
        public AF_TARGETTYPE ActiveType;
        public AF_TARGETTYPE OldActiveType;
        public bool isOtherObjectSingle;
        public bool isMoveMode;
        public bool isEquipMode;
        public GameObject EquippingAvatar;
        public string EquippingPartsName;
        public Dropdown BoxActivateAvatar;

        private Vector3 oldPosition;
        private Quaternion oldRotation;
        private Vector3 defaultPosition;
        private Quaternion defaultRotation;

        private ConfigSettingLabs configLab;
        

        private ManageAnimation manim;
        private AvatarKeyOperator akeyo;

        private List<GameObject> HasAvatarList;


        // Start is called before the first frame update
        void Start()
        {
            GameObject aarea = GameObject.Find("AnimateArea");
            manim = aarea.GetComponent<ManageAnimation>();
            configLab = GameObject.Find("Canvas").GetComponent<ConfigSettingLabs>();

            HasAvatarList = new List<GameObject>();
            HasAvatarList.Add(null);

            //ShowHandleBody(false);
            if (this.ActiveIKHandle) ActivateHandleView(false, this.ActiveIKHandle);

            isMoveMode = false;
            isEquipMode = false;
            isOtherObjectSingle = false;

            oldPosition = this.transform.position;
            oldRotation = this.transform.rotation;

            akeyo = new AvatarKeyOperator(manim.cfg_keymove_speed_rot, manim.cfg_keymove_speed_trans);

            SaveDefaultTransform(true,true);

        }

        // Update is called once per frame
        void Update()
        {
            //---key operation for current selected avatar translation
            if (manim.keyOperationMode == KeyOperationMode.MoveAvatar)
            { //this avatar is active ?
                if (ActiveAvatar != null)
                {
                    akeyo.SetSpeed(manim.cfg_keymove_speed_rot, manim.cfg_keymove_speed_trans);
                    akeyo.CallKeyOperation(ActiveIKHandle);
                }
            }
        }
        private void LateUpdate()
        {
        }
        /*
        public void OnActivateHandHandle()
        {
            Debug.Log("Activated Object= " + this.name);
            RotationHandle rhan = GetComponent<RotationHandle>();
            PositionHandle phan = GetComponent<PositionHandle>();
            Debug.Log(rhan.enabled);
            Debug.Log(phan.enabled);
            if (rhan)
            {
                rhan.enabled = true;
            }
            if (phan)
            {
                phan.enabled = true;
            }

        }
        */
        public void SetActiveFace()
        {
            ManageAvatarTransform mat = ActiveAvatar.GetComponent<ManageAvatarTransform>();
            ActiveFace = mat.GetFaceMesh().GetComponent<SkinnedMeshRenderer>();
            /*
            int cnt = ActiveAvatar.transform.childCount;
            SkinnedMeshRenderer mesh = null;
            for (int i = 0; i < cnt; i++)
            {
                mesh = ActiveAvatar.transform.GetChild(i).GetComponent<SkinnedMeshRenderer>();
                if (mesh != null)
                {
                    if (mesh.sharedMesh.blendShapeCount > 0)
                    {
                        ActiveFace = mesh;
                    }
                }
            }
            */
        }
        public GameObject GetEffectiveActiveAvatar()
        {
            if (ActiveType == AF_TARGETTYPE.VRM)
            {
                return ActiveAvatar;
            }
            else if (ActiveType == AF_TARGETTYPE.OtherObject)
            {
                return ActiveAvatar;
                /*
                if (isOtherObjectSingle)
                {
                    return ActiveAvatar;
                }
                else
                {
                    return ActiveAvatar.transform.parent.gameObject;
                }
                */
            }
            else
            {
                return ActiveAvatar;
            }
        }
        public AF_TARGETTYPE ConvertTag2Type(GameObject avatar)
        {
            AF_TARGETTYPE ret = AF_TARGETTYPE.Unknown;

            if (avatar.tag == "Player") { ret = AF_TARGETTYPE.VRM; }
            else if (avatar.tag == "OtherPlayer") { ret = AF_TARGETTYPE.OtherObject;  }
            else if (avatar.tag == "CameraPlayer") { ret = AF_TARGETTYPE.Camera; }
            else if (avatar.tag == "LightPlayer") { ret = AF_TARGETTYPE.Light; }
            else if (avatar.tag == "EffectDestination") { ret = AF_TARGETTYPE.Effect; }

            return ret;
        }


        /// <summary>
        /// Full body of the activate avatar function (NOT USE)
        /// </summary>
        /// <param name="hitObject"></param>
        /// <param name="isWebGL"></param>
        public void _ActivateAvatarBody(GameObject hitObject, bool isWebGL = true)
        {
            bool isEquipThisItem = _IsEquipped(hitObject.tag, hitObject);

            if (ActiveAvatar)
            { //---Preparing to enable the current object(futurely old active)
                ManageAvatarTransform mat = GetEffectiveActiveAvatar().GetComponent<ManageAvatarTransform>();
                if (ActiveType == AF_TARGETTYPE.VRM)
                {
                    if (GetEffectiveActiveAvatar().GetComponent<OperateLoadedVRM>().GetFixMoving() != 1)
                    {
                        CapsuleCollider capc;
                        if (GetEffectiveActiveAvatar().TryGetComponent<CapsuleCollider>(out capc))
                        {
                            capc.enabled = true;
                        }
                        //---hide IKHandles of all objects OTHER THAN this time object
                        ActivateHandleView(false, ActiveIKHandle);
                        if (isMoveMode)
                        {
                            if (ActiveIKHandle) ShowHandleBody(false, ActiveIKHandle);
                        }

                        //---change updateWhenOffscreen flag
                        List<GameObject> meshcnt = mat.CheckSkinnedMeshAvailable();
                        /*if (meshcnt.Count == 1)
                        {
                            meshcnt[0].GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = false;
                        }
                        else
                        {
                            GameObject face = mat.GetFaceMesh();
                            if (face != null) face.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = false;
                            GameObject body = mat.GetBodyMesh();
                            if (body != null) body.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = false;
                            GameObject hair = mat.GetHairMesh();
                            if (hair != null) hair.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = false;

                        }*/

                    }
                }
                else if (ActiveType == AF_TARGETTYPE.OtherObject)
                {
                    if (GetEffectiveActiveAvatar().GetComponent<OperateLoadedOther>().GetFixMoving() != 1)
                    {
                        BoxCollider boxc;
                        if (GetEffectiveActiveAvatar().TryGetComponent<BoxCollider>(out boxc))
                        {
                            boxc.enabled = true;
                        }
                        //---hide IKHandles of all objects OTHER THAN this time object
                        ActiveIKHandle.SetActive(false);

                    }
                }
                else if (ActiveType == AF_TARGETTYPE.Image)
                {
                    if (GetEffectiveActiveAvatar().GetComponent<OperateLoadedOther>().GetFixMoving() != 1)
                    {
                        BoxCollider boxc;
                        if (GetEffectiveActiveAvatar().TryGetComponent<BoxCollider>(out boxc))
                        {
                            boxc.enabled = true;
                        }
                        //---hide IKHandles of all objects OTHER THAN this time object
                        ActiveIKHandle.SetActive(false);

                    }
                }
                else if (ActiveType == AF_TARGETTYPE.Camera)
                {
                    if (GetEffectiveActiveAvatar().GetComponent<OperateLoadedCamera>().GetFixMoving() != 1)
                    {
                        BoxCollider boxc;
                        if (GetEffectiveActiveAvatar().TryGetComponent<BoxCollider>(out boxc))
                        {
                            boxc.enabled = true;
                        }
                        //---hide IKHandles of all objects OTHER THAN this time object
                        //ActiveIKHandle.SetActive(false);

                    }
                }
                else if (ActiveType == AF_TARGETTYPE.Light)
                {
                    if (GetEffectiveActiveAvatar().GetComponent<OperateLoadedLight>().GetFixMoving() != 1)
                    {
                        BoxCollider boxc;
                        if (GetEffectiveActiveAvatar().TryGetComponent<BoxCollider>(out boxc))
                        {
                            boxc.enabled = true;
                        }
                        //---hide IKHandles of all objects OTHER THAN this time object
                        //ActiveIKHandle.SetActive(false);

                    }
                }
                else if (ActiveType == AF_TARGETTYPE.Effect)
                {
                    if (GetEffectiveActiveAvatar().GetComponent<OperateLoadedEffect>().GetFixMoving() != 1)
                    {
                        BoxCollider boxc;
                        if (GetEffectiveActiveAvatar().TryGetComponent<BoxCollider>(out boxc))
                        {
                            boxc.enabled = true;
                        }
                        //---hide IKHandles of all objects OTHER THAN this time object
                        //ActiveIKHandle.SetActive(false);

                    }
                }

                //---save now active object as old selected object
                OldActiveAvatar = ActiveAvatar;
                OldActiveType = ActiveType;
                OldActiveIKHandle = ActiveIKHandle;

            }


            //---activate this avatar and ik handles
            if ((hitObject.tag == "Player") || (hitObject.tag == "SampleData"))
            {
                //---turn off for operating IK Handle
                CapsuleCollider col = hitObject.GetComponent<CapsuleCollider>();
                col.enabled = false;

                OperateLoadedVRM hitolvrm = hitObject.GetComponent<OperateLoadedVRM>();

                if (hitolvrm.relatedHandleParent) ActivateHandleView(true, hitolvrm.relatedHandleParent);
                ActiveAvatar = hitObject;
                ActiveIKHandle = hitolvrm.relatedHandleParent;
                SetActiveFace();
                //ActivateAvatar("VRM");
                ActiveType = AF_TARGETTYPE.VRM;
                hitolvrm.isMoveMode = isMoveMode;

                if (isMoveMode)
                {
                    if (ActiveIKHandle) ShowHandleBody(true, ActiveIKHandle);
                }
                //---change updateWhenOffscreen flag
                ManageAvatarTransform mat = GetEffectiveActiveAvatar().GetComponent<ManageAvatarTransform>();
                List<GameObject> meshcnt = mat.CheckSkinnedMeshAvailable();
                if (meshcnt.Count == 1)
                {
                    meshcnt[0].GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                }
                else
                {
                    GameObject face = mat.GetFaceMesh();
                    if (face != null) face.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                    GameObject body = mat.GetBodyMesh();
                    if (body != null) body.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                    GameObject hair = mat.GetHairMesh();
                    if (hair != null) hair.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                }

                //---Return VRM avatar name
#if !UNITY_EDITOR && UNITY_WEBGL
            if (isWebGL) ChangeActiveAvatarSelection(ActiveAvatar.name, "VRM");
#endif

            }
            else if ((hitObject.tag == "OtherPlayerCollider"))
            {
                OperateLoadedOther hitoloth = hitObject.transform.parent.GetComponent<OperateLoadedOther>();
                if (!isEquipThisItem) hitoloth.relatedHandleParent.SetActive(true);

                //---set up current hit object
                ActiveAvatar = hitObject; //.transform.parent.gameObject;
                ActiveIKHandle = hitoloth.relatedHandleParent;
                ActiveFace = null;
                isOtherObjectSingle = false;
                //ChangeTargetOtherObjectHandleState(hitObject, 1);
                //ActivateAvatar(hitoloth.objectType);
                ActiveType = AF_TARGETTYPE.OtherObject;

                //---Return the Other Object name (because hit object is child)

#if !UNITY_EDITOR && UNITY_WEBGL
            if (isWebGL) ChangeActiveAvatarSelection(GetEffectiveActiveAvatar().name, hitoloth.objectType);
#endif


            }
            else if (hitObject.tag == "OtherPlayer")
            {
                //---select item on object list of HTML (always parent item (real own)
                OperateLoadedOther hitoloth = hitObject.GetComponent<OperateLoadedOther>();
                if (!isEquipThisItem) hitoloth.relatedHandleParent.SetActive(true);

                //---set up current hit object
                ActiveAvatar = hitObject; //.transform.GetChild(0).gameObject; //.transform.parent.gameObject;
                ActiveIKHandle = hitoloth.relatedHandleParent;
                ActiveFace = null;
                isOtherObjectSingle = true;
                //ChangeTargetOtherObjectHandleState(hitObject, 1);
                //ActivateAvatar(hitoloth.objectType);
                ActiveType = AF_TARGETTYPE.OtherObject;

                //---Return the Other Object name (because hit object is child)
#if !UNITY_EDITOR && UNITY_WEBGL
            if (isWebGL) ChangeActiveAvatarSelection(GetEffectiveActiveAvatar().name, hitoloth.objectType);
#endif

            }
            else if (hitObject.tag == "CameraPlayer")
            {
                //---turn off for operating IK Handle
                BoxCollider col = hitObject.GetComponent<BoxCollider>();
                if (!isEquipThisItem) col.enabled = false;

                OperateLoadedCamera hitoll = hitObject.GetComponent<OperateLoadedCamera>();
                if (!isEquipThisItem) hitoll.relatedHandleParent.SetActive(true);

                //---set up current hit object
                ActiveAvatar = hitObject;
                ActiveIKHandle = hitoll.relatedHandleParent;
                ActiveFace = null;
                ActiveType = AF_TARGETTYPE.Camera;
#if !UNITY_EDITOR && UNITY_WEBGL
            if (isWebGL) ChangeActiveAvatarSelection(ActiveAvatar.name, "Camera");
#endif
            }
            else if (hitObject.tag == "LightPlayer")
            {
                //---turn off for operating IK Handle
                BoxCollider col = hitObject.GetComponent<BoxCollider>();
                if (!isEquipThisItem) col.enabled = false;

                OperateLoadedLight hitoll = hitObject.GetComponent<OperateLoadedLight>();
                if (!isEquipThisItem) hitoll.relatedHandleParent.SetActive(true);

                //---set up current hit object
                ActiveAvatar = hitObject;
                ActiveIKHandle = hitoll.relatedHandleParent;
                ActiveFace = null;
                ActiveType = AF_TARGETTYPE.Light;
#if !UNITY_EDITOR && UNITY_WEBGL
            if (isWebGL) ChangeActiveAvatarSelection(ActiveAvatar.name, "Light");
#endif
            }
            else if (hitObject.tag == "EffectDestination")
            {
                //---turn off for operating IK Handle
                BoxCollider col = hitObject.GetComponent<BoxCollider>();
                if (!isEquipThisItem) col.enabled = false;

                OperateLoadedEffect hitoll = hitObject.GetComponent<OperateLoadedEffect>();
                if (!isEquipThisItem) hitoll.relatedHandleParent.SetActive(true);

                //---set up current hit object
                ActiveAvatar = hitObject;
                ActiveIKHandle = hitoll.relatedHandleParent;
                ActiveFace = null;
                ActiveType = AF_TARGETTYPE.Effect;
#if !UNITY_EDITOR && UNITY_WEBGL
            if (isWebGL) ChangeActiveAvatarSelection(ActiveAvatar.name, "Effect");
#endif
            }

        }
        private bool _IsEquipped(string tag, GameObject item)
        {
            bool isEquip = false;

            OtherObjectDummyIK ooik;
            if (tag != "Player")
            {
                if (tag == "OtherPlayerCollider")
                {
                    //---collider child object to parent object
                    GameObject ikhandle = item.transform.parent.GetComponent<OperateLoadedOther>().relatedHandleParent;
                    if (ikhandle.TryGetComponent<OtherObjectDummyIK>(out ooik))
                    {
                        isEquip = ooik.isEquipping;

                    }
                }
                else
                {
                    //---collider child object to parent object
                    GameObject ikhandle = item.transform.GetComponent<OperateLoadedBase>().relatedHandleParent;
                    if (ikhandle.TryGetComponent<OtherObjectDummyIK>(out ooik))
                    {
                        isEquip = ooik.isEquipping;
                    }
                }
            }
            return isEquip;
        }

        /// <summary>
        /// Body of ActivateAvatar
        /// </summary>
        /// <param name="tag">tag of target GameObject</param>
        /// <param name="name">target GameObject name</param>
        /// <param name="isWebGL"></param>
        /// <returns></returns>
        private bool _ActivateAvatarFunction(string tag, string name, bool isWebGL)
        {
            bool ret = false;
            GameObject[] vrm = GameObject.FindGameObjectsWithTag(tag);
            for (var i = 0; i < vrm.Length; i++)
            {
                bool ishit = false;
                if (tag == "OtherPlayerCollider")
                {
                    if (vrm[i].transform.parent.name == name) ishit = true;
                }
                else
                {
                    if (vrm[i].name == name) ishit = true;
                }

                //---to activate hitted object
                if (ishit)
                {
                    //---equipping item
                    bool isEquip = false;
                    OtherObjectDummyIK ooik;
                    if (tag != "Player")
                    {
                        if (tag == "OtherPlayerCollider")
                        {
                            //---collider child object to parent object
                            GameObject ikhandle = vrm[i].transform.parent.GetComponent<OperateLoadedOther>().relatedHandleParent;
                            if (ikhandle.TryGetComponent<OtherObjectDummyIK>(out ooik))
                            {
                                isEquip = ooik.isEquipping;

                            }
                        }
                        else
                        {
                            //---collider child object to parent object
                            GameObject ikhandle = vrm[i].transform.GetComponent<OperateLoadedBase>().relatedHandleParent;
                            if (ikhandle.TryGetComponent<OtherObjectDummyIK>(out ooik))
                            {
                                isEquip = ooik.isEquipping;
                            }
                        }
                    }

                    if (!isEquip)
                    {
                        _ActivateAvatarBody(vrm[i], isWebGL);
                    }
                    else
                    {
                        //---if object is equipped by other avatar, show property only (no selection mode)
                        OperateLoadedBase olb = vrm[i].GetComponent<OperateLoadedBase>();
#if !UNITY_EDITOR && UNITY_WEBGL
                        //if (isWebGL) AfterActivateAvatar(vrm[i].name);
                        if (isWebGL) {
                            ChangeActiveAvatarSelection(GetEffectiveActiveAvatar().name, olb.objectType);
                        }
#endif

                    }
                    ret = true;
                    break;
                    

                }
            }
            return ret;
        }

        /// <summary>
        /// To activate specified avatar object
        /// </summary>
        /// <param name="param">target GameObject name</param>
        /// <param name="isWebGL"></param>
        public void ActivateAvatar(string param, bool isWebGL)
        {
            //string name = ActiveAvatar.name;
            bool ret = false;
            ret = _ActivateAvatarFunction("Player", param, isWebGL);
            if (!ret)
            {
                ret = _ActivateAvatarFunction("OtherPlayer", param, isWebGL);
                if (!ret)
                {
                    ret = _ActivateAvatarFunction("LightPlayer", param, isWebGL);
                    if (!ret)
                    {
                        ret = _ActivateAvatarFunction("CameraPlayer", param, isWebGL);
                        if (!ret)
                        {
                            ret = _ActivateAvatarFunction("EffectDestination", param, isWebGL);
                        }
                    }
                }
            }

        }
        public void ActivateAvatarFromOuter(string param)
        {
            ActivateAvatar(param, true);

        }

        //  Change Mode "MOVE" <-> "IK"  (WebGL version)
        public void ChangeMoveModeFromOuter(int flag) //***call from HTML
        {

            bool tog = (flag == 1 ? true : false);
            isMoveMode = tog;

            OperateLoadedVRM ovrm = null;
            if (!ActiveAvatar.TryGetComponent<OperateLoadedVRM>(out ovrm)) return;
            ovrm.isMoveMode = isMoveMode;

            //---bone limited asist is enabled when only not Move-mode
            manim.IsBoneLimited = !isMoveMode;

            ShowHandleBody(tog, ActiveIKHandle);

        }
        //  change Mode "EQUIP" <-> "MOVE","IK"
        public void ChangeEquipModeFromOuter(int flag)
        {
            bool tog = (flag == 1 ? true : false);
            isEquipMode = tog;


        }
        //========================================================================================================================
        //  New Activate methods: using Layer "Handle", "HiddenHandle"
        public void AddAvatarBox(string name, GameObject avatar, GameObject ikparent)
        {
#if UNITY_EDITOR || UNITY_STANDALONE

            //Dropdown.OptionData opt = new Dropdown.OptionData();
            //opt.text = name;

            HasAvatarList.Add(ikparent);
            //BoxActivateAvatar.options.Add(opt);

            GameObject newui = GameObject.Find("newUI");
            if (newui != null)
            {
                UserUISpace.UserUIManager uuim = newui.GetComponent<UserUISpace.UserUIManager>();
                uuim.objlist_add_item(name);
            }
            
#endif
        }
        public void RemoveAvatarBox(GameObject ikparent)
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            int index = HasAvatarList.FindIndex(match =>
            {
                if (match == ikparent) return true;
                return false;
            });
            if (index > -1)
            {
                //BoxActivateAvatar.options.RemoveAt(index);
                HasAvatarList.RemoveAt(index);

                GameObject newui = GameObject.Find("newUI");
                if (newui != null)
                {
                    UserUISpace.UserUIManager uuim = GameObject.Find("newUI").GetComponent<UserUISpace.UserUIManager>();
                    uuim.objlist_del_item(index);
                }
                
            }
#endif

        }
        public void ChangeEnableAvatar(int index)
        {
            if ((index >= HasAvatarList.Count) || (HasAvatarList[index] == null)) return;

            EnableTransactionHandle(null, HasAvatarList[index]);
        }
        public void ChangeEnableAvatarFromOuter(string name)
        {
            NativeAnimationAvatar nav = manim.GetCastByAvatar(name);
            if (nav == null) return;

            EnableTransactionHandle(null, nav.ikparent);
        }
        public void EnableHandle_Avatar(GameObject ikparent, AF_TARGETTYPE type = AF_TARGETTYPE.Unknown)
        {
            if (ikparent == null)
            {
                ActiveAvatar = null;
                ActiveIKHandle = null;
                ActiveFace = null;
                return;
            }
            const string TARGETLAYER = "Handle";
            ikparent.layer = LayerMask.NameToLayer(TARGETLAYER);

            Transform[] bts = ikparent.GetComponentsInChildren<Transform>();
            int cnt = bts.Length; // ikparent.transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                GameObject child = null; // ikparent.transform.GetChild(i).gameObject;
                child = bts[i].gameObject;
                child.layer = LayerMask.NameToLayer(TARGETLAYER);
            }

            
            UserGroundOperation ugo;
            OtherObjectDummyIK dik;

            ActiveIKHandle = ikparent;
            if (ikparent.TryGetComponent<UserGroundOperation>(out ugo))
            { //---VRM
                ActiveAvatar = ugo.relatedAvatar;
                ActiveType = ConvertTag2Type(ugo.relatedAvatar);
                SetActiveFace();

                //---Change enable/disable the ground handle.
                OperateLoadedVRM hitolvrm = ActiveAvatar.GetComponent<OperateLoadedVRM>();
                hitolvrm.ChangeToggleAvatarMoveFromOuter(-1);
                /*hitolvrm.isMoveMode = isMoveMode;
                if (isMoveMode)
                {
                    if (ActiveIKHandle) ShowHandleBody(true, ActiveIKHandle);
                }*/
                //---change updateWhenOffscreen flag
                ManageAvatarTransform mat = GetEffectiveActiveAvatar().GetComponent<ManageAvatarTransform>();
                List<GameObject> meshcnt = mat.CheckSkinnedMeshAvailable();
                if (meshcnt.Count == 1)
                {
                    meshcnt[0].GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                }
                else
                {
                    GameObject face = mat.GetFaceMesh();
                    if (face != null) face.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                    GameObject body = mat.GetBodyMesh();
                    if (body != null) body.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                    GameObject hair = mat.GetHairMesh();
                    if (hair != null) hair.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                }
            }
            else if (ikparent.TryGetComponent<OtherObjectDummyIK>(out dik))
            { //---OtherObject, etc
                ActiveAvatar = dik.relatedAvatar;
                ActiveType = ConvertTag2Type(dik.relatedAvatar);
                ActiveFace = null;
            }

            //---test: change position of ViewPointObj
            /*
            if (configLab.GetIntVal("focus_camera_onselect") == 1)
            {
                Camera.main.gameObject.GetComponent<CameraOperation1>().FocusCameraToVRMFromOuter(ActiveAvatar.name);
            }
            */

        }
        public void DisableHandle_Avatar(GameObject ikparent, AF_TARGETTYPE type = AF_TARGETTYPE.Unknown)
        {
            const string TARGETLAYER = "HiddenHandle";
            if (ikparent.name.ToLower() != "dlightik")
            {
                ikparent.layer = LayerMask.NameToLayer(TARGETLAYER);
            }

            Transform[] bts = ikparent.GetComponentsInChildren<Transform>();
            int cnt = bts.Length; //ikparent.transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                GameObject child = null;
                child = bts[i].gameObject; // ikparent.transform.GetChild(i).gameObject;
                if (child.name.ToLower() != "dlightik")
                {
                    child.layer = LayerMask.NameToLayer(TARGETLAYER);
                }
            }
        }

        /// <summary>
        /// Change Enable/Disable the handle exlusively.
        /// </summary>
        /// <param name="avatar"></param>
        /// <param name="ikparent">IK-parent, want enable</param>
        /// <param name="type"></param>
        public void EnableTransactionHandle(GameObject avatar, GameObject ikparent, AF_TARGETTYPE type = AF_TARGETTYPE.Unknown)
        {
            Transform[] bts = gameObject.GetComponentsInChildren<Transform>();

            int cnt = bts.Length; // gameObject.transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                GameObject child = null;// gameObject.transform.GetChild(i).gameObject;
                child = bts[i].gameObject;

                DisableHandle_Avatar(child);
            }
            //---check equipping item
            bool isEquip = false;
            OtherObjectDummyIK ooik;
            if (ikparent.TryGetComponent(out ooik))
            {
                isEquip = ooik.isEquipping;
            }

            if (!isEquip)
            {
                //---save now active object as old selected object
                OldActiveAvatar = ActiveAvatar;
                OldActiveType = ActiveType;
                OldActiveIKHandle = ActiveIKHandle;

                EnableHandle_Avatar(ikparent);
            }
            
        }
        //========================================================================================================================
        //  Backup and Restore methods
        private void SaveDefaultTransform(bool ispos, bool isrotate)
        {
            if (ispos) defaultPosition = transform.position;
            if (isrotate) defaultRotation = transform.rotation;
        }
        private void LoadDefaultTransform(bool ispos, bool isrotate)
        {
            if (ispos) transform.position = defaultPosition;
            if (isrotate) transform.rotation = defaultRotation;
        }



        //=============================================================================================================***
        // Manage the IK handle 
        public Vector3 GetPositionFromOuter()
        {
            Vector3 ret;
            ret = ActiveAvatar.transform.position;

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(JsonUtility.ToJson(ret));
#endif

            return ret;
        }
        public void SetPositionFromOuter(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;
            bool isabs = param[3] == '1' ? true : false;

            if ((ActiveAvatar.tag == "Player") || (ActiveAvatar.tag == "SampleData") || (ActiveAvatar.tag == "LightPlayer") || (ActiveAvatar.tag == "CameraPlayer") || (ActiveAvatar.tag == "EffectDestination"))
            {
                ActiveIKHandle.transform.DOLocalMove(new Vector3(x, y, z), 0.2f).SetRelative(!isabs);
            }
            else if (ActiveAvatar.tag == "OtherPlayer")
            {
                ActiveAvatar.transform.DOLocalMove(new Vector3(x, y, z), 0.2f).SetRelative(!isabs);
            }
        }
        public Vector3 GetRotationFromOuter()
        {
            Vector3 ret;
            ret = ActiveAvatar.transform.rotation.eulerAngles;

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(JsonUtility.ToJson(ret));
#endif

            return ret;

        }
        public void SetRotationFromOuter(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;
            bool isabs = param[3] == '1' ? true : false;

            //if ((ActiveAvatar.tag == "Player") || (ActiveAvatar.tag == "SampleData") )
            //{
            //ActiveIKHandle.transform.DOLocalRotate(new Vector3(x, y, z), 0.2f).SetRelative(!isabs);
            ActiveIKHandle.transform.rotation = Quaternion.Euler(new Vector3(x, y, z));
            /*}
            else if (ActiveAvatar.tag == "OtherPlayer")
            {
                ActiveIKHandle.transform.DOLocalRotate(new Vector3(x, y, z), 0.2f).SetRelative(!isabs);
            }*/
        }
        public void SetScale(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;

            if ((GetEffectiveActiveAvatar().tag == "Player") || (GetEffectiveActiveAvatar().tag == "SampleData"))
            {
                ActiveAvatar.transform.DOScale(new Vector3(x, y, z), 0.2f);
                //ActiveIKHandle.transform.DOScale(new Vector3(x, y, z), 0.2f);
            }
            else if (GetEffectiveActiveAvatar().tag == "OtherPlayer")
            {
                GetEffectiveActiveAvatar().transform.DOScale(new Vector3(x, y, z), 0.2f);
                //ActiveIKHandle.transform.DOScale(new Vector3(x, y, z), 0.2f);
            }
        }
        public Vector3 GetScale()
        {
            Vector3 ret = Vector3.zero;
            if ((GetEffectiveActiveAvatar().tag == "Player") || (GetEffectiveActiveAvatar().tag == "SampleData"))
            {
                ret = ActiveAvatar.transform.localScale;
            }
            else if (GetEffectiveActiveAvatar().tag == "OtherPlayer")
            {
                ret = GetEffectiveActiveAvatar().transform.localScale;
            }
            string jstr = JsonUtility.ToJson(ret);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(jstr);
#endif
            return ret;
        }
        /*public void ActivateIKHandle(string parts)
        {
            GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
            int cnt = ikhp.transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                UserHandleOperation uho = ikhp.transform.GetChild(i).GetComponent<UserHandleOperation>();

                if (parts == uho.PartsName)
                {
                    uho.ActivateHandle();
                }
            }
        }*/
        /// <summary>
        /// To activate IK Handle ball for indicated avatar
        /// </summary>
        public void ActivateHandleView(bool flag, GameObject par)
        {
            //GameObject par = GameObject.FindGameObjectWithTag("IKHandleWorld");
            for (int i = 0; i < par.transform.childCount; i++)
            {
                GameObject ch = par.transform.GetChild(i).gameObject;
                ch.SetActive(flag);
            }
            
        }
        ///<summary>
        /// To activate Collider of IK Handle Parent (for Transform Gizmo)
        ///</summary>
        public void ShowHandleBody(bool flag, GameObject ikparent)
        {
            //GameObject[] hans = GameObject.FindGameObjectsWithTag("IKHandle");

            //hans[i].SetActive(false);
            if (ikparent == null) return;

            ikparent.GetComponent<BoxCollider>().enabled = flag;
            
        }
        public void ResetHandleFor(string param)
        {
            int cnt = ActiveIKHandle.transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                GameObject child = ActiveIKHandle.transform.GetChild(i).gameObject;
                if (child.GetComponent<UserHandleOperation>().PartsName == param)
                {
                    child.GetComponent<UserHandleOperation>().LoadDefaultTransform();
                }

            }
        }
        public void ResetAllHandle()
        {
            UserHandleOperation[] bts = ActiveIKHandle.GetComponentsInChildren<UserHandleOperation>();
            foreach (UserHandleOperation bt in bts)
            {
                bt.LoadDefaultTransform();
            }
            /*
            for (int i = 0; i < ActiveIKHandle.transform.childCount; i++)
            {
                GameObject ch = ActiveIKHandle.transform.GetChild(i).gameObject;
                ch.GetComponent<UserHandleOperation>().LoadDefaultTransform();
            }
            */
        }
        public void ResetParentHandlePosition()
        {
            //LoadDefaultTransform(true, false);
            GameObject efv = GetEffectiveActiveAvatar();
            if ((efv.tag == "Player") || (efv.tag == "SampleData"))
            {
                ActiveIKHandle.GetComponent<UserGroundOperation>().LoadDefaultTransform(true, false);
            }
            else if ((efv.tag == "OtherPlayer") || (efv.tag == "LightPlayer") || (efv.tag == "CameraPlayer") || (efv.tag == "EffectDestination") )
            {
                ActiveIKHandle.GetComponent<OtherObjectDummyIK>().LoadDefaultTransform(true, false);
            }

        }
        public void ResetParentHandleRotation()
        {
            //LoadDefaultTransform(false, true);
            GameObject efv = GetEffectiveActiveAvatar();
            if ((efv.tag == "Player") || (efv.tag == "SampleData"))
            {
                ActiveIKHandle.GetComponent<UserGroundOperation>().LoadDefaultTransform(false, true);

            }
            else if (efv.tag == "OtherPlayer")
            {
                ActiveIKHandle.GetComponent<OtherObjectDummyIK>().LoadDefaultTransform(false, true);
            }
            else if (efv.tag == "LightPlayer")
            {
                ActiveIKHandle.GetComponent<OtherObjectDummyIK>().LoadDefaultTransform(false, true);
            }
            else if (efv.tag == "CameraPlayer")
            {
                ActiveIKHandle.GetComponent<OtherObjectDummyIK>().LoadDefaultTransform(false, true);
            }
            else if (efv.tag == "EffectDestination")
            {
                ActiveIKHandle.GetComponent<OtherObjectDummyIK>().LoadDefaultTransform(false, true);
            }
        }
        public void ResetParentScale()
        {
            //LoadDefaultTransform(false, true);
            GameObject efv = GetEffectiveActiveAvatar();
            if ((efv.tag == "Player") || (efv.tag == "SampleData"))
            {
                efv.transform.DOScale(new Vector3(1f, 1f, 1f), 0.1f);
                ActiveIKHandle.transform.DOScale(new Vector3(1f, 1f, 1f), 0.1f);

            }
            else if (efv.tag == "OtherPlayer")
            {
                efv.transform.DOScale(new Vector3(1f, 1f, 1f), 0.1f);
            }
        }
        //===============================================================================================================***
        //  Hand Pose
        public string ListHandPose()
        {
            string ret = "";
            string[] arms = new string[2];
            LeftHandPoseController ctl = ActiveAvatar.GetComponent<LeftHandPoseController>();
            RightHandPoseController ctr = ActiveAvatar.GetComponent<RightHandPoseController>();

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
        /// <param name="param">a,b,c : a = 0(right), 1(left), b = pose type(1-6), c = pose weight</param>
        public void PosingHandFromOuter(string param)
        {
            if (!ActiveAvatar) return;

            //Debug.Log("unity . param=" + param);
            string[] prm = param.Split(',');
            string handtype = prm[0];
            int posetype  = int.TryParse(prm[1], out posetype) ? posetype : 1;
            float value = float.TryParse(prm[2], out value) ? value : 0f;
            //Debug.Log("unity . int=" + posetype);
            //Debug.Log("unity . float=" + value);

            LeftHandPoseController ctl = ActiveAvatar.GetComponent<LeftHandPoseController>();
            RightHandPoseController ctr = ActiveAvatar.GetComponent<RightHandPoseController>();

            if (handtype == "l")
            {
                ctl.ResetPose();
                ctl.SetPose(posetype, value);
            }else
            {
                ctr.ResetPose();
                ctr.SetPose(posetype, value);
            }
        }



        //================================================================================================================***
        //  Blend Shapes

        /// <summary>
        /// Get all blend shapes, the avatar has.
        /// </summary>
        /// <returns>List string Blend shape lis</returns>
        public List<string> ListAvatarBlendShape()
        {
            List<string> ret = new List<string>();
            if (ActiveFace)
            {
                int bscnt = ActiveFace.sharedMesh.blendShapeCount;
                for (int i = 0; i < bscnt; i++)
                {
                    ret.Add(ActiveFace.sharedMesh.GetBlendShapeName(i) + "=" + ActiveFace.GetBlendShapeWeight(i));

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
            string ret = string.Join(",",lst.ToArray());
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
            return ret;
        }
        
        public float getAvatarBlendShape(string param)
        {
            float ret = 0f;
            List<string> lst = ListAvatarBlendShape();
            for (int i = 0; i < lst.Count; i++)
            {
                if (lst[i].IndexOf(param) > -1)
                {
                    ret = ActiveFace.GetBlendShapeWeight(i);
                    break;
                }
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
            return ret;
        }
        public int getAvatarBlendShapeIndex(string name)
        {
            return ActiveFace.sharedMesh.GetBlendShapeIndex(name);
        }
        public void changeAvatarBlendShape(string param)
        {
            string[] prm = param.Split(',');
            int index = int.TryParse(prm[0], out index) ? index : 0;
            float value = float.TryParse(prm[1], out value) ? value : 0f;
            ActiveFace.SetBlendShapeWeight(index, value);
        }
        public void changeAvatarBlendShapeByName(string param)
        {
            string[] prm = param.Split(',');
            string shapename = prm[0];
            float value = float.TryParse(prm[1], out value) ? value : 0f;
            int index = getAvatarBlendShapeIndex(shapename);
            if (index > -1)
            {
                ActiveFace.SetBlendShapeWeight(index, value);
            }
        }



        //================================================================================================================***
        //  Preset Face


        /// <summary>
        /// Get all preset face.
        /// </summary>
        /// <returns>List string Preset face list</returns>
        public List<string> ListPresetFace()
        {
            List<string> ret = new List<string>();
            VRMBlendShapeProxy bs = ActiveAvatar.GetComponent<VRMBlendShapeProxy>();
            /*for (int i = 0; i < bs.BlendShapeAvatar.Clips.Count; i++)
            {
                string ln = (bs.BlendShapeAvatar.Clips[i].BlendShapeName);
                if (bs.BlendShapeAvatar.Clips[i].Values.Length > 0)
                {
                    ln += "," + bs.BlendShapeAvatar.Clips[i].Values[0];
                    ret.Add(ln);
                }
            }*/
            //VRMBlendShapeProxy bs = ActiveAvatar.GetComponent<VRMBlendShapeProxy>();
            IEnumerable<KeyValuePair<BlendShapeKey, float>> bskey = bs.GetValues();
            IEnumerator<KeyValuePair<BlendShapeKey, float>> bselist = bskey.GetEnumerator();
            while (bselist.MoveNext())
            {
                //Debug.Log(bselist.Current.Key + " - " + bselist.Current.Value);
                string ln = bselist.Current.Key + "=" + bselist.Current.Value;
                ret.Add(ln);
            }
            
            return ret;
        }
        public string ListPresetFaceFromOuter()
        {
            string ret = "";
            if (ActiveAvatar)
            {
                List<string> lst = ListPresetFace();
                ret = string.Join(",", lst.ToArray());
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
            return ret;
        }
        ///<summary>
        ///To judge paramater's string and BlendShapePreset enum as String
        ///</summary>
        ///<returns>BlendShapePreset name</returns>
        public BlendShapePreset MatchPresetFace(string param)
        {
            BlendShapePreset hitval = BlendShapePreset.Unknown;
            foreach (BlendShapePreset value in Enum.GetValues(typeof(BlendShapePreset)))
            {
                string name = Enum.GetName(typeof(BlendShapePreset), value);
                if (param == name)
                {
                    hitval = value;
                    break;
                }
            }
            return hitval;
        }
        public float getAvatarPresetFace(string param)
        {
            if (!ActiveAvatar) return -1f;

            float ret = 0f;
            VRMBlendShapeProxy bs = ActiveAvatar.GetComponent<VRMBlendShapeProxy>();

            BlendShapePreset hitval = MatchPresetFace(param);
            
            BlendShapeKey key;
            if (hitval == BlendShapePreset.Unknown)
            {
                key = BlendShapeKey.CreateUnknown(param);
            }
            else
            {
                key = BlendShapeKey.CreateFromPreset(hitval);
            }
            ret = bs.GetValue(key);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
            return ret;
        }
        public void changeAvatarPresetFace(string param)
        {
            if (!ActiveAvatar) return;

            string[] prm = param.Split(',');
            string name = prm[0];
            //int index = int.TryParse(prm[1], out index) ? index : 0;
            float value = float.TryParse(prm[2], out value) ? value : 0f;

            VRMBlendShapeProxy bs = ActiveAvatar.GetComponent<VRMBlendShapeProxy>();
            BlendShapePreset hitval = MatchPresetFace(name);
            BlendShapeKey key;
            if (hitval == BlendShapePreset.Unknown)
            {
                key = BlendShapeKey.CreateUnknown(name);
            }
            else
            {
                key = BlendShapeKey.CreateFromPreset(hitval);
            }

            bs.AccumulateValue(key, value);
            bs.Apply();
        }
        private IEnumerator _ShowAvatar360()
        {
            /*
            ActiveIKHandle.transform.DOLocalRotate(new Vector3(0f, 360f, 0f),5f,RotateMode.FastBeyond360);
            yield return new WaitForSeconds(1f);

            Transform leftleg = ActiveIKHandle.transform.Find("LeftLeg" ).transform;
            Transform rightleg = ActiveIKHandle.transform.Find("RightLeg").transform;

            leftleg.DOLocalMoveZ(1f, 0.025f).SetLoops(-1, LoopType.Yoyo);
            yield return new WaitForSeconds(0.5f);
            rightleg.DOLocalMoveZ(1f, 0.025f).SetLoops(-1, LoopType.Yoyo);
            */
            float moveval = Camera.main.transform.position.z * -1;

            DOTween.Sequence()
            .Append(
                Camera.main.transform.DOLocalMove(new Vector3(moveval, 0f, moveval), 2f).SetRelative()
            )
            .Join(
                Camera.main.transform.DOLookAt(ActiveAvatar.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Chest).position, 0.5f)
            );

            yield return new WaitForSeconds(1f);
        }


        //================================================================================================================***
        //  Manage the animation


        public void SetAnimationMode(int isAnimation)
        {
            Animator anim = ActiveAvatar.GetComponent<Animator>();
            if (isAnimation == 1)
            {
                ActiveAvatar.GetComponent<VRIK>().enabled = false;
            }
            else
            {
                ActiveAvatar.GetComponent<VRIK>().enabled = true;
            }
        }
        public void StartAnimation()
        {
            Animation anim;
            if (ActiveAvatar.TryGetComponent<Animation>(out anim))
            {
                anim.Play();
            }
        }
        public void ResumeAnimation()
        {
            Animation anim;
            if (ActiveAvatar.TryGetComponent<Animation>(out anim))
            {
                anim.Play(PlayMode.StopSameLayer);
            }
        }
        public void StopAnimation()
        {
            Animation anim;
            if (ActiveAvatar.TryGetComponent<Animation>(out anim))
            {
                anim.Stop();
            }
        }


        //================================================================================================================***
        //  Manage the Equip
        public void StartEquip(string avatarname, string partsname)
        {
            GameObject[] avatars = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject avatar in avatars)
            {
                if (avatar.name == avatarname)
                {
                    EquippingAvatar = avatar;
                    EquippingPartsName = partsname;
                    ChangeEquipModeFromOuter(1);
                    GameObject pts = avatar.GetComponent<OperateLoadedVRM>().GetIKHandle(partsname);
                    break;
                }
            }
        }
        public void EndEquip()
        {
            EquippingAvatar = null;
            ChangeEquipModeFromOuter(0);
        }
    }
}