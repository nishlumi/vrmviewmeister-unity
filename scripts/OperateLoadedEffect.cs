using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using DG.Tweening;
using System.Linq;
using UniVRM10;

namespace UserHandleSpace
{
    [Serializable]
    public class EffectCurrentStates
    {
        public string genre = "";
        public string effectName = "";
        public List<string> effectList = new List<string>();
    }
    public class OperateLoadedEffect : OperateLoadedBase
    {

        [DllImport("__Internal")]
        private static extern void ReceiveObjectVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);
        [DllImport("__Internal")]
        private static extern void SendPreviewingEffectEnd(string val);

        private ManageAnimation manim;



        public GameObject targetEffect;
        [SerializeField] private AssetReferenceT<GameObject> targetEffectRef;
        
        private string[] EffectNames;

        [SerializeField]
        private bool isVRMCollider;
        private float vrmColliderSize;
        public GameObject previewColliderSphere;
        private List<NativeAnimationAvatar> targetColliderCasts;


        private bool isPreview;

        /*
         * 1 - play
         * 2 - play with loop
         * 3 - pause
         * 0, other - stop
         */
        public UserAnimationState animationStartFlag;

        override protected void Awake()
        {
            base.Awake();


            targetType = AF_TARGETTYPE.Effect;
            EffectNames = new string[2];
            EffectNames[0] = "";
            EffectNames[1] = "";
            isPreview = false;
        }
        // Start is called before the first frame update
        override protected void Start()
        {
            base.Start();

            manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

            isVRMCollider = false;
            vrmColliderSize = 0.1f;
            targetColliderCasts = new List<NativeAnimationAvatar>();
        }

