using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UniVRM10;
using DG.Tweening;
using RootMotion.FinalIK;


namespace UserHandleSpace
{
    [Serializable]
    public class UserBvhMotionList
    {
        public int frames = 0;
        public float frameTime = 0.0f;
        public List<List<float>> motionList = new List<List<float>>();
    }
    
    public class UserBvhJsonJoint
    {
        public Vector3 offset = Vector3.zero;
        public string[] channels = new string[] { "3", "Zrotation", "Xrotation", "Yrotation" };
        //{ "6", "Xposition", "Yposition", "Zposition", "Zrotation", "Xrotation", "Yrotation" };
        public string joint = "";
        public bool endsite = false;
        public List<UserBvhJsonJoint> children = new List<UserBvhJsonJoint>();
    }
    [Serializable]
    public class UserBvhJson : UserBvhJsonJoint
    {
        public bool hierarchy = true;
        public string root = "joint_Root";
        //public List<UserBvhJsonJoint> children = new List<UserBvhJsonJoint>();
    }
    [Serializable]
    public class UserMotionCsvManager
    {
        public List<string> bonenames = new List<string>();
        public List<List<Vector3>> rotatevalue = new List<List<Vector3>>();
    }

    public class UserBVHRecorder : BVHRecorder
    {
        public UserBVHRecorder()
        {
            
        }
        
    }

    //===============================================================================================================================
    public partial class ManageAvatarTransform 
    {
        public UserBvhJson savedBvhJoint;
        public UserBvhMotionList savedBvhMotion;
        public UserMotionCsvManager savedCsvMotion;
        public BVHRecorder recbvh;
        BipedIK BaseBik;


        /// <summary>
        /// Set up a bone information for Bvh-format when initial T-pose
        /// </summary>
        public void GenerateBvhBoneInformation(Animator anim)
        {
            if( recbvh == null)
            {
                gameObject.AddComponent<BVHRecorder>();
            }
            recbvh = GetComponent<BVHRecorder>();
            recbvh.targetAvatar = anim;
            recbvh.scripted = true;
            recbvh.enforceHumanoidBones = true;
            recbvh.blender = true;
            recbvh.renameBones = true;
            recbvh.getBones();
            if (recbvh.bones.Count == 0)
            {
                foreach (HumanBodyBones value in Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if ((value != HumanBodyBones.Jaw) && (value != HumanBodyBones.LastBone))
                    {
                        Transform bonetran = anim.GetBoneTransform(value);
                        if (bonetran != null)
                        {
                            recbvh.bones.Add(bonetran);
                        }
                    }


                }
            }
            
            recbvh.buildSkeleton();
            recbvh.genHierarchy();
        }
        public void StartRecordBVH()
        {
            recbvh.clearCapture();
            recbvh.capturing = true;
        }
        public void EndRecordBVH()
        {
            recbvh.capturing = false;
            
        }
        public void ExportRecordedBVH()
        {
            string ret = recbvh.genBVH();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        

        /// <summary>
        /// Re-generate Joint and Motion data for BVH-format
        /// </summary>
        /// <param name="curproj">current animation project data</param>
        public void RegenerateBVH(NativeAnimationProject curproj)
        {
            BaseBik = gameObject.GetComponent<BipedIK>();

            if (savedBvhJoint == null)
            {
                savedBvhJoint = new UserBvhJson();
                savedBvhJoint.channels = new string[] { "6", "Xposition", "Yposition", "Zposition", "Zrotation", "Xrotation", "Yrotation" };
            }
            else
            {
                savedBvhJoint.children.Clear();
            }
            if (savedBvhMotion == null)
            {
                savedBvhMotion = new UserBvhMotionList();
            }
            else
            {
                savedBvhMotion.frames = 0;
                savedBvhMotion.frameTime = 0.0f;
                savedBvhMotion.motionList.Clear();
            }
            savedBvhMotion.frames = curproj.timelineFrameLength;
            savedBvhMotion.frameTime = curproj.baseDuration;

            if (savedCsvMotion == null)
            {
                savedCsvMotion = new UserMotionCsvManager();
            }
            else
            {
                savedCsvMotion.rotatevalue.Clear();
            }
        }
        /// <summary>
        /// Generate joint data for BVH-format (call once)
        /// </summary>
        public void ExportForBVH()
        {
            /*
            string[] CNSBONES = {"Hips","Spine","Chest","UpperChest","Neck","Head",
                "LeftShoulder", "LeftUpperArm", "LeftLowerArm", "LeftHand",
                "RightShoulder","RightUpperArm","RightLowerArm","RightHand",
                "LeftUpperLeg","LeftLowerLeg","LeftFoot","LeftToes",
                "RightUpperLeg","RightLowerLeg","RightFoot","RightToes"
            };
            Transform roottran = BaseBik.references.pelvis;
            Animator anim = gameObject.GetComponent<Animator>();

            //---old----
            int childcnt = roottran.childCount;
            for (int i = 0; i < childcnt; i++)
            {
                UserBvhJsonJoint cjoi = enumerateJointForBVH(roottran.GetChild(i), roottran.gameObject.name);

                savedBvhJoint.children.Add(cjoi);
            }

            */

            //---new---
            //UserBvhJsonJoint bvhpelvis = GetJointForBVH(BaseBik.references.pelvis);

            UserBvhJsonJoint bvhspine = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.Spine));

