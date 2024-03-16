using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebXR;
using TMPro;

namespace UserHandleSpace
{
    public class ownscr_HandMenu : MonoBehaviour
    {
        public enum HandMenuDirection
        {
            Left,
            Right
        }
        public HandMenuDirection HandType;

        [SerializeField]
        GameObject handL;
        [SerializeField]
        GameObject handR;
        [SerializeField]
        CameraManagerXR cameraManager;
        [SerializeField]
        GameObject HandMenu;
        [SerializeField]
        GameObject HandMenuOpener;
        [SerializeField]
        GameObject HandMenuButton1;
        [SerializeField]
        GameObject HandMenuButton2;
        [SerializeField]
        protected ManageAnimation managa;
        [Range(0, 10)]
        public int PanelMode;
        [SerializeField]
        GameObject PanelMoveKey;
        [SerializeField]
        GameObject PanelOperateBtns;
        //public TextMesh LabelSelobj;
        //public TextMesh LabelKeynumber;
        public TextMeshPro LabelSelobjP;
        public TextMeshPro LabelKeynumberP;
        [SerializeField]
        GameObject MoveTargetCameraButton;
        [SerializeField]
        GameObject MoveTargetObjectButton;
        [SerializeField]
        GameObject OpeMoveButton;
        [SerializeField]
        GameObject OpeRotButton;
        [SerializeField]
        GameObject OpeSizeButton;
        private string sv_targettype;
        private string sv_movetype;

        public OperateActiveVRM oavrm;


        // Start is called before the first frame update
        void Start()
        {
            oavrm = managa.ikArea.GetComponent<OperateActiveVRM>();
            oavrm.GetEffectiveActiveAvatar();
        }

