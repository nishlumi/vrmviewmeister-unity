using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using DG.Tweening;


namespace UserHandleSpace
{

    /// <summary>
    /// Base class to attach for All Objects
    /// </summary>
    public class OperateLoadedBase : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);

        protected const string CAMERAROLE = "#Camera";

        public const string SHAD_STD = "standard";
        public const string SHAD_VRM = "vrm/mtoon";
        public const string SHAD_VRM10 = "vrm10/mtoon10";
        public const string SHAD_WT = "fx/water4";
        public const string SHAD_SWT = "fx/simplewater4";
        public const string SHAD_SKE = "pencilshader/sketchshader";
        public const string SHAD_PSKE = "pencilshader/postsketchshader";
        public const string SHAD_REALTOON = "realtoon/version 5/lite/default";
        public const string SHAD_COMIC = "custom/comicshader";
        public const string SHAD_ICE = "custom/iceshader";
        public const string SHAD_MICRA = "custom/pixelizetexture";


        public string Title;
        public GameObject relatedHandleParent;
        public string objectType;
        protected bool isFixMoving;
        public AF_TARGETTYPE targetType;
        protected int jumpNum;
        protected float jumpPower;
        protected AvatarPunchEffect effectPunch;
        protected AvatarShakeEffect effectShake;

        protected Vector3 oldPosition;
        protected Quaternion oldRotation;
        protected Vector3 defaultPosition;
        protected Quaternion defaultRotation;
        protected Vector3 defaultColliderPosition;

        protected Vector3 oldikposition;

        //protected List<NativeAnimationAvatar> copyToList;

        //---render texture----------------------------------
        protected string renderTextureParentId;


        /*
         * Specification: userShared -----------------------
         * GameObject
         *   -> Parts object1
         *        Parts material1
         *        Parts material2
         *      Parts object2
         *        Parts material1
         *        
         * key - 0 = object.child's name 
         *       1 = material name
         * 
         */
        //---Pointer of the each materials

        /// <summary>
        /// SkinnedMeshRenderer objectname : available Material(s)
        /// </summary>
        [SerializeField]
        public Dictionary<string, Material> userSharedMaterials;
        //---property saver of material
        [SerializeField]
        protected Dictionary<string, MaterialProperties> userSharedTextureFiles;
        [SerializeField]
        protected Dictionary<string, MaterialProperties> backupTextureFiles;

        protected Dictionary<string, Shader> cachedShaders;

        public List<MaterialProperties> backup_userMaterialProperties;

        virtual protected void Awake()
        {
            effectPunch = new AvatarPunchEffect();
            effectShake = new AvatarShakeEffect();
            jumpNum = 0;
            jumpPower = 1f;

        }
        // Start is called before the first frame update
        virtual protected void Start()
        {
            //SaveDefaultTransform(true, true);

            //copyToList = new List<NativeAnimationAvatar>();
            cachedShaders = new Dictionary<string, Shader>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        virtual protected void OnDestroy()
        {
            if (cachedShaders != null)
            {
                Dictionary<string, Shader>.Enumerator enums = cachedShaders.GetEnumerator();
                while (enums.MoveNext())
                {
                    var s = enums.Current;
                    Resources.UnloadAsset(s.Value);
                }
                cachedShaders.Clear();
            }
            
        }
        public List<GameObject> GetColliderAvailable()
        {
            BoxCollider coli;
            MeshCollider meli;

            List<GameObject> ret = new List<GameObject>();

            //---other object self own
            if (
                    (TryGetComponent(out coli))
                )
            {
                //first object with **Renderer
                ret.Add(gameObject);
            }
            else if (TryGetComponent(out meli))
            {
                ret.Add(gameObject);
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (
                    (child.TryGetComponent(out coli))
                )
                {
                    //first object with **Renderer
                    ret.Add(child);
                }
                else if (child.TryGetComponent(out meli))
                {
                    ret.Add(child);
                }
            }
            return ret;
        }
        public void SetColliderAvailable(bool flag)
        {
            BoxCollider boli;
            CapsuleCollider capli;
            MeshCollider meli;
            //---object self
            if (
                    (TryGetComponent(out boli))
                )
            {
                boli.enabled = flag;
                
            }
            else if (TryGetComponent(out meli))
            {
                meli.enabled = flag;
            }
            else if (TryGetComponent(out capli))
            {
                capli.enabled = flag;
            }

            //---child object
            for (int i = 0; i < transform.childCount; i++) 
            {
                GameObject child = transform.GetChild(i).gameObject;
                if (
                    (child.TryGetComponent(out boli))
                )
                {
                    boli.enabled = flag;

                }
                else if (child.TryGetComponent(out meli))
                {
                    meli.enabled = flag;
                }
                else if (child.TryGetComponent(out capli))
                {
                    capli.enabled = flag;
                }
            }

        }
        public int GetEnableWholeIK()
        {
            int ret = 0;
            if (relatedHandleParent != null)
            {
                ret = relatedHandleParent.activeSelf ? 1 : 0;
            }
            return ret;
        }
        public virtual void SetEnableWholeIK(int flag)
        {
            bool tst = flag == 1 ? true : false;
            /*
             * Dummy method =========================
             */
        }
        public void SetEnable(int flag)
        {
            if (flag == 1)
            {
                gameObject.SetActive(true);
                if (relatedHandleParent != null) relatedHandleParent.SetActive(true);
            }
            else
            {
                gameObject.SetActive(false);
                if (relatedHandleParent != null) relatedHandleParent.SetActive(false);
            }
        }
        public bool GetEnable()
        {
            bool ret = false;
            ret = gameObject.activeSelf;
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret ? 1 : 0);
#endif

            return ret;
        }
        public void SaveDefaultColliderPosition(Vector3 pos)
        {
            defaultColliderPosition = new Vector3(pos.x, pos.y, pos.z);
        }
        public void LoadDefaultColliderPosition()
        {
            CapsuleCollider cap = GetComponent<CapsuleCollider>();
            cap.center = defaultColliderPosition;
        }
        public void SaveDefaultTransform(bool ispos, bool isrotate)
        {
            if (ispos) defaultPosition = transform.position;
            if (isrotate) defaultRotation = transform.rotation;
        }
        public void LoadDefaultTransform(bool ispos, bool isrotate)
        {
            if (ispos) transform.position = defaultPosition;
            if (isrotate) transform.rotation = defaultRotation;
        }
        public int GetFixMoving()
        {
            return isFixMoving ? 1 : 0;
        }
        public int GetFixMovingFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(isFixMoving ? 1 : 0);
#endif
            return isFixMoving ? 1 : 0;
        }
        public virtual void SetFixMoving(bool flag)
        {
            /* Available 3D Object
             * VRM - avatar self: CapsuleCollider                                     IK : BoxCollider, SphereCollider
             * Oth - avatar self: none     avatar child(with Mesh) : BoxCollider      IK : BoxCollider
             * IMG - avatar self: none     avatar child(with Mesh) : BoxCollider      IK : BoxCollider
             * CAM - avatar self: none
             * 
             */
            isFixMoving = flag;

            switch (targetType)
            {
                case AF_TARGETTYPE.VRM:
                    transform.gameObject.GetComponent<CapsuleCollider>().enabled = !isFixMoving;
                    if (relatedHandleParent != null) relatedHandleParent.SetActive(!isFixMoving);
                    break;
                case AF_TARGETTYPE.OtherObject:
                case AF_TARGETTYPE.Image:
                    List<GameObject> colist = GetColliderAvailable();
                    colist.ForEach(item =>
                    {
                        BoxCollider bc;
                        MeshCollider mc;
                        if (item.TryGetComponent<BoxCollider>(out bc))
                        {
                            bc.enabled = !isFixMoving;
                        }
                        if (item.TryGetComponent<MeshCollider>(out mc))
                        {
                            mc.enabled = !isFixMoving;
                        }
                        
                    });
                    if (relatedHandleParent != null) relatedHandleParent.SetActive(!isFixMoving);
                    break;
                case AF_TARGETTYPE.Light:
                case AF_TARGETTYPE.Camera:
                    transform.gameObject.GetComponent<BoxCollider>().enabled = !isFixMoving;
                    if (relatedHandleParent != null) relatedHandleParent.SetActive(!isFixMoving);
                    break;
                case AF_TARGETTYPE.Effect:
                    transform.gameObject.GetComponent<CapsuleCollider>().enabled = !isFixMoving;
                    break;
            }
            
            /*if (flag)
            {
                transform.gameObject.GetComponent<BoxCollider>().enabled = false;
                relatedHandleParent.SetActive(false);
            }
            else
            {
                transform.gameObject.GetComponent<BoxCollider>().enabled = true;
                relatedHandleParent.SetActive(true);
            }*/
        }
        public void SetFixMovingFromOuter(string param)
        {
            SetFixMoving(param == "1" ? true : false);
        }
        public void SetObjectTitle(string title)
        {
            Title = title;
        }
        public void GetObjectTitle()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(Title);
