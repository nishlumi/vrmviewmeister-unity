﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.Networking;
using UnityEngine.UI;
using RootMotion.FinalIK;
using UniGLTF;
using VRM;
using VRMShaders;

using UserHandleSpace;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using TriLibCore;
using TriLibCore.General;
using TriLibCore.SFB;
using TriLibCore.Mappers;
using System;


namespace UserVRMSpace
{

    [Serializable]
    public class BasicObjectInformation
    {
        public string id = "";
        public string Title = "";
        public string type = "";
        public string roleName = "";
        public string roleTitle = "";
        public AnimationTargetParts motion = new AnimationTargetParts();
    }

    [Serializable]
    public class VRMObjectInformation : VRMMetaObject
    {
        public string id;
        public string type;
        public string roleName = "";
        public string roleTitle = "";

        public int isDuplicate;
        public AnimationTargetParts motion = new AnimationTargetParts();

        public VRMObjectInformation()
        {
            isDuplicate = 0;
        }
    }

    [Serializable]
    public class OtherObjectMaterialInfo
    {
        public string name;
        public string shaderName;

        public OtherObjectMaterialInfo()
        {
            name = "";
            shaderName = "";
        }
    }


    [Serializable]
    public class OtherObjectInformation : BasicObjectInformation
    {
        public string fileExt;
        public string animationName;
        public string parentName;
        public AnimationType animationType;
        public float animationLength;
        public WrapMode animationWrapMode;
        public OtherObjectMaterialInfo[] materials;

        public OtherObjectInformation()
        {
            id = "";
            Title = "";
            fileExt = "";
            parentName = "";
            animationName = "";
            animationType = AnimationType.Legacy;
            animationLength = 0f;
            animationWrapMode = WrapMode.Default;

        }

    }
    [Serializable]
    public class LightObjectInformation : BasicObjectInformation
    {

    }
    [Serializable]
    public class CameraObjectInformation : BasicObjectInformation
    {

    }

    [Serializable]
    public class TextObjectInformation : BasicObjectInformation
    {
        public string text = "";
        public int fontSize = 14;
        public int fontStyle = 0;
    }
    [Serializable]
    public class ImageObjectInformation : BasicObjectInformation
    {
        public string location = ""; //ui, 3d
        public int width = 0;
        public int height = 0;
    }
    [Serializable]
    public class EffectObjectInformation : BasicObjectInformation
    {

    }

    [Serializable]
    public class HTMLConnectByteData
    {
        public string filename = "";
        public byte[] data;
        public int length = 0;
    }


    public partial class FileMenuCommands : MonoBehaviour
    {
        // Define variables
        //For file browser
        [DllImport("__Internal")]
        private static extern void FileImportCaptureClick();

        [DllImport("__Internal")]
        private static extern void sendVRMInfo(byte[] thumbnail, int size, string type, string info, string licenseType, string height, string blendShape);

        [DllImport("__Internal")]
        private static extern void sendOtherObjectInfo(string type, string info);


        [DllImport("__Internal")]
        private static extern void ReceiveObjectVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);


        public string FilePath;
        public GameObject ParentObject;
        //public GameObject VRMChara;
        //public GameObject ViewPointObject;
        public Text VRMTitle;
        public Text VRMVersion;
        public Text VRMAuthor;
        public Text VRMHeight;
        public Image VRMAvatar;
        //private Renderer renderer;
        private int i = 0;
        private Vector3 thead_position;
        private Vector3 thead_size;

        private VRMImporterContext pendingContext;
        private VRMMetaObject pendingVRMmeta;
        private RuntimeGltfInstance pendingInstance;

        private GameObject _loadedGameObject;
        //private string _loadedObjectFileName = "";

        private ConfigSettingLabs configLab;

        private ManageAnimation managa;

        // Start is called before the first frame update
        void Start()
        {
            configLab = GameObject.Find("Canvas").GetComponent<ConfigSettingLabs>();

            //VRMChara = GameObject.FindWithTag("Player");
            if (!ParentObject)
            {
                ParentObject = GameObject.FindWithTag("GameController");
            }
            //Object[] data = AssetDatabase.LoadAllAssetsAtPath("Assets/PoseAnime/PoseVRM1_anicon1.controller");
            //Debug.Log("data.length=" + data.Length);
            //foreach (Object o in data)
            //{
            //    Debug.Log(o);
            //}
            //RuntimeAnimatorController animator = (RuntimeAnimatorController)AssetDatabase.LoadAssetAtPath("Assets/PoseAnime/PoseVRM1_anicon1.controller", typeof(RuntimeAnimatorController));
            //Debug.Log(animator);

            managa = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

        }
        private void OnDestroy()
        {
            if (pendingContext != null) return;
        }
        // Update is called once per frame
        void Update()
        {
            //if (_loadedGameObject != null) OnShowedOtherObject();

        }
        // Update is called once per frame
        void LastUpdate()
        {
        }

        public void OnButtonPointerDown()
        {
            //file browser when using Unity
#if !UNITY_EDITOR && UNITY_WEBGL
            FileImportCaptureClick();
#else
            ExtensionFilter[] ext = new ExtensionFilter[] {
                new ExtensionFilter("VRM File", "vrm"),
                //new ExtensionFilter("Fbx File", "fbx"),
            };
            //string[] paths = StandaloneFileBrowser.OpenFilePanel("VRMファイルを選んで下さい。", "", ext, false);
            /*if (paths.Length > 0)
            {
                LoadVRMURI(paths[0]);
            }*/
            IList<ItemWithStream> paths = StandaloneFileBrowser.OpenFilePanel("VRMファイルを選んで下さい。", "", ext, false);
            for (int i = 0; i <  paths.Count; i++)
            {
                Stream stm = paths[i].OpenStream();
                byte[] byt = new byte[stm.Length];
                stm.Read(byt, 0, (int)stm.Length);

                //StartCoroutine(LoadVRMByte(byt));
                PreviewVRM_body(byt);
                StartCoroutine(AcceptLoadVRM());

                Debug.Log(paths[i]);
            }
            return;

            /*FilePath = UnityEditor.EditorUtility.OpenFilePanel("Open VRM file", "", "vrm");
            if (!System.String.IsNullOrEmpty(FilePath))
            {
                LoadVRMURI(FilePath);
            }*/
#endif
        }
        public string GetAppDataPath()
        {
            string ret = Application.persistentDataPath;
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
            return ret;
        }
        public void SetUserDataString(string param)
        {
            String[] arr = param.Split(',');
            if (arr.Length > 1)
            {
                SetUserDataString(arr[0], arr[1]);
            }
        }
        public void SetUserDataString(string key, string val)
        {
            PlayerPrefs.SetString(key, val);
        }
        public void SetUserDataInt(string param)
        {
            String[] arr = param.Split(',');
            if (arr.Length > 1)
            {
                int val = 0;
                bool flag = int.TryParse(arr[1], out val);
                if (flag == true)
                {
                    SetUserDataInt(arr[0], val);
                }
                
            }
        }
        public void SetUserDataInt(string key, int val)
        {
            PlayerPrefs.SetInt(key, val);
        }
        public void SetUserDataFloat(string param)
        {
            String[] arr = param.Split(',');
            if (arr.Length > 1)
            {
                float val = 0;
                bool flag = float.TryParse(arr[1], out val);
                if (flag == true)
                {
                    SetUserDataFloat(arr[0], val);
                }

            }
        }
        public void SetUserDataFloat(string key, float val)
        {
            PlayerPrefs.SetFloat(key, val);
        }
        public object GetUserData(string param)
        {
            string[] prm = param.Split(',');
            string key = prm[0];
            string typename = prm[1];
            string is_contacthtml = prm[2];
            if (typename == "string")
            {
                string ret = PlayerPrefs.GetString(key,"");
#if !UNITY_EDITOR && UNITY_WEBGL
                if (is_contacthtml == "1") ReceiveStringVal(ret);
#endif
                return ret;
            }
            else if (typename == "int")
            {
                int ret = PlayerPrefs.GetInt(key, 0);
#if !UNITY_EDITOR && UNITY_WEBGL
                if (is_contacthtml == "1") ReceiveIntVal(ret);
#endif
                return ret;
            }
            else if (typename == "float")
            {
                float ret = PlayerPrefs.GetFloat(key, 0);
#if !UNITY_EDITOR && UNITY_WEBGL
                if (is_contacthtml == "1") ReceiveFloatVal(ret);
#endif
                return ret;
            }
            return null;
        }
        public void OnClickRemoveObject()
        {
            GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

            if (ovrm.ActiveType == AF_TARGETTYPE.VRM)
            {
                DestroyVRM(ovrm.GetEffectiveActiveAvatar().name);
            }
            else if (ovrm.ActiveType == AF_TARGETTYPE.OtherObject)
            {
                DestroyOther(ovrm.GetEffectiveActiveAvatar().name);
            }
            else if (ovrm.ActiveType == AF_TARGETTYPE.Light)
            {
                DestroyLight(ovrm.GetEffectiveActiveAvatar().name);
            }
            else if (ovrm.ActiveType == AF_TARGETTYPE.Camera)
            {
                DestroyCamera(ovrm.GetEffectiveActiveAvatar().name);
            }
            else if (ovrm.ActiveType == AF_TARGETTYPE.Text)
            {
                DestroyText(ovrm.GetEffectiveActiveAvatar().name);
            }
            else if (ovrm.ActiveType == AF_TARGETTYPE.UImage)
            {
                DestroyUImage(ovrm.GetEffectiveActiveAvatar().name);
            }
            else if (ovrm.ActiveType == AF_TARGETTYPE.Image)
            {
                DestroyImage(ovrm.GetEffectiveActiveAvatar().name);
            }
            else if (ovrm.ActiveType == AF_TARGETTYPE.Effect)
            {
                DestroyEffect(ovrm.GetEffectiveActiveAvatar().name);
            }
        }
        //=============================================================================================================================
        //  VRM functions
        //=============================================================================================================================

