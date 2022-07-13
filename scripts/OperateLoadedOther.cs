using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using DG.Tweening;

namespace UserHandleSpace
{
    /// <summary>
    /// Attach for Other object(FBX, OBJ, etc...)
    /// </summary>
    public class OperateLoadedOther : OperateLoadedBase
    {

        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);
        [DllImport("__Internal")]
        private static extern void SendPreviewingOtherObjectAnimationEnd(string val);


        ManageAnimation manim;

        public int childCount;

        public float animationRemainTime;
        private float local_animRemainTime;

        public float SeekPosition
        {
            set
            {
                Animation anim = null;
                if (transform.TryGetComponent<Animation>(out anim))
                {
                    if (value > -1f)
                    {
                        anim.clip.SampleAnimation(transform.gameObject, value);
                        local_animRemainTime = value;
                    }

                }
            }
            get
            {
                return local_animRemainTime;
            }
        }
        public UserAnimationState animationStartFlag;  //1 - play, 0 - stop, 2 - playing, 3 - seeking, 4 - pause
        protected Animation currentAnim;
        protected bool triggerCurrentAnimOnFinished;

        //public string renderTextureParentId;
        private Texture backupShareTexture;

        private string OldShaderName;

        


        /**
         *  This class is manager function, all Fbx, Obj Object has.
         *  Variables only...
         */
        // Start is called before the first frame update
        void Awake()
        {
            effectPunch = new AvatarPunchEffect();
            effectShake = new AvatarShakeEffect();


            childCount = 0;
            animationRemainTime = 0f;
            local_animRemainTime = 0f;
            //userSharedMaterials = new Dictionary<string, Material>();
            //userSharedTextureFiles = new Dictionary<string, MaterialProperties>();
            //backupTextureFiles = new Dictionary<string, MaterialProperties>();
            targetType = AF_TARGETTYPE.OtherObject;
        }
        private void Start()
        {
            manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            currentAnim = gameObject.GetComponent<Animation>();
            triggerCurrentAnimOnFinished = false;

            if (currentAnim != null)
            {
                foreach (AnimationState state in currentAnim)
                {
                    AnimationEvent aev = new AnimationEvent();
                    aev.time = state.clip.length;
                    aev.floatParameter = state.clip.length;
                    aev.functionName = "OnAnimationFinished";
                    state.clip.AddEvent(aev);
                }
            }
            
        }
        // Update is called once per frame
        void Update()
        {
            
        }
        private void OnDestroy()
        {

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
        }

        /// <summary>
        /// Fire on finish an animation of the object, callback to HTML
        /// </summary>
        /// <param name="param"></param>
        private void OnAnimationFinished(float param)
        {
            if ((currentAnim.wrapMode == WrapMode.Loop) || (currentAnim.wrapMode == WrapMode.PingPong) || (currentAnim.wrapMode == WrapMode.ClampForever)) return;
            if (manim.IsPlaying) return;

            Debug.Log(param);
            string ret = currentAnim.clip.name + "," + param.ToString();
#if !UNITY_EDITOR && UNITY_WEBGL
            SendPreviewingOtherObjectAnimationEnd(ret);
#endif
        }


