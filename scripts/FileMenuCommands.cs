using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;

using UnityEngine.Networking;
using UnityEngine.UI;
using RootMotion.FinalIK;
using UniGLTF;
using UniGLTF.Extensions.VRMC_vrm;
using UniVRM10;
using VRMShaders;

using UserHandleSpace;
using UserUISpace;
using LumisIkApp;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using TriLibCore;
using TriLibCore.General;
using TriLibCore.SFB;
using TriLibCore.Mappers;
using System;
using System.Linq;
using TMPro;
using DG.Tweening;


namespace UserVRMSpace
{

    [Serializable]
    public class BasicObjectInformation
    {
        public string isOverwrite = "n";
        public string id = "";
        public string Title = "";
        public string type = "";
        public string roleName = "";
        public string roleTitle = "";
        public AnimationTargetParts motion = new AnimationTargetParts();
    }

    public enum BasicUssageLicense
    {
        Disallow,
        Allow
    };

    [Serializable]
    public class VRMObjectInformation : BasicObjectInformation
    {
        //public string id;
        //public string type;
        //public string roleName = "";
        //public string roleTitle = "";

        public string ExporterVersion;

        //public string Title;

        public string Version;

        public string Author;

        public string ContactInformation;

        public string Reference;

        public Texture2D Thumbnail;

        //old: AllowedUser
        public AvatarPermissionType AllowedUser;

        //old: UssageLicense: AllowExcessivelyViolentUsage
        public int ViolentUssage;

        //old: AllowExcessivelySexualUsage
        public int SexualUssage;

        //old: CommercialUsageType
        public CommercialUsageType CommercialUssage;

        public string OtherPermissionUrl;

        public int LicenseType;

        public string OtherLicenseUrl;

        //---for VRM1.0
        public int PoliticalUssage;

        public int AntiSocialUssage;

        public int useCreditNotation;

        public int AllowRedistribution;

        public int AllowModification;

        public int isDuplicate;
        //public AnimationTargetParts motion = new AnimationTargetParts();

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
        private static extern void sendObjectError(string type, string info);

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
        //public Text VRMTitle;
        //public Text VRMVersion;
        //public Text VRMAuthor;
        //public Text VRMHeight;
        //public Image VRMAvatar;
        //private Renderer renderer;
        private int i = 0;
        private Vector3 thead_position;
        private Vector3 thead_size;

        //private VRMImporterContext pendingContext;
        //private VRMMetaObject pendingVRMmeta;
        private VRM10ObjectMeta pendingVRMmeta;
        private RuntimeGltfInstance pendingInstance;

        private NativeAnimationAvatar lastLoadedAvatar;
        private OpeningNativeAnimationAvatar openingNative;

        private GameObject _loadedGameObject;
        private string _loadedObjectFileName = "";

        private ConfigSettingLabs configLab;

        private ManageAnimation managa;

        public bool IsBackToHTML = true;

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

            lastLoadedAvatar = null;
            openingNative = new OpeningNativeAnimationAvatar();

            float a = -1.5853840462654034e-10f;
            Debug.Log(a);
            Debug.Log(a.ToString("F11"));