        public void LoadVRMURI(string url)
        {
            StartCoroutine(LoadVRMuriBody(url));
        }
        public IEnumerator LoadVRMuriBody(string url)
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
                    //StartCoroutine(LoadVRM_body(www.downloadHandler.data));
                    PreviewVRM_body(www.downloadHandler.data);
                }
            }
        }
        public void VRMByteFileSelected(string param)
        {
            HTMLConnectByteData hdata = JsonUtility.FromJson<HTMLConnectByteData>(param);
            if (hdata != null)
            {
                if (hdata.data.Length > 0)
                {
                    PreviewVRM_body(hdata.data);
                }
            }
        }
        static IAwaitCaller GetIAwaitCaller(bool useAsync)
        {
            if (useAsync)
            {
                return new RuntimeOnlyAwaitCaller();
            }
            else
            {
                return new ImmediateCaller();
            }
        }
        static IMaterialDescriptorGenerator GetVrmMaterialGenerator(bool useUrp, VRM.glTF_VRM_extensions vrm)
        {
            if (useUrp)
            {
                return new VRM.VRMUrpMaterialDescriptorGenerator(vrm);
            }
            else
            {
                return new VRM.VRMMaterialDescriptorGenerator(vrm);
            }
        }
        public async void PreviewLoad09x_VRM(byte[] data)
        {
            VrmUtility.MaterialGeneratorCallback materialCallback = (VRM.glTF_VRM_extensions vrm) => GetVrmMaterialGenerator(false, vrm);
            using (GltfData gdata = new GlbBinaryParser(data, "").Parse())
            {
                VRMData vrm = new VRMData(gdata);
                using var context = new VRMImporterContext(vrm);

                var meta = await context.ReadMetaAsync(GetIAwaitCaller(false));
                pendingVRMmeta = meta;

                pendingInstance = await context.LoadAsync(GetIAwaitCaller(false));
            }
            
            
        }
        /*
        public async void PreviewLoad08x_VRM(byte[] data)
        {
            GltfData gdata =  new GlbBinaryParser(data, "").Parse();
            VRMData vrm = new VRMData(gdata);
            using var context = new VRMImporterContext(vrm);
            var meta = await context.ReadMetaAsync();
            pendingVRMmeta = meta;

            pendingInstance = await context.LoadAsync();
        }
        */
        public void PreviewLoad066_VRM(byte[] data)
        {
            /*
            pendingContext = new VRMImporterContext();

            pendingContext.ParseGlb(data);
            pendingContext.Load();
            */
            
        }
        /// <summary>
        /// To preview loaded VRM for load accept dialog
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public void PreviewVRM_body(byte[] data)
        {

            //PreviewLoad066_VRM(data);
            //PreviewLoad08x_VRM(data);
            PreviewLoad09x_VRM(data);

            //yield return null;


            pendingInstance.gameObject.name = "vrm_" + DateTime.Now.ToFileTime().ToString();
            pendingInstance.gameObject.tag = "Player";
            pendingInstance.gameObject.layer = LayerMask.NameToLayer("Player");

            ManageAvatarTransform mat = pendingInstance.gameObject.AddComponent<ManageAvatarTransform>();
            List<GameObject> meshcnt = mat.CheckSkinnedMeshAvailable();
            GameObject mat_f = null;
            GameObject mat_b = null;
            if (meshcnt.Count == 1)
            {
                mat_b = meshcnt[0];
                //---Irregular VRM (ex: Abiss Horizon, etc)
                mat_b.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
            }
            else
            {
                //---VRM made by VRoid Studio etc
                mat_f = mat.GetFaceMesh();
                mat_b = mat.GetBodyMesh();
                GameObject mat_h = mat.GetHairMesh();
                /*
                if (mat_f != null) mat_f.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                if (mat_b != null) mat_b.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                if (mat_h != null) mat_h.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                */
            }
            


            //--- load thumbnail
            ///VRMAvatar.sprite = Sprite.Create(pendingContext.Meta.Thumbnail, new Rect(0, 0, pendingContext.Meta.Thumbnail.width, pendingContext.Meta.Thumbnail.height), Vector2.zero);


            //Animator conanime = pendingInstance.gameObject.GetComponent<Animator>();


            //---check the height
            //Transform thead = conanime.GetBoneTransform(HumanBodyBones.Head);
            //Transform tfoot = conanime.GetBoneTransform(HumanBodyBones.RightFoot);
            //float centery = context.Root.GetComponent<SkinnedMeshRenderer>().bounds.center.y;

            //Debug.Log("Height=" + (thead.position.y - tfoot.position.y));
            //Debug.Log(thead.position);
            //this.thead_position = thead.position;
            //this.thead_size = thead.lossyScale;

            //Vector3 bodyBounds = new Vector3(0, 0, 0);
            double hei = 0;

            //---get Mesh bounds
            /*
            System.Action<UniGLTF.MeshWithMaterials> callmesh1 = (n) =>
            {
                if (n.Mesh.name.IndexOf("Body") > -1)
                {
                    hei += n.Mesh.bounds.size.y;   //System.Math.Round(n.Mesh.bounds.center.y * 2, 2, System.MidpointRounding.AwayFromZero);
                    bodyBounds.x = n.Mesh.bounds.size.x;
                    bodyBounds.y += n.Mesh.bounds.size.y;
                    bodyBounds.z = n.Mesh.bounds.size.z;


                }
            };
            pendingContext.Meshes.ForEach(callmesh1);*/
            //---Body Mesh own
            SkinnedMeshRenderer mat_b_mesh = mat_b.GetComponent<SkinnedMeshRenderer>();
            hei = mat_b_mesh.bounds.size.y;
            //bodyBounds.x = mat_b_mesh.bounds.size.x;
            //bodyBounds.y += mat_b_mesh.bounds.size.y;
            //bodyBounds.z = mat_b_mesh.bounds.size.z;

            hei = System.Math.Round(hei, 2, System.MidpointRounding.AwayFromZero);
            VRMHeight.text = (hei * 100).ToString() + " cm";
            string strHeight = (hei * 100).ToString() + " cm";
            byte[] incolor = pendingVRMmeta.Thumbnail.EncodeToPNG();

            VRMObjectInformation vrmoi = new VRMObjectInformation();

            vrmoi.Title = pendingVRMmeta.Title;
            vrmoi.Author = pendingVRMmeta.Author;
            vrmoi.Version = pendingVRMmeta.Version;
            vrmoi.ContactInformation = pendingVRMmeta.ContactInformation;
            vrmoi.ExporterVersion = pendingVRMmeta.ExporterVersion;
            vrmoi.Reference = pendingVRMmeta.Reference;
            vrmoi.Thumbnail = pendingVRMmeta.Thumbnail;
            vrmoi.LicenseType = pendingVRMmeta.LicenseType;
            vrmoi.OtherLicenseUrl = pendingVRMmeta.OtherLicenseUrl;
            vrmoi.AllowedUser = pendingVRMmeta.AllowedUser;
            vrmoi.ViolentUssage = pendingVRMmeta.ViolentUssage;
            vrmoi.SexualUssage = pendingVRMmeta.SexualUssage;
            vrmoi.CommercialUssage = pendingVRMmeta.CommercialUssage;
            vrmoi.id = pendingInstance.gameObject.name;
            vrmoi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.VRM);

            string json = JsonUtility.ToJson(vrmoi);
            Debug.Log(json);

            //---get BlendShapes
            SkinnedMeshRenderer aface = null;
            string blendShapeList = "";
            
            if (mat_f != null)
            {
                aface = mat_f.GetComponent<SkinnedMeshRenderer>();
                if (aface != null)
                {
                    List<string> ret = new List<string>();
                    int bscnt = aface.sharedMesh.blendShapeCount;
                    for (int i = 0; i < bscnt; i++)
                    {
                        ret.Add(aface.sharedMesh.GetBlendShapeName(i) + "=" + aface.GetBlendShapeWeight(i));

                    }
                    blendShapeList = string.Join(",", ret.ToArray());

                }
            }

            //---return VRM Meta information to WebGL
