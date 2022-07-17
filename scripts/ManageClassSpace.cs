using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using DG.Tweening;
using System.Linq;

using System.IO;
using TriLibCore;
using TriLibCore.General;
using TriLibCore.SFB;


namespace UserHandleSpace
{

    //===============================================================================================================
    //  Material and General class
    //===============================================================================================================
    [Serializable]
    public class MaterialPropertiesBase
    {
        public string name = "";
        public string shaderName = "";
        //---standard / VRM/MToon
        public Color color = Color.white;
        public float blendmode = 0;
        public int textureIsCamera = 0;
        public string textureRole = "";
        public string texturePath = "";
        public Color emissioncolor = Color.white;
        //---standard
        public float metallic = 0;
        public float glossiness = 0;
        //---VRM/MToon
        public float cullmode = 0;
        public Color shadetexcolor = Color.white;
        public float shadingtoony = 0;
        public Color rimcolor = Color.white;
        public float rimfresnel = 0;
        public float srcblend = 0;
        public float dstblend = 0;
        //---FX/Water (Basic)
        public float waveScale = 0.0703f;
        //---FX/Water4
        public float fresnelScale = 0.75f;
        public Color reflectionColor = new Color(0.54f, 0.95f, 0.99f, 0.5f);
        public Color specularColor = new Color(0.72f, 0.72f, 0.72f, 1f);
        public Vector4 waveAmplitude = new Vector4(0.3f, 0.35f, 0.25f, 0.25f);
        public Vector4 waveFrequency = new Vector4(1.3f, 1.35f, 1.25f, 1.25f);
        public Vector4 waveSteepness = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
        public Vector4 waveSpeed = new Vector4(1.2f, 1.375f, 1.1f, 1.5f);
        public Vector4 waveDirectionAB = new Vector4(0.3f, 0.85f, 0.85f, 0.25f);
        public Vector4 waveDirectionCD = new Vector4(0.1f, 0.9f, 0.5f, 0.5f);
    }
    [Serializable]
    public class MaterialProperties : MaterialPropertiesBase
    {
        public Texture realTexture = null;
    }

    //===============================================================================================================
    //  Pose file class
    //===============================================================================================================
    [Serializable]
    public class AvatarEquipSaveClass
    {
        public HumanBodyBones bodybonename;

        /// <summary>
        /// Equipment role name in projects
        /// </summary>
        public string equipitem;
        public Vector3 position;
        public Vector3 rotation;
    }
    [Serializable]
    public class AvatarEquipmentClass
    {
        public List<AvatarEquipSaveClass> list;
        public AvatarEquipmentClass()
        {
            list = new List<AvatarEquipSaveClass>();
        }
    }
    [Serializable]
    public class BasicStringFloatList
    {
        public string text;
        public float value;

        public BasicStringFloatList(string t, float v)
        {
            text = t;
            value = v;
        }
    }

    [Serializable]
    public class BasicStringIntList
    {
        public string text;
        public int value;

        public BasicStringIntList(string t, int v)
        {
            text = t;
            value = v;
        }
    }
    [Serializable]
    public class BasicStringColorList
    {
        public string text;
        public Color value;

        public BasicStringColorList(string t, Color v)
        {
            text = t;
            value = v;
        }
    }
    [Serializable]
    public class BasicTransformInformation
    {
        public string id = "";
        public string dimension = "";
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }
    /*
     * "EyeViewHandle", "Head", "LookAt", "Aim", "Chest", "Pelvis", "LeftLowerArm", "LeftHand",
            "RightLowerArm","RightHand","LeftLowerLeg","LeftLeg","RightLowerLeg","RightLeg"
     * */
    public enum IKBoneType
    {
        IKParent = 0,
        EyeViewHandle,
        Head,
        LookAt,
        Aim,
        Chest,
        Pelvis,
        LeftShoulder,
        LeftLowerArm,
        LeftHand,
        RightShoulder,
        RightLowerArm,
        RightHand,
        LeftLowerLeg,
        LeftLeg,
        RightLowerLeg,
        RightLeg,
    };
    public enum RangeIKBoneType
    {
        Head_Chest = 0,
        Chest_Pelvis,
        Chest_LeftLowerArm,
        Chest_RightLowerArm,
        LeftLower_LeftHand,
        RightLower_RightHand,
        Pelvis_LeftLower,
        Pelvis_RightLower,
        LeftLower_LeftLeg,
        RightLower_RightLeg
    };

    [Serializable]
    public class AvatarTransformSaveClass
    {
        public string sampleavatar;
        public string thumbnail;
        public AnimationFrameActor frameData;

        //public string type;
        /* Sample avatar specify information
         * 0-2 bounds.extents
         * 3-5 Chest.transform.position
         * 6-8 Pelvis.transform.position
         * */
        //public float[] bodyinfo = new float[3 + 3 + 3];

        //public float[] bodyHeight = new float[3];
        /*
         * List[0] - Height
         * List[1]~[n] - IKbone
         */
        //public List<Vector3> bodyInfoList = new List<Vector3>();
        //---effective transform data
        /*
        public List<float> handpose;
        public List<BasicStringFloatList> blendshapes;
        public List<Vector3> positions;
        public List<Vector3> rotations;
        public List<Vector3> bonerotations;
        public List<AvatarEquipSaveClass> equips;
        //---options
        public float duration;
        public int useik;
        */

    }
    [Serializable]
    public class AvatarSingleIKTransform
    {
        public string ikname;
        public Vector3 position;
        public Vector3 rotation;
        public AvatarSingleIKTransform(string name, Vector3 pos, Vector3 rot)
        {
            ikname = name;
            position = pos;
            rotation = rot;
        }
    }
    [Serializable]
    public class AvatarAllIKParts
    {
        public List<AvatarSingleIKTransform> list = new List<AvatarSingleIKTransform>();
    }

