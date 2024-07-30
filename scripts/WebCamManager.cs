using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class WebCamManager : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ReceiveStringVal(string val);

    [DllImport("__Internal")]
    private static extern void ReceiveIntVal(int val);
    [DllImport("__Internal")]
    private static extern void ReceiveFloatVal(float val);

    private int camwidth;
    private int camheight;
    private int fps;

    WebCamDevice[] wcDevices;
    WebCamTexture wcTexture;

    // Start is called before the first frame update
    void Start()
    {
        camwidth = 256;
        camheight = 256;
        fps = 30;

        wcDevices = WebCamTexture.devices;
        wcTexture = new WebCamTexture(wcDevices[0].name, camwidth, camheight, fps);

        Renderer rd = GetComponent<Renderer>();
        rd.material.mainTexture = wcTexture;
        wcTexture.Play();
    }
    // Update is called once per frame

    void Update()
    {
        
    }
    public void SetupCamera(int width, int height, int pfps)
    {
        camwidth = width;
        camheight = height;
        fps = pfps;
    }
    public void ChangeCamera(string name, bool withplay = false)
    {
        StopCamera();
        Destroy(wcTexture);
        wcTexture = null;
        wcTexture = new WebCamTexture(name, camwidth, camheight, fps);
        if (withplay) PlayCamera();
    }
    public void ApplyCamera()
    {
        wcTexture.Stop();
        Destroy(wcTexture);
        wcTexture = null;

        wcTexture = new WebCamTexture(camwidth, camheight, fps);
    }
    public void PlayCamera()
    {
        wcTexture.Play();
    }
    public void PauseCamera()
    {
        wcTexture.Pause();
    }
    public void StopCamera()
    {
        wcTexture.Stop();
    }
    public WebCamTexture GetCameraTexture()
    {
        return wcTexture;
    }
    public List<string> GetDevices()
    {
        List<string> lst = new List<string>();
        for (int i = 0; i < wcDevices.Length; i++)
        {
            lst.Add(wcDevices[i].name);
        }
        return lst;
    }
    public void GetDevicesFromOuter()
    {
        List<string> lst = GetDevices();
        string ret = string.Join(',', lst);
#if !UNITY_EDITOR && UNITY_WEBGL
        ReceiveStringVal(ret);
#endif
    }
}