#if !UNITY_EDITOR && UNITY_WEBGL
            Debug.Log("incolor="+incolor.Length);
            sendVRMInfo(incolor, incolor.Length, "VRM", json, pendingVRMmeta.LicenseType.ToString(), strHeight, blendShapeList);
#endif

            ScriptableObject.Destroy(vrmoi);
        }

        public void CheckNormalizedVRM()
        {
            GameObject contextRoot = pendingInstance.gameObject;

            Animator anim = contextRoot.GetComponent<Animator>();
            Transform chest = anim.GetBoneTransform(HumanBodyBones.Chest);
            Transform neck = anim.GetBoneTransform(HumanBodyBones.Neck);

            string ret = "";

            if (chest == null) ret += "c";
            if (neck == null) ret += "n";

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif

        }
        /// <summary>
        /// Effectively load and show VRM
        /// </summary>
        /// <returns></returns>
        public IEnumerator AcceptLoadVRM()
        {
            
            GameObject contextRoot = pendingInstance.gameObject;

            //---------------------------------------------------------------------------------------------------
            //---old UniVRM
            VRMImporterContext context = pendingContext;

            contextRoot.transform.SetParent(ParentObject.transform, true);
            contextRoot.transform.position = new Vector3(0f, 0f, 0f);
            contextRoot.transform.rotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));

            //context.EnableUpdateWhenOffscreen();
            pendingInstance.EnableUpdateWhenOffscreen();

            
            ManageAvatarTransform mat = contextRoot.GetComponent<ManageAvatarTransform>();
            List<GameObject> meshcnt = mat.CheckSkinnedMeshAvailable();
            GameObject mat_b = null;
            if (meshcnt.Count == 1)
            {
                mat_b = meshcnt[0];
                //---Irregular VRM (ex: Abiss Horizon, etc)
                //mat_b.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
            }
            else
            {
                //---VRM made by VRoid Studio etc
                //GameObject mat_f = mat.GetFaceMesh();
                mat_b = mat.GetBodyMesh();
                //GameObject mat_h = mat.GetHairMesh();
                //if (mat_f != null) mat_f.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                //if (mat_b != null) mat_b.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                //if (mat_h != null) mat_h.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
            }

            //context.ShowMeshes();
            pendingInstance.ShowMeshes();


            yield return null;

            contextRoot.AddComponent<Blinker>();
            Animator conanime = contextRoot.GetComponent<Animator>();

            GameObject ikworld = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");

            //Transform thead = conanime.GetBoneTransform(HumanBodyBones.Head);
            //Transform tfoot = conanime.GetBoneTransform(HumanBodyBones.RightFoot);

            CapsuleCollider bc = contextRoot.AddComponent<CapsuleCollider>();
            Vector3 bodyBounds = new Vector3(0, 0, 0);
            //double hei = 0;

            //---Body Mesh own
            SkinnedMeshRenderer mat_b_mesh = mat_b.GetComponent<SkinnedMeshRenderer>();
            bodyBounds.x = mat_b_mesh.bounds.size.x;
            bodyBounds.y += mat_b_mesh.bounds.size.y;
            bodyBounds.z = mat_b_mesh.bounds.size.z;

            //---set up collider

            bc.center = mat_b_mesh.bounds.center;
            bc.radius = mat_b_mesh.bounds.extents.x * 0.5f;
            bc.height = mat_b_mesh.bounds.size.y;



            //---Add neccesary Components
            Rigidbody rbd = contextRoot.AddComponent<Rigidbody>();
            rbd.isKinematic = true;

            OperateLoadedVRM olvrm = contextRoot.AddComponent<OperateLoadedVRM>();
            olvrm.SaveDefaultColliderPosition(bc.center);
            //olvrm.SetContext(context);
            olvrm.Title = contextRoot.GetComponent<VRMMeta>().Meta.Title;

            olvrm.SetActiveFace();
            olvrm.InitializeBlendShapeList();
            olvrm.ListGravityInfo();

            bool useFullBodyIK = false; // configLab.GetIntVal("use_fullbody_bipedik") == 1 ? true : false;

            //---set up IK

            VRMLookAtHead vlook = contextRoot.GetComponent<VRMLookAtHead>();

            //---for VRIK
            /*
             * GameObject ikhandles = ikworld.GetComponent<OperateLoadedObj>().CreateIKHandle(context.Root);
            vlook.Target = ikhandles.transform.GetChild(0);

            VRIK vik = context.Root.AddComponent<VRIK>();
            SetupVRIK(ikhandles, conanime, vik, bodyBounds);
            */

            GameObject ikparent;
            //---for FullBodyBiped IK
            if (useFullBodyIK)
            {
                ikparent = ikworld.GetComponent<OperateLoadedObj>().CreateFullBodyIKHandle(contextRoot);
                vlook.Target = ikparent.transform.GetChild(0);
                ikparent.GetComponent<UserGroundOperation>().relatedAvatar = contextRoot;

                FullBodyBipedIK fullik = contextRoot.AddComponent<FullBodyBipedIK>();
                RootMotion.BipedReferences biref = new RootMotion.BipedReferences();
                RootMotion.BipedReferences.AutoDetectReferences(ref biref, fullik.transform, RootMotion.BipedReferences.AutoDetectParams.Default);
                fullik.SetReferences(biref, null);
                LookAtIK laik = contextRoot.AddComponent<LookAtIK>();
                CCDIK cik = contextRoot.AddComponent<CCDIK>();

                SetupFullBodyIK(ikparent, conanime, fullik, laik, cik, bodyBounds);

            }
            else
            {
                //---for Biped IK
                ikparent = ikworld.GetComponent<OperateLoadedObj>().CreateBipedIKHandle(contextRoot);
                vlook.Target = ikparent.transform.GetChild(0);
                ikparent.GetComponent<UserGroundOperation>().relatedAvatar = contextRoot;

                BipedIK bik = contextRoot.AddComponent<BipedIK>();
                RootMotion.BipedReferences biref = new RootMotion.BipedReferences();
                RootMotion.BipedReferences.AutoDetectReferences(ref biref, bik.transform, RootMotion.BipedReferences.AutoDetectParams.Default);

                //bik.solvers.AssignReferences(biref);
                bik.references = biref;
                //bik.SetToDefaults();
                //bik.references.eyes = new Transform[0];

                CCDIK cik = contextRoot.AddComponent<CCDIK>();


                //---set null for VRM Look At Head (Not use LookAt of BipedIK)
                SetupBipedIK(ikparent, conanime, bik, cik, bodyBounds);

            }
            //---material


            //---for hand IK
            SetupHand(contextRoot, conanime);

            contextRoot.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));
            OperateActiveVRM ovrm = ikworld.GetComponent<OperateActiveVRM>();
            //ovrm.ActivateAvatar(contextRoot.name,false);
            ovrm.EnableTransactionHandle(null, ikparent);
            ovrm.AddAvatarBox(contextRoot.name, null, ikparent);

            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

            float[] calcBodyInfo = mat.ParseBodyInfo(mat_b.GetComponent<SkinnedMeshRenderer>());
            List<Vector3> bodyinfoList = mat.ParseBodyInfoList(mat_b.GetComponent<SkinnedMeshRenderer>().bounds, ikparent);

            bool isOverwrite = false; // n - new, o - overwrite
            //mana.FirstAddAvatar(context.Root.name, context.Root, ikparent, "VRM", AF_TARGETTYPE.VRM, calcBodyInfo);
            NativeAnimationAvatar nav = managa.FirstAddAvatar2(out isOverwrite, contextRoot.name, contextRoot, ikparent, "VRM", AF_TARGETTYPE.VRM, calcBodyInfo, bodyinfoList);

            olvrm.SetTPoseBodyInfo(mat_b.GetComponent<SkinnedMeshRenderer>().bounds);
            olvrm.SetTPoseBodyList(bodyinfoList);
            olvrm.RegetTextureConfig();

            pendingContext = null;

            string js = nav.roleName + "," + nav.roleTitle + "," + (isOverwrite ? "o" : "n");
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }

        public void DestroyVRM(string param)
        {
            GameObject ikhp = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
            GameObject[] vrm = GameObject.FindGameObjectsWithTag("Player");
            for (var i = 0; i < vrm.Length; i++)
            {
                if (vrm[i].name == param)
                {
                    GameObject ik = vrm[i].GetComponent<OperateLoadedVRM>().relatedHandleParent;

                    OperateLoadedVRM olvrm = vrm[i].GetComponent<OperateLoadedVRM>();

                    ovrm.RemoveAvatarBox(ik);

                    if (ovrm.ActiveAvatar.name == vrm[i].name)
                    {
                        ovrm.ActiveAvatar = null;
                    }
                    if (ovrm.ActiveIKHandle.name == ik.name)
                    {
                        ovrm.ActiveIKHandle = null;
                    }

                    managa.DetachAvatarFromRole(param + ",avatar");

                    Destroy(ik);
                    //olvrm.GetContext().Dispose();
                    Destroy(vrm[i]);
                    break;
                }
            }
        }



        //=============================================================================================================================
        //  FBX, Obj, etc functions
        //=============================================================================================================================
        public void OnBtnLoadObjPointerDown()
        {
            //ConfigSettingLabs cnf = GameObject.Find("Canvas").GetComponent<ConfigSettingLabs>();

            ExtensionFilter[] ext = new ExtensionFilter[] {
                //new ExtensionFilter("VRM File", "vrm"),
                new ExtensionFilter("Fbx File", "fbx"),
                new ExtensionFilter("Obj File", "obj"),
                new ExtensionFilter("Zip File", "zip"),
            };
            //IList<ItemWithStream> paths = StandaloneFileBrowser.OpenFilePanel("Objectファイルを選んでください", "", ext, false);

            GameObject objpar = managa.AvatarArea; // GameObject.Find("View Body");
            TriLibCore.AssetLoaderFilePicker pick = TriLibCore.AssetLoaderFilePicker.Create();

            TriLibCore.AssetLoaderOptions opt = TriLibCore.AssetLoader.CreateDefaultLoaderOptions();
            if (configLab.GetIntVal("use_animation_generic_when_otherobject") == 1)
            {
                opt.AnimationType = AnimationType.Generic;
            }
            else
            {
                opt.AnimationType = AnimationType.Legacy;
            }

            //opt.ExternalDataMapper = ScriptableObject.CreateInstance<TriLibCore.Samples.ExternalDataMapperSample>();
            //opt.TextureMapper = ScriptableObject.CreateInstance<TriLibCore.Samples.TextureMapperSample>();
            pick.LoadModelFromFilePickerAsync("Objectファイルを選んでください", onLoad: OnLoad, onMaterialsLoad: OnMaterialsLoad, OnProgress, null, OnErrorTriLib, wrapperGameObject: objpar, assetLoaderOptions: opt);

            /*
            for (int i = 0; i < paths.Count; i++)
            {
                Stream stm = paths[i].OpenStream();
                byte[] byt = new byte[stm.Length];
                //stm.Read(byt, 0, (int)stm.Length);
                TriLibCore.AssetLoaderContext lcon =  TriLibCore.AssetLoader.LoadModelFromStream(stm, wrapperGameObject:objpar, assetLoaderOptions: opt,onLoad: OnLoad);
                Debug.Log(lcon);
                
                //StartCoroutine(LoadVRMByte(byt));
                Debug.Log(paths[i]);
            }*/
            return;
        }
        private void OnLoad(AssetLoaderContext assetLoaderContext)
        {
            //if (_loadedGameObject != null)
            //{
            //    Destroy(_loadedGameObject);
            //}
            assetLoaderContext.RootGameObject.SetActive(false);
            //_loadedObjectFileName = assetLoaderContext.Filename;
            //_loadedGameObject.name = _loadedObjectFileName;

        }
        private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
        {

        }
        private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
        {
            GameObject ikhp = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");


            GameObject oth = (assetLoaderContext.RootGameObject);
            oth.SetActive(true);
            //_loadedObjectFileName = assetLoaderContext.Filename;
            oth.tag = "OtherPlayer";
            oth.layer = LayerMask.NameToLayer("Player");
            OperateLoadedOther ol = oth.AddComponent<OperateLoadedOther>();
            oth.AddComponent<ManageAvatarTransform>();
            Vector3 orirot = oth.transform.rotation.eulerAngles;

            ol.childCount = oth.transform.childCount;

            GameObject copycube = (GameObject)Resources.Load("IKHandleCube");
            GameObject ikcube = Instantiate(copycube, copycube.transform.position, Quaternion.identity, ikhp.transform);
            ikcube.GetComponent<OtherObjectDummyIK>().relatedAvatar = oth;
            ikcube.tag = "IKHandle";
            ikcube.transform.rotation = Quaternion.Euler(orirot); // Quaternion.Euler(0f, 0f, 0f);

            ol.relatedHandleParent = ikcube;

            //---Set up Handle Object for parent object.
            //oth.AddComponent<RotationHandle>();
            //oth.AddComponent<PositionHandle>();
            //oth.AddComponent<ScaleHandle>();

            _loadedGameObject = oth;


            OtherObjectInformation obi = OnShowedOtherObject(oth);

            //---change ID to timestamp 
            ol.Title = oth.name;
            obi.Title = oth.name;
            oth.name = "obj_" + DateTime.Now.ToFileTime().ToString();  //_loadedObjectFileName == "" ? oth.name : _loadedObjectFileName;
            ikcube.name = "ikparent_" + oth.name;

            obi.id = oth.name;
            
            obi.fileExt = assetLoaderContext.FileExtension;
            obi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.OtherObject);


            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = managa.FirstAddAvatar(oth.name, oth, ikcube, "OtherObject", AF_TARGETTYPE.OtherObject);
            obi.roleName = nav.roleName;
            obi.roleTitle = nav.roleTitle;

            string js = JsonUtility.ToJson(obi);
            Debug.Log(js);

            OperateActiveVRM ovrm = managa.ikArea.GetComponent<OperateActiveVRM>();
            ovrm.EnableTransactionHandle(null, ikcube);
            ovrm.AddAvatarBox(oth.name, null, ikcube);

            string fext = assetLoaderContext.FileExtension;
            fext = (fext == "") ? "OTH:OBJECT" : "OTH:" + fext.ToUpper();
            ol.objectType = fext;
