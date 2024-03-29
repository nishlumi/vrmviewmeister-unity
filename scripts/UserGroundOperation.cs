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
        private Animator animator;
        public Transform LeftShoulderIK;
        public Transform RightShoulderIK;

        private Vector3 oldPosition;
        private Quaternion oldRotation;
        private Vector3 defaultPosition;
        private Quaternion defaultRotation;

        private ConfigSettingLabs csl;
        private BasicTransformInformation bti;
        private ManageAnimation animarea;
        private OperateLoadedVRM ovrm;
        private OperateActiveVRM oavrm;
        private AvatarKeyOperator akeyo;

        private void Awake()
        {
            
        }
        // Start is called before the first frame update
        void Start()
        {
            SaveDefaultTransform(true, true);
            GameObject can = GameObject.Find("AnimateArea");
            csl = can.GetComponent<ConfigSettingLabs>();
            bti = new BasicTransformInformation();
            bti.dimension = "3d";
            animarea = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

            ovrm = relatedAvatar.GetComponent<OperateLoadedVRM>();

            GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
            oavrm = ikhp.GetComponent<OperateActiveVRM>();
            akeyo = new AvatarKeyOperator(animarea.cfg_keymove_speed_rot, animarea.cfg_keymove_speed_trans);
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
                    //---get child
                    /*if (LeftShoulderIK != null)
                    {
                        Transform animtrans = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
                        LeftShoulderIK.position = animtrans.position;
                        LeftShoulderIK.rotation = animtrans.rotation;
                    }
                    if (RightShoulderIK != null)
                    {
                        Transform animtrans = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
                        RightShoulderIK.position = animtrans.position;
                        RightShoulderIK.rotation = animtrans.rotation;
                    }*/
                }
                /*
                //---key operation for current selected avatar translation
                if (animarea.keyOperationMode == KeyOperationMode.MoveAvatar) 
                { //this avatar is active ?
                    if (oavrm.ActiveAvatar.GetInstanceID() == relatedAvatar.GetInstanceID())
                    {
                        akeyo.SetSpeed(animarea.cfg_keymove_speed_rot, animarea.cfg_keymove_speed_trans);
                        akeyo.CallKeyOperation(gameObject);
                    }
                }
                */
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
        public void SetRelatedAvatar(GameObject avatar)
        {
            relatedAvatar = avatar;
            animator = avatar.GetComponent<Animator>();
        }
    }

}