#endif
        }

        //---Transform for manual operation--------------------------------------------=============
        public virtual void GetCommonTransformFromOuter()
        {
            Vector3 pos = GetPosition();
            Vector3 rot = GetRotation();
            Vector3 sca = GetScale();
            string ret = "";
            ret = pos.x + "," + pos.y + "," + pos.z + "%" + rot.x + "," + rot.y + "," + rot.z + "%" + sca.x + "," + sca.y + "," + sca.z;
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public Vector3 GetPosition()
        {
            if (relatedHandleParent != null)
            {
                return relatedHandleParent.transform.position;
            }
            else
            {
                return gameObject.transform.position;
            }
        }
        public Vector3 GetPositionFromOuter()
        {
            Vector3 ret;
            if (relatedHandleParent != null)
            {
                ret = relatedHandleParent.transform.position;
            }
            else
            {
                ret = gameObject.transform.position;
            }

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(JsonUtility.ToJson(ret));
#endif

            return ret;
        }
        public void SetPosition(Vector3 pos)
        {
            if (relatedHandleParent != null)
            {
                relatedHandleParent.transform.position = (pos);
            }
            else
            {
                gameObject.transform.position = (pos);
            }
                
        }
        public void SetPositionFromOuter(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;
            bool isabs = prm[3] == "1" ? true : false;

            SetPosition(new Vector3(x, y, z));
            /*if (relatedHandleParent != null)
            {
                relatedHandleParent.transform.DOMove(new Vector3(x, y, z), 0.1f).SetRelative(!isabs);
            }
            else
            {
                gameObject.transform.DOMove(new Vector3(x, y, z), 0.1f).SetRelative(!isabs);
            }*/
                

        }
        public Vector3 GetRotation()
        {
            Vector3 ret;

            if (relatedHandleParent != null)
            {
                ret = relatedHandleParent.transform.rotation.eulerAngles;
            }
            else
            {
                ret = gameObject.transform.rotation.eulerAngles;
            }
            
            return ret;
        }
        public Vector3 GetRotationFromOuter()
        {
            Vector3 ret;
            if (relatedHandleParent != null)
            {
                ret = relatedHandleParent.transform.rotation.eulerAngles;
            }
            else
            {
                ret = gameObject.transform.rotation.eulerAngles;
            }
            

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(JsonUtility.ToJson(ret));
#endif

            return ret;

        }
        public void SetRotation(Vector3 rot)
        {
            if (relatedHandleParent != null)
            {
                relatedHandleParent.transform.rotation = Quaternion.Euler(rot);
            }
            else
            {
                gameObject.transform.rotation = Quaternion.Euler(rot);
            }
            
        }
        public void SetRotationFromOuter(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;
            bool isabs = prm[3] == "1" ? true : false;

            if (relatedHandleParent != null)
            {
                relatedHandleParent.transform.rotation = Quaternion.Euler(new Vector3(x, y, z));
            }
            else
            {
                gameObject.transform.rotation = Quaternion.Euler(new Vector3(x, y, z));
            }
            

        }
        public virtual void SetScale(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;

            //---Different to OperateActiveVRM.SetScale:
            // OperateActiveVRM: base point is child object with collider. refer to transform.parent.
            // At here: transform.gameObject IS Other Object own.

            transform.DOScale(new Vector3(x, y, z), 0.1f);
            //relatedHandleParent.transform.DOScale(new Vector3(x, y, z), 0.1f);

        }
        public virtual Vector3 GetScale()
        {
            Vector3 ret = Vector3.zero;

            ret = transform.localScale;

            return ret;
        }
        public virtual void GetScaleFromOuter()
        {
            Vector3 ret = Vector3.zero;

            ret = transform.localScale;

            string jstr = JsonUtility.ToJson(ret);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(jstr);
#endif
        }
        public virtual void EnableIK(bool flag)
        {
            OtherObjectDummyIK ooik = gameObject.TryGetComponent<OtherObjectDummyIK>(out ooik) ? ooik : null;

            if (ooik != null)
            {
                ooik.enabled = flag;
            }
            
        }
        //---effect: punch------------------------------------
        public virtual void SetPunch(AvatarPunchEffect param)
        {
            if (param != null)
            {
                effectPunch.elasiticity = param.elasiticity;
                effectPunch.isEnable = param.isEnable;
                effectPunch.punch.x = param.punch.x;
                effectPunch.punch.y = param.punch.y;
                effectPunch.punch.z = param.punch.z;
                effectPunch.translationType = param.translationType;
                effectPunch.vibrato = param.vibrato;
            }
        }
        public virtual void SetPunchFromOuter(string param)
        {
            AvatarPunchEffect ape = JsonUtility.FromJson<AvatarPunchEffect>(param);
            if (ape != null)
            {
                effectPunch = ape;
            }
        }
        public AvatarPunchEffect GetPunch()
        {
            return effectPunch;
        }
        public void GetPunchFromOuter()
        {
            string js = JsonUtility.ToJson(effectPunch);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }
        //---effect: shake------------------------------------
        public virtual void SetShake(AvatarShakeEffect param)
        {
            if (param != null)
            {
                effectShake.fadeOut = param.fadeOut;
                effectShake.isEnable = param.isEnable;
                effectShake.randomness = param.randomness;
                effectShake.strength = param.strength;
                effectShake.translationType = param.translationType;
                effectShake.vibrato = param.vibrato;
            }
        }
        public virtual void SetShakeFromOuter(string param)
        {
            AvatarShakeEffect ashe = JsonUtility.FromJson<AvatarShakeEffect>(param);
            if (ashe != null)
            {
                effectShake = ashe;
            }
        }
        public AvatarShakeEffect GetShake()
        {
            return effectShake;
        }
        public void GetShakeFromOuter()
        {
            string js = JsonUtility.ToJson(effectShake);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }
        //---effect: shake------------------------------------
        public void SetJump(string param)
        {
            string[] prm = param.Split(',');
            float power = float.TryParse(prm[0], out power) ? power : 0f;
            int num = int.TryParse(prm[1], out num) ? num : 0;

            jumpNum = num;
            jumpPower = power;
        }
        public float GetJumpPower()
        {
            return jumpPower;
        }
        public int GetJumpNum()
        {
            return jumpNum;
        }
        public void GetJumpPowerFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(jumpPower);
#endif
        }
        public void GetJumpNumFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(jumpNum);
#endif
        }

        private void SetLayerRecursive(GameObject self, int layer)
        {
            self.layer = layer;
            foreach ( Transform tra in self.transform)
            {
                SetLayerRecursive(tra.gameObject, layer);
            }
        }
        public void SetVisibleAvatar(int flag)
        {
            const string TARGET_SHOWLAYER = "Player";
            const string TARGET_HIDDENLAYER = "HiddenPlayer";

            if (flag == 0)
            { //---avatar hide!
                //gameObject.layer = LayerMask.NameToLayer(TARGET_HIDDENLAYER);
                SetLayerRecursive(gameObject, LayerMask.NameToLayer(TARGET_HIDDENLAYER));
            }
            else
            { //---avatar show!
                //gameObject.layer = LayerMask.NameToLayer(TARGET_SHOWLAYER);
                SetLayerRecursive(gameObject, LayerMask.NameToLayer(TARGET_SHOWLAYER));
            }
        }
        public bool GetVisibleAvatar()
        {
            string ret = LayerMask.LayerToName(gameObject.layer);
            return ret == "HiddenLayer" ? false : true;
        }
        public void GetVisibleAvatarFromOuter()
        {
            string ret = LayerMask.LayerToName(gameObject.layer);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret == "HiddenLayer" ? 0 : 1);
