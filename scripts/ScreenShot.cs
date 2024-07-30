using System;
using System.IO;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UserHandleSpace;
using UserUISpace;

public class ScreenShot : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void sendCapture(byte[] rawimg, int size);

    public Camera Cam2DTop;
    public Camera Cam3DBottom;
    public Camera ThumbnailCamera;
    public int ThumbnailWidth;
    public int ThumbnailHeight;

    public void SetCameraForScreenshot(string param)
    {
        GameObject camobj = GameObject.Find(param);
        if (camobj != null)
        {
            Camera cam = camobj.GetComponent<Camera>();
            if (cam != null)
            {
                Cam3DBottom = cam;
            }
        }
    }
    public void CaptureScreen(int isTransparent)
    {
        StartCoroutine(_CaptureScreenBody(isTransparent));
    }
    IEnumerator _CaptureScreenBody(int isTransparent)
    {
        yield return new WaitForEndOfFrame();

        GameObject stageman = GameObject.FindGameObjectWithTag("GroundWorld");
        float save_a = 0f;
        Color save_bkcolor = new Color(Cam3DBottom.backgroundColor.r, Cam3DBottom.backgroundColor.g, Cam3DBottom.backgroundColor.b, Cam3DBottom.backgroundColor.a); ;
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);

        if (isTransparent == 1)
        {

            save_a = Cam3DBottom.backgroundColor.a;
            Cam3DBottom.backgroundColor = Color.clear; // new Color(Cam2DTop.backgroundColor.r, Cam2DTop.backgroundColor.g, Cam2DTop.backgroundColor.b, 0f);
            //stageman.GetComponent<OperateStage>().ActiveStage.SetActive(false);
            LayerCullingHide(Cam3DBottom, "Stage");
        }
        RenderTexture rt = new RenderTexture(screenShot.width, screenShot.height, 32);
        RenderTexture prev = Cam3DBottom.targetTexture;
        RenderTexture prev2d = Cam2DTop.targetTexture;

        Canvas msgarea = GameObject.Find("MsgArea").GetComponent<Canvas>();
        msgarea.renderMode = RenderMode.ScreenSpaceCamera;

        
        Cam3DBottom.targetTexture = rt;
        Cam3DBottom.Render();
        Cam3DBottom.targetTexture = prev;

        Cam2DTop.targetTexture = rt;
        Cam2DTop.Render();
        Cam2DTop.targetTexture = prev2d;

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);
        screenShot.Apply();

        byte[] bytes = screenShot.EncodeToPNG();
        UnityEngine.Object.Destroy(screenShot);

#if UNITY_EDITOR
        string fileName = "cap_" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png";

        Debug.Log(Application.persistentDataPath + "/" + fileName);
        File.WriteAllBytes(Application.persistentDataPath + "/" + fileName, bytes);
#endif
#if !UNITY_EDITOR && UNITY_WEBGL
        sendCapture(bytes, bytes.Length);
#endif
        if (isTransparent == 1)
        {
            Cam3DBottom.backgroundColor = new Color(save_bkcolor.r, save_bkcolor.g, save_bkcolor.b, save_bkcolor.a);
            //stageman.GetComponent<OperateStage>().ActiveStage.SetActive(true);
            LayerCullingShow(Cam3DBottom, "Stage");
        }
        msgarea.renderMode = RenderMode.ScreenSpaceOverlay;

    }
    public void FreeCaptureScreen(Camera cam, UserUIARWin HideUI)
    {
        StartCoroutine(_FreeCaptureScreenBody(true, cam, HideUI));
    }
    IEnumerator _FreeCaptureScreenBody(bool isTransparent, Camera cam, UserUIARWin HideUI)
    {
        HideUI.ShowUI(false);

        yield return new WaitForEndOfFrame();

        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGBA32, false);

        RenderTexture rt = new RenderTexture(screenShot.width, screenShot.height, 32);
        RenderTexture prev = cam.targetTexture;

        cam.targetTexture = rt;
        cam.Render();
        cam.targetTexture = prev;

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);
        screenShot.Apply();

        byte[] bytes = screenShot.EncodeToPNG();
        UnityEngine.Object.Destroy(screenShot);

#if !UNITY_EDITOR && UNITY_WEBGL
        sendCapture(bytes, bytes.Length);
