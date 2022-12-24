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

        protected List<NativeAnimationAvatar> copyToList;

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
        [SerializeField]
        public Dictionary<string, Material> userSharedMaterials;
        //---property saver of material
        [SerializeField]
        protected Dictionary<string, MaterialProperties> userSharedTextureFiles;
        [SerializeField]
        protected Dictionary<string, MaterialProperties> backupTextureFiles;

        private void Awake()
        {
            effectPunch = new AvatarPunchEffect();
            effectShake = new AvatarShakeEffect();
            jumpNum = 0;
            jumpPower = 1f;

        }
        // Start is called before the first frame update
        void Start()
        {
            SaveDefaultTransform(true, true);

            copyToList = new List<NativeAnimationAvatar>();
        }

        // Update is called once per frame
        void Update()
        {

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

        }

        /// <summary>
        /// To write real Material to List of MaterialProperties
        /// </summary>
        /// <returns>packed Listof MaterialProperties</returns>
        public virtual List<MaterialProperties> ListUserMaterialObject()
        {
            List<MaterialProperties> ret = new List<MaterialProperties>();

            foreach (KeyValuePair<string, Material> kvp in userSharedMaterials)
            {
                Material mat = kvp.Value;

                MaterialProperties matp = new MaterialProperties();

                matp.name = kvp.Key;
                matp.matName = mat.name;
                
                matp.shaderName = mat.shader.name;
                
                if (mat.shader.name.ToLower() == "vrm/mtoon")
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

                    matp.texturePath = userSharedTextureFiles[kvp.Key].texturePath;
                    matp.textureRole = userSharedTextureFiles[kvp.Key].textureRole;
                    matp.textureIsCamera = userSharedTextureFiles[kvp.Key].textureIsCamera;

                }
                else if (mat.shader.name.ToLower() == "standard")
                {
                    matp.color = mat.color;
                    matp.emissioncolor = mat.GetColor("_EmissionColor");

                    matp.cullmode = 0;
                    matp.blendmode = mat.GetFloat("_Mode");
                    matp.metallic = mat.GetFloat("_Metallic");
                    matp.glossiness = mat.GetFloat("_Glossiness");

                    matp.texturePath = userSharedTextureFiles[kvp.Key].texturePath;
                    matp.textureRole = userSharedTextureFiles[kvp.Key].textureRole;
                    matp.textureIsCamera = userSharedTextureFiles[kvp.Key].textureIsCamera;

                }
                else if ( (mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4") )
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
                ret.Add(ListGetOneUserMaterial(matlst.Current.Key));
            }

            return ret;
        }
        public virtual void ListUserMaterialFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            List<string> list = ListUserMaterial();
            string js = string.Join("\r\n", list.ToArray());
            ReceiveStringVal(js);
#endif
        }

        /// <summary>
        /// To write 1 - material to csv-string 
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public virtual string ListGetOneUserMaterial(string param)
        {
            const string SEPSTR = "=";
            string ret = "";

            //Debug.Log("param=" + param);
            //Debug.Log(userSharedMaterials.ContainsKey(param));

            if (userSharedMaterials.ContainsKey(param))
            {
                Material mat = userSharedMaterials[param];
                //Debug.Log("material name=" + mat.name);
                string texturePath = userSharedTextureFiles[param].texturePath;

                if (mat.shader.name.ToLower() == "vrm/mtoon")
                {
                    ret = (
                        param + SEPSTR +
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
                else if (mat.shader.name.ToLower() == "standard")
                {
                    ret = (
                        param + SEPSTR +
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
                else if ( (mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4") )
                {
                    Vector4 wa = mat.GetVector("_GAmplitude");
                    Vector4 wf = mat.GetVector("_GFrequency");
                    Vector4 wt = mat.GetVector("_GSteepness");
                    Vector4 ws = mat.GetVector("_GSpeed");
                    Vector4 wdab = mat.GetVector("_GDirectionAB");
                    Vector4 wdcd = mat.GetVector("_GDirectionCD");
                    ret = (
                        param + SEPSTR +
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
        public virtual void ListGetOneUserMaterialFromOuter(string param)
        {
            string ret = ListGetOneUserMaterial(param);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }

        /// <summary>
        /// To set material property from Unity
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
                        Shader target = Shader.Find(vmat.shaderName);
                        if (target != null)
                        {
                            mat.shader = target;
                        }
                    }
                    else if (propname.ToLower() == "color")
                    {
                        if ( (mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4") )
                        {
                            mat.SetColor("_BaseColor", vmat.color);
                        }
                        else if ((mat.shader.name.ToLower() == "standard") || (mat.shader.name.ToLower() == "vrm/mtoon"))
                        {
                            mat.SetColor("_Color", vmat.color);
                        }
                        
                    }
                    else if (propname.ToLower() == "renderingtype")
                    {
                        if (mat.shader.name.ToLower() == "standard")
                        {
                            mat.SetFloat("_Mode", vmat.blendmode);
                        }
                        else if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_BlendMode", vmat.blendmode);
                        }
                    }
                    else if (propname.ToLower() == "cullmode")
                    { //0 - off, 1 - front, 2 - back
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_CullMode", vmat.cullmode);
                        }
                    }
                    else if (propname.ToLower() == "alphacutoff")
                    {
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
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
                        if (mat.shader.name.ToLower() == "standard")
                        {
                            mat.SetFloat("_Metallic", vmat.metallic);
                        }
                    }
                    else if (propname.ToLower() == "glossiness")
                    {
                        if (mat.shader.name.ToLower() == "standard")
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
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetColor("_ShadeColor", vmat.shadetexcolor);
                        }
                    }
                    else if (propname.ToLower() == "shadingtoony")
                    {
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_ShadeToony", vmat.shadingtoony);
                        }   
                    }
                    else if (propname.ToLower() == "shadingshift")
                    {
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_ShadeShift", vmat.shadingshift);
                        }
                    }
                    else if (propname.ToLower() == "receiveshadow")
                    {
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_ReceiveShadowRate", vmat.receiveshadow);
                        }
                    }
                    else if (propname.ToLower() == "shadinggrade")
                    {
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_ShadingGradeRate", vmat.shadinggrade);
                        }
                    }
                    else if (propname.ToLower() == "lightcolorattenuation")
                    {
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_LightColorAttenuation", vmat.lightcolorattenuation);
                        }
                    }
                    else if (propname.ToLower() == "rimcolor")
                    {
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetColor("_RimColor", vmat.rimcolor);
                        }
                    }
                    else if (propname.ToLower() == "rimfresnel")
                    {
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_RimFresnelPower", vmat.rimfresnel);
                        }                        
                    }
                    else if (propname.ToLower() == "srcblend")
                    {
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_SrcBlend", vmat.srcblend);
                        }
                    }
                    else if (propname.ToLower() == "dstblend")
                    {
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_DstBlend", vmat.dstblend);
                        }
                    }
                    else if (propname.ToLower() == "fresnelscale")
                    {
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
                        {
                            mat.SetFloat("_FresnelScale", vmat.fresnelScale);
                        }
                    }
                    else if (propname.ToLower() == "reflectioncolor")
                    {
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
                        {
                            mat.SetColor("_ReflectionColor", vmat.reflectionColor);
                        }
                    }
                    else if (propname.ToLower() == "specularcolor")
                    {
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
                        {
                            mat.SetColor("_SpecularColor", vmat.specularColor);
                        }
                    }
                    else if (propname.ToLower() == "waveamplitude")
                    {
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
                        {
                            mat.SetVector("_GAmplitude", vmat.waveAmplitude);
                        }
                    }
                    else if (propname.ToLower() == "wavefrequency")
                    {
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
                        {
                            mat.SetVector("_GFrequency", vmat.waveFrequency);
                        }
                    }
                    else if (propname.ToLower() == "wavesteepness")
                    {
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
                        {
                            mat.SetVector("_GSteepness", vmat.waveSteepness);
                        }
                    }
                    else if (propname.ToLower() == "wavespeed")
                    {
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
                        {
                            mat.SetVector("_GSpeed", vmat.waveSpeed);
                        }
                    }
                    else if (propname.ToLower() == "wavedirectionab")
                    {
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
                        {
                            mat.SetVector("_GDirectionAB", vmat.waveDirectionAB);
                        }
                    }
                    else if (propname.ToLower() == "wavedirectioncd")
                    {
                        if ( (mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4") )
                        {
                            mat.SetVector("_GDirectionCD", vmat.waveDirectionCD);
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
                        Shader target = Shader.Find(value);
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
                            if ((mat.shader.name.ToLower() == "standard") || (mat.shader.name.ToLower() == "vrm/mtoon"))
                            {
                                mat.SetColor("_Color", col);
                            }
                            else if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
                            {
                                mat.SetColor("_BaseColor", col);
                            }
                        }
                    }
                    else if (propname.ToLower() == "renderingtype")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == "standard")
                        {
                            mat.SetFloat("_Mode", fv);
                        }
                        else if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_BlendMode", fv);
                        }
                    }
                    else if (propname.ToLower() == "cullmode")
                    { //0 - off, 1 - front, 2 - back
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_CullMode", fv);
                        }
                    }
                    else if (propname.ToLower() == "alphacutoff")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
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
                        if (mat.shader.name.ToLower() == "standard")
                        {
                            mat.SetFloat("_Metallic", fv);
                        }
                    }
                    else if (propname.ToLower() == "glossiness")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == "standard")
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
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
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
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_ShadeToony", fv);
                        }
                    }
                    else if (propname.ToLower() == "shadingshift")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_ShadeShift", fv);
                        }
                    }
                    else if (propname.ToLower() == "receiveshadow")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_ReceiveShadowRate", fv);
                        }
                    }
                    else if (propname.ToLower() == "shadinggrade")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_ShadingGradeRate", fv);
                        }
                    }
                    else if (propname.ToLower() == "lightcolorattenuation")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_LightColorAttenuation", fv);
                        }
                    }
                    else if (propname.ToLower() == "rimcolor")
                    {
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
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
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_RimFresnelPower", fv);
                        }
                    }
                    else if (propname.ToLower() == "srcblend")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_SrcBlend", fv);
                        }
                    }
                    else if (propname.ToLower() == "dstblend")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            mat.SetFloat("_DstBlend", fv);
                        }
                    }
                    else if (propname.ToLower() == "fresnelscale")
                    {
                        float fv = float.TryParse(value, out fv) ? fv : 0;
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
                        {
                            mat.SetFloat("_FresnelScale", fv);
                        }
                    }
                    else if (propname.ToLower() == "reflectioncolor")
                    {
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
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
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
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
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
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
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
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
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
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
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
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
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
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
                        if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
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
                    
                    if (value.shaderName.ToLower() == "standard")
                    {
                        seq.Join(DOVirtual.DelayedCall(duration, () =>
                        {
                            Shader target = Shader.Find(value.shaderName);
                            if (target != null)
                            {
                                mat.shader = target;
                                SetUserMaterial(mat_name + ",maintex," + value.texturePath);
                            }
                        }, false));


                        if (mat.HasProperty("_Color")) seq.Join(mat.DOColor(value.color, duration));
                        if (mat.HasProperty("_Mode")) seq.Join(mat.DOFloat(value.blendmode, "_Mode", duration));
                        if (mat.HasProperty("_Metallic")) seq.Join(mat.DOFloat(value.metallic, "_Metallic", duration));
                        if (mat.HasProperty("_Glossiness")) seq.Join(mat.DOFloat(value.glossiness, "_Glossiness", duration));

                        seq.Join(DOVirtual.DelayedCall(duration, () => mat.EnableKeyword("EMISSION"), false));
                        if (mat.HasProperty("_EmissionColor")) seq.Join(mat.DOColor(value.emissioncolor, "_EmissionColor", duration));

                    }
                    else if (value.shaderName.ToLower() == "vrm/mtoon")
                    {
                        seq.Join(DOVirtual.DelayedCall(duration, () =>
                        {
                            Shader target = Shader.Find(value.shaderName);
                            if (target != null)
                            {
                                mat.shader = target;
                                //Debug.Log("sequence join, delay, texture=" + value.texturePath);
                                SetUserMaterial(mat_name + ",maintex," + value.texturePath);
                            }
                        }, false));


                        if (mat.HasProperty("_Color")) seq.Join(mat.DOColor(value.color, duration));
                        if (mat.HasProperty("_BlendMode")) seq.Join(mat.DOFloat(value.blendmode, "_BlendMode", duration));
                        if (mat.HasProperty("_CullMode")) seq.Join(mat.DOFloat(value.cullmode, "_CullMode", duration));
                        if (mat.HasProperty("_ShadeColor")) seq.Join(mat.DOColor(value.shadetexcolor, "_ShadeColor", duration));
                        if (mat.HasProperty("_ShadeToony")) seq.Join(mat.DOFloat(value.shadingtoony, "_ShadeToony", duration));
                        if (mat.HasProperty("_RimColor")) seq.Join(mat.DOColor(value.rimcolor, "_RimColor", duration));
                        if (mat.HasProperty("_RimFresnelPower")) seq.Join(mat.DOFloat(value.rimfresnel, "_RimFresnelPower", duration));
                        if (mat.HasProperty("_SrcBlend")) seq.Join(mat.DOFloat(value.srcblend, "_SrcBlend", duration));
                        if (mat.HasProperty("_DstBlend")) seq.Join(mat.DOFloat(value.dstblend, "_DstBlend", duration));
                        if (mat.HasProperty("_Cutoff")) seq.Join(mat.DOFloat(value.cutoff, "_Cutoff", duration));
                        if (mat.HasProperty("_ShadeShift")) seq.Join(mat.DOFloat(value.shadingshift, "_ShadeShift", duration));
                        if (mat.HasProperty("_ReceiveShadowRate")) seq.Join(mat.DOFloat(value.receiveshadow, "_ReceiveShadowRate", duration));
                        if (mat.HasProperty("_ShadingGradeRate")) seq.Join(mat.DOFloat(value.shadinggrade, "_ShadingGradeRate", duration));
                        if (mat.HasProperty("_LightColorAttenuation")) seq.Join(mat.DOFloat(value.shadinggrade, "_LightColorAttenuation", duration));

                        seq.Join(DOVirtual.DelayedCall(duration, () => mat.EnableKeyword("EMISSION"), false));
                        if (mat.HasProperty("_EmissionColor")) seq.Join(mat.DOColor(value.emissioncolor, "_EmissionColor", duration));

                    }
                    else if ((mat.shader.name.ToLower() == "fx/water4") || (mat.shader.name.ToLower() == "fx/simplewater4"))
                    {
                        seq.Join(DOVirtual.DelayedCall(duration, () =>
                        {
                            Shader target = Shader.Find(value.shaderName);
                            if (target != null)
                            {
                                mat.shader = target;
                            }
                        }, false));

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

                }
            }


            return seq;
        }
    }

}
