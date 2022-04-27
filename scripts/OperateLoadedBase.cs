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
                relatedHandleParent.transform.Translate(pos);
            }
            else
            {
                gameObject.transform.Translate(pos);
            }
                
        }
        public void SetPositionFromOuter(string param)
        {
            string[] prm = param.Split(',');
            float x = float.TryParse(prm[0], out x) ? x : 0f;
            float y = float.TryParse(prm[1], out y) ? y : 0f;
            float z = float.TryParse(prm[2], out z) ? z : 0f;
            bool isabs = prm[3] == "1" ? true : false;

            if (relatedHandleParent != null)
            {
                relatedHandleParent.transform.DOMove(new Vector3(x, y, z), 0.1f).SetRelative(!isabs);
            }
            else
            {
                gameObject.transform.DOMove(new Vector3(x, y, z), 0.1f).SetRelative(!isabs);
            }
                

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
            relatedHandleParent.transform.DOScale(new Vector3(x, y, z), 0.1f);

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
        public virtual void SetPunch(string param)
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
        public virtual void SetShake(string param)
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
    }

}
