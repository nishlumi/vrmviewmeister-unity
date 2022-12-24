using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using DG.Tweening;

using System.IO;
using TriLibCore;
using TriLibCore.General;
using TriLibCore.SFB;


using UserHandleSpace;

namespace UserUISpace
{
    public class UserUIManager : MonoBehaviour
    {
        private ManageAnimation manim;

        private UIDocument MainUI;
        private VisualElement rootElement;

        private List<string> lstmenu_actor_setting;
        private List<string> lstmenu_objlist;

        private List<string> lstproject_rolestrlist;
        private List<string> lstproject_rolestrlist_back;
        NativeAnimationAvatar currentSelectedRole;

        // Start is called before the first frame update
        void Start()
        {
            currentSelectedRole = null;
            GameObject aa = GameObject.Find("AnimateArea");
            manim = aa.GetComponent<ManageAnimation>();

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

            /*rootElement.Q<Button>("tlb_menunew_vrm").clicked += Newobj_VRM_OnClick;
            rootElement.Q<Button>("tlb_menunew_oobject").clicked += Newobj_OtherObject_OnClick;
            rootElement.Q<Button>("tlb_menunew_spotlight").clicked += Newobj_SpotLight_OnClick;
            rootElement.Q<Button>("tlb_menunew_pointlight").clicked += Newobj_PointLight_OnClick;
            rootElement.Q<Button>("tlb_menunew_camera").clicked += Newobj_Camera_OnClick;
            rootElement.Q<Button>("tlb_menunew_eff").clicked += Newobj_Effect_OnClick;
            rootElement.Q<Button>("tlb_menunew_img").clicked += Newobj_Image_OnClick;
            rootElement.Q<Button>("tlb_menunew_uimg").clicked += Newobj_UImage_OnClick;
            rootElement.Q<Button>("tlb_menunew_text").clicked += Newobj_Text_OnClick;*/

            Button tlb_btn_openproj = rootElement.Q<Button>("tlb_btn_openproj");
            tlb_btn_openproj.clicked += OpenProj_OnClick;

            Button tlb_btn_actorsetting = rootElement.Q<Button>("tlb_btn_actorsetting");
            tlb_btn_actorsetting.clicked += ActorSetting_OnClick;

            Button tlb_btn_hidemenu = rootElement.Q<Button>("tlb_btn_hidemenu");
            tlb_btn_hidemenu.clicked += HideMenu_OnClick;

            Button tlb_btn_playanim = rootElement.Q<Button>("tlb_btn_playanimation");
            tlb_btn_playanim.clicked += PlayAnimation_OnClick;
            Button tlb_btn_pauseanim = rootElement.Q<Button>("tlb_btn_pauseanimation");
            tlb_btn_pauseanim.clicked += PauseAnimation_OnClick;
            Button tlb_btn_stopanim = rootElement.Q<Button>("tlb_btn_stopanimation");
            tlb_btn_stopanim.clicked += StopAnimation_OnClick;

            Button tlb_btn_selwater = rootElement.Q<Button>("tlb_btn_selwater");
            tlb_btn_selwater.clicked += selectwater_OnClick;
            Button tlb_btn_changewater = rootElement.Q<Button>("tlb_btn_changewater");
            tlb_btn_changewater.clicked += wateropt1_OnClick;


            //---tlb_menunew set up 
            VisualElement tlb_menu_new = rootElement.Q<VisualElement>("tlb_menu_new");
            List<string> menu_new_strlst = new List<string>();
            menu_new_strlst.Add("VRM");
            menu_new_strlst.Add("OtherObject");
            menu_new_strlst.Add("Spot light");
            menu_new_strlst.Add("Point light");
            menu_new_strlst.Add("Camera");
            menu_new_strlst.Add("Effect");
            menu_new_strlst.Add("Image");
            menu_new_strlst.Add("UImage");
            menu_new_strlst.Add("Text");
            menu_new_strlst.Add("Blank Object Cube");
            menu_new_strlst.Add("Blank Object Sphere");
            menu_new_strlst.Add("Blank Object Plane");
            menu_new_strlst.Add("Blank Water level");
            ListView tlb_menunew_listview = CreateVListView(menu_new_strlst, tlb_menu_new, "tlb_menunew_listview");
            tlb_menunew_listview.onSelectedIndicesChange += inx =>
            {
                IEnumerator iem = inx.GetEnumerator();
                while (iem.MoveNext())
                {
                    int index = (int)iem.Current;
                    switch (index)
                    {
                        case 0:
                            Newobj_VRM_OnClick(); break;
                        case 1:
                            Newobj_OtherObject_OnClick(); break;
                        case 2:
                            Newobj_SpotLight_OnClick(); break;
                        case 3:
                            Newobj_PointLight_OnClick(); break;
                        case 4:
                            Newobj_Camera_OnClick(); break;
                        case 5:
                            Newobj_Effect_OnClick(); break;
                        case 6:
                            Newobj_Image_OnClick(); break;
                        case 7:
                            Newobj_UImage_OnClick(); break;
                        case 8:
                            Newobj_Text_OnClick(); break;
                        case 9:
                            Newobj_Blank_OnClick(UserPrimitiveType.Cube); break;
                        case 10:
                            Newobj_Blank_OnClick(UserPrimitiveType.Sphere); break;
                        case 11:
                            Newobj_Blank_OnClick(UserPrimitiveType.Plane); break;
                        case 12:
                            Newobj_Blank_OnClick(UserPrimitiveType.WaterLevel); break;
                    }
                }
            };

            //---testmenupanel set up
            VisualElement panel = rootElement.Q<VisualElement>("testmenupanel");
            //ListView lv = panel.Q<ListView>();
            lstmenu_actor_setting = new List<string>();
            lstmenu_actor_setting.Add("VRM");
            lstmenu_actor_setting.Add("OtherObject");
            lstmenu_actor_setting.Add("Image");
            lstmenu_actor_setting.Add("UImage");
            lstmenu_actor_setting.Add("BGM");
            lstmenu_actor_setting.Add("SE");
            /*System.Func<VisualElement> make = () => new Label();
            System.Action<VisualElement, int> bindItem = (e, i) => (e as Label).text = lstmenu_actor_setting[i];

            lv.makeItem = make;
            lv.bindItem = bindItem;
            lv.itemsSource = lstmenu_actor_setting;*/

            ListView lv = CreateVListView(lstmenu_actor_setting, panel, "");
            lv.onSelectedIndicesChange += inx =>
            {
                IEnumerator iem = inx.GetEnumerator();
                while (iem.MoveNext())
                {
                    Debug.Log(iem.Current);
                    Debug.Log(lstmenu_actor_setting[(int)iem.Current]);
                }
            };

            //---objlist panel set up
            lstmenu_objlist = new List<string>();
            lstmenu_objlist.Add("----");
            VisualElement objlistpanel = rootElement.Q<VisualElement>("objlistpanel");
            ListView objlist = objlistpanel.Q<ListView>("objlist");
            
            
            System.Func<VisualElement> objlist_make = () => new Label();
            System.Action<VisualElement, int> objlist_bindItem = (e, i) => (e as Label).text = lstmenu_objlist[i];
            objlist.makeItem = objlist_make;
            objlist.bindItem = objlist_bindItem;
            objlist.itemsSource = lstmenu_objlist;

            //ListView objlist = CreateVListView(lstmenu_objlist, objlistpanel, "objlist");
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
                HideMenu_OnClick();
            };