    [Serializable]
    public class VRMGravityInfoBase
    {
        public int id = 0;
        public float power = 0f;
        public Vector3 dir = new Vector3(0, -1f, 0);

        public VRMGravityInfoBase()
        {

        }
        public VRMGravityInfoBase(int p_id, float p_power, float p_x, float p_y, float p_z)
        {
            id = p_id;
            power = p_power;
            dir = new Vector3(p_x, p_y, p_z);
        }
    }
    [Serializable]
    public class VRMGravityInfo : VRMGravityInfoBase
    {
        public string comment = "";
        public string rootBoneName = "";
        
        public VRMGravityInfo()
        {
            
        }
        public VRMGravityInfo(string p_comment, string p_bonename, float p_power, float p_x, float p_y, float p_z)
        {
            comment = p_comment;
            rootBoneName = p_bonename;
            power = p_power;
            dir = new Vector3(p_x,p_y,p_z);
        }
    }
    [Serializable]
    public class AvatarGravityClass
    {
        public List<VRMGravityInfo> list;
        public AvatarGravityClass()
        {
            list = new List<VRMGravityInfo>();
        }
    }
    [Serializable]
    public class AvatarIKMappingClass
    {
        public IKBoneType parts;
        public string name;
        public AvatarIKMappingClass()
        {
            parts = IKBoneType.IKParent;
            name = "self";
        }

    }

    [Serializable]
    public class AvatarPunchEffect
    {
        public int isEnable;
        public AF_MOVETYPE translationType;
        public Vector3 punch;
        public int vibrato;
        public float elasiticity;
        public AvatarPunchEffect()
        {
            isEnable = 0;
            translationType = AF_MOVETYPE.Translate;
            punch = Vector3.zero;
            vibrato = 10;
            elasiticity = 1f;
        }
        public void Copy(AvatarPunchEffect src)
        {
            isEnable = src.isEnable;
            translationType = src.translationType;
            punch.x = src.punch.x;
            punch.y = src.punch.y;
            punch.z = src.punch.z;
            vibrato = src.vibrato;
            elasiticity = src.elasiticity;
        }

    }

    [Serializable]
    public class AvatarShakeEffect
    {
        public int isEnable;
        public AF_MOVETYPE translationType;
        public float strength;
        public int vibrato;
        public int randomness;
        public int fadeOut;
        public AvatarShakeEffect()
        {
            isEnable = 0;
            translationType = AF_MOVETYPE.Translate;
            strength = 1f;
            vibrato = 10;
            randomness = 90;
            fadeOut = 1;
        }
        public void Copy(AvatarShakeEffect src)
        {
            isEnable = src.isEnable;
            translationType = src.translationType;
            strength = src.strength;
            vibrato = src.vibrato;
            randomness = src.randomness;
            fadeOut = src.fadeOut;
        }
    }


    //===============================================================================================================
    //  Motion file class
    //===============================================================================================================
    public enum AF_TARGETTYPE
    {
        VRM = 0,
        OtherObject,
        Light,
        Camera,
        Text,
        Image,
        UImage,
        Audio,
        Effect,
        SystemEffect,
        Stage,

        Unknown = 99
    }
    public enum AF_MOVETYPE
    {
        Rest = 0,
        Start,
        NormalTransform,
        Translate,
        Rotate,
        Scale,
        BlendShape,
        LookAt,

        AnimStart,
        AnimStop,
        AnimSeek,
        AnimProperty,

        ObjectTexture,

        Light = 20,
        LightProperty,
        Camera,
        CameraProperty,
        CameraOn,
        CameraOff,
        Text,
        TextProperty,
        Image,
        ImageProperty,
        Audio,
        AudioProperty,

        SystemEffect,
        SystemEffectOff,

        Stage,
        StageProperty,
        SkyProperty,

        Equipment,

        AnimPause,

        //---effect
        Punch = 40,
        Shake,
        Jump,
        Coloring,
        Collider,

        //---vrm
        GravityProperty = 50,
        VRMIKProperty = 51,
        VRMBlink = 52,


        Stop = 99
    };
    public enum AF_TRANSLATETYPE
    {
        Normal = 0,
        Punch,
        Shake,
    };
    public enum ParseIKBoneType
    {
        IKParent = 0,
        EyeViewHandle,
        Head,
        LookAt,
        Aim,
        Chest,
        Pelvis,
        LeftShoulder,
        LeftLowerArm,
        LeftHand,
        RightShoulder,
        RightLowerArm,
        RightHand,
        LeftLowerLeg,
        LeftLeg,
        RightLowerLeg,
        RightLeg,
        LeftHandPose,
        RightHandPose,
        BlendShape,

        Unknown = 25,
        UseHumanBodyBones = 99
    };
    public enum UseBodyInfoType
    {
        TimelineCharacter = 0,
        CurrentAvatar
    }
    public enum UserAnimationState
    {
        Stop = 0,
        Play = 1,
        PlayWithLoop,
        Playing,
        Seeking,
        Pause
    }
    public enum UserPrimitiveType
    {
        Sphere = 0,
        Capsule = 1,
        Cylinder = 2,
        Cube = 3,
        Plane = 4,
        Quad = 5,
        WaterLevel = 6
    }

    [Serializable]
    public class AnimationTargetParts
    {
        public ParseIKBoneType vrmBone;
        public HumanBodyBones vrmHumanBodyBone;

        public AF_MOVETYPE animationType;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
        public AvatarPunchEffect effectPunch;
        public AvatarShakeEffect effectShake;
        public int jumpNum;
        public float jumpPower;

