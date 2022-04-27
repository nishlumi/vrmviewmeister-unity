using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using RootMotion.FinalIK;
using UserHandleSpace;

public class OperateAvatarIK : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ChangeBendSliderHTML(int value);

    public float bendValue;
    public GameObject[] checkingUI;
    // Start is called before the first frame update
    void Start()
    {
        bendValue = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnPointerDownBendUp(BaseEventData evt)
    {
        Debug.Log("Bend Up");
        Slider sli = evt.selectedObject.GetComponent<Slider>();
        sli.value += 1;
        GameObject ikhp = GameObject.Find("IKHandleWorld");
        string pn = ikhp.GetComponent<OperateActiveVRM>().ActivePartsName;
        JudgeParts(ikhp.GetComponent<OperateActiveVRM>(), pn, sli.value);
    }
    public void OnPointerDownBendDown(BaseEventData evt)
    {
        Debug.Log("Bend Down");
        Slider sli = evt.selectedObject.GetComponent<Slider>();
        sli.value -= 1;
        GameObject ikhp = GameObject.Find("IKHandleWorld");
        string pn = ikhp.GetComponent<OperateActiveVRM>().ActivePartsName;
        JudgeParts(ikhp.GetComponent<OperateActiveVRM>(), pn, sli.value);
    }
    public void OnChangeBendSlider()
    {
        Slider sli = GameObject.Find("BendSlider").GetComponent<Slider>();
        changeSwivelFromOuter(sli.value);
    }
    //=======================Change Mode "MOVE" <-> "IK"  (Unity internal version)****************
    public void OnChangeAvatarMoveToggle(bool value)
    {
        //GameObject bhp = GameObject.Find("BoneHandleParent");
        //GameObject grnd = bhp.transform.Find("CharaGround").gameObject;
        Toggle tog = checkingUI[1].GetComponent<Toggle>();
        //Debug.Log(grnd.name + " - " + tog.isOn);
        //grnd.SetActive(tog.isOn);

        GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
        ovrm.ChangeMoveModeFromOuter(tog.isOn ? 1 : 0);


        //ovrm.isMoveMode = tog.isOn;

        //ovrm.ShowHandleBody(tog.isOn, ovrm.ActiveIKHandle);
    }
    public void OnChangeAvatarFixToggle(bool value)
    {
        Toggle tog = GameObject.Find("AvatarFixToggle").GetComponent<Toggle>();

        GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        OperateLoadedBase olb = ovrm.GetEffectiveActiveAvatar().GetComponent<OperateLoadedBase>();
        olb.SetFixMoving(tog.isOn);
    }
    //---user methods
    //===============================Change Mode "MOVE" <-> "IK"  (WebGL version)******************
    public void ChangeToggleAvatarMoveFromOuter(int flag) //***call from HTML
    {
        bool tog = (flag == 1 ? true : false);

        GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
        ovrm.isMoveMode = tog;

        ovrm.ShowHandleBody(tog, ovrm.ActiveIKHandle);

    }
    public void OnClickBtnPlayAnim()
    {
        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        OperateLoadedOther olo = ovrm.GetEffectiveActiveAvatar().TryGetComponent<OperateLoadedOther>(out olo) ? olo : null;
        if (olo != null)
        {
            if (olo.GetSystemAnimationType(0) != "")
            {
                olo.PlayAnimation();
            }
        }

    }
    public void OnClickBtnPauseAnim()
    {
        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        OperateLoadedOther olo = ovrm.GetEffectiveActiveAvatar().TryGetComponent<OperateLoadedOther>(out olo) ? olo : null;
        if (olo != null)
        {
            if (olo.GetSystemAnimationType(0) != "")
            {
                olo.PauseAnimation();
            }
        }

    }
    public void OnClickBtnStopAnim()
    {
        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        OperateLoadedOther olo = ovrm.GetEffectiveActiveAvatar().TryGetComponent<OperateLoadedOther>(out olo) ? olo : null;
        if (olo != null)
        {
            if (olo.GetSystemAnimationType(0) != "")
            {
                olo.StopAnimation();
            }
        }

    }
    public void OnChangeSlidAnimSeek()
    {
        GameObject slid = GameObject.Find("SlidAnimSeek");

        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        OperateLoadedOther olo = ovrm.GetEffectiveActiveAvatar().TryGetComponent<OperateLoadedOther>(out olo) ? olo : null;
        if (olo != null)
        {
            if (olo.GetSystemAnimationType(0) != "")
            {
                olo.SetSeekPosAnimation(slid.GetComponent<Slider>().value);
                olo.SeekPlayAnimation();
            }
        }
    }
    public void OnClickBtnSshot()
    {
        Camera.main.GetComponent<ScreenShot>().CaptureScreen(0);
    }
    public void SetInpAvatarScale(float v)
    {
        InputField inpf = GameObject.Find("inp_AvatarScale").GetComponent<InputField>();
        inpf.text = (Mathf.Abs(v) * 100f).ToString();
    }
    public void OnChangeInpAvatarScale()
    {
        InputField inpf = GameObject.Find("inp_AvatarScale").GetComponent<InputField>();

        Debug.Log(inpf.text);
        int v = int.TryParse(inpf.text, out v) ? v : 100;
        float f = (float)v / 100f;

        GameObject ikhp = GameObject.Find("IKHandleParent");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();

        ovrm.SetScale(f + "," + f + ","+ f);
    }

    //---Below is no used (will delete)=========================================================
    public void changeSwivelFromOuter(float value) //***call from HTML
    {
        GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
        OperateActiveVRM oavrm = ikhp.GetComponent<OperateActiveVRM>();
        string pn = oavrm.ActivePartsName;
        JudgeParts(oavrm, pn, value);
    }
    void JudgeParts(OperateActiveVRM oavrm, string parts, float value)
    { //スライダーの値を選択したIK部位のヒントの値にセットする
        VRIK vik;
        if (oavrm.ActiveAvatar.TryGetComponent<VRIK>(out vik))
        {
            if (parts == "leftarm")
            {
                vik.solver.leftArm.swivelOffset = value;
            }
            else if (parts == "rightarm")
            {
                vik.solver.rightArm.swivelOffset = value;
            }
            else if (parts == "leftleg")
            {
                vik.solver.leftLeg.swivelOffset = value;
            }
            else if (parts == "rightleg")
            {
                vik.solver.rightLeg.swivelOffset = value;
            }
        }
        
    }
    public void ReloadPartsSwivel(string parts)
    { //選択したIK部位のヒントの値をスライダーに描き戻す
        Slider sli = GameObject.Find("BendSlider").GetComponent<Slider>();
        GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
        if (ovrm.ActiveAvatar) {
            VRIK vik;
            if (ovrm.ActiveAvatar.TryGetComponent<VRIK>(out vik))
            {
                if (parts == "leftarm")
                {
                    sli.value = vik.solver.leftArm.swivelOffset;
                }
                else if (parts == "rightarm")
                {
                    sli.value = vik.solver.rightArm.swivelOffset;
                }
                else if (parts == "leftleg")
                {
                    sli.value = vik.solver.leftLeg.swivelOffset;
                }
                else if (parts == "rightleg")
                {
                    sli.value = vik.solver.rightLeg.swivelOffset;
                }
#if !UNITY_EDITOR && UNITY_WEBGL
                ChangeBendSliderHTML((int)sli.value);
#endif

            }
        }
    }
    public void ResetSwivel(string parts)
    {
        Slider sli = GameObject.Find("BendSlider").GetComponent<Slider>();
        GameObject ikhp = GameObject.FindGameObjectWithTag("IKHandleWorld");
        OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
        VRIK vik;
        if (ovrm.ActiveAvatar.TryGetComponent<VRIK>(out vik))
        {
            if (parts == "leftarm")
            {
                vik.solver.leftArm.swivelOffset = 0f;
                sli.value = vik.solver.leftArm.swivelOffset;
            }
            else if (parts == "rightarm")
            {
                vik.solver.rightArm.swivelOffset = 0f;
                sli.value = vik.solver.rightArm.swivelOffset;
            }
            else if (parts == "leftleg")
            {
                vik.solver.leftLeg.swivelOffset = 0f;
                sli.value = vik.solver.leftLeg.swivelOffset;
            }
            else if (parts == "rightleg")
            {
                vik.solver.rightLeg.swivelOffset = 0f;
                sli.value = vik.solver.rightLeg.swivelOffset;
            }
#if !UNITY_EDITOR && UNITY_WEBGL
            ChangeBendSliderHTML((int)sli.value);
#endif

        }

    }

}
