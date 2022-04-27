using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UserHandleSpace;
using DG.Tweening;


namespace UserHandleSpace
{
    /// <summary>
    /// Management class for LookAt bone 
    /// </summary>
    public class UserLookAtOperation : MonoBehaviour
    {
        //public GameObject avatar;
        public string PartsName;
        public GameObject relatedAvatar;
        private Transform TargetBone;
        Animator animator;
        private Vector3 oldPosition;
        private Quaternion oldRotation;
        public Vector3 defaultPosition;
        public Quaternion defaultRotation;

        private ConfigSettingLabs cnf;

        private OperateLoadedVRM ovrm;
        private ManageAvatarTransform mat;
        private ManageAnimation manim;

        // Start is called before the first frame update
        void Start()
        {
            //animator = avatar.GetComponent<Animator>();
            oldPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, this.transform.localPosition.z);
            oldRotation = new Quaternion(this.transform.rotation.x, this.transform.rotation.y, this.transform.rotation.z, this.transform.rotation.w);

            SaveDefaultTransform();

            ovrm = relatedAvatar.GetComponent<OperateLoadedVRM>();

            cnf = GameObject.FindGameObjectWithTag("UICanvas").GetComponent<ConfigSettingLabs>();

            mat = relatedAvatar.GetComponent<ManageAvatarTransform>();
            manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
        }

        // Update is called once per frame
        private void Update()
        {
            

            if (manim.IsBoneLimited)
            {

            }
            Vector3 dir = TargetBone.position - transform.position;

            Quaternion lookAtRotation = Quaternion.LookRotation(dir, Vector3.up);
            Quaternion offsetRotation = Quaternion.FromToRotation(Vector3.forward, Vector3.forward);
            transform.rotation = lookAtRotation * offsetRotation;


            oldPosition = this.transform.localPosition;
            oldRotation = this.transform.localRotation;


        }
        void LastUpdate()
        {
            //Debug.Log(transform.position);
            //Vector3 LeftHand = animator.GetIKPosition(AvatarIKGoal.LeftHand);
            //animator.SetIKPosition(AvatarIKGoal.LeftHand, transform.position);

        }
        public void OnDragging(Object[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                //Debug.Log(objects[i]);
            }


            //IInput input = IOC.Resolve<IRTE>().Input;
            //Vector3 v1 = input.GetPointerXY(0);
            //Debug.Log(v1);
        }
        public void SaveDefaultTransform()
        {
            defaultPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
            defaultRotation = new Quaternion(transform.localRotation.x, transform.localRotation.y, transform.localRotation.z, transform.localRotation.w);
        }
        public void LoadDefaultTransform()
        {
            transform.localPosition = defaultPosition;
            transform.localRotation = defaultRotation;
        }
        public void ActivateHandle()
        {
            /*RotationHandle rhan = GetComponent<RotationHandle>();
            PositionHandle phan = GetComponent<PositionHandle>();
            if (rhan)
            {
                rhan.enabled = true;
            }
            if (phan)
            {
                phan.enabled = true;
            }*/
            //---set parts name, and set up related IK Hint(swivel)
            GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
            ikhp.GetComponent<OperateActiveVRM>().ActivePartsName = this.PartsName;

            GameObject canvas = GameObject.FindGameObjectWithTag("UICanvas");
            OperateAvatarIK oaik = canvas.GetComponent<OperateAvatarIK>();
            oaik.ReloadPartsSwivel(this.PartsName);
        }
        /*
        public void DeactivateHandles()
        {
            GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
            GameObject avatar = ikhp.GetComponent<OperateActiveVRM>().ActiveAvatar;
            GameObject[] hans = GameObject.FindGameObjectsWithTag("IKHandle");
            for (int i = 0; i < hans.Length; i++)
            {
                
                RotationHandle rhan = hans[i].GetComponent<RotationHandle>();
                PositionHandle phan = hans[i].GetComponent<PositionHandle>();
                
                if (rhan)
                {
                    rhan.enabled = false;
                }
                if (phan)
                {
                    phan.enabled = false;
                }
            }
        }*/
        public void SetRelatedAvatar(GameObject avatar, HumanBodyBones boneType)
        {
            relatedAvatar = avatar;
            animator = avatar.GetComponent<Animator>();

            TargetBone = animator.GetBoneTransform(boneType);
        }
    }
}

