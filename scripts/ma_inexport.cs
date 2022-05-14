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
using UserVRMSpace;

namespace UserHandleSpace
{
    public partial class ManageAnimation
    {
        private float[] TryParseFloatArray(string[] lst, int start, int size)
        {
            float[] vec3 = new float[size];
            int vi = 0;
            int fullsize = start + size;

            for (int i = start; i < lst.Length; i++)
            {
                if (i >= fullsize) break;

                if (float.TryParse(lst[i], out vec3[vi]))
                {
                    vi++;
                }
                else
                {
                    vec3[vi] = 0f;
                }
            }
            return vec3;
        }
        private string SerializeMaterial(MaterialProperties mat)
        {
            const string CST_SEPSTR_PROP = "=";

            string ln = "";

            //---standard
            if (mat.shaderName.ToLower() == "standard")
            {
                ln = mat.name + CST_SEPSTR_PROP +
                    mat.shaderName + CST_SEPSTR_PROP +
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.color) + CST_SEPSTR_PROP +
                    mat.blendmode.ToString() + CST_SEPSTR_PROP +
                    mat.texturePath + CST_SEPSTR_PROP +
                    mat.metallic.ToString() + CST_SEPSTR_PROP +
                    mat.glossiness + CST_SEPSTR_PROP +
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.emissioncolor)
                ;
            }
            //---VRM/MToon
            else if (mat.shaderName.ToLower() == "vrm/mtoon")
            {
                ln = mat.name + CST_SEPSTR_PROP +
                    mat.shaderName + CST_SEPSTR_PROP +
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.color) + CST_SEPSTR_PROP +
                    mat.cullmode.ToString() + CST_SEPSTR_PROP +
                    mat.blendmode.ToString() + CST_SEPSTR_PROP +
                    mat.texturePath + CST_SEPSTR_PROP +
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.emissioncolor) + CST_SEPSTR_PROP +
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.shadetexcolor) + CST_SEPSTR_PROP +
                    mat.shadingtoony.ToString() + CST_SEPSTR_PROP +
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.rimcolor) + CST_SEPSTR_PROP +
                    mat.rimfresnel.ToString() + CST_SEPSTR_PROP +
                    mat.srcblend.ToString() + CST_SEPSTR_PROP +
                    mat.dstblend.ToString()
                ;
            }
            //---FX/Water4
            else if (mat.shaderName.ToLower() == "fx/water4")
            {
                ln = mat.name + CST_SEPSTR_PROP +
                    mat.shaderName + CST_SEPSTR_PROP +
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.color) + CST_SEPSTR_PROP +
                    mat.fresnelScale.ToString() + CST_SEPSTR_PROP +
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.reflectionColor) + CST_SEPSTR_PROP +
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.specularColor) + CST_SEPSTR_PROP +
                    //---6
                    mat.waveAmplitude.x + CST_SEPSTR_PROP + mat.waveAmplitude.y + CST_SEPSTR_PROP + mat.waveAmplitude.z + CST_SEPSTR_PROP + mat.waveAmplitude.w + CST_SEPSTR_PROP +
                    mat.waveFrequency.x + CST_SEPSTR_PROP + mat.waveFrequency.y + CST_SEPSTR_PROP + mat.waveFrequency.z + CST_SEPSTR_PROP + mat.waveFrequency.w + CST_SEPSTR_PROP +
                    mat.waveSteepness.x + CST_SEPSTR_PROP + mat.waveSteepness.y + CST_SEPSTR_PROP + mat.waveSteepness.z + CST_SEPSTR_PROP + mat.waveSteepness.w + CST_SEPSTR_PROP +
                    mat.waveSpeed.x     + CST_SEPSTR_PROP + mat.waveSpeed.y     + CST_SEPSTR_PROP + mat.waveSpeed.z     + CST_SEPSTR_PROP + mat.waveSpeed.w     + CST_SEPSTR_PROP +
                    mat.waveDirectionAB.x + CST_SEPSTR_PROP + mat.waveDirectionAB.y + CST_SEPSTR_PROP + mat.waveDirectionAB.z + CST_SEPSTR_PROP + mat.waveDirectionAB.w + CST_SEPSTR_PROP +
                    mat.waveDirectionCD.x + CST_SEPSTR_PROP + mat.waveDirectionCD.y + CST_SEPSTR_PROP + mat.waveDirectionCD.z + CST_SEPSTR_PROP + mat.waveDirectionCD.w
                ;
            }

            return ln;
        }
        private bool SetObjectMaterial(string rawstr, List<MaterialProperties> matProp, string sepprop, string sepitem)
        {
            bool ret = false;
            string[] arr = rawstr.Split(sepitem);
            foreach (string matstr in arr)
            {
                MaterialProperties mat = new MaterialProperties();
                string[] matc = matstr.Split(sepprop);

                mat.name = matc[0];
                mat.shaderName = matc[1];

                if (mat.shaderName.ToLower() == "standard")
                {
                    mat.color = ColorUtility.TryParseHtmlString(matc[2], out mat.color) ? mat.color : Color.white;
                    mat.blendmode = float.TryParse(matc[3], out mat.blendmode) ? mat.blendmode : 0;
                    mat.texturePath = matc[4];

                    mat.metallic = float.TryParse(matc[5], out mat.metallic) ? mat.metallic : 0;
                    mat.glossiness = float.TryParse(matc[6], out mat.glossiness) ? mat.glossiness : 0;
                    mat.emissioncolor = ColorUtility.TryParseHtmlString(matc[7], out mat.emissioncolor) ? mat.emissioncolor : Color.white;

                }
                else if (mat.shaderName.ToLower() == "vrm/mtoon")
                {
                    mat.color = ColorUtility.TryParseHtmlString(matc[2], out mat.color) ? mat.color : Color.white;
                    mat.cullmode = float.TryParse(matc[3], out mat.cullmode) ? mat.cullmode : 0;
                    mat.blendmode = float.TryParse(matc[4], out mat.blendmode) ? mat.blendmode : 0;
                    mat.texturePath = matc[5];

                    mat.emissioncolor = ColorUtility.TryParseHtmlString(matc[6], out mat.emissioncolor) ? mat.emissioncolor : Color.white;
                    mat.shadetexcolor = ColorUtility.TryParseHtmlString(matc[7], out mat.shadetexcolor) ? mat.shadetexcolor : Color.white;
                    mat.shadingtoony = float.TryParse(matc[8], out mat.shadingtoony) ? mat.shadingtoony : 0;
                    mat.rimcolor = ColorUtility.TryParseHtmlString(matc[9], out mat.rimcolor) ? mat.rimcolor : Color.white;
                    mat.rimfresnel = float.TryParse(matc[10], out mat.rimfresnel) ? mat.rimfresnel : 0;
                    mat.srcblend = float.TryParse(matc[11], out mat.srcblend) ? mat.srcblend : 0;
                    mat.dstblend = float.TryParse(matc[12], out mat.dstblend) ? mat.dstblend : 0;

                }
                else if (mat.shaderName.ToLower() == "fx/water4")
                {
                    mat.color = ColorUtility.TryParseHtmlString(matc[2], out mat.color) ? mat.color : Color.white;
                    mat.fresnelScale = float.TryParse(matc[3], out mat.fresnelScale) ? mat.fresnelScale : 0;
                    mat.reflectionColor = ColorUtility.TryParseHtmlString(matc[4], out mat.reflectionColor) ? mat.reflectionColor : Color.white;
                    mat.specularColor = ColorUtility.TryParseHtmlString(matc[5], out mat.specularColor) ? mat.specularColor : Color.white;
                    {
                        float x = float.TryParse(matc[6], out x) ? x : 0;
                        float y = float.TryParse(matc[7], out y) ? y : 0;
                        float z = float.TryParse(matc[8], out z) ? z : 0;
                        float w = float.TryParse(matc[9], out w) ? w : 0;
                        mat.waveAmplitude = new Vector4(x, y, z, w);
                    }
                    {
                        float x = float.TryParse(matc[10], out x) ? x : 0;
                        float y = float.TryParse(matc[11], out y) ? y : 0;
                        float z = float.TryParse(matc[12], out z) ? z : 0;
                        float w = float.TryParse(matc[13], out w) ? w : 0;
                        mat.waveFrequency = new Vector4(x, y, z, w);
                    }
                    {
                        float x = float.TryParse(matc[14], out x) ? x : 0;
                        float y = float.TryParse(matc[15], out y) ? y : 0;
                        float z = float.TryParse(matc[16], out z) ? z : 0;
                        float w = float.TryParse(matc[17], out w) ? w : 0;
                        mat.waveSteepness = new Vector4(x, y, z, w);
                    }
                    {
                        float x = float.TryParse(matc[18], out x) ? x : 0;
                        float y = float.TryParse(matc[19], out y) ? y : 0;
                        float z = float.TryParse(matc[20], out z) ? z : 0;
                        float w = float.TryParse(matc[21], out w) ? w : 0;
                        mat.waveSpeed = new Vector4(x, y, z, w);
                    }
                    {
                        float x = float.TryParse(matc[22], out x) ? x : 0;
                        float y = float.TryParse(matc[23], out y) ? y : 0;
                        float z = float.TryParse(matc[24], out z) ? z : 0;
                        float w = float.TryParse(matc[25], out w) ? w : 0;
                        mat.waveDirectionAB = new Vector4(x, y, z, w);
                    }
                    {
                        float x = float.TryParse(matc[26], out x) ? x : 0;
                        float y = float.TryParse(matc[27], out y) ? y : 0;
                        float z = float.TryParse(matc[28], out z) ? z : 0;
                        float w = float.TryParse(matc[29], out w) ? w : 0;
                        mat.waveDirectionCD = new Vector4(x, y, z, w);
                    }

                }
                /*
                mat.color = ColorUtility.TryParseHtmlString(matc[2], out mat.color) ? mat.color : Color.white;
                mat.cullmode = float.TryParse(matc[3], out mat.cullmode) ? mat.cullmode : 0;
                mat.blendmode = float.TryParse(matc[4], out mat.blendmode) ? mat.blendmode : 0;
                mat.texturePath = matc[5];
                mat.metallic = float.TryParse(matc[6], out mat.metallic) ? mat.metallic : 0;
                mat.glossiness = float.TryParse(matc[7], out mat.glossiness) ? mat.glossiness : 0;
                mat.emissioncolor = ColorUtility.TryParseHtmlString(matc[8], out mat.emissioncolor) ? mat.emissioncolor : Color.white;
                mat.shadetexcolor = ColorUtility.TryParseHtmlString(matc[9], out mat.shadetexcolor) ? mat.shadetexcolor : Color.white;
                mat.shadingtoony = float.TryParse(matc[10], out mat.shadingtoony) ? mat.shadingtoony : 0;
                mat.rimcolor = ColorUtility.TryParseHtmlString(matc[11], out mat.rimcolor) ? mat.rimcolor : Color.white;
                mat.rimfresnel = float.TryParse(matc[12], out mat.rimfresnel) ? mat.rimfresnel : 0;
                mat.srcblend = float.TryParse(matc[13], out mat.srcblend) ? mat.srcblend : 0;
                mat.dstblend = float.TryParse(matc[14], out mat.dstblend) ? mat.dstblend : 0;
                */
                matProp.Add(mat);
            }
            if (matProp.Count > 0) ret = true;
            return ret;
        }
        /// <summary>
        /// Convert "movingData" CSV-format to AnimationTargetParts
        /// </summary>
        /// <param name="actor">Target object to load</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="param">csv data to load</param>
        /// <param name="atp">destination frame data</param>
        /// <returns></returns>
        private AnimationTargetParts CsvToFrameData(NativeAnimationFrameActor actor, AF_TARGETTYPE targetType, string param, AnimationTargetParts atp)
        {
            string[] lst = param.Split(',');
            const string CST_SEPSTR_PROP = "=";
            const string CST_SEPSTR_ITEM = "%";

            /* 
             * Column and meaning 
             * Index:
             * 0 = 部位(0 - All object's base parts.  1~n - ParseIKBoneType {enum}
             * 1 = オプション部位(string: each optional parts) {string}
             * 2 = アニメーションタイプ  {string}
             * 3 = 値数  {int,float}
             * 4 = 実際の値  {int,float}
             * 
             */
            //Debug.Log(param);
            if (lst.Length < 3)
            {
                //---error check
                return atp;
            }
            int boneParts;
            
            int.TryParse(lst[CSV_PARTS], out boneParts);
            string optParts = lst[CSV_OPTPARTS]; //.ToLower();

            //int movetype;
            //movetype = int.TryParse(lst[CSV_ANIMTYPE], out movetype) ? movetype : 0;
            //atp.animationType = (AF_MOVETYPE)movetype;
            string movetype = lst[CSV_ANIMTYPE];

            int valueCount = int.TryParse(lst[CSV_VALCNT], out valueCount) ? valueCount : 1;

            //---mostly common 
            if (movetype == "punch")
            {
                float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                atp.animationType = AF_MOVETYPE.Punch;
                atp.effectPunch.punch = new Vector3(vec3[0], vec3[1], vec3[2]);
                atp.effectPunch.isEnable = (int)vec3[3];
                atp.effectPunch.vibrato = (int)vec3[4];
                atp.effectPunch.elasiticity = vec3[5];
            }
            else if (movetype == "shake")
            {
                float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                atp.animationType = AF_MOVETYPE.Shake;
                atp.effectShake.isEnable = (int)vec3[0];
                atp.effectShake.strength = vec3[1];
                atp.effectShake.vibrato = (int)vec3[2];
                atp.effectShake.randomness = (int)vec3[3];
                atp.effectShake.fadeOut = (int)vec3[4];

            }

            //---start each type 
            if (targetType == AF_TARGETTYPE.VRM)
            {
                atp.vrmBone = (ParseIKBoneType)boneParts;
                //---check numbers of received value.


                if ((atp.vrmBone >= ParseIKBoneType.IKParent) && (atp.vrmBone <= ParseIKBoneType.RightLeg))
                {

                    if (valueCount > 2)
                    {
                        //---Here, Value must be 3.
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                        if (movetype == "position")
                        {
                            atp.animationType = AF_MOVETYPE.Translate;
                            atp.position = new Vector3(vec3[0], vec3[1], vec3[2]);
                            atp.jumpNum = (int)vec3[3];
                            atp.jumpPower = vec3[4];
                        }
                        else if (movetype == "rotation")
                        {
                            atp.animationType = AF_MOVETYPE.Rotate;
                            atp.rotation = new Vector3(vec3[0], vec3[1], vec3[2]);
                        }
                        else if (movetype == "scale")
                        {
                            atp.animationType = AF_MOVETYPE.Scale;
                            atp.scale = new Vector3(vec3[0], vec3[1], vec3[2]);
                        }
                    }

                }
                else if (atp.vrmBone == ParseIKBoneType.UseHumanBodyBones)
                {
                    int hbb = int.TryParse(optParts, out hbb) ? hbb : (int)HumanBodyBones.LastBone;
                    atp.vrmHumanBodyBone = (HumanBodyBones)hbb;
                    if (valueCount > 2)
                    {
                        //---Here, Value must be 3.
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                        if (movetype == "position")
                        {
                            atp.animationType = AF_MOVETYPE.Translate;
                            atp.position = new Vector3(vec3[0], vec3[1], vec3[2]);
                        }
                        else if (movetype == "rotation")
                        {
                            atp.animationType = AF_MOVETYPE.Rotate;
                            atp.rotation = new Vector3(vec3[0], vec3[1], vec3[2]);
                        }
                        else if (movetype == "scale")
                        {
                            atp.animationType = AF_MOVETYPE.Scale;
                            atp.scale = new Vector3(vec3[0], vec3[1], vec3[2]);
                        }
                    }
                }
                else if ((atp.vrmBone == ParseIKBoneType.LeftHandPose) || (atp.vrmBone == ParseIKBoneType.RightHandPose))
                {
                    //---Here, Value must be 2.
                    if (valueCount > 1)
                    {
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.animationType = AF_MOVETYPE.NormalTransform;
                        atp.handpose = new List<float>();
                        atp.isHandPose = 1;
                        atp.handpose.Add(vec3[0]);
                        atp.handpose.Add(vec3[1]);

                    }

                }
                else if (movetype == "blendshape")
                {
                    if (valueCount > 0)
                    {
                        //float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.animationType = AF_MOVETYPE.BlendShape;
                        for (int lst_i = CSV_BEGINVAL; lst_i < lst.Length; lst_i++)
                        {
                            string lste = lst[lst_i];
                            string[] blendItem = lste.Split('=');
                            int blendIndex = int.TryParse(blendItem[0], out blendIndex) ? blendIndex : -1;
                            float blendVal = float.TryParse(blendItem[1], out blendVal) ? blendVal : 0f;
                            if (
                                (blendIndex > -1) &&
                                (blendIndex < actor.blendShapeList.Count)
                            )
                            {
                                string bname = actor.blendShapeList[blendIndex];
                                atp.isBlendShape = 1;
                                atp.blendshapes.Add(new BasicStringFloatList(bname, blendVal));
                            }

                        }

                        //atp.blendshapes.Add(new BasicStringFloatList(optParts, vec3[0]));
                    }
                }
                else if (movetype == "head_options")
                {
                    if (valueCount > 0)
                    {
                        atp.animationType = AF_MOVETYPE.VRMBlink;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.isblink = (int)vec3[0];
                        atp.interval = vec3[1];
                        atp.openingSeconds = vec3[2];
                        atp.closeSeconds = vec3[3];
                        atp.closingTime = vec3[4];
                        atp.headLock = (int)vec3[5];

                    }
                }
                else if (movetype == "equipment")
                {
                    atp.animationType = AF_MOVETYPE.Equipment;
                    for (int lst_i = CSV_BEGINVAL; lst_i < lst.Length; lst_i++)
                    {
                        string lste = lst[lst_i];
                        string[] equipItem = lste.Split(CST_SEPSTR_PROP);
                        int ival = int.TryParse(equipItem[0], out ival) ? ival : 0;
                        if (ival != 0)
                        {
                            AvatarEquipSaveClass aes = new AvatarEquipSaveClass();

                            aes.bodybonename = (HumanBodyBones)ival;
                            aes.equipitem = equipItem[1];
                            atp.equipDestinations.Add(aes);
                        }

                    }
                }
                else if (movetype == "gravity")
                {
                    atp.animationType = AF_MOVETYPE.GravityProperty;
                    for (int lst_i = CSV_BEGINVAL; lst_i < lst.Length; lst_i++)
                    {
                        string lste = lst[lst_i];
                        string[] graitem = lste.Split(CST_SEPSTR_PROP);
                        int gravIndex = int.TryParse(graitem[0], out gravIndex) ? gravIndex : -1;
                        if ((gravIndex > -1) && (gravIndex < actor.gravityBoneList.Count))
                        {
                            VRMGravityInfo vgi = new VRMGravityInfo();
                            string[] g_name = actor.gravityBoneList[gravIndex].Split('/');
                            vgi.comment = g_name[0];
                            vgi.rootBoneName = g_name[1];
                            float power = float.TryParse(graitem[1], out power) ? power : 0f;
                            vgi.power = power;
                            float x = float.TryParse(graitem[2], out x) ? x : 0f;
                            float y = float.TryParse(graitem[3], out y) ? y : -1f;
                            float z = float.TryParse(graitem[4], out z) ? z : 0f;
                            vgi.dir.x = x;
                            vgi.dir.y = y;
                            vgi.dir.z = z;
                            atp.gravity.list.Add(vgi);
                        }
                        
                    }
                }
                else if (movetype == "vrmik")
                {
                    atp.animationType = AF_MOVETYPE.VRMIKProperty;
                    for (int i = CSV_BEGINVAL; i < lst.Length; i++)
                    {
                        string[] arr = lst[i].Split(CST_SEPSTR_PROP);
                        AvatarIKMappingClass aikmc = new AvatarIKMappingClass();
                        int iparts = int.TryParse(arr[0], out iparts) ? iparts : 99;
                        if (iparts != 99)
                        {
                            aikmc.parts = (IKBoneType)iparts;
                            aikmc.name = arr[1];
                            atp.handleList.Add(aikmc);
                        }
                    }
                }
                else if (movetype == "objtex")
                {
                    atp.animationType = AF_MOVETYPE.ObjectTexture;

                    //---Open and set material properties from raw-string
                    SetObjectMaterial(lst[4], atp.matProp, CST_SEPSTR_PROP, CST_SEPSTR_ITEM);


                }
            }
            else
            { //---For  Other object, Light, Camera, Image, Effect, Text, UImage, Audio, SystemEffect------------------

                //---Common parse
                if ((movetype == "position") || (movetype == "rotation") || (movetype == "scale"))
                {
                    //---Here, Value must be 3.
                    float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                    if (movetype == "position")
                    {
                        atp.animationType = AF_MOVETYPE.Translate;
                        atp.position = new Vector3(vec3[0], vec3[1], vec3[2]);
                        atp.jumpNum = (int)vec3[3];
                        atp.jumpPower = vec3[4];
                    }
                    else if (movetype == "rotation")
                    {
                        atp.animationType = AF_MOVETYPE.Rotate;
                        atp.rotation = new Vector3(vec3[0], vec3[1], vec3[2]);
                    }
                    else if (movetype == "scale")
                    {
                        atp.animationType = AF_MOVETYPE.Scale;
                        atp.scale = new Vector3(vec3[0], vec3[1], vec3[2]);
                    }
                }

                //---Each parse
                if (targetType == AF_TARGETTYPE.OtherObject)
                {
                    atp.animName = optParts;
                    if (movetype == "animstart")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStart;
                        atp.animPlaying = UserAnimationState.Play;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.animSeek = vec3[0];
                    }
                    else if (movetype == "animstop")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStop;
                        atp.animPlaying = UserAnimationState.Stop;
                    }
                    else if (movetype == "animseek")
                    {
                        atp.animationType = AF_MOVETYPE.AnimSeek;
                        atp.animPlaying = UserAnimationState.Seeking;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.animSeek = vec3[0];
                    }
                    else if (movetype == "animpause")
                    {
                        atp.animationType = AF_MOVETYPE.AnimPause;
                        atp.animPlaying = UserAnimationState.Pause;
                    }
                    else if (movetype == "rest")
                    {
                        atp.animationType = AF_MOVETYPE.Rest;
                        atp.animPlaying = UserAnimationState.Playing;
                    }
                    else if (movetype == "animprop")
                    {
                        atp.animationType = AF_MOVETYPE.AnimProperty;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        //atp.animPlaying = (UserAnimationState)vec3[0];
                        atp.animSpeed = vec3[0];
                        atp.color = new Color(vec3[1], vec3[2], vec3[3], vec3[4]);
                        atp.animName = optParts;
                        atp.animLoop = (int)vec3[5];
                    }
                    else if (movetype == "objtex")
                    {
                        atp.animationType = AF_MOVETYPE.ObjectTexture;

                        //---Open and set material properties from raw-string
                        SetObjectMaterial(lst[4], atp.matProp, CST_SEPSTR_PROP, CST_SEPSTR_ITEM);
                        
                    }
                }
                else if (targetType == AF_TARGETTYPE.Light)
                {
                    float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                    if (movetype == "lightprop")
                    {
                        atp.animationType = AF_MOVETYPE.LightProperty;
                        atp.range = vec3[0];
                        atp.color = new Color(vec3[1], vec3[2], vec3[3], vec3[4]);
                        atp.power = vec3[5];
                        atp.spotAngle = vec3[6];
                        atp.lightRenderMode = (LightRenderMode)vec3[7];
                    }
                }
                else if (targetType == AF_TARGETTYPE.Camera)
                {
                    float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                    if (movetype == "cameraon")
                    {
                        atp.animationType = AF_MOVETYPE.CameraOn;
                        atp.cameraPlaying = (int)UserAnimationState.Play;
                    }
                    else if (movetype == "camera")
                    {
                        atp.animationType = AF_MOVETYPE.Camera;
                        atp.cameraPlaying = (int)UserAnimationState.Playing;
                    }
                    else if (movetype == "cameraoff")
                    {
                        atp.animationType = AF_MOVETYPE.CameraOff;
                        atp.cameraPlaying = (int)UserAnimationState.Stop;
                    }
                    else if (movetype == "cameraprop")
                    {
                        atp.animationType = AF_MOVETYPE.CameraProperty;
                        atp.color = new Color(vec3[0], vec3[1], vec3[2], vec3[3]);
                        atp.fov = vec3[4];
                        atp.depth = vec3[5];
                        atp.viewport = new Rect(vec3[6], vec3[7], vec3[8], vec3[9]);
                        atp.renderTex = new Vector3(vec3[10], vec3[11], 0f);
                        atp.renderFlag = (int)vec3[12];
                        atp.clearFlag = (int)vec3[13];
                    }
                }
                else if (targetType == AF_TARGETTYPE.Text)
                {
                    if (movetype == "text")
                    {
                        atp.animationType = AF_MOVETYPE.Text;
                        for (int i = CSV_BEGINVAL; i < lst.Length; i++)
                        {
                            atp.text = lst[i];
                        }
                    }
                    else if (movetype == "textprop")
                    {
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                        atp.animationType = AF_MOVETYPE.TextProperty;
                        atp.color = new Color(vec3[0], vec3[1], vec3[2], vec3[3]);
                        atp.fontSize = (int)vec3[4];
                        atp.fontStyle = (FontStyle)vec3[5];
                    }

                }
                else if (targetType == AF_TARGETTYPE.Image)
                {
                    if (movetype == "imageprop")
                    {
                        atp.animationType = AF_MOVETYPE.ImageProperty;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.color = new Color(vec3[0], vec3[1], vec3[2], vec3[3]);
                    }
                }
                else if (targetType == AF_TARGETTYPE.UImage)
                {
                    if (movetype == "imageprop")
                    {
                        atp.animationType = AF_MOVETYPE.ImageProperty;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.color = new Color(vec3[0], vec3[1], vec3[2], vec3[3]);
                    }
                }
                else if (targetType == AF_TARGETTYPE.Audio)
                {
                    if (movetype == "animstart")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStart;
                        atp.audioName = optParts;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        //atp.isSE = (int)vec3[0];
                        atp.seekTime = vec3[1];
                    }
                    else if (movetype == "animstop")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStop;
                        atp.audioName = optParts;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        //atp.isSE = (int)vec3[0];
                        atp.seekTime = vec3[1];
                    }
                    else if (movetype == "animpause")
                    {
                        atp.animationType = AF_MOVETYPE.AnimPause;
                        atp.audioName = optParts;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        //atp.isSE = (int)vec3[0];
                        atp.seekTime = vec3[1];
                    }
                    else if (movetype == "animseek")
                    {
                        atp.animationType = AF_MOVETYPE.AnimSeek;
                        atp.audioName = optParts;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        //atp.isSE = (int)vec3[0];
                        atp.seekTime = vec3[1];

                    }
                    else if (movetype == "rest")
                    {
                        atp.animationType = AF_MOVETYPE.Rest;
                        atp.audioName = optParts;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        //atp.isSE = (int)vec3[0];
                        atp.seekTime = vec3[1];
                    }
                    else if (movetype == "audioprop")
                    {
                        atp.animationType = AF_MOVETYPE.AudioProperty;
                        atp.audioName = optParts;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        //atp.isSE = (int)vec3[0];
                        atp.isLoop = (int)vec3[1];
                        atp.isMute = (int)vec3[2];
                        atp.pitch = vec3[3];
                        atp.volume = vec3[4];
                    }

                }
                else if (targetType == AF_TARGETTYPE.Effect)
                {

                    if (movetype == "animstart")
                    {
                        string[] eff = optParts.Split('%');
                        atp.effectGenre = eff[0];
                        atp.effectName = eff[1];
                        atp.animationType = AF_MOVETYPE.AnimStart;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.animLoop = (int)vec3[0];
                        atp.animPlaying = UserAnimationState.Play;
                    }
                    else if (movetype == "animpause")
                    {
                        atp.animationType = AF_MOVETYPE.AnimPause;
                        atp.animPlaying = UserAnimationState.Pause;
                    }
                    else if (movetype == "animstop")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStop;
                        atp.animPlaying = UserAnimationState.Stop;
                    }
                    else if (movetype == "rest")
                    {
                        atp.animationType = AF_MOVETYPE.Rest;
                        atp.animPlaying = UserAnimationState.Playing;
                    }
                    else if (movetype == "collider")
                    {
                        string[] target = optParts.Split('%');
                        atp.VRMColliderTarget = new List<string>(target);
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.isVRMCollider = (int)vec3[0];
                        atp.VRMColliderSize = vec3[1];
                    }
                }
                else if (targetType == AF_TARGETTYPE.SystemEffect)
                {
                    atp.effectName = optParts;
                    float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                    if (movetype == "systemeffect")
                    {
                        atp.animationType = AF_MOVETYPE.SystemEffect;
                        List<float> flst = new List<float>(vec3);
                        atp.effectValues = flst;
                    }
                    else if (movetype == "systemeffectoff")
                    {
                        atp.animationType = AF_MOVETYPE.SystemEffectOff;
                    }
                }
                else if (targetType == AF_TARGETTYPE.Stage)
                {
                    if (movetype == "stageprop")
                    {
                        atp.animationType = AF_MOVETYPE.StageProperty;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.stageType = (int)vec3[0];
                        Vector4 vec4 = new Vector4(vec3[1], vec3[2], vec3[3], vec3[4]);
                        atp.wavespeed = vec4;
                        atp.wavescale = vec3[5];

                        //---windzone
                        atp.windPower = vec3[6];
                        atp.windFrequency = vec3[7];
                        atp.windDurationMin = vec3[8];
                        atp.windDurationMax = vec3[9];

                        if (atp.stageType == (int)StageKind.User)
                        {
                            MaterialProperties matmain = new MaterialProperties();

                            //---user stage
                            matmain.metallic = vec3[10];
                            matmain.glossiness = vec3[11];
                            matmain.emissioncolor = new Color(vec3[12], vec3[13], vec3[14], vec3[15]);
                            matmain.color = new Color(vec3[16], vec3[17], vec3[18], vec3[19]);
                            matmain.blendmode = vec3[20];

                            //---user stage texture
                            string[] texparts = optParts.Split("\t");
                            //int utex_i = CSV_BEGINVAL + valueCount;

                            matmain.texturePath = texparts[0]; // lst[utex_i];
                            atp.matProp.Add(matmain);
                            //utex_i++;

                            MaterialProperties matnormal = new MaterialProperties();
                            matnormal.texturePath = texparts[1]; // lst[utex_i];
                            atp.matProp.Add(matnormal);
                        }
                        


                    }
                    else if (movetype == "skyprop")
                    {
                        atp.animationType = AF_MOVETYPE.SkyProperty;
                        int intskytype = int.TryParse(lst[4], out intskytype) ? intskytype : 0;
                        atp.skyType = (CameraClearFlags)intskytype;
                        atp.skyColor = ColorUtility.TryParseHtmlString(lst[5], out atp.skyColor) ? atp.skyColor : Color.white;
                        string[] ssf = lst[6].Split('%');
                        foreach (string item in ssf)
                        {
                            string[] arr = item.Split('=');
                            float arrfloat = float.TryParse(arr[1], out arrfloat) ? arrfloat : 0;
                            atp.skyShaderFloat.Add(new BasicStringFloatList(arr[0], arrfloat));
                        }
                        string[] ssc = lst[7].Split('%');
                        foreach (string item in ssc)
                        {
                            string[] arr = item.Split('=');
                            Color col = ColorUtility.TryParseHtmlString(arr[1], out col) ? col : Color.white;
                            atp.skyShaderColor.Add(new BasicStringColorList(arr[0], col));
                        }
                        atp.skyShaderName = lst[8];
                    }
                    else if (movetype == "systemlightprop")
                    {
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                        atp.rotation = new Vector3(vec3[0], vec3[1], vec3[2]);
                        atp.range = vec3[3];
                        atp.color = new Color(vec3[4], vec3[5], vec3[6], vec3[7]);
                        atp.power = vec3[8];
                        atp.shadowStrength = vec3[9];
                    }
                }
            }


            return atp;
        }


        /// <summary>
        /// Convert "movingData" AnimationTargetParts to CSV-format
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="atp"></param>
        /// <returns></returns>
        public string DataToCSV(AF_TARGETTYPE targetType, AnimationTargetParts atp)
        {
            const string CST_SEPSTR_PROP = "=";
            const string CST_SEPSTR_ITEM = "%";
            List<string> ret = new List<string>();
            /*
                CSV format:
                |       0      |      1       |     2   |         3           |         4~N      |
                 object IK type (VRM)bone type move type float paramater count float paramaters
                 char           string          string      integer               float
                                optional 
                                 other than
                                   VRM
            */

            //---Common converting
            if (atp.animationType == AF_MOVETYPE.Translate)
            {
                ret.Add(((int)atp.vrmBone).ToString());
                if ((targetType == AF_TARGETTYPE.VRM) && (atp.vrmBone == ParseIKBoneType.UseHumanBodyBones))
                {
                    //---real HumanBodyBones information
                    ret.Add(((int)atp.vrmHumanBodyBone).ToString());
                }
                else
                {
                    //---If VRM, IK information
                    ret.Add("");
                }
                
                ret.Add("position");
                ret.Add("5");
                ret.Add(atp.position.x.ToString());
                ret.Add(atp.position.y.ToString());
                ret.Add(atp.position.z.ToString());
                ret.Add(atp.jumpNum.ToString());
                ret.Add(atp.jumpPower.ToString());

            }
            else if (atp.animationType == AF_MOVETYPE.Rotate)
            {
                ret.Add(((int)atp.vrmBone).ToString());
                if ((targetType == AF_TARGETTYPE.VRM) && (atp.vrmBone == ParseIKBoneType.UseHumanBodyBones))
                {
                    ret.Add(((int)atp.vrmHumanBodyBone).ToString());
                }
                else
                {
                    ret.Add("");
                }
                ret.Add("rotation");
                ret.Add("3");
                ret.Add(atp.rotation.x.ToString());
                ret.Add(atp.rotation.y.ToString());
                ret.Add(atp.rotation.z.ToString());
            }
            else if (atp.animationType == AF_MOVETYPE.Scale)
            {
                ret.Add(((int)atp.vrmBone).ToString());
                if ((targetType == AF_TARGETTYPE.VRM) && (atp.vrmBone == ParseIKBoneType.UseHumanBodyBones))
                {
                    ret.Add(((int)atp.vrmHumanBodyBone).ToString());
                }
                else
                {
                    ret.Add("");
                }
                ret.Add("scale");
                ret.Add("3");
                ret.Add(atp.scale.x.ToString());
                ret.Add(atp.scale.y.ToString());
                ret.Add(atp.scale.z.ToString());
            }
            else if (atp.animationType == AF_MOVETYPE.Punch)
            {
                ret.Add(((int)atp.vrmBone).ToString());
                ret.Add(((int)atp.effectPunch.translationType).ToString());
                ret.Add("punch");
                ret.Add("6");
                ret.Add(atp.effectPunch.punch.x.ToString());
                ret.Add(atp.effectPunch.punch.y.ToString());
                ret.Add(atp.effectPunch.punch.z.ToString());
                ret.Add(atp.effectPunch.isEnable.ToString());
                ret.Add(atp.effectPunch.vibrato.ToString());
                ret.Add(atp.effectPunch.elasiticity.ToString());
            }
            else if (atp.animationType == AF_MOVETYPE.Shake)
            {
                ret.Add(((int)atp.vrmBone).ToString());
                ret.Add(((int)atp.effectShake.translationType).ToString());
                ret.Add("shake");
                ret.Add("5");
                ret.Add(atp.effectShake.isEnable.ToString());
                ret.Add(atp.effectShake.strength.ToString());
                ret.Add(atp.effectShake.vibrato.ToString());
                ret.Add(atp.effectShake.randomness.ToString());
                ret.Add(atp.effectShake.fadeOut.ToString());
            }
            else
            {
                //---each object converting
                if (targetType == AF_TARGETTYPE.VRM)
                {
                    if ((atp.vrmBone == ParseIKBoneType.LeftHandPose) || (atp.vrmBone == ParseIKBoneType.RightHandPose))
                    {
                        ret.Add(((int)atp.vrmBone).ToString());
                        ret.Add("");
                        ret.Add("normaltransform");
                        ret.Add(atp.handpose.Count.ToString());
                        atp.handpose.ForEach(hand =>
                        {
                            ret.Add(hand.ToString());
                        });
                    }
                    else if (atp.animationType == AF_MOVETYPE.BlendShape)
                    {
                        ret.Add(((int)atp.vrmBone).ToString());
                        ret.Add("");
                        ret.Add("blendshape");
                        ret.Add(atp.blendshapes.Count.ToString());
                        for (int i = 0; i < atp.blendshapes.Count; i++)
                        {

                            ret.Add(i.ToString() + "=" + atp.blendshapes[i].value);
                        }
                    }
                    else if (atp.animationType == AF_MOVETYPE.VRMBlink)
                    {
                        ret.Add(((int)atp.vrmBone).ToString());
                        ret.Add("");
                        ret.Add("head_options");
                        ret.Add("6");
                        ret.Add(atp.isblink.ToString());
                        ret.Add(atp.interval.ToString());
                        ret.Add(atp.openingSeconds.ToString());
                        ret.Add(atp.closeSeconds.ToString());
                        ret.Add(atp.closingTime.ToString());
                        ret.Add(atp.headLock.ToString());
                    }
                    else if (atp.animationType == AF_MOVETYPE.Equipment)
                    {
                        ret.Add(((int)ParseIKBoneType.Unknown).ToString());
                        ret.Add("");
                        ret.Add("equipment");
                        ret.Add(atp.equipDestinations.Count.ToString());
                        atp.equipDestinations.ForEach(equip =>
                        {
                            ret.Add(((int)equip.bodybonename).ToString() + CST_SEPSTR_PROP + equip.equipitem);
                        });

                    }
                    else if (atp.animationType == AF_MOVETYPE.GravityProperty)
                    {
                        ret.Add(((int)ParseIKBoneType.Unknown).ToString());
                        ret.Add("");
                        ret.Add("gravity");
                        ret.Add(atp.gravity.list.Count.ToString());
                        for (int i = 0; i < atp.gravity.list.Count; i++)
                        {
                            VRMGravityInfo gra = atp.gravity.list[i];
                            ret.Add(i.ToString() + "=" + gra.power + CST_SEPSTR_PROP + gra.dir.x.ToString() + CST_SEPSTR_PROP + gra.dir.y.ToString() + CST_SEPSTR_PROP + gra.dir.z.ToString());
                        }
                    }
                    else if (atp.animationType == AF_MOVETYPE.VRMIKProperty)
                    {
                        ret.Add(((int)ParseIKBoneType.Unknown).ToString());
                        ret.Add("");
                        ret.Add("vrmik");
                        ret.Add(atp.handleList.Count.ToString());
                        foreach (var item in atp.handleList)
                        {
                            ret.Add(((int)item.parts).ToString() + CST_SEPSTR_PROP + item.name);
                        }
                        
                    }
                    else if (atp.animationType == AF_MOVETYPE.ObjectTexture)
                    {
                        ret.Add(((int)ParseIKBoneType.Unknown).ToString());
                        ret.Add("");
                        ret.Add("objtex");
                        ret.Add("0");
                        List<string> lst = new List<string>();
                        foreach (MaterialProperties mat in atp.matProp)
                        {
                            string ln = SerializeMaterial(mat);
                            /*
                            ln = mat.name + CST_SEPSTR_PROP + 
                                mat.shaderName + CST_SEPSTR_PROP +
                                "#" + ColorUtility.ToHtmlStringRGBA(mat.color) + CST_SEPSTR_PROP +
                                mat.cullmode.ToString() + CST_SEPSTR_PROP +
                                mat.blendmode.ToString() + CST_SEPSTR_PROP +
                                "" + CST_SEPSTR_PROP +
                                "0" + CST_SEPSTR_PROP +
                                "0" + CST_SEPSTR_PROP +
                                "#" + ColorUtility.ToHtmlStringRGBA(mat.emissioncolor) + CST_SEPSTR_PROP +
                                "#" + ColorUtility.ToHtmlStringRGBA(mat.shadetexcolor) + CST_SEPSTR_PROP +
                                mat.shadingtoony.ToString() + CST_SEPSTR_PROP +
                                "#" + ColorUtility.ToHtmlStringRGBA(mat.rimcolor) + CST_SEPSTR_PROP +
                                mat.rimfresnel.ToString() + CST_SEPSTR_PROP +
                                mat.srcblend.ToString() + CST_SEPSTR_PROP +
                                mat.dstblend.ToString() + CST_SEPSTR_PROP +
                                mat.srcblend.ToString() + CST_SEPSTR_PROP +
                                mat.dstblend.ToString()
                            ;*/
                            lst.Add(ln);
                        }
                        ret.Add(string.Join(CST_SEPSTR_ITEM, lst));
                    }
                }
                else if (targetType == AF_TARGETTYPE.OtherObject)
                {
                    ret.Add(((int)atp.vrmBone).ToString());
                    ret.Add(atp.animName);
                    if (atp.animationType == AF_MOVETYPE.AnimStart)
                    {
                        ret.Add("animstart");
                        ret.Add("1");
                        ret.Add(atp.animSeek.ToString());
                    }
                    else if (atp.animationType == AF_MOVETYPE.AnimStop)
                    {
                        ret.Add("animstop");
                        ret.Add("0");
                    }
                    else if (atp.animationType == AF_MOVETYPE.AnimSeek)
                    {
                        ret.Add("animseek");
                        ret.Add("1");
                        ret.Add(atp.animSeek.ToString());
                    }
                    else if (atp.animationType == AF_MOVETYPE.AnimPause)
                    {
                        ret.Add("animpause");
                        ret.Add("0");
                    }
                    else if (atp.animationType == AF_MOVETYPE.Rest)
                    {
                        ret.Add("rest");
                        ret.Add("0");
                    }
                    else if (atp.animationType == AF_MOVETYPE.AnimProperty)
                    {
                        ret.Add("animprop");
                        ret.Add("6");
                        //ret.Add(atp.animPlaying.ToString());
                        ret.Add(atp.animSpeed.ToString());
                        ret.Add(atp.color.r.ToString());
                        ret.Add(atp.color.g.ToString());
                        ret.Add(atp.color.b.ToString());
                        ret.Add(atp.color.a.ToString());
                        ret.Add(atp.animLoop.ToString());

                    }
                    else if (atp.animationType == AF_MOVETYPE.ObjectTexture)
                    {
                        ret.Add("objtex");
                        ret.Add("0");
                        List<string> lst = new List<string>();
                        foreach (MaterialProperties mat in atp.matProp)
                        {
                            string ln = SerializeMaterial(mat);
                            
                            /*
                            ln = mat.name + CST_SEPSTR_PROP +
                                mat.shaderName + CST_SEPSTR_PROP +
                                "#" + ColorUtility.ToHtmlStringRGBA(mat.color) + CST_SEPSTR_PROP +
                                mat.cullmode.ToString() + CST_SEPSTR_PROP +
                                mat.blendmode.ToString() + CST_SEPSTR_PROP +
                                mat.texturePath + CST_SEPSTR_PROP +
                                mat.metallic.ToString() + CST_SEPSTR_PROP +
                                mat.glossiness + CST_SEPSTR_PROP +
                                "#" + ColorUtility.ToHtmlStringRGBA(mat.emissioncolor) + CST_SEPSTR_PROP +
                                "#" + ColorUtility.ToHtmlStringRGBA(mat.shadetexcolor) + CST_SEPSTR_PROP +
                                mat.shadingtoony.ToString() + CST_SEPSTR_PROP +
                                "#" + ColorUtility.ToHtmlStringRGBA(mat.rimcolor) + CST_SEPSTR_PROP +
                                mat.rimfresnel.ToString() + CST_SEPSTR_PROP +
                                mat.srcblend.ToString() + CST_SEPSTR_PROP +
                                mat.dstblend.ToString() + CST_SEPSTR_PROP +
                                mat.fresnelScale.ToString() + CST_SEPSTR_PROP +
                                "#" + ColorUtility.ToHtmlStringRGBA(mat.reflectionColor)  + CST_SEPSTR_PROP +
                                "#" + ColorUtility.ToHtmlStringRGBA(mat.specularColor) + CST_SEPSTR_PROP +
                                mat.waveAmplitude.x + CST_SEPSTR_PROP + mat.waveAmplitude.y + CST_SEPSTR_PROP + mat.waveAmplitude.z + CST_SEPSTR_PROP + mat.waveAmplitude.w + CST_SEPSTR_PROP +
                                mat.waveFrequency.x + CST_SEPSTR_PROP + mat.waveFrequency.y + CST_SEPSTR_PROP + mat.waveFrequency.z + CST_SEPSTR_PROP + mat.waveFrequency.w + CST_SEPSTR_PROP +
                                mat.waveSteepness.x + CST_SEPSTR_PROP + mat.waveSteepness.y + CST_SEPSTR_PROP + mat.waveSteepness.z + CST_SEPSTR_PROP + mat.waveSteepness.w + CST_SEPSTR_PROP +
                                mat.waveSpeed.x + CST_SEPSTR_PROP + mat.waveSpeed.y + CST_SEPSTR_PROP + mat.waveSpeed.z + CST_SEPSTR_PROP + mat.waveSpeed.w + CST_SEPSTR_PROP +
                                mat.waveDirectionAB.x + CST_SEPSTR_PROP + mat.waveDirectionAB.y + CST_SEPSTR_PROP + mat.waveDirectionAB.z + CST_SEPSTR_PROP + mat.waveDirectionAB.w + CST_SEPSTR_PROP +
                                mat.waveDirectionCD.x + CST_SEPSTR_PROP + mat.waveDirectionCD.y + CST_SEPSTR_PROP + mat.waveDirectionCD.z + CST_SEPSTR_PROP + mat.waveDirectionCD.w
                            ;*/
                            lst.Add(ln);
                        }
                        ret.Add(string.Join(CST_SEPSTR_ITEM, lst));
                    }
                }
                else if (targetType == AF_TARGETTYPE.Light)
                {
                    ret.Add(((int)atp.vrmBone).ToString());
                    ret.Add("");
                    ret.Add("lightprop");
                    ret.Add("8");
                    ret.Add(atp.range.ToString());
                    ret.Add(atp.color.r.ToString());
                    ret.Add(atp.color.g.ToString());
                    ret.Add(atp.color.b.ToString());
                    ret.Add(atp.color.a.ToString());
                    ret.Add(atp.power.ToString());
                    ret.Add(atp.spotAngle.ToString());
                    ret.Add(((int)atp.lightRenderMode).ToString());

                }
                else if (targetType == AF_TARGETTYPE.Camera)
                {
                    ret.Add(((int)atp.vrmBone).ToString());
                    ret.Add("");
                    if (atp.animationType == AF_MOVETYPE.CameraOn)
                    {
                        ret.Add("cameraon");
                        ret.Add("1");
                        ret.Add(atp.cameraPlaying.ToString());
                    }
                    else if (atp.animationType == AF_MOVETYPE.Camera)
                    {
                        ret.Add("camera");
                        ret.Add("1");
                        ret.Add(atp.cameraPlaying.ToString());
                    }
                    else if (atp.animationType == AF_MOVETYPE.CameraOff)
                    {
                        ret.Add("cameraoff");
                        ret.Add("1");
                        ret.Add(atp.cameraPlaying.ToString());
                    }
                    else if (atp.animationType == AF_MOVETYPE.CameraProperty)
                    {
                        ret.Add("cameraprop");
                        ret.Add("14");
                        ret.Add(atp.color.r.ToString());
                        ret.Add(atp.color.g.ToString());
                        ret.Add(atp.color.b.ToString());
                        ret.Add(atp.color.a.ToString());
                        ret.Add(atp.fov.ToString());
                        ret.Add(atp.depth.ToString());
                        ret.Add(atp.viewport.x.ToString());
                        ret.Add(atp.viewport.y.ToString());
                        ret.Add(atp.viewport.width.ToString());
                        ret.Add(atp.viewport.height.ToString());
                        ret.Add(atp.renderTex.x.ToString());
                        ret.Add(atp.renderTex.y.ToString());
                        ret.Add(atp.renderFlag.ToString());
                        ret.Add(atp.clearFlag.ToString());

                    }

                }
                else if (targetType == AF_TARGETTYPE.Text)
                {
                    ret.Add(((int)atp.vrmBone).ToString());
                    ret.Add("");

                    if (atp.animationType == AF_MOVETYPE.Text)
                    {
                        ret.Add("text");
                        //ret.Add("");
                        ret.Add("1");
                        ret.Add(atp.text);
                    }
                    else if (atp.animationType == AF_MOVETYPE.TextProperty)
                    {
                        ret.Add("textprop");
                        ret.Add("6");
                        ret.Add(atp.color.r.ToString());
                        ret.Add(atp.color.g.ToString());
                        ret.Add(atp.color.b.ToString());
                        ret.Add(atp.color.a.ToString());
                        ret.Add(atp.fontSize.ToString());
                        ret.Add(atp.fontStyle.ToString());
                    }

                }
                else if (targetType == AF_TARGETTYPE.Image)
                {
                    ret.Add(((int)atp.vrmBone).ToString());
                    ret.Add("");
                    ret.Add("imageprop");
                    ret.Add("4");
                    ret.Add(atp.color.r.ToString());
                    ret.Add(atp.color.g.ToString());
                    ret.Add(atp.color.b.ToString());
                    ret.Add(atp.color.a.ToString());

                }
                else if (targetType == AF_TARGETTYPE.UImage)
                {
                    ret.Add(((int)atp.vrmBone).ToString());
                    ret.Add("");
                    ret.Add("imageprop");
                    ret.Add("4");
                    ret.Add(atp.color.r.ToString());
                    ret.Add(atp.color.g.ToString());
                    ret.Add(atp.color.b.ToString());
                    ret.Add(atp.color.a.ToString());
                }
                else if (targetType == AF_TARGETTYPE.Audio)
                {
                    ret.Add(((int)atp.vrmBone).ToString());
                    ret.Add(atp.audioName);
                    if (atp.animationType == AF_MOVETYPE.AnimStart)
                    {
                        ret.Add("animstart");
                        ret.Add("2");
                        ret.Add(atp.isSE.ToString());
                        ret.Add(atp.seekTime.ToString());
                    }
                    else if (atp.animationType == AF_MOVETYPE.AnimPause)
                    {
                        ret.Add("animpause");
                        ret.Add("2");
                        ret.Add(atp.isSE.ToString());
                        ret.Add(atp.seekTime.ToString());
                    }
                    else if (atp.animationType == AF_MOVETYPE.AnimStop)
                    {
                        ret.Add("animstop");
                        ret.Add("2");
                        ret.Add(atp.isSE.ToString());
                        ret.Add(atp.seekTime.ToString());
                    }
                    else if (atp.animationType == AF_MOVETYPE.AnimSeek)
                    {
                        ret.Add("animseek");
                        ret.Add("2");
                        ret.Add(atp.isSE.ToString());
                        ret.Add(atp.seekTime.ToString());
                    }
                    else if (atp.animationType == AF_MOVETYPE.Rest)
                    {
                        ret.Add("rest");
                        ret.Add("1");
                        ret.Add(atp.isSE.ToString());
                        ret.Add(atp.seekTime.ToString());
                    }
                    else if (atp.animationType == AF_MOVETYPE.AudioProperty)
                    {
                        ret.Add("audioprop");
                        ret.Add("5");
                        ret.Add(atp.isSE.ToString());
                        ret.Add(atp.isLoop.ToString());
                        ret.Add(atp.isMute.ToString());
                        ret.Add(atp.pitch.ToString());
                        ret.Add(atp.volume.ToString());
                    }

                }
                else if (targetType == AF_TARGETTYPE.Effect)
                {
                    ret.Add(((int)atp.vrmBone).ToString());


                    if (atp.animationType == AF_MOVETYPE.Collider)
                    {
                        ret.Add(string.Join("%", atp.VRMColliderTarget));
                        ret.Add("collider");
                        ret.Add("2");
                        ret.Add(atp.isVRMCollider.ToString());
                        ret.Add(atp.VRMColliderSize.ToString());
                    }
                    else
                    {
                        ret.Add(atp.effectGenre + "%" + atp.effectName);
                        if (atp.animationType == AF_MOVETYPE.AnimStart)
                        {
                            ret.Add("animstart");
                            ret.Add("1");
                            ret.Add(atp.animLoop.ToString());
                        }
                        else if (atp.animationType == AF_MOVETYPE.AnimPause)
                        {
                            ret.Add("animpause");
                            ret.Add("0");
                        }
                        else if (atp.animationType == AF_MOVETYPE.AnimStop)
                        {
                            ret.Add("animstop");
                            ret.Add("0");
                        }
                        else if (atp.animationType == AF_MOVETYPE.Rest)
                        {
                            ret.Add("rest");
                            ret.Add("0");
                        }
                    }

                    
                }
                else if (targetType == AF_TARGETTYPE.SystemEffect)
                {
                    ret.Add(((int)atp.vrmBone).ToString());
                    if (atp.animationType == AF_MOVETYPE.SystemEffect)
                    {
                        ret.Add(atp.effectName);
                        ret.Add("systemeffect");
                        ret.Add(atp.effectValues.Count.ToString());
                        atp.effectValues.ForEach(v =>
                        {
                            ret.Add(v.ToString());
                        });
                    }
                    else if (atp.animationType == AF_MOVETYPE.SystemEffectOff)
                    {
                        ret.Add(atp.effectName);
                        ret.Add("systemeffectoff");
                        ret.Add("0");
                    }
                }
                else if (targetType == AF_TARGETTYPE.Stage)
                {
                    ret.Add(((int)atp.vrmBone).ToString());
                    if (atp.animationType == AF_MOVETYPE.StageProperty)
                    {
                        string allCount = "";
                        string optParts = "";
                        if ((atp.matProp.Count > 0) && (atp.stageType == (int)StageKind.User))
                        {
                            allCount = "21";
                            optParts = atp.matProp[0].texturePath + "\t" + atp.matProp[1].texturePath;
                        }
                        else
                        {
                            allCount = "10";
                        }
                        ret.Add(optParts);
                        ret.Add("stageprop");
                        ret.Add(allCount);
                        //0 ~ 9
                        ret.Add(atp.stageType.ToString());
                        ret.Add(atp.wavespeed.x.ToString());
                        ret.Add(atp.wavespeed.y.ToString());
                        ret.Add(atp.wavespeed.z.ToString());
                        ret.Add(atp.wavespeed.w.ToString());
                        ret.Add(atp.wavescale.ToString());
                        ret.Add(atp.windPower.ToString());
                        ret.Add(atp.windFrequency.ToString());
                        ret.Add(atp.windDurationMin.ToString());
                        ret.Add(atp.windDurationMax.ToString());

                        if (atp.matProp.Count > 0)
                        {
                            MaterialProperties mat0 = atp.matProp[0];
                            MaterialProperties mat1 = atp.matProp[1];
                            //10 ~ 20
                            ret.Add(mat0.metallic.ToString());
                            ret.Add(mat0.glossiness.ToString());
                            ret.Add(mat0.emissioncolor.r.ToString());
                            ret.Add(mat0.emissioncolor.g.ToString());
                            ret.Add(mat0.emissioncolor.b.ToString());
                            ret.Add(mat0.emissioncolor.a.ToString());
                            ret.Add(mat0.color.r.ToString());
                            ret.Add(mat0.color.g.ToString());
                            ret.Add(mat0.color.b.ToString());
                            ret.Add(mat0.color.a.ToString());
                            ret.Add(mat0.blendmode.ToString());
                            //---below is string param
                            //ret.Add(atp.matProp[0].texturePath); //maintex
                            //ret.Add(atp.matProp[1].texturePath); //bump map
                        }
                        
                    }
                    else if (atp.animationType == AF_MOVETYPE.SkyProperty)
                    {
                        ret.Add("");
                        ret.Add("skyprop");
                        ret.Add("0");
                        ret.Add(((int)atp.skyType).ToString());
                        ret.Add("#"+ColorUtility.ToHtmlStringRGBA(atp.skyColor));
                        List<string> ssf = new List<string>();
                        atp.skyShaderFloat.ForEach(item =>
                        {
                            ssf.Add(item.text + "=" + item.value.ToString());
                        });
                        ret.Add(string.Join("%", ssf));
                        List<string> ssc = new List<string>();
                        atp.skyShaderColor.ForEach(item =>
                        {
                            ssc.Add(item.text + "=#" + ColorUtility.ToHtmlStringRGBA(item.value));
                        });
                        ret.Add(string.Join("%", ssc));
                        ret.Add(atp.skyShaderName);
                    }
                    else if (atp.animationType == AF_MOVETYPE.LightProperty)
                    {
                        ret.Add("");
                        ret.Add("systemlightprop");
                        ret.Add("10");
                        ret.Add(atp.rotation.x.ToString());
                        ret.Add(atp.rotation.y.ToString());
                        ret.Add(atp.rotation.z.ToString());
                        ret.Add(atp.range.ToString());
                        ret.Add(atp.color.r.ToString());
                        ret.Add(atp.color.g.ToString());
                        ret.Add(atp.color.b.ToString());
                        ret.Add(atp.color.a.ToString());
                        ret.Add(atp.power.ToString());
                        ret.Add(atp.shadowStrength.ToString());

                    }

                }

            }


            return string.Join(",", ret);
        }

