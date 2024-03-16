using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using DG.Tweening;
using UnityStandardAssets.Water;

namespace UserHandleSpace
{
    public enum StageKind
    {
        Default = 0,        //special stage 1
        BasicSeaLevel,
        SeaDaytime,
        SeaNight,
        DryGround,
        Desert,
        Field1,
        Field2,
        Field3,
        Field4,
        User
    };
    [Serializable]
    public class VVStageMapPoint
    {
        public float height;
        public float[] paintedTerrain;
        public int[] details;
        public VVStageMapPoint(float defaultHeight, int terrainLayerCnt, int detaiLayerCnt)
        {
            height = defaultHeight;
            paintedTerrain = new float[terrainLayerCnt];
            for (int i = 0; i < paintedTerrain.Length; i++) paintedTerrain[i] = 0f;
            paintedTerrain[0] = 1f;
            details = new int[detaiLayerCnt];
            for (int i = 0; i < detaiLayerCnt; i++) details[i] = 0;
        }
    }
    [Serializable]
    public class VVStageMap
    {
        public int width = 129;
        public int height = 129;
        List<List<VVStageMapPoint>> heightInfo;
        public VVStageMap(float defaultHeight, int terrainCnt, int detailCnt, int w = 129, int h = 129)
        {
            heightInfo = new List<List<VVStageMapPoint>>();
            for (int y = 0; y < h; y++)
            {
                List<VVStageMapPoint> mapRow = new List<VVStageMapPoint>();
                for (int x = 0; x < w; x++)
                {
                    mapRow.Add(new VVStageMapPoint(defaultHeight, terrainCnt, detailCnt));
                }
                heightInfo.Add(mapRow);
            }
        }
        public void dispose()
        {

        }
        public void SetMapPoint(int x, int y, VVStageMapPoint point)
        {
            heightInfo[y][x] = point;
        }
        public void SetOffsetMapData(int sx, int sy, VVStageMapPoint[,] data)
        {

        }
    }

    public class OperateStage : MonoBehaviour
    {

        [DllImport("__Internal")]
        private static extern void ReceiveObjectVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);

        string[] StageNames = {"DefaultStage", "BasicSeaLevel","DaytimeWaterStage", "NighttimeWaterStage", "DryGroundStage",
            "DesertStage","FieldStage1", "FieldStage2", "FieldStage3", "FieldStage4","UserStage"
        };

        public GameObject StageParent;
        public List<GameObject> StageList;
        public GameObject ActiveStage;
        private StageKind ActiveStageType;
        private AsyncOperationHandle<GameObject> ActiveTargetStageHandle;

        public string ActiveUserStageMainTextureName;
        public string ActiveUserStageBumpmapTextureName;

        protected Vector3 oldPosition;
        protected Quaternion oldRotation;
        protected Vector3 defaultPosition;
        protected Quaternion defaultRotation;

        protected Vector3 defaultSystemLightRotation;

        private Terrain editStage;
        const float DEFAULT_BASEHEIGHT = 0.033342486245f;
        const float HEIGHT_CHANGEVAL = 0.001f;

        private VVStageMap editStageMap;

        protected Color finalDefaultColor;
        protected MaterialProperties finalMatprop;
        protected MaterialProperties finalUserStageMatprop;

        [SerializeField]
        GameObject wxrCamSet;
        ManageAnimation manim;

