using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using VRM;
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


        static string[] IKBoneNames = {  "IKParent", "EyeViewHandle", "Head", "LookAt", "Aim", "Chest", "Pelvis", "LeftLowerArm", "LeftHand",
            "RightLowerArm","RightHand","LeftLowerLeg","LeftLeg","RightLowerLeg","RightLeg"
        };
        const int IKbonesCount = 15;

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

        public NativeAnimationProject currentProject;
        public bool IsExternalProject;
        public bool IsPlaying;
        public bool IsPause;
        public bool IsPreview;
        public bool IsBoneLimited;
        public bool IsLimitedPelvis;
        public bool IsLimitedArms;
        public bool IsLimitedLegs;
        private bool OldIsLimitedPelvis;
        private bool OldIsLimitedArms;
        private bool OldIsLimitedLegs;
        private int isOldLoop = 0;
        private int oldPreviewMarker;
        private int currentMarker;
        private AnimationParsingOptions currentPlayingOptions;
        private Sequence currentSeq;
        private int seqInIndex;
        private FrameClipboard clipboard;

        public NativeAnimationProjectMaterialPackage materialManager;


        protected NativeAnimationAvatar SingleMotionTargetRole = null;


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
        private ConfigSettingLabs configLab;


        private void Awake()
        {
            IsPause = false;
            IsPlaying = false;
            IsPreview = false;
            IsBoneLimited = true;
            IsLimitedPelvis = true;
            IsLimitedArms = true;
            IsLimitedLegs = true;
            OldIsLimitedPelvis = true;
            OldIsLimitedArms = true;
            OldIsLimitedLegs = true;
            currentMarker = 1;
            seqInIndex = 1;
            currentSeq = null;
            oldPreviewMarker = 1;

            ChangeFullIKType(false);
        }
        // Start is called before the first frame update
        void Start()
        {
            configLab = GameObject.Find("Canvas").GetComponent<ConfigSettingLabs>();

            currentProject = new NativeAnimationProject();
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
                    "IKParent", "EyeViewHandle", "Head", "LookAt", "Aim", "Chest", "Pelvis", "LeftLowerArm", "LeftHand",
                    "RightLowerArm","RightHand","LeftLowerLeg","LeftLeg","RightLowerLeg","RightLeg"
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
        //===========================================================================================================================
        //  Utility functions
        //===========================================================================================================================
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
        /// <param name="param">[0] - p = pelvis, a = arms, l = legs, [1] - 1 = true, 0 = false</param>
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
                        cast.avatar.GetComponent<OperateLoadedVRM>().EnableRotationLimit(flag);
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
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(flag);
#endif
        }
        private string TrimBlendShapeName(string bsname)
        {
            string[] spr = bsname.Split('.');
            //is ***.***_**_**_... => [***] [***_**_**_]
            //is NOT ***_**_**_=> [***_**_**_]
            string bs = spr[spr.Length - 1].Replace("M_F00_000_00_", "");
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
                if ((av.avatar.avatarId == id) && (av.avatar.type == type))
                {
                    return true;
                }
                return false;
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

            ret = currentProject.timeline.characters.Find(av =>
            {
                if ((av.avatar.roleName == role) && (av.avatar.type == type))
                {
                    return true;
                }
                return false;
            });
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
            ret = actor.frames.Find(item =>
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
            foreach (NativeAnimationAvatar avatar in currentProject.casts)
            {
                AnimationAvatar aa = new AnimationAvatar();
                aa.avatarId = avatar.avatarId;
                Array.Copy(avatar.bodyHeight, aa.bodyHeight, avatar.bodyHeight.Length);
                aa.roleName = avatar.roleName;
                aa.roleTitle = avatar.roleTitle;
                aa.type = avatar.type;
                ret.Add(JsonUtility.ToJson(aa));
            }

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(string.Join("%", ret));
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
                VRMMeta vmeta = null;
                if (child.TryGetComponent(out vmeta))
                {
                    if (vmeta.Meta.Title == roleTitle)
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
            else if (type == AF_TARGETTYPE.Text)
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
            Debug.Log(apo.targetRole);
            Debug.Log(apo.targetType);
            int ret = -1;
            if (naf != null)
            {
                Debug.Log("naf.targetId="+naf.targetId);
                int index = GetNearMaxFrameIndex(naf, apo.index);
                Debug.Log(index);
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
        /// Create An empty cast and timeline
        /// </summary>
        /// <param name="param">CSV-String: [0] - cast type(AF_TARGETTYPE), [1] - cast title</param>
        /// <returns>CSV-string: [0] role name(id), [1] - role title</returns>
        public NativeAnimationAvatar CreateEmptyCast(string param)
        {
            string[] prm = param.Split(',');
            int t = int.TryParse(prm[0], out t) ? t : 99;
            AF_TARGETTYPE castType = (AF_TARGETTYPE)t;
            string[] cns_roleNames = { "VRM", "OtherObject", "Light", "Camera", "Text", "Image", "UImage", "Audio", "Effect", "SystemEffect", "Stage" };

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
            nav.roleTitle = nav.roleName;

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
        public NativeAnimationAvatar FirstAddAvatar2(out bool isOverWrite, string id, GameObject avatar, GameObject ikparent, string RoleName, AF_TARGETTYPE type, float[] bodyheight = null,  List < Vector3> bodyinfo = null)
        {
            isOverWrite = false;
            if ((currentProject != null) && (currentProject.isSharing || currentProject.isReadOnly)) return (NativeAnimationAvatar)null;

            VRMMeta vmeta = avatar.GetComponent<VRMMeta>();
            string rtitle = "";
            if (vmeta != null)
            {
                rtitle = vmeta.Meta.Title;
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
                if (nav != null)
                { //---role is exist, and not allocate cast
                    string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    char[] Charsarr = new char[3];
                    System.Random random = new System.Random();

                    for (int i = 0; i < Charsarr.Length; i++)
                    {
                        Charsarr[i] = characters[random.Next(characters.Length)];
                    }

                    convertTitle += "_" + new String(Charsarr);
                }
                nav = new NativeAnimationAvatar();
                nav.avatarId = id;
                nav.avatar = avatar;
                nav.ikparent = ikparent;
                nav.type = type;
                nav.roleName = RoleName + "_" + DateTime.Now.ToFileTime().ToString();  //"(" + GetCountTypeOf(type) + ")";
                nav.roleTitle = convertTitle; // nav.roleName;
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
            foreach (NativeAnimationAvatar avatar in currentProject.casts)
            {
                if (avatar.avatarId == prm[0])
                {
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
            foreach (NativeAnimationAvatar avatar in currentProject.casts)
            {
                if (avatar.roleName == prm[0])
                {
                    avatar.avatarId = prm[1];
                    NativeAnimationAvatar tmpav = GetEffectiveAvatarObjects(prm[1], avatar.type);

                    //---change reference to avatar object.
                    avatar.avatar = tmpav.avatar;
                    avatar.ikparent = tmpav.ikparent;

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
            foreach (NativeAnimationAvatar avatar in currentProject.casts)
            {
                if (prm[1] == "role")
                {
                    if (avatar.roleName == prm[0])
                    {
                        avatar.avatarId = "";
                        avatar.avatar = null;
                        avatar.ikparent = null;
                    }
                }
                else if(prm[1] == "avatar")
                {
                    if (avatar.avatarId == prm[0])
                    {
                        avatar.avatarId = "";
                        avatar.avatar = null;
                        avatar.ikparent = null;
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
                 (type == AF_TARGETTYPE.Light) || (type == AF_TARGETTYPE.Camera) || (type == AF_TARGETTYPE.Effect)
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
        public Vector3 CalculateReposition(GameObject avatar, GameObject ikparent, AF_TARGETTYPE type, float[] bounds, Vector3 lst, ParseIKBoneType vrmBone, Vector3 pelvisCondition)
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
                loadTargetPelvis.y = bounds[PELVIS_Y] - pelvisCondition.y; //lst.y;
                loadTargetExtents.x = bounds[HEIGHT_X];
                loadTargetExtents.y = bounds[HEIGHT_Y];
                loadTargetExtents.z = bounds[HEIGHT_Z];

                currentTargetExtents.x = bnd.extents.x * 2f;
                currentTargetExtents.y = bnd.extents.y * 2f;
                currentTargetExtents.z = bnd.extents.z * 2f;

            }

            Transform boneTran = ikparent.transform.Find(IKBoneNames[(int)vrmBone]);

            Vector3 fnl;


            //---Absorb the difference in height.
            fnl.x = currentTargetExtents.x * (lst.x / loadTargetExtents.x);
            if (vrmBone == ParseIKBoneType.Pelvis) 
            {
                //---Pelvis only: add sample result of "rest pelvis - pose pelvis"
                fnl.y = currentTargetExtents.y * (lst.y / loadTargetExtents.y) + loadTargetPelvis.y;
            }
            else
            {
                fnl.y = currentTargetExtents.y * (lst.y / loadTargetExtents.y);
            }

            fnl.z = currentTargetExtents.z * (lst.z / loadTargetExtents.z);

            return new Vector3(fnl.x, fnl.y, lst.z);
            
        }

        /// <summary>
        /// To calculate difference of height of VRM.
        /// </summary>
        /// <param name="currentActor">T-pose data of current target avatar</param>
        /// <param name="roller">T-pose data of the timeline actor</param>
        /// <param name="lst">target pose data</param>
        /// <param name="vrmBone">target body part</param>
        /// <returns></returns>
        public Vector3 CalculateDifferenceInHeight(Vector3 currentActor, Vector3 roller, Vector3 lst, ParseIKBoneType vrmBone)
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

            ret = lst + qv;
            /*
            ret.x = lst.x * qv.x;
            ret.y = lst.y * qv.y;
            ret.z = lst.z * qv.z;
            */

            return ret;
        }
        


//===========================================================================================================================
//  Play functions
//===========================================================================================================================
        
        /// <summary>
        /// Parse body of Animation Process. (apply ease at end)
        /// </summary>
        /// <param name="animateFlow"></param>
        /// <param name="targetObject"></param>
        /// <param name="frame"></param>
        /// <param name="aro"></param>
        /// <returns></returns>
        private Sequence ProcessBody_forFrame(Sequence animateFlow, NativeAnimationFrameActor targetObject, NativeAnimationFrame frame, AnimationParsingOptions aro)
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
                animateFlow.SetEase(frame.ease);

                //---moving data loop for 1 avatar
                for (int i = frame.movingData.Count - 1; i >= 0; i--)
                {
                    AnimationTargetParts movedata = frame.movingData[i];

                    if (targetObject.targetType == AF_TARGETTYPE.SystemEffect)
                    {
                        animateFlow = ParseForSystemEffect(animateFlow, frame, movedata, targetObject, aro);
                    }
                    else
                    {

                        animateFlow = ParseForCommon(animateFlow, frame, movedata, targetObject, null, aro);


                        if (targetObject.targetType == AF_TARGETTYPE.VRM)
                        {
                            animateFlow = ParseForVRM(animateFlow, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.OtherObject)
                        {
                            animateFlow = ParseForOtherObject(animateFlow, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.Light)
                        {
                            animateFlow = ParseForLight(animateFlow, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.Camera)
                        {
                            animateFlow = ParseForCamera(animateFlow, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.Text)
                        {
                            animateFlow = ParseForText(animateFlow, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.Image)
                        {
                            animateFlow = ParseForImage(animateFlow, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.UImage)
                        {
                            animateFlow = ParseForUImage(animateFlow, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.Audio)
                        {
                            animateFlow = ParseForAudio(animateFlow, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.Effect)
                        {
                            animateFlow = ParseForEffect(animateFlow, frame, movedata, targetObject, aro);
                        }
                        else if (targetObject.targetType == AF_TARGETTYPE.Stage)
                        {
                            animateFlow = ParseForStage(animateFlow, frame, movedata, targetObject, aro);
                        }
                    }

                }

            }
            return animateFlow;
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
                    aro.index = targetFrameIndex;
                    Sequence nseq = DOTween.Sequence();
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
                    IsBoneLimited = false;
                    OldIsLimitedPelvis = IsLimitedPelvis;
                    OldIsLimitedArms = IsLimitedArms;
                    OldIsLimitedLegs = IsLimitedLegs;
                    IsLimitedPelvis = false;
                    IsLimitedArms = false;
                    IsLimitedLegs = false;
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
                IsBoneLimited = true;
                
                IsLimitedPelvis = OldIsLimitedPelvis;
                IsLimitedArms = OldIsLimitedArms;
                IsLimitedLegs = OldIsLimitedLegs;

#if !UNITY_EDITOR && UNITY_WEBGL
            SendPlayingPreviewAnimationInfoOnComplete(currentPlayingOptions.finalizeIndex < 0 ? currentPlayingOptions.index : currentPlayingOptions.finalizeIndex);
#endif
            });
        }
        public void BackupPreviewMarker()
        {
            oldPreviewMarker = currentPlayingOptions.index;
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
                IsBoneLimited = false;

                OldIsLimitedPelvis = IsLimitedPelvis;
                OldIsLimitedArms = IsLimitedArms;
                OldIsLimitedLegs = IsLimitedLegs;
                IsLimitedPelvis = false;
                IsLimitedArms = false;
                IsLimitedLegs = false;
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
                IsBoneLimited = true;
                IsLimitedPelvis = OldIsLimitedPelvis;
                IsLimitedArms = OldIsLimitedArms;
                IsLimitedLegs = OldIsLimitedLegs;
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    
                });
#if !UNITY_EDITOR && UNITY_WEBGL
                SendPlayingPreviewAnimationInfoOnComplete(currentPlayingOptions.index);
#endif
            });

            PreviewProcessBody(animateFlow, actor, aro, false);
        }
        public void GetEaseFromOuter(string param)
        {
            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);
            NativeAnimationFrameActor actor = GetFrameActorFromRole(aro.targetRole, aro.targetType);
            NativeAnimationFrame curframe = GetFrame(actor, aro.index);

            int js = (int)curframe.ease;
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
            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);

            NativeAnimationFrameActor actor = GetFrameActorFromRole(aro.targetRole, aro.targetType);

            int nearmin = GetNearMinFrameIndex(actor, aro.index);
            NativeAnimationFrame minframe = actor.frames[nearmin]; // GetFrame(actor, nearmin);
            NativeAnimationFrame curframe = GetFrame(actor, aro.index);
            //---update "ease" only
            curframe.ease = aro.ease;

            /* NOT USE
            //---start Preview
            AnimationParsingOptions apo = new AnimationParsingOptions();
            apo.index = nearmin;
            apo.finalizeIndex = nearmin;
            apo.targetId = aro.targetId;
            apo.targetType = aro.targetType;
            string[] roles = GetRoleSpecifiedAvatar(aro.targetId);
            apo.targetRole = roles[0];


            PreviewSingleFrame(apo);
            apo.index = aro.index;
            apo.finalizeIndex = aro.index;
            PreviewSingleFrame(apo);
            */
            string js = "{ " +
                "\"roleName\": \"" + actor.targetRole + "\"," +
                "\"avatarId\" : \"" + actor.targetId + "\"," + 
                "\"type\": " + (int)aro.targetType + "," +
                "\"nearMinIndex\": " + minframe.index + "," + 
                "\"index\":" + aro.index + 
            "}";
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }
        public void GetDurationFromOuter(string param)
        {
            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);

            NativeAnimationFrameActor actor = GetFrameActorFromRole(aro.targetRole, aro.targetType);
            
            NativeAnimationFrame curframe = GetFrame(actor, aro.index);
            float js = curframe.duration;
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
            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);

            NativeAnimationFrameActor actor = GetFrameActorFromRole(aro.targetRole, aro.targetType);
            NativeAnimationFrame curframe = GetFrame(actor, aro.index);
            curframe.duration = aro.duration;
        }
        /// <summary>
        /// Reset frame duration.(all frame)
        /// </summary>
        /// <param name="param">JSON-string for AnimationRegisterOptions</param>
        public void ResetAutoDuration(string param)
        {
            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);

            NativeAnimationFrameActor actor = GetFrameActorFromRole(aro.targetRole, aro.targetType);
            AdjustAllFrameDuration(actor, currentProject.baseDuration);
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
                IsBoneLimited = false;
                OldIsLimitedPelvis = IsLimitedPelvis;
                OldIsLimitedArms = IsLimitedArms;
                OldIsLimitedLegs = IsLimitedLegs;
                IsLimitedPelvis = false;
                IsLimitedArms = false;
                IsLimitedLegs = false;
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
                    IsBoneLimited = false;
                    OldIsLimitedPelvis = IsLimitedPelvis;
                    OldIsLimitedArms = IsLimitedArms;
                    OldIsLimitedLegs = IsLimitedLegs;
                    IsLimitedPelvis = false;
                    IsLimitedArms = false;
                    IsLimitedLegs = false;

                    foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
                    {
                        actor.frameIndexMarker = 0;
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
                            OperateLoadedVRM olvrm = actor.avatar.GetComponent<OperateLoadedVRM>();
                            olvrm.ListGravityInfo();
                        }
                    }
                    foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
                    {
                        actor.frameIndexMarker = 0;
                    }
                    //StopAllTimeline();
                    IsPause = false;
                    IsPlaying = false;
                    IsBoneLimited = true;
                    
                    IsLimitedPelvis = OldIsLimitedPelvis;
                    IsLimitedArms = OldIsLimitedArms;
                    IsLimitedLegs = OldIsLimitedLegs;

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
                }


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
                }
            }
            
            foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
            {
                actor.frameIndexMarker = 0;
            }
        }

        /// <summary>
        /// To play routine animation of while version
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
        public void BuildPlayTimelineRoutine2()
        { //---loop is character -> frame   TEST!!
            foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
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
                currentSeq.Join(PlayEachTimeline(actorSeq, actor));
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
            animateFlow.OnStart(() =>
            {
                //Debug.Log(frameIndex + " - Started");

                //---For non-DOTween method and properties
                SpecialUpdateFor_no_DOTween(currentProject.timeline, frameIndex);

                seqInIndex++;
            });
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
        /// <returns></returns>
        public Sequence PlayEachTimeline(Sequence actorSeq, NativeAnimationFrameActor actor)
        {
            List<float> durationList = new List<float>();

            TweenCallback cb_start = () =>
            {
                //---For non-DOTween method and properties
                SpecialUpdate_body(actor, actor.frameIndexMarker);
                //actor.frameIndexMarker++;
                //Debug.Log(actor.avatar.roleTitle + " keyframe " + actor.frameIndexMarker + "start");
            };
            TweenCallback cb_end = () =>
            {
                //Debug.Log(actor.avatar.roleTitle + " keyframe " + actor.frameIndexMarker + "finish");
            };

            //---this loop is internal key-frame  in the character.
            if (actor.frames.Count > 0)
            {
                while (actor.frameIndexMarker < actor.frames.Count)
                {
                    if (actor.targetRole != "")
                    {
                        if ((actor.avatar != null) && (actor.avatar.avatar != null))
                        {
                            //=======================================
                            //  Tween sequence for Key-frame
                            //---non-DOTween functions of each frame
                            Sequence seq = DOTween.Sequence();
                            seq.SetAutoKill(false);
                            seq.SetLink(gameObject);

                            //seq.OnPlay(cb_start);
                            //seq.OnRewind(cb_start);
                            seq.OnStepComplete(cb_end);
                            seq.AppendCallback(cb_start);

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

                            actorSeq.Append(ProcessBody_forFrame(seq, actor, avatarFrame, currentPlayingOptions));
                        }


                    }
                    actor.frameIndexMarker++;
                }
                actorSeq.AppendCallback(cb_end);
                if (durationList.Count > 0)
                {
                    durationList.Sort();

                    //---set maximum duration as sequence interval
                }
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
                SpecialUpdate_body(actor, frameIndex);

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
            if ((clipboard.targetRoleName == "") || (clipboard.keyFrame < 0)) return;

            NativeAnimationFrameActor nactor = GetFrameActorFromRole(clipboard.targetRoleName, clipboard.targetType);

            string[] prm = param.Split(',');
            string role = prm[0];
            int tmpi = int.TryParse(prm[1], out tmpi) ? tmpi : -1;
            if (tmpi == -1)
            {
                return;
            }
            AF_TARGETTYPE type = (AF_TARGETTYPE)tmpi;
            int newindex = int.TryParse(prm[2], out newindex) ? newindex : -1;

            if (role != nactor.targetRole) return;

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

            }
            else
            {
                NativeAnimationFrame nframe = nactor.frames[findex].SCopy();
                nframe.index = newindex;
                if ((overwriteIndex == -1) || (nactor.frames.Count <= overwriteIndex))
                {
                    nactor.frames.Add(nframe);
                }
                else
                {
                    nactor.frames.Insert(overwriteIndex, nframe);
                }
                

                SortActorFrames(nactor);
                adjustNearMaxFrameIndex(nactor, nframe);
            }

            //---To adjust all frame duration and finalIndex of the actor
            AdjustAllFrame(nactor, currentProject.baseDuration, true, true);

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(param);
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
        /// <param name="param">[0] - name, [1] - url, [2] - load flag(1 - load, 0 - no load)</param>
        public void LoadMaterialTexture(string param)
        {
            string[] prm = param.Split(",");
            bool isEffectiveLoad = false;
            if (prm.Length >= 3) isEffectiveLoad = (prm[2] == "1" ? true : false);

            NativeAP_OneMaterial nap = new NativeAP_OneMaterial(prm[0], prm[1], OneMaterialType.texture2d);

            int ishit = materialManager.textures.FindIndex(item =>
            {
                if (item.name == prm[0]) return true;
                return false;
            });
            string ret = "";
            if (ishit < 0)
            {
                if (isEffectiveLoad)
                {
                    StartCoroutine(nap.Open(prm[1]));
                }
                

                materialManager.textures.Add(nap);
                ret = nap.name;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif

        }

        /// <summary>
        /// Only to load texture file 
        /// </summary>
        /// <param name="param">[0] - name, [1] - url</param>
        public void LoadOneMaterialTexture(string param)
        {
            string[] prm = param.Split(",");
            int ishit = materialManager.textures.FindIndex(item =>
            {
                if (item.name == prm[0]) return true;
                return false;
            });
            string ret = "";
            if (ishit > -1)
            {
                StartCoroutine(materialManager.textures[ishit].Open());
                ret = materialManager.textures[ishit].name;
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
        /// 
        /// </summary>
        /// <param name="param">[0] - oldname, [1] - newname</param>
        public void RenameMaterialName(string param)
        {
            string[] prm = param.Split(",");
            int ishit = materialManager.textures.FindIndex(item =>
            {
                if (item.name == prm[0]) return true;
                return false;
            });
            string ret = "";
            if (ishit > -1)
            {
                int isnewhit = materialManager.textures.FindIndex(item =>
                {
                    if (item.name == prm[1]) return true;
                    return false;
                });
                if (isnewhit < 0)
                {
                    materialManager.textures[ishit].name = prm[1];
                    ret = prm[1];
                }
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif

        }
        public int RemoveMaterial(string name)
        {
            return materialManager.RemoveTexture(name);
        }
        public void RemoveMaterialFromOuter(string name)
        {
            int ret = materialManager.RemoveTexture(name);
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
        public void EnumMaterialTexture()
        {
            List<string> arr = new List<string>();
            materialManager.textures.ForEach(item =>
            {
                arr.Add(item.name+","+((int)item.materialType).ToString()+","+item.size.x.ToString()+","+item.size.y.ToString());
            });
            string ret = string.Join('\t', arr);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
    }


}