//===========================================================================================================================
//  Load functions
//===========================================================================================================================
        public void NewProject()
        {
            //---destroy current project
            for (int i = currentProject.casts.Count-1; i >= 0; i--)
            {
                NativeAnimationAvatar cast = currentProject.casts[i];

                //---destroy real objects
                if (cast.type == AF_TARGETTYPE.Audio)
                {
                    OperateLoadedAudio ola = cast.avatar.GetComponent<OperateLoadedAudio>();
                    if (ola != null)
                    {
                        List<string> str = ola.ListAudio();
                        str.ForEach(item =>
                        {
                            ola.RemoveAudio(item);
                        });
                    }
                    
                }
                else
                {
                    if (cast.avatar != null)
                    {
                        DestroyEffectiveAvatar(cast.avatar, cast.ikparent, cast.type);
                    }
                }
                //---detach and remove cast and timeline actor
                DeleteAvatarFromCast(cast.roleName + "," + (int)cast.type);


            }
            currentProject.casts.Clear();
            /*foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
            {
                actor.frames.Clear();
            }
            currentProject.timeline.characters.Clear();
            */

            //---clear materials in the project
            currentProject.materialManager.Dispose();


            currentProject = null;

            //---new project
            currentProject = new NativeAnimationProject(initialFrameCount);

            //--fixed objects-------------------------------------------------------------------------------------------
            FirstAddFixedAvatar("SystemEffect", gameObject, gameObject, "SystemEffect", AF_TARGETTYPE.SystemEffect);
            GameObject[] audios = GameObject.FindGameObjectsWithTag("AudioPlayer");
            for (int i = 0; i < audios.Length; i++)
            {
                GameObject audio = audios[i];
                FirstAddFixedAvatar(audio.name, audio, audio, audio.name, AF_TARGETTYPE.Audio);
            }
            GameObject stage = GameObject.FindGameObjectWithTag("GroundWorld");
            FirstAddFixedAvatar("Stage", stage, stage, "Stage", AF_TARGETTYPE.Stage);

            /*
            //--non-fixed objects------------------------------------------------------------------------------------
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            for (int i = 0; i < players.Length; i++)
            {
                ManageAvatarTransform mat = players[i].GetComponent<ManageAvatarTransform>();
                OperateLoadedVRM olvrm = players[i].GetComponent<OperateLoadedVRM>();

                float[] calcBodyInfo = mat.ParseBodyInfo(olvrm.GetTPoseBodyInfo());
                List<Vector3> bodyinfoList = mat.ParseBodyInfoList(olvrm.GetTPoseBodyInfo(), olvrm.relatedHandleParent);

                bool isoverwrite = false;

                FirstAddAvatar2(out isoverwrite, players[i].name, players[i], olvrm.relatedHandleParent, "VRM", AF_TARGETTYPE.VRM, calcBodyInfo, bodyinfoList);
            }
            GameObject[] others = GameObject.FindGameObjectsWithTag("OtherPlayer");
            for (int i = 0; i < others.Length; i++)
            {
                OperateLoadedOther olo = others[i].GetComponent<OperateLoadedOther>();

                FirstAddAvatar(others[i].name, others[i], olo.relatedHandleParent, "OtherObject", AF_TARGETTYPE.OtherObject);
            }
            GameObject[] lights = GameObject.FindGameObjectsWithTag("LightPlayer");
            for (int i = 0; i < lights.Length; i++)
            {
                OperateLoadedLight oll = lights[i].GetComponent<OperateLoadedLight>();
                FirstAddAvatar(lights[i].name, lights[i], oll.relatedHandleParent, "Light", AF_TARGETTYPE.Light);
            }
            GameObject[] cameras = GameObject.FindGameObjectsWithTag("CameraPlayer");
            for (int i = 0; i < cameras.Length; i++)
            {
                OperateLoadedCamera olc = cameras[i].GetComponent<OperateLoadedCamera>();

                FirstAddAvatar(cameras[i].name, cameras[i], olc.relatedHandleParent, "Camera", AF_TARGETTYPE.Camera);
            }
            Transform txtarea = MsgArea.transform;
            for (int i = 0; i < txtarea.childCount; i++)
            {
                GameObject text = txtarea.GetChild(i).gameObject;
                //OperateLoadedText olt = text.GetComponent<OperateLoadedText>();
                FirstAddAvatar(text.name, text, null, "Text", AF_TARGETTYPE.Text);
            }
            Transform imgarea = ImgArea.transform;
            for (int i = 0; i < imgarea.childCount; i++)
            {
                GameObject img = imgarea.GetChild(i).gameObject;
                //OperateLoadedUImage olui = img.GetComponent<OperateLoadedUImage>();
                FirstAddAvatar(img.name, img, null, "UImage", AF_TARGETTYPE.UImage);
            }
            GameObject[] eff = GameObject.FindGameObjectsWithTag("EffectDestination");
            for (int i = 0; i < eff.Length; i++)
            {
                OperateLoadedEffect oll = eff[i].GetComponent<OperateLoadedEffect>();
                FirstAddAvatar(eff[i].name, eff[i], eff[i], "Effect", AF_TARGETTYPE.Effect);
            }
            */

        }
        public void OpenProject(string url)
        {
            StartCoroutine(LoadProjectURI(url));
        }
        public IEnumerator LoadProjectURI(string url)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();
                //if (www.isNetworkError || www.isHttpError)
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                    yield break;
                }
                else
                {
                    //StartCoroutine(LoadVRM_body(www.downloadHandler.data));
                    LoadProject(www.downloadHandler.text);
                }
            }
        }
        public void LoadProject(string param)
        {
            //---reset current project (do not merge)
            NewProject();

            AnimationProject proj = JsonUtility.FromJson<AnimationProject>(param);
            currentProject = ConvertProjectNative(proj);

            //---after settings
            SetFps(currentProject.fps);

#if !UNITY_EDITOR && UNITY_WEBGL
        ReceiveStringVal(param);
#endif

        }
        private NativeAnimationProject ConvertProjectNative(AnimationProject proj)
        {
            NativeAnimationProject nproj = new NativeAnimationProject(initialFrameCount);

            IsExternalProject = proj.isSharing;

            nproj.mkey = proj.mkey;
            nproj.meta = proj.meta;
            nproj.baseDuration = proj.baseDuration;
            nproj.isSharing = proj.isSharing;
            nproj.isReadOnly = proj.isReadOnly;
            nproj.isNew = false;
            nproj.isOpenAndEdit = true;
            nproj.timelineFrameLength = proj.timelineFrameLength;
            nproj.fps = proj.fps;
            nproj.materialManager.SetFromRaw(proj.materialManager);

            //---set up Avatar
            nproj.casts = new List<NativeAnimationAvatar>();
            //nproj.casts.Clear();
            //---re-save old casts in current project
            for (var i = 0; i < currentProject.casts.Count; i++)
            {
                NativeAnimationAvatar naa = currentProject.casts[i];
                if ( //---leaving type: VRM, OtherObject, Light, Camera, Image, UImage, Text, Effect
                    (naa.type == AF_TARGETTYPE.VRM) || (naa.type == AF_TARGETTYPE.OtherObject) ||
                    (naa.type == AF_TARGETTYPE.Light) || (naa.type == AF_TARGETTYPE.Camera) ||
                    (naa.type == AF_TARGETTYPE.Image) || (naa.type == AF_TARGETTYPE.UImage) ||
                    (naa.type == AF_TARGETTYPE.Text) || (naa.type == AF_TARGETTYPE.Effect)
                )
                {
                    nproj.casts.Add(naa);
                }
            }
            //---load from the file
            proj.casts.ForEach((avatar) =>
            {
                nproj.casts.Add(ParseEffectiveAvatar(avatar));
            });

            //---set up each frames of timelines.
            //---re-save frame actor and old timeline in current project
            currentProject.timeline.characters.ForEach(chara =>
            {
                if ( //---leaving type: VRM, OtherObject, Light, Camera, Image, UImage, Text, Effect
                    (chara.targetType == AF_TARGETTYPE.VRM) || (chara.targetType == AF_TARGETTYPE.OtherObject) ||
                    (chara.targetType == AF_TARGETTYPE.Light) || (chara.targetType == AF_TARGETTYPE.Camera) ||
                    (chara.targetType == AF_TARGETTYPE.Image) || (chara.targetType == AF_TARGETTYPE.UImage) ||
                    (chara.targetType == AF_TARGETTYPE.Text) || (chara.targetType == AF_TARGETTYPE.Effect)
                )
                {
                    nproj.timeline.characters.Add(chara);
                }
            });
            //---load from the file
            foreach (AnimationFrameActor actor in proj.timeline.characters)
            {
                NativeAnimationFrameActor nact = new NativeAnimationFrameActor();
                nact.SetFromRaw(actor);
                NativeAnimationAvatar nav = nproj.casts.Find(match =>
                {
                    if (match.roleName == actor.targetRole) return true;
                    return false;
                });
                nact.avatar = nav;

                //---trim "M_F00_000_00_" of the BlendShape WHEN LOADED project also.
                for (int bsi = 0; bsi < nact.blendShapeList.Count; bsi++)
                {
                    nact.blendShapeList[bsi] = TrimBlendShapeName(nact.blendShapeList[bsi]);
                }

                foreach (AnimationFrame fr in actor.frames)
                {
                    NativeAnimationFrame nframe = ParseEffectiveFrame(nact, fr);
                    nact.frames.Add(nframe);
                }
                nproj.timeline.characters.Add(nact);
            }


            return nproj;
        }



        /// <summary>
        /// Generate NativeAnimationAvatar(Cast) from AnimationAvatar(JSON-based)
        /// </summary>
        /// <param name="avatar"></param>
        /// <returns></returns>
        private NativeAnimationAvatar ParseEffectiveAvatar(AnimationAvatar avatar)
        {
            NativeAnimationAvatar nav = new NativeAnimationAvatar(); // GetTargetObjects(avatar.avatarId, avatar.type);

            nav.roleName = avatar.roleName;
            nav.roleTitle = avatar.roleTitle;
            nav.type = avatar.type;
            nav.avatarId = avatar.avatarId;
            nav.path = avatar.path;
            Array.Copy(avatar.bodyHeight, nav.bodyHeight, avatar.bodyHeight.Length);

            if (avatar.avatarId != "")
            { 
                NativeAnimationAvatar tmp = GetEffectiveAvatarObjects(avatar.avatarId, avatar.type);
                if (tmp != null)
                {
                    nav.avatar = tmp.avatar;
                    nav.ikparent = tmp.ikparent;
                }
            }
            NativeAnimationAvatar hitav = GetCastByNameInProject(avatar.roleTitle);
            if ((hitav != null) && 
                ((avatar.type != AF_TARGETTYPE.SystemEffect) && (avatar.type != AF_TARGETTYPE.Audio) && (avatar.type != AF_TARGETTYPE.Stage))
            )
            {
                NativeAnimationFrameActor nact = GetFrameActorFromRole(hitav.roleName, hitav.type);
                if (nact != null)
                {
                    if (nact.frames.Count == 0)
                    { //---if existed avatar don't has key-frame data, overwrite to JSON-based data.
                        DetachAvatarFromRole(avatar.roleName + "," + "role");
                        nav.avatar = hitav.avatar;
                        nav.ikparent = hitav.ikparent;
                        DeleteAvatarFromCast(avatar.roleName + "," + ((int)avatar.type).ToString());
                    }
                }
            }


            return nav;
        }
        private NativeAnimationAvatar ParseEffectiveAvatar(string role, string actorId, AF_TARGETTYPE type)
        {
            NativeAnimationAvatar nav = new NativeAnimationAvatar(); //GetTargetObjects(avatarId, type);
            nav.roleName = role;
            nav.type = type;
            //nav.bodyHeight = avatar.bodyHeight;

            return nav;
        }
        /// <summary>
        /// Convert AnimationFrame to NativeAnimationFrame
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public NativeAnimationFrame ParseEffectiveFrame(NativeAnimationFrameActor actor, AnimationFrame frame)
        {
            NativeAnimationFrame naframe = new NativeAnimationFrame();
            //naframe.targetId = frame.targetId;
            //naframe.targetType = frame.targetType;
            naframe.duration = frame.duration;
            //naframe.enabled = frame.enabled;
            //naframe.bodyHeight = frame.bodyHeight;
            naframe.index = frame.index;
            naframe.finalizeIndex = frame.finalizeIndex;
            naframe.key = frame.key;
            naframe.ease = frame.ease;

            //foreach (AnimationTargetParts line in frame.movingData)
            foreach (string line in frame.movingData)
            {
                AnimationTargetParts atp = new AnimationTargetParts();
                if (line != null)
                {
                    //naframe.movingData.Add(line);
                    naframe.movingData.Add(CsvToFrameData(actor, actor.targetType, line, atp));
                }
            }

            return naframe;
        }

        //=====================================================================================================================
        //
        //  Single Motion functions
        //
        //=====================================================================================================================
        /// <summary>
        /// To decide FrameActor of indicated role.
        /// </summary>
        /// <param name="roleName"></param>
        public void SetLoadTargetSingleMotion(string roleName)
        {
            SingleMotionTargetRole = GetCastInProject(roleName);
        }
        public void LoadSingleMotion(string uri)
        {
            StartCoroutine(LoadSingleMotionURI(uri));
        }
        public IEnumerator LoadSingleMotionURI(string uri)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(uri))
            {
                yield return www.SendWebRequest();
                //if (www.isNetworkError || www.isHttpError)
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log(www.error);
                    yield break;
                }
                else
                {
                    string ret  = LoadSingleMotion_body(www.downloadHandler.text);
#if !UNITY_EDITOR && UNITY_WEBGL
                    ReceiveStringVal(ret);
#endif
                }
            }
        }
        /// <summary>
        /// To load AnimationSingleMotion to specified FrameActor.
        /// (*) It necessary that you call "SetLoadTargetSingleMotion" beforely.
        /// </summary>
        /// <param name="param">JSON data - AnimationSingleMotion</param>
        public string LoadSingleMotion_body(string param)
        {
            string ret = "";

            if (currentProject.isReadOnly || currentProject.isSharing) return "";
            if (currentSeq != null)
            {
                currentSeq.Kill();
                currentSeq = null;
            }

            AnimationSingleMotion asm = JsonUtility.FromJson<AnimationSingleMotion>(param);
            
            if (SingleMotionTargetRole != null)
            { //---overwrite an existed cast and frame actor(timeline)
                NativeAnimationFrameActor naf = GetFrameActorFromRole(SingleMotionTargetRole.roleName, SingleMotionTargetRole.type);

                if (naf != null)
                {
                    if (naf.targetType != asm.targetType) return "typeerr";

                    naf.targetType = asm.targetType;
                    naf.compiled = asm.compiled;
                    naf.avatar = SingleMotionTargetRole;

                    naf.blendShapeList.Clear();
                    asm.blendShapeList.ForEach(item =>
                    {
                        naf.blendShapeList.Add(item);
                    });
                    Array.Copy(asm.bodyHeight, naf.bodyHeight, asm.bodyHeight.Length);

                    naf.bodyInfoList.Clear();
                    asm.bodyInfoList.ForEach(item =>
                    {
                        naf.bodyInfoList.Add(new Vector3(item.x, item.y, item.z));
                    });
                    naf.frames.Clear();

                    //---For returning HTML
                    AnimationFrameActor afa = new AnimationFrameActor();
                    afa.SetFromNative(naf);

                    foreach (AnimationSingleFrame fr in asm.frames)
                    {
                        if (fr.index < currentProject.timelineFrameLength)
                        {
                            NativeAnimationFrame naframe = new NativeAnimationFrame();
                            naframe.index = fr.index;
                            naframe.finalizeIndex = fr.finalizeIndex;
                            naframe.duration = fr.duration;
                            naframe.key = fr.key;
                            naframe.ease = fr.ease;
                            naframe.useBodyInfo = UseBodyInfoType.TimelineCharacter;

                            AnimationFrame aframe = new AnimationFrame();
                            aframe.SetFromNative(naframe);

                            foreach (string linedata in fr.movingData)
                            {
                                AnimationTargetParts atp = new AnimationTargetParts();
                                atp = CsvToFrameData(naf, asm.targetType, linedata, atp);
                                naframe.movingData.Add(atp);

                                //---For returning HTML
                                aframe.movingData.Add(linedata);
                            }

                            naf.frames.Add(naframe);

                            afa.frames.Add(aframe);
                        }
                        
                    }

                    ret = JsonUtility.ToJson(afa);
                }
            }
            return ret;
        }