        // Start is called before the first frame update
        void Start()
        {
            StageParent = GameObject.FindGameObjectWithTag("GroundWorld");
            manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

            ActiveStageType = StageKind.Default;
            //ActiveStage = StageList[0]; // StageParent.transform.Find("DefaultStage").gameObject;
            //await SelectStageRef(0);

            ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            //mana.FirstAddFixedAvatar(gameObject.name, gameObject, gameObject, "Stage", AF_TARGETTYPE.Stage);

            /*
            if (StageList[1] != null)
            {
                editStage = StageList[1].GetComponent<Terrain>();
                editStageMap = new VVStageMap(DEFAULT_BASEHEIGHT, editStage.terrainData.alphamapLayers, editStage.terrainData.alphamapWidth, editStage.terrainData.alphamapHeight);

            }
            */


            //ResetEditableStage();
            //SetStageTest("");

            finalDefaultColor = Color.white;

            ActiveUserStageMainTextureName = "";
            ActiveUserStageBumpmapTextureName = "";
            finalMatprop = new MaterialProperties();
            finalUserStageMatprop = new MaterialProperties();

            defaultSystemLightRotation = GetSystemDirectionalLight().GetRotation();
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnDestroy()
        {
            RelaseStageRef();

            if (ActiveUserStageMainTextureName != "")
            {
                MeshRenderer mesh = ActiveStage.GetComponent<MeshRenderer>();
                manim.UnReferMaterial(OneMaterialType.Texture, ActiveUserStageMainTextureName);
                mesh.material.SetTexture("_MainTex", null);
            }
            if (ActiveUserStageBumpmapTextureName != "")
            {
                MeshRenderer mesh = ActiveStage.GetComponent<MeshRenderer>();
                manim.UnReferMaterial(OneMaterialType.Texture, ActiveUserStageBumpmapTextureName);
                mesh.material.SetTexture("_BumpMap", null);
            }
        }
        public virtual void GetCommonTransformFromOuter()
        {
            Vector3 pos = GetPositionFromOuter(0);
            Vector3 rot = GetRotationFromOuter(0);
            Vector3 sca = GetScale(0);
            string ret = "";
            ret = pos.x + "," + pos.y + "," + pos.z + "%" +
                rot.x + "," + rot.y + "," + rot.z + "%" +
                sca.x + "," + sca.y + "," + sca.z + "%" +
                "0,0,0"
            ;
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public void SetDefault()
        {
            SelectStage((int)StageKind.Default);
            GetCameraOperation().SetDefaultSky();
            OperateLoadedLight oll = GetSystemDirectionalLight();
            
            oll.SetDefault();
            oll.SetRotation(defaultSystemLightRotation);
            GetWindzone().SetDefault();
        }
        private void SetLayerRecursive(GameObject self, int layer)
        {
            self.layer = layer;
            foreach (Transform tra in self.transform)
            {
                SetLayerRecursive(tra.gameObject, layer);
            }
        }
        public void SetVisibleAvatar(int flag)
        {
            const string TARGET_SHOWLAYER = "Stage";
            const string TARGET_HIDDENLAYER = "HiddenPlayer";

            if (flag == 0)
            { //---avatar hide!
                //gameObject.layer = LayerMask.NameToLayer(TARGET_HIDDENLAYER);
                SetLayerRecursive(ActiveStage, LayerMask.NameToLayer(TARGET_HIDDENLAYER));
            }
            else
            { //---avatar show!
                //gameObject.layer = LayerMask.NameToLayer(TARGET_SHOWLAYER);
                SetLayerRecursive(ActiveStage, LayerMask.NameToLayer(TARGET_SHOWLAYER));
            }
        }

        //----------------------------------------------------------------------------------------
        public List<string> ListStage()
        {
            /*List<string> ret = new List<string>();
            StageList.ForEach(item =>
            {
                ret.Add(item.name);
            });*/
            List<string> ret = new List<string>(StageNames);
            return ret;
        }
        
        public void ListStageFromOuter()
        {
            List<string> ret = ListStage();
            string js = string.Join(",", ret);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(js);
#endif
        }
        public GameObject FindStageByType(StageKind skind)
        {
            GameObject ret = null;

            ret = StageList.Find(item =>
            {
                if (item.name == StageNames[(int)skind]) return true;
                return false;
            });

            return ret;
        }
        public void SelectStage(int param)
        {
            /*StageList.ForEach(action =>
            {
                action.SetActive(false);
            });
            if ((0 <= param) && (param < StageList.Count)) {
                ActiveStage = StageList[param];
                ActiveStageType = (StageKind)param;
                ActiveStage.SetActive(true);
            }*/
            if ((0 <= param) && (param < StageNames.Length))
            {
                if (ActiveStageType == (StageKind)param) return;

                RelaseStageRef();

                ActiveStageType = (StageKind)param;
                if (ActiveStageType == StageKind.Default)
                {
                    ActiveStage = StageList[0];
                    ActiveStage.SetActive(true);
                    Material mat = ActiveStage.GetComponent<Renderer>().materials[0];
                    mat.color = Color.white;
                }
                else if (ActiveStageType == StageKind.User)
                {
                    GameObject tmps = FindStageByType(ActiveStageType);
                    if (tmps != null)
                    {
                        ActiveStage = tmps;
                        ActiveStage.SetActive(true);
                    }

                }
                else if (
                        (ActiveStageType == StageKind.BasicSeaLevel) ||
                        (ActiveStageType == StageKind.SeaDaytime) ||
                        (ActiveStageType == StageKind.SeaNight)
                    )
                {
                    GameObject tmps = FindStageByType(ActiveStageType);
                    if (tmps != null)
                    {
                        ActiveStage = tmps;
                        ActiveStage.SetActive(true);

                        List<MaterialProperties> mats = ListUserMaterialObject();
                        if (mats.Count > 0) SetUserMaterialObject(mats[0]);
                    }


                }
            }
        }
        public StageKind GetActiveStageType(int is_contacthtml = 1)
        {
            StageKind ret = ActiveStageType;

#if !UNITY_EDITOR && UNITY_WEBGL
            if (is_contacthtml == 1)
            {
                ReceiveIntVal((int)ret);
            }
#endif
            return ret;
        }
        public void SelectStageFromOuter(int param)
        {
            StageKind skind = (StageKind)param;
            if (
                (skind == StageKind.Default) ||
                (skind == StageKind.User) ||
                (skind == StageKind.BasicSeaLevel) ||
                (skind == StageKind.SeaDaytime) ||
                (skind == StageKind.SeaNight)
            )
            {
                SelectStage(param);
                
            }
            else
            {
                DOVirtual.DelayedCall(0.0001f, async () =>
                {
                    await SelectStageRef(param);
                });
            }
            
        }
        public async System.Threading.Tasks.Task<GameObject> SelectStageRef(int param)
        {
            if ((0 <= param) && (param < StageNames.Length))
            {
                if (ActiveStageType == (StageKind)param) return ActiveStage;

                RelaseStageRef();

                //ActiveStage = StageList[param];
                ActiveStageType = (StageKind)param;
                if (ActiveStageType == StageKind.Default)
                {
                    /*ActiveStage = StageList[0];
                    ActiveStage.SetActive(true);
                    Material mat = ActiveStage.GetComponent<Renderer>().materials[0];
                    mat.color = Color.white;*/
                }
                else if (ActiveStageType == StageKind.User)
                {
                    /*GameObject tmps = FindStageByType(ActiveStageType);
                    if (tmps != null)
                    {
                        ActiveStage = tmps;
                        ActiveStage.SetActive(true);
                    }*/
                    
                }
                else if (
                        (ActiveStageType == StageKind.BasicSeaLevel) ||
                        (ActiveStageType == StageKind.SeaDaytime) ||
                        (ActiveStageType == StageKind.SeaNight)
                    )
                {
                    /*GameObject tmps = FindStageByType(ActiveStageType);
                    if (tmps != null)
                    {
                        ActiveStage = tmps;
                        ActiveStage.SetActive(true);

                        List<MaterialProperties> mats = ListUserMaterialObject();
                        if (mats.Count > 0) SetUserMaterialObject(mats[0]);
                    }

                    */
                }
                else
                {
                    string sname = "Stage/" + StageNames[param];
                    //Debug.Log(sname);

                    ActiveTargetStageHandle = Addressables.InstantiateAsync(sname);

                    System.Threading.Tasks.Task<GameObject> eff = ActiveTargetStageHandle.Task;
                    ActiveStage = await eff;

                    
                  
                }
                

                return ActiveStage;
            }
            else
            {
                return null;

            }
            
        }
        
        public void RelaseStageRef()
        {
            if (
                (ActiveStageType == StageKind.Default) ||
                (ActiveStageType == StageKind.BasicSeaLevel) ||
                (ActiveStageType == StageKind.SeaDaytime) ||
                (ActiveStageType == StageKind.SeaNight) ||
                (ActiveStageType == StageKind.User)
            )
            {
                ActiveStage.SetActive(false);
            }
            else
            {
                if (ActiveStage != null)
                {
                    Addressables.ReleaseInstance(ActiveTargetStageHandle);
                    GameObject.Destroy(ActiveStage);
                }
            }
            ActiveStage = null;
        }
        
        //-----------------------------------------------------------------------------------------------
        public Material GetActiveStageMaterial()
        {
            Renderer r = ActiveStage.GetComponentInChildren<Renderer>();
            if (!r)
            {
                return null;
            }
            Material mat = r.sharedMaterial;
            if (!mat)
            {
                return null;
            }
            return mat;
        }
        public Material GetStageMaterial()
        {
            Renderer r = GetComponent<Renderer>();
            if (!r)
            {
                return null;
            }
            Material mat = r.sharedMaterial;
            if (!mat)
            {
                return null;
            }
            return mat;
        }

        public Color GetDefaultStageColor(int is_contacthtml = 1)
        {
            Color col = Color.white;
            if (ActiveStageType == StageKind.Default)
            {
                col = finalDefaultColor; // ActiveStage.GetComponent<MeshRenderer>().sharedMaterial.color;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            if (is_contacthtml == 1)
            {
                ReceiveStringVal(ColorUtility.ToHtmlStringRGBA(col));
            }
#endif
            return col;
        }

        /// <summary>
        /// Set saving color only
        /// </summary>
        /// <param name="param"></param>
        public void SetDefaultStageColorObject(Color param)
        {
            finalDefaultColor = param;
        }
        public void SetDefaultStageColor(string param)
        {
            if (ActiveStageType == StageKind.Default)
            {
                MeshRenderer mesh = ActiveStage.GetComponent<MeshRenderer>();

                //Debug.Log(mesh.sharedMaterial.color);
                Color color = default(Color);
                if (ColorUtility.TryParseHtmlString(param, out color))
                {
                    ActiveStage.GetComponent<MeshRenderer>().sharedMaterial.color = color;
                }
            }
        }
        public void SetDefaultStageColor(Color param)
        {
            if (ActiveStageType == StageKind.Default)
            {
                MeshRenderer mesh = ActiveStage.GetComponent<MeshRenderer>();

                mesh.sharedMaterial.color = param;

            }
        }
        public Sequence SetDefaultStageColorTween(Sequence seq, Color param, float duration)
        {
            if (ActiveStageType == StageKind.Default)
            {
                MeshRenderer mesh = ActiveStage.GetComponent<MeshRenderer>();

                seq.Join(mesh.sharedMaterial.DOColor(param, duration));

            }
            return seq;
        }
        //------------------------------------------------------------------------------------------------
        public float GetFloatUserStage(string param)
        {
            float ret = 0;

            if (ActiveStageType == StageKind.User)
            {
                MeshRenderer mesh = ActiveStage.GetComponent<MeshRenderer>();
                if (mesh.materials.Length > 0)
                {
                    if (param == "renderingtype")
                    {
                        ret = mesh.material.GetFloat("_Mode");
                    }
                    else if (param == "metallic")
                    {
                        ret = mesh.material.GetFloat("_Metallic");
                    }
                    else if (param == "glossiness")
                    {
                        ret = mesh.material.GetFloat("_Glossiness");
                    }
                }
            }

            return ret;
        }
        public Color GetColorUserStage(string param)
        {
            Color ret = Color.white;
            if (ActiveStageType == StageKind.User)
            {
                MeshRenderer mesh = ActiveStage.GetComponent<MeshRenderer>();
                if (mesh.materials.Length > 0)
                {
                    if (param == "color")
                    {
                        ret = mesh.material.GetColor("_Color");
                    }
                    else if (param == "emissioncolor")
                    {
                        ret = mesh.material.GetColor("_EmissionColor");

                    }
                }
            }


            return ret;
        }
        public Material userStageMaterial
        {
            get
            {
                return ActiveStage.GetComponent<MeshRenderer>().material;
            }
        }


        /// <summary>
        /// get material's property. return format: name=value
        /// </summary>
        public void GetMaterialUserStageFromOuter()
        {
            string ret = "";
            if (ActiveStageType == StageKind.User)
            {
                MeshRenderer mesh = ActiveStage.GetComponent<MeshRenderer>();
                if (mesh.materials.Length > 0)
                {
                    ret += "color=#" + ColorUtility.ToHtmlStringRGBA(finalUserStageMatprop.color) + "\t";
                    ret += "renderingtype=" + finalUserStageMatprop.blendmode.ToString() + "\t";
                    ret += "metallic=" + finalUserStageMatprop.metallic.ToString() + "\t";
                    ret += "glossiness=" + finalUserStageMatprop.glossiness.ToString() + "\t";
                    ret += "emissioncolor=#" + ColorUtility.ToHtmlStringRGBA(finalUserStageMatprop.emissioncolor) + "\t";
                    ret += "maintex" + ActiveUserStageMainTextureName + "\t";
                    ret += "normaltex" + ActiveUserStageBumpmapTextureName;
                }
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);

#endif
        }

        /// <summary>
        /// Set value for UI only
        /// </summary>
        /// <param name="param"></param>
        public void SetMaterialObjectToUserStage(string param)
        {
            string[] prm = param.Split(',');
            float val = float.TryParse(prm[1], out val) ? val : 0f;

            if (prm[0] == "color")
            {
                Color col = ColorUtility.TryParseHtmlString(prm[1], out col) ? col : Color.white;
                finalUserStageMatprop.color = col;
            }
            else if (prm[0] == "renderingtype")
            {
                finalUserStageMatprop.blendmode = val;
            }
            else if (prm[0] == "metallic")
            {
                finalUserStageMatprop.metallic = val;
            }
            else if (prm[0] == "glossiness")
            {
                finalUserStageMatprop.glossiness = val;
            }
            else if (prm[0] == "emissioncolor")
            {
                Color col = ColorUtility.TryParseHtmlString(prm[1], out col) ? col : Color.white;
                finalUserStageMatprop.emissioncolor = col;
            }
            else if (prm[0] == "main")
            {
                if (ActiveUserStageMainTextureName != prm[1])
                {
                    
                    NativeAP_OneMaterial nap = manim.FindTexture(prm[1]);
                    if (nap != null)
                    {
                        ActiveUserStageMainTextureName = prm[1];
                    }
                }

            }
            else if (prm[0] == "normal")
            {
                if (ActiveUserStageBumpmapTextureName != prm[1])
                {
                    NativeAP_OneMaterial nap = manim.FindTexture(prm[1]);
                    if (nap != null)
                    {
                        ActiveUserStageBumpmapTextureName = prm[1];
                    }
                }

            }
        }
        public void SetMaterialToUserStage(string param)
        {
            string[] prm = param.Split(',');
            float val = float.TryParse(prm[1], out val) ? val : 0f;

            if (ActiveStageType == StageKind.User)
            {
                MeshRenderer mesh = ActiveStage.GetComponent<MeshRenderer>();
                if (mesh.materials.Length > 0)
                {
                    if (prm[0] == "color")
                    {
                        Color col = ColorUtility.TryParseHtmlString(prm[1], out col) ? col : Color.white;
                        mesh.material.SetColor("_Color", col);
                    }
                    else if (prm[0] == "renderingtype")
                    {
                        mesh.material.SetFloat("_Mode", val);
                    }
                    else if (prm[0] == "metallic")
                    {
                        mesh.material.SetFloat("_Metallic", val);
                    }
                    else if (prm[0] == "glossiness")
                    {
                        mesh.material.SetFloat("_Glossiness", val);
                    }
                    else if (prm[0] == "emissioncolor")
                    {
                        Color col = ColorUtility.TryParseHtmlString(prm[1], out col) ? col : Color.white;
                        mesh.material.EnableKeyword("_EMISSION");
                        mesh.material.SetColor("_EmissionColor", col);

                    }
                }
            }
        }
       
        public void SetTextureToUserStage(string param)
        {
            string[] prm = param.Split(',');
            if (ActiveStageType == StageKind.User)
            {
                MeshRenderer mesh = ActiveStage.GetComponent<MeshRenderer>();
                if (mesh.materials.Length > 0)
                {
                    //StartCoroutine(LoadImageuri(prm[1], mesh.material, prm[0]));

                    //---new version
                    if (prm[0] == "main")
                    {
                        if (ActiveUserStageMainTextureName != prm[1])
                        {
                            manim.UnReferMaterial(OneMaterialType.Texture, ActiveUserStageMainTextureName);
                            mesh.material.SetTexture("_MainTex", null);

                            NativeAP_OneMaterial nap = manim.FindTexture(prm[1]);
                            if (nap != null)
                            {
                                mesh.material.SetTexture("_MainTex", nap.ReferTexture2D());
                                ActiveUserStageMainTextureName = prm[1];
                            }
                        }
                        
                    }
                    else if (prm[0] == "normal")
                    {
                        if (ActiveUserStageBumpmapTextureName != prm[1])
                        {
                            manim.UnReferMaterial(OneMaterialType.Texture, ActiveUserStageBumpmapTextureName);
                            mesh.material.SetTexture("_BumpMap", null);

                            NativeAP_OneMaterial nap = manim.FindTexture(prm[1]);
                            if (nap != null)
                            {
                                mesh.material.EnableKeyword("_NORMALMAP");
                                mesh.material.SetTexture("_BumpMap", nap.ReferTexture2D());
                                ActiveUserStageBumpmapTextureName = prm[1];
                            }
                        }
                        
                    }
                    
                    
                }
            }
        }
        private IEnumerator LoadImageuri(string url, Material mat, string texname)
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
                    StartCoroutine(DownloadTextureImage(www.downloadHandler.data, mat, texname));
                }
            }
        }
        public IEnumerator DownloadTextureImage(byte[] data, Material mat, string texname)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(data);