            Button btn_objlist_remove = objlistpanel.Q<Button>("btn_objlist_remove");
            btn_objlist_remove.clicked += btn_objlist_remove_OnClick;


            //---project panel set up
            GenerateProjectPanelUI();
        }

        /// <summary>
        /// Set up ListView element.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="parentObject"></param>
        /// <param name="objectName"></param>
        /// <returns></returns>
        protected ListView CreateVListView(List<string> items, VisualElement parentObject, string objectName)
        {
            ListView list = parentObject.Q<ListView>(objectName);

            System.Func<VisualElement> objlist_make = () => new Label();
            System.Action<VisualElement, int> objlist_bindItem = (e, i) => (e as Label).text = items[i];
            list.makeItem = objlist_make;
            list.bindItem = objlist_bindItem;
            list.itemsSource = items;

            return list;
        }
        void AddListViewItem(ListView parentObject, string item)
        {

            parentObject.itemsSource.Add(item);
            System.Action<VisualElement, int> objlist_bindItem = (e, i) => (e as Label).text = (string)parentObject.itemsSource[i];
            parentObject.bindItem = objlist_bindItem;
        }
        void ShowHideToolbarMenu(VisualElement panel)
        {
            if (panel.style.visibility == Visibility.Visible)
            {
                panel.style.visibility = Visibility.Hidden;
            }
            else //if((panel.style.visibility == null) || (panel.style.visibility == Visibility.Hidden))
            {
                panel.style.visibility = Visibility.Visible;
            }
             
        }

