using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

using UserHandleSpace;

namespace UserUISpace
{
    public class UserUIARWin : MonoBehaviour
    {
        [SerializeField]
        GameObject cameraset;

        [SerializeField]
        protected ManageAnimation manim;

        [SerializeField]
        CameraManagerXR cameraManager;

        [SerializeField]
        Camera arcam;

        [SerializeField]
        ScreenShot sshot;

        private UIDocument MainUI;
        private VisualElement rootElement;
        private VisualElement NamePanel;
        private VisualElement LeftPanel;
        private VisualElement RightPanel;
        private VisualElement MiniPanel;
        private VisualElement BonePanel;
        private VisualElement VBTabPanel;
        private VisualElement VPBonePanel;
        private VisualElement VPExpressionPanel;
        private VisualElement VPHandPanel;
        private VisualElement VPAnimPanel;
        private VisualElement pnlc_panel1;
        private Label arlab_selectavatar;
        private Label arlab_keynumber;
        private VisualElement RotatePopupPanel;

        private ARMenuFunction armf;

        private bool showik_state;
        private bool cutout_state;
        private bool is_minimize = false;
        private bool pushed_tp_right = false;
        private bool pushed_tp_left = false;
        private bool pushed_tp_up = false;
        private bool pushed_tp_down = false;
        private bool pushed_tp_forward = false;
        private bool pushed_tp_back = false;

        private string pushed_blendshape_btn = "";
        private float pushed_blendshape_dir = 0f;
        private string pushed_blendshape_label = "";

        private Coroutine m_hideLabelCoroutine;


        readonly List<string> poselist = new List<string> { "slid_lefthand_normal", "slid_lefthand_close", "slid_lefthand_pointing", "slid_lefthand_vsign", "slid_lefthand_thumbup", "slid_lefthand_grip" };

