using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "HandPose_", menuName = "Create HandPose Asset")]
public class HandPoseAsset : ScriptableObject
{
    [System.Serializable]
    public class FingerPoseThumb
    {
        [Range(-2f, 2f), Tooltip("指開き")] public float spread;
        [Range(-1f, 1f), Tooltip("指根本捻り")] public float roll;
        [Range(-2f, 2f), Tooltip("指1曲げ")] public float stretched1;
        [Range(-2f, 2f), Tooltip("指2曲げ")] public float stretched2;
        [Range(-3f, 3f), Tooltip("指3曲げ")] public float stretched3;
        [Range(-1f, 1f), Tooltip("指1捻り")] public float roll1;
        [Range(-1f, 1f), Tooltip("指2捻り")] public float roll2;
        [Range(-1f, 1f), Tooltip("指3捻り")] public float roll3;

        public void Lerp(FingerPoseThumb a, FingerPoseThumb b, float t)
        {
            spread = Mathf.Lerp(a.spread, b.spread, t);
            roll = Mathf.Lerp(a.roll, b.roll, t);
            stretched1 = Mathf.Lerp(a.stretched1, b.stretched1, t);
            stretched2 = Mathf.Lerp(a.stretched2, b.stretched2, t);
            stretched3 = Mathf.Lerp(a.stretched3, b.stretched3, t);
            roll1 = Mathf.Lerp(a.roll1, b.roll1, t);
            roll2 = Mathf.Lerp(a.roll2, b.roll2, t);
            roll3 = Mathf.Lerp(a.roll3, b.roll3, t);
        }
    }

    [System.Serializable]
    public class FingerPose
    {
        [Range(-2f, 2f), Tooltip("指開き")] public float spread;
        [Range(-1f, 1f), Tooltip("指捻り")] public float roll;
        [Range(-2f, 2f), Tooltip("指1曲げ")] public float stretched1;
        [Range(-2f, 2f), Tooltip("指2曲げ")] public float stretched2;
        [Range(-2f, 2f), Tooltip("指3曲げ")] public float stretched3;

        public void Lerp(FingerPose a, FingerPose b, float t)
        {
            spread = Mathf.Lerp(a.spread, b.spread, t);
            roll = Mathf.Lerp(a.roll, b.roll, t);
            stretched1 = Mathf.Lerp(a.stretched1, b.stretched1, t);
            stretched2 = Mathf.Lerp(a.stretched2, b.stretched2, t);
            stretched3 = Mathf.Lerp(a.stretched3, b.stretched3, t);
        }
    }

    [System.Serializable]
    public class HandPose
    {
        public FingerPoseThumb thumb;
        public FingerPose index;
        public FingerPose middle;
        public FingerPose ring;
        public FingerPose little;

        public void Lerp(HandPose a, HandPose b, float t)
        {
            thumb.Lerp(a.thumb, b.thumb, t);
            index.Lerp(a.index, b.index, t);
            middle.Lerp(a.middle, b.middle, t);
            ring.Lerp(a.ring, b.ring, t);
            little.Lerp(a.little, b.little, t);
        }
    }

    public HandPose handPose;
}
