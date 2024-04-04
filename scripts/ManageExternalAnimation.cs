using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UniVRM10;
using UserVRMSpace;
using System.Linq;
using System.Threading.Tasks;
using UniGLTF;
using VRMShaders;
using UnityEngine.Video;

namespace UserHandleSpace
{
    public class ManageExternalAnimation : MonoBehaviour
    {
        public class ManagedVRMA
        {
            public string filename = "";
            public string name = "";

            public ManagedVRMA()
            {
                filename = "";
                name = "";
            }
        }
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);

        Dictionary<string, Vrm10AnimationInstance> VrmaList;
        Dictionary<string, int> RefList;    


        // Start is called before the first frame update
        void Start()
        {
            VrmaList = new Dictionary<string, Vrm10AnimationInstance>();
            RefList = new Dictionary<string, int>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnDestroy()
        {
            if (VrmaList != null)
            {
                var venum = VrmaList.GetEnumerator();
                while (venum.MoveNext())
                {
                    venum.Current.Value.Dispose();
                }
                VrmaList.Clear();
            }
        }
        public void OpenVRMA(string param)
        {
            ManagedVRMA managedVRMA = JsonUtility.FromJson<ManagedVRMA>(param);

            StartCoroutine(LoadVRMAbody(managedVRMA));
        }
        public IEnumerator LoadVRMAbody(ManagedVRMA url)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url.filename))
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
                    StartLoadVRMAbody(url, www.downloadHandler.data).ConfigureAwait(false);
                }
            }
        }
        public async Task<Vrm10AnimationInstance> StartLoadVRMAbody(ManagedVRMA mvrma, byte[] data)
        {
            using GltfData gdata = new GlbBinaryParser(data, "test").Parse();
            using var loader = new VrmAnimationImporter(gdata);
            var instance = await loader.LoadAsync(new ImmediateCaller());
            
            var vrmaInst = instance.GetComponent<Vrm10AnimationInstance>();
            vrmaInst.ShowBoxMan(false);
            VrmaList[mvrma.name] = vrmaInst;
            RefList[mvrma.name] = 0;

            Animation vanim = vrmaInst.GetComponent<Animation>();
            List<string> jsclips = new List<string>();
            foreach (AnimationState state in vanim)
            {
                jsclips.Add("{\"name\": \"" + state.name + "\", \"time\":" + state.length.ToString() + "}");
            }

            List <string> htmlarr = new List<string>
            {
                "{",
                "\"filename\": \"" + mvrma.name + "\"," ,
                "\"clips\" : [" + String.Join(",",jsclips) + "]",
                "}"
            };
            string htmlret = String.Join("", htmlarr);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(htmlret);
#endif

            return vrmaInst;
        }
        public List<string> EnumVRMA()
        {
            List<string> list = new List<string>();
            var genom = VrmaList.Keys.GetEnumerator();
            while (genom.MoveNext()) 
            {
                list.Add(genom.Current);
            }
            return list;

        }
        public void EnumVRMAFromOuter()
        {
            List<string> arr = EnumVRMA();
            string ret = String.Join("\t", arr);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public Vrm10AnimationInstance GetVRMA(string name)
        {
            return VrmaList[name];
        }
        public void HitVRMAFromOuter(string name)
        {
            string ret = "0";
            if (GetVRMA(name) != null) 
            {
                ret = "1";
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        public bool HitVRMA(string name)
        {
            if (GetVRMA(name) == null)
            {
                return false;                
            }
            else
            {
                return true;
            }
            
        }
        public void CloseVRMA(string name)
        {
            VrmaList[name].Dispose();
            VrmaList[name] = null;

            VrmaList.Remove(name);
            RefList.Remove(name);
        }
        public int HitVRMAReference(string name)
        {
            return RefList[name];
        }
        public void CountAddVRMAReference(string name)
        {
            if (RefList.ContainsKey(name)) RefList[name] += 1;
        }
        public void CountDownVRMAReference(string name)
        {
            if (RefList.ContainsKey(name)) RefList[name] -= 1;
        }
        public void HitVRMAReferenceFromOuter(string name)
        {
            int ret = HitVRMAReference(name);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret);
#endif
        }
    }

}
