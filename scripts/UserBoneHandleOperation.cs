using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UserHandleSpace
{
    public class UserBoneHandleOperation : MonoBehaviour
    {
        private Vector3 oldPosition;
        private Quaternion oldRotation;
        public Vector3 defaultPosition;
        public Quaternion defaultRotation;
        public bool IsPosition = false;
        public bool IsRotation = true;

        public Transform TargetBone;


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (TargetBone != null)
            {
                if ((IsPosition) && (TargetBone.localPosition != oldPosition))
                {
                    TargetBone.localPosition = transform.localPosition;
                }
                if ((IsRotation) && (TargetBone.localRotation != oldRotation))
                {
                    TargetBone.localRotation = transform.localRotation;
                }

                oldPosition = TargetBone.localPosition;
                oldRotation = transform.localRotation;
            }
        }
        public void SaveDefaultTransform()
        {
            defaultPosition = new Vector3(TargetBone.localPosition.x, TargetBone.localPosition.y, TargetBone.localPosition.z);
            defaultRotation = new Quaternion(TargetBone.localRotation.x, TargetBone.localRotation.y, TargetBone.localRotation.z, TargetBone.localRotation.w);
        }
        public void LoadDefaultTransform(bool ismove, bool isrotate)
        {
            if (ismove) TargetBone.localPosition = defaultPosition;
            if (isrotate) TargetBone.localRotation = defaultRotation;
        }
    }

}