        //---vrm options
        public int isHandPose;
        public List<float> handpose;
        public int isBlendShape;
        public List<BasicStringFloatList> blendshapes;
        public int equipType;
        public List<AvatarEquipSaveClass> equipDestinations;
        public AvatarGravityClass gravity;
        public string viewHandle;
        public List<AvatarIKMappingClass> handleList;
        public int isblink;
        public float interval;
        public float openingSeconds;
        public float closeSeconds;
        public float closingTime;
        public int headLock;


        //---vrm and other object
        public UserAnimationState animPlaying;
        public float animSpeed;
        public float animSeek;
        public int animLoop;
        public string animName;
        public MaterialProperties vmatProp;

        //---other object
        public string renderTextureId;
        //public int isEquip;
        //public string equippedRoleName;
        public List<MaterialProperties> matProp;


        //---light options
        public LightType lightType;
        public float range;
        public Color color;
        public float power;
        public float spotAngle;
        public LightRenderMode lightRenderMode;
        public float shadowStrength;

        //---camera options
        public int cameraPlaying;
        public int clearFlag;
        public float fov;
        public float depth;
        public Rect viewport;
        public int renderFlag;
        public Vector2 renderTex;

        //---text options
        public string text = "";
        public int fontSize;
        public FontStyle fontStyle;
        public TextAnchor textAlignment;

        //---image options

        //---audio
        public string audioName;
        public int isSE;
        public int isLoop;
        public int isMute;
        public float volume;
        public float pitch;
        public float seekTime;

        //---effect
        public string effectGenre;
        public int isVRMCollider;
        public float VRMColliderSize;
        public List<string> VRMColliderTarget;

        //---system effect
        public string effectName = "";
        public List<float> effectValues;

        //---stage
        public int stageType;
        public Vector4 wavespeed;
        public float wavescale;
        public float windDurationMin;
        public float windDurationMax;
        public float windPower;
        public float windFrequency;
        public float userStageMetallic;
        public float userStageGlossiness;
        public Color userStageEmissionColor;
        public CameraClearFlags skyType;
        public Color skyColor;
        public string skyShaderName;
        public List<BasicStringFloatList> skyShaderFloat;
        public List<BasicStringColorList> skyShaderColor;


        public AnimationTargetParts()
        {
            vrmBone = ParseIKBoneType.IKParent;
            vrmHumanBodyBone = HumanBodyBones.Hips;
            animationType = AF_MOVETYPE.Rest;
            effectPunch = new AvatarPunchEffect();
            effectShake = new AvatarShakeEffect();
            jumpNum = 0;
            jumpPower = 1f;
            isHandPose = 0;
            isBlendShape = 0;
            equipType = 0;
            equipDestinations = new List<AvatarEquipSaveClass>();
            isblink = 0;
            interval = 5.0f;
            openingSeconds = 0.03f;
            closingTime = 0.06f;
            closeSeconds = 0.1f;
            closingTime = 0f;
            headLock = 1;
            animPlaying = 0;
            animLoop = 0;
            animSeek = 0;
            animSpeed = 1.0f;
            animName = "";
            handpose = new List<float>();
            blendshapes = new List<BasicStringFloatList>();
            viewHandle = "self";
            handleList = new List<AvatarIKMappingClass>();
            matProp = new List<MaterialProperties>();
            effectValues = new List<float>();
            windDurationMin = 0.01f;
            windDurationMax = 0.02f;
            windPower = 0;
            windFrequency = 0.01f;
            skyColor = ColorUtility.TryParseHtmlString("#314D79",out skyColor) ? skyColor : Color.blue;
            skyType = CameraClearFlags.SolidColor;
            skyShaderFloat = new List<BasicStringFloatList>();
            skyShaderColor = new List<BasicStringColorList>();
            renderTex = new Vector2();
            //isEquip = 0;
            //equippedRoleName = "";
            gravity = new AvatarGravityClass();
            isVRMCollider = 0;
            VRMColliderSize = 0.1f;
            VRMColliderTarget = new List<string>();
            vmatProp = new MaterialProperties();

        }
        public AnimationTargetParts SCopy()
        {
            AnimationTargetParts sc = (AnimationTargetParts)MemberwiseClone();
            if (sc.handpose != null)
            {
                sc.handpose = new List<float>(handpose);
            }
            if (sc.blendshapes != null)
            {
                sc.blendshapes = new List<BasicStringFloatList>(blendshapes);
            }
            if (sc.effectValues != null)
            {
                sc.effectValues = new List<float>(effectValues);
            }
            if (sc.equipDestinations != null)
            {
                sc.equipDestinations = new List<AvatarEquipSaveClass>(equipDestinations);
            }
            if (sc.gravity != null)
            {
                sc.gravity = new AvatarGravityClass();
                sc.gravity.list = new List<VRMGravityInfo>(gravity.list);
            }
            if (sc.handleList != null)
            {
                sc.handleList = new List<AvatarIKMappingClass>(handleList);
            }
            if (sc.matProp != null)
            {
                sc.matProp = new List<MaterialProperties>(matProp);
            }
            if (sc.skyShaderFloat != null)
            {
                sc.skyShaderFloat = new List<BasicStringFloatList>(skyShaderFloat);
            }
            if (sc.skyShaderColor != null)
            {
                sc.skyShaderColor = new List<BasicStringColorList>(skyShaderColor);
            }

            return sc;
        }
    }

    //==============================================================
    //  AnimationFrame
    //==============================================================

    [Serializable]
    public class AnimationFrame
    {
        public int index;
        public int finalizeIndex;
        public string key;
        public float duration;
        public Ease ease;

        public List<string> movingData = new List<string>();
        //public List<AnimationTargetParts> movingData = new List<AnimationTargetParts>();