            UserBvhJsonJoint bvhchest = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.Chest));

            UserBvhJsonJoint bvhupperchest = null;
            if (animator.GetBoneTransform(HumanBodyBones.UpperChest) != null)
            {
                bvhupperchest = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.UpperChest));
            }

            UserBvhJsonJoint bvhneck = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.Neck));
            UserBvhJsonJoint bvhhead = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.Head));

            UserBvhJsonJoint bvhLeftshoulder = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.LeftShoulder));
            UserBvhJsonJoint bvhLeftupperarm = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm));
            UserBvhJsonJoint bvhLeftlowerarm = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm));
            UserBvhJsonJoint bvhLefthand = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.LeftHand));

            UserBvhJsonJoint bvhRightshoulder = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.RightShoulder));
            UserBvhJsonJoint bvhRightupperarm = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.RightUpperArm));
            UserBvhJsonJoint bvhRightlowerarm = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.RightLowerArm));
            UserBvhJsonJoint bvhRighthand = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.RightHand));

            UserBvhJsonJoint bvhLeftupperleg = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg));
            UserBvhJsonJoint bvhLeftlowerleg = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg));
            UserBvhJsonJoint bvhLeftfoot = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.LeftFoot));
            UserBvhJsonJoint bvhLefttoes = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.LeftToes));

            UserBvhJsonJoint bvhRightupperleg = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.RightUpperLeg));
            UserBvhJsonJoint bvhRightlowerleg = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg));
            UserBvhJsonJoint bvhRightfoot = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.RightFoot));
            UserBvhJsonJoint bvhRighttoes = GetJointForBVH(animator.GetBoneTransform(HumanBodyBones.RightToes));

            bvhRighttoes.endsite = true;
            bvhRightfoot.children.Add(bvhRighttoes);
            bvhRightlowerleg.children.Add(bvhRightfoot);
            bvhRightupperleg.children.Add(bvhRightlowerleg);

            bvhLefttoes.endsite = true;
            bvhLeftfoot.children.Add(bvhLefttoes);
            bvhLeftlowerleg.children.Add(bvhLeftfoot);
            bvhLeftupperleg.children.Add(bvhLeftlowerleg);

            bvhRighthand.endsite = true;
            bvhRightlowerarm.children.Add(bvhRighthand);
            bvhRightupperarm.children.Add(bvhRightlowerarm);
            bvhRightshoulder.children.Add(bvhRightupperarm);

            bvhLefthand.endsite = true;
            bvhLeftlowerarm.children.Add(bvhLefthand);
            bvhLeftupperarm.children.Add(bvhLeftlowerarm);
            bvhLeftshoulder.children.Add(bvhLeftupperarm);

            bvhhead.endsite = true;
            bvhneck.children.Add(bvhhead);

            if (bvhupperchest != null)
            {
                bvhupperchest.children.Add(bvhneck);
                bvhupperchest.children.Add(bvhLeftshoulder);
                bvhupperchest.children.Add(bvhRightshoulder);

                bvhchest.children.Add(bvhupperchest);
            }
            else
            {
                bvhchest.children.Add(bvhneck);
                bvhchest.children.Add(bvhLeftshoulder);
                bvhchest.children.Add(bvhRightshoulder);
            }

            bvhspine.children.Add(bvhchest);

            //add to root(hips)
            savedBvhJoint.children.Add(bvhspine);
            savedBvhJoint.children.Add(bvhLeftupperleg);
            savedBvhJoint.children.Add(bvhRightupperleg);

        }

        /// <summary>
        /// Generation motion data for BVH-format (call each every frame)
        /// </summary>
        public void ExportMotionForBVH()
        {
            /*
            Transform roottran = BaseBik.references.pelvis;

            List<float> posrotList = new List<float>();

            Vector3 rot = roottran.localRotation.eulerAngles;

            posrotList.Add(roottran.localPosition.x);
            posrotList.Add(roottran.localPosition.y);
            posrotList.Add(roottran.localPosition.z);
            posrotList.Add(rot.z);
            posrotList.Add(rot.x);
            posrotList.Add(rot.y);

            //---old---
            int childcnt = roottran.childCount;
            for (int i = 0; i < childcnt; i++)
            {
                enumerateValueForBVH(roottran.GetChild(i), posrotList);

            }
            savedBvhMotion.motionList.Add(posrotList);
            */

            //---new---
            List<Vector3> motList = new List<Vector3>();
            Animator anim = gameObject.GetComponent<Animator>();

            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.Hips)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.Spine)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.Chest)));
            if (animator.GetBoneTransform(HumanBodyBones.UpperChest) != null)
                motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.UpperChest)));

            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.Neck)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.Head)));

            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.LeftShoulder)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.LeftUpperArm)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.LeftLowerArm)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.LeftHand)));

            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.RightShoulder)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.RightUpperArm)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.RightLowerArm)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.RightHand)));

            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.LeftFoot)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.LeftToes)));

            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.RightUpperLeg)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.RightFoot)));
            motList.Add(GetMotionValueForCsv(animator.GetBoneTransform(HumanBodyBones.RightToes)));

            savedCsvMotion.rotatevalue.Add(motList);
        }

        /// <summary>
        /// retrieve joint data for BVH-format
        /// </summary>
        /// <param name="bone"></param>
        /// <returns></returns>
        private UserBvhJsonJoint enumerateJointForBVH(Transform bone, string parentName)
        {
            UserBvhJsonJoint joint = new UserBvhJsonJoint();
            string[] names = bone.gameObject.name.Split(":");

            joint.joint = names[names.Length - 1];

            if (
                (bone.childCount == 0) 
            )
            {
                joint.endsite = true;
            }
            else
            {
                if (
                    (parentName == "LeftHand") ||
                    (parentName == "RightHand") ||
                    (parentName == "Head")
                )
                {
                    joint.endsite = true;
                }
                else
                {
                    for (int i = 0; i < bone.childCount; i++)
                    {
                        UserBvhJsonJoint cjoi = enumerateJointForBVH(bone.GetChild(i), joint.joint);

                        joint.children.Add(cjoi);
                        //---end site is one. 
                        if (cjoi.endsite) break;
                    }
                }
                
            }

            return joint;
        }
        private UserBvhJsonJoint GetJointForBVH(Transform bone)
        {
            UserBvhJsonJoint joint = new UserBvhJsonJoint();

            string[] names = bone.gameObject.name.Split(":");

            joint.joint = names[names.Length - 1];

            return joint;
        }

        /// <summary>
        /// retrieve motion data for BVH-format
        /// </summary>
        /// <param name="bone"></param>
        /// <param name="values"></param>
        private void enumerateValueForBVH(Transform bone, List<float> values)
        {
            string[] names = bone.gameObject.name.Split(":");
            string bonename = names[names.Length - 1];

            Vector3 rot = bone.localRotation.eulerAngles;

            values.Add(bone.localPosition.x);
            values.Add(bone.localPosition.y);
            values.Add(bone.localPosition.z);
            values.Add(rot.z);
            values.Add(rot.x);
            values.Add(rot.y);

            if (bone.childCount > 0)
            {
                if (
                    (bonename != "LeftHand") && (bonename != "RightHand") && (bonename != "Head")
                ) {
                    for (int i = 0; i < bone.childCount; i++)
                    {
                        enumerateValueForBVH(bone.GetChild(i), values);

                    }
                }
                
            }
        }
        private Vector3 GetMotionValueForCsv(Transform bone)
        {
            //Vector3 ret = Vector3.zero;

            Vector3 rot = bone.localRotation.eulerAngles;

            //string frm = (rot.z.ToString() + " " + rot.x.ToString() + " " + rot.y.ToString());

            return bone.localRotation.eulerAngles;
        }

        /// <summary>
        /// Convert BVH internal class to text-format
        /// </summary>
        /// <returns></returns>
        public string TextOutputBVH()
        {
            string ret = "";
            List<string> txtarr = new List<string>();
            txtarr.Add("HIERARCHY");
            txtarr.Add("ROOT " + savedBvhJoint.root);
            txtarr.Add("{");
            int indentation = 2;
            txtarr.Add("  OFFSET " + savedBvhJoint.offset.x.ToString() + " " + savedBvhJoint.offset.y.ToString() + " " + savedBvhJoint.offset.z.ToString());
            txtarr.Add("  CHANNELS " + String.Join(' ', savedBvhJoint.channels));
            foreach (UserBvhJsonJoint joi in savedBvhJoint.children)
            {
                List<string> cret = enumerateTextOutputJoint(joi, indentation);
                txtarr.AddRange(cret);
            }
            txtarr.Add("}");
            txtarr.Add("MOTION");
            txtarr.Add("Frames: " + savedBvhMotion.frames.ToString());
            txtarr.Add("Frame Time: " + savedBvhMotion.frameTime.ToString());
            /*foreach (List<float> lst in savedBvhMotion.motionList)
            {
                txtarr.Add(String.Join(' ', lst));
            }*/
            foreach (List<Vector3> lst in savedCsvMotion.rotatevalue)
            {
                List<string> lstarr = new List<string>();
                for (int i = 0; i < lst.Count; i++)
                {
                    lstarr.Add(lst[i].z.ToString() + " " + lst[i].x.ToString() + " " + lst[i].y.ToString());
                }
                txtarr.Add(String.Join(' ',lstarr));
            }

            ret = String.Join('\n', txtarr);

            return ret;
        }
        private List<string> enumerateTextOutputJoint(UserBvhJsonJoint joi, int indentation)
        {
            List<string> ret = new List<string>();
            int subindentation = indentation + 2;
            string sp = new string(' ', subindentation);


            if (joi.endsite)
            {
                ret.Add(sp + "End Site");
            }
            else
            {
                ret.Add(sp + "JOINT " + joi.joint);
            }
            ret.Add(sp + "{");
            ret.Add(sp + "  OFFSET " + joi.offset.x.ToString() + " " + joi.offset.y.ToString() + " " + joi.offset.z.ToString());
            if (!joi.endsite) ret.Add(sp + "  CHANNELS " + String.Join(' ', joi.channels));
                
            if (joi.children.Count > 0)
            {
                    
                for (int i = 0; i < joi.children.Count; i++)
                {
                    List<string> cret = enumerateTextOutputJoint(joi.children[i], subindentation);
                    foreach (string ln in cret)
                    {
                        ret.Add(ln);
                    }
                        
                }
                    
            }
            ret.Add(sp + "}");
                

            return ret;
        }

        //===================================================================================================
        public void ExportMotionToCsv(int frameIndex, float duration)
        {
            List<string> values = new List<string>();
            values.Add(frameIndex.ToString());
            values.Add(duration.ToString());

            Animator anim = GetComponent<Animator>();
            for (int i = 0; i <  (int)HumanBodyBones.LastBone; i++)
            {
                Transform bonetran = anim.GetBoneTransform((HumanBodyBones)i);
                if (bonetran != null)
                {
                    Vector3 rot = bonetran.localRotation.eulerAngles;

                    values.Add(String.Format("%s %s %s", rot.x, rot.y, rot.z));
                }
            }
            
        }
    }
    

}