//===========================================================================================================================
//  Save functions
//===========================================================================================================================

        /// <summary>
        /// To save animation project data.
        /// </summary>
        /// <returns>JSON format of AnimationProject</returns>
        public string SaveProject()
        {
            string ret = "";

            AnimationProject aniproj = new AnimationProject(initialFrameCount);
            aniproj.isSharing = currentProject.isSharing;
            aniproj.isReadOnly = currentProject.isReadOnly;
            aniproj.isNew = false;
            aniproj.isOpenAndEdit = currentProject.isOpenAndEdit;
            aniproj.mkey = DateTime.Now.ToFileTime();
            aniproj.baseDuration = currentProject.baseDuration;
            aniproj.timelineFrameLength = currentProject.timelineFrameLength;
            aniproj.meta = currentProject.meta.SCopy();
            aniproj.materialManager.SetFromNative(currentProject.materialManager);
            //---avatar
            currentProject.casts.ForEach(avatar =>
            {
                AnimationAvatar aa = new AnimationAvatar();

                aa.SetFromNative(avatar);
                aniproj.casts.Add(aa);
            });

            //---timeline characters
            foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
            {
                AnimationFrameActor rawactor = new AnimationFrameActor();
                rawactor.SetFromNative(actor);
                foreach (NativeAnimationFrame frame in actor.frames)
                {
                    AnimationFrame afg = new AnimationFrame();
                    afg.duration = frame.duration;
                    afg.finalizeIndex = frame.finalizeIndex;
                    afg.index = frame.index;
                    afg.key = frame.key;
                    afg.ease = frame.ease;
                    foreach (AnimationTargetParts mv in frame.movingData)
                    {
                        afg.movingData.Add(DataToCSV(rawactor.targetType, mv));
                        //afg.movingData.Add(mv);
                    }
                    rawactor.frames.Add(afg);
                }
                aniproj.timeline.characters.Add(rawactor);
            }

            /*currentProject.timeline.tmFrame.ForEach(timelineGroup =>
            {
                AnimationFrame afg = new AnimationFrame();
                afg.index = timelineGroup.index;
                afg.key = timelineGroup.key;

                //---frames
                timelineGroup.characters.ForEach(frame =>
                {
                    AnimationFrameActor af = new AnimationFrameActor();
                    af.targetId = frame.targetId;
                    af.targetRole = frame.targetRole;
                    af.targetType = frame.targetType;
                    frame.movingData.ForEach(mv =>
                    {
                        af.movingData.Add(DataToCSV(frame.targetType, mv));
                    });
                });
            });*/

            ret = JsonUtility.ToJson(aniproj);
#if !UNITY_EDITOR && UNITY_WEBGL
        
            ReceiveStringVal(ret);
        
#endif


            return ret;
        }

        /// <summary>
        /// Get cast and frame actor's timeline for single motion file to save
        /// </summary>
        /// <param name="param">CSV-string: [0] - roleName, [1] - type id</param>
        /// <returns>JSON format of AnimationSingleMotion</returns>
        public string SaveSingleMotion(string param)
        {
            string ret = "";
            string[] prm = param.Split(',');
            string roleName = prm[0];
            int tmp = int.TryParse(prm[1], out tmp) ? tmp : 99;
            AF_TARGETTYPE type = (AF_TARGETTYPE)tmp;

            AnimationSingleMotion asm = new AnimationSingleMotion();

            asm.targetType = type;
            asm.compiled = currentPlayingOptions.isCompileAnimation;

            NativeAnimationFrameActor naf = GetFrameActorFromRole(roleName, type);

            if (naf != null)
            {
                asm.blendShapeList = naf.blendShapeList;
                Array.Copy(naf.bodyHeight, asm.bodyHeight, naf.bodyHeight.Length);
                asm.bodyInfoList = naf.bodyInfoList;

                foreach (NativeAnimationFrame frame in naf.frames)
                {
                    AnimationSingleFrame asf = new AnimationSingleFrame();
                    asf.duration = frame.duration;
                    asf.finalizeIndex = frame.finalizeIndex;
                    asf.index = frame.index;
                    asf.key = frame.key;
                    
                    foreach (AnimationTargetParts mv in frame.movingData)
                    {
                        asf.movingData.Add(DataToCSV(type, mv));
                    }
                    asm.frames.Add(asf);
                }

                ret = JsonUtility.ToJson(asm);

            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
            return ret;
        }
    }

}
