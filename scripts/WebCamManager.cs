using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebCamManager : MonoBehaviour
{
    private int camwidth;
    private int camheight;
    private int fps;

    WebCamTexture wcTexture;

    // Start is called before the first frame update
    void Start()
    {
        camwidth = 256;
        camheight = 256;
        fps = 30;
        wcTexture = new WebCamTexture(camwidth, camheight, fps);

        Renderer rd = GetComponent<Renderer>();
        rd.material.mainTexture = wcTexture;
        //wcTexture.Play();
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
}
