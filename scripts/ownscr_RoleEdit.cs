using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UserHandleSpace;
using DG.Tweening;
using VRM;

using System.IO;


public class ownscr_RoleEdit : MonoBehaviour
{
    public GameObject AnimateArea;
    public GameObject ViewArea;
    public GameObject IKArea;

    private GameObject pnl_roleEdit_new;
    private GameObject pnl_roleEdit_edit;
    private GameObject Scrview_new;
    private GameObject Scrview_new_content;
    private GameObject Scrview_edit_content;

    ManageAnimation manim;

    List<NativeAnimationAvatar> LstAvatar;
    List<NativeAnimationAvatar> LstRoler;
    List<BasicStringIntList> LstTargetType;
    List<GameObject> LstEditAvatar;

    int sel_targetType;
    int sel_new_avatar;
    int sel_edit_role;
    int sel_edit_avatar;

    private void Awake()
    {
        LstTargetType = new List<BasicStringIntList>();
        LstTargetType.Add(new BasicStringIntList("", -1));
        LstTargetType.Add(new BasicStringIntList("Player", (int)AF_TARGETTYPE.VRM));
        LstTargetType.Add(new BasicStringIntList("OtherPlayer", (int)AF_TARGETTYPE.OtherObject));
        LstTargetType.Add(new BasicStringIntList("LightPlayer", (int)AF_TARGETTYPE.Light));
        LstTargetType.Add(new BasicStringIntList("CameraPlayer", (int)AF_TARGETTYPE.Camera));
        LstTargetType.Add(new BasicStringIntList("TextPlayer", (int)AF_TARGETTYPE.Text));
        LstTargetType.Add(new BasicStringIntList("OtherPlayer", (int)AF_TARGETTYPE.Image));
        LstTargetType.Add(new BasicStringIntList("UImagePlayer", (int)AF_TARGETTYPE.UImage));
        LstTargetType.Add(new BasicStringIntList("EffectDestination", (int)AF_TARGETTYPE.Effect));
    }
    // Start is called before the first frame update
    void Start()
    {
        sel_new_avatar = 0;
        sel_edit_role = 0;
        sel_edit_avatar = 0;
        manim = AnimateArea.GetComponent<ManageAnimation>();
        LstAvatar = new List<NativeAnimationAvatar>();
        LstRoler = new List<NativeAnimationAvatar>();
        LstEditAvatar = new List<GameObject>();

        pnl_roleEdit_new = transform.Find("pnl_roleEdit_new").gameObject;
        pnl_roleEdit_edit = transform.Find("pnl_roleEdit_edit").gameObject;
        Scrview_new = pnl_roleEdit_new.transform.Find("Scrview_new").gameObject;
        Scrview_new_content = Scrview_new.transform.Find("Viewport").Find("Content").gameObject;
        Scrview_edit_content = pnl_roleEdit_edit.transform.Find("Scrview_edit").Find("Viewport").Find("Content").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickBtnCloseRoleEdit()
    {
        GetComponent<RectTransform>().DOScale(new Vector3(0f, 0f, 0f), 0.2f);
        gameObject.SetActive(false);
        Camera.main.gameObject.GetComponent<RuntimeGizmos.TransformGizmo>().enabled = true;
    }
    public void OnToggleRoleEdit_New_Group(bool value)
    {
        Debug.Log("new" + value);
        transform.Find("pnl_roleEdit_new").gameObject.SetActive(true);
        transform.Find("pnl_roleEdit_edit").gameObject.SetActive(false);
    }
    public void OnToggleRoleEdit_Edit_Group(bool value)
    {
        Debug.Log("edit" + value);
        transform.Find("pnl_roleEdit_new").gameObject.SetActive(false);
        transform.Find("pnl_roleEdit_edit").gameObject.SetActive(true);
    }

    //---new mode----------------------------------------
    public void OnChangeLstTargetType()
    {
        int v = transform.Find("lst_targetType").gameObject.GetComponent<Dropdown>().value;

        sel_targetType = v;
        BasicStringIntList targetType = LstTargetType[v];
        v--;

        /*GameObject lst_new_avatar = transform.Find("pnl_roleEdit_new").Find("lst_new_avatar").gameObject;
        Dropdown dlst_new_avatar = lst_new_avatar.GetComponent<Dropdown>();
        dlst_new_avatar.ClearOptions();
        
        dlst_new_avatar.options.Add(new Dropdown.OptionData(" "));*/

        //---edit mode panel
        /*GameObject lst_edit_role = transform.Find("pnl_roleEdit_edit").Find("lst_edit_role").gameObject;
        Dropdown dlst_edit_role = lst_edit_role.GetComponent<Dropdown>();
        dlst_edit_role.ClearOptions();
        dlst_edit_role.options.Add(new Dropdown.OptionData(" "));
        GameObject lst_edit_avatar = transform.Find("pnl_roleEdit_edit").Find("lst_edit_avatar").gameObject;
        Dropdown dlst_edit_avatar = lst_edit_avatar.GetComponent<Dropdown>();
        dlst_edit_avatar.ClearOptions();
        dlst_edit_avatar.options.Add(new Dropdown.OptionData(" "));*/

        LstAvatar.Clear();
        LstRoler.Clear();
        LstEditAvatar.Clear();
        for (int i = Scrview_new_content.transform.childCount-1; i >= 0 ; i--)
        {
            Destroy(Scrview_new_content.transform.GetChild(i).gameObject);
        }
        for (int i = 0; i < Scrview_edit_content.transform.childCount; i++)
        {
            Destroy(Scrview_edit_content.transform.GetChild(i).gameObject);
        }

        List<GameObject> panellist = new List<GameObject>();
        foreach (NativeAnimationAvatar avatar in  manim.currentProject.casts)
        {
            if ((AF_TARGETTYPE)targetType.value == avatar.type)
            {
                /*Dropdown.OptionData opt = new Dropdown.OptionData();
                if (avatar.avatar != null)
                {
                    if (avatar.type == AF_TARGETTYPE.VRM)
                    {
                        opt.text = avatar.avatar.GetComponent<VRMMeta>().Meta.Title;
                    }
                    else
                    {
                        opt.text = avatar.avatar.name;
                    }
                    dlst_new_avatar.options.Add(opt);
                    LstAvatar.Add(avatar);   
                }*/
                //---edit mode ---
                //dlst_edit_role.options.Add(new Dropdown.OptionData(avatar.roleTitle));
                LstRoler.Add(avatar);

                //===new code
                GameObject copypanel = (GameObject)Resources.Load("pnl_new_editRole");
                GameObject panel = Instantiate(copypanel, copypanel.transform.position, Quaternion.identity, Scrview_new_content.transform);
                if (avatar.avatar != null)
                {
                    string nametext = manim.GetObjectTitle(avatar);
                    panel.transform.GetChild(0).GetComponent<Text>().text = nametext;
                    panel.transform.GetChild(1).GetComponent<InputField>().text = avatar.roleTitle;
                    panel.transform.GetChild(2).GetComponent<Text>().text = avatar.avatar.name;
                    LstAvatar.Add(avatar);
                }

                GameObject copypanel2 = (GameObject)Resources.Load("pnl_edit_editAvatar");
                GameObject panel2 = Instantiate(copypanel2, copypanel2.transform.position, Quaternion.identity, Scrview_edit_content.transform);
                panel2.transform.GetChild(0).GetComponent<Text>().text = avatar.roleTitle;
                panel2.transform.GetChild(2).GetComponent<Text>().text = avatar.roleName;
                panel2.transform.GetChild(1).GetComponent<Dropdown>().ClearOptions();
                panel2.transform.GetChild(1).GetComponent<Dropdown>().options.Add(new Dropdown.OptionData(""));

                panellist.Add(panel2);
            }
        }
        //---edit mode---

        if (targetType.value != -1)
        {
            GameObject[] objs = GameObject.FindGameObjectsWithTag(targetType.text);
            for (int i = 0; i < objs.Length; i++)
            {
                GameObject av = objs[i];

                /*if (targetType.text == "Player")
                {
                    string title = av.GetComponent<VRMMeta>().Meta.Title;
                    if (title != "")
                    {
                        dlst_edit_avatar.options.Add(new Dropdown.OptionData(title));
                    }
                    else
                    {
                        dlst_edit_avatar.options.Add(new Dropdown.OptionData(av.name));
                    }
                    
                }
                else
                {
                    dlst_edit_avatar.options.Add(new Dropdown.OptionData(av.name));
                }*/
            
                LstEditAvatar.Add(av);
                
                //=== new code
                foreach(GameObject gao in panellist)
                {
                    Dropdown dp = gao.transform.GetChild(1).GetComponent<Dropdown>();
                    
                    if (targetType.text == "Player")
                    {
                        string title = av.GetComponent<VRMMeta>().Meta.Title;
                        if (title != "")
                        {
                            dp.options.Add(new Dropdown.OptionData(title));
                        }
                        else
                        {
                            dp.options.Add(new Dropdown.OptionData(av.name));
                        }

                    }
                    else
                    {
                        dp.options.Add(new Dropdown.OptionData(av.name));
                    }
                }
                
            }
            foreach (GameObject gao in panellist)
            {
                //---recover current settings (if exists)
                NativeAnimationAvatar nav = manim.currentProject.casts.Find(match =>
                {
                    if (match.roleName == gao.transform.GetChild(2).GetComponent<Text>().text) return true;
                    return false;
                });

                int hit = LstEditAvatar.FindIndex((match) =>
                {
                    if (match.name == nav.avatarId)
                    {
                        return true;
                    }
                    return false;
                });
                if (hit > -1)
                {
                    Dropdown dp = gao.transform.GetChild(1).GetComponent<Dropdown>();

                    dp.value = hit+1;
                }

               
            }

        }

        
    }
    public void OnClickSaveNewMode()
    {
        for (int i = 0; i <  Scrview_new_content.transform.childCount; i++)
        {
            GameObject pnl_new_editRole = Scrview_new_content.transform.GetChild(i).gameObject;
            List<string> ln = pnl_new_editRole.GetComponent<ownscr_PnlNewEditRole>().GetEditedRoleList();

            manim.EditActorsRole(ln[0] + "," + ln[1]);
        }
    }

    public void OnChangeLstNewAvatar()
    {
        GameObject lst_new_avatar = transform.Find("pnl_roleEdit_new").Find("lst_new_avatar").gameObject;
        Dropdown dlst_new_avatar = lst_new_avatar.GetComponent<Dropdown>();
        sel_new_avatar = dlst_new_avatar.value;

        InputField inp = transform.Find("pnl_roleEdit_new").Find("inp_new_avatar_role").gameObject.GetComponent<InputField>();
        inp.text = LstAvatar[sel_new_avatar - 1].roleTitle;
    }
    public void OnChangeInpNewAvatarRole(string value)
    {
        InputField inp = transform.Find("pnl_roleEdit_new").Find("inp_new_avatar_role").gameObject.GetComponent<InputField>();
        Debug.Log(inp.text);

        //GameObject lst_new_avatar = transform.Find("pnl_roleEdit_new").Find("lst_new_avatar").gameObject;
        //Dropdown dlst_new_avatar = lst_new_avatar.GetComponent<Dropdown>();
        //int index = dlst_new_avatar.value;

        //---To edit role title of the actor
        if ((sel_new_avatar > 0) && ( (sel_new_avatar - 1) < LstAvatar.Count))
        {
            manim.EditActorsRole(LstAvatar[sel_new_avatar - 1].avatarId + "," + inp.text);
            
        }

    }

    //---edit mode------------------------------------
    public void OnClickSaveEditMode()
    {
        List<int> lst = new List<int>();
        //---create temporary list
        for (int i = 0; i < Scrview_edit_content.transform.childCount; i++)
        {
            GameObject pnl = Scrview_edit_content.transform.GetChild(i).gameObject;
            int sel = pnl.GetComponent<ownscr_PnlNewEditRole>().GetSelectedAvatarList();
            lst.Add(sel);
        }


        for (int i = 0; i < Scrview_edit_content.transform.childCount; i++)
        {
            GameObject pnl = Scrview_edit_content.transform.GetChild(i).gameObject;
            int sel = pnl.GetComponent<ownscr_PnlNewEditRole>().GetSelectedAvatarList();

            //---duplicate check
            for (int s = i+1; s < lst.Count; s++)
            {
                if (lst[s] == sel)
                {
                    string csv = LstRoler[i].roleName + ",role";
                    manim.DetachAvatarFromRole(csv);
                    Scrview_edit_content.transform.GetChild(s).gameObject.GetComponent<ownscr_PnlNewEditRole>().SelectAvatarList(0);
                }
            }

            //---attach 
            if (sel == 0)
            {
                string csv = LstRoler[i].roleName + ",role";
                manim.DetachAvatarFromRole(csv);
            }
            else
            {
                //---To attach selected Player object to the avatar in the animation project
                string csv = LstRoler[i].roleName + "," + LstEditAvatar[sel - 1].name;
                
                manim.AttachAvatarToRole(csv);
            }

        }
    }
    public void OnChangeLstEditRole()
    {
        GameObject lst_edit_role = transform.Find("pnl_roleEdit_edit").Find("lst_edit_role").gameObject;
        Dropdown dlst_edit_role = lst_edit_role.GetComponent<Dropdown>();
        sel_edit_role = dlst_edit_role.value;

        GameObject lst_edit_avatar = transform.Find("pnl_roleEdit_edit").Find("lst_edit_avatar").gameObject;
        Dropdown dlst_edit_avatar = lst_edit_avatar.GetComponent<Dropdown>();
        sel_edit_avatar = dlst_edit_avatar.value;

        //---recover current settings (if exists)
        if (LstRoler[sel_edit_role-1].avatar != null)
        {
            int hit = LstEditAvatar.FindIndex((match) =>
            {
                if (match.name == LstRoler[sel_edit_role-1].avatarId)
                {
                    return true;
                }
                return false;
            });
            if (hit > -1)
            {
                dlst_edit_avatar.value = hit;
            }
            
        }
        

    }
    public void OnChangeLstEditAvatar()
    {
        GameObject lst_edit_avatar = transform.Find("pnl_roleEdit_edit").Find("lst_edit_avatar").gameObject;
        Dropdown dlst_edit_avatar = lst_edit_avatar.GetComponent<Dropdown>();
        sel_edit_avatar = dlst_edit_avatar.value;

        if (sel_edit_avatar == 0)
        {
            string csv = LstRoler[sel_edit_role - 1].roleName + ",role";
            manim.DetachAvatarFromRole(csv);
        }
        else
        {
            //---To attach selected Player object to the avatar in the animation project
            string csv = LstRoler[sel_edit_role - 1].roleName + "," + LstEditAvatar[sel_edit_avatar - 1].name;
            manim.AttachAvatarToRole(csv);
        }
        
    }
}
