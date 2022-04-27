using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

using UserHandleSpace;

namespace UserUISpace
{
    public class UserUIManager : MonoBehaviour
    {
        private UIDocument MainUI;
        private VisualElement rootElement;

        private List<string> lstmenu_actor_setting;
        private List<string> lstmenu_objlist;

        // Start is called before the first frame update
        void Start()
        {
            MainUI = GetComponent<UIDocument>();
            rootElement = MainUI.rootVisualElement;

            GenerateUI();
        }

        // Update is called once per frame
        void Update()
        {

        }
        public VisualElement relatedElement
        {
            get
            {
                return rootElement;
            }
        }
        void GenerateUI ()
        {
            //---toolbar
            Button tlb_btn_newobj = rootElement.Q<Button>("tlb_btn_newobj");
            tlb_btn_newobj.clicked += Newobj_OnClick;

            rootElement.Q<Button>("tlb_menunew_vrm").clicked += Newobj_VRM_OnClick;
            rootElement.Q<Button>("tlb_menunew_oobject").clicked += Newobj_OtherObject_OnClick;
            rootElement.Q<Button>("tlb_menunew_spotlight").clicked += Newobj_SpotLight_OnClick;
            rootElement.Q<Button>("tlb_menunew_pointlight").clicked += Newobj_PointLight_OnClick;
            rootElement.Q<Button>("tlb_menunew_camera").clicked += Newobj_Camera_OnClick;
            rootElement.Q<Button>("tlb_menunew_eff").clicked += Newobj_Effect_OnClick;
            rootElement.Q<Button>("tlb_menunew_img").clicked += Newobj_Image_OnClick;
            rootElement.Q<Button>("tlb_menunew_uimg").clicked += Newobj_UImage_OnClick;
            rootElement.Q<Button>("tlb_menunew_text").clicked += Newobj_Text_OnClick;

            Button tlb_btn_actorsetting = rootElement.Q<Button>("tlb_btn_actorsetting");
            tlb_btn_actorsetting.clicked += ActorSetting_OnClick;

            Button tlb_btn_hidemenu = rootElement.Q<Button>("tlb_btn_hidemenu");
            tlb_btn_hidemenu.clicked += HideMenu_OnClick;


            VisualElement panel = rootElement.Q<VisualElement>("testmenupanel");
            ListView lv = panel.Q<ListView>();
            lstmenu_actor_setting = new List<string>();
            lstmenu_actor_setting.Add("VRM");
            lstmenu_actor_setting.Add("OtherObject");
            lstmenu_actor_setting.Add("Image");
            lstmenu_actor_setting.Add("UImage");
            lstmenu_actor_setting.Add("BGM");
            lstmenu_actor_setting.Add("SE");
            System.Func<VisualElement> make = () => new Label();
            System.Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = lstmenu_actor_setting[i];

            lv.makeItem = make;
            lv.bindItem = bindItem;
            lv.itemsSource = lstmenu_actor_setting;
            lv.onSelectedIndicesChange += inx =>
            {
                IEnumerator iem = inx.GetEnumerator();
                while (iem.MoveNext())
                {
                    Debug.Log(iem.Current);
                    Debug.Log(lstmenu_actor_setting[(int)iem.Current]);
                }
            };

            //---left panel
            lstmenu_objlist = new List<string>();
            ListView objlist = rootElement.Q<ListView>("objlist");
            System.Func<VisualElement> objlist_make = () => new Label();
            System.Action<VisualElement, int> objlist_bindItem = (e, i) => (e as Label).text = lstmenu_objlist[i];
            objlist.makeItem = objlist_make;
            objlist.bindItem = objlist_bindItem;
            objlist.itemsSource = lstmenu_objlist;
            objlist.onSelectedIndicesChange += inx =>
            {
                IEnumerator iem = inx.GetEnumerator();
                while (iem.MoveNext())
                {
                    int index = (int)iem.Current;
                    GameObject ikhp = GameObject.Find("IKHandleParent");
                    OperateActiveVRM ovrm = ikhp.GetComponent<OperateActiveVRM>();
                    ovrm.ChangeEnableAvatar(index);
                }
            };
        }
        void AddListViewItem(ListView parentObject, string item)
        {
            parentObject.itemsSource.Add(item);
        }
        void ShowHideToolbarMenu(VisualElement panel)
        {
            if ((panel.style.visibility == null) || (panel.style.visibility == Visibility.Hidden))
            {
                panel.style.visibility = Visibility.Visible;
            }
            else
            {
                panel.style.visibility = Visibility.Hidden;
            }
        }
        void Newobj_OnClick()
        {
            VisualElement panel = rootElement.Q<VisualElement>("tlb_menu_new");
            ShowHideToolbarMenu(panel);
        }
        void Newobj_VRM_OnClick()
        {
            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();
            fmc.OnButtonPointerDown();

            Newobj_OnClick();
        }
        void Newobj_OtherObject_OnClick()
        {
            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();
            fmc.OnBtnLoadObjPointerDown();

            Newobj_OnClick();
        }
        void Newobj_SpotLight_OnClick()
        {
            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();
            fmc.OpenSpotLight();

            Newobj_OnClick();
        }
        void Newobj_PointLight_OnClick()
        {
            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();
            fmc.OpenPointLight();

            Newobj_OnClick();
        }
        void Newobj_Camera_OnClick()
        {
            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();
            fmc.OnClickBtnAddCamera();

            Newobj_OnClick();
        }
        void Newobj_Effect_OnClick()
        {
            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();
            fmc.CreateSingleEffect();

            Newobj_OnClick();
        }
        void Newobj_Text_OnClick()
        {
            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();
            fmc.OpenSample1Text();

            Newobj_OnClick();
        }
        void Newobj_Image_OnClick()
        {
            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();
            fmc.OnClickBtnImage();

            Newobj_OnClick();
        }
        void Newobj_UImage_OnClick()
        {
            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();
            fmc.OnClickBtnUImage();

            Newobj_OnClick();
        }
        void HideMenu_OnClick()
        {
            VisualElement panel = rootElement.Q<VisualElement>("objlistpanel");
            ShowHideToolbarMenu(panel);
        }
        void ActorSetting_OnClick()
        {
            VisualElement panel = rootElement.Q<VisualElement>("testmenupanel");
            ShowHideToolbarMenu(panel);
        }

        //--------------------------------------------
        public void objlist_add_item(string item)
        {
            ListView objlist = rootElement.Q<ListView>("objlist");
            lstmenu_objlist.Add(item);
            System.Action<VisualElement, int> objlist_bindItem = (e, i) => (e as Label).text = lstmenu_objlist[i];
            objlist.bindItem = objlist_bindItem;
            //AddListViewItem(objlist, item);
            Debug.Log(objlist.itemsSource);
            Debug.Log(item);
        }
    }
}