        public AnimationFrame()
        {
            index = 0;
            finalizeIndex = 0;
            key = "";
            duration = 0.1f;
            ease = Ease.Linear;
        }
        public AnimationFrame SCopy()
        {
            AnimationFrame sc = (AnimationFrame)MemberwiseClone();
            if (movingData != null)
            {
                sc.movingData = new List<string>(movingData);
                //sc.movingData = new List<AnimationTargetParts>(movingData);
            }

            return sc;
        }
        public void SetFromNative(NativeAnimationFrame naf)
        {
            index = naf.index;
            finalizeIndex = naf.finalizeIndex;
            key = naf.key;
            duration = naf.duration;
            ease = naf.ease;
        }
    }
    [Serializable]
    public class NativeAnimationFrame : AnimationFrame
    {

        public UseBodyInfoType useBodyInfo;
        public new List<AnimationTargetParts> movingData = new List<AnimationTargetParts>();

        public NativeAnimationFrame()
        {
            useBodyInfo = UseBodyInfoType.TimelineCharacter;
            movingData = new List<AnimationTargetParts>();
        }
        public new NativeAnimationFrame SCopy()
        {
            NativeAnimationFrame sc = (NativeAnimationFrame)MemberwiseClone();
            if (movingData != null)
            {
                sc.movingData = new List<AnimationTargetParts>(movingData);
            }
            return sc;
        }
        public void SetFromRaw(AnimationFrame af)
        {
            index = af.index;
            finalizeIndex = af.finalizeIndex;
            key = af.key;
            duration = af.duration;
            ease = af.ease;

        }
        public AnimationTargetParts FindMovingData(AF_MOVETYPE movetype, ParseIKBoneType bone = ParseIKBoneType.Unknown)
        {
            return movingData.Find(match =>
            {
                if (match.animationType == movetype)
                {
                    if (bone == ParseIKBoneType.Unknown) return true;

                    if (match.vrmBone == bone) return true;
                    return false;
                }
                return false;
            });

        }
    }
    //==============================================================
    //  AvatarAttachedNativeAnimationFrame, ConfirmedNativeAnimationFrame
    //==============================================================

    [Serializable]
    public class AvatarAttachedNativeAnimationFrame
    {
        public string id = "";
        public string role = "";
        public AF_TARGETTYPE type = AF_TARGETTYPE.Unknown;
        public AnimationFrame frame = null;
        public AvatarAttachedNativeAnimationFrame(AnimationFrameActor actor)
        {
            id = actor.targetId;
            role = actor.targetRole;
            type = actor.targetType;
            frame = new AnimationFrame();
        }
    }
    [Serializable]
    public class ConfirmedNativeAnimationFrame
    {
        public List<AvatarAttachedNativeAnimationFrame> frames = new List<AvatarAttachedNativeAnimationFrame>();
    }

    //==============================================================
    //  AnimationSingleMotion
    //==============================================================
    [Serializable]
    public class AnimationSingleMotion
    {
        //public AnimationAvatar cast;
        //public AnimationFrameActor timeline;
        public int version = 1;

        public AF_TARGETTYPE targetType = AF_TARGETTYPE.Unknown;
        public int compiled = 0;
        public List<AnimationSingleFrame> frames;

        /* Sample avatar specify information
         * 0-2 bounds.extents
         * 
         * 
         * */
        public float[] bodyHeight = new float[3];
        public List<Vector3> bodyInfoList;
        public List<string> blendShapeList;

        public AnimationSingleMotion()
        {
            
            bodyInfoList = new List<Vector3>();
            blendShapeList = new List<string>();
            frames = new List<AnimationSingleFrame>();
        }
        public AnimationSingleMotion SCopy()
        {
            AnimationSingleMotion sc = (AnimationSingleMotion)MemberwiseClone();
            //--- To clone to this class children from "sc"
            if (bodyHeight != null)
            {
                sc.bodyHeight = new float[sc.bodyHeight.Length];
                Array.Copy(sc.bodyHeight, bodyHeight, sc.bodyHeight.Length);
            }
            if (bodyInfoList != null)
            {
                for (int i = 0; i < sc.bodyInfoList.Count; i++)
                {
                    bodyInfoList.Add(new Vector3(sc.bodyInfoList[i].x, sc.bodyInfoList[i].y, sc.bodyInfoList[i].z));
                }
            }
            if (blendShapeList != null)
            {
                for (int i = 0; i < sc.blendShapeList.Count; i++)
                {
                    blendShapeList.Add(sc.blendShapeList[i]);
                }
            }
            return sc;
        }
    }
    [Serializable]
    public class AnimationSingleFrame
    {
        public int index;
        public int finalizeIndex;
        public string key;
        public float duration;
        public Ease ease;

        public List<string> movingData = new List<string>();

        public AnimationSingleFrame()
        {
            index = 0;
            finalizeIndex = 0;
            key = "";
            duration = 0.1f;
            ease = Ease.Linear;
        }
        public AnimationSingleFrame SCopy()
        {
            AnimationSingleFrame sc = (AnimationSingleFrame)MemberwiseClone();
            if (movingData != null)
            {
                sc.movingData = new List<string>(movingData);
            }

            return sc;
        }
    }

    //==============================================================
    //  AnimationFrameActor
    //==============================================================

    [Serializable]
    public class AnimationFrameActor
    {
        public string targetId = "";
        public string targetRole = "";
        public AF_TARGETTYPE targetType = AF_TARGETTYPE.Unknown;
        public int enabled = 1;
        public int compiled = 0;

        /* Sample avatar specify information
         * 0-2 bounds.extents
         * 
         * 
         * */
        public float[] bodyHeight = new float[3];
        public List<Vector3> bodyInfoList;
        public List<string> blendShapeList;
        public List<string> gravityBoneList;

        public List<AnimationFrame> frames;
        
