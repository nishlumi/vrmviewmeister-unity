using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using RootMotion.FinalIK;
using LumisIkApp;

namespace UserHandleSpace
{
    
    /// <summary>
    /// Management class for each IK handle (child)
    /// </summary>
    public class UserHandleOperation : MonoBehaviour
    {
        public enum OperateType
        {
            MOVE = 0,
            ROTATE = 1
        }
        //public GameObject avatar;
        public bool IsFixTransform;
        public string PartsName;
        public GameObject relatedAvatar;
        private VvmIk VvmIk;
        Animator animator;
        private Vector3 oldPosition;
        private Quaternion oldRotation;
        public Vector3 defaultPosition;
        public Quaternion defaultRotation;

        public Transform RootTransform;

        private ConfigSettingLabs cnf;

        private OperateLoadedVRM ovrm;
        private ManageAvatarTransform mat;
        private ManageAnimation manim;

        private const float cns_lowerleg_z = 0.05f;

        private OperateType cur_operatetype = OperateType.MOVE;
        public bool is_current_marker;

        private void Awake()
        {
            cnf = GameObject.Find("Canvas").GetComponent<ConfigSettingLabs>();

            //mat = relatedAvatar.GetComponent<ManageAvatarTransform>();
            manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();
            
        }
        // Start is called before the first frame update
        void Start()
        {
            IsFixTransform = true;

            VvmIk = relatedAvatar.GetComponent<VvmIk>();

            //animator = avatar.GetComponent<Animator>();
            oldPosition = new Vector3(this.transform.localPosition.x, this.transform.localPosition.y, this.transform.localPosition.z);
            oldRotation = new Quaternion(this.transform.rotation.x, this.transform.rotation.y, this.transform.rotation.z, this.transform.rotation.w);

            SaveDefaultTransform();

            //ovrm = relatedAvatar.GetComponent<OperateLoadedVRM>();

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

            if (!IsFixTransform) return;

            if (manim == null) return;
            if (manim.IsLimitedPelvis)
            {
                if (PartsName == "pelvis")
                {
                    Transform leftlowerleg = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                    Transform rightlowerleg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                    GameObject ll = null; // transform.parent.Find("LeftLowerLeg").gameObject;
                    GameObject rl = null; // transform.parent.Find("RightLowerLeg").gameObject;
                    Transform[] childTransforms = RootTransform.GetComponentsInChildren<Transform>();
                    foreach (Transform childTransform in childTransforms)
                    {
                        if (childTransform.name == "LeftLowerLeg") ll = childTransform.gameObject;
                        if (childTransform.name == "RightLowerLeg") rl = childTransform.gameObject;
                    }

                    Vector3 llnewpos = transform.parent.InverseTransformPoint(leftlowerleg.position);
                    Vector3 rlnewpos = transform.parent.InverseTransformPoint(rightlowerleg.position);

                    bool isfire = false;
                    if (this.transform.localPosition != oldPosition)
                    {
                        float pelvisY = oldPosition.y - this.transform.localPosition.y;
                        //if (ll != null) seq.Join(ll.transform.DOLocalMoveZ(pelvisY*-1f, 0.1f).SetRelative(true));
                        //if (rl != null) seq.Join(rl.transform.DOLocalMoveZ(pelvisY*-1f, 0.1f).SetRelative(true));
                        isfire = true;
                    }
                    /*if (this.transform.localPosition.z != oldPosition.z)
                    {
                        float pelvisZ = oldPosition.z - this.transform.localPosition.z;
                        //if (ll != null) seq.Join(ll.transform.DOLocalMoveZ(pelvisZ  * -1f, 0.1f).SetRelative(true));
                        //if (rl != null) seq.Join(rl.transform.DOLocalMoveZ(pelvisZ  * -1f, 0.1f).SetRelative(true));
                        isfire = true;
                    }*/
                    if (isfire)
                    {
                        if (ll != null)
                        {
                            //seq.Join(ll.transform.DOMoveX(leftlowerleg.position.x, 0.01f));
                            //seq.Join(ll.transform.DOMoveY(leftlowerleg.position.y, 0.01f));
                            //seq.Join(ll.transform.DOMoveZ(leftlowerleg.position.z - cns_lowerleg_z, 0.01f));

                            seq.Join(ll.transform.DOLocalMoveX(llnewpos.x, 0.01f));
                            seq.Join(ll.transform.DOLocalMoveY(llnewpos.y, 0.01f));
                            seq.Join(ll.transform.DOLocalMoveZ(llnewpos.z - cns_lowerleg_z, 0.01f));

                        }
                        if (rl != null)
                        {
                            //seq.Join(rl.transform.DOMoveX(rightlowerleg.position.x, 0.01f));
                            //seq.Join(rl.transform.DOMoveY(rightlowerleg.position.y, 0.01f));
                            //seq.Join(rl.transform.DOMoveZ(rightlowerleg.position.z - cns_lowerleg_z, 0.01f));

                            seq.Join(rl.transform.DOLocalMoveX(rlnewpos.x, 0.01f));
                            seq.Join(rl.transform.DOLocalMoveY(rlnewpos.y, 0.01f));
                            seq.Join(rl.transform.DOLocalMoveZ(rlnewpos.z - cns_lowerleg_z, 0.01f));
                        }
/*
                        Transform neck = animator.GetBoneTransform(HumanBodyBones.Neck);
                        GameObject chest = transform.parent.Find("Chest").gameObject;

                        if ((neck != null) && (chest != null))
                        {
                            Vector3 necknewpos = transform.parent.InverseTransformPoint(neck.position);

                            seq.Join(chest.transform.DOLocalMoveX(necknewpos.x, 0.01f));
                            seq.Join(chest.transform.DOLocalMoveZ(necknewpos.z, 0.01f));
                        }*/

                    }
                }
                
                

                seq.Play();
            }
            if (manim.IsLimitedArms)
            {
                if (PartsName == "leftarm")
                {
                    Transform leftlowerarm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                    GameObject ll = null; // transform.parent.Find("LeftLowerArm").gameObject;
                    Transform[] childTransforms = RootTransform.GetComponentsInChildren<Transform>();
                    foreach (Transform childTransform in childTransforms)
                    {
                        if (childTransform.name == "LeftLowerArm") ll = childTransform.gameObject;
                    }

                    Vector3 llnewpos = transform.parent.InverseTransformPoint(leftlowerarm.position);

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
                        //seq.Join(ll.transform.DOMove(new Vector3(leftlowerarm.position.x, leftlowerarm.position.y, leftlowerarm.position.z), 0.1f));
                        //Vector3 newhandrot = leftlowerarm.localRotation.eulerAngles;
                        //newhandrot.z = this.transform.localRotation.z;
                        //seq.Join(ll.transform.DOLocalRotate(newhandrot, 0.1f));

                        seq.Join(ll.transform.DOLocalMoveX(llnewpos.x, 0.01f));
                        //seq.Join(ll.transform.DOLocalMoveY(llnewpos.y, 0.01f));
                        seq.Join(ll.transform.DOLocalMoveZ(llnewpos.z + cns_lowerleg_z, 0.01f));

                        //Vector3 ltar = new Vector3(ll.transform.position.x, transform.position.y, ll.transform.position.z);
                        //Vector3 ltar2 = Quaternion.LookRotation(-ltar).eulerAngles;

                        //transform.localRotation = Quaternion.Euler(ltar2);
                    }
                }
                else if (PartsName == "rightarm")
                {
                    Transform rightlowerarm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                    GameObject rl = null; // transform.parent.Find("RightLowerArm").gameObject;
                    Transform[] childTransforms = RootTransform.GetComponentsInChildren<Transform>();
                    foreach (Transform childTransform in childTransforms)
                    {
                        if (childTransform.name == "RightLowerArm") rl = childTransform.gameObject;
                    }

                    Vector3 rlnewpos = transform.parent.InverseTransformPoint(rightlowerarm.position);

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
                        //seq.Join(rl.transform.DOMove(new Vector3(rightlowerarm.position.x, rightlowerarm.position.y, rightlowerarm.position.z), 0.1f));
                        //Vector3 newhandrot = rightlowerarm.localRotation.eulerAngles;
                        //newhandrot.z = this.transform.localRotation.z;
                        //seq.Join(rl.transform.DOLocalRotate(newhandrot, 0.1f));

                        seq.Join(rl.transform.DOLocalMoveX(rlnewpos.x, 0.01f));
                        //seq.Join(rl.transform.DOLocalMoveY(rlnewpos.y, 0.01f));
                        seq.Join(rl.transform.DOLocalMoveZ(rlnewpos.z + cns_lowerleg_z, 0.01f));
                    }
                }
                else if (PartsName == "leftlowerarm")
                { //---synchronize hand and lower arm rotation.
                    if ((this.transform.localRotation != oldRotation) && manim.IsRelatedLeftLowerArm2Hand)
                    {
                        Transform[] childTransforms = RootTransform.GetComponentsInChildren<Transform>();
                        GameObject ll = null;
                        foreach (Transform childTransform in childTransforms)
                        {
                            if (childTransform.name == "LeftHand")
                            {
                                ll = childTransform.gameObject;
                                break;
                            }
                        }

                        //Transform leftlowerarm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                        Vector3 rot = transform.rotation.eulerAngles;
                        seq.Join(ll.transform.DORotate(rot, 0.1f));
                    }
                }
                else if (PartsName == "rightlowerarm")
                { //---synchronize hand and lower arm rotation.
                    if ((this.transform.localRotation != oldRotation) && manim.IsRelatedRightLowerArm2Hand)
                    {
                        Transform[] childTransforms = RootTransform.GetComponentsInChildren<Transform>();
                        GameObject rl = null;
                        foreach (Transform childTransform in childTransforms)
                        {
                            if (childTransform.name == "RightHand")
                            {
                                rl = childTransform.gameObject;
                                break;
                            }
                        }

                        //Transform rightlowerarm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                        Vector3 rot = transform.rotation.eulerAngles;
                        seq.Join(rl.transform.DORotate(rot, 0.1f));
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

                    GameObject ll = null; // transform.parent.Find("LeftLowerLeg").gameObject;
                    Transform[] childTransforms = RootTransform.GetComponentsInChildren<Transform>();
                    foreach (Transform childTransform in childTransforms)
                    {
                        if (childTransform.name == "LeftLowerLeg") ll = childTransform.gameObject;
                    }
                    UserHandleOperation uholl = ll.GetComponent<UserHandleOperation>();

                    Vector3 calcLeg = this.transform.localPosition - oldPosition;

                    Vector3 rrot = new Vector3(ll.transform.localRotation.eulerAngles.x, ll.transform.localRotation.eulerAngles.y, ll.transform.localRotation.eulerAngles.z);
                    //float degree = Vector3.Angle(ll.transform.localPosition, transform.localPosition);
                    rrot.x = transform.localPosition.z <= 0 ? leftlowerleg.localRotation.eulerAngles.x - 45f : leftlowerleg.localRotation.eulerAngles.x;
                    //if (transform.localPosition.z > 0) rrot.x = (rrot.x < 90f) ? rrot.x + 45f : rrot.x;

                    
                    float legZ = oldPosition.z - this.transform.localPosition.z;
                    float legY = oldPosition.y - this.transform.localPosition.y;
                    float legX = oldPosition.x - this.transform.localPosition.x;

                    Vector3 lowerlegRot = leftlowerleg.localRotation.eulerAngles;
                    Vector3 upperlegRot = leftupperleg.localRotation.eulerAngles;

                    Vector3 llnewpos = transform.parent.InverseTransformPoint(leftlowerleg.position);
                    //llnewpos.x = ll.transform.localPosition.x;
                    //llnewpos.y = ll.transform.localPosition.y;
                    //llnewpos.z = ll.transform.localPosition.z;

                    //---Z-axis
                    if ((this.transform.localPosition.z != oldPosition.z) && (adjustZ != 0f))
                    {

                        if (ll != null)
                        {
                            //seq.Join(ll.transform.DOLocalMoveY(leftlowerleg.position.y, 0.01f));
                            //seq.Join(ll.transform.DOLocalMoveZ(leftlowerleg.position.z - cns_lowerleg_z, 0.01f));

                            //seq.Join(ll.transform.DOLocalMoveY(ll.transform.localPosition.y + legY, 0.01f));

                            //---foot is front 
                            seq.Join(ll.transform.DOLocalMoveY(llnewpos.y, 0.01f));
                            seq.Join(ll.transform.DOLocalMoveZ(llnewpos.z - cns_lowerleg_z, 0.01f));




                            //float degree = Vector3.Angle(ll.transform.localPosition, transform.localPosition);
                            if (manim.cfg_enable_foot_autorotate) 
                                seq.Join(transform.DOLocalRotate(rrot, 0.1f));
                        }
                    }
                    //---Y-axis
                    if ((this.transform.localPosition.y != oldPosition.y) && (adjustY != 0f))
                    {

                        if (ll != null)
                        {
                            //seq.Join(ll.transform.DOLocalMoveY(leftlowerleg.position.y, 0.01f));
                            //seq.Join(ll.transform.DOLocalMoveZ(leftlowerleg.position.z - cns_lowerleg_z, 0.01f));

                            //seq.Join(ll.transform.DOLocalMoveZ(ll.transform.localPosition.z + legZ - cns_lowerleg_z, 0.01f));

                            seq.Join(ll.transform.DOLocalMoveY(llnewpos.y, 0.01f));
                            seq.Join(ll.transform.DOLocalMoveZ(llnewpos.z - cns_lowerleg_z, 0.01f));




                            //float degree = Vector3.Angle(ll.transform.localPosition, transform.localPosition);
                            if (manim.cfg_enable_foot_autorotate)
                                seq.Join(transform.DOLocalRotate(rrot, 0.1f));
                            //Debug.Log(degree);
                        }
                    }
                    //---X-axis
                    if ((this.transform.localPosition.x != oldPosition.x) && (adjustX != 0f))
                    {
                        if (ll != null)
                        {
                            //seq.Join(ll.transform.DOMoveX(leftlowerleg.localPosition.x, 0.1f));

                            //if (legX > 0) llnewpos.x = ll.transform.localPosition.x - legX;
                            //else llnewpos.x = ll.transform.localPosition.x - legX;
                            
                            if (transform.localPosition.x == 0)
                            {
                                seq.Join(ll.transform.DOLocalMoveX(leftlowerleg.localPosition.x, 0.01f));
                            }
                            else
                            {
                                seq.Join(ll.transform.DOLocalMoveX(llnewpos.x, 0.01f));
                            }


                            if (manim.cfg_enable_foot_autorotate)
                                seq.Join(transform.DOLocalRotate(rrot, 0.1f));
                        }
                    }
                }
                else if (PartsName == "rightleg")
                {
                    float adjustX = cnf.GetFloatVal("ikbone_adjust_leg_x");
                    float adjustY = cnf.GetFloatVal("ikbone_adjust_leg_y");
                    float adjustZ = cnf.GetFloatVal("ikbone_adjust_leg_z");
                    Transform rightlowerleg = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);

                    GameObject rl = null; // transform.parent.Find("RightLowerLeg").gameObject;
                    Transform[] childTransforms = RootTransform.GetComponentsInChildren<Transform>();
                    foreach (Transform childTransform in childTransforms)
                    {
                        if (childTransform.name == "RightLowerLeg") rl = childTransform.gameObject;
                    }
                    Vector3 calcLeg = this.transform.localPosition - oldPosition;

                    Vector3 rrot = new Vector3(rl.transform.localRotation.eulerAngles.x, rl.transform.localRotation.eulerAngles.y, rl.transform.localRotation.eulerAngles.z);
                    //float degree = Vector3.Angle(rl.transform.localPosition, transform.localPosition);
                    rrot.x = transform.localPosition.z <= 0 ? rightlowerleg.localRotation.eulerAngles.x - 45f : rightlowerleg.localRotation.eulerAngles.x;
                    //if (transform.localPosition.z > 0) rrot.x = (rrot.x < 90) ? rrot.x + 45f : rrot.x;

                    float legZ = oldPosition.z - this.transform.localPosition.z;
                    float legY = oldPosition.y - this.transform.localPosition.y;
                    float legX = oldPosition.x - this.transform.localPosition.x;

                    Vector3 llnewpos = transform.parent.InverseTransformPoint(rightlowerleg.position);
                    //llnewpos.x = rl.transform.localPosition.x;
                    //llnewpos.y = rl.transform.localPosition.y;
                    //llnewpos.z = rl.transform.localPosition.z;

                    //---Z-axis 
                    if ((this.transform.localPosition.z != oldPosition.z) && (adjustZ != 0f))
                    {
                        if (rl != null)
                        {
                            //---from VRM
                            //seq.Join(rl.transform.DOLocalMoveY(rightlowerleg.position.y, 0.01f));
                            //seq.Join(rl.transform.DOLocalMoveZ(rightlowerleg.position.z - cns_lowerleg_z, 0.01f));

                            //---during IK marker
                            seq.Join(rl.transform.DOLocalMoveY(llnewpos.y, 0.01f));
                            seq.Join(rl.transform.DOLocalMoveZ(llnewpos.z - cns_lowerleg_z, 0.01f));


                            if (manim.cfg_enable_foot_autorotate) seq.Join(transform.DOLocalRotate(rrot, 0.1f));
                        }
                    }
                    //---Y-axis
                    if ((this.transform.localPosition.y != oldPosition.y) && (adjustY != 0f))
                    {

                        if (rl != null)
                        {
                            //seq.Join(rl.transform.DOLocalMoveY(rightlowerleg.transform.position.y, 0.01f));
                            //seq.Join(rl.transform.DOLocalMoveZ(rightlowerleg.transform.position.z - cns_lowerleg_z, 0.01f));

                            seq.Join(rl.transform.DOLocalMoveY(llnewpos.y, 0.01f));
                            seq.Join(rl.transform.DOLocalMoveZ(llnewpos.z - cns_lowerleg_z, 0.01f));


                            //float degree = Vector3.Angle(rl.transform.localPosition, transform.localPosition);
                            if (manim.cfg_enable_foot_autorotate) seq.Join(transform.DOLocalRotate(rrot, 0.1f));
                        }
                    }
                    //---X-axis
                    if ((this.transform.localPosition.x != oldPosition.x) && (adjustX != 0f))
                    {
                        if (rl != null)
                        {
                            //seq.Join(rl.transform.DOMoveX(rightlowerleg.localPosition.x, 0.1f));

                            if (transform.localPosition.x == 0)
                            {
                                seq.Join(rl.transform.DOLocalMoveX(rightlowerleg.localPosition.x, 0.01f));
                            }
                            else
                            {
                                seq.Join(rl.transform.DOLocalMoveX(llnewpos.x, 0.01f));
                            }

                            if (manim.cfg_enable_foot_autorotate) seq.Join(transform.DOLocalRotate(rrot, 0.1f));
                        }
                    }
                }
                seq.Play();
            }
            //2022.05.17 entrust the management from CCDIK
            if (PartsName == "head") //---direct moving instead of IK
            {
                if ((this.transform.localPosition != oldPosition) || (this.transform.localRotation != oldRotation))
                {
                    Transform neck = animator.GetBoneTransform(HumanBodyBones.Neck);
                    Vector3 rot = transform.rotation.eulerAngles;
                    //rot.x *= -1f;
                    //rot.z *= -1f;
                    seq.Join(neck.DORotate(rot, 0.1f));
                    //neck.DORotateQuaternion(Quaternion.LookRotation(transform.localPosition),0.1f);
                }

            }
            else if (PartsName == "aim")
            {
                if (manim.IsLimitedAim)
                {
                    if ((this.transform.localPosition != oldPosition) || (this.transform.localRotation != oldRotation))
                    {
                        //Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
                        //GameObject ikhead = transform.parent.Find("Head").gameObject;

                        //Transform leftshoulder = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
                        //Transform rightshoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);

                        //UserGroundOperation ugo = transform.parent.GetComponent<UserGroundOperation>();
                        //Transform ikleft = ugo.LeftShoulderIK;
                        //Transform ikright = ugo.RightShoulderIK;

                        Transform neck = animator.GetBoneTransform(HumanBodyBones.Neck);
                        GameObject chest = null; // transform.parent.Find("Chest").gameObject;
                        Transform[] childTransforms = RootTransform.GetComponentsInChildren<Transform>();
                        foreach (Transform childTransform in childTransforms)
                        {
                            if (childTransform.name == "Chest") chest = childTransform.gameObject;
                        }

                        if ((neck != null) && (chest != null))
                        {
                            Vector3 necknewpos = transform.parent.InverseTransformPoint(neck.position);

                            //seq.Join(chest.transform.DOLocalMoveX(necknewpos.x, 0.01f));
                            //seq.Join(chest.transform.DOLocalMoveZ(necknewpos.z, 0.01f));
                        }
                    }
                }
            }
            else if (PartsName == "lookat")
            {
                
            }
            else if (PartsName == "chest")
            {
                if (manim.IsLimitedChest)
                {
                    //if (manim.camxr.isActiveNormal())
                    {
                        Transform head = animator.GetBoneTransform(HumanBodyBones.Head);
                        Transform leftlowerarm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                        Transform rightlowerarm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);

                        GameObject hd = null; // transform.parent.Find("Head").gameObject;
                        GameObject ll = null; // transform.parent.Find("LeftLowerArm").gameObject;
                        GameObject rl = null; // transform.parent.Find("RightLowerArm").gameObject;

                        Transform[] childTransforms = RootTransform.GetComponentsInChildren<Transform>();
                        foreach (var childTransform in childTransforms)
                        {
                            if (childTransform.name == "Head") hd = childTransform.gameObject;
                            if (childTransform.name == "LeftLowerArm") ll = childTransform.gameObject;
                            if (childTransform.name == "RightLowerArm") rl = childTransform.gameObject;
                        }
                        Vector3 hdnewpos = transform.parent.InverseTransformPoint(head.position);
                        Vector3 llnewpos = transform.parent.InverseTransformPoint(leftlowerarm.position);
                        Vector3 rlnewpos = transform.parent.InverseTransformPoint(rightlowerarm.position);


                        if (this.transform.localPosition != oldPosition)
                        {
                            //seq.Join(ll.transform.DOLocalMoveX(llnewpos.x, 0.01f));
                            ////seq.Join(ll.transform.DOLocalMoveY(llnewpos.y, 0.01f));
                            //seq.Join(ll.transform.DOLocalMoveZ(llnewpos.z + cns_lowerleg_z, 0.01f));

                            //seq.Join(rl.transform.DOLocalMoveX(rlnewpos.x, 0.01f));
                            ////seq.Join(rl.transform.DOLocalMoveY(rlnewpos.y, 0.01f));
                            //seq.Join(rl.transform.DOLocalMoveZ(rlnewpos.z + cns_lowerleg_z, 0.01f));

                            seq.Join(hd.transform.DOLocalMoveX(hdnewpos.x, 0.01f));
                            //seq.Join(hd.transform.DOLocalMoveY(hdnewpos.y, 0.01f));
                            seq.Join(hd.transform.DOLocalMoveZ(hdnewpos.z, 0.01f));
                        }

                        seq.Play();
                    }
                    
                }
                

            }
            else if (PartsName == "leftshoulder") //---direct moving instead of IK
            {
                if ((this.transform.localPosition != oldPosition) || (this.transform.localRotation != oldRotation))
                {
                    Transform leftshoulder = animator.GetBoneTransform(HumanBodyBones.LeftShoulder);
                    Vector3 rot = transform.rotation.eulerAngles;
                    //rot.z = rot.z * -1;
                    seq.Join(leftshoulder.DORotate(rot, 0.1f));
                    //leftshoulder.rotation = Quaternion.Euler(leftshoulder.rotation.eulerAngles.x, leftshoulder.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                    //leftshoulder.LookAt(transform);
                    //transform.position = leftshoulder.position;
                    //transform.localRotation = leftshoulder.localRotation;
                }

            }
            else if (PartsName == "rightshoulder") //---direct moving instead of IK
            {
                if ((this.transform.localPosition != oldPosition) || (this.transform.localRotation != oldRotation)) 
                {
                    Transform rightshoulder = animator.GetBoneTransform(HumanBodyBones.RightShoulder);
                    Vector3 rot = transform.rotation.eulerAngles;
                    //rot.z = rot.z * -1;
                    seq.Join(rightshoulder.DORotate(rot, 0.1f));
                    //rightshoulder.rotation = Quaternion.Euler(rightshoulder.rotation.eulerAngles.x, rightshoulder.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
                    //rightshoulder.LookAt(transform);
                    //transform.position = rightshoulder.position;
                    //transform.localRotation = rightshoulder.localRotation;
                }

            }
            
            /*
            //---move and rotate divide
            if (transform.localPosition != oldPosition)
            {
                if (cur_operatetype != OperateType.MOVE)
                {
                    if (VvmIk.GetIKMarker(gameObject))
                    {
                        VvmIk.SetIKRotationWeight(transform.gameObject, 0f);
                    }
                    
                }
                cur_operatetype = OperateType.MOVE;
            }
            else
            {
                if (cur_operatetype != OperateType.ROTATE)
                {
                    if (VvmIk.GetIKMarker(gameObject))
                    {
                        //TODO: 1. apply bone rotation to ik rotation. 
                        //VvmIk.SetRotation_Bone2IK(transform.gameObject);
                        Quaternion qt = VvmIk.GetRotation_Bone(gameObject);
                        transform.rotation = qt;
                        //2. on rotation weight
                        VvmIk.SetIKRotationWeight(transform.gameObject, 1f);
                    }
                    
                }
                cur_operatetype = OperateType.ROTATE;


            }
            */

            CheckCurrentMarker();


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
        public void LoadDefaultTransform(bool ismove, bool isrotate)
        {
            if (ismove) transform.localPosition = defaultPosition;
            if (isrotate) transform.localRotation = defaultRotation;
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
            mat = relatedAvatar.GetComponent<ManageAvatarTransform>();
            ovrm = relatedAvatar.GetComponent<OperateLoadedVRM>();

        }
        public void CheckCurrentMarker()
        {
            is_current_marker = false;
            MeshRenderer mr = GetComponent<MeshRenderer>();
            for (int i = 0; i <  mr.sharedMaterials.Length; i++)
            {
                if (mr.sharedMaterials[i].shader.name.ToLower() == "custom/outline")
                {
                    is_current_marker = true;
                    break;
                }
            }
        }
    }
}