#if !UNITY_EDITOR && UNITY_WEBGL
            sendOtherObjectInfo(fext, js);
#endif



        }
        private void OnErrorTriLib(IContextualizedError ErrorContext)
        {
            Debug.Log(ErrorContext.GetInnerException());
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ErrorContext.GetInnerException().Message);
#endif
        }
        public OtherObjectInformation OnShowedOtherObject(GameObject oth)
        {
            //GameObject oth = _loadedGameObject;

            OperateLoadedOther olo = oth.GetComponent<OperateLoadedOther>();


            OtherObjectInformation ret = new OtherObjectInformation();


            //---Check for animation (target object itself!!
            Animation anim;
            //Animator animt;
            if (oth.TryGetComponent<Animation>(out anim))
            {
                foreach (AnimationState aa in anim)
                {
                    Debug.Log(aa.name);
                }
                if (anim.clip)
                {
                    anim.playAutomatically = false;
                    anim.Stop();
                    anim.wrapMode = WrapMode.Default;

                    ret.animationName = anim.clip.name;
                    ret.animationLength = anim.clip.length;
                    ret.animationWrapMode = anim.wrapMode;
                    ret.animationType = AnimationType.Legacy;

                    /*animt = oth.AddComponent<Animator>();
                    animt.runtimeAnimatorController = (RuntimeAnimatorController)RuntimeAnimatorController.Instantiate(Resources.Load("PoseOtherObject_anicon1"));
                    Debug.Log(animt.runtimeAnimatorController.animationClips.Length);
                    animt.runtimeAnimatorController.animationClips[0] = anim.clip;

                    AnimatorStateInfo sinfo = animt.GetNextAnimatorStateInfo(0);
                    AnimatorClipInfo[] ci = animt.GetNextAnimatorClipInfo(0);
                    Debug.Log(ci.Length);

                    anim = null;*/
                }
            }


            List<OtherObjectMaterialInfo> oomis = new List<OtherObjectMaterialInfo>();

            int cnt = oth.transform.childCount;

            if (cnt == 0)
            {
                BoxCollider tmpbc;
                if (!oth.TryGetComponent<BoxCollider>(out tmpbc))
                {
                    oth.AddComponent<BoxCollider>();
                }
                MeshRenderer mr;
                if (oth.TryGetComponent<MeshRenderer>(out mr))
                {
                    Material[] mat = oth.GetComponent<Renderer>().materials;
                    for (int m = 0; m < mat.Length; m++)
                    {
                        Material mt = mat[m];
                        OtherObjectMaterialInfo oomi = new OtherObjectMaterialInfo();
                        oomi.name = mt.name;
                        oomi.shaderName = mt.shader.name;
                        if (mt.shader.name.ToLower() == "standard")
                        {
                            olo.RegisterUserMaterial(mt.name, mt);
                        }


                        oomis.Add(oomi);
                    }
                }
            }

            //---Check MeshRenderer and save information
            
            MeshRenderer[] mesharr = oth.GetComponentsInChildren<MeshRenderer>();
            SkinnedMeshRenderer[] skinarr = oth.GetComponentsInChildren<SkinnedMeshRenderer>();

            for (int i = 0; i < mesharr.Length; i++)
            {
                enumMaterialFuncBody(mesharr[i].gameObject, ref oomis, ref olo);
            }
            for (int i = 0; i < skinarr.Length; i++)
            {
                enumMaterialFuncBody(skinarr[i].gameObject, ref oomis, ref olo);
            }

            Animation[] anims = oth.GetComponentsInChildren<Animation>();
            for (int i = 0; i < anims.Length; i++)
            {
                Animation child_anim = anims[i];
                if (child_anim.clip)
                {
                    child_anim.playAutomatically = false;
                    child_anim.Stop();
                    child_anim.wrapMode = WrapMode.Default;

                    ret.animationName = child_anim.clip.name;
                    ret.animationLength = child_anim.clip.length;
                    ret.animationWrapMode = child_anim.wrapMode;
                }
            }

            /*
            for (int i = 0; i < cnt; i++)
            {
                //---Check for Material
                MeshRenderer mr;
                SkinnedMeshRenderer smr;

                GameObject child = oth.transform.GetChild(i).gameObject;
                
                if (
                    (child.TryGetComponent<MeshRenderer>(out mr))
                    ||
                    (child.TryGetComponent<SkinnedMeshRenderer>(out smr))
                    )
                {
                    //---For collider judge, add Collider ( effective, use the parent of Collider object )
                    child.tag = "OtherPlayerCollider";
                    child.layer = LayerMask.NameToLayer("Player");
                    child.AddComponent<BoxCollider>();

                    Material[] mat = child.GetComponent<Renderer>().materials;
                    

                    for (int m = 0; m < mat.Length; m++)
                    {
                        Material mt = mat[m];
                        OtherObjectMaterialInfo oomi = new OtherObjectMaterialInfo();
                        oomi.name = mt.name;
                        oomi.shaderName = mt.shader.name;
                        if (mt.shader.name.ToLower() == "standard")
                        {
                            olo.RegisterUserMaterial(mt.name, mt);
                        }

                        
                        oomis.Add(oomi);
                        
                    }
                    

                }

                //---Check for Animation
                if (child.TryGetComponent<Animation>(out anim))
                {
                    if (anim.clip)
                    {
                        anim.playAutomatically = false;
                        anim.Stop();
                        anim.wrapMode = WrapMode.Default;

                        ret.animationName = anim.clip.name;
                        ret.animationLength = anim.clip.length;
                        ret.animationWrapMode = anim.wrapMode;
                    }
                }

            }*/


            ret.materials = oomis.ToArray();

            //_loadedGameObject = null;

            return ret;
        }
        private void enumMaterialFuncBody(GameObject child, ref List<OtherObjectMaterialInfo> oomis, ref OperateLoadedOther olo)
        {
            //---For collider judge, add Collider ( effective, use the parent of Collider object )
            child.tag = "OtherPlayerCollider";
            child.layer = LayerMask.NameToLayer("Player");
            child.AddComponent<BoxCollider>();

            Material[] mat = child.GetComponent<Renderer>().materials;


            for (int m = 0; m < mat.Length; m++)
            {
                Material mt = mat[m];
                OtherObjectMaterialInfo oomi = new OtherObjectMaterialInfo();
                oomi.name = mt.name;
                oomi.shaderName = mt.shader.name;
                if (mt.shader.name.ToLower() == "standard")
                {
                    olo.RegisterUserMaterial(mt.name, mt);
                }


                oomis.Add(oomi);

            }
        }
        public void ObjectFileSelected(string uri)
        {
            //StartCoroutine(LoadOtherObjectURI(uri));

            GameObject objpar = managa.AvatarArea; // GameObject.Find("View Body");
            TriLibCore.AssetLoaderOptions opt = TriLibCore.AssetLoader.CreateDefaultLoaderOptions();
            if (configLab.GetIntVal("use_animation_generic_when_otherobject") == 1)
            {
                opt.AnimationType = AnimationType.Generic;
            }
            else
            {
                opt.AnimationType = AnimationType.Legacy;
            }
            //Debug.Log(uri);
            string[] prm = uri.Split(',');
            string url = prm[0];
            string filename = prm[1];
            string ext = prm[2];
            //Debug.Log("filename=[" + filename + "]");
            //Debug.Log("ext=[" + ext + "]");

            var webRequest = AssetDownloader.CreateWebRequest(url.Replace("blob:",""));
            AssetDownloader.LoadModelFromUri(webRequest, 
                onLoad: OnLoad, 
                onMaterialsLoad: OnMaterialsLoad, null, 
                onError: OnErrorTriLib,  
                wrapperGameObject: objpar, assetLoaderOptions: opt, fileExtension: ext
            );
        }
        private IEnumerator LoadOtherObjectURI(string url)
        {

            //Debug.Log(url);
            string[] prm = url.Split(',');
            string uri = prm[0];
            string filename = prm[1];
            string ext = TriLibCore.Utils.FileUtils.GetFileExtension(filename, false); // System.IO.Path.GetExtension(filename);
            using (UnityWebRequest www = UnityWebRequest.Get(uri))
            {
                DownloadHandlerBuffer dhb = new DownloadHandlerBuffer();
                www.downloadHandler = dhb;

                yield return www.SendWebRequest();
                //if (www.isNetworkError || www.isHttpError)
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                    yield break;
                }
                else
                {
                    GameObject objpar = managa.AvatarArea; //GameObject.Find("View Body");

                    AssetLoaderOptions options = AssetLoader.CreateDefaultLoaderOptions();
                    options.ExternalDataMapper = ScriptableObject.CreateInstance<FilePickerExternalDataMapper>();
                    options.TextureMapper = ScriptableObject.CreateInstance<FilePickerTextureMapper>();

                    //Debug.Log("filename=[" + filename + "]");
                    //Debug.Log("ext=[" + ext + "]");
                    //Debug.Log("data.length=[" + dhb.data.Length + "]");
                    //AssetDownloader.LoadModelFromUri(www, OnLoad, OnMaterialsLoad, null, OnErrorTriLib, null, options);
                    MemoryStream mems = new MemoryStream(dhb.data);
                    ItemWithStream istream = new ItemWithStream
                    {
                        Name = filename,
                        Stream = mems
                    };
                    IList<ItemWithStream> ill = new ItemWithStream[1];
                    ill[0] = istream;
                    var opstream = istream.OpenStream();
                    
                    //---effectively open
                    if (ext == "zip")
                    {
                        AssetLoaderZip.LoadModelFromZipStream(opstream,
                            OnLoad, OnMaterialsLoad, 
                            null, OnErrorTriLib, 
                            objpar, options, ill
                        );
                    }
                    else
                    {
                        AssetLoader.LoadModelFromStream(opstream, filename: filename, fileExtension: ext, 
                            OnLoad, OnMaterialsLoad, null, 
                            OnErrorTriLib, 
                            objpar, options, ill
                        );
                    }
                    
                }
            }
        }
        private IEnumerator LoadOtherObject_body(byte[] data)
        {

            var assetLoaderOptions = AssetLoader.CreateDefaultLoaderOptions();
            var webRequest = AssetDownloader.CreateWebRequest("");
            //AssetDownloader.LoadModelFromUri(webRequest, OnLoad, OnMaterialsLoad, OnProgress, OnError, null, assetLoaderOptions);

            yield return null;
        }
        public void DestroyOther(string param)
        {
            GameObject ikhp = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
            GameObject[] vrm = GameObject.FindGameObjectsWithTag("OtherPlayer");
            for (var i = 0; i < vrm.Length; i++)
            {
                if (vrm[i].name == param)
                {
                    GameObject ik = vrm[i].GetComponent<OperateLoadedOther>().relatedHandleParent;

                    OperateLoadedOther olvrm = vrm[i].GetComponent<OperateLoadedOther>();
                    if (olvrm.IsPlayingAnimation() == 1)
                    {
                        olvrm.StopAnimation();
                    }

                    ovrm.RemoveAvatarBox(ik);

                    if (ovrm.GetEffectiveActiveAvatar().name == vrm[i].name)
                    {
                        ovrm.ActiveAvatar = null;
                    }
                    if (ovrm.ActiveIKHandle.name == ik.name)
                    {
                        ovrm.ActiveIKHandle = null;
                    }

                    managa.DetachAvatarFromRole(param + ",avatar");

                    Destroy(ik);
                    Destroy(vrm[i]);
                    break;
                }
            }
        }
        public void CreateBlankQuad()
        {
            CreateBlankCube(PrimitiveType.Cube);
        }
        public void CreateBlankObject(int param)
        {
            /*
             * 
             * 0 - sphere, 1 - capsule, 2 - cylinder, 3 - cube, 4 - plane, 5 - quad
             */
            CreateBlankCube((PrimitiveType)param);
        }
        public void CreateBlankCube(PrimitiveType ptype)
        {
            string[] pritype = {"BlankSphere", "BlankCapsule", "BlankCylinder", "BlankCube", "BlankPlane", "BlankQuad" };
            GameObject ikhp = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");

            GameObject copyprim = (GameObject)Resources.Load(pritype[(int)ptype]);
            GameObject oth = Instantiate(copyprim, copyprim.transform.position, Quaternion.identity, managa.AvatarArea.transform);  //GameObject.CreatePrimitive(ptype);


            oth.transform.SetParent(managa.AvatarArea.transform);
            oth.name = "obj_" + DateTime.Now.ToFileTime().ToString();  //_loadedObjectFileName == "" ? oth.name : _loadedObjectFileName;
            oth.tag = "OtherPlayer";
            oth.layer = LayerMask.NameToLayer("Player");
            OperateLoadedOther ol = oth.AddComponent<OperateLoadedOther>();
            oth.AddComponent<ManageAvatarTransform>();

            GameObject copycube = (GameObject)Resources.Load("IKHandleCube");
            GameObject ikcube = Instantiate(copycube, copycube.transform.position, Quaternion.identity, ikhp.transform);
            ikcube.name = "ikparent_" + oth.name;
            ikcube.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            ikcube.tag = "IKHandle";

            ol.relatedHandleParent = ikcube;
            ol.Title = ptype.ToString();
            ikcube.GetComponent<OtherObjectDummyIK>().relatedAvatar = oth;

            OtherObjectInformation obi = OnShowedOtherObject(oth);
            obi.id = oth.name;
            obi.Title = ptype.ToString();
            obi.fileExt = "";
            obi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.OtherObject);

            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = managa.FirstAddAvatar(oth.name, oth, ikcube, "OtherObject", AF_TARGETTYPE.OtherObject);
            obi.roleName = nav.roleName;
            obi.roleTitle = nav.roleTitle;

            string js = JsonUtility.ToJson(obi);
            Debug.Log(js);

            OperateActiveVRM ovrm = managa.ikArea.GetComponent<OperateActiveVRM>();
            ovrm.EnableTransactionHandle(null, ikcube);
            ovrm.AddAvatarBox(oth.name, null, ikcube);

            string fext = "";
            fext = (fext == "") ? "OTH:OBJECT" : "OTH:" + fext.ToUpper();
            ol.objectType = fext;
