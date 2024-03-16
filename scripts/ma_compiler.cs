using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;
using UniVRM10;
using DG.Tweening;


namespace UserHandleSpace
{
    public partial class ManageAnimation
    {
//===========================================================================================================================
//  Analyze and parse functions
//===========================================================================================================================

        /*
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
        }*/
        private Sequence ParseForTranslateCommon(Sequence seq, NativeAnimationFrame frame, AnimationTranslateTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationTargetParts pelvisCondition, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;
            OperateLoadedBase olb = naa.avatar.GetComponent<OperateLoadedBase>();

            List<Vector3> movedatavalues = null;
            //---Cut specified position by index
            if (options.addTranslateExecuteIndex == -1)
            { //--- -1 convert to max count (for Play an animation)
                movedatavalues = movedata.values;
            }
            else
            { //--- for Preview an 1 frame motion
                movedatavalues = new List<Vector3>();
                int stpos = 0;
                for (int i = stpos; i < movedata.values.Count; i++)
                {
                    if (i == options.addTranslateExecuteIndex) movedatavalues.Add(movedata.values[i]);
                }
            }
            int lastcount = movedatavalues.Count - 1;


            //---DO a motion
            if (targetObjects.targetType == AF_TARGETTYPE.VRM)
            {
                Transform[] bts = naa.ikparent.GetComponentsInChildren<Transform>();
                OperateLoadedVRM olvrm = naa.avatar.GetComponent<OperateLoadedVRM>();
                GameObject trueik = olvrm.relatedTrueIKParent;

                
                int index = (int)movedata.vrmBone;

                if (movedata.vrmBone == ParseIKBoneType.IKParent)
                {
                    if (targetObjects.compiled == 1)
                    { //---HumanBodyBones based 

                        if (movedata.jumpNum >= 1) seq.Join(naa.avatar.transform.DOJump(movedatavalues[0], movedata.jumpPower, movedata.jumpNum, frame.duration));
                        seq.Join(naa.avatar.transform.DOPath(movedatavalues.ToArray(), frame.duration, PathType.CatmullRom));

                        //---Path version
                        /*if (movedatavalues.Count > 1)
                        {
                            seq.Join(naa.avatar.transform.DOPath(movedatavalues.ToArray(), frame.duration, PathType.CatmullRom));
                        }
                        else if (movedatavalues.Count == 1)
                        {
                            if (movedata.jumpNum <= 0)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOMove(movedatavalues[0], frame.duration));
                                else naa.avatar.transform.position = movedatavalues[0];
                            }
                        }*/
                        
                    }
                    //---IK marker based
                    if (movedata.jumpNum >= 1) seq.Join(naa.ikparent.transform.DOJump(movedatavalues[movedatavalues.Count - 1], movedata.jumpPower, movedata.jumpNum, frame.duration));
                    seq.Join(trueik.transform.DOPath(movedatavalues.ToArray(), frame.duration, PathType.CatmullRom));
                    /*
                    //---Path version
                    if (movedatavalues.Count > 1)
                    {
                        seq.Join(naa.ikparent.transform.DOPath(movedatavalues.ToArray(), frame.duration, PathType.CatmullRom));
                    }
                    else if (movedatavalues.Count == 1)
                    {
                        if (movedata.jumpNum <= 0)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.ikparent.transform.DOMove(movedatavalues[0], frame.duration));
                            else naa.ikparent.transform.position = movedatavalues[0];
                        }
                    }*/
                }
                else
                { //---Each IK parts---------------
                    { //---Transform for IK

                        //---for blendable: get information of previous frame

                        if ((movedata.vrmBone >= ParseIKBoneType.EyeViewHandle) && (movedata.vrmBone <= ParseIKBoneType.RightLeg))
                        {
                            GameObject realObject = null;// naa.ikparent.transform.Find(IKBoneNames[index]).gameObject;
                            foreach (Transform bt in bts)
                            {
                                if (bt.name == IKBoneNames[index])
                                {
                                    realObject = bt.gameObject;
                                    break;
                                }
                            }

                            //---Position (absorb a distance of height)
                            if (movedata.animationType == AF_MOVETYPE.Translate)
                            {
                                List<Vector3> curList = olvrm.GetTPoseBodyList();
                                int vbone = (int)movedata.vrmBone;

                                //---another version: Multiple height diff percentage to the pose value.
                                List<Vector3> repoarr = new List<Vector3>();
                                foreach (var v in movedatavalues)
                                {
                                    repoarr.Add(CalculateDifferenceByHeight(naa.bodyHeight, targetObjects.bodyHeight, v, movedata.vrmBone, 1, 1, 1));
                                }
                                seq.Join(realObject.transform.DOLocalPath(repoarr.ToArray(), frame.duration, PathType.CatmullRom));
                                /*
                                //---Path version
                                if (movedatavalues.Count > 1)
                                {
                                    List<Vector3> repoarr = new List<Vector3>();
                                    foreach (var v in movedatavalues)
                                    {
                                        repoarr.Add(CalculateDifferenceByHeight(naa.bodyHeight, targetObjects.bodyHeight, v, movedata.vrmBone, 1, 1, 1));
                                    }
                                    seq.Join(realObject.transform.DOLocalPath(repoarr.ToArray(), frame.duration, PathType.CatmullRom));
                                }
                                else if (movedatavalues.Count == 1)
                                {
                                    Vector3 repos = Vector3.zero;
                                    repos = CalculateDifferenceByHeight(naa.bodyHeight, targetObjects.bodyHeight, movedatavalues[0], movedata.vrmBone, 1, 1, 1);

                                    if (options.isExecuteForDOTween == 1) seq.Join(realObject.transform.DOLocalMove(repos, frame.duration));
                                    else realObject.transform.localPosition = repos;
                                }*/

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
                    //if (movedatavalues.Count > 1)
                    { //---don't exist DOAnchorPath ?, do DOAnchorPos
                        Sequence childq = DOTween.Sequence();
                        foreach (var v in movedatavalues)
                        {
                            Vector2 v2 = new Vector2(Screen.width * (v.x / 100f), Screen.height * (v.y / 100f));
                            if (options.isExecuteForDOTween == 1) childq.Join(rectt.DOAnchorPos(v2, frame.duration));
                            else rectt.anchoredPosition = v2;

                            if (options.isBuildDoTween == 0)
                            {
                                olui.currentPositionPercent = new Vector2(v.x, v.y);
                            }
                        }
                        seq.Join(childq);
                    }
                    /*else if (movedatavalues.Count == 1)
                    {
                        Vector2 v2 = new Vector2(Screen.width * (movedatavalues[0].x / 100f), Screen.height * (movedatavalues[0].y / 100f));
                        if (options.isExecuteForDOTween == 1) seq.Join(rectt.DOAnchorPos(v2, frame.duration));
                        else rectt.anchoredPosition = v2;

                        if (options.isBuildDoTween == 0)
                        {
                            olui.currentPositionPercent = new Vector2(movedatavalues[0].x, movedatavalues[0].y);
                        }
                    }*/

                    
                }
            }
            else if (targetObjects.targetType == AF_TARGETTYPE.Stage)
            {
                OperateStage os = naa.avatar.GetComponent<OperateStage>();

                if (os.ActiveStage != null)
                {
                    //---Path version
                    //if (movedatavalues.Count > 1)
                    {
                        seq.Join(os.ActiveStage.transform.DOPath(movedatavalues.ToArray(), frame.duration, PathType.CatmullRom));
                    }
                    /*else if (movedatavalues.Count == 1)
                    {
                        if (options.isExecuteForDOTween == 1) seq.Join(os.ActiveStage.transform.DOMove(movedatavalues[0], frame.duration));
                        else os.ActiveStage.transform.position = movedatavalues[0];
                    }*/
                }

            }
            else
            { //---OtherLight, Light, Camera, Effect, Text3D
                if (targetObjects.compiled == 1)
                {
                    if (movedata.jumpNum >= 1) seq.Join(naa.avatar.transform.DOJump(movedata.values[movedatavalues.Count - 1], movedata.jumpPower, movedata.jumpNum, frame.duration));
                    //---Path version
                    //if (movedatavalues.Count > 1)
                    {
                        seq.Join(naa.avatar.transform.DOPath(movedatavalues.ToArray(), frame.duration, PathType.CatmullRom));
                    }
                    /*else if (movedatavalues.Count == 1)
                    {
                        if (movedata.jumpNum <= 0)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOMove(movedatavalues[0], frame.duration));
                            else naa.avatar.transform.position = movedatavalues[0];
                        } 
                    }*/
                }
                {
                    if (movedata.jumpNum >= 1) seq.Join(naa.ikparent.transform.DOJump(movedata.values[movedatavalues.Count - 1], movedata.jumpPower, movedata.jumpNum, frame.duration));
                    //---Path version
                    //if (movedatavalues.Count > 1)
                    {
                        seq.Join(naa.ikparent.transform.DOPath(movedatavalues.ToArray(), frame.duration, PathType.CatmullRom));
                    }
                    /*else if (movedatavalues.Count == 1)
                    {
                        if (movedata.jumpNum <= 0)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.ikparent.transform.DOMove(movedatavalues[0], frame.duration));
                            else naa.ikparent.transform.position = movedatavalues[0];
                        }
                    }*/
                }

                if (options.isBuildDoTween == 0)
                { //---when not motion, directly set last indexed movedatavalues
                    naa.avatar.transform.position = movedatavalues[movedatavalues.Count - 1];
                }
            }
            return seq;
        }
        private Sequence ParseForCommon(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationTargetParts pelvisCondition, AnimationParsingOptions options)
        {
            //AnimationTargetParts movedata = movedatalst[0];

            /*
            List<Vector3> movedatavalues = new List<Vector3>();
            int stpos = 0;
            for (int i = stpos; i < movedata.values.Count; i++)
            {
                if (i == options.addTranslateExecuteIndex) movedatavalues.Add(movedata.values[i]);
            }
            */


            //===Common type======================================###
            RotateMode rotrm = RotateMode.Fast;
            if (movedata.isRotate360 == 1)
            {
                rotrm = RotateMode.FastBeyond360;
            }

            NativeAnimationAvatar naa = targetObjects.avatar;
            OperateLoadedBase olb = naa.avatar.GetComponent<OperateLoadedBase>();

            if (movedata.animationType == AF_MOVETYPE.Punch)
            {
                /*if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                     {
                         olb.SetPunch(JsonUtility.ToJson(movedata.effectPunch));
                     },false));
                }*/
                //if (targetObjects.compiled == 1)
                {
                    if (movedata.effectPunch.isEnable == 1)
                    {
                        /*if (naa.type == AF_TARGETTYPE.Text)
                        {
                            RectTransform rectt = naa.avatar.GetComponent<RectTransform>();

                            if (movedata.effectPunch.translationType == AF_MOVETYPE.Translate)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(rectt.DOPunchAnchorPos(movedata.effectPunch.punch, frame.duration, movedata.effectPunch.vibrato, movedata.effectPunch.elasiticity));
                            }
                            else if (movedata.effectPunch.translationType == AF_MOVETYPE.Rotate)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(rectt.DOPunchRotation(movedata.effectPunch.punch, frame.duration, movedata.effectPunch.vibrato, movedata.effectPunch.elasiticity));
                            }
                            else if (movedata.effectPunch.translationType == AF_MOVETYPE.Scale)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(rectt.DOPunchScale(movedata.effectPunch.punch, frame.duration, movedata.effectPunch.vibrato, movedata.effectPunch.elasiticity));
                            }
                        }
                        else*/
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

                }

                if (options.isBuildDoTween == 0)
                {
                    if ((naa.type == AF_TARGETTYPE.Text) || (naa.type == AF_TARGETTYPE.Text3D))
                    {
                        OperateLoadedText olt = naa.avatar.GetComponent<OperateLoadedText>();
                        olt.SetPunch(movedata.effectPunch);
                    }
                    else
                    {
                        olb.SetPunch(movedata.effectPunch);
                    }
                }
            }
            else if (movedata.animationType == AF_MOVETYPE.Shake)
            {
                /*if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        olb.SetShake(JsonUtility.ToJson(movedata.effectShake));
                    }, false));
                }*/
                //if (targetObjects.compiled == 1)
                {
                    if (movedata.effectShake.isEnable == 1)
                    {
                        /*if (naa.type == AF_TARGETTYPE.Text)
                        { //---3D text
                            RectTransform rectt = naa.avatar.GetComponent<RectTransform>();

                            if (movedata.effectShake.translationType == AF_MOVETYPE.Translate)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(rectt.DOShakeAnchorPos(frame.duration, movedata.effectShake.strength, movedata.effectShake.vibrato, movedata.effectShake.randomness, false, movedata.effectShake.fadeOut == 1 ? true : false));
                            }
                            else if (movedata.effectShake.translationType == AF_MOVETYPE.Rotate)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(rectt.DOShakeRotation(frame.duration, movedata.effectShake.strength, movedata.effectShake.vibrato, movedata.effectShake.randomness, movedata.effectShake.fadeOut == 1 ? true : false));
                            }
                            else if (movedata.effectShake.translationType == AF_MOVETYPE.Scale)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(rectt.DOShakeScale(frame.duration, movedata.effectShake.strength, movedata.effectShake.vibrato, movedata.effectShake.randomness, movedata.effectShake.fadeOut == 1 ? true : false));
                            }
                        }
                        else*/
                        {
                            if (movedata.effectShake.translationType == AF_MOVETYPE.Translate)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DOShakePosition(frame.duration, movedata.effectShake.strength, movedata.effectShake.vibrato, movedata.effectShake.randomness, false, movedata.effectShake.fadeOut == 1 ? true : false));
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
                        

                }

                if (options.isBuildDoTween == 0)
                {
                    if ((naa.type == AF_TARGETTYPE.Text) || (naa.type == AF_TARGETTYPE.Text3D))
                    {
                        OperateLoadedText olt = naa.avatar.GetComponent<OperateLoadedText>();
                        olt.SetShake(movedata.effectShake);
                    }
                    else
                    {
                        olb.SetShake(movedata.effectShake);
                    }
                    
                }
            }
            else if (movedata.animationType == AF_MOVETYPE.Rigid)
            {
                if (options.isBuildDoTween == 1)
                {
                    Rigidbody rb = null;
                    if ((naa.type == AF_TARGETTYPE.OtherObject) || (naa.type == AF_TARGETTYPE.Image) || (naa.type == AF_TARGETTYPE.Text3D))
                    { //---Rigidbody is the object itself.
                        rb = naa.avatar.GetComponent<Rigidbody>();
                    }
                    else if ((naa.type == AF_TARGETTYPE.Camera) || (naa.type == AF_TARGETTYPE.Light) || (naa.type == AF_TARGETTYPE.Effect))
                    { //---Rigidbody is the IKParent;
                        rb = naa.ikparent.GetComponent<Rigidbody>();
                    }
                    else if (naa.type == AF_TARGETTYPE.VRM)
                    { //---Rigidbody is the True IKParent;
                        rb = olb.relatedTrueIKParent.GetComponent<Rigidbody>();
                    }
                    
                    seq.Join(DOTween.To(() => rb.drag, x => rb.drag = x, movedata.rigidDrag, frame.duration));
                    seq.Join(DOTween.To(() => rb.angularDrag, x => rb.angularDrag = x, movedata.rigidAngularDrag, frame.duration));
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        if (naa.type == AF_TARGETTYPE.Text3D)
                        {
                            OperateLoadedText olt = naa.avatar.GetComponent<OperateLoadedText>();
                            olt.SetEasyCollision(movedata.useCollision);
                            olt.SetUseGravity(movedata.useRigidGravity);
                        }
                        else
                        {
                            olb.SetEasyCollision(movedata.useCollision);
                            olb.SetUseGravity(movedata.useRigidGravity);
                        }
                    }, false));
                }
                else
                {
                    
                    if (naa.type == AF_TARGETTYPE.Text3D)
                    {
                        OperateLoadedText olt = naa.avatar.GetComponent<OperateLoadedText>();
                        olt.SetDrag(movedata.rigidDrag, movedata.rigidAngularDrag);
                        olt.SetEasyCollision(movedata.useCollision);
                        olt.SetUseGravity(movedata.useRigidGravity);
                    }
                    else
                    {
                        olb.SetDrag(movedata.rigidDrag, movedata.rigidAngularDrag);
                        olb.SetEasyCollision(movedata.useCollision);
                        olb.SetUseGravity(movedata.useRigidGravity);
                    }
                }
            }

            //===Each type======================================###
            if (targetObjects.targetType == AF_TARGETTYPE.VRM)
            {
                Transform[] bts = naa.ikparent.GetComponentsInChildren<Transform>();
                GameObject trueik = naa.avatar.GetComponent<OperateLoadedVRM>().relatedTrueIKParent;

                int index = (int)movedata.vrmBone;

                if (movedata.vrmBone == ParseIKBoneType.IKParent)
                {
                    if (movedata.animationType == AF_MOVETYPE.Translate)
                    {
                        if (targetObjects.compiled == 1)
                        { //---HumanBodyBones based 
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
                        
                        { //---IK marker based
                            if (movedata.jumpNum >= 1)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(trueik.transform.DOJump(movedata.position, movedata.jumpPower, movedata.jumpNum, frame.duration));
                            }
                            else
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(trueik.transform.DOMove(movedata.position, frame.duration));
                                else trueik.transform.position = movedata.position;
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
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DORotate(movedata.rotation, frame.duration, rotrm));
                            else naa.avatar.transform.rotation = Quaternion.Euler(movedata.rotation);

                        }

                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(trueik.transform.DORotate(movedata.rotation, frame.duration, rotrm));
                            //if (options.isExecuteForDOTween == 1) seq.Join(naa.ikparent.transform.DOBlendableRotateBy(movedata.rotation, frame.duration, rotrm).SetRelative(false));
                            else trueik.transform.rotation = Quaternion.Euler(movedata.rotation);

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
                { //---Each IK parts---//
                    if (targetObjects.compiled == 1)
                    { //---Transform for HumanBodyBones
                        if (movedata.vrmBone == ParseIKBoneType.UseHumanBodyBones)
                        {
                            Transform boneTransform = naa.avatar.GetComponent<Animator>().GetBoneTransform(movedata.vrmHumanBodyBone);
                            if (movedata.animationType == AF_MOVETYPE.Rotate)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(boneTransform.DOLocalRotate(movedata.rotation, frame.duration));
                                //if (options.isExecuteForDOTween == 1) seq.Join(boneTransform.DOBlendableLocalRotateBy(movedata.rotation, frame.duration).SetRelative(false));
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

                        //---for blendable: get information of previous frame
                        /*
                        int beforeIndex = targetObjects.frames.FindIndex(match =>
                        {
                            if (match.index == frame.index) return true;
                            return false;
                        });
                        NativeAnimationFrame beforeFrame = null;
                        AnimationTargetParts beforeMoveData = null;
                        if (beforeIndex > -1)
                        {
                            beforeFrame = targetObjects.frames[beforeIndex];
                            beforeMoveData = beforeFrame.movingData.Find(match =>
                            {
                                if ((match.vrmBone == movedata.vrmBone) &&
                                    (match.animationType == movedata.animationType)
                                ) {
                                    return true;
                                }
                                else
                                {
                                    return false;
                                }
                            });
                        }
                        */
                        
                        if ((movedata.vrmBone >= ParseIKBoneType.EyeViewHandle) && (movedata.vrmBone <= ParseIKBoneType.RightLeg))
                        {
                            GameObject realObject = null;// naa.ikparent.transform.Find(IKBoneNames[index]).gameObject;
                            foreach (Transform bt in bts)
                            {
                                if (bt.name == IKBoneNames[index])
                                {
                                    realObject = bt.gameObject;
                                    break;
                                }
                            }

                            //---Position (absorb a distance of height)
                            if (movedata.animationType == AF_MOVETYPE.Translate)
                            {
                                List<Vector3> curList = naa.avatar.GetComponent<OperateLoadedVRM>().GetTPoseBodyList();
                                int vbone = (int)movedata.vrmBone;

                                Vector3 repos = movedata.position;

                                /*repos = CalculateDifferenceInHeight(
                                    curList[vbone],
                                    frame.useBodyInfo == UseBodyInfoType.TimelineCharacter ? targetObjects.bodyInfoList[vbone] : curList[vbone],
                                    movedata.position, movedata.vrmBone
                                );*/

                                //---another version: Multiple height diff percentage to the pose value.
                                /*if ((movedata.vrmBone == ParseIKBoneType.LeftLeg) || (movedata.vrmBone == ParseIKBoneType.RightLeg))
                                { //---left and right leg(foot) is not change difference of the height. (as start position)
                                    repos = movedata.position;
                                }
                                else*/
                                //---Path version
                                AnimationTranslateTargetParts attp = frame.FindTranslateMoving(AF_MOVETYPE.Translate, ParseIKBoneType.IKParent);
                                if (attp.values.Count > 1)
                                {
                                    seq.Join(trueik.transform.DOPath(attp.values.ToArray(), frame.duration, PathType.CatmullRom));
                                }
                                else if (attp.values.Count == 1)
                                {
                                    if (options.isExecuteForDOTween == 1) seq.Join(trueik.transform.DOMove(attp.values[0], frame.duration));
                                    else trueik.transform.position = attp.values[0];
                                }


                                {
                                    repos = CalculateDifferenceByHeight(naa.bodyHeight, targetObjects.bodyHeight, movedata.position, movedata.vrmBone, 1, 1, 1);
                                }
                                

                                //---for blendable
                                if (options.isExecuteForDOTween == 1) seq.Join(realObject.transform.DOLocalMove(repos, frame.duration));
                                else realObject.transform.localPosition = repos;
                                
                            }

                            //---Rotation
                            if (movedata.animationType == AF_MOVETYPE.Rotate)
                            {
                                //if (beforeIndex == -1)
                                //{
                                if (options.isExecuteForDOTween == 1)
                                {
                                    seq.Join(realObject.transform.DOLocalRotate(movedata.rotation, frame.duration));
                                }
                                else realObject.transform.rotation = Quaternion.Euler(movedata.rotation);
                                //}
                                /*else
                                {
                                    if (options.isExecuteForDOTween == 1) seq.Join(realObject.transform.DOBlendableLocalRotateBy(movedata.rotation, frame.duration));
                                    else realObject.transform.rotation = Quaternion.Euler(movedata.rotation);
                                }*/
                                    
                            }

                            //---Rigidbody and Collision
                            if (movedata.animationType == AF_MOVETYPE.Rigid)
                            {
                                Collider col = realObject.GetComponent<Collider>();
                                Rigidbody rig = realObject.GetComponent<Rigidbody>();

                                if (options.isBuildDoTween == 1)
                                {
                                    

                                    seq.Join(DOTween.To(() => rig.drag, x => rig.drag = x, movedata.rigidDrag, frame.duration));
                                    seq.Join(DOTween.To(() => rig.angularDrag, x => rig.angularDrag = x, movedata.rigidAngularDrag, frame.duration));
                                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                                    {
                                        col.isTrigger = movedata.useCollision == 1 ? false : true;
                                        rig.useGravity = movedata.useRigidGravity == 1 ? true : false;
                                    }, false));
                                }
                                else
                                {
                                    rig.drag = movedata.rigidDrag;
                                    rig.angularDrag = movedata.rigidAngularDrag;
                                    col.isTrigger = movedata.useCollision == 1 ? false : true;
                                    rig.useGravity = movedata.useRigidGravity == 1 ? true : false;

                                }
                            }
                        }

                    }
                }

            }
            else if ((targetObjects.targetType == AF_TARGETTYPE.Text) || (targetObjects.targetType == AF_TARGETTYPE.Text3D) || (targetObjects.targetType == AF_TARGETTYPE.UImage))
            {

                if (targetObjects.targetType == AF_TARGETTYPE.Text3D)
                {
                    Transform tran3d = naa.avatar.transform;
                    OperateLoadedText olt = naa.avatar.GetComponent<OperateLoadedText>();

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
                            olt.SetJump(movedata.jumpPower.ToString() + "," + movedata.jumpNum.ToString());
                            
                        }
                    }
                    else if (movedata.animationType == AF_MOVETYPE.Rotate)
                    {
                        if (targetObjects.compiled == 1)
                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DORotate(movedata.rotation, frame.duration, rotrm));
                            else naa.avatar.transform.rotation = Quaternion.Euler(movedata.rotation);

                        }

                        {
                            if (options.isExecuteForDOTween == 1) seq.Join(naa.ikparent.transform.DORotate(movedata.rotation, frame.duration, rotrm));
                            else naa.ikparent.transform.rotation = Quaternion.Euler(movedata.rotation);

                        }
                        if (options.isBuildDoTween == 0)
                        {
                            naa.avatar.transform.rotation = Quaternion.Euler(movedata.rotation);
                        }
                    }
                    else if (movedata.animationType == AF_MOVETYPE.Scale)
                    {

                        olt.TextAnimationTween(seq, movedata, options, frame.duration);

                    }
                }
                else
                {
                    RectTransform rectt = targetObjects.avatar.avatar.GetComponent<RectTransform>();
                    OperateLoadedUImage olui = targetObjects.avatar.avatar.GetComponent<OperateLoadedUImage>();


                    if (movedata.animationType == AF_MOVETYPE.Translate)
                    {

                        Vector2 v2 = new Vector2(Screen.width * (movedata.position.x / 100f), Screen.height * (movedata.position.y / 100f));
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

                
            }
            else if (targetObjects.targetType == AF_TARGETTYPE.Stage)
            {
                OperateStage os = naa.avatar.GetComponent<OperateStage>();

                if (os.ActiveStage != null)
                {
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
                else if (movedata.animationType == AF_MOVETYPE.Rotate)
                {
                    if (targetObjects.compiled == 1)
                    {
                        if (options.isExecuteForDOTween == 1) seq.Join(naa.avatar.transform.DORotate(movedata.rotation, frame.duration, rotrm));
                        else naa.avatar.transform.rotation = Quaternion.Euler(movedata.rotation);

                    }

                    {
                        if (options.isExecuteForDOTween == 1) seq.Join(naa.ikparent.transform.DORotate(movedata.rotation, frame.duration, rotrm));
                        else naa.ikparent.transform.rotation = Quaternion.Euler(movedata.rotation);

                    }

                    if (options.isBuildDoTween == 0)
                    {
                        naa.avatar.transform.rotation = Quaternion.Euler(movedata.rotation);
                    }
                }
                else if (movedata.animationType == AF_MOVETYPE.Scale)
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
                        LeftHandPoseController lhand = ovrm.LeftHandCtrl;

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

                            //---Preset hand posing
                            if (movedata.handpose[0] > -1)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => lhand.handPoseValue, x => lhand.handPoseValue = x, movedata.handpose[1], frame.duration));
                                else lhand.handPoseValue = (int)movedata.handpose[1];                                
                            }
                            else
                            { //---Manually finger posing
                                if (options.isExecuteForDOTween == 1)
                                {
                                    seq = lhand.AnimationFinger("t", movedata.fingerpose, frame.duration, seq);
                                    seq = lhand.AnimationFinger("i", movedata.fingerpose, frame.duration, seq);
                                    seq = lhand.AnimationFinger("m", movedata.fingerpose, frame.duration, seq);
                                    seq = lhand.AnimationFinger("r", movedata.fingerpose, frame.duration, seq);
                                    seq = lhand.AnimationFinger("l", movedata.fingerpose, frame.duration, seq);
                                }
                                else
                                {
                                    lhand.PoseFinger("t", movedata.fingerpose.Thumbs.Count, movedata.fingerpose.Thumbs.ToArray());
                                    lhand.PoseFinger("i", movedata.fingerpose.Index.Count, movedata.fingerpose.Index.ToArray());
                                    lhand.PoseFinger("m", movedata.fingerpose.Middle.Count, movedata.fingerpose.Middle.ToArray());
                                    lhand.PoseFinger("r", movedata.fingerpose.Ring.Count, movedata.fingerpose.Ring.ToArray());
                                    lhand.PoseFinger("l", movedata.fingerpose.Little.Count, movedata.fingerpose.Little.ToArray());
                                }
                            }