        //---Only Playing mode: List index of frames
        public int frameIndexMarker;

        public AnimationFrameActor()
        {
            bodyInfoList = new List<Vector3>();
            blendShapeList = new List<string>();
            gravityBoneList = new List<string>();
            frames = new List<AnimationFrame>();
            frameIndexMarker = 0;
        }
        public void SetFromNative(NativeAnimationFrameActor naf)
        {
            targetId = naf.targetId;
            targetType = naf.targetType;
            targetRole = naf.targetRole;
            enabled = naf.enabled;
            //bodyHeight = naf.bodyHeight;
            Array.Copy(naf.bodyHeight, bodyHeight, naf.bodyHeight.Length);
            for (int i = 0; i < naf.bodyInfoList.Count; i++)
            {
                bodyInfoList.Add(new Vector3(naf.bodyInfoList[i].x, naf.bodyInfoList[i].y, naf.bodyInfoList[i].z));
            }
            for (int i = 0; i < naf.blendShapeList.Count; i++)
            {
                blendShapeList.Add(naf.blendShapeList[i]);
            }
            for (int i = 0; i < naf.gravityBoneList.Count; i++)
            {
                gravityBoneList.Add(naf.gravityBoneList[i]);
            }
            frameIndexMarker = naf.frameIndexMarker;
        }
        public AnimationFrameActor SCopy()
        {
            //--- sc is Copy.
            AnimationFrameActor sc = (AnimationFrameActor)MemberwiseClone();
            //--- To clone to this class children from "sc"
            if (bodyHeight != null)
            {
                sc.bodyHeight = new float[sc.bodyHeight.Length];
                Array.Copy(sc.bodyHeight, bodyHeight, sc.bodyHeight.Length);
            }
            if (bodyInfoList != null)
            {
                for (int i = 0; i < sc.bodyInfoList.Count; i++)
                {
                    bodyInfoList.Add(new Vector3 (sc.bodyInfoList[i].x, sc.bodyInfoList[i].y, sc.bodyInfoList[i].z) );
                }
            }
            if (blendShapeList != null)
            {
                for (int i = 0; i < sc.blendShapeList.Count; i++)
                {
                    blendShapeList.Add(sc.blendShapeList[i]);
                }
            }
            if (gravityBoneList != null)
            {
                for (int i = 0; i < sc.gravityBoneList.Count; i++)
                {
                    gravityBoneList.Add(sc.gravityBoneList[i]);
                }
            }
            return sc;
        }
    }
    public class NativeAnimationFrameActor : AnimationFrameActor
    {
        public NativeAnimationAvatar avatar;

        public new List<NativeAnimationFrame> frames;

        public NativeAnimationFrameActor()
        {
            avatar = new NativeAnimationAvatar();
            frames = new List<NativeAnimationFrame>();
        }

        public void SetFromRaw(AnimationFrameActor naf)
        {
            targetId = naf.targetId;
            targetType = naf.targetType;
            targetRole = naf.targetRole;
            enabled = naf.enabled;
            //bodyHeight = naf.bodyHeight;
            Array.Copy(naf.bodyHeight, bodyHeight, naf.bodyHeight.Length);
            bodyInfoList.Clear();
            for (int i = 0; i < naf.bodyInfoList.Count; i++)
            {
                bodyInfoList.Add(new Vector3(naf.bodyInfoList[i].x, naf.bodyInfoList[i].y, naf.bodyInfoList[i].z));
            }
            blendShapeList.Clear();
            for (int i = 0; i < naf.blendShapeList.Count; i++)
            {
                blendShapeList.Add(naf.blendShapeList[i]);
            }
            for (int i = 0; i < naf.gravityBoneList.Count; i++)
            {
                gravityBoneList.Add(naf.gravityBoneList[i]);
            }
            frameIndexMarker = naf.frameIndexMarker;
        }
        public new NativeAnimationFrameActor SCopy()
        {
            NativeAnimationFrameActor sc = (NativeAnimationFrameActor)MemberwiseClone();
            if (sc.avatar != null)
            {
                sc.avatar = avatar.SCopy();
            }
            return sc;
        }
    }


    //==============================================================
    //  AnimationMotionTimeline
    //==============================================================

    [Serializable]
    public class AnimationMotionTimeline
    {

        public List<AnimationFrameActor> characters;

        public AnimationMotionTimeline()
        {
            characters = new List<AnimationFrameActor>();
        }
        public AnimationMotionTimeline SCopy()
        {
            AnimationMotionTimeline sc = (AnimationMotionTimeline)MemberwiseClone();
            if (sc.characters != null)
            {
                sc.characters = new List<AnimationFrameActor>(characters);
            }

            return sc;
        }
    }
    public class NativeAnimationMotionTimeline
    {
        public List<NativeAnimationFrameActor> characters;

        public NativeAnimationMotionTimeline ()
        {
            characters = new List<NativeAnimationFrameActor>();
        }
        public NativeAnimationMotionTimeline SCopy()
        {
            NativeAnimationMotionTimeline sc = (NativeAnimationMotionTimeline)MemberwiseClone();
            if (sc.characters != null)
            {
                sc.characters = new List<NativeAnimationFrameActor>(characters);
            }

            return sc;
        }

    }
    //==============================================================
    //  AnimationAvatar
    //==============================================================

    [Serializable]
    public class AnimationAvatar
    {
        public string roleName = "";
        public string roleTitle = "";
        public string avatarId = "";
        public string avatarTitle = "";
        public AF_TARGETTYPE type = AF_TARGETTYPE.Unknown;
        public float[] bodyHeight = new float[3];
        public List<Vector3> bodyInfoList = new List<Vector3>();
        public string path = "";
        public string ext = "";