#if !UNITY_EDITOR && UNITY_WEBGL
            sendOtherObjectInfo(fext, js);
#endif



        }
        //=============================================================================================================================
        //  Light functions
        //=============================================================================================================================
        public void OpenSpotLight()
        {
            OpenLightObject("spot");
        }
        public void OpenPointLight()
        {
            OpenLightObject("point");
        }
        public void OpenLightObject(string param)
        {
            GameObject ikworld = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            GameObject[] lt = ikworld.GetComponent<OperateLoadedObj>().CreateLight(param);


            lt[0].name = "lit_" + DateTime.Now.ToFileTime().ToString();
            lt[0].tag = "LightPlayer";
            lt[0].layer = LayerMask.NameToLayer("Player");
            OperateLoadedLight ol = lt[0].AddComponent<OperateLoadedLight>();
            lt[0].AddComponent<ManageAvatarTransform>();

            lt[1].name = "ikparent_" + lt[0].name;
            lt[1].tag = "IKHandle";
            lt[1].layer = LayerMask.NameToLayer("Handle");

            ol.relatedHandleParent = lt[1];
            ol.Title = param + " light";

            Light light = lt[0].GetComponent<Light>();
            BasicObjectInformation loi = new BasicObjectInformation();
            loi.id = lt[0].name;
            loi.Title = Enum.GetName(typeof(LightType), light.type);
            loi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.Light);
            loi.motion.lightType = light.type;
            loi.motion.range = light.range;
            loi.motion.power = light.intensity;
            loi.motion.color = light.color;
            loi.motion.spotAngle = light.spotAngle;

            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = managa.FirstAddAvatar(lt[0].name, lt[0], lt[1], "Light", AF_TARGETTYPE.Light);
            loi.roleName = nav.roleName;
            loi.roleTitle = nav.roleTitle;

            OperateActiveVRM ovrm = managa.ikArea.GetComponent<OperateActiveVRM>();
            ovrm.EnableTransactionHandle(null, lt[1]);
            ovrm.AddAvatarBox(lt[0].name, null, lt[1]);

            string js = JsonUtility.ToJson(loi);

            string fext = "LIGHT";
