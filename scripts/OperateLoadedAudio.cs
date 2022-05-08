using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using DG.Tweening;

namespace UserHandleSpace
{
    public class OperateLoadedAudio : OperateLoadedBase
    {
        [DllImport("__Internal")]
        private static extern void ReceiveStringVal(string val);
        [DllImport("__Internal")]
        private static extern void ReceiveIntVal(int val);
        [DllImport("__Internal")]
        private static extern void ReceiveFloatVal(float val);

        private Dictionary<string, AudioClip> userAudioList;

        public AudioSource audioPlayer;
        private AudioClip targetAudio;
        public string targetAudioName;

        public bool IsSE;
        //1 - play, 2 - playing, 3 - pause, 0 - stop
        private UserAnimationState audioStatFlag;
        //1 - seek on, 0 - seek off(default)
        private int audioSeekFlag;

        private void Awake()
        {
            userAudioList = new Dictionary<string, AudioClip>();
            audioSeekFlag = 0;
            audioStatFlag = 0;



            targetType = AF_TARGETTYPE.Audio;
        }
        // Start is called before the first frame update
        void Start()
        {
            ManageAnimation mana = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            //mana.FirstAddFixedAvatar(gameObject.name, gameObject, gameObject, gameObject.name, AF_TARGETTYPE.Audio);

        }

        // Update is called once per frame
        void Update()
        {

        }


        /// <summary>
        /// To pack all properties for HTML-UI
        /// </summary>
        public void GetIndicatedPropertyFromOuter()
        {
            string ret = "";

            int pflag = (int)GetPlayFlag();

            List<string> arr = new List<string>();
            arr.Add(IsSE ? "SE" : "BGM");
            arr.Add(pflag.ToString());
            arr.Add(targetAudioName);
            arr.Add(GetAudioLength().ToString());
            arr.Add(GetSeekSeconds().ToString());
            arr.Add(GetVolume().ToString());
            arr.Add(GetPitch().ToString());
            arr.Add(GetLoop().ToString());

            ret = String.Join("\t", arr);

            //0 - type
            //1 - play flag
            //2 - current audio
            //3 - audio length
            //4 - seek pos
            //5 - volume
            //6 - pitch
            //7 - loop

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }

        public string ListAudioFromOuter()
        {
            /*
             *  keyname,audio name,audio length%keyname,audio name,audio length%...
             */
            List<string> ret = new List<string>();
            var dic = userAudioList.GetEnumerator();
            while (dic.MoveNext())
            {
                ret.Add(dic.Current.Key + "," + dic.Current.Value.name + "," + dic.Current.Value.samples.ToString());
            }

#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(string.Join("%", ret));
#endif
            return string.Join("%", ret);
        }
        public List<string> ListAudio()
        {
            /*
             *  keyname,audio name,audio length%keyname,audio name,audio length%...
             */
            List<string> ret = new List<string>();
            var dic = userAudioList.GetEnumerator();
            while (dic.MoveNext())
            {
                ret.Add(dic.Current.Key);
            }

            return ret;
        }
        public void AddAudio(string name, AudioClip ac)
        {
            userAudioList[name] = ac;
        }
        public void RemoveAudio(string name)
        {
            if (userAudioList.ContainsKey(name) == true)
            {
                if (IsPlayingCheck(name) == true)
                { //---if playing parametered audio, stop before removing.
                    audioPlayer.Stop();
                }
                Destroy(userAudioList[name]);
                userAudioList.Remove(name);
            }
            
        }
        public void SetAudio(string name)
        {
            AudioClip ac;
            if (userAudioList.TryGetValue(name, out ac))
            {
                audioPlayer.clip = ac;
                targetAudio = ac;
                targetAudioName = name;
            }

        }
        public void ClearSetAudio()
        {
            targetAudio = null;
            audioPlayer.clip = null;
            targetAudioName = "";
        }
        //=============================================================================

