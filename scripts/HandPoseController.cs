using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class HandPoseController : MonoBehaviour
{
    protected Animator animator;
    protected HumanPose humanPose;
    protected HumanPoseHandler poseHandler;

    protected bool switchLeft;
    public HandPoseAsset normal;
    public List<HandPoseAsset> poses;

    public int currentPose;
    [Range(0f, 1f)] public float handPoseValue;
    
    public HandPoseAsset.HandPose targetHandPose;
    public bool DisableFingerRoll;

    protected int[] fingerMuscleIndex = { 55, 59, 63, 67, 71 };    // 指のMuscle配列の先頭
    protected HumanBodyBones[] finglerBoneL =
    {
        HumanBodyBones.LeftThumbProximal,
        HumanBodyBones.LeftIndexProximal,
        HumanBodyBones.LeftMiddleProximal,
        HumanBodyBones.LeftRingProximal,
        HumanBodyBones.LeftLittleProximal,

        HumanBodyBones.LeftHand,
    };
    protected HumanBodyBones[] finglerBoneR =
    {
        HumanBodyBones.RightThumbProximal,
        HumanBodyBones.RightIndexProximal,
        HumanBodyBones.RightMiddleProximal,
        HumanBodyBones.RightRingProximal,
        HumanBodyBones.RightLittleProximal,

        HumanBodyBones.RightHand,
    };

    virtual protected void Awake()
    {
        poses = new List<HandPoseAsset>();
    }

    virtual protected void Start()
    {
        animator = GetComponent<Animator>();
        poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
        //switchLeft = true;

        targetHandPose = new HandPoseAsset.HandPose();
        targetHandPose.thumb = new HandPoseAsset.FingerPoseThumb();
        targetHandPose.index = new HandPoseAsset.FingerPose();
        targetHandPose.middle = new HandPoseAsset.FingerPose();
        targetHandPose.ring = new HandPoseAsset.FingerPose();
        targetHandPose.little = new HandPoseAsset.FingerPose();
     
    }

    virtual protected void Update()
    {
        
        if (poses == null) return;

        if ((0 <= currentPose) && (currentPose < poses.Count))
        {
            targetHandPose.Lerp(normal.handPose, poses[currentPose].handPose, handPoseValue);
        }
        /*else
        {
            targetHandPose.Lerp(normal.handPose, normal.handPose, 0f);
        }*/
        

    }

    virtual protected void LateUpdate()
    {
        if (targetHandPose == null) return;
        if (poseHandler == null) return;

        // Humanoid.Muscle値を書き換えて指ポーズをセットしたい
        poseHandler.GetHumanPose(ref humanPose);
        //overwriteHandPose_muscle(isLeft: true);
        //overwriteHandPose_muscle(isLeft: false);
        overwriteHandPose_muscle(switchLeft);
        poseHandler.SetHumanPose(ref humanPose);

        // Muscle処理の後に指をロールさせる
        //overwriteHandPose_roll(isLeft: true);
        //overwriteHandPose_roll(isLeft: false);
        overwriteHandPose_roll(switchLeft);
    }

    // 指ポーズをセット(muscle)
    protected void overwriteHandPose_muscle(bool isLeft)
    {
        setFingerMuscle(isLeft, fingerMuscleIndex[0], targetHandPose.thumb.spread, targetHandPose.thumb.stretched1, targetHandPose.thumb.stretched2, targetHandPose.thumb.stretched3);
        setFingerMuscle(isLeft, fingerMuscleIndex[1], targetHandPose.index.spread, targetHandPose.index.stretched1, targetHandPose.index.stretched2, targetHandPose.index.stretched3);
        setFingerMuscle(isLeft, fingerMuscleIndex[2], targetHandPose.middle.spread, targetHandPose.middle.stretched1, targetHandPose.middle.stretched2, targetHandPose.middle.stretched3);
        setFingerMuscle(isLeft, fingerMuscleIndex[3], targetHandPose.ring.spread, targetHandPose.ring.stretched1, targetHandPose.ring.stretched2, targetHandPose.ring.stretched3);
        setFingerMuscle(isLeft, fingerMuscleIndex[4], targetHandPose.little.spread, targetHandPose.little.stretched1, targetHandPose.little.stretched2, targetHandPose.little.stretched3);
    }

    // 指ポーズをセット(roll)
    protected void overwriteHandPose_roll(bool isLeft)
    {
        if (DisableFingerRoll == true) return;

        // 人差し指から小指のロール処理
        HumanBodyBones[] finglerBone = isLeft ? finglerBoneL : finglerBoneR;
        setFingerRoll(isLeft, finglerBone[1], finglerBone[1] + 1, targetHandPose.index.roll);
        setFingerRoll(isLeft, finglerBone[2], finglerBone[2] + 1, targetHandPose.middle.roll);
        setFingerRoll(isLeft, finglerBone[3], finglerBone[3] + 1, targetHandPose.ring.roll);
        setFingerRoll(isLeft, finglerBone[4], finglerBone[4] + 1, targetHandPose.little.roll);

        // 親指ロール処理
        var thumb3 = animator.GetBoneTransform(finglerBone[0] + 2);
        var thumb3_tip = (thumb3 != null && thumb3.childCount > 0) ? thumb3.GetChild(0) : null;

        setFingerRoll(isLeft, finglerBone[5], finglerBone[1], targetHandPose.thumb.roll, overwriteRollBone: finglerBone[0]); // 手首 ⇒ 人差し指への軸で回転
        setFingerRoll(isLeft, finglerBone[0], finglerBone[0] + 1, targetHandPose.thumb.roll1);
        setFingerRoll(isLeft, finglerBone[0] + 1, finglerBone[0] + 2, targetHandPose.thumb.roll2);
        setFingerRoll(isLeft, finglerBone[0] + 2, default, targetHandPose.thumb.roll3, overwriteTarget: thumb3_tip);
    }

    // Humanoid.Muscle値書き換え
    protected void setFingerMuscle(bool isLeft, int index, float spread, float stretched1, float stretched2, float stretched3)
    {
        if (isLeft == false) index += 20;    // Left ⇒ Right
        humanPose.muscles[index++] = stretched1;
        humanPose.muscles[index++] = spread;
        humanPose.muscles[index++] = stretched2;
        humanPose.muscles[index++] = stretched3;
    }

    // 指ロール処理
    protected void setFingerRoll(bool isLeft, HumanBodyBones bone, HumanBodyBones target, float roll, Transform overwriteTarget = null, HumanBodyBones overwriteRollBone = default)
    {
        if (target == default && overwriteTarget == null) return;
        var boneT = animator.GetBoneTransform((overwriteRollBone == default) ? bone : overwriteRollBone);
        var targetT = (overwriteTarget == null) ? animator.GetBoneTransform(target) : overwriteTarget;
        float sign = isLeft ? -1f : 1f;

        if (boneT != null && targetT != null) boneT.Rotate(boneT.position - targetT.position, roll * 90f * sign, Space.World);
    }

    public void ResetPose()
    {
        handPoseValue = 0f;
    }
    public void ResetManualPose()
    {
        targetHandPose.Lerp(normal.handPose, normal.handPose, 0f);
    }
    public void SetPose(int pos, float val)
    {
        currentPose = pos;
        handPoseValue = val;        
    }
    public UserHandleSpace.AvatarFingerForHPC BackupFinger()
    {
        UserHandleSpace.AvatarFingerForHPC ret = new UserHandleSpace.AvatarFingerForHPC();
        {
            ret.Thumbs.Add(targetHandPose.thumb.spread);
            ret.Thumbs.Add(targetHandPose.thumb.roll);
            ret.Thumbs.Add(targetHandPose.thumb.stretched1);
            ret.Thumbs.Add(targetHandPose.thumb.stretched2);
            ret.Thumbs.Add(targetHandPose.thumb.stretched3);
            ret.Thumbs.Add(targetHandPose.thumb.roll1);
            ret.Thumbs.Add(targetHandPose.thumb.roll2);
            ret.Thumbs.Add(targetHandPose.thumb.roll3);
        }
        {
            ret.Index.Add(targetHandPose.index.spread);
            ret.Index.Add(targetHandPose.index.roll);
            ret.Index.Add(targetHandPose.index.stretched1);
            ret.Index.Add(targetHandPose.index.stretched2);
            ret.Index.Add(targetHandPose.index.stretched3);
        }
        {
            ret.Middle.Add(targetHandPose.middle.spread);
            ret.Middle.Add(targetHandPose.middle.roll);
            ret.Middle.Add(targetHandPose.middle.stretched1);
            ret.Middle.Add(targetHandPose.middle.stretched2);
            ret.Middle.Add(targetHandPose.middle.stretched3);
        }
        {
            ret.Ring.Add(targetHandPose.ring.spread);
            ret.Ring.Add(targetHandPose.ring.roll);
            ret.Ring.Add(targetHandPose.ring.stretched1);
            ret.Ring.Add(targetHandPose.ring.stretched2);
            ret.Ring.Add(targetHandPose.ring.stretched3);
        }
        {
            ret.Little.Add(targetHandPose.little.spread);
            ret.Little.Add(targetHandPose.little.roll);
            ret.Little.Add(targetHandPose.little.stretched1);
            ret.Little.Add(targetHandPose.little.stretched2);
            ret.Little.Add(targetHandPose.little.stretched3);
        }

        return ret;
    }
    public void PoseFinger(string finger, int count, float[] values)
    {
        if (finger == "t")
        {
            targetHandPose.thumb.spread = values[0];
            targetHandPose.thumb.roll = values[1];
            targetHandPose.thumb.stretched1 = values[2];
            targetHandPose.thumb.stretched2 = values[3];
            targetHandPose.thumb.stretched3 = values[4];
            targetHandPose.thumb.roll1 = values[5];
            targetHandPose.thumb.roll2 = values[6];
            targetHandPose.thumb.roll3 = values[7];
        }
        else if (finger == "i")
        {
            targetHandPose.index.spread = values[0];
            targetHandPose.index.roll = values[1];
            targetHandPose.index.stretched1 = values[2];
            targetHandPose.index.stretched2 = values[3];
            targetHandPose.index.stretched3 = values[4];
        }
        else if (finger == "m")
        {
            targetHandPose.middle.spread = values[0];
            targetHandPose.middle.roll = values[1];
            targetHandPose.middle.stretched1 = values[2];
            targetHandPose.middle.stretched2 = values[3];
            targetHandPose.middle.stretched3 = values[4];
        }
        else if (finger == "r")
        {
            targetHandPose.ring.spread = values[0];
            targetHandPose.ring.roll = values[1];
            targetHandPose.ring.stretched1 = values[2];
            targetHandPose.ring.stretched2 = values[3];
            targetHandPose.ring.stretched3 = values[4];
        }
        else if (finger == "l")
        {
            targetHandPose.little.spread = values[0];
            targetHandPose.little.roll = values[1];
            targetHandPose.little.stretched1 = values[2];
            targetHandPose.little.stretched2 = values[3];
            targetHandPose.little.stretched3 = values[4];
        }
    }
    public Sequence AnimationFinger(string finger, UserHandleSpace.AvatarFingerForHPC fingerCls, float duration, Sequence seq)
    {
        if (finger == "t")
        {
            seq.Join(DOTween.To(() => targetHandPose.thumb.spread, x => targetHandPose.thumb.spread = x, fingerCls.Thumbs[0], duration));
            seq.Join(DOTween.To(() => targetHandPose.thumb.roll, x => targetHandPose.thumb.roll = x, fingerCls.Thumbs[1], duration));
            seq.Join(DOTween.To(() => targetHandPose.thumb.stretched1, x => targetHandPose.thumb.stretched1 = x, fingerCls.Thumbs[2], duration));
            seq.Join(DOTween.To(() => targetHandPose.thumb.stretched2, x => targetHandPose.thumb.stretched2 = x, fingerCls.Thumbs[3], duration));
            seq.Join(DOTween.To(() => targetHandPose.thumb.stretched3, x => targetHandPose.thumb.stretched3 = x, fingerCls.Thumbs[4], duration));
            seq.Join(DOTween.To(() => targetHandPose.thumb.roll1, x => targetHandPose.thumb.roll1 = x, fingerCls.Thumbs[5], duration));
            seq.Join(DOTween.To(() => targetHandPose.thumb.roll2, x => targetHandPose.thumb.roll2 = x, fingerCls.Thumbs[6], duration));
            seq.Join(DOTween.To(() => targetHandPose.thumb.roll3, x => targetHandPose.thumb.roll3 = x, fingerCls.Thumbs[7], duration));
        }
        else if (finger == "i")
        {
            seq.Join(DOTween.To(() => targetHandPose.index.spread, x => targetHandPose.index.spread = x, fingerCls.Index[0], duration));
            seq.Join(DOTween.To(() => targetHandPose.index.roll, x => targetHandPose.index.roll = x, fingerCls.Index[1], duration));
            seq.Join(DOTween.To(() => targetHandPose.index.stretched1, x => targetHandPose.index.stretched1 = x, fingerCls.Index[2], duration));
            seq.Join(DOTween.To(() => targetHandPose.index.stretched2, x => targetHandPose.index.stretched2 = x, fingerCls.Index[3], duration));
            seq.Join(DOTween.To(() => targetHandPose.index.stretched3, x => targetHandPose.index.stretched3 = x, fingerCls.Index[4], duration));

        }
        else if (finger == "m")
        {
            seq.Join(DOTween.To(() => targetHandPose.middle.spread, x => targetHandPose.middle.spread = x, fingerCls.Middle[0], duration));
            seq.Join(DOTween.To(() => targetHandPose.middle.roll, x => targetHandPose.middle.roll = x, fingerCls.Middle[1], duration));
            seq.Join(DOTween.To(() => targetHandPose.middle.stretched1, x => targetHandPose.middle.stretched1 = x, fingerCls.Middle[2], duration));
            seq.Join(DOTween.To(() => targetHandPose.middle.stretched2, x => targetHandPose.middle.stretched2 = x, fingerCls.Middle[3], duration));
            seq.Join(DOTween.To(() => targetHandPose.middle.stretched3, x => targetHandPose.middle.stretched3 = x, fingerCls.Middle[4], duration));
        }
        else if (finger == "r")
        {
            seq.Join(DOTween.To(() => targetHandPose.ring.spread, x => targetHandPose.ring.spread = x, fingerCls.Ring[0], duration));
            seq.Join(DOTween.To(() => targetHandPose.ring.roll, x => targetHandPose.ring.roll = x, fingerCls.Ring[1], duration));
            seq.Join(DOTween.To(() => targetHandPose.ring.stretched1, x => targetHandPose.ring.stretched1 = x, fingerCls.Ring[2], duration));
            seq.Join(DOTween.To(() => targetHandPose.ring.stretched2, x => targetHandPose.ring.stretched2 = x, fingerCls.Ring[3], duration));
            seq.Join(DOTween.To(() => targetHandPose.ring.stretched3, x => targetHandPose.ring.stretched3 = x, fingerCls.Ring[4], duration));

        }
        else if (finger == "l")
        {
            seq.Join(DOTween.To(() => targetHandPose.little.spread, x => targetHandPose.little.spread = x, fingerCls.Little[0], duration));
            seq.Join(DOTween.To(() => targetHandPose.little.roll, x => targetHandPose.little.roll = x, fingerCls.Little[1], duration));
            seq.Join(DOTween.To(() => targetHandPose.little.stretched1, x => targetHandPose.little.stretched1 = x, fingerCls.Little[2], duration));
            seq.Join(DOTween.To(() => targetHandPose.little.stretched2, x => targetHandPose.little.stretched2 = x, fingerCls.Little[3], duration));
            seq.Join(DOTween.To(() => targetHandPose.little.stretched3, x => targetHandPose.little.stretched3 = x, fingerCls.Little[4], duration));

        }

        return seq;
    }
    public static string StringifyFinger(UserHandleSpace.AvatarFingerForHPC fingerCls, string finger)
    {
        string ret = "";
        List<string> strfloat = new List<string>();

        if (finger == "t")
        {
            foreach (float v in fingerCls.Thumbs)
            {
                strfloat.Add(v.ToString());
            }
        }
        else if (finger == "i")
        {
            foreach (float v in fingerCls.Index)
            {
                strfloat.Add(v.ToString());
            }
        }
        else if (finger == "m")
        {
            foreach (float v in fingerCls.Middle)
            {
                strfloat.Add(v.ToString());
            }
        }
        else if (finger == "r")
        {
            foreach (float v in fingerCls.Ring)
            {
                strfloat.Add(v.ToString());
            }
        }
        else if (finger == "l")
        {
            foreach (float v in fingerCls.Little)
            {
                strfloat.Add(v.ToString());
            }
        }
        ret = string.Join("&", strfloat);


        return ret;
    }
    public static List<float> ParseFinger(string rottext, string finger)
    {
        List<float> ret = new List<float>();

        if (finger == "t")
        {
            string[] sections = rottext.Split("&");

            foreach (string sec in sections)
            {
                float v = float.TryParse(sec, out v) ? v : 0;
                ret.Add(v);
            }            
        }
        else if ((finger == "i") || (finger == "m") || (finger == "r") || (finger == "l"))
        {
            string[] sections = rottext.Split("&");

            foreach (string sec in sections)
            {
                float v = float.TryParse(sec, out v) ? v : 0;
                ret.Add(v);
            }
        }

        return ret;
    }
}