#if !UNITY_EDITOR && UNITY_WEBGL
            sendOtherObjectInfo(fext, js);
#endif
        }
        public void DestroyLight(string param)
        {
            GameObject ikhp = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
            GameObject[] vrm = GameObject.FindGameObjectsWithTag("LightPlayer");
            for (var i = 0; i < vrm.Length; i++)
            {
                if (vrm[i].name == param)
                {
                    GameObject ik = vrm[i].GetComponent<OperateLoadedLight>().relatedHandleParent;

                    OperateLoadedLight olvrm = vrm[i].GetComponent<OperateLoadedLight>();

                    ovrm.RemoveAvatarBox(ik);

                    if (ovrm.ActiveAvatar.name == vrm[i].name)
                    {
                        ovrm.ActiveAvatar = null;
                    }
                    if (ovrm.ActiveIKHandle.name == ik.name)
                    {
                        ovrm.ActiveIKHandle = null;
                    }

                    managa.DetachAvatarFromRole(param + ",avatar");

                    Destroy(ik);
                    Destroy(vrm[i]);
                    break;
                }
            }
        }
        //=============================================================================================================================
        //  Camera functions
        //=============================================================================================================================
        public void OnClickBtnAddCamera()
        {
            CreateCameraObject("");
        }
        public void CreateCameraObject(string param)
        {
            GameObject ikworld = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            GameObject[] lt = ikworld.GetComponent<OperateLoadedObj>().CreateCamera();


            lt[0].name = "cam_" + DateTime.Now.ToFileTime().ToString();
            lt[0].tag = "CameraPlayer";
            lt[0].layer = LayerMask.NameToLayer("Player");
            OperateLoadedCamera ol = lt[0].AddComponent<OperateLoadedCamera>();
            lt[0].AddComponent<ManageAvatarTransform>();
            Camera cam = lt[0].GetComponent<Camera>();

            lt[1].name = "ikparent_" + lt[0].name;
            lt[1].tag = "IKHandle";
            lt[1].layer = LayerMask.NameToLayer("Handle");

            ol.relatedHandleParent = lt[1];
            ol.Title = "Camera";

            CameraObjectInformation loi = new CameraObjectInformation();
            loi.id = lt[0].name;
            loi.Title = "Camera";
            loi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.Camera);
            loi.motion.fov = cam.fieldOfView;
            loi.motion.depth = cam.depth;
            loi.motion.viewport = cam.rect;

            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = managa.FirstAddAvatar(lt[0].name, lt[0], lt[1], "Camera", AF_TARGETTYPE.Camera);
            loi.roleName = nav.roleName;
            loi.roleTitle = nav.roleTitle;

            OperateActiveVRM ovrm = managa.ikArea.GetComponent<OperateActiveVRM>();
            ovrm.EnableTransactionHandle(null, lt[1]);
            ovrm.AddAvatarBox(lt[0].name, null, lt[1]);

            string js = JsonUtility.ToJson(loi);

            string fext = "CAMERA";
#if !UNITY_EDITOR && UNITY_WEBGL
            sendOtherObjectInfo(fext, js);
#endif
        }
        public void DestroyCamera(string param)
        {
            GameObject ikhp = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
            GameObject[] vrm = GameObject.FindGameObjectsWithTag("CameraPlayer");
            for (var i = 0; i < vrm.Length; i++)
            {
                if (vrm[i].name == param)
                {
                    GameObject ik = vrm[i].GetComponent<OperateLoadedCamera>().relatedHandleParent;

                    OperateLoadedCamera olvrm = vrm[i].GetComponent<OperateLoadedCamera>();
                    olvrm.EndPreview();

                    ovrm.RemoveAvatarBox(ik);

                    if (ovrm.ActiveAvatar.name == vrm[i].name)
                    {
                        ovrm.ActiveAvatar = null;
                    }
                    if (ovrm.ActiveIKHandle.name == ik.name)
                    {
                        ovrm.ActiveIKHandle = null;
                    }

                    managa.DetachAvatarFromRole(param + ",avatar");

                    Destroy(ik);
                    Destroy(vrm[i]);
                    break;
                }
            }
        }
        //=============================================================================================================================
        //  Text functions
        //=============================================================================================================================
        public void OpenSample1Text ()
        {
            OpenText("ABC,tl");
        }
        /// <summary>
        /// Create and Show Text UI
        /// </summary>
        /// <param name="param">CSV-string: 0=text label, 1=anchor position(tl=TopLeft, bl=BottomLeft, tr=TopRight, br=BottomRight)</param>
        public void OpenText(string param)
        {
            string[] prm = param.Split(',');

            GameObject ikworld = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            GameObject txt  = ikworld.GetComponent<OperateLoadedObj>().CreateText(prm[0],prm[1]);


            txt.name = "txt_" + DateTime.Now.ToFileTime().ToString();
            txt.tag = "TextPlayer";
            txt.layer = LayerMask.NameToLayer("UserUI");
            txt.AddComponent<ManageAvatarTransform>();
            RectTransform rectra = txt.GetComponent<RectTransform>();
            rectra.anchoredPosition3D = new Vector3(0f, 0f, 0f);
            rectra.localScale = new Vector3(1f, 1f, 1f);

            Text text = txt.GetComponent<Text>();
            TextObjectInformation toi = new TextObjectInformation();
            toi.id = txt.name;
            toi.Title = "Text";
            toi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.Text);
            toi.motion.fontSize = text.fontSize;
            toi.motion.fontStyle = text.fontStyle;
            toi.motion.textAlignment = text.alignment;


            txt.GetComponent<OperateLoadedText>().Title = "Text";
            
            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = managa.FirstAddAvatar(txt.name, txt, null, "Text", AF_TARGETTYPE.Text);
            toi.roleName = nav.roleName;
            toi.roleTitle = nav.roleTitle;


            string js = JsonUtility.ToJson(toi);


            string fext = "TEXT";
