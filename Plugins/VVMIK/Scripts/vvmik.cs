using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LumisIkApp
{
    [Serializable]
    public class VvmIkConstraint
    {
        public Transform BoneTran = null;
        public Vector3 LimitFrom = Vector3.zero;
        public Vector3 LimitTo = Vector3.zero;

        public void Process()
        {
            Vector3 rot = BoneTran.localRotation.eulerAngles;
            rot.x = Mathf.Repeat(rot.x + 180f, 360f) - 180f;
            rot.y = Mathf.Repeat(rot.y + 180f, 360f) - 180f;
            rot.z = Mathf.Repeat(rot.z + 180f, 360f) - 180f;

            if ((LimitFrom.x != 0f) && (LimitTo.x != 0f) && (rot.x != 0f))
            {
                if (LimitFrom.x < rot.x) rot.x = LimitFrom.x;
                if (rot.x < LimitTo.x) rot.x = LimitTo.x;
            }
            if ((LimitFrom.y != 0f) && (LimitTo.y != 0f) && (rot.y != 0f))
            {
                if (LimitFrom.y < rot.y) rot.y = LimitFrom.y;
                if (rot.y < LimitTo.y) rot.y = LimitTo.y;
            }
            if ((LimitFrom.z != 0f) && (LimitTo.z != 0f) && (rot.z != 0f))
            {
                if (LimitFrom.z < rot.z) rot.z = LimitFrom.z;
                if (rot.z < LimitTo.z) rot.z = LimitTo.z;

            }
            BoneTran.localRotation = Quaternion.Euler(rot);
        }
    }
    public class VvmIk : MonoBehaviour
    {

        public bool IsApplyIK = true;

        [Header("LookAt")]
        public Transform lookAtObject = null;
        public float lookAtWeight = 1.0f;
        public float lookAtBodyWeight = 0f;
        public float lookAtHeadWeight = 1f;
        public float lookAtEyeWeight = 0f;
        public float lookAtClampWeight = 0f;

        [Space(10)]

        [Header("Head and Neck")]
        public Transform Head = null;
        public Transform Neck = null;
        public Vector3 NeckReversed = Vector3.zero;
        

        [Space(10)]

        [Header("Body")]
        public Transform UpperChest = null;
        public Transform Chest = null;
        public Transform Spine = null;
        public Vector3 SpineReversed = Vector3.zero;
        public Transform waist = null;
        public Transform Hips = null;


        [Space(10)]
        [Header("Right Arm")]
        public bool ActiveRightShoulder = true;
        public bool ActiveRightArm = true;
        public bool ActiveRightHand = true;

        public Transform RightShoulder = null;
        public Vector3 RightShoulderReversed = Vector3.zero;

        public Transform RightLowerArm = null;
        public float RightLowerArmWeight = 1.0f;

        public Transform RightHand = null;
        public float RightHandPositionWeight = 1f;
        public float RightHandRotationWeight = 1f;

        [Space(10)]
        [Header("Left Arm")]
        public bool ActiveLeftShoulder = true;
        public bool ActiveLeftArm = true;
        public bool ActiveLeftHand = true;

        public Transform LeftShoulder = null;
        public Vector3 LeftShoulderReversed = Vector3.zero;

        public Transform LeftLowerArm = null;
        public float LeftLowerArmWeight = 1.0f;

        public Transform LeftHand = null;
        public float LeftHandPositionWeight = 1f;
        public float LeftHandRotationWeight = 1f;

        [Space(10)]
        [Header("Right Leg")]
        public bool ActiveRightLeg = true;
        public bool ActiveRightFoot = true;
        public bool ActiveRightToes = true;

        public Transform RightLowerLeg = null;
        public float RightLowerLegWeight = 1f;

        public Transform RightFoot = null;
        public float RightFootPositionWeight = 1f;
        public float RightFootRotationWeight = 1f;

        public Transform RightToes = null;

        [Space(10)]
        [Header("Left Leg")]
        public bool ActiveLeftLeg = true;
        public bool ActiveLeftFoot = true;
        public bool ActiveLeftToes = true;

        public Transform LeftLowerLeg = null;
        public float LeftLowerLegWeight = 1f;

        public Transform LeftFoot = null;
        public float LeftFootPositionWeight = 1f;
        public float LeftFootRotationWeight = 1f;

        public Transform LeftToes = null;

        [Space(10)]
        [Header("Constraints")]
        public List<VvmIkConstraint> constraints = new List<VvmIkConstraint>();

        private Animator animator;
        private Dictionary<HumanBodyBones, Vector3> savedPosition = new Dictionary<HumanBodyBones, Vector3>();
        private Dictionary<HumanBodyBones, Vector3> savedRotation = new Dictionary<HumanBodyBones, Vector3>();


        private void Awake()
        {
            animator = GetComponent<Animator>();

            SaveDefaultPosition();
            SaveDefaultRotation();
        }
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            foreach (VvmIkConstraint con in constraints)
            {
                if (con.BoneTran != null)
                {
                    con.Process();
                }
            }
        }
        void OnAnimatorIK()
        {

            if (IsApplyIK == false) return;

            //---Directly bone transform----------------------------------------------------------------------------------
            if (Neck != null)
            {
                Vector3 rot = Neck.localRotation.eulerAngles;
                if (NeckReversed.x != 0) rot.x *= NeckReversed.x;
                if (NeckReversed.y != 0) rot.y *= NeckReversed.y;
                if (NeckReversed.z != 0) rot.z *= NeckReversed.z;
                animator.SetBoneLocalRotation(HumanBodyBones.Neck, Quaternion.Euler(rot));
            }
            if (Head != null)
            {
                animator.SetBoneLocalRotation(HumanBodyBones.Head, Head.localRotation);
            }
            if (UpperChest != null)
            {
                animator.SetBoneLocalRotation(HumanBodyBones.UpperChest, UpperChest.localRotation);
            }
            if (Chest != null)
            {
                animator.SetBoneLocalRotation(HumanBodyBones.Chest, Chest.localRotation);
            }
            if (Spine != null)
            {
                Vector3 rot = Spine.localRotation.eulerAngles;
                if (SpineReversed.x != 0) rot.x *= SpineReversed.x;
                if (SpineReversed.y != 0) rot.y *= SpineReversed.y;
                if (SpineReversed.z != 0) rot.z *= SpineReversed.z;
                animator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.Euler(rot));
            }
            if (Hips != null)
            {
                animator.SetBoneLocalRotation(HumanBodyBones.Hips, Hips.localRotation);
            }

            //---IK transform---------------------------------------------------------------------------------------------
            if (lookAtObject != null)
            {
                animator.SetLookAtWeight(lookAtWeight, lookAtBodyWeight, lookAtHeadWeight, lookAtEyeWeight, lookAtClampWeight);
                animator.SetLookAtPosition(lookAtObject.position);
            }
            if (waist != null)
            {
                animator.bodyPosition = waist.position;
                animator.bodyRotation = waist.rotation;
            }

            //===Shoulder, Arm, Hand===============================================
            if (ActiveRightShoulder)
            {
                if (RightShoulder != null)
                {
                    Vector3 rot = RightShoulder.localRotation.eulerAngles;
                    if (RightShoulderReversed.x != 0) rot.x *= RightShoulderReversed.x;
                    if (RightShoulderReversed.y != 0) rot.y *= RightShoulderReversed.y;
                    if (RightShoulderReversed.z != 0) rot.z *= RightShoulderReversed.z;
                    animator.SetBoneLocalRotation(HumanBodyBones.RightShoulder, Quaternion.Euler(rot));
                }
            }
            if (ActiveRightArm)
            {
                if (RightLowerArm != null)
                {
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, RightLowerArmWeight);
                    animator.SetIKHintPosition(AvatarIKHint.RightElbow, RightLowerArm.position);
                }
            }
            else
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, 0);
            }
            if (ActiveRightHand)
            {
                if (RightHand != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, RightHandPositionWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, RightHandRotationWeight);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, RightHand.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, RightHand.rotation);
                }
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
            }

            if (ActiveLeftShoulder)
            {
                if (LeftShoulder != null)
                {
                    Vector3 rot = LeftShoulder.localRotation.eulerAngles;
                    if (LeftShoulderReversed.x != 0) rot.x *= LeftShoulderReversed.x;
                    if (LeftShoulderReversed.y != 0) rot.y *= LeftShoulderReversed.y;
                    if (LeftShoulderReversed.z != 0) rot.z *= LeftShoulderReversed.z;
                    animator.SetBoneLocalRotation(HumanBodyBones.LeftShoulder, Quaternion.Euler(rot));
                }
            }
            if (ActiveLeftArm)
            {
                if (LeftLowerArm != null)
                {
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, LeftLowerArmWeight);
                    animator.SetIKHintPosition(AvatarIKHint.LeftElbow, LeftLowerArm.position);
                }
            }
            else
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftElbow, 0);
            }
            if (ActiveLeftHand)
            {
                if (LeftHand != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, LeftHandPositionWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, LeftHandRotationWeight);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, LeftHand.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, LeftHand.rotation);
                }
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 0);
            }



            //===Leg, Foot==============================================================================
            if (ActiveRightLeg)
            {
                if (RightLowerLeg != null)
                {
                    animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, RightLowerLegWeight);
                    animator.SetIKHintPosition(AvatarIKHint.RightKnee, RightLowerLeg.position);
                }
            }
            else
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.RightKnee, 0);
            }
            if (ActiveRightFoot)
            {
                if (RightFoot != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, RightFootPositionWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, RightFootRotationWeight);
                    animator.SetIKPosition(AvatarIKGoal.RightFoot, RightFoot.position);
                    animator.SetIKRotation(AvatarIKGoal.RightFoot, RightFoot.rotation);
                }
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
            }
            if (ActiveRightToes)
            {
                if (RightToes != null)
                {
                    animator.SetBoneLocalRotation(HumanBodyBones.RightToes, RightToes.localRotation);
                }
            }

            
            if (ActiveLeftLeg)
            {
                if (LeftLowerLeg != null)
                {
                    animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, LeftLowerLegWeight);
                    animator.SetIKHintPosition(AvatarIKHint.LeftKnee, LeftLowerLeg.position);
                }
            }
            else
            {
                animator.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 0);
            }
            if (ActiveLeftFoot)
            {
                if (LeftFoot != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, LeftFootPositionWeight);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, LeftFootRotationWeight);
                    animator.SetIKPosition(AvatarIKGoal.LeftFoot, LeftFoot.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftFoot, LeftFoot.rotation);
                }
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
            }
            if (ActiveLeftToes)
            {
                if (LeftToes != null)
                {
                    animator.SetBoneLocalRotation(HumanBodyBones.LeftToes, LeftToes.localRotation);
                }
            }
            
        }


        /// <summary>
        /// Set rotation of the bone manually.
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="transform"></param>
        public void SetManuallyTransform(HumanBodyBones bone, Transform transform)
        {
            animator.SetBoneLocalRotation(bone, transform.localRotation);
        }

        /// <summary>
        /// Set rotation of the bone manually.
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="rotation"></param>
        public void SetManuallyRotation(HumanBodyBones bone, Vector3 rotation)
        {
            animator.SetBoneLocalRotation(bone, Quaternion.Euler(rotation));
        }


        /// <summary>
        /// Save default pose position (T-pose)
        /// </summary>
        public void SaveDefaultPosition()
        {
            foreach (HumanBodyBones hBone in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if ((hBone !=  HumanBodyBones.Jaw) && (hBone != HumanBodyBones.LastBone))
                {
                    
                    Transform tran = animator.GetBoneTransform(hBone);
                    if (tran != null)
                    {
                        Vector3 pos = new Vector3();
                        pos.x = tran.position.x;
                        pos.y = tran.position.y;
                        pos.z = tran.position.z;
                        savedPosition.Add(hBone, pos);
                    }
                    
                }
                
            }
        }
        /// <summary>
        /// Save default pose rotation (T-pose)
        /// </summary>
        public void SaveDefaultRotation()
        {
            foreach (HumanBodyBones hBone in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if ((hBone != HumanBodyBones.Jaw) && (hBone != HumanBodyBones.LastBone))
                {
                    Transform tran = animator.GetBoneTransform(hBone);
                    if (tran != null)
                    {
                        Vector3 defrot = tran.rotation.eulerAngles;

                        Vector3 rot = new Vector3();
                        rot.x = defrot.x;
                        rot.y = defrot.y;
                        rot.z = defrot.z;
                        savedRotation.Add(hBone, rot);
                    }
                    
                }
                
            }
        }

    }

}
