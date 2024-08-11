using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        [DllImport("__Internal")]
        private static extern void IntervalLoadingProject(float val);

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
            List<string> catmats = new List<string>();

            catmats.Add(mat.name);
            //ln = mat.name + CST_SEPSTR_PROP;
            //if (currentProject.version >= 2)
            {
                //ln += mat.matName + CST_SEPSTR_PROP;
                catmats.Add(mat.matName);
            }
            //ln += mat.shaderName + CST_SEPSTR_PROP;
            catmats.Add(mat.shaderName);

            //---standard
            if (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_STD.ToLower())
            {
                /*ln += "#" + ColorUtility.ToHtmlStringRGBA(mat.color) + CST_SEPSTR_PROP +
                    mat.blendmode.ToString() + CST_SEPSTR_PROP +
                    mat.texturePath + CST_SEPSTR_PROP +
                    mat.metallic.ToString() + CST_SEPSTR_PROP +
                    mat.glossiness + CST_SEPSTR_PROP +
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.emissioncolor)
                ;*/
                catmats.Add("#" + ColorUtility.ToHtmlStringRGBA(mat.color));
                catmats.Add(mat.blendmode.ToString());
                catmats.Add(mat.texturePath);
                catmats.Add(mat.metallic.ToString());
                catmats.Add(mat.glossiness.ToString());
                catmats.Add("#" + ColorUtility.ToHtmlStringRGBA(mat.emissioncolor));
            }
            //---VRM/MToon
            else if ((mat.shaderName.ToLower() == OperateLoadedBase.SHAD_VRM.ToLower()) || (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_VRM10.ToLower()))
            {
                /*ln += "#" + ColorUtility.ToHtmlStringRGBA(mat.color) + CST_SEPSTR_PROP +
                    mat.cullmode.ToString() + CST_SEPSTR_PROP +
                    mat.blendmode.ToString() + CST_SEPSTR_PROP +
                    mat.texturePath + CST_SEPSTR_PROP +
                    //---v1 = 6, v2 = 7
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.emissioncolor) + CST_SEPSTR_PROP +
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.shadetexcolor) + CST_SEPSTR_PROP +
                    mat.shadingtoony.ToString() + CST_SEPSTR_PROP +
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.rimcolor) + CST_SEPSTR_PROP +
                    mat.rimfresnel.ToString() + CST_SEPSTR_PROP +
                    mat.srcblend.ToString() + CST_SEPSTR_PROP +
                    mat.dstblend.ToString() + CST_SEPSTR_PROP +
                    //---v2 = 14
                    mat.cutoff.ToString() + CST_SEPSTR_PROP +
                    mat.shadingshift.ToString() + CST_SEPSTR_PROP +
                    mat.receiveshadow.ToString() + CST_SEPSTR_PROP +
                    mat.shadinggrade.ToString() + CST_SEPSTR_PROP +
                    mat.lightcolorattenuation.ToString()
                ;*/
                catmats.Add("#" + ColorUtility.ToHtmlStringRGBA(mat.color));
                catmats.Add(mat.cullmode.ToString());
                catmats.Add(mat.blendmode.ToString());
                catmats.Add(mat.texturePath);
                //---v1 = 6, v2 = 7
                catmats.Add("#" + ColorUtility.ToHtmlStringRGBA(mat.emissioncolor));
                catmats.Add("#" + ColorUtility.ToHtmlStringRGBA(mat.shadetexcolor));
                catmats.Add(mat.shadingtoony.ToString());
                catmats.Add("#" + ColorUtility.ToHtmlStringRGBA(mat.rimcolor));
                catmats.Add(mat.rimfresnel.ToString());
                catmats.Add(mat.srcblend.ToString());
                catmats.Add(mat.dstblend.ToString());
                //---v2 = 14
                catmats.Add(mat.cutoff.ToString());
                catmats.Add(mat.shadingshift.ToString());
                catmats.Add(mat.receiveshadow.ToString());
                catmats.Add(mat.shadinggrade.ToString());
                catmats.Add(mat.lightcolorattenuation.ToString());

            }
            //---FX/Water4
            else if ( (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_WT.ToLower()) || (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_SWT.ToLower()) )
            {
                /*ln += "#" + ColorUtility.ToHtmlStringRGBA(mat.color) + CST_SEPSTR_PROP +
                    mat.fresnelScale.ToString() + CST_SEPSTR_PROP +
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.reflectionColor) + CST_SEPSTR_PROP +
                    "#" + ColorUtility.ToHtmlStringRGBA(mat.specularColor) + CST_SEPSTR_PROP +
                    //---6
                    mat.waveAmplitude.x + CST_SEPSTR_PROP + mat.waveAmplitude.y + CST_SEPSTR_PROP + mat.waveAmplitude.z + CST_SEPSTR_PROP + mat.waveAmplitude.w + CST_SEPSTR_PROP +
                    mat.waveFrequency.x + CST_SEPSTR_PROP + mat.waveFrequency.y + CST_SEPSTR_PROP + mat.waveFrequency.z + CST_SEPSTR_PROP + mat.waveFrequency.w + CST_SEPSTR_PROP +
                    mat.waveSteepness.x + CST_SEPSTR_PROP + mat.waveSteepness.y + CST_SEPSTR_PROP + mat.waveSteepness.z + CST_SEPSTR_PROP + mat.waveSteepness.w + CST_SEPSTR_PROP +
                    mat.waveSpeed.x     + CST_SEPSTR_PROP + mat.waveSpeed.y     + CST_SEPSTR_PROP + mat.waveSpeed.z     + CST_SEPSTR_PROP + mat.waveSpeed.w     + CST_SEPSTR_PROP +
                    mat.waveDirectionAB.x + CST_SEPSTR_PROP + mat.waveDirectionAB.y + CST_SEPSTR_PROP + mat.waveDirectionAB.z + CST_SEPSTR_PROP + mat.waveDirectionAB.w + CST_SEPSTR_PROP +
                    mat.waveDirectionCD.x + CST_SEPSTR_PROP + mat.waveDirectionCD.y + CST_SEPSTR_PROP + mat.waveDirectionCD.z + CST_SEPSTR_PROP + mat.waveDirectionCD.w + CST_SEPSTR_PROP +
                    mat.waveScale.ToString()
                ;*/
                catmats.Add("#" + ColorUtility.ToHtmlStringRGBA(mat.color));
                catmats.Add(mat.fresnelScale.ToString());
                catmats.Add("#" + ColorUtility.ToHtmlStringRGBA(mat.reflectionColor));
                catmats.Add("#" + ColorUtility.ToHtmlStringRGBA(mat.specularColor));
                //---6
                catmats.Add(mat.waveAmplitude.x.ToString()); catmats.Add(mat.waveAmplitude.y.ToString()); catmats.Add(mat.waveAmplitude.z.ToString()); catmats.Add(mat.waveAmplitude.w.ToString());
                catmats.Add(mat.waveFrequency.x.ToString()); catmats.Add(mat.waveFrequency.y.ToString()); catmats.Add(mat.waveFrequency.z.ToString()); catmats.Add(mat.waveFrequency.w.ToString());
                catmats.Add(mat.waveSteepness.x.ToString()); catmats.Add(mat.waveSteepness.y.ToString()); catmats.Add(mat.waveSteepness.z.ToString()); catmats.Add(mat.waveSteepness.w.ToString());
                catmats.Add(mat.waveSpeed.x.ToString()); catmats.Add(mat.waveSpeed.y.ToString()); catmats.Add(mat.waveSpeed.z.ToString()); catmats.Add(mat.waveSpeed.w.ToString());
                catmats.Add(mat.waveDirectionAB.x.ToString()); catmats.Add(mat.waveDirectionAB.y.ToString()); catmats.Add(mat.waveDirectionAB.z.ToString()); catmats.Add(mat.waveDirectionAB.w.ToString());
                catmats.Add(mat.waveDirectionCD.x.ToString()); catmats.Add(mat.waveDirectionCD.y.ToString()); catmats.Add(mat.waveDirectionCD.z.ToString()); catmats.Add(mat.waveDirectionCD.w.ToString());
                catmats.Add(mat.waveScale.ToString());

            }
            else if (mat.shaderName.ToLower() == "fx/water (basic)")
            {
                /*ln += mat.waveScale.ToString() + CST_SEPSTR_PROP +
                    mat.waveSpeed.x + CST_SEPSTR_PROP + mat.waveSpeed.y + CST_SEPSTR_PROP + mat.waveSpeed.z + CST_SEPSTR_PROP + mat.waveSpeed.w
                ;*/

                catmats.Add(mat.waveScale.ToString());
                catmats.Add(mat.waveSpeed.x.ToString()); catmats.Add(mat.waveSpeed.y.ToString()); catmats.Add(mat.waveSpeed.z.ToString()); catmats.Add(mat.waveSpeed.w.ToString());

            }
            else if (mat.shaderName.ToLower() == "lux water/watersurface")
            {
                //ln += mat.waveScale.ToString()
                //;
                catmats.Add(mat.waveScale.ToString());

            }
            else if ((mat.shaderName.ToLower() == OperateLoadedBase.SHAD_SKE.ToLower()) || (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_PSKE.ToLower()))
            {
                catmats.Add(mat.outlinewidth.ToString());
                catmats.Add(mat.strokedensity.ToString());
                catmats.Add(mat.addbrightness.ToString());
                catmats.Add(mat.multbrightness.ToString());
                catmats.Add(mat.shadowbrightness.ToString());

            }
            else if (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_REALTOON.ToLower())
            {
                catmats.Add(mat.enableTexTransparent.ToString());
                catmats.Add(mat.mainColorInAmbientLightOnly.ToString());
                catmats.Add(mat.doubleSided.ToString());
                catmats.Add(mat.outlineZPosCam.ToString());
                catmats.Add(mat.thresHold.ToString());
                catmats.Add(mat.shadowHardness.ToString());
            }
            else if (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_COMIC.ToLower())
            {
                catmats.Add(mat.enableTexTransparent.ToString());
                catmats.Add(mat.lineWidth.ToString());
                catmats.Add("#" + ColorUtility.ToHtmlStringRGBA(mat.lineColor));
                catmats.Add(mat.tone1Threshold.ToString());
            }
            else if (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_ICE.ToLower())
            {
                catmats.Add("#" + ColorUtility.ToHtmlStringRGBA(mat.iceColor));
                catmats.Add(mat.transparency.ToString());
                catmats.Add(mat.baseTransparency.ToString());
                catmats.Add(mat.iceRoughness.ToString());
                catmats.Add(mat.distortion.ToString());

            }
            else if (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_MICRA.ToLower())
            {
                catmats.Add(mat.pixelSize.ToString());
            }
            else if (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_CUSTOMCUTOUT.ToLower())
            {
                catmats.Add("#" + ColorUtility.ToHtmlStringRGBA(mat.color));
            }

            ln = String.Join(CST_SEPSTR_PROP, catmats);

            return ln;
        }
        private bool SetObjectMaterial(string rawstr, List<MaterialProperties> matProp, string sepprop, string sepitem)
        {
            bool ret = false;
            string[] arr = rawstr.Split(sepitem);
            foreach (string matstr in arr)
            {
                if (matstr == "") continue;

                MaterialProperties mat = new MaterialProperties();
                string[] matc = matstr.Split(sepprop);

                int inx = 0;

                mat.name = matc[inx++];
                if (currentProject.version >= 2)
                {
                    mat.matName = matc[inx++];
                }
                mat.shaderName = matc[inx++];

                if (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_STD.ToLower())
                {
                    //---most less is 8 items.
                    if (matc.Length < 8) continue;

                    mat.color = ColorUtility.TryParseHtmlString(matc[inx++], out mat.color) ? mat.color : Color.white;
                    mat.blendmode = float.TryParse(matc[inx++], out mat.blendmode) ? mat.blendmode : 0;
                    mat.texturePath = matc[inx++];

                    //v1 = 5 ~ 7, v2 = 6 ~ 8
                    mat.metallic = float.TryParse(matc[inx++], out mat.metallic) ? mat.metallic : 0;
                    mat.glossiness = float.TryParse(matc[inx++], out mat.glossiness) ? mat.glossiness : 0;
                    mat.emissioncolor = ColorUtility.TryParseHtmlString(matc[inx++], out mat.emissioncolor) ? mat.emissioncolor : Color.white;

                }
                else if ((mat.shaderName.ToLower() == OperateLoadedBase.SHAD_VRM.ToLower()) || (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_VRM10.ToLower()))
                {
                    //---most less is 13 items.
                    if (matc.Length < 13) continue;

                    // v1 = 2 ~ 5, v2 = 3 ~ 6
                    mat.color = ColorUtility.TryParseHtmlString(matc[inx++], out mat.color) ? mat.color : Color.white;
                    mat.cullmode = float.TryParse(matc[inx++], out mat.cullmode) ? mat.cullmode : 0;
                    mat.blendmode = float.TryParse(matc[inx++], out mat.blendmode) ? mat.blendmode : 0;
                    mat.texturePath = matc[inx++];

                    // v1 = 6 ~ 12, v2 = 7 ~ 13
                    mat.emissioncolor = ColorUtility.TryParseHtmlString(matc[inx++], out mat.emissioncolor) ? mat.emissioncolor : Color.white;
                    mat.shadetexcolor = ColorUtility.TryParseHtmlString(matc[inx++], out mat.shadetexcolor) ? mat.shadetexcolor : Color.white;
                    mat.shadingtoony = float.TryParse(matc[inx++], out mat.shadingtoony) ? mat.shadingtoony : 0;
                    mat.rimcolor = ColorUtility.TryParseHtmlString(matc[inx++], out mat.rimcolor) ? mat.rimcolor : Color.white;
                    mat.rimfresnel = float.TryParse(matc[inx++], out mat.rimfresnel) ? mat.rimfresnel : 0;
                    mat.srcblend = float.TryParse(matc[inx++], out mat.srcblend) ? mat.srcblend : 0;
                    mat.dstblend = float.TryParse(matc[inx++], out mat.dstblend) ? mat.dstblend : 0;
                    // v2 = 14
                    if ((currentProject.version >= 2) && (matc.Length > 17))
                    {
                        mat.cutoff = float.TryParse(matc[inx++], out mat.cutoff) ? mat.cutoff : 0.5f;
                        mat.shadingshift = float.TryParse(matc[inx++], out mat.shadingshift) ? mat.shadingshift : 0;
                        mat.receiveshadow = float.TryParse(matc[inx++], out mat.receiveshadow) ? mat.receiveshadow : 1;
                        mat.shadinggrade = float.TryParse(matc[inx++], out mat.shadinggrade) ? mat.shadinggrade : 1;
                        mat.lightcolorattenuation = float.TryParse(matc[inx++], out mat.lightcolorattenuation) ? mat.lightcolorattenuation : 0;
                    }
                    

                }
                else if ( (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_WT.ToLower()) || (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_SWT.ToLower()) )
                {
                    //---most less is 31 items.
                    if (matc.Length < 31) continue;

                    // v1 = 2 ~ 5, v2 = 3 ~ 6
                    mat.color = ColorUtility.TryParseHtmlString(matc[inx++], out mat.color) ? mat.color : Color.white;
                    mat.fresnelScale = float.TryParse(matc[inx++], out mat.fresnelScale) ? mat.fresnelScale : 0;
                    mat.reflectionColor = ColorUtility.TryParseHtmlString(matc[inx++], out mat.reflectionColor) ? mat.reflectionColor : Color.white;
                    mat.specularColor = ColorUtility.TryParseHtmlString(matc[inx++], out mat.specularColor) ? mat.specularColor : Color.white;
                    { // v1 = 6 ~ 29, v2 = 7 ~ 30
                        float x = float.TryParse(matc[inx++], out x) ? x : 0;
                        float y = float.TryParse(matc[inx++], out y) ? y : 0;
                        float z = float.TryParse(matc[inx++], out z) ? z : 0;
                        float w = float.TryParse(matc[inx++], out w) ? w : 0;
                        mat.waveAmplitude = new Vector4(x, y, z, w);
                    }
                    {
                        float x = float.TryParse(matc[inx++], out x) ? x : 0;
                        float y = float.TryParse(matc[inx++], out y) ? y : 0;
                        float z = float.TryParse(matc[inx++], out z) ? z : 0;
                        float w = float.TryParse(matc[inx++], out w) ? w : 0;
                        mat.waveFrequency = new Vector4(x, y, z, w);
                    }
                    {
                        float x = float.TryParse(matc[inx++], out x) ? x : 0;
                        float y = float.TryParse(matc[inx++], out y) ? y : 0;
                        float z = float.TryParse(matc[inx++], out z) ? z : 0;
                        float w = float.TryParse(matc[inx++], out w) ? w : 0;
                        mat.waveSteepness = new Vector4(x, y, z, w);
                    }
                    {
                        float x = float.TryParse(matc[inx++], out x) ? x : 0;
                        float y = float.TryParse(matc[inx++], out y) ? y : 0;
                        float z = float.TryParse(matc[inx++], out z) ? z : 0;
                        float w = float.TryParse(matc[inx++], out w) ? w : 0;
                        mat.waveSpeed = new Vector4(x, y, z, w);
                    }
                    {
                        float x = float.TryParse(matc[inx++], out x) ? x : 0;
                        float y = float.TryParse(matc[inx++], out y) ? y : 0;
                        float z = float.TryParse(matc[inx++], out z) ? z : 0;
                        float w = float.TryParse(matc[inx++], out w) ? w : 0;
                        mat.waveDirectionAB = new Vector4(x, y, z, w);
                    }
                    {
                        float x = float.TryParse(matc[inx++], out x) ? x : 0;
                        float y = float.TryParse(matc[inx++], out y) ? y : 0;
                        float z = float.TryParse(matc[inx++], out z) ? z : 0;
                        float w = float.TryParse(matc[inx++], out w) ? w : 0;
                        mat.waveDirectionCD = new Vector4(x, y, z, w);
                    }
                    // v1 = 30, v2 = 31
                    mat.waveScale = float.TryParse(matc[inx++], out mat.waveScale) ? mat.waveScale : 0;

                }
                else if (mat.shaderName.ToLower() == "fx/water (basic)")
                {
                    //---most less is 7 items.
                    if (matc.Length < 7) continue;

                    // v1 = 2, v2 = 3
                    mat.waveScale = float.TryParse(matc[inx++], out mat.waveScale) ? mat.waveScale : 0;
                    { // v1 = 3 ~ 6, v2 = 4 ~ 7
                        float x = float.TryParse(matc[inx++], out x) ? x : 0;
                        float y = float.TryParse(matc[inx++], out y) ? y : 0;
                        float z = float.TryParse(matc[inx++], out z) ? z : 0;
                        float w = float.TryParse(matc[inx++], out w) ? w : 0;
                        mat.waveSpeed = new Vector4(x, y, z, w);
                    }
                }
                else if (mat.shaderName.ToLower() == "lux water/watersurface")
                {
                    // v1 = 2, v2 = 3
                    mat.waveScale = float.TryParse(matc[inx++], out mat.waveScale) ? mat.waveScale : 0;
                }
                else if ((mat.shaderName.ToLower() == OperateLoadedBase.SHAD_SKE.ToLower()) || (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_PSKE.ToLower()))
                {
                    //---most less is 3 items.
                    if (matc.Length < 5) continue;

                    // 
                    mat.outlinewidth = float.TryParse(matc[inx++], out mat.outlinewidth) ? mat.outlinewidth : 0.1f;
                    mat.strokedensity = float.TryParse(matc[inx++], out mat.strokedensity) ? mat.strokedensity : 0.5f;
                    mat.addbrightness = float.TryParse(matc[inx++], out mat.addbrightness) ? mat.addbrightness : 1;
                    mat.multbrightness = float.TryParse(matc[inx++], out mat.multbrightness) ? mat.multbrightness : 1;
                    mat.shadowbrightness = float.TryParse(matc[inx++], out mat.shadowbrightness) ? mat.shadowbrightness : 1;

                }
                else if (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_REALTOON.ToLower())
                {
                    mat.enableTexTransparent = float.TryParse(matc[inx++], out mat.enableTexTransparent) ? mat.enableTexTransparent : 0;
                    mat.mainColorInAmbientLightOnly = float.TryParse(matc[inx++], out mat.mainColorInAmbientLightOnly) ? mat.mainColorInAmbientLightOnly : 0;
                    mat.doubleSided = int.TryParse(matc[inx++], out mat.doubleSided) ? mat.doubleSided : 0;
                    mat.outlineZPosCam = float.TryParse(matc[inx++], out mat.outlineZPosCam) ? mat.outlineZPosCam : 0;
                    mat.thresHold = float.TryParse(matc[inx++], out mat.thresHold) ? mat.thresHold : 0.85f;
                    mat.shadowHardness = float.TryParse(matc[inx++], out mat.shadowHardness) ? mat.shadowHardness : 1;

                }
                else if (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_COMIC.ToLower())
                {
                    mat.enableTexTransparent = float.TryParse(matc[inx++], out mat.enableTexTransparent) ? mat.enableTexTransparent : 0;
                    mat.lineWidth = float.TryParse(matc[inx++], out mat.lineWidth) ? mat.lineWidth : 0.01f;
                    mat.lineColor = ColorUtility.TryParseHtmlString(matc[inx++], out mat.lineColor) ? mat.lineColor : new Color(0.1f, 0.1f, 0.1f, 1.0f);
                    mat.tone1Threshold = float.TryParse(matc[inx++], out mat.tone1Threshold) ? mat.tone1Threshold : 0.1f;
                    
                }
                else if (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_ICE.ToLower())
                {
                    mat.iceColor = ColorUtility.TryParseHtmlString(matc[inx++], out mat.iceColor) ? mat.iceColor : Color.white;
                    mat.transparency = float.TryParse(matc[inx++], out mat.transparency) ? mat.transparency : 1.5f;
                    mat.baseTransparency = float.TryParse(matc[inx++], out mat.baseTransparency) ? mat.baseTransparency : 0.5f;
                    mat.iceRoughness = float.TryParse(matc[inx++], out mat.iceRoughness) ? mat.iceRoughness : 0.005f;
                    mat.distortion = float.TryParse(matc[inx++], out mat.distortion) ? mat.distortion : 1;
                   
                }
                else if (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_MICRA.ToLower())
                {
                    mat.pixelSize = float.TryParse(matc[inx++], out mat.pixelSize) ? mat.pixelSize : 0.01f;
                }
                else if (mat.shaderName.ToLower() == OperateLoadedBase.SHAD_CUSTOMCUTOUT.ToLower())
                {
                    mat.color = ColorUtility.TryParseHtmlString(matc[inx++], out mat.color) ? mat.color : Color.white;
                }
                matProp.Add(mat);
            }
            if (matProp.Count > 0) ret = true;
            return ret;
        }
        private AnimationTranslateTargetParts CsvToFrameTranslateData(NativeAnimationFrameActor actor, AF_TARGETTYPE targetType, string param, AnimationTranslateTargetParts atp)
        {
            string[] lst = param.Split(',');
            if (lst.Length < 3)
            {
                //---error check
                return atp;
            }
            int boneParts;

            int.TryParse(lst[CSV_PARTS], out boneParts);
            string optParts = lst[CSV_OPTPARTS]; //.ToLower();
            string movetype = lst[CSV_ANIMTYPE];

            int valueCount = int.TryParse(lst[CSV_VALCNT], out valueCount) ? valueCount : 1;

            //---start each type 
            if (targetType == AF_TARGETTYPE.VRM)
            {
                atp.vrmBone = (ParseIKBoneType)boneParts;
                //---check numbers of received value.


                //if ((atp.vrmBone >= ParseIKBoneType.IKParent) && (atp.vrmBone <= ParseIKBoneType.RightLeg))
                if (IsVRMParseBoneType(atp.vrmBone))
                {

                    if (valueCount > 2)
                    {
                        //---Here, Value must be 3.
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                        if (movetype == "position")
                        {
                            atp.animationType = AF_MOVETYPE.Translate;
                            atp.values.Add(new Vector3(vec3[0], vec3[1], vec3[2]));
                            atp.jumpNum = (int)vec3[3];
                            atp.jumpPower = vec3[4];
                        }
                    }
                }
            }
            else
            { //---For  Other object, Light, Camera, Image, Effect, Text, UImage, Audio, SystemEffect------------------

                //---Common parse
                
                //---Here, Value must be 3.
                float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                if (movetype == "position")
                {
                    atp.animationType = AF_MOVETYPE.Translate;
                    atp.values.Add(new Vector3(vec3[0], vec3[1], vec3[2]));
                    atp.jumpNum = (int)vec3[3];
                    atp.jumpPower = vec3[4];
                }

            }

            return atp;
        }
        /// <summary>
        /// Convert "movingData" CSV-format to AnimationTargetParts
        /// </summary>
        /// <param name="actor">Target object to load</param>
        /// <param name="targetType">Target object type</param>
        /// <param name="param">csv data to load</param>
        /// <param name="atp">destination frame data</param>
        /// <param name="file_version">file format version</param>
        /// <returns></returns>
        private AnimationTargetParts CsvToFrameData(NativeAnimationFrameActor actor, AF_TARGETTYPE targetType, string param, AnimationTargetParts atp, int file_version)
        {
            string[] lst = param.Split(',',StringSplitOptions.None);
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

            if ((movetype == "rest") &&
                ((targetType != AF_TARGETTYPE.OtherObject) && (targetType != AF_TARGETTYPE.Audio) && (targetType != AF_TARGETTYPE.Effect))
            )
            { // rest exists in OtherObject, Audio, Effect.
                return atp;
            }
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
            else if (movetype == "rigid")
            { //---version >= 5
                float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                atp.animationType = AF_MOVETYPE.Rigid;
                atp.rigidDrag = vec3[0];
                atp.rigidAngularDrag = vec3[1];
                atp.useCollision = (int)vec3[2];
                atp.useRigidGravity = (int)vec3[3];
            }

            //---start each type 
            if (targetType == AF_TARGETTYPE.VRM)
            {
                atp.vrmBone = (ParseIKBoneType)boneParts;
                //---check numbers of received value.


                //if ((atp.vrmBone >= ParseIKBoneType.IKParent) && (atp.vrmBone <= ParseIKBoneType.RightLeg))
                if (IsVRMParseBoneType(atp.vrmBone))

                {

                    if (valueCount > 2)
                    {
                        //---Here, Value must be 3.
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                        if (movetype == "xxxposition")
                        {
                            atp.animationType = AF_MOVETYPE.Translate;
                            atp.position = new Vector3(vec3[0], vec3[1], vec3[2]);
                            atp.jumpNum = (int)vec3[3];
                            atp.jumpPower = vec3[4];
                        }
                        else if ((movetype == "rotation") || (movetype == "rotation:v5"))
                        {
                            atp.animationType = AF_MOVETYPE.Rotate;
                            if ((movetype == "rotation") && (atp.vrmBone == ParseIKBoneType.Aim))
                            { //---if before version 5, Y-axis minus 180 angles.
                                atp.rotation = new Vector3(vec3[0], vec3[1], vec3[2]);
                            }
                            else
                            {
                                atp.rotation = new Vector3(vec3[0], vec3[1], vec3[2]);
                            }
                            
                            if (valueCount > 3)
                            { //---add for rotate 360. (2023.05.05)
                                atp.isRotate360 = (int)vec3[3];
                            }
                            else
                            {
                                atp.isRotate360 = 0;
                            }
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

                        if (movetype == "xxxposition")
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
                        //---parse preset hand pose (4 ~ 5)
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, 2); 
                        atp.animationType = AF_MOVETYPE.NormalTransform;
                        atp.handpose = new List<float>();
                        atp.isHandPose = 1;
                        atp.handpose.Add(vec3[0]);
                        atp.handpose.Add(vec3[1]);

                        // manually finger pose (6 ~ 10)
                        AvatarFingerForHPC afih = new AvatarFingerForHPC();
                        if (lst.Length > 10) {
                            afih.Thumbs = HandPoseController.ParseFinger(lst[6], "t");
                            afih.Index = HandPoseController.ParseFinger(lst[7], "i");
                            afih.Middle = HandPoseController.ParseFinger(lst[8], "m");
                            afih.Ring = HandPoseController.ParseFinger(lst[9], "r");
                            afih.Little = HandPoseController.ParseFinger(lst[10], "l");
                        }

                        atp.animationType = AF_MOVETYPE.NormalTransform;
                        atp.isHandPose = 1;
                        atp.fingerpose = afih;

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

                            if (file_version >= 4)
                            { //---convert and apply: PROX:happy=0.7, PROX:FCL_MTH_Angry=0.65, ...
                                if (blendIndex == -1)
                                { //---if X of x=n is string, X is Shape key. directly save to movingData.blendshapes.
                                    atp.isBlendShape = 1;
                                    atp.blendshapes.Add(new BasicBlendShapeKey(blendItem[0], blendVal, 1));
                                }
                            }
                            else
                            {
                                if (
                                (blendIndex > -1) &&
                                (blendIndex < actor.blendShapeList.Count)
                            )
                                {
                                    //---convert and apply: 0=99,1=45  --> blendShapeList[0] (probably...PROX:happy) , blendShapeList[1](PROX:angry)
                                    //CAUTION: 別VRMのモーションを読み込むと動かさないシェイプが出る可能性あり。
                                    string bname = actor.blendShapeList[blendIndex];
                                    atp.isBlendShape = 1;
                                    //---registered key ONLY ( Therefore, is_changed is 1)
                                    atp.blendshapes.Add(new BasicBlendShapeKey(bname, blendVal, 1));
                                }
                            }                            

                        }

                        //atp.blendshapes.Add(new BasicStringFloatList(optParts, vec3[0]));
                    }
                }
                else if (movetype == "autoblendshape")
                {
                    if (valueCount > 0)
                    {
                        atp.animationType = AF_MOVETYPE.VRMAutoBlendShape;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.isblink = (int)vec3[0];
                        atp.interval = vec3[1];
                        atp.openingSeconds = vec3[2];
                        atp.closeSeconds = vec3[3];

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
                            //---version >= 4
                            if (file_version >= 4)
                            {
                                if (equipItem.Length > 1)
                                {
                                    int intflag = int.TryParse(equipItem[2], out intflag) ? intflag : 0;
                                    aes.equipflag = intflag;
                                }
                            }
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
                        if (file_version >= 6)
                        {
                            if (gravIndex == -1)
                            {
                                int boneListHit = actor.gravityBoneList.FindIndex(match =>
                                {
                                    if (match == graitem[0]) return true;
                                    return false;
                                });
                                if (boneListHit > -1)
                                {
                                    VRMGravityInfo vgi = new VRMGravityInfo();
                                    string[] g_name = actor.gravityBoneList[boneListHit].Split('/');
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
                        else
                        {
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
                else if (movetype == "animstart")
                {
                    atp.animationType = AF_MOVETYPE.AnimStart;
                    atp.animPlaying = UserAnimationState.Play;
                    string[] arr = optParts.Split(CST_SEPSTR_PROP);
                    atp.text = arr[0];
                    if (arr.Length > 1) atp.animName = arr[1];
                    float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                    atp.animSeek = vec3[0];
                    if (vec3.Length > 1) atp.animSpeed = vec3[1];
                    if (vec3.Length > 2) atp.animLoop = (int)vec3[2];
                }
                else if (movetype == "animstop")
                {
                    atp.animationType = AF_MOVETYPE.AnimStart;// AnimStop;
                    atp.animPlaying = UserAnimationState.Stop;
                    string[] arr = optParts.Split(CST_SEPSTR_PROP);
                    atp.text = arr[0];
                    if (arr.Length > 1) atp.animName = arr[1];
                    float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                    atp.animSeek = vec3[0];
                    if (vec3.Length > 1) atp.animSpeed = vec3[1];
                    if (vec3.Length > 2) atp.animLoop = (int)vec3[2];
                }
                else if (movetype == "animseek")
                {
                    atp.animationType = AF_MOVETYPE.AnimStart;// AnimSeek;
                    atp.animPlaying = UserAnimationState.Seeking;
                    string[] arr = optParts.Split(CST_SEPSTR_PROP);
                    atp.text = arr[0];
                    if (arr.Length > 1) atp.animName = arr[1];
                    float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                    atp.animSeek = vec3[0];
                    if (vec3.Length > 1) atp.animSpeed = vec3[1];
                    if (vec3.Length > 2) atp.animLoop = (int)vec3[2];
                }
                else if (movetype == "animpause")
                {
                    atp.animationType = AF_MOVETYPE.AnimStart;// AnimPause;
                    atp.animPlaying = UserAnimationState.Pause;
                    string[] arr = optParts.Split(CST_SEPSTR_PROP);
                    atp.text = arr[0];
                    if (arr.Length > 1) atp.animName = arr[1];
                    float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                    atp.animSeek = vec3[0];
                    if (vec3.Length > 1) atp.animSpeed = vec3[1];
                    if (vec3.Length > 2) atp.animLoop = (int)vec3[2];
                }
                else if (movetype == "rest")
                {
                    atp.animationType = AF_MOVETYPE.AnimStart;// Rest;
                    atp.animPlaying = UserAnimationState.Playing;
                    string[] arr = optParts.Split(CST_SEPSTR_PROP);
                    atp.text = arr[0];
                    if (arr.Length > 1) atp.animName = arr[1];
                    float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                    atp.animSeek = vec3[0];
                    if (vec3.Length > 1) atp.animSpeed = vec3[1];
                    if (vec3.Length > 2) atp.animLoop = (int)vec3[2];
                }
                else if (movetype == "vrmaanimstop")
                {
                    atp.animationType = AF_MOVETYPE.AnimStop;
                    atp.animPlaying = UserAnimationState.Stop;


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
            }
            else
            { //---For  Other object, Light, Camera, Image, Effect, Text, UImage, Audio, SystemEffect------------------

                //---Common parse
                if ((movetype == "xxxposition") || (movetype == "rotation") || (movetype == "scale"))
                {
                    //---Here, Value must be 3.
                    float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                    if (movetype == "xxxposition")
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
                        if (vec3.Length > 1) atp.animSpeed = vec3[1];
                        if (vec3.Length > 2) atp.animLoop = (int)vec3[2];
                    }
                    else if (movetype == "animstop")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStart;// AnimStop;
                        atp.animPlaying = UserAnimationState.Stop;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.animSeek = vec3[0];
                        if (vec3.Length > 1) atp.animSpeed = vec3[1];
                        if (vec3.Length > 2) atp.animLoop = (int)vec3[2];
                    }
                    else if (movetype == "animseek")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStart;// AnimSeek;
                        atp.animPlaying = UserAnimationState.Seeking;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.animSeek = vec3[0];
                        if (vec3.Length > 1) atp.animSpeed = vec3[1];
                        if (vec3.Length > 2) atp.animLoop = (int)vec3[2];
                    }
                    else if (movetype == "animpause")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStart;// AnimPause;
                        atp.animPlaying = UserAnimationState.Pause;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.animSeek = vec3[0];
                        if (vec3.Length > 1) atp.animSpeed = vec3[1];
                        if (vec3.Length > 2) atp.animLoop = (int)vec3[2];
                    }
                    else if (movetype == "rest")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStart;// Rest;
                        atp.animPlaying = UserAnimationState.Playing;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.animSeek = vec3[0];
                        if (vec3.Length > 1) atp.animSpeed = vec3[1];
                        if (vec3.Length > 2) atp.animLoop = (int)vec3[2];
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
                        atp.lightType = (LightType)vec3[8];

                        if (vec3.Length > 9)
                        {
                            atp.halo = (int)vec3[9];
                            atp.flareType = (int)vec3[10];
                            atp.flareColor = new Color(vec3[11], vec3[12], vec3[13], vec3[14]);
                            atp.flareBrightness = vec3[15];
                            atp.flareFade = vec3[16];
                        }
                    }
                }
                else if (targetType == AF_TARGETTYPE.Camera)
                {
                    float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                    if (movetype == "cameraon")
                    {
                        atp.animationType = AF_MOVETYPE.Camera;// CameraOn;
                        atp.animPlaying = UserAnimationState.Play;
                        atp.cameraPlaying = (int)UserAnimationState.Play;
                    }
                    else if (movetype == "camera")
                    {
                        atp.animationType = AF_MOVETYPE.Camera;
                        atp.animPlaying = UserAnimationState.Playing;
                        atp.cameraPlaying = (int)UserAnimationState.Playing;
                    }
                    else if (movetype == "cameraoff")
                    {
                        atp.animationType = AF_MOVETYPE.Camera;// CameraOff;
                        atp.animPlaying = UserAnimationState.Stop;
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
                else if ((targetType == AF_TARGETTYPE.Text) || (targetType == AF_TARGETTYPE.Text3D))
                {
                    if (movetype == "text")
                    {
                        atp.animationType = AF_MOVETYPE.Text;
                        string tmptext = lst[CSV_BEGINVAL];
                        if (lst.Length > 1)
                        { //---if lst is multiple, split , . re-join by ,.
                            List<string> tmps = new List<string>();
                            for (int i = CSV_BEGINVAL; i < lst.Length; i++)
                            {
                                tmps.Add(lst[i]);
                            }
                            tmptext = String.Join(',', tmps);
                        }
                        
                        atp.text = tmptext;
                    }
                    else if (movetype == "textprop")
                    {
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                        atp.animationType = AF_MOVETYPE.TextProperty;
                        atp.color = new Color(vec3[0], vec3[1], vec3[2], vec3[3]);
                        atp.fontSize = (int)vec3[4];
                        //---version >= 5
                        if (file_version < 5)
                        { //---convert text fontstyleto TextMeshPro styles string(original)
                            if (vec3[5] == 0) atp.fontStyles = "-----";
                            if (vec3[5] == 1) atp.fontStyles = "b----";
                            if (vec3[5] == 2) atp.fontStyles = "-i---";
                            if (vec3[5] == 3) atp.fontStyles = "bi---";

                        }
                    }
                    else if (movetype == "textprop:v5")
                    {
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                        atp.animationType = AF_MOVETYPE.TextProperty;
                        atp.color = new Color(vec3[0], vec3[1], vec3[2], vec3[3]);
                        atp.fontSize = (int)vec3[4];


                        int floatedPos = CSV_BEGINVAL + valueCount;
                        if ((file_version >= 5) && (lst.Length > (floatedPos)))
                        {
                            //---index: start from 4 + 5(float parameters max) + 1 ~ n
                            atp.fontStyles = lst[floatedPos];

                            atp.textAlignmentOptions = lst[floatedPos + 1];

                            int cnv_textOverflow = 0;
                            atp.textOverflow = int.TryParse(lst[floatedPos + 2], out cnv_textOverflow) ? cnv_textOverflow : -1;

                            if (targetType == AF_TARGETTYPE.Text)
                            {
                                atp.dimension = lst[floatedPos + 3];
                            }
                            else if (targetType == AF_TARGETTYPE.Text3D)
                            {
                                atp.dimension = lst[floatedPos + 3];
                            }

                            bool isgradient = lst[floatedPos + 4] == "1" ? true : false;
                            atp.IsGradient = isgradient;

                            string[] strcolors = lst[floatedPos + 5].Split(CST_SEPSTR_ITEM);
                            atp.gradients = new Color[4];
                            for (int i = 0; i < strcolors.Length; i++)
                            {
                                ColorUtility.TryParseHtmlString(strcolors[i], out atp.gradients[i]);
                            }

                            float.TryParse(lst[floatedPos + 6], out atp.fontOutlineWidth);

                            ColorUtility.TryParseHtmlString(lst[floatedPos + 7], out atp.fontOutlineColor);

                        }


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
                        atp.animPlaying = UserAnimationState.Play;
                        atp.audioName = optParts;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        //atp.isSE = (int)vec3[0];
                        atp.seekTime = vec3[1];
                    }
                    else if (movetype == "animstop")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStart;// AnimStop;
                        atp.animPlaying = UserAnimationState.Stop;
                        atp.audioName = optParts;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        //atp.isSE = (int)vec3[0];
                        atp.seekTime = vec3[1];
                    }
                    else if (movetype == "animpause")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStart;// AnimPause;
                        atp.animPlaying = UserAnimationState.Pause;
                        atp.audioName = optParts;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        //atp.isSE = (int)vec3[0];
                        atp.seekTime = vec3[1];
                    }
                    else if (movetype == "animseek")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStart;// AnimSeek;
                        atp.animPlaying = UserAnimationState.Seeking;
                        atp.audioName = optParts;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        //atp.isSE = (int)vec3[0];
                        atp.seekTime = vec3[1];

                    }
                    else if (movetype == "rest")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStart;// Rest;
                        atp.animPlaying = UserAnimationState.Playing;
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
                        atp.animationType = AF_MOVETYPE.AnimStart;// AnimPause;
                        atp.animPlaying = UserAnimationState.Pause;
                    }
                    else if (movetype == "animstop")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStart;// AnimStop;
                        atp.animPlaying = UserAnimationState.Stop;
                    }
                    else if (movetype == "rest")
                    {
                        atp.animationType = AF_MOVETYPE.AnimStart;// Rest;
                        atp.animPlaying = UserAnimationState.Playing;
                    }
                    else if (movetype == "collider")
                    {
                        atp.animationType = AF_MOVETYPE.Collider;
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
                        atp.animPlaying = UserAnimationState.Play;
                        List<float> flst = new List<float>(vec3);
                        atp.effectValues = flst;
                    }
                    else if (movetype == "systemeffectoff")
                    {
                        atp.animationType = AF_MOVETYPE.SystemEffect;
                        atp.animPlaying = UserAnimationState.Stop;
                        if (valueCount > 0)
                        {
                            List<float> flst = new List<float>(vec3);
                            atp.effectValues = flst;

                        }
                    }
                }
                else if (targetType == AF_TARGETTYPE.Stage)
                {
                    if (movetype == "stage")
                    {
                        atp.animationType = AF_MOVETYPE.Stage;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.stageType = (int)vec3[0];
                    }
                    else if (movetype == "stageprop")
                    {
                        atp.animationType = AF_MOVETYPE.StageProperty;
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);
                        atp.stageType = (int)vec3[0];
                        //---windzone
                        atp.windPower = vec3[1];
                        atp.windFrequency = vec3[2];
                        atp.windDurationMin = vec3[3];
                        atp.windDurationMax = vec3[4];

                        //---stage 
                        //Vector4 vec4 = new Vector4(vec3[1], vec3[2], vec3[3], vec3[4]);
                        //atp.wavespeed = vec4;
                        //atp.wavescale = vec3[5];


                        if (atp.stageType == (int)StageKind.User)
                        {
                            MaterialProperties matmain = new MaterialProperties();

                            //---user stage
                            matmain.metallic = vec3[5];
                            matmain.glossiness = vec3[6];
                            matmain.emissioncolor = new Color(vec3[7], vec3[8], vec3[9], vec3[10]);
                            matmain.color = new Color(vec3[11], vec3[12], vec3[13], vec3[14]);
                            matmain.blendmode = vec3[15];

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
                        else if ((atp.stageType == (int)StageKind.BasicSeaLevel) || (atp.stageType == (int)StageKind.SeaDaytime) || (atp.stageType == (int)StageKind.SeaNight))
                        {
                            List<MaterialProperties> tmpmats = new List<MaterialProperties>();
                            SetObjectMaterial(lst[9], tmpmats, CST_SEPSTR_PROP, CST_SEPSTR_ITEM);
                            if (tmpmats.Count > 0)
                            {
                                atp.vmatProp = tmpmats[0];
                            }

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
                            if (arr.Length > 1)
                            {
                                float arrfloat = float.TryParse(arr[1], out arrfloat) ? arrfloat : 0;
                                atp.skyShaderFloat.Add(new BasicStringFloatList(arr[0], arrfloat));
                            }
                            
                        }
                        string[] ssc = lst[7].Split('%');
                        foreach (string item in ssc)
                        {
                            string[] arr = item.Split('=');
                            if (arr.Length > 1)
                            {
                                Color col = ColorUtility.TryParseHtmlString(arr[1], out col) ? col : Color.white;
                                atp.skyShaderColor.Add(new BasicStringColorList(arr[0], col));
                            }
                            
                        }
                        atp.skyShaderName = lst[8];
                    }
                    else if (movetype == "systemlightprop")
                    {
                        float[] vec3 = TryParseFloatArray(lst, CSV_BEGINVAL, valueCount);

                        atp.animationType = AF_MOVETYPE.LightProperty;
                        atp.rotation = new Vector3(vec3[0], vec3[1], vec3[2]);
                        atp.range = vec3[3];
                        atp.color = new Color(vec3[4], vec3[5], vec3[6], vec3[7]);
                        atp.power = vec3[8];
                        atp.shadowStrength = vec3[9];
                        if (vec3.Length > 10)
                        {
                            atp.halo = (int)vec3[10];
                            atp.flareType = (int)vec3[11];
                            atp.flareColor = new Color(vec3[12], vec3[13], vec3[14], vec3[15]);
                            atp.flareBrightness = vec3[16];
                            atp.flareFade = vec3[17];
                        }
                    }
                }
            }


            return atp;
        }


        /// <summary>
        /// Convert "translateMovingData" AnimationTranslateTargetParts to CSV-format (Translate only)
        /// </summary>
        /// <param name="targetType"></param>
        /// <param name="attp"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string TranslateDataToCSV(AF_TARGETTYPE targetType, AnimationTranslateTargetParts attp, Vector3 value)
        {
            
            List<string> ret = new List<string>();

            if (attp.animationType == AF_MOVETYPE.Translate)
            {
                ret.Add(((int)attp.vrmBone).ToString());
                if ((targetType == AF_TARGETTYPE.VRM) && (attp.vrmBone == ParseIKBoneType.UseHumanBodyBones))
                {
                    //---real HumanBodyBones information
                    //ret.Add(((int)attp.vrmHumanBodyBone).ToString());
                }
                else
                {
                    //---If VRM, IK information
                    ret.Add("");
                }

                ret.Add("position");
                ret.Add("5");
                ret.Add(value.x.ToString());
                ret.Add(value.y.ToString());
                ret.Add(value.z.ToString());
                ret.Add(attp.jumpNum.ToString());
                ret.Add(attp.jumpPower.ToString());

            }
            return string.Join(",", ret);
        }
        /// <summary>
        /// Convert "movingData" AnimationTargetParts to CSV-format (Rotate/Scale/etc...)
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
                 object IK type (VRM)bone type move type float paramater count  float paramaters
                 char           string          string        integer               float
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
                if (atp.vrmBone == ParseIKBoneType.Aim)
                {
                    ret.Add("rotation:v5");
                    ret.Add("4");
                    ret.Add(atp.rotation.x.ToString());
                    ret.Add(atp.rotation.y.ToString());
                    ret.Add(atp.rotation.z.ToString());
                    ret.Add(atp.isRotate360.ToString());
                }
                else
                {
                    ret.Add("rotation");
                    ret.Add("4");
                    ret.Add(atp.rotation.x.ToString());
                    ret.Add(atp.rotation.y.ToString());
                    ret.Add(atp.rotation.z.ToString());
                    ret.Add(atp.isRotate360.ToString());
                }
                
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
            else if (atp.animationType == AF_MOVETYPE.Rigid)
            { //---version >= 5
                ret.Add(((int)atp.vrmBone).ToString());
                ret.Add("");
                ret.Add("rigid");
                ret.Add("4");
                ret.Add(atp.rigidDrag.ToString());
                ret.Add(atp.rigidAngularDrag.ToString());
                ret.Add(atp.useCollision.ToString());
                ret.Add(atp.useRigidGravity.ToString());
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

                        ret.Add("7");
                        // preset hand pose (4 ~ 5)
                        atp.handpose.ForEach(hand =>
                        {
                            ret.Add(hand.ToString());
                        });
                        

                        //--- manually finger pose (6 ~ 10)
                        ret.Add(HandPoseController.StringifyFinger(atp.fingerpose, "t"));
                        ret.Add(HandPoseController.StringifyFinger(atp.fingerpose, "i"));
                        ret.Add(HandPoseController.StringifyFinger(atp.fingerpose, "m"));
                        ret.Add(HandPoseController.StringifyFinger(atp.fingerpose, "r"));
                        ret.Add(HandPoseController.StringifyFinger(atp.fingerpose, "l"));

                    }
                    else if (atp.animationType == AF_MOVETYPE.BlendShape)
                    {
                        ret.Add(((int)atp.vrmBone).ToString());
                        ret.Add("");
                        ret.Add("blendshape");
                        ret.Add(atp.blendshapes.Count.ToString());
                        //---registered key ONLY
                        for (int i = 0; i < atp.blendshapes.Count; i++)
                        {
                            //ret.Add(i.ToString() + "=" + atp.blendshapes[i].value);
                            //---version >= 4
                            ret.Add(atp.blendshapes[i].text + "=" + atp.blendshapes[i].value);
                        }
                    }
                    else if (atp.animationType == AF_MOVETYPE.VRMAutoBlendShape)
                    {
                        ret.Add(((int)atp.vrmBone).ToString());
                        ret.Add("");
                        ret.Add("autoblendshape");
                        ret.Add("4");
                        ret.Add(atp.isblink.ToString());
                        ret.Add(atp.interval.ToString());
                        ret.Add(atp.openingSeconds.ToString());
                        ret.Add(atp.closeSeconds.ToString());
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
                        ret.Add(((int)atp.vrmBone).ToString());
                        ret.Add("");
                        ret.Add("equipment");
                        ret.Add(atp.equipDestinations.Count.ToString());
                        atp.equipDestinations.ForEach(equip =>
                        { //version >= 4: add equip.equipflag.ToString()
                            ret.Add(((int)equip.bodybonename).ToString() + CST_SEPSTR_PROP + equip.equipitem + CST_SEPSTR_PROP + equip.equipflag.ToString());
                        });

                    }
                    else if (atp.animationType == AF_MOVETYPE.GravityProperty)
                    {
                        ret.Add(((int)atp.vrmBone).ToString());
                        ret.Add("");
                        ret.Add("gravity");
                        ret.Add(atp.gravity.list.Count.ToString());
                        for (int i = 0; i < atp.gravity.list.Count; i++)
                        {
                            VRMGravityInfo gra = atp.gravity.list[i];
                            //---version <= 5
                            //ret.Add(i.ToString() + CST_SEPSTR_PROP + gra.power + CST_SEPSTR_PROP + gra.dir.x.ToString() + CST_SEPSTR_PROP + gra.dir.y.ToString() + CST_SEPSTR_PROP + gra.dir.z.ToString());
                            //---version >= 6
                            ret.Add(
                                gra.comment + "/" + gra.rootBoneName + CST_SEPSTR_PROP + 
                                gra.power + CST_SEPSTR_PROP + 
                                gra.dir.x.ToString() + CST_SEPSTR_PROP + 
                                gra.dir.y.ToString() + CST_SEPSTR_PROP + 
                                gra.dir.z.ToString()
                            );
                        }
                    }
                    else if (atp.animationType == AF_MOVETYPE.VRMIKProperty)
                    {
                        ret.Add(((int)atp.vrmBone).ToString());
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
                        ret.Add(((int)atp.vrmBone).ToString());
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
                    else if (atp.animationType == AF_MOVETYPE.AnimStart)
                    {
                        ret.Add(((int)atp.vrmBone).ToString());
                        ret.Add(atp.text + CST_SEPSTR_PROP + atp.animName);
                        switch (atp.animPlaying)
                        {
                            case UserAnimationState.Play:
                                ret.Add("animstart");
                                //ret.Add("1");
                                //ret.Add(atp.animSeek.ToString());
                                break;
                            case UserAnimationState.Playing:
                                ret.Add("rest");
                                //ret.Add("0");
                                break;
                            case UserAnimationState.Pause:
                                ret.Add("animpause");
                                //ret.Add("0");
                                break;
                            case UserAnimationState.Stop:
                                ret.Add("animstop");
                                //ret.Add("0");
                                break;
                            case UserAnimationState.Seeking:
                                ret.Add("animseek");
                                //ret.Add("1");
                                //ret.Add(atp.animSeek.ToString());
                                break;
                            default:
                                break;
                        }
                        ret.Add("3");
                        ret.Add(atp.animSeek.ToString());
                        ret.Add(atp.animSpeed.ToString());
                        ret.Add(atp.animLoop.ToString());
                    }
                    else if (atp.animationType == AF_MOVETYPE.AnimStop)
                    {
                        ret.Add(((int)atp.vrmBone).ToString());
                        ret.Add("");
                        ret.Add("vrmaanimstop");
                        ret.Add("0");
                    }
                    else if (atp.animationType == AF_MOVETYPE.AnimProperty)
                    {
                        ret.Add(((int)atp.vrmBone).ToString());
                        ret.Add(atp.animName);
                        ret.Add("animprop");
                        ret.Add("6");
                        //ret.Add(atp.animPlaying.ToString());
                        ret.Add(atp.animSpeed.ToString());
                        
                        ret.Add(atp.animLoop.ToString());

                    }
                }
                else if (targetType == AF_TARGETTYPE.OtherObject)
                {
                    ret.Add(((int)atp.vrmBone).ToString());
                    ret.Add(atp.animName);
                    if (atp.animationType == AF_MOVETYPE.AnimStart)
                    {
                        switch (atp.animPlaying)
                        {
                            case UserAnimationState.Play:
                                ret.Add("animstart");
                                //ret.Add("3");
                                //ret.Add(atp.animSeek.ToString());
                                //ret.Add(atp.animSpeed.ToString());
                                //ret.Add(atp.animLoop.ToString());
                                break;
                            case UserAnimationState.Playing:
                                ret.Add("rest");
                                //ret.Add("0");
                                break;
                            case UserAnimationState.Pause:
                                ret.Add("animpause");
                                //ret.Add("0");
                                break;
                            case UserAnimationState.Stop:
                                ret.Add("animstop");
                                //ret.Add("0");
                                break;
                            case UserAnimationState.Seeking:
                                ret.Add("animseek");
                                //ret.Add("1");
                                //ret.Add(atp.animSeek.ToString());
                                break;
                            default:
                                break;
                        }
                        ret.Add("3");
                        ret.Add(atp.animSeek.ToString());
                        ret.Add(atp.animSpeed.ToString());
                        ret.Add(atp.animLoop.ToString());

                    }
                    /*
                    else if (atp.animationType == AF_MOVETYPE.AnimStop)
                    {
                        
                    }
                    else if (atp.animationType == AF_MOVETYPE.AnimSeek)
                    {
                        
                    }
                    else if (atp.animationType == AF_MOVETYPE.AnimPause)
                    {
                        
                    }
                    else if (atp.animationType == AF_MOVETYPE.Rest)
                    {
                        
                    }*/
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
                    ret.Add("17");
                    ret.Add(atp.range.ToString());
                    ret.Add(atp.color.r.ToString());
                    ret.Add(atp.color.g.ToString());
                    ret.Add(atp.color.b.ToString());
                    ret.Add(atp.color.a.ToString());
                    ret.Add(atp.power.ToString());
                    ret.Add(atp.spotAngle.ToString());
                    ret.Add(((int)atp.lightRenderMode).ToString());
                    ret.Add(((int)atp.lightType).ToString());
                    ret.Add(atp.halo.ToString());
                    ret.Add(atp.flareType.ToString());
                    ret.Add(atp.flareColor.r.ToString());
                    ret.Add(atp.flareColor.g.ToString());
                    ret.Add(atp.flareColor.b.ToString());
                    ret.Add(atp.flareColor.a.ToString());
                    ret.Add(atp.flareBrightness.ToString());
                    ret.Add(atp.flareFade.ToString());

                }
                else if (targetType == AF_TARGETTYPE.Camera)
                {
                    ret.Add(((int)atp.vrmBone).ToString());
                    ret.Add("");
                    if (atp.animationType == AF_MOVETYPE.Camera)
                    {
                        if (atp.animPlaying == UserAnimationState.Play)
                        {
                            ret.Add("cameraon");
                            ret.Add("1");
                            ret.Add(((int)atp.animPlaying).ToString());
                        }
                        else if (atp.animPlaying == UserAnimationState.Playing)
                        {
                            ret.Add("camera");
                            ret.Add("1");
                            ret.Add(((int)atp.animPlaying).ToString());
                        }
                        else if (atp.animPlaying == UserAnimationState.Stop)
                        {
                            ret.Add("cameraoff");
                            ret.Add("1");
                            ret.Add(((int)atp.animPlaying).ToString());
                        }
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
                else if ((targetType == AF_TARGETTYPE.Text) || (targetType == AF_TARGETTYPE.Text3D))
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

                        ret.Add("textprop:v5");
                        ret.Add("5");
                        ret.Add(atp.color.r.ToString());
                        ret.Add(atp.color.g.ToString());
                        ret.Add(atp.color.b.ToString());
                        ret.Add(atp.color.a.ToString());
                        ret.Add(atp.fontSize.ToString());

                        //---version >= 5
                        //ret.Add(atp.fontStyle.ToString());
                        ret.Add(atp.fontStyles); //4+5+1 = 10

                        //---4 + 5 + 2~n (11~n)
                        ret.Add(atp.textAlignmentOptions);
                        ret.Add(atp.textOverflow.ToString());
                        ret.Add(atp.dimension);
                        ret.Add(atp.IsGradient ? "1" : "0");
                        ret.Add(
                            "#" + ColorUtility.ToHtmlStringRGBA(atp.gradients[0]) + CST_SEPSTR_ITEM +
                            "#" + ColorUtility.ToHtmlStringRGBA(atp.gradients[1]) + CST_SEPSTR_ITEM +
                            "#" + ColorUtility.ToHtmlStringRGBA(atp.gradients[2]) + CST_SEPSTR_ITEM +
                            "#" + ColorUtility.ToHtmlStringRGBA(atp.gradients[3])
                        );
                        ret.Add(atp.fontOutlineWidth.ToString());
                        ret.Add("#" + ColorUtility.ToHtmlStringRGBA(atp.fontOutlineColor));
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
                        if (atp.animPlaying == UserAnimationState.Play)
                        {
                            ret.Add("animstart");
                            ret.Add("2");
                            ret.Add(atp.isSE.ToString());
                            ret.Add(atp.seekTime.ToString());
                        }
                        else if (atp.animPlaying == UserAnimationState.Pause)
                        {
                            ret.Add("animpause");
                            ret.Add("2");
                            ret.Add(atp.isSE.ToString());
                            ret.Add(atp.seekTime.ToString());
                        }
                        else if (atp.animPlaying == UserAnimationState.Stop)
                        {
                            ret.Add("animstop");
                            ret.Add("2");
                            ret.Add(atp.isSE.ToString());
                            ret.Add(atp.seekTime.ToString());
                        }
                        else if (atp.animPlaying == UserAnimationState.Seeking)
                        {
                            ret.Add("animseek");
                            ret.Add("2");
                            ret.Add(atp.isSE.ToString());
                            ret.Add(atp.seekTime.ToString());
                        }
                        else if (atp.animPlaying == UserAnimationState.Playing)
                        {
                            ret.Add("rest");
                            ret.Add("1");
                            ret.Add(atp.isSE.ToString());
                            ret.Add(atp.seekTime.ToString());
                        }
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
                        if (atp.animPlaying == UserAnimationState.Play)
                        {
                            ret.Add("animstart");
                            ret.Add("1");
                            ret.Add(atp.animLoop.ToString());
                        }
                        else if (atp.animPlaying == UserAnimationState.Pause)
                        {
                            ret.Add("animpause");
                            ret.Add("0");
                        }
                        else if (atp.animPlaying == UserAnimationState.Stop)
                        {
                            ret.Add("animstop");
                            ret.Add("0");
                        }
                        else if (atp.animPlaying == UserAnimationState.Playing)
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
                        if (atp.animPlaying == UserAnimationState.Play)
                        {
                            ret.Add("systemeffect");
                        }
                        else
                        {
                            ret.Add("systemeffectoff");
                        }
                        
                        ret.Add(atp.effectValues.Count.ToString());
                        atp.effectValues.ForEach(v =>
                        {
                            ret.Add(v.ToString());
                        });
                    }
                    /*else if (atp.animationType == AF_MOVETYPE.SystemEffectOff)
                    {
                        ret.Add(atp.effectName);
                        ret.Add("systemeffectoff");
                        ret.Add("0");
                    }*/
                }
                else if (targetType == AF_TARGETTYPE.Stage)
                {
                    ret.Add(((int)atp.vrmBone).ToString());
                    if (atp.animationType == AF_MOVETYPE.Stage)
                    {
                        ret.Add("");
                        ret.Add("stage");
                        ret.Add("1");
                        ret.Add(atp.stageType.ToString());
                    }
                    else if (atp.animationType == AF_MOVETYPE.StageProperty)
                    {
                        string allCount = "";
                        string optParts = "";
                        if ((atp.matProp.Count > 0) && (atp.stageType == (int)StageKind.User))
                        {
                            allCount = "16";
                            optParts = atp.matProp[0].texturePath + "\t" + atp.matProp[1].texturePath;
                        }
                        else
                        {
                            allCount = "5";
                        }
                        ret.Add(optParts);
                        ret.Add("stageprop");
                        ret.Add(allCount);
                        //0 ~ 4  (real: 4~ )
                        ret.Add(atp.stageType.ToString());
                        ret.Add(atp.windPower.ToString());
                        ret.Add(atp.windFrequency.ToString());
                        ret.Add(atp.windDurationMin.ToString());
                        ret.Add(atp.windDurationMax.ToString());

                        if (atp.stageType == (int)StageKind.User)
                        {
                            if (atp.matProp.Count > 0)
                            {
                                MaterialProperties mat0 = atp.matProp[0];
                                MaterialProperties mat1 = atp.matProp[1];
                                //5 ~ 15
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
                        else if ((atp.stageType == (int)StageKind.BasicSeaLevel) || (atp.stageType == (int)StageKind.SeaDaytime) || (atp.stageType == (int)StageKind.SeaNight)) 
                        {
                            List<string> lst = new List<string>();
                            string water4ln = SerializeMaterial(atp.vmatProp);
                            //5 (real: 9~ )
                            lst.Add(water4ln);

                            ret.Add(string.Join(CST_SEPSTR_ITEM,lst));
                        }


                        // --- セットの順番を全部見直し 2022.07.10

                        //ret.Add(atp.wavespeed.x.ToString());
                        //ret.Add(atp.wavespeed.y.ToString());
                        //ret.Add(atp.wavespeed.z.ToString());
                        //ret.Add(atp.wavespeed.w.ToString());
                        //ret.Add(atp.wavescale.ToString());


                        
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
                        ret.Add("18");
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
                        ret.Add(atp.halo.ToString());
                        ret.Add(atp.flareType.ToString());
                        ret.Add(atp.flareColor.r.ToString());
                        ret.Add(atp.flareColor.g.ToString());
                        ret.Add(atp.flareColor.b.ToString());
                        ret.Add(atp.flareColor.a.ToString());
                        ret.Add(atp.flareBrightness.ToString());
                        ret.Add(atp.flareFade.ToString());

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
            string ret = "";
            //try
            {
                //---destroy current project
                for (int i = currentProject.casts.Count - 1; i >= 0; i--)
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
                    else if (cast.type == AF_TARGETTYPE.SystemEffect)
                    {
                        cast.avatar.GetComponent<ManageSystemEffect>().SetDefault();
                    }
                    else if (cast.type == AF_TARGETTYPE.Stage)
                    {
                        cast.avatar.GetComponent<OperateStage>().SetDefault();
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
                //Debug.Log("New Project:1:" + currentProject.casts.Count.ToString());
                currentProject.casts.Clear();
                /*foreach (NativeAnimationFrameActor actor in currentProject.timeline.characters)
                {
                    actor.frames.Clear();
                }
                currentProject.timeline.characters.Clear();
                */

                //---clear materials in the project
                if ((currentProject != null) && (currentProject.materialManager != null))
                {
                    currentProject.materialManager.Dispose();
                }

                //Debug.Log("New Project:2:" + currentProject.materialManager.ToString());


                currentProject = null;

                //---new project
                currentProject = new NativeAnimationProject(initialFrameCount);

                //[Edit point] 2022.09.xx
                currentProject.version = PROJECT_VERSION;

                //Debug.Log("New Project:3:");
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

                //Debug.Log("New Project:4:");
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
            /*catch (Exception e)
            {
                Debug.Log(e.Message);
            }*/
            Debug.Log("prepare mkey");
            ret = "mkey," + currentProject.mkey.ToString();
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
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
        public async void LoadProject(string param)
        {
            //---reset current project (do not merge)
            //NewProject();

            AnimationProject proj = JsonUtility.FromJson<AnimationProject>(param);
            currentProject = await ConvertProjectNative(proj);

            //---after settings
            //SetFps(currentProject.fps);


            AnimationProject retproj = Body_SaveProject(currentProject);
            string ret = JsonUtility.ToJson(retproj);
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif

        }
        private async System.Threading.Tasks.Task<NativeAnimationProject> ConvertProjectNative(AnimationProject proj)
        {
            OpennigAnimationProject oapro = new OpennigAnimationProject();

            NativeAnimationProject nproj = new NativeAnimationProject(initialFrameCount);

            IsExternalProject = proj.isSharing;

            nproj.version = proj.version;
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
            /*for (var i = 0; i < proj.casts.Count; i++)
            {
                AnimationAvatar naa = proj.casts[i];
                if ( //---leaving type: VRM, OtherObject, Light, Camera, Image, UImage, Text, Effect
                    (naa.type == AF_TARGETTYPE.VRM) || (naa.type == AF_TARGETTYPE.OtherObject) ||
                    (naa.type == AF_TARGETTYPE.Image) || (naa.type == AF_TARGETTYPE.UImage)
                )
                {
                    if (naa.type == AF_TARGETTYPE.OtherObject)
                    {
                        if (naa.path != "%BLANK%")
                        {
                            fullVal++;
                        }
                    }
                    else
                    {
                        fullVal++;
                    }
                        
                }
            }*/

#if !UNITY_EDITOR && UNITY_WEBGL
            IntervalLoadingProject(0);
#endif
            float percentv = 100f / (float)proj.casts.Count;
            float cur = 0;
            float curval = 0;
            //---load from the file
            foreach (AnimationAvatar avatar in proj.casts) 
            {
                
                //---normal use input
                nproj.casts.Add(await ParseEffectiveAvatar(avatar));
                cur++;
                curval = percentv * cur;
#if !UNITY_EDITOR && UNITY_WEBGL
            //IntervalLoadingProject(curval);
#endif
            }

            //---set up each frames of timelines.
            //---re-save frame actor and old timeline in current project
            currentProject.timeline.characters.ForEach(chara =>
            {
                if ( //---leaving type: VRM, OtherObject, Light, Camera, Image, UImage, Text, Effect
                    (chara.targetType == AF_TARGETTYPE.VRM) || (chara.targetType == AF_TARGETTYPE.OtherObject) ||
                    (chara.targetType == AF_TARGETTYPE.Light) || (chara.targetType == AF_TARGETTYPE.Camera) ||
                    (chara.targetType == AF_TARGETTYPE.Image) || (chara.targetType == AF_TARGETTYPE.UImage) ||
                    (chara.targetType == AF_TARGETTYPE.Text) || (chara.targetType == AF_TARGETTYPE.Effect) ||
                    (chara.targetType == AF_TARGETTYPE.Text3D)
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
                    NativeAnimationFrame nframe = ParseEffectiveFrame(nact, fr, nproj.version);
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
        private async System.Threading.Tasks.Task<NativeAnimationAvatar> ParseEffectiveAvatar(AnimationAvatar avatar)
        {
            NativeAnimationAvatar nav = new NativeAnimationAvatar(); // GetTargetObjects(avatar.avatarId, avatar.type);

            nav.roleName = avatar.roleName;
            nav.roleTitle = avatar.roleTitle;
            nav.type = avatar.type;
            nav.avatarId = avatar.avatarId;
            nav.path = avatar.path;
            nav.ext = avatar.ext;
            nav.avatarTitle = avatar.avatarTitle;
            Array.Copy(avatar.bodyHeight, nav.bodyHeight, avatar.bodyHeight.Length);

            //---Search by avatar ID, link successfully.
            if (avatar.avatarId != "")
            { 
                NativeAnimationAvatar tmp = GetEffectiveAvatarObjects(avatar.avatarId, avatar.type);
                if (tmp != null)
                {
                    nav.avatar = tmp.avatar;
                    nav.ikparent = tmp.ikparent;
                }
            }
            //---Search by role title
            NativeAnimationAvatar hitav = GetCastByNameInProject(avatar.roleTitle);
            if ((hitav != null) && 
                ((avatar.type != AF_TARGETTYPE.SystemEffect) && (avatar.type != AF_TARGETTYPE.Audio) && (avatar.type != AF_TARGETTYPE.Stage))
            )
            {
                NativeAnimationFrameActor nact = GetFrameActorFromRole(hitav.roleName, hitav.type);
                if (nact != null)
                {
                    //---if existed avatar don't has key-frame data, overwrite to JSON-based data.
                    if (nact.frames.Count == 0)
                    { 
                        DetachAvatarFromRole(avatar.roleName + "," + "role");
                        nav.avatar = hitav.avatar;
                        nav.ikparent = hitav.ikparent;
                        DeleteAvatarFromCast(avatar.roleName + "," + ((int)avatar.type).ToString());
                    }
                }
            }

            //---finally, Open and recover effective object.
            OpeningNativeAnimationAvatar oap = await GenerateEachTypeObject(nav);

            if ((oap != null) && (oap.cast != null))
            {
                //---from newly loaded avatar at this time 
                nav.avatar = oap.cast.avatar;
                nav.ikparent = oap.cast.ikparent;
                nav.avatarId = oap.cast.avatarId;
                nav.avatarTitle = oap.cast.avatarTitle;

            }

            return nav;
        }
        
        /// <summary>
        /// Open and recover the effective object, finally link role with avatar.
        /// </summary>
        /// <param name="nav"></param>
        private async System.Threading.Tasks.Task<OpeningNativeAnimationAvatar> GenerateEachTypeObject(NativeAnimationAvatar nav)
        {
            FileMenuCommands fmc = GetComponent<FileMenuCommands>();
            OpeningNativeAnimationAvatar ret = null;

            switch (nav.type)
            {
                case AF_TARGETTYPE.VRM:
                    if (nav.path == "")
                    {
                        ret = new OpeningNativeAnimationAvatar();
                        ret.cast = new NativeAnimationAvatar();
                        ret.baseInfo = new BasicObjectInformation();
                        ret.cast.type = nav.type;
                        ret.cast.avatarId = "";
                        ret.cast.avatar = null;
                        ret.cast.ikparent = null;
                        ret.baseInfo.id = nav.avatarId;
                        ret.baseInfo.roleName = nav.roleName;
                        ret.baseInfo.roleTitle = nav.roleTitle;
                        ret.baseInfo.type = Enum.GetName(typeof(AF_TARGETTYPE), nav.type);
                    }
                    else
                    {
                        Debug.Log(nav.avatarId + "/" + nav.path);
                        //ret = fmc.LoadVRM(nav.path);
                        //ret = fmc.AcceptLoadVRMUnity();
                        
                    }
                    
                    break;
                case AF_TARGETTYPE.OtherObject:
                    if (nav.path == "%BLANK%")
                    {
                        int ptype = int.TryParse(nav.ext, out ptype) ? ptype : 0;
                        ret = fmc.Body_CreateBlankCube((UserPrimitiveType)ptype);
                        ret.cast.path = nav.path;
                    }
                    else
                    {
                        if (nav.path == "")
                        {
                            ret = new OpeningNativeAnimationAvatar();
                            ret.cast = new NativeAnimationAvatar();
                            ret.baseInfo = new BasicObjectInformation();
                            ret.cast.type = nav.type;
                            ret.cast.avatarId = "";
                            ret.cast.avatar = null;
                            ret.cast.ikparent = null;
                            ret.baseInfo.id = nav.avatarId;
                            ret.baseInfo.roleName = nav.roleName;
                            ret.baseInfo.roleTitle = nav.roleTitle;
                            ret.baseInfo.type = Enum.GetName(typeof(AF_TARGETTYPE), nav.type);
                        }
                        else
                        {
                            //ret = fmc.LoadOtherObject(nav.path);
                        }
                        
                    }
                    
                    break;
                case AF_TARGETTYPE.Light:
                    ret = fmc.Body_OpenLightObject(nav.ext == "" ? "spot" : nav.ext);
                    break;
                case AF_TARGETTYPE.Camera:
                    ret = fmc.Body_CreateCameraObject("");
                    break;
                case AF_TARGETTYPE.Image:
                    if (nav.path == "")
                    {
                        ret = new OpeningNativeAnimationAvatar();
                        ret.cast = new NativeAnimationAvatar();
                        ret.baseInfo = new BasicObjectInformation();
                        ret.cast.type = nav.type;
                        ret.cast.avatarId = "";
                        ret.cast.avatar = null;
                        ret.cast.ikparent = null;
                        ret.baseInfo.id = nav.avatarId;
                        ret.baseInfo.roleName = nav.roleName;
                        ret.baseInfo.roleTitle = nav.roleTitle;
                        ret.baseInfo.type = Enum.GetName(typeof(AF_TARGETTYPE), nav.type);
                    }
                    else
                    {
                        //ret = fmc.LoadImageFile(nav.path);
                    }
                    
                    break;
                case AF_TARGETTYPE.UImage:
                    if (nav.path == "")
                    {
                        ret = new OpeningNativeAnimationAvatar();
                        ret.cast = new NativeAnimationAvatar();
                        ret.baseInfo = new BasicObjectInformation();
                        ret.cast.type = nav.type;
                        ret.cast.avatarId = "";
                        ret.cast.avatar = null;
                        ret.cast.ikparent = null;
                        ret.baseInfo.id = nav.avatarId;
                        ret.baseInfo.roleName = nav.roleName;
                        ret.baseInfo.roleTitle = nav.roleTitle;
                        ret.baseInfo.type = Enum.GetName(typeof(AF_TARGETTYPE), nav.type);
                    }
                    else
                    {
                        //ret = fmc.LoadUImageFile(nav.path);
                    }
                    
                    break;
                case AF_TARGETTYPE.Text:
                    //ret = await fmc.Body_OpenText("ABC,tl,2");
                    ret = await fmc.OpenRecoveryText("ABC,tl,2");
                    break;
                case AF_TARGETTYPE.Effect:
                    ret = fmc.Body_CreateSingleEffect();
                    break;
                case AF_TARGETTYPE.Text3D:
                    ret = await fmc.OpenRecoveryText("ABC,tl,3");
                    break;

            }
            
            return ret;
        }
        /// <summary>
        /// Convert AnimationFrame to NativeAnimationFrame
        /// </summary>
        /// <param name="frame"></param>
        /// <returns></returns>
        public NativeAnimationFrame ParseEffectiveFrame(NativeAnimationFrameActor actor, AnimationFrame frame, int file_version = 0)
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
            naframe.memo = frame.memo;

            //---for Translate only
            for (int i = 0; i < (int)ParseIKBoneType.Unknown; i++)
            {
                if (IsVRMParseBoneType((ParseIKBoneType)i))
                {
                    string pikt = i.ToString();
                    List<string> translateLst = frame.movingData.FindAll(match =>
                    {
                        string[] lst = match.Split(',');
                        if (lst.Length > 2)
                        {
                            if (
                                (lst[0] == pikt) &&  //---ParseIKBoneType
                                (lst[2] == "position") //---raw string for AF_MOVETYPE
                            )
                            {
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return false;
                        }
                    });
                    if (translateLst.Count > 0)
                    {
                        AnimationTranslateTargetParts attp = new AnimationTranslateTargetParts((ParseIKBoneType)i, AF_MOVETYPE.Translate);

                        foreach (string line in translateLst)
                        {
                            attp = CsvToFrameTranslateData(actor, actor.targetType, line, attp);
                        }
                        naframe.translateMovingData.Add(attp);
                    }
                    if (actor.avatar.type != AF_TARGETTYPE.VRM)
                    { //---VRM other than is IKParent only.
                        break;
                    }
                }
                
            }
            

            //---for Rotate/Scale/etc...
            foreach (string line in frame.movingData)
            {
                AnimationTargetParts atp = new AnimationTargetParts();
                if (line != null)
                {
                    try
                    {
                        //naframe.movingData.Add(line);
                        naframe.movingData.Add(CsvToFrameData(actor, actor.targetType, line, atp, file_version));
                        //Memo: position do not generate. "position" change to "xxxposition".
                    }
                    catch (Exception err)
                    {
                        Debug.LogWarning("Found an error:\n" + err.Message.ToString() + "\n" + line);
                    }
                    
                }
            }
            if (file_version < 3) //---version < 3 
            {
                naframe = ConvertAimBipedIK2VVMIK(actor, naframe);
            }
            

            return naframe;
        }

        /// <summary>
        /// Convert Aim of BipedIK (translate) to VVMIK's one (rotate).
        /// </summary>
        /// <param name="naframe"></param>
        /// <returns></returns>
        public NativeAnimationFrame ConvertAimBipedIK2VVMIK(NativeAnimationFrameActor nactor, NativeAnimationFrame naframe)
        {
            for (int i = 0; i < naframe.movingData.Count; i++)
            {
                var attp = naframe.movingData[i];
                if (attp.vrmBone == ParseIKBoneType.LeftShoulder)
                {
                    attp.rotation.y = MathF.Abs(attp.rotation.y) - 180f;
                }
                else if (attp.vrmBone == ParseIKBoneType.RightShoulder)
                {
                    attp.rotation.y = MathF.Abs(attp.rotation.y) - 180f;
                }
                else if (attp.vrmBone == ParseIKBoneType.Head)
                {
                    attp.rotation.y = MathF.Abs(attp.rotation.y) - 180f;
                }
                else if (attp.vrmBone == ParseIKBoneType.Chest)
                {
                    if (Mathf.Abs(attp.rotation.y) > 175f)  {
                        attp.rotation.y = MathF.Abs(attp.rotation.y) - 180f;
                    }
                }
                else if (attp.vrmBone == ParseIKBoneType.Aim)
                {
                    if (MathF.Abs(attp.rotation.y) > 175f)
                    {
                        attp.rotation.y = MathF.Abs(attp.rotation.y) - 180f;
                    }
                }
            }
            /*
            List<AnimationTranslateTargetParts> attps = naframe.translateMovingData.FindAll(match =>
            {
                if (match.vrmBone == ParseIKBoneType.Aim) return true;
                return false;
            });
            int aimtranIndex = naframe.movingData.FindIndex(match =>
            {
                if (match.vrmBone == ParseIKBoneType.Aim) return true;
                return false;
            });

            if (attps.Count > 0)
            {
                GameObject aimobj = nactor.avatar.ikparent.transform.Find("Aim").gameObject;
                if ((aimobj != null) && (aimtranIndex > -1))
                {
                    Transform spineTran = nactor.avatar.avatar.transform.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Spine);
                    Vector3 actorDirection = spineTran.forward;

                    attps.ForEach(atp =>
                    {
                        Vector3 markerDirection = atp.values[0];
                        Vector3 delta = markerDirection - actorDirection;
                        naframe.movingData[aimtranIndex].rotation = Quaternion.FromToRotation(actorDirection, delta).eulerAngles;
                    });
                }
                
            }

            */
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
        public void LoadSingleMotionDirect(string data)
        {
            string ret = LoadSingleMotion_body(data);
#if !UNITY_EDITOR && UNITY_WEBGL
                    ReceiveStringVal(ret);
#endif
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
                NativeAnimationAvatar nav = GetCastInProject(SingleMotionTargetRole.roleName, SingleMotionTargetRole.type);

                if (naf != null)
                {
                    if (naf.targetType != asm.targetType) return "typeerr";

                    naf.targetType = asm.targetType;
                    naf.compiled = asm.compiled;
                    naf.avatar = SingleMotionTargetRole;

                    naf.blendShapeList.Clear();
                    /* ---This blendShapeList is saved VRM's shapes. 
                     * blendShapeList(hoge, foo, fuga)
                     * 0=100 ... hoge=100
                     * Here, NOT Current NativeAnimationAvatar's blendShapes(Expression) of NativeAnimationFrameActor.
                     * When compiling an animation, check blendshape(Expression) as current NativeAnimationAvatar
                     */
                    asm.blendShapeList.ForEach(item =>
                    {
                        naf.blendShapeList.Add(item);
                    });
                    
                    //---sample avatar height --> frame actor height
                    Array.Copy(asm.bodyHeight, naf.bodyHeight, asm.bodyHeight.Length);



                    naf.bodyInfoList.Clear();
                    /*asm.bodyInfoList.ForEach(item =>
                    {
                        naf.bodyInfoList.Add(new Vector3(item.x, item.y, item.z));
                    });*/
                    int IKBoneCnt = (int)ParseIKBoneType.Unknown;
                    for (int bi = 0; bi < IKBoneCnt; bi++)
                    {
                        if (IsVRMParseBoneType((ParseIKBoneType)bi))
                        {
                            if (bi < asm.bodyInfoList.Count) //bi < IKBoneCnt and bodyInfoList.Count
                            {
                                Vector3 item = asm.bodyInfoList[bi];
                                naf.bodyInfoList.Add(new Vector3(item.x, item.y, item.z));
                            }
                        }
                        
                        
                    }


                    naf.frames.Clear();

                    if (naf.avatar.type == AF_TARGETTYPE.VRM)
                    {
                        var vvmrec = naf.avatar.avatar.GetComponent<VVMMotionRecorder>();
                        vvmrec.ClearAllFrames4VRMA();
                        vvmrec.ClearCurves();
                    }

                    //---For returning HTML
                    AnimationFrameActor afa = new AnimationFrameActor();
                    afa.SetFromNative(naf);

                    foreach (AnimationSingleFrame fr in asm.frames)
                    {
                        if (fr.index <= currentProject.timelineFrameLength)
                        {
                            NativeAnimationFrame naframe = new NativeAnimationFrame();
                            naframe.index = fr.index;
                            naframe.finalizeIndex = fr.finalizeIndex;
                            naframe.duration = fr.duration;
                            naframe.key = fr.key;
                            naframe.ease = fr.ease;
                            naframe.memo = fr.memo;
                            naframe.useBodyInfo = UseBodyInfoType.TimelineCharacter;

                            AnimationFrame aframe = new AnimationFrame();
                            aframe.SetFromNative(naframe);

                            //---for Translate only
                            for (int i = 0; i < (int)ParseIKBoneType.Unknown; i++)
                            {
                                if (IsVRMParseBoneType((ParseIKBoneType)i))
                                {
                                    string pikt = i.ToString();
                                    List<string> translateLst = fr.movingData.FindAll(match =>
                                    {
                                        string[] lst = match.Split(',');
                                        if (lst.Length > 0)
                                        {
                                            if (
                                                (lst[0] == pikt) &&  //---ParseIKBoneType
                                                (lst[2] == "position") //---raw string for AF_MOVETYPE
                                            )
                                            {
                                                return true;
                                            }
                                            else
                                            {
                                                return false;
                                            }
                                        }
                                        else
                                        {
                                            return false;
                                        }
                                    });
                                    if (translateLst.Count > 0)
                                    {
                                        AnimationTranslateTargetParts attp = new AnimationTranslateTargetParts((ParseIKBoneType)i, AF_MOVETYPE.Translate);
                                        foreach (string line in translateLst)
                                        {
                                            attp = CsvToFrameTranslateData(naf, asm.targetType, line, attp);
                                        }
                                        naframe.translateMovingData.Add(attp);
                                    }
                                    if (naf.avatar.type != AF_TARGETTYPE.VRM)
                                    { //---VRM other than is IKParent only.
                                        break;
                                    }
                                }
                                
                            }

                            //---for Rotate/Scale/etc...
                            foreach (string linedata in fr.movingData)
                            {
                                if ((linedata != null) && (linedata != ""))
                                {
                                    AnimationTargetParts atp = new AnimationTargetParts();
                                    atp = CsvToFrameData(naf, asm.targetType, linedata, atp, asm.version);
                                    naframe.movingData.Add(atp);

                                    //---For returning HTML
                                    aframe.movingData.Add(linedata);
                                }
                                
                            }

                            naf.frames.Add(naframe);
                            if (asm.version < 3) //---version < 3
                            {
                                naframe = ConvertAimBipedIK2VVMIK(naf, naframe);
                            }

                            afa.frames.Add(aframe);
                        }   
                    }
                    

                    //---apply height difference with absorb to this avatar (VRM only)
                    CalculateAllFrameForCurrent(nav, naf);
                    //------update also the height of frame actor (NECCESARY): this avatar height --> frame actor height ( 1:1 )
                    Array.Copy(nav.bodyHeight, naf.bodyHeight, nav.bodyHeight.Length);

                    ret = JsonUtility.ToJson(afa);

                }
            }
            return ret;
        }
//===========================================================================================================================
//  Save functions
//===========================================================================================================================

        public AnimationProject Body_SaveProject(NativeAnimationProject napro)
        {
            AnimationProject aniproj = new AnimationProject(initialFrameCount);
            aniproj.version = PROJECT_VERSION;
            aniproj.isSharing = napro.isSharing;
            aniproj.isReadOnly = napro.isReadOnly;
            aniproj.isNew = false;
            aniproj.isOpenAndEdit = napro.isOpenAndEdit;
            aniproj.mkey = DateTime.Now.ToFileTime();
            aniproj.baseDuration = napro.baseDuration;
            aniproj.timelineFrameLength = napro.timelineFrameLength;
            aniproj.meta = napro.meta.SCopy();
            aniproj.materialManager.SetFromNative(napro.materialManager);
            //---avatar
            napro.casts.ForEach(avatar =>
            {
                AnimationAvatar aa = new AnimationAvatar();

                aa.SetFromNative(avatar);
                aniproj.casts.Add(aa);
            });

            //---timeline characters
            foreach (NativeAnimationFrameActor actor in napro.timeline.characters)
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
                    afg.memo = frame.memo;
                    //---translate to csv
                    foreach (AnimationTranslateTargetParts tmv in frame.translateMovingData)
                    {
                        foreach (Vector3 vv in tmv.values)
                        {
                            string ln = TranslateDataToCSV(rawactor.targetType, tmv, vv);
                            if (ln != "")
                            {
                                afg.movingData.Add(ln);
                            }
                            
                        }
                        
                    }
                    //---OTHER THAN translate to csv
                    foreach (AnimationTargetParts mv in frame.movingData)
                    {
                        string ln = DataToCSV(rawactor.targetType, mv);
                        if (ln != "")
                        {
                            afg.movingData.Add(ln);
                        }
                        
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

            return aniproj;
        }
        /// <summary>
        /// To save animation project data.
        /// </summary>
        /// <returns>JSON format of AnimationProject</returns>
        public string SaveProject()
        {
            string ret = "";

            AnimationProject aniproj = Body_SaveProject(currentProject);

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

            asm.version = PROJECT_VERSION;
            asm.targetType = type;
            asm.compiled = currentPlayingOptions.isCompileAnimation;

            NativeAnimationFrameActor naf = GetFrameActorFromRole(roleName, type);

            if (naf != null)
            {
                asm.blendShapeList = naf.blendShapeList;
                //---AnimationAvatar -> AnimationSingleMotion: bodyHeight information. (as current avatar height info!!)
                Array.Copy(naf.avatar.bodyHeight, asm.bodyHeight, naf.avatar.bodyHeight.Length);
                asm.bodyInfoList = naf.bodyInfoList;

                asm.gravityBoneList = naf.gravityBoneList;

                List<Vector3> curList = naf.avatar.avatar.GetComponent<OperateLoadedVRM>().GetTPoseBodyList();

                foreach (NativeAnimationFrame frame in naf.frames)
                {
                    AnimationSingleFrame asf = new AnimationSingleFrame();
                    asf.duration = frame.duration;
                    asf.finalizeIndex = frame.finalizeIndex;
                    asf.index = frame.index;
                    asf.key = frame.key;

                    //---translate to csv
                    foreach (AnimationTranslateTargetParts tmv in frame.translateMovingData)
                    {
                        foreach (Vector3 vv in tmv.values)
                        {
                            string ln = TranslateDataToCSV(type, tmv, vv);
                            if (ln != "")
                            {
                                asf.movingData.Add(ln);
                            }
                            
                        }

                    }
                    //---absorb height distance, apply changes.
                    
                    {
                        foreach (AnimationTargetParts mv in frame.movingData)
                        {
                            string ln = DataToCSV(type, mv);
                            if (ln != "")
                            {
                                asf.movingData.Add(ln);
                            }
                            
                        }
                        asm.frames.Add(asf);
                    }
                    
                }

                ret = JsonUtility.ToJson(asm);

            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
            return ret;
        }

        public void SaveBvhData(string param)
        {
            string ret = "";
            string[] prm = param.Split(',');
            string roleName = prm[0];
            int tmp = int.TryParse(prm[1], out tmp) ? tmp : 99;
            AF_TARGETTYPE type = (AF_TARGETTYPE)tmp;
            string jsontype = prm[2];


            NativeAnimationFrameActor naf = GetFrameActorFromRole(roleName, type);
            if (naf != null)
            {
                ManageAvatarTransform mat = naf.avatar.avatar.GetComponent<ManageAvatarTransform>();
                if (mat != null)
                {
                    ret = mat.TextOutputBVH();
                    
                }
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ReceiveStringVal(ret);
#endif
        }
    }

}