        public AnimationAvatar SCopy()
        {
            AnimationAvatar sc = (AnimationAvatar)MemberwiseClone();
            if (bodyHeight != null)
            {
                sc.bodyHeight = new float[sc.bodyHeight.Length];
                Array.Copy(sc.bodyHeight, bodyHeight, sc.bodyHeight.Length);
            }
            if (bodyInfoList != null)
            {
                for (int i = 0; i < sc.bodyInfoList.Count; i++)
                {
                    bodyInfoList.Add(new Vector3 ( sc.bodyInfoList[i].x, sc.bodyInfoList[i].y, sc.bodyInfoList[i].z) );
                }
            }
            return sc;
        }
        public void SetFromNative(NativeAnimationAvatar nav)
        {
            roleName = nav.roleName;
            roleTitle = nav.roleTitle;
            avatarId = nav.avatarId;
            avatarTitle = nav.avatarTitle;
            type = nav.type;
            path = nav.path;
            ext = nav.ext;
            if (bodyHeight != null)
            {
                Array.Copy(nav.bodyHeight, bodyHeight, nav.bodyHeight.Length);
            }
            if (bodyInfoList != null)
            {
                for (int i = 0; i < nav.bodyInfoList.Count; i++)
                {
                    bodyInfoList.Add(new Vector3(nav.bodyInfoList[i].x, nav.bodyInfoList[i].y, nav.bodyInfoList[i].z));
                }
            }
        }
    }
    public class NativeAnimationAvatar : AnimationAvatar
    {
        public GameObject avatar;
        public GameObject ikparent;
        public NativeAnimationAvatar()
        {
            avatar = null;
            ikparent = null;
        }
        public new NativeAnimationAvatar SCopy()
        {
            NativeAnimationAvatar sc = (NativeAnimationAvatar)MemberwiseClone();

            return sc;
        }
    }
    //==============================================================
    //  AnimationProjectMetaInformation
    //==============================================================

    [Serializable]
    public class AnimationProjectMetaInformation
    {
        public string name = "";
        public string license = "";
        public string description = "";
        public string coverImage = "";
        public string referURL = "";

        public AnimationProjectMetaInformation SCopy()
        {
            return (AnimationProjectMetaInformation)MemberwiseClone();
        }
    }
    [Serializable]
    public class AnimationProjectFixedProperties
    {
        
    }
    //==============================================================
    //  AnimationProjectOneMaterial
    //==============================================================
    [Serializable]
    public enum OneMaterialType
    {
        Texture = 0,
        Shader,
        unknown = 9,
    }
    [Serializable]
    public enum OneMaterialFrom
    {
        app = 0,
        project
    }
    [Serializable]
    public class AP_OneMaterial : IDisposable
    {
        public string group = "";
        public string name = "";
        public string path = "";
        public OneMaterialType materialType = OneMaterialType.unknown;
        public Vector2 size;

        public void Dispose()
        {

        }
    }
    public class NativeAP_OneMaterial : AP_OneMaterial
    {
        public int refCount = 0;
        private UnityEngine.Object effectiveObject;

        public UnityEngine.Object data
        {
            get 
            {
                return effectiveObject;
            }
        }
        public new void Dispose()
        {
            UnityEngine.Object.Destroy(effectiveObject);
        }
        public NativeAP_OneMaterial()
        {
            effectiveObject = null;
            size = new Vector2();
        }
        public NativeAP_OneMaterial(string material_name, string uri, OneMaterialType matType)
        {
            group = "";
            name = material_name;
            path = uri;
            materialType = matType;
            size = new Vector2();
            effectiveObject = null;
            //StartCoroutine(Open(uri));
        }

