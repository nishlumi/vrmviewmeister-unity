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

namespace UserHandleSpace
{
    public enum StageKind
    {
        Default = 0,        //special stage 1
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

        string[] StageNames = {"DefaultStage", "DaytimeWaterStage", "NightimeWaterStage", "DryGroundStage",
            "DesertStage","FieldStage1", "FieldStage2", "FieldStage3", "FieldStage4","UserStage"
        };

        public GameObject StageParent;
        public List<GameObject> StageList;
        public GameObject ActiveStage;
        private StageKind ActiveStageType;

        public string ActiveUserStageMainTextureName;
        public string ActiveUserStageBumpmapTextureName;

        protected Vector3 oldPosition;
        protected Quaternion oldRotation;
        protected Vector3 defaultPosition;
        protected Quaternion defaultRotation;

        private Terrain editStage;
        const float DEFAULT_BASEHEIGHT = 0.033342486245f;
        const float HEIGHT_CHANGEVAL = 0.001f;

        private VVStageMap editStageMap;

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

            ActiveUserStageMainTextureName = "";
            ActiveUserStageBumpmapTextureName = "";
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
                manim.materialManager.UnRefer(ActiveUserStageMainTextureName);
                mesh.material.SetTexture("_MainTex", null);
            }
            if (ActiveUserStageBumpmapTextureName != "")
            {
                MeshRenderer mesh = ActiveStage.GetComponent<MeshRenderer>();
                manim.materialManager.UnRefer(ActiveUserStageBumpmapTextureName);
                mesh.material.SetTexture("_BumpMap", null);
            }
        }
        public virtual void GetCommonTransformFromOuter()
        {
            Vector3 pos = GetPositionFromOuter(0);
            Vector3 rot = GetRotationFromOuter(0);
            Vector3 sca = GetScale(0);
            string ret = "";
            ret = pos.x + "," + pos.y + "," + pos.z + "%" + rot.x + "," + rot.y + "," + rot.z + "%" + sca.x + "," + sca.y + "," + sca.z;
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
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
        public void SelectStage(int param)
        {
            StageList.ForEach(action =>
            {
                action.SetActive(false);
            });
            if ((0 <= param) && (param < StageList.Count)) {
                ActiveStage = StageList[param];
                ActiveStageType = (StageKind)param;
                ActiveStage.SetActive(true);
            }
            /*
            for (int i = 0; i < StageParent.transform.childCount; i++)
            {
                StageParent.transform.GetChild(i).gameObject.SetActive(false);
            }

            ActiveStage = StageParent.transform.GetChild(param).gameObject;
            ActiveStageType = (StageKind)param;
            ActiveStage.SetActive(true);
            */
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
        public async System.Threading.Tasks.Task<GameObject> SelectStageRef(int param)
        {
            if ((0 <= param) && (param < StageNames.Length))
            {
                RelaseStageRef();

                //ActiveStage = StageList[param];
                ActiveStageType = (StageKind)param;
                if (param == 0)
                {
                    ActiveStage = StageList[0];
                    ActiveStage.SetActive(true);
                }
                else
                {
                    string sname = "Stage/" + StageNames[param];
                    Debug.Log(sname);

                    AsyncOperationHandle<GameObject> targetStageHandle = Addressables.InstantiateAsync(sname);

                    System.Threading.Tasks.Task<GameObject> eff = targetStageHandle.Task;
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
            if (ActiveStageType == StageKind.Default)
            {
                ActiveStage.SetActive(false);
            }
            else
            {
                Addressables.ReleaseInstance(ActiveStage);
            }
            
        }
        
        //-----------------------------------------------------------------------------------------------
        public Material GetActiveStageMaterial()
        {
            Renderer r = ActiveStage.GetComponent<Renderer>();
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
                col = ActiveStage.GetComponent<MeshRenderer>().sharedMaterial.color;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            if (is_contacthtml == 1)
            {
                ReceiveStringVal(ColorUtility.ToHtmlStringRGBA(col));
            }
#endif
            return col;
        }
        public void SetDefaultStageColor(string param)
        {
            if (ActiveStageType == StageKind.Default)
            {
                MeshRenderer mesh = ActiveStage.GetComponent<MeshRenderer>();

                Debug.Log(mesh.sharedMaterial.color);
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
        //------------------------------------------------------------------------------------------------
        public float GetFloatUserStage(string param)
        {
            float ret = 0;

            if (ActiveStageType == StageKind.User)
            {
                MeshRenderer mesh = ActiveStage.GetComponent<MeshRenderer>();
                if (mesh.materials.Length > 0)
                {
                    if (param == "metallic")
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
                    if (param == "emissioncolor")
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
                    ret += "metallic=" + mesh.material.GetFloat("_Metallic").ToString() + "\t";
                    ret += "glossiness=" + mesh.material.GetFloat("_Glossiness").ToString() + "\t";
                    ret += "emissioncolor=#" + ColorUtility.ToHtmlStringRGBA(mesh.material.GetColor("_EmissionColor"));
                }
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);

#endif
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
                    if (prm[0] == "metallic")
                    {
                        mesh.material.SetFloat("_Metallic", val);
                    }
                    else if (prm[0] == "glossiness")
                    {
                        mesh.material.SetFloat("_Glossiness", val);
                    }
                    else if (prm[0] == "emissioncolor")
                    {
                        Color col = ColorUtility.TryParseHtmlString("#" + prm[1], out col) ? col : Color.white;
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
                            manim.materialManager.UnRefer(ActiveUserStageMainTextureName);
                            mesh.material.SetTexture("_MainTex", null);

                            NativeAP_OneMaterial nap = manim.materialManager.FindTexture(prm[1]);
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
                            manim.materialManager.UnRefer(ActiveUserStageBumpmapTextureName);
                            mesh.material.SetTexture("_BumpMap", null);

                            NativeAP_OneMaterial nap = manim.materialManager.FindTexture(prm[1]);
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
                string js = JsonUtility.ToJson(ret);
#if !UNITY_EDITOR && UNITY_WEBGL
            if (is_contacthtml == 1)
            {
                ReceiveStringVal(js);
            }
#endif
            }
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

        //------------------------------------------------------------------------------------------------------------------------
        //   Camera Operation 1
        //------------------------------------------------------------------------------------------------------------------------
        public CameraOperation1 GetCameraOperation()
        {
            return Camera.main.gameObject.GetComponent<CameraOperation1>();
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

            Debug.Log(editStage.terrainData.terrainLayers[0].diffuseTexture);
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

            Debug.Log(editStage.terrainData.detailPrototypes.Length);
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