        // Update is called once per frame
        void Update()
        {
            if ((cameraManager.isActiveAR()) || (cameraManager.isActiveVR()))
            {
                
            }
        }
        private void FixedUpdate()
        {
            if ((!cameraManager.isActiveAR()) && (!cameraManager.isActiveVR()))
            {
                HandMenu.SetActive(false);
                HandMenuOpener.SetActive(false);
                if (HandMenuButton1 != null) HandMenuButton1.SetActive(false);
            }
            else
            {
                HandMenuOpener.SetActive(true);
                if (HandMenuButton1 != null) HandMenuButton1.SetActive(true);

            }
        }
        public ManageAnimation AnimationMan
        {
            get { return managa; }
        } 
        public CameraManagerXR CameraMan
        {
            get { return cameraManager; }
        }
        public void SetObjectTitle(string name)
        {
            //LabelSelobj.text = name;
            LabelSelobjP.text = name;
        }
        public void LabelWriteKeyFrame(int index)
        {
            //LabelKeynumber.text = index.ToString();
            LabelKeynumberP.text = index.ToString();
        }
        public void ChangePanel(int flag)
        {
            if (flag == 0)
            {
                PanelMoveKey.SetActive(true);
                PanelOperateBtns.SetActive(false);
            }else if (flag == 1)
            {
                PanelMoveKey.SetActive(false);
                PanelOperateBtns.SetActive(true);
            }
        }
        public void TurnPreviousObject(string name = "")
        {
            int ishit = AnimationMan.GetCastIndexByAvatar(AnimationMan.VRARSelectedAvatarName,true);
            //if (ishit > -1) LabelSelobj.text = AnimationMan.currentProject.casts[ishit].roleTitle;

            Debug.Log("AnimationMan.VRARSelectedAvatarName=" + AnimationMan.VRARSelectedAvatarName);
            Debug.Log("ishit : Count=" + ishit.ToString() + " : " + AnimationMan.currentProject.casts.Count.ToString());
            for (int i = ishit-1; i >= 0; i--)
            {
                var cast = AnimationMan.currentProject.casts[i];
                Debug.Log(i.ToString() + " - " + cast.type.ToString() + ": " + cast.roleName);
                if (
                    (cast.type == AF_TARGETTYPE.VRM) ||
                    (cast.type == AF_TARGETTYPE.OtherObject) ||
                    (cast.type == AF_TARGETTYPE.Light) ||
                    (cast.type == AF_TARGETTYPE.Camera) ||
                    (cast.type == AF_TARGETTYPE.Image) ||
                    (cast.type == AF_TARGETTYPE.Text3D) ||
                    (cast.type == AF_TARGETTYPE.Effect)
                )
                {
                    /*bool ishitcast = true;
                    if (cast.type == AF_TARGETTYPE.Text)
                    {
                        OperateLoadedText olt = cast.avatar.GetComponent<OperateLoadedText>();
                        if (olt.GetDimension() != "3d")
                        {
                            ishitcast = false;
                        }
                    }
                    if (ishitcast)*/
                    {
                        Debug.Log(i.ToString() + " - " + cast.roleTitle);
                        //oavrm.ChangeEnableAvatarFromOuter(cast.avatarId);
                        AnimationMan.VRARSelectedAvatarName = cast.roleName;

                        SetObjectTitle(cast.roleTitle);
                        break;
                    }
                    

                }
            }
        }
        public void TurnNextObject(string name = "")
        {
            int ishit = AnimationMan.GetCastIndexByAvatar(AnimationMan.VRARSelectedAvatarName,true);
            //if (ishit > -1) LabelSelobj.text = AnimationMan.currentProject.casts[ishit].roleTitle;

            Debug.Log("AnimationMan.VRARSelectedAvatarName=" + AnimationMan.VRARSelectedAvatarName);
            Debug.Log("ishit : Count=" + ishit.ToString() + " : " + AnimationMan.currentProject.casts.Count.ToString());
            for (int i = ishit+1; i < AnimationMan.currentProject.casts.Count; i++)
            {
                var cast = AnimationMan.currentProject.casts[i];
                Debug.Log(i.ToString() + " - " + cast.type.ToString() + ": " + cast.roleName);
                if (
                    (cast.type == AF_TARGETTYPE.VRM) ||
                    (cast.type == AF_TARGETTYPE.OtherObject) ||
                    (cast.type == AF_TARGETTYPE.Light) ||
                    (cast.type == AF_TARGETTYPE.Camera) ||
                    (cast.type == AF_TARGETTYPE.Image) ||
                    (cast.type == AF_TARGETTYPE.Text3D) ||
                    (cast.type == AF_TARGETTYPE.Effect)
                )
                {
                    /*bool ishitcast = true;
                    if (cast.type == AF_TARGETTYPE.Text)
                    {
                        OperateLoadedText olt = cast.avatar.GetComponent<OperateLoadedText>();
                        if (olt.GetDimension() != "3d")
                        {
                            ishitcast = false;
                        }
                    }
                    if (ishitcast)*/
                    {
                        Debug.Log(i.ToString() + " - " + cast.roleTitle);
                        //LabelSelobj.text = cast.avatarId;
                        //oavrm.ChangeEnableAvatarFromOuter(cast.avatarId);
                        AnimationMan.VRARSelectedAvatarName = cast.roleName;

                        SetObjectTitle(cast.roleTitle);
                        break;
                    }
                    
                }
            }
        }
        public void RegisterKeyFrame()
        {
            NativeAnimationAvatar nav = AnimationMan.GetCastInProject(AnimationMan.VRARSelectedAvatarName);

            AnimationRegisterOptions aro = new AnimationRegisterOptions();
            aro.targetId = nav.avatarId;
            aro.targetType = nav.type;
            aro.index = AnimationMan.oldPreviewMarker;
            aro.registerMoveTypes = new List<int>() { (int)AF_MOVETYPE.Translate, (int)AF_MOVETYPE.NormalTransform, (int)AF_MOVETYPE.AllProperties };
            for (int i = 0; i < 17; i++)
            {
                aro.registerBoneTypes.Add(i);
            }

            AnimationMan.RegisterFrame(aro);
        }
        private void PreviewKeyFrameBody(int index, NativeAnimationAvatar nav)
        {
            AnimationParsingOptions aro = new AnimationParsingOptions();

            AnimationMan.PreparePreviewMarker();
            aro.index = index;

            aro.isCameraPreviewing = 0;
            aro.isExecuteForDOTween = 1;
            /*foreach (var cast in AnimationMan.currentProject.casts)
            {
                aro.targetRole = cast.roleName;
                aro.targetType = cast.type;
                AnimationMan.PreviewSingleFrame(aro);
            }*/
            aro.targetRole = nav.roleName;
            aro.targetType = nav.type;
            AnimationMan.PreviewSingleFrame(aro);

            AnimationMan.BackupPreviewMarker();
            AnimationMan.FinishPreviewMarker();

            LabelWriteKeyFrame(aro.index);
        }
        