        //===============================================================================================
        // menunew

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

        void Newobj_Blank_OnClick(UserPrimitiveType type)
        {
            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();
            fmc.CreateBlankCube(type);

            Newobj_OnClick();
        }

        void HideMenu_OnClick()
        {
            VisualElement panel = rootElement.Q<VisualElement>("objlistpanel");
            //ShowHideToolbarMenu(panel);
            panel.style.display = (panel.style.display == DisplayStyle.Flex) ? DisplayStyle.None : DisplayStyle.Flex;
        }
        void OpenProj_OnClick()
        {
            Label lab_proj_title = rootElement.Q<Label>("lab_proj_title");
            Label lab_proj_description = rootElement.Q<Label>("lab_proj_description");
            Label lab_proj_license = rootElement.Q<Label>("lab_proj_license");
            Label lab_proj_url = rootElement.Q<Label>("lab_proj_url");


            //------
            string[] textext = new string[1] { "vvmproj" };
            ExtensionFilter[] ext = new ExtensionFilter[] {
                new ExtensionFilter("All File", textext),
                //new ExtensionFilter("text File", "txt"),
                //new ExtensionFilter("json File", "json"),
                new ExtensionFilter("project file", "vvmproj")
            };

            ListView rolelist = rootElement.Q<ListView>("proj_rolelist");

            IList<ItemWithStream> paths = StandaloneFileBrowser.OpenFilePanel("Select project file", "", ext, false);
            string ret = "";
            for (int i = 0; i < paths.Count; i++)
            {
                Stream stm = paths[i].OpenStream();
                using (var fs = new StreamReader(stm))
                {
                    ret = fs.ReadToEnd();

                    manim.LoadProject(ret);

                    lab_proj_title.text = manim.currentProject.meta.name;
                    lab_proj_description.text = manim.currentProject.meta.description;
                    lab_proj_license.text = manim.currentProject.meta.license;
                    lab_proj_url.text = manim.currentProject.meta.referURL;

                    manim.currentProject.casts.ForEach(item =>
                    {
                        if (
                            (item.type != AF_TARGETTYPE.SystemEffect) && (item.type != AF_TARGETTYPE.Audio) &&
                            (item.type != AF_TARGETTYPE.Stage)
                        )
                        {
                            AddListViewItem(rolelist, item.roleTitle);
                            lstproject_rolestrlist_back.Add(item.roleName);
                        }
                        
                    });
                    
                }
            }
        }
        void ActorSetting_OnClick()
        {
            //VisualElement panel = rootElement.Q<VisualElement>("testmenupanel");
            //ShowHideToolbarMenu(panel);
            //GameObject animatearea = GameObject.Find("AnimateArea");
            //UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();
            //StartCoroutine(fmc.DownloadAAS("default"));

            VisualElement projectpanel = rootElement.Q<VisualElement>("projectpanel");
            if (projectpanel.style.display == DisplayStyle.Flex)
            {
                projectpanel.style.display = DisplayStyle.None;
            }
            else
            {
                projectpanel.style.display = DisplayStyle.Flex;
            }
            
        }
        void PlayAnimation_OnClick()
        {
            AnimationParsingOptions apo = new AnimationParsingOptions();
            apo.isExecuteForDOTween = 1;
            apo.isBuildDoTween = 1;
            apo.isCompileAnimation = 1;
            apo.isLoop = 0;
            apo.endDelay = 1.5f;

            string opt = JsonUtility.ToJson(apo);
            manim.StartAllTimeline(opt);
        }
        void PauseAnimation_OnClick()
        {
            manim.PauseAllTimeline();
        }
        void StopAnimation_OnClick()
        {
            //manim.StopAllTimeline();
            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();
            StartCoroutine(fmc.ListGetAAS("effect"));

        }
        async void selectwater_OnClick()
        {
            OperateStage os = GameObject.Find("Stage").GetComponent<OperateStage>();
            os.SelectStage((int)StageKind.BasicSeaLevel);
            //os.SelectStage((int)StageKind.SeaNight);
            os.ListGetOneUserMaterial("");

        }
        void wateropt1_OnClick()
        {
            OperateActiveVRM oav =  GameObject.Find("IKHandleParent").GetComponent<OperateActiveVRM>();
            OperateLoadedVRM ovrm = oav.ActiveAvatar.GetComponent<OperateLoadedVRM>();
            OperateStage os = GameObject.Find("Stage").GetComponent<OperateStage>();

            ManageAvatarTransform mat = oav.ActiveAvatar.GetComponent<ManageAvatarTransform>();
            /*
            Label datatext = rootElement.Q<Label>("dataText");
            string data = datatext.text;
            mat.SetIKTransformAll2(data);
            return;
            */

            

            //os.SetUserMaterial("wavefrequency,1\t1\t1\t1");
            /*
            AnimationRegisterOptions aro = new AnimationRegisterOptions();
            aro.index = 17;
            aro.targetRole = "SystemEffect";
            aro.targetType = AF_TARGETTYPE.SystemEffect;
            aro.duration = 0.5f;

            string js = JsonUtility.ToJson(aro);
            manim.SetDuration(js);
            */

            /*
            //ovrm.SetGravityPower(",tail01,1.0");
            //oav.changeAvatarBlendShapeByName("Fcl_MTH_U,42");

            //ovrm.SetHandFingerMode("f");
            ovrm.LeftHandCtrl.currentPose = -1;
            ovrm.RotateFinger("l", "i", new float[] { 0, 0, 0.50f, 0.50f, 0.50f});
            */

            Transform cube = GameObject.Find("Cube").transform;
            Vector3 tmpsp1 = GameObject.Find("tmpsp1").transform.position; // new Vector3(0.1146f, 0.0027f, -0.0174f);
            Vector3 tmpsp2 = GameObject.Find("tmpsp2").transform.position; //new Vector3(-0.1146f, -0.0031f, 0.0183f);
            Vector3 tmpsp2_1 = new Vector3(tmpsp2.x, tmpsp2.y, tmpsp1.z);

            GameObject.Find("tmpsp3").transform.position = (tmpsp1 + tmpsp2) / 2;
            //GameObject.Find("tmpsp3").transform.Translate(Vector3.forward);
            Vector3 tmpsp3 = GameObject.Find("tmpsp3").transform.position;

            Vector3 sa_21 = (tmpsp2 - tmpsp1);
            Vector3 sa_211 = (tmpsp2_1 - tmpsp1);
            Vector3 diff = tmpsp1 - tmpsp2;
            float aangle = Vector3.Angle(tmpsp1, tmpsp2);
            float angle_x = Mathf.Atan2(Mathf.Abs(tmpsp1.x), Mathf.Abs(tmpsp2.x));
            float angle_y = Mathf.Atan2(Mathf.Abs(tmpsp1.y), Mathf.Abs(tmpsp2.y));
            float angle_z = Mathf.Atan2(Mathf.Abs(tmpsp1.z), Mathf.Abs(tmpsp2.z));

            float angle_z2y = Mathf.Atan2(tmpsp2.z - tmpsp1.z, tmpsp2.x - tmpsp1.x) * Mathf.Rad2Deg * -1;
            float angle_y2z = Mathf.Atan2(tmpsp2.y - tmpsp1.y, tmpsp2.x - tmpsp1.x) * Mathf.Rad2Deg;
            Debug.Log(aangle);
            Debug.Log(diff);
            Debug.Log(angle_x.ToString() + "/" + angle_y.ToString() + "/" + angle_z.ToString());
            
            float rad = aangle * Mathf.Deg2Rad;
            //Vector3 vec = new Vector3(0, Mathf.Cos(rad) * Mathf.Rad2Deg * -1, Mathf.Sin(rad) * Mathf.Rad2Deg * -1);
            Vector3 vec = ManageAvatarTransform.GetTwoPointAngleDirection(tmpsp1, tmpsp2, Vector3.right); //new Vector3(0f, angle_z2y, angle_y2z);
            Debug.Log(vec);
            
            
            Quaternion nq = Quaternion.LookRotation(tmpsp2- tmpsp1);
            Quaternion q_for = Quaternion.FromToRotation(tmpsp1, tmpsp2);
            Debug.Log("LookAt=");
            Debug.Log(nq.eulerAngles);
            Debug.Log("FromTo=");
            Debug.Log(q_for);
            //cube.rotation = nq;
            //cube.rotation = q_for;
            cube.rotation = Quaternion.Euler(vec);

            GameObject.Find("tmpsp3").transform.rotation = Quaternion.Euler(vec);
            GameObject.Find("tmpsp3").transform.Translate(Vector3.forward);
        }