                            if (options.isBuildDoTween == 0)
                            {
                                //ovrm.SetHandFingerMode(movedata.isHandPose.ToString());
                                ovrm.SetBackupHandPosing("l", (int)movedata.handpose[0], movedata.handpose[1], movedata.fingerpose);
                            }
                        }
                        


                    }

                    if (movedata.vrmBone == ParseIKBoneType.RightHandPose)
                    {
                        RightHandPoseController rhand = ovrm.RightHandCtrl;

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

                            //---Preset hand posing
                            if (movedata.handpose[0] > -1)
                            {
                                if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => rhand.handPoseValue, x => rhand.handPoseValue = x, movedata.handpose[1], frame.duration));
                                else rhand.handPoseValue = (int)movedata.handpose[1];
                            }
                            else
                            { //---Manually finger posing
                                if (options.isExecuteForDOTween == 1)
                                {
                                    seq = rhand.AnimationFinger("t", movedata.fingerpose, frame.duration, seq);
                                    seq = rhand.AnimationFinger("i", movedata.fingerpose, frame.duration, seq);
                                    seq = rhand.AnimationFinger("m", movedata.fingerpose, frame.duration, seq);
                                    seq = rhand.AnimationFinger("r", movedata.fingerpose, frame.duration, seq);
                                    seq = rhand.AnimationFinger("l", movedata.fingerpose, frame.duration, seq);
                                }
                                else
                                {
                                    rhand.PoseFinger("t", movedata.fingerpose.Thumbs.Count, movedata.fingerpose.Thumbs.ToArray());
                                    rhand.PoseFinger("i", movedata.fingerpose.Index.Count, movedata.fingerpose.Index.ToArray());
                                    rhand.PoseFinger("m", movedata.fingerpose.Middle.Count, movedata.fingerpose.Middle.ToArray());
                                    rhand.PoseFinger("r", movedata.fingerpose.Ring.Count, movedata.fingerpose.Ring.ToArray());
                                    rhand.PoseFinger("l", movedata.fingerpose.Little.Count, movedata.fingerpose.Little.ToArray());
                                }

                            }
                        }
                        

                        if (options.isBuildDoTween == 0)
                        {
                            //ovrm.isHandPosing = movedata.isHandPose == 1 ? true : false;
                            ovrm.SetBackupHandPosing("r", (int)movedata.handpose[0], movedata.handpose[1], movedata.fingerpose);
                        }



                    }

                }
            }


            //---Blend Shape (registered key ONLY)
            if (movedata.animationType == AF_MOVETYPE.BlendShape)
            {
                //OperateLoadedVRM mainface = naa.avatar.GetComponent<OperateLoadedVRM>();
                //SkinnedMeshRenderer face = ovrm.GetBlendShapeTarget();
                List<SkinnedMeshRenderer> facelist = ovrm.GetBlendShapeTargets();

                //VRMBlendShapeProxy prox = naa.avatar.GetComponent<VRMBlendShapeProxy>();
                Vrm10RuntimeExpression expression = naa.avatar.GetComponent<Vrm10Instance>().Runtime.Expression;
                IReadOnlyList<ExpressionKey> eklist = expression.ExpressionKeys;

                //int maxcnt = face.sharedMesh.blendShapeCount;
                foreach (BasicBlendShapeKey val in movedata.blendshapes)
                {
                    float weight = val.value;

                    if (val.text.StartsWith("PROX:"))
                    { //---from BlendShape Proxy
                        /*
                        BlendShapeKey key = ovrm.getProxyBlendShapeKey(val.text);
                        if (key.Name != "d%d")
                        {
                            
                            //Debug.Log("val.text=" + val.text + " / " + key.Name + " = " + prox.GetValue(key).ToString() + " -> " + weight.ToString());
                            float progress = prox.GetValue(key);
                            if (options.isExecuteForDOTween == 1)
                                seq.Join(DOTween.To(() => progress, x => progress = x, weight, frame.duration))
                                    .OnUpdate(() =>
                                    {
                                        prox.AccumulateValue(key, progress);
                                        prox.Apply();
                                    });
                            else ovrm.changeProxyBlendShapeByName(val.text + "=" + weight.ToString());

                            //---backup as app blend shape key (prefix: PROX:)
                            ovrm.SetBlendShapeToBackup(val.text, weight);
                        }
                        */
                        //---1.x
                        ExpressionKey ekey = ovrm.getVrm10ExpressionKey(val.text);
                        if (ekey.Name != "d%d")
                        {
                            float progress = expression.GetWeight(ekey);
                            if (options.isExecuteForDOTween == 1)
                                
                                seq.Join(DOTween.To(() => expression.GetWeight(ekey), x => ovrm.changeProxyBlendShapeByName(ekey, x), weight, frame.duration));
                                /*seq.Join(DOTween.To(() => progress, x => progress = x, weight, frame.duration))
                                    .OnUpdate(() =>
                                    {
                                        expression.SetWeight(ekey, progress * 0.01f);
                                        
                                    });*/
                                
                            else ovrm.changeProxyBlendShapeByName(val.text + "=" + weight.ToString());

                            //---backup as app blend shape key (prefix: PROX:)
                            ovrm.SetBlendShapeToBackup(val.text, weight, 1);
                        }
                        
                    }
                    else
                    { //---from SkinnedMeshRenderer
                        string hitName = "";
                        /*for (int chki = 0; chki < maxcnt; chki++)
                        {
                            if ((face.sharedMesh.GetBlendShapeName(chki) + "$").Contains(val.text + "$"))
                            {
                                hitName = face.sharedMesh.GetBlendShapeName(chki);
                                break;
                            }
                        }*/

                        int bindex = -1; //face.sharedMesh.GetBlendShapeIndex(hitName);
                        //if (bindex > -1) face.SetBlendShapeWeight(bindex, weight);
                        foreach (SkinnedMeshRenderer mesh in facelist)
                        {
                            bindex = ovrm.getAvatarBlendShapeIndex(mesh, val.text);
                            if (bindex > -1)
                            {
                                hitName = val.text;
                                break;
                            }
                        }

                        if (bindex > -1)
                        {
                            //if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => ovrm.getAvatarBlendShapeValue(val.text), x => ovrm.changeAvatarBlendShapeByName(val.text, x), weight, frame.duration));
                            if (options.isExecuteForDOTween == 1) seq = ovrm.AnimationBlendShape(seq, val.text, weight, frame.duration);
                            else ovrm.changeAvatarBlendShapeByName(val.text, val.value);  //face.SetBlendShapeWeight(bindex, weight);

                            //---write as backup to Loaded setting.
                            ovrm.SetBlendShapeToBackup(hitName, weight, 1);
                        }
                    }
                }
            }
            if (movedata.animationType == AF_MOVETYPE.VRMAutoBlendShape)
            {
                if (options.isBuildDoTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        ovrm.SetAutoBlendShapeInterval(movedata.interval);
                        ovrm.SetAutoBlendShapeOpeningSeconds(movedata.openingSeconds);
                        ovrm.SetAutoBlendShapeCloseSeconds(movedata.closeSeconds);
                        ovrm.SetPlayFlagAutoBlendShape(movedata.isblink);
                        if (movedata.isblink == 1)
                        {
                            //ovrm.AutoBlendShape(0);
                            ovrm.PlayAutoBlendShape();
                        }
                        else
                        { //---already pause/stop, is not meaning...
                            //ovrm.PauseAutoBlendShape();
                            ovrm.StopAutoBlendShape();
                        }

                    }, false));
                }
                else
                {
                    //---Auto BlendShape
                    ovrm.SetAutoBlendShapeInterval(movedata.interval);
                    ovrm.SetAutoBlendShapeOpeningSeconds(movedata.openingSeconds);
                    ovrm.SetAutoBlendShapeCloseSeconds(movedata.closeSeconds);
                    ovrm.SetPlayFlagAutoBlendShape(movedata.isblink);
                    if (movedata.isblink == 1)
                    {
                        //ovrm.AutoBlendShape(0);
                        ovrm.PlayAutoBlendShape();
                    }
                    else
                    { //---already pause/stop, is not meaning...
                        //ovrm.PauseAutoBlendShape();
                        ovrm.StopAutoBlendShape();
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
                //:::local function: unequip function
                void tmpfunc_unequip(AvatarEquipSaveClass body)
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
                }
                //:::local function: equip function
                void tmpfunc_equip(AvatarEquipSaveClass body)
                {
                    //---check existed equipment
                    AvatarEquipSaveClass equip = ovrm.equipDestinations.list.Find(match =>
                    {
                        if ((match.bodybonename == body.bodybonename) && (match.equipitem == body.equipitem)) return true;
                        return false;
                    });
                    if (equip == null) //---body don't has this animation equip...EQUIP
                    {
                        NativeAnimationAvatar equipitemCast = GetCastInProject(body.equipitem);
                        if (equipitemCast != null)
                        { //---get an equippable avatar
                            //---load an equipment side FrameActor
                            NativeAnimationFrameActor nafact = GetFrameActorFromRole(equipitemCast.roleName, equipitemCast.type);
                            if (nafact != null)
                            {
                                NativeAnimationFrame naf_frame = nafact.frames.Find(nafact_frame =>
                                {
                                    if (nafact_frame.index == frame.index) return true;
                                    return false;
                                });
                                if (naf_frame != null)
                                { 
                                    //---get transform info of an equipment side.
                                    AnimationTargetParts translate_atp = naf_frame.FindMovingData(AF_MOVETYPE.Translate);
                                    AnimationTargetParts rotation_atp = naf_frame.FindMovingData(AF_MOVETYPE.Rotate);
                                    OperateLoadedBase cast_olb = equipitemCast.avatar.GetComponent<OperateLoadedBase>();
                                    if (cast_olb != null)
                                    { //---save transform info of key-frame to avatar object's OperateLoaded-component.
                                        cast_olb.SetPosition(translate_atp.position);
                                        cast_olb.SetRotation(rotation_atp.rotation);
                                    }
                                }
                            }
                            //ovrm.SetPosition(body.position);
                            //ovrm.SetRotation(body.rotation);
                            //vi devas sxargi POSITION kaj ROTATION antaux cxi tiu metodo.
                            ovrm.EquipObject((HumanBodyBones)body.bodybonename, equipitemCast);
                        }
                    }
                }
                //if (options.isBuildDoTween == 0)
                if (options.isExecuteForDOTween == 1)
                {
                    if (options.isBuildDoTween == 1)
                    { //---when animation 
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
                                tmpfunc_equip(body);
                            });

                            //---new version(2024/01/06~)
                            movedata.equipDestinations.ForEach(body =>
                            {
                                if (body.equipflag == 1)
                                { //---start equip

                                }
                                else if (body.equipflag == -1)
                                { //---finish unequip

                                }
                            });

                        }, false));
                    }
                    else
                    { //---when preview
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
                            tmpfunc_equip(body);
                        });

                        
                    }
                    
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
                        if (mat.includeMotion == 1) seq = ovrm.SetMaterialTween(seq, mat.name, mat, frame.duration);
                    }
                }

                if (options.isBuildDoTween == 0)
                {
                    //---Properties back up.
                    foreach (MaterialProperties mat in movedata.matProp)
                    {
                        ovrm.SetTextureConfig(mat.name, mat, true);
                    }
                    
                }
            }
            //---VRMAnimation

            if (movedata.animationType == AF_MOVETYPE.AnimStart)
            {
                switch (movedata.animPlaying)
                {
                    case UserAnimationState.Play:
                        if (options.isBuildDoTween == 1)
                        {
                            seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                            {
                                ovrm.SetVRMAContinuously(movedata.text);
                                ovrm.SetTargetClip(movedata.animName);
                                ovrm.SetWrapMode(movedata.animLoop);
                                ovrm.SetSpeedAnimation(movedata.animSpeed);

                                ovrm.SeekPlayAnimation(movedata.animSeek);
                                ovrm.PlayAnimation(0);
                                ovrm.SetPlayFlagAnimation(UserAnimationState.Play);
                            }, false));
                        }
                        if (options.isBuildDoTween == 0)
                        {
                            ovrm.SetVRMAContinuously(movedata.text);
                            ovrm.SetTargetClip(movedata.animName);
                            ovrm.SetWrapMode(movedata.animLoop);
                            ovrm.SetSpeedAnimation(movedata.animSpeed);

                            ovrm.PlayAnimation(0);
                            ovrm.SeekPlayAnimation(movedata.animSeek);
                            ovrm.SetPlayFlagAnimation(movedata.animPlaying);
                        }
                        break;
                    case UserAnimationState.Pause:
                        if (options.isBuildDoTween == 1)
                        {
                            seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                            {
                                ovrm.SetVRMAContinuously(movedata.text);
                                ovrm.SetTargetClip(movedata.animName);
                                ovrm.SetWrapMode(movedata.animLoop);
                                ovrm.SetSpeedAnimation(movedata.animSpeed);

                                ovrm.PauseAnimation();
                                ovrm.SetPlayFlagAnimation(UserAnimationState.Pause);
                            }, false));
                        }
                        if (options.isBuildDoTween == 0)
                        {
                            ovrm.SetVRMAContinuously(movedata.text);
                            ovrm.SetTargetClip(movedata.animName);
                            ovrm.SetWrapMode(movedata.animLoop);
                            ovrm.SetSpeedAnimation(movedata.animSpeed);
                            ovrm.SetSeekPosAnimation(movedata.animSeek);

                            ovrm.PauseAnimation();
                            ovrm.SetPlayFlagAnimation(movedata.animPlaying);
                        }
                        break;
                    case UserAnimationState.Seeking:
                        if (options.isBuildDoTween == 1)
                        {
                            seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                            {
                                ovrm.SetVRMAContinuously(movedata.text);
                                ovrm.SetTargetClip(movedata.animName);
                                ovrm.SetWrapMode(movedata.animLoop);
                                ovrm.SetSpeedAnimation(movedata.animSpeed);

                                ovrm.SetPlayFlagAnimation(UserAnimationState.Seeking);
                            }, false));
                            //seq.Join(DOTween.To(() => ovrm.SeekPosition, x => ovrm.SeekPosition = x, movedata.animSeek, frame.duration));
                        }

                        if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => ovrm.SeekPosition, x => ovrm.SeekPosition = x, movedata.animSeek, frame.duration));
                        else ovrm.SeekPosition = movedata.animSeek;

                        if (options.isBuildDoTween == 0)
                        {
                            ovrm.SetVRMAContinuously(movedata.text);
                            ovrm.SetTargetClip(movedata.animName);
                            ovrm.SetWrapMode(movedata.animLoop);
                            ovrm.SetSpeedAnimation(movedata.animSpeed);
                            ovrm.SetSeekPosAnimation(movedata.animSeek);

                            ovrm.SeekPlayAnimation(movedata.animSeek);
                            ovrm.SetPlayFlagAnimation(movedata.animPlaying);
                        }
                        break;
                    case UserAnimationState.Stop:
                        if (options.isBuildDoTween == 1)
                        {
                            seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                            {
                                ovrm.SetVRMAContinuously(movedata.text);
                                ovrm.SetTargetClip(movedata.animName);
                                ovrm.SetWrapMode(movedata.animLoop);
                                ovrm.SetSpeedAnimation(movedata.animSpeed);

                                ovrm.StopAnimation();
                                ovrm.SetPlayFlagAnimation(UserAnimationState.Stop);
                            }, false));
                        }
                        if (options.isBuildDoTween == 0)
                        {
                            ovrm.SetVRMAContinuously(movedata.text);
                            ovrm.SetTargetClip(movedata.animName);
                            ovrm.SetWrapMode(movedata.animLoop);
                            ovrm.SetSpeedAnimation(movedata.animSpeed);

                            ovrm.StopAnimation();
                            ovrm.SetPlayFlagAnimation(movedata.animPlaying);
                        }
                        break;
                    default:
                        if (options.isBuildDoTween == 1)
                        {
                            seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                            {
                                ovrm.SetVRMAContinuously(movedata.text);
                                ovrm.SetTargetClip(movedata.animName);
                                ovrm.SetWrapMode(movedata.animLoop);
                                ovrm.SetSpeedAnimation(movedata.animSpeed);

                                ovrm.SetPlayFlagAnimation(UserAnimationState.Playing);
                            }, false));
                        }
                        if (options.isBuildDoTween == 0)
                        {
                            ovrm.SetVRMAContinuously(movedata.text);
                            ovrm.SetTargetClip(movedata.animName);
                            ovrm.SetWrapMode(movedata.animLoop);
                            ovrm.SetSpeedAnimation(movedata.animSpeed);

                            ovrm.SetSeekPosAnimation(movedata.animSeek);
                            ovrm.SetPlayFlagAnimation(movedata.animPlaying);
                        }
                        break;
                }


            }
            else if (movedata.animationType == AF_MOVETYPE.AnimStop)
            { //---AnimStop is ClearVRMA function
                if (options.isBuildDoTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        ovrm.ClearVRMA();
                        
                        ovrm.SetPlayFlagAnimation(UserAnimationState.Playing);
                    }, false));
                }
                if (options.isBuildDoTween == 0)
                {
                    ovrm.ClearVRMA();

                    ovrm.SetPlayFlagAnimation(movedata.animPlaying);
                }
            }
            else if (movedata.animationType == AF_MOVETYPE.AnimProperty)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        ovrm.SetTargetClip(movedata.animName);
                        ovrm.SetWrapMode(movedata.animLoop);
                    }, false));
                }
                if (options.isBuildDoTween == 0)
                {
                    ovrm.SetTargetClip(movedata.animName);
                    ovrm.SetWrapMode(movedata.animLoop);
                    ovrm.SetSpeedAnimation(movedata.animSpeed);
                    ovrm.SetSeekPosAnimation(movedata.animSeek);
                }

                if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => ovrm.GetSpeedAnimation(), x => ovrm.SetSpeedAnimation(x), movedata.animSpeed, frame.duration));
                else ovrm.SetSpeedAnimation(movedata.animSpeed);

            }
            else if (movedata.animationType == AF_MOVETYPE.Rest)
            {

            }
            
            

            return seq;
        }
        private Sequence ParseForOtherObject(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;
            OperateLoadedOther olo = naa.avatar.GetComponent<OperateLoadedOther>();
            if (movedata.animationType == AF_MOVETYPE.AnimStart)
            {

                
                    switch (movedata.animPlaying)
                    {
                        case UserAnimationState.Play:
                            if (options.isBuildDoTween == 1)
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
                            break;
                        case UserAnimationState.Pause:
                            if (options.isBuildDoTween == 1)
                            {
                                seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                                {
                                    olo.SetTargetClip(movedata.animName);
                                    olo.SetWrapMode(movedata.animLoop);
                                    olo.SetSpeedAnimation(movedata.animSpeed);

                                    olo.PauseAnimation();
                                    olo.SetPlayFlagAnimation(UserAnimationState.Pause);
                                }, false));
                            }
                            if (options.isBuildDoTween == 0)
                            {
                                olo.SetTargetClip(movedata.animName);
                                olo.SetWrapMode(movedata.animLoop);
                                olo.SetSpeedAnimation(movedata.animSpeed);
                                olo.SetSeekPosAnimation(movedata.animSeek);

                                //olo.PauseAnimation();
                                olo.SetPlayFlagAnimation(movedata.animPlaying);
                            }
                            break;
                        case UserAnimationState.Seeking:
                            if (options.isBuildDoTween == 1)
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
                                olo.SetSeekPosAnimation(movedata.animSeek);

                                //olo.SeekPlayAnimation(movedata.animSeek);
                                olo.SetPlayFlagAnimation(movedata.animPlaying);
                            }
                            break;
                        case UserAnimationState.Stop:
                            if (options.isBuildDoTween == 1)
                            {
                                seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                                {
                                    olo.SetTargetClip(movedata.animName);
                                    olo.SetWrapMode(movedata.animLoop);
                                    olo.SetSpeedAnimation(movedata.animSpeed);

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
                            break;
                        default:
                            if (options.isBuildDoTween == 1)
                            {
                                seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                                {
                                    olo.SetTargetClip(movedata.animName);
                                    olo.SetWrapMode(movedata.animLoop);
                                    olo.SetSpeedAnimation(movedata.animSpeed);

                                    olo.SetPlayFlagAnimation(UserAnimationState.Playing);
                                }, false));
                            }
                            if (options.isBuildDoTween == 0)
                            {
                                olo.SetTargetClip(movedata.animName);
                                olo.SetWrapMode(movedata.animLoop);
                                olo.SetSpeedAnimation(movedata.animSpeed);
                                olo.SetSeekPosAnimation(movedata.animSeek);

                                olo.SetPlayFlagAnimation(movedata.animPlaying);
                            }
                            break;
                    }

                
            }
            else if (movedata.animationType == AF_MOVETYPE.AnimStop)
            {
                
            }
            else if (movedata.animationType == AF_MOVETYPE.AnimSeek)
            {

                
                
            }
            else if (movedata.animationType == AF_MOVETYPE.AnimPause)
            {
                
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
                    olo.SetSpeedAnimation(movedata.animSpeed);
                    olo.SetSeekPosAnimation(movedata.animSeek);
                }

                if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => olo.GetSpeedAnimation(), x => olo.SetSpeedAnimation(x), movedata.animSpeed, frame.duration));
                else olo.SetSpeedAnimation(movedata.animSpeed);

            }
            else if (movedata.animationType == AF_MOVETYPE.Rest)
            {
                
            }
            
            else if (movedata.animationType == AF_MOVETYPE.ObjectTexture)
            {
                if (options.isExecuteForDOTween == 1)
                {
                    foreach (MaterialProperties mat in movedata.matProp)
                    {
                        //Debug.Log("Parse, texturePath="+mat.texturePath);
                        seq = olo.SetMaterialTween(seq, mat.name, mat, frame.duration);
                        
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
            OperateLoadedLight oll = naa.avatar.GetComponent<OperateLoadedLight>();
            LensFlare flare = oll.OwnFlare;

            if (movedata.animationType == AF_MOVETYPE.LightProperty)
            {
                if (options.isBuildDoTween == 1)
                {
                    seq = oll.SetTweenLight(seq, lt, movedata, frame.duration);
                    //---Light type
                    //if (options.isBuildDoTween == 0) lt.type = movedata.lightType;
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        //---Light type
                        lt.type = movedata.lightType;

                        //---Light Render Mode
                        lt.renderMode = movedata.lightRenderMode;

                        //---flareType
                        oll.SetFlare(movedata.flareType);

                        //---flareType
                        oll.SetFlareColor(movedata.flareColor);

                        //---flareColor
                        oll.SetFlareColor(movedata.flareColor);

                        //oll.SetTweenLight(seq, lt, movedata, frame.duration);
                    }, false));


                    /*
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

                    //---Halo
                    //seq.Join(DOTween.To(() => RenderSettings.haloStrength, x => RenderSettings.haloStrength = x, movedata.halo, frame.duration));

                    //---flareType
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        oll.SetFlare(movedata.flareType);
                    }, false));

                    //---flareColor
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        oll.SetFlareColor(movedata.flareColor);
                    }, false));

                    //---flareBrightness
                     seq.Join(DOTween.To(() => flare.brightness, x => flare.brightness = x, movedata.flareBrightness, frame.duration));

                    //---flare fade
                    seq.Join(DOTween.To(() => flare.fadeSpeed, x => flare.fadeSpeed = x, movedata.flareFade, frame.duration));

                    */
                }
                else if (options.isBuildDoTween == 0)
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
                    oll.SetHalo(movedata.halo);
                    oll.SetFlare(movedata.flareType);
                    oll.SetFlareColor(movedata.flareColor);
                    oll.SetFlareBrightness(movedata.flareBrightness);
                    oll.SetFlareFade(movedata.flareFade);
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
                { //---execute the animation and the preview
                    switch (movedata.animPlaying)
                    {
                        case UserAnimationState.Play:
                            seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                            {
                                olc.SetCameraPlaying(movedata.animPlaying);
                                if (options.isCameraPreviewing == 1) olc.PreviewCamera();
                            }, false));
                            break;
                        case UserAnimationState.Playing:
                            seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                            {
                                olc.SetCameraPlaying(movedata.animPlaying);
                            }, false));
                            break;
                        case UserAnimationState.Stop:
                            seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                            {
                                olc.SetCameraPlaying(movedata.animPlaying);
                                olc.EndPreview();
                            }, false));
                            break;
                    }
                    
                }
            }
            /*
            else if (movedata.animationType == AF_MOVETYPE.CameraOn)
            {
                //if (options.isBuildDoTween == 0)
                if (options.isExecuteForDOTween == 1)
                {
                    
                    //olc.SetCameraPlaying(movedata.cameraPlaying);
                    //if (options.isCameraPreviewing == 1) olc.PreviewCamera();
                }
            }
            else if (movedata.animationType == AF_MOVETYPE.CameraOff)
            {
                //if (options.isBuildDoTween == 0)
                if (options.isExecuteForDOTween == 1)
                {
                    
                    //olc.SetCameraPlaying(movedata.cameraPlaying);
                    //olc.EndPreview();
                }
            }
            */
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
                        //olc.SetClearFlag(movedata.clearFlag);

                        //---LOAD configuration only: render texture etc...
                        olc.SetRenderTexture(movedata.renderTex);
                        olc.SetCameraRenderFlag(movedata.renderFlag);
                        if ((movedata.renderTex.x > 0) && (movedata.renderTex.y > 0))
                        {
                            olc.AutoReloadRenderTexture();
                        }
                    }, false));
                }
                if (options.isBuildDoTween == 0)
                {
                    cam.fieldOfView = movedata.fov;
                    cam.backgroundColor = movedata.color;
                    cam.rect = new Rect(movedata.viewport);
                    cam.depth = movedata.depth;

                    olc.SetRenderTexture(movedata.renderTex);
                    olc.SetCameraRenderFlag(movedata.renderFlag);
                    olc.AutoReloadRenderTexture();
                }
            }
            return seq;
        }
        private Sequence ParseForText(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;
            OperateLoadedText olt = naa.avatar.GetComponent<OperateLoadedText>();

            if (options.isBuildDoTween == 1)
            {
                olt.TextAnimationTween(seq, movedata, options, frame.duration);
            }
            else
            { //---when preview 
                if (movedata.animationType == AF_MOVETYPE.Text)
                {
                    olt.SetVVMText(movedata.text);
                }
                if (movedata.animationType == AF_MOVETYPE.TextProperty)
                {
                    olt.SetFontSize(movedata.fontSize);
                    olt.SetFontStyles(movedata.fontStyles);
                    olt.SetFontColor(movedata.color);
                    olt.SetTextAlignment(movedata.textAlignmentOptions);
                    olt.SetTextOverflow(movedata.textOverflow);
                    
                    olt.SetEnableColorGradient(movedata.IsGradient);
                    string[] gradientDirs = { "tl", "tr", "bl", "br" };
                    for (int i = 0; i < movedata.gradients.Length; i++)
                    {
                        olt.SetColorGradient(gradientDirs[i], movedata.gradients[i]);
                    }
                    olt.SetFontOutlineWidth(movedata.fontOutlineWidth);
                    olt.SetFontOutlineColor(movedata.fontOutlineColor);
                    
                }
                
                
            }
            /*Text ui = naa.avatar.GetComponent<Text>();

            if (movedata.animationType == AF_MOVETYPE.Text)
            {
                if (options.isExecuteForDOTween == 1) seq.Join(ui.DOText(movedata.text, frame.duration));
                ui.text = movedata.text;
            }
            else if (movedata.animationType == AF_MOVETYPE.TextProperty)
            {
                if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => ui.fontSize, x => ui.fontSize = x, movedata.fontSize, frame.duration));
                else ui.fontSize = movedata.fontSize;                
                

                if (options.isExecuteForDOTween == 1) seq.Join(ui.DOColor(movedata.color, frame.duration));
                else ui.color = movedata.color;
            }*/

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

                    switch (movedata.animPlaying)
                    {
                        case UserAnimationState.Play:
                            if (ola.IsSE)
                            {
                                ola.PlaySe();
                            }
                            else
                            {
                                ola.PlayAudio();
                            }
                            break;
                        case UserAnimationState.Seeking:
                            break;
                        case UserAnimationState.Pause:
                            ola.PauseAudio();
                            break;
                        case UserAnimationState.Stop:
                            ola.StopAudio();
                            break;
                    }

                }
                /*
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

                }*/
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
                        switch (movedata.animPlaying)
                        {
                            case UserAnimationState.Play:
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
                                break;
                            case UserAnimationState.Seeking:
                                seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                                {
                                    //ola.IsSE = movedata.isSE == 1 ? true : false;
                                    ola.SetAudio(movedata.audioName);

                                    ola.SetSeekSeconds(movedata.seekTime);
                                    ola.SetPlayFlag(UserAnimationState.Seeking);
                                }, false));
                                break;
                            case UserAnimationState.Pause:
                                seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                                {
                                    //ola.IsSE = movedata.isSE == 1 ? true : false;
                                    ola.SetAudio(movedata.audioName);

                                    ola.SetSeekSeconds(movedata.seekTime);
                                    ola.PauseAudio();
                                    ola.SetPlayFlag(UserAnimationState.Pause);
                                }, false));
                                break;
                            case UserAnimationState.Stop:
                                seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                                {
                                    //ola.IsSE = movedata.isSE == 1 ? true : false;
                                    ola.SetAudio(movedata.audioName);

                                    ola.SetSeekSeconds(movedata.seekTime);
                                    ola.StopAudio();
                                    ola.SetPlayFlag(UserAnimationState.Stop);
                                }, false));
                                break;
                            default:
                                seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                                {
                                    //ola.IsSE = movedata.isSE == 1 ? true : false;
                                    ola.SetAudio(movedata.audioName);

                                    ola.SetSeekSeconds(movedata.seekTime);
                                    ola.SetPlayFlag(UserAnimationState.Playing);
                                }, false));
                                break;
                        }
                        

                    }
                    /*
                    else if (movedata.animationType == AF_MOVETYPE.AnimPause)
                    {
                        
                    }
                    else if (movedata.animationType == AF_MOVETYPE.AnimSeek)
                    {
                        

                    }
                    else if (movedata.animationType == AF_MOVETYPE.AnimStop)
                    {
                        

                    }
                    else if (movedata.animationType == AF_MOVETYPE.Rest)
                    {
                        

                    }
                    */

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
            { // execute the animation and the preview 
                if (movedata.animationType == AF_MOVETYPE.AnimStart)
                {
                    switch (movedata.animPlaying)
                    {
                        case UserAnimationState.PlayWithLoop:
                        case UserAnimationState.Play:
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
                                //ole.SetPlayFlagEffect(movedata.animPlaying);
                            }, false));
                            break;
                        case UserAnimationState.Playing:
                            break;
                        case UserAnimationState.Pause:
                            seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                            {
                                ole.PauseEffect();
                                //ole.SetPlayFlagEffect(UserAnimationState.Pause);
                                //ole.SetPlayFlagEffect(movedata.animPlaying);
                            }, false));
                            break;
                        case UserAnimationState.Stop:
                            seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                            {
                                ole.StopEffect();
                                //ole.SetPlayFlagEffect(UserAnimationState.Stop);
                                //ole.SetPlayFlagEffect(movedata.animPlaying);

                            }, false));
                            break;
                        default:
                            seq.Join(DOVirtual.DelayedCall(frame.duration, async () =>
                            {
                                await ole.SetEffectRef("Effects/" + movedata.effectGenre + "/" + movedata.effectName);

                                //ole.SetPlayFlagEffect(UserAnimationState.Playing);
                                //ole.SetPlayFlagEffect(movedata.animPlaying);

                            }, false));
                            break;
                    }
                    
                    
                }
                /*
                else if (movedata.animationType == AF_MOVETYPE.AnimPause)
                {
                    
                    
                }
                else if (movedata.animationType == AF_MOVETYPE.AnimStop)
                {
                    
                }
                else if (movedata.animationType == AF_MOVETYPE.Rest)
                {
                    
                }
                */
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
            { //---execute the preview
                if (movedata.animationType == AF_MOVETYPE.Collider)
                {
                    ole.IsVRMCollider = movedata.isVRMCollider == 1 ? true : false;
                    ole.ResetColliderTarget(movedata.VRMColliderTarget);
                    ole.VRMColliderSize = movedata.VRMColliderSize;
                }
                else if (
                    (movedata.animationType == AF_MOVETYPE.AnimStart) ||
                    (movedata.animationType == AF_MOVETYPE.AnimPause) ||
                    (movedata.animationType == AF_MOVETYPE.AnimStop) ||
                    (movedata.animationType == AF_MOVETYPE.Rest)
                )
                {
                    ole.SetPlayFlagEffect(movedata.animPlaying);
                    //Debug.Log("animPlaying=" + ((int)movedata.animPlaying).ToString());
                    DOVirtual.DelayedCall(0.001f, async () =>
                    {
                        await ole.SetEffectRef("Effects/" + movedata.effectGenre + "/" + movedata.effectName);
                        
                    },false);

                    
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
                    bool isEnable = false;
                    //movedata.animationType == AF_MOVETYPE.SystemEffectOff ? false : true;
                    if (movedata.animPlaying == UserAnimationState.Play)
                    {
                        isEnable = true;
                    }

                    //Debug.Log("Fukugen SystemEffect=" + effectName);
                    //Debug.Log("  enabled=" + (isEnable ? "true" : "false"));
                    //if (movedata.effectValues.Count > 0)
                    //{
                    //    Debug.Log("  value=" + movedata.effectValues[0].ToString());
                    //}
                    if (options.isBuildDoTween == 1)
                    {
                        seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                        {
                            mse.EnablePostProcessing(effectName + "," + (isEnable ? "1" : "0"));
                        },false));
                        seq = mse.SetEffectValues(seq, effectName, movedata.effectValues, isEnable, true, frame.duration);
                    }
                    else
                    {
                        mse.EnablePostProcessing(effectName + "," + (isEnable ? "1" : "0"));
                        seq = mse.SetEffectValues(seq, effectName, movedata.effectValues, isEnable, true, frame.duration);
                        mse.SetEffectBackup(effectName, movedata.effectValues, isEnable);
                        
                    }
                }
            }


            return seq;
        }
        private Sequence ParseForStage(Sequence seq, NativeAnimationFrame frame, AnimationTargetParts movedata, NativeAnimationFrameActor targetObjects, AnimationParsingOptions options)
        {
            NativeAnimationAvatar naa = targetObjects.avatar;
            OperateStage os = naa.avatar.GetComponent<OperateStage>();

            //---Stage
            
            if (movedata.animationType == AF_MOVETYPE.Stage)
            {
                StageKind skind = (StageKind)movedata.stageType;

                if (options.isExecuteForDOTween == 1)
                {
                    
                    if (
                        (skind == StageKind.Default) ||
                        (skind == StageKind.User) ||
                        (skind == StageKind.BasicSeaLevel) ||
                        (skind == StageKind.SeaDaytime) ||
                        (skind == StageKind.SeaNight)
                    )
                    {
                        seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                        {
                            os.SelectStage(movedata.stageType);
                        }, false));

                    }
                    else
                    {
                        seq.Join(DOVirtual.DelayedCall(frame.duration, async () =>
                        {
                            await os.SelectStageRef(movedata.stageType);
                        }, false));
                    }
                    
                    
                }
                if (options.isBuildDoTween == 0)
                {
                    //--- below method is to set a value for saving and apply to UI !!!ONLY!!!                  
                    if (
                        (skind == StageKind.Default) ||
                        (skind == StageKind.User) ||
                        (skind == StageKind.BasicSeaLevel) ||
                        (skind == StageKind.SeaDaytime) ||
                        (skind == StageKind.SeaNight)
                    )
                    {
                        os.SelectStage(movedata.stageType);
                    }
                    else
                    {
                        DOVirtual.DelayedCall(0.00001f, async () =>
                        {
                            await os.SelectStageRef(movedata.stageType);
                        }, false);
                    }
                    
                }
            }
            
            //---Stage property
            if (movedata.animationType == AF_MOVETYPE.StageProperty)
            {
                StageKind skind = (StageKind)movedata.stageType;

                //===During an animation
                if (options.isExecuteForDOTween == 1)
                {
                    /*seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        os.SelectStage(movedata.stageType);
                    }, false));*/

                    if (skind == StageKind.Default)
                    {
                        //seq = os.SetDefaultStageColorTween(seq, movedata.color, frame.duration);

                    }
                    else if (skind == StageKind.User)
                    {

                        //---temporary change for next material setting.
                        os.SelectStage(movedata.stageType);

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
                    else if ((skind == StageKind.SeaDaytime) || (skind == StageKind.SeaNight) || (skind == StageKind.BasicSeaLevel))
                    {

                        //---temporary change for next material setting.
                        os.SelectStage(movedata.stageType);

                        seq = os.SetMaterialTween(seq, skind, movedata.vmatProp.name, movedata.vmatProp, frame.duration);
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
                //===Preview or show a Frame 
                if (options.isBuildDoTween == 0)
                {
                    //--- below method is to set a value for saving and apply to UI !!!ONLY!!!
                    //os.SelectStage(movedata.stageType);

                    if (skind == StageKind.Default)
                    {
                        //if (os.GetActiveStageMaterial() != null)
                        //{
                            //os.SetDefaultStageColor(movedata.color);
                            //os.SetDefaultStageColorObject(movedata.color);
                        //}
                    }
                    else if (skind == StageKind.User)
                    {
                        os.SetMaterialObjectToUserStage("metallic," + movedata.userStageMetallic);
                        os.SetMaterialObjectToUserStage("glossiness," + movedata.userStageGlossiness);
                        os.SetMaterialObjectToUserStage("emissioncolor," + movedata.userStageEmissionColor);

                        if (movedata.matProp.Count > 1)
                        {
                            os.SetMaterialObjectToUserStage("main," + movedata.matProp[0].texturePath);
                            os.SetMaterialObjectToUserStage("normal," + movedata.matProp[1].texturePath);
                        }

                    }
                    else if ((skind == StageKind.SeaDaytime) || (skind == StageKind.SeaNight) || (skind == StageKind.BasicSeaLevel))
                    {

                        //os.SetWaterWaveSpeed(movedata.wavespeed);
                        //os.SetWaterWaveScale(movedata.wavescale);

                        os.SetUserMaterialObject(movedata.vmatProp);

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
                LensFlare flare = oll.OwnFlare;

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

                //---Halo
                if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => RenderSettings.haloStrength, x => RenderSettings.haloStrength = x, movedata.halo, frame.duration));
                else oll.SetHalo(movedata.halo);

                //---flareType
                seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                {
                    oll.SetFlare(movedata.flareType);
                }, false));

                //---flareColor
                if (options.isExecuteForDOTween == 1)
                {
                    seq.Join(DOVirtual.DelayedCall(frame.duration, () =>
                    {
                        oll.SetFlareColor(movedata.flareColor);
                    }, false));
                }
                else
                {
                    oll.SetFlareColor(movedata.flareColor);
                }

                //---flareBrightness
                if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => flare.brightness, x => flare.brightness = x, movedata.flareBrightness, frame.duration)); 
                else oll.SetFlareBrightness(movedata.flareBrightness);

                //---flare fade
                if (options.isExecuteForDOTween == 1) seq.Join(DOTween.To(() => flare.fadeSpeed, x => flare.fadeSpeed = x, movedata.flareFade, frame.duration));
                else oll.SetFlareBrightness(movedata.flareBrightness);
            }




            return seq;
        }

