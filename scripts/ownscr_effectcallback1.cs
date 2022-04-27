using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UserHandleSpace
{
    public class ownscr_effectcallback1 : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void SendPreviewingEffectEnd(string val);


        public bool isPreview;
        public string sourceID;
        public string genre;
        public string effectName;


        // Start is called before the first frame update
        void Start()
        {
            isPreview = false;
            sourceID = "";
            genre = "";
            effectName = "";
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnParticleSystemStopped()
        {
            if (isPreview)
            {
#if !UNITY_EDITOR && UNITY_WEBGL
                string ret = sourceID + "," + genre + "," + effectName;
                SendPreviewingEffectEnd(ret);
#endif
            }
            isPreview = false;
            sourceID = "";
        }
        public void ClearSetting()
        {
            isPreview = false;
            sourceID = "";
            genre = "";
            effectName = "";
        }
    }
}

