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
}