        // Update is called once per frame
        void Update()
        {
            if (targetEffect != null)
            {
                //---Re-positionning for Effect Object(set this IK object's poisition)
                targetEffect.transform.position = transform.position;
                targetEffect.transform.rotation = transform.rotation;

                
            }
            
            oldPosition = transform.position;
            oldRotation = transform.rotation;
        }
        private void LateUpdate()
        {
            if (IsVRMCollider)
            {
                if (transform.position != oldPosition)
                {
                    targetColliderCasts.ForEach(cast =>
                    {
                        Vrm10Instance vinst = cast.avatar.GetComponent<Vrm10Instance>();
                        if (vinst != null)
                        {
                            vinst.Runtime.ReconstructSpringBone();
                        }
                    });
                }
            }
        }
        override protected void OnDestroy()
        {
            base.OnDestroy();

            RelaseEffectRef();
        }
        /// <summary>
        /// To pack all properties for HTML-UI
        /// </summary>
        /// <param name="type">1 - normal light, 0 - directional light</param>
        public void GetIndicatedPropertyFromOuter(int type)
        {
            string ret = "";
            Camera lt = transform.gameObject.GetComponent<Camera>();

            int pflag = (int)animationStartFlag;
            string js = GetCurrentEffectFromOuter(0);

            List<string> targetvrms = EnumColliderTarget();
            int colliderTargetSize = targetvrms.Count;
            string vrmstext = string.Join(',', targetvrms);

            ret = pflag.ToString() + "\t" + js + "\t" + (isVRMCollider == true ? "1" : "0") + "\t" + vrmColliderSize.ToString() + "\t" + vrmstext
            ;

            //Debug.Log("GetIndicatedPropertyFromOuter= " + ret);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }

        //----------------------------------------------------------------------------------------------------------------
        public override void SetEnableWholeIK(int intflag)
        {
            bool flag = intflag == 1 ? true : false;

            relatedHandleParent.SetActive(flag);

        }

        public string ListEffectGenre()
        {
            string ret = "";
            List<string> lst = new List<string>();
            GameObject[] efs = GameObject.FindGameObjectsWithTag("EffectSystem");
            for (int i = 0; i < efs.Length; i++)
            {
                lst.Add(efs[i].name);
            }
            ret = string.Join(",", lst);

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif

            return ret;
        }
        /// <summary>
        /// Get all effect object in specified genre.
        /// </summary>
        /// <param name="genre"></param>
        /// <returns></returns>
        public List<GameObject> ListEffects(string genre)
        {
            List<GameObject> ret = new List<GameObject>();
            GameObject[] efs = GameObject.FindGameObjectsWithTag("EffectSystem");
            for (int i = 0; i < efs.Length; i++)
            {
                if (genre.ToLower() == efs[i].name.ToLower())
                {
                    //List<GameObject> gos = new List<GameObject>();
                    for (int c = 0; c < efs[i].transform.childCount; c++)
                    {
                        GameObject cld = efs[i].transform.GetChild(c).gameObject;
                        ret.Add(cld);
                    }
                    //ret = gos.ToArray();
                    break;
                }
            }
            return ret;
        }
        public string ListEffectsFromOuter(string genre)
        {
            List<GameObject> efs = ListEffects(genre);

            string ret = "";
            List<string> arr = new List<string>();

            for (int i = 0; i < efs.Count; i++)
            {
                arr.Add(efs[i].name);
            }
            ret = string.Join(",", arr);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">0 - genre, 1 - effect name</param>
        /// <returns></returns>
        public GameObject FindEffect(string param)
        {
            GameObject ret = null;
            string[] prm = param.Split(',');

            List<GameObject> effects = ListEffects(prm[0]);
            if (effects != null)
            {
                for (int i = 0; i < effects.Count; i++)
                {
                    if (prm[1].ToLower() == effects[i].name.ToLower())
                    {
                        ret = effects[i];
                        EffectNames[0] = prm[0];
                        EffectNames[1] = prm[1];
                        break;
                    }
                }
            }

            

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">0 - genre, 1 - effect name</param>
        /// <returns></returns>
        public void SetEffect(string param)
        {
            GameObject eff = FindEffect(param);

            if (eff != null)
            {
                targetEffect = eff;

            }
        }
        private void SetEffectRefHandler(AsyncOperationHandle handle, Exception e)
        {

        }

        public async System.Threading.Tasks.Task<GameObject> SetEffectRef(string param)
        {
            string[] prm = param.Split("/");
            if ((EffectNames[0] == prm[1]) && (EffectNames[1] == prm[2])) return null;
            if ((prm[0] == "") || (prm[1] == "")) return null;

            EffectNames[0] = prm[1];
            EffectNames[1] = prm[2];

            RelaseEffectRef();

            //---addressable
            AsyncOperationHandle<GameObject> targetEffectHandle = Addressables.InstantiateAsync(param);


            //targetEffectHandle.Completed += instantiate_Completed;

            System.Threading.Tasks.Task<GameObject> eff = targetEffectHandle.Task;
            targetEffect = await eff;
            /*
            if (eff != null)
            {
                targetEffect = eff;
            }*/
            return targetEffect;
            


            
        }
        public void RelaseEffectRef()
        {
            if (targetEffect != null) Addressables.ReleaseInstance(targetEffect);
        }
        public EffectCurrentStates GetCurrentEffect()
        {
            EffectCurrentStates ecs = new EffectCurrentStates();
            ecs.genre = EffectNames[0];
            ecs.effectName = EffectNames[1];
            return ecs;
        }
        public string GetCurrentEffectFromOuter(int is_contacthtml = 1)
        {
            string ret = "";
            EffectCurrentStates ecs = new EffectCurrentStates();
            ecs.genre = EffectNames[0];
            ecs.effectName = EffectNames[1];
            ecs.effectList = new List<string>();

            List<GameObject> efs = ListEffects(ecs.genre);

            //Debug.Log("efs.Length=" + efs.Count.ToString());
            if (efs != null)
            {
                for (int i = 0; i < efs.Count; i++)
                {
                    ecs.effectList.Add(efs[i].name);
                }
            }


            ret = JsonUtility.ToJson(ecs);
            //Debug.Log("ret = JsonUtility.ToJson(ecs);");
            //Debug.Log(ret);
#if !UNITY_EDITOR && UNITY_WEBGL
            if (is_contacthtml == 1) ReceiveStringVal(ret);
#endif

            return ret;
        }
        public void ResetEffect()
        {
            targetEffect = null;
        }
        public void PreviewEffect(int isLoop = -1)
        {
            if (targetEffect != null)
            {
                ParticleSystem ps = targetEffect.GetComponent<ParticleSystem>();
                ownscr_effectcallback1 efcall = targetEffect.GetComponent<ownscr_effectcallback1>();
                efcall.isPreview = true;
                efcall.genre = EffectNames[0];
                efcall.effectName = EffectNames[1];
                efcall.sourceID = gameObject.name;

                ParticleSystem.MainModule main = ps.main;
                main.stopAction = ParticleSystemStopAction.Callback;
                //---if isLoop is -1, no change.(default setting)
                if (isLoop != -1) main.loop = isLoop == 1 ? true : false;

                AudioSource audio = null;
                if (targetEffect.TryGetComponent<AudioSource>(out audio))
                {
                    audio.PlayOneShot(audio.clip);
                }


                ps.Play();
            }
        }
        public void PlayEffect(int isLoop = -1)
        {
            if (targetEffect != null)
            {
                ParticleSystem ps = targetEffect.GetComponent<ParticleSystem>();
                ownscr_effectcallback1 efcall = targetEffect.GetComponent<ownscr_effectcallback1>();
                efcall.isPreview = false;
                efcall.genre = EffectNames[0];
                efcall.effectName = EffectNames[1];
                efcall.sourceID = gameObject.name;

                ParticleSystem.MainModule main = ps.main;
                //---if isLoop is -1, no change.(default setting)
                if (isLoop != -1) main.loop = isLoop == 1 ? true : false;

                AudioSource audio = null;
                if (targetEffect.TryGetComponent<AudioSource>(out audio))
                {
                    audio.PlayOneShot(audio.clip);
                }

                ps.Play();
            }
        }
        public void StopEffect()
        {
            if (targetEffect != null)
            {
                ParticleSystem ps = targetEffect.GetComponent<ParticleSystem>();
                ps.Stop();
                ownscr_effectcallback1 efcall = targetEffect.GetComponent<ownscr_effectcallback1>();
                efcall.ClearSetting();
            }
        }
        public void PauseEffect()
        {
            if (targetEffect != null)
            {
                ParticleSystem ps = targetEffect.GetComponent<ParticleSystem>();
                ps.Pause();
            }
        }
        public int IsPlayingEffect(int is_contacthtml = 1)
        {
            int ret = 0;
            if (targetEffect != null)
            {
                ParticleSystem ps = targetEffect.GetComponent<ParticleSystem>();
                ret = ps.isPlaying ? 1 : 0;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml == 1) ReceiveIntVal(ret);
#endif
            return ret;
        }
        public int IsPausedEffect(int is_contacthtml = 1)
        {
            int ret = 0;
            if (targetEffect != null)
            {
                ParticleSystem ps = targetEffect.GetComponent<ParticleSystem>();
                ret = ps.isPaused ? 1 : 0;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml == 1) ReceiveIntVal(ret);
#endif
            return ret;
        }


        /// <summary>
        /// set play flag for effect
        /// </summary>
        /// <param name="flag">UserAnimationState</param>
        public void SetPlayFlagEffect(UserAnimationState flag)
        {
            animationStartFlag = flag;
        }
        public void SetPlayFlagEffectFromOuter(int flag)
        {
            animationStartFlag = (UserAnimationState)flag;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="is_contacthtml"></param>
        /// <returns>UserAnimationState</returns>
        public UserAnimationState GetPlayFlagEffectFromOuter(int is_contacthtml = 1)
        {
#if !UNITY_EDITOR && UNITY_WEBGL
        if (is_contacthtml == 1) {
            ReceiveIntVal((int)animationStartFlag);
        }
#endif
            return animationStartFlag;
        }
        public UserAnimationState GetPlayFlagEffect()
        {
            return animationStartFlag;
        }

        //======================================================================
        // VRMSpringBone operator
        public bool IsVRMCollider {
            set
            {
                isVRMCollider = value;
            }
            get
            {
                return isVRMCollider;
            }
        }
        public void SetIsVRMColliderFromOuter(int flag)
        {
            isVRMCollider = flag == 1 ? true : false;
            GenerateVRMCollider(isVRMCollider);
        }
        public void GetIsVRMColliderFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(isVRMCollider ? 1 : 0);
#endif
        }
        //-------------------------------------------------------
        public float VRMColliderSize
        {
            set
            {
                vrmColliderSize = value;
                GenerateVRMCollider(isVRMCollider);
            }
            get
            {
                return vrmColliderSize;
            }
        }
        public void SetVRMColliderSize(float val)
        {
            vrmColliderSize = val;
            GenerateVRMCollider(isVRMCollider);
        }
        public void GetVRMWindPowerFromOuter ()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(vrmColliderSize);
#endif
        }
        //----------------------------------------------------
        public void AddColliderTarget(string param)
        {
            NativeAnimationAvatar cast = manim.GetCastInProject(param);
            if (cast != null)
            {
                if (cast.type == AF_TARGETTYPE.VRM)
                {
                    targetColliderCasts.Add(cast);
                    ApplyAllVRMSpringBone(cast);
                }
            }
        }
        public void DelColliderTarget(string param)
        {
            NativeAnimationAvatar cast = targetColliderCasts.Find(match =>
            {
                if (match.roleName == param) return true;
                return false;
            });
            if (cast != null)
            {
                ApplyAllVRMSpringBone(cast, true);
                targetColliderCasts.Remove(cast);
            }
        }
        public void DelColliderTarget(NativeAnimationAvatar cast)
        {
            ApplyAllVRMSpringBone(cast, true);
            targetColliderCasts.Remove(cast);
        }
        public void ResetColliderTarget(List<string> rolelist)
        {
            for (var i = targetColliderCasts.Count-1; i >= 0; i--)
            {
                NativeAnimationAvatar cast = targetColliderCasts[i];

                DelColliderTarget(cast);
            }

            //--renewly add
            rolelist.ForEach(item =>
            {
                AddColliderTarget(item);
            });
        }
        public List<string> EnumColliderTarget()
        {
            List<string> ret = new List<string>();
            foreach (NativeAnimationAvatar cast in  targetColliderCasts)
            {
                ret.Add(cast.roleName);
            }
            return ret;
        }
        public void EnumColliderTargetFromOuter()
        {
            List<string> ret = EnumColliderTarget();

            string js = string.Join(",", ret);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }

        /// <summary>
        /// generate, enable/disable VRMSpringBoneColliderGroup to this effect object
        /// </summary>
        /// <param name="isenable"></param>
        private void GenerateVRMCollider(bool isenable)
        {
            /**
             *  Effect: 
             *  Effect: VRM10SpringBoneCollidarGroup
             *    |- Sphere: VRM10SpringBoneCollidar
             *  exists at first.
             *  neccesary to do: add VRM10SpringBoneCollidarGroup VRM10Instance.SpringBone
             */

            if (isenable)
            {
                VRM10SpringBoneColliderGroup colgrp = null;
                if (!gameObject.TryGetComponent(out colgrp))
                {
                    colgrp = gameObject.AddComponent<VRM10SpringBoneColliderGroup>();
                }
                colgrp.enabled = true;

                if (colgrp.Colliders.Count > 0)
                {
                    colgrp.Colliders[0].enabled = true;
                    colgrp.Colliders[0].Radius = vrmColliderSize;
                }

                //---set collider size (Radius * 2)
                float fsize = vrmColliderSize * 2f;
                previewColliderSphere.SetActive(true);
                previewColliderSphere.transform.localScale = new Vector3(fsize, fsize, fsize);
            }
            else
            {
                VRM10SpringBoneCollider collider = null;
                if (gameObject.TryGetComponent<VRM10SpringBoneCollider>(out collider))
                {
                    collider.enabled = false;
                    previewColliderSphere.transform.localScale = Vector3.zero;
                    previewColliderSphere.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Apply adding and deleting VRMSpringBoneColliderGroup to target cast's VRMSpringBone
        /// </summary>
        /// <param name="cast"></param>
        /// <param name="isDelete"></param>
        private void ApplyAllVRMSpringBone(NativeAnimationAvatar cast, bool isDelete = false)
        {
            if (cast.type == AF_TARGETTYPE.VRM)
            {
                Vrm10Instance vinst = cast.avatar.GetComponent<Vrm10Instance>();
                if (vinst == null) return;

                //---location: this effect object
                VRM10SpringBoneColliderGroup colgrp = null;
                if (!gameObject.TryGetComponent(out colgrp))
                {
                    colgrp = gameObject.AddComponent<VRM10SpringBoneColliderGroup>();
                }
                
                /*VRMSpringBoneColliderGroup collider = null;
                if (!gameObject.TryGetComponent<VRMSpringBoneColliderGroup>(out collider))
                {
                    collider = gameObject.AddComponent<VRMSpringBoneColliderGroup>();

                }*/
                //---add/delete collider to the spring bone
                /*
                 * TODO: 追加方法
                 * VRM1.0ではSpringBoneやCollidarの構造が異なるので下記は動かない。
                 * 要再検討。
                 * 
                 */

                if (colgrp != null)
                {
                    Vrm10InstanceSpringBone SpringBone = vinst.SpringBone; // cast.avatar.GetComponent<Vrm10Instance>().SpringBone;
                    if (isDelete)
                    {
                        int delIndex = SpringBone.ColliderGroups.FindIndex(match =>
                        {
                            if (match.gameObject.name == colgrp.gameObject.name) return true;
                            return false;
                        });
                        if (delIndex != -1)
                        {
                            SpringBone.ColliderGroups.RemoveAt(delIndex);
                        }
                    }
                    else
                    {
                        SpringBone.ColliderGroups.Add(colgrp);
                    }
                    //---loop of VRM's SpringBones
                    SpringBone.Springs.ForEach(spring =>
                    {
                        if (isDelete)
                        { //---Remove from SpringBone
                            int delIndex = -1;
                            //---loop of CollidarGroups in SpringBone in VRM's SpringBones
                            delIndex = spring.ColliderGroups.FindIndex(match =>
                            {
                                if (match.gameObject.name == colgrp.gameObject.name) return true;
                                return false;
                            });
                            if (delIndex > -1)
                            {
                                spring.ColliderGroups.RemoveAt(delIndex);
                                //colgrp.Colliders.RemoveAt(delIndex);
                            }
                        }
                        else
                        { //---Add collidar to SpringBone
                            spring.ColliderGroups.Add(colgrp);
                        }
                    });
                    /*
                    VRMSpringBone[] vsbones = cast.avatar.transform.GetComponentsInChildren<VRMSpringBone>();
                    foreach (VRMSpringBone bone in vsbones)
                    {
                        if (bone.ColliderGroups == null)
                        {
                            bone.ColliderGroups = new VRMSpringBoneColliderGroup[] { };
                        }
                        List<VRMSpringBoneColliderGroup> lst = bone.ColliderGroups.ToList<VRMSpringBoneColliderGroup>();
                        int index = lst.FindIndex(match =>
                        {
                            if (match.transform.gameObject.name == collider.transform.gameObject.name) return true;
                            return false;
                        });
                        if (index == -1)
                        {
                            if (isDelete) {
                                VRMSpringBoneColliderGroup deltarget = lst.Find(match => {
                                    if (match.gameObject.name == collider.gameObject.name) return true;
                                    return false;
                                });
                                if (deltarget != null) lst.Remove(deltarget);
                            }else {
                                lst.Add(collider);
                            }
                            bone.ColliderGroups = lst.ToArray();
                        }
                    }*/
                }
                
            }
        }
    }

}
