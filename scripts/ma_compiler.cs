using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using VRM;
using DG.Tweening;


namespace UserHandleSpace
{
    public partial class ManageAnimation
    {
//===========================================================================================================================
//  Analyze and parse functions
//===========================================================================================================================

        private void SpecialUpdate_body(NativeAnimationFrameActor actor, int frameIndex)
        { //---Update for NON-DOTween method and object

            if ((actor.targetRole != "") && (actor.avatar != null))
            {
                if (frameIndex < actor.frames.Count)
                {
                    NativeAnimationFrame avatarFrame = actor.frames[frameIndex];
                    NativeAnimationAvatar naa = actor.avatar;

                    foreach (AnimationTargetParts movedata in avatarFrame.movingData)
                    {
                        //---VRM
                        if (actor.targetType == AF_TARGETTYPE.VRM)
                        {
                            OperateLoadedVRM ovrm = naa.avatar.GetComponent<OperateLoadedVRM>();

                            if (movedata.animationType == AF_MOVETYPE.NormalTransform)
                            {
                                if (movedata.vrmBone == ParseIKBoneType.LeftHandPose)
                                {
                                    LeftHandPoseController lhand = naa.avatar.GetComponent<LeftHandPoseController>();
                                    //---left
                                    if (movedata.handpose.Count > 0) lhand.currentPose = (int)movedata.handpose[0];
                                    

                                }

                                if (movedata.vrmBone == ParseIKBoneType.RightHandPose)
                                {
                                    RightHandPoseController rhand = naa.avatar.GetComponent<RightHandPoseController>();
                                    //---right
                                    if (movedata.handpose.Count > 0) rhand.currentPose = (int)movedata.handpose[0];

                                }
                            }
                            if (movedata.animationType == AF_MOVETYPE.VRMBlink)
                            {
                                ovrm.SetBlinkFlag(movedata.isblink);

                                ovrm.SetHeadLock(movedata.headLock);
                            }
                            if (movedata.animationType == AF_MOVETYPE.Equipment)
                            {
                                ovrm.SetEquipFlag(movedata.equipType);

                                //---For unequipping
                                ovrm.equipDestinations.list.ForEach(body =>
                                {
                                    AvatarEquipSaveClass equip = movedata.equipDestinations.Find(match =>
                                    {
                                        if ((match.bodybonename == body.bodybonename) && (match.equipitem == body.equipitem)) return true;
                                        return false;
                                    });
                                    if (equip == null) //---animation don't has this body equip...UNEQUIP
                                    {
                                        NativeAnimationAvatar cast = GetCastInProject(body.equipitem);
                                        if (cast != null)
                                        {
                                            ovrm.UnequipObject((HumanBodyBones)body.bodybonename, cast.roleName);
                                        }

                                    }
                                });
                                //---For equipping
                                movedata.equipDestinations.ForEach(body =>
                                {
                                    AvatarEquipSaveClass equip = ovrm.equipDestinations.list.Find(match =>
                                    {
                                        if ((match.bodybonename == body.bodybonename) && (match.equipitem == body.equipitem)) return true;
                                        return false;
                                    });
                                    if (equip == null) //---body don't has this animation equip...EQUIP
                                    {
                                        NativeAnimationAvatar cast = GetCastInProject(body.equipitem);
                                        if (cast != null)
                                        {
                                            //ovrm.SetPosition(body.position);
                                            //ovrm.SetRotation(body.rotation);
                                            //vi devas sxargi POSITION kaj ROTATION antaux cxi tiu metodo.
                                            ovrm.EquipObject((HumanBodyBones)body.bodybonename, cast);
                                        }
                                    }
                                });
                            }
                            else if (movedata.animationType == AF_MOVETYPE.GravityProperty)
                            {
                                movedata.gravity.list.ForEach(action =>
                                {
                                    ovrm.SetGravityDir(action.comment,action.rootBoneName,action.dir.x,action.dir.y,action.dir.z);
                                });
                            }
                            //---Changing properties for VRM IK 
                            else if (movedata.animationType == AF_MOVETYPE.VRMIKProperty)
                            {
                                movedata.handleList.ForEach(item =>
                                {
                                    ovrm.SetIKTarget(item.parts, item.name, true);
                                });
                            }
                        }
                        //---OtherObject
                        else if (actor.targetType == AF_TARGETTYPE.OtherObject)
                        {
                            OperateLoadedOther olo = naa.avatar.GetComponent<OperateLoadedOther>();
                            if (movedata.animationType == AF_MOVETYPE.AnimStart)
                            {
                                olo.PlayAnimation();
                            }
                            else if (movedata.animationType == AF_MOVETYPE.AnimStop)
                            {
                                olo.StopAnimation();
                            }
                            else if (movedata.animationType == AF_MOVETYPE.AnimPause)
                            {
                                olo.PauseAnimation();
                            }
                            else if (movedata.animationType == AF_MOVETYPE.AnimProperty)
                            {
                                olo.SetTargetClip(movedata.animName);
                                olo.SetWrapMode(movedata.animLoop);
                            }
                            else if (movedata.animationType == AF_MOVETYPE.ObjectTexture)
                            {
                                //---recover Material
                                
                                foreach (MaterialProperties mat in movedata.matProp)
                                {
                                    string param = "";
                                    param = mat.name + ",shader," + mat.shaderName;
                                    olo.SetUserMaterial(param);

                                    param = mat.name + ",maintex," + mat.texturePath;
                                    olo.SetUserMaterial(param);

                                }
                            }

                        }
                        else if (actor.targetType == AF_TARGETTYPE.Light)
                        {
                            if (movedata.animationType == AF_MOVETYPE.LightProperty)
                            {
                                Light lt = naa.avatar.GetComponent<Light>();

                                //---Light type
                                lt.type = movedata.lightType;

                                //---Light Render Mode
                                lt.renderMode = movedata.lightRenderMode;
                            }
                        }
                        else if (actor.targetType == AF_TARGETTYPE.Camera)
                        {
                            OperateLoadedCamera olc = naa.avatar.GetComponent<OperateLoadedCamera>();
                            Camera cam = naa.avatar.GetComponent<Camera>();
                            if (movedata.animationType == AF_MOVETYPE.CameraOn)
                            {
                                //olc.SetCameraPlaying(movedata.cameraPlaying);
                                olc.PreviewCamera();
                            }
                            else if (movedata.animationType == AF_MOVETYPE.CameraOff)
                            {
                                //olc.SetCameraPlaying(movedata.cameraPlaying);
                                olc.EndPreview();
                            }
                            else if (movedata.animationType == AF_MOVETYPE.CameraProperty)
                            {
                                olc.SetClearFlag(movedata.clearFlag);

                                
                                //---render texture : Camera SIDE
                                olc.SetRenderTexture(movedata.renderTex);
                                if ((movedata.renderTex.x > 0) && (movedata.renderTex.y > 0)) olc.SetCameraRenderFlag(movedata.renderFlag);

                            }
                        }
                        else if (actor.targetType == AF_TARGETTYPE.Audio)
                        {
                            OperateLoadedAudio ola = naa.avatar.GetComponent<OperateLoadedAudio>();

                            ola.SetAudio(movedata.audioName);

                            if (movedata.animationType == AF_MOVETYPE.AnimStart)
                            {
                                if (movedata.isSE == 1)
                                {
                                    ola.PlaySe();
                                }
                                else
                                {
                                    ola.PlayAudio();
                                }
                            }
                            else if (movedata.animationType == AF_MOVETYPE.AnimSeek)
                            {
                                if (movedata.seekTime != -1f)
                                {
                                    ola.SetSeekSeconds(movedata.seekTime);
                                }
                            }
                            else if (movedata.animationType == AF_MOVETYPE.AnimStop)
                            {
                                ola.StopAudio();
                            }
                            else if (movedata.animationType == AF_MOVETYPE.AnimPause)
                            {
                                ola.PauseAudio();
                            }
                            else if (movedata.animationType == AF_MOVETYPE.AudioProperty)
                            {
                                ola.SetLoop(movedata.isLoop);
                                ola.SetMute(movedata.isMute);

                            }
                        }
                        else if (actor.targetType == AF_TARGETTYPE.Effect)
                        {
                            OperateLoadedEffect ole = naa.avatar.GetComponent<OperateLoadedEffect>();

                            if (movedata.animationType == AF_MOVETYPE.AnimStart)
                            {
                                ole.SetEffect(movedata.effectGenre + "," + movedata.effectName);
                                if (movedata.animLoop == 1)
                                {
                                    ole.PlayEffect(1);
                                    //ole.SetPlayFlagEffect(2);
                                }
                                else
                                {
                                    ole.PlayEffect();
                                    //ole.SetPlayFlagEffect(1);
                                }

                            }
                            else if (movedata.animationType == AF_MOVETYPE.AnimStop)
                            {
                                ole.StopEffect();
                                //ole.SetPlayFlagEffect(0);
                            }
                            else if (movedata.animationType == AF_MOVETYPE.AnimPause)
                            {
                                ole.PauseEffect();
                            }
                            else if (movedata.animationType == AF_MOVETYPE.Collider)
                            {
                                ole.IsVRMCollider = movedata.isVRMCollider == 1 ? true : false;
                                ole.ResetColliderTarget(movedata.VRMColliderTarget);
                            }
                        }
                        else if (actor.targetType == AF_TARGETTYPE.Stage)
                        {
                            OperateStage os = naa.avatar.GetComponent<OperateStage>();
                            if (movedata.animationType == AF_MOVETYPE.StageProperty)
                            {
                                os.SelectStage(movedata.stageType);

                                if (os.GetActiveStageType() == StageKind.Default)
                                {
                                    os.SetDefaultStageColor(movedata.color);
                                }
                                else if (os.GetActiveStageType() == StageKind.User)
                                {
                                    if (movedata.matProp.Count > 1)
                                    {
                                        os.SetTextureToUserStage("main," + movedata.matProp[0].texturePath);
                                        os.SetTextureToUserStage("normal," + movedata.matProp[1].texturePath);
                                    }
                                }
                            }
                            

                            //---sky
                            if (movedata.animationType == AF_MOVETYPE.SkyProperty)
                            {
                                CameraOperation1 cam = os.GetCameraOperation();

                                cam.SetClearFlag(movedata.skyType);
                                if (movedata.skyType == CameraClearFlags.Color)
                                {
                                    cam.SetSkyColor(movedata.skyColor);
                                }
                                else if (movedata.skyType == CameraClearFlags.Skybox)
                                {
                                    cam.SetSkyShader(movedata.skyShaderName);
                                }
                            }
                            
                        }

                    }
                }

            }
        }
        private Sequence ParseForCommon(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationTargetParts pelvisCondition, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;
            OperateLoadedBase olb = naa.avatar.GetComponent<OperateLoadedBase>();

            if (targetObjects.targetType == AF_TARGETTYPE.VRM)
            {
                int index = (int)movedata.vrmBone;

                if (movedata.vrmBone == ParseIKBoneType.IKParent)
                {
                    if (movedata.animationType == AF_MOVETYPE.Translate)
                    {
                        if (targetObjects.compiled == 1)
                        {
                            if (movedata.jumpNum >= 1)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOJump(movedata.position, movedata.jumpPower, movedata.jumpNum, frame.duration));
                            }
                            else
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOMove(movedata.position, frame.duration));
                                else naa.avatar.transform.position = movedata.position;
                            }
                            
                        }

