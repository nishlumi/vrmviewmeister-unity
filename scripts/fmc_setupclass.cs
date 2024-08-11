using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRM;
using RootMotion.FinalIK;
using LumisIkApp;
using UserHandleSpace;

namespace UserVRMSpace
{

    public partial class FileMenuCommands
    {

        /*===================================================================================================
         * Set up VRIK for this VRM avatar
         * 
         */
        void SetupVRIK(GameObject ikhandles, Animator animator, VRIK vik, Vector3 bnd)
        {
            //GameObject par = GameObject.FindGameObjectWithTag("IKHandleWorld");
            int cnt = ikhandles.transform.childCount;


            //GameObject[] hans = GameObject.FindGameObjectsWithTag("IKHandle");
            for (int i = 0; i < cnt; i++)
            {
                GameObject hans = ikhandles.transform.GetChild(i).gameObject;
                if (hans.name.IndexOf("Head") > -1)
                {
                    Transform hanstrans = animator.GetBoneTransform(HumanBodyBones.Head);
                    Vector3 newpos;
                    newpos.x = hanstrans.position.x;
                    newpos.y = bnd.y < hanstrans.position.y ? hanstrans.position.y : bnd.y; // hanstrans.position.y + (hanstrans.localScale.y / 2);
                    newpos.z = hanstrans.position.z;
                    hans.transform.position = newpos;

                    Debug.Log("thead_position=" + hanstrans.localScale);
                    Debug.Log("thead_size=" + hanstrans.lossyScale);
                    //hans.transform.position.Set(hans.transform.position.x, this.thead_position.y, hans.transform.position.z);
                    vik.solver.spine.headTarget = hans.transform;
                    vik.solver.spine.positionWeight = 1.0f;
                    vik.solver.spine.rotationWeight = 1.0f;

                }
                else if (hans.name.IndexOf("Chest") > -1)
                {
                    Vector3 anim = animator.GetBoneTransform(HumanBodyBones.Chest).position;
                    Vector3 newpos;
                    newpos.x = anim.x;
                    newpos.y = anim.y;
                    newpos.z = hans.transform.position.z;
                    hans.transform.position = newpos;

                    vik.solver.spine.chestGoal = hans.transform;
                    vik.solver.spine.chestGoalWeight = 1f;
                    vik.solver.spine.chestClampWeight = 0.5f;
                    vik.solver.spine.rotateChestByHands = 0.5f;

                }
                else if (hans.name.IndexOf("LeftLowerArm") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position;
                    hans.transform.position = hanspos;

                    vik.solver.leftArm.bendGoal = hans.transform;
                    vik.solver.leftArm.bendGoalWeight = 0.5f;
                    vik.solver.leftArm.swivelOffset = 0;
                }
                else if (hans.name.IndexOf("RightLowerArm") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position;
                    hans.transform.position = hanspos;

                    vik.solver.rightArm.bendGoal = hans.transform;
                    vik.solver.rightArm.bendGoalWeight = 0.5f;
                    vik.solver.rightArm.swivelOffset = 0;
                }
                else if (hans.name.IndexOf("LeftHand") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
                    hans.transform.position = hanspos;

                    vik.solver.leftArm.target = hans.transform;
                    vik.solver.leftArm.positionWeight = 1.0f;
                    vik.solver.leftArm.rotationWeight = 1.0f;
                    //vik.solver.leftArm.bendGoal = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                    //vik.solver.leftArm.bendGoalWeight = 0.5f;
                    //vik.solver.leftArm.swivelOffset = 0;

                    /*GameObject should = animator.GetBoneTransform(HumanBodyBones.LeftShoulder).gameObject;
                    RotationLimitHinge hinge = should.AddComponent<RotationLimitHinge>();
                    hinge.axis.y = -1;
                    hinge.min = 23f;
                    hinge.max = -147.25f;
                    hinge.useLimits = true;*/


                }
                else if (hans.name.IndexOf("RightHand") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.RightHand).position;

                    hans.transform.position = hanspos;

                    vik.solver.rightArm.target = hans.transform;
                    vik.solver.rightArm.positionWeight = 1.0f;
                    vik.solver.rightArm.rotationWeight = 1.0f;
                    //vik.solver.rightArm.bendGoal = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                    //vik.solver.rightArm.bendGoalWeight = 0.5f;
                    //vik.solver.rightArm.swivelOffset = 0;

                    /* GameObject should = animator.GetBoneTransform(HumanBodyBones.RightShoulder).gameObject;
                     RotationLimitHinge hinge = should.AddComponent<RotationLimitHinge>();
                     hinge.axis.y = 1;
                     hinge.min = 23f;
                     hinge.max = -147.25f;
                     hinge.useLimits = true;*/

                }
                else if (hans.name.IndexOf("Pelvis") > -1)
                {
                    Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                    Transform spine = animator.GetBoneTransform(HumanBodyBones.Spine);
                    Vector3 newpos = new Vector3();
                    newpos.x = spine.position.x;
                    newpos.y = hips.position.y;
                    newpos.z = spine.position.z;
                    hans.transform.position = newpos;

                    vik.solver.spine.pelvisTarget = hans.transform;
                    vik.solver.spine.pelvisPositionWeight = 1.0f;
                    vik.solver.spine.pelvisRotationWeight = 1.0f;
                    vik.solver.spine.maintainPelvisPosition = 1.0f;

                }
                else if (hans.name.IndexOf("LeftLowerLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                    hans.transform.position = new Vector3(pt.position.x, pt.position.y, -0.2f);

                    vik.solver.leftLeg.bendGoal = hans.transform;
                    vik.solver.leftLeg.bendGoalWeight = 1f;
                    vik.solver.leftLeg.bendToTargetWeight = 1.0f;
                    //vik.solver.leftLeg.swivelOffset = -65;

                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.min = -4.920818f;
                    rlh.max = 134.0647f;
                    rlh.axis.x = 0f;
                    rlh.axis.y = 1;

                }
                else if (hans.name.IndexOf("RightLowerLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                    hans.transform.position = new Vector3(pt.position.x, pt.position.y, -0.2f);

                    vik.solver.rightLeg.bendGoal = hans.transform;
                    vik.solver.rightLeg.bendGoalWeight = 1f;
                    vik.solver.rightLeg.bendToTargetWeight = 1.0f;
                    //vik.solver.rightLeg.swivelOffset = 65;

                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.min = -4.920818f;
                    rlh.max = 134.0647f;
                    rlh.axis.x = 0f;
                    rlh.axis.y = 1;

                }
                else if (hans.name.IndexOf("LeftLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    //Debug.Log(pt.gameObject.GetComponent<Renderer>().bounds.size);
                    Vector3 hanspos = pt.position;
                    Vector3 hanssize = pt.localScale;
                    hanspos.y = hanspos.y / 2f; //hans.transform.position.y + (hans.transform.localScale.normalized.y / 10f / 2.0f);
                    //hanspos.z *= -1;
                    hanspos.z = -(bnd.z / 2f / 10f); // hans.transform.localScale.normalized.z;
                    hans.transform.position = hanspos;

                    vik.solver.leftLeg.target = hans.transform;
                    vik.solver.leftLeg.positionWeight = 1.0f;
                    vik.solver.leftLeg.rotationWeight = 1.0f;
                    //vik.solver.leftLeg.bendGoal = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                    //vik.solver.leftLeg.bendGoalWeight = 0.5f;
                    //vik.solver.leftLeg.bendToTargetWeight = 1.0f;
                    //vik.solver.leftLeg.swivelOffset = -65;



                }
                else if (hans.name.IndexOf("RightLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                    Vector3 hanspos = pt.position;
                    Vector3 hanssize = pt.localScale;
                    hanspos.y = hanspos.y / 2f;  //hans.transform.position.y + (hans.transform.localScale.normalized.y / 10f / 2.0f);
                    //hanspos.z *= -1;
                    hanspos.z = -(bnd.z / 2.0f / 10f);  // hans.transform.localScale.normalized.z;
                    hans.transform.position = hanspos;

                    vik.solver.rightLeg.target = hans.transform;
                    vik.solver.rightLeg.positionWeight = 1.0f;
                    vik.solver.rightLeg.rotationWeight = 1.0f;
                    //vik.solver.rightLeg.bendGoal = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                    //vik.solver.rightLeg.bendGoalWeight = 0.5f;
                    //vik.solver.rightLeg.bendToTargetWeight = 1.0f;
                    //vik.solver.rightLeg.swivelOffset = 65;

                }
            }

        }

        /*===================================================================================================
         * Set up FullBodyBipedIK for this VRM avatar
         * 
         */

        IEnumerator SetupFullBodyIK(GameObject ikhandles, Animator animator, FullBodyBipedIK vik, LookAtIK laik, CCDIK cik, Vector3 bnd)
        {
            int cnt = ikhandles.transform.childCount;

            animator.gameObject.transform.rotation = Quaternion.Euler(0, 180f, 0f);


            for (int i = 0; i < cnt; i++)
            {
                GameObject hans = ikhandles.transform.GetChild(i).gameObject;


                if (hans.name.IndexOf("Head") > -1)
                {
                    Transform necktrans = animator.GetBoneTransform(HumanBodyBones.Neck);
                    Transform headtrans = animator.GetBoneTransform(HumanBodyBones.Head);
                    Vector3 newpos;
                    newpos.x = headtrans.position.x;
                    newpos.y = headtrans.position.y + 0.1f; // bnd.y < headtrans.position.y ? headtrans.position.y : bnd.y; // hanstrans.position.y + (hanstrans.localScale.y / 2);
                    newpos.z = headtrans.position.z;

                    //vik.solver.headMapping.bone = headtrans;
                    /*
                    laik.solver.target = hans.transform;
                    laik.solver.bodyWeight = 0f;
                    laik.solver.headWeight = 1f;
                    laik.solver.head.transform = headtrans;
                    */

                    Transform[] chaintrans = { null };
                    if (necktrans == null)
                    {
                        chaintrans[0] = headtrans;
                    }
                    else
                    {
                        chaintrans[0] = necktrans;
                    }

                    cik.solver.SetChain(chaintrans, headtrans); //new Transform[1] { necktrans }
                    //cik.solver.AddBone(necktrans);
                    cik.solver.AddBone(headtrans);
                    cik.solver.bones[0].weight = 1f;
                    cik.solver.bones[1].weight = 1f;
                    cik.solver.target = hans.transform;
                    cik.solver.SetIKPositionWeight(1.0f);
                    cik.solver.tolerance = 1;
                    cik.solver.useRotationLimits = true;
                    cik.solver.maxIterations = 2;

                    //2022.05.17 -- Leave control to UserHandleOperation (DummyIK) 
                    cik.enabled = false;
                    //hans.GetComponent<UserHandleSpace.UserHandleOperation>().enabled = false;
                    yield return null;
                    //necktrans.Rotate(new Vector3(-13f, 0f, 0f));
                    hans.transform.position = newpos;
                    hans.transform.rotation = Quaternion.Euler(new Vector3(0, -180f, 0f));


                }
                else if (hans.name.IndexOf("LookAt") > -1)
                {
                    Transform necktrans = animator.GetBoneTransform(HumanBodyBones.Neck);
                    Transform headtrans = animator.GetBoneTransform(HumanBodyBones.Head);

                    Vector3 newpos;
                    newpos.x = headtrans.position.x;
                    newpos.y = headtrans.position.y; // bnd.y < headtrans.position.y ? headtrans.position.y : bnd.y;
                    newpos.z = -0.5f;
                    hans.transform.position = newpos;

                    
                    laik.solver.target = hans.transform;
                    laik.solver.eyes = new IKSolverLookAt.LookAtBone[0];
                    laik.solver.IKPositionWeight = 1f;
                    laik.solver.bodyWeight = 0f;
                    laik.solver.headWeight = 0.5f;
                    laik.solver.eyesWeight = 0f;
                    laik.solver.clampWeight = 0f;
                    laik.solver.clampWeightEyes = 0f;
                    laik.solver.clampWeightHead = 0.5f;
                    laik.solver.head.transform = headtrans;
                    laik.solver.head.axis = new Vector3(0f, 0f, 1f);

                    laik.enabled = true;
                }              
                else if (hans.name.IndexOf("Chest") > -1)
                {
                    Transform necktrans = animator.GetBoneTransform(HumanBodyBones.Neck);
                    Transform LeftSho = hans.transform.Find("LeftShoulder");
                    Transform RightSho = hans.transform.Find("RightShoulder");

                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.LeftShoulder).position;
                    //LeftSho.position = hanspos;

                    vik.solver.leftShoulderEffector.target = LeftSho;
                    vik.solver.leftShoulderEffector.positionWeight = 1f;
                    vik.solver.leftShoulderEffector.rotationWeight = 1f;

                    hanspos = animator.GetBoneTransform(HumanBodyBones.RightShoulder).position;
                    //RightSho.position = hanspos;

                    vik.solver.rightShoulderEffector.target = RightSho;
                    vik.solver.rightShoulderEffector.positionWeight = 1f;
                    vik.solver.rightShoulderEffector.rotationWeight = 1f;

                    //LeftSho.gameObject.layer = 10;
                    //RightSho.gameObject.layer = 10;

                    hans.transform.position = necktrans.position;
                }
                /*
                else if (hans.name.IndexOf("LeftShoulder") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.LeftShoulder).position;
                    hans.transform.localPosition = hanspos;

                    vik.solver.leftShoulderEffector.target = hans.transform;
                    vik.solver.leftShoulderEffector.positionWeight = 1f;
                    vik.solver.leftShoulderEffector.rotationWeight = 1f;

                }
                else if (hans.name.IndexOf("RightShoulder") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.RightShoulder).position;
                    hans.transform.localPosition = hanspos;

                    vik.solver.rightShoulderEffector.target = hans.transform;
                    vik.solver.rightShoulderEffector.positionWeight = 1f;
                    vik.solver.rightShoulderEffector.rotationWeight = 1f;
                }
                */
                else if (hans.name.IndexOf("LeftLowerArm") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position;
                    hans.transform.localPosition = hanspos;

                    vik.solver.leftArmChain.bendConstraint.bendGoal = hans.transform;
                    vik.solver.leftArmChain.bendConstraint.weight = 1f;
                    
                }
                else if (hans.name.IndexOf("RightLowerArm") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position;
                    hans.transform.localPosition = hanspos;

                    vik.solver.rightArmChain.bendConstraint.bendGoal = hans.transform;
                    vik.solver.rightArmChain.bendConstraint.weight = 1f;
                }
                else if (hans.name.IndexOf("LeftHand") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
                    hans.transform.localPosition = hanspos;
                    hans.transform.Rotate(new Vector3(0, 180f, 0f));

                    vik.solver.leftHandEffector.target = hans.transform;
                    vik.solver.leftHandEffector.positionWeight = 1.0f;
                    vik.solver.leftHandEffector.rotationWeight = 1.0f;
                }
                else if (hans.name.IndexOf("RightHand") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.RightHand).position;

                    hans.transform.localPosition = hanspos;
                    hans.transform.Rotate(new Vector3(0, 180f, 0f));

                    vik.solver.rightHandEffector.target = hans.transform;
                    vik.solver.rightHandEffector.positionWeight = 1.0f;
                    vik.solver.rightHandEffector.rotationWeight = 1.0f;

                }
                else if (hans.name.IndexOf("Aim") > -1)
                {
                    Transform hips = animator.GetBoneTransform(HumanBodyBones.Chest);
                    Transform spine = animator.GetBoneTransform(HumanBodyBones.Spine);
                    Vector3 newpos = new Vector3();
                    newpos.x = spine.position.x;
                    newpos.y = hips.position.y;
                    newpos.z = spine.position.z;
                    hans.transform.position = newpos;

                    vik.solver.bodyEffector.target = hans.transform;
                    vik.solver.bodyEffector.positionWeight = 1.0f;
                    vik.solver.bodyEffector.rotationWeight = 1.0f;

                    UserHandleOperation uho = hans.GetComponent<UserHandleOperation>();
                    uho.IsFixTransform = false;
                }
                else if (hans.name.IndexOf("Pelvis") > -1)
                {
                    Transform LeftULeg = hans.transform.Find("LeftUpperLeg");
                    Transform RightULeg = hans.transform.Find("RightUpperLeg");

                    Transform pt = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                    //LeftULeg.position = pt.position;

                    vik.solver.leftThighEffector.target = LeftULeg;
                    vik.solver.leftThighEffector.positionWeight = 1.0f;
                    vik.solver.leftThighEffector.rotationWeight = 1.0f;
                    

                    pt = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                    //RightULeg.position = pt.position;

                    vik.solver.rightThighEffector.target = RightULeg;
                    vik.solver.rightThighEffector.positionWeight = 1.0f;
                    vik.solver.rightThighEffector.rotationWeight = 1.0f;

                    LeftULeg.gameObject.layer = 10;
                    RightULeg.gameObject.layer = 10;
                }
                /*
                else if (hans.name.IndexOf("LeftUpperLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                    hans.transform.localPosition = pt.position;

                    vik.solver.leftThighEffector.target = hans.transform;
                    vik.solver.leftThighEffector.positionWeight = 1.0f;
                    vik.solver.leftThighEffector.rotationWeight = 1.0f;
                }
                */
                else if (hans.name.IndexOf("LeftLowerLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                    hans.transform.position = pt.position;
                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.axis.x = 1f;
                    rlh.axis.y = 0f;
                    rlh.axis.z = 0f;
                    rlh.min = -16f;
                    rlh.max = 145f;
                    hans.transform.localPosition = pt.position;


                    vik.solver.leftLegChain.bendConstraint.bendGoal = hans.transform;
                    vik.solver.leftLegChain.bendConstraint.weight = 1.0f;

                    hans.transform.position = new Vector3(pt.position.x * 1f, pt.position.y, pt.position.z - 0.1f);
                }
                else if (hans.name.IndexOf("LeftLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.LeftFoot);

                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.axis.x = 1f;
                    rlh.axis.y = 0f;
                    rlh.axis.z = 0f;
                    rlh.min = -35f;
                    rlh.max = 60f;
                    /*Vector3 hanspos = pt.position;
                    Vector3 hanssize = pt.localScale;
                    hanspos.y = hanspos.y / 2f;
                    hanspos.z = -(bnd.z / 2f / 10f);
                    hans.transform.position = hanspos;*/
                    hans.transform.localPosition = pt.position;

                    vik.solver.leftFootEffector.target = hans.transform;
                    vik.solver.leftFootEffector.positionWeight = 1.0f;
                    vik.solver.leftFootEffector.rotationWeight = 1.0f;

                }
                /*
                else if (hans.name.IndexOf("RightUpperLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                    hans.transform.localPosition = pt.position;

                    vik.solver.rightThighEffector.target = hans.transform;
                    vik.solver.rightThighEffector.positionWeight = 1.0f;
                    vik.solver.rightThighEffector.rotationWeight = 1.0f;
                }
                */
                else if (hans.name.IndexOf("RightLowerLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                    hans.transform.position = pt.position;
                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.axis.x = 1f;
                    rlh.axis.y = 0f;
                    rlh.axis.z = 0f;
                    rlh.min = -16f;
                    rlh.max = 145f;
                    hans.transform.localPosition = pt.position;

                    vik.solver.rightLegChain.bendConstraint.bendGoal = hans.transform;
                    vik.solver.rightLegChain.bendConstraint.weight = 1.0f;

                    hans.transform.position = new Vector3(pt.position.x * 1f, pt.position.y, pt.position.z - 0.1f);
                }
                else if (hans.name.IndexOf("RightLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.axis.x = 1f;
                    rlh.axis.y = 0f;
                    rlh.axis.z = 0f;
                    rlh.min = -35f;
                    rlh.max = 60f;

                    /*Vector3 hanspos = pt.position;
                    Vector3 hanssize = pt.localScale;
                    hanspos.y = hanspos.y / 2f;
                    hanspos.z = -(bnd.z / 2f / 10f);
                    hans.transform.position = hanspos;*/
                    hans.transform.localPosition = pt.position;

                    vik.solver.rightFootEffector.target = hans.transform;
                    vik.solver.rightFootEffector.positionWeight = 1.0f;
                    vik.solver.rightFootEffector.rotationWeight = 1.0f;
                }


            }
            yield return null;
            pendingInstance.ShowMeshes();
        }
        /*===================================================================================================
         * Set up BipedIK for this VRM avatar
         * 
         */

        IEnumerator SetupBipedIK(GameObject ikhandles, Animator animator, BipedIK vik, CCDIK cik, Vector3 bnd)
        {
            Transform[] bts = ikhandles.GetComponentsInChildren<Transform>();

            string[] IKBoneNames = new string[]{
                //"IKParent",
                 "Pelvis","Chest","Head","Aim","LookAt",

                "LeftLeg","RightLeg",
                "LeftLowerLeg","RightLowerLeg",

                "LeftShoulder","RightShoulder",
                "LeftHand","RightHand",
                "LeftLowerArm","RightLowerArm",

                "EyeViewHandle"
            };
            List<Transform> sortedBt = new List<Transform>();
            for (int i = 0; i < IKBoneNames.Length; i++)
            {
                for (int j = 0; j < bts.Length; j++)
                {
                    Transform t = bts[j];
                    if (t.name == IKBoneNames[i])
                    {
                        sortedBt.Add(t);
                    }
                }
            }

            int cnt = sortedBt.Count; // ikhandles.transform.childCount;
            Transform[] setup_spinetrans = { animator.GetBoneTransform(HumanBodyBones.Spine), animator.GetBoneTransform(HumanBodyBones.Chest) };
            vik.solvers.spine.SetChain(setup_spinetrans, animator.gameObject.transform);

            animator.gameObject.transform.rotation = Quaternion.Euler(0, 0f, 0f);
            yield return null;

            Vector3 chestpos = Vector3.zero;
            Vector3 lefthandpos = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
            Vector3 righthandpos = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
            Vector3 leftlapos = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position;
            Vector3 rightlapos = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position;

            for (int i = 0; i < cnt; i++)
            {
                //yield return null;

                GameObject hans = sortedBt[i].gameObject; // ikhandles.transform.GetChild(i).gameObject;
                //UserHandleSpace.UserHandleOperation uho = hans.GetComponent<UserHandleSpace.UserHandleOperation>();

                if (hans.name.IndexOf("Head") > -1)
                {
                    Transform necktrans = animator.GetBoneTransform(HumanBodyBones.Neck);
                    Transform headtrans = animator.GetBoneTransform(HumanBodyBones.Head);
                    Vector3 newpos;
                    newpos.x = headtrans.position.x;
                    newpos.y = headtrans.position.y + 0.1f; // bnd.y < hanstrans.position.y ? hanstrans.position.y : bnd.y;
                    newpos.z = 0f;
                    hans.transform.position = newpos;
                    hans.transform.rotation = Quaternion.Euler(0f, -180f, 0f);

                    Transform[] chaintrans = { null  };
                    if (necktrans == null)
                    {
                        chaintrans[0] = headtrans;
                    }
                    else
                    {
                        chaintrans[0] = necktrans;
                    }

                    cik.solver.SetChain(chaintrans, headtrans); //new Transform[1] { necktrans }
                    //cik.solver.AddBone(necktrans);
                    cik.solver.AddBone(headtrans);
                    cik.solver.bones[0].weight = 1f;
                    cik.solver.bones[1].weight = 1f;
                    cik.solver.target = hans.transform;
                    cik.solver.SetIKPositionWeight(1.0f);
                    cik.solver.tolerance = 1;
                    cik.solver.useRotationLimits = true;
                    cik.solver.maxIterations = 2;

                    //2022.05.17 -- Leave control to UserHandleOperation (DummyIK) 
                    cik.enabled = false;
                }
                else if (hans.name.IndexOf("EyeViewHandle") > -1)
                {
                    Transform rla = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                    Transform lla = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                    Debug.Log(rla.position);
                    Debug.Log(lla.position);
                }
                else if (hans.name.IndexOf("REAL:LookAt") > -1)
                {


                }
                else if (hans.name.IndexOf("LookAt") > -1)
                {
                    
                    Transform hanstrans = animator.GetBoneTransform(HumanBodyBones.Neck);
                    if (hanstrans == null) hanstrans = animator.GetBoneTransform(HumanBodyBones.Head);

                    Vector3 newpos;
                    newpos.x = hanstrans.position.x;
                    newpos.y = hanstrans.position.y + 0.1f; // bnd.y < hanstrans.position.y ? hanstrans.position.y : bnd.y;
                    newpos.z = ((bnd.z < 0 ? bnd.z : bnd.z * -1f) - 0.2f) * -1;
                    //hans.transform.position = newpos;

                    //---only set IK marker of effective transform to the BipedIK.
                    vik.solvers.lookAt.target = hans.transform;
                    vik.solvers.lookAt.eyes = new IKSolverLookAt.LookAtBone[0];
                    vik.references.eyes = new Transform[0];
                    vik.solvers.lookAt.head.axis = new Vector3(0f, 0f, 1f);
                    vik.solvers.lookAt.headWeight = 1.0f;
                    vik.solvers.lookAt.IKPositionWeight = 1.0f;
                    vik.solvers.lookAt.head.weight = 1.0f;
                    vik.solvers.lookAt.bodyWeight = 0f;
                    vik.solvers.lookAt.eyesWeight = 0f;

                }
                /*else if (hans.name == "REAL:Aim")
                {
                    //---locate Showable IK marker to target position
                    Transform hanstrans = animator.GetBoneTransform(HumanBodyBones.Chest);
                    if (hanstrans == null) hanstrans = animator.GetBoneTransform(HumanBodyBones.Spine);
                    hans.transform.position = new Vector3(hanstrans.position.x, hanstrans.position.y + 0.1f, ((bnd.z < 0 ? bnd.z : bnd.z * -1f) - 0.25f) * -1);


                }*/
                else if (hans.name == "Aim")
                {
                    //---locate Showable IK marker to target position
                    Transform hanstrans = animator.GetBoneTransform(HumanBodyBones.Chest);
                    if (hanstrans == null) hanstrans = animator.GetBoneTransform(HumanBodyBones.Spine);
                    hans.transform.position = new Vector3(hanstrans.position.x, hanstrans.position.y + 0.1f, (bnd.z < 0 ? bnd.z : bnd.z * -1f) - 0.25f);
                    //UserHandleSpace.UserVRM10IK uvik = hans.GetComponent<UserHandleSpace.UserVRM10IK>();
                    //uvik.SetOldPotion(hans.transform.localPosition);
                    /*
                    Transform realaim = ikhandles.transform.Find("REAL:Aim");
                    if (realaim != null)
                    {
                        realaim.localPosition = new Vector3(hans.transform.localPosition.x * -1, hans.transform.localPosition.y, hans.transform.localPosition.z * -1);
                    }*/
                    //---only set IK marker of effective transform to the BipedIK.
                    vik.solvers.aim.target = hans.transform;
                    vik.solvers.aim.IKPositionWeight = 1f;
                    vik.solvers.aim.clampWeight = 0.5f;
                    vik.solvers.aim.transform = animator.GetBoneTransform(HumanBodyBones.UpperChest);
                    //---if not found UpperChest, insteadly set Chest
                    if (vik.solvers.aim.transform == null) vik.solvers.aim.transform = animator.GetBoneTransform(HumanBodyBones.Chest);
                    vik.solvers.aim.useRotationLimits = true;

                }
                else if (hans.name.IndexOf("Chest") > -1)
                {
                    Transform hanstrans = animator.GetBoneTransform(HumanBodyBones.Neck);
                    if (hanstrans == null) hanstrans = animator.GetBoneTransform(HumanBodyBones.Spine);
                    hans.transform.position = new Vector3(hanstrans.position.x, hanstrans.position.y, hanstrans.position.z);
                    //hans.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
                    chestpos = hans.transform.position;

                    vik.solvers.spine.target = hans.transform;
                    vik.solvers.spine.IKPositionWeight = 1.0f;
                    

                }
                else if (hans.name.IndexOf("LeftShoulder") > -1)
                {
                    //Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position;
                    //hanspos.x /= -2f;
                    //hans.transform.position = hanspos;
                    //hans.transform.rotation = Quaternion.Euler(270f, 0f, 0f);


                }
                else if (hans.name.IndexOf("RightShoulder") > -1)
                {
                    //hans.transform.rotation = Quaternion.Euler(270f, 0f, 0f);

                }
                else if (hans.name.IndexOf("LeftLowerArm") > -1)
                {
                    //yield return null;
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                    Vector3 hanspos = pt.position;
                    hanspos.z = 0.15f;
                    hanspos.x *= -1f;

                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.axis.x = 0f;
                    rlh.axis.y = 1f;
                    rlh.axis.z = 0f;
                    rlh.min = -105f;
                    rlh.max = 93f;

                    vik.solvers.leftHand.bendModifier = IKSolverLimb.BendModifier.Goal;
                    vik.solvers.leftHand.bendGoal = hans.transform;
                    vik.solvers.leftHand.bendModifierWeight = 1f;
                    //vik.solvers.ikSolvers[0]
                    

                    //rightlapos.z = chestpos.z * 2f;
                    hans.transform.position = hanspos;
                    hans.transform.rotation = Quaternion.Euler(0, 180f, 0);
                }
                else if (hans.name.IndexOf("RightLowerArm") > -1)
                {
                    //yield return null;
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                    Vector3 hanspos = pt.position;
                    hanspos.z = 0.15f;
                    hanspos.x *= -1f;

                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.axis.x = 0f;
                    rlh.axis.y = 1f;
                    rlh.axis.z = 0f;
                    rlh.min = -105f;
                    rlh.max = 93f;

                    vik.solvers.rightHand.bendModifier = IKSolverLimb.BendModifier.Goal;
                    vik.solvers.rightHand.bendGoal = hans.transform;
                    vik.solvers.rightHand.bendModifierWeight = 1f;
                   

                    //leftlapos.z = chestpos.z * 2f;
                    hans.transform.position = hanspos;
                    hans.transform.rotation = Quaternion.Euler(0, 180f, 0);
                }
                else if (hans.name.IndexOf("LeftHand") > -1)
                {
                    //Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
                    //hanspos.z = chestpos.z;
                    righthandpos.z = 0;
                    hans.transform.position = righthandpos;
                    hans.transform.rotation = Quaternion.Euler(0, 180f, 0);

                    vik.solvers.leftHand.target = hans.transform;
                    vik.solvers.leftHand.IKPositionWeight = 1.0f;
                    vik.solvers.leftHand.IKRotationWeight = 1.0f;
                }
                else if (hans.name.IndexOf("RightHand") > -1)
                {
                    //Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
                    //hanspos.z = chestpos.z;
                    lefthandpos.z = 0;
                    hans.transform.position = lefthandpos;
                    hans.transform.rotation = Quaternion.Euler(0, 180f, 0);

                    vik.solvers.rightHand.target = hans.transform;
                    vik.solvers.rightHand.IKPositionWeight = 1.0f;
                    vik.solvers.rightHand.IKRotationWeight = 1.0f;

                }
                else if (hans.name.IndexOf("Pelvis") > -1)
                {
                    Transform upche = animator.GetBoneTransform(HumanBodyBones.Hips);
                    Transform spine = animator.GetBoneTransform(HumanBodyBones.Spine);
                    Vector3 newpos = new Vector3();
                    newpos.x = upche.position.x;
                    newpos.y = upche.position.y;
                    newpos.z = upche.position.z;
                    hans.transform.position = newpos;
                    hans.transform.rotation = Quaternion.Euler(0, 180f, 0);

                    vik.solvers.pelvis.transform = animator.GetBoneTransform(HumanBodyBones.Hips);
                    vik.solvers.pelvis.target = hans.transform;
                    vik.solvers.pelvis.positionWeight = 1.0f;
                    vik.solvers.pelvis.rotationWeight = 1.0f;
                }
                else if (hans.name.IndexOf("LeftLowerLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);

                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.axis.x = 1f;
                    rlh.axis.y = 0f;
                    rlh.axis.z = 0f;
                    rlh.min = -16f;
                    rlh.max = 145f;

                    vik.solvers.leftFoot.bendModifier = IKSolverLimb.BendModifier.Goal;
                    vik.solvers.leftFoot.bendGoal = hans.transform;
                    vik.solvers.leftFoot.bendModifierWeight = 1.0f;
                    //yield return null;
                    hans.transform.position = new Vector3(pt.position.x*-1f, pt.position.y, pt.position.z-0.1f);
                }
                else if (hans.name.IndexOf("RightLowerLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);

                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.axis.x = 1f;
                    rlh.axis.y = 0f;
                    rlh.axis.z = 0f;
                    rlh.min = -16f;
                    rlh.max = 145f;

                    vik.solvers.rightFoot.bendModifier = IKSolverLimb.BendModifier.Goal;
                    vik.solvers.rightFoot.bendGoal = hans.transform;
                    vik.solvers.rightFoot.bendModifierWeight = 1.0f;

                    //hans.transform.position = new Vector3(pt.position.x, pt.position.y, pt.position.z - (hans.transform.localScale.z * 0.5f));
                    //yield return null;
                    hans.transform.position = new Vector3(pt.position.x*-1f, pt.position.y, pt.position.z - 0.1f);
                }
                else if (hans.name.IndexOf("LeftLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.axis.x = 1f;
                    rlh.axis.y = 0f;
                    rlh.axis.z = 0f;
                    rlh.min = -35f;
                    rlh.max = 60f;

                    /* Vector3 hanspos = pt.position;
                    Vector3 hanssize = pt.localScale;
                    hanspos.y = hanspos.y / 2f;
                    hanspos.z = -(bnd.z / 2f / 10f);
                    hans.transform.position = hanspos;*/
                    //hans.transform.localPosition = new Vector3(pt.position.x, pt.position.y, pt.position.z);
                    //hans.transform.rotation = Quaternion.Euler(0, 180f, 0);

                    vik.solvers.leftFoot.target = hans.transform;
                    vik.solvers.leftFoot.IKPositionWeight = 1.0f;
                    vik.solvers.leftFoot.IKRotationWeight = 1.0f;

                }
                else if (hans.name.IndexOf("RightLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.axis.x = 1f;
                    rlh.axis.y = 0f;
                    rlh.axis.z = 0f;
                    rlh.min = -35f;
                    rlh.max = 60f;

                    /*Vector3 hanspos = pt.position;
                    Vector3 hanssize = pt.localScale;
                    hanspos.y = hanspos.y / 2f;
                    hanspos.z = -(bnd.z / 2f / 10f);
                    hans.transform.position = hanspos;*/
                    //hans.transform.localPosition = new Vector3(pt.position.x, pt.position.y, pt.position.z);
                    //hans.transform.rotation = Quaternion.Euler(0, 180f, 0);


                    vik.solvers.rightFoot.target = hans.transform;
                    vik.solvers.rightFoot.IKPositionWeight = 1.0f;
                    vik.solvers.rightFoot.IKRotationWeight = 1.0f;
                }

                hans.GetComponent<UserHandleOperation>().SaveDefaultTransform();
            }
        }

        /// <summary>
        /// Set up VVMIK
        /// </summary>
        /// <param name="ikhandles"></param>
        /// <param name="animator"></param>
        /// <param name="vik"></param>
        /// <param name="cik"></param>
        /// <param name="bnd"></param>
        void SetupVVMIK(GameObject ikhandles, Animator animator, VvmIk vik, CCDIK cik, Vector3 bnd, bool IsReparent = false)
        {
            Transform[] bts = ikhandles.GetComponentsInChildren<Transform>();

            int cnt = bts.Length; // ikhandles.transform.childCount;
            Vector3 baseReverse = new Vector3(0, 0, 0);
            Vector3 secondReverse = new Vector3(-1, 0, -1);
            if (IsReparent)
            {
                secondReverse.x = 1;
                secondReverse.z = 1;
            }

            for (int i = 0; i < cnt; i++)
            {
                GameObject hans = bts[i].gameObject; // ikhandles.transform.GetChild(i).gameObject;
                if (hans.name.IndexOf("Head") > -1)
                {
                    Transform necktrans = animator.GetBoneTransform(HumanBodyBones.Neck);
                    Transform headtrans = animator.GetBoneTransform(HumanBodyBones.Head);
                    Vector3 newpos;
                    newpos.x = headtrans.position.x;
                    newpos.y = headtrans.position.y + 0.1f; // bnd.y < hanstrans.position.y ? hanstrans.position.y : bnd.y;
                    newpos.z = 0f;
                    hans.transform.position = newpos;


                    vik.Neck = hans.transform;
                    vik.NeckReversed = secondReverse;
                }
                else if (hans.name.IndexOf("LookAt") > -1)
                {
                    Transform hanstrans = animator.GetBoneTransform(HumanBodyBones.Head);
                    if (hanstrans == null) hanstrans = animator.GetBoneTransform(HumanBodyBones.Head);
                    Transform eyetrans = null;
                    if (animator.GetBoneTransform(HumanBodyBones.LeftEye) != null)
                    {
                        eyetrans = animator.GetBoneTransform(HumanBodyBones.LeftEye);
                    }
                    else
                    {
                        eyetrans = animator.GetBoneTransform(HumanBodyBones.Head);
                    }

                    Vector3 newpos;
                    newpos.x = hanstrans.position.x;
                    newpos.y = eyetrans.position.y; // bnd.y < hanstrans.position.y ? hanstrans.position.y : bnd.y;
                    newpos.z = (bnd.z < 0 ? bnd.z : bnd.z * -1f) - 0.2f;
                    hans.transform.position = new Vector3(newpos.x, eyetrans.position.y, newpos.z);

                    vik.lookAtObject = hans.transform;
                    vik.lookAtHeadWeight = 1;
                    vik.lookAtEyeWeight = 0;
                    vik.lookAtClampWeight = 0;
                    vik.lookAtBodyWeight = 0;
                    vik.lookAtWeight = 1;
                }
                else if (hans.name.IndexOf("Aim") > -1)
                {
                    Transform hanstrans = animator.GetBoneTransform(HumanBodyBones.Chest);
                    if (hanstrans == null) hanstrans = animator.GetBoneTransform(HumanBodyBones.Spine);
                    hans.transform.position = new Vector3(hanstrans.position.x, hanstrans.position.y + 0.1f, hanstrans.position.z - 0.1f);
                    //(bnd.z < 0 ? bnd.z : bnd.z * -1f)

                    vik.Spine = hans.transform;
                    vik.SpineReversed = secondReverse;

                    VvmIkConstraint vic = new VvmIkConstraint();
                    vic.BoneTran = hans.transform;
                    vic.LimitFrom.x = 80;
                    vic.LimitTo.x = -30;
                    vic.LimitFrom.y = 50f;
                    vic.LimitTo.y = -50f;
                    vic.LimitFrom.z = 15f;
                    vic.LimitTo.z = -15f;
                    //vik.constraints.Add(vic);
                }
                else if (hans.name.IndexOf("Chest") > -1)
                {
                    Transform hanstrans = animator.GetBoneTransform(HumanBodyBones.Neck);
                    if (hanstrans == null) hanstrans = animator.GetBoneTransform(HumanBodyBones.Spine);
                    hans.transform.position = new Vector3(hanstrans.position.x, hanstrans.position.y, hanstrans.position.z);
                    Transform upperchest = animator.GetBoneTransform(HumanBodyBones.UpperChest);

                    if (upperchest == null)
                    { //---model, don't has Upper Chest
                        vik.Chest = hans.transform;
                        vik.ChestReversed = secondReverse;
                    }
                    else
                    { //---model, has Upper Chest and Chest both.
                        vik.UpperChest = hans.transform;
                        vik.UpperChestReversed = secondReverse;
                    }
                    

                    VvmIkConstraint vic = new VvmIkConstraint();
                    vic.BoneTran = hans.transform;
                    vic.LimitFrom.x = 15f;
                    vic.LimitTo.x = -15f;
                    vic.LimitFrom.y = 50f;
                    vic.LimitTo.y = -50f;
                    vic.LimitFrom.z = 15f;
                    vic.LimitTo.z = -15f;
                    //vik.constraints.Add(vic);
                }
                else if (hans.name.IndexOf("LeftShoulder") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position;
                    hanspos.x /= -2f;
                    hans.transform.position = hanspos;

                    vik.LeftShoulder = hans.transform;
                    vik.LeftShoulderReversed = secondReverse;
                }
                else if (hans.name.IndexOf("RightShoulder") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position;
                    hanspos.x /= -2f;
                    hans.transform.position = hanspos;

                    vik.RightShoulder = hans.transform;
                    vik.RightShoulderReversed = secondReverse;
                }
                else if (hans.name.IndexOf("LeftLowerArm") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).position;
                    hanspos.x *= -1f;
                    hans.transform.position = hanspos;


                    vik.LeftLowerArm = hans.transform;
                    vik.LeftLowerArmWeight = 1f;
                }
                else if (hans.name.IndexOf("RightLowerArm") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.RightLowerArm).position;
                    hanspos.x *= -1f;
                    hans.transform.position = hanspos;

                    vik.RightLowerArm = hans.transform;
                    vik.RightLowerArmWeight = 1f;
                }
                else if (hans.name.IndexOf("LeftHand") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal).position;
                    hanspos.x *= -1f;
                    hanspos.z = 0;
                    hans.transform.position = hanspos;

                    vik.LeftHand = hans.transform;
                    vik.LeftHandPositionWeight = 1f;
                    vik.LeftHandRotationWeight = 1f;
                }
                else if (hans.name.IndexOf("RightHand") > -1)
                {
                    Vector3 hanspos = animator.GetBoneTransform(HumanBodyBones.RightIndexDistal).position;
                    hanspos.x *= -1f;
                    hanspos.z = 0;
                    hans.transform.position = hanspos;

                    vik.RightHand = hans.transform;
                    vik.RightHandPositionWeight = 1f;
                    vik.RightHandRotationWeight = 1f;

                }
                else if (hans.name.IndexOf("Pelvis") > -1)
                {
                    Transform upche = animator.GetBoneTransform(HumanBodyBones.Hips);
                    Transform spine = animator.GetBoneTransform(HumanBodyBones.Spine);
                    Transform chest = animator.GetBoneTransform(HumanBodyBones.Chest);
                    Vector3 newpos = new Vector3();
                    /*if (chest != null)
                    {
                        newpos.y = (chest.position.y + spine.position.y) / 2f;
                    }
                    else*/
                    {
                        newpos.y = upche.position.y + 0.05f;
                        //newpos.y = ((spine.position.y + upche.position.y) * 0.01f) * 95f;
                    }
                    newpos.x = spine.position.x;
                    
                    newpos.z = spine.position.z * -1;
                    hans.transform.position = newpos;

                    vik.waist = hans.transform;
                }
                else if (hans.name.IndexOf("LeftLowerLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                    //hans.transform.position = new Vector3(pt.position.x, pt.position.y, pt.position.z - (hans.transform.localScale.z * 0.5f)) ;
                    hans.transform.localPosition = new Vector3(pt.position.x*-1f, pt.position.y, pt.position.z - 0.05f);
                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.axis.x = 1f;
                    rlh.axis.y = 0f;
                    rlh.axis.z = 0f;
                    rlh.min = -16f;
                    rlh.max = 145f;

                    vik.LeftLowerLeg = hans.transform;
                    vik.LeftLowerLegWeight = 1f;
                }
                else if (hans.name.IndexOf("RightLowerLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                    //hans.transform.position = new Vector3(pt.position.x, pt.position.y, pt.position.z - (hans.transform.localScale.z * 0.5f));
                    hans.transform.localPosition = new Vector3(pt.position.x*-1f, pt.position.y, pt.position.z - 0.05f);
                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.axis.x = 1f;
                    rlh.axis.y = 0f;
                    rlh.axis.z = 0f;
                    rlh.min = -16f;
                    rlh.max = 145f;

                    vik.RightLowerLeg = hans.transform;
                    vik.RightLowerLegWeight = 1f;
                }
                else if (hans.name.IndexOf("LeftLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.axis.x = 1f;
                    rlh.axis.y = 0f;
                    rlh.axis.z = 0f;
                    rlh.min = -35f;
                    rlh.max = 60f;

                    Vector3 hanspos = pt.position;
                    hanspos.x *= -1f;
                    hanspos.y = 0.06f;
                    hanspos.z *= -1f;
                    hans.transform.localPosition = hanspos;


                    vik.LeftFoot = hans.transform;
                    vik.LeftFootPositionWeight = 1f;
                    vik.LeftFootRotationWeight = 1f;

                    VvmIkConstraint vic = new VvmIkConstraint();
                    vic.BoneTran = hans.transform;
                    vic.LimitFrom.x = 70f;
                    vic.LimitTo.x = -30f;
                    vic.LimitFrom.y = 45f;
                    vic.LimitTo.y = -45f;
                    vic.LimitFrom.z = 15f;
                    vic.LimitTo.z = -15f;
                    //vik.constraints.Add(vic);

                }
                else if (hans.name.IndexOf("LeftToes") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.LeftToes);

                    Vector3 hanspos = pt.position;
                    hanspos.x = 0f;
                    hanspos.z = 2f;
                    hans.transform.localPosition = hanspos;
                    vik.LeftToes = hans.transform;
                    vik.LeftToesReversed.x = 0f;
                }
                else if (hans.name.IndexOf("RightLeg") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                    RotationLimitHinge rlh = pt.gameObject.AddComponent<RotationLimitHinge>();
                    rlh.axis.x = 1f;
                    rlh.axis.y = 0f;
                    rlh.axis.z = 0f;
                    rlh.min = -35f;
                    rlh.max = 60f;

                    Vector3 hanspos = pt.position;
                    hanspos.x *= -1f;
                    hanspos.y = 0.06f;
                    hanspos.z *= -1f;
                    hans.transform.localPosition = hanspos;

                    vik.RightFoot = hans.transform;
                    vik.RightFootPositionWeight = 1.0f;
                    vik.RightFootRotationWeight = 1.0f;

                    VvmIkConstraint vic = new VvmIkConstraint();
                    vic.BoneTran = hans.transform;
                    vic.LimitFrom.x = 70f;
                    vic.LimitTo.x = -30f;
                    vic.LimitFrom.y = 45f;
                    vic.LimitTo.y = -45f;
                    vic.LimitFrom.z = 15f;
                    vic.LimitTo.z = -15f;
                    //vik.constraints.Add(vic);
                }
                else if (hans.name.IndexOf("RightToes") > -1)
                {
                    Transform pt = animator.GetBoneTransform(HumanBodyBones.RightToes);

                    Vector3 hanspos = pt.position;
                    hanspos.x = 0f;
                    hanspos.z = 2f;
                    hans.transform.localPosition = hanspos;

                    vik.RightToes = hans.transform;
                    vik.RightToesReversed.x = 0f;
                }
            }
            //animator.gameObject.GetComponent<UserHandleSpace.OperateLoadedVRM>().SetVisibleAvatar(1);
            pendingInstance.ShowMeshes();
        }

        void SetupHand(GameObject obj, Animator animator)
        {
            GameObject vb = GameObject.Find("bkup"); //managa.AvatarArea; //
            GameObject sample = vb.transform.Find("sand_reia").gameObject;

            LeftHandPoseController hpl = obj.AddComponent<LeftHandPoseController>();
            LeftHandPoseController saml = sample.GetComponent<LeftHandPoseController>();
            //hpc.switchLeft = true;
            hpl.targetHandPose = new HandPoseAsset.HandPose();
            hpl.normal = Instantiate((HandPoseAsset)Resources.Load("HandPose_open")); //;(HandPoseAsset)Resources.Load("HandPose_normal")
            /*hpl.pose1 = Instantiate((HandPoseAsset)Resources.Load("HandPose_normal"));  //;(HandPoseAsset)Resources.Load("HandPose_open")
            hpl.pose2 = Instantiate((HandPoseAsset)Resources.Load("HandPose_close")); //;(HandPoseAsset)Resources.Load("HandPose_close")
            hpl.pose3 = Instantiate((HandPoseAsset)Resources.Load("HandPose_indicate")); //sam.pose3;(HandPoseAsset)Resources.Load("HandPose_indicate")
            hpl.pose4 = Instantiate((HandPoseAsset)Resources.Load("HandPose_peace")); //;(HandPoseAsset)Resources.Load("HandPose_peace")
            hpl.pose5 = Instantiate((HandPoseAsset)Resources.Load("HandPose_thumbsup")); //sam.pose5;(HandPoseAsset)Resources.Load("HandPose_thumbsup")
            hpl.pose6 = Instantiate((HandPoseAsset)Resources.Load("HandPose_glob1")); //sam.pose6;(HandPoseAsset)Resources.Load("HandPose_glob1")
            */
            hpl.poses.Add(Instantiate((HandPoseAsset)Resources.Load("HandPose_normal")));
            hpl.poses.Add(Instantiate((HandPoseAsset)Resources.Load("HandPose_close")));
            hpl.poses.Add(Instantiate((HandPoseAsset)Resources.Load("HandPose_indicate")));
            hpl.poses.Add(Instantiate((HandPoseAsset)Resources.Load("HandPose_peace")));
            hpl.poses.Add(Instantiate((HandPoseAsset)Resources.Load("HandPose_thumbsup")));
            hpl.poses.Add(Instantiate((HandPoseAsset)Resources.Load("HandPose_glob1")));

            

            RightHandPoseController samr = sample.GetComponent<RightHandPoseController>();
            RightHandPoseController hpr = obj.AddComponent<RightHandPoseController>();

            hpr.targetHandPose = new HandPoseAsset.HandPose();
            hpr.normal = Instantiate((HandPoseAsset)Resources.Load("HandPose_open")); //;(HandPoseAsset)Resources.Load("HandPose_normal")
            /*
            hpr.pose1 = Instantiate((HandPoseAsset)Resources.Load("HandPose_normal"));  //;(HandPoseAsset)Resources.Load("HandPose_open")
            hpr.pose2 = Instantiate((HandPoseAsset)Resources.Load("HandPose_close")); //;(HandPoseAsset)Resources.Load("HandPose_close")
            hpr.pose3 = Instantiate((HandPoseAsset)Resources.Load("HandPose_indicate")); //sam.pose3;(HandPoseAsset)Resources.Load("HandPose_indicate")
            hpr.pose4 = Instantiate((HandPoseAsset)Resources.Load("HandPose_peace")); //;(HandPoseAsset)Resources.Load("HandPose_peace")
            hpr.pose5 = Instantiate((HandPoseAsset)Resources.Load("HandPose_thumbsup")); //sam.pose5;(HandPoseAsset)Resources.Load("HandPose_thumbsup")
            hpr.pose6 = Instantiate((HandPoseAsset)Resources.Load("HandPose_glob1")); //sam.pose6;(HandPoseAsset)Resources.Load("HandPose_glob1")
            */
            hpr.poses.Add(Instantiate((HandPoseAsset)Resources.Load("HandPose_normal")));
            hpr.poses.Add(Instantiate((HandPoseAsset)Resources.Load("HandPose_close")));
            hpr.poses.Add(Instantiate((HandPoseAsset)Resources.Load("HandPose_indicate")));
            hpr.poses.Add(Instantiate((HandPoseAsset)Resources.Load("HandPose_peace")));
            hpr.poses.Add(Instantiate((HandPoseAsset)Resources.Load("HandPose_thumbsup")));
            hpr.poses.Add(Instantiate((HandPoseAsset)Resources.Load("HandPose_glob1")));

            //hpl.enabled = false;
            //hpr.enabled = false;
        }

    }

}