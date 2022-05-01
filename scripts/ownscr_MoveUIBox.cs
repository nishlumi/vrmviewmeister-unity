using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserHandleSpace;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using TriLibCore;
using TriLibCore.General;
using TriLibCore.SFB;

public class ownscr_MoveUIBox : MonoBehaviour
{
    public GameObject infoObject;
    public float objectTop;
    public float objectLeft;
    private bool showInfo;
    private Vector3 infoPos;

    private bool IsOpenFilePanel;
    private bool IsOpenTimelinePanel;
    private bool IsOpenMovePanel;

    private Vector3 curMousePos;
    private Vector3 curMouseTranslation;

    // Start is called before the first frame update
    void Start()
    {
        showInfo = true;
        IsOpenFilePanel = false;
        IsOpenTimelinePanel = false;
        IsOpenMovePanel = false;

        curMousePos = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnPointerClick()
    {
        Camera.main.GetComponent<ScreenShot>().CaptureScreen(1);
        return;

        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        //ovrm.ResetParentHandlePosition();
        //ovrm.ResetParentHandleRotation();

        //ovrm.SetPositionFromOuter("0.15,0,0,1");

        //ovrm.SetScale("0.5,0.5,0.5");
        //ovrm.ActivateAvatarFromOuter("ダイドー - アビス・ホライズン");
        GameObject inp = GameObject.Find("InputField");
        InputField inpf = inp.GetComponent<InputField>();
        ovrm.PosingHandFromOuter(inpf.text);
        //int kao = ovrm.getAvatarBlendShapeIndex("Face.M_F00_000_00_Fcl_ALL_Joy");
        //ovrm.changeAvatarBlendShape(kao + ",100");
        //ovrm.ShowAvatar360(10f);

        /*
        GameObject target = GameObject.Find("basevroid01");
        Animator animator = target.GetComponent<Animator>();
        Camera.main.transform.DOLookAt(animator.GetBoneTransform(HumanBodyBones.Head).position, 0.5f);
        Camera.main.GetComponent<CameraOperation1>().targetObject.transform.DOLocalMove(target.transform.localPosition, 0.5f);
        */

        GameObject can = GameObject.Find("Canvas");
        /*ConfigSettingLabs cs = can.GetComponent<ConfigSettingLabs>();
        */
        //cs.ChangeGroundColor();
        //can.GetComponent<UserVRMSpace.FileMenuCommands>().OnShowedOtherObject();

        //can.GetComponent<UserVRMSpace.FileMenuCommands>().DestroyVRM("");

        /*
        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
        List<string> lst = ovrm.ListPresetFace();
        for (int i = 0; i < lst.Count; i++)
        {
            Debug.Log(lst[i]);
        }
        Debug.Log(ovrm.getAvatarPresetFace("Joy"));
        */

        //ovrm.changeAvatarPresetFace("Angry,1.0");
        /*RectTransform rpos = infoObject.GetComponent<RectTransform>();
        showInfo = !showInfo;
        showUIObject(showInfo);*/
        //ovrm.ResetAllHandle();
    }
    public void OnTst1PointerClick()
    {
        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        GameObject inp = GameObject.Find("InputField");
        InputField inpf = inp.GetComponent<InputField>();
        


        OperateLoadedVRM oload = ovrm.ActiveAvatar.GetComponent<OperateLoadedVRM>();
        //ovrm.ActiveAvatar.GetComponent<ManageAvatarTransform>().AnimateAvatarTransform(inpf.text);

        //GameObject.Find("AnimateArea").GetComponent<ManageAnimation>().LoadAnimation(openTextfile());

        //oload.EquipObject(HumanBodyBones.RightHand, GameObject.Find(inpf.text));
        //oload.EquipObjectFromOuter(inpf.text);
        GameObject tex = ikhp.GetComponent<OperateLoadedObj>().CreateText("ほげぇtest","bl");

    }
    
    public string openTextfile() 
    {  //---TEST CODE======
        string ret = "";
#if UNITY_EDITOR
        ExtensionFilter[] ext = new ExtensionFilter[] {
                new ExtensionFilter("text File", "txt"),
                new ExtensionFilter("json File", "json"),
            };
        //string[] paths = StandaloneFileBrowser.OpenFilePanel("VRMファイルを選んで下さい。", "", ext, false);
        /*if (paths.Length > 0)
        {
            LoadVRMURI(paths[0]);
        }*/
        IList<ItemWithStream> paths = StandaloneFileBrowser.OpenFilePanel("データファイルを選んで下さい。", "", ext, false);
        for (int i = 0; i < paths.Count; i++)
        {
            Stream stm = paths[i].OpenStream();
            using (var fs = new StreamReader(stm))
            {
                ret = fs.ReadToEnd();
            }
        }
#endif
        return ret;
    }

    public void OnTst2PointerClick()
    {
        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        //ovrm.SetScale("0.5,0.5,0.5");

        GameObject inp = GameObject.Find("InputField");
        InputField inpf = inp.GetComponent<InputField>();


        //OperateLoadedVRM oload;
        //inpf.text = ovrm.ActiveAvatar.GetComponent<ManageAvatarTransform>().BackupAvatarTransform("VRM");

        //inpf.text = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>().SaveAnimation(ovrm.ActiveAvatar.name + ",VRM");
        /*
        if (ovrm.ActiveAvatar.TryGetComponent<OperateLoadedVRM>(out oload))
        {
            oload.UnequipObject(HumanBodyBones.RightHand, inpf.text);
        }
        */
        
    }
    public void OnAct1PointerClick()
    {
        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
        ManageAnimation manim = GameObject.Find("AnimateArea").GetComponent<ManageAnimation>();

        //ovrm.SetScale("0.5,0.5,0.5");

        GameObject inp = GameObject.Find("inpf2");
        InputField inpf = inp.GetComponent<InputField>();

        //GameObject tmp = GameObject.Find(inpf.text);
        NativeAnimationAvatar nav = manim.GetCastInProject(inpf.text);

        //ovrm.GetEffectiveActiveAvatar().GetComponent<OperateLoadedVRM>().EquipObject(HumanBodyBones.RightHand, nav);
        ovrm.GetEffectiveActiveAvatar().GetComponent<OperateLoadedEffect>().IsVRMCollider = true;
        ovrm.GetEffectiveActiveAvatar().GetComponent<OperateLoadedEffect>().SetVRMColliderSize(0.25f);

        //OperateLoadedCamera olc = tmp.GetComponent<OperateLoadedCamera>();
        //Debug.Log(olc.GetViewport());
        //OperateLoadedEffect ole = tmp.GetComponent<OperateLoadedEffect>();
        //ole.SetEffect("Explosion,TinyExplosion");
        //ole.PreviewEffect();

        //ovrm.ResetParentHandlePosition();

        //ovrm.ActivateAvatar(inpf.text,true);
        //GameObject.Find("MsgArea").transform.GetChild(0).GetComponent<OperateLoadedText>().SetSizeFromOuter("100,100");

        //ovrm.GetEffectiveActiveAvatar().GetComponent<OperateLoadedOther>().SetPositionFromOuter(inpf.text);


        //Debug.Log(Application.persistentDataPath);
        //ovrm.ActivateAvatarFromOuter(inpf.text);

        //GameObject canvas = GameObject.Find("Canvas");
        //canvas.GetComponent<UserVRMSpace.FileMenuCommands>().ObjectFileSelected("http://localhost:5020/vrmview/static/res/testship01.fbx");

        //InputField inpf2 = GameObject.Find("inpf2").GetComponent<InputField>();
        //GameObject.Find(inpf.text).GetComponent<OperateLoadedVRM>().SetFixMovingFromOuter(inpf2.text);

        //OperateLoadedOther olo = ovrm.ActiveAvatar.transform.parent.GetComponent<OperateLoadedOther>();
        //olo.PlayAnimation();

        //OperateLoadedOther olo = ovrm.GetEffectiveActiveAvatar().GetComponent<OperateLoadedOther>();
        //string js = JsonUtility.ToJson(olo.ListUserMaterial());
        //Debug.Log(js);

        //Camera.main.GetComponent<CameraOperation1>().ChangeMainCamera(inpf.text);

    }
    public void OnAct2PointerClick()
    {
        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        //ovrm.SetScale("0.5,0.5,0.5");

        GameObject inp = GameObject.Find("inpf2");
        InputField inpf = inp.GetComponent<InputField>();

        //ovrm.GetEffectiveActiveAvatar().GetComponent<OperateLoadedVRM>().UnequipObject(HumanBodyBones.RightHand, "");
        ovrm.GetEffectiveActiveAvatar().GetComponent<OperateLoadedEffect>().AddColliderTarget(inpf.text);

        /*
        OperateLoadedOther olo = ovrm.ActiveAvatar.transform.parent.GetComponent<OperateLoadedOther>();
        olo.StopAnimation();
        */
    }
    public void OnClickBtnFileClose()
    {
        IsOpenFilePanel = !IsOpenFilePanel;
        if (IsOpenFilePanel)
        {
            GameObject.Find("OpeFileOpen").transform.GetComponent<RectTransform>().DOAnchorPos(new Vector2(377f, -130f), 0.3f);
        }
        else
        {
            GameObject.Find("OpeFileOpen").transform.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-10f, -130f), 0.3f);
        }
    }

    public void OnClickBtnMoveClose()
    {
        IsOpenMovePanel = !IsOpenMovePanel;
        if (IsOpenMovePanel)
        {
            GameObject.Find("OpeMovePanel").transform.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-3f, 5f), 0.3f);
        }
        else
        {
            GameObject.Find("OpeMovePanel").transform.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-3f, -155f), 0.3f);
        }
    }
    public async void OnClickBtnEffect1()
    {
        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
        GameObject avt = ovrm.GetEffectiveActiveAvatar();
        Debug.Log(avt.name);
        string efname = "Effects/Explosion/TinyExplosion";
        OperateLoadedEffect ole = avt.GetComponent<OperateLoadedEffect>();
        await ole.SetEffectRef(efname);
        ole.PreviewEffect();
        /*
        GameObject ef1 = GameObject.Find("DustExplosion");
        ParticleSystem eft = ef1.GetComponent<ParticleSystem>();
        StartCoroutine(Effect1Live(eft));
        */
        /*
        GameObject ef1 = GameObject.Find("EffectDest01");
        OperateLoadedEffect ole = ef1.GetComponent<OperateLoadedEffect>();
        ole.SetEffect("Water,Shower");
        ole.PlayEffect();
        */
        /*
        
        
        GameObject av = ovrm.GetEffectiveActiveAvatar();
        OperateLoadedVRM olv = av.GetComponent<OperateLoadedVRM>();
        olv.SetVisibleAvatar(0);
        */
    }
    public async void OnClickBtnStageId()
    {
        GameObject inp = GameObject.Find("inp_StageId");
        InputField inpf = inp.GetComponent<InputField>();
        int v = int.TryParse(inpf.text, out v) ? v : 0;

        GameObject grd = GameObject.FindGameObjectWithTag("GroundWorld");
        OperateStage os = grd.GetComponent<OperateStage>();

        await os.SelectStageRef(v);
    }
    public void OnClickEnableHandleShow()
    {
        CameraOperation1 cam1 = GameObject.Find("Main Camera").GetComponent<CameraOperation1>();
        if (cam1.GetEnableHandleShowCamera())
        {
            cam1.EnableHandleShowCamera(0);
        }
        else
        {
            cam1.EnableHandleShowCamera(1);
        }
        
    }
    public void OnClickBtnScreenUp()
    {
        CameraOperation1 co = Camera.main.gameObject.GetComponent<CameraOperation1>();
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            curMouseTranslation = curMouseTranslation + Vector3.up;
            string param = curMouseTranslation.x + "," + curMouseTranslation.y;
            co.TranslateCameraPosFromOuter(param);
        }
        else
        {
            curMousePos = curMousePos + Vector3.up;
            string param = curMousePos.x + "," + curMousePos.y;
            co.RotateCameraPosFromOuter(param);
        }
        
    }
    public void OnClickBtnScreenDown()
    {
        CameraOperation1 co = Camera.main.gameObject.GetComponent<CameraOperation1>();
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            curMouseTranslation = curMouseTranslation + Vector3.down;
            string param = curMouseTranslation.x + "," + curMouseTranslation.y;
            co.TranslateCameraPosFromOuter(param);
        }
        else
        {
            curMousePos = curMousePos + Vector3.down;
            string param = curMousePos.x + "," + curMousePos.y;
            co.RotateCameraPosFromOuter(param);

        }
    }
    public void OnClickBtnScreenLeft()
    {
        CameraOperation1 co = Camera.main.gameObject.GetComponent<CameraOperation1>();
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            curMouseTranslation = curMouseTranslation + Vector3.left;
            string param = curMouseTranslation.x + "," + curMouseTranslation.y;
            co.TranslateCameraPosFromOuter(param);
        }
        else
        {
            curMousePos = curMousePos + Vector3.left;
            string param = curMousePos.x + "," + curMousePos.y;
            co.RotateCameraPosFromOuter(param);

        }
    }
    public void OnClickBtnScreenRight()
    {
        CameraOperation1 co = Camera.main.gameObject.GetComponent<CameraOperation1>();
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            curMouseTranslation = curMouseTranslation + Vector3.right;
            string param = curMouseTranslation.x + "," + curMouseTranslation.y;
            co.TranslateCameraPosFromOuter(param);
        }
        else
        {
            curMousePos = curMousePos + Vector3.right;
            string param = curMousePos.x + "," + curMousePos.y;
            co.RotateCameraPosFromOuter(param);

        }
    }
    private IEnumerator Effect1Live(ParticleSystem ps)
    {
        ps.Play();
        yield return new WaitForSeconds(2f);
        ps.Stop();
    }

    void showUIObject(bool flag)
    {
        RectTransform rpos = infoObject.GetComponent<RectTransform>();
        if (flag)
        {
            rpos.anchoredPosition = new Vector2(objectLeft, objectTop);
        }
        else
        {
            rpos.anchoredPosition = new Vector2(0 - rpos.sizeDelta.x, objectTop);
        }
    }
    public void Btn_CenteringCamera_OnClick()
    {
        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
        Camera.main.GetComponent<CameraOperation1>().CenteringCameraForAvatar(ovrm.GetEffectiveActiveAvatar().name);
    }
    public void BtnEnableActivate_OnClick()
    {
        ConfigSettingLabs conf = GameObject.Find("Canvas").GetComponent<ConfigSettingLabs>();
        int v = conf.GetIntVal("enable_activateavatar_from_unity");
        if (v == 1)
        {
            conf.SetIntVal("enable_activateavatar_from_unity", 0);
        }
        else
        {
            conf.SetIntVal("enable_activateavatar_from_unity", 1);
        }
    }
    public void Box_ActivateAvatar_OnChange()
    {
        Dropdown dd = GameObject.Find("Box_ActivateAvatar").GetComponent<Dropdown>();
        Debug.Log(dd.value);

        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
        ovrm.ChangeEnableAvatar(dd.value);
    }
    public void OnChange_inp_Jumpnum()
    {
        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
        OperateLoadedBase olb = ovrm.GetEffectiveActiveAvatar().GetComponent<OperateLoadedBase>();

        GameObject inp = GameObject.Find("inp_jumpnum");
        InputField inpf = inp.GetComponent<InputField>();

        string js = "1," + inpf.text;
        olb.SetJump(js);
    }
}
