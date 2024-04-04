using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UniVRM10;
using UserVRMSpace;
using System.Linq;

namespace UserHandleSpace
{



    /// <summary>
    /// Manage animation of all avatar, other object, camera, light.
    /// </summary>
    public partial class ManageAnimation : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void SendPlayingAnimationInfoOnUpdate(int val);

        [DllImport("__Internal")]
        private static extern void SendPlayingAnimationInfoOnComplete(int val);

        [DllImport("__Internal")]
        private static extern void SendPlayingAnimationInfoOnPause(int val);

        [DllImport("__Internal")]
        private static extern void SendPlayingPreviewAnimationInfoOnComplete(int val);


        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);

        [DllImport("__Internal")]
        private static extern void EndingVRAR(string val);


        static string[] IKBoneNames = {  
            "IKParent", "EyeViewHandle", 
            "Head",                            //BipedIK=Head, FBBIK=Head
            "LookAt",                          //BipedIK=LookAt, FBBIK=LookAt
            "Aim",                             //BipedIK=Aim(position only), FBBIK=Head
            "Chest",                           //BipedIK=Head, FBBIK=Head
            "Pelvis",                          //BipedIK=Head, FBBIK=Head
            "LeftShoulder","LeftLowerArm", "LeftHand",
            "RightShoulder","RightLowerArm","RightHand",
            "LeftLowerLeg","LeftLeg",
            "RightLowerLeg","RightLeg"
        };
        const int IKbonesCount = 17;

        const int HEIGHT_X = 0;
        const int HEIGHT_Y = 1;
        const int HEIGHT_Z = 2;
        const int CHEST_X = 3;
        const int CHEST_Y = 4;
        const int CHEST_Z = 5;
        const int PELVIS_X = 6;
        const int PELVIS_Y = 7;
        const int PELVIS_Z = 8;

        const int CSV_PARTS = 0;
        const int CSV_OPTPARTS = 1;
        const int CSV_ANIMTYPE = 2;
        const int CSV_VALCNT = 3;
        const int CSV_BEGINVAL = 4;

        public static  int PROJECT_VERSION = 5;

        //-------------------------------------------------------------------------
        // Important objects

        public NativeAnimationProjectMaterialPackage materialManager;
        public NativeAnimationProject currentProject;
        private int initialFrameCount = 60;
        public bool IsExternalProject;
        public bool IsPlaying;
        public bool IsPause;
        public bool IsPreview;
        public bool IsBoneLimited;
        public bool IsLimitedPelvis;
        public bool IsLimitedArms;
        public bool IsLimitedLegs;
        public bool IsLimitedChest;
        public bool IsLimitedAim;
        public bool IsRecordingOtherMotion;
        public bool OldIsLimitedPelvis;
        public bool OldIsLimitedArms;
        public bool OldIsLimitedLegs;
        public bool OldIsLimitedChest;
        public bool OldIsLimitedAim;
        
        private int isOldLoop = 0;
        public int oldPreviewMarker;
        private int currentMarker;
        private AnimationParsingOptions currentPlayingOptions;
        private Sequence currentSeq;
        private int seqInIndex;
        private FrameClipboard clipboard;
        private int bkupScreenWidth;
        private int bkupScreenHeight;
        private Vector3 bkup_camerapos;
        private Quaternion bkup_camerarot;
        public Vector3 bkup_lastvrar_pos;

        public KeyOperationMode keyOperationMode;
        public float cfg_dist_cam2view;
        public float cfg_keymove_speed_rot;
        public float cfg_keymove_speed_trans;
        public bool cfg_enable_foot_autorotate;
        public int cfg_vrarctrl_panel_left;
        public int cfg_vrarctrl_panel_right;
        public Vector3 cfg_vrar_camera_initpos;
        public bool cfg_vrar_save_camerapos;
        

        protected NativeAnimationAvatar SingleMotionTargetRole = null;

        public ManageExternalAnimation MexAnim;


        /// <summary>
        /// GameObject as the folder, put a 3D object
        /// </summary>
        public GameObject AvatarArea;

        /// <summary>
        /// GameObject as the folder, put a 
        /// </summary>
        public GameObject SystemViewArea;
        public GameObject ImgArea;
        public GameObject MsgArea;
        public GameObject ikArea;
        public GameObject AudioArea;
        public FileMenuCommands fcom;
        [SerializeField]
        private ConfigSettingLabs configLab;

        public CameraManagerXR camxr;
        private bool IsEndVRAR;
        public string VRARSelectedAvatarName;
        [SerializeField]
        private ownscr_HandMenu VRHandMenu;

        [SerializeField]
        GameObject GizmoRenderer;
        [SerializeField]
        GameObject ObjectInfoView;

        private void Awake()
        {
            IsPause = false;
            IsPlaying = false;
            IsPreview = false;
            IsBoneLimited = true;
            IsLimitedPelvis = true;
            IsLimitedArms = true;
            IsLimitedLegs = true;
            IsLimitedChest = true;
            IsLimitedAim = true;
            OldIsLimitedPelvis = true;
            OldIsLimitedArms = true;
            OldIsLimitedLegs = true;
            OldIsLimitedChest = true;
            OldIsLimitedAim = true;
            IsRecordingOtherMotion = false;
            currentMarker = 1;
            seqInIndex = 1;
            currentSeq = null;
            oldPreviewMarker = 1;
            keyOperationMode = KeyOperationMode.MoveCamera;

            bkupScreenWidth = Screen.width;
            bkupScreenHeight = Screen.height;
            bkup_camerapos = Vector3.zero;
            bkup_lastvrar_pos = Vector3.zero;

            cfg_dist_cam2view = 2.5f;
            cfg_keymove_speed_rot = 0.1f;
            cfg_keymove_speed_trans = cfg_keymove_speed_rot / 10f;
            cfg_enable_foot_autorotate = false;
            cfg_vrarctrl_panel_left = 1;
            cfg_vrarctrl_panel_right = 0;
            cfg_vrar_camera_initpos = Vector3.zero;
            cfg_vrar_save_camerapos = false;

            IsEndVRAR = true;

            ChangeFullIKType(false);
        }
        // Start is called before the first frame update
        void Start()
        {
            //configLab = GameObject.Find("Canvas").GetComponent<ConfigSettingLabs>();

            currentProject = new NativeAnimationProject(initialFrameCount);
            materialManager = new NativeAnimationProjectMaterialPackage();
            NewProject();
            /*
            AvatarArea = GameObject.Find("View Body");
            ikArea = GameObject.Find("IKHandleParent");
            SystemViewArea = GameObject.Find("SystemViewBody");
            ImgArea = GameObject.Find("ImgArea");
            MsgArea = GameObject.Find("MsgArea");
            */
            IsExternalProject = false;
            currentPlayingOptions = new AnimationParsingOptions();
            clipboard = new FrameClipboard();
        }

        // Update is called once per frame
        void Update()
        {
            //---To play animation of Update version
            if (IsPlaying)
            {
                if (!IsPause)
                {

                    /*if (currentMarker > currentProject.timelineFrameLength)
                    {
                        //currentSeq.Play();
                        StopAllTimeline();

                        return;
                    }*/
                    /*else
                    {
                        
                        //StartCoroutine(PlayAllTimeline(currentProject.timeline, currentMarker));
                        Sequence seq =  PlayAllTimeline(currentProject.timeline, currentMarker);

                        if (seq != null)
                        {
                            currentSeq.Append(seq)
                            .AppendInterval(currentProject.baseDuration);
                        }
                        
                    }*/
                    //string js = "";
                    //AnimationParsingOptions aro = new AnimationParsingOptions();
                    //aro.index = currentMarker;
                    //js = JsonUtility.ToJson(aro);
#if !UNITY_EDITOR && UNITY_WEBGL
                    //SendPlayingAnimationInfoOnUpdate(currentMarker);
#endif
                    //currentMarker++;
                    

                }
            }
            if ((bkupScreenWidth != Screen.width) || (bkupScreenHeight != Screen.height))
            {
                if (currentSeq != null)
                {
                    currentSeq.Kill();
                    currentSeq = null;
                }
            }
            bkupScreenWidth = Screen.width;
            bkupScreenHeight = Screen.height;
        }
        private void FixedUpdate()
        {
            if (!IsEndVRAR)
            { //---VR/AR mode end trigger event
                if ((!camxr.isActiveAR() && !camxr.isActiveVR()) && camxr.isActiveNormal())
                { //---if VR/AR mode ended ?
                    Debug.Log("Ending VR/AR.");
                    ChangeColliderState_OtherObjects(true);
                    ChangeStateEndingVRAR();
                    //---return edit data on VR/AR to HTML-UI
                    FinishVRWithEditData();
                    IsEndVRAR = true;
                }
            }
        }
        private void OnDestroy()
        {
            foreach (NativeAnimationAvatar av in currentProject.casts)
            {
                av.avatar = null;
                av.ikparent = null;
            }
            materialManager.Dispose();
        }
        public void ChangeFullIKType(bool useFullIK)
        {
            if (useFullIK)
            {
                IKBoneNames = new string[] {
                    "IKParent", "EyeViewHandle", "Head", "LookAt", "Chest",
                    "LeftShoulder", "LeftLowerArm", "LeftHand",
                    "RightShoulder","RightLowerArm","RightHand",
                    "LeftUpperLeg","LeftLowerLeg","LeftLeg",
                    "RightUpperLeg","RightLowerLeg","RightLeg"
                };
            }
            else
            {
                IKBoneNames = new string []{
                    "IKParent", "EyeViewHandle",
                    "Head", "LookAt", "Aim", "Chest", "Pelvis",
                    "LeftShoulder","LeftLowerArm", "LeftHand",
                    "RightShoulder","RightLowerArm","RightHand",
                    "LeftLowerLeg","LeftLeg",
                    "RightLowerLeg","RightLeg"
                };
            }
        }
        public static string GetVRMIKBoneName(IKBoneType parts)
        {
            return IKBoneNames[(int)parts];
        }
        public static IKBoneType GetVRMIKBoneEnum(string name)
        {
            IKBoneType ret = IKBoneType.IKParent;

            for (int i = 0; i < IKBoneNames.Length; i++)
            {
                if (IKBoneNames[i] == name)
                {
                    ret = (IKBoneType)i;
                    break;
                }
            }
            return ret;
        }
        public void SetValFromOuter(string param)
        {
            string[] prm = param.Split(',');
            
            if (prm[1] == "distance_camera_viewpoint")
            {
                configLab.SetValFromOuter(param);
                cfg_dist_cam2view = configLab.GetFloatVal("distance_camera_viewpoint", 2.5f);
            }
            else if (prm[1] == "camera_keymove_speed")
            {
                configLab.SetValFromOuter(param);
                cfg_keymove_speed_trans = configLab.GetFloatVal("camera_keymove_speed", 0.01f);  //cfg_keymove_speed_rot / 10;
            }
            else if (prm[1] == "camera_keyrotate_speed")
            {
                configLab.SetValFromOuter(param);
                cfg_keymove_speed_rot = configLab.GetFloatVal("camera_keyrotate_speed", 0.1f);
            }
            else if (prm[1] == "enable_foot_autorotate")
            {
                cfg_enable_foot_autorotate =  prm[2] == "1" ? true : false;
            }
            else if (prm[1] == "vrarctrl_panel_left")
            {
                configLab.SetValFromOuter(param);
                cfg_vrarctrl_panel_left = configLab.GetIntVal("vrarctrl_panel_left", 0);
            }
            else if (prm[1] == "vrarctrl_panel_right")
            {
                configLab.SetValFromOuter(param);
                cfg_vrarctrl_panel_right = configLab.GetIntVal("vrarctrl_panel_right", 1);
            }
            else if (prm[1] == "vrar_camera_initpos")
            {
                configLab.SetValFromOuter(param);
                string[] parr = prm[2].Split(":");
                float ipx = float.TryParse(parr[0], out ipx) ? ipx : 0f;
                float ipy = float.TryParse(parr[1], out ipy) ? ipy : 0f;
                float ipz = float.TryParse(parr[2], out ipz) ? ipz : 0f;
                cfg_vrar_camera_initpos = new Vector3(ipx, ipy, ipz);
            }
            else if (prm[1] == "vrar_save_camerapos")
            {
                configLab.SetValFromOuter(param);
                cfg_vrar_save_camerapos = configLab.GetIntVal("vrar_save_camerapos", 0) == 1 ? true : false;
            }
        }
        //===========================================================================================================================
        //  Utility functions
        //===========================================================================================================================
        public void SetInitialTimelineLength(int count)
        {
            initialFrameCount = count;
        }
        public void SetTimelineFrameLength(int count)
        {
            if ((currentProject != null) && (currentProject.isSharing || currentProject.isReadOnly)) return;


            //---if decrease Frame, delete frame data from all characters.
            if (currentProject.timelineFrameLength > count)
            {
                for (int i = 0; i <  currentProject.timeline.characters.Count; i++)
                {
                    for (int f = currentProject.timeline.characters[i].frames.Count-1; f >= 0; f--)
                    {
                        if (currentProject.timeline.characters[i].frames[f].index >= count)
                        {
                            currentProject.timeline.characters[i].frames.RemoveAt(f);
                        }
                    }
                }
            }

            currentProject.timelineFrameLength = count;
        }
        public void SetFps(int count)
        {
            currentProject.fps = count;
            currentProject.baseDuration = (float)currentProject.fps / 6000f;

            foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
            {
                AdjustAllFrameDuration(actor, currentProject.baseDuration);
            }

            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }
        }
        public void SetRecordingOtherMotion(int flag)
        {
            if (flag == 1)
            {
                IsRecordingOtherMotion = true;

            }
            else
            {
                IsRecordingOtherMotion = false;
            }
        }
        public void SetBoneLimited(int flag)
        {
            if (flag == 1)
            {
                IsBoneLimited = true;

            }else
            {
                IsBoneLimited = false;
            }
            
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">[0] - p = pelvis, a = arms, l = legs, c = chest, m = aim, [1] - 1 = true, 0 = false</param>
        public void SetLimitedBones(string param)
        {
            string[] js = param.Split(',');
            if (js[0] == "p")
            {
                IsLimitedPelvis = js[1] == "1" ? true : false;
            }
            else if (js[0] == "a")
            {
                IsLimitedArms = js[1] == "1" ? true : false;
            }
            else if (js[0] == "l")
            {
                IsLimitedLegs = js[1] == "1" ? true : false;
            }
            else if (js[0] == "c")
            {
                IsLimitedChest = js[1] == "1" ? true : false;
            }
            else if (js[0] == "m")
            {
                IsLimitedAim = js[1] == "1" ? true : false;
            }
        }
        public void SetHingeLimited(int flag)
        {
            //---Change enable state for RotationLimitedHinge also.
            if (currentProject != null)
            {
                currentProject.casts.ForEach(cast =>
                {
                    if (cast.type == AF_TARGETTYPE.VRM)
                    {
                        OperateLoadedVRM olvrm = null;
                        if ((cast.avatar != null) && (cast.avatar.TryGetComponent<OperateLoadedVRM>(out olvrm)))
                        {
                            olvrm.EnableRotationLimit(flag);
                        }
                        
                    }

                });
            }
            
        }
        public void GetBoneLimited()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(IsBoneLimited ? 1 : 0);
#endif
        }
        public void GetLimitedBones(string param)
        {
            int flag = 0;
            if (param == "p")
            {
                flag = IsLimitedPelvis ? 1 : 0;
            }
            else if (param == "a")
            {
                flag = IsLimitedArms ? 1 : 0;
            }
            else if (param == "l")
            {
                flag = IsLimitedLegs ? 1 : 0;
            }
            else if (param == "c")
            {
                flag = IsLimitedChest ? 1 : 0;
            }
            else if (param == "m")
            {
                flag = IsLimitedAim ? 1 : 0;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(flag);
#endif
        }
        public void Reload2DObject(string param)
        {
            string[] prm = param.Split(',');
            int x = int.TryParse(prm[0], out x) ? x : 0;
            int y = int.TryParse(prm[1], out y) ? y : 0;

            currentProject.timeline.characters.ForEach(chara =>
            {
                if (chara.avatar.type == AF_TARGETTYPE.Text)
                {
                    OperateLoadedText olt = chara.avatar.avatar.GetComponent<OperateLoadedText>();
                    olt.ReloadPosition(x, y);
                }
                if (chara.avatar.type == AF_TARGETTYPE.UImage)
                {
                    OperateLoadedUImage olt = chara.avatar.avatar.GetComponent<OperateLoadedUImage>();
                    olt.ReloadPosition(x, y);
                }
            });
        }
        private string TrimBlendShapeName(string bsname)
        {
            string[] spr = bsname.Split('.');
            //is ***.***_**_**_... => [***] [***_**_**_]
            //is NOT ***_**_**_=> [***_**_**_]
            // string bs = spr[spr.Length - 1].Replace("M_F00_000_00_", "");
            string bs = bsname.Replace("M_F00_000_00_", "");
            bs = bs.Replace("M_A00_000_00_", "");

            return bs;
        }
        private void AdjustAllFrameDuration(NativeAnimationFrameActor actor, float baseDuration)
        {
            for (int i = 0; i < actor.frames.Count; i++)
            {
                NativeAnimationFrame frame = actor.frames[i];
                int dist = 1;
                if (i > 0)
                {
                    dist = actor.frames[i].index - actor.frames[i - 1].index + 1;
                }
                actor.frames[i].duration = baseDuration * (float)dist;
                    
            }
        }


        /// <summary>
        /// Adjust finalizeIndex of specified FrameActor
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="bDur"></param>
        /// <param name="modDuration"></param>
        /// <param name="modIndex"></param>
        private void AdjustAllFrame(NativeAnimationFrameActor actor, float bDur, bool modDuration, bool modIndex)
        {
            for (int i = 0; i < actor.frames.Count; i++)
            {
                NativeAnimationFrame frame = actor.frames[i];
                
                if (modDuration)
                {
                    int dist = 1;
                    if (i > 0)
                    {
                        dist = actor.frames[i].index - actor.frames[i - 1].index + 1;
                    }
                    actor.frames[i].duration = bDur * (float)dist;
                }
                if (modIndex)
                {
                    if (i > 0)
                    {
                        int nearmin = i - 1;
                        if (nearmin > -1)
                        {
                            actor.frames[i].finalizeIndex = actor.frames[nearmin].finalizeIndex + 1;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get available number of specified objects in the project
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public int GetCountTypeOf(AF_TARGETTYPE type)
        {
            int ret = 0;
            if (currentProject == null) return ret;

            foreach (NativeAnimationAvatar av in currentProject.casts)
            {
                if (av.type == type)
                {
                    ret++;
                }
            }

            return ret;
        }

        //=== Functions to get Frame, Cast ===================================================================================
        /// <summary>
        /// To get role name and title by avatar object id.
        /// </summary>
        /// <param name="param">avatar's id</param>
        /// <returns>CSV-string ... [0] role name(id), [1] role title</returns>
        public string[] GetRoleSpecifiedAvatar(string param)
        {
            string[] ret = new string[2];
            ret[0] = "";
            ret[1] = "";
            foreach (NativeAnimationAvatar avatar in currentProject.casts)
            {
                if (avatar.avatarId == param)
                {
                    ret[0] = avatar.roleName;
                    ret[1] = avatar.roleTitle;
                    break;
                }
            }
            return ret;
        }
        /// <summary>
        /// To get role name and title by avatar object id. (Call from HTML)
        /// </summary>
        /// <param name="param">avatar's id</param>
        /// 
        public void GetRoleSpecifiedAvatarFromOuter(string param)
        {
            string ret = "";
            foreach (NativeAnimationAvatar avatar in currentProject.casts)
            {
                if (avatar.avatarId == param)
                {
                    ret = avatar.roleName + "," + avatar.roleTitle;
                    break;
                }
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }

        /// <summary>
        /// Get target cast(AnimationAvatar) by avatar ID (gameObject.name)
        /// </summary>
        /// <param name="avatarId"></param>
        /// <returns></returns>
        public NativeAnimationAvatar GetCastByAvatar(string avatarId)
        {
            NativeAnimationAvatar ret = currentProject.casts.Find(match =>
            {
                if (match.avatarId == avatarId) return true;
                return false;
            });
            return ret;
        }

        /// <summary>
        /// Get target cast(AnimationAvatar) index by avatar ID (gameObject.name)
        /// </summary>
        /// <param name="avatarId"></param>
        /// <returns>avatar index</returns>
        public int GetCastIndexByAvatar(string avatarId, bool useRole = false)
        {
            int ret = currentProject.casts.FindIndex(match =>
            {
                if (useRole)
                {
                    if (match.roleName == avatarId) return true;
                }
                else
                {
                    if (match.avatarId == avatarId) return true;
                }
                
                return false;
            });
            return ret;
        }
        /// <summary>
        /// Get target cast(AnimationAvatar) from role name
        /// </summary>
        /// <param name="role">role name</param>
        /// <param name="type">if not specify, target all</param>
        /// <returns></returns>
        public NativeAnimationAvatar GetCastInProject(string role, AF_TARGETTYPE type = AF_TARGETTYPE.Unknown)
        {
            NativeAnimationAvatar naa = null;
            naa = currentProject.casts.Find(match =>
            {
                if (type == AF_TARGETTYPE.Unknown)
                {
                    if ((match.roleName == role)) return true;
                }
                else
                {
                    if ((match.roleName == role) && (match.type == type)) return true;
                }
                
                return false;
            });
            return naa;
        }

        /// <summary>
        /// Get target cast(AnimationAvatar) from role name
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public NativeAnimationAvatar GetCastInProject(string role)
        {
            NativeAnimationAvatar naa = null;
            naa = currentProject.casts.Find(match =>
            {
                if ((match.roleName == role)) return true;
                return false;
            });
            return naa;
        }

        /// <summary>
        /// Get target cast(AnimationAvatar) from role title
        /// </summary>
        /// <param name="roleTitle"></param>
        /// <returns></returns>
        public NativeAnimationAvatar GetCastByNameInProject(string roleTitle)
        {
            NativeAnimationAvatar naa = null;
            naa = currentProject.casts.Find(match =>
            {
                if ((match.roleTitle == roleTitle)) return true;
                return false;
            });
            return naa;
        }
        public List<NativeAnimationAvatar> GetCastsByRoleType(AF_TARGETTYPE casttype)
        {
            List<NativeAnimationAvatar> ret = new List<NativeAnimationAvatar>();
            ret = currentProject.casts.FindAll(match =>
            {
                if ((match.type == casttype) && (match.avatar != null)) return true;
                return false;
            });
            return ret;
        }
        /// <summary>
        /// Get target timeline actor object(with avatar) from object ID.
        /// </summary>
        /// <param name="id">avatar object' ID</param>
        /// <param name="type">avatar AF_TARGETTYPE</param>
        /// <returns></returns>
        public NativeAnimationFrameActor GetFrameActorFromObjectID(string id, AF_TARGETTYPE type)
        {
            NativeAnimationFrameActor ret = null;

            ret = currentProject.timeline.characters.Find(av =>
            {
                if (av.avatar == null)
                {
                    return false;
                }
                else
                {
                    if ((av.avatar.avatarId == id) && (av.avatar.type == type))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                
            });

            return ret;
        }

        /// <summary>
        /// Get target timeline actor(with avatar) from role name
        /// </summary>
        /// <param name="role">actor's role name</param>
        /// <param name="type">actor's AF_TARGETTYPE</param>
        /// <returns></returns>
        public NativeAnimationFrameActor GetFrameActorFromRole(string role, AF_TARGETTYPE type)
        {
            NativeAnimationFrameActor ret = null;

            int ishit = currentProject.timeline.characters.FindIndex(av =>
            {
                if (av.avatar == null)
                {
                    return false;
                }
                else
                {
                    if ((av.avatar.roleName == role) && (av.avatar.type == type))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                
                
            });
            if (ishit > -1)
            {
                ret = currentProject.timeline.characters[ishit];
            }
            
            return ret;
        }

        /// <summary>
        /// Get specified frame of actor (from Unity)
        /// </summary>
        /// <param name="actor">Target actor(FrameActor)</param>
        /// <param name="index">internal index</param>
        /// <param name="isFinalize">index is finalizeIndex</param>
        /// <returns></returns>
        private NativeAnimationFrame GetFrame(NativeAnimationFrameActor actor, int index, bool isFinalize = false)
        {
            NativeAnimationFrame ret = null;
            int ishit = -1;
            ishit = actor.frames.FindIndex(item =>
            {
                if (isFinalize)
                {
                    if (item.finalizeIndex == index)
                    {
                        return true;
                    }
                }
                else
                {
                    if (item.index == index)
                    {
                        return true;
                    }
                }

                return false;
            });
            if (ishit > -1)
            {
                ret = actor.frames[ishit];
            }
            /*for (var i = 0; i < currentProject.timeline.groups.Count; i++)
            {
                if (currentProject.timeline.groups[i].index == index)
                {
                    ret = currentProject.timeline.groups[i];
                    break;
                }
            }*/
            return ret;
        }

        /// <summary>
        /// Get specified frame of actor (call from HTML)
        /// </summary>
        /// <param name="param">CSV-string. [0] - role id, [1] - object type, [2] - frame index</param>
        public void GetFrameFromOuter(string param)
        {
            string ret = "";
            string[] prm = param.Split(',');
            int itype = int.TryParse(prm[1], out itype) ? itype : 99;
            AF_TARGETTYPE atype = (AF_TARGETTYPE)itype;
            int frameIndex = int.TryParse(prm[2], out frameIndex) ? frameIndex : 0;

            NativeAnimationFrameActor actor = currentProject.timeline.characters.Find(match =>
            {
                if ((match.targetRole == prm[0]) && (match.targetType == atype)) return true;
                return false;
            });
            if (actor != null)
            {
                NativeAnimationFrame frame = GetFrame(actor, frameIndex);
                if (frame != null) ret = JsonUtility.ToJson(frame);
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        /// <summary>
        /// Get List index from internal frame index
        /// </summary>
        /// <param name="actor">Actor, get a frame</param>
        /// <param name="index">List's index</param>
        /// <returns>Index as the Array</returns>
        private int GetFrameIndex(NativeAnimationFrameActor actor, int index)
        {
            int ret = -1;

            for (var i = 0; i < actor.frames.Count; i++)
            {
                if (actor.frames[i].index == index)
                {
                    ret = i;
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// To get a frame mostly minimum and nearly.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="index"></param>
        /// <returns>Index as the Array</returns>
        private int GetNearMinFrameIndex(NativeAnimationFrameActor actor, int index)
        {
            int ret = -1;

            for (var i = actor.frames.Count - 1; i >= 0; i--)
            {
                if (actor.frames[i].index < index)
                {
                    ret = i;
                    break;
                }
            }
            return ret;
        }

        /// <summary>
        /// To get a frame mostly maximum and nearly.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="index"></param>
        /// <returns>Index as the Array</returns>
        private int GetNearMaxFrameIndex(NativeAnimationFrameActor actor, int index)
        {
            int ret = -1;

            for (var i = 0; i < actor.frames.Count; i++)
            {
                if (actor.frames[i].index > index)
                {
                    ret = i;
                    break;
                }
            }
            return ret;
        }
        /// <summary>
        /// To get all actors (Call from HTML)
        /// </summary>
        public void GetAllActorsFromOuter()
        {
            List<string> ret = new List<string>();
            if (currentProject != null)
            {
                if (currentProject.casts != null)
                {
                    foreach (NativeAnimationAvatar avatar in currentProject.casts)
                    {
                        AnimationAvatar aa = new AnimationAvatar();
                        if (avatar != null)
                        {
                            aa.avatarId = avatar.avatarId;
                            //Array.Copy(avatar.bodyHeight, aa.bodyHeight, avatar.bodyHeight.Length); Not neccessary...
                            aa.roleName = avatar.roleName;
                            aa.roleTitle = avatar.roleTitle;
                            aa.type = avatar.type;
                            string js = JsonUtility.ToJson(aa);
                            ret.Add(js);
                        }

                    }
                }
                
            }

            string jsret = string.Join("%", ret);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(jsret);
#endif
        }

        /// <summary>
        /// Get the avatar object and IK, related to AnimationAvatar
        /// </summary>
        /// <param name="id">avatar's id</param>
        /// <param name="type">avatar's type</param>
        /// <returns>NativeAnimationAvatar(avatar, ikparent only use)</returns>
        private NativeAnimationAvatar GetEffectiveAvatarObjects(string id, AF_TARGETTYPE type)
        {
            NativeAnimationAvatar ret = new NativeAnimationAvatar();
            try
            {
                if (
                (type == AF_TARGETTYPE.VRM) ||
                (type == AF_TARGETTYPE.OtherObject) ||
                (type == AF_TARGETTYPE.Light) ||
                (type == AF_TARGETTYPE.Image)
            )
                {
                    if (AvatarArea.transform.Find(id) != null)
                    {
                        ret.avatar = AvatarArea.transform.Find(id).gameObject;
                        if (type == AF_TARGETTYPE.VRM)
                        {
                            ret.ikparent = ret.avatar.GetComponent<OperateLoadedVRM>().relatedHandleParent;
                        }
                        else if (type == AF_TARGETTYPE.OtherObject)
                        {
                            ret.ikparent = ret.avatar.GetComponent<OperateLoadedOther>().relatedHandleParent;
                        }
                        else if (type == AF_TARGETTYPE.Light)
                        {
                            ret.ikparent = ret.avatar.GetComponent<OperateLoadedLight>().relatedHandleParent;
                        }
                        else if (type == AF_TARGETTYPE.Image)
                        {
                            ret.ikparent = ret.avatar.GetComponent<OperateLoadedOther>().relatedHandleParent;

                        }
                    }
                    else
                    {
                        ret.avatar = null;
                        ret.ikparent = null;
                    }


                }
                else if (type == AF_TARGETTYPE.Camera)
                {
                    ret.avatar = AvatarArea.transform.Find(id).gameObject;
                    ret.ikparent = ret.avatar.GetComponent<OperateLoadedCamera>().relatedHandleParent;
                }
                else if (type == AF_TARGETTYPE.Text)
                {
                    ret.avatar = MsgArea.transform.Find(id).gameObject;
                    ret.ikparent = null;
                }
                else if (type == AF_TARGETTYPE.UImage)
                {
                    ret.avatar = ImgArea.transform.Find(id).gameObject;
                    ret.ikparent = null;
                }
                else if (type == AF_TARGETTYPE.Effect)
                {
                    ret.avatar = AvatarArea.transform.Find(id).gameObject;
                    ret.ikparent = ret.avatar.GetComponent<OperateLoadedEffect>().relatedHandleParent;
                }
                else if (type == AF_TARGETTYPE.SystemEffect)
                {
                    ret.avatar = GameObject.Find("AnimateArea");
                    ret.ikparent = ret.avatar;
                }
                else if (type == AF_TARGETTYPE.Audio)
                {
                    if ((id == "BGM") || (id == "SE"))
                    {
                        ret.avatar = GameObject.Find(id);
                        ret.ikparent = ret.avatar;
                    }
                }
                else if (type == AF_TARGETTYPE.Stage)
                {
                    ret.avatar = GameObject.FindGameObjectWithTag("GroundWorld");
                    ret.ikparent = ret.avatar;
                }
                else if (type == AF_TARGETTYPE.Text3D)
                {
                    ret.avatar = AvatarArea.transform.Find(id).gameObject;
                    ret.ikparent = ret.avatar.GetComponent<OperateLoadedText>().relatedHandleParent;
                }
            }
            catch (Exception)
            {
                ret.avatar = null;
                ret.ikparent = null;
            }
            
            return ret;
        }

        /// <summary>
        /// Get VRM and IK-parent by role title = VRMMeta.Meta.Title
        /// </summary>
        /// <param name="roleTitle"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private NativeAnimationAvatar GetEffectiveAvatarFromRoleTitle(string roleTitle, AF_TARGETTYPE type)
        {
            NativeAnimationAvatar ret = new NativeAnimationAvatar();

            int cnt = AvatarArea.transform.childCount;
            for (int i = 0; i < cnt; i++)
            {
                GameObject child = AvatarArea.transform.GetChild(i).gameObject;
                Vrm10Instance vmeta = null;
                if (child.TryGetComponent(out vmeta))
                {
                    if (vmeta.Vrm.Meta.Name == roleTitle)
                    {
                        ret.avatar = child;
                        ret.ikparent = ret.avatar.GetComponent<OperateLoadedVRM>().relatedHandleParent;
                        break;
                    }
                }

            }


            return ret;
        }
        public string GetObjectTitle(NativeAnimationAvatar avatar)
        {
            string ret = "";
            AF_TARGETTYPE type = avatar.type;

            if ((type == AF_TARGETTYPE.VRM) || (type == AF_TARGETTYPE.OtherObject) || (type == AF_TARGETTYPE.Light) || (type == AF_TARGETTYPE.Image))
            {
                if (type == AF_TARGETTYPE.VRM)
                {
                    ret = avatar.avatar.GetComponent<OperateLoadedVRM>().Title;
                }
                else if (type == AF_TARGETTYPE.OtherObject)
                {
                    ret = avatar.avatar.GetComponent<OperateLoadedOther>().Title;
                }
                else if (type == AF_TARGETTYPE.Light)
                {
                    ret = avatar.avatar.GetComponent<OperateLoadedLight>().Title;
                }
                else if (type == AF_TARGETTYPE.Image)
                {
                    ret = avatar.avatar.GetComponent<OperateLoadedOther>().Title;

                }
            }
            else if (type == AF_TARGETTYPE.Camera)
            {
                ret = avatar.avatar.GetComponent<OperateLoadedCamera>().Title;
            }
            else if ((type == AF_TARGETTYPE.Text) || (type == AF_TARGETTYPE.Text3D))
            {
                ret = avatar.avatar.GetComponent<OperateLoadedText>().Title;
            }
            else if (type == AF_TARGETTYPE.UImage)
            {
                ret = avatar.avatar.GetComponent<OperateLoadedUImage>().Title;
            }
            else if (type == AF_TARGETTYPE.Audio)
            {
                ret = avatar.avatar.GetComponent<OperateLoadedAudio>().Title;
            }
            else if (type == AF_TARGETTYPE.Effect)
            {
                ret = avatar.avatar.GetComponent<OperateLoadedEffect>().Title;
            }
            else if (type == AF_TARGETTYPE.SystemEffect)
            {
                ret = "System effect";
            }
            else if (type == AF_TARGETTYPE.Stage)
            {
                ret = "Stage";
            }
            return ret;
        }
        
        /// <summary>
        /// Get frame index nearly previous position
        /// </summary>
        /// <param name="param"></param>
        public void GetPreviousExistKeyframeFromOuter(string param)
        {
            AnimationParsingOptions apo = JsonUtility.FromJson<AnimationParsingOptions>(param);
            
            int ret = GetPreviousExistKeyframe(apo);

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret);
#endif
        }
        public int GetPreviousExistKeyframe(AnimationParsingOptions apo)
        {
            NativeAnimationFrameActor naf = GetFrameActorFromRole(apo.targetRole, apo.targetType);
            int ret = -1;
            if (naf != null)
            {
                int index = GetNearMinFrameIndex(naf, apo.index);

                if (index > -1)
                {
                    ret = naf.frames[index].index;
                }
            }
            return ret;
        }

        /// <summary>
        /// Get frame index nearly previous position
        /// </summary>
        /// <param name="param"></param>
        public void GetNextExistKeyframeFromOuter(string param)
        {
            AnimationParsingOptions apo = JsonUtility.FromJson<AnimationParsingOptions>(param);

            int ret = GetNextExistKeyframe(apo);


#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret);
#endif
        }
        public int GetNextExistKeyframe(AnimationParsingOptions apo)
        {
            NativeAnimationFrameActor naf = GetFrameActorFromRole(apo.targetRole, apo.targetType);
            //Debug.Log(apo.targetRole);
            //Debug.Log(apo.targetType);
            int ret = -1;
            if (naf != null)
            {
                //Debug.Log("naf.targetId="+naf.targetId);
                int index = GetNearMaxFrameIndex(naf, apo.index);
                //Debug.Log(index);
                if (index > -1)
                {
                    ret = naf.frames[index].index;
                }
            }

            return ret;
        }


        //=== Functions to setup Cast ===================================================================================
        /// <summary>
        /// Refresh all casts in curren animation project.
        /// </summary>
        /// <param name="param"></param>
        public void ResetAllAvatar()
        {
            currentProject.casts.ForEach(avt =>
            {
                avt.avatarId = "";
                avt.avatar = null;
                avt.ikparent = null;
            });

        }

        /// <summary>
        /// Generate avatar ID and check existing
        /// </summary>
        /// <param name="prefix"></param>
        /// <param name="instanceID"></param>
        /// <returns></returns>
        public string CheckAndSetAvatarId(string prefix, int instanceID)
        {
            string ret = "";

            bool isLoop = true;

            while (isLoop)
            {
                DateTime curdt = DateTime.Now;
                string tmpname = prefix + curdt.AddMilliseconds(instanceID).ToString("yyyyMMddHHmmssFFFFFFF") ; //DateTime.Now.ToFileTime().ToString();
                NativeAnimationAvatar chknav = GetCastByAvatar(tmpname);
                if (chknav == null)
                {
                    ret = tmpname;
                    isLoop = false;
                }                
            }
            

            return ret;
        }
        public int CheckAvatarTitleExist(string title, AF_TARGETTYPE type)
        {
            List<NativeAnimationAvatar> existCnt = currentProject.casts.FindAll(tmpnav =>
            {
                if (type == tmpnav.type)
                {
                    OperateLoadedBase olb = tmpnav.avatar.GetComponent<OperateLoadedBase>();
                    if (olb != null)
                    {
                        if (olb.Title.IndexOf(title) > -1)
                        {
                            return true;
                        }
                    }
                }
                return false;
            });
            int ret = 0;
            if (existCnt.Count > 0)
            {
                string lasttitle = existCnt.Last<NativeAnimationAvatar>().avatar.GetComponent<OperateLoadedBase>().Title;
                string[] tarr = lasttitle.Split("_");

                int lastintstr = 0;
                if (int.TryParse(tarr[tarr.Length - 1], out lastintstr))
                {
                    ret = lastintstr + 1;
                }
                else
                {
                    ret = existCnt.Count + 1;
                }
            }
            return ret;
        }

        /// <summary>
        /// Check wheather title is exist in casts. if exised, return next counter.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public int CheckRoleTitleExist(string title, AF_TARGETTYPE type)
        {
            List<NativeAnimationAvatar> existCnt = currentProject.casts.FindAll(tmpnav =>
            {
                if (type == tmpnav.type)
                {
                    if (tmpnav.roleTitle.IndexOf(title) > -1)
                    {
                        return true;
                    }
                }
                return false;
            });
            int ret = 0;
            if (existCnt.Count > 0)
            {
                NativeAnimationAvatar lastav = existCnt.Last<NativeAnimationAvatar>();
                string lasttitle = "";
                if (lastav != null)
                {
                    lasttitle = lastav.roleTitle;
                }
                
                string[] tarr = lasttitle.Split("_");

                int lastintstr = 0;
                if (int.TryParse(tarr[tarr.Length-1],out lastintstr))
                {
                    ret = lastintstr + 1;
                }
                else
                {
                    ret = existCnt.Count + 1;
                }
            }
            return ret;

        }

        /// <summary>
        /// Create An empty cast and timeline
        /// </summary>
        /// <param name="param">CSV-String: [0] - cast type(AF_TARGETTYPE), [1] - cast title</param>
        /// <returns>CSV-string: [0] role name(id), [1] - role title</returns>
        public NativeAnimationAvatar CreateEmptyCast(string param)
        {
            string[] prm = param.Split(',');
            int t = int.TryParse(prm[0], out t) ? t : 99;
            AF_TARGETTYPE castType = (AF_TARGETTYPE)t;
            string[] cns_roleNames = { "VRM", "OtherObject", "Light", "Camera", "Text", "Image", "UImage", "Audio", "Effect", "SystemEffect", "Stage", "Text3D" };

            NativeAnimationAvatar naa = new NativeAnimationAvatar();
            naa.avatar = null;
            naa.ikparent = null;
            naa.avatarId = "";

            if (castType == AF_TARGETTYPE.Unknown)
            {
                naa.roleName = "";
                naa.roleTitle = "";
                naa.type = castType;
                
            }
            else
            {
                naa.roleName = cns_roleNames[t] + "_" + DateTime.Now.ToFileTime().ToString();
                naa.roleTitle = naa.roleName;
                naa.type = castType;

                currentProject.casts.Add(naa);

                NativeAnimationFrameActor naf = new NativeAnimationFrameActor();
                naf.targetId = "";
                naf.targetRole = naa.roleName;
                naf.targetType = castType;
                naf.avatar = naa;
                currentProject.timeline.characters.Add(naf);
            }

            string js = naa.roleName + "," + naa.roleTitle;
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif

            return naa;

        }
        public NativeAnimationAvatar FirstAddFixedAvatar(string id, GameObject avatar, GameObject ikparent, string RoleName, AF_TARGETTYPE type)
        {
            if ((currentProject != null) && (currentProject.isSharing || currentProject.isReadOnly)) return (NativeAnimationAvatar)null;

            NativeAnimationAvatar nav = new NativeAnimationAvatar();
            nav.avatarId = id;
            nav.avatar = avatar;
            nav.ikparent = ikparent;
            nav.type = type;
            nav.roleName = RoleName;  //"(" + GetCountTypeOf(type) + ")";
            nav.roleTitle = nav.roleName;

            //---for timeline.characters
            NativeAnimationFrameActor naf = new NativeAnimationFrameActor();
            naf.targetId = id;
            naf.targetRole = nav.roleName;
            naf.targetType = type;
            naf.avatar = nav;

            currentProject.casts.Add(nav);

            //---for timeline.characters
            currentProject.timeline.characters.Add(naf);

            return nav;
        }
        /// <summary>
        /// Set up each avatar object (to connect animation project)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="avatar"></param>
        /// <param name="ikparent"></param>
        /// <param name="RoleName"></param>
        /// <param name="type"></param>
        /// <param name="bodyinfo"></param>
        public NativeAnimationAvatar FirstAddAvatar(string id, GameObject avatar, GameObject ikparent, string RoleName, AF_TARGETTYPE type, float[] bodyinfo = null)
        {
            if ((currentProject != null) && (currentProject.isSharing || currentProject.isReadOnly)) return (NativeAnimationAvatar)null;

            NativeAnimationAvatar nav = new NativeAnimationAvatar();
            nav.avatarId = id;
            nav.avatar = avatar;
            nav.ikparent = ikparent;
            nav.type = type;
            nav.roleName = RoleName + "_" + DateTime.Now.ToFileTime().ToString();  //"(" + GetCountTypeOf(type) + ")";

            
            int existCnt = CheckRoleTitleExist(RoleName, type);


            if (existCnt == 0)
            {
                nav.roleTitle = RoleName;
            }
            else
            {
                nav.roleTitle = RoleName + "_" + existCnt.ToString();
            }
            

            //---for timeline.characters
            NativeAnimationFrameActor naf = new NativeAnimationFrameActor();
            naf.targetId = id;
            naf.targetRole = nav.roleName;
            naf.targetType = type;
            naf.avatar = nav;

            if (type == AF_TARGETTYPE.VRM)
            {
                Array.Copy(bodyinfo, nav.bodyHeight, bodyinfo.Length);

                //---for timeline.characters
                Array.Copy(bodyinfo, naf.bodyHeight, bodyinfo.Length);
            }
            currentProject.casts.Add(nav);

            //---for timeline.characters
            currentProject.timeline.characters.Add(naf);

            return nav;
        }

        /// <summary>
        /// Set up each avatar object (to connect animation project)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="avatar"></param>
        /// <param name="ikparent"></param>
        /// <param name="RoleName"></param>
        /// <param name="type"></param>
        /// <param name="bodyheight"></param>
        /// <param name="bodyinfo"></param>
        public NativeAnimationAvatar FirstAddAvatarForVRM(out bool isOverWrite, string id, GameObject avatar, GameObject ikparent, string RoleName, AF_TARGETTYPE type, float[] bodyheight = null,  List < Vector3> bodyinfo = null)
        {
            isOverWrite = false;
            if ((currentProject != null) && (currentProject.isSharing || currentProject.isReadOnly)) return (NativeAnimationAvatar)null;

            Vrm10Instance vmeta = avatar.GetComponent<Vrm10Instance>();
            string rtitle = "";
            if (vmeta != null)
            {
                rtitle = vmeta.Vrm.Meta.Name != null ? vmeta.Vrm.Meta.Name : "";
            }

            bool isExists = false;

            //---If VRM's title equal to role title, allocate to existed cast.
            NativeAnimationAvatar nav = GetCastByNameInProject(rtitle);
            if ((nav != null) && (nav.avatar == null))
            { //---already this time VRM exists in the project (And unallocated)
                nav.avatarId = id;
                nav.avatar = avatar;
                nav.ikparent = ikparent;
                // (*) roleName inherits the previous name.
                //nav.roleName = RoleName + "_" + DateTime.Now.ToFileTime().ToString();  //"(" + GetCountTypeOf(type) + ")";

                isExists = true;
                isOverWrite = true;
            }
            else
            {
                string convertTitle = rtitle;
                /*if (nav != null)
                { //---role is exist, and not allocate cast
                    string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    char[] Charsarr = new char[3];
                    System.Random random = new System.Random();

                    for (int i = 0; i < Charsarr.Length; i++)
                    {
                        Charsarr[i] = characters[random.Next(characters.Length)];
                    }

                    convertTitle += "_" + new String(Charsarr);
                }*/
                
                int existCnt = CheckRoleTitleExist(rtitle, type);

                nav = new NativeAnimationAvatar();
                nav.avatarId = id;
                nav.avatar = avatar;
                nav.ikparent = ikparent;
                nav.type = type;
                nav.roleName = RoleName + "_" + DateTime.Now.ToFileTime().ToString();  //"(" + GetCountTypeOf(type) + ")";
                if (existCnt == 0)
                {
                    nav.roleTitle = convertTitle; // nav.roleName;
                }
                else
                {
                    nav.roleTitle = convertTitle + "_" + existCnt.ToString();
                }
                
            }



            //---for timeline.characters
            NativeAnimationFrameActor naf = null;
            naf = GetFrameActorFromRole(nav.roleName, nav.type);
            if (naf == null)
            {
                naf = new NativeAnimationFrameActor();
                naf.targetId = id;
                naf.targetRole = nav.roleName;
                naf.targetType = type;
                naf.avatar = nav;

            }

            if (type == AF_TARGETTYPE.VRM)
            {
                if (bodyheight != null)
                {
                    Array.Copy(bodyheight, nav.bodyHeight, bodyheight.Length);
                    //---for timeline.characters
                    Array.Copy(bodyheight, naf.bodyHeight, bodyheight.Length);
                }
                if (bodyinfo != null)
                {
                    for (int i = 0; i < bodyinfo.Count; i++)
                    {
                        nav.bodyInfoList.Add(new Vector3(bodyinfo[i].x, bodyinfo[i].y, bodyinfo[i].z));
                        //---for timeline.characters
                        naf.bodyInfoList.Add(new Vector3(bodyinfo[i].x, bodyinfo[i].y, bodyinfo[i].z));
                    }
                }
                OperateLoadedVRM olvrm = avatar.GetComponent<OperateLoadedVRM>();
                //---Set to blendShapeList of FrameActor the blendShapeList, what VRM has.
                naf.blendShapeList.Clear();
                olvrm.blendShapeList.ForEach(item =>
                {
                    naf.blendShapeList.Add(TrimBlendShapeName(item.text));
                });

                naf.gravityBoneList.Clear();
                olvrm.gravityList.list.ForEach(item =>
                {
                    naf.gravityBoneList.Add(item.comment + "/" + item.rootBoneName);
                });
                
            }
            
            if (!isExists)
            {
                currentProject.casts.Add(nav);

                //---for timeline.characters
                currentProject.timeline.characters.Add(naf);
            }
            

            return nav;
        }
        /// <summary>
        /// Set up each avatar object to connect animation project (necessary automatically attach)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="avatar"></param>
        /// <param name="ikparent"></param>
        /// <param name="RoleName"></param>
        /// <param name="type"></param>
        /// <param name="path"></param>
        public NativeAnimationAvatar FirstAddAvatarForFileObject(out bool isOverWrite, string id, GameObject avatar, GameObject ikparent, string RoleName, AF_TARGETTYPE type, string path)
        {
            isOverWrite = false;
            bool isExists = false;

            if ((currentProject != null) && (currentProject.isSharing || currentProject.isReadOnly)) return (NativeAnimationAvatar)null;

            NativeAnimationAvatar nav = currentProject.casts.Find(match =>
            {
                if ((match.path == path) && (match.type == type) && (match.avatar == null)) return true;
                return false;
            });
            if ((nav != null))
            { //---already this time the each object exists in the project (And unallocated)
                nav.avatarId = id;
                nav.avatar = avatar;
                nav.ikparent = ikparent;
                // (*) roleName inherits the previous name.
                //nav.roleName = RoleName + "_" + DateTime.Now.ToFileTime().ToString();  //"(" + GetCountTypeOf(type) + ")";

                isExists = true;
                isOverWrite = true;
            }
            else
            {
                nav = new NativeAnimationAvatar();
                nav.avatarId = id;
                nav.avatar = avatar;
                nav.ikparent = ikparent;
                nav.type = type;
                nav.roleName = RoleName + "_" + DateTime.Now.ToFileTime().ToString();  //"(" + GetCountTypeOf(type) + ")";

                
                int existCnt = CheckRoleTitleExist(path, type);
                if (existCnt == 0)
                {
                    nav.roleTitle = path;
                }
                else
                {
                    nav.roleTitle = path + "_" + existCnt.ToString();
                }
                nav.path = path;
            }

            //---for timeline.characters
            NativeAnimationFrameActor naf = null;
            naf = GetFrameActorFromRole(nav.roleName, nav.type);
            if (naf == null)
            {
                naf = new NativeAnimationFrameActor();
                naf.targetId = id;
                naf.targetRole = nav.roleName;
                naf.targetType = type;
                naf.avatar = nav;

            }
            if (!isExists)
            {
                currentProject.casts.Add(nav);

                //---for timeline.characters
                currentProject.timeline.characters.Add(naf);
            }            

            return nav;
        }

        /// <summary>
        /// To set role title of cast for the avatar
        /// </summary>
        /// <param name="param">CSV-string - [0]=avatar's id, [1]=role title</param>
        public void EditActorsRole(string param)
        {
            /*  CSV-string
                [0] : avatar's id
                [1] : role title
            */
            string[] prm = param.Split(',');
            for (int i = 0; i < currentProject.casts.Count; i++)
            {
                NativeAnimationAvatar avatar = currentProject.casts[i];

                if (avatar.avatarId == prm[0])
                {
                    //---repair for influence of renaming role title
                    currentProject.casts.ForEach(vrm =>
                    {
                        if (vrm.type == AF_TARGETTYPE.VRM)
                        {
                            vrm.avatar.GetComponent<OperateLoadedVRM>().ApplyRenameIKTargetRoleTitle(avatar.roleTitle + "\t" + prm[1]);
                        }
                    });

                    //---apply changes!
                    avatar.roleTitle = prm[1];
                    //Debug.Log(JsonUtility.ToJson(avatar));
                }
            }
        }
        /// <summary>
        /// To attach specify avatar object to the cast for a role in current project.
        /// </summary>
        /// <param name="param">CSV-string. [0] is roleName. [1] is avatar object's ID.</param>
        public void AttachAvatarToRole(string param)
        {
            string[] prm = param.Split(',');
            /*
             * prm[0] - roleName
             * prm[1] - avatar ID
             */
            for (int i = 0; i < currentProject.casts.Count; i++)
            {
                NativeAnimationAvatar avatar = currentProject.casts[i];

                if (avatar.roleName == prm[0])
                {
                    //---overwrite target cast info to the called role 
                    avatar.avatarId = prm[1];
                    NativeAnimationAvatar tmpav = GetEffectiveAvatarObjects(prm[1], avatar.type);

                    //---change reference to avatar object.
                    avatar.avatar = tmpav.avatar;
                    avatar.ikparent = tmpav.ikparent;
                    avatar.path = tmpav.path;
                    avatar.ext = tmpav.ext;

                    //---update bodyInfoList current avatar body info.
                    /*
                    if ((!currentProject.isSharing && !currentProject.isReadOnly) && (avatar.type == AF_TARGETTYPE.VRM))
                    {
                        NativeAnimationFrameActor factor = currentProject.timeline.characters.Find(match =>
                        {
                            if (match.targetRole == avatar.roleName) return true;
                            return false;
                        });
                        if (factor != null)
                        {
                            factor.bodyInfoList.Clear();
                            List<Vector3> list = tmpav.avatar.GetComponent<OperateLoadedVRM>().GetTPoseBodyList();
                            foreach (Vector3 v in list)
                            {
                                factor.bodyInfoList.Add(new Vector3(v.x, v.y, v.z));
                            }
                        }
                        
                    }*/

                    //---timeline(frame actor)
                    NativeAnimationFrameActor naf = GetFrameActorFromRole(avatar.roleName, avatar.type);
                    naf.targetId = avatar.avatarId;

                    //---apply height difference with absorb to this role(frame actor) (VRM only)
                    //------height: old:avatar --> new:tmpav
                    //Array.Copy(tmpav.bodyHeight, avatar.bodyHeight, tmpav.bodyHeight.Length);

                    CalculateAllFrameForCurrent(avatar, naf);
                    //------update also the height of frame actor (NECCESARY): this avatar height --> frame actor height ( 1:1 )
                    Array.Copy(tmpav.bodyHeight, naf.bodyHeight, tmpav.bodyHeight.Length);

                }
            }

            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }
        }

        /// <summary>
        /// Detach avatar object from specify cast
        /// </summary>
        /// <param name="param">CSV-string. [0] is roleName or avatar id, [1] is avatar / role</param>
        public void DetachAvatarFromRole(string param)
        {
            string[] prm = param.Split(',');
            for (int i = 0; i < currentProject.casts.Count; i++)
            {
                NativeAnimationAvatar avatar = currentProject.casts[i];
                NativeAnimationFrameActor naf = GetFrameActorFromRole(avatar.roleName, avatar.type);

                if (prm[1] == "role")
                {
                    if (avatar.roleName == prm[0])
                    {
                     
                        //---cast
                        avatar.avatarId = "";
                        avatar.avatar = null;
                        avatar.ikparent = null;
                        avatar.path = "";
                        avatar.ext = "";

                        //---timeline
                        naf.targetId = "";
                    }
                }
                else if(prm[1] == "avatar")
                {
                    if (avatar.avatarId == prm[0])
                    {
                        //---cast
                        avatar.avatarId = "";
                        avatar.avatar = null;
                        avatar.ikparent = null;
                        avatar.path = "";
                        avatar.ext = "";

                        //---timeline
                        naf.targetId = "";
                    }
                }
            }

            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }
        }

        /// <summary>
        /// Delete NativeAnimationAvatar (also  nullize all reference ) (Call from HTML)
        /// </summary>
        /// <param name="param">CSV-string: [0]=roleName, [1]=object type</param>
        public void DeleteAvatarFromCast(string param)
        {
            int isHit = -1;
            string[] prm = param.Split(',');
            int itype = int.TryParse(prm[1], out itype) ? itype : 99;
            for (int i = 0; i < currentProject.casts.Count; i++)
            {
                NativeAnimationAvatar avatar = currentProject.casts[i];
                if ((avatar.roleName == prm[0]) && (avatar.type == (AF_TARGETTYPE)itype))
                {
                    isHit = i;
                    //---nullize reference
                    avatar.avatar = null;
                    avatar.ikparent = null;
                    break;
                }
            }
            if (isHit > -1)
            {
                //---nullize and delete also character in timeline.characters.
                int isCHit = -1;
                for (int c = 0; c < currentProject.timeline.characters.Count; c++)
                {
                    NativeAnimationFrameActor actor = currentProject.timeline.characters[c];

                    //---nullize reference of the timeline actor
                    if ((actor.targetType == currentProject.casts[isHit].type) && (actor.targetRole == currentProject.casts[isHit].roleName))
                    {
                        isCHit = c;
                        actor.avatar = null;
                    }
                }
                //---remove target avatar object from casts
                currentProject.casts.RemoveAt(isHit);
                if (isCHit > -1)
                {
                    currentProject.timeline.characters.RemoveAt(isCHit);
                }

            }

            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }
        }
        public void DestroyEffectiveAvatar(GameObject avatar, GameObject ikparent, AF_TARGETTYPE type)
        {
            if ((type == AF_TARGETTYPE.VRM) || (type == AF_TARGETTYPE.OtherObject) || (type == AF_TARGETTYPE.Image) ||
                 (type == AF_TARGETTYPE.Light) || (type == AF_TARGETTYPE.Camera) || (type == AF_TARGETTYPE.Effect) || (type == AF_TARGETTYPE.Text3D)
            )
            {
                //fcom.DestroyVRM(cast.avatar.name);
                DetachAvatarFromRole(avatar.name + ",avatar");
                Destroy(ikparent);
                Destroy(avatar);
                
            }
            
            else if ((type == AF_TARGETTYPE.Text) || (type == AF_TARGETTYPE.UImage))
            {
                //fcom.DestroyText(cast.avatar.name);
                Destroy(avatar);
            }
        }
        public void DeleteAllEmptyTimeline()
        {
            List<string> ret = new List<string>();
            for (int tl_i = currentProject.timeline.characters.Count-1; tl_i >= 0; tl_i--)
            {
                if (currentProject.timeline.characters[tl_i].frames.Count == 0)
                {
                    int cast_i = currentProject.casts.FindIndex(item =>
                    {
                        if (item.roleName == currentProject.timeline.characters[tl_i].targetRole)
                        {
                            if (item.avatarId == "") return true;
                            return false;
                        }
                        else
                        {
                            return false;
                        }
                    });
                    if (cast_i > -1)
                    {
                        ret.Add(currentProject.casts[cast_i].roleName + "," + ((int)currentProject.casts[cast_i].type).ToString());
                        DeleteAvatarFromCast(currentProject.casts[cast_i].roleName + "," + ((int)currentProject.casts[cast_i].type).ToString());
                        
                    }
                }
            }
            string js = string.Join(",", ret);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }

        /// <summary>
        /// To reset specified IK-target of all VRM
        /// </summary>
        /// <param name="param"></param>
        public void ApplyAllVRM_ResetIKTargetBySearchObject(string param)
        {
            currentProject.casts.ForEach(item =>
            {
                if (item.type == AF_TARGETTYPE.VRM)
                {
                    item.avatar.GetComponent<OperateLoadedVRM>().ResetIKMappingBySearchObject(param);
                }
            });
        }

        //=== Functions to check, utility ===================================================================================
        public string NullCheckAllActorsForRole()
        {
            List<string> lst = new List<string>();
            string ret = "";

            foreach (NativeAnimationAvatar avatar in currentProject.casts)
            {
                if (avatar.avatar == null)
                {
                    lst.Add(avatar.roleTitle);
                }
            }
            ret = string.Join(",", lst);
            return ret;
        }
        public void NullCheckAllActorsForRoleFromOuter()
        {
            List<string> lst = new List<string>();
            string ret = "";

            foreach (NativeAnimationAvatar avatar in currentProject.casts)
            {
                if (avatar.avatar == null)
                {
                    lst.Add(avatar.roleTitle);
                }
            }
            ret = string.Join(",", lst);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        private AF_TARGETTYPE ConvertStringToAF(string type)
        {
            AF_TARGETTYPE realtype = AF_TARGETTYPE.Unknown;
            if (type == "VRM")
            {
                realtype = AF_TARGETTYPE.VRM;
            }
            else if (type == "LIGHT")
            {
                realtype = AF_TARGETTYPE.Light;
            }
            else if (type == "CAMERA")
            {
                realtype = AF_TARGETTYPE.Camera;
            }
            else if (type == "TEXT")
            {
                realtype = AF_TARGETTYPE.Text;
            }
            else if (type == "IMAGE")
            {
                realtype = AF_TARGETTYPE.Image;
            }
            else if (type == "UIMAGE")
            {
                realtype = AF_TARGETTYPE.UImage;
            }
            else if (type == "AUDIO")
            {
                realtype = AF_TARGETTYPE.Audio;
            }
            else if (type == "TEXT3D")
            {
                realtype = AF_TARGETTYPE.Text3D;
            }
            else
            {
                //---else is OtherObject. (because OTH:FBX, OTH:OBJ, etc...)
                realtype = AF_TARGETTYPE.OtherObject;
            }

            return realtype;
        }

        /// <summary>
        /// To adjust index of the frame near to maximumly 
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="currentFrame"></param>
        private void adjustNearMaxFrameIndex(NativeAnimationFrameActor actor, NativeAnimationFrame currentFrame)
        {
            int nearmax = GetNearMaxFrameIndex(actor, currentFrame.index);
            if ((nearmax > -1) && (nearmax < actor.frames.Count))
            {
                actor.frames[nearmax].finalizeIndex = currentFrame.finalizeIndex + 1;
                currentFrame.duration = currentProject.baseDuration * (float)(actor.frames[nearmax].index - currentFrame.index);
            }

            //---renew
            //int ret = -1;

            //---change after than nearmax (nearmax+1 ~ n)
            for (var i = nearmax+1; i < actor.frames.Count; i++)
            {
                if (
                    (actor.frames[i].index > currentFrame.index) && 
                    ((i-1) >= 0)
                )
                {
                    actor.frames[i].finalizeIndex = actor.frames[i-1].finalizeIndex + 1;
                }
            }
        }



        /// <summary>
        /// To sort frame of actor by index 
        /// </summary>
        /// <param name="actor"></param>
        private void SortActorFrames(NativeAnimationFrameActor actor)
        {
            actor.frames.Sort((a, b) =>
            {
                if (a == null)
                {
                    if (b == null) return 0;
                    return -1;
                }
                else
                {
                    if (b == null) return 1;

                    return a.index.CompareTo(b.index);
                }
            });
        }
        /// <summary>
        /// To calculate body parts position 
        /// </summary>
        /// <param name="avatar">current loaded avatar</param>
        /// <param name="ikparent">current loaded avatar's IK parent</param>
        /// <param name="type">current target type</param>
        /// <param name="bounds">sample avatar's body bounds</param>
        /// <param name="lst">Value to calculate</param>
        /// <param name="vrmBone">Body parts to calculate</param>
        /// <param name="pelvisCondition">current loaded avatar's pelvis position</param>
        /// <returns>Calculated value</returns>
        /*public Vector3 CalculateReposition(GameObject avatar, GameObject ikparent, AF_TARGETTYPE type, float[] bounds, Vector3 position, ParseIKBoneType vrmBone, Vector3 pelvisCondition)
        { //---no use. futurely delete.
            
            //---current avatar body information
            //GameObject body = avatar.GetComponent<ManageAvatarTransform>().GetBodyMesh();
            Bounds bnd = avatar.GetComponent<OperateLoadedVRM>().GetTPoseBodyInfo(); //body.GetComponent<SkinnedMeshRenderer>().bounds;

            Vector3 loadTargetPelvis = Vector3.zero;
            Vector3 loadTargetExtents = Vector3.zero;
            Vector3 currentTargetExtents = Vector3.zero;

            //---To check difference of sample avatar body(base: T-pose) and current avatar body(base: T-pose).
            if (type == AF_TARGETTYPE.VRM) 
            {
                //---base is pelvis.
                loadTargetPelvis.y = bounds[PELVIS_Y] - pelvisCondition.y; //position.y;
                loadTargetExtents.x = bounds[HEIGHT_X];
                loadTargetExtents.y = bounds[HEIGHT_Y];
                loadTargetExtents.z = bounds[HEIGHT_Z];

                currentTargetExtents.x = bnd.extents.x * 2f;
                currentTargetExtents.y = bnd.extents.y * 2f;
                currentTargetExtents.z = bnd.extents.z * 2f;

            }
            Transform[] bts = ikparent.GetComponentsInChildren<Transform>();
            Transform boneTran = ikparent.transform.Find(IKBoneNames[(int)vrmBone]);

            Vector3 fnl;


            //---Absorb the difference in height.
            fnl.x = currentTargetExtents.x * (position.x / loadTargetExtents.x);
            if (vrmBone == ParseIKBoneType.Pelvis) 
            {
                //---Pelvis only: add sample result of "rest pelvis - pose pelvis"
                fnl.y = currentTargetExtents.y * (position.y / loadTargetExtents.y) + loadTargetPelvis.y;
            }
            else
            {
                fnl.y = currentTargetExtents.y * (position.y / loadTargetExtents.y);
            }

            fnl.z = currentTargetExtents.z * (position.z / loadTargetExtents.z);

            return new Vector3(fnl.x, fnl.y, position.z);
            
        }*/

        /// <summary>
        /// To calculate difference of height of VRM.
        /// </summary>
        /// <param name="currentActor">T-pose data of current target avatar</param>
        /// <param name="roller">T-pose data of the timeline actor</param>
        /// <param name="position">target pose data</param>
        /// <param name="vrmBone">target body part</param>
        /// <returns></returns>
        public Vector3 CalculateDifferenceInHeight(Vector3 currentActor, Vector3 roller, Vector3 position, ParseIKBoneType vrmBone)
        {
            Vector3 ret = Vector3.zero;
            Vector3 qv = Vector3.zero;

            //---to calculate a ratio
            /*
            if (roller.x != 0f) qv.x = currentActor.x / roller.x;
            if (roller.y != 0f) qv.y = currentActor.y / roller.y;
            if (roller.z != 0f) qv.z = currentActor.z / roller.z;
            */
            qv = currentActor - roller;

            //---to calculate finally
            /*
            if (qv.x == 0f) qv.x = 1f;
            if (qv.y == 0f) qv.y = 1f;
            if (qv.z == 0f) qv.z = 1f;
            */

            ret = position + qv;
            /*
            ret.x = position.x * qv.x;
            ret.y = position.y * qv.y;
            ret.z = position.z * qv.z;
            */

            return ret;
        }

        /// <summary>
        /// To calculate difference of height of VRM. (multiply percentage)
        /// </summary>
        /// <param name="currentActor"></param>
        /// <param name="roller"></param>
        /// <param name="position"></param>
        /// <param name="vrmBone"></param>
        /// <returns></returns>
        public Vector3 CalculateDifferenceByHeight(float[] currentActor, float[] roller, Vector3 position, ParseIKBoneType vrmBone, float is_x, float is_y, float is_z)
        {
            Vector3 ret = position;
            if (roller[0] != 0f)
            {
                float hx = currentActor[0] / roller[0];
                //hx = Mathf.Round(hx * 10000f) / 10000f;
                if (hx == 0f) hx = 1f;

                if (is_x == 0) ret.x = position.x;
                else ret.x = position.x * (hx * is_x);
            }
            if (roller[1] != 0f)
            {
                float hy = currentActor[1] / roller[1];
                //hy = Mathf.Round(hy * 10000f) / 10000f;
                if (hy == 0f) hy = 1f;

                if (is_y == 0) ret.y = position.y;
                else ret.y = position.y * (hy * is_y);
            }
            if (roller[2] != 0f)
            {
                float hz = currentActor[2] / roller[2];
                //hz = Mathf.Round(hz * 10000f) / 10000f;
                if (hz == 0f) hz = 1f;

                if (is_z == 0) ret.z = position.z;
                else ret.z = position.z * (hz * is_z);
            }

            return ret;
        }

        /// <summary>
        /// Calculate move data of all key-frame for specified avatar, apply absorb difference as this transform.
        /// </summary>
        /// <param name="nav">This time animation avatar</param>
        /// <param name="destination">Frame actor data to change</param>
        public void CalculateAllFrameForCurrent(NativeAnimationAvatar nav, NativeAnimationFrameActor destination)
        {
            //---This function is VRM only.
            if (destination.targetType != AF_TARGETTYPE.VRM)
            {
                return;
            }

            /*for (int di = 0; di < destination.bodyHeight.Length; di++)
            {
                Debug.Log("dest=" + destination.bodyHeight[di].ToString());
            }
            for (int ni = 0; ni < nav.bodyHeight.Length; ni++)
            {
                Debug.Log("nav=" + nav.bodyHeight[ni].ToString());
            }*/

            ParseIKBoneType[] sortedIndex = new ParseIKBoneType[IKbonesCount] {
                ParseIKBoneType.IKParent,


                ParseIKBoneType.Pelvis,ParseIKBoneType.Chest,
                ParseIKBoneType.Head,ParseIKBoneType.Aim,ParseIKBoneType.LookAt,

                ParseIKBoneType.LeftShoulder,
                ParseIKBoneType.LeftHand, ParseIKBoneType.LeftLowerArm,
                ParseIKBoneType.RightShoulder,
                ParseIKBoneType.RightHand, ParseIKBoneType.RightLowerArm,

                ParseIKBoneType.LeftLeg,ParseIKBoneType.LeftLowerLeg,
                ParseIKBoneType.RightLeg,ParseIKBoneType.RightLowerLeg,
                ParseIKBoneType.EyeViewHandle
            };
            List<ParseIKBoneType> sortArr = new List<ParseIKBoneType>(sortedIndex);

            for (int i = 0; i < destination.frames.Count; i++)
            {
                NativeAnimationFrame naframe = destination.frames[i];

                //---search ParseIKBone(Type) from movingData
                for (int m = 0; m < naframe.translateMovingData.Count; m++)
                {
                    //AnimationTargetParts movedata = naframe.movingData[m];
                    AnimationTranslateTargetParts movedata = naframe.translateMovingData[m];
                    
                    if (
                        (movedata.animationType == AF_MOVETYPE.Translate) && 
                        ((movedata.vrmBone >= ParseIKBoneType.EyeViewHandle) && (movedata.vrmBone <= ParseIKBoneType.RightLeg))
                    )
                    {
                        //vrmBone : SORTED !
                        //bodyInfoList: original sort
                        /*
                        // change motion difference: new:nav --> old:destination 
                        float[] curact = { nav.bodyHeight[0], nav.bodyInfoList[(int)movedata.vrmBone].y, nav.bodyInfoList[(int)movedata.vrmBone].z };
                        float[] dstact = { destination.bodyHeight[0], destination.bodyInfoList[(int)movedata.vrmBone].y, destination.bodyInfoList[(int)movedata.vrmBone].z };

                        
                        if (movedata.vrmBone == ParseIKBoneType.EyeViewHandle)
                        { //EyeViewHandle, y-axis is multiply VRM bound Y
                            curact[1] = nav.bodyHeight[1];
                            dstact[1] = destination.bodyHeight[1];
                        }
                        if ((movedata.vrmBone == ParseIKBoneType.LookAt) || (movedata.vrmBone == ParseIKBoneType.Aim))
                        { //Aim and LookAt, z-axis is fixed base value * VRM bound Z
                            curact[2] = 0.5f * nav.bodyHeight[2];
                            dstact[2] = 0.5f * destination.bodyHeight[2];
                        }
                        else
                        {
                            curact[2] = 1;
                            dstact[2] = 1;
                        }
                        for (int c = 0; c < curact.Length; c++) curact[c] = MathF.Round(curact[c], 6);
                        for (int c = 0; c < dstact.Length; c++) dstact[c] = MathF.Round(dstact[c], 6);
                        */

                        
                        //---judge sorted bone index, find real bone index
                        int sortInx = sortArr.FindIndex(v =>
                        {
                            if (v == movedata.vrmBone) return true;
                            return false;
                        });
                        if (sortInx >= 0)
                        {
                            int bodyInx = (int)sortArr[sortInx]; //bodyInfoList of SORTED index
                            for (var ti = 0; ti < movedata.values.Count; ti++)
                            {
                                Vector3 tranVal = movedata.values[ti];
                                //tranVal.x = Mathf.Round(tranVal.x * 100000f) / 100000f;
                                //tranVal.y = Mathf.Round(tranVal.y * 100000f) / 100000f;
                                //tranVal.z = Mathf.Round(tranVal.z * 100000f) / 100000f;
                                //---Round float value by string
                                string tmpx = tranVal.x.ToString("0.0000");
                                float ftmpx = float.Parse(tmpx);
                                {
                                    tranVal.x = ftmpx;
                                }
                                {
                                    tmpx = tranVal.y.ToString("0.0000");
                                    ftmpx = float.Parse(tmpx);
                                    tranVal.y = ftmpx;
                                }
                                {
                                    tmpx = tranVal.z.ToString("0.0000");
                                    ftmpx = float.Parse(tmpx);
                                    tranVal.z = ftmpx;
                                }


                                //--refer each parts position (original: global )
                                Vector3 cur_whole = nav.bodyInfoList[bodyInx];
                                Vector3 role_whole = destination.bodyInfoList[bodyInx];
                                
                                
                                {
                                    tmpx = role_whole.x.ToString("0.0000");
                                    role_whole.x = float.Parse (tmpx);
                                }
                                {
                                    tmpx = role_whole.y.ToString("0.0000");
                                    role_whole.y = float.Parse(tmpx);
                                }
                                {
                                    tmpx = role_whole.z.ToString("0.0000");
                                    role_whole.z = float.Parse(tmpx);
                                }

                                float cur_height_parts_y = (nav.bodyHeight[1] + cur_whole.y) / 2f;
                                float role_height_parts_y = (destination.bodyHeight[1] + role_whole.y) / 2f;
                                

                                float[] cur_whole_val = new float[3] { nav.bodyHeight[0], cur_height_parts_y, nav.bodyHeight[2] };
                                float[] role_whole_val = new float[3] { destination.bodyHeight[0], role_height_parts_y, destination.bodyHeight[2] };


                                //---body height version (original)
                                //float[] cur_whole_val = nav.bodyHeight;
                                //float[] role_whole_val = destination.bodyHeight;

                                //Debug.Log("vrmBone=" + movedata.vrmBone.ToString());
                                //Debug.Log($"  cur={cur_whole_val[0]}\t{cur_whole_val[1]}\t{cur_whole_val[2]}");
                                //Debug.Log($"  role={role_whole_val[0]}\t{role_whole_val[1]}\t{role_whole_val[2]}");
                                //Debug.Log($"  tranVal={tranVal.x}\t{tranVal.y}\t{tranVal.z}");
                                //^^^ keyframe: local 

                                Vector3 newpos = CalculateDifferenceByHeight(cur_whole_val, role_whole_val, tranVal, movedata.vrmBone, 1f, 1f, 1f);
                                //Debug.Log($"  newpos={newpos.x}\t{newpos.y}\t{newpos.z}");

                                destination.frames[i].translateMovingData[m].values[ti] = newpos;
                            }
                        }
                        



                        //---not by whole height, but a height of EACH PARTS !!
                        //Vector3 newpos = CalculateDifferenceByHeight(nav.bodyHeight, destination.bodyHeight, movedata.position, movedata.vrmBone, 1, 1, 0);

                        /*for (var ti = 0; ti < movedata.values.Count; ti++)
                        {
                            Vector3 tranVal = movedata.values[ti];

                            float[] cur_whole_val = nav.bodyHeight;
                            float[] role_whole_val = destination.bodyHeight;

                            Vector3 newpos = CalculateDifferenceByHeight(cur_whole_val, role_whole_val, tranVal, movedata.vrmBone, 1f, 1f, 1f);

                            destination.frames[i].translateMovingData[m].values[ti] = newpos;
                        }*/
                        //Vector3 newpos = CalculateDifferenceByHeight(curact, dstact, movedata.position, movedata.vrmBone, 1, 1, 1);


                        //destination.frames[i].movingData[m].position = newpos;
                    }
                }
            }
            //===USER OPERATION: update also the height of frame actor
        }


        //===Each properties edit function====================================================================================================

        public void GetThisTimeFromOuter(string param)
        {
            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);
            NativeAnimationFrameActor actor = GetFrameActorFromRole(aro.targetRole, aro.targetType);
            NativeAnimationFrame curframe = GetFrame(actor, aro.index);
            float allduration = actor.frames.Select(x => x.duration).Sum();
            float sumduration = 0;
            for (int i = 0; i < actor.frames.Count; i++)
            {
                var frame = actor.frames[i];
                if (frame.index <= aro.index)
                {
                    sumduration += frame.duration;
                }
                else
                {
                    break;
                }
            }
            string js = sumduration.ToString() + "," + allduration.ToString();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }
        public void GetMemoFromOuter(string param)
        {
            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);
            NativeAnimationFrameActor actor = GetFrameActorFromRole(aro.targetRole, aro.targetType);
            NativeAnimationFrame curframe = GetFrame(actor, aro.index);

            string ret = "";
            if (curframe != null)
            {
                ret = curframe.memo;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public void SetMemo(string param)
        {
            if (currentProject.isReadOnly || currentProject.isSharing) return;

            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }
            string js = "{ }";

            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);

            NativeAnimationFrameActor actor = GetFrameActorFromRole(aro.targetRole, aro.targetType);

            int nearmin = GetNearMinFrameIndex(actor, aro.index);
            NativeAnimationFrame minframe = null;
            int minframeIndex = -1;
            if (minframe != null)
            {
                minframe = actor.frames[nearmin]; // GetFrame(actor, nearmin);
                minframeIndex = minframe.index;
            }

            NativeAnimationFrame curframe = GetFrame(actor, aro.index);
            if (curframe != null)
            {
                //---update "ease" only
                curframe.memo = aro.memo;

                js = "{ " +
                    "\"roleName\": \"" + actor.targetRole + "\"," +
                    "\"avatarId\" : \"" + actor.targetId + "\"," +
                    "\"type\": " + (int)aro.targetType + "," +
                    "\"nearMinIndex\": " + minframeIndex + "," +
                    "\"index\":" + aro.index +
                "}";
            }

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }
        public void GetEaseFromOuter(string param)
        {
            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);
            NativeAnimationFrameActor actor = GetFrameActorFromRole(aro.targetRole, aro.targetType);
            NativeAnimationFrame curframe = GetFrame(actor, aro.index);

            int js = -1;
            if (curframe != null)
            {
                js = (int)curframe.ease;
            }
            else
            {
                js = (int)Ease.Linear;
            }
            
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(js);
#endif

        }
        /// <summary>
        /// Set Easing
        /// </summary>
        /// <param name="param">JSON-string for AnimationRegisterOptions</param>
        public void SetEase(string param)
        {
            if (currentProject.isReadOnly || currentProject.isSharing) return;

            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }
            string js = "{ }";

            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);

            NativeAnimationFrameActor actor = GetFrameActorFromRole(aro.targetRole, aro.targetType);

            int nearmin = GetNearMinFrameIndex(actor, aro.index);
            NativeAnimationFrame minframe = null;
            int minframeIndex = -1;
            if (minframe != null)
            {
                minframe = actor.frames[nearmin]; // GetFrame(actor, nearmin);
                minframeIndex = minframe.index;
            }
            
            NativeAnimationFrame curframe = GetFrame(actor, aro.index);
            if (curframe != null)
            {
                //---update "ease" only
                curframe.ease = aro.ease;

                js = "{ " +
                    "\"roleName\": \"" + actor.targetRole + "\"," +
                    "\"avatarId\" : \"" + actor.targetId + "\"," +
                    "\"type\": " + (int)aro.targetType + "," +
                    "\"nearMinIndex\": " + minframeIndex + "," +
                    "\"index\":" + aro.index +
                "}";
            }        
            
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }
        public void GetDurationFromOuter(string param)
        {
            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);

            NativeAnimationFrameActor actor = GetFrameActorFromRole(aro.targetRole, aro.targetType);

            NativeAnimationFrame curframe = GetFrame(actor, aro.index);
            float js = 0;
            if (curframe != null)
            {
                js = curframe.duration;
            }
            else
            {
                js = 0;
            }
            
            //Debug.Log("duration="+js.ToString());
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(js);
#endif
        }
        /// <summary>
        /// Set duration manually
        /// </summary>
        /// <param name="param">JSON-string for AnimationRegisterOptions</param>
        public void SetDuration(string param)
        {
            if (currentProject.isReadOnly || currentProject.isSharing) return;

            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }

            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);

            NativeAnimationFrameActor actor = GetFrameActorFromRole(aro.targetRole, aro.targetType);
            NativeAnimationFrame curframe = GetFrame(actor, aro.index);
            if (curframe != null)
            {
                curframe.duration = aro.duration;
            }
        }
        public void SetBaseDuration(float param)
        {
            if (param != 0f)
            {
                currentProject.baseDuration = param;
            }
        }
        public void ApplyBaseDuration(float param)
        {
            //---set to project
            SetBaseDuration(param);

            //---reset all actor keyframes.
            foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
            {
                AdjustAllFrameDuration(actor, currentProject.baseDuration);
            }

            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }
        }
        public void GetBaseDurationFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(currentProject.baseDuration);
#endif
        }
        /// <summary>
        /// Reset frame duration.(specified frame. all frame is loop at JS)
        /// </summary>
        /// <param name="param">JSON-string for AnimationRegisterOptions</param>
        public void ResetAutoDuration(string param)
        {
            if (currentProject.isReadOnly || currentProject.isSharing) return;

            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }

            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);

            NativeAnimationFrameActor actor = GetFrameActorFromRole(aro.targetRole, aro.targetType);
            NativeAnimationFrame curframe = GetFrame(actor, aro.index);
            int nearMinIndex = GetNearMinFrameIndex(actor, aro.index);
            NativeAnimationFrame minframe = actor.frames[nearMinIndex];
            if (curframe != null)
            {
                int dist = 1;
                if (minframe != null)
                {
                    dist = curframe.index - minframe.index;
                }
                else
                {
                    dist = curframe.index;
                }
                curframe.duration = currentProject.baseDuration * (float)dist;
            }
            //AdjustAllFrameDuration(actor, currentProject.baseDuration);
        }

        /// <summary>
        /// Get specified range of duration of specified avatar(timeline, role)
        /// </summary>
        /// <param name="param">[0] - role name, [1] - start frame number, [2] - end frame number</param>
        public void GetAvatarDurationBetween(string param)
        {
            string[] prms = param.Split(",");
            int st = int.TryParse(prms[1], out st) ? st : 0;
            int ed = int.TryParse(prms[2], out ed) ? ed : currentProject.timelineFrameLength;
            NativeAnimationAvatar nav = GetCastInProject(prms[0]);
            float ret = 0f;

            if (nav != null)
            {
                NativeAnimationFrameActor naf =  GetFrameActorFromRole(nav.roleName, nav.type);
                if (naf != null)
                {
                    for (int i = 0; i < naf.frames.Count; i++)
                    {
                        if ((st <= naf.frames[i].index) && (naf.frames[i].index <= ed))
                        {
                            ret += naf.frames[i].duration;
                        }
                    }
                }
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
        }
        public void GetRegisteredBonesFromOuter(string param)
        {
            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);

            NativeAnimationFrameActor actor = GetFrameActorFromRole(aro.targetRole, aro.targetType);

            NativeAnimationFrame curframe = GetFrame(actor, aro.index);

            List<string> retarr = new List<string>();
            
            curframe.translateMovingData.ForEach(m =>
            {
                retarr.Add(m.vrmBone.ToString());
            });
            int ishitprop = curframe.movingData.FindIndex(m =>
            {
                if ((m.animationType != AF_MOVETYPE.Translate) && (m.animationType != AF_MOVETYPE.Rotate) && (m.animationType != AF_MOVETYPE.Scale) &&
                    (m.animationType != AF_MOVETYPE.Punch) && (m.animationType != AF_MOVETYPE.Shake)
                )
                {
                    return true;
                }
                return false;
            });
            if (ishitprop > -1)
            {
                retarr.Add("props");
            }

            string js = string.Join(",",retarr);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }

        /// <summary>
        /// To check wheather specified int value exist in the actor frames.
        /// </summary>
        /// <param name="param">0 - role, 1 - role type, 2 - begin frame, 3 - end frame</param>
        public void CheckTargetFrameIndexList(string param)
        {
            string[] prms = param.Split("\t");
            string targetRole = prms[0];
            int itype = int.TryParse(prms[1], out itype) ? itype : 0;
            AF_TARGETTYPE targetType = (AF_TARGETTYPE)itype;
            int beginIndex = int.TryParse(prms[2], out beginIndex) ? beginIndex : 0;
            int endIndex = int.TryParse(prms[3], out endIndex) ? endIndex : 0;

            string js = "";

            NativeAnimationFrameActor naf = GetFrameActorFromRole(targetRole, targetType);
            if (naf != null)
            {
                List<string> intlist = new List<string>();
                for (int i = beginIndex; i <= endIndex; i++)
                {
                    NativeAnimationFrame nafr = GetFrame(naf, i);
                    if (nafr != null)
                    {
                        intlist.Add(i.ToString());
                    }
                }
                js = string.Join(",",intlist);
            }
            else
            {
                js = "-1";
            }
            

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }

        /// <summary>
        /// Set global transform only to specified frame
        /// </summary>
        /// <param name="param">AnimationTransformRegisterOptions</param>
        public void SetGlobalTransform(string param)
        {
            if (currentProject.isReadOnly || currentProject.isSharing) return;

            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }

            AnimationTransformRegisterOptions atro = JsonUtility.FromJson<AnimationTransformRegisterOptions>(param);
            NativeAnimationFrameActor actor = GetFrameActorFromRole(atro.targetRole, atro.targetType);
            NativeAnimationFrame curframe = GetFrame(actor, atro.index);

            
            //Debug.Log(param);
            //Debug.Log("isAbsolutePosition=" + atro.isAbsolutePosition.ToString());
            //Debug.Log("posx=" + atro.posx.ToString());
            //Debug.Log("posy=" + atro.posy.ToString());
            //Debug.Log("posz=" + atro.posz.ToString());

            if (curframe != null)
            {
                Debug.Log(curframe.index);

                for (int i = 0; i < curframe.translateMovingData.Count; i++)
                {
                    AnimationTranslateTargetParts attp = curframe.translateMovingData[i];
                    if (attp.vrmBone == ParseIKBoneType.IKParent)
                    {
                        for (int j = 0; j < attp.values.Count; j++)
                        {
                            Vector3 v = attp.values[j];
                            if (atro.isAbsolutePosition == 1)
                            {
                                v.x = atro.posx;
                                v.y = atro.posy;
                                v.z = atro.posz;
                            }
                            else
                            {
                                v.x += atro.posx;
                                v.y += atro.posy;
                                v.z += atro.posz;
                            }
                            attp.values[j] = v;
                        }
                    }
                }
                foreach (AnimationTargetParts atp in curframe.movingData)
                {
                    if (atp.vrmBone == ParseIKBoneType.IKParent)
                    {
                        if (atp.animationType == AF_MOVETYPE.Rotate)
                        {
                            if (atro.isAbsoluteRotation == 1)
                            {
                                atp.rotation.x = atro.rotx;
                                atp.rotation.y = atro.roty;
                                atp.rotation.z = atro.rotz;
                            }
                            else
                            {
                                atp.rotation.x += atro.rotx;
                                atp.rotation.y += atro.roty;
                                atp.rotation.z += atro.rotz;
                            }


                            //curframe.SetMovingData(AF_MOVETYPE.Rotate, ParseIKBoneType.IKParent, atp);
                        }
                    }
                }
                /*AnimationTargetParts atp_translate = curframe.FindMovingData(AF_MOVETYPE.Translate, ParseIKBoneType.IKParent);
                if (atp_translate != null)
                {
                    
                }

                AnimationTargetParts atp_rotation = curframe.FindMovingData(AF_MOVETYPE.Rotate, ParseIKBoneType.IKParent);
                if (atp_rotation != null)
                {
                    
                }*/
            }

        }


        //=================================================================================================================================================================
        //  Play functions
        //=================================================================================================================================================================

        /// <summary>
        /// Parse body of Animation Process. (apply ease at end)
        /// </summary>
        /// <param name="keyFrameSeq"></param>
        /// <param name="targetObject"></param>
        /// <param name="frame"></param>
        /// <param name="aro"></param>
        /// <returns>One key-frame's sequence</returns>
        private Sequence ProcessBody_forFrame(Sequence keyFrameSeq, NativeAnimationFrameActor targetObject, NativeAnimationFrame frame, AnimationParsingOptions aro)
        {
            bool IsExecTargetCheck = ((targetObject.avatar.avatar != null) && (targetObject.avatar.ikparent != null));
            if ((targetObject.targetType == AF_TARGETTYPE.Text) || (targetObject.targetType == AF_TARGETTYPE.UImage))
            { //---re-check role and avatar connection if avatar is text and Uimage ? (2D)
                IsExecTargetCheck = (targetObject.avatar.avatar != null);
            }
            if (IsExecTargetCheck)
            {
                /*AnimationTargetParts pelvisatp = frame.movingData.Find(item =>
                {
                    if (item.vrmBone == ParseIKBoneType.Pelvis)
                    {
                        return true;
                    }
                    return false;
                });*/
                //---DO NOT set this timing.
                //keyFrameSeq.SetEase(frame.ease);

                
                //---moving data loop for 1 avatar
                //for (int i = frame.movingData.Count - 1; i >= 0; i--)

                //---loop of translateMovingData
                for (int i = 0; i < frame.translateMovingData.Count; i++)
                {
                    AnimationTranslateTargetParts transdata = frame.translateMovingData[i];
                    //---execute for Translate
                    keyFrameSeq = ParseForTranslateCommon(keyFrameSeq, frame, transdata, targetObject, null, aro);
                }
                

                //---loop of movingData
                for (int i = 0; i < frame.movingData.Count; i++)
                {
                    AnimationTargetParts movedata = frame.movingData[i];

                    if (targetObject.targetType == AF_TARGETTYPE.SystemEffect)
                    {
                        keyFrameSeq = ParseForSystemEffect(keyFrameSeq, frame, movedata, targetObject, aro);
                    }
                    else
                    {
                        //---execute for Rotate, Scale, Jump, Shake
                        keyFrameSeq = ParseForCommon(keyFrameSeq, frame, movedata, targetObject, null, aro);

                        if (targetObject.targetType == AF_TARGETTYPE.VRM)
                        {
                            keyFrameSeq = ParseForVRM(keyFrameSeq, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.OtherObject)
                        {
                            keyFrameSeq = ParseForOtherObject(keyFrameSeq, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.Light)
                        {
                            keyFrameSeq = ParseForLight(keyFrameSeq, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.Camera)
                        {
                            keyFrameSeq = ParseForCamera(keyFrameSeq, frame, movedata, targetObject, aro);
                        }
                        else if ((targetObject.targetType == AF_TARGETTYPE.Text) || (targetObject.targetType == AF_TARGETTYPE.Text3D))
                        {
                            keyFrameSeq = ParseForText(keyFrameSeq, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.Image)
                        {
                            keyFrameSeq = ParseForImage(keyFrameSeq, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.UImage)
                        {
                            keyFrameSeq = ParseForUImage(keyFrameSeq, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.Audio)
                        {
                            keyFrameSeq = ParseForAudio(keyFrameSeq, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.Effect)
                        {
                            keyFrameSeq = ParseForEffect(keyFrameSeq, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.Stage)
                        {
                            keyFrameSeq = ParseForStage(keyFrameSeq, frame, movedata, targetObject, aro);
                        }
                    }

                }

            }
            return keyFrameSeq;
        }
        /// <summary>
        /// Calculate indicated frame duration between 2 keyframes.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="aro"></param>
        /// <param name="maxdex"></param>
        /// <returns></returns>
        public float GetVirtualFrameBetweenKeyFrames(NativeAnimationFrameActor actor, AnimationParsingOptions aro, int mindex, int maxdex, int direction)
        {
            //int mindex = GetNearMinFrameIndex(actor, aro.index);
            //int maxdex = GetNearMaxFrameIndex(actor, aro.index);
            //if (mindex == -1) mindex = 0;

            NativeAnimationFrame minframe = actor.frames[mindex]; // GetFrame(actor, mindex);
            NativeAnimationFrame maxframe = actor.frames[maxdex]; // GetFrame(actor, maxdex);

            float totaldistance = (float)maxframe.index - (float)minframe.index;
            float cur2totaldist = 0f;
            if (totaldistance != 0f)
            {
                if (direction == -1)
                { //--- back 
                    cur2totaldist = ((float)(maxframe.index - aro.index) / totaldistance);
                }
                else if (direction == 1)
                { //---progress
                    cur2totaldist = 1f - ((float)(maxframe.index - aro.index) / totaldistance);
                }
                else
                {
                    cur2totaldist = 0f;
                }

            }

            float ret = maxframe.duration * cur2totaldist;

            return ret;
        }

        /// <summary>
        /// Load and Play the single timeline frames (all avatar, if no specify)
        /// (Neccesary: NativeAnimationAvatar[])
        /// </summary>
        /// <param name="param">parsing option(JSON-format)</param>
        public void PreviewSingleFrame(string param)
        {
            AnimationParsingOptions aro = JsonUtility.FromJson<AnimationParsingOptions>(param);
            PreviewSingleFrame(aro);
        }

        /// <summary>
        /// Body of the core for PreviewSingleFrame 
        /// </summary>
        /// <param name="animateFlow"></param>
        /// <param name="actor"></param>
        /// <param name="aro"></param>
        /// <param name="isFinalize"></param>
        /// <returns></returns>
        protected PreviewFrameReturner PreviewProcessBody(Sequence animateFlow, NativeAnimationFrameActor actor, AnimationParsingOptions aro, bool isFinalize)
        {
            PreviewFrameReturner pref = new PreviewFrameReturner();
            pref.seq = DOTween.Sequence();

            if (actor.avatar.avatar != null)
            {
                //---if compiled animation, disable IK marker.
                actor.compiled = currentPlayingOptions.isCompileAnimation;
                if (currentPlayingOptions.isCompileAnimation == 1)
                {
                    OperateLoadedBase ovrm = actor.avatar.avatar.GetComponent<OperateLoadedBase>();
                    if (ovrm != null)
                    {
                        ovrm.EnableIK(false);
                    }

                }

                NativeAnimationFrame frame = GetFrame(actor, aro.index, isFinalize);

                bool isVirtualFrame = false;
                float curdist = -1f;
                int targetFrameIndex = aro.index;
                int backupFrameIndex = aro.index;
                float backupDuration = 0f;
                int mindex = -1;
                int maxdex = -1;
                int bkup_isBuildDoTween = aro.isBuildDoTween;

                if (frame == null)
                { //---Search a frame between 2 key-frames.

                    //---back to preview frame
                    mindex = GetNearMinFrameIndex(actor, aro.index);
                    if (mindex == -1) mindex = 0;

                    //---progress to future frame
                    maxdex = GetNearMaxFrameIndex(actor, aro.index);
                    if (configLab.GetIntVal("recover_tomax_overframe",1) == 1)
                    {
                        if (maxdex == -1) maxdex = actor.frames.Count - 1;
                    }
                    

                    if (mindex == maxdex)
                    { //---if over last position, set to last position
                        frame = actor.frames[mindex];
                        targetFrameIndex = frame.index;
                    }
                    else if (maxdex == -1)
                    { //---if max index is over actor.frames, do not preview motion

                    }
                    else if (maxdex > -1)
                    {
                        //---necessarily use nearly maximum frame
                        NativeAnimationFrame maxframe = actor.frames[maxdex];

                        int calcFlag = 0;
                        if (aro.index < oldPreviewMarker)
                        { //---to refer a frame BEFORE previous frame
                            frame = actor.frames[mindex];
                            calcFlag = -1;

                            //---start frame
                            float bkupdur = actor.frames[maxdex].duration;
                            actor.frames[maxdex].duration = 0f;
                            Sequence nseq = DOTween.Sequence();
                            ProcessBody_forFrame(nseq, actor, actor.frames[maxdex], aro);
                            animateFlow.Append(nseq);
                            actor.frames[maxdex].duration = bkupdur;
                        }
                        else if (aro.index > oldPreviewMarker)
                        { //---to refer a frame AFTER previous frame
                            frame = maxframe;
                            calcFlag = 1;

                            //---start frame
                            float bkupdur = actor.frames[mindex].duration;
                            actor.frames[mindex].duration = 0f;
                            Sequence nseq = DOTween.Sequence();
                            ProcessBody_forFrame(nseq, actor, actor.frames[mindex], aro);
                            animateFlow.Append(nseq);
                            actor.frames[mindex].duration = bkupdur;
                        }
                        else
                        {
                            frame = GetFrame(actor, aro.index);
                        }

                        if (frame != null)
                        {
                            backupDuration = frame.duration;

                            // mindex - aro.index - maxdex : calculate duration of aro.index
                            
                            curdist = GetVirtualFrameBetweenKeyFrames(actor, aro, mindex, maxdex, calcFlag);

                            //---executing frame is near max frame.
                            targetFrameIndex = frame.index;
                            isVirtualFrame = true;

                            //---When back to previous frame also, use the duration of future frame.
                            frame.duration = maxframe.duration;
                            aro.isBuildDoTween = 1;
                        }
                    }
                }

                if (frame != null)
                {
                    /*
                     * frame: direct hit OR nearly hit (min / max) keyframe
                     * TODO:
                     *   ボーン単位で直接ヒット OR 最も近いキーフレームのボーンを先行して取得に変える ≒ MMD
                     *   -> 
                     */
                    aro.index = targetFrameIndex;
                    Sequence nseq = DOTween.Sequence();
                    TweenCallback cb_end = () => {
                        //---for general animation clip
                        SetGeneralAnimationFrame(actor.avatar, aro.index, frame);
                        SetVRMAnimationFrame(actor.avatar, aro.index, frame);
                        /*
                        VVMMotionRecorder vmrec = actor.avatar.avatar.GetComponent<VVMMotionRecorder>();
                        if (vmrec != null)
                        {
                            if (vmrec.IsExistFrame(aro.index))
                            {
                                vmrec.ModifyKeyFrame(aro.index, frame.ease, frame.duration);
                            }
                            else
                            {
                                vmrec.AddKeyFrame(aro.index, frame.ease, frame.duration);
                            }
                        }*/
                    };
                    animateFlow.OnKill(cb_end);
                    animateFlow.OnComplete(cb_end);
                    ProcessBody_forFrame(nseq, actor, frame, aro);
                    animateFlow.Append(nseq);

                    aro.isBuildDoTween = bkup_isBuildDoTween;
                }
                pref.currentDuration = curdist;
                pref.backupDuration = backupDuration;
                if (isVirtualFrame)
                { //---frame is virtual frame
                    pref.index = frame.index;
                    //Debug.Log(frame.index + ": " + curdist.ToString() + " / " + frame.duration);
                    animateFlow.Goto(curdist);
                    //---recover duration of previous frame
                    frame.duration = backupDuration;
                }
                /*else
                { //---not virtual frame (normal performing)
                    if (frame != null)
                    {
                        animateFlow.Goto(frame.duration);
                    }
                    
                }*/

                //---recover option's index
                aro.index = backupFrameIndex;

                //animateFlow.Append(seq);
            }

            return pref;
        }

        /// <summary>
        /// Load and Play the single timeline frames (all avatar, if no specify)
        /// (Neccesary: NativeAnimationAvatar[])
        /// </summary>
        /// <param name="aro">parsing option</param>
        public void PreviewSingleFrame(AnimationParsingOptions aro)
        {
            try
            {
                bool isFinalize = false;
                if (aro.finalizeIndex > 0)
                {
                    isFinalize = true;
                }
                //Debug.Log("aro.index="+aro.index);
                currentPlayingOptions = aro;

                Sequence animateFlow = DOTween.Sequence();
                //animateFlow.SetAutoKill(false);

                //IsBoneLimited = false;
                TweenCallback cb_start = () =>
                {
                    IsPreview = true;
                    /*
                    IsBoneLimited = false;
                    OldIsLimitedPelvis = IsLimitedPelvis;
                    OldIsLimitedArms = IsLimitedArms;
                    OldIsLimitedLegs = IsLimitedLegs;
                    IsLimitedPelvis = false;
                    IsLimitedArms = false;
                    IsLimitedLegs = false;
                    */
                };

                animateFlow.OnPlay(cb_start);
                animateFlow.OnRewind(cb_start);


                NativeAnimationFrameActor naf = GetFrameActorFromRole(aro.targetRole, aro.targetType);
                if ((naf != null) && (naf.avatar.avatar != null) && (LayerMask.LayerToName(naf.avatar.avatar.layer) != "HiddenPlayer"))
                {
                    //Sequence seq = DOTween.Sequence();
                    PreviewFrameReturner pref = PreviewProcessBody(animateFlow, naf, aro, isFinalize);
                    //animateFlow.Join(seq); 
                }

                else
                {
                    foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
                    {
                        if ((actor.avatar.avatar != null) && (LayerMask.LayerToName(actor.avatar.avatar.layer) != "HiddenPlayer"))
                        {
                            //Sequence seq = DOTween.Sequence();
                            PreviewFrameReturner pref = PreviewProcessBody(animateFlow, actor, aro, isFinalize);
                            //animateFlow.Join(seq);
                        }

                    }
                }


                //---Entrust execution to Goto of PreviewProcessBody.
                /*
                if (aro.index >= oldPreviewMarker)
                {
                    animateFlow.PlayForward();
                }
                else
                {
                    animateFlow.PlayBackwards();
                }
                */
                //animateFlow.Play();
                //animateFlow.Kill();
            }
            catch (Exception err) 
            {
                Debug.Log("PreviewSingleFrame: " + err.Message);
                Debug.LogError(err.StackTrace);
            }
            

        }
        public void PreparePreviewMarker()
        {
            IsBoneLimited = false;
            OldIsLimitedPelvis = IsLimitedPelvis;
            OldIsLimitedArms = IsLimitedArms;
            OldIsLimitedLegs = IsLimitedLegs;
            OldIsLimitedChest = IsLimitedChest;
            OldIsLimitedAim = IsLimitedAim;
            IsLimitedPelvis = false;
            IsLimitedArms = false;
            IsLimitedLegs = false;
            IsLimitedChest = false;
            IsLimitedAim = false;
        }
        public void FinishPreviewMarker()
        {
            DOVirtual.DelayedCall(0.15f, () =>
            {
                if (currentPlayingOptions.isCompileAnimation == 1)
                {
                    foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
                    {
                        if ((actor.avatar != null) && (actor.avatar.avatar != null))
                        {
                            OperateLoadedBase ovrm = actor.avatar.avatar.GetComponent<OperateLoadedBase>();

                            if (actor.targetType == AF_TARGETTYPE.VRM)
                            {
                                OperateLoadedVRM olvrm = actor.avatar.avatar.GetComponent<OperateLoadedVRM>();
                                olvrm.ListGravityInfo();
                            }
                        }

                    }
                }
                IsPreview = false;
                //IsBoneLimited = true;
                
                //IsLimitedPelvis = OldIsLimitedPelvis;
                //IsLimitedArms = OldIsLimitedArms;
                //IsLimitedLegs = OldIsLimitedLegs;

#if !UNITY_EDITOR && UNITY_WEBGL
            SendPlayingPreviewAnimationInfoOnComplete(currentPlayingOptions.finalizeIndex < 0 ? currentPlayingOptions.index : currentPlayingOptions.finalizeIndex);
#endif
            });
        }
        public void FinishPreviewMarker2()
        {

            IsLimitedPelvis = OldIsLimitedPelvis;
            IsLimitedArms = OldIsLimitedArms;
            IsLimitedLegs = OldIsLimitedLegs;
            IsLimitedChest = OldIsLimitedChest;
            IsLimitedAim = OldIsLimitedAim;
        }
        public void BackupPreviewMarker()
        {
            oldPreviewMarker = currentPlayingOptions.index;
        }
        public void RecoverBoneLimitatonMarker(string param)
        {
            string [] js = param.Split("\t");
            int hingeflag = int.TryParse(js[0], out hingeflag) ? hingeflag : 0;
            SetHingeLimited(hingeflag);

            //limitation Pelvis
            if (js.Length >= 2) SetLimitedBones(js[1]);
            if (js.Length >= 3) SetLimitedBones(js[2]);
            if (js.Length >= 4) SetLimitedBones(js[3]);
        }
        /// <summary>
        /// To animate previewly ONE actor (Pose load only)
        /// </summary>
        /// <param name="actor">target frame actor</param>
        /// <param name="aro">options</param>
        public void PreviewSingleFrame(NativeAnimationFrameActor actor, AnimationParsingOptions aro)
        {
            Sequence animateFlow = DOTween.Sequence();
            //IsPreview = true;
            //IsBoneLimited = false;
            currentPlayingOptions = aro;

            animateFlow.OnPlay(() =>
            {
                IsPreview = true;
                /*
                IsBoneLimited = false;

                OldIsLimitedPelvis = IsLimitedPelvis;
                OldIsLimitedArms = IsLimitedArms;
                OldIsLimitedLegs = IsLimitedLegs;
                IsLimitedPelvis = false;
                IsLimitedArms = false;
                IsLimitedLegs = false;
                */
            });
            /*animateFlow.OnComplete(() =>
            {
                IsPreview = false;
                IsBoneLimited = true;
            });*/
            animateFlow.OnKill(() =>
            {

                if (currentPlayingOptions.isCompileAnimation == 1)
                {
                    foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
                    {
                        if ((actor.avatar != null) && (actor.avatar.avatar != null))
                        {
                            OperateLoadedBase ovrm = actor.avatar.avatar.GetComponent<OperateLoadedBase>();
                            if (ovrm != null)
                            {
                                ovrm.EnableIK(true);
                            }
                            if (actor.targetType == AF_TARGETTYPE.VRM)
                            {
                                OperateLoadedVRM olvrm = actor.avatar.avatar.GetComponent<OperateLoadedVRM>();
                                olvrm.ListGravityInfo();
                            }
                            

                        }
                        
                    }
                }
                IsPreview = false;
                /*
                IsBoneLimited = true;
                IsLimitedPelvis = OldIsLimitedPelvis;
                IsLimitedArms = OldIsLimitedArms;
                IsLimitedLegs = OldIsLimitedLegs;
                */
                
#if !UNITY_EDITOR && UNITY_WEBGL
                SendPlayingPreviewAnimationInfoOnComplete(currentPlayingOptions.index);
#endif
            });

            PreviewProcessBody(animateFlow, actor, aro, false);
        }
        public void StartAllTimeline(string param)
        {
            StartAllTimeline(JsonUtility.FromJson<AnimationParsingOptions>(param));
        }
        public void StartAllTimeline(AnimationParsingOptions apo)
        {
            currentPlayingOptions = apo; // JsonUtility.FromJson<AnimationParsingOptions>(param);

            if ((currentSeq != null) && 
                (currentPlayingOptions.isRebuildAnimation == 0) && 
                (currentPlayingOptions.isLoop == isOldLoop) && 
                (currentSeq.IsActive())
            )
            {//---To play only-------------------------------
                /*
                IsBoneLimited = false;
                OldIsLimitedPelvis = IsLimitedPelvis;
                OldIsLimitedArms = IsLimitedArms;
                OldIsLimitedLegs = IsLimitedLegs;
                IsLimitedPelvis = false;
                IsLimitedArms = false;
                IsLimitedLegs = false;
                */
                currentSeq.Restart();
            }
            else
            {//---To build and play--------------------------
                isOldLoop = currentPlayingOptions.isLoop;

                currentSeq.Kill();
                //=======================================
                // Most parent sequence for timeline
                currentSeq = DOTween.Sequence();
                currentSeq.SetAutoKill(false);
                currentSeq.SetLink(gameObject);
                if (currentPlayingOptions.isLoop == 1)
                {
                    currentSeq.SetLoops(-1, LoopType.Restart);
                }
                TweenCallback cb_start = () =>
                {
                    //Debug.Log("Re-start an animation!");
                    IsPause = false;
                    IsPlaying = true;
                    /*
                    IsBoneLimited = false;
                    OldIsLimitedPelvis = IsLimitedPelvis;
                    OldIsLimitedArms = IsLimitedArms;
                    OldIsLimitedLegs = IsLimitedLegs;
                    IsLimitedPelvis = false;
                    IsLimitedArms = false;
                    IsLimitedLegs = false;
                    */

                    foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
                    {
                        actor.frameIndexMarker = 0;
                        //---other, setting for each avatar type
                        if (actor.avatar.type == AF_TARGETTYPE.VRM)
                        {
                            if (currentPlayingOptions.isCompileAnimation == 1)
                            {
                                OperateLoadedVRM olvrm = actor.avatar.avatar.GetComponent<OperateLoadedVRM>();
                                StartCoroutine(olvrm.EnableIKOperationMode(false));
                            }

                        }
                    }
                };
                currentSeq.OnPlay(cb_start);
                currentSeq.OnRewind(cb_start);
                currentSeq.OnPause(() =>
                {
                    //Debug.Log("An animation paused.");
                    IsPause = true;
#if !UNITY_EDITOR && UNITY_WEBGL
                    SendPlayingAnimationInfoOnPause(seqInIndex);
#endif

                });
                TweenCallback cb_endcall = () =>
                {
                    DOVirtual.DelayedCall(currentPlayingOptions.endDelay, () =>
                    {
                        /*
                        AnimationParsingOptions aro = new AnimationParsingOptions();
                        aro.index = 1;
                        aro.finalizeIndex = 1;
                        aro.isExecuteForDOTween = 0;
                        PreviewSingleFrame(aro);
                        */
                        /*if (currentPlayingOptions.isShowIK == 0)
                        {
                            CameraOperation1 co1 = Camera.main.gameObject.GetComponent<CameraOperation1>();
                            co1.EnableHandleShowCamera(1);
                        }*/
                        
                    });
                    foreach (NativeAnimationAvatar actor in currentProject.casts)
                    {
                        if (actor.type == AF_TARGETTYPE.VRM)
                        {
                            if (actor.avatar != null)
                            {
                                OperateLoadedVRM olvrm = actor.avatar.GetComponent<OperateLoadedVRM>();
                                olvrm.ListGravityInfo();

                                if (!IsRecordingOtherMotion)
                                {
                                    ManageAvatarTransform mat = actor.avatar.GetComponent<ManageAvatarTransform>();
                                    if (mat != null)
                                    {
                                        //mat.EndRecordBVH();
                                    }
                                }
                                
                            }
                            
                        }
                    }
                    foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
                    {
                        actor.frameIndexMarker = 0;
                    }
                    //StopAllTimeline();
                    IsPause = false;
                    IsPlaying = false;
                    if (IsRecordingOtherMotion)
                    {
                        IsRecordingOtherMotion = false;
                    }

                    /*
                    IsBoneLimited = true;
                    
                    IsLimitedPelvis = OldIsLimitedPelvis;
                    IsLimitedArms = OldIsLimitedArms;
                    IsLimitedLegs = OldIsLimitedLegs;
                    */
                    //---recover IsLimited-flags
                    FinishPreviewMarker2();

                    //---apply latest frame marker to hand menu
                    VRHandMenu.LabelWriteKeyFrame(currentProject.timelineFrameLength);

                    //Debug.Log("An animation finished.");
#if !UNITY_EDITOR && UNITY_WEBGL
                    SendPlayingAnimationInfoOnComplete(seqInIndex);
#endif
                };
                if (currentPlayingOptions.isLoop == 1)
                {
                    currentSeq.OnKill(cb_endcall);
                }
                else
                {
                    currentSeq.OnComplete(cb_endcall);
                }
                
                
                currentSeq.OnUpdate(() =>
                {
                    //Debug.Log("***Parent: " + seqInIndex);
#if !UNITY_EDITOR && UNITY_WEBGL
                    //SendPlayingAnimationInfoOnUpdate(seqInIndex);
#endif


                });

                //---decide start position and end position of the timeline.
                if (currentPlayingOptions.index > 0)
                {
                    currentMarker = currentPlayingOptions.index;
                }
                else
                {
                    currentMarker = 1;
                }
                if (currentPlayingOptions.endIndex <= 0) currentPlayingOptions.endIndex = currentProject.timelineFrameLength;
                seqInIndex = 1;
                foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
                {
                    actor.frameIndexMarker = 0;

                    if (actor.avatar.type == AF_TARGETTYPE.VRM)
                    {
                        ClearGenerateAnimationFrame(actor.avatar);
                    }
                    
                }

                IsRecordingOtherMotion = true;

                //---Build for full animation ----//
                BuildPlayTimelineRoutine2();
                //currentSeq.Pause();

                if (currentPlayingOptions.isRebuildAnimation != 1) currentSeq.Restart();

            }
        }
        public IEnumerator StartTimeline_body()
        {
            IsPause = false;
            IsPlaying = true;
            yield return null;
        }

        public void PauseAllTimeline()
        {
            if (!currentSeq.IsActive())
            { //---first play
                AnimationParsingOptions apo = new AnimationParsingOptions();
                apo.index = 1;
                apo.finalizeIndex = 1;
                apo.isExecuteForDOTween = 1;
                apo.isBuildDoTween = 1;
                apo.isLoop = 0;
                StartAllTimeline(apo);

            }
            else
            { //---second play and more...
                IsPause = !IsPause;
                if (currentSeq.IsComplete())
                {
                    currentSeq.Restart();
                }
                else
                {
                    currentSeq.TogglePause();
                }
            }
            
            
        }
        public void StopAllTimeline()
        {
            IsPlaying = false;
            IsPause = false;
            /*DOVirtual.DelayedCall(0.25f, () =>
            {
                IsBoneLimited = true;
            });*/
            currentMarker = 1;
            if (currentSeq.hasLoops)
            {
                currentSeq.Kill();
                IsBoneLimited = true;

                IsLimitedPelvis = OldIsLimitedPelvis;
                IsLimitedArms = OldIsLimitedArms;
                IsLimitedLegs = OldIsLimitedLegs;
                IsLimitedChest = OldIsLimitedChest;
            }
            else
            {
                if (!currentSeq.IsComplete())
                {
                    currentSeq.Complete();
                    IsBoneLimited = true;

                    IsLimitedPelvis = OldIsLimitedPelvis;
                    IsLimitedArms = OldIsLimitedArms;
                    IsLimitedLegs = OldIsLimitedLegs;
                    IsLimitedChest = OldIsLimitedChest;
                }
            }
            
            foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
            {
                actor.frameIndexMarker = 0;
            }
        }

        /// <summary>
        /// To play routine animation of while version (NOT USE)
        /// </summary>
        /// <returns></returns>
        public void BuildPlayTimelineRoutine()
        { //---loop is frame -> character
            while (currentMarker <= currentPlayingOptions.endIndex)
            {

                Sequence seq = PlayAllTimeline(currentProject.timeline, currentMarker);

                if (seq != null) currentSeq.Append(seq);
                //currentSeq.AppendInterval(currentProject.baseDuration);


                currentMarker++;
            }
        }

        /// <summary>
        /// To play routine animation of NEW VERSION
        /// </summary>
        public void BuildPlayTimelineRoutine2()
        { //---loop is character -> frame   
            foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
            {
                if ((actor.avatar != null) && (actor.avatar.avatar != null))
                {
                    //==================================================
                    //  Tween sequence for 1-role = 1-timeline              
                    Sequence actorSeq = DOTween.Sequence();
                    actorSeq.SetAutoKill(false);
                    actorSeq.SetLink(gameObject);
                    actorSeq.OnRewind(() =>
                    {

                        //---For non-DOTween method and properties
                        ///SpecialUpdateFor_no_DOTween(currentProject.timeline, frameIndex);

                        //seqInIndex++;
                        //Debug.Log("(" + actor.avatar.roleTitle + " begin");
                    });
                    actorSeq.OnStepComplete(() =>
                    {
                        //Debug.Log(")" + actor.avatar.roleTitle + " EOF");
                    });
                    actorSeq.OnUpdate(() =>
                    {

                    });

                    TweenCallback cb_endfunc = () =>
                    {
                        //---other, setting for each avatar type
                        if (actor.avatar.type == AF_TARGETTYPE.VRM)
                        {
                            if (currentPlayingOptions.isCompileAnimation == 1)
                            {
                                OperateLoadedVRM olvrm = actor.avatar.avatar.GetComponent<OperateLoadedVRM>();
                                //---recover a bone rotation to IKPosition and IKRotation 
                                StartCoroutine(olvrm.ApplyBoneTransformToIKTransform(actor.avatar.avatar.GetComponent<Animator>()));
                            }

                        }
                    };
                    if (currentPlayingOptions.isLoop == 1)
                    {
                        actorSeq.OnKill(cb_endfunc);
                    }
                    else
                    {
                        actorSeq.OnComplete(cb_endfunc);
                    }

                    //---for BVH and other motion settings---
                    //   First playing, start record as other motion data.
                    if (IsRecordingOtherMotion)
                    {
                        if (actor.targetType == AF_TARGETTYPE.VRM)
                        {
                            ManageAvatarTransform mat = actor.avatar.avatar.GetComponent<ManageAvatarTransform>();
                            if (mat != null)
                            {
                                //mat.RegenerateBVH(currentProject);
                                //mat.ExportForBVH();
                                //mat.StartRecordBVH();
                            }
                        }
                    }

                    //---build each actor's motion as timeline
                    currentSeq.Join(PlayEachTimeline(actorSeq, actor));
                }
                
            }
        }

        /// <summary>
        /// To play animation of single frame (NOT USE)
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="frameIndex">1-based frame number</param>
        /// <returns></returns>
        public Sequence PlayAllTimeline(NativeAnimationMotionTimeline timeline, int frameIndex)
        {
            //bool ret = false;
            Sequence animateFlow = DOTween.Sequence();
            animateFlow.SetAutoKill(false);
            /*animateFlow.OnStart(() =>
            {
                //Debug.Log(frameIndex + " - Started");

                //---For non-DOTween method and properties
                SpecialUpdateFor_no_DOTween(currentProject.timeline, frameIndex);

                seqInIndex++;
            });*/
            animateFlow.OnComplete(() =>
            {
                //Debug.Log("  & Finished");
#if !UNITY_EDITOR && UNITY_WEBGL
                //SendPlayingAnimationInfoOnUpdate(frameIndex);
#endif

            });
            animateFlow.OnUpdate(() =>
            {

            });

            List<float> durationList = new List<float>();


            foreach (NativeAnimationFrameActor actor in timeline.characters)
            {
                if (actor.targetRole != "")
                {
                    if ((actor.avatar != null) && (actor.avatar.avatar != null) && (LayerMask.LayerToName(actor.avatar.avatar.layer) != "HiddenPlayer"))
                    {
                        //---if compiled animation, disable IK marker.
                        actor.compiled = currentPlayingOptions.isCompileAnimation;
                        if (currentPlayingOptions.isCompileAnimation == 1)
                        {
                            OperateLoadedBase ovrm = actor.avatar.avatar.GetComponent<OperateLoadedBase>();
                            if (ovrm != null)
                            {
                                ovrm.EnableIK(false);
                            }

                        }

                        if (actor.frameIndexMarker < actor.frames.Count)
                        {
                            NativeAnimationFrame avatarFrame = actor.frames[actor.frameIndexMarker];
                            /*
                             * frame:
                             * 1...5
                             * in List:
                             *  0  1
                             * [1][5]
                             *   5's changing start 2. finish 5
                             * effective: This is finalizeIndex
                             * 12..x
                             */
                            if (avatarFrame.index == frameIndex)
                            {
                                durationList.Add(avatarFrame.duration);

                                ProcessBody_forFrame(animateFlow, actor, avatarFrame, currentPlayingOptions);
                                //Debug.Log("index="+ avatarFrame.index + "   finalIndex=" + avatarFrame.finalizeIndex + "     =frameIndexMarker=" + actor.frameIndexMarker);

                                actor.frameIndexMarker++;
                            }

                        }
                    }
                    
                }

            }
            if (durationList.Count > 0)
            {
                durationList.Sort();

                //---set maximum duration as sequence interval
            }
            animateFlow.PlayForward();

            //currentSeq.AppendInterval(currentProject.baseDuration * 1000f);

            //yield return null; // new WaitForSeconds(currentProject.baseDuration);
            return animateFlow;
        }

        /// <summary>
        /// To Build to play animation
        /// </summary>
        /// <param name="actorSeq"></param>
        /// <param name="actor"></param>
        /// <returns>Actor's timeline sequence</returns>
        public Sequence PlayEachTimeline(Sequence actorSeq, NativeAnimationFrameActor actor)
        {
            List<float> durationList = new List<float>();

            TweenCallback cb_start = () =>
            {
                //---For non-DOTween method and properties
                //SpecialUpdate_body(actor, actor.frameIndexMarker);
                //actor.frameIndexMarker++;
                //Debug.Log(actor.avatar.roleTitle + " keyframe " + actor.frameIndexMarker + "start");
            };
            TweenCallback cb_end = () =>
            {
                //Debug.Log(actor.avatar.roleTitle + " keyframe " + actor.frameIndexMarker + "finish");

                //---First playing, start record as other motion data.
                if (IsRecordingOtherMotion)
                {
                    if (actor.targetType == AF_TARGETTYPE.VRM)
                    {
                        //---necessary transform is done.
                        SetGeneralAnimationFrame(actor.avatar, actor.frames[actor.frameIndexMarker].index, actor.frames[actor.frameIndexMarker]);
                        /*
                        ManageAvatarTransform mat = actor.avatar.avatar.GetComponent<ManageAvatarTransform>();
                        if (mat != null)
                        {
                            mat.ExportMotionForBVH();
                            
                        }
                        */
                    }
                }
                
                
            };

            //---this loop is internal key-frame  in the character.
            if (actor.frames.Count > 0)
            {
                //---Sequences for Easing group.
                List<Sequence> seqseq = new List<Sequence>();
                Sequence ownseq = DOTween.Sequence();
                NativeAnimationFrame oldframe = null;
                bool isexec_different = false;

                while (actor.frameIndexMarker < actor.frames.Count)
                {
                    if ((actor.targetRole != "") && (actor.targetId != ""))
                    {
                        if ((actor.avatar != null) && (actor.avatar.avatar != null))
                        {
                            //=======================================
                            //  Tween sequence for Key-frame
                            //---non-DOTween functions of each frame
                            Sequence keyFrameSeq = DOTween.Sequence();
                            keyFrameSeq.SetAutoKill(false);
                            keyFrameSeq.SetLink(gameObject);

                            //seq.OnPlay(cb_start);
                            //seq.OnRewind(cb_start);
                            keyFrameSeq.OnStepComplete(cb_end);
                            keyFrameSeq.AppendCallback(cb_start);

                            //---if compiled animation, disable IK marker.
                            actor.compiled = currentPlayingOptions.isCompileAnimation;
                            if (currentPlayingOptions.isCompileAnimation == 1)
                            {
                                OperateLoadedBase ovrm = actor.avatar.avatar.GetComponent<OperateLoadedBase>();
                                if (ovrm != null)
                                {
                                    ovrm.EnableIK(false);
                                }

                            }

                            NativeAnimationFrame avatarFrame = actor.frames[actor.frameIndexMarker];
                            durationList.Add(avatarFrame.duration);

                            if (oldframe == null)
                            { //---first frame
                                ownseq.Append(ProcessBody_forFrame(keyFrameSeq, actor, avatarFrame, currentPlayingOptions));
                                oldframe = avatarFrame;
                            }
                            else
                            { //---2nd frame after...
                                if (avatarFrame.ease == oldframe.ease)
                                { //---during same easing
                                    isexec_different = false;
                                }
                                else
                                { //---frame different from easing of previous frame
                                    //---save previous easing sequence
                                    ownseq.SetEase(oldframe.ease);
                                    actorSeq.Append(ownseq);
                                    //---begin next easing sequence
                                    ownseq = null;
                                    ownseq = DOTween.Sequence();
                                    isexec_different = true;
                                }
                                // ---after 2nd frame, and during same easing...
                                ownseq.Append(ProcessBody_forFrame(keyFrameSeq, actor, avatarFrame, currentPlayingOptions));
                                oldframe = avatarFrame;
                            }

                            //TODO: effectively...Append or Join ???
                            //actorSeq.Append(ProcessBody_forFrame(keyFrameSeq, actor, avatarFrame, currentPlayingOptions));
                        }


                    }
                    actor.frameIndexMarker++;
                }
                ownseq.SetEase(oldframe.ease);
                actorSeq.Append(ownseq);

                actorSeq.AppendCallback(cb_end);
                if (durationList.Count > 0)
                {
                    durationList.Sort();

                    //---set maximum duration as sequence interval
                }
                //actorSeq.SetEase(Ease.InOutCubic);
            }
            
            return actorSeq;
        }

        /// <summary>
        /// Update for NON-DOTween method and object
        /// </summary>
        /// <param name="timeline"></param>
        /// <param name="frameIndex"></param>
        private void SpecialUpdateFor_no_DOTween(NativeAnimationMotionTimeline timeline, int frameIndex)
        {
            foreach (NativeAnimationFrameActor actor in timeline.characters)
            {
                //SpecialUpdate_body(actor, frameIndex);

            }
        }

//===========================================================================================================================
//  Clipboard functions
//===========================================================================================================================

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">csv string - [0] avatar id, [1] avatar type, [2] frame index, [3] is cut (true = 1)</param>
        public void CopyFrame(string param)
        {
            string[] prm = param.Split(',');
            string role = prm[0];
            int tmpi = int.TryParse(prm[1], out tmpi) ? tmpi : -1;
            if (tmpi == -1)
            {
                return;
            }
            AF_TARGETTYPE type = (AF_TARGETTYPE)tmpi;
            int oldindex = int.TryParse(prm[2], out oldindex) ? oldindex : -1;
            string isCut = prm[3];
            if (oldindex == -1) return;

            clipboard.targetRoleName = role;
            clipboard.targetType = type;
            clipboard.keyFrame = oldindex;
            clipboard.isCut = isCut == "1" ? true : false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">csv string - [0] avatar id, [1] avatar type, [2] destination frame index</param>
        public void PasteFrame(string param)
        {
            string[] retarr = new string[4] {"","","","-1" };
            string ret = "";
            bool issuccess = true;

            if ((clipboard.targetRoleName == "") || (clipboard.keyFrame < 0))
            {
                issuccess = false;
                retarr[0] = "null";
                //ret = "null,,";
            }

            if (issuccess)
            {
                NativeAnimationFrameActor nactor = GetFrameActorFromRole(clipboard.targetRoleName, clipboard.targetType);

                string[] prm = param.Split(',');
                string role = prm[0];

                Debug.Log("***param="+param);
                int tmpi = int.TryParse(prm[1], out tmpi) ? tmpi : -1;
                if (tmpi == -1)
                {
                    issuccess = false;
                    //ret = "null,,";
                    retarr[0] = "null";
                }

                if (issuccess)
                {
                    AF_TARGETTYPE type = (AF_TARGETTYPE)tmpi;
                    int newindex = int.TryParse(prm[2], out newindex) ? newindex : -1;

                    if (role != nactor.targetRole)
                    {
                        issuccess = false;
                        //ret = "null,,";
                        retarr[0] = "null";
                    }

                    if (issuccess)
                    {
                        //---remove current paste destination
                        int overwriteIndex = GetFrameIndex(nactor, newindex);
                        if (overwriteIndex > -1) nactor.frames.RemoveAt(overwriteIndex);

                        //---newly add clipboard key-index data
                        int findex = GetFrameIndex(nactor, clipboard.keyFrame);
                        if (clipboard.isCut)
                        {
                            nactor.frames[findex].index = newindex;

                            SortActorFrames(nactor);
                            adjustNearMaxFrameIndex(nactor, nactor.frames[findex]);

                            if (nactor.frames[findex].translateMovingData.Count > 0)
                            {
                                retarr[3] = nactor.frames[findex].translateMovingData[0].values.Count.ToString();
                            }
                            else
                            {
                                retarr[3] = "";
                            }
                            
                        }
                        else
                        {
                            NativeAnimationFrame nframe = nactor.frames[findex].SCopy();
                            nframe.index = newindex;
                            if ((overwriteIndex == -1) || (nactor.frames.Count <= overwriteIndex))
                            {
                                nactor.frames.Add(nframe);
                                if (nactor.frames[nactor.frames.Count - 1].translateMovingData.Count > 0)
                                {
                                    retarr[3] = nactor.frames[nactor.frames.Count - 1].translateMovingData[0].values.Count.ToString();
                                }
                                else
                                {
                                    retarr[3] = "";
                                }
                                
                            }
                            else
                            {
                                nactor.frames.Insert(overwriteIndex, nframe);
                                if (nactor.frames[overwriteIndex].translateMovingData.Count > 0)
                                {
                                    retarr[3] = nactor.frames[overwriteIndex].translateMovingData[0].values.Count.ToString();
                                }
                                else
                                {
                                    retarr[3] = "";
                                }
                                
                            }


                            SortActorFrames(nactor);
                            adjustNearMaxFrameIndex(nactor, nframe);
                        }

                        //---To adjust all frame duration and finalIndex of the actor
                        AdjustAllFrame(nactor, currentProject.baseDuration, false, true);


                        //ret = param;
                        retarr[0] = prm[0];
                        retarr[1] = prm[1];
                        retarr[2] = prm[2];
                    }
                    
                }
                
            }

            ret = string.Join(",", retarr);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif

        }

        public void EditProjectMeta(string param)
        {
            currentProject.meta = JsonUtility.FromJson<AnimationProjectMetaInformation>(param);
        }
        //===========================================================================================================================
        //  Material management functions
        //===========================================================================================================================

        /// <summary>
        /// Add and load texture file
        /// </summary>
        /// <param name="param">[0] - name, [1] - group, [2] - url, [3] - load flag(1 - load, 0 - no load), [4] - save destination a(app) or p(project)</param>
        public void LoadMaterialTexture(string param)
        {
            string[] prm = param.Split("\t");

            string name = prm[0];
            string group = prm[1];
            string url = prm[2];
            bool isEffectiveLoad = false;
            if (prm.Length >= 4) isEffectiveLoad = (prm[3] == "1" ? true : false);
            OneMaterialFrom fromtype = OneMaterialFrom.app;
            if (prm[4] == "a") fromtype = OneMaterialFrom.app;
            else if (prm[4] == "p") fromtype = OneMaterialFrom.project;

            NativeAP_OneMaterial nap = new NativeAP_OneMaterial(name, url, OneMaterialType.Texture);
            nap.group = group;

            string ret = "";

            //===1st: material in project=======
            if (fromtype == OneMaterialFrom.project)
            {
                NativeAP_OneMaterial ishit = materialManager.FindTexture(name, group);

                if (ishit == null)
                {
                    ishit = currentProject.materialManager.FindTexture(name, group);

                    if (ishit == null)
                    {
                        if (isEffectiveLoad)
                        {
                            StartCoroutine(nap.Open(url));
                        }


                        currentProject.materialManager.materials.Add(nap);
                        ret = nap.name;
                    }
                }
                
            }            
            else if (fromtype == OneMaterialFrom.app) 
            {
                NativeAP_OneMaterial ishit = currentProject.materialManager.FindTexture(name, group);
                if (ishit == null)
                {
                    //===2nd: material in app============
                    ishit = materialManager.FindTexture(name, group);
                    if (ishit == null)
                    {
                        if (isEffectiveLoad)
                        {
                            StartCoroutine(nap.Open(url));
                        }


                        materialManager.materials.Add(nap);
                        ret = nap.name;
                    }
                }
                
            }

            
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif

        }

        /// <summary>
        /// Only to load existed texture file 
        /// </summary>
        /// <param name="param">[0] - name, [1] - group, [2] - url, [3] - save destination a(app) or p(project)</param>
        public void LoadOneMaterialTexture(string param)
        {
            string[] prm = param.Split("\t");
            string name = prm[0];
            string group = prm[1];
            string url = prm[2];

            OneMaterialFrom fromtype = OneMaterialFrom.app;
            if (prm[3] == "a") fromtype = OneMaterialFrom.app;
            if (prm[3] == "p") fromtype = OneMaterialFrom.project;

            string ret = "";
            //===1st: material in project=======
            if (fromtype == OneMaterialFrom.project)
            {
                NativeAP_OneMaterial ishit = currentProject.materialManager.FindTexture(name, group);
                if (ishit != null)
                {
                    StartCoroutine(ishit.Open(url));
                    ret = ishit.name;
                }
            }
            else if (fromtype == OneMaterialFrom.app)
            {
                //===2nd: material in app============
                NativeAP_OneMaterial ishit = materialManager.FindTexture(name, group);
                if (ishit != null)
                {
                    StartCoroutine(ishit.Open(url));
                    ret = ishit.name;
                }
            }
            
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        /*
        public void LoadMaterialBundle(string param)
        {
            string[] prm = param.Split(",");
            materialManager.OpenBundle(prm[0], prm[1]);
        }
        */

        /// <summary>
        /// Find material from materialManager (project, app)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public NativeAP_OneMaterial FindTexture(string name, string group = "")
        {
            //===1st: material in project
            NativeAP_OneMaterial ishit = currentProject.materialManager.FindTexture(name, group);
            //Debug.Log("1st search==>"+name);
            //Debug.Log(ishit);

            if (ishit == null)
            {
                //===2nd: material in app============
                ishit = materialManager.FindTexture(name, group);
                //Debug.Log("2nd search==>" + name);
                //Debug.Log(ishit);
            }
            return ishit;
        }
        /// <summary>
        /// Rename material
        /// </summary>
        /// <param name="param">[0] - oldname, [1] - newname</param>
        public void RenameMaterialName(string param)
        {
            string[] prm = param.Split("\t");
            //===1st: material in project=======
            int ishit = currentProject.materialManager.materials.FindIndex(item =>
            {
                if (item.name == prm[0]) return true;
                return false;
            });
            string ret = "";
            if (ishit > -1)
            {
                int isnewhit = currentProject.materialManager.materials.FindIndex(item =>
                {
                    if (item.name == prm[1]) return true;
                    return false;
                });
                if (isnewhit < 0)
                {
                    currentProject.materialManager.materials[ishit].name = prm[1];
                    ret = prm[1];
                }
            }
            else
            {
                //===2nd: material in app============
                ishit = materialManager.materials.FindIndex(item =>
                {
                    if (item.name == prm[0]) return true;
                    return false;
                });
                if (ishit > -1)
                {
                    int isnewhit = materialManager.materials.FindIndex(item =>
                    {
                        if (item.name == prm[1]) return true;
                        return false;
                    });
                    if (isnewhit < 0)
                    {
                        materialManager.materials[ishit].name = prm[1];
                        ret = prm[1];
                    }
                }
            }

            
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif

        }

        /// <summary>
        /// Remove material from materialManager(project, app)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int RemoveMaterial(string name)
        {
            //Debug.Log("project search==>");
            int ret = currentProject.materialManager.RemoveTexture(name);
            if (ret == -1)
            {
                //Debug.Log("app search==>");
                ret = materialManager.RemoveTexture(name);
            }
            return ret;
        }
        public void RemoveMaterialFromOuter(string name)
        {
            int ret = RemoveMaterial(name);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret);
#endif
        }
        /*
        public void RemoveMaterialBundle(string name)
        {
            materialManager.CloseBundle(name);
        }
        */

        /// <summary>
        /// Unreference material from target object(project, app)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public int UnReferMaterial(OneMaterialType type, string name, string group = "")
        {
            int ret = currentProject.materialManager.UnRefer(type, name, group);
            //Debug.Log("unref project[" + name + "]=" + ret.ToString());
            if (ret == -1)
            {
                ret = materialManager.UnRefer(type, name, group);
                //Debug.Log("unref project[" + name + "]=" + ret.ToString());
            }
            return ret;
        }
        public void EnumMaterialTexture(int param)
        {
            OneMaterialFrom fromtype = (OneMaterialFrom)param;

            List<string> arr = new List<string>();
            if (fromtype == OneMaterialFrom.app)
            {
                materialManager.materials.ForEach(item =>
                {
                    arr.Add(item.name + "," + ((int)item.materialType).ToString() + "," + item.size.x.ToString() + "," + item.size.y.ToString());
                });
            }
            else if (fromtype == OneMaterialFrom.project)
            {
                currentProject.materialManager.materials.ForEach(item =>
                {
                    arr.Add(item.name + "," + ((int)item.materialType).ToString() + "," + item.size.x.ToString() + "," + item.size.y.ToString());
                });
            }
            string ret = string.Join('\t', arr);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        //===========================================================================================================================
        //  VR/AR mode functions
        //===========================================================================================================================
        public float GetMoveRate()
        {
            return camxr.MoveRate;
        }
        public void GetMoveRateFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(camxr.MoveRate);
#endif
        }
        public float GetRotateRate()
        {
            return camxr.RotateRate;
        }
        public void GetRotateRateFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(camxr.RotateRate);
#endif
        }
        public void SetMoveRate(float val)
        {
            camxr.MoveRate = val;
        }
        public void SetRotateRate(float val)
        {
            camxr.RotateRate = val;
        }
        /// <summary>
        /// Change states avatar by 3D-type
        /// </summary>
        /// <param name="isNormal"></param>
        private void ChangeColliderState_OtherObjects(bool isNormal = false)
        {
            OperateActiveVRM oavrm = ikArea.GetComponent<OperateActiveVRM>();

            for (int i = 0; i < currentProject.casts.Count; i++)
            {
                NativeAnimationAvatar nav = currentProject.casts[i];
                if (nav != null)
                {
                    if (nav.type == AF_TARGETTYPE.OtherObject)
                    {
                        Rigidbody rid = nav.avatar.GetComponent<Rigidbody>();
                        //rid.isKinematic = isNormal;
                        if (isNormal)
                        {

                        }
                        else
                        {

                        }
                    }
                    else if (nav.type == AF_TARGETTYPE.VRM)
                    {
                        nav.avatar.GetComponent<OperateLoadedVRM>().ChangeNormalVRAR_IKTarget();
                    }
                    if (isNormal)
                    {
                        if (oavrm.ActiveAvatar != nav.avatar)
                        { //---if no select objects, disable IK marker.(normally specification)
                            if (
                                (nav.type != AF_TARGETTYPE.SystemEffect) &&
                                (nav.type != AF_TARGETTYPE.Audio) &&
                                (nav.type != AF_TARGETTYPE.Stage) &&
                                (nav.type != AF_TARGETTYPE.Text) &&
                                (nav.type != AF_TARGETTYPE.UImage)

                            )
                            {

                                oavrm.DisableHandle_Avatar(nav.ikparent, nav.type);

                            }

                        }
                    }
                    else
                    { //---all objects enable IK marker when VR/AR mode.
                        if (
                                (nav.type != AF_TARGETTYPE.SystemEffect) &&
                                (nav.type != AF_TARGETTYPE.Audio) &&
                                (nav.type != AF_TARGETTYPE.Stage) &&
                                (nav.type != AF_TARGETTYPE.Text) &&
                                (nav.type != AF_TARGETTYPE.UImage)
                            )
                        {

                            oavrm.EnableHandle_Avatar(nav.ikparent, nav.type);

                        }
                    }
                }
                
            }
        }

        /// <summary>
        /// Change states on enter VR/AR
        /// </summary>
        public void ChangeStateEnteringVRAR()
        {
            OperateActiveVRM oavrm = ikArea.GetComponent<OperateActiveVRM>();
            NativeAnimationAvatar nav = GetCastByAvatar(oavrm.ActiveAvatar.name);
            if (nav != null)
            {
                if ((nav.type == AF_TARGETTYPE.SystemEffect) || (nav.type == AF_TARGETTYPE.Audio) || (nav.type == AF_TARGETTYPE.Stage) ||
                (nav.type == AF_TARGETTYPE.Text) || (nav.type == AF_TARGETTYPE.UImage)
                )
                { //---if selected avatar is non target. choose firstly hitted avatar.
                    foreach (var cast in currentProject.casts)
                    {
                        if ((nav.type != AF_TARGETTYPE.SystemEffect) && (nav.type != AF_TARGETTYPE.Audio) && (nav.type != AF_TARGETTYPE.Stage) &&
                            (nav.type != AF_TARGETTYPE.Text) && (nav.type != AF_TARGETTYPE.UImage)
                        )
                        {
                            nav = cast;
                            break;
                        }
                    }
                }
                
            }
            
            
            //---input key number label, select objeect title
            var HandMenus = FindObjectsByType<ownscr_HandMenu>(FindObjectsSortMode.InstanceID);
            if (HandMenus != null)
            {
                foreach (var hm in HandMenus)
                {
                    /*if (hm.HandType == ownscr_HandMenu.HandMenuDirection.Left)
                    {
                        hm.ChangePanel(cfg_vrarctrl_panel_left);
                    }
                    else if (hm.HandType == ownscr_HandMenu.HandMenuDirection.Right)
                    {
                        hm.ChangePanel(cfg_vrarctrl_panel_right);
                    }*/
                    if (hm.HandType == ownscr_HandMenu.HandMenuDirection.Left)
                    {
                        hm.LabelWriteKeyFrame(oldPreviewMarker);
                        hm.SetObjectTitle(nav.roleTitle);
                    }
                }
            }
            
            VRARSelectedAvatarName = nav.roleName;

            camxr.GetComponent<CameraOperation1>().targetObject.SetActive(false);
            GizmoRenderer.SetActive(false);
            ObjectInfoView.SetActive(false);
        }

        /// <summary>
        /// Change states on Ending VR/AR
        /// </summary>
        public void ChangeStateEndingVRAR()
        {
            OperateActiveVRM oavrm = ikArea.GetComponent<OperateActiveVRM>();
            //---recover from selected avatar on VR/AR to normal selected avatar.
            NativeAnimationAvatar nav = GetCastInProject(VRARSelectedAvatarName);
            if (nav != null)
            {
                //---normally method use.
                oavrm.ChangeEnableAvatarFromOuter(nav.avatar.name);
            }
            GizmoRenderer.SetActive(true);
            ObjectInfoView.SetActive(true);
            camxr.GetComponent<CameraOperation1>().targetObject.SetActive(true);

        }
        public void EnterVR(string param)
        {
            string[] prm = param.Split(",");
            cfg_vrar_save_camerapos = prm[0] == "1" ? true : false;
            string[] parr = prm[1].Split(":");
            float ipx = float.TryParse(parr[0], out ipx) ? ipx : 0f;
            float ipy = float.TryParse(parr[1], out ipy) ? ipy : 0f;
            float ipz = float.TryParse(parr[2], out ipz) ? ipz : 0f;
            cfg_vrar_camera_initpos = new Vector3(ipx, ipy, ipz);

            bkup_camerapos = new Vector3(camxr.transform.position.x, camxr.transform.position.y, camxr.transform.position.z);
            bkup_camerarot = new Quaternion(camxr.transform.rotation.x, camxr.transform.rotation.y, camxr.transform.rotation.z, camxr.transform.rotation.w);
            

            IsEndVRAR = false;
            ChangeColliderState_OtherObjects(false);

            ChangeStateEnteringVRAR();
            camxr.ToggleVR();
            if (cfg_vrar_save_camerapos)
            {
                camxr.transform.position = new Vector3(bkup_lastvrar_pos.x, bkup_lastvrar_pos.y, bkup_lastvrar_pos.z);
            }
            else
            {
                camxr.transform.position = new Vector3(cfg_vrar_camera_initpos.x, cfg_vrar_camera_initpos.y, cfg_vrar_camera_initpos.z);
            }
        }
        public void EnterAR(string param)
        {
            string[] prm = param.Split(",");
            cfg_vrar_save_camerapos = prm[0] == "1" ? true : false;
            string[] parr = prm[1].Split(":");
            float ipx = float.TryParse(parr[0], out ipx) ? ipx : 0f;
            float ipy = float.TryParse(parr[1], out ipy) ? ipy : 0f;
            float ipz = float.TryParse(parr[2], out ipz) ? ipz : 0f;
            cfg_vrar_camera_initpos = new Vector3(ipx, ipy, ipz);

            bkup_camerapos = new Vector3(camxr.transform.position.x, camxr.transform.position.y, camxr.transform.position.z);
            bkup_camerarot = new Quaternion(camxr.transform.rotation.x, camxr.transform.rotation.y, camxr.transform.rotation.z, camxr.transform.rotation.w);

            IsEndVRAR = false;
            ChangeColliderState_OtherObjects(false);
            ChangeStateEnteringVRAR();
            camxr.ToggleAR();
            if (cfg_vrar_save_camerapos)
            {
                camxr.transform.position = new Vector3(bkup_lastvrar_pos.x, bkup_lastvrar_pos.y, bkup_lastvrar_pos.z);
            }
            else
            {
                camxr.transform.position = new Vector3(cfg_vrar_camera_initpos.x, cfg_vrar_camera_initpos.y, cfg_vrar_camera_initpos.z);
            }
        }
        public bool IsVRAR()
        {
            return !camxr.isActiveNormal();
        }

        /// <summary>
        /// Return edit data on VR/AR to HTML-UI
        /// </summary>
        public void FinishVRWithEditData()
        {
            AnimationProject aniproj = Body_SaveProject(currentProject);

            string ret = JsonUtility.ToJson(aniproj);
#if !UNITY_EDITOR && UNITY_WEBGL
        
            EndingVRAR(ret);
        
#endif
            bkup_lastvrar_pos.x = camxr.transform.position.x;
            bkup_lastvrar_pos.y = camxr.transform.position.y;
            bkup_lastvrar_pos.z = camxr.transform.position.z;

            camxr.transform.position = new Vector3(bkup_camerapos.x, bkup_camerapos.y, bkup_camerapos.z);
            camxr.transform.rotation = new Quaternion(bkup_camerarot.x, bkup_camerarot.y, bkup_camerarot.z, bkup_camerarot.w);
        }
    }


}