#if !UNITY_EDITOR && UNITY_WEBGL
            sendOtherObjectInfo(fext, js);
#endif
        }
        public void DestroyText(string param)
        {
            GameObject msgarea = managa.MsgArea; // GameObject.Find("MsgArea");
            
            for (var i = 0; i < msgarea.transform.childCount; i++)
            {
                GameObject vrm = msgarea.transform.GetChild(i).gameObject;
                if (vrm.name == param)
                {
                    managa.DetachAvatarFromRole(param + ",avatar");

                    Destroy(vrm);
                    break;
                }
            }
        }
        //=============================================================================================================================
        //  Image functions
        //=============================================================================================================================
        public void OnClickBtnImage()
        {
            InputField inpf2 = GameObject.Find("inpf2").GetComponent<InputField>();
            //ImageFileSelected(inpf2.text);
            UIImageFileSelected(inpf2.text);

            string[] ext = new string[2];
            ext[0] = "jpg";
            ext[1] = "png";

            ExtensionFilter[] exts = new ExtensionFilter[] {
                new ExtensionFilter("JPEG File", "jpg"),
                new ExtensionFilter("PNG File", "png"),
                new ExtensionFilter("Image File", ext),
            };
            IList<ItemWithStream> paths = StandaloneFileBrowser.OpenFilePanel("画像ファイルを選んでください。。", "", exts, false);
            for (int i = 0; i < paths.Count; i++)
            {
                Stream stm = paths[i].OpenStream();
                byte[] byt = new byte[stm.Length];
                stm.Read(byt, 0, (int)stm.Length);
                StartCoroutine(DownloadImage_body(byt));
                //Debug.Log(paths[i]);
            }


        }
        public void OnClickBtnUImage()
        {
            InputField inpf2 = GameObject.Find("inpf2").GetComponent<InputField>();
            //ImageFileSelected(inpf2.text);
            UIImageFileSelected(inpf2.text);

            string[] ext = new string[2];
            ext[0] = "jpg";
            ext[1] = "png";

            ExtensionFilter[] exts = new ExtensionFilter[] {
                new ExtensionFilter("JPEG File", "jpg"),
                new ExtensionFilter("PNG File", "png"),
                new ExtensionFilter("Image File", ext),
            };
            IList<ItemWithStream> paths = StandaloneFileBrowser.OpenFilePanel("画像ファイルを選んでください。。", "", exts, false);
            for (int i = 0; i < paths.Count; i++)
            {
                Stream stm = paths[i].OpenStream();
                byte[] byt = new byte[stm.Length];
                stm.Read(byt, 0, (int)stm.Length);
                StartCoroutine(DownloadUIImage_body(byt));
                //Debug.Log(paths[i]);
            }


        }

        public void ImageFileSelected(string url)
        {
            StartCoroutine(LoadImageuri(url,false));
        }
        public void UIImageFileSelected(string url)
        {
            StartCoroutine(LoadImageuri(url,true));
        }

        public IEnumerator LoadImageuri(string url, bool is_ui)
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
                    if (is_ui)
                    {
                        StartCoroutine(DownloadUIImage_body(www.downloadHandler.data));
                    }
                    else
                    {
                        StartCoroutine(DownloadImage_body(www.downloadHandler.data));
                    }
                    
                }
            }
        }
        public IEnumerator DownloadUIImage_body(byte[] data)
        {
            GameObject ikhp = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");

            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);
            Rect rect = new Rect(0f, 0f, tex.width, tex.height);
            Sprite spr = Sprite.Create(tex, rect, Vector2.zero);

            GameObject img = new GameObject();
            RectTransform rectra = img.AddComponent<RectTransform>();
            img.AddComponent<ManageAvatarTransform>();
            OperateLoadedUImage olui = img.AddComponent<OperateLoadedUImage>();
            olui.Title = "UI image";
            Image imgcon = img.AddComponent<Image>();
            imgcon.sprite = spr;

            img.name = "uimg_" + DateTime.Now.ToFileTime().ToString();
            img.layer = LayerMask.NameToLayer("UserUI");
            img.tag = "UImagePlayer";

            float rectex = 0f;
            if (tex.width > tex.height)
            {
                rectex = (float)tex.height / (float)tex.width;
                rectra.sizeDelta = new Vector2(100f / rectex, 100f);
            }
            else
            {
                rectex = (float)tex.width / (float)tex.height;
                rectra.sizeDelta = new Vector2(100f, 100f / rectex);
            }

            img.transform.SetParent(managa.ImgArea.transform);
            olui.SetAnchorPos("tl");
            olui.SetPositionFromOuter("0,0,0");
            rectra.anchoredPosition3D = new Vector3(rectra.anchoredPosition3D.x, rectra.anchoredPosition3D.y, 0f);
            rectra.localScale = new Vector3(1f, 1f, 1f);


            //---return information to HTML
            ImageObjectInformation ioi = new ImageObjectInformation();
            ioi.id = img.name;
            ioi.Title = "UImage";
            ioi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.UImage);
            ioi.location = "ui";
            ioi.width = tex.width;
            ioi.height = tex.height;

            string fext = "UIMAGE";

            //---Set up for Animation
            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = managa.FirstAddAvatar(img.name, img, null, "UImage", AF_TARGETTYPE.UImage);
            ioi.roleName = nav.roleName;
            ioi.roleTitle = nav.roleTitle;

            string js = JsonUtility.ToJson(ioi);
#if !UNITY_EDITOR && UNITY_WEBGL
            sendOtherObjectInfo(fext, js);
#endif


            yield return null;
        }
        public void DestroyUImage(string param)
        {
            GameObject imgarea = managa.ImgArea; // GameObject.Find("ImgArea");

            for (var i = 0; i < imgarea.transform.childCount; i++)
            {
                GameObject vrm = imgarea.transform.GetChild(i).gameObject;
                if (vrm.name == param)
                {
                    managa.DetachAvatarFromRole(param + ",avatar");

                    Destroy(vrm);
                    break;
                }
            }
        }
        public IEnumerator DownloadImage_body(byte[] data)
        {
            GameObject viewBody = managa.AvatarArea; // GameObject.Find("View Body");
            GameObject ikhp = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");

            //---load image as texture
            Texture2D tex = new Texture2D(1,1);
            Material mat = new Material(Shader.Find("Standard"));

            //tex = ((DownloadHandlerTexture)handler).texture;
            tex.LoadImage(data);
            mat.SetTexture("_MainTex", tex);
            mat.SetFloat("_Mode", 2f);

            //---create effective image 3D object 
            GameObject plat = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plat.GetComponent<Renderer>().material = mat;
            float rectex = 0f;
            if (tex.width > tex.height)
            {
                rectex = (float)tex.height / (float)tex.width;
                plat.transform.localScale = new Vector3(plat.transform.localScale.x / rectex, plat.transform.localScale.y, 0.01f);
            }
            else
            {
                rectex = (float)tex.width / (float)tex.height;
                plat.transform.localScale = new Vector3(plat.transform.localScale.x, plat.transform.localScale.y / rectex, 0.01f);
            }
            plat.transform.rotation = Quaternion.Euler(0f, 0f, 180f);

            MeshRenderer mr;
            SkinnedMeshRenderer smr;
            if (
            (plat.TryGetComponent<MeshRenderer>(out mr))
            ||
            (plat.TryGetComponent<SkinnedMeshRenderer>(out smr))
            )
            {
                //---For collider judge, add Collider ( effective, use the parent of Collider object )
                plat.tag = "OtherPlayerCollider";
                plat.layer = LayerMask.NameToLayer("Player");
            }

            //---create dummy 3D object for operating
            GameObject empt = new GameObject();
            empt.transform.SetParent(viewBody.transform);

            empt.name = "img_" + DateTime.Now.ToFileTime().ToString();
            empt.tag = "OtherPlayer";
            empt.layer = LayerMask.NameToLayer("Player");
            plat.transform.SetParent(empt.transform);

            //---Set up IK handle
            OperateLoadedOther ol = empt.AddComponent<OperateLoadedOther>();
            OnShowedOtherObject(empt);

            empt.AddComponent<ManageAvatarTransform>();

            GameObject copycube = (GameObject)Resources.Load("IKHandleCube");
            GameObject ikcube = Instantiate(copycube, copycube.transform.position, Quaternion.identity, ikhp.transform);
            ikcube.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            ikcube.tag = "IKHandle";
            ikcube.name = "ikparent_" + empt.name;

            ol.relatedHandleParent = ikcube;
            ol.Title = "Image object";
            ikcube.GetComponent<OtherObjectDummyIK>().relatedAvatar = empt;

            //---return information to HTML
            ImageObjectInformation ioi = new ImageObjectInformation();
            ioi.id = empt.name;
            ioi.Title = "Image";
            ioi.location = "3d";
            ioi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.Image);
            ioi.width = tex.width;
            ioi.height = tex.height;

            string fext = "IMAGE";
            ol.objectType = fext;

            //---Set up for Animation
            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = managa.FirstAddAvatar(empt.name, empt, ikcube, "Image", AF_TARGETTYPE.Image);
            ioi.roleName = nav.roleName;
            ioi.roleTitle = nav.roleTitle;

            OperateActiveVRM ovrm = managa.ikArea.GetComponent<OperateActiveVRM>();
            ovrm.EnableTransactionHandle(null, ikcube);
            ovrm.AddAvatarBox(empt.name, null, ikcube);

            string js = JsonUtility.ToJson(ioi);
