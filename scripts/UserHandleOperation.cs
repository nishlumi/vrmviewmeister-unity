using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using RootMotion.FinalIK;

namespace UserHandleSpace
{
    
    /// <summary>
    /// Management class for each IK handle (child)
    /// </summary>
    public class UserHandleOperation : MonoBehaviour
    {
        //public GameObject avatar;
        public string PartsName;
        public GameObject relatedAvatar;
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
            //---futurely delete 
            /*
            if (Input.GetMouseButtonDown(0))
            { //---ハンドルコントローラーにフォーカスが当たった
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(ray, out hit,100f))
                {
                    //Debug.Log(this.name + " = " + hit.collider.gameObject.name);
                    if (this.name == hit.collider.gameObject.name)
                    {
                        //this.DeactivateHandles();
                        this.ActivateHandle();

                        //---if position or rotation is diferent to old this, reset swivel.
                        if ((this.transform.position != oldPosition) || 
                            (this.transform.rotation != oldRotation))
                        {
                            //GameObject canvas = GameObject.FindGameObjectWithTag("UICanvas");
                            //OperateAvatarIK oaik = canvas.GetComponent<OperateAvatarIK>();
                            //oaik.ResetSwivel(this.PartsName);
                        }
                        
                    }
                }
            }
            */
            Sequence seq = DOTween.Sequence();
            
            
            if (manim.IsLimitedPelvis)
            {
                if (PartsName == "pelvis")
                {
                    Transform leftlowerleg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                    Transform rightlowerleg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                    GameObject ll = transform.parent.Find("LeftLowerLeg").gameObject;
                    GameObject rl = transform.parent.Find("RightLowerLeg").gameObject;

                    bool isfire = false;
                    if (this.transform.localPosition.y != oldPosition.y)
                    {
                        float pelvisY = oldPosition.y - this.transform.localPosition.y;
                        //if (ll != null) seq.Join(ll.transform.DOLocalMoveZ(pelvisY*-1f, 0.1f).SetRelative(true));
                        //if (rl != null) seq.Join(rl.transform.DOLocalMoveZ(pelvisY*-1f, 0.1f).SetRelative(true));
                        isfire = true;
                    }
                    if (this.transform.localPosition.z != oldPosition.z)
                    {
                        float pelvisZ = oldPosition.z - this.transform.localPosition.z;
                        //if (ll != null) seq.Join(ll.transform.DOLocalMoveZ(pelvisZ  * -1f, 0.1f).SetRelative(true));
                        //if (rl != null) seq.Join(rl.transform.DOLocalMoveZ(pelvisZ  * -1f, 0.1f).SetRelative(true));
                        isfire = true;
                    }
                    if (isfire)
                    {
                        if (ll != null)
                        {
                            seq.Join(ll.transform.DOMoveZ(leftlowerleg.position.z, 0.1f));
                            seq.Join(ll.transform.DOMoveY(leftlowerleg.position.y, 0.1f));
                        }
                        if (rl != null)
                        {
                            seq.Join(rl.transform.DOMoveZ(rightlowerleg.position.z, 0.1f));
                            seq.Join(rl.transform.DOMoveY(rightlowerleg.position.y, 0.1f));
                        }
                    }
                }
                
                

                seq.Play();
            }
            if (manim.IsLimitedArms)
            {
                if (PartsName == "leftarm")
                {
                    Transform leftlowerarm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                    GameObject ll = transform.parent.Find("LeftLowerArm").gameObject;

                    bool isfire = false;
                    if (this.transform.localPosition.z != oldPosition.z)
                    {
                        isfire = true;
                    }
                    if (this.transform.localPosition.y != oldPosition.y)
                    {
                        isfire = true;
                    }
                    if (this.transform.localPosition.x != oldPosition.x)
                    {
                        isfire = true;
                    }
                    if (isfire && (ll != null))
                    {
                        seq.Join(ll.transform.DOMove(leftlowerarm.position, 0.1f));
                        Vector3 newhandrot = leftlowerarm.rotation.eulerAngles;
                        newhandrot.z = this.transform.rotation.z;
                        seq.Join(this.transform.DORotate(newhandrot, 0.1f));
                        
                    }
                }
                else if (PartsName == "rightarm")
                {
                    Transform rightlowerarm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                    GameObject rl = transform.parent.Find("RightLowerArm").gameObject;

                    bool isfire = false;
                    if (this.transform.localPosition.z != oldPosition.z)
                    {
                        isfire = true;
                    }
                    if (this.transform.localPosition.y != oldPosition.y)
                    {
                        isfire = true;
                    }
                    if (this.transform.localPosition.x != oldPosition.x)
                    {
                        isfire = true;
                    }
                    if (isfire && (rl != null))
                    {
                        seq.Join(rl.transform.DOMove(rightlowerarm.position, 0.1f));
                        Vector3 newhandrot = rightlowerarm.rotation.eulerAngles;
                        newhandrot.z = this.transform.rotation.z;
                        seq.Join(this.transform.DORotate(newhandrot, 0.1f));
                    }
                }
                seq.Play();
            }
            if (manim.IsLimitedLegs)
            {
                if (PartsName == "leftleg")
                {
                    float adjustX = cnf.GetFloatVal("ikbone_adjust_leg_x");
                    float adjustY = cnf.GetFloatVal("ikbone_adjust_leg_y");
                    float adjustZ = cnf.GetFloatVal("ikbone_adjust_leg_z");
                    Transform hip = animator.GetBoneTransform(HumanBodyBones.Hips);
                    Transform leftupperleg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                    Transform leftlowerleg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                    Transform leftfoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    Vector3 dist_hip2upper = hip.localPosition + leftupperleg.localPosition - gameObject.transform.parent.transform.localPosition;
                    Vector3 dist_upper2lower = dist_hip2upper + leftlowerleg.localPosition;
                    Vector3 dist_lower2foot = dist_upper2lower + leftfoot.localPosition;

                    GameObject ll = transform.parent.Find("LeftLowerLeg").gameObject;
                    Vector3 calcLeg = this.transform.localPosition - oldPosition;

                    Vector3 rrot = new Vector3(ll.transform.localRotation.eulerAngles.x, ll.transform.localRotation.eulerAngles.y, ll.transform.localRotation.eulerAngles.z);
                    //float degree = Vector3.Angle(ll.transform.localPosition, transform.localPosition);
                    rrot.x = transform.localPosition.z <= 0 ? leftlowerleg.localRotation.eulerAngles.x - 45f : leftlowerleg.localRotation.eulerAngles.x;
                    //if (transform.localPosition.z > 0) rrot.x = (rrot.x < 90f) ? rrot.x + 45f : rrot.x;

                    //---original leg transform to operate
                    //Transform referll = ll.transform; //leftlowerleg; //


                    //---Z-axis
                    if ((this.transform.localPosition.z != oldPosition.z) && (adjustZ != 0f))
                    {
                        float legZ = oldPosition.z - this.transform.localPosition.z;

                        if (ll != null)
                        {

                            //+seq.Join(ll.transform.DOLocalMoveZ(referll.localPosition.z-0.05f,0.1f)); 
                            //ll.transform.DOLocalMoveZ(legZ * adjustZ * -1f, 0.1f).SetRelative(true); //
                            seq.Join(ll.transform.DOMoveY(leftlowerleg.position.y, 0.1f));
                            seq.Join(ll.transform.DOMoveZ(leftlowerleg.position.z, 0.1f));

                            //float degree = Vector3.Angle(ll.transform.localPosition, transform.localPosition);
                            if (cnf.enable_foot_autorotate) seq.Join(transform.DOLocalRotate(rrot, 0.1f));
                        }
                    }
                    //---Y-axis
                    if ((this.transform.localPosition.y != oldPosition.y) && (adjustY != 0f))
                    {
                        float legY = this.transform.localPosition.y - oldPosition.y;

                        if (ll != null)
                        {
                            //Vector3 finallower = dist_upper2lower;
                            //+seq.Join(ll.transform.DOLocalMoveY(legY * adjustY * 1f, 0.1f).SetRelative(true));
                            //+seq.Join(ll.transform.DOLocalMoveZ(legY * adjustZ * -1f, 0.1f).SetRelative(true));
                            //+seq.Join(ll.transform.DOLocalMoveY(finallower.y, 0.1f)); //referll.localPosition.y + calcLeg.y
                            //+seq.Join(ll.transform.DOLocalMoveZ(finallower.z, 0.1f));
                            seq.Join(ll.transform.DOMoveY(leftlowerleg.position.y, 0.1f));
                            seq.Join(ll.transform.DOMoveZ(leftlowerleg.position.z, 0.1f));


                            //float degree = Vector3.Angle(ll.transform.localPosition, transform.localPosition);
                            if (cnf.enable_foot_autorotate) seq.Join(transform.DOLocalRotate(rrot, 0.1f));
                            //Debug.Log(degree);
                        }
                    }
                    //---X-axis
                    if ((this.transform.localPosition.x != oldPosition.x) && (adjustX != 0f))
                    {
                        float legX = oldPosition.x - this.transform.localPosition.x;
                        if (ll != null)
                        {
                            //+seq.Join(ll.transform.DOLocalMoveX(legX * adjustX * -1f, 0.1f).SetRelative(true));
                            seq.Join(ll.transform.DOMoveX(leftlowerleg.position.x, 0.1f));
                            if (cnf.enable_foot_autorotate) seq.Join(transform.DOLocalRotate(rrot, 0.1f));
                        }
                    }
                }
                else if (PartsName == "rightleg")
                {
                    float adjustX = cnf.GetFloatVal("ikbone_adjust_leg_x");
                    float adjustY = cnf.GetFloatVal("ikbone_adjust_leg_y");
                    float adjustZ = cnf.GetFloatVal("ikbone_adjust_leg_z");
                    Transform rightlowerleg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);

                    GameObject rl = transform.parent.Find("RightLowerLeg").gameObject;
                    Vector3 calcLeg = this.transform.localPosition - oldPosition;

                    Vector3 rrot = new Vector3(rl.transform.localRotation.eulerAngles.x, rl.transform.localRotation.eulerAngles.y, rl.transform.localRotation.eulerAngles.z);
                    //float degree = Vector3.Angle(rl.transform.localPosition, transform.localPosition);
                    rrot.x = transform.localPosition.z <= 0 ? rightlowerleg.localRotation.eulerAngles.x - 45f : rightlowerleg.localRotation.eulerAngles.x;
                    //if (transform.localPosition.z > 0) rrot.x = (rrot.x < 90) ? rrot.x + 45f : rrot.x;

                    //---original leg transform to operate
                    //Transform referrl = rl.transform; //rightlowerleg

                    //---Z-axis 
                    if ((this.transform.localPosition.z != oldPosition.z) && (adjustZ != 0f))
                    {
                        float legZ = oldPosition.z - this.transform.localPosition.z;
                        if (rl != null)
                        {
                            //seq.Join(rl.transform.DOLocalMoveZ(referrl.localPosition.z - 0.05f, 0.1f));  //rl.transform.DOLocalMoveZ(legZ * adjustZ * -1f, 0.1f).SetRelative(true); //
                            seq.Join(rl.transform.DOMoveY(rightlowerleg.transform.position.y, 0.1f));
                            seq.Join(rl.transform.DOMoveZ(rightlowerleg.position.z, 0.1f));

                            if (cnf.enable_foot_autorotate) seq.Join(transform.DOLocalRotate(rrot, 0.1f));
                        }
                    }
                    //---Y-axis
                    if ((this.transform.localPosition.y != oldPosition.y) && (adjustY != 0f))
                    {
                        float legY = oldPosition.y - this.transform.localPosition.y;

                        if (rl != null)
                        {
                            //seq.Join(rl.transform.DOLocalMoveY(legY * adjustY * -1f, 0.1f).SetRelative(true));
                            //seq.Join(rl.transform.DOLocalMoveZ(legY * adjustZ * 1f, 0.1f).SetRelative(true));
                            //seq.Join(rl.transform.DOLocalMoveY(referrl.localPosition.y, 0.1f));
                            //seq.Join(rl.transform.DOLocalMoveZ(referrl.localPosition.z - 0.05f, 0.1f));
                            seq.Join(rl.transform.DOMoveY(rightlowerleg.transform.position.y, 0.1f));
                            seq.Join(rl.transform.DOMoveZ(rightlowerleg.transform.position.z, 0.1f));

                            //float degree = Vector3.Angle(rl.transform.localPosition, transform.localPosition);
                            if (cnf.enable_foot_autorotate) seq.Join(transform.DOLocalRotate(rrot, 0.1f));
                        }
                    }
                    //---X-axis
                    if ((this.transform.localPosition.x != oldPosition.x) && (adjustX != 0f))
                    {
                        float legX = oldPosition.x - this.transform.localPosition.x;
                        if (rl != null)
                        {
                            //seq.Join(rl.transform.DOLocalMoveX(legX * adjustX * -1f, 0.1f).SetRelative(true));
                            seq.Join(rl.transform.DOMoveX(rightlowerleg.position.x, 0.1f));

                            if (cnf.enable_foot_autorotate) seq.Join(transform.DOLocalRotate(rrot, 0.1f));
                        }
                    }
                }
                seq.Play();
            }
            
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
        public void SetRelatedAvatar(GameObject avatar)
        {
            relatedAvatar = avatar;
            animator = avatar.GetComponent<Animator>();
        }
    }
}