        /// <summary>
        /// Set play flag
        /// </summary>
        /// <param name="flag">UserAnimationState</param>
        public void SetPlayFlag(UserAnimationState flag)
        {
            audioStatFlag = flag;
            /*
            if ((audioStatFlag != 1) && (audioStatFlag != 2) && (audioStatFlag != 3))
            {
                audioStatFlag = 0;
            }*/
        }
        public void SetPlayFlagFromOuter(int flag)
        {
            audioStatFlag = (UserAnimationState)flag;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>UserAnimationState</returns>
        public UserAnimationState GetPlayFlag()
        {
            return audioStatFlag;
        }
        public void GetPlayFlagFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal((int)audioStatFlag);
#endif
        }
        //=============================================================================
        public void SetSeekFlag(int flag)
        {
            audioSeekFlag = flag;
        }
        public int GetSeekFlag()
        {
            return audioSeekFlag;
        }
        public void GetSeekFlagFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(audioSeekFlag);
#endif
        }
        //=============================================================================
        public bool GetIsSE()
        {
            return IsSE;
        }

        public void PlayAudio()
        {
            if (audioPlayer.clip != null)
            {
                audioPlayer.Play();
            }
        }
        public void PauseAudio()
        {
            if (audioPlayer.clip != null)
            {
                if (audioPlayer.isPlaying) audioPlayer.Pause();
                else audioPlayer.UnPause();
            }
        }
        public void StopAudio()
        {
            audioPlayer.Stop();

        }
        public void PlaySe()
        {
            if (targetAudio != null)
            {
                audioPlayer.PlayOneShot(targetAudio, audioPlayer.volume);
            }
        }
        public void PlaySe(float vol)
        {
            if (targetAudio != null)
            {
                audioPlayer.PlayOneShot(targetAudio, vol);
            }
        }
        public void PlaySe(string name, float vol)
        {
            AudioClip ac;
            if (userAudioList.TryGetValue(name, out ac))
            {
                audioPlayer.PlayOneShot(ac, vol);
            }
        }
        public void PlaySeFromOuter(string param)
        {
            string[] prm = param.Split(',');
            float vol = float.TryParse(prm[1], out vol) ? vol : 0f;
            AudioClip ac;
            if (userAudioList.TryGetValue(prm[0], out ac))
            {
                audioPlayer.PlayOneShot(ac, vol);
            }
        }
        public bool IsPlayingCheck(string name)
        {
            bool ret = false;
            if (audioPlayer.isPlaying)
            {
                ret = (audioPlayer.clip.name == name);
            }
            return ret;
        }
        public void IsPlayingCheckFromOuter(string name)
        {
            int ret = 0;
            if (audioPlayer.isPlaying)
            {
                ret = (audioPlayer.clip.name == name) ? 1 : 0;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret);
#endif
        }
        //=============================================================================
        public float GetAudioLength()
        {
            float ret = 0f;
            if (audioPlayer.clip != null) ret = audioPlayer.clip.length;
            return ret;
        }
        public void GetAudioLengthFromOuter()
        {
            float ret = 0f;
            ret = audioPlayer.clip.length;
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(ret);
#endif
        }
        //=============================================================================
        public void SetSeekSeconds(float time)
        {
            audioPlayer.time = time;
        }
        public float GetSeekSeconds()
        {
            float ret = 0f;
            if (audioPlayer.clip != null) ret = audioPlayer.time;
            return ret;
        }
        public void GetSeekSecondsFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(audioPlayer.time);
#endif
        }
        //=============================================================================
        public void SetLoop(int flag)
        {
            audioPlayer.loop = flag == 1 ? true : false;
        }
        public int GetLoop()
        {
            return audioPlayer.loop ? 1 : 0;
        }
        public void GetLoopFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(audioPlayer.loop ? 1 : 0);
#endif
        }
        //=============================================================================
        public void SetMute(int flag)
        {
            audioPlayer.mute = flag == 1 ? true : false;
        }
        public int GetMute()
        {
            return audioPlayer.mute ? 1 : 0;
        }
        public void GetMuteFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(audioPlayer.mute ? 1 : 0);
#endif
        }
        //=============================================================================
        public void SetVolume(float vol)
        {
            audioPlayer.volume = vol;
        }
        public float GetVolume()
        {
            return audioPlayer.volume;
        }
        public void GetVolumeFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(audioPlayer.volume);
#endif
        }
        //=============================================================================
        public void SetPitch(float vol)
        {
            audioPlayer.pitch = (vol > 3)
                ? 3
                : (vol < -3)
                    ? -3
                    : vol
            ;
        }
        public float GetPitch()
        {
            return audioPlayer.pitch;
        }
        public void GetPitchFromOuter()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveFloatVal(audioPlayer.pitch);
#endif
        }
    }

}