        /// <summary>
        /// Get Effective Object with Renderer (use Image object only)
        /// </summary>
        /// <returns></returns>
        public GameObject GetEffectiveObject()
        {
            MeshRenderer mr;
            SkinnedMeshRenderer smr;
            GameObject ret = null;

            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (
                    (child.TryGetComponent<MeshRenderer>(out mr))
                    ||
                    (child.TryGetComponent<SkinnedMeshRenderer>(out smr))
                )
                {
                    //first object with **Renderer
                    ret = child;
                    break;
                }
            }
            return ret;
        }
        public override Vector3 GetScale()
        {
            Vector3 ret = Vector3.zero;

            ret = transform.localScale;

            return ret;
        }
        public override void GetScaleFromOuter()
        {
            Vector3 ret = Vector3.zero;

            ret = transform.localScale;

            string jstr = JsonUtility.ToJson(ret);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(jstr);
#endif
        }
        public override void SetScale(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;

            //--- Other Object is single and children-has.

            transform.localScale = new Vector3(x, y, z);

            
            //relatedHandleParent.transform.DOScale(new Vector3(x, y, z), 0.1f);

        }
        public override void SetEnableWholeIK(int intflag)
        {
            bool flag = intflag == 1 ? true : false;

            relatedHandleParent.SetActive(flag);

        }
        

        /// <summary>
        /// To pack all properties for HTML-UI
        /// </summary>
        /// <param name="type">1 - OtherObject, 0 - image</param>
        public void GetIndicatedPropertyFromOuter(int type)
        {
            string ret = "";
            List<string> list = ListUserMaterial();
            List<string> animclips = ListAnimationClips();
            string js = string.Join("\r\n", list.ToArray());
            string jsclip = string.Join('=', animclips);
            if (type == 1)
            {
                int pflag = IsPlayingAnimation();
                int playflag = (int)animationStartFlag;
                float seek = animationRemainTime;

                // 1st sep ... \t
                // material
                // child sep 1 ... \r\n
                //           2 ... =
                //           3 ... ,
                // animation clip ... =

                ret = "o"
                    + "\t" +
                    (type == 1 ? js : "")
                    + "\t" +
                    pflag.ToString()
                    + "\t" +
                    playflag.ToString()
                    + "\t" +
                    seek.ToString()
                    + "\t" +
                    GetSpeedAnimation().ToString()
                    + "\t" +
                    GetMaxPosAnimation().ToString()
                    + "\t" + 
                    GetWrapMode().ToString()
                    + "\t" + 
                    jsclip
                    + "\t" +
                    GetTargetClip()
                ;

                //Debug.Log("OtherObject indicated properties=");
                //Debug.Log(ret);
            }
            else
            {
                ret = "i"
                    + "\t" +
                    js
                    + "\t" +
                    ColorUtility.ToHtmlStringRGBA(GetBaseColor(0))
                ;
            }
            
            
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }

        //=======================================================================================================
        //  Below is use for Animation 
        //  neccesary is_contacthtml flag. (unity and html both call there.

        public string GetSystemAnimationType(int is_contacthtml = 1)
        {
            Animator animt;
            Animation anim;
            string ret = "";
            if (transform.TryGetComponent<Animator>(out animt))
            {
                ret = "generic";
            }else
            if (transform.TryGetComponent<Animation>(out anim))
            {
                ret = "legacy";
            }else
            {
                ret = "none";
            }

#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml == 1) {
            ReceiveStringVal(ret);
        }