        //===============================================================================================
        // objlist
        void btn_objlist_remove_OnClick()
        {
            //VisualElement objlistpanel = rootElement.Q<VisualElement>("objlistpanel");
            //ListView objlist = objlistpanel.Q<ListView>("objlist");

            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();
            fmc.OnClickRemoveObject();


        }
        public void objlist_add_item(string item)
        {
            VisualElement objlistpanel = rootElement.Q<VisualElement>("objlistpanel");
            ListView objlist = objlistpanel.Q<ListView>("objlist");

            objlist.itemsSource.Add(item);
            System.Action<VisualElement, int> objlist_bindItem = (e, i) => (e as Label).text = lstmenu_objlist[i];
            objlist.bindItem = objlist_bindItem;
            //AddListViewItem(objlist, item);
            Debug.Log(objlist.itemsSource);
            Debug.Log(item);
        }
        public void objlist_del_item(int index)
        {
            VisualElement objlistpanel = rootElement.Q<VisualElement>("objlistpanel");
            ListView objlist = objlistpanel.Q<ListView>("objlist");

            objlist.itemsSource.RemoveAt(index);
            System.Action<VisualElement, int> objlist_bindItem = (e, i) => (e as Label).text = lstmenu_objlist[i];
            objlist.bindItem = objlist_bindItem;
        }

