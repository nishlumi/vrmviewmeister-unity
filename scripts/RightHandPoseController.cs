using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightHandPoseController : MonoBehaviour
{
    protected Animator animator;
    protected HumanPose humanPose;
    protected HumanPoseHandler poseHandler;

    private bool switchLeft;
    public HandPoseAsset normal;
    public HandPoseAsset pose1;
    public HandPoseAsset pose2;
    public HandPoseAsset pose3;
    public HandPoseAsset pose4;
    public HandPoseAsset pose5;
    public HandPoseAsset pose6;
    [Range(0, 6)] public int currentPose;
    [Range(0f, 1f)] public float handPoseValue;
    [Range(0f, 1f)] public float handPose1Value;
    [Range(0f, 1f)] public float handPose2Value;
    [Range(0f, 1f)] public float handPose3Value;
    [Range(0f, 1f)] public float handPose4Value;
    [Range(0f, 1f)] public float handPose5Value;
    [Range(0f, 1f)] public float handPose6Value;
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


    void Start()
    {
        animator = GetComponent<Animator>();
        poseHandler = new HumanPoseHandler(animator.avatar, animator.transform);

        switchLeft = false;

        targetHandPose = new HandPoseAsset.HandPose();
        targetHandPose.thumb = new HandPoseAsset.FingerPoseThumb();
        targetHandPose.index = new HandPoseAsset.FingerPose();
        targetHandPose.middle = new HandPoseAsset.FingerPose();
        targetHandPose.ring = new HandPoseAsset.FingerPose();
        targetHandPose.little = new HandPoseAsset.FingerPose();

        handPose1Value = 0f;
        handPose2Value = 0f;
        handPose3Value = 0f;
        handPose4Value = 0f;
        handPose5Value = 0f;
        handPose6Value = 0f;
    }

    void Update()
    {
        /*bool pose1change = handPose1Value >= 0f ? true : false;
        bool pose2change = handPose2Value >= 0f ? true : false;
        bool pose3change = handPose3Value >= 0f ? true : false;
        bool pose4change = handPose4Value >= 0f ? true : false;
        bool pose5change = handPose5Value >= 0f ? true : false;
        bool pose6change = handPose6Value >= 0f ? true : false;*/


        if (currentPose == 1)
        {
            targetHandPose.Lerp(normal.handPose, pose1.handPose, handPoseValue);
        }
        else if (currentPose == 2)
        {
            targetHandPose.Lerp(normal.handPose, pose2.handPose, handPoseValue);
        }
        else if (currentPose == 3)
        {
            targetHandPose.Lerp(normal.handPose, pose3.handPose, handPoseValue);
        }
        else if (currentPose == 4)
        {
            targetHandPose.Lerp(normal.handPose, pose4.handPose, handPoseValue);
        }
        else if (currentPose == 5)
        {
            targetHandPose.Lerp(normal.handPose, pose5.handPose, handPoseValue);
        }
        else if (currentPose == 6)
        {
            targetHandPose.Lerp(normal.handPose, pose6.handPose, handPoseValue);
        }
        else
        {
            targetHandPose.Lerp(pose1.handPose, normal.handPose, 1f);
        }

    }

    void LateUpdate()
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
        handPose1Value = 0f;
        handPose2Value = 0f;
        handPose3Value = 0f;
        handPose4Value = 0f;
        handPose5Value = 0f;
        handPose6Value = 0f;
    }
    public void SetPose(int pos, float val)
    {
        currentPose = pos;
        handPoseValue = val;
        switch (pos) {
            case 1:
                handPose1Value = val;
                break;
            case 2:
                handPose2Value = val;
                break;
            case 3:
                handPose3Value = val;
                break;
            case 4:
                handPose4Value = val;
                break;
            case 5:
                handPose5Value = val;
                break;
            case 6:
                handPose6Value = val;
                break;
            default:
                ResetPose();
                break;
        }
    }
}