#endif
            return ret;
        }
        //-----------------------------------------------------
        public int IsPlayingAnimation()
        {
            int ret = 0;
            Animator animt;
            Animation anim;
            if (transform.TryGetComponent<Animator>(out animt))
            {
                if (animt.layerCount > 0)
                {
                    //if (animt.enabled) return;
                    animt.SetBool("isplay1", true);
                    //animt.Play("Object_play");
                    //AnimatorStateInfo state = animt.GetCurrentAnimatorStateInfo(0);
                    //animt.Play(state.shortNameHash, 0, 0f);
                    AnimatorClipInfo[] ci = animt.GetCurrentAnimatorClipInfo(0);
                    if (ci.Length == 0)
                    {
                        ret = 0;
                    }
                    else
                    {
                        ret = 1;
                    }
                }
            }
            if (transform.TryGetComponent<Animation>(out anim))
            {
                ret = anim.isPlaying ? 1 : 0;
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
        //----------------------------------------------------------------------------
        public List<string> ListAnimationClips()
        {
            List<string> arr = new List<string>();
            Animator animt;
            Animation anim;
            if (transform.TryGetComponent<Animator>(out animt))
            {
                if (animt.layerCount > 0)
                {

                }
            }
            if (transform.TryGetComponent<Animation>(out anim))
            {
                foreach (AnimationState state in anim)
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
            Animator animt;
            Animation anim;
            if (transform.TryGetComponent<Animator>(out animt))
            {
                if (animt.layerCount > 0)
                {

                }
            }
            if (transform.TryGetComponent<Animation>(out anim))
            {
                if (anim.clip == null)
                {
                    anim.clip = anim.GetClip(param);
                }
                else 
                {
                    if (param != anim.clip.name)
                    {
                        anim.clip = anim.GetClip(param);
                    }
                }
                
                
            }
        }
        public string GetTargetClip()
        {
            string ret = "";
            Animator animt;
            Animation anim;
            if (transform.TryGetComponent<Animator>(out animt))
            {
                if (animt.layerCount > 0)
                {

                }
            }
            if (transform.TryGetComponent<Animation>(out anim))
            {
                if (anim.clip != null)
                {
                    ret = anim.clip.name;
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
        //------------------------------------------------
        public virtual void PlayAnimation()
        {
            Animator animt;
            Animation anim;
            if (transform.TryGetComponent<Animator>(out animt))
            {
                if (animt.layerCount > 0)
                {
                    //if (animt.enabled) return;
                    animt.SetBool("isplay1", true);
                    //animt.Play("Object_play");
                    //AnimatorStateInfo state = animt.GetCurrentAnimatorStateInfo(0);
                    //animt.Play(state.shortNameHash, 0, 0f);
                    return;
                }
            }
            if (transform.TryGetComponent<Animation>(out anim))
            {
                if (!anim.clip.isLooping)
                { //---manually finish flag is OFF
                    triggerCurrentAnimOnFinished = false;
                }
                foreach (AnimationState state in anim)
                {
                    if (anim.clip.name == state.clip.name)
                    {
                        state.time = animationRemainTime;
                    }
                    
                }
                
                anim.Rewind();
                anim.Play(anim.clip.name);
            }
        }
        public void PauseAnimation()
        {
            Animator animt;
            Animation anim;
            if (transform.TryGetComponent<Animator>(out animt))
            {
                if (animt.layerCount > 0)
                {
                    AnimatorClipInfo[] ci = animt.GetCurrentAnimatorClipInfo(0);
                    if (ci.Length > 0)
                    {
                        animt.SetBool("isplay1", false);
                    }
                    else
                    {
                        animt.SetBool("isplay1", true);
                    }
                    
                    return;
                }
            }
            if (transform.TryGetComponent<Animation>(out anim))
            {
                if (anim.isPlaying)
                {
                    foreach (AnimationState state in anim)
                    {
                        if (anim.clip.name == state.clip.name)
                        {
                            animationRemainTime = state.time;
                        }
                    }
                    anim.Stop();
                }
                else
                {
                    foreach (AnimationState state in anim)
                    {
                        if (anim.clip.name == state.clip.name)
                        {
                            state.time = animationRemainTime;
                        }
                    }
                    anim.Play();
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
        public virtual void StopAnimation()
        {
            Animator animt;
            Animation anim;
            if (transform.TryGetComponent<Animator>(out animt))
            {
                if (animt.layerCount > 0)
                {
                    animt.SetBool("isplay1", false);

                    //AnimatorStateInfo state = animt.GetCurrentAnimatorStateInfo(0);
                    //animt.speed = 0f;
                    //animt.enabled = false;
                    //AnimationState state1 = new AnimationState();
                    return;
                }
            }
            if (transform.TryGetComponent<Animation>(out anim))
            {
                /*foreach (AnimationState state in anim)
                {
                    animationRemainTime = state.normalizedTime;
                }*/
                anim.Stop();
                animationRemainTime = 0f;
            }
        }
        
        //-----------------------------------------------------
        public void SetSpeedAnimation(float to)
        {
            Animator animt;
            Animation anim;
            if (transform.TryGetComponent<Animator>(out animt))
            {
                if (animt.layerCount > 0)
                {
                    animt.speed = to;

                }
            }
            if (transform.TryGetComponent<Animation>(out anim))
            {
                if (anim.clip != null)
                {
                    foreach (AnimationState state in anim)
                    {
                        if (anim.clip.name == state.clip.name)
                        {
                            state.speed = to;
                        }
                    }
                }
                

            }
        }
        public float GetSpeedAnimation()
        {
            float ret = 0f;
            Animator animt;
            Animation anim;
            if (transform.TryGetComponent<Animator>(out animt))
            {
                if (animt.layerCount > 0)
                {
                    ret = animt.speed;

                }
            }
            if (transform.TryGetComponent<Animation>(out anim))
            {
                if (anim.clip != null)
                {
                    foreach (AnimationState state in anim)
                    {
                        if (anim.clip.name == state.clip.name)
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
            Animator animt;
            Animation anim;
            if (transform.TryGetComponent<Animator>(out animt))
            {
                if (animt.layerCount > 0)
                {


                }
            }
            if (transform.TryGetComponent<Animation>(out anim))
            {
                if (value > -1f)
                {
                    animationRemainTime = value;
                    if (anim.clip != null) anim.clip.SampleAnimation(transform.gameObject, animationRemainTime);
                }

            }
        }
        //-----------------------------------------------------
        public float GetMaxPosAnimation()
        {
            float ret = 0f;
            Animator animt;
            Animation anim;
            if (transform.TryGetComponent<Animator>(out animt))
            {
                if (animt.layerCount > 0)
                {


                }
            }
            if (transform.TryGetComponent<Animation>(out anim))
            {
                if (anim.clip != null)
                {
                    ret = anim.clip.length;
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
            Animator animt;
            Animation anim;
            if (transform.TryGetComponent<Animator>(out animt))
            {
                if (animt.layerCount > 0)
                {


                }
            }
            if (transform.TryGetComponent<Animation>(out anim))
            {
                anim.wrapMode = (WrapMode)flag;
            }
        }
        public int GetWrapMode()
        {
            int ret = 0;
            Animator animt;
            Animation anim;
            if (transform.TryGetComponent<Animator>(out animt))
            {
                if (animt.layerCount > 0)
                {


                }
            }
            if (transform.TryGetComponent<Animation>(out anim))
            {
                ret = (int)anim.wrapMode;
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
        //------------------------------------------------------------
        public void SetTransparency(float value)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                MeshRenderer mr;
                SkinnedMeshRenderer smr;
                if (
                        (child.TryGetComponent<MeshRenderer>(out mr))
                        ||
                        (child.TryGetComponent<SkinnedMeshRenderer>(out smr))
                        )
                {
                    Material[] mat = child.GetComponent<Renderer>().sharedMaterials;

                    for (int m = 0; m < mat.Length; m++)
                    {
                        Material mt = mat[m];

                        mt.color = new Color(mt.color.r, mt.color.g, mt.color.b, value);
                    }
                }
            }

        }

        /// <summary>
        /// Get transparency of Object (common for all material)
        /// </summary>
        /// <returns></returns>
        public float GetTransparency()
        {
            float ret = 0;
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                MeshRenderer mr;
                SkinnedMeshRenderer smr;
                bool ishit = false;
                if (
                        (child.TryGetComponent<MeshRenderer>(out mr))
                        ||
                        (child.TryGetComponent<SkinnedMeshRenderer>(out smr))
                        )
                {
                    Material[] mat = child.GetComponent<Renderer>().sharedMaterials;

                    for (int m = 0; m < mat.Length; m++)
                    {
                        Material mt = mat[m];

                        ret = mt.color.a;
                        ishit = true;
                        break;
                    }
                }
                if (ishit) break;
            }
            return ret;
        }
        //-----------------------------------------------------------------
        public void SetBaseColorFromOuter(string param)
        {
            Color col = ColorUtility.TryParseHtmlString(param, out col) ? col : Color.white;

            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                MeshRenderer mr;
                SkinnedMeshRenderer smr;
                if (
                        (child.TryGetComponent<MeshRenderer>(out mr))
                        ||
                        (child.TryGetComponent<SkinnedMeshRenderer>(out smr))
                        )
                {
                    Material[] mat = child.GetComponent<Renderer>().sharedMaterials;

                    for (int m = 0; m < mat.Length; m++)
                    {
                        Material mt = mat[m];
                        mt.DOColor(col, 0.1f);
                    }
                }
            }
        }
        public void SetBaseColor(Color color)
        {
            Color col = color;

            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                MeshRenderer mr;
                SkinnedMeshRenderer smr;
                if (
                        (child.TryGetComponent<MeshRenderer>(out mr))
                        ||
                        (child.TryGetComponent<SkinnedMeshRenderer>(out smr))
                        )
                {
                    Material[] mat = child.GetComponent<Renderer>().sharedMaterials;

                    for (int m = 0; m < mat.Length; m++)
                    {
                        Material mt = mat[m];
                        mt.DOColor(col, 0.01f);
                    }
                }
            }
        }
        public Sequence SetBaseColorForAnimation(Sequence seq, float duration, Color param)
        {
            Color col = param;

            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                MeshRenderer mr;
                SkinnedMeshRenderer smr;
                if (
                        (child.TryGetComponent<MeshRenderer>(out mr))
                        ||
                        (child.TryGetComponent<SkinnedMeshRenderer>(out smr))
                        )
                {
                    Material[] mat = child.GetComponent<Renderer>().sharedMaterials;

                    for (int m = 0; m < mat.Length; m++)
                    {
                        Material mt = mat[m];
                        seq.Join(mt.DOColor(col, duration));
                    }
                }
            }
            return seq;
        }
        public Color GetBaseColor(int is_contacthtml = 1)
        {
            Color ret = Color.white;
            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                MeshRenderer mr;
                SkinnedMeshRenderer smr;
                bool ishit = false;
                if (
                        (child.TryGetComponent<MeshRenderer>(out mr))
                        ||
                        (child.TryGetComponent<SkinnedMeshRenderer>(out smr))
                        )
                {
                    Material[] mat = child.GetComponent<Renderer>().sharedMaterials;

                    for (int m = 0; m < mat.Length; m++)
                    {
                        Material mt = mat[m];

                        ret = mt.color;
                        ishit = true;
                        break;
                    }
                }
                if (ishit) break;
            }

#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml == 1) {
            ReceiveStringVal(ColorUtility.ToHtmlStringRGBA(ret));
        }
#endif
            return ret;
        }


        //----Equip function=====================================================
        public String CheckWhoEquip()
        {
            string ret = "null";
            OtherObjectDummyIK ooik = relatedHandleParent.GetComponent<OtherObjectDummyIK>();
            if (ooik.equippedAvatar != null)
            {
                ret = ooik.equippedAvatar.name;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif

            return ret;
        }

        //---IK set function=======================================================
        public bool CheckIKTargetToAvatar(NativeAnimationAvatar nav)
        {
            bool ret = false;

            List<AvatarIKMappingClass> aiklist = nav.avatar.GetComponent<OperateLoadedVRM>().ikMappingList;
            NativeAnimationAvatar owncast = manim.GetCastByAvatar(gameObject.name);
            if (owncast != null)
            {
                List<AvatarIKMappingClass> ishit = aiklist.FindAll(item =>
                {
                    if (item.name == owncast.roleName) return true;
                    return false;
                });
                if (ishit.Count > 0)
                {
                    ret = true;
                }
            }

            return ret;
        }
    }

}