            if (texname == "main")
            {
                Texture tmp = mat.GetTexture("_MainTex");
                if (tmp != null) Destroy(tmp);
                mat.SetTexture("_MainTex", tex);
            }
            else if (texname == "normal")
            {
                Texture tmp = mat.GetTexture("_BumpMap");
                if (tmp != null) Destroy(tmp);
                mat.EnableKeyword("_NORMALMAP");
                mat.SetTexture("_BumpMap", tex);
            }

            yield return null;
        }
        //------------------------------------------------------------------------------------------------
        public void SaveDefaultTransform(bool ispos, bool isrotate)
        {
            if (ispos) defaultPosition = ActiveStage.transform.position;
            if (isrotate) defaultRotation = ActiveStage.transform.rotation;
        }
        public void LoadDefaultTransform(bool ispos, bool isrotate)
        {
            if (ispos) ActiveStage.transform.position = defaultPosition;
            if (isrotate) ActiveStage.transform.rotation = defaultRotation;
        }
        //---Transform for manual operation--------------------------------------------=============
        public Vector3 GetPositionFromOuter(int is_contacthtml = 1)
        {
            Vector3 ret;
            ret = ActiveStage.transform.position;

#if !UNITY_EDITOR && UNITY_WEBGL
            if (is_contacthtml == 1) ReceiveStringVal(JsonUtility.ToJson(ret));
#endif

            return ret;
        }
        public void SetPositionFromOuter(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;
            bool isabs = prm[3] == "1" ? true : false;


            ActiveStage.transform.DOMove(new Vector3(x, y, z), 0.1f).SetRelative(!isabs);

        }
        public Vector3 GetRotationFromOuter(int is_contacthtml = 1)
        {
            Vector3 ret;
            ret = ActiveStage.transform.rotation.eulerAngles;

#if !UNITY_EDITOR && UNITY_WEBGL
            if (is_contacthtml == 1) ReceiveStringVal(JsonUtility.ToJson(ret));
#endif

            return ret;

        }
        public void SetRotationFromOuter(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;
            bool isabs = prm[3] == "1" ? true : false;


            ActiveStage.transform.DORotate(new Vector3(x, y, z), 0.1f).SetRelative(!isabs);

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
            ActiveStage.transform.DOScale(new Vector3(x, y, z), 0.1f);

        }
        public Vector3 GetScale(int is_contacthtml = 1)
        {
            Vector3 ret = Vector3.zero;

            ret = ActiveStage.transform.localScale;

            string jstr = JsonUtility.ToJson(ret);
#if !UNITY_EDITOR && UNITY_WEBGL
            if (is_contacthtml == 1) ReceiveStringVal(jstr);
#endif
            return ret;
        }

        //=-----------------------------------------------------------------==============================================
        public void SetWaterWaveSpeed(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;
            float w = float.TryParse(prm[3], out w) ? w : 0f;

            Vector4 vec = new Vector4(x, y, z, w);

            if ((ActiveStageType == StageKind.SeaDaytime) || (ActiveStageType == StageKind.SeaNight))
            {
                Renderer r = GetComponent<Renderer>();
                if (!r)
                {
                    return;
                }
                Material mat = r.sharedMaterial;
                if (!mat)
                {
                    return;
                }
                mat.SetVector("WaveSpeed", vec);

            }
            else if (ActiveStageType == StageKind.BasicSeaLevel)
            {
                MeshRenderer[] meshs = GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mr in meshs)
                {
                    Material mat = mr.sharedMaterial;
                    if (mat != null)
                    {
                        mat.SetVector("WaveSpeed", vec);
                    }
                }
            }
        }
        public void SetWaterWaveSpeed(Vector4 param)
        {

            if ((ActiveStageType == StageKind.SeaDaytime) || (ActiveStageType == StageKind.SeaNight))
            {
                Renderer r = GetComponent<Renderer>();
                if (!r)
                {
                    return;
                }
                Material mat = r.sharedMaterial;
                if (!mat)
                {
                    return;
                }
                mat.SetVector("WaveSpeed", param);
            }
            else if (ActiveStageType == StageKind.BasicSeaLevel)
            {
                MeshRenderer[] meshs = GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mr in meshs)
                {
                    Material mat = mr.sharedMaterial;
                    if (mat != null)
                    {
                        mat.SetVector("WaveSpeed", param);
                    }
                }
            }
        }
        public void SetWaterWaveSpeed(string pos, float value)
        {
            if ((ActiveStageType == StageKind.SeaDaytime) || (ActiveStageType == StageKind.SeaNight))
            {
                Renderer r = GetComponent<Renderer>();
                if (!r)
                {
                    return;
                }
                Material mat = r.sharedMaterial;
                if (!mat)
                {
                    return;
                }
                Vector4 vec = mat.GetVector("WaveSpeed");
                if (pos == "x")
                {
                    vec.x = value;
                }
                else if (pos == "y")
                {
                    vec.y = value;
                }
                else if (pos == "z")
                {
                    vec.z = value;
                }
                else if (pos == "w")
                {
                    vec.w = value;
                }
                mat.SetVector("WaveSpeed", vec);
            }
        }
        public Vector4 GetWaterWaveSpeedFromOuter(int is_contacthtml = 1)
        {
            string js = "";
            Vector4 ret = Vector4.zero;
            if ((ActiveStageType == StageKind.SeaDaytime) || (ActiveStageType == StageKind.SeaNight))
            {
                Renderer r = GetComponent<Renderer>();
                if (!r)
                {
                    return ret;
                }
                Material mat = r.sharedMaterial;
                if (!mat)
                {
                    return ret;
                }
                ret = mat.GetVector("WaveSpeed");
                js = JsonUtility.ToJson(ret);
            }
            else if (ActiveStageType == StageKind.BasicSeaLevel)
            {
                MeshRenderer[] meshs = GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mr in meshs)
                {
                    Material mat = mr.sharedMaterial;
                    if (mat != null)
                    {
                        ret = mat.GetVector("WaveSpeed");
                        js = JsonUtility.ToJson(ret);
                        break;
                    }
                }
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            if (is_contacthtml == 1)
            {
                ReceiveStringVal(js);
            }
#endif

            return ret;

        }
        public float GetWaterWaveSpeed(string pos)
        {
            float ret = 0f;
            if ((ActiveStageType == StageKind.SeaDaytime) || (ActiveStageType == StageKind.SeaNight))
            {
                Renderer r = GetComponent<Renderer>();
                if (!r)
                {
                    return ret;
                }
                Material mat = r.sharedMaterial;
                if (!mat)
                {
                    return ret;
                }
                if (pos == "x")
                {
                    ret = mat.GetVector("WaveSpeed").x;
                }
                else if (pos == "y")
                {
                    ret = mat.GetVector("WaveSpeed").y;
                }
                else if (pos == "z")
                {
                    ret = mat.GetVector("WaveSpeed").z;
                }
                else if (pos == "w")
                {
                    ret = mat.GetVector("WaveSpeed").w;
                }
            }
            return ret;

        }
        public void SetWaterWaveScale(float param)
        {
            if ((ActiveStageType == StageKind.SeaDaytime) || (ActiveStageType == StageKind.SeaNight))
            {
                Renderer r = GetComponent<Renderer>();
                if (!r)
                {
                    return;
                }
                Material mat = r.sharedMaterial;
                if (!mat)
                {
                    return;
                }
                mat.SetFloat("_WaveScale", param);
            }
        }
        public float GetWaterWaveScale(int is_contacthtml = 1)
        {
            float ret = 0f;
            if ((ActiveStageType == StageKind.SeaDaytime) || (ActiveStageType == StageKind.SeaNight))
            {
                Renderer r = GetComponent<Renderer>();
                if (!r)
                {
                    return ret;
                }
                Material mat = r.sharedMaterial;
                if (!mat)
                {
                    return ret;
                }
                ret = mat.GetFloat("_WaveScale");
#if !UNITY_EDITOR && UNITY_WEBGL
            if (is_contacthtml == 1)
            {
                ReceiveFloatVal(ret);
            }
#endif
            }

            return ret;
        }

        //=== Water stage materials======================---------------------------------------------------------
        public void ManageWaterComponent()
        {
            if (GetActiveStageType() == StageKind.BasicSeaLevel)
            {
                UnityStandardAssets.Water.GerstnerDisplace gerst = ActiveStage.GetComponent<UnityStandardAssets.Water.GerstnerDisplace>();
                //Debug.Log(gerst);
                gerst.enabled = false;
                DOVirtual.DelayedCall(0.01f, () =>
                 {
                     gerst.enabled = true;
                 });
            }
        }
        public List<MaterialProperties> ListUserMaterialObject()
        {
            List<MaterialProperties> ret = new List<MaterialProperties>();

            MeshRenderer hitmr = null;
            Material mat = null;
            //Debug.Log("ActiveStage=" + ActiveStage.name + "/" + ActiveStageType.ToString());
            if ((ActiveStageType == StageKind.SeaDaytime) || (ActiveStageType == StageKind.SeaNight))
            {
                hitmr = ActiveStage.GetComponentInChildren<MeshRenderer>();
                mat = hitmr.sharedMaterial;
            }
            else if (ActiveStageType == StageKind.BasicSeaLevel)
            {
                /*MeshRenderer[] meshs = ActiveStage.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mr in meshs)
                {
                    hitmr = mr;
                    mat = hitmr.sharedMaterial;
                    break;
                }*/
                
                //mat = ActiveStage.GetComponent<UnityStandardAssets.Water.WaterBase>().sharedMaterial;

                hitmr = ActiveStage.GetComponentInChildren<MeshRenderer>();
                mat = hitmr.sharedMaterial;
            }
            //Debug.Log(hitmr);
            if (hitmr == null) return ret;

            //Debug.Log(hitmr.sharedMaterial.name + "/" + hitmr.sharedMaterial.shader.name);

            if (mat != null)
            {
                MaterialProperties matp = new MaterialProperties();

                matp.name = mat.name;

                matp.shaderName = mat.shader.name;

                if ((ActiveStageType == StageKind.SeaDaytime) || (ActiveStageType == StageKind.SeaNight))
                {
                    //matp.waveScale = mat.GetFloat("_WaveScale");
                    Vector4 tiling = mat.GetVector("_BumpTiling");
                    matp.waveScale = tiling.w;
                }
                else if (ActiveStageType == StageKind.BasicSeaLevel)
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
        public virtual void ListUserMaterialFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            //List<string> list = ListUserMaterial("");
            string js = ListGetOneUserMaterial(""); //string.Join("\r\n", list.ToArray());
            Debug.Log(js);
            ReceiveStringVal(js);
#endif
        }
        /// <summary>
        /// To write 1 - material to csv-string 
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public string ListGetOneUserMaterial(string param)
        {
            MeshRenderer hitmr = null;
            Material mat = null;
            if ((ActiveStageType == StageKind.SeaDaytime) || (ActiveStageType == StageKind.SeaNight))
            {
                hitmr = ActiveStage.GetComponentInChildren<MeshRenderer>();
                mat = hitmr.sharedMaterial;
            }
            else if (ActiveStageType == StageKind.BasicSeaLevel)
            {
                /*MeshRenderer[] meshs = ActiveStage.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mr in meshs)
                {
                    hitmr = mr;
                    mat = hitmr.material;
                    break;
                }*/
                //mat = ActiveStage.GetComponent<WaterBase>().sharedMaterial;
                hitmr = ActiveStage.GetComponentInChildren<MeshRenderer>();
                mat = hitmr.sharedMaterial;
            }
            if (hitmr == null) return "";


            const string SEPSTR = "=";
            string ret = "";

            //Debug.Log("param=" + param);
            //Debug.Log(userSharedMaterials.ContainsKey(param));
            List<MaterialProperties> umat = ListUserMaterialObject();
            //umat.Add(finalMatprop);

            //Debug.Log("umat=" + umat.Count.ToString());

            if (umat.Count > 0)
            {
                if ((ActiveStageType == StageKind.SeaDaytime) || (ActiveStageType == StageKind.SeaNight))
                {
                    ret = (
                        param + SEPSTR +
                        umat[0].shaderName + SEPSTR +
                        umat[0].waveScale.ToString() + SEPSTR +
                        umat[0].fresnelScale.ToString() + SEPSTR +
                        "" + SEPSTR +
                        "" + SEPSTR +
                        "" + SEPSTR +
                        //---7
                        "0,0,0,0" + SEPSTR +
                        "0,0,0,0" + SEPSTR +
                        "0,0,0,0" + SEPSTR +
                        "0,0,0,0" + SEPSTR +
                        "0,0,0,0" + SEPSTR +
                        "0,0,0,0"
                    );

                }
                else if (ActiveStageType == StageKind.BasicSeaLevel)
                {
                    Vector4 wa = umat[0].waveAmplitude;
                    Vector4 wf = umat[0].waveFrequency;
                    Vector4 wt = umat[0].waveSteepness;
                    Vector4 ws = umat[0].waveSpeed;
                    Vector4 wdab = umat[0].waveDirectionAB;
                    Vector4 wdcd = umat[0].waveDirectionCD;
                    ret = (
                        param + SEPSTR +
                        umat[0].shaderName + SEPSTR +
                        umat[0].waveScale.ToString() + SEPSTR + 
                        umat[0].fresnelScale.ToString() + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(umat[0].color) + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(umat[0].reflectionColor) + SEPSTR +
                        ColorUtility.ToHtmlStringRGBA(umat[0].specularColor) + SEPSTR +
                        //---7
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
            // 1 - shader name
            // 2 - wave scale
            // 3 - fresnel scale
            // 4 - base color
            // 5 - reflection color
            // 6 - specular color
            // 7 - wave amplitude
            // 8 - wave frequency
            // 9 - wave steepness
            // 10- wave speed
            // 11- wave direction AB
            // 12- wave direction CD

            return ret;
        }

        /// <summary>
        /// Set value for UI only
        /// </summary>
        /// <param name="orimat"></param>
        public void SetUserMaterialObject(MaterialProperties orimat)
        {

            finalMatprop.name = orimat.name;
            finalMatprop.shaderName = orimat.name;
            finalMatprop.waveScale = orimat.waveScale;
            finalMatprop.fresnelScale = orimat.fresnelScale;
            finalMatprop.color = orimat.color;
            finalMatprop.reflectionColor = orimat.reflectionColor;
            finalMatprop.specularColor = orimat.specularColor;
            finalMatprop.waveAmplitude = orimat.waveAmplitude;
            finalMatprop.waveFrequency = orimat.waveFrequency;
            finalMatprop.waveSpeed = orimat.waveSpeed;
            finalMatprop.waveSteepness = orimat.waveSteepness;
            finalMatprop.waveDirectionAB = orimat.waveDirectionAB;
            finalMatprop.waveDirectionCD = orimat.waveDirectionCD;
        }
        /// <summary>
        /// To set material property from Unity
        /// </summary>
        /// <param name="mat_name"></param>
        /// <param name="propname"></param>
        /// <param name="vmat"></param>
        /// <param name="isSaveOnly"></param>
        public virtual void SetUserMaterial(string propname, MaterialProperties vmat, bool isSaveOnly = false)
        {
            MeshRenderer hitmr = null;
            List<Material> matlist = new List<Material>();

            if ((ActiveStageType == StageKind.SeaDaytime) || (ActiveStageType == StageKind.SeaNight))
            {
                hitmr = ActiveStage.GetComponentInChildren<MeshRenderer>();

                matlist.Add(hitmr.sharedMaterial);
            }
            else if (ActiveStageType == StageKind.BasicSeaLevel)
            {
                MeshRenderer[] meshs = ActiveStage.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mr in meshs)
                {
                    hitmr = mr;
                    matlist.Add(hitmr.sharedMaterial);
                    //break;
                }
                //matlist.Add(ActiveStage.GetComponent<WaterBase>().sharedMaterial);
            }
            if (hitmr == null) return;

            ManageAnimation manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

            //Material mat = hitmr.sharedMaterial;
            foreach (Material mat in matlist)
            {
                if (mat != null)
                {
                    if (propname.ToLower() == "wavescale")
                    {
                        //mat.SetFloat("_WaveScale", vmat.waveScale);
                        Vector4 vec = mat.GetVector("_BumpTiling");
                        vec.w = vmat.waveScale;
                        mat.SetVector("_BumpTiling", vec);
                    }
                    else if (propname.ToLower() == "fresnelscale")
                    {
                        mat.SetFloat("_FresnelScale", vmat.fresnelScale);
                    }
                    else if (propname.ToLower() == "basecolor")
                    {
                        mat.SetColor("_BaseColor", vmat.color);
                    }
                    else if (propname.ToLower() == "reflectioncolor")
                    {
                        mat.SetColor("_ReflectionColor", vmat.reflectionColor);
                    }
                    else if (propname.ToLower() == "specularcolor")
                    {
                        mat.SetColor("_SpecularColor", vmat.specularColor);
                    }
                    else if (propname.ToLower() == "waveamplitude")
                    {
                        mat.SetVector("_GAmplitude", vmat.waveAmplitude);
                    }
                    else if (propname.ToLower() == "wavefrequency")
                    {
                        mat.SetVector("_GFrequency", vmat.waveFrequency);
                    }
                    else if (propname.ToLower() == "wavesteepness")
                    {
                        mat.SetVector("_GSteepness", vmat.waveSteepness);
                    }
                    else if (propname.ToLower() == "wavespeed")
                    {
                        mat.SetVector("_GSpeed", vmat.waveSpeed);
                    }
                    else if (propname.ToLower() == "wavedirectionab")
                    {
                        mat.SetVector("_GDirectionAB", vmat.waveDirectionAB);
                    }
                    else if (propname.ToLower() == "wavedirectioncd")
                    {
                        mat.SetVector("_GDirectionCD", vmat.waveDirectionCD);
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
        /// <param name="param">0--parts(shader,color,cullmode,etc),2-value(Standard,VRM/MToon, #FFFFFF)</param>
        public void SetUserMaterial(string param)
        {
            string[] prm = param.Split(',');
            string propname = prm[0];
            string value = prm[1];

            MeshRenderer hitmr = null;

            List<Material> matlist = new List<Material>();
            //Material mat = null;
            if ((ActiveStageType == StageKind.SeaDaytime) || (ActiveStageType == StageKind.SeaNight))
            {
                hitmr = ActiveStage.GetComponentInChildren<MeshRenderer>();

                matlist.Add(hitmr.material);
            }
            else if (ActiveStageType == StageKind.BasicSeaLevel)
            {
                MeshRenderer[] meshs = ActiveStage.GetComponentsInChildren<MeshRenderer>();
                //Debug.Log("mesh count="+meshs.Length.ToString());
                foreach (MeshRenderer mr in meshs)
                {
                    hitmr = mr;
                    matlist.Add(hitmr.sharedMaterial);
                    //break;
                }
                //matlist.Add(ActiveStage.GetComponent<WaterBase>().sharedMaterial);
            }
            //if (hitmr == null) return;
            //mat = hitmr.sharedMaterial;
            //if (mat == null) return;
            ManageAnimation manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

            foreach (Material mat in matlist)
            {
                //Debug.Log(mat.name + " / " + mat.shader.name);
                if (mat != null)
                {
                    if (propname.ToLower() == "wavescale")
                    {
                        float fv = 0;
                        if (float.TryParse(value, out fv))
                        {
                            //mat.SetFloat("_WaveScale", fv);
                            Vector4 vec = mat.GetVector("_BumpTiling");
                            vec.w = fv;
                            mat.SetVector("_BumpTiling", vec);
                        }
                    }
                    else if (propname.ToLower() == "fresnelscale")
                    {
                        float fv = 0;
                        if (float.TryParse(value, out fv))
                        {
                            mat.SetFloat("_FresnelScale", fv);
                        }
                    }
                    else if (propname.ToLower() == "basecolor")
                    {
                        Color col;
                        if (ColorUtility.TryParseHtmlString(value, out col))
                        {
                            mat.SetColor("_BaseColor", col);
                        }
                    }
                    else if (propname.ToLower() == "reflectioncolor")
                    {
                        Color col;
                        if (ColorUtility.TryParseHtmlString(value, out col))
                        {
                            mat.SetColor("_ReflectionColor", col);
                        }
                    }
                    else if (propname.ToLower() == "specularcolor")
                    {
                        Color col;
                        if (ColorUtility.TryParseHtmlString(value, out col))
                        {
                            mat.SetColor("_SpecularColor", col);
                        }
                    }
                    else if (propname.ToLower() == "waveamplitude")
                    {
                        string[] arr = value.Split("\t");
                        float x = float.TryParse(arr[0], out x) ? x : 0f;
                        float y = float.TryParse(arr[1], out y) ? y : 0f;
                        float z = float.TryParse(arr[2], out z) ? z : 0f;
                        float w = float.TryParse(arr[3], out w) ? w : 0f;
                        Vector4 vec = new Vector4(x, y, z, w);
                        mat.SetVector("_GAmplitude", vec);
                    }
                    else if (propname.ToLower() == "wavefrequency")
                    {
                        string[] arr = value.Split("\t");
                        float x = float.TryParse(arr[0], out x) ? x : 0f;
                        float y = float.TryParse(arr[1], out y) ? y : 0f;
                        float z = float.TryParse(arr[2], out z) ? z : 0f;
                        float w = float.TryParse(arr[3], out w) ? w : 0f;
                        Vector4 vec = new Vector4(x, y, z, w);
                        mat.SetVector("_GFrequency", vec);
                    }
                    else if (propname.ToLower() == "wavesteepness")
                    {
                        string[] arr = value.Split("\t");
                        float x = float.TryParse(arr[0], out x) ? x : 0f;
                        float y = float.TryParse(arr[1], out y) ? y : 0f;
                        float z = float.TryParse(arr[2], out z) ? z : 0f;
                        float w = float.TryParse(arr[3], out w) ? w : 0f;
                        Vector4 vec = new Vector4(x, y, z, w);
                        mat.SetVector("_GSteepness", vec);
                    }
                    else if (propname.ToLower() == "wavespeed")
                    {
                        string[] arr = value.Split("\t");
                        float x = float.TryParse(arr[0], out x) ? x : 0f;
                        float y = float.TryParse(arr[1], out y) ? y : 0f;
                        float z = float.TryParse(arr[2], out z) ? z : 0f;
                        float w = float.TryParse(arr[3], out w) ? w : 0f;
                        Vector4 vec = new Vector4(x, y, z, w);
                        mat.SetVector("_GSpeed", vec);
                    }
                    else if (propname.ToLower() == "wavedirectionab")
                    {
                        string[] arr = value.Split("\t");
                        float x = float.TryParse(arr[0], out x) ? x : 0f;
                        float y = float.TryParse(arr[1], out y) ? y : 0f;
                        float z = float.TryParse(arr[2], out z) ? z : 0f;
                        float w = float.TryParse(arr[3], out w) ? w : 0f;
                        Vector4 vec = new Vector4(x, y, z, w);
                        mat.SetVector("_GDirectionAB", vec);
                    }
                    else if (propname.ToLower() == "wavedirectioncd")
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
        /// <summary>
        /// To set material motion information to DOTween 
        /// </summary>
        /// <param name="seq"></param>
        /// <param name="skind"></param>
        /// <param name="mat_name"></param>
        /// <param name="value"></param>
        /// <param name="duration"></param>
        /// <returns></returns>
        public virtual Sequence SetMaterialTween(Sequence seq, StageKind skind, string mat_name, MaterialProperties value, float duration)
        {
            MeshRenderer hitmr = null;
            if ((skind == StageKind.SeaDaytime) || (skind == StageKind.SeaNight))
            {
                hitmr = ActiveStage.GetComponentInChildren<MeshRenderer>();

                if (hitmr != null)
                {
                    Material mat = hitmr.sharedMaterial;

                    //if (mat.HasProperty("_WaveScale")) seq.Join(mat.DOFloat(value.waveScale, "_WaveScale", duration));
                    if (mat.HasProperty("_BumpTiling"))
                    {
                        Vector4 vec = mat.GetVector("_BumpTiling");
                        vec.w = value.waveScale;
                        seq.Join(mat.DOVector(vec, "_BumpTiling", duration));
                    }
                }
            }
            else if (skind == StageKind.BasicSeaLevel)
            {
                MeshRenderer[] meshs = ActiveStage.GetComponentsInChildren<MeshRenderer>();
                //Debug.Log("mesh count=" + meshs.Length.ToString());
                foreach (MeshRenderer mr in meshs)
                {
                    if (mr.sharedMaterial != null)
                    {
                        if (mr.sharedMaterial.HasProperty("_FresnelScale")) seq.Join(mr.sharedMaterial.DOFloat(value.fresnelScale, "_FresnelScale", duration));
                        if (mr.sharedMaterial.HasProperty("_BaseColor")) seq.Join(mr.sharedMaterial.DOColor(value.color, "_BaseColor", duration));
                        if (mr.sharedMaterial.HasProperty("_ReflectionColor")) seq.Join(mr.sharedMaterial.DOColor(value.reflectionColor, "_ReflectionColor", duration));
                        if (mr.sharedMaterial.HasProperty("_SpecularColor")) seq.Join(mr.sharedMaterial.DOColor(value.specularColor, "_SpecularColor", duration));
                        if (mr.sharedMaterial.HasProperty("_GAmplitude")) seq.Join(mr.sharedMaterial.DOVector(value.waveAmplitude, "_GAmplitude", duration));
                        if (mr.sharedMaterial.HasProperty("_GFrequency")) seq.Join(mr.sharedMaterial.DOVector(value.waveFrequency, "_GFrequency", duration));
                        if (mr.sharedMaterial.HasProperty("_GSteepness")) seq.Join(mr.sharedMaterial.DOVector(value.waveSteepness, "_GSteepness", duration));
                        if (mr.sharedMaterial.HasProperty("_GSpeed")) seq.Join(mr.sharedMaterial.DOVector(value.waveSpeed, "_GSpeed", duration));
                        if (mr.sharedMaterial.HasProperty("_GDirectionAB")) seq.Join(mr.sharedMaterial.DOVector(value.waveDirectionAB, "_GDirectionAB", duration));
                        if (mr.sharedMaterial.HasProperty("_GDirectionCD")) seq.Join(mr.sharedMaterial.DOVector(value.waveDirectionCD, "_GDirectionCD", duration));
                    }
                }
                //Material mat = ActiveStage.GetComponent<UnityStandardAssets.Water.WaterBase>().sharedMaterial;
                
                
                /*
                MeshRenderer[] meshs = ActiveStage.GetComponentsInChildren<MeshRenderer>();
                foreach (MeshRenderer mr in meshs)
                {
                    hitmr = mr;
                    if (hitmr != null)
                    {
                        Material mat = hitmr.material;

                        

                    }
                }*/
                

            }
            if (hitmr == null) return seq;



            return seq;
        }


        //------------------------------------------------------------------------------------------------------------------------
        //   Camera Operation 1
        //------------------------------------------------------------------------------------------------------------------------
        public CameraOperation1 GetCameraOperation()
        {
            //return Camera.main.gameObject.GetComponent<CameraOperation1>();
            return wxrCamSet.GetComponent<CameraOperation1>();
        }
        //------------------------------------------------------------------------------------------------------------------------
        //   Directional light
        //------------------------------------------------------------------------------------------------------------------------
        public OperateLoadedLight GetSystemDirectionalLight()
        {
            OperateLoadedLight oll = GameObject.Find("Directional Light").GetComponent<OperateLoadedLight>();
            return oll;
        }

        //------------------------------------------------------------------------------------------------------------------------
        //   Wind zone
        //------------------------------------------------------------------------------------------------------------------------
        public OperateLoadedWindzone GetWindzone()
        {
            OperateLoadedWindzone olw = GameObject.Find("WindZone").GetComponent<OperateLoadedWindzone>();
            return olw;
        }

        //------------------------------------------------------------------------------------------------------------------------
        //  Terrain (EditableStage)
        //------------------------------------------------------------------------------------------------------------------------
        async public void ResetEditableStage()
        {
            TerrainData tdata = editStage.terrainData;

            //---paint texture---
            float[,,] alphamaps = tdata.GetAlphamaps(0, 0, tdata.alphamapResolution, tdata.alphamapResolution);
            for (int x = 0; x < alphamaps.GetLength(0); x++)
            {
                for (int y = 0; y < alphamaps.GetLength(1); y++)
                {
                    for (int al = 0; al < tdata.alphamapLayers; al++)
                    {
                        alphamaps[x, y, al] = 0f;
                    }
                    alphamaps[x, y, 0] = 1f;
                }
            }
            tdata.SetAlphamaps(0, 0, alphamaps);

            //---ground height
            float[,] heights = tdata.GetHeights(0, 0, tdata.heightmapResolution, tdata.heightmapResolution);
            for (int x = 0; x < heights.GetLength(0); x++)
            {
                for (int y = 0; y < heights.GetLength(1); y++)
                {
                    heights[x, y] = DEFAULT_BASEHEIGHT;
                }
            }
            tdata.SetHeights(0, 0, heights);


            //---tree---
            int treeCnt = tdata.treeInstanceCount;
            for (int i = treeCnt-1; i >= 0; i--)
            {
                TreeInstance tree = tdata.GetTreeInstance(i);
            }
            

            //---detail (grass etc)---
            int detailCnt = tdata.detailPrototypes.Length;
            for (int i = 0; i < detailCnt; i++)
            {
                int[,] details = tdata.GetDetailLayer(0, 0, tdata.detailResolution, tdata.detailResolution, i);
                for (int x = 0; x < details.GetLength(0); x++)
                {
                    for (int y = 0; y < details.GetLength(1); y++)
                    {
                        details[x, y] = 0;
                    }
                    tdata.SetDetailLayer(0, 0, i, details);
                }
            }


            await Task.Delay(100);
        }
        public void SetStageTest(string data)
        {
            return;   
            int OFFSET_X = editStage.terrainData.heightmapResolution / 2;
            int OFFSET_Z = editStage.terrainData.heightmapResolution / 2;
            float[,] heights = editStage.terrainData.GetHeights(0, 0, editStage.terrainData.heightmapResolution, editStage.terrainData.heightmapResolution);
            heights[OFFSET_Z+1, OFFSET_X + 1] = 0.034f;
            heights[OFFSET_Z, OFFSET_X] = 0.034f;
            heights[OFFSET_Z - 1, OFFSET_X] = 0.02f;

            editStage.terrainData.SetHeightsDelayLOD(0, 0, heights);

            //Debug.Log(editStage.terrainData.terrainLayers[0].diffuseTexture);
            float[,,] alphamaps = editStage.terrainData.GetAlphamaps(0, 0, editStage.terrainData.alphamapResolution, editStage.terrainData.alphamapResolution);
            alphamaps[OFFSET_Z + 1, OFFSET_X + 1, 0] = 0.5f;
            alphamaps[OFFSET_Z + 1, OFFSET_X + 1, 2] = 0.5f;

            alphamaps[OFFSET_Z + 1, OFFSET_X + 1, 0] = 0f;
            alphamaps[OFFSET_Z + 1, OFFSET_X, 0] = 0f;
            alphamaps[OFFSET_Z + 1, OFFSET_X - 1, 0] = 0f;
            alphamaps[OFFSET_Z + 1, OFFSET_X + 1, 3] = 2f;
            alphamaps[OFFSET_Z + 1, OFFSET_X, 3] = 3f;
            alphamaps[OFFSET_Z + 1, OFFSET_X - 1, 3] = 2f;
            editStage.terrainData.SetAlphamaps(0, 0, alphamaps);

            //Debug.Log(editStage.terrainData.detailPrototypes.Length);
            int [,] details = editStage.terrainData.GetDetailLayer(0, 0, editStage.terrainData.detailResolution, editStage.terrainData.detailResolution, 0);
            details[editStage.terrainData.detailHeight/2 + 1, editStage.terrainData.detailWidth/2 + 1] = 100;
            editStage.terrainData.SetDetailLayer(0, 0, 0, details);
        }
        public void SetStageData(string data)
        {
            editStageMap = JsonUtility.FromJson<VVStageMap>(data);
        }
    }

}