        // Start is called before the first frame update
        void Start()
        {
            MainUI = GetComponent<UIDocument>();
            rootElement = MainUI.rootVisualElement;


            armf = new ARMenuFunction(manim, manim.ikArea.GetComponent<OperateActiveVRM>(), cameraManager);
            armf.cameraset = cameraset;
            GenerateUI();

            showik_state = true;
            cutout_state = false;

            ShowUI(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (pushed_tp_right) armf.TargetOperationBody("tp_right");
            if (pushed_tp_left) armf.TargetOperationBody("tp_left");
            if (pushed_tp_up) armf.TargetOperationBody("tp_up");
            if (pushed_tp_down) armf.TargetOperationBody("tp_down");
            if (pushed_tp_forward) armf.TargetOperationBody("tp_forward");
            if (pushed_tp_back) armf.TargetOperationBody("tp_back");

            if (pushed_blendshape_btn != "")
            {
                Label lab = VPExpressionPanel.Q<Label>(pushed_blendshape_label);
                if (lab != null)
                {
                    float val = float.Parse(lab.text);
                    val += pushed_blendshape_dir;
                    if (val < 0) val = 0f;
                    if (val > 100f) val = 100f;

                    lab.text = val.ToString();
                    armf.SetVRMExpression(lab.tooltip, val * 0.01f);
                    Toggle tgl = VPExpressionPanel.Q<Toggle>(pushed_blendshape_label.Replace("lab_", "tgl_"));
                    if (tgl != null)
                    {
                        tgl.value = true;
                    }
                }
                
            }
            

        }
        private void FixedUpdate()
        {

            if (cameraManager.isActiveNormal() == true)
            {
                ShowButtonOtherThanVRM(false);
            }

        }

        void GenerateUI()
        {
            NamePanel = rootElement.Q<VisualElement>("panel_curavatar");
            LeftPanel = rootElement.Q<VisualElement>("panel_operate");
            RightPanel = rootElement.Q<VisualElement>("panel_transform");
            MiniPanel = rootElement.Q<VisualElement>("panel_minimize");
            BonePanel = rootElement.Q<VisualElement>("panel_vrm");
            arlab_selectavatar = NamePanel.Q<Label>("arlab_selectavatar");
            arlab_keynumber = LeftPanel.Q<Label>("arlab_keynumber");

            //---NamePanel
            //NamePanel.RegisterCallback<PointerDownEvent>(arbtn_namepanel_OnClick);
            pnlc_panel1 = NamePanel.Q<VisualElement>("pnlc_panel1");
            NamePanel.Q<Button>("arbtn_showselect").clicked += arbtn_namepanel_OnClick;

            //---MiniPanel
            MiniPanel.Q<Button>("arbtn_folding").clicked += arbtn_folding_OnClick;
            //MiniPanel.Q<Button>("arbtn_motplay2").clicked += arbtn_motplay_OnClick;
            //MiniPanel.Q<Button>("arbtn_motstop2").clicked += arbtn_motstop_OnClick;
            MiniPanel.Q<Button>("arbtn_leftpanel").clicked += arbtn_leftpanel_OnClick;
            MiniPanel.Q<Button>("arbtn_rightpanel").clicked += arbtn_rightpanel_OnClick;

            //---Left: operate panel
            LeftPanel.Q<Button>("arbtn_prevselobj").clicked += arbtn_prevselobj_OnClick;
            LeftPanel.Q<Button>("arbtn_nextselobj").clicked += arbtn_nextselobj_OnClick;
            LeftPanel.Q<Button>("arbtn_showik").clicked += arbtn_showik_OnClick;
            LeftPanel.Q<Button>("arbtn_oncutout").clicked += arbtn_oncutout_OnClick;
            LeftPanel.Q<Button>("arbtn_enabletap").clicked += arbtn_enabletap_OnClick;
            LeftPanel.Q<Button>("arbtn_globallocal").clicked += arbtn_globallocal_OnClick;
            LeftPanel.Q<Button>("arbtn_prevkeyframe").clicked += arbtn_prevkeyframe_OnClick;
            LeftPanel.Q<Button>("arbtn_nextkeyframe").clicked += arbtn_nextkeyframe_OnClick;
            LeftPanel.Q<Button>("arbtn_regkeyframe").clicked += regkeyframe_OnClick;
            LeftPanel.Q<Button>("arbtn_motplay").clicked += arbtn_motplay_OnClick;
            LeftPanel.Q<Button>("arbtn_motstop").clicked += arbtn_motstop_OnClick;

            //---Right: transform panel
            RightPanel.Q<Button>("arbtn_mt_camera").clicked += arbtn_mt_camera_OnClick;
            RightPanel.Q<Button>("arbtn_mt_obj").clicked += arbtn_mt_obj_OnClick;
            RightPanel.Q<Button>("arbtn_mt_vrmbone").clicked += arbtn_mt_vrmbone_OnClick;
            RightPanel.Q<Button>("arbtn_obj_move").clicked += arbtn_obj_move_OnClick;
            RightPanel.Q<Button>("arbtn_obj_rot").clicked += arbtn_obj_rot_OnClick;
            RightPanel.Q<Button>("arbtn_obj_size").clicked += arbtn_obj_size_OnClick;
            RightPanel.Q<Button>("arbtn_reset_move").clicked += arbtn_reset_move_OnClick;
            RightPanel.Q<Button>("arbtn_reset_tpose").clicked += arbtn_reset_tpose_OnClick;
            RightPanel.Q<Button>("arbtn_reset_size").clicked += arbtn_reset_size_OnClick;

            //---X+
            string[] tmpxs = { "tpbt_xplus", "tpbr_xplus", "tpbs_xplus" };
            for (int i = 0; i < tmpxs.Length; i++)
            {
                RightPanel.Q<Button>(tmpxs[i]).RegisterCallback<PointerUpEvent>(tp_right_PointerUpEvent, TrickleDown.TrickleDown);
                RightPanel.Q<Button>(tmpxs[i]).RegisterCallback<PointerDownEvent>(tp_right_PointerDownEvent, TrickleDown.TrickleDown);
                RightPanel.Q<Button>(tmpxs[i]).RegisterCallback<PointerLeaveEvent>(tp_right_PointerLeaveEvent, TrickleDown.TrickleDown);
            }
            /*RightPanel.Q<Button>("tpbt_xplus").RegisterCallback<PointerUpEvent>(tp_right_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_xplus").RegisterCallback<PointerDownEvent>(tp_right_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_xplus").RegisterCallback<PointerLeaveEvent>(tp_right_PointerLeaveEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_xplus").RegisterCallback<PointerUpEvent>(tp_right_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_xplus").RegisterCallback<PointerDownEvent>(tp_right_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_xplus").RegisterCallback<PointerUpEvent>(tp_right_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_xplus").RegisterCallback<PointerDownEvent>(tp_right_PointerDownEvent, TrickleDown.TrickleDown);*/
            //---X-
            string[] tmpxs2 = { "tpbt_xmin", "tpbr_xmin", "tpbs_xmin" };
            for (int i = 0; i < tmpxs2.Length; i++)
            {
                RightPanel.Q<Button>(tmpxs2[i]).RegisterCallback<PointerUpEvent>(tp_left_PointerUpEvent, TrickleDown.TrickleDown);
                RightPanel.Q<Button>(tmpxs2[i]).RegisterCallback<PointerDownEvent>(tp_left_PointerDownEvent, TrickleDown.TrickleDown);
                RightPanel.Q<Button>(tmpxs2[i]).RegisterCallback<PointerLeaveEvent>(tp_left_PointerLeaveEvent, TrickleDown.TrickleDown);
            }
            /*RightPanel.Q<Button>("tpbt_xmin").RegisterCallback<PointerUpEvent>(tp_left_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_xmin").RegisterCallback<PointerDownEvent>(tp_left_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_xmin").RegisterCallback<PointerLeaveEvent>(tp_left_PointerLeaveEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_xmin").RegisterCallback<PointerUpEvent>(tp_left_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_xmin").RegisterCallback<PointerDownEvent>(tp_left_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_xmin").RegisterCallback<PointerUpEvent>(tp_left_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_xmin").RegisterCallback<PointerDownEvent>(tp_left_PointerDownEvent, TrickleDown.TrickleDown);*/
            //---Y+
            string[] tmpys = { "tpbt_yplus", "tpbr_yplus", "tpbs_yplus" };
            for (int i = 0; i < tmpys.Length; i++)
            {
                RightPanel.Q<Button>(tmpys[i]).RegisterCallback<PointerUpEvent>(tp_up_PointerUpEvent, TrickleDown.TrickleDown);
                RightPanel.Q<Button>(tmpys[i]).RegisterCallback<PointerDownEvent>(tp_up_PointerDownEvent, TrickleDown.TrickleDown);
                RightPanel.Q<Button>(tmpys[i]).RegisterCallback<PointerLeaveEvent>(tp_up_PointerLeaveEvent, TrickleDown.TrickleDown);
            }
            /*RightPanel.Q<Button>("tpbt_yplus").RegisterCallback<PointerUpEvent>(tp_up_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_yplus").RegisterCallback<PointerDownEvent>(tp_up_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_yplus").RegisterCallback<PointerLeaveEvent>(tp_up_PointerLeaveEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_yplus").RegisterCallback<PointerUpEvent>(tp_up_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_yplus").RegisterCallback<PointerDownEvent>(tp_up_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_yplus").RegisterCallback<PointerUpEvent>(tp_up_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_yplus").RegisterCallback<PointerDownEvent>(tp_up_PointerDownEvent, TrickleDown.TrickleDown);*/
            //---Y-
            string[] tmpys2 = { "tpbt_ymin", "tpbr_ymin", "tpbs_ymin" };
            for (int i = 0; i < tmpys2.Length; i++)
            {
                RightPanel.Q<Button>(tmpys2[i]).RegisterCallback<PointerUpEvent>(tp_down_PointerUpEvent, TrickleDown.TrickleDown);
                RightPanel.Q<Button>(tmpys2[i]).RegisterCallback<PointerDownEvent>(tp_down_PointerDownEvent, TrickleDown.TrickleDown);
                RightPanel.Q<Button>(tmpys2[i]).RegisterCallback<PointerLeaveEvent>(tp_down_PointerLeaveEvent, TrickleDown.TrickleDown);
            }
            /*RightPanel.Q<Button>("tpbt_ymin").RegisterCallback<PointerUpEvent>(tp_down_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_ymin").RegisterCallback<PointerDownEvent>(tp_down_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_ymin").RegisterCallback<PointerLeaveEvent>(tp_down_PointerLeaveEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_ymin").RegisterCallback<PointerUpEvent>(tp_down_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_ymin").RegisterCallback<PointerDownEvent>(tp_down_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_ymin").RegisterCallback<PointerUpEvent>(tp_down_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_ymin").RegisterCallback<PointerDownEvent>(tp_down_PointerDownEvent, TrickleDown.TrickleDown);*/
            //---Z+
            string[] tmpzs = { "tpbt_zplus", "tpbr_zplus", "tpbs_zplus" };
            for (int i = 0; i < tmpzs.Length; i++)
            {
                RightPanel.Q<Button>(tmpzs[i]).RegisterCallback<PointerUpEvent>(tp_forward_PointerUpEvent, TrickleDown.TrickleDown);
                RightPanel.Q<Button>(tmpzs[i]).RegisterCallback<PointerDownEvent>(tp_forward_PointerDownEvent, TrickleDown.TrickleDown);
                RightPanel.Q<Button>(tmpzs[i]).RegisterCallback<PointerLeaveEvent>(tp_forward_PointerLeaveEvent, TrickleDown.TrickleDown);
            }
            /*RightPanel.Q<Button>("tpbt_zplus").RegisterCallback<PointerUpEvent>(tp_forward_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_zplus").RegisterCallback<PointerDownEvent>(tp_forward_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_zplus").RegisterCallback<PointerLeaveEvent>(tp_forward_PointerLeaveEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_zplus").RegisterCallback<PointerUpEvent>(tp_forward_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_zplus").RegisterCallback<PointerDownEvent>(tp_forward_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_zplus").RegisterCallback<PointerUpEvent>(tp_forward_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_zplus").RegisterCallback<PointerDownEvent>(tp_forward_PointerDownEvent, TrickleDown.TrickleDown);*/
            //---Z-
            string[] tmpzs2 = { "tpbt_zmin", "tpbr_zmin", "tpbs_zmin" };
            for (int i = 0; i < tmpzs2.Length; i++)
            {
                RightPanel.Q<Button>(tmpzs2[i]).RegisterCallback<PointerUpEvent>(tp_back_PointerUpEvent, TrickleDown.TrickleDown);
                RightPanel.Q<Button>(tmpzs2[i]).RegisterCallback<PointerDownEvent>(tp_back_PointerDownEvent, TrickleDown.TrickleDown);
                RightPanel.Q<Button>(tmpzs2[i]).RegisterCallback<PointerLeaveEvent>(tp_back_PointerLeaveEvent, TrickleDown.TrickleDown);
            }
            /*RightPanel.Q<Button>("tpbt_zmin").RegisterCallback<PointerUpEvent>(tp_back_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_zmin").RegisterCallback<PointerDownEvent>(tp_back_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbt_zmin").RegisterCallback<PointerLeaveEvent>(tp_back_PointerLeaveEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_zmin").RegisterCallback<PointerUpEvent>(tp_back_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbr_zmin").RegisterCallback<PointerDownEvent>(tp_back_PointerDownEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_zmin").RegisterCallback<PointerUpEvent>(tp_back_PointerUpEvent, TrickleDown.TrickleDown);
            RightPanel.Q<Button>("tpbs_zmin").RegisterCallback<PointerDownEvent>(tp_back_PointerDownEvent, TrickleDown.TrickleDown);*/

            //---BonePanel
            VBTabPanel = BonePanel.Q<VisualElement>("pnlv_tabpanel");
            VPBonePanel = BonePanel.Q<VisualElement>("pnlv_bones");
            VPExpressionPanel = BonePanel.Q<VisualElement>("pnlv_expression");
            VPHandPanel = BonePanel.Q<VisualElement>("pnlv_hand");
            VPAnimPanel = BonePanel.Q<VisualElement>("pnlv_playanim");

            VBTabPanel.Q<Button>("arbtn_vrm_bones").clicked += arbtn_vrm_bones_OnClick;
            VBTabPanel.Q<Button>("arbtn_vrm_blendshapes").clicked += arbtn_vrm_blendshapes_OnClick;
            VBTabPanel.Q<Button>("arbtn_vrm_hand").clicked += arbtn_vrm_hand_OnClick;
            VBTabPanel.Q<Button>("arbtn_vrm_playanim").clicked += arbtn_vrm_playanim_OnClick;

            //BonePanel.Q<Button>("arbtn_bn_ikparent").clicked += arbtn_bn_ikparent_OnClick;
            BonePanel.Q<Button>("arbtn_bn_head").clicked += arbtn_bn_head_OnClick;
            BonePanel.Q<Button>("arbtn_bn_eyeviewhandle").clicked += arbtn_bn_eyeviewhandle_OnClick;
            BonePanel.Q<Button>("arbtn_bn_lookat").clicked += arbtn_bn_lookat_OnClick;
            BonePanel.Q<Button>("arbtn_bn_chest").clicked += arbtn_bn_chest_OnClick;
            BonePanel.Q<Button>("arbtn_bn_aim").clicked += arbtn_bn_aim_OnClick;
            BonePanel.Q<Button>("arbtn_bn_rightshoulder").clicked += arbtn_bn_rightshoulder_OnClick;
            BonePanel.Q<Button>("arbtn_bn_leftshoulder").clicked += arbtn_bn_leftshoulder_OnClick;
            BonePanel.Q<Button>("arbtn_bn_rightlowerarm").clicked += arbtn_bn_rightlowerarm_OnClick;
            BonePanel.Q<Button>("arbtn_bn_leftlowerarm").clicked += arbtn_bn_leftlowerarm_OnClick;
            BonePanel.Q<Button>("arbtn_bn_pelvis").clicked += arbtn_bn_pelvis_OnClick;
            BonePanel.Q<Button>("arbtn_bn_leftarm").clicked += arbtn_bn_lefthand_OnClick;
            BonePanel.Q<Button>("arbtn_bn_rightarm").clicked += arbtn_bn_righthand_OnClick;
            BonePanel.Q<Button>("arbtn_bn_leftlowerleg").clicked += arbtn_bn_leftlowerleg_OnClick;
            BonePanel.Q<Button>("arbtn_bn_rightlowerleg").clicked += arbtn_bn_rightlowerleg_OnClick;
            BonePanel.Q<Button>("arbtn_bn_leftleg").clicked += arbtn_bn_leftleg_OnClick;
            BonePanel.Q<Button>("arbtn_bn_rightleg").clicked += arbtn_bn_rightleg_OnClick;
            BonePanel.Q<Button>("arbtn_bn_lefttoes").clicked += arbtn_bn_lefttoes_OnClick;
            BonePanel.Q<Button>("arbtn_bn_righttoes").clicked += arbtn_bn_righttoes_OnClick;

            for (int i = 0; i < poselist.Count; i++)
            {
                string poselist_item = poselist[i];

                VPHandPanel.Q<Slider>(poselist_item).RegisterValueChangedCallback((evt) =>
                {
                    Slider sl = evt.target as Slider;
                    if (sl != null)
                    {
                        poselist.ForEach((item) =>
                        {
                            Slider tsl = VPHandPanel.Q<Slider>(item);
                            if ((tsl != null) && (sl.name != item))
                            {
                                tsl.value = 0;
                            }
                        });
                        int ishit = poselist.FindIndex(match =>
                        {
                            if (match == sl.name) return true;
                            return false;
                        });
                        if (ishit > -1)
                        {
                            armf.SetVRMHand("l", ishit, sl.value);
                        }


                    }
                });
            }
            for (int i = 0; i < poselist.Count; i++)
            {
                string poselist_item = poselist[i].Replace("left","right");

                VPHandPanel.Q<Slider>(poselist_item).RegisterValueChangedCallback((evt) =>
                {
                    Slider sl = evt.target as Slider;
                    if (sl != null)
                    {
                        poselist.ForEach((item) =>
                        {
                            string rightname = item.Replace("left", "right");
                            Slider tsl = VPHandPanel.Q<Slider>(rightname);
                            if ((tsl != null) && (sl.name != rightname))
                            {
                                tsl.value = 0;
                            }
                        });
                        int ishit = poselist.FindIndex(match =>
                        {
                            if (match.Replace("left","right") == sl.name) return true;
                            return false;
                        });
                        if (ishit > -1)
                        {
                            armf.SetVRMHand("r", ishit, sl.value);
                        }


                    }
                });
            }

            //---Rotate popup
            RotatePopupPanel = RightPanel.Q<VisualElement>("panel_rotatepopup");
            RotatePopupPanel.Q<Button>("axisX").clicked += panel_rotatepopup_axisX_OnClick;
            RotatePopupPanel.Q<Button>("axisY").clicked += panel_rotatepopup_axisY_OnClick;
            RotatePopupPanel.Q<Button>("axisZ").clicked += panel_rotatepopup_axisZ_OnClick;

            //---Other properties panel
            VPAnimPanel.Q<DropdownField>("cmb_playanimlist").RegisterValueChangedCallback(cmb_playanimlist_OnChange);
            VPAnimPanel.Q<Button>("arbtn_anim_play").clicked += arbtn_anim_play_OnClick;
            VPAnimPanel.Q<Button>("arbtn_anim_pause").clicked += arbtn_anim_pause_OnClick;
            VPAnimPanel.Q<Button>("arbtn_anim_stop").clicked += arbtn_anim_stop_OnClick;
            VPAnimPanel.Q<Toggle>("chk_anim_loop").RegisterValueChangedCallback(chk_anim_loop_OnChange);
            VPAnimPanel.Q<DropdownField>("cmb_playflag").RegisterValueChangedCallback(cmb_playflag_OnChange);
            VPAnimPanel.Q<Slider>("slid_playanim_seek").RegisterValueChangedCallback(slid_playanim_seek_OnChange);
            VPAnimPanel.Q<Slider>("slid_playanim_speed").RegisterValueChangedCallback(slid_playanim_speed_OnChange);
        }

        public void ShowUI(bool flag)
        {
            //rootElement.visible = flag;

            {
                LeftPanel.visible = flag;
                RightPanel.visible = flag;
                MiniPanel.visible = flag;
                if (flag == false)
                {
                    BonePanel.visible = flag;
                }
                NamePanel.visible = flag;
                pnlc_panel1.visible = flag;
            }

            //MiniPanel.Q<Button>("arbtn_motplay2").visible = flag;
            //MiniPanel.Q<Button>("arbtn_motstop2").visible = flag;
            MiniPanel.Q<Button>("arbtn_folding").visible = flag;

            RotatePopupPanel.visible = flag;
            is_minimize = false;
        }
        public void ShowVRMBonePanel(bool flag)
        {
            BonePanel.visible = flag;
        }
        public void ShowVRMButton(bool flag)
        {
            RightPanel.Q<Button>("arbtn_mt_vrmbone").visible = flag;
        }
        public void ShowRotateAxisPanel(bool flag)
        {
            RotatePopupPanel.visible = flag;
        }
        public void ShowButtonOtherThanVRM(bool flag)
        {
            if (flag == true)
            {
                RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Visible;
                RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Visible;
            }
            else
            {
                RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Hidden;
                RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Hidden;
            }
        }
        public void SetObjectTitle(string title)
        {
            arlab_selectavatar.text = title;
        }
        public void SetObjectInformation(string title, AF_TARGETTYPE type)
        {
            arlab_selectavatar.text = title;
            armf.SelectedAvatarType = type;
        }
        public void ReconstructHandList()
        {
            ScrollView sv = VPHandPanel.Q<ScrollView>();
            VisualElement svve = sv.Q<VisualElement>();
            if (sv != null)
            {
                //sv.Clear();
                List<string> arr = armf.GetVRMHand();
                

                foreach (string item in arr) 
                {
                    
                    string[] ss = item.Split(",");
                    int posetype = int.Parse(ss[1]);
                    float posevalue = float.Parse(ss[2]);

                    if (posetype > -1)
                    {
                        if (ss[0] == "left")
                        {
                            Slider slid = sv.Q<Slider>(poselist[posetype]);
                            if (slid != null)
                            {
                                slid.value = posevalue;
                            }
                    }
                        else if (ss[0] == "right")
                        {
                            Slider slid = sv.Q<Slider>(poselist[posetype].Replace("left","right"));
                            if (slid != null)
                            {
                                slid.value = posevalue;
                            }
                        }
                    }
                    


                }
                

            }
        }
        public void ReconstructExpressionList()
        {
            ScrollView sv = VPExpressionPanel.Q<ScrollView>();
            VisualElement svve = sv.Q<VisualElement>();
            if (sv != null)
            {
                sv.Clear();
                //get blend shape list
                List<string> arr = armf.GetVRMExpression();
                foreach (string item in arr)
                {
                    string[] ss = item.Split('=');
                    string basename = ss[0].Replace("PROX:", "");
                    float val = float.Parse(ss[1]);

                    Slider slider = new Slider();
                    slider.label = "";
                    slider.name = "sld_" + basename;
                    slider.tooltip = ss[0];
                    slider.value = val;
                    slider.lowValue = 0.01f;
                    slider.highValue = 1.0f;
                    slider.pageSize = 0.01f;
                    slider.style.maxHeight = new StyleLength(new Length(48, LengthUnit.Percent));
                    slider.style.minHeight = new StyleLength(new Length(64, LengthUnit.Pixel));
                    slider.style.fontSize = new StyleLength(new Length(36, LengthUnit.Pixel));
                    slider.style.paddingRight = new StyleLength(new Length(30));
                    slider.style.paddingLeft = new StyleLength(new Length(30));
                    slider.RegisterValueChangedCallback(evt =>
                    {
                        Slider sl = evt.target as Slider;
                        if (sl != null)
                        {
                            armf.SetVRMExpression(sl.tooltip, sl.value);
                            Toggle tgl = sl.parent.Q<Toggle>(sl.name.Replace("sld_", "tgl_"));
                            if (tgl != null)
                            {
                                tgl.value = true;
                            }
                            
                        }
                    });
                    //---other
                    Button btnlow = new Button();
                    btnlow.text = "<";
                    btnlow.name = "lowbtn_" + basename;
                    btnlow.AddToClassList("unity-button");
                    btnlow.style.backgroundColor = new Color(41, 123, 214, 0);
                    btnlow.style.color = new Color(255, 255, 255);
                    btnlow.style.minWidth = new StyleLength(new Length(36, LengthUnit.Pixel));
                    btnlow.style.fontSize = new StyleLength(new Length(36, LengthUnit.Pixel));

                    Label lab = new Label();
                    lab.name = "lab_" + basename;
                    lab.text = Mathf.Round(val * 100).ToString();
                    lab.tooltip = ss[0];
                    lab.style.maxWidth = new StyleLength(new Length(60, LengthUnit.Percent));
                    lab.style.minWidth = new StyleLength(new Length(60, LengthUnit.Percent));
                    lab.style.fontSize = new StyleLength(new Length(36, LengthUnit.Pixel));

                    Button btnhigh = new Button();
                    btnhigh.text = ">";
                    btnhigh.name = "highbtn_" + basename;
                    btnhigh.AddToClassList("unity-button");
                    btnhigh.style.backgroundColor = new Color(41, 123, 214, 0);
                    btnhigh.style.color = new Color(255, 255, 255);
                    btnhigh.style.minWidth = new StyleLength(new Length(36, LengthUnit.Pixel));
                    btnhigh.style.fontSize = new StyleLength(new Length(36, LengthUnit.Pixel));

                    btnlow.RegisterCallback<PointerDownEvent>((evt) =>
                    {
                        Button mybtn = evt.target as Button;
                        Label lab = mybtn.parent.Q<Label>();
                        if (lab != null)
                        {
                            pushed_blendshape_btn = mybtn.name;
                            pushed_blendshape_dir = -1f;
                            pushed_blendshape_label = lab.name;
                            return;
                            float val = float.Parse(lab.text);
                            val -= 1f;
                            if (val < 0) val = 0f;

                            lab.text = val.ToString();
                            armf.SetVRMExpression(lab.tooltip, val * 0.01f);
                            Toggle tgl = mybtn.parent.parent.Q<Toggle>(lab.name.Replace("lab_", "tgl_"));
                            if (tgl != null)
                            {
                                tgl.value = true;
                            }
                        }
                    }, TrickleDown.TrickleDown);
                    btnlow.RegisterCallback<PointerUpEvent>((evt) =>
                    {
                        pushed_blendshape_btn = "";
                        pushed_blendshape_dir = 0;
                    }, TrickleDown.TrickleDown);

                    btnhigh.RegisterCallback<PointerDownEvent>((evt) =>
                    {
                        Button mybtn = evt.target as Button;
                        Label lab = mybtn.parent.Q<Label>();
                        if (lab != null)
                        {
                            pushed_blendshape_btn = mybtn.name;
                            pushed_blendshape_dir = 1f;
                            pushed_blendshape_label = lab.name;
                            return;
                            float val = float.Parse(lab.text);
                            val += 1f;
                            if (val > 100f) val = 100f;

                            lab.text = val.ToString();
                            armf.SetVRMExpression(lab.tooltip, val * 0.01f);
                            Toggle tgl = mybtn.parent.parent.Q<Toggle>(lab.name.Replace("lab_", "tgl_"));
                            if (tgl != null)
                            {
                                tgl.value = true;
                            }
                        }
                    }, TrickleDown.TrickleDown);
                    btnhigh.RegisterCallback<PointerUpEvent>((evt) =>
                    {
                        pushed_blendshape_btn = "";
                        pushed_blendshape_dir = 0;
                    }, TrickleDown.TrickleDown);

                    Toggle tgl = new Toggle();
                    tgl.label = basename;
                    tgl.tooltip = ss[0];
                    tgl.name = lab.name.Replace("lab","tgl_");
                    tgl.value = ss[2] == "1" ? true : false;
                    tgl.style.fontSize = new StyleLength(new Length(36, LengthUnit.Pixel));

                    VisualElement rowmid = new VisualElement();
                    rowmid.style.flexDirection = FlexDirection.Row;
                    rowmid.style.paddingLeft = new StyleLength(new Length(12, LengthUnit.Pixel));
                    rowmid.style.paddingRight = new StyleLength(new Length(12, LengthUnit.Pixel));
                    rowmid.Add(btnlow);
                    rowmid.Add(lab);
                    rowmid.Add(btnhigh);

                    VisualElement row = new VisualElement();
                    row.style.flexDirection = FlexDirection.Column;
                    row.Add(tgl);
                    row.Add(slider);

                    svve.Add(row);

                }
            }

        }
        public void ReconstructAnimationClips()
        {
            DropdownField dp = VPAnimPanel.Q<DropdownField>("cmb_playanimlist");
            if (dp != null)
            {
                dp.choices.Clear();

                //---animation clip list
                List<string> lst = armf.GetAnimationClips();
                foreach (string s in lst)
                {
                    dp.choices.Add(s);
                }
                string tar = armf.GetTargetClip();
                int ishit = lst.FindIndex(match =>
                {
                    if (match == tar) return true;
                    return false;
                });
                if (ishit == -1)
                {
                    dp.index = ishit;
                }

                //---loop check
                VPAnimPanel.Q<Toggle>("chk_anim_loop").value = armf.GetAnimationLoop();

                //---seek pos
                float[] seeks = armf.GetSeekPos();
                Slider slid_seek = VPAnimPanel.Q<Slider>("slid_playanim_seek");
                slid_seek.highValue = seeks[1];
                slid_seek.value = seeks[0];

                //---speed animation
                float speed = armf.GetAnimationSpeed();
                VPAnimPanel.Q<Slider>("slid_playanim_speed").value = speed;

                //---system play flag
                int playflag = armf.GetSystemFlagAnimation();
                DropdownField dpflag = VPAnimPanel.Q<DropdownField>("cmb_playflag");
                dpflag.index = playflag;

                //---state setting
                bool flag = false;
                if (lst.Count > 0)
                {
                    flag = true;
                }
                VPAnimPanel.Q<Button>("arbtn_anim_play").SetEnabled(flag);
                VPAnimPanel.Q<Button>("arbtn_anim_play").SetEnabled(flag);
                VPAnimPanel.Q<Button>("arbtn_anim_play").SetEnabled(flag);
                VPAnimPanel.Q<Toggle>("chk_anim_loop").SetEnabled(flag);
                VPAnimPanel.Q<DropdownField>("cmb_playflag").SetEnabled(flag);
            }
        }
        //########################################################################################################
        void arbtn_folding_OnClick()
        {
            is_minimize = !is_minimize;

            if (is_minimize == true)
            {
                //LeftPanel.visible = false;
                //RightPanel.visible = false;
                //MiniPanel.Q<Button>("arbtn_motplay2").visible = true;
                //MiniPanel.Q<Button>("arbtn_motstop2").visible = true;
            }
            else
            {
                //LeftPanel.visible = true;
                //RightPanel.visible = true;
                //MiniPanel.Q<Button>("arbtn_motplay2").visible = false;
                //MiniPanel.Q<Button>("arbtn_motstop2").visible = false;
            }
            armf.StartPauseAnimation();
        }
        void arbtn_namepanel_OnClick()
        {
            if (pnlc_panel1.visible == true)
            {
                pnlc_panel1.visible = false;
            }
            else
            {
                pnlc_panel1.visible = true;
            }
        }
        void arbtn_leftpanel_OnClick()
        {
            if (LeftPanel.visible == true)
            {
                LeftPanel.visible = false;
            }
            else
            {
                LeftPanel.visible = true;
            }
            //arbtn_namepanel_OnClick();
        }
        void arbtn_rightpanel_OnClick()
        {
            if (RightPanel.visible == true)
            { //---to hide
                RightPanel.visible = false;
                if (BonePanel.visible == true)
                {
                    ShowVRMBonePanel(false);
                }
                //if (armf.SelectedAvatarType == AF_TARGETTYPE.VRM)
                {
                    ShowVRMButton(false);
                }
                
                RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Hidden;
                //RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Hidden;
                ShowRotateAxisPanel(false);

            }
            else
            { //---to show
                RightPanel.visible = true;
                if (armf.SelectedAvatarType == AF_TARGETTYPE.VRM)
                {
                    if (armf.GetMoveTarget() == "b")
                    { //-- oldly select BonePanel, recover selected states.
                        ShowVRMBonePanel(true);
                    }
                    ShowVRMButton(true);
                }
                else
                {
                    ShowVRMBonePanel(false);
                }

                if (armf.GetMoveTarget() == "o")
                {
                    RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Visible;
                    //RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Visible;

                }
                else
                {
                    RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Hidden;
                    //RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Hidden;

                }
                ShowRotateAxisPanel(true);
            }
        }

        //########################################################################################################
        void arbtn_prevselobj_OnClick()
        {
            arlab_selectavatar.text = armf.TurnPreviousObject(arlab_selectavatar.text);
            NamePanel.Q<Label>("arlab_selectbone").text = "";
            if (armf.SelectedAvatarType == AF_TARGETTYPE.VRM)
            {
                ShowVRMButton(true);
                ReconstructExpressionList();
                ReconstructHandList();
                ReconstructAnimationClips();
            }
            else
            {
                ShowVRMButton(true);
                if (armf.SelectedAvatarType == AF_TARGETTYPE.OtherObject)
                {
                    ReconstructAnimationClips();
                }
            }
            ChangeStateVisible_PropertyTab(armf.SelectedAvatarType);
        }
        void arbtn_nextselobj_OnClick()
        {
            arlab_selectavatar.text = armf.TurnNextObject(arlab_selectavatar.text);
            NamePanel.Q<Label>("arlab_selectbone").text = "";
            if (armf.SelectedAvatarType == AF_TARGETTYPE.VRM)
            {
                ShowVRMButton(true);
                ReconstructExpressionList();
                ReconstructHandList();
                ReconstructAnimationClips();
            }
            else
            {
                ShowVRMButton(true);
                if (armf.SelectedAvatarType == AF_TARGETTYPE.OtherObject)
                {
                    ReconstructAnimationClips();
                }
            }
            ChangeStateVisible_PropertyTab(armf.SelectedAvatarType);
        }
        void arbtn_showik_OnClick()
        {
            showik_state = !showik_state;
            armf.ChangeIKMarkerView(showik_state);
        }
        void arbtn_oncutout_OnClick()
        {
            cutout_state = !cutout_state;
            armf.ChangeShaderCutout(cutout_state);
        }
        void arbtn_enabletap_OnClick()
        {
            manim.camxr.IsStartTapControl = !manim.camxr.IsStartTapControl;
            if (manim.camxr.IsStartTapControl)
            {
                LeftPanel.Q<Button>("arbtn_enabletap").style.backgroundColor = new StyleColor(new Color(188, 188, 188, 255));
            }
            else
            {
                LeftPanel.Q<Button>("arbtn_enabletap").style.backgroundColor = new StyleColor(new Color(188, 188, 188, 0));
            }
            
        }
        void arbtn_globallocal_OnClick()
        {
            if (LeftPanel.Q<Button>("arbtn_globallocal").text == "L")
            {
                armf.ChangeSpaceType(Space.World);
                LeftPanel.Q<Button>("arbtn_globallocal").text = "G";
            }
            else if (LeftPanel.Q<Button>("arbtn_globallocal").text == "G")
            {
                armf.ChangeSpaceType(Space.Self);
                LeftPanel.Q<Button>("arbtn_globallocal").text = "L";
            }
        }
        void ChangeSpaceGL(string flag)
        {
            if (flag == "G")
            {
                armf.ChangeSpaceType(Space.World);
                LeftPanel.Q<Button>("arbtn_globallocal").text = "G";
            }
            else if (flag == "L")
            {
                armf.ChangeSpaceType(Space.Self);
                LeftPanel.Q<Button>("arbtn_globallocal").text = "L";
            }
        }
        public void LabelWriteKeyFrame(int index)
        {
            arlab_keynumber.text = index.ToString();
        }
        void arbtn_prevkeyframe_OnClick()
        {
            int val = armf.ChangePreviousKeyFrame();
            arlab_keynumber.text = val.ToString();
        }
        void arbtn_nextkeyframe_OnClick()
        {
            int val = armf.ChangeNextKeyFrame();
            arlab_keynumber.text = val.ToString();
        }
        void regkeyframe_OnClick()
        {
            ScrollView scr = VPExpressionPanel.Q<ScrollView>();
            List<Toggle> tgls = scr.Query<Toggle>().ToList();
            List<BasicStringIntList> bslst = new List<BasicStringIntList>();
            if (armf.SelectedAvatarType == AF_TARGETTYPE.VRM)
            {
                foreach (Toggle toggle in tgls)
                {
                    bslst.Add(new BasicStringIntList(toggle.tooltip, toggle.value ? 1 : 0));
                }
            }
            
            armf.RegisterKeyFrame(bslst: bslst);
        }
        void arbtn_motplay_OnClick()
        {
            armf.StartPauseAnimation();
        }
        void arbtn_motstop_OnClick()
        {
            armf.StopAnimation();
        }
        //#######################################################################################################3
        void arbtn_mt_camera_OnClick()
        {
            armf.ChangeMoveTarget("c");
            //armf.ChangeSpaceType(Space.World);
            ChangeSpaceGL("G");
            ChangeStyleBoneBtn("", true);
            RightPanel.Q<Button>("arbtn_mt_camera").style.backgroundColor = new Color(41, 123, 214, 255);
            RightPanel.Q<Button>("arbtn_mt_obj").style.backgroundColor = new Color(41, 123, 214, 0);
            RightPanel.Q<Button>("arbtn_mt_vrmbone").style.backgroundColor = new Color(41, 123, 214, 0);

            ShowVRMBonePanel(false);
            RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Hidden;
            //RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Hidden;
        }
        void arbtn_mt_obj_OnClick()
        {
            ChangeStyleBoneBtn("", true);
            //armf.ChangeSpaceType(Space.Self);
            ChangeSpaceGL("L");
            armf.ChangeMoveTarget("o");
            RightPanel.Q<Button>("arbtn_mt_camera").style.backgroundColor = new Color(41, 123, 214, 0);
            RightPanel.Q<Button>("arbtn_mt_obj").style.backgroundColor = new Color(41, 123, 214, 255);
            RightPanel.Q<Button>("arbtn_mt_vrmbone").style.backgroundColor = new Color(41, 123, 214, 0);

            ShowVRMBonePanel(false);
            RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Visible;
            //RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Visible;
        }
        void arbtn_mt_vrmbone_OnClick()
        {
            armf.ChangeMoveTarget("b");
            //armf.ChangeSpaceType(Space.Self);
            ChangeSpaceGL("L");
            ChangeStyleBoneBtn("", true);
            RightPanel.Q<Button>("arbtn_mt_vrmbone").style.backgroundColor = new Color(41, 123, 214, 255);

            ShowVRMBonePanel(true);

            //--- bone ik marker, not show size buttons          
            RightPanel.Q<Button>("arbtn_obj_size").style.visibility = Visibility.Hidden;
            //RightPanel.Q<Button>("arbtn_reset_size").style.visibility = Visibility.Hidden;            
           
        }
        void arbtn_obj_move_OnClick()
        {
            armf.ChangeMoveOperationType("translate");
            RightPanel.Q<Button>("arbtn_obj_move").style.backgroundColor = new Color(41, 123, 214, 255);
            RightPanel.Q<Button>("arbtn_obj_rot").style.backgroundColor = new Color(41, 123, 214, 0);
            RightPanel.Q<Button>("arbtn_obj_size").style.backgroundColor = new Color(41, 123, 214, 0);

            rootElement.Q<VisualElement>("tpbox_tran1").style.display = DisplayStyle.Flex;
            rootElement.Q<VisualElement>("tpbox_tran2").style.display = DisplayStyle.Flex;
            rootElement.Q<VisualElement>("tpbox_rot1").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_rot2").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_size1").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_size2").style.display = DisplayStyle.None;
        }
        void arbtn_obj_rot_OnClick()
        {
            armf.ChangeMoveOperationType("rotate");
            RightPanel.Q<Button>("arbtn_obj_move").style.backgroundColor = new Color(41, 123, 214, 0);
            RightPanel.Q<Button>("arbtn_obj_rot").style.backgroundColor = new Color(41, 123, 214, 255);
            RightPanel.Q<Button>("arbtn_obj_size").style.backgroundColor = new Color(41, 123, 214, 0);

            rootElement.Q<VisualElement>("tpbox_tran1").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_tran2").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_rot1").style.display = DisplayStyle.Flex;
            rootElement.Q<VisualElement>("tpbox_rot2").style.display = DisplayStyle.Flex;
            rootElement.Q<VisualElement>("tpbox_size1").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_size2").style.display = DisplayStyle.None;

        }
        void arbtn_obj_size_OnClick()
        {
            armf.ChangeMoveOperationType("size");
            RightPanel.Q<Button>("arbtn_obj_move").style.backgroundColor = new Color(41, 123, 214, 0);
            RightPanel.Q<Button>("arbtn_obj_rot").style.backgroundColor = new Color(41, 123, 214, 0);
            RightPanel.Q<Button>("arbtn_obj_size").style.backgroundColor = new Color(41, 123, 214, 255);

            rootElement.Q<VisualElement>("tpbox_tran1").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_tran2").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_rot1").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_rot2").style.display = DisplayStyle.None;
            rootElement.Q<VisualElement>("tpbox_size1").style.display = DisplayStyle.Flex;
            rootElement.Q<VisualElement>("tpbox_size2").style.display = DisplayStyle.Flex;
        }
        void arbtn_reset_move_OnClick()
        {
            if (armf.GetMoveOperationType() == "translate")
            {
                StartCoroutine(armf.ResetTransformCurrentObject(true, false, false));
            }
            else if (armf.GetMoveOperationType() == "rotate")
            {
                StartCoroutine(armf.ResetTransformCurrentObject(false, true, false));
            }
            else if (armf.GetMoveOperationType() == "size")
            {
                StartCoroutine(armf.ResetTransformCurrentObject(false, false, true));
            }
            
        }
        void arbtn_reset_tpose_OnClick()
        {
            armf.ResetAllBones();
        }
        void arbtn_reset_size_OnClick()
        {
            
        }
        void CommonBtnDownEvent()
        {
            MouseOperationXR[] mxrs = GameObject.FindObjectsOfType<MouseOperationXR>();
            foreach (var  mxr in mxrs) 
            {
                mxr.EmergencyStop = true;
            }
            MouseOperationXR2[] mxr2s = GameObject.FindObjectsOfType<MouseOperationXR2>();
            foreach (var mxr in mxr2s)
            {
                mxr.EmergencyStop = true;
            }
        }
        void CommonBtnUpEvent()
        {
            MouseOperationXR[] mxrs = GameObject.FindObjectsOfType<MouseOperationXR>();
            foreach (var mxr in mxrs)
            {
                mxr.EmergencyStop = false;
            }
            MouseOperationXR2[] mxr2s = GameObject.FindObjectsOfType<MouseOperationXR2>();
            foreach (var mxr in mxr2s)
            {
                mxr.EmergencyStop = false;
            }
        }
        //---X+
        /*void tp_right_OnClick()
        { 
            armf.TargetOperationBody("tp_right");
        }*/
        void tp_right_PointerDownEvent(PointerDownEvent evt)
        {
            armf.TargetOperationBody("tp_right");
            pushed_tp_right = true;
            CommonBtnDownEvent();
            evt.StopPropagation();
        }
        void tp_right_PointerUpEvent(PointerUpEvent evt)
        {
            pushed_tp_right = false;
            CommonBtnUpEvent();
            evt.StopPropagation();
        }
        void tp_right_PointerLeaveEvent(PointerLeaveEvent evt)
        {
            pushed_tp_right = false;
            CommonBtnUpEvent();
            evt.StopPropagation();
        }
        //---X-
        /*void tp_left_OnClick()
        {
            armf.TargetOperationBody("tp_left");
        }*/
        void tp_left_PointerDownEvent(PointerDownEvent evt)
        {
            armf.TargetOperationBody("tp_left");
            pushed_tp_left = true;
            CommonBtnDownEvent();
            evt.StopPropagation();
        }
        void tp_left_PointerUpEvent(PointerUpEvent evt)
        {
            pushed_tp_left = false;
            CommonBtnUpEvent();
            evt.StopPropagation();
        }
        void tp_left_PointerLeaveEvent(PointerLeaveEvent evt)
        {
            pushed_tp_left = false;
            CommonBtnUpEvent();
            evt.StopPropagation();
        }
        //---Y+
        /*void tp_up_OnClick()
        {
            armf.TargetOperationBody("tp_up");
        }*/
        void tp_up_PointerDownEvent(PointerDownEvent evt)
        {
            armf.TargetOperationBody("tp_up");
            pushed_tp_up = true;
            CommonBtnDownEvent();
            evt.StopPropagation();
        }
        void tp_up_PointerUpEvent(PointerUpEvent evt)
        {
            pushed_tp_up = false;
            CommonBtnUpEvent();
            evt.StopPropagation();
        }
        void tp_up_PointerLeaveEvent(PointerLeaveEvent evt)
        {
            pushed_tp_up = false;
            CommonBtnUpEvent();
            evt.StopPropagation();
        }
        //---Y-
        /*void tp_down_OnClick()
        {
            armf.TargetOperationBody("tp_down");
        }*/
        void tp_down_PointerDownEvent(PointerDownEvent evt)
        {
            armf.TargetOperationBody("tp_down");
            pushed_tp_down = true;
            CommonBtnDownEvent();
            evt.StopPropagation();
        }
        void tp_down_PointerUpEvent(PointerUpEvent evt)
        {
            pushed_tp_down = false;
            CommonBtnUpEvent();
            evt.StopPropagation();
        }
        void tp_down_PointerLeaveEvent(PointerLeaveEvent evt)
        {
            pushed_tp_down = false;
            CommonBtnUpEvent();
            evt.StopPropagation();
        }
        //---Z+
        /*void tp_forward_OnClick()
        {
            armf.TargetOperationBody("tp_forward");
        }*/
        void tp_forward_PointerDownEvent(PointerDownEvent evt)
        {
            armf.TargetOperationBody("tp_forward");
            pushed_tp_forward = true;
            CommonBtnDownEvent();
            evt.StopPropagation();
        }
        void tp_forward_PointerUpEvent(PointerUpEvent evt)
        {
            pushed_tp_forward = false;
            CommonBtnUpEvent();
            evt.StopPropagation();
        }
        void tp_forward_PointerLeaveEvent(PointerLeaveEvent evt)
        {
            pushed_tp_forward = false;
            CommonBtnUpEvent();
            evt.StopPropagation();
        }
        //---Z-
        /*void tp_back_OnClick()
        {
            armf.TargetOperationBody("tp_back");
        }*/
        void tp_back_PointerDownEvent(PointerDownEvent evt)
        {
            armf.TargetOperationBody("tp_back");
            pushed_tp_back = true;
            CommonBtnDownEvent();
            evt.StopPropagation();
        }
        void tp_back_PointerUpEvent(PointerUpEvent evt)
        {
            pushed_tp_back = false;
            CommonBtnUpEvent();
            evt.StopPropagation();
        }
        void tp_back_PointerLeaveEvent(PointerLeaveEvent evt)
        {
            pushed_tp_back = false;
            CommonBtnUpEvent();
            evt.StopPropagation();
        }

        //#######################################################################################################
        void ChangeStyleBoneBtn(string targetname, bool alldisable = false)
        {
            Color sel = new Color(41, 123, 214, 255);
            Color non = new Color(41, 123, 214, 0);

            //BonePanel.Q<Button>("arbtn_bn_ikparent").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_head").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_lookat").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_eyeviewhandle").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_chest").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_aim").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_rightshoulder").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_leftshoulder").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_rightlowerarm").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_leftlowerarm").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_pelvis").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_leftarm").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_rightarm").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_leftlowerleg").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_rightlowerleg").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_leftleg").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_rightleg").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_lefttoes").style.backgroundColor = non;
            BonePanel.Q<Button>("arbtn_bn_righttoes").style.backgroundColor = non;

            RightPanel.Q<Button>("arbtn_mt_camera").style.backgroundColor = non;
            RightPanel.Q<Button>("arbtn_mt_obj").style.backgroundColor = non;


            //---select
            if (alldisable != true)
            {
                BonePanel.Q<Button>("arbtn_bn_" + targetname).style.backgroundColor = sel;
            }
            
        }

        void arbtn_bn_head_OnClick()
        {
            string partsname = "head";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---only rotate
            arbtn_obj_rot_OnClick();
        }
        void arbtn_bn_eyeviewhandle_OnClick()
        {
            string partsname = "eyeviewhandle";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---priority move
            arbtn_obj_move_OnClick();
        }
        void arbtn_bn_lookat_OnClick()
        {
            string partsname = "lookat";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---priority move
            arbtn_obj_move_OnClick();
        }
        void arbtn_bn_chest_OnClick()
        {
            string partsname = "chest";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---only rotate
            arbtn_obj_rot_OnClick();
        }
        void arbtn_bn_aim_OnClick()
        {
            string partsname = "aim";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---only rotate
            arbtn_obj_rot_OnClick();
        }
        void arbtn_bn_rightshoulder_OnClick()
        {
            string partsname = "rightshoulder";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---only rotate
            arbtn_obj_rot_OnClick();
        }
        void arbtn_bn_leftshoulder_OnClick()
        {
            string partsname = "leftshoulder";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---only rotate
            arbtn_obj_rot_OnClick();
        }
        void arbtn_bn_rightlowerarm_OnClick()
        {
            string partsname = "rightlowerarm";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---priority move
            arbtn_obj_move_OnClick();
        }
        void arbtn_bn_leftlowerarm_OnClick()
        {
            string partsname = "leftlowerarm";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---priority move
            arbtn_obj_move_OnClick();
        }
        void arbtn_bn_pelvis_OnClick()
        {
            string partsname = "pelvis";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---priority move
            arbtn_obj_move_OnClick();
        }
        void arbtn_bn_lefthand_OnClick()
        {
            string partsname = "leftarm";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---priority move
            arbtn_obj_move_OnClick();
        }
        void arbtn_bn_righthand_OnClick()
        {
            string partsname = "rightarm";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---priority move
            arbtn_obj_move_OnClick();
        }
        void arbtn_bn_leftlowerleg_OnClick()
        {
            string partsname = "leftlowerleg";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---priority move
            arbtn_obj_move_OnClick();
        }
        void arbtn_bn_rightlowerleg_OnClick()
        {
            string partsname = "rightlowerleg";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---priority move
            arbtn_obj_move_OnClick();
        }
        void arbtn_bn_leftleg_OnClick()
        {
            string partsname = "leftleg";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---priority move
            arbtn_obj_move_OnClick();
        }
        void arbtn_bn_rightleg_OnClick()
        {
            string partsname = "rightleg";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---priority move
            arbtn_obj_move_OnClick();
        }
        void arbtn_bn_lefttoes_OnClick()
        {
            string partsname = "lefttoes";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---only rotate
            arbtn_obj_rot_OnClick();
        }
        void arbtn_bn_righttoes_OnClick()
        {
            string partsname = "righttoes";
            armf.SelectVRMBone(partsname);
            ChangeStyleBoneBtn(partsname);
            //---only rotate
            arbtn_obj_rot_OnClick();
        }

        //### vrm panel, change select Bones or Expression ##############################################
        void ChangeStateVisible_PropertyTab(AF_TARGETTYPE type)
        {
            if (type == AF_TARGETTYPE.VRM)
            {
                VBTabPanel.Q<Button>("arbtn_vrm_bones").style.display = DisplayStyle.Flex;
                VBTabPanel.Q<Button>("arbtn_vrm_blendshapes").style.display = DisplayStyle.Flex;
                VBTabPanel.Q<Button>("arbtn_vrm_hand").style.display = DisplayStyle.Flex;
                VBTabPanel.Q<Button>("arbtn_vrm_playanim").style.display = DisplayStyle.Flex;
                arbtn_vrm_bones_OnClick();
            }
            else if (type == AF_TARGETTYPE.OtherObject)
            {
                VBTabPanel.Q<Button>("arbtn_vrm_bones").style.display = DisplayStyle.None;
                VBTabPanel.Q<Button>("arbtn_vrm_blendshapes").style.display = DisplayStyle.None;
                VBTabPanel.Q<Button>("arbtn_vrm_hand").style.display = DisplayStyle.None;
                VBTabPanel.Q<Button>("arbtn_vrm_playanim").style.display = DisplayStyle.Flex;
                arbtn_vrm_playanim_OnClick();
            }
            else
            {
                VBTabPanel.Q<Button>("arbtn_vrm_bones").style.display = DisplayStyle.None;
                VBTabPanel.Q<Button>("arbtn_vrm_blendshapes").style.display = DisplayStyle.None;
                VBTabPanel.Q<Button>("arbtn_vrm_hand").style.display = DisplayStyle.None;
                VBTabPanel.Q<Button>("arbtn_vrm_playanim").style.display = DisplayStyle.None;
                VPBonePanel.style.display = DisplayStyle.None;
                VPExpressionPanel.style.display = DisplayStyle.None;
                VPHandPanel.style.display = DisplayStyle.None;
                VPAnimPanel.style.display = DisplayStyle.None;
            }
        }
        void arbtn_vrm_bones_OnClick()
        {
            VBTabPanel.Q<Button>("arbtn_vrm_bones").style.backgroundColor = new Color(41, 123, 214, 255);
            VBTabPanel.Q<Button>("arbtn_vrm_blendshapes").style.backgroundColor = new Color(41, 123, 214, 0);
            VBTabPanel.Q<Button>("arbtn_vrm_hand").style.backgroundColor = new Color(41, 123, 214, 0);
            VBTabPanel.Q<Button>("arbtn_vrm_playanim").style.backgroundColor = new Color(41, 123, 214, 0);
            VPBonePanel.style.display = DisplayStyle.Flex;
            VPExpressionPanel.style.display = DisplayStyle.None;
            VPHandPanel.style.display = DisplayStyle.None;
            VPAnimPanel.style.display = DisplayStyle.None;

        }
        void arbtn_vrm_blendshapes_OnClick()
        {
            VBTabPanel.Q<Button>("arbtn_vrm_bones").style.backgroundColor = new Color(41, 123, 214, 0);
            VBTabPanel.Q<Button>("arbtn_vrm_blendshapes").style.backgroundColor = new Color(41, 123, 214, 255);
            VBTabPanel.Q<Button>("arbtn_vrm_hand").style.backgroundColor = new Color(41, 123, 214, 0);
            VBTabPanel.Q<Button>("arbtn_vrm_playanim").style.backgroundColor = new Color(41, 123, 214, 0);
            VPBonePanel.style.display = DisplayStyle.None;
            VPExpressionPanel.style.display = DisplayStyle.Flex;
            VPHandPanel.style.display = DisplayStyle.None;
            VPAnimPanel.style.display = DisplayStyle.None;
        }

        void arbtn_vrm_hand_OnClick()
        {
            VBTabPanel.Q<Button>("arbtn_vrm_bones").style.backgroundColor = new Color(41, 123, 214, 0);
            VBTabPanel.Q<Button>("arbtn_vrm_blendshapes").style.backgroundColor = new Color(41, 123, 214, 0);
            VBTabPanel.Q<Button>("arbtn_vrm_hand").style.backgroundColor = new Color(41, 123, 214, 255);
            VBTabPanel.Q<Button>("arbtn_vrm_playanim").style.backgroundColor = new Color(41, 123, 214, 0);
            VPBonePanel.style.display = DisplayStyle.None;
            VPExpressionPanel.style.display = DisplayStyle.None;
            VPHandPanel.style.display = DisplayStyle.Flex;
            VPAnimPanel.style.display = DisplayStyle.None;
        }
        void arbtn_vrm_playanim_OnClick()
        {
            VBTabPanel.Q<Button>("arbtn_vrm_bones").style.backgroundColor = new Color(41, 123, 214, 0);
            VBTabPanel.Q<Button>("arbtn_vrm_blendshapes").style.backgroundColor = new Color(41, 123, 214, 0);
            VBTabPanel.Q<Button>("arbtn_vrm_hand").style.backgroundColor = new Color(41, 123, 214, 0);
            VBTabPanel.Q<Button>("arbtn_vrm_playanim").style.backgroundColor = new Color(41, 123, 214, 255);
            VPBonePanel.style.display = DisplayStyle.None;
            VPExpressionPanel.style.display = DisplayStyle.None;
            VPHandPanel.style.display = DisplayStyle.None;
            VPAnimPanel.style.display = DisplayStyle.Flex;
        }

        /// <summary>
        /// Show rotate axis of current operating IK-Marker
        /// </summary>
        /// <param name="isshow"></param>
        /// <param name="content"></param>
        /// <param name="pos"></param>
        public void ShowRotatePopup(bool isshow, string content, string partsname)
        {
            var lst = RotatePopupPanel.Query<Button>().ToList();
            foreach (var item in lst)
            {
                item.style.display = DisplayStyle.None;
            }
            Button lab = RotatePopupPanel.Q<Button>("axis" + content);

            lab.style.display = DisplayStyle.Flex;
            //lab.text = content;
            //ve.style.top = new StyleLength(new Length(pos.y, LengthUnit.Pixel));
            //ve.style.left = new StyleLength(new Length(pos.x, LengthUnit.Pixel));

            NamePanel.Q<Label>("arlab_selectbone").text = partsname;
        }
        IEnumerator HideRotatePopupAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            VisualElement ve = rootElement.Q<VisualElement>("panel_rotatepopup");
            if (ve != null)
            {
                ve.style.display = DisplayStyle.None;
            }
        }
        void panel_rotatepopup_axisX_OnClick()
        {
            if (MouseOperationXR.CurrentGrabbed != null)
            {
                MouseOperationXR.CurrentGrabbed.rotateAxis = MouseOperationXR.RotateAxis.Y;
                RotatePopupPanel.Q<Button>("axisX").style.display = DisplayStyle.None;
                RotatePopupPanel.Q<Button>("axisY").style.display = DisplayStyle.Flex;
            }
        }
        void panel_rotatepopup_axisY_OnClick()
        {        
            if (MouseOperationXR.CurrentGrabbed != null)
            {
                MouseOperationXR.CurrentGrabbed.rotateAxis = MouseOperationXR.RotateAxis.Z;
                RotatePopupPanel.Q<Button>("axisY").style.display = DisplayStyle.None;
                RotatePopupPanel.Q<Button>("axisZ").style.display = DisplayStyle.Flex;
            }
            
        }
        void panel_rotatepopup_axisZ_OnClick()
        {            
            if (MouseOperationXR.CurrentGrabbed != null)
            {
                MouseOperationXR.CurrentGrabbed.rotateAxis = MouseOperationXR.RotateAxis.X;
                RotatePopupPanel.Q<Button>("axisZ").style.display = DisplayStyle.None;
                RotatePopupPanel.Q<Button>("axisX").style.display = DisplayStyle.Flex;
            }
            
        }

        //### Other Properties #################################################
        void cmb_playanimlist_OnChange(ChangeEvent<string> evt)
        {
            DropdownField dp = VPAnimPanel.Q<DropdownField>("cmb_playanimlist");
            if (dp.index > -1)
            {
                armf.SetAnimationClip(dp.choices[dp.index]);

                //---seek pos
                float[] seeks = armf.GetSeekPos();
                Slider slid_seek = VPAnimPanel.Q<Slider>("slid_playanim_seek");
                slid_seek.highValue = seeks[1];
                slid_seek.value = seeks[0];

                //---speed animation
                float speed = armf.GetAnimationSpeed();
                VPAnimPanel.Q<Slider>("slid_playanim_speed").value = speed;
            }
        }
        void arbtn_anim_play_OnClick()
        {
            armf.StateChangeAnimationClip(0);
            //VPAnimPanel.Q<Button>("arbtn_anim_play").style.display = DisplayStyle.None;
            //VPAnimPanel.Q<Button>("arbtn_anim_pause").style.display = DisplayStyle.Flex;
        }
        void arbtn_anim_pause_OnClick()
        {
            armf.StateChangeAnimationClip(1);
            //VPAnimPanel.Q<Button>("arbtn_anim_play").style.display = DisplayStyle.Flex;
            //VPAnimPanel.Q<Button>("arbtn_anim_pause").style.display = DisplayStyle.None;
        }
        void arbtn_anim_stop_OnClick()
        {
            armf.StateChangeAnimationClip(2);
            //VPAnimPanel.Q<Button>("arbtn_anim_play").style.display = DisplayStyle.Flex;
            //VPAnimPanel.Q<Button>("arbtn_anim_pause").style.display = DisplayStyle.None;
        }
        void chk_anim_loop_OnChange(ChangeEvent<bool> evt)
        {
            armf.SetAnimationLoop(evt.newValue);
        }
        void cmb_playflag_OnChange(ChangeEvent<string> evt)
        {
            DropdownField dp = VPAnimPanel.Q<DropdownField>("cmb_playflag");
            if (dp.index > -1)
            {
                armf.SetSystemFlagAnimation(dp.index);
            }
        }
        void slid_playanim_seek_OnChange(ChangeEvent<float> evt)
        {
            armf.SeekAnimationClip(evt.newValue);
        }
        void slid_playanim_speed_OnChange(ChangeEvent<float> evt)
        {
            armf.SetSpeedAnimation(evt.newValue);
        }
    }

}