//===========================================================================================================================
//  Register functions
//===========================================================================================================================
        public void RegisterFrameFromOuter(string param)
        {
            AnimationRegisterOptions aro = JsonUtility.FromJson<AnimationRegisterOptions>(param);
            RegisterFrame(aro);
        }

        /// <summary>
        /// Register now pose states as key-frame 
        /// </summary>
        /// <param name="aro"></param>
        public void RegisterFrame(AnimationRegisterOptions aro)
        {
            if (currentProject.isReadOnly || currentProject.isSharing) return;

            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }


            AF_TARGETTYPE realtype = aro.targetType;

            AROOperator aroo = new AROOperator();
            bool isHitTranslate = aroo.FindMoveType(aro, AF_MOVETYPE.Translate) > -1;
            bool isHitNormalTranslate = aroo.FindMoveType(aro, AF_MOVETYPE.NormalTransform) > -1;
            bool isHitProperties = aroo.FindMoveType(aro, AF_MOVETYPE.AllProperties) > -1;

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

                            NativeAnimationFrame fr = SaveFrameData(aro.index, (findex == -1 ? null : actor.frames[findex]), nearmin, actor, aro);
                            if (findex == -1)
                            {
                                actor.frames.Add(fr);
                            }
                            else
                            {
                                //---recover remained settings
                                if (aro.ease == Ease.Unset)
                                {
                                    fr.ease = actor.frames[findex].ease;
                                }
                                else
                                {
                                    fr.ease = aro.ease;
                                }
                                

                                actor.frames[findex] = null;
                                actor.frames[findex] = fr;
                            }
                            SortActorFrames(actor);
                            adjustNearMaxFrameIndex(actor, fr);

                            //---for confirming
                            AvatarAttachedNativeAnimationFrame aaFrame = new AvatarAttachedNativeAnimationFrame(actor);
                            aaFrame.frame.SetFromNative(fr);
                            conFrame.frames.Add(aaFrame);

                            //---for general animation clip
                            SetGeneralAnimationFrame(actor.avatar, aro.index, fr);
                            SetVRMAnimationFrame(actor.avatar, aro.index, fr);
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
                        NativeAnimationFrame fr = null;

                        if (findex == -1)
                        { //---new add keyframe
                            fr = SaveFrameData(aro.index, null, nearmin, actor, aro);

                            actor.frames.Add(fr);
                            SortActorFrames(actor);
                            //---adjust duration and index to frame near maximumly, only newly add.
                            adjustNearMaxFrameIndex(actor, fr);

                        }
                        else
                        { //---overwrite keyframe
                            NativeAnimationFrame existedFrame = null;
                            Ease bkupease = Ease.Linear;
                            float bkupduration = 0.01f;
                            if (findex < actor.frames.Count)
                            {
                                existedFrame = actor.frames[findex];
                                bkupease = existedFrame.ease;
                                bkupduration = existedFrame.duration;
                            }
                            fr = SaveFrameData(aro.index, existedFrame, nearmin, actor, aro);

                            //---recover remained settings
                            if (existedFrame != null)
                            {
                                fr.ease = bkupease; // actor.frames[findex].ease;
                                fr.duration = bkupduration; // actor.frames[findex].duration;
                            }
                            

                            actor.frames[findex] = null;
                            actor.frames[findex] = fr;
                            SortActorFrames(actor);

                        }

                        /*NativeAnimationAvatar nav = GetFrameActorFromObjectID(aro.targetId, realtype);
                        NativeAnimationFrameActor fr = SaveFrameData(aro.index, nearmin, nav, aro);
                        nfgrp.characters.Add(fr);*/
                        //---for confirming
                        List<int> tmpmovarr = new List<int>();
                        AvatarAttachedNativeAnimationFrame aaFrame = new AvatarAttachedNativeAnimationFrame(actor);
                        aaFrame.frame.SetFromNative(fr);
                        if (fr.translateMovingData.Count > 0)
                        {
                            aaFrame.translateMoving = fr.translateMovingData[0].values.Count;                            
                        }
                        else
                        {
                            aaFrame.translateMoving = 0;
                        }
                        //---returning content type
                        if (isHitTranslate) tmpmovarr.Add((int)AF_MOVETYPE.Translate);
                        if (isHitNormalTranslate) tmpmovarr.Add((int)AF_MOVETYPE.NormalTransform);
                        if (isHitProperties) tmpmovarr.Add((int)AF_MOVETYPE.AllProperties);
                        aaFrame.MovingTypes = tmpmovarr;
                        
                        
                        conFrame.frames.Add(aaFrame);

                        //---for general animation clip
                        SetGeneralAnimationFrame(actor.avatar, aro.index, fr);
                        SetVRMAnimationFrame(actor.avatar, aro.index, fr);

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
                        //---for general animation clip
                        SetGeneralAnimationFrame(actor.avatar, aro.index, frame);
                        SetVRMAnimationFrame(actor.avatar, aro.index, frame);
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

                        //---for general animation clip
                        SetGeneralAnimationFrame(actor.avatar, aro.index, frame);
                        SetVRMAnimationFrame(actor.avatar, aro.index, frame);
                    }
                }
            }


        }

        /// <summary>
        /// To change frame position 
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
                        AdjustAllFrame(actor, currentProject.baseDuration, false, true);
                        
                    }
                    adjustNearMaxFrameIndex(actor, actor.frames[findex]);

                    if (actor.avatar.type == AF_TARGETTYPE.VRM)
                    {
                        //---for general animation clip
                        VVMMotionRecorder vmrec = actor.avatar.avatar.GetComponent<VVMMotionRecorder>();
                        if (vmrec != null)
                        {
                            vmrec.RemoveKeyFrame(oldindex);
                            vmrec.AddKeyFrame(newindex, actor.frames[findex].ease, actor.frames[findex].duration);
                            vmrec.RemoveKeyFrameVRMA(oldindex);
                            vmrec.AddKeyFrameVRMA(newindex, actor.frames[findex].ease, actor.frames[findex].duration);
                        }
                    }
                    

                    ret = findex;
                }
                
            }
            
            //---returning value is an index of the Array ( not frame number )
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveIntVal(ret);
#endif
        }

        /// <summary>
        /// To insert frame. move previous/next frame position.
        /// </summary>
        /// <param name="param">csv string - [0] base frame index, [1] insert number, [2] insertion type (r - to right)</param>
        public void InsertFrameDuring(string param)
        {
            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }

            string ret = "";

            string[] prm = param.Split(',');
            int newindex = int.TryParse(prm[0], out newindex) ? newindex : -1;
            int insertCount = int.TryParse(prm[1], out insertCount) ? insertCount : 0;
            string directiontype = prm[2] == "" ? "r" : prm[2]; //default is to right.

            currentProject.timeline.characters.ForEach(chara =>
            {
                if (newindex > -1)
                {
                    if (chara.avatar.type == AF_TARGETTYPE.VRM)
                    {
                        VVMMotionRecorder vmrec = chara.avatar.avatar.GetComponent<VVMMotionRecorder>();
                        ManageAvatarTransform mat = chara.avatar.avatar.GetComponent<ManageAvatarTransform>();

                        for (int i = 0; i < chara.frames.Count; i++)
                        {
                            NativeAnimationFrame naf = chara.frames[i];
                            if (naf.index >= newindex)
                            {
                                if (directiontype == "r")
                                {
                                    naf.index += insertCount;
                                    naf.finalizeIndex += insertCount;
                                }

                            }
                        }
                        vmrec.InsertBlankFrame(newindex);
                        mat.recbvh.InsertFrame(newindex);
                    }
                    else
                    {
                        for (int i = 0; i < chara.frames.Count; i++)
                        {
                            NativeAnimationFrame naf = chara.frames[i];
                            if (naf.index >= newindex)
                            {
                                if (directiontype == "r")
                                {
                                    naf.index += insertCount;
                                    naf.finalizeIndex += insertCount;
                                }

                            }
                        }
                    }
                    
                }
                
            });
            currentProject.timelineFrameLength += insertCount;
            ret = newindex.ToString() + "," + insertCount.ToString();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
        /// <summary>
        /// To delete frame. move next frame to left.
        /// </summary>
        /// <param name="param">csv string - [0] base frame index, [1] insert number</param>
        public void DeleteFrame(string param)
        {
            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }

            string ret = "";

            string[] prm = param.Split(',');
            int newindex = int.TryParse(prm[0], out newindex) ? newindex : -1;
            int deleteCount = int.TryParse(prm[1], out deleteCount) ? deleteCount : 0;

            currentProject.timeline.characters.ForEach(chara => 
            {
                
                

                int justhit = -1;
                if (newindex > -1)
                {
                    for (int i = 0; i < chara.frames.Count; i++)
                    {
                        NativeAnimationFrame naf = chara.frames[i];
                        if (naf.index == newindex)
                        {
                            justhit = i;
                        }
                        else if (naf.index > newindex)
                        {
                            naf.index -= deleteCount;
                            naf.finalizeIndex -= deleteCount;
                        }
                    }
                    if (justhit > -1)
                    {
                        if (chara.avatar.type == AF_TARGETTYPE.VRM)
                        {
                            VVMMotionRecorder vmrec = chara.avatar.avatar.GetComponent<VVMMotionRecorder>();
                            ManageAvatarTransform mat = chara.avatar.avatar.GetComponent<ManageAvatarTransform>();
                            mat.recbvh.RemoveFrame(newindex, true);
                            vmrec.RemoveKeyFrame(newindex, true);
                            vmrec.RemoveKeyFrameVRMA(newindex, true);
                        }
                        chara.frames.RemoveAt(justhit);
                        
                    }
                }
            });
            currentProject.timelineFrameLength -= deleteCount;

            ret = newindex.ToString() + "," + deleteCount.ToString();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
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

            AROOperator aroo = new AROOperator();
            bool isHitTranslate = aroo.FindMoveType(aro, AF_MOVETYPE.Translate) > -1;
            bool isHitNormalTranslate = aroo.FindMoveType(aro, AF_MOVETYPE.NormalTransform) > -1;
            bool isHitProperties = aroo.FindMoveType(aro, AF_MOVETYPE.AllProperties) > -1;

            if (aro.index != -1)
            {
                if (aro.targetId == "")
                { //---all characters  ***NOT USE***
                    foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
                    {
                        int hitindex = actor.frames.FindIndex(match =>
                        {
                            if (match.index == aro.index) return true;
                            return false;
                        });
                        if (hitindex > -1)
                        {
                            NativeAnimationFrame frame = actor.frames[hitindex];
                            if (isHitProperties)
                            {
                                int mdinx = actor.frames[hitindex].movingData.Count - 1;
                                for (int i = mdinx; i >= 0; i--)
                                {
                                    AnimationTargetParts atp = actor.frames[hitindex].movingData[i];
                                    if ((atp.animationType != AF_MOVETYPE.Translate) && (atp.animationType != AF_MOVETYPE.Rotate) && (atp.animationType != AF_MOVETYPE.Scale) &&
                                         (atp.animationType != AF_MOVETYPE.Punch) && (atp.animationType != AF_MOVETYPE.Shake)
                                    )
                                    {
                                        actor.frames[hitindex].movingData.RemoveAt(i);
                                    }
                                }
                            }
                            else
                            {
                                actor.frames.RemoveAt(hitindex);
                            }
                            
                        }
                        //---adjust duration and finalIndex
                        AdjustAllFrame(actor, currentProject.baseDuration, false, true);
                    }
                }
                else
                { //--- indicated character only
                    NativeAnimationFrameActor actor = GetFrameActorFromObjectID(aro.targetId, aro.targetType);
                    if (actor != null)
                    {
                        int hitindex = actor.frames.FindIndex(match =>
                        {
                            if (match.index == aro.index) return true;
                            return false;
                        });
                        if (hitindex > -1)
                        {
                            NativeAnimationFrame frame = actor.frames[hitindex];
                            if (isHitProperties)
                            {
                                int mdinx = actor.frames[hitindex].movingData.Count - 1;
                                for (int i = mdinx; i >= 0; i--)
                                {
                                    AnimationTargetParts atp = actor.frames[hitindex].movingData[i];
                                    if ((atp.animationType != AF_MOVETYPE.Translate) && (atp.animationType != AF_MOVETYPE.Rotate) && (atp.animationType != AF_MOVETYPE.Scale) &&
                                         (atp.animationType != AF_MOVETYPE.Punch) && (atp.animationType != AF_MOVETYPE.Shake)
                                    )
                                    {
                                        actor.frames[hitindex].movingData.RemoveAt(i);
                                    }
                                }
                            }
                            else
                            {
                                actor.frames.RemoveAt(hitindex);

                                if (actor.avatar.type == AF_TARGETTYPE.VRM)
                                {
                                    VVMMotionRecorder vmrec = actor.avatar.avatar.GetComponent<VVMMotionRecorder>();
                                    vmrec.RemoveKeyFrame(aro.index);
                                    vmrec.RemoveKeyFrameVRMA(aro.index);

                                    ManageAvatarTransform mat = actor.avatar.avatar.GetComponent<ManageAvatarTransform>();
                                    mat.recbvh.RemoveFrame(aro.index);
                                }
                            }
                            
                            
                        }
                        //---adjust duration and finalIndex
                        AdjustAllFrame(actor, currentProject.baseDuration, false, true);


                    }
                }

            }
        }

        public void DeleteChildKey(string param)
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
                            for (int i = 0; i < frame.translateMovingData.Count; i++) 
                            {
                                frame.translateMovingData[i].values.RemoveAt(aro.addTranslateExecuteIndex);
                            }
                            

                            if (actor.avatar.type == AF_TARGETTYPE.VRM)
                            {
                                VVMMotionRecorder vmrec = actor.avatar.avatar.GetComponent<VVMMotionRecorder>();
                                vmrec.RemoveKeyFrame(aro.index);

                                ManageAvatarTransform mat = actor.avatar.avatar.GetComponent<ManageAvatarTransform>();
                                mat.recbvh.RemoveFrame(aro.index);
                            }

                        }
                        //---adjust duration and finalIndex
                        AdjustAllFrame(actor, currentProject.baseDuration, false, true);


                    }
                }

            }
        }

        /// <summary>
        /// Set up motion data as general animation clip and bvh
        /// </summary>
        /// <param name="avatar"></param>
        /// <param name="index"></param>
        /// <param name="frame"></param>
        public void SetGeneralAnimationFrame(NativeAnimationAvatar avatar, int index, NativeAnimationFrame frame)
        {
            if (avatar.type == AF_TARGETTYPE.VRM)
            {
                //---for general animation clip
                VVMMotionRecorder vmrec = avatar.avatar.GetComponent<VVMMotionRecorder>();
                if (vmrec != null)
                {
                    if (vmrec.IsExistFrame(index))
                    {
                        vmrec.ModifyKeyFrame(index, frame.ease, frame.duration);
                    }
                    else
                    {
                        vmrec.AddKeyFrame(index, frame.ease, frame.duration);
                    }
                }
                //---for BVH format
                ManageAvatarTransform mat = avatar.avatar.GetComponent<ManageAvatarTransform>();
                if (mat != null)
                {
                    if (mat.recbvh.SearchFrame(index) > -1)
                    {
                        mat.recbvh.ModifyFrame(index);
                    }
                    else
                    {
                        mat.recbvh.captureFrame(index);
                    }

                }
            }
            
        }
        public void SetVRMAnimationFrame(NativeAnimationAvatar avatar, int index, NativeAnimationFrame frame)
        {
            if (avatar.type == AF_TARGETTYPE.VRM)
            {
                //---for general animation clip
                VVMMotionRecorder vmrec = avatar.avatar.GetComponent<VVMMotionRecorder>();
                if (vmrec != null)
                {
                    if (vmrec.IsExistFrameVRMA(index))
                    {
                        vmrec.ModifyFrameVRMA(index, frame.ease, frame.duration);
                    }
                    else
                    {
                        vmrec.AddKeyFrameVRMA(index, frame.ease, frame.duration);
                    }
                }
                
            }
        }

        /// <summary>
        /// Clean up general animation clip and bvh
        /// </summary>
        /// <param name="avatar"></param>
        public void ClearGenerateAnimationFrame(NativeAnimationAvatar avatar)
        {
            if (avatar.type == AF_TARGETTYPE.VRM)
            {
                //---for general animation clip
                VVMMotionRecorder vmrec = avatar.avatar.GetComponent<VVMMotionRecorder>();
                if (vmrec != null)
                {
                    vmrec.ClearCurves();
                }
                //---for BVH format
                ManageAvatarTransform mat = avatar.avatar.GetComponent<ManageAvatarTransform>();
                if (mat != null)
                {
                    mat.recbvh.clearCapture();
                }
            }
            
        }
        public void ClearVRMAnimationFrame(NativeAnimationAvatar avatar)
        {
            if (avatar.type == AF_TARGETTYPE.VRM)
            {
                //---for general animation clip
                VVMMotionRecorder vmrec = avatar.avatar.GetComponent<VVMMotionRecorder>();
                if (vmrec != null)
                {
                    vmrec.ClearAllFrames4VRMA();
                }

            }
        }

        /// <summary>
        /// PRIVATE: append to frame as AnimationTargetParts, only Translate motion.
        /// </summary>
        /// <param name="aframe"></param>
        /// <param name="existedFrame"></param>
        /// <param name="pi"></param>
        /// <param name="movetype"></param>
        /// <param name="cmn"></param>
        private NativeAnimationFrame RegisterAppendTranslateMotion(AnimationRegisterOptions options, NativeAnimationFrame aframe, NativeAnimationFrame existedFrame, ParseIKBoneType pi, AF_MOVETYPE movetype, AnimationTargetParts cmn)
        {
            NativeAnimationFrame ret = null;

            int hitindex = (existedFrame == null ? -1 : existedFrame.FindIndexTranslateMoving(AF_MOVETYPE.Translate, pi) ); 
            if (options.isRegisterAppend == 0)
            {
                //---if not found existed, directly create NEW "Translate"
                AnimationTranslateTargetParts attp = new AnimationTranslateTargetParts(cmn.vrmBone, cmn.animationType);
                attp.jumpNum = cmn.jumpNum;
                attp.jumpPower = cmn.jumpPower;
                attp.values.Add(cmn.position);

                int nominx = aframe.FindIndexTranslateMoving(AF_MOVETYPE.Translate, pi);
                if (nominx == -1)
                {
                    aframe.translateMovingData.Add(attp);
                }
                else
                {
                    aframe.translateMovingData[nominx] = attp;
                }
                

                ret = aframe;
            }
            else
            { //---register appending 
                if (existedFrame == null)
                { //---if not found existed, directly create NEW "Translate"  
                    AnimationTranslateTargetParts attp = new AnimationTranslateTargetParts(cmn.vrmBone, cmn.animationType);
                    attp.jumpNum = cmn.jumpNum;
                    attp.jumpPower = cmn.jumpPower;
                    attp.values.Add(cmn.position);

                    int nominx = aframe.FindIndexTranslateMoving(AF_MOVETYPE.Translate, pi);
                    if (nominx == -1)
                    {
                        aframe.translateMovingData.Add(attp);
                    }
                    else
                    {
                        aframe.translateMovingData[nominx] = attp;
                    }

                    ret = aframe;
                }
                else
                {
                    var existedMov = existedFrame.FindTranslateMoving(AF_MOVETYPE.Translate, pi);
                    if ((existedMov != null) && (existedMov.values.Count > 0))
                    { //---add this time data to existed data.

                        if ((options.addTranslateExecuteIndex == -1) || (options.addTranslateExecuteIndex >= existedMov.values.Count))
                        { //---normal add
                            existedFrame.translateMovingData[hitindex].values.Add(cmn.position);
                        }
                        else
                        { //---overwrite indicated indexed values.
                            existedFrame.translateMovingData[hitindex].values[options.addTranslateExecuteIndex] = cmn.position;
                        }
                        
                        ret = existedFrame;
                    }
                    else
                    { //---if not found existed, directly create NEW "Translate"
                        AnimationTranslateTargetParts attp = new AnimationTranslateTargetParts(cmn.vrmBone, cmn.animationType);
                        attp.jumpNum = cmn.jumpNum;
                        attp.jumpPower = cmn.jumpPower;
                        attp.values.Add(cmn.position);

                        int nominx = aframe.FindIndexTranslateMoving(AF_MOVETYPE.Translate, pi);
                        if (nominx == -1)
                        {
                            aframe.translateMovingData.Add(attp);
                        }
                        else
                        {
                            aframe.translateMovingData[nominx] = attp;
                        }

                        ret = aframe;
                    }
                }
                
            }
                
            return ret;
        }
        public NativeAnimationFrame CheckAndLoopAnimationTargetParts(NativeAnimationFrame aframe, List<AnimationTargetParts> atps, AnimationRegisterOptions options, bool onlyexist = false)
        {
            //TODO: ssytemeffect = 7のはずが2に潰れてる。Findの検索がおかしい。
            {
                foreach (AnimationTargetParts cmn in atps)
                {
                    int ishit_mv = aframe.movingData.FindIndex(match =>
                    {
                        if ((match.vrmBone == cmn.vrmBone) && (match.animationType == cmn.animationType)) return true;
                        return false;
                    });
                    //---not found, add newly
                    if (ishit_mv == -1)
                    {
                        aframe.movingData.Add(cmn);
                    }
                    //---found, overwrite same index
                    else
                    {
                        aframe.movingData[ishit_mv] = cmn;
                    }
                }
            }
            
            return aframe;
        }
        public NativeAnimationFrame CheckAndLoopSystemEffect_ATP(NativeAnimationFrame aframe, List<AnimationTargetParts> atps, AnimationRegisterOptions options, bool onlyexist = false)
        {
            //TODO: ssytemeffect = 7のはずが2に潰れてる。Findの検索がおかしい。
            {
                foreach (AnimationTargetParts cmn in atps)
                {
                    int ishit_mv = aframe.movingData.FindIndex(match =>
                    {
                        if ((match.vrmBone == cmn.vrmBone) && (match.animationType == cmn.animationType) && (match.effectName == cmn.effectName)) return true;
                        return false;
                    });
                    //---not found, add newly
                    if (ishit_mv == -1)
                    {
                        aframe.movingData.Add(cmn);
                    }
                    //---found, overwrite same index
                    else
                    {
                        aframe.movingData[ishit_mv] = cmn;
                    }
                }
            }

            return aframe;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="frameNumber">internal frame index</param>
        /// <param name="existedFrame">existed target frame</param>
        /// <param name="nearMinIndex">most nearly minimum frame index</param>
        /// <param name="actor">Frame actor</param>
        /// <param name="options">register option</param>
        /// <returns>overwriting frame data (perhaps, same as existedFrame)</returns>
        public NativeAnimationFrame SaveFrameData(int frameNumber, NativeAnimationFrame existedFrame, int nearMinIndex, NativeAnimationFrameActor actor, AnimationRegisterOptions options)
        {
            AROOperator aroo = new AROOperator();
            bool isHitTranslate = aroo.FindMoveType(options, AF_MOVETYPE.Translate) > -1;
            bool isHitNormalTranslate = aroo.FindMoveType(options, AF_MOVETYPE.NormalTransform) > -1;
            bool isHitProperties = aroo.FindMoveType(options, AF_MOVETYPE.AllProperties) > -1;

            //---Save common information for each object
            NativeAnimationFrame aframe = null;
            if (existedFrame != null)
            {
                aframe = existedFrame;
            }
            else
            {
                aframe = new NativeAnimationFrame();
                //aframe.movingData = new List<AnimationTargetParts>();
            }
            
            
            aframe.index = frameNumber;
            aframe.finalizeIndex = frameNumber;
            aframe.ease = options.ease;
            aframe.memo = options.memo;

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


            //---calculate a duration

            //int[] dist = GetDistanceFromPreviousFrame(frameNumber);
            NativeAnimationFrame oldframe = GetFrame(actor, frameNumber);

            //---calculate baseDuration * distance of previous Frame and current Frame.
            int dist = 1;
            if ((actor.frames.Count > 0) && (nearMinIndex != -1))
            {
                dist = frameNumber - (actor.frames[nearMinIndex].index);
            }
            if (options.duration > 0f)
            {
                aframe.duration = options.duration;
            }
            else
            {
                aframe.duration = (currentProject.baseDuration == 0f ? 0.001f : currentProject.baseDuration) * (float)dist;
            }
            
            if (nearMinIndex != -1)
            {
                aframe.finalizeIndex = actor.frames[nearMinIndex].finalizeIndex;
            }


            //aframe.enabled = 1;


            if (actor.targetType == AF_TARGETTYPE.SystemEffect)
            {
                var retsys = PackForSystemEffect(aframe, actor, options, oldframe);
                //aframe.movingData.AddRange(retsys);
                aframe = CheckAndLoopSystemEffect_ATP(aframe, retsys, options);
            }
            else if (actor.targetType == AF_TARGETTYPE.Audio)
            {
                var retaud = PackForAudio(aframe, actor, options, oldframe);
                //aframe.movingData.AddRange(retaud);
                aframe = CheckAndLoopAnimationTargetParts(aframe, retaud, options);
            }
            else
            {
                //---Each save process for each object
                var retcmn = PackForCommon(aframe, actor, options, oldframe);
                //---parts for translate
                var retlstTranslate = retcmn.FindAll(m =>
                {
                    if (m.animationType == AF_MOVETYPE.Translate) return true;
                    return false;
                });
                //---parts for rotate, scale, etc...
                var retlstOTTranslate = retcmn.FindAll(m =>
                {
                    if (m.animationType != AF_MOVETYPE.Translate) return true;
                    return false;
                });

                //---only Translate loop
                if (isHitTranslate)
                {
                    foreach (AnimationTargetParts cmn in retlstTranslate)
                    {
                        //IKParent ~ RightLeg, this time
                        if (aroo.FindBoneType(options, cmn.vrmBone) > -1)
                        {
                            aframe = RegisterAppendTranslateMotion(options, aframe, aframe, cmn.vrmBone, AF_MOVETYPE.Translate, cmn);

                        }
                        /*
                        * return: aframe = new aframe OR existedFrame
                        * next loop: 
                        *   existedFrame = before looped existedFrame (via Pointer)
                        *   aframe = new aframe OR existedFrame
                        * 
                        */

                        //if (actor.targetType != AF_TARGETTYPE.VRM)
                        //{
                        //    break;
                        //}
                    }
                }
                /*else
                {
                    if (existedFrame != null)
                    {
                        aframe.translateMovingData.Clear();
                        foreach (AnimationTranslateTargetParts cmn in existedFrame.translateMovingData)
                        {
                            aframe.translateMovingData.Add(cmn);
                        }
                    }
                }*/
                //---only Rotate/Scale/Punch/Shake
                if (isHitNormalTranslate)
                {
                    foreach (AnimationTargetParts cmn in retlstOTTranslate)
                    {
                        //IKParent ~ RightLeg, this time
                        if (aroo.FindBoneType(options, cmn.vrmBone) > -1)
                        {
                            int ishit_mv = aframe.movingData.FindIndex(match =>
                            {
                                if ((match.vrmBone == cmn.vrmBone) &&
                                   (match.animationType == cmn.animationType) 
                                ) return true;
                                return false;
                            });
                            //---not found, add newly
                            if (ishit_mv == -1)
                            {
                                aframe.movingData.Add(cmn);
                            }
                            //---found, overwrite same index
                            else
                            {
                                aframe.movingData[ishit_mv] = cmn;
                            }
                        }
                    }
                    //aframe = CheckAndLoopAnimationTargetParts(aframe, retlstOTTranslate, options);
                }
                /*else
                {//---use data of existed frame (NOT UPDATE)
                    if (existedFrame != null)
                    {
                        foreach (AnimationTargetParts cmn in existedFrame.movingData)
                        {
                            
                        }
                    }
                    
                }*/

                /*
                if (isHitTranslate || isHitNormalTranslate)
                { //---INCLUDE Translate/Rotate/Scale/Punch/Shake
                    foreach (AnimationTargetParts cmn in retcmn)
                    { //IKParent ~ RightLeg, this time
                        if (cmn.animationType == AF_MOVETYPE.Translate)
                        { //---"Translate" add to translateMovingData
                            
                            aframe = RegisterAppendTranslateMotion(options, aframe, existedFrame, cmn.vrmBone, AF_MOVETYPE.Translate, cmn);
                            
                        }
                        else
                        { //---directly add OTHER THAN "Translate"
                            int ishit_mv = aframe.movingData.FindIndex(match =>
                            {
                                if ((match.vrmBone == cmn.vrmBone) && (match.animationType == cmn.animationType)) return true;
                                return false;
                            });
                            //---not found, add newly
                            if (ishit_mv == -1) aframe.movingData.Add(cmn);
                            //---found, overwrite same index
                            else aframe.movingData[ishit_mv] = cmn;
                        }

                        if (actor.targetType != AF_TARGETTYPE.VRM)
                        {
                            break;
                        }

                    }
                }
                else
                { 
                }
                */


                List<AnimationTargetParts> retmot = new List<AnimationTargetParts>();
                if (actor.targetType == AF_TARGETTYPE.VRM)
                {
                    retmot = PackForVRM(aframe, actor, options, oldframe);
                    //aframe.movingData.AddRange(retvrm);
                }
                else if (actor.targetType == AF_TARGETTYPE.OtherObject)
                {
                    retmot = PackForOtherObject(aframe, actor, options, oldframe);
                    //aframe.movingData.AddRange(retobj);
                }
                else if (actor.targetType == AF_TARGETTYPE.Light)
                {
                    retmot = PackForLight(aframe, actor, options, oldframe);
                    //aframe.movingData.AddRange(retlt);
                }
                else if (actor.targetType == AF_TARGETTYPE.Camera)
                {
                    retmot = PackForCamera(aframe, actor, options, oldframe);
                    //aframe.movingData.AddRange(retcam);
                }
                else if ((actor.targetType == AF_TARGETTYPE.Text) || (actor.targetType == AF_TARGETTYPE.Text3D))
                {
                    retmot = PackForText(aframe, actor, options, oldframe);
                    //aframe.movingData.AddRange(rettxt);
                }
                else if (actor.targetType == AF_TARGETTYPE.Image)
                {
                    retmot = PackForImage(aframe, actor, options, oldframe);
                    //aframe.movingData.AddRange(retimg);
                }
                else if (actor.targetType == AF_TARGETTYPE.UImage)
                {
                    retmot = PackForUImage(aframe, actor, options, oldframe);
                    //aframe.movingData.AddRange(retuimg);
                }
                else if (actor.targetType == AF_TARGETTYPE.Effect)
                {
                    retmot = PackForEffect(aframe, actor, options, oldframe);
                    //aframe.movingData.AddRange(reteff);
                }
                else if (actor.targetType == AF_TARGETTYPE.Stage)
                {
                    retmot = PackForStage(aframe, actor, options, oldframe);
                    //aframe.movingData.AddRange(retstg);
                }

                bool onlyexist = false;
                if (isHitProperties)
                { //---normal apply this time motion
                    onlyexist = true;
                    aframe = CheckAndLoopAnimationTargetParts(aframe, retmot, options, onlyexist);
                }
                /*else
                {
                    if (existedFrame != null)
                    {
                        foreach (var cmn in retmot)
                        { //---search existed frame by this time motion key, apply the frame like NOT UPDATE
                            var lst = existedFrame.movingData.FindAll(m =>
                            {
                                if ((m.vrmBone == cmn.vrmBone) && (m.animationType == cmn.animationType)) return true;
                                return false;
                            });
                            aframe.movingData.AddRange(lst);
                        }
                    }
                    
                }*/

            }


            return aframe;

        }
        private List<AnimationTargetParts> PackForCommon(NativeAnimationFrame frame, NativeAnimationFrameActor nact, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            List<AnimationTargetParts> movingData = new List<AnimationTargetParts>();

            if (nact.targetType == AF_TARGETTYPE.VRM)
            {
                if ((options.isBlendShapeOnly == 0) && (options.isHandOnly == 0) && (options.isTransformOnly == 0))
                {
                    OperateLoadedVRM olb = nact.avatar.avatar.GetComponent<OperateLoadedVRM>();
                    GameObject trueik = olb.relatedTrueIKParent;

                    //---most common parts
                    AnimationTargetParts[] ikp = new AnimationTargetParts[4];
                    ikp[0] = new AnimationTargetParts();
                    ikp[0].animationType = AF_MOVETYPE.Translate;
                    ikp[0].vrmBone = ParseIKBoneType.IKParent;
                    ikp[0].position = trueik.transform.position;
                    //------position only: jump parts
                    ikp[0].jumpNum = olb.GetJumpNum();
                    ikp[0].jumpPower = olb.GetJumpPower();
                    ikp[1] = new AnimationTargetParts();
                    ikp[1].animationType = AF_MOVETYPE.Rotate;
                    ikp[1].vrmBone = ParseIKBoneType.IKParent;
                    ikp[1].rotation = trueik.transform.rotation.eulerAngles;
                    ikp[1].isRotate360 = options.isRotate360;
                    ikp[2] = new AnimationTargetParts();
                    ikp[2].animationType = AF_MOVETYPE.Scale;
                    ikp[2].vrmBone = ParseIKBoneType.IKParent;
                    ikp[2].scale = nact.avatar.avatar.transform.localScale;
                    //---rigid
                    ikp[3] = new AnimationTargetParts();
                    ikp[3].animationType = AF_MOVETYPE.Rigid;
                    ikp[3].vrmBone = ParseIKBoneType.IKParent;
                    ikp[3].rigidDrag = olb.GetDrag(1);
                    ikp[3].rigidAngularDrag = olb.GetDrag(2);
                    ikp[3].useCollision = olb.GetEasyCollision();
                    ikp[3].useRigidGravity = olb.GetUseGravity();
                    
                    movingData.Add(ikp[0]);
                    movingData.Add(ikp[1]);
                    movingData.Add(ikp[2]);
                    movingData.Add(ikp[3]);
                    

                    //---common effect parts
                    AvatarPunchEffect punch = olb.GetPunch();
                    //if ((punch != null))
                    {
                        AnimationTargetParts pp = new AnimationTargetParts();
                        pp.animationType = AF_MOVETYPE.Punch;
                        pp.vrmBone = ParseIKBoneType.IKParent;
                        pp.effectPunch = punch;
                        movingData.Add(pp);
                    }
                    AvatarShakeEffect shake = olb.GetShake();
                    //if ((shake != null))
                    {
                        AnimationTargetParts pp = new AnimationTargetParts();
                        pp.animationType = AF_MOVETYPE.Shake;
                        pp.vrmBone = ParseIKBoneType.IKParent;
                        pp.effectShake = shake;
                        movingData.Add(pp);
                    }


                    //---Transform for HumanBodyBones------------------------------------------
                    if (options.isCompileForLibrary == 1) 
                    {
                        Animator animator = nact.avatar.avatar.GetComponent<Animator>();
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

                                    movingData.Add(atp[0]);
                                    movingData.Add(atp[1]);
                                }
                            }
                        }
                    }
                    


                    //---Transform for IK----------------------------------------------
                    int[] sortedIndex = new int[IKbonesCount] {
                        (int)ParseIKBoneType.IKParent,


                        (int)ParseIKBoneType.Pelvis,(int)ParseIKBoneType.Chest,
                        (int)ParseIKBoneType.Head,(int)ParseIKBoneType.Aim,(int)ParseIKBoneType.LookAt, 

                        (int)ParseIKBoneType.LeftShoulder,
                        (int)ParseIKBoneType.LeftHand, (int)ParseIKBoneType.LeftLowerArm,
                        (int)ParseIKBoneType.RightShoulder,
                        (int)ParseIKBoneType.RightHand, (int)ParseIKBoneType.RightLowerArm,

                        (int)ParseIKBoneType.LeftLeg,(int)ParseIKBoneType.LeftLowerLeg, 
                        (int)ParseIKBoneType.RightLeg,(int)ParseIKBoneType.RightLowerLeg,
                        (int)ParseIKBoneType.EyeViewHandle
                        
                    };

                    Transform[] bts = nact.avatar.ikparent.GetComponentsInChildren<Transform>();
                    for (int srti = 1; srti < sortedIndex.Length; srti++)
                    {
                        int i = sortedIndex[srti];
                        string ikname = IKBoneNames[i];
                        Transform child = null;// nact.avatar.ikparent.transform.Find(ikname);
                        foreach (Transform bt in bts)
                        {
                            if (bt.name == ikname)
                            {
                                child = bt;
                                break;
                            }
                        }
                        if (child != null)
                        {
                            Collider col = child.GetComponent<Collider>();
                            Rigidbody rig = child.GetComponent<Rigidbody>();

                            AnimationTargetParts[] atp = new AnimationTargetParts[4];
                            atp[0] = new AnimationTargetParts();
                            atp[1] = new AnimationTargetParts();
                            atp[2] = new AnimationTargetParts();
                            atp[3] = new AnimationTargetParts();
                            atp[0].vrmBone = (ParseIKBoneType)i;
                            atp[1].vrmBone = (ParseIKBoneType)i;
                            atp[2].vrmBone = (ParseIKBoneType)i;
                            atp[3].vrmBone = (ParseIKBoneType)i;
                            atp[0].animationType = AF_MOVETYPE.Translate;
                            atp[1].animationType = AF_MOVETYPE.Rotate;
                            atp[2].animationType = AF_MOVETYPE.Scale;
                            atp[3].animationType = AF_MOVETYPE.Rigid;

                            atp[0].position = child.localPosition;
                            atp[1].rotation = child.localRotation.eulerAngles;
                            atp[2].scale = child.localScale;
                            atp[3].rigidDrag = rig.drag;
                            atp[3].rigidAngularDrag = rig.angularDrag;
                            atp[3].useCollision = col.isTrigger ? 0 : 1;
                            atp[3].useRigidGravity = rig.useGravity ? 1 : 0;

                            movingData.Add(atp[0]);
                            movingData.Add(atp[1]);
                            movingData.Add(atp[2]);
                            movingData.Add(atp[3]);
                        }
                        
                    }


                }

            }
            else if ((nact.targetType == AF_TARGETTYPE.Text) || (nact.targetType == AF_TARGETTYPE.Text3D) || (nact.targetType == AF_TARGETTYPE.UImage))
            {
                //---transform is RectTransform
                RectTransform rectt = nact.avatar.avatar.GetComponent<RectTransform>();
                OperateLoadedUImage olu = nact.avatar.avatar.GetComponent<OperateLoadedUImage>();

                AnimationTargetParts[] ikp = new AnimationTargetParts[4];
                if (nact.targetType == AF_TARGETTYPE.Text3D)
                { //---3D text
                    OperateLoadedText olt = nact.avatar.avatar.GetComponent<OperateLoadedText>();
                    bool isEquip = false;
                    if (olt != null)
                    {
                        OtherObjectDummyIK ooik = olt.relatedHandleParent.GetComponent<OtherObjectDummyIK>();
                        isEquip = ooik.isEquipping;
                    }

                    Vector3 v3 = olt.GetPosition3D();

                    ikp[0] = new AnimationTargetParts();
                    ikp[0].animationType = AF_MOVETYPE.Translate;
                    ikp[0].vrmBone = ParseIKBoneType.IKParent;
                    ikp[0].position = v3;
                    movingData.Add(ikp[0]);

                    ikp[1] = new AnimationTargetParts();
                    ikp[1].animationType = AF_MOVETYPE.Rotate;
                    ikp[1].vrmBone = ParseIKBoneType.IKParent;
                    ikp[1].rotation = olt.GetRotation3D();
                    movingData.Add(ikp[1]);

                    ikp[2] = new AnimationTargetParts();
                    ikp[2].animationType = AF_MOVETYPE.Scale;
                    ikp[2].vrmBone = ParseIKBoneType.IKParent;
                    ikp[2].scale = olt.GetTextAreaSize();
                    movingData.Add(ikp[2]);
                    // (*) original scale is fixed (for 3D text)
                    // dynamically size is width x height and BoxCollider.size

                    //---rigid
                    ikp[3] = new AnimationTargetParts();
                    ikp[3].animationType = AF_MOVETYPE.Rigid;
                    ikp[3].vrmBone = ParseIKBoneType.IKParent;
                    ikp[3].rigidDrag = olt.GetDrag(1);
                    ikp[3].rigidAngularDrag = olt.GetDrag(2);
                    ikp[3].useCollision = olt.GetEasyCollision();
                    ikp[3].useRigidGravity = olt.GetUseGravity();

                    movingData.Add(ikp[3]);


                    if (!isEquip)
                    { //---Enable transforming without equipped status
                      //---common effect parts
                        AvatarPunchEffect punch = olt.GetPunch();
                        //if ((punch != null))
                        {
                            AnimationTargetParts pp = new AnimationTargetParts();
                            pp.animationType = AF_MOVETYPE.Punch;
                            pp.vrmBone = ParseIKBoneType.IKParent;
                            pp.effectPunch.Copy(punch);
                            movingData.Add(pp);
                        }
                        AvatarShakeEffect shake = olt.GetShake();
                        //if ((shake != null))
                        {
                            AnimationTargetParts pp = new AnimationTargetParts();
                            pp.animationType = AF_MOVETYPE.Shake;
                            pp.vrmBone = ParseIKBoneType.IKParent;
                            pp.effectShake.Copy(shake);
                            movingData.Add(pp);
                        }

                    }
                }
                else
                { //---2D
                    Vector2 v2 = olu.GetPosition();

                    ikp[0] = new AnimationTargetParts();
                    ikp[0].animationType = AF_MOVETYPE.Translate;
                    ikp[0].vrmBone = ParseIKBoneType.IKParent;
                    ikp[0].position = new Vector3(v2.x, v2.y, 0f); // rectt.anchoredPosition3D;
                    movingData.Add(ikp[0]);

                    ikp[1] = new AnimationTargetParts();
                    ikp[1].animationType = AF_MOVETYPE.Rotate;
                    ikp[1].vrmBone = ParseIKBoneType.IKParent;
                    Vector3 rot2d = Vector3.zero;
                    rot2d.z = rectt.rotation.eulerAngles.z;
                    ikp[1].rotation = rot2d;
                    movingData.Add(ikp[1]);

                    if ((nact.avatar.type == AF_TARGETTYPE.OtherObject) || (nact.avatar.type == AF_TARGETTYPE.Image))
                    {
                        ikp[2] = new AnimationTargetParts();
                        ikp[2].animationType = AF_MOVETYPE.Scale;
                        ikp[2].vrmBone = ParseIKBoneType.IKParent;
                        ikp[2].scale = rectt.sizeDelta;
                        movingData.Add(ikp[2]);
                    }
                }
                
            }
            else if (nact.targetType == AF_TARGETTYPE.Stage)
            {
                OperateStage os = nact.avatar.avatar.GetComponent<OperateStage>();

                //---Here is other of VRM, RectTransform, Audio, SystemEffect
                AnimationTargetParts[] ikp = new AnimationTargetParts[3];
                ikp[0] = new AnimationTargetParts();
                ikp[0].animationType = AF_MOVETYPE.Translate;
                ikp[0].vrmBone = ParseIKBoneType.IKParent;
                ikp[0].position = os.GetPositionFromOuter(0);
                movingData.Add(ikp[0]);

                ikp[1] = new AnimationTargetParts();
                ikp[1].animationType = AF_MOVETYPE.Rotate;
                ikp[1].vrmBone = ParseIKBoneType.IKParent;
                ikp[1].rotation = os.GetRotationFromOuter(0);
                movingData.Add(ikp[1]);

                ikp[2] = new AnimationTargetParts();
                ikp[2].animationType = AF_MOVETYPE.Scale;
                ikp[2].vrmBone = ParseIKBoneType.IKParent;
                ikp[2].scale = os.GetScale(0);
                movingData.Add(ikp[2]);

            }
            else
            {
                OperateLoadedBase olb = nact.avatar.avatar.GetComponent<OperateLoadedBase>();
                bool isEquip = false;
                if (olb != null)
                {
                    OtherObjectDummyIK ooik = olb.relatedHandleParent.GetComponent<OtherObjectDummyIK>();
                    isEquip = ooik.isEquipping;
                }

                //---Here is other of VRM, RectTransform, Audio, SystemEffect
                AnimationTargetParts[] ikp = new AnimationTargetParts[4];
                ikp[0] = new AnimationTargetParts();
                ikp[0].animationType = AF_MOVETYPE.Translate;
                ikp[0].vrmBone = ParseIKBoneType.IKParent;
                ikp[0].position = nact.avatar.ikparent.transform.position;
                //------position only: jump parts
                ikp[0].jumpNum = olb.GetJumpNum();
                ikp[0].jumpPower = olb.GetJumpPower();
                movingData.Add(ikp[0]);

                ikp[1] = new AnimationTargetParts();
                ikp[1].animationType = AF_MOVETYPE.Rotate;
                ikp[1].vrmBone = ParseIKBoneType.IKParent;
                ikp[1].rotation = nact.avatar.ikparent.transform.rotation.eulerAngles;
                movingData.Add(ikp[1]);

                if ((nact.avatar.type == AF_TARGETTYPE.OtherObject) || (nact.avatar.type == AF_TARGETTYPE.Image))
                {
                    ikp[2] = new AnimationTargetParts();
                    ikp[2].animationType = AF_MOVETYPE.Scale;
                    ikp[2].vrmBone = ParseIKBoneType.IKParent;
                    ikp[2].scale = nact.avatar.avatar.transform.localScale;
                    movingData.Add(ikp[2]);
                }

                //---rigid
                ikp[3] = new AnimationTargetParts();
                ikp[3].animationType = AF_MOVETYPE.Rigid;
                ikp[3].vrmBone = ParseIKBoneType.IKParent;
                ikp[3].rigidDrag = olb.GetDrag(1);
                ikp[3].rigidAngularDrag = olb.GetDrag(2);
                ikp[3].useCollision = olb.GetEasyCollision();
                ikp[3].useRigidGravity = olb.GetUseGravity();

                movingData.Add(ikp[3]);

                if (!isEquip)
                { //---Enable transforming without equipped status
                    //---common effect parts
                    AvatarPunchEffect punch = olb.GetPunch();
                    //if ((punch != null))
                    {
                        AnimationTargetParts pp = new AnimationTargetParts();
                        pp.animationType = AF_MOVETYPE.Punch;
                        pp.vrmBone = ParseIKBoneType.IKParent;
                        pp.effectPunch.Copy(punch);
                        movingData.Add(pp);
                    }
                    AvatarShakeEffect shake = olb.GetShake();
                    //if ((shake != null))
                    {
                        AnimationTargetParts pp = new AnimationTargetParts();
                        pp.animationType = AF_MOVETYPE.Shake;
                        pp.vrmBone = ParseIKBoneType.IKParent;
                        pp.effectShake.Copy(shake);
                        movingData.Add(pp);
                    }

                }



            }

            return movingData;
        }
        private List<AnimationTargetParts> PackForVRM(NativeAnimationFrame frame, NativeAnimationFrameActor naf, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            List<AnimationTargetParts> movingData = new List<AnimationTargetParts>();

            OperateLoadedVRM ovrm = naf.avatar.avatar.GetComponent<OperateLoadedVRM>();

            //---handpose
            if ((options.isBlendShapeOnly != 1) && (options.isCompileForLibrary != 1))
            {
                LeftHandPoseController lhand = ovrm.LeftHandCtrl;
                RightHandPoseController rhand = ovrm.RightHandCtrl;

                AnimationTargetParts[] athand = new AnimationTargetParts[2];
                athand[0] = new AnimationTargetParts();
                athand[0].animationType = AF_MOVETYPE.NormalTransform;
                athand[0].vrmBone = ParseIKBoneType.LeftHandPose;
                athand[0].isHandPose = ovrm.isHandPosing ? 1 : 2;
                athand[0].handpose.Add((float)lhand.currentPose);
                athand[0].handpose.Add(lhand.handPoseValue);
                athand[0].fingerpose = ovrm.LeftHandCtrl.BackupFinger();

                athand[1] = new AnimationTargetParts();
                athand[1].animationType = AF_MOVETYPE.NormalTransform;
                athand[1].vrmBone = ParseIKBoneType.RightHandPose;
                athand[1].isHandPose = ovrm.isHandPosing ? 1 : 2;
                athand[1].handpose.Add((float)rhand.currentPose);
                athand[1].handpose.Add(rhand.handPoseValue);
                athand[1].fingerpose = ovrm.RightHandCtrl.BackupFinger();

                movingData.Add(athand[0]);
                movingData.Add(athand[1]);

            }


            //---blendshape
            if (options.isHandOnly != 1)
            {
                GameObject mainface = naf.avatar.avatar.GetComponent<ManageAvatarTransform>().GetFaceMesh();
                SkinnedMeshRenderer face = mainface.GetComponent<SkinnedMeshRenderer>();
                List<BasicStringFloatList> blst = new List<BasicStringFloatList>();


                AnimationTargetParts atblendshape = new AnimationTargetParts();
                atblendshape.animationType = AF_MOVETYPE.BlendShape;
                atblendshape.vrmBone = ParseIKBoneType.BlendShape;
                atblendshape.isBlendShape = 1;

                //---From SkinnedMeshRenderer: newly: [mesh object name]:[blend shape name]
                /*int bscnt = face.sharedMesh.blendShapeCount;
                for (int i = 0; i < bscnt; i++)
                {
                    atblendshape.blendshapes.Add(new BasicStringFloatList(face.sharedMesh.GetBlendShapeName(i), face.GetBlendShapeWeight(i)));

                }*/
                /*
                List<BasicStringFloatList> skinnedlist = ovrm.ListAvatarBlendShapeList();
                foreach (BasicStringFloatList bsf in skinnedlist)
                {
                    atblendshape.blendshapes.Add(bsf);
                }
                */

                //---This timing, overwrite Avatar's blendshape of current AnimationFrameActor.
                naf.blendShapeList.Clear();

                //---From BlendShape Proxy (Key has always "PROX:" - prefix.)

                AnimationTargetParts oldBlendShape = new AnimationTargetParts();
                if (oldframe != null)
                {
                    oldBlendShape = oldframe.movingData.Find(bm =>
                    {
                        if (bm.animationType == AF_MOVETYPE.BlendShape) return true;
                        return false;
                    });
                    oldBlendShape.blendshapes.ForEach(action =>
                    {
                        atblendshape.blendshapes.Add(action);
                    });
                }
                

                List<BasicBlendShapeKey> proxlist =  ovrm.ListProxyBlendShape();
                foreach (BasicBlendShapeKey bsf in proxlist)
                { //---float value is already 0.xxf 
                    int alreadyIsHit = -1;
                    if (oldBlendShape != null)
                    {
                        alreadyIsHit = oldBlendShape.blendshapes.FindIndex(bm =>
                        {
                            if (bm.text == bsf.text) return true;
                            return false;
                        });
                    }
                    
                    //---The key of the blend shape you are about to register
                    int ishit = options.registerExpressions.FindIndex(match =>
                    {
                        if ((match.text == bsf.text) && (match.value == 1)) return true;
                        return false;
                    });
                    if (ishit > -1)
                    {
                        atblendshape.blendshapes.Add(bsf);
                        naf.blendShapeList.Add(bsf.text);
                    }
                    
                }
                //---auto blendshape
                AnimationTargetParts atautoblendshape = new AnimationTargetParts();
                atautoblendshape.animationType = AF_MOVETYPE.VRMAutoBlendShape;
                atautoblendshape.vrmBone = ParseIKBoneType.BlendShape;
                atautoblendshape.isblink = ovrm.GetPlayFlagAutoBlendShape();
                atautoblendshape.interval = ovrm.GetAutoBlendShapeInterval();
                atautoblendshape.openingSeconds = ovrm.GetAutoBlendShapeOpeningSeconds();
                atautoblendshape.closeSeconds = ovrm.GetAutoBlendShapeCloseSeconds();

                //---blink
                AnimationTargetParts atblink = new AnimationTargetParts();
                atblink.animationType = AF_MOVETYPE.VRMBlink;
                atblink.vrmBone = ParseIKBoneType.BlendShape;
                atblink.isblink = ovrm.GetBlinkFlag();
                atblink.interval = ovrm.GetBlinkInterval();
                atblink.openingSeconds = ovrm.GetBlinkOpeningSeconds();
                atblink.closeSeconds = ovrm.GetBlinkCloseSeconds();
                atblink.closingTime = ovrm.GetBlinkClosingTime();

                movingData.Add(atblendshape);
                movingData.Add(atautoblendshape);
                movingData.Add(atblink);

            }
            
            
            if ((options.isBlendShapeOnly == 0) && (options.isHandOnly == 0))
            {
                //---equipment
                AnimationTargetParts atequip = new AnimationTargetParts();
                atequip.vrmBone = ParseIKBoneType.Unknown;
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
                    aes.equipflag = match.equipflag;

                    atequip.equipDestinations.Add(aes);
                });
                movingData.Add(atequip);


                //---gravity info
                AnimationTargetParts atgravity = new AnimationTargetParts();
                atgravity.vrmBone = ParseIKBoneType.Unknown;
                atgravity.animationType = AF_MOVETYPE.GravityProperty;
                //---renewal the list, and apply it to key-frame.
                ovrm.ListGravityInfo();
                ovrm.gravityList.list.ForEach(item =>
                {
                    atgravity.gravity.list.Add(new VRMGravityInfo(item.comment, item.rootBoneName, item.power, item.dir.x, item.dir.y, item.dir.z));
                });
                movingData.Add(atgravity);

                //---special IK handles
                if (ovrm.ikMappingList.Count > 0)
                {
                    AnimationTargetParts atikhandle = new AnimationTargetParts();
                    atikhandle.vrmBone = ParseIKBoneType.Unknown;
                    atikhandle.animationType = AF_MOVETYPE.VRMIKProperty;
                    atikhandle.handleList = new List<AvatarIKMappingClass>(ovrm.ikMappingList);
                    movingData.Add(atikhandle);
                }

                //---Materials

                AnimationTargetParts atmat = new AnimationTargetParts();
                atmat.vrmBone = ParseIKBoneType.Unknown;
                atmat.animationType = AF_MOVETYPE.ObjectTexture;
                List<MaterialProperties> mats = ovrm.ListUserMaterialObject();
                foreach (MaterialProperties mat in mats)
                {
                    int ishit = options.registerMaterials.FindIndex(match =>
                    {
                        if ((match.text == mat.name) && (match.value == 1)) return true;
                        return false;
                    });
                    if (ishit > -1)
                    {
                        mat.includeMotion = options.registerMaterials[ishit].value;
                        atmat.matProp.Add(mat);
                    }
                    
                }
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
                if (atmat.matProp.Count > 0) movingData.Add(atmat);

                //---VRMAnimation
                if (ovrm.IsEnableVRMA())
                {

                    if (options.isPropertyOnly != 1)
                    {

                        AnimationTargetParts atobj = new AnimationTargetParts();

                        atobj.vrmBone = ParseIKBoneType.Unknown;
                        atobj.text = ovrm.GetVRMAFileName();
                        atobj.animSpeed = ovrm.GetSpeedAnimation();
                        atobj.animName = ovrm.GetTargetClip();
                        atobj.animLoop = ovrm.GetWrapMode();


                        atobj.animSeek = ovrm.GetSeekPosAnimation();
                        atobj.animPlaying = ovrm.GetPlayFlagAnimation();
                        if (atobj.animPlaying == UserAnimationState.Play)
                        {
                            atobj.animationType = AF_MOVETYPE.AnimStart;
                        }
                        else if (atobj.animPlaying == UserAnimationState.Stop)
                        {
                            atobj.animationType = AF_MOVETYPE.AnimStart;// AnimStop;
                        }
                        else if (atobj.animPlaying == UserAnimationState.Seeking)
                        {
                            atobj.animationType = AF_MOVETYPE.AnimStart;// AnimSeek;
                        }
                        else if (atobj.animPlaying == UserAnimationState.Pause)
                        {
                            atobj.animationType = AF_MOVETYPE.AnimStart;// AnimPause;
                        }
                        else if (atobj.animPlaying == UserAnimationState.Playing)
                        {
                            atobj.animationType = AF_MOVETYPE.AnimStart;// Rest;
                        }
                        movingData.Add(atobj);
                    }
                }else
                {
                    AnimationTargetParts atobj = new AnimationTargetParts();
                    atobj.animationType = AF_MOVETYPE.AnimStop;
                    atobj.animPlaying = UserAnimationState.Stop;
                    movingData.Add(atobj);
                }
            }


            return movingData;
        }
        private List<AnimationTargetParts> PackForOtherObject(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {

            List<AnimationTargetParts> movingData = new List<AnimationTargetParts>();

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
                        
                    //movingData.Add(atobj2);
                }

                if (options.isPropertyOnly != 1)
                {

                    AnimationTargetParts atobj = new AnimationTargetParts();

                    atobj.animSpeed = olo.GetSpeedAnimation();
                    atobj.animName = olo.GetTargetClip();
                    atobj.animLoop = olo.GetWrapMode();

                    
                    atobj.animSeek = olo.GetSeekPosAnimation();
                    atobj.animPlaying = olo.GetPlayFlagAnimation();
                    if (atobj.animPlaying == UserAnimationState.Play)
                    {
                        atobj.animationType = AF_MOVETYPE.AnimStart;
                        
                    }
                    else if (atobj.animPlaying == UserAnimationState.Stop)
                    {
                        atobj.animationType = AF_MOVETYPE.AnimStart;// AnimStop;
                        //atobj.animPlaying = olo.GetPlayFlagAnimation();
                    }
                    else if (atobj.animPlaying == UserAnimationState.Seeking)
                    {
                        atobj.animationType = AF_MOVETYPE.AnimStart;// AnimSeek;
                        //atobj.animPlaying = olo.GetPlayFlagAnimation();

                    }
                    else if (atobj.animPlaying == UserAnimationState.Pause)
                    {
                        atobj.animationType = AF_MOVETYPE.AnimStart;// AnimPause;
                        //atobj.animPlaying = olo.GetPlayFlagAnimation();
                    }
                    else if (atobj.animPlaying == UserAnimationState.Playing)
                    {
                        atobj.animationType = AF_MOVETYPE.AnimStart;// Rest;
                        //atobj.animPlaying = olo.GetPlayFlagAnimation();
                    }
                    movingData.Add(atobj);
                }


            }

            //---Materials
            
            AnimationTargetParts atobj3 = new AnimationTargetParts();
            atobj3.animationType = AF_MOVETYPE.ObjectTexture;
            List<MaterialProperties> mats = olo.ListUserMaterialObject();
            foreach (MaterialProperties mat in mats)
            {
                int ishit = options.registerMaterials.FindIndex(match =>
                {
                    if ((match.text == mat.name) && (match.value == 1)) return true;
                    return false;
                });
                if (ishit > -1)
                {
                    mat.includeMotion = options.registerMaterials[ishit].value;
                    atobj3.matProp.Add(mat);
                }
                
                
            }

            if (atobj3.matProp.Count > 0) movingData.Add(atobj3);
            


            return movingData;
        }
        private List<AnimationTargetParts> PackForLight(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            List<AnimationTargetParts> movingData = new List<AnimationTargetParts>();

            AnimationTargetParts atlight = new AnimationTargetParts();
            OperateLoadedLight oll = nav.avatar.avatar.GetComponent<OperateLoadedLight>();

            Light lt = nav.avatar.avatar.GetComponent<Light>();
            atlight.animationType = AF_MOVETYPE.LightProperty;

            atlight.lightType = lt.type;
            atlight.range = lt.range;
            atlight.power = lt.intensity;
            atlight.spotAngle = lt.spotAngle;
            atlight.color = lt.color;
            atlight.lightRenderMode = lt.renderMode;

            //atlight.halo = oll.GetHalo();

            atlight.flareType = oll.GetFlare();
            atlight.flareColor = oll.GetFlareColor();
            atlight.flareBrightness = oll.GetFlareBrightness();
            atlight.flareFade = oll.GetFlareFade();

            movingData.Add(atlight);

            return movingData;
        }
        private List<AnimationTargetParts> PackForCamera(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            List<AnimationTargetParts> movingData = new List<AnimationTargetParts>();

            OperateLoadedCamera olc = nav.avatar.avatar.GetComponent<OperateLoadedCamera>();
            Camera cam = nav.avatar.avatar.GetComponent<Camera>();
            if (options.isPropertyOnly != 1)
            {
                AnimationTargetParts atcam1 = new AnimationTargetParts();
                atcam1.animPlaying = olc.GetCameraPlaying();

                if (olc.GetCameraPlaying() == UserAnimationState.Play)
                {
                    atcam1.animationType = AF_MOVETYPE.Camera;// CameraOn;
                }
                else if (olc.GetCameraPlaying() == UserAnimationState.Playing)
                {
                    atcam1.animationType = AF_MOVETYPE.Camera;
                }
                else if (olc.GetCameraPlaying() == UserAnimationState.Stop)
                {
                    atcam1.animationType = AF_MOVETYPE.Camera;// CameraOff;
                }
                atcam1.cameraPlaying = (int)olc.GetCameraPlaying();
                movingData.Add(atcam1);
            }
            if (options.isDefineOnly != 1)
            {
                AnimationTargetParts atcam2 = new AnimationTargetParts();
                atcam2.animationType = AF_MOVETYPE.CameraProperty;
                atcam2.clearFlag = (int)cam.clearFlags;
                atcam2.color = cam.backgroundColor;
                atcam2.fov = cam.fieldOfView;
                atcam2.depth = cam.depth;
                atcam2.viewport.x = cam.rect.x;
                atcam2.viewport.y = cam.rect.y;
                atcam2.viewport.width = cam.rect.width;
                atcam2.viewport.height = cam.rect.height;

                //---render texture : Camera SIDE
                atcam2.renderFlag = olc.GetCameraRenderFlag();
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
                
                movingData.Add(atcam2);
            }



            return movingData;
        }
        private List<AnimationTargetParts> PackForText(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            List<AnimationTargetParts> movingData = new List<AnimationTargetParts>();

            OperateLoadedText olt = nav.avatar.avatar.GetComponent<OperateLoadedText>();

            Text text = nav.avatar.avatar.GetComponent<Text>();

            if (options.isPropertyOnly != 1)
            {
                AnimationTargetParts attext = new AnimationTargetParts();
                attext.text = olt.GetVVMText(); // text.text;
                attext.animationType = AF_MOVETYPE.Text;

                movingData.Add(attext);
            }

            if (options.isDefineOnly != 1)
            {
                AnimationTargetParts atprop = new AnimationTargetParts();
                atprop.animationType = AF_MOVETYPE.TextProperty;
                atprop.fontSize = olt.GetFontSize(); // text.fontSize;
                //atprop.fontStyle = text.fontStyle;
                atprop.fontStyles = olt.GetFontStyles();
                atprop.color = olt.GetFontColor(); // text.color;
                atprop.textAlignmentOptions = olt.GetTextAlignment();
                atprop.textOverflow = olt.GetTextOverflow();
                atprop.dimension = olt.GetDimension();

                atprop.IsGradient = olt.GetIsColorGradient();
                atprop.gradients = olt.GetColorGradient();
                atprop.fontOutlineWidth = olt.GetFontOutlineWidth();
                atprop.fontOutlineColor = olt.GetFontOutlineColor();
                
                movingData.Add(atprop);

            }


            return movingData;
        }
        private List<AnimationTargetParts> PackForImage(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            List<AnimationTargetParts> movingData = new List<AnimationTargetParts>();

            AnimationTargetParts atp = new AnimationTargetParts();
            OperateLoadedOther olo = nav.avatar.avatar.GetComponent<OperateLoadedOther>();
            //GameObject imgobj = olo.GetEffectiveObject();

            atp.animationType = AF_MOVETYPE.ImageProperty;
            atp.color = olo.GetBaseColor(0);

            movingData.Add(atp);

            return movingData;
        }
        private List<AnimationTargetParts> PackForUImage(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            List<AnimationTargetParts> movingData = new List<AnimationTargetParts>();

            AnimationTargetParts atp = new AnimationTargetParts();
            OperateLoadedUImage olo = nav.avatar.avatar.GetComponent<OperateLoadedUImage>();


            atp.animationType = AF_MOVETYPE.ImageProperty;
            atp.color = olo.GetImageBaseColor();

            movingData.Add(atp);

            return movingData;
        }
        private List<AnimationTargetParts> PackForAudio(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            List<AnimationTargetParts> movingData = new List<AnimationTargetParts>();

            OperateLoadedAudio ola = nav.avatar.avatar.GetComponent<OperateLoadedAudio>();

            if (options.isPropertyOnly != 1)
            {
                AnimationTargetParts atp = new AnimationTargetParts();
                atp.audioName = ola.targetAudioName;

                atp.isSE = ola.GetIsSE() ? 1 : 0;


                atp.seekTime = ola.GetSeekSeconds();
                atp.animPlaying = ola.GetPlayFlag();

                if (atp.animPlaying == UserAnimationState.Stop)  //---stop
                {
                    atp.animationType = AF_MOVETYPE.AnimStart;// AnimStop;
                }
                else if (atp.animPlaying == UserAnimationState.Play) //---play
                {
                    atp.animationType = AF_MOVETYPE.AnimStart;
                }
                else if (atp.animPlaying == UserAnimationState.Pause) //---pause
                {
                    atp.animationType = AF_MOVETYPE.AnimStart;// AnimPause;
                }
                else if (atp.animPlaying == UserAnimationState.Playing) //---playing
                {
                    atp.animationType = AF_MOVETYPE.AnimStart;// Rest;
                }
                else if (atp.animPlaying == UserAnimationState.Seeking)//---seek
                {
                    atp.animationType = AF_MOVETYPE.AnimStart;// AnimSeek;
                }
                movingData.Add(atp);
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

                movingData.Add(atprop);

            }




            return movingData;
        }
        private List<AnimationTargetParts> PackForEffect(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            List<AnimationTargetParts> movingData = new List<AnimationTargetParts>();

            //AnimationTargetParts atobj = new AnimationTargetParts();

            OperateLoadedEffect ole = nav.avatar.avatar.GetComponent<OperateLoadedEffect>();

            if (ole.IsVRMCollider)
            { //---Effect is for VRM Collider
                AnimationTargetParts atobj_col = new AnimationTargetParts();
                atobj_col.animationType = AF_MOVETYPE.Collider;
                atobj_col.isVRMCollider = ole.IsVRMCollider ? 1 : 0;
                atobj_col.VRMColliderSize = ole.VRMColliderSize;
                atobj_col.VRMColliderTarget = ole.EnumColliderTarget();

                movingData.Add(atobj_col);
            }
            else
            { //---Effect is normal animation effect
                //---save the animation
                if (ole.targetEffect != null)
                {
                    AnimationTargetParts atobj = new AnimationTargetParts();
                    atobj.animPlaying = ole.GetPlayFlagEffectFromOuter(0);
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
                        atobj.animationType = AF_MOVETYPE.AnimStart;// AnimPause;

                    }
                    else if (atobj.animPlaying == UserAnimationState.Stop)
                    {
                        atobj.animationType = AF_MOVETYPE.AnimStart;// AnimStop;
                        atobj.animLoop = -1;
                    }
                    else if (atobj.animPlaying == UserAnimationState.Playing)
                    {
                        atobj.animationType = AF_MOVETYPE.AnimStart;// Rest;

                    }
                    movingData.Add(atobj);
                }
            }

            
            return movingData;
        }
        private List<AnimationTargetParts> PackForSystemEffect(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            List<AnimationTargetParts> movingData = new List<AnimationTargetParts>();

            ManageSystemEffect mse = nav.avatar.avatar.GetComponent<ManageSystemEffect>();

            for (int i = 0; i < mse.ProcessNames.Length; i++)
            {
                AnimationTargetParts ateff = new AnimationTargetParts();

                string processname = mse.ProcessNames[i];
                int ena = mse.GetEnablePostProcessing(processname + ",0");

                ateff.effectName = processname;
                ateff.effectValues = mse.PackEffectValue(processname);
                ateff.animationType = AF_MOVETYPE.SystemEffect;
                if (ena == 1)
                {
                    ateff.animPlaying = UserAnimationState.Play;
                }
                else
                {
                    ateff.animPlaying = UserAnimationState.Stop;
                    //ateff.animationType = AF_MOVETYPE.SystemEffectOff;
                }


                movingData.Add(ateff);
            }



            return movingData;
        }
        private List<AnimationTargetParts> PackForStage(NativeAnimationFrame frame, NativeAnimationFrameActor nav, AnimationRegisterOptions options, NativeAnimationFrame oldframe)
        {
            List<AnimationTargetParts> movingData = new List<AnimationTargetParts>();

            OperateStage os = nav.avatar.avatar.GetComponent<OperateStage>();

            StageKind tmpstg = os.GetActiveStageType(0);

            //---Stage itself (loop of frame.movingData is descendant...)
            AnimationTargetParts atp1 = new AnimationTargetParts();
            atp1.animationType = AF_MOVETYPE.Stage;
            atp1.stageType = (int)tmpstg;
            movingData.Add(atp1);

            //---property
            AnimationTargetParts atp = new AnimationTargetParts();
            atp.animationType = AF_MOVETYPE.StageProperty;

            atp.stageType = (int)tmpstg;

            atp.color = os.GetDefaultStageColor(0);

            if (
                (tmpstg == StageKind.BasicSeaLevel) ||
                (tmpstg == StageKind.SeaDaytime) ||
                (tmpstg == StageKind.SeaNight)
            )
            {
                //Debug.Log("stage type=" + tmpstg.ToString());
                List<MaterialProperties> lstmp = os.ListUserMaterialObject();
                if (lstmp.Count > 0) atp.vmatProp = lstmp[0];
            }
            //atp.wavescale = os.GetWaterWaveScale(0);
            //atp.wavespeed = os.GetWaterWaveSpeedFromOuter(0);

            //---user stage properties
            if (tmpstg == StageKind.User)
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

            movingData.Add(atp);


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
            movingData.Add(atsky);

            //---Directional Light on stage
            OperateLoadedLight oll = os.GetSystemDirectionalLight();
            AnimationTargetParts atlight = new AnimationTargetParts();
            atlight.animationType = AF_MOVETYPE.LightProperty;
            atlight.rotation = oll.GetRotation();
            atlight.color = oll.GetColor();
            atlight.power = oll.GetPower();
            atlight.shadowStrength = oll.GetShadowPower();

            atlight.halo = oll.GetHalo();

            atlight.flareType = oll.GetFlare();
            atlight.flareColor = oll.GetFlareColor();
            atlight.flareBrightness = oll.GetFlareBrightness();
            atlight.flareFade = oll.GetFlareFade();
            movingData.Add(atlight);         


            return movingData;
        }
    }
}