        public void ChangePreviousKeyFrame()
        {
            NativeAnimationAvatar nav = AnimationMan.GetCastInProject(AnimationMan.VRARSelectedAvatarName);
            int index = AnimationMan.oldPreviewMarker - 1;
            if (index <= 0)
            {
                
                index = AnimationMan.currentProject.timelineFrameLength;
            }
            AnimationMan.currentProject.casts.ForEach(act => {
                PreviewKeyFrameBody(index, act);
            });

        }
        public void ChangeNextKeyFrame()
        {
            NativeAnimationAvatar nav = AnimationMan.GetCastInProject(AnimationMan.VRARSelectedAvatarName);
            int index = AnimationMan.oldPreviewMarker + 1;
            if (AnimationMan.currentProject.timelineFrameLength < index)
            {
                index = 1;
            }
            AnimationMan.currentProject.casts.ForEach(act => {
                PreviewKeyFrameBody(index, act);
            });
            

        }
        public void PlayAnimation()
        {
            
            AnimationParsingOptions aro = new AnimationParsingOptions();
            aro.isLoop = 0;
            aro.index = 1;
            aro.isExecuteForDOTween = 1;
            aro.isCameraPreviewing = 1;
            aro.isBuildDoTween = 1;
            aro.isCompileAnimation = 0;

            AnimationMan.SetRecordingOtherMotion(1);
            AnimationMan.PreparePreviewMarker();
            AnimationMan.StartAllTimeline(aro);
        }
        public void StopAnimation()
        {
            AnimationMan.StopAllTimeline();
        }
        public void StartPauseAnimation()
        {
            AnimationMan.PreparePreviewMarker();
            AnimationMan.PauseAllTimeline();
        }
        public void EndVRAR()
        {
            if (cameraManager.isActiveVR())
            {
                cameraManager.ToggleVR();
            }
            if (cameraManager.isActiveAR())
            {
                cameraManager.ToggleAR();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="targettype">c - camera, o - active object</param>
        public void ChangeMoveTarget(string targettype)
        {
            float pushstate = 0.25f;
            float upstate = 0.5f;
            if (targettype == "c")
            {
                if (sv_targettype == "c") return;

                Vector3 v3 = new Vector3(MoveTargetCameraButton.transform.localPosition.x, MoveTargetCameraButton.transform.localPosition.y, MoveTargetCameraButton.transform.localPosition.z);
                v3.y = pushstate;
                MoveTargetCameraButton.transform.localPosition = v3;
                Debug.Log("move camera butn=" + v3.x.ToString() + "," + v3.y.ToString() + "," + v3.z.ToString());

                Vector3 objv3 = new Vector3(MoveTargetObjectButton.transform.localPosition.x, MoveTargetObjectButton.transform.localPosition.y, MoveTargetObjectButton.transform.localPosition.z);
                objv3.y = upstate;
                MoveTargetObjectButton.transform.localPosition = objv3;
            }
            else if (targettype == "o")
            {
                if (sv_targettype == "o") return;

                Vector3 v3 = new Vector3(MoveTargetCameraButton.transform.localPosition.x, MoveTargetCameraButton.transform.localPosition.y, MoveTargetCameraButton.transform.localPosition.z);
                v3.y = upstate;
                MoveTargetCameraButton.transform.localPosition = v3;
                Debug.Log("move MoveTargetCameraButton butn=" + v3.x.ToString() + "," + v3.y.ToString() + "," + v3.z.ToString());

                Vector3 objv3 = new Vector3(MoveTargetObjectButton.transform.localPosition.x, MoveTargetObjectButton.transform.localPosition.y, MoveTargetObjectButton.transform.localPosition.z);
                objv3.y = pushstate;
                MoveTargetObjectButton.transform.localPosition = objv3;
            }
            Transform hmcrosskey = PanelMoveKey.transform.Find("hmcrosskey");
            int cnt = hmcrosskey.childCount;
            Debug.Log("hmcrosskey children=" + cnt.ToString());
            for (int i = 0; i < cnt; i++)
            {
                Transform child = hmcrosskey.GetChild(i);
                child.GetComponent<ownscr_CrossKey>().MoveTarget = targettype;
            }
            sv_targettype = targettype;
        }
        public void ChangeMoveOperationType(string movetype)
        {
            float pushstate = 0.25f;
            float upstate = 0.5f;
            if (movetype == "translate")
            {
                if (sv_movetype == "translate") return;

                Vector3 v3 = new Vector3(OpeMoveButton.transform.localPosition.x, OpeMoveButton.transform.localPosition.y, OpeMoveButton.transform.localPosition.z);
                v3.y = pushstate;
                OpeMoveButton.transform.localPosition = v3;
                Debug.Log("move camera butn=" + v3.x.ToString() + "," + v3.y.ToString() + "," + v3.z.ToString());

                Vector3 objv3 = new Vector3(OpeRotButton.transform.localPosition.x, OpeRotButton.transform.localPosition.y, OpeRotButton.transform.localPosition.z);
                objv3.y = upstate;
                OpeRotButton.transform.localPosition = objv3;

                Vector3 sizv3 = new Vector3(OpeSizeButton.transform.localPosition.x, OpeSizeButton.transform.localPosition.y, OpeSizeButton.transform.localPosition.z);
                sizv3.y = upstate;
                OpeSizeButton.transform.localPosition = sizv3;
            }
            else if (movetype == "rotate")
            {
                if (sv_movetype == "rotate") return;

                Vector3 v3 = new Vector3(OpeMoveButton.transform.localPosition.x, OpeMoveButton.transform.localPosition.y, OpeMoveButton.transform.localPosition.z);
                v3.y = upstate;
                OpeMoveButton.transform.localPosition = v3;
                Debug.Log("move MoveTargetCameraButton butn=" + v3.x.ToString() + "," + v3.y.ToString() + "," + v3.z.ToString());

                Vector3 objv3 = new Vector3(OpeRotButton.transform.localPosition.x, OpeRotButton.transform.localPosition.y, OpeRotButton.transform.localPosition.z);
                objv3.y = pushstate;
                OpeRotButton.transform.localPosition = objv3;

                Vector3 sizv3 = new Vector3(OpeSizeButton.transform.localPosition.x, OpeSizeButton.transform.localPosition.y, OpeSizeButton.transform.localPosition.z);
                sizv3.y = upstate;
                OpeSizeButton.transform.localPosition = sizv3;
            }
            else if (movetype == "size")
            {
                if (sv_movetype == "size") return;

                Vector3 v3 = new Vector3(OpeMoveButton.transform.localPosition.x, OpeMoveButton.transform.localPosition.y, OpeMoveButton.transform.localPosition.z);
                v3.y = upstate;
                OpeMoveButton.transform.localPosition = v3;
                Debug.Log("move MoveTargetCameraButton butn=" + v3.x.ToString() + "," + v3.y.ToString() + "," + v3.z.ToString());

                Vector3 objv3 = new Vector3(OpeRotButton.transform.localPosition.x, OpeRotButton.transform.localPosition.y, OpeRotButton.transform.localPosition.z);
                objv3.y = upstate;
                OpeRotButton.transform.localPosition = objv3;

                Vector3 sizv3 = new Vector3(OpeSizeButton.transform.localPosition.x, OpeSizeButton.transform.localPosition.y, OpeSizeButton.transform.localPosition.z);
                sizv3.y = pushstate;
                OpeSizeButton.transform.localPosition = sizv3;
            }
            Transform hmcrosskey = PanelMoveKey.transform.Find("hmcrosskey");
            int cnt = hmcrosskey.childCount;
            Debug.Log("hmcrosskey children=" + cnt.ToString());
            for (int i = 0; i < cnt; i++)
            {
                Transform child = hmcrosskey.GetChild(i);
                child.GetComponent<ownscr_CrossKey>().OperationType = movetype;
            }
            sv_movetype = movetype;
        }
        public void ResetTransformCurrentObject(bool ismove, bool isrotate, bool issize)
        {
            NativeAnimationAvatar nav = AnimationMan.GetCastInProject(AnimationMan.VRARSelectedAvatarName);
            //int ishit = AnimationMan.GetCastIndexByAvatar(AnimationMan.VRARSelectedAvatarName, true);
            if (nav != null)
            {
                
                if (nav.type == AF_TARGETTYPE.VRM)
                {
                    nav.avatar.GetComponent<OperateLoadedVRM>().relatedTrueIKParent.GetComponent<OtherObjectDummyIK>().LoadDefaultTransform(ismove, isrotate);
                }
                else
                {
                    if (ismove)
                    {
                        nav.avatar.transform.position = new Vector3(0, 0, 0);
                        nav.ikparent.transform.position = new Vector3(0, 0, 0);
                    }
                    if (isrotate)
                    {
                        nav.avatar.transform.rotation = Quaternion.Euler(0, 0, 0);
                        nav.ikparent.transform.rotation = Quaternion.Euler(0, 0, 0);
                    }
                    if (issize)
                    {
                        nav.avatar.transform.localScale = Vector3.one;
                    }

                    OtherObjectDummyIK oodik = null;
                    if (nav.ikparent.TryGetComponent<OtherObjectDummyIK>(out oodik))
                    {
                        oodik.LoadDefaultTransform(ismove, isrotate);
                        
                        
                    }
                    
                }
                
            }
        }

        public void ShowHandMenu(bool isopen_handmenu)
        {
            HandMenu.SetActive(isopen_handmenu);
            //HandMenuOpener.SetActive(!isopen_handmenu);
        }


        public void ChangeIKMarkerView(bool flag)
        {
            cameraManager.ChangeIKMarkerStateWhenVRAR(flag);
        }
        /// <summary>
        /// Change Shader to "Cutout". OtherObject only.
        /// </summary>
        /// <param name="flag"></param>
        public void ChangeShaderCutout(bool flag)
        {
            NativeAnimationAvatar nav = AnimationMan.GetCastInProject(AnimationMan.VRARSelectedAvatarName);
            if (nav != null)
            {
                if (nav.type == AF_TARGETTYPE.OtherObject)
                {
                    var olo = nav.avatar.GetComponent<OperateLoadedOther>();
                    olo.SetShaderCutoutForce(flag);
                }
            }
        }
    }
}