        /// <summary>
        /// Open material file from HTML-URI
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public IEnumerator Open(string uri)
        {
            path = uri;
            using (UnityWebRequest www = UnityWebRequest.Get(uri))
            {
                yield return www.SendWebRequest();
                //if (www.isNetworkError || www.isHttpError)
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                    yield break;
                }
                else
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(www.downloadHandler.data);
                    effectiveObject = tex;
                    size.x = tex.width;
                    size.y = tex.height;

                    yield return null;
                }
            }
        }

        /// <summary>
        /// Open material file from saved URI
        /// </summary>
        /// <returns></returns>
        public IEnumerator Open()
        {
            using (UnityWebRequest www = UnityWebRequest.Get(path))
            {
                yield return www.SendWebRequest();
                //if (www.isNetworkError || www.isHttpError)
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                    yield break;
                }
                else
                {
                    Texture2D tex = new Texture2D(1, 1);
                    tex.LoadImage(www.downloadHandler.data);
                    effectiveObject = tex;
                    size.x = tex.width;
                    size.y = tex.height;

                    yield return null;
                }
            }
        }

        /// <summary>
        /// Open material file from Native file dialog
        /// </summary>
        public void OpenFile()
        {
            string[] ext = new string[2];
            ext[0] = "jpg";
            ext[1] = "png";

            ExtensionFilter[] exts = new ExtensionFilter[] {
                new ExtensionFilter("JPEG File", "jpg"),
                new ExtensionFilter("PNG File", "png"),
                new ExtensionFilter("Image File", ext),
            };
            IList<ItemWithStream> paths = StandaloneFileBrowser.OpenFilePanel("Please select an image file.", "", exts, false);
            for (int i = 0; i < paths.Count; i++)
            {
                Stream stm = paths[i].OpenStream();
                path = paths[i].Name;

                byte[] byt = new byte[stm.Length];
                stm.Read(byt, 0, (int)stm.Length);
                Texture2D tex = new Texture2D(1, 1);
                tex.LoadImage(byt);

                effectiveObject = tex;
                size.x = tex.width;
                size.y = tex.height;
            }
        }

        /// <summary>
        /// Set an object directly
        /// </summary>
        /// <param name="obj"></param>
        public void SetObject(UnityEngine.Object obj)
        {
            effectiveObject = obj;
        }

        /// <summary>
        /// To get reference of the texture
        /// </summary>
        /// <returns></returns>
        public Texture2D ReferTexture2D()
        {
            if (effectiveObject == null) return null;

            refCount++;
            return (Texture2D)effectiveObject;
        }

        /// <summary>
        /// Unreference from an object
        /// </summary>
        /// <returns></returns>
        public int UnRefer()
        {
            if (effectiveObject == null) return 0;

            refCount--;
            return refCount;
        }
    }

    //==============================================================
    //  AnimationProjectMaterialPackage
    //==============================================================

    [Serializable]
    public class AnimationProjectMaterialPackage : IDisposable
    {
        public List<AP_OneMaterial> materials = new List<AP_OneMaterial>();

        public void Dispose()
        {
            materials.ForEach(item => {
                item.Dispose();
                item = null;
            });
            materials.Clear();
        }
        public void SetFromNative(NativeAnimationProjectMaterialPackage pkg)
        {
            foreach( NativeAP_OneMaterial naom in pkg.materials)
            {
                AP_OneMaterial aom = new AP_OneMaterial();
                aom.name = naom.name;
                aom.group = naom.group;
                //---do not save path made by URL.createObjectURL()
                aom.path = (naom.path.IndexOf("blob:") > -1) ? "" :  naom.path;
                aom.materialType = naom.materialType;
                aom.size = new Vector2(naom.size.x, naom.size.y);
                materials.Add(aom);
            }
        }
    }
    public class NativeAnimationProjectMaterialPackage : AnimationProjectMaterialPackage 
    {
        public new List<NativeAP_OneMaterial> materials = new List<NativeAP_OneMaterial>();
        //public Dictionary<string, AssetBundle> bundles = new Dictionary<string, AssetBundle>();

        public new void Dispose()
        {
            materials.ForEach(item => {
                item.Dispose();
                item = null;
            });
            materials.Clear();

            /*
            Dictionary<string, AssetBundle>.Enumerator de_bundle =  bundles.GetEnumerator();
            while (de_bundle.MoveNext())
            {
                de_bundle.Current.Value.Unload(true);
            }
            bundles.Clear();
            */
        }
        public void SetFromRaw(AnimationProjectMaterialPackage pkg)
        {
            foreach (AP_OneMaterial aom in pkg.materials)
            {
                NativeAP_OneMaterial naom = new NativeAP_OneMaterial();
                naom.name = aom.name;
                naom.group = aom.group;
                naom.path = aom.path;
                naom.materialType = aom.materialType;
                naom.size = new Vector2(aom.size.x, aom.size.y);
                materials.Add(naom);
            }
        }
        /*
        public IEnumerator OpenBundle(string name, string uri)
        {
            using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(uri))
            {
                yield return www.SendWebRequest();
                //if (www.isNetworkError || www.isHttpError)
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                    yield break;
                }
                else
                {
                    AssetBundle bnd = DownloadHandlerAssetBundle.GetContent(www);
                    bundles[name] = bnd;
                    
                    yield return null;
                }
            }
        }
        public void CloseBundle(string name)
        {
            if (bundles.ContainsKey(name))
            {
                bundles[name].Unload(true);
                bundles.Remove(name);
            }
        }
        public Texture FindTextureFromBundle(string path)
        {
            Texture tex = null;

            Dictionary<string, AssetBundle>.Enumerator de_bundle = bundles.GetEnumerator();
            while (de_bundle.MoveNext())
            {
                AssetBundle bnd = de_bundle.Current.Value;
                if (bnd.Contains(path))
                {
                    tex = bnd.LoadAsset<Texture>(path);
                    break;
                }
            }

            return tex;
        }
        public Shader FindShaderFromBundle(string path)
        {
            Shader tex = null;

            Dictionary<string, AssetBundle>.Enumerator de_bundle = bundles.GetEnumerator();
            while (de_bundle.MoveNext())
            {
                AssetBundle bnd = de_bundle.Current.Value;
                if (bnd.Contains(path))
                {
                    tex = bnd.LoadAsset<Shader>(path);
                    break;
                }
            }

            return tex;
        }
        */


        /// <summary>
        /// Get the object, search by group & name
        /// </summary>
        /// <param name="name">material name</param>
        /// <param name="group">material group (optional)</param>
        /// <returns></returns>
        public NativeAP_OneMaterial FindTexture(string name, string group = "")
        {
            return materials.Find(item =>
            {
                if ((item.materialType == OneMaterialType.Texture) && (item.name == name))
                {
                    if (group == "")
                    {
                        return true;
                    }
                    else
                    {
                        if (item.group == group) return true;
                        return false;
                    }

                }
                return false;
            });
        }
        /// <summary>
        /// Nullize parameter's object, if obj found in this list.
        /// </summary>
        /// <param name="obj"></param>
        public int UnRefer(UnityEngine.Object obj)
        {
            NativeAP_OneMaterial nap = materials.Find(item =>
            {
                if (item.data == obj) return true;
                return false;
            });
            int ret = -1;
            if (nap != null)
            {
                nap.UnRefer();
                obj = null;
                ret = nap.refCount;
            }
            return ret;
        }
        public int UnRefer(OneMaterialType type, string name, string group = "")
        {
            NativeAP_OneMaterial nap = materials.Find(item =>
            {
                if ((item.materialType == type) && (item.name == name))
                {
                    if (item.group == "")
                    {
                        return true;
                    }
                    else
                    {
                        if (item.group == group) return true;
                        return false;
                    }
                    
                }
                return false;
            });
            int ret = -1;
            if (nap != null)
            {
                nap.UnRefer();
                ret = nap.refCount;
            }
            return ret;
        }
        public int RemoveTexture(string name, string group = "")
        {
            NativeAP_OneMaterial nap = materials.Find(item =>
            {
                //Debug.Log(item.name + "-" + item.group + "-ref=" + item.refCount.ToString());
                if ((item.materialType == OneMaterialType.Texture) && (item.name == name))
                {
                    if(item.group == "")
                    {
                        return true;
                    }
                    else
                    {
                        if (item.group == group) return true;
                        return false;
                    }
                }
                return false;
            });
            int ret = -1;
            //Debug.Log(name + "=>");
            if (nap != null)
            {
                //Debug.Log("nap: " + nap.name + "=" + nap.refCount.ToString());
                if (nap.refCount <= 0)
                {
                    //Debug.Log("Dispose!");
                    nap.Dispose();
                    materials.Remove(nap);
                    ret = 0;
                }
                else
                {
                    ret = nap.refCount;
                }
            }
            return ret;
        }
    }

    //==============================================================
    //  AnimationProject
    //==============================================================

    [Serializable]
    public class AnimationProject
    {
        public long mkey;
        public int version;
        public List<AnimationAvatar> casts;
        public AnimationMotionTimeline timeline;
        public int timelineFrameLength;
        public int fps;
        public float baseDuration;
        public AnimationProjectMetaInformation meta;
        public AnimationProjectFixedProperties fixedProp;
        public AnimationProjectMaterialPackage materialManager;
        public bool isSharing;
        public bool isReadOnly;
        public bool isNew;
        public bool isOpenAndEdit;

        public AnimationProject(int frameCount)
        {
            mkey = 0;
            version = 1;
            casts = new List<AnimationAvatar>();
            timeline = new AnimationMotionTimeline();
            timelineFrameLength = frameCount;
            fps = 60;
            baseDuration = (float)fps / 6000f;
            meta = new AnimationProjectMetaInformation();
            fixedProp = new AnimationProjectFixedProperties();
            materialManager = new AnimationProjectMaterialPackage();
            isSharing = false;
            isReadOnly = false;
            isNew = true;
            isOpenAndEdit = false;

        }

        public AnimationProject SCopy()
        {
            return (AnimationProject)MemberwiseClone();
        }
    }
    public class NativeAnimationProject : AnimationProject
    {
        public new List<NativeAnimationAvatar> casts;

        public new NativeAnimationMotionTimeline timeline;

        public new NativeAnimationProjectMaterialPackage materialManager;

        public NativeAnimationProject(int frameCount) : base(frameCount)
        {
            
            casts = new List<NativeAnimationAvatar>();
            timeline = new NativeAnimationMotionTimeline();

            materialManager = new NativeAnimationProjectMaterialPackage();
        }
        public new NativeAnimationProject SCopy()
        {
            return (NativeAnimationProject)MemberwiseClone();
        }
    }

    [Serializable]
    public class OpennigAnimationProject
    {
        public List<UserVRMSpace.BasicObjectInformation> castInfo;
        public AnimationProject project;
        public OpennigAnimationProject()
        {
            castInfo = new List<UserVRMSpace.BasicObjectInformation>();
            project = null;
        }
    }
    public class OpeningNativeAnimationAvatar
    {
        public NativeAnimationAvatar cast;
        public UserVRMSpace.BasicObjectInformation baseInfo;
        public OpeningNativeAnimationAvatar ()
        {
            cast = null;
            baseInfo = null;
        }
    }
    //==============================================================
    //  Animation other option class
    //==============================================================

    [Serializable]
    public class AnimationRegisterOptions
    {
        public int index = -1;
        public float duration = 0.1f;
        public string targetId = "";
        public string targetRole = "";
        public AF_TARGETTYPE targetType = AF_TARGETTYPE.Unknown;
        public int isTransformOnly = 0;
        public int isHandOnly = 0;
        public int isBlendShapeOnly = 0;
        public int isPropertyOnly = 0;
        public int isDefineOnly = 0;
        public int isAnimationSeekOnly = 0;
        public int isSystemEffectOnly = 0;

        /// <summary>
        /// To compile an animation as an external using.(futurely as library)
        /// </summary>
        public int isCompileForLibrary = 0;

        public Ease ease = Ease.Linear;
    }
    [Serializable]
    public class AnimationParsingOptions
    {
        public int index = -1;
        public int finalizeIndex = -1;
        public int endIndex = -1;
        public string targetId = "";
        public string targetRole = "";
        public AF_TARGETTYPE targetType = AF_TARGETTYPE.Unknown;

        /// <summary>
        /// To use DoTween for performing an animation(1 - use, 0 - not use(manual transform))
        /// </summary>
        public int isExecuteForDOTween = 0;
        public int isSystemEffectOnly = 0;
        public int isCameraPreviewing = 0;

        /// <summary>
        /// To build an animation as perform of DOTween.
        /// </summary>
        public int isBuildDoTween = 0;

        /// <summary>
        /// To compile an animation as an external using.(futurely as library)
        /// </summary>
        public int isCompileAnimation = 0;

        /// <summary>
        /// To rebuild forcely
        /// </summary>
        public int isRebuildAnimation = 0;

        /// <summary>
        /// To show IK-marker of an objects
        /// </summary>
        public int isShowIK = 0;

        public int isLoop = 0;

        public float endDelay = 0f;

        public Ease ease = Ease.Linear;
    }

    public class FrameClipboard
    {
        public string targetRoleName = "";
        public AF_TARGETTYPE targetType;
        public int keyFrame = -1;
        public bool isCut = false;
    }

    public class PreviewFrameReturner
    {
        public Sequence seq = null;
        public int index = 0;
        public float currentDuration = 0f;
        public float backupDuration = 0f;
    }
}