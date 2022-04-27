using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using DG.Tweening;


namespace UserHandleSpace
{

    /// <summary>
    /// Management class for IK parent handle
    /// </summary>
    public class UserGroundOperation : MonoBehaviour
    {
        [DllImport("__Internal")]
        private static extern void ChangeTransformOnUpdate(string val);

        public GameObject relatedAvatar;

        private Vector3 oldPosition;
        private Quaternion oldRotation;
        private Vector3 defaultPosition;
        private Quaternion defaultRotation;

        private ConfigSettingLabs csl;
        private BasicTransformInformation bti;
        private ManageAnimation animarea;
        private OperateLoadedVRM ovrm;

        private void Awake()
        {
            
        }
        // Start is called before the first frame update
        void Start()
        {
            SaveDefaultTransform(true, true);
            GameObject can = GameObject.Find("Canvas");
            csl = can.GetComponent<ConfigSettingLabs>();
            bti = new BasicTransformInformation();
            bti.dimension = "3d";
            animarea = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

            ovrm = relatedAvatar.GetComponent<OperateLoadedVRM>();
        }

        // Update is called once per frame
        void Update()
        {

            /*if (csl.GetBoolVal("is_move_with_vrmbody_with_ik"))
            {
                Vector3 direc = oldPosition - transform.position;
                if (direc.y != 0)
                {
                    GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
                    OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

                    CapsuleCollider capc;
                    if (ovrm.ActiveAvatar.TryGetComponent<CapsuleCollider>(out capc))
                    {
                        ovrm.ActiveAvatar.transform.DOMoveY(transform.position.y * 0.5f, 0.1f);
                    }
                }
            }*/
            

            if (relatedAvatar != null)
            {
                if ((transform.position != oldPosition) || (transform.rotation != oldRotation))
                {
                    bti.id = relatedAvatar.name;
                    bti.position = transform.position;
                    bti.rotation = transform.rotation.eulerAngles;
                    bti.scale = transform.localScale;
#if !UNITY_EDITOR && UNITY_WEBGL
                    if (!animarea.IsPlaying)  ChangeTransformOnUpdate(JsonUtility.ToJson(bti));
#endif

                }
            }
            oldPosition = transform.position;
            oldRotation = transform.rotation;
        }

        public void SaveDefaultTransform(bool ispos, bool isrotate)
        {
            if (ispos) defaultPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z) ;
            if (isrotate) defaultRotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        }
        public void LoadDefaultTransform(bool ispos, bool isrotate)
        {
            if (ispos) transform.position = defaultPosition;
            if (isrotate) transform.rotation = defaultRotation;
        }

    }

}