                        {
                            if (movedata.jumpNum >= 1)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(naa.ikparent.transform.DOJump(movedata.position, movedata.jumpPower, movedata.jumpNum, frame.duration));
                            }
                            else
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(naa.ikparent.transform.DOMove(movedata.position, frame.duration));
                                else naa.ikparent.transform.position = movedata.position;
                            }
                                
                        }
                        if (options.isBuildDoTween == 0)
                        {
                            olb.SetJump(movedata.jumpPower.ToString() + "," + movedata.jumpNum.ToString());
                        }
                    }
                    if (movedata.animationType == AF_MOVETYPE.Rotate)
                    {
                        if (targetObjects.compiled == 1)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DORotate(movedata.rotation, frame.duration));
                            else naa.avatar.transform.rotation = Quaternion.Euler(movedata.rotation);

                        }

                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.ikparent.transform.DORotate(movedata.rotation, frame.duration));
                            else naa.ikparent.transform.rotation = Quaternion.Euler(movedata.rotation);

                        }
                    }
                    if (movedata.animationType == AF_MOVETYPE.Scale)
                    {
                        if (targetObjects.compiled == 1)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOScale(movedata.scale, frame.duration));
                            else naa.avatar.transform.localScale = movedata.scale;

                        }

                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOScale(movedata.scale, frame.duration));
                            else naa.avatar.transform.localScale = movedata.scale;

                        }
                    }
                }
                else
                {
                    if (targetObjects.compiled == 1)
                    { //---Transform for HumanBodyBones
                        if (movedata.vrmBone == ParseIKBoneType.UseHumanBodyBones)
                        {
                            Transform boneTransform = naa.avatar.GetComponent<Animator>().GetBoneTransform(movedata.vrmHumanBodyBone);
                            if (movedata.animationType == AF_MOVETYPE.Rotate)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(boneTransform.DOLocalRotate(movedata.rotation, frame.duration));
                                else boneTransform.localRotation = Quaternion.Euler(movedata.rotation);
                            }
                            if (movedata.animationType == AF_MOVETYPE.Scale)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(boneTransform.DOScale(movedata.scale, frame.duration));
                                else boneTransform.localScale = movedata.scale;
                            }
                        }
                    }

                    { //---Transform for IK
                        if ((movedata.vrmBone >= ParseIKBoneType.EyeViewHandle) && (movedata.vrmBone <= ParseIKBoneType.RightLeg))
                        { 
                            GameObject realObject = naa.ikparent.transform.Find(IKBoneNames[index]).gameObject;
                            //---Position (absorb a distance of height)
                            if (movedata.animationType == AF_MOVETYPE.Translate)
                            {
                                /*
                                Vector3 repos = CalculateReposition(
                                    naa.avatar, naa.ikparent, targetObjects.targetType, 
                                    targetObjects.bodyHeight, 
                                    movedata.position, movedata.vrmBone, pelvisCondition.position
                                );*/
                                List<Vector3> curList = naa.avatar.GetComponent<OperateLoadedVRM>().GetTPoseBodyList();
                                int vbone = (int)movedata.vrmBone;

                                Vector3 repos = CalculateDifferenceInHeight(
                                    curList[vbone],
                                    frame.useBodyInfo == UseBodyInfoType.TimelineCharacter ? targetObjects.bodyInfoList[vbone] : curList[vbone],
                                    movedata.position, movedata.vrmBone
                                );
                                if (options.isExecuteForDOTween == 1) seq.Join(realObject.transform.DOLocalMove(repos, frame.duration));
                                else realObject.transform.localPosition = repos;
                            }

                            //---Rotation
                            if (movedata.animationType == AF_MOVETYPE.Rotate)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(realObject.transform.DOLocalRotate(movedata.rotation, frame.duration));
                                else realObject.transform.rotation = Quaternion.Euler(movedata.rotation);
                            }
                        }

                    }
                }

            }
            else if ((targetObjects.targetType == AF_TARGETTYPE.Text) || (targetObjects.targetType == AF_TARGETTYPE.UImage))
            {
                RectTransform rectt = targetObjects.avatar.avatar.GetComponent<RectTransform>();
                OperateLoadedUImage olui = targetObjects.avatar.avatar.GetComponent<OperateLoadedUImage>();

                if (movedata.animationType == AF_MOVETYPE.Translate)
                {
                    
                    Vector2 v2 = new Vector2(Screen.width * (movedata.position.x/100f), Screen.height * (movedata.position.y/100f));
                    if (options.isExecuteForDOTween == 1) seq.Join(rectt.DOAnchorPos(v2, frame.duration));
                    else rectt.anchoredPosition = v2;

                    if (options.isBuildDoTween == 0)
                    {
                        olui.currentPositionPercent = new Vector2(movedata.position.x, movedata.position.y);
                    }
                }
                else if (movedata.animationType == AF_MOVETYPE.Rotate)
                {
                    //---2D object is Z-dimension only.
                    Vector3 v3 = new Vector3(movedata.rotation.x, movedata.rotation.y, movedata.rotation.z);
                    if (options.isExecuteForDOTween == 1) seq.Join(rectt.DORotate(v3, frame.duration));
                    else rectt.rotation = Quaternion.Euler(v3);
                }
                else if (movedata.animationType == AF_MOVETYPE.Scale)
                {
                    Vector2 v2 = new Vector2(movedata.scale.x, movedata.scale.y);
                    if (options.isExecuteForDOTween == 1) seq.Join(rectt.DOSizeDelta(v2, frame.duration));
                    else rectt.sizeDelta = v2;

                }
            }
            else if (targetObjects.targetType == AF_TARGETTYPE.Stage)
            {
                OperateStage os = naa.avatar.GetComponent<OperateStage>();

                if (movedata.animationType == AF_MOVETYPE.Translate)
                {
                    if (options.isExecuteForDOTween == 1) seq.Join(os.ActiveStage.transform.DOMove(movedata.position, frame.duration));
                    else os.ActiveStage.transform.position = movedata.position;
                }
                if (movedata.animationType == AF_MOVETYPE.Rotate)
                {
                    if (options.isExecuteForDOTween == 1) seq.Join(os.ActiveStage.transform.DORotate(movedata.rotation, frame.duration));
                    else os.ActiveStage.transform.rotation = Quaternion.Euler(movedata.rotation);
                }
                if (movedata.animationType == AF_MOVETYPE.Scale)
                {
                    if (options.isExecuteForDOTween == 1) seq.Join(os.ActiveStage.transform.DOScale(movedata.scale, frame.duration));
                    else os.ActiveStage.transform.localScale = movedata.scale;
                }
            }
            else
            {
                if (movedata.animationType == AF_MOVETYPE.Translate)
                {
                    if (targetObjects.compiled == 1)
                    {
                        if (movedata.jumpNum >= 1)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOJump(movedata.position, movedata.jumpPower, movedata.jumpNum, frame.duration));
                        }
                        else
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOMove(movedata.position, frame.duration));
                            else naa.avatar.transform.position = movedata.position;
                        }
                    }
                    

                    {
                        if (movedata.jumpNum >= 1)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.ikparent.transform.DOJump(movedata.position, movedata.jumpPower, movedata.jumpNum, frame.duration));
                        }
                        else
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.ikparent.transform.DOMove(movedata.position, frame.duration));
                            else naa.ikparent.transform.position = movedata.position;
                        }

                    }

                    if (options.isBuildDoTween == 0)
                    {
                        olb.SetJump(movedata.jumpPower.ToString() + "," + movedata.jumpNum.ToString());
                    }
                }
                if (movedata.animationType == AF_MOVETYPE.Rotate)
                {
                    if (targetObjects.compiled == 1)
                    {
                        if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DORotate(movedata.rotation, frame.duration));
                        else naa.avatar.transform.rotation = Quaternion.Euler(movedata.rotation);

                    }

                    {
                        if (options.isExecuteForDOTween == 1) seq.Join(naa.ikparent.transform.DORotate(movedata.rotation, frame.duration));
                        else naa.ikparent.transform.rotation = Quaternion.Euler(movedata.rotation);

                    }
                }
                if (movedata.animationType == AF_MOVETYPE.Scale)
                {
                    if (targetObjects.compiled == 1)
                    {
                        if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOScale(movedata.scale, frame.duration));
                        else naa.avatar.transform.localScale = movedata.scale;

                    }

                    {
                        if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOScale(movedata.scale, frame.duration));
                        else naa.avatar.transform.localScale = movedata.scale;

                    }
                }
            }
            if (movedata.animationType == AF_MOVETYPE.Punch)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                     {
                         olb.SetPunch(JsonUtility.ToJson(movedata.effectPunch));
                     },false));
                }
                //if (targetObjects.compiled == 1)
                {
                    if (movedata.effectPunch.isEnable == 1)
                    {
                        if (movedata.effectPunch.translationType == AF_MOVETYPE.Translate)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOPunchPosition(movedata.effectPunch.punch, frame.duration, movedata.effectPunch.vibrato, movedata.effectPunch.elasiticity));
                        }
                        else if (movedata.effectPunch.translationType == AF_MOVETYPE.Rotate)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOPunchRotation(movedata.effectPunch.punch, frame.duration, movedata.effectPunch.vibrato, movedata.effectPunch.elasiticity));
                        }
                        else if (movedata.effectPunch.translationType == AF_MOVETYPE.Scale)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOPunchScale(movedata.effectPunch.punch, frame.duration, movedata.effectPunch.vibrato, movedata.effectPunch.elasiticity));
                        }
                    }
                       
                }

                /*if (options.isBuildDoTween == 0)
                {
                    olb.SetPunch(JsonUtility.ToJson(movedata.effectPunch));
                }*/
            }
            if (movedata.animationType == AF_MOVETYPE.Shake)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        olb.SetShake(JsonUtility.ToJson(movedata.effectShake));
                    }, false));
                }
                //if (targetObjects.compiled == 1)
                {
                    if (movedata.effectShake.isEnable == 1)
                    {
                        if (movedata.effectShake.translationType == AF_MOVETYPE.Translate)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOShakePosition(frame.duration,movedata.effectShake.strength,movedata.effectShake.vibrato,movedata.effectShake.randomness,false,movedata.effectShake.fadeOut == 1 ? true : false));
                        }
                        else if (movedata.effectShake.translationType == AF_MOVETYPE.Rotate)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOShakeRotation(frame.duration, movedata.effectShake.strength, movedata.effectShake.vibrato, movedata.effectShake.randomness, movedata.effectShake.fadeOut == 1 ? true : false));
                        }
                        else if (movedata.effectShake.translationType == AF_MOVETYPE.Scale)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOShakeScale(frame.duration, movedata.effectShake.strength, movedata.effectShake.vibrato, movedata.effectShake.randomness, movedata.effectShake.fadeOut == 1 ? true : false));
                        }
                    }

                }
                
                /*if (options.isBuildDoTween == 0)
                {
                    olb.SetShake(JsonUtility.ToJson(movedata.effectShake));
                }*/
            }
            return seq;
        }

        private Sequence ParseForVRM(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;

            OperateLoadedVRM ovrm = naa.avatar.GetComponent<OperateLoadedVRM>();

            //---HandPose
            if (targetObjects.compiled != 1)
            {
                if (movedata.animationType == AF_MOVETYPE.NormalTransform)
                {
                    if (movedata.vrmBone == ParseIKBoneType.LeftHandPose)
                    {
                        LeftHandPoseController lhand = naa.avatar.GetComponent<LeftHandPoseController>();
                        //---left
                        if (movedata.handpose.Count > 0)
                        {
                            if (options.isExecuteForDOTween == 1)
                            {
                                seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                                {
                                    lhand.currentPose = (int)movedata.handpose[0];
                                }, false));
                            }
                            //lhand.currentPose = (int)movedata.handpose[0];
                        }
                        if (movedata.handpose.Count > 1)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => lhand.handPoseValue, x => lhand.handPoseValue = x, movedata.handpose[1], frame.duration));
                            else lhand.handPoseValue = (int)movedata.handpose[1];
                        }

                    }

                    if (movedata.vrmBone == ParseIKBoneType.RightHandPose)
                    {
                        RightHandPoseController rhand = naa.avatar.GetComponent<RightHandPoseController>();
                        //---right
                        if (movedata.handpose.Count > 0)
                        {
                            if (options.isExecuteForDOTween == 1)
                            {
                                seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                                {
                                    rhand.currentPose = (int)movedata.handpose[0];
                                }, false));
                            }
                            //rhand.currentPose = (int)movedata.handpose[0];
                        }
                        if (movedata.handpose.Count > 1)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => rhand.handPoseValue, x => rhand.handPoseValue = x, movedata.handpose[1], frame.duration));
                            else rhand.handPoseValue = (int)movedata.handpose[1];
                        }

                    }

                }
            }


            //---Blend Shape
            if (movedata.animationType == AF_MOVETYPE.BlendShape)
            {
                //OperateLoadedVRM mainface = naa.avatar.GetComponent<OperateLoadedVRM>();
                SkinnedMeshRenderer face = ovrm.GetBlendShapeTarget();
                int maxcnt = face.sharedMesh.blendShapeCount;
                foreach (BasicStringFloatList val in movedata.blendshapes)
                {
                    float weight = val.value;

                    string hitName = "";
                    for (int chki = 0; chki < maxcnt; chki++)
                    {
                        if ((face.sharedMesh.GetBlendShapeName(chki) + "$").Contains(val.text+"$"))
                        {
                            hitName = face.sharedMesh.GetBlendShapeName(chki);
                            break;
                        }
                    }
                    
                    int bindex = face.sharedMesh.GetBlendShapeIndex(hitName);
                    //if (bindex > -1) face.SetBlendShapeWeight(bindex, weight);

                    if (bindex > -1)
                    {
                        if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => face.GetBlendShapeWeight(bindex), x => face.SetBlendShapeWeight(bindex, x), weight, frame.duration));
                        else ovrm.changeAvatarBlendShapeByName(val.text, val.value);  //face.SetBlendShapeWeight(bindex, weight);
                    }

                }
            }
            if (movedata.animationType == AF_MOVETYPE.VRMBlink)
            {
                if (options.isBuildDoTween == 0)
                {
                    ovrm.SetBlinkFlag(movedata.isblink);
                    ovrm.SetBlinkInterval(movedata.interval);
                    ovrm.SetBlinkOpeningSeconds(movedata.openingSeconds);
                    ovrm.SetBlinkCloseSeconds(movedata.closeSeconds);
                    ovrm.SetBlinkClosingTime(movedata.closingTime);
                    ovrm.SetHeadLock(movedata.headLock);

                }
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOTween.To(() => ovrm.BlinkEye.Interval, x => ovrm.BlinkEye.Interval = x, movedata.interval, frame.duration));
                    seq.Join(DOTween.To(() => ovrm.BlinkEye.OpeningSeconds, x => ovrm.BlinkEye.OpeningSeconds = x, movedata.openingSeconds, frame.duration));
                    seq.Join(DOTween.To(() => ovrm.BlinkEye.CloseSeconds, x => ovrm.BlinkEye.CloseSeconds = x, movedata.closeSeconds, frame.duration));
                    seq.Join(DOTween.To(() => ovrm.BlinkEye.ClosingTime, x => ovrm.BlinkEye.ClosingTime = x, movedata.closingTime, frame.duration));
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        ovrm.SetBlinkFlag(movedata.isblink);
                    }, false));
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        ovrm.SetHeadLock(movedata.headLock);
                    }, false));
                }
            }
            if (movedata.animationType == AF_MOVETYPE.Equipment)
            { //===Not include an animation (Until there is a prospect of correction.) ============================================================

                //if (options.isBuildDoTween == 0)
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        ovrm.SetEquipFlag(movedata.equipType);

                        //---For unequipping
                        ovrm.equipDestinations.list.ForEach(body =>
                        {
                            AvatarEquipSaveClass equip = movedata.equipDestinations.Find(match =>
                            {
                                if ((match.bodybonename == body.bodybonename) && (match.equipitem == body.equipitem)) return true;
                                return false;
                            });
                            if (equip == null) //---animation don't has this body equip...UNEQUIP
                            {
                                NativeAnimationAvatar cast = GetCastInProject(body.equipitem);
                                if (cast != null)
                                {
                                    ovrm.UnequipObject((HumanBodyBones)body.bodybonename, cast.roleName);
                                }

                            }
                        });
                        //---For equipping
                        movedata.equipDestinations.ForEach(body =>
                        {
                            AvatarEquipSaveClass equip = ovrm.equipDestinations.list.Find(match =>
                            {
                                if ((match.bodybonename == body.bodybonename) && (match.equipitem == body.equipitem)) return true;
                                return false;
                            });
                            if (equip == null) //---body don't has this animation equip...EQUIP
                            {
                                NativeAnimationAvatar cast = GetCastInProject(body.equipitem);
                                if (cast != null)
                                {
                                    //ovrm.SetPosition(body.position);
                                    //ovrm.SetRotation(body.rotation);
                                    //vi devas sxargi POSITION kaj ROTATION antaux cxi tiu metodo.
                                    ovrm.EquipObject((HumanBodyBones)body.bodybonename, cast);
                                }
                            }
                        });
                    }, false));
                }
                
            }

            //---VRM Gravity information
            if (movedata.animationType == AF_MOVETYPE.GravityProperty)
            {
                //if (options.isBuildDoTween == 0)
                if (options.isExecuteForDOTween == 1)
                {
                    movedata.gravity.list.ForEach(action =>
                    {
                        //ovrm.SetGravityPower(action.comment + "," + action.rootBoneName + "," + action.power.ToString());
                        ovrm.SetAnimationGravityPower(action.comment, action.rootBoneName, seq, action.power, frame.duration);

                        //ovrm.SetGravityDirFromOuter(action.comment + "," + action.rootBoneName + "," + action.dir.x.ToString() + "," + action.dir.y.ToString() + "," + action.dir.z.ToString());
                        seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                        {
                            ovrm.SetGravityDir(action.comment, action.rootBoneName, action.dir.x, action.dir.y, action.dir.z);
                        }, false));
                    });
                }
            }
            //---Changing properties for VRM IK 
            if (movedata.animationType == AF_MOVETYPE.VRMIKProperty)
            {
                //if (options.isBuildDoTween == 0)
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        //---recoverly as setting 
                        if (movedata.handleList.Count == 0) ovrm.ResetIKMappingList();

                        movedata.handleList.ForEach(item =>
                        {
                            ovrm.SetIKTarget(item.parts, item.name, true);
                        });
                    }, false));
                }
            }
            if (movedata.animationType == AF_MOVETYPE.ObjectTexture)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    /*
                    ovrm.SetMaterialTween(seq, "srcblend", movedata.vmatProp, frame.duration);
                    ovrm.SetMaterialTween(seq, "dstblend", movedata.vmatProp, frame.duration);
                    ovrm.SetMaterialTween(seq, "color", movedata.vmatProp, frame.duration);
                    ovrm.SetMaterialTween(seq, "cullmode", movedata.vmatProp, frame.duration);
                    ovrm.SetMaterialTween(seq, "renderingtype", movedata.vmatProp, frame.duration);
                    ovrm.SetMaterialTween(seq, "emissioncolor", movedata.vmatProp, frame.duration);
                    ovrm.SetMaterialTween(seq, "shadetexcolor", movedata.vmatProp, frame.duration);
                    ovrm.SetMaterialTween(seq, "rimcolor", movedata.vmatProp, frame.duration);
                    ovrm.SetMaterialTween(seq, "shadingtoony", movedata.vmatProp, frame.duration);
                    ovrm.SetMaterialTween(seq, "rimfresnel", movedata.vmatProp, frame.duration);
                    */
                    foreach (MaterialProperties mat in movedata.matProp)
                    {
                        seq = ovrm.SetMaterialTween(seq, mat.name, mat, frame.duration);
                    }
                }
                /*if (options.isBuildDoTween == 0)
                {
                    ovrm.SetTextureConfig(movedata.vmatProp);
                }*/
            }
            return seq;
        }
        private Sequence ParseForOtherObject(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;
            OperateLoadedOther olo = naa.avatar.GetComponent<OperateLoadedOther>();
            if (movedata.animationType == AF_MOVETYPE.AnimStart)
            {

                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                     {
                         olo.SetTargetClip(movedata.animName);
                         olo.SetWrapMode(movedata.animLoop);
                         olo.SetSpeedAnimation(movedata.animSpeed);

                         olo.SeekPlayAnimation(movedata.animSeek);
                         olo.PlayAnimation();
                         olo.SetPlayFlagAnimation(UserAnimationState.Play);
                     }, false));
                }
                if (options.isBuildDoTween == 0)
                {
                    olo.SetTargetClip(movedata.animName);
                    olo.SetWrapMode(movedata.animLoop);
                    olo.SetSpeedAnimation(movedata.animSpeed);

                    //olo.PlayAnimation();
                    olo.SeekPlayAnimation(movedata.animSeek);
                    olo.SetPlayFlagAnimation(movedata.animPlaying);
                }
            }
            else if (movedata.animationType == AF_MOVETYPE.AnimStop)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        olo.StopAnimation();
                        olo.SetPlayFlagAnimation(UserAnimationState.Stop);
                    }, false));
                }
                if (options.isBuildDoTween == 0)
                {
                    olo.SetTargetClip(movedata.animName);
                    olo.SetWrapMode(movedata.animLoop);
                    olo.SetSpeedAnimation(movedata.animSpeed);

                    //olo.StopAnimation();
                    olo.SetPlayFlagAnimation(movedata.animPlaying);
                }
            }
            else if (movedata.animationType == AF_MOVETYPE.AnimSeek)
            {

                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        olo.SetTargetClip(movedata.animName);
                        olo.SetWrapMode(movedata.animLoop);
                        olo.SetSpeedAnimation(movedata.animSpeed);

                        olo.SetPlayFlagAnimation(UserAnimationState.Seeking);
                    }, false));
                }
                if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => olo.SeekPosition, x => olo.SeekPosition = x, movedata.animSeek, frame.duration));
                else olo.SeekPosition = movedata.animSeek;

                if (options.isBuildDoTween == 0)
                {
                    olo.SetTargetClip(movedata.animName);
                    olo.SetWrapMode(movedata.animLoop);
                    olo.SetSpeedAnimation(movedata.animSpeed);

                    //olo.SeekPlayAnimation(movedata.animSeek);
                    olo.SetPlayFlagAnimation(movedata.animPlaying);
                }
            }
            else if (movedata.animationType == AF_MOVETYPE.AnimPause)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        olo.PauseAnimation();
                        olo.SetPlayFlagAnimation(UserAnimationState.Pause);
                    }, false));
                }
                if (options.isBuildDoTween == 0)
                {
                    olo.SetTargetClip(movedata.animName);
                    olo.SetWrapMode(movedata.animLoop);
                    olo.SetSpeedAnimation(movedata.animSpeed);

                    //olo.PauseAnimation();
                    olo.SetPlayFlagAnimation(movedata.animPlaying);
                }
            }
            else if (movedata.animationType == AF_MOVETYPE.AnimProperty)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        olo.SetTargetClip(movedata.animName);
                        olo.SetWrapMode(movedata.animLoop);
                    }, false));
                }
                if (options.isBuildDoTween == 0)
                {
                    olo.SetTargetClip(movedata.animName);
                    olo.SetWrapMode(movedata.animLoop);
                }

                if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => olo.GetSpeedAnimation(), x => olo.SetSpeedAnimation(x), movedata.animSpeed, frame.duration));
                else olo.SetSpeedAnimation(movedata.animSpeed);

            }
            else if (movedata.animationType == AF_MOVETYPE.Rest)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        olo.SetPlayFlagAnimation(UserAnimationState.Playing);
                    }, false));
                }
                if (options.isBuildDoTween == 0)
                {
                    olo.SetPlayFlagAnimation(movedata.animPlaying);
                }
            }
            
            else if (movedata.animationType == AF_MOVETYPE.ObjectTexture)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    foreach (MaterialProperties mat in movedata.matProp)
                    {
                        seq = olo.SetMaterialTween(seq, mat.name, mat, frame.duration);
                        /*
                        olo.SetMaterialTween(seq, mat.name, "color", mat, frame.duration);

                        olo.SetMaterialTween(seq, mat.name, "cullmode", mat, frame.duration);
                        olo.SetMaterialTween(seq, mat.name, "renderingtype", mat, frame.duration);

                        olo.SetMaterialTween(seq, mat.name, "emissioncolor", mat, frame.duration);

                        if (mat.shaderName.ToLower() == "standard")
                        {
                            olo.SetMaterialTween(seq, mat.name, "metallic", mat, frame.duration);
                            olo.SetMaterialTween(seq, mat.name, "glossiness", mat, frame.duration);
                        }
                        else if (mat.shaderName.ToLower() == "vrm/mtoon")
                        {
                            olo.SetMaterialTween(seq, mat.name, "shadetexcolor", mat, frame.duration);
                            olo.SetMaterialTween(seq, mat.name, "rimcolor", mat, frame.duration);
                            olo.SetMaterialTween(seq, mat.name, "shadingtoony", mat, frame.duration);
                            olo.SetMaterialTween(seq, mat.name, "rimfresnel", mat, frame.duration);
                        }

                        seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                         {
                             olo.SetUserMaterial(mat.name + ",shader," + mat.shaderName);
                         }, false));

                        seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                        {
                            olo.SetUserMaterial(mat.name + ",maintex," + mat.texturePath);
                        }, false));
                        */
                    }
                }
                /*
                if (options.isBuildDoTween == 0)
                {
                    //---recover Material
                    foreach (OperateLoadedOther.MaterialProperties mat in movedata.matProp)
                    {
                        
                        string param = "";
                        param = mat.name + ",shader," + mat.shaderName;
                        olo.SetUserMaterial(param);

                        param = mat.name + ",maintex," + mat.texturePath;
                        olo.SetUserMaterial(param);

                    }
                }
                */
            }
            

            return seq;
        }
        private Sequence ParseForLight(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;

            Light lt = naa.avatar.GetComponent<Light>();

            if (movedata.animationType == AF_MOVETYPE.LightProperty)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    //---Light type
                    //if (options.isBuildDoTween == 0) lt.type = movedata.lightType;
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        lt.type = movedata.lightType;
                    }, false));

                    //---Range
                    seq.Join(DOTween.To(() => lt.range, x => lt.range = x, movedata.range, frame.duration));

                    //---Color
                    seq.Join(lt.DOColor(movedata.color, frame.duration));

                    //---Power
                    seq.Join(lt.DOIntensity(movedata.power, frame.duration));

                    //---Angle (SpotLight only)
                    if (lt.type == LightType.Spot)
                    {
                        seq.Join(DOTween.To(() => lt.spotAngle, x => lt.spotAngle = x, movedata.spotAngle, frame.duration));
                    }

                    //---Light Render Mode
                    //if (options.isBuildDoTween == 0) lt.renderMode = movedata.lightRenderMode;
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        lt.renderMode = movedata.lightRenderMode;
                    }, false));
                }
                else
                {
                    lt.type = movedata.lightType;
                    lt.range = movedata.range;
                    lt.color = movedata.color;
                    lt.intensity = movedata.power;
                    if (lt.type == LightType.Spot)
                    {
                        lt.spotAngle = movedata.spotAngle;
                    }
                    lt.renderMode = movedata.lightRenderMode;
                }

            }

            return seq;
        }
        private Sequence ParseForCamera(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;

            OperateLoadedCamera olc = naa.avatar.GetComponent<OperateLoadedCamera>();
            Camera cam = naa.avatar.GetComponent<Camera>();
            if (movedata.animationType == AF_MOVETYPE.Camera)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        olc.SetCameraPlaying(movedata.cameraPlaying);
                    }, false));
                }
            }
            else if (movedata.animationType == AF_MOVETYPE.CameraOn)
            {
                //if (options.isBuildDoTween == 0)
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        olc.SetCameraPlaying(movedata.cameraPlaying);
                        if (options.isCameraPreviewing == 1) olc.PreviewCamera();
                    }, false));
                    //olc.SetCameraPlaying(movedata.cameraPlaying);
                    //if (options.isCameraPreviewing == 1) olc.PreviewCamera();
                }
            }
            else if (movedata.animationType == AF_MOVETYPE.CameraOff)
            {
                //if (options.isBuildDoTween == 0)
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        olc.SetCameraPlaying(movedata.cameraPlaying);
                        olc.EndPreview();
                    }, false));
                    //olc.SetCameraPlaying(movedata.cameraPlaying);
                    //olc.EndPreview();
                }
            }
            else if (movedata.animationType == AF_MOVETYPE.CameraProperty)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(cam.DOFieldOfView(movedata.fov, frame.duration));

                    seq.Join(cam.DOColor(movedata.color, frame.duration));

                    seq.Join(cam.DORect(movedata.viewport, frame.duration));

                    seq.Join(DOTween.To(() => cam.depth, x => cam.depth = x, movedata.depth, frame.duration));

                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        olc.SetClearFlag(movedata.clearFlag);

                        //---LOAD configuration only: render texture etc...
                        olc.SetRenderTexture(movedata.renderTex);
                        if ((movedata.renderTex.x > 0) && (movedata.renderTex.y > 0)) olc.SetCameraRenderFlag(movedata.renderFlag);
                    }, false));
                }
                else
                {
                    cam.fieldOfView = movedata.fov;
                    cam.backgroundColor = movedata.color;
                    cam.rect = movedata.viewport;
                    cam.depth = movedata.depth;
                }

                /*if (options.isBuildDoTween == 0)
                {
                    olc.SetClearFlag(movedata.clearFlag);

                    //---LOAD configuration only: render texture etc...
                    olc.SetRenderTexture(movedata.renderTex);
                    if ((movedata.renderTex.x > 0) && (movedata.renderTex.y > 0)) olc.SetCameraRenderFlag(movedata.renderFlag);
                }*/
            }
            return seq;
        }
        private Sequence ParseForText(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;

            Text ui = naa.avatar.GetComponent<Text>();
            if (movedata.animationType == AF_MOVETYPE.Text)
            {
                if (options.isExecuteForDOTween == 1) seq.Join(ui.DOText(movedata.text, frame.duration));
                ui.text = movedata.text;
            }
            else if (movedata.animationType == AF_MOVETYPE.TextProperty)
            {
                if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => ui.fontSize, x => ui.fontSize = x, movedata.fontSize, frame.duration));
                else ui.fontSize = movedata.fontSize;

                ui.fontStyle = movedata.fontStyle;

                if (options.isExecuteForDOTween == 1) seq.Join(ui.DOColor(movedata.color, frame.duration));
                else ui.color = movedata.color;
            }
            return seq;
        }
        private Sequence ParseForImage(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;

            if (movedata.animationType == AF_MOVETYPE.ImageProperty)
            {
                OperateLoadedOther olo = naa.avatar.GetComponent<OperateLoadedOther>();

                if (options.isExecuteForDOTween == 1)
                {
                    olo.SetBaseColorForAnimation(seq, frame.duration, movedata.color);
                }
                else
                {
                    olo.SetBaseColor(movedata.color);
                }
            }

            return seq;
        }
        private Sequence ParseForUImage(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;
            Image ui = naa.avatar.GetComponent<Image>();

            if (movedata.animationType == AF_MOVETYPE.ImageProperty)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(ui.DOColor(movedata.color, frame.duration));
                }
                else
                {
                    ui.color = movedata.color;
                }
            }

            if (options.isBuildDoTween == 0)
            {
            }

            return seq;
        }
        private Sequence ParseForAudio(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;

            OperateLoadedAudio ola = naa.avatar.GetComponent<OperateLoadedAudio>();
            

            if (options.isBuildDoTween == 0)
            { //---mainly preview operation------------------------
                //ola.IsSE = movedata.isSE == 1 ? true : false;

                if (movedata.animationType == AF_MOVETYPE.AnimStart)
                {
                    ola.SetAudio(movedata.audioName);
                    ola.SetSeekSeconds(movedata.seekTime);
                    ola.SetPlayFlag(movedata.animPlaying);

                    if (ola.IsSE)
                    {
                        ola.PlaySe();
                    }
                    else
                    {
                        ola.PlayAudio();
                    }
                }
                else if (movedata.animationType == AF_MOVETYPE.AnimPause)
                {
                    ola.SetAudio(movedata.audioName);
                    ola.SetSeekSeconds(movedata.seekTime);
                    ola.SetPlayFlag(movedata.animPlaying);

                    ola.PauseAudio();
                }
                else if (movedata.animationType == AF_MOVETYPE.AnimSeek)
                {
                    ola.SetAudio(movedata.audioName);
                    ola.SetSeekSeconds(movedata.seekTime);
                    ola.SetPlayFlag(movedata.animPlaying);

                }
                else if (movedata.animationType == AF_MOVETYPE.AnimStop)
                {
                    ola.SetAudio(movedata.audioName);
                    ola.SetSeekSeconds(movedata.seekTime);
                    ola.SetPlayFlag(movedata.animPlaying);

                    ola.StopAudio();

                }
                else if (movedata.animationType == AF_MOVETYPE.Rest)
                {
                    ola.SetAudio(movedata.audioName);
                    ola.SetSeekSeconds(movedata.seekTime);
                    ola.SetPlayFlag(movedata.animPlaying);

                }
            }
            else
            { //---just animation only-------------------------------------
                if (options.isExecuteForDOTween == 1)
                {
                    /*seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        ola.IsSE = movedata.isSE == 1 ? true : false;
                        ola.SetAudio(movedata.audioName);
                    }, false));*/
                    if (movedata.animationType == AF_MOVETYPE.AnimStart)
                    {
                        seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                        {
                            //ola.IsSE = movedata.isSE == 1 ? true : false;
                            ola.SetAudio(movedata.audioName);

                            ola.SetSeekSeconds(movedata.seekTime);
                            if (ola.IsSE)
                            {
                                ola.PlaySe();
                            }
                            else
                            {
                                ola.PlayAudio();
                            }
                            ola.SetPlayFlag(UserAnimationState.Play);
                        }, false));

                    }
                    else if (movedata.animationType == AF_MOVETYPE.AnimPause)
                    {
                        seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                        {
                            //ola.IsSE = movedata.isSE == 1 ? true : false;
                            ola.SetAudio(movedata.audioName);

                            ola.SetSeekSeconds(movedata.seekTime);
                            ola.PauseAudio();
                            ola.SetPlayFlag(UserAnimationState.Pause);
                        }, false));
                    }
                    else if (movedata.animationType == AF_MOVETYPE.AnimSeek)
                    {
                        seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                        {
                            //ola.IsSE = movedata.isSE == 1 ? true : false;
                            ola.SetAudio(movedata.audioName);

                            ola.SetSeekSeconds(movedata.seekTime);
                            ola.SetPlayFlag(UserAnimationState.Seeking);
                        }, false));

                    }
                    else if (movedata.animationType == AF_MOVETYPE.AnimStop)
                    {
                        seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                        {
                            //ola.IsSE = movedata.isSE == 1 ? true : false;
                            ola.SetAudio(movedata.audioName);

                            ola.SetSeekSeconds(movedata.seekTime);
                            ola.StopAudio();
                            ola.SetPlayFlag(UserAnimationState.Stop);
                        }, false));

                    }
                    else if (movedata.animationType == AF_MOVETYPE.Rest)
                    {
                        seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                        {
                            //ola.IsSE = movedata.isSE == 1 ? true : false;
                            ola.SetAudio(movedata.audioName);

                            ola.SetSeekSeconds(movedata.seekTime);
                            ola.SetPlayFlag(UserAnimationState.Playing);
                        }, false));

                    }

                }
            }
            


            if (movedata.animationType == AF_MOVETYPE.AudioProperty)
            {
                if (options.isBuildDoTween == 0)
                {
                    ola.SetLoop(movedata.isLoop);
                    ola.SetMute(movedata.isMute);

                    ola.SetPitch(movedata.pitch);
                    ola.SetVolume(movedata.volume);
                }
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        ola.SetLoop(movedata.isLoop);
                    }, false));
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        ola.SetMute(movedata.isMute);
                    }, false));
                }

                //---preview and just animation also-------------------
                if (options.isExecuteForDOTween == 1) seq.Join(ola.audioPlayer.DOPitch(movedata.pitch, frame.duration));
                else ola.SetPitch(movedata.pitch);

                if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => ola.GetVolume(), x => ola.SetVolume(x), movedata.volume, frame.duration));
                else ola.SetVolume(movedata.volume);

            }
            return seq;
        }
        private Sequence ParseForEffect(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;
            OperateLoadedEffect ole = naa.avatar.GetComponent<OperateLoadedEffect>();

            if (options.isExecuteForDOTween == 1)
            {
                if (movedata.animationType == AF_MOVETYPE.AnimStart)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, async () =>
                    {
                        await ole.SetEffectRef("Effects/" + movedata.effectGenre + "/" + movedata.effectName);
                        if (movedata.animLoop == 1)
                        {
                            ole.PlayEffect(1);
                            //ole.SetPlayFlagEffect(UserAnimationState.PlayWithLoop);
                        }
                        else
                        {
                            ole.PlayEffect();
                            //ole.SetPlayFlagEffect(UserAnimationState.Play);
                        }
                        ole.SetPlayFlagEffect(movedata.animPlaying);
                    }, false));
                    
                }
                else if (movedata.animationType == AF_MOVETYPE.AnimPause)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        ole.PauseEffect();
                        //ole.SetPlayFlagEffect(UserAnimationState.Pause);
                        ole.SetPlayFlagEffect(movedata.animPlaying);
                    }, false));
                    
                }
                else if (movedata.animationType == AF_MOVETYPE.AnimStop)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        ole.StopEffect();
                        //ole.SetPlayFlagEffect(UserAnimationState.Stop);
                        ole.SetPlayFlagEffect(movedata.animPlaying);

                    }, false));
                }
                else if (movedata.animationType == AF_MOVETYPE.Rest)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, async () =>
                    {
                        await ole.SetEffectRef("Effects/" + movedata.effectGenre + "/" + movedata.effectName);

                        //ole.SetPlayFlagEffect(UserAnimationState.Playing);
                        ole.SetPlayFlagEffect(movedata.animPlaying);

                    }, false));
                }
                else if (movedata.animationType == AF_MOVETYPE.Collider)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        ole.IsVRMCollider = movedata.isVRMCollider == 1 ? true : false;
                        ole.ResetColliderTarget(movedata.VRMColliderTarget);

                    }, false));
                }
                
            }
            if (options.isBuildDoTween == 0)
            {
                if (movedata.animationType == AF_MOVETYPE.Collider)
                {
                    ole.IsVRMCollider = movedata.isVRMCollider == 1 ? true : false;
                    ole.ResetColliderTarget(movedata.VRMColliderTarget);
                    ole.VRMColliderSize = movedata.VRMColliderSize;
                }
                else
                {
                    DOVirtual.DelayedCall(0.001f, async () =>
                    {
                        await ole.SetEffectRef("Effects/" + movedata.effectGenre + "/" + movedata.effectName);
                        ole.SetPlayFlagEffect(movedata.animPlaying);
                    });

                    
                }
            }


            if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => ole.VRMColliderSize, x => ole.VRMColliderSize = x, movedata.VRMColliderSize, frame.duration));
            else ole.VRMColliderSize = movedata.VRMColliderSize;

            return seq;
        }
        private Sequence ParseForSystemEffect(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;

            ManageSystemEffect mse = gameObject.GetComponent<ManageSystemEffect>();

            for (int i = 0; i < mse.ProcessNames.Length; i++)
            {
                string effectName = mse.ProcessNames[i];
                if (movedata.effectName.ToLower() == effectName.ToLower())
                {
                    bool isEnable = movedata.animationType == AF_MOVETYPE.SystemEffectOff ? false : true;
                    seq = mse.SetEffectValues(seq, effectName, movedata.effectValues, isEnable, options.isExecuteForDOTween == 1 ? true : false, frame.duration);
                }
            }


            return seq;
        }
        private Sequence ParseForStage(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;
            OperateStage os = naa.avatar.GetComponent<OperateStage>();

            //---Stage property
            if (movedata.animationType == AF_MOVETYPE.StageProperty)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    if (os.GetActiveStageType() == StageKind.Default)
                    {
                        seq = os.SetDefaultStageColorTween(seq, movedata.color, frame.duration);
                        
                    }
                    else if (os.GetActiveStageType() == StageKind.User)
                    {
                        MaterialProperties mat0 = movedata.matProp[0];
                        MaterialProperties mat1 = movedata.matProp[1];
                        seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                        {
                            if (movedata.matProp.Count > 1)
                            {
                                os.SetTextureToUserStage("main," + mat0.texturePath);
                                os.SetTextureToUserStage("normal," + mat1.texturePath);
                            }
                        }, false));
                        seq.Join(os.userStageMaterial.DOColor(mat0.color, "_Color", frame.duration));
                        seq.Join(os.userStageMaterial.DOFloat(mat0.blendmode, "_Mode", frame.duration));
                        seq.Join(os.userStageMaterial.DOFloat(mat0.metallic, "_Metallic", frame.duration));
                        seq.Join(os.userStageMaterial.DOFloat(mat0.glossiness, "_Glossiness", frame.duration));
                        seq.Join(DOVirtual.DelayedCall(frame.duration, () => os.userStageMaterial.EnableKeyword("_EMISSION"), false));
                        seq.Join(os.userStageMaterial.DOColor(mat0.emissioncolor, "_EmissionColor", frame.duration));
                        
                    }
                    else if ((os.GetActiveStageType() == StageKind.SeaDaytime) || (os.GetActiveStageType() == StageKind.SeaNight))
                    {
                        if (os.GetActiveStageMaterial() != null)
                        {
                            seq.Join(os.GetActiveStageMaterial().DOVector(movedata.wavespeed, "WaveSpeed", frame.duration));
                            /*seq.Join(DOTween.To(() => os.GetWaterWaveSpeed("x"), x => os.SetWaterWaveSpeed("x", x), movedata.wavespeed.x, frame.duration));
                            seq.Join(DOTween.To(() => os.GetWaterWaveSpeed("y"), x => os.SetWaterWaveSpeed("y", x), movedata.wavespeed.y, frame.duration));
                            seq.Join(DOTween.To(() => os.GetWaterWaveSpeed("z"), x => os.SetWaterWaveSpeed("z", x), movedata.wavespeed.z, frame.duration));
                            seq.Join(DOTween.To(() => os.GetWaterWaveSpeed("w"), x => os.SetWaterWaveSpeed("w", x), movedata.wavespeed.w, frame.duration));
                            */
                            seq.Join(os.GetActiveStageMaterial().DOFloat(movedata.wavescale, "_WaveScale", frame.duration));
                        }

                    }

                    //---wind zone
                    OperateLoadedWindzone olw = os.GetWindzone();
                    if (olw != null)
                    {
                        seq.Join(DOTween.To(() => olw.windPower, x => olw.windPower = x, movedata.windPower, frame.duration));
                        seq.Join(DOTween.To(() => olw.windFrequency, x => olw.windFrequency = x, movedata.windFrequency, frame.duration));
                        seq.Join(DOTween.To(() => olw.windDurationMin, x => olw.windDurationMin = x, movedata.windDurationMin, frame.duration));
                        seq.Join(DOTween.To(() => olw.windDurationMax, x => olw.windDurationMax = x, movedata.windDurationMax, frame.duration));
                    }

                }

                if (options.isBuildDoTween == 0)
                {
                    DOVirtual.DelayedCall(0.001f, async () =>
                    {
                        await os.SelectStageRef(movedata.stageType);
                    });
                    

                    if (os.GetActiveStageType() == StageKind.Default)
                    {
                        if (os.GetActiveStageMaterial() != null)
                        {
                            os.SetDefaultStageColor(movedata.color);
                        }
                    }
                    else if (os.GetActiveStageType() == StageKind.User)
                    {
                        os.SetMaterialToUserStage("metallic,"+movedata.userStageMetallic);
                        os.SetMaterialToUserStage("glossiness," + movedata.userStageGlossiness);
                        os.SetMaterialToUserStage("emissioncolor," + movedata.userStageEmissionColor);

                        if (movedata.matProp.Count > 1)
                        {
                            os.SetTextureToUserStage("main," + movedata.matProp[0].texturePath);
                            os.SetTextureToUserStage("normal," + movedata.matProp[1].texturePath);
                        }

                    }
                    else if ((os.GetActiveStageType() == StageKind.SeaDaytime) || (os.GetActiveStageType() == StageKind.SeaNight))
                    {

                        os.SetWaterWaveSpeed(movedata.wavespeed);
                        os.SetWaterWaveScale(movedata.wavescale);

                    }

                    //---wind zone
                    OperateLoadedWindzone olw = os.GetWindzone();
                    if (olw != null)
                    {
                        olw.windPower = movedata.windPower;
                        olw.windFrequency = movedata.windFrequency;
                        olw.windDurationMin = movedata.windDurationMin;
                        olw.windDurationMax = movedata.windDurationMax;
                    }
                }

                
            }

            //---sky
            if (movedata.animationType == AF_MOVETYPE.SkyProperty)
            {
                CameraOperation1 cam = os.GetCameraOperation();
                if (options.isBuildDoTween == 0)
                {
                    cam.SetClearFlag(movedata.skyType);

                    if (movedata.skyType == CameraClearFlags.Color)
                    {
                        cam.SetSkyColor(movedata.skyColor);
                    }
                    else if (movedata.skyType == CameraClearFlags.Skybox)
                    {
                        cam.SetSkyShader(movedata.skyShaderName);
                    }
                }

                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () => cam.SetClearFlag(movedata.skyType), false));

                    if (movedata.skyType == CameraClearFlags.Skybox)
                    {
                        seq.Join(DOVirtual.DelayedCall(frame.duration, () => cam.SetSkyShader(movedata.skyShaderName), false));

                        movedata.skyShaderFloat.ForEach(item =>
                        {
                            seq.Join(cam.skyboxMaterial.DOFloat(item.value, item.text, frame.duration));
                        });
                        movedata.skyShaderColor.ForEach(item =>
                        {
                            seq.Join(cam.skyboxMaterial.DOColor(item.value, item.text, frame.duration));
                        });
                    }
                    else if (movedata.skyType == CameraClearFlags.Color)
                    {
                        seq = cam.SetSkyColorTween(seq, movedata.skyColor, frame.duration);
                    }
                }
                else
                {
                    cam.SetClearFlag(movedata.skyType);
                    if (movedata.skyType == CameraClearFlags.Color)
                    {
                        cam.SetSkyColor(movedata.skyColor);
                    }
                    else if (movedata.skyType == CameraClearFlags.Skybox)
                    {
                        cam.SetSkyShader(movedata.skyShaderName);
                        movedata.skyShaderFloat.ForEach(item =>
                        {
                            cam.skyboxMaterial.SetFloat(item.text, item.value);
                        });
                        movedata.skyShaderColor.ForEach(item =>
                        {
                            cam.skyboxMaterial.SetColor(item.text, item.value);
                        });

                    }
                }
            }


            //---Directional Light on stage
            if (movedata.animationType == AF_MOVETYPE.LightProperty)
            {
                OperateLoadedLight oll = os.GetSystemDirectionalLight();
                GameObject dl = oll.gameObject; //GameObject.Find("Directional Light");
                GameObject dl_han = oll.relatedHandleParent;
                Light lt = dl.GetComponent<Light>();

                //---rotation(for system effect only)
                if (options.isExecuteForDOTween == 1) seq.Join(dl_han.transform.DORotate(movedata.rotation, frame.duration));
                else dl_han.transform.rotation = Quaternion.Euler(movedata.rotation);

                //---Color
                if (options.isExecuteForDOTween == 1) seq.Join(lt.DOColor(movedata.color, frame.duration));
                else oll.SetColor(movedata.color);

                //---Power
                if (options.isExecuteForDOTween == 1) seq.Join(lt.DOIntensity(movedata.power, frame.duration));
                else oll.SetPower(movedata.power);

                //---shadowStrength
                if (options.isExecuteForDOTween == 1) seq.Join(lt.DOShadowStrength(movedata.shadowStrength, frame.duration));
                else oll.SetShadowPower(movedata.shadowStrength);
            }




            return seq;
        }

