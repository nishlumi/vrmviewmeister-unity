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

        [Serializable]
        public class MaterialProperties
        {
            public string name = "";
            public string shaderName = "";
            public Color color = Color.white;
            public float cullmode = 0;
            public float blendmode = 0;
            public int textureIsCamera = 0;
            public string textureRole = "";
            public string texturePath = "";
            public Texture realTexture = null;
            public float metallic = 0;
            public float glossiness = 0;
            public Color emissioncolor = Color.white;
            public Color shadetexcolor = Color.white;
            public float shadingtoony = 0;
            public Color rimcolor = Color.white;
            public float rimfresnel = 0;
        }

        ManageAnimation manim;

        public int childCount;

        public float animationRemainTime;
        public float SeekPosition
        {
            set
            {
                animationRemainTime = value;
                Animation anim = null;
                if (transform.TryGetComponent<Animation>(out anim))
                {
                    if (value > -1f)
                    {
                        anim.clip.SampleAnimation(transform.gameObject, animationRemainTime);
                    }

                }
            }
            get
            {
                return animationRemainTime;
            }
        }
        public UserAnimationState animationStartFlag;  //1 - play, 0 - stop, 2 - playing, 3 - seeking, 4 - pause
        protected Animation currentAnim;
        protected bool triggerCurrentAnimOnFinished;

        public string renderTextureParentId;
        private Texture backupShareTexture;

        private string OldShaderName;

        [SerializeField]
        public Dictionary<string, Material> userSharedMaterials;
        [SerializeField]
        public Dictionary<string, MaterialProperties> userSharedTextureFiles;
        public Dictionary<string, MaterialProperties> backupTextureFiles;

        private const string CAMERAROLE = "#Camera";

        /**
         *  This class is manager function, all Fbx, Obj Object has.
         *  Variables only...
         */
        // Start is called before the first frame update
        void Awake()
        {
            childCount = 0;
            animationRemainTime = 0f;
            userSharedMaterials = new Dictionary<string, Material>();
            userSharedTextureFiles = new Dictionary<string, MaterialProperties>();
            backupTextureFiles = new Dictionary<string, MaterialProperties>();
            targetType = AF_TARGETTYPE.OtherObject;
        }
        private void Start()
        {
            manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            currentAnim = gameObject.GetComponent<Animation>();
            triggerCurrentAnimOnFinished = false;

            foreach (AnimationState state in currentAnim)
            {
                AnimationEvent aev = new AnimationEvent();
                aev.time = state.clip.length;
                aev.floatParameter = state.clip.length;
                aev.functionName = "OnAnimationFinished";
                state.clip.AddEvent(aev);
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
                            manim.materialManager.UnRefer(userSharedTextureFiles[kvp.Key].texturePath);
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
        public void SetShaderSetting(string param)
        { //---NOT USE
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

                        if (mt.shader.name.ToLower() == "standard")
                        {
                            OldShaderName = mt.shader.name;

                            Shader newshd = Shader.Find("VRM/MToon");
                            if (newshd != null)
                            {
                                mt.shader = newshd;
                                mt.SetFloat("_CullMode", (float)UnityEngine.Rendering.CullMode.Off);
                                mt.SetFloat("_BlendMode", 0f);

                                mt.color = Color.white;
                            }
                        }
                        else if (mt.shader.name.ToLower() == "vrm/mtoon")
                        {
                            Shader newshd = Shader.Find("Standard");
                            if (newshd != null)
                            {
                                mt.shader = newshd;
                                mt.SetFloat("_Mode", 0f);
                                mt.color = Color.white;
                            }
                        }


                    }
                }
            }
        }
        public void RegisterUserMaterial(string name, Material mat)
        {
            userSharedMaterials[name] = mat;
            Texture tex = mat.GetTexture("_MainTex");

            MaterialProperties matp = new MaterialProperties();
            //--- "" is recovering to default. else is manually set any texture.
            matp.texturePath = ""; // (tex != null) ? tex.name : "";
            userSharedTextureFiles[name] = matp;

            MaterialProperties backmatp = new MaterialProperties();
            backmatp.realTexture = tex;
            backmatp.texturePath = matp.texturePath;
            backupTextureFiles[name] = backmatp;
        }
        public List <MaterialProperties> ListUserMaterialObject()
        {
            List<MaterialProperties> ret = new List<MaterialProperties>();

            foreach (KeyValuePair<string, Material> kvp in userSharedMaterials)
            {
                Material mat = kvp.Value;

                //string texturePath = userSharedTextureFiles.ContainsKey(kvp.Key) ? userSharedTextureFiles[kvp.Key].texturePath : "";

                MaterialProperties matp = new MaterialProperties();

                matp.name = kvp.Key;
                matp.color = mat.color;
                matp.shaderName = mat.shader.name;
                matp.emissioncolor = mat.GetColor("_EmissionColor");
                if (mat.shader.name.ToLower() == "vrm/mtoon")
                {
                    matp.cullmode = mat.GetFloat("_CullMode");
                    matp.blendmode = mat.GetFloat("_BlendMode");
                    matp.shadetexcolor = mat.GetColor("_ShadeColor");
                    matp.shadingtoony = mat.GetFloat("_ShadeToony");
                    matp.rimcolor = mat.GetColor("_RimColor");
                    matp.rimfresnel = mat.GetFloat("_RimFresnelPower");
                }
                else
                {
                    matp.cullmode = 0;
                    matp.blendmode = mat.GetFloat("_Mode");
                    matp.metallic = mat.GetFloat("_Metallic");
                    matp.glossiness = mat.GetFloat("_Glossiness");
                }
                matp.texturePath = userSharedTextureFiles[kvp.Key].texturePath;
                matp.textureRole = userSharedTextureFiles[kvp.Key].textureRole;
                matp.textureIsCamera = userSharedTextureFiles[kvp.Key].textureIsCamera;

                ret.Add(matp);
            }

            return ret;
        }
        public string ListGetOneUserMaterial(string param)
        {
            string ret = "";

            Debug.Log("param="+param);
            Debug.Log(userSharedMaterials.ContainsKey(param));
            if (userSharedMaterials.ContainsKey(param))
            {
                Material mat = userSharedMaterials[param];
                Debug.Log("material name=" + mat.name);
                string texturePath = userSharedTextureFiles[param].texturePath;

                if (mat.shader.name.ToLower() == "vrm/mtoon")
                {
                    ret = (
                        param + "," +
                        mat.shader.name + "," +
                        ColorUtility.ToHtmlStringRGBA(mat.color) + "," +
                        mat.GetFloat("_CullMode").ToString() + "," +
                        mat.GetFloat("_BlendMode").ToString() + "," +
                        texturePath + "," +

                        "0" + "," +
                        "0" + "," +
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_EmissionColor")) + "," +
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_ShadeColor")) + "," +
                        mat.GetFloat("_ShadeToony").ToString() + "," +
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_RimColor")) + "," +
                        mat.GetFloat("_RimFresnelPower").ToString()
                    );
                }
                else
                {
                    ret = (
                        param + "," +
                        mat.shader.name + "," +
                        ColorUtility.ToHtmlStringRGBA(mat.color) + "," +
                        "0" + "," +
                        mat.GetFloat("_Mode").ToString() + "," +
                        texturePath + "," +

                        mat.GetFloat("_Metallic").ToString() + "," +
                        mat.GetFloat("_Glossiness").ToString() + "," +
                        ColorUtility.ToHtmlStringRGBA(mat.GetColor("_EmissionColor")) + "," +
                        ColorUtility.ToHtmlStringRGBA(Color.white) + "," +
                        "0" + "," +
                        ColorUtility.ToHtmlStringRGBA(Color.white) + "," +
                        "0"
                    );
                }
            }
            Debug.Log("ret=" + ret);
            // 0 - key name
            // 1 - shader name
            // 2 - material color
            // 3 - Cull mode
            // 4 - Blend mode
            // 5 - Texture name
            // 6 - Metallic (Standard)
            // 7 - Glossiness (Standard)
            // 8 - Emission Color 
            // 9 - Shade Texture Color (VRM/MToon)
            // 10- Shaing Toony (VRM/MToon)
            // 11- Rim Color (VRM/MToon)
            // 12- Rim Fresnel Power (VRM/MToon)

            return ret;
        }
        public void ListGetOneUserMaterialFromOuter(string param)
        {
            string ret = ListGetOneUserMaterial(param);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public List<string> ListUserMaterial()
        {
            List<string> ret = new List<string>();

            Dictionary<string, Material>.Enumerator matlst = userSharedMaterials.GetEnumerator();
            while (matlst.MoveNext())
            {
                ret.Add(ListGetOneUserMaterial(matlst.Current.Key)); 
            }
            
            return ret;
        }
        public void ListUserMaterialFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            List<string> list = ListUserMaterial();
            string js = string.Join("\r\n", list.ToArray());
            ReceiveStringVal(js);
#endif
        }
        public void GetUserMaterial()
        { //---NOT USE------------------------------------------
            string name = "";
            Material mat = userSharedMaterials[name];

            string ret = name + "," + mat.shader.name + "," + ColorUtility.ToHtmlStringRGBA(mat.color);
            //---cull mode
            if (mat.shader.name.ToLower() == "vrm/mtoon")
            {
                ret += "," + mat.GetFloat("_CullMode").ToString();
            }
            //---mode - rendering type
            if (mat.shader.name.ToLower() == "standard")
            {
                ret += "," + mat.GetFloat("_Mode").ToString();
            }
            else if (mat.shader.name.ToLower() == "vrm/mtoon")
            {
                ret += "," + mat.GetFloat("_BlendMode").ToString();
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif

            //return mat;
        }
        public Material GetUserMaterial(string name)
        {
            if (userSharedMaterials.ContainsKey(name))
            {
                return userSharedMaterials[name];
            }
            else
            {
                return null;
            }
            
        }
        public Sequence SetMaterialTween(Sequence seq, string name, string propname, MaterialProperties value, float duration)
        {
            //string[] prm = param.Split(',');
            string mat_name = name;
            //string propname = prm[1];
            //string value = val;
            if (userSharedMaterials.ContainsKey(mat_name))
            {
                Material mat = userSharedMaterials[mat_name];
                if (mat != null)
                {

                    if (propname.ToLower() == "color")
                    {
                        seq.Join(mat.DOColor(value.color, duration));
                    }
                    else if (propname.ToLower() == "renderingtype")
                    {
                        if (mat.shader.name.ToLower() == "standard")
                        {
                            seq.Join(mat.DOFloat(value.blendmode, "_Mode", duration));
                        }
                        else if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            seq.Join(mat.DOFloat(value.blendmode, "_BlendMode", duration));
                        }
                    }
                    else if (propname.ToLower() == "cullmode")
                    { //0 - off, 1 - front, 2 - back
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            seq.Join(mat.DOFloat(value.cullmode, "_CullMode", duration));
                        }
                    }
                    else if (propname.ToLower() == "metallic")
                    { //metallic
                        if (mat.shader.name.ToLower() == "standard")
                        {
                            seq.Join(mat.DOFloat(value.metallic, "_Metallic", duration));
                        }
                    }
                    else if (propname.ToLower() == "glossiness")
                    { //glossiness
                        if (mat.shader.name.ToLower() == "standard")
                        {
                            seq.Join(mat.DOFloat(value.glossiness, "_Glossiness", duration));
                        }
                    }
                    else if (propname.ToLower() == "emissioncolor")
                    { //emission color
                        seq.Join(DOVirtual.DelayedCall(duration, () => mat.EnableKeyword("EMISSION")));
                        seq.Join(mat.DOColor(value.emissioncolor, "_EmissionColor", duration));
                    }
                    else if (propname.ToLower() == "shadetexcolor")
                    { //shade texture color
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            seq.Join(mat.DOColor(value.shadetexcolor, "_ShadeColor", duration));
                        }
                    }
                    else if (propname.ToLower() == "shadingtoony")
                    { //shading toony
                        seq.Join(mat.DOFloat(value.shadingtoony, "_ShadeToony", duration));
                    }
                    else if (propname.ToLower() == "rimcolor")
                    { //rim color
                        if (mat.shader.name.ToLower() == "vrm/mtoon")
                        {
                            seq.Join(mat.DOColor(value.rimcolor, "_RimColor", duration));
                        }
                    }
                    else if (propname.ToLower() == "rimfresnel")
                    { //rim fresnel power
                        seq.Join(mat.DOFloat(value.rimfresnel, "_RimFresnelPower", duration));
                    }
                }
            }
            

            return seq;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">0-destination material name, 1-parts(shader,color,cullmode),2-value(Standard,VRM/MToon, #FFFFFF)</param>
        public void SetUserMaterial(string param)
        {
            string[] prm = param.Split(',');
            string mat_name = prm[0];
            string propname = prm[1];
            string value = prm[2];
            //Debug.Log("paramater="+mat_name);
            if (userSharedMaterials.ContainsKey(mat_name))
            {
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
                            mat.SetColor("_Color", col);
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
                                    manim.materialManager.UnRefer(userSharedTextureFiles[mat_name].texturePath);
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
                                    manim.materialManager.UnRefer(userSharedTextureFiles[mat_name].texturePath);
                                }
                                mat.SetTexture("_MainTex", null);

                                //---set new texture
                                NativeAP_OneMaterial nap = manim.materialManager.FindTexture(value);
                                if (nap != null)
                                {
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
                }
            }
            
        }
        private IEnumerator LoadImageuri(string url, Material mat)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
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
                    StartCoroutine(DownloadTextureImage(www.downloadHandler.data, mat));
                }
            }
        }
        public IEnumerator DownloadTextureImage(byte[] data, Material mat)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);

            mat.SetTexture("_MainTex", tex);

            yield return null;
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
            string jsclip = string.Join(',', animclips);
            if (type == 1)
            {
                int pflag = IsPlayingAnimation();
                int playflag = (int)animationStartFlag;
                float seek = animationRemainTime;

                // 1st sep ... \t
                // 2nd sep ... ,

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

                ;

                Debug.Log("OtherObject indicated properties=");
                Debug.Log(ret);
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
                        state.normalizedTime = animationRemainTime;
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
                            animationRemainTime = state.normalizedTime;
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
                            state.normalizedTime = animationRemainTime;
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
                foreach (AnimationState state in anim)
                {
                    if (anim.clip.name == state.clip.name)
                    {
                        state.speed = to;
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
                    anim.clip.SampleAnimation(transform.gameObject, animationRemainTime);
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
        //-------------------------------------------------------------------------------------------------------------------------
        public string SetRenderTextureFromCamera(Material targetMat, string role)
        {
            /*if (role == "")
            { //---delete function
                DestroyCameraRenderTexture(targetMat);
                return "";
            }*/
            string ret = "";
            NativeAnimationAvatar nav = manim.GetCastInProject(role, AF_TARGETTYPE.Camera);
            if (nav != null)
            {
                ret = nav.roleTitle;
                //renderTextureParentId = role;
                OperateLoadedCamera olc = nav.avatar.GetComponent<OperateLoadedCamera>();

                /*Texture tmp = targetMat.GetTexture("_MainTex");
                //---check an texture with render texture of CURRENT role
                if (tmp != olc.GetRenderTexture())
                {
                    Destroy(tmp);
                }*/
                targetMat.SetTexture("_MainTex", olc.GetRenderTexture());
            }

            return ret;
        }
        /*
        public void SetCameraRenderTexture(string role)
        { //---OLD TEST, NOT USE
            if (role == "")
            {
                
            }
            else
            {
                GameObject animarea = GameObject.Find("AnimateArea");
                //---search Camera role in the project
                NativeAnimationFrameActor nafa =  animarea.GetComponent<ManageAnimation>().GetFrameActorFromRole(role, AF_TARGETTYPE.Camera);
                NativeAnimationAvatar nav = nafa.avatar;
                if (nav != null)
                {
                    renderTextureParentId = role;
                    OperateLoadedCamera olc = nav.avatar.GetComponent<OperateLoadedCamera>();
                    MeshRenderer mr;
                    //---set Render texture of the Camera to material's texture of this OtherObject
                    if (transform.TryGetComponent<MeshRenderer>(out mr))
                    {
                        olc.ApplyRenderTexture();
                        backupShareTexture = mr.sharedMaterial.mainTexture;
                        mr.sharedMaterial.mainTexture = olc.GetRenderTexture();
                    }
                }

            }
        }
        */
        public void DestroyCameraRenderTexture(Material targetMat)
        {
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
                        /*else
                        { //---if general texture, destroy this.
                            Texture tmp = targetMat.GetTexture("_MainTex");
                            Destroy(tmp);
                        }*/
                    }
                    /*
                    else
                    {
                        Texture tmp = targetMat.GetTexture("_MainTex");
                        Destroy(tmp);
                    }
                    */
                    break;
                }
            }
        }
        public string GetCameraRenderTexture()
        {
            return renderTextureParentId;
        }
        public void GetCameraRenderTextureFromOuter()
        {
            string ret = GetCameraRenderTexture();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(renderTextureParentId);
#endif
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
    }

}