#endif
        }



        //########################################################################
        // Render texture (for OtherObject)
        //########################################################################
        public virtual string SetRenderTextureFromCamera(Material targetMat, string role)
        {
            ManageAnimation manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            string ret = "";
            NativeAnimationAvatar nav = manim.GetCastInProject(role, AF_TARGETTYPE.Camera);
            if (nav != null)
            {
                ret = nav.roleTitle;
                //renderTextureParentId = role;
                OperateLoadedCamera olc = nav.avatar.GetComponent<OperateLoadedCamera>();
              
                targetMat.SetTexture("_MainTex", olc.GetRenderTexture());
            }

            return ret;
        }
        public virtual void DestroyCameraRenderTexture(Material targetMat)
        {
            ManageAnimation manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

            Dictionary<string, Material>.Enumerator userMat = userSharedMaterials.GetEnumerator();
            while (userMat.MoveNext())
            {
                if (userMat.Current.Value.name == targetMat.name)
                {
                    string oldpath = userSharedTextureFiles[userMat.Current.Key].texturePath;
                    if (oldpath.IndexOf(CAMERAROLE) > -1)
                    {
                        NativeAnimationAvatar tmpnav = manim.GetCastInProject(oldpath.Replace(CAMERAROLE, ""), AF_TARGETTYPE.Camera);
                        if (tmpnav.avatar.GetComponent<OperateLoadedCamera>().GetRenderTexture() == targetMat.GetTexture("_MainTex"))
                        { //---if render texture of any role, nullize here only.
                            targetMat.SetTexture("_MainTex", null);
                        }
                        
                    }
                    
                    break;
                }
            }
        }
        public virtual string GetCameraRenderTexture()
        {
            return renderTextureParentId;
        }
        public virtual void GetCameraRenderTextureFromOuter()
        {
            string ret = GetCameraRenderTexture();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(renderTextureParentId);
#endif
        }

        //############################################################################
        //  Material properties
        //############################################################################

        /// <summary>
        /// To register materials as the pointer, GameObject has.
        /// </summary>
        public virtual void RegisterUserMaterial()
        {
            userSharedMaterials = new Dictionary<string, Material>();
            userSharedTextureFiles = new Dictionary<string, MaterialProperties>();
            backupTextureFiles = new Dictionary<string, MaterialProperties>();

            backup_userMaterialProperties = new List<MaterialProperties>();


            ManageAvatarTransform matra = GetComponent<ManageAvatarTransform>();
            List<GameObject> meshcnt = matra.CheckSkinnedMeshAvailable();
            foreach (GameObject item in meshcnt)
            {
                SkinnedMeshRenderer skn = null;
                MeshRenderer mr = null;
                Material[] mats = null;
                if (item.TryGetComponent<SkinnedMeshRenderer>(out skn))
                {
                    //mats = skn.materials;
                    mats = skn.sharedMaterials;
                }
                if (item.TryGetComponent<MeshRenderer>(out mr))
                {
                    //mats = mr.materials;
                    mats = mr.sharedMaterials;
                }
                if (mats != null)
                {
                    foreach (Material mat in mats)
                    {
                        //---name of SkinedMeshRenderer GameObject
                        string keyname = item.name;// + "_" + mat.name;
                        string newkeyname = "";
                        int dupCount = 0;
                        string suffix = "";
                        while (userSharedMaterials.ContainsKey(keyname+suffix))
                        {
                            dupCount++;
                            suffix = "(" + dupCount.ToString() + ")";
                        }
                        /*if (userSharedMaterials.ContainsKey(keyname))
                        {
                            keyname += DateTime.Now.ToFileTime().ToString();
                        }*/
                        //userSharedMaterials & userSharedTextureFiles & backupTextureFiles always match key-name.

                        newkeyname = keyname + suffix;
                        //---change Shader, exclude: Standard, VRM/MToon, FX/Water4
                        /*if ((mat.shader.name != "Standard") && (mat.shader.name != "VRM/MToon") && (mat.shader.name != "FX/Water4") && (mat.shader.name != "FX/Water (Basic)"))
                        {
                            mat.shader = Shader.Find("Standard");
                        }*/

                        /*
                         * Materials : GameObject.name [0]~[n] : Material
                         * GameObject1.name (Material, Material,Material,...)
                         * GameObject2.name (Material)
                         * ...
                         * 
                         */
                        userSharedMaterials.Add(newkeyname, mat);

                        MaterialProperties matp = new MaterialProperties();
                        matp.texturePath = "";
                        userSharedTextureFiles.Add(newkeyname, matp);

                        MaterialProperties backmatp = new MaterialProperties();
                        backmatp.realTexture = mat.GetTexture("_MainTex");
                        backmatp.texturePath = matp.texturePath;
                        backupTextureFiles.Add(newkeyname, backmatp);
                    }
                }
            }
            backup_userMaterialProperties = ListUserMaterialObject();
        }

        /// <summary>
        /// Generate MaterialProperites from raw Material (one of each userSharedMaterial)
        /// </summary>
        /// <param name="gobjName"></param>
        /// <param name="mat"></param>
        /// <returns></returns>
        public virtual MaterialProperties GetUserMaterialObject(string gobjName, Material mat)
        {
            MaterialProperties matp = new MaterialProperties();

            matp.name = gobjName;
            matp.matName = mat.name;

            matp.shaderName = mat.shader.name;

            if (mat.shader.name.ToLower() == SHAD_VRM)
            {
                matp.color = mat.color;
                matp.emissioncolor = mat.GetColor("_EmissionColor");

                matp.cullmode = mat.GetFloat("_CullMode");
                matp.blendmode = mat.GetFloat("_BlendMode");
                matp.shadetexcolor = mat.GetColor("_ShadeColor");
                matp.shadingtoony = mat.GetFloat("_ShadeToony");
                matp.rimcolor = mat.GetColor("_RimColor");
                matp.rimfresnel = mat.GetFloat("_RimFresnelPower");
                matp.srcblend = mat.GetFloat("_SrcBlend");
                matp.dstblend = mat.GetFloat("_DstBlend");
                matp.cutoff = mat.GetFloat("_Cutoff");
                matp.shadingshift = mat.GetFloat("_ShadeShift");
                matp.receiveshadow = mat.GetFloat("_ReceiveShadowRate");
                matp.shadinggrade = mat.GetFloat("_ShadingGradeRate");
                matp.lightcolorattenuation = mat.GetFloat("_LightColorAttenuation");

                matp.texturePath = userSharedTextureFiles[gobjName].texturePath;
                matp.textureRole = userSharedTextureFiles[gobjName].textureRole;
                matp.textureIsCamera = userSharedTextureFiles[gobjName].textureIsCamera;

            }
            else if (mat.shader.name.ToLower() == SHAD_VRM10)
            {
                matp.color = mat.color;
                matp.emissioncolor = mat.GetColor("_EmissionColor");

                matp.cullmode = mat.GetFloat("_M_CullMode");
                matp.blendmode = mat.GetFloat("_AlphaMode");
                matp.shadetexcolor = mat.GetColor("_ShadeColor");
                matp.shadingtoony = mat.GetFloat("_ShadingToonyFactor");
                matp.rimcolor = mat.GetColor("_RimColor");
                matp.rimfresnel = mat.GetFloat("_RimFresnelPower");
                matp.srcblend = mat.GetFloat("_M_SrcBlend");
                matp.dstblend = mat.GetFloat("_M_DstBlend");
                matp.cutoff = mat.GetFloat("_Cutoff");
                matp.shadingshift = mat.GetFloat("_ShadingShiftFactor");
                matp.receiveshadow = 0f;
                matp.shadinggrade = 1f;
                matp.lightcolorattenuation = 0;

                matp.texturePath = userSharedTextureFiles[gobjName].texturePath;
                matp.textureRole = userSharedTextureFiles[gobjName].textureRole;
                matp.textureIsCamera = userSharedTextureFiles[gobjName].textureIsCamera;
            }
            else if (mat.shader.name.ToLower() == SHAD_STD)
            {
                matp.color = mat.color;
                matp.emissioncolor = mat.GetColor("_EmissionColor");

                matp.cullmode = 0;
                matp.blendmode = mat.GetFloat("_Mode");
                matp.metallic = mat.GetFloat("_Metallic");
                matp.glossiness = mat.GetFloat("_Glossiness");

                matp.texturePath = userSharedTextureFiles[gobjName].texturePath;
                matp.textureRole = userSharedTextureFiles[gobjName].textureRole;
                matp.textureIsCamera = userSharedTextureFiles[gobjName].textureIsCamera;

            }
            else if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
            {
                matp.fresnelScale = mat.GetFloat("_FresnelScale");
                matp.color = mat.GetColor("_BaseColor");
                matp.reflectionColor = mat.GetColor("_ReflectionColor");
                matp.specularColor = mat.GetColor("_SpecularColor");

                matp.waveAmplitude = mat.GetVector("_GAmplitude");
                matp.waveFrequency = mat.GetVector("_GFrequency");
                matp.waveSteepness = mat.GetVector("_GSteepness");
                matp.waveSpeed = mat.GetVector("_GSpeed");
                matp.waveDirectionAB = mat.GetVector("_GDirectionAB");
                matp.waveDirectionCD = mat.GetVector("_GDirectionCD");
            }
            else if ((mat.shader.name.ToLower() == SHAD_SKE) || (mat.shader.name.ToLower() == SHAD_PSKE))
            {
                matp.outlinewidth = mat.GetFloat("_OutlineWidth");
                if (mat.shader.name.ToLower() == SHAD_PSKE)
                {
                    matp.strokedensity = mat.GetFloat("_StrokeDensity");
                    matp.addbrightness = mat.GetFloat("_AddBrightNess");
                    matp.multbrightness = mat.GetFloat("_MultBrightNess");
                }
                else
                {
                    matp.shadowbrightness = mat.GetFloat("_ShadowBrightNess");
                }
            }
            else if (mat.shader.name.ToLower() == SHAD_REALTOON)
            {
                matp.enableTexTransparent = mat.GetFloat("_EnableTextureTransparent");
                matp.mainColorInAmbientLightOnly = mat.GetFloat("_MCIALO");
                matp.doubleSided = mat.GetInteger("_DoubleSided");
                matp.outlineZPosCam = mat.GetFloat("_OutlineZPostionInCamera");
                matp.thresHold = mat.GetFloat("_SelfShadowThreshold");
                matp.shadowHardness = mat.GetFloat("_ShadowTHardness");
            }
            else if (mat.shader.name.ToLower() == SHAD_COMIC)
            {
                matp.enableTexTransparent = mat.GetFloat("_EnableTextureTransparent");
                matp.lineWidth = mat.GetFloat("_LineWidth");
                matp.lineColor = mat.GetColor("_LineColor");
                matp.tone1Threshold = mat.GetFloat("_Tone1Threshold");

            }
            else if (mat.shader.name.ToLower() == SHAD_ICE)
            {
                matp.iceColor = mat.GetColor("_Color");
                matp.transparency = mat.GetFloat("_Transparency");
                matp.baseTransparency = mat.GetFloat("_BaseTransparency");
                matp.iceRoughness = mat.GetFloat("_IceRoughness");
                matp.distortion = mat.GetFloat("_Distortion");
            }
            else if (mat.shader.name.ToLower() == SHAD_MICRA)
            {
                matp.pixelSize = mat.GetFloat("_PixelSize");
            }

            return matp;
        }
        /// <summary>
        /// Generate a list of MaterialProperties from raw Material
        /// </summary>
        /// <returns>packed List of MaterialProperties</returns>
        public virtual List<MaterialProperties> ListUserMaterialObject()
        {
            List<MaterialProperties> ret = new List<MaterialProperties>();

            foreach (KeyValuePair<string, Material> kvp in userSharedMaterials)
            {
                Material mat = kvp.Value;

                MaterialProperties matp = GetUserMaterialObject(kvp.Key, mat);

                ret.Add(matp);
            }

            return ret;
        }

        /// <summary>
        /// To write normal csv-string to serializable csv-string 
        /// </summary>
        /// <returns></returns>
        public virtual List<string> ListUserMaterial()
        {
            List<string> ret = new List<string>();

            Dictionary<string, Material>.Enumerator matlst = userSharedMaterials.GetEnumerator();
            while (matlst.MoveNext())
            {
                //Debug.Log("matlst.Current.Key=" + matlst.Current.Key);
                ret.Add(ListGetOneUserMaterial2(matlst.Current.Key));
            }

            return ret;
        }
        public virtual void ListUserMaterialFromOuter()
        {
            List<string> list = ListUserMaterial();
            string js = string.Join("\r\n", list.ToArray());
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }

        /// <summary>
        /// Generate material properties csv-string from raw Material data (To write 1 - material to csv-string )
        /// </summary>
        /// <param name="gobjName"></param>
        /// <returns></returns>
        public virtual string ListGetOneUserMaterial(string gobjName)
        {
            const string SEPSTR = "=";
            string ret = "";

            //Debug.Log("param=" + param);
            //Debug.Log(userSharedMaterials.ContainsKey(param));

            if (userSharedMaterials.ContainsKey(gobjName))
            {
                Material mat = userSharedMaterials[gobjName];
                //Debug.Log("material name=" + mat.name);
                string texturePath = userSharedTextureFiles[gobjName].texturePath;

                if (mat.shader.name.ToLower() == SHAD_VRM)
                {
                    ret = (
                        gobjName + SEPSTR +
                        mat.name + SEPSTR + 
                        mat.shader.name + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(mat.color) + SEPSTR +
                        mat.GetFloat("_CullMode").ToString() + SEPSTR +
                        mat.GetFloat("_BlendMode").ToString() + SEPSTR +
                        texturePath + SEPSTR +
                        //---v1 = 6, v2 = 7
                        "0" + SEPSTR +
                        "0" + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_EmissionColor")) + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_ShadeColor")) + SEPSTR +
                        mat.GetFloat("_ShadeToony").ToString() + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_RimColor")) + SEPSTR +
                        mat.GetFloat("_RimFresnelPower").ToString() + SEPSTR +
                        mat.GetFloat("_SrcBlend").ToString() + SEPSTR +
                        mat.GetFloat("_DstBlend").ToString() + SEPSTR + 
                        //---v2 = 16
                        mat.GetFloat("_Cutoff").ToString() + SEPSTR + 
                        mat.GetFloat("_ShadeShift").ToString() + SEPSTR + 
                        mat.GetFloat("_ReceiveShadowRate").ToString() + SEPSTR + 
                        mat.GetFloat("_ShadingGradeRate").ToString() + SEPSTR + 
                        mat.GetFloat("_LightColorAttenuation").ToString()
                    );                    
                }
                else if (mat.shader.name.ToLower() == SHAD_VRM10)
                {
                    ret = (
                        gobjName + SEPSTR +
                        mat.name + SEPSTR +
                        mat.shader.name + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(mat.color) + SEPSTR +
                        mat.GetFloat("_M_CullMode").ToString() + SEPSTR +
                        mat.GetFloat("_AlphaMode").ToString() + SEPSTR +
                        texturePath + SEPSTR +
                        //---v1 = 6, v2 = 7
                        "0" + SEPSTR +
                        "0" + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_EmissionColor")) + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_ShadeColor")) + SEPSTR +
                        mat.GetFloat("_ShadingToonyFactor").ToString() + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_RimColor")) + SEPSTR +
                        mat.GetFloat("_RimFresnelPower").ToString() + SEPSTR +
                        mat.GetFloat("_M_SrcBlend").ToString() + SEPSTR +
                        mat.GetFloat("_M_DstBlend").ToString() + SEPSTR +
                        //---v2 = 16
                        mat.GetFloat("_Cutoff").ToString() + SEPSTR +
                        mat.GetFloat("_ShadingShiftFactor").ToString() + SEPSTR +
                        "1" + SEPSTR +
                        "1" + SEPSTR +
                        "0"
                    );

                }
                else if (mat.shader.name.ToLower() == SHAD_STD)
                {
                    ret = (
                        gobjName + SEPSTR +
                        mat.name + SEPSTR +
                        mat.shader.name + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(mat.color) + SEPSTR +
                        "0" + SEPSTR +
                        mat.GetFloat("_Mode").ToString() + SEPSTR +
                        texturePath + SEPSTR +
                        //---v1 = 6, v2 = 7
                        mat.GetFloat("_Metallic").ToString() + SEPSTR +
                        mat.GetFloat("_Glossiness").ToString() + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_EmissionColor")) + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(Color.white) + SEPSTR +
                        "0" + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(Color.white) + SEPSTR +
                        "0" + SEPSTR +
                        "1" + SEPSTR +
                        "0" + SEPSTR + 
                        //---v2 = 16
                        "0.5" + SEPSTR + 
                        "0" + SEPSTR + 
                        "1" + SEPSTR + 
                        "1" + SEPSTR + 
                        "0"
                    );
                }
                else if ( (mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT) )
                {
                    Vector4 wa = mat.GetVector("_GAmplitude");
                    Vector4 wf = mat.GetVector("_GFrequency");
                    Vector4 wt = mat.GetVector("_GSteepness");
                    Vector4 ws = mat.GetVector("_GSpeed");
                    Vector4 wdab = mat.GetVector("_GDirectionAB");
                    Vector4 wdcd = mat.GetVector("_GDirectionCD");
                    ret = (
                        gobjName + SEPSTR +
                        mat.name + SEPSTR +
                        mat.shader.name + SEPSTR +
                        mat.GetFloat("_FresnelScale").ToString() + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_BaseColor")) + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_ReflectionColor")) + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_SpecularColor")) + SEPSTR +
                        //---v1 = 6, v2 = 7
                        wa.x.ToString() + "," + wa.y.ToString() + "," + wa.z.ToString() + "," + wa.w.ToString() + SEPSTR +
                        wf.x.ToString() + "," + wf.y.ToString() + "," + wf.z.ToString() + "," + wf.w.ToString() + SEPSTR +
                        wt.x.ToString() + "," + wt.y.ToString() + "," + wt.z.ToString() + "," + wt.w.ToString() + SEPSTR +
                        ws.x.ToString() + "," + ws.y.ToString() + "," + ws.z.ToString() + "," + ws.w.ToString() + SEPSTR +
                        wdab.x.ToString() + "," + wdab.y.ToString() + "," + wdab.z.ToString() + "," + wdab.w.ToString() + SEPSTR +
                        wdcd.x.ToString() + "," + wdcd.y.ToString() + "," + wdcd.z.ToString() + "," + wdcd.w.ToString()
                    );
                }
                else if ((mat.shader.name.ToLower() == SHAD_SKE))
                {

                    string [] tmparr = new string[] {
                        gobjName,
                        mat.name,
                        mat.shader.name,
                        "0",
                        "0",
                        "0",
                        mat.GetFloat("_ShadowBrightNess").ToString()
                    };
                    ret = String.Join(SEPSTR, tmparr);
                }
                else if (mat.shader.name.ToLower() == SHAD_PSKE)
                {
                    string[] tmparr = new string[] {
                        gobjName,
                        mat.name,
                        mat.shader.name,
                        mat.GetFloat("_StrokeDensity").ToString(),
                        mat.GetFloat("_AddBrightNess").ToString(),
                        mat.GetFloat("_MultBrightNess").ToString(),
                        "0"
                    };
                    ret = String.Join(SEPSTR, tmparr);
                }
                else if (mat.shader.name.ToLower() == SHAD_REALTOON)
                {
                    string[] tmparr = new string[] {
                        gobjName,
                        mat.name,
                        mat.shader.name,
                        mat.GetFloat("_EnableTextureTransparent").ToString(),
                        mat.GetFloat("_MCIALO").ToString(),
                        mat.GetInteger("_DoubleSided").ToString(),
                        mat.GetFloat("_OutlineZPostionInCamera").ToString(),
                        mat.GetFloat("_SelfShadowThreshold").ToString(),
                        mat.GetFloat("_ShadowTHardness").ToString()
                    };
                    ret = String.Join(SEPSTR, tmparr);
                }
                else if (mat.shader.name.ToLower() == SHAD_COMIC)
                {
                    string[] tmparr = new string[] {
                        gobjName,
                        mat.name,
                        mat.shader.name,
                        mat.GetFloat("_EnableTextureTransparent").ToString(),
                        mat.GetFloat("_LineWidth").ToString(),
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_LineColor")),
                        mat.GetFloat("_Tone1Threshold").ToString()
                    };
                    ret = String.Join(SEPSTR, tmparr);
                }
                else if (mat.shader.name.ToLower() == SHAD_ICE)
                {
                    string[] tmparr = new string[] {
                        gobjName,
                        mat.name,
                        mat.shader.name,
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_Color")),
                        mat.GetFloat("_Transparency").ToString(),
                        mat.GetFloat("_BaseTransparency").ToString(),
                        mat.GetFloat("_IceRoughness").ToString(),
                        mat.GetFloat("_Distortion").ToString()
                    };
                    ret = String.Join(SEPSTR, tmparr);
                }
                else if (mat.shader.name.ToLower() == SHAD_MICRA)
                {
                    string[] tmparr = new string[]
                    {
                        gobjName,
                        mat.name,
                        mat.shader.name,
                        mat.GetFloat("_PixelSize").ToString()
                    };
                    ret = String.Join(SEPSTR, tmparr);
                }
            }
            /////Debug.Log("ret=" + ret);
            // 0 - key name
            // 1 - material name
            // 2 - shader name
            // 3 - material color
            // 4 - Cull mode
            // 5 - Blend mode
            // 6 - Texture name
            // 7 - Metallic (Standard)
            // 8 - Glossiness (Standard)
            // 9 - Emission Color 
            // 10- Shade Texture Color (VRM/MToon)
            // 11- Shaing Toony (VRM/MToon)
            // 12- Rim Color (VRM/MToon)
            // 13- Rim Fresnel Power (VRM/MToon)
            // 14- SrcBlend (VRM/MToon)
            // 15- DstBlend (VRM/MToon)

            return ret;
        }

        /// <summary>
        /// Generate material properties csv-string from backup properties
        /// </summary>
        /// <param name="gobjName"></param>
        /// <returns></returns>
        public virtual string ListGetOneUserMaterial2(string gobjName)
        {
            const string SEPSTR = "=";
            string ret = "";

            int inx = backup_userMaterialProperties.FindIndex(item =>
            {
                if (item.name == gobjName)  return true;
                return false;
            });
            if (inx > -1)
            {
                List<string> retarr = new List<string>();
                MaterialProperties mat = backup_userMaterialProperties[inx];
                string texturePath = userSharedTextureFiles[gobjName].texturePath;

                if (mat.shaderName.ToLower() == SHAD_STD)
                {
                    retarr.AddRange(new string[]
                    {
                        mat.name, mat.matName, mat.shaderName,
                        ColorUtility.ToHtmlStringRGBA(mat.color),
                        "0", mat.blendmode.ToString(),
                        texturePath, mat.metallic.ToString(), mat.glossiness.ToString(),
                        ColorUtility.ToHtmlStringRGBA(mat.emissioncolor),
                        ColorUtility.ToHtmlStringRGBA(Color.white),
                        "0",
                        ColorUtility.ToHtmlStringRGBA(Color.white),
                        "0", "1", "0", 
                        "0.5", "0", "1", "1", "0"
                    });
                }
                else if (mat.shaderName.ToLower() == SHAD_VRM)
                {
                    retarr.AddRange(new string[]  { 
                        mat.name, mat.matName, mat.shaderName,
                        ColorUtility.ToHtmlStringRGBA(mat.color),
                        mat.cullmode.ToString(), mat.blendmode.ToString(),
                        texturePath, "0", "0",
                        ColorUtility.ToHtmlStringRGBA(mat.emissioncolor),
                        ColorUtility.ToHtmlStringRGBA(mat.shadetexcolor),
                        mat.shadingtoony.ToString(),
                        ColorUtility.ToHtmlStringRGBA(mat.rimcolor),
                        mat.rimfresnel.ToString(),
                        mat.srcblend.ToString(), mat.dstblend.ToString(),

                        mat.cutoff.ToString(), mat.shadingshift.ToString(),
                        mat.receiveshadow.ToString(), mat.shadinggrade.ToString(),
                        mat.lightcolorattenuation.ToString()
                    });
                }
                else if (mat.shaderName.ToLower() == SHAD_VRM10)
                {
                    retarr.AddRange(new string[]  {
                        mat.name, mat.matName, mat.shaderName,
                        ColorUtility.ToHtmlStringRGBA(mat.color),
                        mat.cullmode.ToString(), mat.blendmode.ToString(),
                        texturePath, "0", "0",
                        ColorUtility.ToHtmlStringRGBA(mat.emissioncolor),
                        ColorUtility.ToHtmlStringRGBA(mat.shadetexcolor),
                        mat.shadingtoony.ToString(),
                        ColorUtility.ToHtmlStringRGBA(mat.rimcolor),
                        mat.rimfresnel.ToString(),
                        mat.srcblend.ToString(), mat.dstblend.ToString(),

                        mat.cutoff.ToString(), mat.shadingshift.ToString(),
                        "1", "1", "0"
                    });
                }
                else if ((mat.shaderName.ToLower() == SHAD_WT) || (mat.shaderName.ToLower() == SHAD_SWT))
                {
                    Vector4 wa = mat.waveAmplitude;
                    Vector4 wf = mat.waveFrequency;
                    Vector4 wt = mat.waveSteepness;
                    Vector4 ws = mat.waveSpeed;
                    Vector4 wdab = mat.waveDirectionAB;
                    Vector4 wdcd = mat.waveDirectionCD;

                    retarr.AddRange(new string[]  {
                        mat.name, mat.matName, mat.shaderName,
                        mat.fresnelScale.ToString(),
                        ColorUtility.ToHtmlStringRGBA(mat.color),
                        ColorUtility.ToHtmlStringRGBA(mat.reflectionColor),
                        ColorUtility.ToHtmlStringRGBA(mat.specularColor),
                        //---v1 = 6, v2 = 7
                        wa.x.ToString() + "," + wa.y.ToString() + "," + wa.z.ToString() + "," + wa.w.ToString(),
                        wf.x.ToString() + "," + wf.y.ToString() + "," + wf.z.ToString() + "," + wf.w.ToString(),
                        wt.x.ToString() + "," + wt.y.ToString() + "," + wt.z.ToString() + "," + wt.w.ToString(),
                        ws.x.ToString() + "," + ws.y.ToString() + "," + ws.z.ToString() + "," + ws.w.ToString(),
                        wdab.x.ToString() + "," + wdab.y.ToString() + "," + wdab.z.ToString() + "," + wdab.w.ToString(),
                        wdcd.x.ToString() + "," + wdcd.y.ToString() + "," + wdcd.z.ToString() + "," + wdcd.w.ToString()

                    });
                }
                else if ((mat.shaderName.ToLower() == SHAD_SKE))
                {

                    retarr.AddRange(new string[]  {
                        gobjName,
                        mat.matName,
                        mat.shaderName,
                        "0",
                        "0",
                        "0",
                        mat.shadowbrightness.ToString()
                    });
                    
                }
                else if ((mat.shaderName.ToLower() == SHAD_PSKE))
                {

                    retarr.AddRange(new string[]  {
                        gobjName,
                        mat.matName,
                        mat.shaderName,
                        mat.strokedensity.ToString(),
                        mat.addbrightness.ToString(),
                        mat.multbrightness.ToString(),
                        "0"
                    });

                }
                else if ((mat.shaderName.ToLower() == SHAD_REALTOON))
                {

                    retarr.AddRange(new string[]  {
                        gobjName,
                        mat.matName,
                        mat.shaderName,
                        mat.enableTexTransparent.ToString(),
                        mat.mainColorInAmbientLightOnly.ToString(),
                        mat.doubleSided.ToString(),
                        mat.outlineZPosCam.ToString(),
                        mat.thresHold.ToString(),
                        mat.shadowHardness.ToString()
                    });

                }
                else if ((mat.shaderName.ToLower() == SHAD_COMIC))
                {

                    retarr.AddRange(new string[]  {
                        gobjName,
                        mat.matName,
                        mat.shaderName,
                        mat.enableTexTransparent.ToString(),
                        mat.lineWidth.ToString(),
                        mat.lineColor.ToString(),
                        mat.tone1Threshold.ToString()
                    });

                }
                else if ((mat.shaderName.ToLower() == SHAD_ICE))
                {

                    retarr.AddRange(new string[]  {
                        gobjName,
                        mat.matName,
                        mat.shaderName,
                        ColorUtility.ToHtmlStringRGBA(mat.iceColor),
                        mat.transparency.ToString(),
                        mat.baseTransparency.ToString(),
                        mat.iceRoughness.ToString(),
                        mat.distortion.ToString()
                    });

                }
                else if ((mat.shaderName.ToLower() == SHAD_MICRA))
                {

                    retarr.AddRange(new string[]  {
                        gobjName,
                        mat.matName,
                        mat.shaderName,
                        mat.pixelSize.ToString()
                    });

                }
                ret = String.Join(SEPSTR, retarr);
            }
            return ret;
        }

        /// <summary>
        /// Get material properties as text data (for HTML)
        /// </summary>
        /// <param name="gobjName">GameObject name</param>
        public virtual void ListGetOneUserMaterialFromOuter(string gobjName)
        {
            string ret = ListGetOneUserMaterial(gobjName);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }

        /// <summary>
        /// Find a shader from Resouces, cache(own object), BuiltIn
        /// </summary>
        /// <param name="path">Shader path name</param>
        /// <returns></returns>
        public virtual Shader FindShader(string path)
        {
            Shader ret = null;
            if (cachedShaders.ContainsKey(path))
            {
                ret = cachedShaders[path];
            }
            else
            {
                if (path.ToLower() == SHAD_REALTOON)
                {
                    ret = Resources.Load<Shader>("L_Default");
                    cachedShaders.Add(path, ret);

                }
                else
                {
                    ret = Shader.Find(path);
                }
                
            }

            return ret;

        }

        /// <summary>
        /// To set material property from Unity (NOT USE)
        /// </summary>
        /// <param name="item_name"></param>
        /// <param name="mat_name"></param>
        /// <param name="propname"></param>
        /// <param name="vmat"></param>
        /// <param name="isSaveOnly"></param>
        public virtual void SetUserMaterial(string item_name, string mat_name, string propname, MaterialProperties vmat, bool isSaveOnly = false)
        {
            string fullkey = item_name + "_" + mat_name;

            if (userSharedMaterials.ContainsKey(fullkey))
            {
                ManageAnimation manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

                Material mat = userSharedMaterials[fullkey];
                if (mat != null)
                {
                    if (propname.ToLower() == "shader")
                    {
                        Shader target = FindShader(vmat.shaderName); //Shader.Find(vmat.shaderName);
                        if (target != null)
                        {
                            mat.shader = target;
                        }
                    }
                    else if (propname.ToLower() == "color")
                    {
                        if ( (mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT) )
                        {
                            mat.SetColor("_BaseColor", vmat.color);
                        }
                        else if ((mat.shader.name.ToLower() == SHAD_STD) || (mat.shader.name.ToLower() == SHAD_VRM))
                        {
                            mat.SetColor("_Color", vmat.color);
                        }
                        
                    }
                    else if (propname.ToLower() == "renderingtype")
                    {
                        if (mat.shader.name.ToLower() == SHAD_STD)
                        {
                            mat.SetFloat("_Mode", vmat.blendmode);
                        }
                        else if (mat.shader.name.ToLower() == SHAD_VRM)
                        {
                            mat.SetFloat("_BlendMode", vmat.blendmode);
                        }
                        else if (mat.shader.name.ToLower() == SHAD_VRM10)
                        {
                            mat.SetFloat("_AlphaMode", vmat.blendmode);
                        }
                    }
                    else if (propname.ToLower() == "cullmode")
                    { //0 - off, 1 - front, 2 - back
                        if (mat.shader.name.ToLower() == SHAD_VRM)
                        {
                            mat.SetFloat("_CullMode", vmat.cullmode);
                        }
                        else if (mat.shader.name.ToLower() == SHAD_VRM10)
                        {
                            mat.SetFloat("_M_CullMode", vmat.cullmode);
                        }
                    }
                    else if (propname.ToLower() == "alphacutoff")
                    {
                        if (
                            (mat.shader.name.ToLower() == SHAD_VRM) ||
                            (mat.shader.name.ToLower() == SHAD_VRM10)
                        )
                        {
                            mat.SetFloat("_Cutoff", vmat.cutoff);
                        }
                    }
                    else if (propname.ToLower() == "maintex")
                    { //texture path or role name
                        /*
                         * Search order:
                         * 1, Camera           = #Camera[Cam_932931229]
                         * 2, Default material = ""
                         * 3, materialPackage  = [material name]
                         * 
                         */
                        if ((userSharedTextureFiles[fullkey].texturePath == vmat.texturePath) && (vmat.texturePath == ""))
                        {
                            mat.SetTexture("_MainTex", null);
                            mat.SetTexture("_MainTex", backupTextureFiles[fullkey].realTexture);
                            userSharedTextureFiles[fullkey].texturePath = vmat.texturePath;
                            userSharedTextureFiles[fullkey].textureRole = "";
                            userSharedTextureFiles[fullkey].textureIsCamera = 0;
                        }
                        else
                        {
                            if (vmat.texturePath.IndexOf(CAMERAROLE) > -1)
                            { //value: #Cameracam_12335667-------------------------------
                                string rolename = vmat.texturePath.Replace(CAMERAROLE, "");


                                //---old texture nullize
                                if (userSharedTextureFiles[fullkey].textureIsCamera == 0)
                                { //---old is general texture
                                    manim.UnReferMaterial(OneMaterialType.Texture, userSharedTextureFiles[fullkey].texturePath);
                                }
                                mat.SetTexture("_MainTex", null);

                                //---set new texture
                                if (rolename != "")
                                {
                                    SetRenderTextureFromCamera(mat, rolename);
                                    userSharedTextureFiles[fullkey].texturePath = vmat.texturePath;
                                    userSharedTextureFiles[fullkey].textureRole = rolename;
                                    userSharedTextureFiles[fullkey].textureIsCamera = 1;
                                }

                            }
                            else if (vmat.texturePath == "")
                            { //---recover default texture
                                //---old texture nullize
                                if (userSharedTextureFiles[fullkey].textureIsCamera == 0)
                                { //---old is general texture
                                    manim.UnReferMaterial(OneMaterialType.Texture, userSharedTextureFiles[fullkey].texturePath);

                                }
                                mat.SetTexture("_MainTex", null);
                                mat.SetTexture("_MainTex", backupTextureFiles[fullkey].realTexture);
                                userSharedTextureFiles[fullkey].texturePath = vmat.texturePath;
                                userSharedTextureFiles[fullkey].textureRole = "";
                                userSharedTextureFiles[fullkey].textureIsCamera = 0;
                            }
                            else
                            { // value: materialManager name ---------------------------
                                
                                //---new version
                                //value: NativeAP_OneMaterial.name-------------------------------
                                //---nullize old texture
                                if (userSharedTextureFiles[fullkey].textureIsCamera == 0)
                                {
                                    manim.UnReferMaterial(OneMaterialType.Texture, userSharedTextureFiles[fullkey].texturePath);
                                }
                                mat.SetTexture("_MainTex", null);

                                //---set new texture
                                NativeAP_OneMaterial nap = manim.FindTexture(vmat.texturePath);
                                if (nap != null)
                                {
                                    mat.SetTexture("_MainTex", nap.ReferTexture2D());
                                    userSharedTextureFiles[fullkey].texturePath = vmat.texturePath;
                                    userSharedTextureFiles[fullkey].textureRole = "";
                                    userSharedTextureFiles[fullkey].textureIsCamera = 0;
                                }
                            }
                        }


                    }
                    else if (propname.ToLower() == "metallic")
                    {
                        if (mat.shader.name.ToLower() == SHAD_STD)
                        {
                            mat.SetFloat("_Metallic", vmat.metallic);
                        }
                    }
                    else if (propname.ToLower() == "glossiness")
                    {
                        if (mat.shader.name.ToLower() == SHAD_STD)
                        {
                            mat.SetFloat("_Glossiness", vmat.glossiness);
                        }
                    }
                    else if (propname.ToLower() == "emissioncolor")
                    {
                        mat.EnableKeyword("EMISSION");
                        mat.SetColor("_EmissionColor", vmat.emissioncolor);

                    }
                    else if (propname.ToLower() == "shadetexcolor")
                    {
                        if (
                            (mat.shader.name.ToLower() == SHAD_VRM) ||
                            (mat.shader.name.ToLower() == SHAD_VRM10)
                        )
                        {
                            mat.SetColor("_ShadeColor", vmat.shadetexcolor);
                        }
                    }
                    else if (propname.ToLower() == "shadingtoony")
                    {
                        if (mat.shader.name.ToLower() == SHAD_VRM)
                        {
                            mat.SetFloat("_ShadeToony", vmat.shadingtoony);
                        }
                        else if (mat.shader.name.ToLower() == SHAD_VRM10)
                        {
                            mat.SetFloat("_ShadingToonyFactor", vmat.shadingtoony);
                        }
                    }
                    else if (propname.ToLower() == "shadingshift")
                    {
                        if (mat.shader.name.ToLower() == SHAD_VRM)
                        {
                            mat.SetFloat("_ShadeShift", vmat.shadingshift);
                        }
                        else if (mat.shader.name.ToLower() == SHAD_VRM10)
                        {
                            mat.SetFloat("_ShadingShiftFactor", vmat.shadingshift);
                        }
                    }
                    else if (propname.ToLower() == "receiveshadow")
                    {
                        if (mat.shader.name.ToLower() == SHAD_VRM)
                        {
                            mat.SetFloat("_ReceiveShadowRate", vmat.receiveshadow);
                        }
                    }
                    else if (propname.ToLower() == "shadinggrade")
                    {
                        if (mat.shader.name.ToLower() == SHAD_VRM)
                        {
                            mat.SetFloat("_ShadingGradeRate", vmat.shadinggrade);
                        }
                    }
                    else if (propname.ToLower() == "lightcolorattenuation")
                    {
                        if (mat.shader.name.ToLower() == SHAD_VRM)
                        {
                            mat.SetFloat("_LightColorAttenuation", vmat.lightcolorattenuation);
                        }
                    }
                    else if (propname.ToLower() == "rimcolor")
                    {
                        if (
                            (mat.shader.name.ToLower() == SHAD_VRM) ||
                            (mat.shader.name.ToLower() == SHAD_VRM10)
                        )
                        {
                            mat.SetColor("_RimColor", vmat.rimcolor);
                        }
                    }
                    else if (propname.ToLower() == "rimfresnel")
                    {
                        if (
                            (mat.shader.name.ToLower() == SHAD_VRM) ||
                            (mat.shader.name.ToLower() == SHAD_VRM10)
                        )
                        {
                            mat.SetFloat("_RimFresnelPower", vmat.rimfresnel);
                        }                        
                    }
                    else if (propname.ToLower() == "srcblend")
                    {
                        if (
                            (mat.shader.name.ToLower() == SHAD_VRM)
                            
                        )
                        {
                            mat.SetFloat("_SrcBlend", vmat.srcblend);
                        }
                        else if (mat.shader.name.ToLower() == SHAD_VRM10)
                        {
                            mat.SetFloat("_M_SrcBlend", vmat.srcblend);
                        }
                    }
                    else if (propname.ToLower() == "dstblend")
                    {
                        if (
                            (mat.shader.name.ToLower() == SHAD_VRM)
                        )
                        {
                            mat.SetFloat("_DstBlend", vmat.dstblend);
                        }
                        else if (mat.shader.name.ToLower() == SHAD_VRM10)
                        {
                            mat.SetFloat("_M_DstBlend", vmat.dstblend);
                        }
                    }
                    else if (propname.ToLower() == "fresnelscale")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            mat.SetFloat("_FresnelScale", vmat.fresnelScale);
                        }
                    }
                    else if (propname.ToLower() == "reflectioncolor")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            mat.SetColor("_ReflectionColor", vmat.reflectionColor);
                        }
                    }
                    else if (propname.ToLower() == "specularcolor")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            mat.SetColor("_SpecularColor", vmat.specularColor);
                        }
                    }
                    else if (propname.ToLower() == "waveamplitude")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            mat.SetVector("_GAmplitude", vmat.waveAmplitude);
                        }
                    }
                    else if (propname.ToLower() == "wavefrequency")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            mat.SetVector("_GFrequency", vmat.waveFrequency);
                        }
                    }
                    else if (propname.ToLower() == "wavesteepness")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            mat.SetVector("_GSteepness", vmat.waveSteepness);
                        }
                    }
                    else if (propname.ToLower() == "wavespeed")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            mat.SetVector("_GSpeed", vmat.waveSpeed);
                        }
                    }
                    else if (propname.ToLower() == "wavedirectionab")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            mat.SetVector("_GDirectionAB", vmat.waveDirectionAB);
                        }
                    }
                    else if (propname.ToLower() == "wavedirectioncd")
                    {
                        if ( (mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT) )
                        {
                            mat.SetVector("_GDirectionCD", vmat.waveDirectionCD);
                        }
                    }
                    else if (propname.ToLower() == "outlinewidth")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_SKE) || (mat.shader.name.ToLower() == SHAD_PSKE))
                        {
                            mat.SetFloat("_OutlineWidth", vmat.outlinewidth);
                        }
                    }
                    else if (propname.ToLower() == "strokedensity")
                    {
                        if (mat.shader.name.ToLower() == SHAD_PSKE)
                        {
                            mat.SetFloat("_StrokeDensity", vmat.strokedensity);
                        }
                    }
                    else if (propname.ToLower() == "addbrightness")
                    {
                        if (mat.shader.name.ToLower() == SHAD_PSKE)
                        {
                            mat.SetFloat("_AddBrightNess", vmat.addbrightness);
                        }
                    }
                    else if (propname.ToLower() == "multbrightness")
                    {
                        if (mat.shader.name.ToLower() == SHAD_PSKE)
                        {
                            mat.SetFloat("_MultBrightNess", vmat.multbrightness);
                        }
                    }
                    else if (propname.ToLower() == "shadowbrightness")
                    {
                        if (mat.shader.name.ToLower() == SHAD_SKE)
                        {
                            mat.SetFloat("_ShadowBrightNess", vmat.shadowbrightness);
                        }
                    }
                    else if (propname.ToLower() == "enabletextransparent")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_REALTOON) || (mat.shader.name.ToLower() == SHAD_COMIC))
                        {
                            mat.SetFloat("_EnableTextureTransparent", vmat.enableTexTransparent);
                        }
                    }
                    else if (propname.ToLower() == "maincolorinambientlightonly")
                    {
                        if (mat.shader.name.ToLower() == SHAD_REALTOON)
                        {
                            mat.SetFloat("_MCIALO", vmat.mainColorInAmbientLightOnly);
                        }
                    }
                    else if (propname.ToLower() == "doublesided")
                    {
                        if (mat.shader.name.ToLower() == SHAD_REALTOON)
                        {
                            mat.SetInteger("_DoubleSided", vmat.doubleSided);
                        }
                    }
                    else if (propname.ToLower() == "outlinezposcam")
                    {
                        if (mat.shader.name.ToLower() == SHAD_REALTOON)
                        {
                            mat.SetFloat("_OutlineZPostionInCamera", vmat.outlineZPosCam);
                        }
                    }
                    else if (propname.ToLower() == "threshold")
                    {
                        if (mat.shader.name.ToLower() == SHAD_REALTOON)
                        {
                            mat.SetFloat("_SelfShadowThreshold", vmat.thresHold);
                        }
                    }
                    else if (propname.ToLower() == "shadowhardness")
                    {
                        if (mat.shader.name.ToLower() == SHAD_REALTOON)
                        {
                            mat.SetFloat("_ShadowTHardness", vmat.shadowHardness);
                        }
                    }
                    else if (propname.ToLower() == "linewidth")
                    {
                        if (mat.shader.name.ToLower() == SHAD_COMIC)
                        {
                            mat.SetFloat("_LineWidth", vmat.lineWidth);
                        }
                    }
                    else if (propname.ToLower() == "linecolor")
                    {
                        if (mat.shader.name.ToLower() == SHAD_COMIC)
                        {
                            mat.SetColor("_LineColor", vmat.lineColor);
                        }
                    }
                    else if (propname.ToLower() == "tone1threshold")
                    {
                        if (mat.shader.name.ToLower() == SHAD_COMIC)
                        {
                            mat.SetFloat("_Tone1Threshold", vmat.tone1Threshold);
                        }
                    }
                    else if (propname.ToLower() == "icecolor")
                    {
                        if (mat.shader.name.ToLower() == SHAD_ICE)
                        {
                            mat.SetColor("_Color", vmat.iceColor);
                        }
                    }
                    else if (propname.ToLower() == "transparency")
                    {
                        if (mat.shader.name.ToLower() == SHAD_ICE)
                        {
                            mat.SetFloat("_Transparency", vmat.transparency);
                        }
                    }
                    else if (propname.ToLower() == "basetransparency")
                    {
                        if (mat.shader.name.ToLower() == SHAD_ICE)
                        {
                            mat.SetFloat("_BaseTransparency", vmat.baseTransparency);
                        }
                    }
                    else if (propname.ToLower() == "basetransparency")
                    {
                        if (mat.shader.name.ToLower() == SHAD_ICE)
                        {
                            mat.SetFloat("_BaseTransparency", vmat.baseTransparency);
                        }
                    }
                    else if (propname.ToLower() == "iceroughness")
                    {
                        if (mat.shader.name.ToLower() == SHAD_ICE)
                        {
                            mat.SetFloat("_IceRoughness", vmat.iceRoughness);
                        }
                    }
                    else if (propname.ToLower() == "distortion")
                    {
                        if (mat.shader.name.ToLower() == SHAD_ICE)
                        {
                            mat.SetFloat("_Distortion", vmat.distortion);
                        }
                    }
                    else if (propname.ToLower() == "pixelsize")
                    {
                        if (mat.shader.name.ToLower() == SHAD_MICRA)
                        {
                            mat.SetFloat("_PixelSize", vmat.pixelSize);
                        }
                    }
                }
            }
        }
        public void SetUserMaterialFromOuter(string param)
        {
            SetUserMaterial(param);
        }
        /// <summary>
        /// To set material property from HTML
        /// </summary>
        /// <param name="param">0-destination material name [itemname-materialname], 1-parts(shader,color,cullmode,etc),2-value(Standard,VRM/MToon, #FFFFFF)</param>
        public void SetUserMaterial(string param)
        {
            string[] prm = param.Split(',');
            string mat_name = prm[0];
            string propname = prm[1];
            string value = prm[2];
            //Debug.Log("paramater="+mat_name);

            if (userSharedMaterials.ContainsKey(mat_name))
            {
                ManageAnimation manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

                Material mat = userSharedMaterials[mat_name];
                if (mat != null)
                {
                    if (propname.ToLower() == "shader")
                    {

                        Shader target = FindShader(value); // Shader.Find(value);
                        if (target != null)
                        {
                            mat.shader = target;
                        }
                    }
                    else if (propname.ToLower() == "color")
                    {
                        Color col;
                        if (ColorUtility.TryParseHtmlString(value, out col))
                        {
                            //mat.color = col;
                            if ((mat.shader.name.ToLower() == SHAD_STD) || (mat.shader.name.ToLower() == SHAD_VRM))
                            {
                                mat.SetColor("_Color", col);
                            }
                            else if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                            {
                                mat.SetColor("_BaseColor", col);
                            }
                        }
                    }
                    else if (propname.ToLower() == "renderingtype")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_STD)
                        {
                            mat.SetFloat("_Mode", fv);
                        }
                        else if (mat.shader.name.ToLower() == SHAD_VRM)
                        {
                            mat.SetFloat("_BlendMode", fv);
                        }
                        else if (mat.shader.name.ToLower() == SHAD_VRM10)
                        {
                            mat.SetFloat("_AlphaMode", fv);
                        }
                    }
                    else if (propname.ToLower() == "cullmode")
                    { //0 - off, 1 - front, 2 - back
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_VRM)
                        {
                            mat.SetFloat("_CullMode", fv);
                        }
                        else if (mat.shader.name.ToLower() == SHAD_VRM10)
                        {
                            mat.SetFloat("_M_CullMode", fv);
                        }
                    }
                    else if (propname.ToLower() == "alphacutoff")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (
                            (mat.shader.name.ToLower() == SHAD_VRM) ||
                            (mat.shader.name.ToLower() == SHAD_VRM10)
                        )
                        {
                            mat.SetFloat("_Cutoff", fv);
                        }
                    }
                    else if (propname.ToLower() == "maintex")
                    { //texture path or role name
                        /*
                         * Search order:
                         * 1, Camera           = #Camera[Cam_932931229]
                         * 2, Default material = ""
                         * 3, materialPackage  = [material name]
                         * 
                         */
                        if ((userSharedTextureFiles[mat_name].texturePath == value) && (value == ""))
                        {
                            //Debug.Log("texture clear");
                            mat.SetTexture("_MainTex", null);
                            mat.SetTexture("_MainTex", backupTextureFiles[mat_name].realTexture);
                            userSharedTextureFiles[mat_name].texturePath = value;
                            userSharedTextureFiles[mat_name].textureRole = "";
                            userSharedTextureFiles[mat_name].textureIsCamera = 0;
                        }
                        else
                        {
                            if (value.IndexOf(CAMERAROLE) > -1)
                            { //value: #Cameracam_12335667-------------------------------
                                string rolename = value.Replace(CAMERAROLE, "");

                                //---old texture nullize
                                if (userSharedTextureFiles[mat_name].textureIsCamera == 0)
                                { //---old is general texture
                                    manim.UnReferMaterial(OneMaterialType.Texture, userSharedTextureFiles[mat_name].texturePath);
                                }
                                mat.SetTexture("_MainTex", null);

                                //---set new texture
                                if (rolename != "")
                                {
                                    SetRenderTextureFromCamera(mat, rolename);
                                    userSharedTextureFiles[mat_name].texturePath = value;
                                    userSharedTextureFiles[mat_name].textureRole = rolename;
                                    userSharedTextureFiles[mat_name].textureIsCamera = 1;
                                }

                            }
                            else if (value == "")
                            { //---recover default texture
                                //---old texture nullize
                                if (userSharedTextureFiles[mat_name].textureIsCamera == 0)
                                { //---old is general texture
                                    manim.UnReferMaterial(OneMaterialType.Texture, userSharedTextureFiles[mat_name].texturePath);
                                }
                                //Debug.Log("  " + mat_name);
                                mat.SetTexture("_MainTex", null);
                                mat.SetTexture("_MainTex", backupTextureFiles[mat_name].realTexture);
                                userSharedTextureFiles[mat_name].texturePath = value;
                                userSharedTextureFiles[mat_name].textureRole = "";
                                userSharedTextureFiles[mat_name].textureIsCamera = 0;
                            }
                            else
                            { // value: materialManager name ---------------------------
                                /*
                                string[] path = value.Split('\t');
                                if (path.Length > 1)
                                {
                                    StartCoroutine(LoadImageuri(path[1], mat));
                                    userSharedTextureFiles[mat_name] = value;
                                }
                                */
                                //---new version
                                //value: NativeAP_OneMaterial.name-------------------------------
                                //---nullize old texture
                                if (userSharedTextureFiles[mat_name].textureIsCamera == 0)
                                {
                                    manim.UnReferMaterial(OneMaterialType.Texture, userSharedTextureFiles[mat_name].texturePath);
                                }
                                mat.SetTexture("_MainTex", null);

                                //---set new texture
                                NativeAP_OneMaterial nap = manim.FindTexture(value);
                                if (nap != null)
                                {
                                    //Debug.Log(nap.materialType.ToString() + "/" + nap.name);
                                    mat.SetTexture("_MainTex", nap.ReferTexture2D());
                                    userSharedTextureFiles[mat_name].texturePath = value;
                                    userSharedTextureFiles[mat_name].textureRole = "";
                                    userSharedTextureFiles[mat_name].textureIsCamera = 0;
                                }
                            }
                        }


                    }
                    else if (propname.ToLower() == "metallic")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_STD)
                        {
                            mat.SetFloat("_Metallic", fv);
                        }
                    }
                    else if (propname.ToLower() == "glossiness")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_STD)
                        {
                            mat.SetFloat("_Glossiness", fv);
                        }
                    }
                    else if (propname.ToLower() == "emissioncolor")
                    {
                        Color col;
                        if (ColorUtility.TryParseHtmlString(value, out col))
                        {
                            mat.EnableKeyword("EMISSION");
                            mat.SetColor("_EmissionColor", col);
                        }
                    }
                    else if (propname.ToLower() == "shadetexcolor")
                    {
                        if (
                            (mat.shader.name.ToLower() == SHAD_VRM) ||
                            (mat.shader.name.ToLower() == SHAD_VRM10)
                        )
                        {
                            Color col;
                            if (ColorUtility.TryParseHtmlString(value, out col))
                            {
                                mat.SetColor("_ShadeColor", col);
                            }
                        }
                    }
                    else if (propname.ToLower() == "shadingtoony")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_VRM)
                        {
                            mat.SetFloat("_ShadeToony", fv);
                        }
                        else if (mat.shader.name.ToLower() == SHAD_VRM10)
                        {
                            mat.SetFloat("_ShadingToonyFactor", fv);
                        }
                    }
                    else if (propname.ToLower() == "shadingshift")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_VRM)
                        {
                            mat.SetFloat("_ShadeShift", fv);
                        }
                        else if (mat.shader.name.ToLower() == SHAD_VRM10)
                        {
                            mat.SetFloat("_ShadingShiftFactor", fv);
                        }
                    }
                    else if (propname.ToLower() == "receiveshadow")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_VRM)
                        {
                            mat.SetFloat("_ReceiveShadowRate", fv);
                        }
                    }
                    else if (propname.ToLower() == "shadinggrade")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_VRM)
                        {
                            mat.SetFloat("_ShadingGradeRate", fv);
                        }
                    }
                    else if (propname.ToLower() == "lightcolorattenuation")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_VRM)
                        {
                            mat.SetFloat("_LightColorAttenuation", fv);
                        }
                    }
                    else if (propname.ToLower() == "rimcolor")
                    {
                        if (
                            (mat.shader.name.ToLower() == SHAD_VRM) ||
                            (mat.shader.name.ToLower() == SHAD_VRM10)
                        )
                        {
                            Color col;
                            if (ColorUtility.TryParseHtmlString(value, out col))
                            {
                                mat.SetColor("_RimColor", col);
                            }
                        }
                    }
                    else if (propname.ToLower() == "rimfresnel")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (
                            (mat.shader.name.ToLower() == SHAD_VRM) ||
                            (mat.shader.name.ToLower() == SHAD_VRM10)
                        )
                        {
                            mat.SetFloat("_RimFresnelPower", fv);
                        }
                    }
                    else if (propname.ToLower() == "srcblend")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (
                            (mat.shader.name.ToLower() == SHAD_VRM)
                        )
                        {
                            mat.SetFloat("_SrcBlend", fv);
                        }
                        else if (mat.shader.name.ToLower() == SHAD_VRM10)
                        {
                            mat.SetFloat("_M_SrcBlend", fv);
                        }
                    }
                    else if (propname.ToLower() == "dstblend")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (
                            (mat.shader.name.ToLower() == SHAD_VRM)
                        )
                        {
                            mat.SetFloat("_DstBlend", fv);
                        }
                        else if (mat.shader.name.ToLower() == SHAD_VRM10)
                        {
                            mat.SetFloat("_M_DstBlend", fv);
                        }
                    }
                    else if (propname.ToLower() == "fresnelscale")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            mat.SetFloat("_FresnelScale", fv);
                        }
                    }
                    else if (propname.ToLower() == "reflectioncolor")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            Color col;
                            if (ColorUtility.TryParseHtmlString(value, out col))
                            {
                                mat.SetColor("_ReflectionColor", col);
                            }
                            
                        }
                    }
                    else if (propname.ToLower() == "specularcolor")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            Color col;
                            if (ColorUtility.TryParseHtmlString(value, out col))
                            {
                                mat.SetColor("_SpecularColor", col);
                            }
                        }
                    }
                    else if (propname.ToLower() == "waveamplitude")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            string[] arr = value.Split("\t");
                            float x = float.TryParse(arr[0], out x) ? x : 0f;
                            float y = float.TryParse(arr[1], out y) ? y : 0f;
                            float z = float.TryParse(arr[2], out z) ? z : 0f;
                            float w = float.TryParse(arr[3], out w) ? w : 0f;
                            Vector4 vec = new Vector4(x, y, z, w);
                            mat.SetVector("_GAmplitude", vec);
                        }
                    }
                    else if (propname.ToLower() == "wavefrequency")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            string[] arr = value.Split("\t");
                            float x = float.TryParse(arr[0], out x) ? x : 0f;
                            float y = float.TryParse(arr[1], out y) ? y : 0f;
                            float z = float.TryParse(arr[2], out z) ? z : 0f;
                            float w = float.TryParse(arr[3], out w) ? w : 0f;
                            Vector4 vec = new Vector4(x, y, z, w);
                            mat.SetVector("_GFrequency", vec);
                        }
                    }
                    else if (propname.ToLower() == "wavesteepness")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            string[] arr = value.Split("\t");
                            float x = float.TryParse(arr[0], out x) ? x : 0f;
                            float y = float.TryParse(arr[1], out y) ? y : 0f;
                            float z = float.TryParse(arr[2], out z) ? z : 0f;
                            float w = float.TryParse(arr[3], out w) ? w : 0f;
                            Vector4 vec = new Vector4(x, y, z, w);
                            mat.SetVector("_GSteepness", vec);
                        }
                    }
                    else if (propname.ToLower() == "wavespeed")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            string[] arr = value.Split("\t");
                            float x = float.TryParse(arr[0], out x) ? x : 0f;
                            float y = float.TryParse(arr[1], out y) ? y : 0f;
                            float z = float.TryParse(arr[2], out z) ? z : 0f;
                            float w = float.TryParse(arr[3], out w) ? w : 0f;
                            Vector4 vec = new Vector4(x, y, z, w);
                            mat.SetVector("_GSpeed", vec);
                        }
                    }
                    else if (propname.ToLower() == "wavedirectionab")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            string[] arr = value.Split("\t");
                            float x = float.TryParse(arr[0], out x) ? x : 0f;
                            float y = float.TryParse(arr[1], out y) ? y : 0f;
                            float z = float.TryParse(arr[2], out z) ? z : 0f;
                            float w = float.TryParse(arr[3], out w) ? w : 0f;
                            Vector4 vec = new Vector4(x, y, z, w);
                            mat.SetVector("_GDirectionAB", vec);
                        }
                    }
                    else if (propname.ToLower() == "wavedirectioncd")
                    {
                        if ((mat.shader.name.ToLower() == SHAD_WT) || (mat.shader.name.ToLower() == SHAD_SWT))
                        {
                            string[] arr = value.Split("\t");
                            float x = float.TryParse(arr[0], out x) ? x : 0f;
                            float y = float.TryParse(arr[1], out y) ? y : 0f;
                            float z = float.TryParse(arr[2], out z) ? z : 0f;
                            float w = float.TryParse(arr[3], out w) ? w : 0f;
                            Vector4 vec = new Vector4(x, y, z, w);
                            mat.SetVector("_GDirectionCD", vec);
                        }
                    }
                    else if (propname.ToLower() == "outlinewidth")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if ((mat.shader.name.ToLower() == SHAD_SKE) || (mat.shader.name.ToLower() == SHAD_PSKE))
                        {
                            mat.SetFloat("_OutlineWidth", fv);
                        }
                    }
                    else if (propname.ToLower() == "strokedensity")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_PSKE)
                        {
                            mat.SetFloat("_StrokeDensity", fv);
                        }
                    }
                    else if (propname.ToLower() == "addbrightness")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_PSKE)
                        {
                            mat.SetFloat("_AddBrightNess", fv);
                        }
                    }
                    else if (propname.ToLower() == "multbrightness")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_PSKE)
                        {
                            mat.SetFloat("_MultBrightNess", fv);
                        }
                    }
                    else if (propname.ToLower() == "shadowbrightness")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_SKE)
                        {
                            mat.SetFloat("_ShadowBrightNess", fv);
                        }
                    }
                    else if (propname.ToLower() == "enabletextransparent")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if ((mat.shader.name.ToLower() == SHAD_REALTOON) || (mat.shader.name.ToLower() == SHAD_COMIC))
                        {
                            mat.SetFloat("_EnableTextureTransparent", fv);
                        }
                    }
                    else if (propname.ToLower() == "maincolorinambientlightonly")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_REALTOON)
                        {
                            mat.SetFloat("_MCIALO", fv);
                        }
                    }
                    else if (propname.ToLower() == "doublesided")
                    {
                        int fv = int.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_REALTOON)
                        {
                            mat.SetInteger("_DoubleSided", fv);
                        }
                    }
                    else if (propname.ToLower() == "outlinezposcam")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_REALTOON)
                        {
                            mat.SetFloat("_OutlineZPostionInCamera", fv);
                        }
                    }
                    else if (propname.ToLower() == "threshold")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_REALTOON)
                        {
                            mat.SetFloat("_SelfShadowThreshold", fv);
                        }
                    }
                    else if (propname.ToLower() == "shadowhardness")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_REALTOON)
                        {
                            mat.SetFloat("_ShadowTHardness", fv);
                        }
                    }
                    else if (propname.ToLower() == "linewidth")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_COMIC)
                        {
                            mat.SetFloat("_LineWidth", fv);
                        }
                    }
                    else if (propname.ToLower() == "linecolor")
                    {
                        if (mat.shader.name.ToLower() == SHAD_COMIC)
                        {
                            Color col;
                            if (ColorUtility.TryParseHtmlString(value, out col))
                            {
                                mat.SetColor("_LineColor", col);
                            }
                        }
                    }
                    else if (propname.ToLower() == "tone1threshold")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_COMIC)
                        {
                            mat.SetFloat("_Tone1Threshold", fv);
                        }
                    }
                    else if (propname.ToLower() == "icecolor")
                    {
                        if (mat.shader.name.ToLower() == SHAD_ICE)
                        {
                            Color col;
                            if (ColorUtility.TryParseHtmlString(value, out col))
                            {
                                mat.SetColor("_Color", col);
                            }
                        }
                    }
                    else if (propname.ToLower() == "transparency")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_ICE)
                        {
                            mat.SetFloat("_Transparency", fv);
                        }
                    }
                    else if (propname.ToLower() == "basetransparency")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_ICE)
                        {
                            mat.SetFloat("_BaseTransparency", fv);
                        }
                    }
                    else if (propname.ToLower() == "basetransparency")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_ICE)
                        {
                            mat.SetFloat("_BaseTransparency", fv);
                        }
                    }
                    else if (propname.ToLower() == "iceroughness")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_ICE)
                        {
                            mat.SetFloat("_IceRoughness", fv);
                        }
                    }
                    else if (propname.ToLower() == "distortion")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_ICE)
                        {
                            mat.SetFloat("_Distortion", fv);
                        }
                    }
                    else if (propname.ToLower() == "pixelsize")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == SHAD_MICRA)
                        {
                            mat.SetFloat("_PixelSize", fv);
                        }
                    }

                    //---back up after to set raw property.
                    MaterialProperties matp = GetUserMaterialObject(mat_name, mat);
                    SetTextureConfig(mat_name, matp);
                }
            }

        }

        /// <summary>
        /// To set material motion information to DOTween 
        /// </summary>
        /// <param name="seq"></param>
        /// <param name="mat_name"></param>
        /// <param name="value"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public virtual Sequence SetMaterialTween(Sequence seq, string mat_name, MaterialProperties value, float duration)
        {
            if (userSharedMaterials.ContainsKey(mat_name))
            {
                Material mat = userSharedMaterials[mat_name];
                if (mat != null)
                {
                    //---change only shader
                    if (mat.shader.name != value.shaderName)
                    {
                        seq.Join(DOVirtual.DelayedCall(duration, () =>
                        {
                            Shader target = FindShader(value.shaderName); //Shader.Find(value.shaderName);
                            if (target != null)
                            {
                                mat.shader = target;
                                //---once direct apply, after changing shader.
                                SetMaterialTween(seq, mat_name, value, duration);
                            }
                        }, false));
                    }

                    //---each properties
                    if (value.shaderName.ToLower() == SHAD_STD)
                    {
                        


                        if (mat.HasProperty("_Color")) seq.Join(mat.DOColor(value.color, duration));
                        if (mat.HasProperty("_Mode")) seq.Join(mat.DOFloat(value.blendmode, "_Mode", duration));
                        if (mat.HasProperty("_Metallic")) seq.Join(mat.DOFloat(value.metallic, "_Metallic", duration));
                        if (mat.HasProperty("_Glossiness")) seq.Join(mat.DOFloat(value.glossiness, "_Glossiness", duration));

                        seq.Join(DOVirtual.DelayedCall(duration, () => mat.EnableKeyword("EMISSION"), false));
                        if (mat.HasProperty("_EmissionColor")) seq.Join(mat.DOColor(value.emissioncolor, "_EmissionColor", duration));

                    }
                    else if (value.shaderName.ToLower() == SHAD_VRM)
                    {
                        


                        if (mat.HasProperty("_Color")) seq.Join(mat.DOColor(value.color, duration));
                        if (mat.HasProperty("_BlendMode")) seq.Join(mat.DOFloat(value.blendmode, "_BlendMode", duration));  //For UniVRM0.x
                        if (mat.HasProperty("_CullMode")) seq.Join(mat.DOFloat(value.cullmode, "_CullMode", duration));
                        if (mat.HasProperty("_ShadeColor")) seq.Join(mat.DOColor(value.shadetexcolor, "_ShadeColor", duration));
                        if (mat.HasProperty("_ShadeToony")) seq.Join(mat.DOFloat(value.shadingtoony, "_ShadeToony", duration)); //For UniVRM0.x
                        if (mat.HasProperty("_RimColor")) seq.Join(mat.DOColor(value.rimcolor, "_RimColor", duration));
                        if (mat.HasProperty("_RimFresnelPower")) seq.Join(mat.DOFloat(value.rimfresnel, "_RimFresnelPower", duration));
                        if (mat.HasProperty("_SrcBlend")) seq.Join(mat.DOFloat(value.srcblend, "_SrcBlend", duration));
                        if (mat.HasProperty("_DstBlend")) seq.Join(mat.DOFloat(value.dstblend, "_DstBlend", duration));
                        if (mat.HasProperty("_Cutoff")) seq.Join(mat.DOFloat(value.cutoff, "_Cutoff", duration));
                        if (mat.HasProperty("_ShadeShift")) seq.Join(mat.DOFloat(value.shadingshift, "_ShadeShift", duration)); //For UniVRM0.x
                        if (mat.HasProperty("_ReceiveShadowRate")) seq.Join(mat.DOFloat(value.receiveshadow, "_ReceiveShadowRate", duration)); //For UniVRM0.x
                        if (mat.HasProperty("_ShadingGradeRate")) seq.Join(mat.DOFloat(value.shadinggrade, "_ShadingGradeRate", duration)); //For UniVRM0.x
                        if (mat.HasProperty("_LightColorAttenuation")) seq.Join(mat.DOFloat(value.shadinggrade, "_LightColorAttenuation", duration));

                        seq.Join(DOVirtual.DelayedCall(duration, () => mat.EnableKeyword("EMISSION"), false));
                        if (mat.HasProperty("_EmissionColor")) seq.Join(mat.DOColor(value.emissioncolor, "_EmissionColor", duration));
                    }
                    else if (value.shaderName.ToLower() == SHAD_VRM10)
                    {
                        


                        if (mat.HasProperty("_Color")) seq.Join(mat.DOColor(value.color, duration));
                        if (mat.HasProperty("_AlphaMode")) seq.Join(mat.DOFloat(value.blendmode, "_AlphaMode", duration));  //For VRM1.0
                        if (mat.HasProperty("_M_CullMode")) seq.Join(mat.DOFloat(value.cullmode, "_M_CullMode", duration));
                        if (mat.HasProperty("_ShadeColor")) seq.Join(mat.DOColor(value.shadetexcolor, "_ShadeColor", duration));
                        if (mat.HasProperty("_ShadingToonyFactor")) seq.Join(mat.DOFloat(value.shadingtoony, "_ShadingToonyFactor", duration)); //For VRM1.0
                        if (mat.HasProperty("_RimColor")) seq.Join(mat.DOColor(value.rimcolor, "_RimColor", duration));
                        if (mat.HasProperty("_RimFresnelPower")) seq.Join(mat.DOFloat(value.rimfresnel, "_RimFresnelPower", duration));
                        if (mat.HasProperty("_M_SrcBlend")) seq.Join(mat.DOFloat(value.srcblend, "_M_SrcBlend", duration));
                        if (mat.HasProperty("_M_DstBlend")) seq.Join(mat.DOFloat(value.dstblend, "_M_DstBlend", duration));
                        if (mat.HasProperty("_Cutoff")) seq.Join(mat.DOFloat(value.cutoff, "_Cutoff", duration));
                        if (mat.HasProperty("_ShadingShiftFactor")) seq.Join(mat.DOFloat(value.shadingshift, "_ShadingShiftFactor", duration)); //For VRM1.0
                        //if (mat.HasProperty("_LightColorAttenuation")) seq.Join(mat.DOFloat(value.shadinggrade, "_LightColorAttenuation", duration));

                        seq.Join(DOVirtual.DelayedCall(duration, () => mat.EnableKeyword("EMISSION"), false));
                        if (mat.HasProperty("_EmissionColor")) seq.Join(mat.DOColor(value.emissioncolor, "_EmissionColor", duration));

                    }
                    else if ((value.shaderName.ToLower() == SHAD_WT) || (value.shaderName.ToLower() == SHAD_SWT))
                    {
                        

                        if (mat.HasProperty("_FresnelScale")) seq.Join(mat.DOFloat(value.fresnelScale, "_FresnelScale", duration));
                        if (mat.HasProperty("_BaseColor")) seq.Join(mat.DOColor(value.color, "_BaseColor", duration));
                        if (mat.HasProperty("_ReflectionColor")) seq.Join(mat.DOColor(value.reflectionColor, "_ReflectionColor", duration));
                        if (mat.HasProperty("_SpecularColor")) seq.Join(mat.DOColor(value.specularColor, "_SpecularColor", duration));
                        if (mat.HasProperty("_GAmplitude")) seq.Join(mat.DOVector(value.waveAmplitude, "_GAmplitude", duration));
                        if (mat.HasProperty("_GFrequency")) seq.Join(mat.DOVector(value.waveFrequency, "_GFrequency", duration));
                        if (mat.HasProperty("_GSteepness")) seq.Join(mat.DOVector(value.waveSteepness, "_GSteepness", duration));
                        if (mat.HasProperty("_GSpeed")) seq.Join(mat.DOVector(value.waveSpeed, "_GSpeed", duration));
                        if (mat.HasProperty("_GDirectionAB")) seq.Join(mat.DOVector(value.waveDirectionAB, "_GDirectionAB", duration));
                        if (mat.HasProperty("_GDirectionCD")) seq.Join(mat.DOVector(value.waveDirectionCD, "_GDirectionCD", duration));
                    }
                    else if ((value.shaderName.ToLower() == SHAD_SKE) || (value.shaderName.ToLower() == SHAD_PSKE))
                    {
                        

                        if (mat.HasProperty("_OutlineWidth")) seq.Join(mat.DOFloat(value.outlinewidth, "_OutlineWidth", duration));
                        if (mat.shader.name.ToLower() == SHAD_PSKE)
                        {
                            if (mat.HasProperty("_StrokeDensity")) seq.Join(mat.DOFloat(value.strokedensity, "_StrokeDensity", duration));
                            if (mat.HasProperty("_AddBrightNess")) seq.Join(mat.DOFloat(value.addbrightness, "_AddBrightNess", duration));
                            if (mat.HasProperty("_MultBrightNess")) seq.Join(mat.DOFloat(value.multbrightness, "_MultBrightNess", duration));
                        }
                        if (mat.HasProperty("_ShadowBrightNess")) seq.Join(mat.DOFloat(value.shadowbrightness, "_ShadowBrightNess", duration));

                    }
                    else if (value.shaderName.ToLower() == SHAD_REALTOON)
                    {
                        

                        if (mat.HasProperty("_EnableTextureTransparent")) seq.Join(mat.DOFloat(value.enableTexTransparent, "_EnableTextureTransparent", duration));
                        if (mat.HasProperty("_MCIALO")) seq.Join(mat.DOFloat(value.mainColorInAmbientLightOnly, "_MCIALO", duration));
                        if (mat.HasProperty("_DoubleSided")) seq.Join(mat.DOFloat(value.doubleSided, "_DoubleSided", duration));
                        if (mat.HasProperty("_OutlineZPostionInCamera")) seq.Join(mat.DOFloat(value.outlineZPosCam, "_OutlineZPostionInCamera", duration));
                        if (mat.HasProperty("_SelfShadowThreshold")) seq.Join(mat.DOFloat(value.thresHold, "_SelfShadowThreshold", duration));
                        if (mat.HasProperty("_ShadowTHardness")) seq.Join(mat.DOFloat(value.shadowHardness, "_ShadowTHardness", duration));
                    }
                    else if (value.shaderName.ToLower() == SHAD_COMIC)
                    {
                        

                        if (mat.HasProperty("_EnableTextureTransparent")) seq.Join(mat.DOFloat(value.enableTexTransparent, "_EnableTextureTransparent", duration));
                        if (mat.HasProperty("_LineWidth")) seq.Join(mat.DOFloat(value.lineWidth, "_LineWidth", duration));
                        if (mat.HasProperty("_LineColor")) seq.Join(mat.DOColor(value.lineColor, "_LineColor", duration));
                        if (mat.HasProperty("_Tone1Threshold")) seq.Join(mat.DOFloat(value.tone1Threshold, "_Tone1Threshold", duration));

                    }
                    else if (value.shaderName.ToLower() == SHAD_ICE)
                    {
                        

                        if (mat.HasProperty("_Color")) seq.Join(mat.DOColor(value.iceColor, "_Color", duration));
                        if (mat.HasProperty("_Transparency")) seq.Join(mat.DOFloat(value.transparency, "_Transparency", duration));
                        if (mat.HasProperty("_BaseTransparency")) seq.Join(mat.DOFloat(value.baseTransparency, "_BaseTransparency", duration));
                        if (mat.HasProperty("_IceRoughness")) seq.Join(mat.DOFloat(value.iceRoughness, "_IceRoughness", duration));
                        if (mat.HasProperty("_Distortion")) seq.Join(mat.DOFloat(value.distortion, "_Distortion", duration));

                    }
                    else if (value.shaderName.ToLower() == SHAD_MICRA)
                    {
                        
                        

                        if (mat.HasProperty("_PixelSize")) seq.Join(mat.DOFloat(value.pixelSize, "_PixelSize", duration));

                    }
                }
            }


            return seq;
        }

        /// <summary>
        /// Set to backup normally NON-animate (when PREVIEW only)
        /// </summary>
        /// <param name="mat_name">GameObject name</param>
        /// <param name="value">target MaterialProperties</param>
        /// <param name="isClone"></param>
        public void SetTextureConfig(string mat_name, MaterialProperties value, bool isClone = false)
        {
            int inx = backup_userMaterialProperties.FindIndex(item =>
            {
                if (item.name == value.name)  return true;
                return false;
            });
            if (inx != -1)
            {
                if (isClone)
                {
                    backup_userMaterialProperties[inx] = value.Clone();
                }
                else
                {
                    backup_userMaterialProperties[inx] = value;
                }
                
                
            }
        }
    }
}