//===========================================================================================================================
//  Register functions
//===========================================================================================================================
        public void RegisterFrame(string param)
        {
            if (currentProject.isReadOnly || currentProject.isSharing) return;

            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }


            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);
            AF_TARGETTYPE realtype = aro.targetType;

            if (aro.index != -1)
            {
                /*
                bool isNew = false;
                // --- Check wheather target frame is existing.
                
                int nearmin = GetNearMinFrameIndex(aro.index);

                NativeAnimationFrame nfgrp;
                if (findex == -1)
                { // --- newly register this frame ---
                    nfgrp = new NativeAnimationFrame();
                    isNew = true;

                    nfgrp.index = aro.index;
                    nfgrp.finalizeIndex = nfgrp.index;
                    nfgrp.key = "fr_" + DateTime.Now.ToFileTime().ToString();
                }
                else
                {
                    nfgrp = currentProject.timeline.tmFrame[findex]; // GetFrame(aro.index);
                }*/
                ConfirmedNativeAnimationFrame conFrame = new ConfirmedNativeAnimationFrame();

                if (aro.targetId == "")
                {
                    //---all objects--------------------------
                    foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
                    {
                        if ((actor.avatar.avatar != null))
                        {
                            int findex = GetFrameIndex(actor, aro.index);
                            //---return real index
                            int nearmin = GetNearMinFrameIndex(actor, aro.index);

                            NativeAnimationFrame fr = SaveFrameData(aro.index, nearmin, actor, aro);
                            if (findex == -1)
                            {
                                actor.frames.Add(fr);
                            }
                            else
                            {
                                //---recover remained settings
                                fr.ease = actor.frames[findex].ease;

                                actor.frames[findex] = null;
                                actor.frames[findex] = fr;
                            }
                            SortActorFrames(actor);
                            adjustNearMaxFrameIndex(actor, fr);

                            //---for confirming
                            AvatarAttachedNativeAnimationFrame aaFrame = new AvatarAttachedNativeAnimationFrame(actor);
                            aaFrame.frame.SetFromNative(fr);
                            conFrame.frames.Add(aaFrame);
                        }
                    }
                }
                else
                {
                    //---specified object--------------------
                    NativeAnimationFrameActor actor = GetFrameActorFromObjectID(aro.targetId, realtype);
                    if ((actor != null) && (actor.avatar.avatar != null))
                    {
                        int findex = GetFrameIndex(actor, aro.index);
                        //---return real index
                        int nearmin = GetNearMinFrameIndex(actor, aro.index);

                        //---always new create, and save overwritely.
                        NativeAnimationFrame fr = SaveFrameData(aro.index, nearmin, actor, aro);
                        if (findex == -1)
                        {
                            actor.frames.Add(fr);
                        }
                        else
                        {
                            //---recover remained settings
                            fr.ease = actor.frames[findex].ease;

                            actor.frames[findex] = null;
                            actor.frames[findex] = fr;
                        }
                        SortActorFrames(actor);
                        adjustNearMaxFrameIndex(actor, fr);

                        /*NativeAnimationAvatar nav = GetFrameActorFromObjectID(aro.targetId, realtype);
                        NativeAnimationFrameActor fr = SaveFrameData(aro.index, nearmin, nav, aro);
                        nfgrp.characters.Add(fr);*/
                        //---for confirming
                        AvatarAttachedNativeAnimationFrame aaFrame = new AvatarAttachedNativeAnimationFrame(actor);
                        aaFrame.frame.SetFromNative(fr);
                        conFrame.frames.Add(aaFrame);
                    }
                }

                //---auto correct frame index(finalizeIndex)
                /*if (nearmin != -1)
                {
                    nfgrp.finalizeIndex = currentProject.timeline.tmFrame[nearmin].index + 1;
                }*/

                /*
                //---Sort added frame and current frames
                if (isNew)
                {
                    currentProject.timeline.tmFrame.Add(nfgrp);
                    findex = currentProject.timeline.tmFrame.Count - 1;
                }
                

                //--- re-adjust current frame and near max frame(previous current)
                if (currentProject.timeline.tmFrame.Count > 2)
                {
                    adjustNearMaxFrameIndex(findex, nfgrp);
                }
                */
                string js = JsonUtility.ToJson(conFrame);
#if !UNITY_EDITOR && UNITY_WEBGL
        
        ReceiveStringVal(js);
        
#endif
            }
        }

        /// <summary>
        /// To change Directly a frame properties (duration, etc...)
        /// </summary>
        /// <param name="param">JSON of AnimationRegisterOptions</param>
        public void DirectModifyFrame(string param)
        {
            if (currentProject.isReadOnly || currentProject.isSharing) return;
            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }

            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);
            AF_TARGETTYPE realtype = aro.targetType;

            if (aro.targetId == "")
            {
                foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
                {
                    NativeAnimationFrame frame = actor.frames.Find(match =>
                    {
                        if (match.index == aro.index) return true;
                        return false;
                    });
                    if (frame != null)
                    {
                        frame.duration = aro.duration;
                    }
                }
            }
            else
            {
                NativeAnimationFrameActor actor = currentProject.timeline.characters.Find(match =>
                {
                    if ((match.avatar.avatarId == aro.targetId) && (match.avatar.type == aro.targetType)) return true;
                    return false;
                });
                if (actor != null)
                {
                    NativeAnimationFrame frame = actor.frames.Find(match =>
                    {
                        if (match.index == aro.index) return true;
                        return false;
                    });
                    if (frame != null)
                    {
                        frame.duration = aro.duration;
                    }
                }
            }


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param">csv string - [0] avatar id, [1] avatar type, [2] old frame index, [3] new frame index</param>
        public void ChangeFramePosition(string param)
        {
            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }

            bool isSuccess = true;
            int ret = -1;

            string[] prm = param.Split(',');
            string id = prm[0];
            int tmpi = int.TryParse(prm[1], out tmpi) ? tmpi : -1;
            if (tmpi == -1)
            {
                isSuccess = false;
            }
            AF_TARGETTYPE type = (AF_TARGETTYPE)tmpi;
            int oldindex = int.TryParse(prm[2], out oldindex) ? oldindex : -1;
            int newindex = int.TryParse(prm[3], out newindex) ? newindex : -1;
            if (oldindex == -1) isSuccess = false;
            if (newindex == -1) isSuccess = false;

            if (isSuccess)
            {
                //---specified object--------------------
                NativeAnimationFrameActor actor = GetFrameActorFromObjectID(id, type);
                int findex = GetFrameIndex(actor, oldindex);
                int new_findex = GetFrameIndex(actor, newindex);
                if (new_findex == -1)
                { //not exist frame of newindex
                    actor.frames[findex].index = newindex;
                    SortActorFrames(actor);
                    int nearmin = GetNearMinFrameIndex(actor, newindex);
                    if (nearmin > -1)
                    {
                        int dist = newindex - (actor.frames[nearmin].index + 1);
                        actor.frames[findex].finalizeIndex = actor.frames[nearmin].index + 1;
                        //actor.frames[findex].duration = (currentProject.baseDuration == 0f ? 0.01f : currentProject.baseDuration) * (float)dist;

                        //---To adjust all frame duration and finalIndex of the actor
                        AdjustAllFrame(actor, currentProject.baseDuration, true, true);
                        
                    }
                    adjustNearMaxFrameIndex(actor, actor.frames[findex]);

                    ret = findex;
                }
                
            }
            
            //---returning value is an index of the Array ( not frame number )
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret);
#endif
        }
        public void UnregisterFrame(string param)
        {
            if (currentProject.isReadOnly || currentProject.isSharing) return;
            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }

            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);
            AF_TARGETTYPE realtype = aro.targetType;
            if (aro.index != -1)
            {
                if (aro.targetId == "")
                { //---all characters
                    foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
                    {
                        NativeAnimationFrame frame = actor.frames.Find(match =>
                        {
                            if (match.index == aro.index) return true;
                            return false;
                        });
                        if (frame != null)
                        {
                            actor.frames.Remove(frame);
                        }
                        //---adjust duration and finalIndex
                        AdjustAllFrame(actor, currentProject.baseDuration, true, true);
                    }
                }
                else
                { //--- indicated character only
                    NativeAnimationFrameActor actor = GetFrameActorFromObjectID(aro.targetId, aro.targetType);
                    if (actor != null)
                    {
                        NativeAnimationFrame frame = actor.frames.Find(match =>
                        {
                            if (match.index == aro.index) return true;
                            return false;
                        });
                        if (frame != null)
                        {
                            actor.frames.Remove(frame);
                        }
                        //---adjust duration and finalIndex
                        AdjustAllFrame(actor, currentProject.baseDuration, true, true);


                    }
                }

            }
        }
        public NativeAnimationFrame SaveFrameData(int frameNumber, int nearMinIndex, NativeAnimationFrameActor actor, AnimationRegisterOptions options)
        {
            //---Save common information for each object
            NativeAnimationFrame aframe = new NativeAnimationFrame();
            aframe.movingData = new List<AnimationTargetParts>();
            aframe.index = frameNumber;
            aframe.finalizeIndex = frameNumber;
            aframe.ease = options.ease;

            //---update bodyInfoList current avatar body info.
            aframe.useBodyInfo = UseBodyInfoType.CurrentAvatar;

            /*actor.bodyInfoList.Clear();
            OperateLoadedVRM ovrm = actor.avatar.avatar.GetComponent<OperateLoadedVRM>();
            float[] heights = actor.avatar.avatar.GetComponent<ManageAvatarTransform>().ParseBodyInfo(ovrm.GetTPoseBodyInfo());
            List <Vector3> list = ovrm.GetTPoseBodyList();

            Array.Copy(heights, actor.bodyHeight, heights.Length);
            foreach (Vector3 v in list)
            {
                actor.bodyInfoList.Add(new Vector3(v.x, v.y, v.z));
            }*/



            //int[] dist = GetDistanceFromPreviousFrame(frameNumber);
            NativeAnimationFrame oldframe = GetFrame(actor, frameNumber);

            //---calculate baseDuration * distance of previous Frame and current Frame.
            int dist = 1;
            if ((actor.frames.Count > 0) && (nearMinIndex != -1))
            {
                dist = frameNumber - (actor.frames[nearMinIndex].index);
            }

            aframe.duration = (currentProject.baseDuration == 0f ? 0.01f : currentProject.baseDuration) * (float)dist;
            if (nearMinIndex != -1)
            {
                aframe.finalizeIndex = actor.frames[nearMinIndex].finalizeIndex;
            }


            //aframe.enabled = 1;


            if (actor.targetType == AF_TARGETTYPE.SystemEffect)
            {
                aframe = PackForSystemEffect(aframe, actor, options, oldframe);
            }
            else if (actor.targetType == AF_TARGETTYPE.Audio)
            {
                aframe = PackForAudio(aframe, actor, options, oldframe);
            }
            else
            {
                //---Each save process for each object
                aframe = PackForCommon(aframe, actor, options, oldframe);
                if (actor.targetType == AF_TARGETTYPE.VRM)
                {
                    aframe = PackForVRM(aframe, actor, options, oldframe);
                }
                else if (actor.targetType == AF_TARGETTYPE.OtherObject)
                {
                    aframe = PackForOtherObject(aframe, actor, options, oldframe);
                }
                else if (actor.targetType == AF_TARGETTYPE.Light)
                {
                    aframe = PackForLight(aframe, actor, options, oldframe);
                }
                else if (actor.targetType == AF_TARGETTYPE.Camera)
                {
                    aframe = PackForCamera(aframe, actor, options, oldframe);
                }
                else if (actor.targetType == AF_TARGETTYPE.Text)
                {
                    aframe = PackForText(aframe, actor, options, oldframe);
                }
                else if (actor.targetType == AF_TARGETTYPE.Image)
                {
                    aframe = PackForImage(aframe, actor, options, oldframe);
                }
                else if (actor.targetType == AF_TARGETTYPE.UImage)
                {
                    aframe = PackForUImage(aframe, actor, options, oldframe);
                }
                else if (actor.targetType == AF_TARGETTYPE.Effect)
                {
                    aframe = PackForEffect(aframe, actor, options, oldframe);
                }
                else if (actor.targetType == AF_TARGETTYPE.Stage)
                {
                    aframe = PackForStage(aframe, actor, options, oldframe);
                }


            }


            return aframe;

        }
        private NativeAnimationFrame PackForCommon(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            if (nav.targetType == AF_TARGETTYPE.VRM)
            {
                if ((options.isBlendShapeOnly == 0) && (options.isHandOnly == 0) && (options.isTransformOnly == 0))
                {
                    OperateLoadedBase olb = nav.avatar.avatar.GetComponent<OperateLoadedBase>();

                    //---most common parts
                    AnimationTargetParts[] ikp = new AnimationTargetParts[3];
                    ikp[0] = new AnimationTargetParts();
                    ikp[0].animationType = AF_MOVETYPE.Translate;
                    ikp[0].vrmBone = ParseIKBoneType.IKParent;
                    ikp[0].position = nav.avatar.ikparent.transform.position;
                    //------position only: jump parts
                    ikp[0].jumpNum = olb.GetJumpNum();
                    ikp[0].jumpPower = olb.GetJumpPower();
                    ikp[1] = new AnimationTargetParts();
                    ikp[1].animationType = AF_MOVETYPE.Rotate;
                    ikp[1].vrmBone = ParseIKBoneType.IKParent;
                    ikp[1].rotation = nav.avatar.ikparent.transform.rotation.eulerAngles;
                    ikp[2] = new AnimationTargetParts();
                    ikp[2].animationType = AF_MOVETYPE.Scale;
                    ikp[2].vrmBone = ParseIKBoneType.IKParent;
                    ikp[2].scale = nav.avatar.avatar.transform.localScale;

                    frame.movingData.Add(ikp[0]);
                    frame.movingData.Add(ikp[1]);
                    frame.movingData.Add(ikp[2]);

                    //---common effect parts
                    if ((olb.GetPunch() != null) && olb.GetPunch().isEnable == 1)
                    {
                        AnimationTargetParts pp = new AnimationTargetParts();
                        pp.animationType = AF_MOVETYPE.Punch;
                        pp.vrmBone = ParseIKBoneType.IKParent;
                        pp.effectPunch = olb.GetPunch();
                        frame.movingData.Add(pp);
                    }
                    if ((olb.GetShake() != null) && olb.GetShake().isEnable == 1)
                    {
                        AnimationTargetParts pp = new AnimationTargetParts();
                        pp.animationType = AF_MOVETYPE.Shake;
                        pp.vrmBone = ParseIKBoneType.IKParent;
                        pp.effectShake = olb.GetShake();
                        frame.movingData.Add(pp);
                    }


                    //---Transform for HumanBodyBones------------------------------------------
                    if (options.isCompileForLibrary == 1) 
                    {
                        Animator animator = nav.avatar.avatar.GetComponent<Animator>();
                        foreach (HumanBodyBones hBone in Enum.GetValues(typeof(HumanBodyBones)))
                        {
                            if (hBone != HumanBodyBones.LastBone)
                            {
                                Transform boneTran = animator.GetBoneTransform(hBone);

                                if (boneTran != null)
                                {
                                    AnimationTargetParts[] atp = new AnimationTargetParts[3];
                                    atp[0] = new AnimationTargetParts();
                                    atp[1] = new AnimationTargetParts();
                                    atp[0].vrmBone = ParseIKBoneType.UseHumanBodyBones;
                                    atp[1].vrmBone = ParseIKBoneType.UseHumanBodyBones;
                                    atp[0].vrmHumanBodyBone = hBone;
                                    atp[1].vrmHumanBodyBone = hBone;
                                    atp[0].animationType = AF_MOVETYPE.Rotate;
                                    atp[1].animationType = AF_MOVETYPE.Scale;
                                    atp[0].rotation = boneTran.localRotation.eulerAngles;
                                    atp[1].scale = boneTran.localScale;

                                    frame.movingData.Add(atp[0]);
                                    frame.movingData.Add(atp[1]);
                                }
                            }
                        }
                    }
                    


                    //---Transform for IK----------------------------------------------
                    int[] sortedIndex = new int[IKbonesCount] {
                        (int)ParseIKBoneType.IKParent,


                        (int)ParseIKBoneType.Pelvis,(int)ParseIKBoneType.Chest,
                        (int)ParseIKBoneType.Head,(int)ParseIKBoneType.Aim,(int)ParseIKBoneType.LookAt, 

                        (int)ParseIKBoneType.LeftHand, (int)ParseIKBoneType.LeftLowerArm,
                        (int)ParseIKBoneType.RightHand, (int)ParseIKBoneType.RightLowerArm,

                        (int)ParseIKBoneType.LeftLeg, (int)ParseIKBoneType.LeftLowerLeg,
                        (int)ParseIKBoneType.RightLeg,(int)ParseIKBoneType.RightLowerLeg,
                        (int)ParseIKBoneType.EyeViewHandle
                        
                    };

                    for (int srti = 1; srti < sortedIndex.Length; srti++)
                    {
                        int i = sortedIndex[srti];
                        string ikname = IKBoneNames[i];
                        Transform child = nav.avatar.ikparent.transform.Find(ikname);

                        AnimationTargetParts[] atp = new AnimationTargetParts[3];
                        atp[0] = new AnimationTargetParts();
                        atp[1] = new AnimationTargetParts();
                        atp[2] = new AnimationTargetParts();
                        atp[0].vrmBone = (ParseIKBoneType)i;
                        atp[1].vrmBone = (ParseIKBoneType)i;
                        atp[2].vrmBone = (ParseIKBoneType)i;
                        atp[0].animationType = AF_MOVETYPE.Translate;
                        atp[1].animationType = AF_MOVETYPE.Rotate;
                        atp[2].animationType = AF_MOVETYPE.Scale;
                        atp[0].position = child.localPosition;
                        atp[1].rotation = child.localRotation.eulerAngles;
                        atp[2].scale = child.localScale;

                        frame.movingData.Add(atp[0]);
                        frame.movingData.Add(atp[1]);
                        frame.movingData.Add(atp[2]);
                    }


                }

            }
            else if ((nav.targetType == AF_TARGETTYPE.Text) || (nav.targetType == AF_TARGETTYPE.UImage))
            {
                //---transform is RectTransform
                RectTransform rectt = nav.avatar.avatar.GetComponent<RectTransform>();
                OperateLoadedUImage olu = nav.avatar.avatar.GetComponent<OperateLoadedUImage>();
                Vector2 v2 = olu.GetPosition();

                AnimationTargetParts[] ikp = new AnimationTargetParts[3];
                ikp[0] = new AnimationTargetParts();
                ikp[0].animationType = AF_MOVETYPE.Translate;
                ikp[0].vrmBone = ParseIKBoneType.IKParent;
                ikp[0].position = new Vector3(v2.x, v2.y, 0f); // rectt.anchoredPosition3D;
                frame.movingData.Add(ikp[0]);

                ikp[1] = new AnimationTargetParts();
                ikp[1].animationType = AF_MOVETYPE.Rotate;
                ikp[1].vrmBone = ParseIKBoneType.IKParent;
                Vector3 rot2d = Vector3.zero;
                rot2d.z = rectt.rotation.eulerAngles.z;
                ikp[1].rotation = rot2d;
                frame.movingData.Add(ikp[1]);

                if ((nav.avatar.type == AF_TARGETTYPE.OtherObject) || (nav.avatar.type == AF_TARGETTYPE.Image))
                {
                    ikp[2] = new AnimationTargetParts();
                    ikp[2].animationType = AF_MOVETYPE.Scale;
                    ikp[2].vrmBone = ParseIKBoneType.IKParent;
                    ikp[2].scale = rectt.sizeDelta;
                    frame.movingData.Add(ikp[2]);
                }
            }
            else if (nav.targetType == AF_TARGETTYPE.Stage)
            {
                OperateStage os = nav.avatar.avatar.GetComponent<OperateStage>();

                //---Here is other of VRM, RectTransform, Audio, SystemEffect
                AnimationTargetParts[] ikp = new AnimationTargetParts[3];
                ikp[0] = new AnimationTargetParts();
                ikp[0].animationType = AF_MOVETYPE.Translate;
                ikp[0].vrmBone = ParseIKBoneType.IKParent;
                ikp[0].position = os.GetPositionFromOuter(0);
                frame.movingData.Add(ikp[0]);

                ikp[1] = new AnimationTargetParts();
                ikp[1].animationType = AF_MOVETYPE.Rotate;
                ikp[1].vrmBone = ParseIKBoneType.IKParent;
                ikp[1].rotation = os.GetRotationFromOuter(0);
                frame.movingData.Add(ikp[1]);

                ikp[2] = new AnimationTargetParts();
                ikp[2].animationType = AF_MOVETYPE.Scale;
                ikp[2].vrmBone = ParseIKBoneType.IKParent;
                ikp[2].scale = os.GetScale(0);
                frame.movingData.Add(ikp[2]);

            }
            else
            {
                OperateLoadedBase olb = nav.avatar.avatar.GetComponent<OperateLoadedBase>();
                bool isEquip = false;
                if (olb != null)
                {
                    OtherObjectDummyIK ooik = olb.relatedHandleParent.GetComponent<OtherObjectDummyIK>();
                    isEquip = ooik.isEquipping;
                }

                if (!isEquip)
                { //---Enable transfroming without equipped status
                    //---Here is other of VRM, RectTransform, Audio, SystemEffect
                    AnimationTargetParts[] ikp = new AnimationTargetParts[3];
                    ikp[0] = new AnimationTargetParts();
                    ikp[0].animationType = AF_MOVETYPE.Translate;
                    ikp[0].vrmBone = ParseIKBoneType.IKParent;
                    ikp[0].position = nav.avatar.ikparent.transform.position;
                    //------position only: jump parts
                    ikp[0].jumpNum = olb.GetJumpNum();
                    ikp[0].jumpPower = olb.GetJumpPower();
                    frame.movingData.Add(ikp[0]);

                    ikp[1] = new AnimationTargetParts();
                    ikp[1].animationType = AF_MOVETYPE.Rotate;
                    ikp[1].vrmBone = ParseIKBoneType.IKParent;
                    ikp[1].rotation = nav.avatar.ikparent.transform.rotation.eulerAngles;
                    frame.movingData.Add(ikp[1]);

                    if ((nav.avatar.type == AF_TARGETTYPE.OtherObject) || (nav.avatar.type == AF_TARGETTYPE.Image))
                    {
                        ikp[2] = new AnimationTargetParts();
                        ikp[2].animationType = AF_MOVETYPE.Scale;
                        ikp[2].vrmBone = ParseIKBoneType.IKParent;
                        ikp[2].scale = nav.avatar.avatar.transform.localScale;
                        frame.movingData.Add(ikp[2]);
                    }

                    //---common effect parts
                    if ((olb.GetPunch() != null) && olb.GetPunch().isEnable == 1)
                    {
                        AnimationTargetParts pp = new AnimationTargetParts();
                        pp.animationType = AF_MOVETYPE.Punch;
                        pp.vrmBone = ParseIKBoneType.IKParent;
                        pp.effectPunch = olb.GetPunch();
                        frame.movingData.Add(pp);
                    }
                    if ((olb.GetShake() != null) && olb.GetShake().isEnable == 1)
                    {
                        AnimationTargetParts pp = new AnimationTargetParts();
                        pp.animationType = AF_MOVETYPE.Shake;
                        pp.vrmBone = ParseIKBoneType.IKParent;
                        pp.effectShake = olb.GetShake();
                        frame.movingData.Add(pp);
                    }

                }



            }

            return frame;
        }
        private NativeAnimationFrame PackForVRM(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            OperateLoadedVRM ovrm = nav.avatar.avatar.GetComponent<OperateLoadedVRM>();

            //---handpose
            if ((options.isBlendShapeOnly != 1) && (options.isCompileForLibrary != 1))
            {
                LeftHandPoseController lhand = nav.avatar.avatar.GetComponent<LeftHandPoseController>();
                RightHandPoseController rhand = nav.avatar.avatar.GetComponent<RightHandPoseController>();

                AnimationTargetParts[] athand = new AnimationTargetParts[2];
                athand[0] = new AnimationTargetParts();
                athand[0].animationType = AF_MOVETYPE.NormalTransform;
                athand[0].vrmBone = ParseIKBoneType.LeftHandPose;
                athand[0].isHandPose = 1;
                athand[0].handpose.Add((float)lhand.currentPose);
                athand[0].handpose.Add(lhand.handPoseValue);

                athand[1] = new AnimationTargetParts();
                athand[1].animationType = AF_MOVETYPE.NormalTransform;
                athand[1].vrmBone = ParseIKBoneType.RightHandPose;
                athand[1].isHandPose = 1;
                athand[1].handpose.Add((float)rhand.currentPose);
                athand[1].handpose.Add(rhand.handPoseValue);

                frame.movingData.Add(athand[0]);
                frame.movingData.Add(athand[1]);

            }


            //---blendshape
            if (options.isHandOnly != 1)
            {
                GameObject mainface = nav.avatar.avatar.GetComponent<ManageAvatarTransform>().GetFaceMesh();
                SkinnedMeshRenderer face = mainface.GetComponent<SkinnedMeshRenderer>();
                List<BasicStringFloatList> blst = new List<BasicStringFloatList>();


                AnimationTargetParts atblendshape = new AnimationTargetParts();
                atblendshape.animationType = AF_MOVETYPE.BlendShape;
                atblendshape.vrmBone = ParseIKBoneType.BlendShape;
                atblendshape.isBlendShape = 1;

                int bscnt = face.sharedMesh.blendShapeCount;
                for (int i = 0; i < bscnt; i++)
                {
                    atblendshape.blendshapes.Add(new BasicStringFloatList(face.sharedMesh.GetBlendShapeName(i), face.GetBlendShapeWeight(i)));

                }

                //---blink
                AnimationTargetParts atblink = new AnimationTargetParts();
                atblink.animationType = AF_MOVETYPE.VRMBlink;
                atblink.vrmBone = ParseIKBoneType.BlendShape;
                atblink.isblink = ovrm.GetBlinkFlag();
                atblink.interval = ovrm.GetBlinkInterval();
                atblink.openingSeconds = ovrm.GetBlinkOpeningSeconds();
                atblink.closeSeconds = ovrm.GetBlinkCloseSeconds();
                atblink.closingTime = ovrm.GetBlinkClosingTime();

                frame.movingData.Add(atblendshape);
                frame.movingData.Add(atblink);

            }
            
            
            if ((options.isBlendShapeOnly == 0) && (options.isHandOnly == 0))
            {
                //---equipment
                AnimationTargetParts atequip = new AnimationTargetParts();
                atequip.equipType = ovrm.GetEquipFlag();
                atequip.animationType = AF_MOVETYPE.Equipment;

                //---equip / unequip
                List<AvatarEquipSaveClass> lst = ovrm.GetEquipmentInformation();
                lst.ForEach(match =>
                {
                    AvatarEquipSaveClass aes = new AvatarEquipSaveClass();
                    aes.bodybonename = match.bodybonename;
                    aes.equipitem = match.equipitem;
                    aes.position = match.position;
                    aes.rotation = match.rotation;
                    atequip.equipDestinations.Add(aes);
                });
                frame.movingData.Add(atequip);


                //---gravity info
                AnimationTargetParts atgravity = new AnimationTargetParts();
                atgravity.animationType = AF_MOVETYPE.GravityProperty;
                //---renewal the list, and apply it to key-frame.
                ovrm.ListGravityInfo();
                ovrm.gravityList.list.ForEach(item =>
                {
                    atgravity.gravity.list.Add(new VRMGravityInfo(item.comment, item.rootBoneName, item.power, item.dir.x, item.dir.y, item.dir.z));
                });
                frame.movingData.Add(atgravity);

                //---special IK handles
                if (ovrm.ikMappingList.Count > 0)
                {
                    AnimationTargetParts atikhandle = new AnimationTargetParts();
                    atikhandle.animationType = AF_MOVETYPE.VRMIKProperty;
                    atikhandle.handleList = new List<AvatarIKMappingClass>(ovrm.ikMappingList);
                    frame.movingData.Add(atikhandle);
                }

                //---Materials

                AnimationTargetParts atmat = new AnimationTargetParts();
                atmat.animationType = AF_MOVETYPE.ObjectTexture;
                atmat.matProp = ovrm.ListUserMaterialObject();
                /*
                atmat.vmatProp.dstblend = ovrm.userSharedProperties.dstblend;
                atmat.vmatProp.srcblend = ovrm.userSharedProperties.srcblend;
                atmat.vmatProp.color = ovrm.userSharedProperties.color;
                atmat.vmatProp.cullmode = ovrm.userSharedProperties.cullmode;
                atmat.vmatProp.blendmode = ovrm.userSharedProperties.blendmode;
                atmat.vmatProp.emissioncolor = ovrm.userSharedProperties.emissioncolor;
                atmat.vmatProp.shadetexcolor = ovrm.userSharedProperties.shadetexcolor;
                atmat.vmatProp.shadingtoony = ovrm.userSharedProperties.shadingtoony;
                atmat.vmatProp.rimcolor = ovrm.userSharedProperties.rimcolor;
                atmat.vmatProp.rimfresnel = ovrm.userSharedProperties.rimfresnel;
                */
                if (atmat.matProp.Count > 0) frame.movingData.Add(atmat);


            }


            return frame;
        }
        private NativeAnimationFrame PackForOtherObject(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {


            OperateLoadedOther olo = nav.avatar.avatar.GetComponent<OperateLoadedOther>();
            //---save the animation
            if (olo.GetSystemAnimationType(0) != "")
            {
                if (options.isDefineOnly != 1)
                {
                    AnimationTargetParts atobj2 = new AnimationTargetParts();
                        
                    atobj2.animationType = AF_MOVETYPE.AnimProperty;
                    atobj2.animSpeed = olo.GetSpeedAnimation();
                    atobj2.animName = olo.GetTargetClip();
                    atobj2.animLoop = olo.GetWrapMode();
                        
                    //frame.movingData.Add(atobj2);
                }

                if (options.isPropertyOnly != 1)
                {

                    AnimationTargetParts atobj = new AnimationTargetParts();

                    atobj.animSpeed = olo.GetSpeedAnimation();
                    atobj.animName = olo.GetTargetClip();
                    atobj.animLoop = olo.GetWrapMode();

                    atobj.animPlaying = olo.GetPlayFlagAnimation();
                    atobj.animSeek = olo.GetSeekPosAnimation();
                    if (olo.GetPlayFlagAnimation() == UserAnimationState.Play)
                    {
                        atobj.animationType = AF_MOVETYPE.AnimStart;
                    }
                    else if (olo.GetPlayFlagAnimation() == UserAnimationState.Stop)
                    {
                        atobj.animationType = AF_MOVETYPE.AnimStop;
                    }
                    else if (olo.GetPlayFlagAnimation() == UserAnimationState.Seeking)
                    {
                        atobj.animationType = AF_MOVETYPE.AnimSeek;
                        
                    }
                    else if (olo.GetPlayFlagAnimation() == UserAnimationState.Pause)
                    {
                        atobj.animationType = AF_MOVETYPE.AnimPause;
                    }
                    else if (olo.GetPlayFlagAnimation() == UserAnimationState.Playing)
                    {
                        atobj.animationType = AF_MOVETYPE.Rest;
                    }
                    frame.movingData.Add(atobj);
                }


            }

            //---Materials
            
            AnimationTargetParts atobj3 = new AnimationTargetParts();
            atobj3.animationType = AF_MOVETYPE.ObjectTexture;
            atobj3.matProp = olo.ListUserMaterialObject();
            
            if (atobj3.matProp.Count > 0) frame.movingData.Add(atobj3);
            


            return frame;
        }
        private NativeAnimationFrame PackForLight(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            AnimationTargetParts atlight = new AnimationTargetParts();
            Light lt = nav.avatar.avatar.GetComponent<Light>();
            atlight.animationType = AF_MOVETYPE.LightProperty;

            atlight.lightType = lt.type;
            atlight.range = lt.range;
            atlight.power = lt.intensity;
            atlight.spotAngle = lt.spotAngle;
            atlight.color = lt.color;
            atlight.lightRenderMode = lt.renderMode;

            frame.movingData.Add(atlight);

            return frame;
        }
        private NativeAnimationFrame PackForCamera(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {

            OperateLoadedCamera olc = nav.avatar.avatar.GetComponent<OperateLoadedCamera>();
            Camera cam = nav.avatar.avatar.GetComponent<Camera>();
            if (options.isPropertyOnly != 1)
            {
                AnimationTargetParts atcam1 = new AnimationTargetParts();

                if (olc.GetCameraPlaying() == UserAnimationState.Play)
                {
                    atcam1.animationType = AF_MOVETYPE.CameraOn;
                }
                else if (olc.GetCameraPlaying() == UserAnimationState.Playing)
                {
                    atcam1.animationType = AF_MOVETYPE.Camera;
                }
                else if (olc.GetCameraPlaying() == UserAnimationState.Stop)
                {
                    atcam1.animationType = AF_MOVETYPE.CameraOff;
                }
                atcam1.cameraPlaying = (int)olc.GetCameraPlaying();
                frame.movingData.Add(atcam1);
            }
            if (options.isDefineOnly != 1)
            {
                AnimationTargetParts atcam2 = new AnimationTargetParts();
                atcam2.animationType = AF_MOVETYPE.CameraProperty;
                atcam2.clearFlag = (int)cam.clearFlags;
                atcam2.color = cam.backgroundColor;
                atcam2.fov = cam.fieldOfView;
                atcam2.depth = cam.depth;
                atcam2.viewport = cam.rect;

                //---render texture : Camera SIDE
                atcam2.renderFlag = olc.GetCameraRenderFlag();
                if (olc.GetCameraRenderFlag() == 1)
                {
                    RenderTexture rt = olc.GetRenderTexture();
                    if (rt == null)
                    {
                        atcam2.renderTex.x = olc.RenderSize.x;
                        atcam2.renderTex.y = olc.RenderSize.y;
                    }
                    else
                    {
                        atcam2.renderTex.x = rt.width;
                        atcam2.renderTex.y = rt.height;
                    }

                }
                else
                {
                    atcam2.renderTex.x = -1f;
                    atcam2.renderTex.y = -1f;
                }
                frame.movingData.Add(atcam2);
            }



            return frame;
        }
        private NativeAnimationFrame PackForText(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            Text text = nav.avatar.avatar.GetComponent<Text>();

            if (options.isPropertyOnly != 1)
            {
                AnimationTargetParts attext = new AnimationTargetParts();
                attext.text = text.text;
                attext.animationType = AF_MOVETYPE.Text;

                frame.movingData.Add(attext);
            }

            if (options.isDefineOnly != 1)
            {
                AnimationTargetParts atprop = new AnimationTargetParts();
                atprop.animationType = AF_MOVETYPE.TextProperty;
                atprop.fontSize = text.fontSize;
                atprop.fontStyle = text.fontStyle;
                atprop.color = text.color;
                frame.movingData.Add(atprop);

            }


            return frame;
        }
        private NativeAnimationFrame PackForImage(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            AnimationTargetParts atp = new AnimationTargetParts();
            OperateLoadedOther olo = nav.avatar.avatar.GetComponent<OperateLoadedOther>();
            //GameObject imgobj = olo.GetEffectiveObject();

            atp.animationType = AF_MOVETYPE.ImageProperty;
            atp.color = olo.GetBaseColor();

            frame.movingData.Add(atp);

            return frame;
        }
        private NativeAnimationFrame PackForUImage(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            AnimationTargetParts atp = new AnimationTargetParts();
            OperateLoadedUImage olo = nav.avatar.avatar.GetComponent<OperateLoadedUImage>();


            atp.animationType = AF_MOVETYPE.ImageProperty;
            atp.color = olo.GetImageBaseColor();

            frame.movingData.Add(atp);

            return frame;
        }
        private NativeAnimationFrame PackForAudio(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            OperateLoadedAudio ola = nav.avatar.avatar.GetComponent<OperateLoadedAudio>();

            if (options.isPropertyOnly != 1)
            {
                AnimationTargetParts atp = new AnimationTargetParts();
                atp.audioName = ola.targetAudioName;

                atp.isSE = ola.GetIsSE() ? 1 : 0;


                atp.seekTime = ola.GetSeekSeconds();
                atp.animPlaying = ola.GetPlayFlag();

                if (ola.GetPlayFlag() == UserAnimationState.Stop)  //---stop
                {
                    atp.animationType = AF_MOVETYPE.AnimStop;
                }
                else if (ola.GetPlayFlag() == UserAnimationState.Play) //---play
                {
                    atp.animationType = AF_MOVETYPE.AnimStart;
                }
                else if (ola.GetPlayFlag() == UserAnimationState.Pause) //---pause
                {
                    atp.animationType = AF_MOVETYPE.AnimPause;
                }
                else if (ola.GetPlayFlag() == UserAnimationState.Playing) //---playing
                {
                    atp.animationType = AF_MOVETYPE.Rest;
                }
                else if (ola.GetPlayFlag() == UserAnimationState.Seeking)//---seek
                {
                    atp.animationType = AF_MOVETYPE.AnimSeek;
                }
                frame.movingData.Add(atp);
            }


            if (options.isDefineOnly != 1)
            {
                AnimationTargetParts atprop = new AnimationTargetParts();
                atprop.animationType = AF_MOVETYPE.AudioProperty;
                atprop.audioName = ola.targetAudioName;
                atprop.isSE = ola.GetIsSE() ? 1 : 0;

                atprop.isLoop = ola.GetLoop();
                atprop.isMute = ola.GetMute();
                atprop.pitch = ola.GetPitch();
                atprop.volume = ola.GetVolume();

                frame.movingData.Add(atprop);

            }




            return frame;
        }
        private NativeAnimationFrame PackForEffect(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            //AnimationTargetParts atobj = new AnimationTargetParts();

            OperateLoadedEffect ole = nav.avatar.avatar.GetComponent<OperateLoadedEffect>();

            if (ole.IsVRMCollider)
            { //---Effect is for VRM Collider
                AnimationTargetParts atobj_col = new AnimationTargetParts();
                atobj_col.animationType = AF_MOVETYPE.Collider;
                atobj_col.isVRMCollider = ole.IsVRMCollider ? 1 : 0;
                atobj_col.VRMColliderSize = ole.VRMColliderSize;
                atobj_col.VRMColliderTarget = ole.EnumColliderTarget();

                frame.movingData.Add(atobj_col);
            }
            else
            { //---Effect is normal animation effect
                //---save the animation
                if (ole.targetEffect != null)
                {
                    AnimationTargetParts atobj = new AnimationTargetParts();
                    atobj.animPlaying = ole.GetPlayFlagEffect(0);
                    EffectCurrentStates ecs = ole.GetCurrentEffect();
                    atobj.effectGenre = ecs.genre; //nav.avatar.avatar.transform.parent.name;
                    atobj.effectName = ecs.effectName; //ole.targetEffect.name;

                    if (atobj.animPlaying == UserAnimationState.Play)
                    { //single play
                        atobj.animationType = AF_MOVETYPE.AnimStart;
                        atobj.animLoop = 0;

                    }
                    else if (atobj.animPlaying == UserAnimationState.PlayWithLoop)
                    { //play with loop
                        atobj.animationType = AF_MOVETYPE.AnimStart;

                        atobj.animLoop = 1;
                    }
                    else if (atobj.animPlaying == UserAnimationState.Pause)
                    {
                        atobj.animationType = AF_MOVETYPE.AnimPause;

                    }
                    else if (atobj.animPlaying == UserAnimationState.Stop)
                    {
                        atobj.animationType = AF_MOVETYPE.AnimStop;
                        atobj.animLoop = -1;
                    }
                    else if (atobj.animPlaying == UserAnimationState.Playing)
                    {
                        atobj.animationType = AF_MOVETYPE.Rest;

                    }
                    frame.movingData.Add(atobj);
                }
            }

            
            return frame;
        }
        private NativeAnimationFrame PackForSystemEffect(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            ManageSystemEffect mse = nav.avatar.avatar.GetComponent<ManageSystemEffect>();

            for (int i = 0; i < mse.ProcessNames.Length; i++)
            {
                AnimationTargetParts ateff = new AnimationTargetParts();

                string processname = mse.ProcessNames[i];
                int ena = mse.GetEnablePostProcessing(processname + ",0");

                ateff.effectName = processname;
                if (ena == 1)
                {
                    ateff.effectValues = mse.PackEffectValue(processname);
                    ateff.animationType = AF_MOVETYPE.SystemEffect;
                }
                else
                {
                    ateff.animationType = AF_MOVETYPE.SystemEffectOff;
                }


                frame.movingData.Add(ateff);
            }



            return frame;
        }
        private NativeAnimationFrame PackForStage(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            OperateStage os = nav.avatar.avatar.GetComponent<OperateStage>();

            AnimationTargetParts atp = new AnimationTargetParts();
            atp.animationType = AF_MOVETYPE.StageProperty;

            atp.stageType = (int)os.GetActiveStageType();

            atp.color = os.GetDefaultStageColor(0);

            atp.wavescale = os.GetWaterWaveScale(0);

            atp.wavespeed = os.GetWaterWaveSpeedFromOuter(0);

            //---user stage properties
            if (os.GetActiveStageType() == StageKind.User)
            {
                //atp.userStageMetallic = os.GetFloatUserStage("metallic");
                //atp.userStageGlossiness = os.GetFloatUserStage("glossiness");
                //atp.userStageEmissionColor = os.GetColorUserStage("emissioncolor");

                MaterialProperties matmain = new MaterialProperties();
                matmain.color = os.GetColorUserStage("color");
                matmain.blendmode = os.GetFloatUserStage("renderingtype");
                matmain.metallic = os.GetFloatUserStage("metallic");
                matmain.glossiness = os.GetFloatUserStage("glossiness");
                matmain.emissioncolor = os.GetColorUserStage("emissioncolor");
                matmain.texturePath = os.ActiveUserStageMainTextureName;
                atp.matProp.Add(matmain);

                MaterialProperties matnormal = new MaterialProperties();
                matnormal.texturePath = os.ActiveUserStageBumpmapTextureName;
                atp.matProp.Add(matnormal);
            }        

            //---Windzone
            OperateLoadedWindzone olw = os.GetWindzone();
            if (olw != null)
            {
                atp.windPower = olw.windPower;
                atp.windFrequency = olw.windFrequency;
                atp.windDurationMin = olw.windDurationMin;
                atp.windDurationMax = olw.windDurationMax;
            }

            frame.movingData.Add(atp);

            //---sky
            CameraOperation1 cam = os.GetCameraOperation();
            AnimationTargetParts atsky = new AnimationTargetParts();
            atsky.animationType = AF_MOVETYPE.SkyProperty;
            atsky.skyType = cam.GetClearFlag();
            if (atsky.skyType == CameraClearFlags.Color)
            {
                atsky.skyColor = cam.GetSkyColor();
            }
            else if (atsky.skyType == CameraClearFlags.Skybox)
            {
                atsky.skyShaderName = cam.GetSkyShader();
                atsky.skyShaderFloat = cam.ListSkyMaterialFloat();
                atsky.skyShaderColor = cam.ListSkyMaterialColor();
            }
            frame.movingData.Add(atsky);

            //---Directional Light on stage
            OperateLoadedLight oll = os.GetSystemDirectionalLight();
            AnimationTargetParts atlight = new AnimationTargetParts();
            atlight.animationType = AF_MOVETYPE.LightProperty;
            atlight.rotation = oll.GetRotation();
            atlight.color = oll.GetColor();
            atlight.power = oll.GetPower();
            atlight.shadowStrength = oll.GetShadowPower();
            frame.movingData.Add(atlight);

            


            return frame;
        }
    }
}