            float b = 2.193940f;
            Debug.Log(b);
            Debug.Log(b.ToString("F8"));

        }
        private void OnDestroy()
        {
            //if (pendingContext != null) return;
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
                PreviewVRM_body(byt,false).ConfigureAwait(false);
                AcceptLoadVRM();

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

        public NativeAnimationAvatar LastLoaded
        {
            get
            {
                return lastLoadedAvatar;
            }
        }
        public void OnClickRemoveObject()
        {
            GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

            NativeAnimationAvatar nav = managa.GetCastByAvatar(ovrm.ActiveAvatar.name);

            if (nav.type == AF_TARGETTYPE.VRM)
            {
                DestroyVRM(ovrm.GetEffectiveActiveAvatar().name);
            }
            else if (nav.type == AF_TARGETTYPE.OtherObject)
            {
                DestroyOther(ovrm.GetEffectiveActiveAvatar().name);
            }
            else if (nav.type == AF_TARGETTYPE.Light)
            {
                DestroyLight(ovrm.GetEffectiveActiveAvatar().name);
            }
            else if (nav.type == AF_TARGETTYPE.Camera)
            {
                DestroyCamera(ovrm.GetEffectiveActiveAvatar().name);
            }
            else if (nav.type == AF_TARGETTYPE.Text)
            {
                DestroyText(ovrm.GetEffectiveActiveAvatar().name);
            }
            else if (nav.type == AF_TARGETTYPE.UImage)
            {
                DestroyUImage(ovrm.GetEffectiveActiveAvatar().name);
            }
            else if (nav.type == AF_TARGETTYPE.Image)
            {
                DestroyImage(ovrm.GetEffectiveActiveAvatar().name);
            }
            else if (nav.type == AF_TARGETTYPE.Effect)
            {
                DestroyEffect(ovrm.GetEffectiveActiveAvatar().name);
            }
        }
        public IEnumerator ListGetAAS(string param)
        {
            var opHandle = Addressables.LoadResourceLocationsAsync(param);
            yield return opHandle;

            List<string> arr = new List<string>();
            if (opHandle.Status == AsyncOperationStatus.Succeeded &&
                opHandle.Result != null &&
                opHandle.Result.Count > 0)
            {
                for (int i = 0; i < opHandle.Result.Count; i++)
                {
                    //Debug.Log("address is: " + opHandle.Result[i].PrimaryKey);
                    arr.Add(opHandle.Result[i].PrimaryKey);
                }
                
            }

            string ret = string.Join('\t', arr);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif            
        }
        public IEnumerator DownloadAAS(string param)
        {
            GameObject npd = GameObject.Find("newProgressDlg");
            UserUIProgressDlg uui = npd.GetComponent<UserUIProgressDlg>();

            AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(param);
            //Debug.Log("download size=" + getDownloadSize.Result.ToString());

            yield return getDownloadSize.Result;

            //UnityEvent<float> ProgressEvent;
            //UnityEvent<bool> CompletionEvent;
            AsyncOperationHandle dHandle = Addressables.DownloadDependenciesAsync(param, false);
            float progress = 0;

            //Debug.Log("total bytes=" + dHandle.GetDownloadStatus().TotalBytes.ToString());
            uui.EnableDlg(true);
            while (dHandle.Status == AsyncOperationStatus.None)
            {
                float percentageComplete = dHandle.GetDownloadStatus().Percent;
                //Debug.Log("percent=" + percentageComplete.ToString());
                if (percentageComplete > progress * 1.1)
                {
                    progress = percentageComplete;
                    uui.SetProgressValue(progress);
                }
                yield return percentageComplete;
            }
            uui.EnableDlg(false);
            Addressables.Release(dHandle);

        }
        public void CheckUniVRMVersionFromOuter()
        {
            string ret = "1x";
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }

        //=============================================================================================================================
        //  VRM functions
        //=============================================================================================================================


        /// <summary>
        /// Load VRM by object URL from HTML
        /// </summary>
        /// <param name="url"></param>
        public void LoadVRMURI(string url)
        {
            StartCoroutine(LoadVRMuriBody(url,true));
        }

        /// <summary>
        /// Load VRM by object URL from Unity
        /// </summary>
        /// <param name="url"></param>
        public OpeningNativeAnimationAvatar LoadVRM(string url)
        {
            StartCoroutine(LoadVRM_UnityBody(url,false));

            return openingNative;
        }
        public IEnumerator LoadVRMuriBody(string url, bool isBackHTML)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();
                //if (www.isNetworkError || www.isHttpError)
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError(www.error);
                    yield break;
                }
                else
                {
                    //StartCoroutine(LoadVRM_body(www.downloadHandler.data));
                    PreviewVRM_body(www.downloadHandler.data, isBackHTML).ConfigureAwait(false);
                }
            }
        }
        public IEnumerator LoadVRM_UnityBody(string url, bool isBackHTML)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();
                //if (www.isNetworkError || www.isHttpError)
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError(www.error);
                    yield break;
                }
                else
                {
                    //StartCoroutine(LoadVRM_body(www.downloadHandler.data));
                    PreviewVRM_body(www.downloadHandler.data, isBackHTML).ConfigureAwait(false);
                    yield return null;
                    AcceptLoadVRMUnity();
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
                    PreviewVRM_body(hdata.data,false).ConfigureAwait(false);
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
        static IMaterialDescriptorGenerator GetVrmMaterialGenerator(bool useUrp)
        {
            if(useUrp)
            {
                return new UrpVrm10MaterialDescriptorGenerator();
            }
            else
            {
                return new BuiltInVrm10MaterialDescriptorGenerator();
            }
        }
        public async Task<RuntimeGltfInstance> PreviewLoad10x_VRM(byte[] data)
        {
            RuntimeGltfInstance ret = null;
            Vrm10Instance vinst = await Vrm10.LoadBytesAsync(data,
                canLoadVrm0X: true,
                showMeshes: false,
                awaitCaller: GetIAwaitCaller(false),
                materialGenerator: GetVrmMaterialGenerator(false)
            );
            if (vinst != null)
            {
                pendingInstance = vinst.GetComponent<RuntimeGltfInstance>();
                pendingVRMmeta = vinst.Vrm.Meta;
                ret = pendingInstance;
            }
            else
            {
                var gltfModel = await GltfUtility.LoadBytesAsync("",data,
                    awaitCaller: GetIAwaitCaller(false)
                );
                if (gltfModel == null)
                {
                    throw new Exception("Failed to load the file as glTF format.");
                }
                pendingInstance = gltfModel;
                ret = pendingInstance;
            }
            return ret;
        }
        /*
        public async Task<RuntimeGltfInstance> PreviewLoad09x_VRM(byte[] data)
        {
            RuntimeGltfInstance ret = null;
            VrmUtility.MaterialGeneratorCallback materialCallback = (VRM.glTF_VRM_extensions vrm) => GetVrmMaterialGenerator(false, vrm);
            using (GltfData gdata = new GlbBinaryParser(data, "").Parse())
            {
                VRMData vrm = new VRMData(gdata);
                using var context = new VRMImporterContext(vrm);

                var meta = await context.ReadMetaAsync(GetIAwaitCaller(false));
                pendingVRMmeta = meta;

                pendingInstance = await context.LoadAsync(GetIAwaitCaller(false));
                ret = pendingInstance;
            }
            return ret;
        }*/
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
        public async Task PreviewVRM_body(byte[] data, bool isBackHTML)
        {
            //PreviewLoad066_VRM(data);
            //PreviewLoad08x_VRM(data);
            //await PreviewLoad09x_VRM(data);
            RuntimeGltfInstance vinst = await PreviewLoad10x_VRM(data);

            if (vinst == null)
            {
                //---return VRM Meta information to WebGL
#if !UNITY_EDITOR && UNITY_WEBGL
            //Debug.Log("incolor="+incolor.Length);
            if (isBackHTML) sendObjectError("ERROR","VRM\tloaderror");
#endif
            }
            else
            {
                //yield return null;

                pendingInstance.gameObject.name = managa.CheckAndSetAvatarId("vrm_", pendingInstance.gameObject.GetInstanceID());
                pendingInstance.gameObject.tag = "Player";
                pendingInstance.gameObject.layer = LayerMask.NameToLayer("Player");

                //Debug.Log("pendingInstance.gameObject.name=" + pendingInstance.gameObject.name);

                ManageAvatarTransform mat = pendingInstance.gameObject.AddComponent<ManageAvatarTransform>();
                List<GameObject> meshcnt = mat.CheckSkinnedMeshAvailable();
                GameObject mat_f = null;
                GameObject mat_b = null;
                if (meshcnt.Count == 1)
                {
                    mat_b = meshcnt[0];
                    //---Irregular VRM (ex: Abiss Horizon, etc)
                    mat_b.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;

                    //---if face not found, mat_b equal to mat_f
                    mat_f = meshcnt[0];
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

                Bounds maxbounds = mat.CalculateAllBounds(meshcnt);

                //--- load thumbnail


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
                SkinnedMeshRenderer mat_f_mesh = mat_f.GetComponent<SkinnedMeshRenderer>();

                //hei = mat_b_mesh.bounds.size.y;
                //hei = mat_f_mesh.bounds.max.y;
                hei = maxbounds.size.y;

                hei = System.Math.Round(hei, 2, System.MidpointRounding.AwayFromZero);
                string strHeight = (hei * 100).ToString() + " cm";
                byte[] incolor = new byte[] { };
                if (pendingVRMmeta.Thumbnail == null)
                {
                    Debug.LogWarning("thumbnail is null.");
                }
                else
                {
                    incolor = pendingVRMmeta.Thumbnail.EncodeToPNG();
                }

                VRMObjectInformation vrmoi = new VRMObjectInformation();

                vrmoi.Title = pendingVRMmeta.Name;
                vrmoi.Author = string.Join("\t", pendingVRMmeta.Authors);
                vrmoi.Version = pendingVRMmeta.Version;
                vrmoi.ContactInformation = pendingVRMmeta.ContactInformation;
                vrmoi.ExporterVersion = ""; // pendingVRMmeta.ExporterVersion;
                vrmoi.Reference = string.Join("\t", pendingVRMmeta.References);
                vrmoi.Thumbnail = pendingVRMmeta.Thumbnail;
                //vrmoi.LicenseType = pendingVRMmeta.LicenseType;
                vrmoi.OtherLicenseUrl = pendingVRMmeta.OtherLicenseUrl;
                vrmoi.AllowedUser = pendingVRMmeta.AvatarPermission;
                vrmoi.ViolentUssage = pendingVRMmeta.ViolentUsage ? (int)BasicUssageLicense.Allow : (int)BasicUssageLicense.Disallow;
                vrmoi.SexualUssage = pendingVRMmeta.SexualUsage ? (int)BasicUssageLicense.Allow : (int)BasicUssageLicense.Disallow;
                vrmoi.CommercialUssage = pendingVRMmeta.CommercialUsage;
                vrmoi.PoliticalUssage = pendingVRMmeta.PoliticalOrReligiousUsage ? (int)BasicUssageLicense.Allow : (int)BasicUssageLicense.Disallow;
                vrmoi.AntiSocialUssage = pendingVRMmeta.AntisocialOrHateUsage ? (int)BasicUssageLicense.Allow : (int)BasicUssageLicense.Disallow;
                vrmoi.AllowModification = pendingVRMmeta.Modification == ModificationType.prohibited ? (int)BasicUssageLicense.Allow : (int)BasicUssageLicense.Disallow;
                vrmoi.AllowRedistribution = pendingVRMmeta.Redistribution ? (int)BasicUssageLicense.Allow : (int)BasicUssageLicense.Disallow;
                vrmoi.useCreditNotation = pendingVRMmeta.CreditNotation == CreditNotationType.unnecessary ? (int)BasicUssageLicense.Allow : (int)BasicUssageLicense.Disallow;

                vrmoi.id = pendingInstance.gameObject.name;
                vrmoi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.VRM);

                string json = JsonUtility.ToJson(vrmoi);

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
            //Debug.Log("incolor="+incolor.Length);
            if (isBackHTML) sendVRMInfo(incolor, incolor.Length, "VRM", json, "0", strHeight, blendShapeList);
#endif

                //ScriptableObject.Destroy(vrmoi);
                openingNative.baseInfo = vrmoi;
                //Debug.Log("PreviewBody=" + vrmoi.Title);
            }

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
            if (IsBackToHTML) ReceiveStringVal(ret);
#endif

        }

        /// <summary>
        /// finally load and decide VRM from HTML
        /// </summary>
        public void AcceptLoadVRM()
        {
            StartCoroutine(Body_AcceptLoadVRM(true));
        }
        public OpeningNativeAnimationAvatar AcceptLoadVRMUnity()
        {

            StartCoroutine(Body_AcceptLoadVRM(false));

            return openingNative;
        }
        /// <summary>
        /// Effectively load and show VRM (no demand wheather caller is HTML or Unity)
        /// </summary>
        /// <returns></returns>
        public IEnumerator Body_AcceptLoadVRM(bool isBackHTML)
        {
            bool save_IsLimitedArms = managa.IsLimitedArms;
            bool save_IsLimitedLegs = managa.IsLimitedLegs;
            bool save_IsLimitedPelvis = managa.IsLimitedPelvis;

            managa.IsLimitedPelvis = false;
            managa.IsLimitedLegs = false;
            managa.IsLimitedArms = false;

            GameObject contextRoot = pendingInstance.gameObject;
            
            //---------------------------------------------------------------------------------------------------
            //---old UniVRM
            //VRMImporterContext context = pendingContext;

            contextRoot.transform.SetParent(ParentObject.transform, true);
            contextRoot.transform.position = new Vector3(0f, 0f, 0f);
            contextRoot.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 0f));

            //context.EnableUpdateWhenOffscreen();
            pendingInstance.EnableUpdateWhenOffscreen();

            
            ManageAvatarTransform mat = contextRoot.GetComponent<ManageAvatarTransform>();
            List<GameObject> meshcnt = mat.CheckSkinnedMeshAvailable();
            GameObject mat_f = null;
            GameObject mat_b = null;
            SkinnedMeshRenderer mat_b_mesh = null;
            float hei = 0;

            if (meshcnt.Count == 1)
            {
                mat_b = meshcnt[0];
                //---Irregular VRM (ex: Abiss Horizon, etc)
                //mat_b.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                mat_b_mesh = mat_b.GetComponent<SkinnedMeshRenderer>();

                hei = mat_b_mesh.bounds.max.y;
            }
            else
            {
                //---VRM made by VRoid Studio etc
                mat_f = mat.GetFaceMesh();
                SkinnedMeshRenderer mat_f_mesh = mat_f.GetComponent<SkinnedMeshRenderer>();

                mat_b = mat.GetBodyMesh();
                mat_b_mesh = mat_b.GetComponent<SkinnedMeshRenderer>();
                //GameObject mat_h = mat.GetHairMesh();
                //if (mat_f != null) mat_f.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                //if (mat_b != null) mat_b.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;
                //if (mat_h != null) mat_h.GetComponent<SkinnedMeshRenderer>().updateWhenOffscreen = true;

                hei = mat_f_mesh.bounds.max.y;
            }



            //context.ShowMeshes();
            //olvrm.SetContext(context);
            //pendingInstance.ShowMeshes();


            yield return null;

            contextRoot.AddComponent<UniVRM10.VRM10Viewer.VRM10Blinker>();
            Animator conanime = contextRoot.GetComponent<Animator>();

            GameObject ikworld = managa.ikArea; 

            //Transform thead = conanime.GetBoneTransform(HumanBodyBones.Head);
            //Transform tfoot = conanime.GetBoneTransform(HumanBodyBones.RightFoot);

            CapsuleCollider bc = contextRoot.AddComponent<CapsuleCollider>();
            Vector3 bodyBounds = new Vector3(0, 0, 0);
            

            //---Body Mesh own
            bodyBounds.x = mat_b_mesh.bounds.size.x;
            bodyBounds.y += mat_b_mesh.bounds.size.y;
            bodyBounds.z = mat_b_mesh.bounds.size.z;

            //---set up collider

            bc.center = mat_b_mesh.bounds.center;
            bc.radius = mat_b_mesh.bounds.extents.x * 0.5f;
            bc.height = hei; // mat_b_mesh.bounds.size.y;



            //---Add neccesary Components
            Rigidbody rbd = contextRoot.AddComponent<Rigidbody>();
            rbd.isKinematic = true;

            Vrm10Instance vinst = contextRoot.GetComponent<Vrm10Instance>();
            OperateLoadedVRM olvrm = contextRoot.AddComponent<OperateLoadedVRM>();

            var testanim = contextRoot.GetComponent<Animator>();
            //Debug.Log("eye name=" + testanim.GetBoneTransform(HumanBodyBones.LeftEye).name);
            //Debug.Log("         " + vinst.Runtime.ControlRig.GetBoneTransform(HumanBodyBones.LeftEye).name);
            //Debug.Log("         " + vinst.Runtime.ControlRig.Bones[HumanBodyBones.LeftEye].ControlBone.name);
            //Debug.Log("         " + vinst.Humanoid.LeftEye.name);
            
            olvrm.SaveDefaultColliderPosition(bc.center);
            olvrm.Title = pendingVRMmeta.Name; // contextRoot.GetComponent<VRMMeta>().Meta.Title;
            //olvrm.SetVisibleAvatar(0);

            olvrm.SetActiveFace();

            olvrm.ListGravityInfo();
            olvrm.RegisterUserMaterial();
            //olvrm.ListProxyBlendShape();
            //---change material Standard to VRM10
            var umat = olvrm.ListUserMaterialObject();
            umat.ForEach(action =>
            { 
                if (action.shaderName.ToLower() != OperateLoadedBase.SHAD_VRM10)
                {
                    olvrm.SetUserMaterial(action.name + ",shader,VRM10/MToon10");
                }
                
            });

            bool useFullBodyIK = false; // configLab.GetIntVal("use_fullbody_bipedik") == 1 ? true : false;
            bool useVVMIK = true;

            //---set up IK

            //VRMLookAtHead vlook = contextRoot.GetComponent<VRMLookAtHead>();
            VRM10LookTarget vlook = contextRoot.GetComponent<VRM10LookTarget>();
            vinst.LookAtTargetType = VRM10ObjectLookAt.LookAtTargetTypes.SpecifiedTransform;          

            //---for VRIK
            /*
             * GameObject ikhandles = ikworld.GetComponent<OperateLoadedObj>().CreateIKHandle(context.Root);
            vlook.Target = ikhandles.transform.GetChild(0);

            VRIK vik = context.Root.AddComponent<VRIK>();
            SetupVRIK(ikhandles, conanime, vik, bodyBounds);
            */

            GameObject ikparent;
            BipedIK bik = null;
            //---for FullBodyBiped IK
            if (useFullBodyIK)
            {
                ikparent = ikworld.GetComponent<OperateLoadedObj>().CreateFullBodyIKHandle(contextRoot);
                //vlook.Target = ikparent.transform.GetChild(0);
                //vinst.Gaze = ikparent.transform.Find("EyeViewHandle");
                vinst.LookAtTarget = ikparent.transform.Find("EyeViewHandle");
                ikparent.GetComponent<UserGroundOperation>().relatedAvatar = contextRoot;

                FullBodyBipedIK fullik = contextRoot.AddComponent<FullBodyBipedIK>();
                RootMotion.BipedReferences biref = new RootMotion.BipedReferences();
                RootMotion.BipedReferences.AutoDetectReferences(ref biref, fullik.transform, RootMotion.BipedReferences.AutoDetectParams.Default);
                fullik.SetReferences(biref, null);
                fullik.solver.SetLimbOrientations(new RootMotion.BipedLimbOrientations(
                    new RootMotion.BipedLimbOrientations.LimbOrientation(Vector3.forward, Vector3.forward, Vector3.left),
                    new RootMotion.BipedLimbOrientations.LimbOrientation(Vector3.forward, Vector3.forward, Vector3.left),
                    new RootMotion.BipedLimbOrientations.LimbOrientation(Vector3.back, Vector3.forward, Vector3.left),
                    new RootMotion.BipedLimbOrientations.LimbOrientation(Vector3.back, Vector3.forward, Vector3.right)
                ));
                LookAtIK laik = contextRoot.AddComponent<LookAtIK>();
                CCDIK cik = contextRoot.AddComponent<CCDIK>();

                StartCoroutine(SetupFullBodyIK(ikparent, conanime, fullik, laik, cik, bodyBounds));

            }
            else
            {
                bool IsReparentBone = true;
                if (useVVMIK)
                {
                    //---for VVM IK
                    ikparent = ikworld.GetComponent<OperateLoadedObj>().CreateVVMIKHandle(contextRoot, IsReparentBone);
                    //vlook.Target = ikparent.transform.GetChild(0);
                	//vinst.Gaze = ikparent.transform.Find("EyeViewHandle");
                    vinst.LookAtTarget = ikparent.transform.Find("EyeViewHandle");
                    ikparent.GetComponent<UserGroundOperation>().relatedAvatar = contextRoot;

                    RuntimeAnimatorController ruanim = Instantiate<RuntimeAnimatorController>((RuntimeAnimatorController)Resources.Load("vvmik_anicon"));
                    conanime.runtimeAnimatorController = ruanim;

                    VvmIk vik = contextRoot.AddComponent<VvmIk>();
                    SetupVVMIK(ikparent, conanime, vik, null, bodyBounds, IsReparentBone);
                }
                else
                {
                    //---for Biped IK
                    ikparent = ikworld.GetComponent<OperateLoadedObj>().CreateBipedIKHandle(contextRoot);
                    //vlook.Target = ikparent.transform.GetChild(0);
                	vinst.Gaze = ikparent.transform.Find("EyeViewHandle");
                    ikparent.GetComponent<UserGroundOperation>().relatedAvatar = contextRoot;

                    bik = contextRoot.AddComponent<BipedIK>();
                    RootMotion.BipedReferences biref = new RootMotion.BipedReferences();
                    RootMotion.BipedReferences.AutoDetectReferences(ref biref, bik.transform, RootMotion.BipedReferences.AutoDetectParams.Default);
                    RootMotion.BipedLimbOrientations biplimb_orient = new RootMotion.BipedLimbOrientations(
                        new RootMotion.BipedLimbOrientations.LimbOrientation(Vector3.forward, Vector3.forward, Vector3.left),
                        new RootMotion.BipedLimbOrientations.LimbOrientation(Vector3.forward, Vector3.forward, Vector3.left),
                        new RootMotion.BipedLimbOrientations.LimbOrientation(Vector3.forward, Vector3.forward, Vector3.left),
                        new RootMotion.BipedLimbOrientations.LimbOrientation(Vector3.forward, Vector3.forward, Vector3.left)
                    );
                    bik.SetToDefaults();

                    //bik.solvers.AssignReferences(biref);
                    bik.references = biref;
                    //bik.SetToDefaults();
                    //bik.references.eyes = new Transform[0];

                    CCDIK cik = contextRoot.AddComponent<CCDIK>();


                    //---set null for VRM Look At Head (Not use LookAt of BipedIK)
                    StartCoroutine(SetupBipedIK(ikparent, conanime, bik, cik, bodyBounds));
                }
                

            }
            //---material

            //---for hand IK
            SetupHand(contextRoot, conanime);
            olvrm.SetRelateHandController();
            //olvrm.SetHandFingerMode("2");


            //contextRoot.transform.rotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
            OperateActiveVRM ovrm = ikworld.GetComponent<OperateActiveVRM>();
            //ovrm.ActivateAvatar(contextRoot.name,false);
            ovrm.EnableTransactionHandle(null, ikparent);
            ovrm.AddAvatarBox(contextRoot.name, null, ikparent);


            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

            float[] calcBodyInfo = mat.ParseBodyInfo(mat_b.GetComponent<SkinnedMeshRenderer>());
            List<Vector3> bodyinfoList = mat.ParseBodyInfoList(mat_b.GetComponent<SkinnedMeshRenderer>().bounds, ikparent);
            olvrm.SetTPoseBodyInfo(mat_b.GetComponent<SkinnedMeshRenderer>().bounds);
            olvrm.SetTPoseBodyList(bodyinfoList);

            NativeAnimationAvatar tmpnav = new NativeAnimationAvatar();
            tmpnav.avatar = contextRoot;
            tmpnav.avatarId = contextRoot.name;
            tmpnav.avatarTitle = pendingVRMmeta.Name;
            tmpnav.ikparent = ikparent;
            openingNative.cast = tmpnav;
          
            StartCoroutine(olvrm.SetupAdditionalExpression());
            olvrm.InitializeBlendShapeList();

            yield return null;

            if (bik != null)
            {
                bik.solvers.leftHand.bendGoal.Translate(new Vector3(0, 0, 0.1f));
                bik.solvers.rightHand.bendGoal.Translate(new Vector3(0, 0, 0.1f));
            }

            bool isOverwrite = false; // n - new, o - overwrite
            //mana.FirstAddAvatar(context.Root.name, context.Root, ikparent, "VRM", AF_TARGETTYPE.VRM, calcBodyInfo);
            NativeAnimationAvatar nav = null;
            if (isBackHTML)
            {
                nav = managa.FirstAddAvatarForVRM(out isOverwrite, contextRoot.name, contextRoot, ikparent, "VRM", AF_TARGETTYPE.VRM, calcBodyInfo, bodyinfoList);
                string js = nav.roleName + "," + nav.roleTitle + "," + (isOverwrite ? "o" : "n");
#if !UNITY_EDITOR && UNITY_WEBGL
                ReceiveStringVal(js);
#endif
                lastLoadedAvatar = nav;
            }
            pendingInstance.ShowMeshes();

            //pendingContext = null;

            //contextRoot.AddComponent<BVHRecorder>();
            mat.GenerateBvhBoneInformation(conanime);

            yield return null;
            managa.IsLimitedArms = save_IsLimitedArms;
            managa.IsLimitedLegs = save_IsLimitedLegs;
            managa.IsLimitedPelvis = save_IsLimitedPelvis;
            /*
            HumanPoseHandler hph = new HumanPoseHandler(conanime.avatar, conanime.transform);
            HumanPose hp = new HumanPose();
            hph.GetHumanPose(ref hp);
            for (int i = 0; i < hp.muscles.Length; i++)
            {
                Debug.Log(HumanTrait.MuscleName[i] + "=" + hp.muscles[i].ToString());
            }
            */
            VVMMotionRecorder vvmrec = contextRoot.AddComponent<VVMMotionRecorder>();
            vvmrec.PrepareExportVRMA(managa.transform.Find("tmpVRMA"), vinst);

            

        }

        public string DestroyVRM(string param)
        {
            string ret = "";
            GameObject ikhp = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
            GameObject[] vrm = GameObject.FindGameObjectsWithTag("Player");
            for (var i = 0; i < vrm.Length; i++)
            {
                if (vrm[i].name == param)
                {
                    GameObject ik = vrm[i].GetComponent<OperateLoadedVRM>().relatedHandleParent;
                    GameObject truik = vrm[i].GetComponent<OperateLoadedVRM>().relatedTrueIKParent;

                    OperateLoadedVRM olvrm = vrm[i].GetComponent<OperateLoadedVRM>();

                    ovrm.RemoveAvatarBox(ik);

                    if (ovrm.ActiveAvatar != null)
                    {
                        if (ovrm.ActiveAvatar.name == vrm[i].name)
                        {
                            ovrm.ActiveAvatar = null;
                        }
                    }
                    if (ovrm.ActiveIKHandle != null)
                    {
                        if (ovrm.ActiveIKHandle.name == ik.name)
                        {
                            ovrm.ActiveIKHandle = null;
                        }
                    }
                    

                    managa.DetachAvatarFromRole(param + ",avatar");

                    Destroy(truik);
                    Destroy(ik);
                    //olvrm.GetContext().Dispose();
                    Destroy(vrm[i]);

                    ret = param;

                    break;
                }
            }
            return ret;
        }
        public void DestroyVRMFromOuter(string param)
        {
            string ret = DestroyVRM(param);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }



        //=================================================================================================================================================================
        //  FBX, Obj, etc functions
        //=================================================================================================================================================================
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

            
            TriLibCore.AssetLoaderOptions opt = TriLibCore.AssetLoader.CreateDefaultLoaderOptions(); //Resources.Load<TriLibCore.AssetLoaderOptions>("myapp_assetLoadOption"); // 
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
            pick.LoadModelFromFilePickerAsync("Please select the object file", onLoad: OnLoad, onMaterialsLoad: OnMaterialsLoad, OnProgress, null, OnErrorTriLib, wrapperGameObject: objpar, assetLoaderOptions: opt);

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
            if (assetLoaderContext.Filename != null)
            {
                _loadedObjectFileName = assetLoaderContext.Filename;
            }

        }
        private void OnProgress(AssetLoaderContext assetLoaderContext, float progress)
        {

        }
        private OtherObjectInformation Body_OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
        {
            GameObject ikhp = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");


            GameObject oth = (assetLoaderContext.RootGameObject);
            oth.SetActive(true);
            oth.tag = "OtherPlayer";
            oth.layer = LayerMask.NameToLayer("Player");
            OperateLoadedOther ol = oth.AddComponent<OperateLoadedOther>();
            oth.AddComponent<ManageAvatarTransform>();
            Vector3 orirot = oth.transform.rotation.eulerAngles;
            Rigidbody rbody = oth.AddComponent<Rigidbody>();
            MouseOperationXR moxr = oth.AddComponent<MouseOperationXR>();
            rbody.drag = 10;
            rbody.angularDrag = 10;
            rbody.useGravity = false;
            rbody.isKinematic = false;


            ol.childCount = oth.transform.childCount;

            GameObject copycube = (GameObject)Resources.Load("IKHandleCube");
            GameObject ikcube = Instantiate(copycube, copycube.transform.position, Quaternion.identity, ikhp.transform);
            OtherObjectDummyIK oodk = ikcube.GetComponent<OtherObjectDummyIK>();
            oodk.relatedAvatar = oth;
            oodk.relatedType = AF_TARGETTYPE.OtherObject;
            ikcube.tag = "IKHandle";
            ikcube.transform.rotation = Quaternion.Euler(orirot); // Quaternion.Euler(0f, 0f, 0f);

            ol.relatedHandleParent = ikcube;


            _loadedGameObject = oth;


            //-------------------------------------
            // Enumrate Material
            OtherObjectInformation obi = OnShowedOtherObject(oth);

            //---change ID to timestamp 
            //oth.name = "obj_" + DateTime.Now.ToFileTime().ToString();  //_loadedObjectFileName == "" ? oth.name : _loadedObjectFileName;
            oth.name = managa.CheckAndSetAvatarId("obj_",oth.GetInstanceID());
            ikcube.name = "ikparent_" + oth.name;

            obi.id = oth.name;
            obi.Title = _loadedObjectFileName; // oth.name;
            obi.fileExt = assetLoaderContext.FileExtension;
            obi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.OtherObject);

            ol.Title = obi.Title;


            bool isOverwrite = false; // n - new, o - overwrite

            NativeAnimationAvatar nav = managa.FirstAddAvatarForFileObject(out isOverwrite, oth.name, oth, ikcube, "OtherObject", AF_TARGETTYPE.OtherObject, obi.Title);
            nav.ext = assetLoaderContext.FileExtension;
            nav.path = assetLoaderContext.Filename;
            obi.roleName = nav.roleName;
            obi.roleTitle = nav.roleTitle;

            //string js = JsonUtility.ToJson(obi);
            //Debug.Log(js);
            obi.isOverwrite = (isOverwrite ? "o" : "n");

            OperateActiveVRM ovrm = managa.ikArea.GetComponent<OperateActiveVRM>();
            ovrm.EnableTransactionHandle(null, ikcube);
            ovrm.AddAvatarBox(oth.name, null, ikcube);

            string fext = assetLoaderContext.FileExtension;
            fext = (fext == "") ? "OTH:OBJECT" : "OTH:" + fext.ToUpper();

            ol.objectType = fext;

            NativeAnimationAvatar tmpnav = new NativeAnimationAvatar();
            tmpnav.avatar = oth;
            tmpnav.avatarId = oth.name;
            tmpnav.avatarTitle = obi.Title;
            tmpnav.ikparent = ikcube;
            openingNative.cast = tmpnav;
            openingNative.baseInfo = obi;

            lastLoadedAvatar = nav;

            return obi;
        }
        private void OnMaterialsLoad4HTML(AssetLoaderContext assetLoaderContext)
        {
            OtherObjectInformation obi = Body_OnMaterialsLoad(assetLoaderContext);

            string js = JsonUtility.ToJson(obi);
            string fext = assetLoaderContext.FileExtension;
#if !UNITY_EDITOR && UNITY_WEBGL
            sendOtherObjectInfo(fext, js);
#endif
        }
        private void OnMaterialsLoad(AssetLoaderContext assetLoaderContext)
        {
            OtherObjectInformation obi = Body_OnMaterialsLoad(assetLoaderContext);

            string js = JsonUtility.ToJson(obi);
            string fext = assetLoaderContext.FileExtension;

        }
        private void OnErrorTriLib4HTML(IContextualizedError ErrorContext)
        {
            Debug.LogError(ErrorContext.GetInnerException());
#if !UNITY_EDITOR && UNITY_WEBGL
            if (IsBackToHTML) ReceiveStringVal(ErrorContext.GetInnerException().Message);
#endif
        }
        private void OnErrorTriLib(IContextualizedError ErrorContext)
        {
            Debug.LogError(ErrorContext.GetInnerException());

        }
        public OtherObjectInformation OnShowedOtherObject(GameObject oth, bool isLoadedObject = true)
        {
            //GameObject oth = _loadedGameObject;

            OperateLoadedOther olo = oth.GetComponent<OperateLoadedOther>();



            olo.RegisterUserMaterial();

            OtherObjectInformation ret = new OtherObjectInformation();


            //---Check for animation (target object itself!!
            Animation anim;
            //Animator animt;
            if (oth.TryGetComponent<Animation>(out anim))
            {
                
                if (anim.clip)
                {
                    anim.playAutomatically = false;
                    anim.Stop();
                    anim.wrapMode = WrapMode.Default;

                    ret.animationName = anim.clip.name;
                    ret.animationLength = anim.clip.length;
                    ret.animationWrapMode = anim.wrapMode;
                    ret.animationType = AnimationType.Legacy;

                    //-------------------------------------------------
                    // below: will use futurely Trilib full-support New Animator System

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

            BoxCollider tmpbc;
            SphereCollider tmpsc;
            CapsuleCollider tmpccc;
            MeshCollider tmpmcc;
            if (cnt == 0)
            {
                
                /*if (
                    (!oth.TryGetComponent<BoxCollider>(out tmpbc)) && (!oth.TryGetComponent<SphereCollider>(out tmpsc)) && (!oth.TryGetComponent<CapsuleCollider>(out tmpccc))
                )
                { //---if any collider not found, forcely add BoxCollider
                    oth.AddComponent<BoxCollider>();
                }*/

                /*
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
                }*/
            }
            if (oth.TryGetComponent<BoxCollider>(out tmpbc))
            {

            }
            else
            {
                if (oth.TryGetComponent<SphereCollider>(out tmpsc))
                {

                }
                else if (oth.TryGetComponent<CapsuleCollider>(out tmpccc))
                {

                }
                else if (oth.TryGetComponent<MeshCollider>(out tmpmcc))
                {

                }
                else
                {
                    //---if any collider not found, forcely add BoxCollider
                    oth.AddComponent<BoxCollider>();
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

            //ret.materials = oomis.ToArray();

            //_loadedGameObject = null;

            if (isLoadedObject == true)
            {
                BoxCollider boxc;
                if (oth.TryGetComponent<BoxCollider>(out boxc))
                {
                    boxc.isTrigger = true;
                    //boxc.size = Vector3.zero;

                    // すべての子のBoxColliderの位置とサイズを取得する
                    List<Bounds> bounds = new List<Bounds>();
                    foreach (Transform child in oth.transform)
                    {
                        BoxCollider tmpcol;
                        if (child.TryGetComponent<BoxCollider>(out tmpcol))
                        {
                            bounds.Add(tmpcol.bounds);
                        }

                    }
                    // MinとMaxを取得する
                    Vector3 min = Vector3.zero;
                    Vector3 max = Vector3.zero;
                    foreach (Bounds bound in bounds)
                    {
                        min = Vector3.Min(min, bound.min);
                        max = Vector3.Max(max, bound.max);
                    }

                    // 親のBoxColliderのサイズを設定する
                    boxc.size = new Vector3(max.x - min.x, max.y - min.y, max.z - min.z);

                    // 親のBoxColliderの位置を設定する
                    boxc.center = new Vector3(0, boxc.size.y * 0.5f, 0);
                }
            }
            
            



            return ret;
        }
        private void enumMaterialFuncBody(GameObject child, ref List<OtherObjectMaterialInfo> oomis, ref OperateLoadedOther olo)
        {
            //---For collider judge, add Collider ( effective, use the parent of Collider object )
            child.tag = "OtherPlayerCollider";
            child.layer = LayerMask.NameToLayer("Player");

            BoxCollider boxc;
            SphereCollider tmpsc;
            CapsuleCollider tmpccc;
            MeshCollider tmpmcc;
            if (child.TryGetComponent<BoxCollider>(out boxc))
            {
                
            }
            else
            {
                if (child.TryGetComponent<SphereCollider>(out tmpsc))
                {

                }
                else if (child.TryGetComponent<CapsuleCollider>(out tmpccc))
                {

                }
                else if (child.TryGetComponent<MeshCollider>(out tmpmcc))
                {

                }
                else
                {
                    //---if any collider not found, forcely add BoxCollider
                    boxc = child.AddComponent<BoxCollider>();
                    boxc.isTrigger = true;
                }
            }


            /*
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

            */

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
            string[] prm = uri.Split('\t');
            string url = prm[0];
            string filename = prm[1];
            string ext = TriLibCore.Utils.FileUtils.GetFileExtension(filename, false);
            //Debug.Log("filename=[" + filename + "]");
            //Debug.Log("ext=[" + ext + "]");
            _loadedObjectFileName = filename;

            var webRequest = AssetDownloader.CreateWebRequest(url.Replace("blob:",""));
            AssetDownloader.LoadModelFromUri(webRequest, 
                onLoad: OnLoad, 
                onMaterialsLoad: OnMaterialsLoad, null, 
                onError: OnErrorTriLib,  
                wrapperGameObject: objpar, assetLoaderOptions: opt, fileExtension: ext
            );
        }

        /// <summary>
        /// Load other 3D object from Unity
        /// </summary>
        /// <param name="url"></param>
        public OpeningNativeAnimationAvatar LoadOtherObject(string url)
        {
            StartCoroutine(LoadOtherObject_Body(false, url));

            return openingNative;
        }
        /// <summary>
        /// Load other 3D object from HTML
        /// </summary>
        /// <param name="url"></param>
        public void LoadOtherObjectURI(string url)
        {
            StartCoroutine(LoadOtherObject_Body(true, url));
            
        }
        private IEnumerator LoadOtherObject_Body(bool isBackHTML, string url)
        {
            //Debug.Log(url);
            string[] prm = url.Split('\t');
            string uri = prm[0];
            string filename = prm[1];
            string ext = prm[2]; // TriLibCore.Utils.FileUtils.GetFileExtension(filename, false); // System.IO.Path.GetExtension(filename);
            _loadedObjectFileName = filename;

            using (UnityWebRequest www = UnityWebRequest.Get(uri))
            {
                DownloadHandlerBuffer dhb = new DownloadHandlerBuffer();
                www.downloadHandler = dhb;

                yield return www.SendWebRequest();
                //if (www.isNetworkError || www.isHttpError)
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError(www.error);
                    yield break;
                }
                else
                {
                    GameObject objpar = managa.AvatarArea; //GameObject.Find("View Body");

                    AssetLoaderOptions options = AssetLoader.CreateDefaultLoaderOptions();
                    //options.ExternalDataMapper = ScriptableObject.CreateInstance<FilePickerExternalDataMapper>();
                    //options.TextureMappers.Append(ScriptableObject.CreateInstance<FilePickerTextureMapper>());
                    

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
                    EffectiveLoadAssetLoader(isBackHTML, opstream, filename, ext, objpar, options, ill);
                }
            }
        }
        private void EffectiveLoadAssetLoader(bool isBackHTML, Stream opstream, string filename, string ext, GameObject objectParent, AssetLoaderOptions options, IList<ItemWithStream> ill)
        {
            if (isBackHTML)
            {
                //---effectively open
                if (ext == "zip")
                {
                    AssetLoaderZip.LoadModelFromZipStream(opstream,
                        OnLoad, OnMaterialsLoad4HTML,
                        null, OnErrorTriLib4HTML,
                        objectParent, options, ill
                    );
                }
                else
                {
                    AssetLoader.LoadModelFromStream(opstream, filename: filename, fileExtension: ext,
                        OnLoad, OnMaterialsLoad4HTML, null,
                        OnErrorTriLib4HTML,
                        objectParent, options, ill
                    );
                }
            }
            else
            {
                //--- Unity only ---
                //---effectively open
                if (ext == "zip")
                {
                    AssetLoaderZip.LoadModelFromZipStream(opstream,
                        OnLoad, OnMaterialsLoad,
                        null, OnErrorTriLib,
                        objectParent, options, ill
                    );
                }
                else
                {
                    AssetLoader.LoadModelFromStream(opstream, filename: filename, fileExtension: ext,
                        OnLoad, OnMaterialsLoad, null,
                        OnErrorTriLib,
                        objectParent, options, ill
                    );
                }
            }
            

        }
        public string DestroyOther(string param)
        {
            string ret = "";
            GameObject ikhp = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
            
            GameObject[] vrm = GameObject.FindGameObjectsWithTag("OtherPlayer");
            GameObject oth = GameObject.Find(param);
            //Debug.Log(param);
            if (oth != null)
            {
                OperateLoadedOther olvrm = oth.GetComponent<OperateLoadedOther>();
                GameObject ik = olvrm.relatedHandleParent;

                if (olvrm.IsPlayingAnimation() == 1)
                {
                    olvrm.StopAnimation();
                }

                ovrm.RemoveAvatarBox(ik);

                if (ovrm.ActiveAvatar != null)
                {
                    if (ovrm.GetEffectiveActiveAvatar().name == oth.name)
                    {
                        ovrm.ActiveAvatar = null;
                    }
                }
                if (ovrm.ActiveIKHandle != null)
                {
                    if (ovrm.ActiveIKHandle.name == ik.name)
                    {
                        ovrm.ActiveIKHandle = null;
                    }
                }
                

                managa.DetachAvatarFromRole(param + ",avatar");

                ret = oth.name;
                Destroy(ik);
                Destroy(oth);

                NativeAnimationAvatar nav = managa.GetCastInProject("Stage");
                if (nav != null)
                {
                    OperateStage ost = nav.avatar.GetComponent<OperateStage>();
                    ost.ManageWaterComponent();


                }
            }
            return ret;
        }
        public void DestroyOtherFromOuter(string param)
        {
            string ret = DestroyOther(param);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public NativeAnimationAvatar CreateBlankQuad()
        {
            return CreateBlankCube(UserPrimitiveType.Cube);
        }
        public NativeAnimationAvatar CreateBlankObject(int param)
        {
            /*
             * 
             * 0 - sphere, 1 - capsule, 2 - cylinder, 3 - cube, 4 - plane, 5 - quad, 6 - water level
             */
            return CreateBlankCube((UserPrimitiveType)param);
        }
        public OpeningNativeAnimationAvatar Body_CreateBlankCube(UserPrimitiveType ptype)
        {
            OpeningNativeAnimationAvatar oap = new OpeningNativeAnimationAvatar();

            string[] pritype = { "BlankSphere", "BlankCapsule", "BlankCylinder", "BlankCube", "BlankPlane", "BlankQuad", "BlankWaterLevel" };
            GameObject ikhp = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");

            GameObject copyprim = (GameObject)Resources.Load(pritype[(int)ptype]);
            GameObject copyoth = Instantiate(copyprim, copyprim.transform.position, Quaternion.identity, managa.AvatarArea.transform);  //GameObject.CreatePrimitive(ptype);
            copyoth.name = "BlankObject";

            MeshRenderer mr = copyprim.GetComponentInChildren<MeshRenderer>();


            GameObject oth = new GameObject();

            copyoth.transform.SetParent(oth.transform);

            //---avatar object
            oth.transform.SetParent(managa.AvatarArea.transform);
            oth.name = managa.CheckAndSetAvatarId("obj_",oth.GetInstanceID()); // + DateTime.Now.ToFileTime().ToString();  //_loadedObjectFileName == "" ? oth.name : _loadedObjectFileName;
            oth.tag = "OtherPlayer";
            oth.layer = LayerMask.NameToLayer("Player");
            OperateLoadedOther ol = oth.AddComponent<OperateLoadedOther>();
            oth.AddComponent<ManageAvatarTransform>();
            Rigidbody rbody = oth.AddComponent<Rigidbody>();
            MouseOperationXR moxr = oth.AddComponent<MouseOperationXR>();
            rbody.drag = 10;
            rbody.angularDrag = 10;
            rbody.useGravity = false;
            rbody.isKinematic = false;

            if (ptype == UserPrimitiveType.Cube)
            {
                BoxCollider boxc = oth.AddComponent<BoxCollider>();
                boxc.isTrigger = true;
                BoxCollider copyboxc = copyoth.GetComponent<BoxCollider>();
                boxc.size = copyboxc.size;
                boxc.center = Vector3.zero;
            }
            else if (
                (ptype == UserPrimitiveType.Cylinder) ||
                (ptype == UserPrimitiveType.Capsule)
            )
            {
                CapsuleCollider capsuleCollider = oth.AddComponent<CapsuleCollider>();
                capsuleCollider.isTrigger = true;
                CapsuleCollider copycap = copyoth.GetComponent<CapsuleCollider>();
                capsuleCollider.radius = copycap.radius;
                capsuleCollider.height = copycap.height;
            }
            else if (ptype == UserPrimitiveType.Sphere)
            {
                SphereCollider sphereCollider = oth.AddComponent<SphereCollider>();
                sphereCollider.isTrigger = true;
                SphereCollider copysph = copyoth.GetComponent<SphereCollider>();
                sphereCollider.radius = copysph.radius;
            }
            

            //--- Create unique material for This Blank Object
            Material mat = new Material(mr.sharedMaterial);
            mat.name = "mat_" + oth.name;
            MeshRenderer cmr = copyoth.GetComponentInChildren<MeshRenderer>();
            if (cmr != null)
            {
                cmr.sharedMaterial = mat;
            }
            

            //---ikparent
            GameObject copycube = (GameObject)Resources.Load("IKHandleCube");
            GameObject ikcube = Instantiate(copycube, copycube.transform.position, Quaternion.identity, ikhp.transform);
            ikcube.name = "ikparent_" + oth.name;
            ikcube.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            ikcube.tag = "IKHandle";

            //---addional setting
            ol.relatedHandleParent = ikcube;
            ol.Title = ptype.ToString();
            ol.objectType = ptype.ToString();
            OtherObjectDummyIK oodk = ikcube.GetComponent<OtherObjectDummyIK>();
            oodk.relatedAvatar = oth;
            oodk.relatedType = AF_TARGETTYPE.OtherObject;

            int existCnt = managa.CheckRoleTitleExist(ptype.ToString(), AF_TARGETTYPE.OtherObject);
            if (existCnt > 0)
            {
                ol.Title = ptype.ToString() + "_" + existCnt;
            }

            //---material save
            OtherObjectInformation obi = OnShowedOtherObject(oth,false);
            obi.id = oth.name;
            obi.Title = ol.Title;
            obi.fileExt = ((int)ptype).ToString();
            obi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.OtherObject);

            OperateActiveVRM ovrm = managa.ikArea.GetComponent<OperateActiveVRM>();
            ovrm.EnableTransactionHandle(null, ikcube);
            ovrm.AddAvatarBox(oth.name, null, ikcube);


            NativeAnimationAvatar nav = new NativeAnimationAvatar();
            nav.avatar = oth;
            nav.ikparent = ikcube;
            nav.avatarId = oth.name;
            nav.avatarTitle = obi.Title;
            nav.roleTitle = obi.Title;


            oap.cast = nav;
            oap.baseInfo = obi;

            return oap;
        }
        public NativeAnimationAvatar CreateBlankCube(UserPrimitiveType ptype)
        {
            OpeningNativeAnimationAvatar tmpnav = Body_CreateBlankCube(ptype);

            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = managa.FirstAddAvatar(tmpnav.cast.avatar.name, tmpnav.cast.avatar, tmpnav.cast.ikparent, "OtherObject", AF_TARGETTYPE.OtherObject);
            nav.path = "%BLANK%";
            nav.ext = ((int)ptype).ToString();
            nav.roleTitle = tmpnav.cast.roleTitle;


            tmpnav.baseInfo.roleName = nav.roleName;
            tmpnav.baseInfo.roleTitle = nav.roleTitle;
            tmpnav.baseInfo.Title = nav.roleTitle;

            string js = JsonUtility.ToJson(tmpnav.baseInfo);
            //Debug.Log(js);


            string fext = "";
            fext = (fext == "") ? "OTH:OBJECT" : "OTH:" + fext.ToUpper();
            
#if !UNITY_EDITOR && UNITY_WEBGL
            sendOtherObjectInfo(fext, js);
#endif
            return nav;


        }
        //=============================================================================================================================
        //  Light functions
        //=============================================================================================================================
        public NativeAnimationAvatar OpenSpotLight()
        {
            return OpenLightObject("spot");
        }
        public NativeAnimationAvatar OpenPointLight()
        {
            return OpenLightObject("point");
        }
        public OpeningNativeAnimationAvatar Body_OpenLightObject(string param)
        {
            OpeningNativeAnimationAvatar onav = new OpeningNativeAnimationAvatar();

            GameObject ikworld = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            GameObject[] lt = ikworld.GetComponent<OperateLoadedObj>().CreateLight(param);


            lt[0].name = managa.CheckAndSetAvatarId("lit_",lt[0].GetInstanceID()); // + DateTime.Now.ToFileTime().ToString();
            lt[0].tag = "LightPlayer";
            lt[0].layer = LayerMask.NameToLayer("Player");
            OperateLoadedLight ol = lt[0].AddComponent<OperateLoadedLight>();
            lt[0].AddComponent<ManageAvatarTransform>();

            lt[1].name = "ikparent_" + lt[0].name;
            lt[1].tag = "IKHandle";
            lt[1].layer = LayerMask.NameToLayer("Handle");

            ol.relatedHandleParent = lt[1];
            ol.Title = param + " light";

            int existCnt = managa.CheckRoleTitleExist(ol.Title, AF_TARGETTYPE.Light);
            if (existCnt > 0)
            {
                ol.Title = ol.Title + "_" + existCnt.ToString();
            }


            NativeAnimationAvatar nav = new NativeAnimationAvatar();
            nav.avatar = lt[0];
            nav.ikparent = lt[1];
            nav.avatarId = lt[0].name;
            nav.avatarTitle = ol.Title;
            nav.roleTitle = ol.Title;


            OperateActiveVRM ovrm = managa.ikArea.GetComponent<OperateActiveVRM>();
            ovrm.EnableTransactionHandle(null, nav.ikparent);
            ovrm.AddAvatarBox(nav.avatar.name, null, nav.ikparent);


            Light light = nav.avatar.GetComponent<Light>();
            BasicObjectInformation loi = new BasicObjectInformation();
            loi.id = nav.avatar.name;
            loi.Title = ol.Title; // Enum.GetName(typeof(LightType), light.type);
            loi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.Light);
            loi.motion.lightType = light.type;
            loi.motion.range = light.range;
            loi.motion.power = light.intensity;
            loi.motion.color = light.color;
            loi.motion.spotAngle = light.spotAngle;


            onav.cast = nav;
            onav.baseInfo = loi;

            return onav;
        }
        public NativeAnimationAvatar OpenLightObject(string param)
        {
            //---new 
            OpeningNativeAnimationAvatar tmpnav = Body_OpenLightObject(param);

            /*
            Light light = tmpnav.avatar.GetComponent<Light>();
            BasicObjectInformation loi = new BasicObjectInformation();
            loi.id = tmpnav.avatar.name;
            loi.Title = Enum.GetName(typeof(LightType), light.type);
            loi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.Light);
            loi.motion.lightType = light.type;
            loi.motion.range = light.range;
            loi.motion.power = light.intensity;
            loi.motion.color = light.color;
            loi.motion.spotAngle = light.spotAngle;
            */

            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = managa.FirstAddAvatar(tmpnav.cast.avatar.name, tmpnav.cast.avatar, tmpnav.cast.ikparent, "Light", AF_TARGETTYPE.Light);
            nav.ext = param;
            nav.roleTitle = tmpnav.cast.roleTitle;

            tmpnav.baseInfo.roleName = nav.roleName;
            tmpnav.baseInfo.roleTitle = nav.roleTitle;


            string js = JsonUtility.ToJson(tmpnav.baseInfo);

            string fext = "LIGHT";
#if !UNITY_EDITOR && UNITY_WEBGL
            sendOtherObjectInfo(fext, js);
#endif
            return nav;
        }
        public string DestroyLight(string param)
        {
            string ret = "";
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

                    if (ovrm.ActiveAvatar != null)
                    {
                        if (ovrm.ActiveAvatar.name == vrm[i].name)
                        {
                            ovrm.ActiveAvatar = null;
                        }
                    }
                    if (ovrm.ActiveIKHandle != null)
                    {
                        if (ovrm.ActiveIKHandle.name == ik.name)
                        {
                            ovrm.ActiveIKHandle = null;
                        }
                    }
                    

                    managa.DetachAvatarFromRole(param + ",avatar");

                    ret = vrm[i].name;
                    Destroy(ik);
                    Destroy(vrm[i]);

                    break;
                }
            }
            return ret;
        }
        public void DestroyLightFromOuter(string param)
        {
            string ret = DestroyLight(param);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        //=============================================================================================================================
        //  Camera functions
        //=============================================================================================================================
        public NativeAnimationAvatar OnClickBtnAddCamera()
        {
            return CreateCameraObject("");
        }
        public OpeningNativeAnimationAvatar Body_CreateCameraObject(string param)
        {
            OpeningNativeAnimationAvatar onav = new OpeningNativeAnimationAvatar();

            GameObject ikworld = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            GameObject[] lt = ikworld.GetComponent<OperateLoadedObj>().CreateCamera();


            lt[0].name = managa.CheckAndSetAvatarId("cam_",lt[0].GetInstanceID()); // + DateTime.Now.ToFileTime().ToString();
            lt[0].tag = "CameraPlayer";
            lt[0].layer = LayerMask.NameToLayer("Player");
            OperateLoadedCamera ol = lt[0].AddComponent<OperateLoadedCamera>();
            lt[0].AddComponent<ManageAvatarTransform>();
            

            lt[1].name = "ikparent_" + lt[0].name;
            lt[1].tag = "IKHandle";
            lt[1].layer = LayerMask.NameToLayer("Handle");

            ol.relatedHandleParent = lt[1];
            ol.Title = "Camera";

            int existCnt = managa.CheckRoleTitleExist(ol.Title, AF_TARGETTYPE.Camera);
            if (existCnt > 0)
            {
                ol.Title = ol.Title + "_" + existCnt.ToString();
            }

            OperateActiveVRM ovrm = managa.ikArea.GetComponent<OperateActiveVRM>();
            ovrm.EnableTransactionHandle(null, lt[1]);
            ovrm.AddAvatarBox(lt[0].name, null, lt[1]);

            NativeAnimationAvatar nav = new NativeAnimationAvatar();
            nav.avatar = lt[0];
            nav.ikparent = lt[1];
            nav.roleTitle = ol.Title;
            nav.avatarId = lt[0].name;
            nav.avatarTitle = ol.Title;


            Camera cam = nav.avatar.GetComponent<Camera>();

            CameraObjectInformation loi = new CameraObjectInformation();
            loi.id = nav.avatar.name;
            loi.Title = ol.Title;
            loi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.Camera);
            loi.motion.fov = cam.fieldOfView;
            loi.motion.depth = cam.depth;
            loi.motion.viewport = cam.rect;

            onav.cast = nav;
            onav.baseInfo = loi;

            return onav;
        }
        public NativeAnimationAvatar CreateCameraObject(string param)
        {
            OpeningNativeAnimationAvatar tmpnav = Body_CreateCameraObject(param);

            /*
            Camera cam = tmpnav.avatar.GetComponent<Camera>();

            CameraObjectInformation loi = new CameraObjectInformation();
            loi.id = tmpnav.avatar.name;
            loi.Title = "Camera";
            loi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.Camera);
            loi.motion.fov = cam.fieldOfView;
            loi.motion.depth = cam.depth;
            loi.motion.viewport = cam.rect;
            */

            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = managa.FirstAddAvatar(tmpnav.cast.avatar.name, tmpnav.cast.avatar, tmpnav.cast.ikparent, "Camera", AF_TARGETTYPE.Camera);
            tmpnav.baseInfo.roleName = nav.roleName;
            tmpnav.baseInfo.roleTitle = nav.roleTitle;

            string js = JsonUtility.ToJson(tmpnav.baseInfo);

            string fext = "CAMERA";
#if !UNITY_EDITOR && UNITY_WEBGL
            sendOtherObjectInfo(fext, js);
#endif
            return nav;
        }
        public string DestroyCamera(string param)
        {
            string ret = "";
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

                    if (ovrm.ActiveAvatar != null)
                    {
                        if (ovrm.ActiveAvatar.name == vrm[i].name)
                        {
                            ovrm.ActiveAvatar = null;
                        }
                    }
                    if (ovrm.ActiveIKHandle != null)
                    {
                        if (ovrm.ActiveIKHandle.name == ik.name)
                        {
                            ovrm.ActiveIKHandle = null;
                        }
                    }
                    

                    managa.DetachAvatarFromRole(param + ",avatar");

                    ret = vrm[i].name;
                    Destroy(ik);
                    Destroy(vrm[i]);


                    break;
                }
            }
            return ret;
        }
        public void DestroyCameraFromOuter(string param)
        {
            string ret = DestroyCamera(param);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        //=============================================================================================================================
        //  Text functions
        //=============================================================================================================================
        public NativeAnimationAvatar OpenSample1Text()
        {
            NativeAnimationAvatar avatar = null;
            DOVirtual.DelayedCall(0.0001f, async () =>
            {
                avatar = await OpenText("ABC,tl,2");
            });
            return avatar;
        }
        public NativeAnimationAvatar OpenSample2Text()
        {
            NativeAnimationAvatar avatar = null;
            DOVirtual.DelayedCall(0.0001f, async () =>
            {
                avatar = await OpenText("ABCabc,tl,3");
            });
            return avatar;
        }
        public async System.Threading.Tasks.Task<OpeningNativeAnimationAvatar> OpenRecoveryText(string param)
        {
            
            var task = await Body_OpenText(param);

            return task;
        }
        public async System.Threading.Tasks.Task<OpeningNativeAnimationAvatar> Body_OpenText(string param)
        {
            GameObject ikhp = managa.ikArea;
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

            OpeningNativeAnimationAvatar onav = new OpeningNativeAnimationAvatar();

            string[] prm = param.Split(',');

            int dimension = int.TryParse(prm[2], out dimension) ? dimension : 2;

            GameObject ikworld = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            
            GameObject[] txt = await ikworld.GetComponent<OperateLoadedObj>().CreateText(prm[0], prm[1], dimension);

            OperateLoadedText olt = txt[0].GetComponent<OperateLoadedText>();

            txt[0].tag = "TextPlayer";
            if (dimension == 2)
            {
                txt[0].name = managa.CheckAndSetAvatarId("txt_", txt[0].GetInstanceID()); // + DateTime.Now.ToFileTime().ToString();
                txt[0].layer = LayerMask.NameToLayer("UserUI");
            }
            else if (dimension == 3)
            {
                txt[0].name = managa.CheckAndSetAvatarId("txt3d_", txt[0].GetInstanceID()); // + DateTime.Now.ToFileTime().ToString();
                txt[0].layer = LayerMask.NameToLayer("Player");

            }

            txt[0].AddComponent<ManageAvatarTransform>();
            
            //RectTransform rectra = txt[0].GetComponent<RectTransform>();
            //rectra.anchoredPosition3D = new Vector3(0f, 0f, 0f);
            //rectra.localScale = new Vector3(1f, 1f, 1f);


            

            //---setup ik marker (if dimension is 3D)
            if (dimension == 3)
            {
                txt[1].name = "ikparent_" + txt[0].name;
                txt[1].tag = "IKHandle";
                txt[1].layer = LayerMask.NameToLayer("Handle");

                txt[1].AddComponent<MouseOperationXR>();

                olt.relatedHandleParent = txt[1];
                olt.Title = "Text3D";

                ovrm.EnableTransactionHandle(null, txt[1]);
                ovrm.AddAvatarBox(txt[0].name, null, txt[1]);

            }else if (dimension == 2)
            {
                olt.Title = "Text";
            }


            //---basic information
            //Text text = txt.GetComponent<Text>();
            //TextMeshProUGUI text = txt[0].GetComponent<TextMeshProUGUI>();
            TextObjectInformation toi = new TextObjectInformation();
            toi.id = txt[0].name;
            
            if (dimension == 2)
            {
                toi.Title = "Text";
                toi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.Text);
            }
            else if (dimension == 3)
            {
                toi.Title = "Text3D";
                toi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.Text3D);
            }
            
            toi.motion.fontSize = olt.GetFontSize();
            //toi.motion.fontStyle = text.fontStyle;
            toi.motion.fontStyles = olt.GetFontStyles();
            //toi.motion.textAlignment = text.alignment;
            toi.motion.textAlignmentOptions = "tl";
            toi.motion.textOverflow = (int)TMPro.TextOverflowModes.Overflow;
            toi.motion.dimension = dimension.ToString() + "d";
            

            int existCnt = managa.CheckRoleTitleExist(toi.Title, AF_TARGETTYPE.Text);
            if (existCnt > 0)
            {
                toi.Title = toi.Title + "_" + existCnt.ToString();
            }

            NativeAnimationAvatar nav = new NativeAnimationAvatar();
            nav.avatar = txt[0];
            if (dimension == 2)
            {
                nav.ikparent = null;
            }
            else if (dimension == 3)
            {
                nav.ikparent = txt[1];
            }
            nav.roleTitle = toi.Title;            
            nav.avatarId = txt[0].name;
            nav.avatarTitle = toi.Title;


            onav.cast = nav;
            onav.baseInfo = toi;

            

            return onav;
        }
        /// <summary>
        /// Create and Show Text UI
        /// </summary>
        /// <param name="param">CSV-string: 0=text label, 1=anchor position(tl=TopLeft, bl=BottomLeft, tr=TopRight, br=BottomRight), 2=dimension(2, 3)</param>
        public async System.Threading.Tasks.Task<NativeAnimationAvatar> OpenText(string param)
        {

            OpeningNativeAnimationAvatar tmpnav = await Body_OpenText(param);


            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = null;
            
            if (tmpnav.baseInfo.motion.dimension == "2d")
            {
                nav = managa.FirstAddAvatar(tmpnav.cast.avatar.name, tmpnav.cast.avatar, tmpnav.cast.ikparent, "Text", AF_TARGETTYPE.Text);
            }
            else if (tmpnav.baseInfo.motion.dimension == "3d")
            {
                nav = managa.FirstAddAvatar(tmpnav.cast.avatar.name, tmpnav.cast.avatar, tmpnav.cast.ikparent, "Text", AF_TARGETTYPE.Text3D);
            }
                
            tmpnav.baseInfo.roleName = nav.roleName;
            tmpnav.baseInfo.roleTitle = nav.roleTitle;

            string js = JsonUtility.ToJson(tmpnav.baseInfo);


            string fext = "TEXT";
#if !UNITY_EDITOR && UNITY_WEBGL
            sendOtherObjectInfo(fext, js);
#endif
            return nav;
        }
        public string DestroyText(string param)
        {
            string ret = "";
            GameObject msgarea = managa.MsgArea; // GameObject.Find("MsgArea");
            
            for (var i = 0; i < msgarea.transform.childCount; i++)
            {
                GameObject vrm = msgarea.transform.GetChild(i).gameObject;
                if (vrm.name == param)
                {
                    managa.DetachAvatarFromRole(param + ",avatar");

                    managa.ikArea.GetComponent<OperateLoadedObj>().RelaseGeneralAssetRef(vrm);
                    Destroy(vrm);

                    ret = vrm.name;

                    break;
                }
            }
            return ret;

        }
        public void DestroyTextFromOuter(string param)
        {
            string ret = DestroyText(param);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public string DestroyText3D(string param)
        {
            string ret = "";
            //---3D text
            GameObject ikhp = managa.ikArea;
            OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
            GameObject[] vrms = GameObject.FindGameObjectsWithTag("TextPlayer");
            for (var i = 0; i < vrms.Length; i++)
            {
                if (vrms[i].name == param)
                {
                    OperateLoadedText olt = vrms[i].GetComponent<OperateLoadedText>();

                    GameObject ik = olt.relatedHandleParent;


                    ovrm.RemoveAvatarBox(ik);

                    if (ovrm.ActiveAvatar != null)
                    {
                        if (ovrm.ActiveAvatar.name == vrms[i].name)
                        {
                            ovrm.ActiveAvatar = null;
                        }
                    }
                    if (ovrm.ActiveIKHandle != null)
                    {
                        if (ovrm.ActiveIKHandle.name == ik.name)
                        {
                            ovrm.ActiveIKHandle = null;
                        }
                    }


                    managa.DetachAvatarFromRole(param + ",avatar");

                    ret = vrms[i].name;
                    Destroy(ik);
                    managa.ikArea.GetComponent<OperateLoadedObj>().RelaseGeneralAssetRef(vrms[i]);
                    Destroy(vrms[i]);


                    break;
                }
            }
            return ret;
        }
        public void DestroyText3DFromOuter(string param)
        {
            string ret = DestroyText3D(param);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        //=============================================================================================================================
        //  Image functions
        //=============================================================================================================================
        public void OnClickBtnImage()
        {
            //InputField inpf2 = GameObject.Find("inpf2").GetComponent<InputField>();
            ////ImageFileSelected(inpf2.text);
            //UIImageFileSelected(inpf2.text);

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
                byte[] byt = new byte[stm.Length];
                stm.Read(byt, 0, (int)stm.Length);
                StartCoroutine(DownloadImage_body(byt, false, paths[i].Name, paths[i].Name));
                //Debug.Log(paths[i]);
            }


        }
        public void OnClickBtnUImage()
        {
            //InputField inpf2 = GameObject.Find("inpf2").GetComponent<InputField>();
            ////ImageFileSelected(inpf2.text);
            //UIImageFileSelected(inpf2.text);

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
                byte[] byt = new byte[stm.Length];
                stm.Read(byt, 0, (int)stm.Length);
                StartCoroutine(DownloadUIImage_body(byt,false, paths[i].Name, paths[i].Name));
                //Debug.Log(paths[i]);
            }


        }

        public void ImageFileSelected(string url)
        {
            StartCoroutine(LoadImageuri(url,false,true));
        }
        public void UIImageFileSelected(string url)
        {
            StartCoroutine(LoadImageuri(url,true,true));
        }

        /// <summary>
        /// Load image file from Unity
        /// </summary>
        /// <param name="url"></param>
        public OpeningNativeAnimationAvatar LoadImageFile(string url)
        {
            StartCoroutine(LoadImageuri(url, false, false));

            return openingNative;
        }
        /// <summary>
        /// Load UI image file from Unity
        /// </summary>
        /// <param name="url"></param>
        public OpeningNativeAnimationAvatar LoadUImageFile(string url)
        {
            StartCoroutine(LoadImageuri(url, true, false));

            return openingNative;
        }
        public IEnumerator LoadImageuri(string url, bool is_ui, bool isBackHTML)
        {
            string[] prm = url.Split('\t');
            string uri = prm[0];
            string filename = prm[1];
            string ext = TriLibCore.Utils.FileUtils.GetFileExtension(filename, false); // System.IO.Path.GetExtension(filename);

            using (UnityWebRequest www = UnityWebRequest.Get(uri))
            {
                yield return www.SendWebRequest();
                //if (www.isNetworkError || www.isHttpError)
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogError(www.error);
                    yield break;
                }
                else
                {
                    if (is_ui)
                    {
                        StartCoroutine(DownloadUIImage_body(www.downloadHandler.data, isBackHTML, filename, ext));
                    }
                    else
                    {
                        StartCoroutine(DownloadImage_body(www.downloadHandler.data, isBackHTML, filename, ext));
                    }
                    
                }
            }
        }
        public IEnumerator DownloadUIImage_body(byte[] data, bool isBackHTML, string filename, string extension)
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

            img.name = managa.CheckAndSetAvatarId("uimg_",img.GetInstanceID()); // + DateTime.Now.ToFileTime().ToString();
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
            ioi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.UImage);
            ioi.location = "ui";
            ioi.width = tex.width;
            ioi.height = tex.height;

            string fext = "UIMAGE";
            //---Set up for Animation
            bool isOverwrite = false;
            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = managa.FirstAddAvatarForFileObject(out isOverwrite, img.name, img, null, "UImage", AF_TARGETTYPE.UImage, filename);
            ioi.Title = nav.roleTitle;
            ioi.roleName = nav.roleName;
            ioi.roleTitle = nav.roleTitle;
            ioi.isOverwrite = (isOverwrite ? "o" : "n");
            if (isBackHTML)
            {
                string js = JsonUtility.ToJson(ioi);
#if !UNITY_EDITOR && UNITY_WEBGL
                sendOtherObjectInfo(fext, js);
#endif

            }
            lastLoadedAvatar = nav;

            NativeAnimationAvatar tmpnav = new NativeAnimationAvatar();
            tmpnav.avatar = img;
            tmpnav.ikparent = null;
            tmpnav.avatarId = img.name;
            tmpnav.avatarTitle = ioi.Title;
            tmpnav.roleTitle = ioi.Title;


            openingNative.cast = tmpnav;
            openingNative.baseInfo = ioi;

            yield return null;
        }
        public string DestroyUImage(string param)
        {
            string ret = "";
            GameObject imgarea = managa.ImgArea; // GameObject.Find("ImgArea");

            for (var i = 0; i < imgarea.transform.childCount; i++)
            {
                GameObject vrm = imgarea.transform.GetChild(i).gameObject;
                if (vrm.name == param)
                {
                    managa.DetachAvatarFromRole(param + ",avatar");

                    ret = vrm.name;
                    Destroy(vrm);


                    break;
                }
            }
            return ret;
        }
        public void DestroyUImageFromOuter(string param)
        {
            string ret = DestroyUImage(param);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public IEnumerator DownloadImage_body(byte[] data, bool isBackHTML, string filename, string extension)
        {
            GameObject viewBody = managa.AvatarArea; // GameObject.Find("View Body");
            GameObject ikhp = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");

            //---load image as texture
            Texture2D tex = new Texture2D(1,1);
            Material mat = new Material(Shader.Find("Unlit/Transparent"));

            //tex = ((DownloadHandlerTexture)handler).texture;
            tex.LoadImage(data);
            mat.SetTexture("_MainTex", tex);
            mat.SetFloat("_Mode", 2f);

            yield return null;

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
            BoxCollider boxc;
            if (plat.TryGetComponent<BoxCollider>(out boxc))
            { //---physical effect off
                boxc.isTrigger = true;
            }

            //---create dummy 3D object for operating
            GameObject empt = new GameObject();
            empt.transform.SetParent(viewBody.transform);

            empt.name = managa.CheckAndSetAvatarId("img_",empt.GetInstanceID()); // + DateTime.Now.ToFileTime().ToString();
            empt.tag = "OtherPlayer";
            empt.layer = LayerMask.NameToLayer("Player");
            plat.transform.SetParent(empt.transform);

            //---Set up IK handle
            OperateLoadedOther ol = empt.AddComponent<OperateLoadedOther>();
            empt.AddComponent<ManageAvatarTransform>();

            //---call common other object editing
            OnShowedOtherObject(empt);
            BoxCollider emptbox = empt.GetComponent<BoxCollider>();
            emptbox.size = new Vector3(plat.transform.localScale.x, plat.transform.localScale.y, plat.transform.localScale.z);
            emptbox.center = Vector3.zero;

            Rigidbody rbody = empt.AddComponent<Rigidbody>();
            MouseOperationXR moxr = empt.AddComponent<MouseOperationXR>();
            rbody.drag = 10;
            rbody.angularDrag = 10;
            rbody.useGravity = false;
            rbody.isKinematic = false;

            GameObject copycube = (GameObject)Resources.Load("IKHandleCube");
            GameObject ikcube = Instantiate(copycube, copycube.transform.position, Quaternion.identity, ikhp.transform);
            ikcube.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            ikcube.tag = "IKHandle";
            ikcube.name = "ikparent_" + empt.name;

            ol.relatedHandleParent = ikcube;
            ol.Title = "Image object";
            OtherObjectDummyIK oodk = ikcube.GetComponent<OtherObjectDummyIK>();
            oodk.relatedAvatar = empt;
            oodk.relatedType = AF_TARGETTYPE.Image;

            OperateActiveVRM ovrm = managa.ikArea.GetComponent<OperateActiveVRM>();
            ovrm.EnableTransactionHandle(null, ikcube);
            ovrm.AddAvatarBox(empt.name, null, ikcube);

            //---return information to HTML
            ImageObjectInformation ioi = new ImageObjectInformation();
            ioi.id = empt.name;
            ioi.location = "3d";
            ioi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.Image);
            ioi.width = tex.width;
            ioi.height = tex.height;

            string fext = "IMAGE";
            ol.objectType = fext;

            //---Set up for Animation
            bool isOverwrite = false;
            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = managa.FirstAddAvatarForFileObject(out isOverwrite, empt.name, empt, ikcube, "Image", AF_TARGETTYPE.Image, filename);
            ioi.Title = nav.roleTitle;
            ioi.roleName = nav.roleName;
            ioi.roleTitle = nav.roleTitle;
            ioi.isOverwrite = (isOverwrite ? "o" : "n");

            if (isBackHTML)
            {
                string js = JsonUtility.ToJson(ioi);
#if !UNITY_EDITOR && UNITY_WEBGL
                sendOtherObjectInfo(fext, js);
#endif

            }
            lastLoadedAvatar = nav;

            NativeAnimationAvatar tmpnav = new NativeAnimationAvatar();
            tmpnav.avatar = empt;
            tmpnav.ikparent = ikcube;
            tmpnav.avatarId = empt.name;
            tmpnav.avatarTitle = ioi.Title;
            tmpnav.roleTitle = ioi.Title;


            openingNative.cast = tmpnav;
            openingNative.baseInfo = ioi;

            //yield return null;
        }
        public Texture LoadImage_bytebody(byte[] data)
        {

            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);

            return tex;
        }
        public string DestroyImage(string param)
        {
            string ret = DestroyOther(param);
            return ret;

        }
        public void DestroyImageFromOuter(string param)
        {
            DestroyOtherFromOuter(param);
        }
        //=============================================================================================================================
        //  Effect functions
        //=============================================================================================================================
        public OpeningNativeAnimationAvatar Body_CreateSingleEffect()
        {
            OpeningNativeAnimationAvatar onav = new OpeningNativeAnimationAvatar();

            GameObject ikworld = managa.ikArea; // GameObject.FindGameObjectWithTag("IKHandleWorld");
            GameObject[] lt = ikworld.GetComponent<OperateLoadedObj>().CreateEffect();

            lt[0].name = managa.CheckAndSetAvatarId("eff_", lt[0].GetInstanceID()); // + DateTime.Now.ToFileTime().ToString();
            lt[0].tag = "EffectDestination";
            lt[0].layer = LayerMask.NameToLayer("Player");
            lt[0].AddComponent<ManageAvatarTransform>();

            lt[1].name = "ikparent_" + lt[0].name;
            lt[1].tag = "IKHandle";
            lt[1].layer = LayerMask.NameToLayer("Handle");

            OperateLoadedEffect ole = lt[0].GetComponent<OperateLoadedEffect>();
            ole.relatedHandleParent = lt[1];
            ole.Title = "Effect";

            int existCnt = managa.CheckRoleTitleExist(ole.Title, AF_TARGETTYPE.Effect);
            if (existCnt > 0)
            {
                ole.Title = ole.Title + "_" + existCnt.ToString();
            }

            OperateActiveVRM ovrm = managa.ikArea.GetComponent<OperateActiveVRM>();
            ovrm.EnableTransactionHandle(null, lt[1]);
            ovrm.AddAvatarBox(lt[0].name, null, lt[1]);

            NativeAnimationAvatar nav = new NativeAnimationAvatar();
            nav.avatar = lt[0];
            nav.ikparent = lt[1];
            nav.avatarId = lt[0].name;
            nav.avatarTitle = ole.Title;

            BasicObjectInformation boi = new BasicObjectInformation();
            boi.id = nav.avatar.name;
            boi.Title = ole.Title;
            boi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.Effect);

            onav.cast = nav;
            onav.baseInfo = boi;

            return onav;
        }
        public NativeAnimationAvatar CreateSingleEffect()
        {
            OpeningNativeAnimationAvatar tmpnav = Body_CreateSingleEffect();

            /*
            BasicObjectInformation boi = new BasicObjectInformation();
            boi.id = tmpnav.avatar.name;
            boi.Title = "Effect";
            boi.type = Enum.GetName(typeof(AF_TARGETTYPE), AF_TARGETTYPE.Effect);
            */

            //ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            NativeAnimationAvatar nav = managa.FirstAddAvatar(tmpnav.cast.avatar.name, tmpnav.cast.avatar, tmpnav.cast.ikparent, "Effect", AF_TARGETTYPE.Effect);
            tmpnav.baseInfo.roleName = nav.roleName;
            tmpnav.baseInfo.roleTitle = nav.roleTitle;

            string js = JsonUtility.ToJson(tmpnav.baseInfo);

            string fext = "EFFECT";
#if !UNITY_EDITOR && UNITY_WEBGL
                sendOtherObjectInfo(fext, js);
#endif
            return nav;

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

        public string DestroyEffect(string param)
        {
            string ret = "";
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

                    if (ovrm.ActiveAvatar != null)
                    {
                        if (ovrm.GetEffectiveActiveAvatar().name == vrm[i].name)
                        {
                            ovrm.ActiveAvatar = null;
                        }
                    }
                    if (ovrm.ActiveIKHandle != null)
                    {
                        if (ovrm.ActiveIKHandle.name == ik.name)
                        {
                            ovrm.ActiveIKHandle = null;
                        }
                    }
                    

                    managa.DetachAvatarFromRole(param + ",avatar");

                    ret = vrm[i].name;
                    Destroy(vrm[i]);
                    Destroy(ik);
                    break;
                }
            }
            return ret;
        }
        public void DestroyEffectFromOuter(string param)
        {
            string ret = DestroyEffect(param);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif

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
                OpenBGM(paths[i].Name+"\t"+ paths[i].Name);
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
            string[] prm = url.Split('\t');
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

            //Debug.Log(okaudiotype);
            if (okaudiotype != AudioType.UNKNOWN)
            {
                //Debug.Log("url:" + prm[0] + "\n" + okaudiotype);
                
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
                        Debug.LogError(www.error);
                        yield break;
                    }
                    else
                    {
                        AudioClip ac = dhan.audioClip;
                        if (ac != null)
                        {
                            //Debug.Log(ac.GetType());
                            //Debug.Log(ac.ToString());
                        }
                        else
                        {
                            Debug.LogError("Audio failed...");
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
            string js = BoxName + "," + filename + "," + ac.samples.ToString() + "," + ac.length.ToString();

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
            

        }
    }
}