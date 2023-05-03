using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
using RootMotion.FinalIK;


namespace UserHandleSpace
{
    public class UserVRM10IK : MonoBehaviour
    {
        public enum AsistType
        {
            EyeViewHandle,
            LookAt,
            Aim
        }
        /// <summary>
        ///  For VRM 1.0, BipedIK: IK marker of effective transform 
        /// </summary>
        public Transform relatedOriginalIK;

        /// <summary>
        /// IK marker name of effective transform
        /// </summary>
        public string originalIKName;

        /// <summary>
        /// Type of related IK marker 
        /// </summary>
        public AsistType asistType;

        /*
         * Relation of this object and IK marker
         * <VRM>
         *  |
         * <IK>
         *    <EyeViewHandle> <LookAt> <Aim> is hidden IK marker for effective transform
         *          |            |       |
         *    Showable IK marker by operating an User
         */
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

        }

        // Update is called once per frame
        void Update()
        {
            if (relatedOriginalIK != null)
            {
                if (oldPosition != this.transform.localPosition)
                {
                    Vector3 diffpos = oldPosition - this.transform.localPosition;
                    float dy = oldPosition.y - this.transform.localPosition.y;

                    Vector3 pos = Vector3.zero;
                    if (asistType == AsistType.EyeViewHandle)
                    {
                        pos.x = this.transform.localPosition.x * -1;
                        pos.y = this.transform.localPosition.y;
                        pos.z = this.transform.localPosition.z * -1;
                    }
                    else if (asistType == AsistType.LookAt)
                    {
                        pos.x = this.transform.localPosition.x * -1;
                        if (dy > 0)
                        {
                            pos.y = (relatedOriginalIK.localPosition.y + dy);
                        }
                        else if (dy < 0)
                        {
                            pos.y = (relatedOriginalIK.localPosition.y + dy);
                        }
                        else
                        {
                            pos.y = relatedOriginalIK.localPosition.y;
                        }
                        pos.z = this.transform.localPosition.z * -1;
                    }
                    else if (asistType == AsistType.Aim)
                    {
                        pos.x = this.transform.localPosition.x * -1;
                        if (dy > 0)
                        {
                            pos.y = relatedOriginalIK.localPosition.y + dy;
                        }
                        else if (dy < 0)
                        {
                            pos.y = relatedOriginalIK.localPosition.y + dy;
                        }
                        else
                        {
                            pos.y = relatedOriginalIK.localPosition.y;
                        }
                        
                        pos.z = this.transform.localPosition.z * -1;
                    }

                    relatedOriginalIK.localPosition = pos;
                    relatedOriginalIK.localRotation = this.transform.localRotation;
                }
                
                oldPosition = this.transform.localPosition;
                oldRotation = this.transform.localRotation;
            }
        }
        public void SetOldPotion(Vector3 pos)
        {
            oldPosition = pos;
        }
    }

}