        //===============================================================================================
        // project panel
        void GenerateProjectPanelUI()
        {
            VisualElement projectpanel = rootElement.Q<VisualElement>("projectpanel");

            Button btn_proj_tabinfo = projectpanel.Q<Button>("btn_proj_tabinfo");
            btn_proj_tabinfo.clicked += btn_proj_tabinfo_OnClick;
            Button btn_proj_tabrole = projectpanel.Q<Button>("btn_proj_tabrole");
            btn_proj_tabrole.clicked += btn_proj_tabrole_OnClick;
            Button btn_proj_tabmaterial = projectpanel.Q<Button>("btn_proj_tabmaterial");
            btn_proj_tabmaterial.clicked += btn_proj_tabmaterial_OnClick;


            lstproject_rolestrlist = new List<string>();
            lstproject_rolestrlist_back = new List<string>();
            ListView lv = CreateVListView(lstproject_rolestrlist, projectpanel, "proj_rolelist");
            lv.onSelectedIndicesChange += inx =>
            {
                IEnumerator iem = inx.GetEnumerator();
                while (iem.MoveNext())
                {
                    int curinx = (int)iem.Current;
                    Debug.Log(iem.Current);
                    Debug.Log(lstproject_rolestrlist_back[curinx]);

                    NativeAnimationAvatar nav = manim.GetCastInProject(lstproject_rolestrlist_back[curinx]);
                    currentSelectedRole = nav;

                    TextField roletitle = projectpanel.Q<TextField>("proj_input_roletitle");
                    TextField roletype = projectpanel.Q<TextField>("proj_input_type");
                    TextField rolecast = projectpanel.Q<TextField>("proj_input_cast");
                    roletitle.value = nav.roleTitle;
                    roletype.value = nav.type.ToString();
                    if (nav.avatar != null) rolecast.value = nav.avatar.name;

                    VisualElement eachpanelLight = projectpanel.Q<VisualElement>("proj_eachpanel_light");
                    VisualElement eachpanelOO = projectpanel.Q<VisualElement>("proj_eachpanel_otherobject");
                    Button proj_btn_load_obj = projectpanel.Q<Button>("proj_btn_load_obj");

                    eachpanelLight.style.display = DisplayStyle.None;
                    eachpanelOO.style.display = DisplayStyle.None;
                    //proj_btn_load_obj.style.display = DisplayStyle.None;
                    if (nav.type == AF_TARGETTYPE.Light)
                    {   
                        eachpanelLight.style.display = DisplayStyle.Flex;
                    }
                    else if (nav.type == AF_TARGETTYPE.OtherObject)
                    {
                        eachpanelOO.style.display = DisplayStyle.Flex;
                    }
                    if (
                        (nav.type == AF_TARGETTYPE.VRM) || (nav.type == AF_TARGETTYPE.OtherObject) ||
                        (nav.type == AF_TARGETTYPE.Image) || (nav.type == AF_TARGETTYPE.UImage)
                    )
                    {
                        
                        proj_btn_load_obj.style.display = DisplayStyle.Flex;

                    }
                }
            };

            Button proj_btn_load_blankobject = projectpanel.Q<Button>("proj_btn_load_blankobject");
            proj_btn_load_blankobject.clicked += proj_btn_load_blankobject_OnClick;

            Button proj_btn_load_obj = projectpanel.Q<Button>("proj_btn_load_obj");
            proj_btn_load_obj.clicked += proj_btn_load_obj_OnClick;

            Button proj_btn_set_object = projectpanel.Q<Button>("proj_btn_set_object");
            proj_btn_set_object.clicked += proj_btn_set_object_OnClick;
        }
        void btn_proj_tabinfo_OnClick()
        {
            VisualElement projectpanel = rootElement.Q<VisualElement>("projectpanel");

            VisualElement tabinfo = projectpanel.Q<VisualElement>("projtab_info");
            VisualElement tabrole = projectpanel.Q<VisualElement>("projtab_role");
            VisualElement tabmate = projectpanel.Q<VisualElement>("projtab_material");
            tabinfo.style.display = DisplayStyle.Flex;
            tabrole.style.display = DisplayStyle.None;
            tabmate.style.display = DisplayStyle.None;
        }
        void btn_proj_tabrole_OnClick()
        {
            VisualElement projectpanel = rootElement.Q<VisualElement>("projectpanel");

            VisualElement tabinfo = projectpanel.Q<VisualElement>("projtab_info");
            VisualElement tabrole = projectpanel.Q<VisualElement>("projtab_role");
            VisualElement tabmate = projectpanel.Q<VisualElement>("projtab_material");
            tabinfo.style.display = DisplayStyle.None;
            tabrole.style.display = DisplayStyle.Flex;
            tabmate.style.display = DisplayStyle.None;
        }
        void btn_proj_tabmaterial_OnClick()
        {
            VisualElement projectpanel = rootElement.Q<VisualElement>("projectpanel");

            VisualElement tabinfo = projectpanel.Q<VisualElement>("projtab_info");
            VisualElement tabrole = projectpanel.Q<VisualElement>("projtab_role");
            VisualElement tabmate = projectpanel.Q<VisualElement>("projtab_material");
            tabinfo.style.display = DisplayStyle.None;
            tabrole.style.display = DisplayStyle.None;
            tabmate.style.display = DisplayStyle.Flex;
        }