#if !UNITY_EDITOR && UNITY_WEBGL
            sendOtherObjectInfo(fext, js);
#endif



            yield return null;


        }
        public Texture LoadImage_bytebody(byte[] data)
        {

            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);

            return tex;
        }
        public void DestroyImage(string param)
        {
            DestroyOther(param);

        }
        //=============================================================================================================================
        //  Effect functions
        //=============================================================================================================================
        public void CreateSingleEffect()
        {
            GameObject ikworld = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            GameObject[] lt = ikworld.GetComponent<OperateLoadedObj>().CreateEffect();

            lt[0].name = "eff_" + DateTime.Now.ToFileTime().ToString();
            lt[0].tag = "EffectDestination";
            lt[0].layer = LayerMask.NameToLayer("Player");
            lt[0].AddComponent<ManageAvatarTransform>();

            lt[1].name = "ikparent_" + lt[0].name;
            lt[1].tag = "IKHandle";
            lt[1].layer = LayerMask.NameToLayer("Handle");

            OperateLoadedEffect ole = lt[0].GetComponent<OperateLoadedEffect>();
            ole.relatedHandleParent = lt[1];
            ole.Title = "Effect";

            BasicObjectInformation boi = new BasicObjectInformation();
            boi.id = lt[0].name;
            boi.Title = "Effect";
            boi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.Effect);

            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = managa.FirstAddAvatar(lt[0].name, lt[0], lt[1], "Effect", AF_TARGETTYPE.Effect);
            boi.roleName = nav.roleName;
            boi.roleTitle = nav.roleTitle;

            OperateActiveVRM ovrm = managa.ikArea.GetComponent<OperateActiveVRM>();
            ovrm.EnableTransactionHandle(null, lt[1]);
            ovrm.AddAvatarBox(lt[0].name, null, lt[1]);

            string js = JsonUtility.ToJson(boi);

            string fext = "EFFECT";
#if !UNITY_EDITOR && UNITY_WEBGL
                sendOtherObjectInfo(fext, js);
#endif

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
        public GameObject[] ListEffects(string genre)
        {
            GameObject[] ret = null;
            GameObject[] efs = GameObject.FindGameObjectsWithTag("EffectSystem");
            for (int i = 0; i < efs.Length; i++)
            {
                if (genre.ToLower() == efs[i].name.ToLower())
                {
                    List<GameObject> gos = new List<GameObject>();
                    for (int c = 0; c < efs[i].transform.childCount; c++)
                    {
                        GameObject cld = efs[i].transform.GetChild(c).gameObject;
                        gos.Add(cld);
                    }
                    ret = gos.ToArray();
                    break;
                }
            }
            return ret;
        }
        public string ListEffectsFromOuter(string genre)
        {
            GameObject[] efs = ListEffects(genre);

            string ret = "";
            List<string> arr = new List<string>();

            for (int i = 0; i < efs.Length; i++)
            {
                arr.Add(efs[i].name);
            }
            ret = string.Join(",", arr);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
            return ret;

        }

        public void DestroyEffect(string param)
        {
            GameObject ikhp = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
            GameObject[] vrm = GameObject.FindGameObjectsWithTag("EffectDestination");

            for (var i = 0; i < vrm.Length; i++)
            {
                if (vrm[i].name == param)
                {
                    OperateLoadedEffect ole = vrm[i].GetComponent<OperateLoadedEffect>();
                    ole.StopEffect();
                    GameObject ik = ole.relatedHandleParent;

                    ovrm.RemoveAvatarBox(ik);

                    if (ovrm.GetEffectiveActiveAvatar().name == vrm[i].name)
                    {
                        ovrm.ActiveAvatar = null;
                    }
                    if (ovrm.ActiveIKHandle.name == ik.name)
                    {
                        ovrm.ActiveIKHandle = null;
                    }

                    managa.DetachAvatarFromRole(param + ",avatar");

                    Destroy(vrm[i]);
                    Destroy(ik);
                    break;
                }
            }
        }
        //=============================================================================================================================
        //  Audio functions
        //=============================================================================================================================
        public void FileOpenAudio()
        {
            string[] ext = new string[3];
            ext[0] = "wav";
            ext[1] = "mp3";
            ext[2] = "ogg";

            ExtensionFilter[] exts = new ExtensionFilter[] {
                new ExtensionFilter("wave File", "wav"),
                new ExtensionFilter("mp3 File", "mp3"),
                new ExtensionFilter("ogg File", "ogg"),
                new ExtensionFilter("audio File", ext),
            };
            IList<ItemWithStream> paths = StandaloneFileBrowser.OpenFilePanel("オーディオファイルを選んでください。。", "", exts, false);
            for (int i = 0; i < paths.Count; i++)
            {
                //Debug.Log(paths[i].Name);
                /*Stream stm = paths[i].OpenStream();
                byte[] byt = new byte[stm.Length];
                stm.Read(byt, 0, (int)stm.Length);
                StartCoroutine(DownloadUIImage_body(byt));
                Debug.Log(paths[i]);*/
                OpenBGM(paths[i].Name+","+ paths[i].Name);
            }
        }
        public void OpenBGM(string url)
        {
            StartCoroutine(LoadAudiouri(url,"BGM"));
        }
        public void OpenSE(string url)
        {
            StartCoroutine(LoadAudiouri(url, "SE"));
        }
        public IEnumerator LoadAudiouri(string url, string BoxName)
        {
            string[] prm = url.Split(',');
            string ext = System.IO.Path.GetExtension(prm[1]);
            Debug.Log("name=>" + prm[1] + ",  extention=>" + ext);
            AudioType okaudiotype = AudioType.UNKNOWN;
            if (ext.ToLower().IndexOf("wav") > -1)
            {
                okaudiotype = AudioType.WAV;
            }
            else if (ext.ToLower().IndexOf("mp3") > -1)
            {
                okaudiotype = AudioType.MPEG;
            }
            else if (ext.ToLower().IndexOf("ogg") > -1)
            {
                okaudiotype = AudioType.OGGVORBIS;
            }

            Debug.Log(okaudiotype);
            if (okaudiotype != AudioType.UNKNOWN)
            {
                Debug.Log("url:" + prm[0] + "\n" + 
                    okaudiotype);
                
                using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(prm[0], okaudiotype))
                {
                    DownloadHandlerAudioClip dhan = new DownloadHandlerAudioClip("", okaudiotype);
                    dhan.streamAudio = false;
                    if (okaudiotype == AudioType.OGGVORBIS) dhan.compressed = false;
                    www.downloadHandler = dhan;

                    yield return www.SendWebRequest();
                    //if (www.isNetworkError || www.isHttpError)
                    if (www.result == UnityWebRequest.Result.ConnectionError)
                    {
                        Debug.Log(www.error);
                        yield break;
                    }
                    else
                    {
                        AudioClip ac = dhan.audioClip;
                        Debug.Log("Audio downloade->");
                        if (ac != null)
                        {
                            Debug.Log(ac.GetType());
                            Debug.Log(ac.ToString());
                        }
                        else
                        {
                            Debug.Log("Audio failed...");
                        }
                        
                        DownloadAudio_body(ac, Path.GetFileName(prm[1]), BoxName);

                    }
                }

            }

        }
        public void DownloadAudio_body(AudioClip ac, string filename, string BoxName)
        {
            GameObject AudioArea = managa.AudioArea; //GameObject.Find("AudioArea");
            GameObject destination = AudioArea.transform.Find(BoxName).gameObject;
            if (destination != null)
            {
                OperateLoadedAudio ola = destination.GetComponent<OperateLoadedAudio>();
                ola.Title = "Audio";
                ola.AddAudio(filename, ac);
            }
            string js = BoxName + "," + filename + "," + ac.samples;
            Debug.Log(js);

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
            

        }
    }
}