#endif
        yield return new WaitForEndOfFrame();

        HideUI.ShowUI(true);

    }
    public byte[] CaptureThumbnail(int isTransparent, GameObject targetAvatar)
    {
        //StartCoroutine(_CapturePrepareThumbnail(targetAvatar));

        return _CaptureThumbnailBody(isTransparent,targetAvatar);
        
    }
    public void PrepareCaptureThumbnail(GameObject targetAvatar)
    {
        //---Move in front of that avatar
        OperateLoadedVRM ovrm = targetAvatar.GetComponent<OperateLoadedVRM>();
        Animator conanime = targetAvatar.GetComponent<Animator>();
        Transform avatarCenter = conanime.GetBoneTransform(HumanBodyBones.Hips);

        ThumbnailCamera.transform.position = new Vector3(
            ovrm.relatedHandleParent.transform.position.x, avatarCenter.position.y, ovrm.relatedHandleParent.transform.position.z - 2
        );
        //yield return null;
    }
    private byte[] _CaptureThumbnailBody(int isTransparent, GameObject targetAvatar)
    {
        //---Move in front of that avatar
        
        OperateLoadedVRM ovrm = targetAvatar.GetComponent<OperateLoadedVRM>();

        //Animator conanime = targetAvatar.GetComponent<Animator>();
        //Transform avatarCenter = conanime.GetBoneTransform(HumanBodyBones.Hips);
        ManageAvatarTransform mat = targetAvatar.GetComponent<ManageAvatarTransform>();
        GameObject bodyobj = mat.GetBodyMesh();
        Bounds bnd = bodyobj.GetComponent<SkinnedMeshRenderer>().bounds;
        
        Vector3 newpos = new Vector3(
            ovrm.relatedHandleParent.transform.position.x, ovrm.relatedHandleParent.transform.position.y + (bnd.size.y / 2f), ovrm.relatedHandleParent.transform.position.z - 2
        );

        ThumbnailCamera.transform.position = newpos;
        

        //---Perform caputure
        GameObject stageman = GameObject.FindGameObjectWithTag("GroundWorld");
        float save_a = 0f;
        Texture2D screenShot = new Texture2D(ThumbnailWidth, ThumbnailHeight, TextureFormat.RGBA32, false);

        if (isTransparent == 1)
        {

            save_a = ThumbnailCamera.backgroundColor.a;
            ThumbnailCamera.backgroundColor = Color.clear;
            //stageman.GetComponent<OperateStage>().ActiveStage.SetActive(false);
            LayerCullingHide(ThumbnailCamera, "Stage");
        }
        RenderTexture rt = new RenderTexture(screenShot.width, screenShot.height, 32);
        RenderTexture prev = ThumbnailCamera.targetTexture;
        ThumbnailCamera.targetTexture = rt;
        ThumbnailCamera.Render();
        ThumbnailCamera.targetTexture = prev;
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);
        screenShot.Apply();

        byte[] bytes = screenShot.EncodeToPNG();
        UnityEngine.Object.Destroy(screenShot);

        if (isTransparent == 1)
        {
            ThumbnailCamera.backgroundColor = new Color(ThumbnailCamera.backgroundColor.r, ThumbnailCamera.backgroundColor.g, ThumbnailCamera.backgroundColor.b, save_a);
            //stageman.GetComponent<OperateStage>().ActiveStage.SetActive(true);
            LayerCullingShow(ThumbnailCamera, "Stage");
        }

        return bytes;
        
    }

    //********************************************************************************
    //---Camera extension
    public static void LayerCullingShow(Camera cam, int layerMask)
    {
        cam.cullingMask |= layerMask;
    }

    public static void LayerCullingShow(Camera cam, string layer)
    {
        LayerCullingShow(cam, 1 << LayerMask.NameToLayer(layer));
    }

    public static void LayerCullingHide(Camera cam, int layerMask)
    {
        cam.cullingMask &= ~layerMask;
    }

    public static void LayerCullingHide(Camera cam, string layer)
    {
        LayerCullingHide(cam, 1 << LayerMask.NameToLayer(layer));
    }

    public static void LayerCullingToggle(Camera cam, int layerMask)
    {
        cam.cullingMask ^= layerMask;
    }

    public static void LayerCullingToggle(Camera cam, string layer)
    {
        LayerCullingToggle(cam, 1 << LayerMask.NameToLayer(layer));
    }

    public static bool LayerCullingIncludes(Camera cam, int layerMask)
    {
        return (cam.cullingMask & layerMask) > 0;
    }

    public static bool LayerCullingIncludes(Camera cam, string layer)
    {
        return LayerCullingIncludes(cam, 1 << LayerMask.NameToLayer(layer));
    }

    public static void LayerCullingToggle(Camera cam, int layerMask, bool isOn)
    {
        bool included = LayerCullingIncludes(cam, layerMask);
        if (isOn && !included)
        {
            LayerCullingShow(cam, layerMask);
        }
        else if (!isOn && included)
        {
            LayerCullingHide(cam, layerMask);
        }
    }

    public static void LayerCullingToggle(Camera cam, string layer, bool isOn)
    {
        LayerCullingToggle(cam, 1 << LayerMask.NameToLayer(layer), isOn);
    }

}
