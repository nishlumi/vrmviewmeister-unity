using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserHandleSpace;
using DG.Tweening;

using System.IO;
using TriLibCore;
using TriLibCore.General;
using TriLibCore.SFB;

public class ownscr_OpeTimeline : MonoBehaviour
{

    public GameObject AnimateArea;
    public GameObject ViewArea;
    public GameObject IKArea;

    private bool IsOpenTimelinePanel;

    // Start is called before the first frame update
    void Start()
    {
        IsOpenTimelinePanel = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnClickBtnTLClose()
    {
        IsOpenTimelinePanel = !IsOpenTimelinePanel;
        if (IsOpenTimelinePanel)
        {
            GameObject.Find("OpeTimeline").transform.GetComponent<RectTransform>().DOAnchorPos(new Vector2(38f, 5f), 0.3f);
        }
        else
        {
            GameObject.Find("OpeTimeline").transform.GetComponent<RectTransform>().DOAnchorPos(new Vector2(38f, -155f), 0.3f);
        }
    }

    public void OnChangeSliderTL(float value)
    {
        float v = GameObject.Find("sliderTL").GetComponent<Slider>().value;
        v = Mathf.Round(v);
        GameObject.Find("inputFrameTL").GetComponent<InputField>().text = v.ToString();

        ManageAnimation ma = AnimateArea.GetComponent<ManageAnimation>();
        OperateActiveVRM oavrm = IKArea.GetComponent<OperateActiveVRM>();

        int opti = int.TryParse(GameObject.Find("inputOptDOTween").GetComponent<InputField>().text, out opti) ? opti : 0;

        NativeAnimationAvatar cast = ma.GetCastByAvatar(oavrm.GetEffectiveActiveAvatar().name);
        if (cast == null) return;
        ma.currentProject.casts.ForEach(cast =>
        {
            AnimationParsingOptions aro = new AnimationParsingOptions();

            aro.index = (int)v;
            aro.targetId = ""; // (oavrm.GetEffectiveActiveAvatar() != null) ? oavrm.GetEffectiveActiveAvatar().name : "";
            aro.targetRole = cast.roleName;
            aro.targetType = cast.type; //oavrm.ActiveType;
            aro.isExecuteForDOTween = opti;
            ma.PreviewSingleFrame(JsonUtility.ToJson(aro));
        });
        ma.BackupPreviewMarker();
        ma.FinishPreviewMarker();
    }
    public void OnChangeInputFrameTL()
    {
        float v = 0;
        GameObject.Find("sliderTL").GetComponent<Slider>().value = float.TryParse(GameObject.Find("inputFrameTL").GetComponent<InputField>().text, out v) ? v : 1f;

        ManageAnimation ma = AnimateArea.GetComponent<ManageAnimation>();
        OperateActiveVRM oavrm = IKArea.GetComponent<OperateActiveVRM>();

        int opti = int.TryParse(GameObject.Find("inputOptDOTween").GetComponent<InputField>().text, out opti) ? opti : 0;

        ma.PreparePreviewMarker();
        ma.currentProject.casts.ForEach(cast =>
        {
            AnimationParsingOptions aro = new AnimationParsingOptions();

            aro.index = (int)v;
            aro.targetId = ""; // (oavrm.GetEffectiveActiveAvatar() != null) ? oavrm.GetEffectiveActiveAvatar().name : "";
            aro.targetRole = cast.roleName;
            aro.targetType = cast.type; //oavrm.ActiveType;
            aro.isExecuteForDOTween = opti;
            ma.PreviewSingleFrame(JsonUtility.ToJson(aro));
        });
        ma.BackupPreviewMarker();
        ma.FinishPreviewMarker();

    }
    public void OnClickBtnRegTL()
    {
        ManageAnimation ma = AnimateArea.GetComponent<ManageAnimation>();
        OperateActiveVRM oavrm = IKArea.GetComponent<OperateActiveVRM>();

        AnimationRegisterOptions aro = new AnimationRegisterOptions();
        int index = 0;
        aro.index = int.TryParse(GameObject.Find("inputFrameTL").GetComponent<InputField>().text, out index) ? index : 1;
        aro.targetId = oavrm.GetEffectiveActiveAvatar().name;
        aro.targetType = oavrm.ActiveType;
        aro.isCompileForLibrary = 0;

        ma.RegisterFrame(JsonUtility.ToJson(aro));
    }
    public void OnClickBtnSRegTL()
    {
        ManageAnimation ma = AnimateArea.GetComponent<ManageAnimation>();
        OperateActiveVRM oavrm = IKArea.GetComponent<OperateActiveVRM>();

        AnimationRegisterOptions aro = new AnimationRegisterOptions();
        int index = 0;
        aro.index = int.TryParse(GameObject.Find("inputFrameTL").GetComponent<InputField>().text, out index) ? index : 1;
        aro.targetId = ""; // "AnimateArea";
        //aro.targetType = AF_TARGETTYPE.SystemEffect;
        aro.isCompileForLibrary = 0;
        //aro.isTransformOnly = 1;

        ma.RegisterFrame(JsonUtility.ToJson(aro));
    }
    public void OnClickBtnUnRegTL()
    {
        ManageAnimation ma = AnimateArea.GetComponent<ManageAnimation>();
        OperateActiveVRM oavrm = IKArea.GetComponent<OperateActiveVRM>();

        AnimationRegisterOptions aro = new AnimationRegisterOptions();
        int index = 0;
        aro.index = int.TryParse(GameObject.Find("inputFrameTL").GetComponent<InputField>().text, out index) ? index : 1;
        aro.targetId = oavrm.GetEffectiveActiveAvatar().name;
        aro.targetType = oavrm.ActiveType;

        ma.UnregisterFrame(JsonUtility.ToJson(aro));
    }
    public void OnClickBtnPlayTL()
    {
        int opti = int.TryParse(GameObject.Find("inputOptDOTween").GetComponent<InputField>().text, out opti) ? opti : 0;
        int optcomp = GameObject.Find("Tgl_compile").GetComponent<Toggle>().isOn ? 1 : 0;
        int optloop = GameObject.Find("Tgl_animloop").GetComponent<Toggle>().isOn ? 1 : 0;

        AnimationParsingOptions apo = new AnimationParsingOptions();
        apo.isExecuteForDOTween = opti;
        apo.isBuildDoTween = 1;
        apo.isCompileAnimation = optcomp;
        apo.isLoop = optloop;
        apo.endDelay = 1.5f;

        ManageAnimation ma = AnimateArea.GetComponent<ManageAnimation>();
        string opt = JsonUtility.ToJson(apo);
        ma.StartAllTimeline(opt);
    }
    public void OnClickBtnStopTL()
    {
        ManageAnimation ma = AnimateArea.GetComponent<ManageAnimation>();
        ma.StopAllTimeline();
    }
    public void OnClickBtnPauseTL()
    {
        ManageAnimation ma = AnimateArea.GetComponent<ManageAnimation>();
        ma.PauseAllTimeline();
    }
    public void OnClickNewProj()
    {
        GameObject manim = GameObject.Find("AnimateArea");
        ManageAnimation maa = manim.GetComponent<ManageAnimation>();
        maa.NewProject();
    }
    public void OnClickOpenProj()
    {
        GameObject manim = GameObject.Find("AnimateArea");
        ManageAnimation maa = manim.GetComponent<ManageAnimation>();


        //------
        string[] textext = new string[3] { "txt", "json", "vvmproj" };
        ExtensionFilter[] ext = new ExtensionFilter[] {
            new ExtensionFilter("All File", textext),
            new ExtensionFilter("text File", "txt"),
            new ExtensionFilter("json File", "json"),
            new ExtensionFilter("project file", "vvmproj")
        };

        IList<ItemWithStream> paths = StandaloneFileBrowser.OpenFilePanel("データファイルを選んで下さい。", "", ext, false);
        string ret = "";
        for (int i = 0; i < paths.Count; i++)
        {
            Stream stm = paths[i].OpenStream();
            using (var fs = new StreamReader(stm))
            {
                ret = fs.ReadToEnd();

                maa.LoadProject(ret);
                GameObject slid = GameObject.Find("sliderTL");
                if (slid != null)
                {
                    Slider ss = slid.GetComponent<Slider>();
                    ss.maxValue = maa.currentProject.timelineFrameLength;
                }
            }
        }
    }
    public void OnClickSaveProj()
    {
        GameObject manim = GameObject.Find("AnimateArea");
        ManageAnimation maa = manim.GetComponent<ManageAnimation>();

        string data = maa.SaveProject(); ;

        string[] textext = new string[3] { "txt", "json", "vvmproj" };
        ExtensionFilter[] ext = new ExtensionFilter[] {
            new ExtensionFilter("All File", textext),
            new ExtensionFilter("text File", "txt"),
            new ExtensionFilter("json File", "json"),
            new ExtensionFilter("project file", "vvmproj")
        };
        ItemWithStream istream = StandaloneFileBrowser.SaveFilePanel("app", Application.persistentDataPath, "pose.json", ext);
        //Stream stm = istream.OpenStream();
        if (istream.Name != "")
        {
            FileStream fs = new FileStream(istream.Name, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(data);
            sw.Close();
            fs.Close();
        }
    }
    public void OnClickBtnOpenRoleEdit()
    {
        GameObject dlg = gameObject.transform.parent.Find("DlgRoleEdit").gameObject;

        dlg.SetActive(true);
        dlg.GetComponent<RectTransform>().DOScale(new Vector3(1f, 1f, 1f), 0.2f);
        Camera.main.gameObject.GetComponent<RuntimeGizmos.TransformGizmo>().enabled = false;

        OnClickBtnTLClose();
    }
    public void OnClickSavePose()
    {
        GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        ManageAvatarTransform mat = ovrm.GetEffectiveActiveAvatar().GetComponent<ManageAvatarTransform>();

        string data = mat.BackupAvatarTransform("vrm");
        mat.PoseSaveList.Add(data);

        string[] textext = new string[2] { "txt", "json" };
        ExtensionFilter[] ext = new ExtensionFilter[] {
            new ExtensionFilter("Pose File", textext),
            new ExtensionFilter("text File", "txt"),
            new ExtensionFilter("json File", "json"),
        };
        ItemWithStream istream =  StandaloneFileBrowser.SaveFilePanel("app", Application.persistentDataPath, "pose.json", ext);
        //Stream stm = istream.OpenStream();
        if (istream.Name != "")
        {
            FileStream fs = new FileStream(istream.Name, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(data);
            sw.Close();
            fs.Close();
        }
        
    }
    public void OnClickOpenPose()
    {
        /*
        string txt = GameObject.Find("InpPoseList").GetComponent<InputField>().text;
        int inx = int.TryParse(txt, out inx) ? inx : 0;
        */
        GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        ManageAvatarTransform mat = ovrm.GetEffectiveActiveAvatar().GetComponent<ManageAvatarTransform>();
        /*if (mat.PoseSaveList.Count > 0)
        {
            mat.AnimateAvatarTransform(mat.PoseSaveList[inx]);
        }*/


        //------
        string[] textext = new string[2] { "txt", "json" };
        ExtensionFilter[] ext = new ExtensionFilter[] {
            new ExtensionFilter("Pose File", textext),
            new ExtensionFilter("text File", "txt"),
            new ExtensionFilter("json File", "json"),
        };
        
        IList<ItemWithStream> paths = StandaloneFileBrowser.OpenFilePanel("データファイルを選んで下さい。", "", ext, false);
        string ret = "";
        for (int i = 0; i < paths.Count; i++)
        {
            Stream stm = paths[i].OpenStream();
            using (var fs = new StreamReader(stm))
            {
                ret = fs.ReadToEnd();
            }
        }
        mat.AnimateAvatarTransform(ret);
    }

    public void OnClickOpenMotion()
    {
        GameObject manim = GameObject.Find("AnimateArea");
        ManageAnimation maa = manim.GetComponent<ManageAnimation>();

        GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        //------
        string[] textext = new string[2] { "txt", "json" };
        ExtensionFilter[] ext = new ExtensionFilter[] {
            new ExtensionFilter("Pose File", textext),
            new ExtensionFilter("text File", "txt"),
            new ExtensionFilter("json File", "json"),
        };

        IList<ItemWithStream> paths = StandaloneFileBrowser.OpenFilePanel("データファイルを選んで下さい。", "", ext, false);
        string ret = "";
        for (int i = 0; i < paths.Count; i++)
        {
            Stream stm = paths[i].OpenStream();
            using (var fs = new StreamReader(stm))
            {
                ret = fs.ReadToEnd();
            }
        }

        NativeAnimationFrameActor naf = maa.GetFrameActorFromObjectID(ovrm.GetEffectiveActiveAvatar().name, AF_TARGETTYPE.VRM);

        maa.SetLoadTargetSingleMotion(naf.targetRole);
        maa.LoadSingleMotion_body(ret);
    }
    public void OnClickSaveMotion()
    {
        GameObject manim = GameObject.Find("AnimateArea");
        ManageAnimation maa = manim.GetComponent<ManageAnimation>();

        GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        NativeAnimationFrameActor naf =  maa.GetFrameActorFromObjectID(ovrm.GetEffectiveActiveAvatar().name, AF_TARGETTYPE.VRM);
        string data = maa.SaveSingleMotion(naf.targetRole + "," + ((int)naf.targetType).ToString());

        string[] textext = new string[2] { "txt", "json" };
        ExtensionFilter[] ext = new ExtensionFilter[] {
            new ExtensionFilter("Pose File", textext),
            new ExtensionFilter("text File", "txt"),
            new ExtensionFilter("json File", "json"),
        };
        ItemWithStream istream = StandaloneFileBrowser.SaveFilePanel("app", Application.persistentDataPath, "mot.json", ext);
        //Stream stm = istream.OpenStream();
        if (istream.Name != "")
        {
            FileStream fs = new FileStream(istream.Name, FileMode.Create);
            StreamWriter sw = new StreamWriter(fs);
            sw.Write(data);
            sw.Close();
            fs.Close();
        }

    }
    public void BtnFrameCopy_OnClck()
    {
        GameObject manim = GameObject.Find("AnimateArea");
        ManageAnimation maa = manim.GetComponent<ManageAnimation>();
        OperateActiveVRM oavrm = IKArea.GetComponent<OperateActiveVRM>();

        AnimationRegisterOptions aro = new AnimationRegisterOptions();
        int index = 0;
        index = int.TryParse(GameObject.Find("inputFrameTL").GetComponent<InputField>().text, out index) ? index : 1;
        string[] roles = maa.GetRoleSpecifiedAvatar(oavrm.GetEffectiveActiveAvatar().name);
        aro.targetType = oavrm.ActiveType;
        

        maa.CopyFrame(roles[0] + "," + ((int)aro.targetType).ToString() + "," + index.ToString() + ",0");

    }
    public void BtnFrameCut_OnClick()
    {
        GameObject manim = GameObject.Find("AnimateArea");
        ManageAnimation maa = manim.GetComponent<ManageAnimation>();
        OperateActiveVRM oavrm = IKArea.GetComponent<OperateActiveVRM>();

        AnimationRegisterOptions aro = new AnimationRegisterOptions();
        int index = 0;
        index = int.TryParse(GameObject.Find("inputFrameTL").GetComponent<InputField>().text, out index) ? index : 1;
        string[] roles = maa.GetRoleSpecifiedAvatar(oavrm.GetEffectiveActiveAvatar().name);
        aro.targetType = oavrm.ActiveType;


        maa.CopyFrame(roles[0] + "," + ((int)aro.targetType).ToString() + "," + index.ToString() + ",0");
    }
    public void BtnFramePaste_OnClick()
    {
        GameObject manim = GameObject.Find("AnimateArea");
        ManageAnimation maa = manim.GetComponent<ManageAnimation>();
        OperateActiveVRM oavrm = IKArea.GetComponent<OperateActiveVRM>();

        AnimationRegisterOptions aro = new AnimationRegisterOptions();
        int index = 0;
        index = int.TryParse(GameObject.Find("inputFrameTL").GetComponent<InputField>().text, out index) ? index : 1;
        string[] roles = maa.GetRoleSpecifiedAvatar(oavrm.GetEffectiveActiveAvatar().name);
        aro.targetType = oavrm.ActiveType;


        maa.PasteFrame(roles[0] + "," + ((int)aro.targetType).ToString() + "," + index.ToString());
    }
}