        void proj_btn_load_blankobject_OnClick()
        {
            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();

            DropdownField drop = rootElement.Q<DropdownField>("proj_cmb_blankobject");
            if (drop.index > 0)
            {
                NativeAnimationAvatar nav = fmc.CreateBlankObject(drop.index - 1);
                manim.AttachAvatarToRole(currentSelectedRole.roleName + "," + nav.avatar.name);
                manim.DetachAvatarFromRole(nav.roleName + ",role");
            }
            
        }
        void proj_btn_set_object_OnClick()
        {
            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();
            NativeAnimationAvatar nav = null;

            VisualElement projectpanel = rootElement.Q<VisualElement>("projectpanel");
            TextField rolecast = projectpanel.Q<TextField>("proj_input_cast");

            if (currentSelectedRole.type == AF_TARGETTYPE.VRM)
            {
                nav = fmc.LastLoaded;
                rolecast.SetValueWithoutNotify(nav.avatarTitle);
            }
            else if (currentSelectedRole.type == AF_TARGETTYPE.OtherObject)
            {
                nav = fmc.LastLoaded;
                rolecast.SetValueWithoutNotify(nav.avatarTitle);
            }
            else if (currentSelectedRole.type == AF_TARGETTYPE.Light)
            {
                DropdownField drop = rootElement.Q<DropdownField>("proj_cmb_lighttype");
                nav = fmc.OpenLightObject(drop.value);
            }
            else if (currentSelectedRole.type == AF_TARGETTYPE.Camera)
            {
                nav = fmc.CreateCameraObject("");
            }
            else if (currentSelectedRole.type == AF_TARGETTYPE.Effect)
            {
                nav = fmc.CreateSingleEffect();
            }
            else if (currentSelectedRole.type == AF_TARGETTYPE.Image)
            {
                nav = fmc.LastLoaded;
            }
            else if (currentSelectedRole.type == AF_TARGETTYPE.UImage)
            {
                nav = fmc.LastLoaded;
            }
            else if (currentSelectedRole.type == AF_TARGETTYPE.Text)
            {
                nav = fmc.OpenText(",tl");
            }

            //manim.DetachAvatarFromRole(nav.roleName + ",role");
            manim.AttachAvatarToRole(currentSelectedRole.roleName + "," + nav.avatar.name);
            
        }
        void proj_btn_load_obj_OnClick()
        {
            GameObject animatearea = GameObject.Find("AnimateArea");
            UserVRMSpace.FileMenuCommands fmc = animatearea.GetComponent<UserVRMSpace.FileMenuCommands>();

            if (currentSelectedRole.type == AF_TARGETTYPE.VRM)
            {
                fmc.OnButtonPointerDown();
            }
            else if (currentSelectedRole.type == AF_TARGETTYPE.OtherObject)
            {
                fmc.OnBtnLoadObjPointerDown();
            }
            else if (currentSelectedRole.type == AF_TARGETTYPE.Image)
            {
                fmc.OnClickBtnImage();
            }
            else if (currentSelectedRole.type == AF_TARGETTYPE.UImage)
            {
                fmc.OnClickBtnUImage();
            }

        }
    }
}

